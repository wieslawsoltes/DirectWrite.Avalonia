using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Vulkan;
using Avalonia;
using global::Avalonia.Win32;

namespace Win32.Avalonia;

internal static class Win32GlManager
{
    public static IPlatformGraphics? Initialize()
    {
        var graphics = InitializeCore();

        if (graphics is not null)
        {
            AvaloniaLocator.CurrentMutable.Bind<IPlatformGraphics>().ToConstant(graphics);
        }

        if (graphics is IPlatformGraphicsOpenGlContextFactory openGlFactory)
        {
            AvaloniaLocator.CurrentMutable.Bind<IPlatformGraphicsOpenGlContextFactory>().ToConstant(openGlFactory);
        }

        return graphics;
    }

    private static IPlatformGraphics? InitializeCore()
    {
        var options = AvaloniaLocator.Current.GetService<Win32PlatformOptions>() ?? new Win32PlatformOptions();
        if (options.RenderingMode is null || options.RenderingMode.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(Win32PlatformOptions)}.{nameof(Win32PlatformOptions.RenderingMode)} must not be empty or null");
        }

        foreach (var renderingMode in options.RenderingMode)
        {
            switch (renderingMode)
            {
                case Win32RenderingMode.Software:
                    return null;
                case Win32RenderingMode.AngleEgl:
                    if (AngleWin32PlatformGraphicsFactory.TryCreate(AvaloniaLocator.Current.GetService<global::Avalonia.Win32.AngleOptions>() ?? new()) is { } angle)
                    {
                        TryRegisterComposition(options, angle);
                        return angle;
                    }
                    break;
                case Win32RenderingMode.Wgl:
                    if (WglPlatformOpenGlInterface.TryCreate() is { } wgl)
                    {
                        return wgl;
                    }
                    break;
                case Win32RenderingMode.Vulkan:
                    if (VulkanSupport.TryInitialize(AvaloniaLocator.Current.GetService<VulkanOptions>() ?? new()) is { } vulkan)
                    {
                        return vulkan;
                    }
                    break;
                default:
                    return LegacyWin32Bridge.Instance.InitializePlatformGraphics();
            }
        }

        throw new InvalidOperationException($"{nameof(Win32PlatformOptions)}.{nameof(Win32PlatformOptions.RenderingMode)} has a value of \"{string.Join(", ", options.RenderingMode)}\", but no options were applied.");
    }

    private static void TryRegisterComposition(Win32PlatformOptions options, IPlatformGraphics graphics)
    {
        if (!IsAngleD3D11Graphics(graphics))
        {
            return;
        }

        if (options.CompositionMode is null || options.CompositionMode.Count == 0)
        {
            throw new InvalidOperationException($"{nameof(Win32PlatformOptions)}.{nameof(Win32PlatformOptions.CompositionMode)} must not be empty or null");
        }

        foreach (var compositionMode in options.CompositionMode)
        {
            switch (compositionMode)
            {
                case Win32CompositionMode.WinUIComposition when WinUiCompositorConnection.TryCreateAndRegister():
                    return;
                case Win32CompositionMode.DirectComposition when DirectCompositionConnection.TryCreateAndRegister():
                    return;
                case Win32CompositionMode.LowLatencyDxgiSwapChain when DxgiConnection.TryCreateAndRegister():
                case Win32CompositionMode.RedirectionSurface:
                    return;
            }
        }

        throw new InvalidOperationException($"{nameof(Win32PlatformOptions)}.{nameof(Win32PlatformOptions.CompositionMode)} has a value of \"{string.Join(", ", options.CompositionMode)}\", but no options were applied.");
    }

    private static bool IsAngleD3D11Graphics(IPlatformGraphics graphics)
        => graphics is D3D11AngleWin32PlatformGraphics
           || string.Equals(graphics.GetType().FullName, "Avalonia.Win32.OpenGl.Angle.D3D11AngleWin32PlatformGraphics", StringComparison.Ordinal);
}