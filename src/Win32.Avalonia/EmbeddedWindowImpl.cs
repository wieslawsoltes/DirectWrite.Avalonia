using Avalonia.Controls;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia;

internal sealed class EmbeddedWindowImpl : WindowImpl
{
    public EmbeddedWindowImpl()
        : base(new SimpleWindow.Options
        {
            Style = WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_CLIPCHILDREN,
            Parent = OffscreenParentWindow.Handle,
            X = 0,
            Y = 0,
            Width = 640,
            Height = 480,
        })
    {
        ShowTaskbarIcon(false);
        CanResize(false);
        SetCanMinimize(false);
        SetCanMaximize(false);
        SetWindowDecorations(WindowDecorations.None);
    }
}