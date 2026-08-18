using Avalonia.OpenGL.Egl;
using Avalonia.Platform.Surfaces;

namespace Win32.Avalonia;

internal interface IWindowsSurfaceFactory
{
    bool RequiresNoRedirectionBitmap { get; }

    IPlatformRenderSurface CreateSurface(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info);
}