using System.Runtime.InteropServices;
using Avalonia.OpenGL.Egl;

namespace Win32.Avalonia;

internal sealed class DirectCompositedWindow : IDisposable
{
    private readonly DirectCompositionShared _shared;
    private readonly nint _targetPointer;
    private readonly nint _visualPointer;

    public DirectCompositedWindow(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info, DirectCompositionShared shared)
    {
        WindowInfo = info;
        _shared = shared;

        var hr = shared.Device.CreateTargetForHwnd(info.Handle, false, out _targetPointer);
        if (hr < 0 || _targetPointer == 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        hr = shared.Device.CreateVisual(out _visualPointer);
        if (hr < 0 || _visualPointer == 0)
        {
            GeneratedComHelpers.Free<IDCompositionTargetCom>(_targetPointer);
            Marshal.ThrowExceptionForHR(hr);
        }

        Target = GeneratedComHelpers.ConvertToManaged<IDCompositionTargetCom>(_targetPointer)
            ?? throw new InvalidOperationException("Unable to wrap the DirectComposition target.");
        Visual = GeneratedComHelpers.ConvertToManaged<IDCompositionVisualCom>(_visualPointer)
            ?? throw new InvalidOperationException("Unable to wrap the DirectComposition visual.");

        hr = Target.SetRoot(_visualPointer);
        if (hr < 0)
        {
            Dispose();
            Marshal.ThrowExceptionForHR(hr);
        }
    }

    public EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo WindowInfo { get; }

    private IDCompositionTargetCom Target { get; }

    private IDCompositionVisualCom Visual { get; }

    public void SetSurface(nint surfacePointer)
    {
        var hr = Visual.SetContent(surfacePointer);
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
    }

    public IDisposable BeginTransaction()
    {
        Monitor.Enter(_shared.SyncRoot);
        return new ActionDisposable(() =>
        {
            try
            {
                var hr = _shared.Device.Commit();
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
            finally
            {
                Monitor.Exit(_shared.SyncRoot);
            }
        });
    }

    public void Dispose()
    {
        GeneratedComHelpers.Free<IDCompositionVisualCom>(_visualPointer);
        GeneratedComHelpers.Free<IDCompositionTargetCom>(_targetPointer);
    }

    private sealed class ActionDisposable(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose()
            => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}