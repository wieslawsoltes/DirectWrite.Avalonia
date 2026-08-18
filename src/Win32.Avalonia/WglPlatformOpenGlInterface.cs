using System.Collections.Generic;
using Avalonia;
using Avalonia.Logging;
using Avalonia.OpenGL;
using Avalonia.Platform;

namespace Win32.Avalonia;

internal sealed class WglPlatformOpenGlInterface : IPlatformGraphics, IPlatformGraphicsOpenGlContextFactory
{
    private WglPlatformOpenGlInterface(WglContext primaryContext)
    {
        PrimaryContext = primaryContext;
    }

    public WglContext PrimaryContext { get; }

    public bool UsesSharedContext => false;

    public IPlatformGraphicsContext CreateContext()
        => CreateContext([PrimaryContext.Version]);

    public IPlatformGraphicsContext GetSharedContext()
        => throw new NotSupportedException();

    public IGlContext CreateContext(IEnumerable<GlVersion>? versions)
        => WglDisplay.CreateContext(versions ?? [PrimaryContext.Version], null)
           ?? throw new OpenGlException("Unable to create additional WGL context.");

    public static WglPlatformOpenGlInterface? TryCreate()
    {
        try
        {
            var options = AvaloniaLocator.Current.GetService<Win32PlatformOptions>() ?? new Win32PlatformOptions();
            if (WglDisplay.CreateContext(options.WglProfiles, null) is { } primary)
            {
                return new WglPlatformOpenGlInterface(primary);
            }
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, "OpenGL")?.Log("WGL", "Unable to initialize WGL: " + ex);
        }

        return null;
    }
}