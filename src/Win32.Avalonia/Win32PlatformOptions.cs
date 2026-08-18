using Avalonia.OpenGL;
using Avalonia.Platform;

namespace Win32.Avalonia;

public enum Win32RenderingMode
{
    Software = 1,
    AngleEgl = 2,
    Wgl = 3,
    Vulkan = 4
}

public enum Win32DpiAwareness
{
    Unaware,
    SystemDpiAware,
    PerMonitorDpiAware
}

public enum Win32CompositionMode
{
    WinUIComposition = 1,
    DirectComposition = 2,
    LowLatencyDxgiSwapChain = 3,
    RedirectionSurface = 4
}

public sealed class Win32PlatformOptions
{
    public bool OverlayPopups { get; set; }

    public IReadOnlyList<Win32RenderingMode> RenderingMode { get; set; } = new[]
    {
        Win32RenderingMode.AngleEgl,
        Win32RenderingMode.Software
    };

    public IReadOnlyList<Win32CompositionMode> CompositionMode { get; set; } = new[]
    {
        Win32CompositionMode.WinUIComposition,
        Win32CompositionMode.DirectComposition,
        Win32CompositionMode.RedirectionSurface
    };

    public float? WinUICompositionBackdropCornerRadius { get; set; }

    public bool ShouldRenderOnUIThread { get; set; }

    public IList<GlVersion> WglProfiles { get; set; } = new List<GlVersion>
    {
        new(GlProfileType.OpenGL, 4, 0),
        new(GlProfileType.OpenGL, 3, 2)
    };

    public IPlatformGraphics? CustomPlatformGraphics { get; set; }

    public Win32DpiAwareness DpiAwareness { get; set; } = Win32DpiAwareness.PerMonitorDpiAware;

    public Func<IReadOnlyList<PlatformGraphicsDeviceAdapterDescription>, int>? GraphicsAdapterSelectionCallback { get; set; }
}