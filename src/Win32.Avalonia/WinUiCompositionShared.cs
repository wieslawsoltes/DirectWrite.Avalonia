namespace Win32.Avalonia;

internal sealed class WinUiCompositionShared : IDisposable
{
    private readonly nint _blurBrushPointer;
    private readonly nint _compositor5Pointer;
    private readonly nint _compositorDesktopInteropPointer;
    private readonly nint _micaBrushDarkPointer;
    private readonly nint _micaBrushLightPointer;

    public WinUiCompositionShared(nint compositorPointer)
    {
        CompositorPointer = compositorPointer;
        Compositor = GeneratedComHelpers.ConvertToManaged<ICompositorCom>(compositorPointer)
            ?? throw new InvalidOperationException("Unable to wrap the WinUI compositor.");
        Compositor5 = GeneratedComHelpers.QueryInterface<ICompositor5Com>(compositorPointer, out _compositor5Pointer);
        DesktopInterop = GeneratedComHelpers.QueryInterface<ICompositorDesktopInteropCom>(compositorPointer, out _compositorDesktopInteropPointer);

        if (Win32Platform.WindowsVersion >= MinAcrylicVersion)
        {
            _blurBrushPointer = WinUiCompositionUtils.CreateAcrylicBlurBackdropBrush(compositorPointer);
        }

        if (Win32Platform.WindowsVersion >= MinHostBackdropVersion)
        {
            _micaBrushLightPointer = WinUiCompositionUtils.CreateMicaBackdropBrush(compositorPointer, 242, 0.6f);
            _micaBrushDarkPointer = WinUiCompositionUtils.CreateMicaBackdropBrush(compositorPointer, 32, 0.8f);
        }
    }

    public static readonly Version MinWinCompositionVersion = WinRTNativeMethods.MinWinCompositionVersion;
    public static readonly Version MinAcrylicVersion = new(10, 0, 15063);
    public static readonly Version MinHostBackdropVersion = new(10, 0, 22000);

    public object SyncRoot { get; } = new();

    public nint BlurBrushPointer => _blurBrushPointer;

    public ICompositorCom Compositor { get; }

    public ICompositor5Com Compositor5 { get; }

    public nint CompositorPointer { get; }

    public ICompositorDesktopInteropCom DesktopInterop { get; }

    public nint MicaBrushDarkPointer => _micaBrushDarkPointer;

    public nint MicaBrushLightPointer => _micaBrushLightPointer;

    public void Dispose()
    {
        GeneratedComHelpers.Free<ICompositionBrushCom>(_micaBrushDarkPointer);
        GeneratedComHelpers.Free<ICompositionBrushCom>(_micaBrushLightPointer);
        GeneratedComHelpers.Free<ICompositionBrushCom>(_blurBrushPointer);
        GeneratedComHelpers.Free<ICompositorDesktopInteropCom>(_compositorDesktopInteropPointer);
        GeneratedComHelpers.Free<ICompositor5Com>(_compositor5Pointer);
        GeneratedComHelpers.Free<ICompositorCom>(CompositorPointer);
    }
}