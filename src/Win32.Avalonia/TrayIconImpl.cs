using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Win32.Avalonia;

internal sealed class TrayIconImpl : ITrayIconImpl
{
    private const uint WmDisplayChange = 0x007E;
    private const uint WmLButtonUp = 0x0202;
    private const uint WmRButtonUp = 0x0205;
    private const uint WmTrayMouse = 0x0400 + 1024;

    private static readonly Dictionary<int, TrayIconImpl> s_trayIcons = new();
    private static readonly uint s_taskBarCreatedMessage = PInvoke.RegisterWindowMessage("TaskbarCreated");
    private static Win32.Avalonia.Interop.Win32Icon? s_emptyIcon;
    private static int s_nextUniqueId;

    private readonly int _uniqueId;
    private readonly Win32NativeToManagedMenuExporter _exporter = new();
    private IconImpl? _iconImpl;
    private Win32.Avalonia.Interop.Win32Icon? _icon;
    private bool _iconAdded;
    private bool _iconStale;
    private string? _tooltipText;
    private bool _disposed;

    public TrayIconImpl()
    {
        _uniqueId = ++s_nextUniqueId;
        s_trayIcons[_uniqueId] = this;
    }

    public Action? OnClicked { get; set; }

    public INativeMenuExporter MenuExporter => _exporter;

    internal static void ChangeWindowMessageFilter(nint hWnd)
    {
    }

