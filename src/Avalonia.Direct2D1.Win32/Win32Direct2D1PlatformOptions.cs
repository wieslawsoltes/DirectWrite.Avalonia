using Avalonia.Platform;
using Avalonia.Win32;

namespace Avalonia.Direct2D1.Win32;

public sealed class Win32Direct2D1PlatformOptions
{
    public IReadOnlyList<Win32RenderingMode> RenderingMode { get; set; } = new[]
    {
        Win32RenderingMode.Software
    };

    public IReadOnlyList<Win32CompositionMode> CompositionMode { get; set; } = new[]
    {
        Win32CompositionMode.RedirectionSurface
    };

    public float? WinUICompositionBackdropCornerRadius { get; set; }

    public bool ShouldRenderOnUIThread { get; set; }

    public IList<Avalonia.OpenGL.GlVersion> WglProfiles { get; set; } = new List<Avalonia.OpenGL.GlVersion>
    {
        new(Avalonia.OpenGL.GlProfileType.OpenGL, 4, 0),
        new(Avalonia.OpenGL.GlProfileType.OpenGL, 3, 2)
    };

    public Win32DpiAwareness DpiAwareness { get; set; } = Win32DpiAwareness.PerMonitorDpiAware;

    public Func<IReadOnlyList<PlatformGraphicsDeviceAdapterDescription>, int>? GraphicsAdapterSelectionCallback { get; set; }

    public Direct2D1Options Direct2D1 { get; set; } = new();

    internal void Validate()
    {
        if (RenderingMode.Any(mode => mode != Win32RenderingMode.Software))
        {
            throw new NotSupportedException(
                "The standalone Direct2D1 backend currently supports only Win32RenderingMode.Software. " +
                "AngleEgl, Wgl, and Vulkan require platform surface integration that is not yet wired into this package.");
        }

        if (CompositionMode.Any(mode => mode != Win32CompositionMode.RedirectionSurface))
        {
            throw new NotSupportedException(
                "The standalone Direct2D1 backend currently supports only Win32CompositionMode.RedirectionSurface. " +
                "Composition-backed texture modes are not yet implemented in this package.");
        }
    }

    internal Win32PlatformOptions ToAvaloniaOptions()
    {
        Validate();

        return new Win32PlatformOptions
        {
            RenderingMode = RenderingMode.ToArray(),
            CompositionMode = CompositionMode.ToArray(),
            WinUICompositionBackdropCornerRadius = WinUICompositionBackdropCornerRadius,
            ShouldRenderOnUIThread = ShouldRenderOnUIThread,
            WglProfiles = WglProfiles,
            DpiAwareness = DpiAwareness,
            GraphicsAdapterSelectionCallback = GraphicsAdapterSelectionCallback
        };
    }
}
