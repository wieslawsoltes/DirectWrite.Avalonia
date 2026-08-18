using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;

namespace Win32.Avalonia;

internal sealed class DxgiSwapchainWindow(DxgiConnection connection, EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo window)
    : EglGlPlatformSurfaceBase
{
    private readonly DxgiConnection _connection = connection;
    private readonly EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo _window = window;

    public override IGlPlatformSurfaceRenderTarget CreateGlRenderTarget(IGlContext context)
    {
        var eglContext = (EglContext)context;
        using (eglContext.EnsureCurrent())
        {
            return new DxgiRenderTarget(_window, eglContext, _connection);
        }
    }
}