    internal static void ProcessWindowMessage(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case WmTrayMouse:
                if (s_trayIcons.TryGetValue((int)wParam, out var tray))
                {
                    tray.WndProc(hWnd, msg, wParam, lParam);
                }
                break;
            case WmDisplayChange:
                foreach (var existingTray in s_trayIcons.Values)
                {
                    if (existingTray._iconAdded)
                    {
                        existingTray._iconStale = true;
                        existingTray.UpdateIcon();
                    }
                }
                break;
            default:
                if (msg == s_taskBarCreatedMessage)
                {
                    foreach (var existingTray in s_trayIcons.Values)
                    {
                        if (existingTray._iconAdded)
                        {
                            existingTray.UpdateIcon(remove: true);
                            existingTray.UpdateIcon();
                        }
                    }
                }
                break;
        }
    }

    public void SetIcon(IWindowIconImpl? icon)
    {
        _iconImpl = icon as IconImpl;
        _iconStale = true;
        UpdateIcon();
    }

    public void SetIsVisible(bool visible)
    {
        UpdateIcon(remove: !visible);
    }

    public void SetToolTipText(string? text)
    {
        _tooltipText = text;
        UpdateIcon(remove: !_iconAdded);
    }

    private void UpdateIcon(bool remove = false)
    {
        Win32.Avalonia.Interop.Win32Icon? newIcon = null;
        if (_iconStale && _iconImpl is not null)
        {
            newIcon = _iconImpl.LoadSmallIcon(1.0);
        }

        var iconData = new TrayNative.NotifyIconData
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<TrayNative.NotifyIconData>(),
            hWnd = Win32Platform.MessageWindowHandle,
            uID = (uint)_uniqueId,
        };

        if (!remove)
        {
            iconData.uFlags = TrayNative.NotifyIconFlags.Tip | TrayNative.NotifyIconFlags.Message | TrayNative.NotifyIconFlags.Icon;
            iconData.uCallbackMessage = WmTrayMouse;
            iconData.hIcon = (_iconStale ? newIcon : _icon)?.Handle ?? GetOrCreateEmptyIcon().Handle;
            iconData.SetToolTip(_tooltipText);

            if (!_iconAdded)
            {
                TrayNative.Shell_NotifyIcon(TrayNative.NotifyIconMessage.Add, ref iconData);
                _iconAdded = true;
            }
            else
            {
                TrayNative.Shell_NotifyIcon(TrayNative.NotifyIconMessage.Modify, ref iconData);
            }
        }
        else
        {
            TrayNative.Shell_NotifyIcon(TrayNative.NotifyIconMessage.Delete, ref iconData);
            _iconAdded = false;
        }

        if (_iconStale)
        {
            _icon?.Dispose();
            _icon = newIcon;
            _iconStale = false;
        }
    }

    private static Win32.Avalonia.Interop.Win32Icon GetOrCreateEmptyIcon()
    {
        if (s_emptyIcon is null)
        {
            using var bitmap = new WriteableBitmap(new PixelSize(32, 32), new Vector(96, 96), PixelFormats.Bgra8888, AlphaFormat.Unpremul);
            s_emptyIcon = new Win32.Avalonia.Interop.Win32Icon(bitmap);
        }

        return s_emptyIcon;
    }

    private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WmTrayMouse)
        {
            switch ((uint)lParam)
            {
                case WmLButtonUp:
                    OnClicked?.Invoke();
                    break;
                case WmRButtonUp:
                    OnRightClicked();
                    break;
            }

            return 0;
        }

        return PInvoke.DefWindowProc(new HWND(hWnd), msg, new WPARAM((nuint)wParam), new LPARAM(lParam));
    }

    private void OnRightClicked()
    {
        var menu = _exporter.GetNativeMenu();
        if (menu is null || menu.Items.Count == 0)
        {
            return;
        }

        var trayMenu = new TrayPopupRoot
        {
            Name = "AvaloniaTrayPopupRoot_" + _tooltipText,
            WindowDecorations = WindowDecorations.None,
            SizeToContent = SizeToContent.WidthAndHeight,
            Background = null,
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
            Content = new TrayIconMenuFlyoutPresenter { ItemsSource = menu.Items }
        };

        PInvoke.GetCursorPos(out var point);
        trayMenu.Position = new PixelPoint(point.X, point.Y);
        trayMenu.Show();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        UpdateIcon(remove: true);
        _icon?.Dispose();
        s_trayIcons.Remove(_uniqueId);
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~TrayIconImpl()
    {
        Dispose();
    }

    private sealed class TrayIconMenuFlyoutPresenter : MenuFlyoutPresenter
    {
        protected override Type StyleKeyOverride => typeof(MenuFlyoutPresenter);

        public override void Close()
        {
            if (this.FindLogicalAncestorOfType<TrayPopupRoot>() is { } host)
            {
                SelectedIndex = -1;
                host.Close();
            }
        }

        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return base.CreateContainerForItemOverride(item, index, recycleKey);
        }
    }

    private sealed class TrayPopupRoot : Window
    {
        private readonly ManagedPopupPositioner _positioner;
        private readonly TrayIconManagedPopupPositionerPopupImplHelper _positionerHelper;

        public TrayPopupRoot()
        {
            _positionerHelper = new TrayIconManagedPopupPositionerPopupImplHelper(MoveResize);
            _positioner = new ManagedPopupPositioner(_positionerHelper);
            Topmost = true;
            Deactivated += OnDeactivated;
            ShowInTaskbar = false;
            ShowActivated = true;
        }

        private void OnDeactivated(object? sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _positionerHelper.Dispose();
        }

        private void MoveResize(PixelPoint position, Size size, double scaling)
        {
            if (PlatformImpl is { } platformImpl)
            {
                platformImpl.Move(position);
                platformImpl.Resize(size, WindowResizeReason.Layout);
            }
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            base.ArrangeCore(finalRect);

            _positioner.Update(new PopupPositionerParameters
            {
                Anchor = PopupAnchor.TopLeft,
                Gravity = PopupGravity.BottomRight,
                AnchorRectangle = new Rect(Position.ToPoint(Screens.Primary?.Scaling ?? 1.0), new Size(1, 1)),
                Size = finalRect.Size,
                ConstraintAdjustment = PopupPositionerConstraintAdjustment.FlipX | PopupPositionerConstraintAdjustment.FlipY,
            });
        }

        private sealed class TrayIconManagedPopupPositionerPopupImplHelper : IManagedPopupPositionerPopup, IDisposable
        {
            private readonly Action<PixelPoint, Size, double> _moveResize;
            private readonly Window _hiddenWindow;

            public TrayIconManagedPopupPositionerPopupImplHelper(Action<PixelPoint, Size, double> moveResize)
            {
                _moveResize = moveResize;
                _hiddenWindow = new Window();
            }

            public IReadOnlyList<ManagedPopupPositionerScreenInfo> Screens =>
                _hiddenWindow.Screens.All
                    .Select(screen => new ManagedPopupPositionerScreenInfo(screen.Bounds.ToRect(1), screen.Bounds.ToRect(1)))
                    .ToArray();

            public Rect ParentClientAreaScreenGeometry
            {
                get
                {
                    if (_hiddenWindow.Screens.Primary is { } screen)
                    {
                        var point = screen.Bounds.TopLeft;
                        var size = screen.Bounds.Size;
                        return new Rect(point.X, point.Y, size.Width * screen.Scaling, size.Height * screen.Scaling);
                    }

                    return default;
                }
            }

            public void MoveAndResize(Point devicePoint, Size virtualSize)
            {
                _moveResize(new PixelPoint((int)devicePoint.X, (int)devicePoint.Y), virtualSize, Scaling);
            }

            public void Dispose()
            {
                _hiddenWindow.Close();
            }

            public double Scaling => _hiddenWindow.Screens.Primary?.Scaling ?? 1.0;
        }
    }
}