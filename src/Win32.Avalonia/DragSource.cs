using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Threading;

namespace Win32.Avalonia;

internal sealed class DragSource : IPlatformDragSource
{
    public Task<DragDropEffects> DoDragDropAsync(
        PointerPressedEventArgs triggerEvent,
        IDataTransfer dataTransfer,
        DragDropEffects allowedEffects)
    {
        Dispatcher.UIThread.VerifyAccess();

        triggerEvent.Pointer.Capture(null);

        using var dataObject = new DataTransferToOleDataObjectWrapper(dataTransfer);
        using var dropSource = new GeneratedComValue<IOleDropSource>(new OleDragSource());
        var hr = OleNative.DoDragDrop(dataObject.DataObjectPointer, dropSource.DangerousGetPointer(), (int)ToOleDropEffect(allowedEffects), out var finalEffect);
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        dataObject.ReleaseDataTransfer();
        return Task.FromResult(FromOleDropEffect((OleDropEffect)(uint)finalEffect));
    }

    private static OleDropEffect ToOleDropEffect(DragDropEffects effects)
    {
        var result = OleDropEffect.None;
        if (effects.HasFlag(DragDropEffects.Copy))
        {
            result |= OleDropEffect.Copy;
        }

        if (effects.HasFlag(DragDropEffects.Move))
        {
            result |= OleDropEffect.Move;
        }

        if (effects.HasFlag(DragDropEffects.Link))
        {
            result |= OleDropEffect.Link;
        }

        return result;
    }

    private static DragDropEffects FromOleDropEffect(OleDropEffect effect)
    {
        var result = DragDropEffects.None;
        if (effect.HasFlag(OleDropEffect.Copy))
        {
            result |= DragDropEffects.Copy;
        }

        if (effect.HasFlag(OleDropEffect.Move))
        {
            result |= DragDropEffects.Move;
        }

        if (effect.HasFlag(OleDropEffect.Link))
        {
            result |= DragDropEffects.Link;
        }

        return result;
    }
}