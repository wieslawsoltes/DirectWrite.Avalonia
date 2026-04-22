using System.Linq;
using Avalonia.OpenGL;

namespace Win32.Avalonia;

internal static class AvaloniaWin32Compatibility
{
    public static global::Avalonia.Win32PlatformOptions ToAvaloniaOptions(this Win32PlatformOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.RenderingMode.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(Win32PlatformOptions)}.{nameof(Win32PlatformOptions.RenderingMode)} must not be empty.");
        }

        if (options.CompositionMode.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(Win32PlatformOptions)}.{nameof(Win32PlatformOptions.CompositionMode)} must not be empty.");
        }

        return new global::Avalonia.Win32PlatformOptions
        {
            OverlayPopups = options.OverlayPopups,
            RenderingMode = options.RenderingMode.Select(ConvertRenderingMode).ToArray(),
            CompositionMode = options.CompositionMode.Select(ConvertCompositionMode).ToArray(),
            WinUICompositionBackdropCornerRadius = options.WinUICompositionBackdropCornerRadius,
            ShouldRenderOnUIThread = options.ShouldRenderOnUIThread,
            WglProfiles = options.WglProfiles
                .Select(profile => new GlVersion(profile.Type, profile.Major, profile.Minor))
                .ToList(),
            CustomPlatformGraphics = options.CustomPlatformGraphics,
            DpiAwareness = ConvertDpiAwareness(options.DpiAwareness),
            GraphicsAdapterSelectionCallback = options.GraphicsAdapterSelectionCallback
        };
    }

    private static global::Avalonia.Win32RenderingMode ConvertRenderingMode(Win32RenderingMode renderingMode)
        => renderingMode switch
        {
            Win32RenderingMode.Software => global::Avalonia.Win32RenderingMode.Software,
            Win32RenderingMode.AngleEgl => global::Avalonia.Win32RenderingMode.AngleEgl,
            Win32RenderingMode.Wgl => global::Avalonia.Win32RenderingMode.Wgl,
            Win32RenderingMode.Vulkan => global::Avalonia.Win32RenderingMode.Vulkan,
            _ => throw new ArgumentOutOfRangeException(nameof(renderingMode), renderingMode, null)
        };

    private static global::Avalonia.Win32CompositionMode ConvertCompositionMode(Win32CompositionMode compositionMode)
        => compositionMode switch
        {
            Win32CompositionMode.WinUIComposition => global::Avalonia.Win32CompositionMode.WinUIComposition,
            Win32CompositionMode.DirectComposition => global::Avalonia.Win32CompositionMode.DirectComposition,
            Win32CompositionMode.LowLatencyDxgiSwapChain => global::Avalonia.Win32CompositionMode.LowLatencyDxgiSwapChain,
            Win32CompositionMode.RedirectionSurface => global::Avalonia.Win32CompositionMode.RedirectionSurface,
            _ => throw new ArgumentOutOfRangeException(nameof(compositionMode), compositionMode, null)
        };

    private static global::Avalonia.Win32DpiAwareness ConvertDpiAwareness(Win32DpiAwareness dpiAwareness)
        => dpiAwareness switch
        {
            Win32DpiAwareness.Unaware => global::Avalonia.Win32DpiAwareness.Unaware,
            Win32DpiAwareness.SystemDpiAware => global::Avalonia.Win32DpiAwareness.SystemDpiAware,
            Win32DpiAwareness.PerMonitorDpiAware => global::Avalonia.Win32DpiAwareness.PerMonitorDpiAware,
            _ => throw new ArgumentOutOfRangeException(nameof(dpiAwareness), dpiAwareness, null)
        };
}