using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Input.Raw;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia;

internal class WindowImpl : IWindowImpl, EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo
{
    private const uint WmClose = 0x0010;
    private const uint WmDestroy = 0x0002;
    private const uint WmMove = 0x0003;
    private const uint WmSize = 0x0005;
    private const uint WmSetFocus = 0x0007;
    private const uint WmKillFocus = 0x0008;
    private const uint WmPaint = 0x000F;
    private const uint WmEraseBkgnd = 0x0014;
    private const uint WmSetCursor = 0x0020;
    private const uint WmDpiChanged = 0x02E0;
    private const uint WmSysCommand = 0x0112;
    private const uint WmLButtonUp = 0x0202;
    private const uint WmNcLButtonDown = 0x00A1;
    private const uint WmGetIcon = 0x007F;
    private const uint WmSetIcon = 0x0080;
    private const int SizeRestored = 0;
    private const int SizeMinimized = 1;
    private const int SizeMaximized = 2;
    private const int ShowHide = 0;
    private const int ShowNormal = 1;
    private const int ShowMinimized = 2;
    private const int ShowMaximized = 3;
    private const int ShowNoActivate = 4;
    private const int ShowRestore = 9;
    private const uint SwpNosize = 0x0001;
    private const uint SwpNomove = 0x0002;
    private const uint SwpNozorder = 0x0004;
    private const uint SwpNoactivate = 0x0010;
    private const uint SwpFramechanged = 0x0020;
    private const int HwndTopmost = -1;
    private const int HwndNotTopmost = -2;
    private const int ScMouseMove = 0xF012;
    private const int HtLeft = 10;
    private const int HtRight = 11;
    private const int HtTop = 12;
    private const int HtTopLeft = 13;
    private const int HtTopRight = 14;
    private const int HtBottom = 15;
    private const int HtBottomLeft = 16;
    private const int HtBottomRight = 17;
    private const string CursorHandleType = "HCURSOR";

    private static readonly Dictionary<WindowEdge, int> s_edgeLookup = new()
    {
        { WindowEdge.East, HtRight },
        { WindowEdge.North, HtTop },
        { WindowEdge.NorthEast, HtTopRight },
        { WindowEdge.NorthWest, HtTopLeft },
        { WindowEdge.South, HtBottom },
        { WindowEdge.SouthEast, HtBottomRight },
        { WindowEdge.SouthWest, HtBottomLeft },
        { WindowEdge.West, HtLeft },
    };

    private readonly SimpleWindow _window;
    private readonly WindowHandleSurface _handleSurface;
    private readonly FramebufferManager _framebuffer;
    private readonly IPlatformRenderSurface[] _surfaces;
    private readonly ScreenImpl _screen;
    private readonly ICompositionEffectsSurface? _compositionEffectsSurface;
    private readonly IPlatformRenderSurface? _gpuSurface;
    private readonly nint _defaultCursorHandle = WindowNative.LoadCursor(0, new nint(WindowNative.IdcArrow));

    private IInputRoot? _inputRoot;
    private IconImpl? _iconImpl;
    private Win32.Avalonia.Interop.Win32Icon? _smallIcon;
    private Win32.Avalonia.Interop.Win32Icon? _bigIcon;
    private Size _minSize = new(0, 0);
    private Size _maxSize = Size.Infinity;
    private WindowState _windowState = WindowState.Normal;
    private WindowResizeReason _resizeReason = WindowResizeReason.Application;
    private bool _topmost;
    private bool _showTaskbarIcon = true;
    private bool _canResize = true;
    private bool _canMinimize = true;
    private bool _canMaximize = true;
    private WindowDecorations _windowDecorations = WindowDecorations.Full;
    private nint _cursorHandle;
    private PlatformThemeVariant _currentThemeVariant = PlatformThemeVariant.Light;
    private WindowTransparencyLevel _transparencyLevel = WindowTransparencyLevel.None;
    private PlatformAllowedWindowActions _allowedWindowActions = PlatformAllowedWindowActions.All;

