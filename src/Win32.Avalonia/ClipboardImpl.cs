using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;

namespace Win32.Avalonia;

internal sealed class ClipboardImpl : IOwnedClipboardImpl, IFlushableClipboardImpl
{
    private const int OleRetryCount = 10;
    private const int OleRetryDelay = 100;
    private const int OleFlushDelay = 10;

    private DataTransferToOleDataObjectWrapper? _lastStoredDataObject;
    private nint _lastStoredDataObjectPointer;

    public async Task ClearAsync()
    {
        using (await OpenClipboardAsync())
        {
            OleNative.EmptyClipboard();
            ClearLastStoredObject();
        }
    }

    public async Task SetDataAsync(IAsyncDataTransfer dataTransfer)
    {
        Dispatcher.UIThread.VerifyAccess();

        var synchronousDataTransfer = dataTransfer as IDataTransfer ?? new BlockingAsyncDataTransfer(dataTransfer);
        var wrapper = new DataTransferToOleDataObjectWrapper(synchronousDataTransfer);
        var retries = OleRetryCount;

        while (true)
        {
            var hr = OleNative.OleSetClipboard(wrapper.DataObjectPointer);
            if (hr == OleHResults.SOk)
            {
                ClearLastStoredObject();
                _lastStoredDataObject = wrapper;
                _lastStoredDataObjectPointer = wrapper.DataObjectPointer;
                return;
            }

            if (--retries == 0)
            {
                wrapper.Dispose();
                Marshal.ThrowExceptionForHR(hr);
            }

            await Task.Delay(OleRetryDelay).ConfigureAwait(true);
        }
    }

    public async Task<IAsyncDataTransfer?> TryGetDataAsync()
    {
        Dispatcher.UIThread.VerifyAccess();
        var retries = OleRetryCount;

        while (true)
        {
            var hr = OleNative.OleGetClipboard(out var dataObjectPointer);
            if (hr == OleHResults.SOk)
            {
                var dataObject = GeneratedComHelpers.ConvertToManaged<IOleDataObject>(dataObjectPointer);
                if (dataObject is null)
                {
                    return null;
                }

                var dataTransfer = new OleDataTransfer(dataObject);
                return dataTransfer.Formats.Count == 0 ? null : dataTransfer;
            }

            if (--retries == 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            await Task.Delay(OleRetryDelay).ConfigureAwait(true);
        }
    }

    public Task<bool> IsCurrentOwnerAsync()
    {
        var isCurrent = _lastStoredDataObject is { IsDisposed: false }
            && _lastStoredDataObjectPointer != nint.Zero
            && OleNative.OleIsCurrentClipboard(_lastStoredDataObjectPointer) == OleHResults.SOk;

        if (!isCurrent)
        {
            ClearLastStoredObject();
        }

        return Task.FromResult(isCurrent);
    }

    public async Task FlushAsync()
    {
        await Task.Delay(OleFlushDelay).ConfigureAwait(true);
        var retries = OleRetryCount;

        while (true)
        {
            var hr = OleNative.OleFlushClipboard();
            if (hr == OleHResults.SOk)
            {
                return;
            }

            if (--retries == 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            await Task.Delay(OleRetryDelay).ConfigureAwait(true);
        }
    }

    private void ClearLastStoredObject()
    {
        _lastStoredDataObject?.Dispose();
        _lastStoredDataObject = null;
        _lastStoredDataObjectPointer = nint.Zero;
    }

    private static async Task<IDisposable> OpenClipboardAsync()
    {
        var retries = OleRetryCount;
        while (!OleNative.OpenClipboard(nint.Zero))
        {
            if (--retries == 0)
            {
                throw new TimeoutException("Timeout opening clipboard.");
            }

            await Task.Delay(OleRetryDelay).ConfigureAwait(true);
        }

        return new ActionDisposable(static () => OleNative.CloseClipboard());
    }

    private sealed class ActionDisposable(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose()
        {
            Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }
}