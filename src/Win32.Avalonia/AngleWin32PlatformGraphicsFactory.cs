using Avalonia.Logging;
using Avalonia.Platform;

namespace Win32.Avalonia;

internal static class AngleWin32PlatformGraphicsFactory
{
    public static IPlatformGraphics? TryCreate(global::Avalonia.Win32.AngleOptions? options)
    {
        Win32AngleEglInterface egl;

        try
        {
            egl = new Win32AngleEglInterface();
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, "OpenGL")
                ?.Log(null, "Unable to load ANGLE: {0}", ex);
            return null;
        }

        var allowedPlatformApis = options?.AllowedPlatformApis ?? new[] { global::Avalonia.Win32.AngleOptions.PlatformApi.DirectX11 };

        foreach (var api in allowedPlatformApis.Distinct())
        {
            switch (api)
            {
                case global::Avalonia.Win32.AngleOptions.PlatformApi.DirectX11:
                    if (D3D11AngleWin32PlatformGraphics.TryCreate(egl) is { } graphics)
                    {
                        return graphics;
                    }
                    break;

                default:
                    Logger.TryGet(LogEventLevel.Error, "OpenGL")
                        ?.Log(null, "Unknown or unsupported requested PlatformApi {0}", api);
                    break;
            }
        }

        return null;
    }
}