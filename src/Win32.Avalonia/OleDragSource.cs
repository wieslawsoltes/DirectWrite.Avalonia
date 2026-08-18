using System.Runtime.InteropServices.Marshalling;

namespace Win32.Avalonia;

[GeneratedComClass]
internal partial class OleDragSource : IOleDropSource
{
    private const int DragDropSUseDefaultCursors = 0x00040102;
    private const int DragDropSDrop = 0x00040100;
    private const int DragDropSCancel = 0x00040101;
    private const int MkLButton = 0x0001;
    private const int MkRButton = 0x0002;
    private const int MkMButton = 0x0010;

    public int QueryContinueDrag(bool escapePressed, int keyState)
    {
        if (escapePressed)
        {
            return DragDropSCancel;
        }

        var pressedMouseButtons = 0;
        if ((keyState & MkLButton) == MkLButton)
        {
            pressedMouseButtons++;
        }

        if ((keyState & MkMButton) == MkMButton)
        {
            pressedMouseButtons++;
        }

        if ((keyState & MkRButton) == MkRButton)
        {
            pressedMouseButtons++;
        }

        if (pressedMouseButtons >= 2)
        {
            return DragDropSCancel;
        }

        return pressedMouseButtons == 0 ? DragDropSDrop : OleHResults.SOk;
    }

    public int GiveFeedback(OleDropEffect effect)
        => DragDropSUseDefaultCursors;
}