    protected WindowImpl(SimpleWindow.Options options)
    {
        _screen = (AvaloniaLocator.Current.GetService<IScreenImpl>() as ScreenImpl) ?? new ScreenImpl();
        _window = new SimpleWindow(WndProc, options);
        _handleSurface = new WindowHandleSurface(() => _window.Handle, GetPixelSize, () => RenderScaling);
        _framebuffer = new FramebufferManager(_window.Handle);
        _gpuSurface = CreateGpuSurface();
        _compositionEffectsSurface = _gpuSurface as ICompositionEffectsSurface;
        _surfaces = _gpuSurface is null
            ? [_handleSurface, _framebuffer]
            : [_handleSurface, _gpuSurface, _framebuffer];
        _cursorHandle = _defaultCursorHandle;
        UpdateAllowedWindowActions();
        UpdateScaling();
    }

    public WindowImpl()
        : this(new SimpleWindow.Options())
    {
    }

    public virtual WindowState WindowState
    {
        get => _windowState;
        set
        {
            _windowState = value;
            PInvoke.ShowWindow(new HWND(_window.Handle), (SHOW_WINDOW_CMD)(value switch
            {
                WindowState.Minimized => ShowMinimized,
                WindowState.Maximized => ShowMaximized,
                _ => ShowRestore,
            }));
            WindowStateChanged?.Invoke(value);
        }
    }

    public bool WindowStateGetterIsUsable => true;

    public Action<WindowState>? WindowStateChanged { get; set; }

    public Action? Activated { get; set; }

    public Func<WindowCloseReason, bool>? Closing { get; set; }

    public Action? Closed { get; set; }

    public Action? Deactivated { get; set; }

    public Action<RawInputEventArgs>? Input { get; set; }

    public Action<Rect>? Paint { get; set; }

    public Action<Size, WindowResizeReason>? Resized { get; set; }

    public Action<double>? ScalingChanged { get; set; }

    public Action<PixelPoint>? PositionChanged { get; set; }

