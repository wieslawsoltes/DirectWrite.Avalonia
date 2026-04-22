using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Platform;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia;

internal sealed class PopupImpl : WindowImpl, IPopupImpl
{
    private readonly IWindowBaseImpl _parent;
    private bool _dropShadowHint = true;

    public PopupImpl(IWindowBaseImpl parent)
        : base(new SimpleWindow.Options
        {
            Style = WINDOW_STYLE.WS_POPUP | WINDOW_STYLE.WS_CLIPSIBLINGS | WINDOW_STYLE.WS_CLIPCHILDREN,
            ExtendedStyle = WINDOW_EX_STYLE.WS_EX_TOOLWINDOW | WINDOW_EX_STYLE.WS_EX_TOPMOST,
            Parent = parent.Handle?.Handle ?? nint.Zero,
        })
    {
        _parent = parent;
        PopupPositioner = new ManagedPopupPositioner(new ManagedPopupPositionerPopupImplHelper(parent, MoveResize));
        ShowTaskbarIcon(false);
        CanResize(false);
        SetCanMinimize(false);
        SetCanMaximize(false);
        SetWindowDecorations(WindowDecorations.None);
    }

    public IPopupPositioner PopupPositioner { get; }

    public override void Show(bool activate, bool isDialog)
        => base.Show(false, isDialog);

    public override Size MaxAutoSizeHint
        => (_parent.TryGetFeature<IScreenImpl>() as ScreenImpl)?.ScreenFromHwnd(Handle?.Handle ?? nint.Zero)?.WorkingArea.ToRect(RenderScaling).Size
           ?? Size.Infinity;

    public void SetWindowManagerAddShadowHint(bool enabled)
    {
        _dropShadowHint = enabled;
        _ = _dropShadowHint;
    }

    public void TakeFocus()
        => _parent.Activate();

    protected override nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == 0x0021)
        {
            return 3;
        }

        if (msg == 0x007E)
        {
            return base.WndProc(hWnd, msg, wParam, lParam);
        }

        return base.WndProc(hWnd, msg, wParam, lParam);
    }

    private void MoveResize(PixelPoint position, Size size, double scaling)
    {
        Move(position);
        Resize(size, WindowResizeReason.Layout);
    }
}