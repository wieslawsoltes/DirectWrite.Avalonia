using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia;

internal sealed class Win32Platform : IWindowingPlatform, IPlatformIconLoader, IPlatformLifetimeEventsImpl
{
    internal const nuint TimerIdDispatcher = 1;
    private const uint WmDispatchWorkItem = 0x0400;
    private const uint WmQueryEndSession = 0x0011;
    private const uint WmSettingChange = 0x001A;
    private const uint WmTimer = 0x0113;

    private static readonly Win32Platform s_instance = new();
    private static readonly LegacyWin32Bridge s_legacyBridge = LegacyWin32Bridge.Instance;

    private static Win32PlatformOptions? s_options;
    private static Compositor? s_compositor;

    private readonly SimpleWindow _messageWindow;
    private readonly Win32DispatcherImpl _dispatcher;

    private Win32Platform()
    {
        _messageWindow = new SimpleWindow(WndProc);
        _dispatcher = new Win32DispatcherImpl(_messageWindow.Handle);
        TrayIconImpl.ChangeWindowMessageFilter(_messageWindow.Handle);
    }

    internal static nint MessageWindowHandle => s_instance._messageWindow.Handle;

    public static Version WindowsVersion { get; } = Environment.OSVersion.Version;

    public static Win32PlatformOptions Options
        => s_options ?? throw new InvalidOperationException($"{nameof(Win32Platform)} has not been initialized.");

    internal static bool UseOverlayPopups => Options.OverlayPopups;

    internal static Compositor Compositor
        => s_compositor ?? throw new InvalidOperationException($"{nameof(Win32Platform)} has not been initialized.");

    public static void Initialize()
        => Initialize(new Win32PlatformOptions());

    public static void Initialize(Win32PlatformOptions options)
    {
        s_options = options;
        var legacyOptions = options.ToAvaloniaOptions();

        s_legacyBridge.SetLegacyOptions(legacyOptions);
        s_legacyBridge.ApplyLegacyDpiAwareness();
        Dispatcher.InitializeUIThreadDispatcher(s_instance._dispatcher);

        var clipboardImpl = new ClipboardImpl();
        var clipboard = new PlatformClipboard(clipboardImpl);
        var platformSettings = new Win32PlatformSettings();
        var screenImpl = new ScreenImpl();
        var renderTimer = options.ShouldRenderOnUIThread ? new UiThreadRenderTimer(60) : new DefaultRenderTimer(60);

        AvaloniaLocator.CurrentMutable
            .Bind<IClipboardImpl>().ToConstant(clipboardImpl)
            .Bind<IClipboard>().ToConstant(clipboard)
            .Bind<ICursorFactory>().ToConstant(CursorFactory.Instance)
            .Bind<IKeyboardDevice>().ToConstant(WindowsKeyboardDevice.Instance)
            .Bind<IPlatformSettings>().ToConstant(platformSettings)
            .Bind<IScreenImpl>().ToConstant(screenImpl)
            .Bind<IRenderLoop>().ToConstant(RenderLoop.FromTimer(renderTimer))
            .Bind<IWindowingPlatform>().ToConstant(s_instance)
            .Bind<PlatformHotkeyConfiguration>().ToConstant(new PlatformHotkeyConfiguration(KeyModifiers.Control)
            {
                OpenContextMenu =
                {
                    new KeyGesture(Key.F10, KeyModifiers.Shift)
                }
            })
            .Bind<KeyGestureFormatInfo>().ToConstant(new KeyGestureFormatInfo(new Dictionary<Key, string>(), meta: "Win"))
            .Bind<IPlatformIconLoader>().ToConstant(s_instance)
            .Bind<IMountedVolumeInfoProvider>().ToConstant(new WindowsMountedVolumeInfoProvider())
            .Bind<IPlatformLifetimeEventsImpl>().ToConstant(s_instance);

        IPlatformGraphics? platformGraphics;
        if (options.CustomPlatformGraphics is not null)
        {
            if (options.CompositionMode.Contains(Win32CompositionMode.RedirectionSurface) == false)
            {
                throw new InvalidOperationException(
                    $"{nameof(Win32PlatformOptions)}.{nameof(Win32PlatformOptions.CustomPlatformGraphics)} is only compatible with {nameof(Win32CompositionMode)}.{nameof(Win32CompositionMode.RedirectionSurface)}.");
            }

            platformGraphics = options.CustomPlatformGraphics;
            AvaloniaLocator.CurrentMutable.Bind<IPlatformGraphics>().ToConstant(platformGraphics);
            if (platformGraphics is IPlatformGraphicsOpenGlContextFactory openGlFactory)
            {
                AvaloniaLocator.CurrentMutable.Bind<IPlatformGraphicsOpenGlContextFactory>().ToConstant(openGlFactory);
            }
        }
        else
        {
            platformGraphics = Win32GlManager.Initialize();
        }

        s_compositor = new Compositor(platformGraphics);
        s_legacyBridge.SetLegacyCompositor(s_compositor);
        AvaloniaLocator.CurrentMutable.Bind<Compositor>().ToConstant(s_compositor);

        if (OleContext.Current is not null)
        {
            AvaloniaLocator.CurrentMutable.Bind<IPlatformDragSource>().ToConstant(new DragSource());
        }
    }

    public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;

    private nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WmDispatchWorkItem
            && wParam.ToInt64() == Win32DispatcherImpl.SignalW
            && lParam.ToInt64() == Win32DispatcherImpl.SignalL)
        {
            _dispatcher.DispatchWorkItem();
        }

        if (msg == WmQueryEndSession && ShutdownRequested is not null)
        {
            var args = new ShutdownRequestedEventArgs();

            ShutdownRequested(this, args);
            if (args.Cancel)
            {
                return nint.Zero;
            }
        }

        if (msg == WmSettingChange
            && AvaloniaLocator.Current.GetService<IPlatformSettings>() is Win32PlatformSettings platformSettings)
        {
            var changedSetting = Marshal.PtrToStringUni(lParam);
            if (changedSetting is "ImmersiveColorSet" or "WindowsThemeElement")
            {
                platformSettings.OnColorValuesChanged();
            }
        }

        if (msg == WmTimer && wParam == (nint)TimerIdDispatcher)
        {
            _dispatcher.FireTimer();
        }

        TrayIconImpl.ProcessWindowMessage(hWnd, msg, wParam, lParam);
        return PInvoke.DefWindowProc(new HWND(hWnd), msg, new WPARAM((nuint)wParam), new LPARAM(lParam));
    }

    public ITrayIconImpl? CreateTrayIcon() => new TrayIconImpl();

    public IWindowImpl CreateWindow() => new WindowImpl();

    public ITopLevelImpl CreateEmbeddableTopLevel() => CreateEmbeddableWindow();

    public IWindowImpl CreateEmbeddableWindow()
    {
        var window = new EmbeddedWindowImpl();
        window.Show(false, false);
        return window;
    }

    public IWindowIconImpl LoadIcon(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        return new IconImpl(stream);
    }

    public IWindowIconImpl LoadIcon(Stream stream) => new IconImpl(stream);

    public IWindowIconImpl LoadIcon(IBitmapImpl bitmap)
    {
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream);
        memoryStream.Position = 0;
        return new IconImpl(memoryStream);
    }

    public void GetWindowsZOrder(ReadOnlySpan<IWindowImpl> windows, Span<long> zOrder)
    {
        for (var index = 0; index < windows.Length; index++)
        {
            zOrder[index] = index;
        }
    }
}