    public Action? LostFocus { get; set; }

    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }

    public Action<bool>? ExtendClientAreaToDecorationsChanged { get; set; }

    public Action? GotInputWhenDisabled { get; set; }

    public PlatformAllowedWindowActions AllowedWindowActions => _allowedWindowActions;

    public Action<PlatformAllowedWindowActions>? AllowedWindowActionsChanged { get; set; }

    public double DesktopScaling => RenderScaling;

    public IPlatformHandle? Handle => _handleSurface;

    public Size ClientSize => new(GetPixelSize().Width / RenderScaling, GetPixelSize().Height / RenderScaling);

    public double RenderScaling { get; private set; } = 1;

    public IPlatformRenderSurface[] Surfaces => _surfaces;

    public Compositor Compositor => Win32Platform.Compositor;

    public Size? FrameSize
    {
        get
        {
            if (!WindowNative.GetWindowRect(_window.Handle, out var rect))
            {
                return null;
            }

            return new Size(rect.right - rect.left, rect.bottom - rect.top) / RenderScaling;
        }
    }

    public PixelPoint Position
    {
        get
        {
            if (!WindowNative.GetWindowRect(_window.Handle, out var rect))
            {
                return default;
            }

            return new PixelPoint(rect.left, rect.top);
        }
    }

    public virtual Size MaxAutoSizeHint
    {
        get
        {
            var screen = _screen.ScreenFromHwnd(_window.Handle);
            return screen?.WorkingArea.ToRect(RenderScaling).Size ?? Size.Infinity;
        }
    }

    public bool IsClientAreaExtendedToDecorations => false;

    public bool NeedsManagedDecorations => false;

    public PlatformRequestedDrawnDecoration RequestedDrawnDecorations => PlatformRequestedDrawnDecoration.None;

    public Thickness ExtendedMargins => default;

    public Thickness OffScreenMargin => default;

    public WindowTransparencyLevel TransparencyLevel => _transparencyLevel;

    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels => default;

    IntPtr EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo.Handle => _window.Handle;

    PixelSize EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo.Size => GetPixelSize();

    double EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo.Scaling => RenderScaling;

    public virtual void Show(bool activate, bool isDialog)
    {
        var command = !activate ? ShowNoActivate : _windowState switch
        {
            WindowState.Minimized => ShowMinimized,
            WindowState.Maximized => ShowMaximized,
            _ => ShowNormal,
        };

        PInvoke.ShowWindow(new HWND(_window.Handle), (SHOW_WINDOW_CMD)command);
        PInvoke.UpdateWindow(new HWND(_window.Handle));
    }

    public void Hide()
        => PInvoke.ShowWindow(new HWND(_window.Handle), (SHOW_WINDOW_CMD)ShowHide);

    public void Activate()
        => WindowNative.SetForegroundWindow(_window.Handle);

    public void SetTopmost(bool value)
    {
        _topmost = value;
        PInvoke.SetWindowPos(
            new HWND(_window.Handle),
            new HWND(new IntPtr(value ? HwndTopmost : HwndNotTopmost)),
            0,
            0,
            0,
            0,
            (SET_WINDOW_POS_FLAGS)(SwpNomove | SwpNosize | SwpNoactivate));
    }

    public void SetTitle(string? title)
        => WindowNative.SetWindowText(_window.Handle, title);

    public void SetParent(IWindowImpl? parent)
    {
        WindowNative.SetWindowLongPtr(_window.Handle, WindowNative.GwlHwndParent, parent?.Handle?.Handle ?? nint.Zero);
    }

    public void SetEnabled(bool enable)
        => WindowNative.EnableWindow(_window.Handle, enable);

    public void SetWindowDecorations(WindowDecorations enabled)
    {
        if (_windowDecorations == enabled)
        {
            return;
        }

        _windowDecorations = enabled;
        UpdateWindowStyles();
    }

    public void SetIcon(IWindowIconImpl? icon)
    {
        if (ReferenceEquals(_iconImpl, icon))
        {
            return;
        }

        _iconImpl = icon as IconImpl;
        RefreshIcon();
    }

    public void ShowTaskbarIcon(bool value)
    {
        if (_showTaskbarIcon == value)
        {
            return;
        }

        _showTaskbarIcon = value;
        UpdateWindowStyles();
    }

    public void CanResize(bool value)
    {
        if (_canResize == value)
        {
            return;
        }

        _canResize = value;
        UpdateWindowStyles();
    }

    public void SetCanMinimize(bool value)
    {
        if (_canMinimize == value)
        {
            return;
        }

        _canMinimize = value;
        UpdateAllowedWindowActions(allowResize: null, allowMinimize: value, allowMaximize: null);
        UpdateWindowStyles();
    }

    public void SetCanMaximize(bool value)
    {
        if (_canMaximize == value)
        {
            return;
        }

        _canMaximize = value;
        UpdateAllowedWindowActions(allowResize: null, allowMinimize: null, allowMaximize: value);
        UpdateWindowStyles();
    }

    public void BeginMoveDrag(PointerPressedEventArgs e)
    {
        e.Pointer.Capture(null);

        Dispatcher.UIThread.Post(() =>
        {
            if (!e.Pointer.IsPrimary)
            {
                throw new InvalidOperationException("BeginMoveDrag failed.");
            }

            WindowNative.SendMessage(_window.Handle, WmSysCommand, ScMouseMove, 0);
            WindowNative.SendMessage(_window.Handle, WmLButtonUp, 0, 0);
        }, DispatcherPriority.Send);
    }

    public void BeginResizeDrag(WindowEdge edge, PointerPressedEventArgs e)
    {
        if (!_canResize || !s_edgeLookup.TryGetValue(edge, out var hitTest))
        {
            return;
        }

        e.Pointer.Capture(null);
        WindowNative.ReleaseCapture();
        PInvoke.DefWindowProc(new HWND(_window.Handle), WmNcLButtonDown, new WPARAM((nuint)hitTest), new LPARAM(0));
    }

    public void Resize(Size clientSize, WindowResizeReason reason = WindowResizeReason.Application)
    {
        _resizeReason = reason;
        var width = Math.Max(1, (int)Math.Ceiling(clientSize.Width * RenderScaling));
        var height = Math.Max(1, (int)Math.Ceiling(clientSize.Height * RenderScaling));
        PInvoke.SetWindowPos(
            new HWND(_window.Handle),
            new HWND(IntPtr.Zero),
            0,
            0,
            width,
            height,
            (SET_WINDOW_POS_FLAGS)(SwpNomove | SwpNozorder | SwpNoactivate));
    }

    public void Move(PixelPoint point)
    {
        PInvoke.SetWindowPos(
            new HWND(_window.Handle),
            new HWND(IntPtr.Zero),
            point.X,
            point.Y,
            0,
            0,
            (SET_WINDOW_POS_FLAGS)(SwpNosize | SwpNozorder | SwpNoactivate));
    }

    public void SetMinMaxSize(Size minSize, Size maxSize)
    {
        _minSize = minSize;
        _maxSize = maxSize;
    }

    public void SetExtendClientAreaToDecorationsHint(bool extendIntoClientAreaHint)
        => ExtendClientAreaToDecorationsChanged?.Invoke(false);

    public void SetExtendClientAreaTitleBarHeightHint(double titleBarHeight)
    {
    }

    public void SetInputRoot(IInputRoot inputRoot)
        => _inputRoot = inputRoot;

    public Point PointToClient(PixelPoint point)
        => new((point.X - Position.X) / RenderScaling, (point.Y - Position.Y) / RenderScaling);

    public PixelPoint PointToScreen(Point point)
        => new(Position.X + (int)Math.Round(point.X * RenderScaling), Position.Y + (int)Math.Round(point.Y * RenderScaling));

    public void SetCursor(ICursorImpl? cursor)
    {
        _cursorHandle = cursor is IPlatformHandle platformHandle
            && string.Equals(platformHandle.HandleDescriptor, CursorHandleType, StringComparison.Ordinal)
            ? platformHandle.Handle
            : _defaultCursorHandle;

        WindowNative.SetClassLongPtr(_window.Handle, WindowNative.GclpHCursor, _cursorHandle);
        WindowNative.SetCursor(_cursorHandle);
    }

    public virtual IPopupImpl? CreatePopup() => new PopupImpl(this);

    public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
    {
        foreach (var level in transparencyLevels)
        {
            if (!TryApplyTransparency(level))
            {
                continue;
            }

            if (_transparencyLevel != level)
            {
                _transparencyLevel = level;
                TransparencyLevelChanged?.Invoke(_transparencyLevel);
            }

            return;
        }

        if (_transparencyLevel != WindowTransparencyLevel.None)
        {
            _transparencyLevel = WindowTransparencyLevel.None;
            TransparencyLevelChanged?.Invoke(_transparencyLevel);
        }
    }

    public void SetFrameThemeVariant(PlatformThemeVariant themeVariant)
    {
        _currentThemeVariant = themeVariant;

        if (Win32Platform.WindowsVersion.Build >= WinUiCompositionShared.MinHostBackdropVersion.Build)
        {
            DwmNative.TrySetWindowAttribute(
                _window.Handle,
                DwmNative.UseImmersiveDarkMode,
                themeVariant == PlatformThemeVariant.Dark ? 1 : 0);

            if (_transparencyLevel == WindowTransparencyLevel.Mica && _compositionEffectsSurface is not null)
            {
                _compositionEffectsSurface.SetBlur(GetMicaBlurEffect());
            }
        }
    }

    public virtual object? TryGetFeature(Type featureType)
    {
        if (featureType == typeof(IScreenImpl))
        {
            return _screen;
        }

        if (featureType == typeof(IClipboard))
        {
            return AvaloniaLocator.Current.GetService<IClipboard>();
        }

        return null;
    }

    public virtual void Dispose()
    {
        DisposeIcons();
        _framebuffer.Dispose();
        if (_gpuSurface is IDisposable disposableSurface)
        {
            disposableSurface.Dispose();
        }

        _window.Dispose();
    }

    protected virtual nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case WmClose:
                if (Closing?.Invoke(WindowCloseReason.WindowClosing) == true)
                {
                    return 0;
                }

                PInvoke.DestroyWindow(new HWND(hWnd));
                return 0;

            case WmDestroy:
                Closed?.Invoke();
                return 0;

            case WmMove:
                PositionChanged?.Invoke(Position);
                return 0;

            case WmSize:
                UpdateWindowState((int)wParam);
                Resized?.Invoke(ClientSize, _resizeReason);
                _resizeReason = WindowResizeReason.Application;
                return 0;

            case WmSetFocus:
                Activated?.Invoke();
                return 0;

            case WmKillFocus:
                Deactivated?.Invoke();
                LostFocus?.Invoke();
                return 0;

            case WmPaint:
            {
                var paintStruct = new WindowNative.PaintStruct { Reserved = new byte[32] };
                WindowNative.BeginPaint(hWnd, out paintStruct);
                WindowNative.EndPaint(hWnd, ref paintStruct);
                Paint?.Invoke(new Rect(default(Point), ClientSize));
                return 0;
            }

            case WmSetCursor:
                WindowNative.SetCursor(_cursorHandle);
                return 1;

            case WmEraseBkgnd:
                return 1;

            case WmDpiChanged:
                if (UpdateScaling())
                {
                    RefreshIcon();
                    ScalingChanged?.Invoke(RenderScaling);
                }

                _resizeReason = WindowResizeReason.DpiChange;
                Resized?.Invoke(ClientSize, _resizeReason);
                _resizeReason = WindowResizeReason.Application;
                return 0;
        }

        return PInvoke.DefWindowProc(new HWND(hWnd), msg, new WPARAM((nuint)wParam), new LPARAM(lParam));
    }

    private PixelSize GetPixelSize()
    {
        PInvoke.GetClientRect(new HWND(_window.Handle), out var rect);
        return new PixelSize(Math.Max(1, rect.right - rect.left), Math.Max(1, rect.bottom - rect.top));
    }

    private IPlatformRenderSurface? CreateGpuSurface()
    {
        var graphics = AvaloniaLocator.Current.GetService<IPlatformGraphics>();
        if (graphics is null)
        {
            return null;
        }

        if (AvaloniaLocator.Current.GetService<IWindowsSurfaceFactory>() is { } surfaceFactory)
        {
            return surfaceFactory.CreateSurface(this);
        }

        if (graphics is WglPlatformOpenGlInterface)
        {
            return new WglGlPlatformSurface(this);
        }

        var graphicsTypeName = graphics.GetType().FullName;
        if (graphicsTypeName is not null && graphicsTypeName.Contains("Angle", StringComparison.Ordinal))
        {
            return new EglGlPlatformSurface(this);
        }

        return null;
    }

    private bool UpdateScaling()
    {
        var dpi = PInvoke.GetDpiForWindow(new HWND(_window.Handle));
        if (dpi == 0)
        {
            dpi = 96;
        }

        var nextScaling = dpi / 96d;
        if (Math.Abs(nextScaling - RenderScaling) < double.Epsilon)
        {
            return false;
        }

        RenderScaling = nextScaling;
        return true;
    }

    private void UpdateWindowState(int sizeType)
    {
        var nextState = sizeType switch
        {
            SizeMinimized => WindowState.Minimized,
            SizeMaximized => WindowState.Maximized,
            _ => WindowState.Normal,
        };

        if (_windowState != nextState)
        {
            _windowState = nextState;
            WindowStateChanged?.Invoke(nextState);
        }
    }

    private void UpdateAllowedWindowActions(bool? allowResize = null, bool? allowMinimize = null, bool? allowMaximize = null)
    {
        var next = _allowedWindowActions;

        _ = allowResize;

        if (allowMinimize.HasValue)
        {
            next = allowMinimize.Value ? next | PlatformAllowedWindowActions.Minimize : next & ~PlatformAllowedWindowActions.Minimize;
        }

        if (allowMaximize.HasValue)
        {
            next = allowMaximize.Value ? next | PlatformAllowedWindowActions.Maximize : next & ~PlatformAllowedWindowActions.Maximize;
        }

        if (next != _allowedWindowActions)
        {
            _allowedWindowActions = next;
            AllowedWindowActionsChanged?.Invoke(next);
        }
    }

    private void UpdateWindowStyles()
    {
        var style = (WINDOW_STYLE)(uint)WindowNative.GetWindowLong(_window.Handle, WindowNative.GwlStyle);
        var exStyle = (WINDOW_EX_STYLE)(uint)WindowNative.GetWindowLong(_window.Handle, WindowNative.GwlExStyle);

        style &= ~(WINDOW_STYLE.WS_BORDER | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SYSMENU |
                   WINDOW_STYLE.WS_THICKFRAME | WINDOW_STYLE.WS_MINIMIZEBOX | WINDOW_STYLE.WS_MAXIMIZEBOX);
        exStyle &= ~(WINDOW_EX_STYLE.WS_EX_APPWINDOW | WINDOW_EX_STYLE.WS_EX_TOOLWINDOW);

        switch (_windowDecorations)
        {
            case WindowDecorations.Full:
                style |= WINDOW_STYLE.WS_BORDER | WINDOW_STYLE.WS_CAPTION | WINDOW_STYLE.WS_SYSMENU;
                break;

            case WindowDecorations.BorderOnly:
                style |= WINDOW_STYLE.WS_BORDER;
                break;
        }

        if (_windowDecorations != WindowDecorations.None && _canResize)
        {
            style |= WINDOW_STYLE.WS_THICKFRAME;
        }

        if (_canMinimize)
        {
            style |= WINDOW_STYLE.WS_MINIMIZEBOX;
        }

        if (_canMaximize)
        {
            style |= WINDOW_STYLE.WS_MAXIMIZEBOX;
        }

        if (_showTaskbarIcon)
        {
            exStyle |= WINDOW_EX_STYLE.WS_EX_APPWINDOW;
        }
        else
        {
            exStyle |= WINDOW_EX_STYLE.WS_EX_TOOLWINDOW;
        }

        WindowNative.SetWindowLong(_window.Handle, WindowNative.GwlStyle, unchecked((int)(uint)style));
        WindowNative.SetWindowLong(_window.Handle, WindowNative.GwlExStyle, unchecked((int)(uint)exStyle));
        PInvoke.SetWindowPos(
            new HWND(_window.Handle),
            new HWND(IntPtr.Zero),
            0,
            0,
            0,
            0,
            (SET_WINDOW_POS_FLAGS)(SwpNomove | SwpNosize | SwpNozorder | SwpNoactivate | SwpFramechanged));
    }

    private void RefreshIcon()
    {
        DisposeIcons();

        if (_iconImpl is not null)
        {
            _smallIcon = _iconImpl.LoadSmallIcon(RenderScaling);
            _bigIcon = _iconImpl.LoadBigIcon(RenderScaling);
        }

        WindowNative.SendMessage(_window.Handle, WmSetIcon, WindowNative.IconSmall, _smallIcon?.Handle ?? 0);
        WindowNative.SendMessage(_window.Handle, WmSetIcon, WindowNative.IconBig, _bigIcon?.Handle ?? 0);
    }

    private void DisposeIcons()
    {
        _smallIcon?.Dispose();
        _smallIcon = null;
        _bigIcon?.Dispose();
        _bigIcon = null;
    }

    private BlurEffect GetMicaBlurEffect()
        => _currentThemeVariant == PlatformThemeVariant.Light ? BlurEffect.MicaLight : BlurEffect.MicaDark;

    private bool SetUseHostBackdropBrush(bool useHostBackdropBrush)
    {
        if (Win32Platform.WindowsVersion.Build < WinUiCompositionShared.MinHostBackdropVersion.Build)
        {
            return true;
        }

        return DwmNative.TrySetWindowAttribute(
            _window.Handle,
            DwmNative.UseHostBackdropBrush,
            useHostBackdropBrush ? 1 : 0);
    }

    private bool TryApplyTransparency(WindowTransparencyLevel level)
    {
        if (level == WindowTransparencyLevel.None || level == WindowTransparencyLevel.Transparent)
        {
            _compositionEffectsSurface?.SetBlur(BlurEffect.None);
            return true;
        }

        if (level == WindowTransparencyLevel.Blur)
        {
            if (_compositionEffectsSurface?.IsBlurSupported(BlurEffect.GaussianBlur) != true)
            {
                return false;
            }

            _compositionEffectsSurface.SetBlur(BlurEffect.GaussianBlur);
            return true;
        }

        if (level == WindowTransparencyLevel.AcrylicBlur)
        {
            if (_compositionEffectsSurface?.IsBlurSupported(BlurEffect.Acrylic) != true)
            {
                return false;
            }

            SetUseHostBackdropBrush(true);
            _compositionEffectsSurface.SetBlur(BlurEffect.Acrylic);
            return true;
        }

        if (level == WindowTransparencyLevel.Mica)
        {
            var micaBlurEffect = GetMicaBlurEffect();
            if (_compositionEffectsSurface?.IsBlurSupported(micaBlurEffect) != true)
            {
                return false;
            }

            SetUseHostBackdropBrush(false);
            _compositionEffectsSurface.SetBlur(micaBlurEffect);
            return true;
        }

        return false;
    }
}