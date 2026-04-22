using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;

namespace Win32.Avalonia;

internal sealed class WglGlPlatformSurface(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info) : IGlPlatformSurface
{
    private readonly EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo _info = info;

    public IGlPlatformSurfaceRenderTarget CreateGlRenderTarget(IGlContext context)
        => new RenderTarget((WglContext)context, _info);

    private sealed class RenderTarget(WglContext context, EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info) : IGlPlatformSurfaceRenderTarget
    {
        private readonly WglContext _context = context;
        private readonly EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo _info = info;
        private nint _dc = context.CreateConfiguredDeviceContext(info.Handle);

        public PlatformRenderTargetState State => PlatformRenderTargetState.Ready;

        public void Dispose()
            => WglGdiResourceManager.ReleaseDc(_info.Handle, _dc);

        public IGlPlatformSurfaceRenderingSession BeginDraw(IRenderTarget.RenderTargetSceneInfo sceneInfo)
        {
            var restoreContext = _context.MakeCurrent(_dc);
            _context.GlInterface.BindFramebuffer(0x8D40, 0);
            return new Session(_context, _dc, _info, restoreContext);
        }

        private sealed class Session(WglContext context, nint dc, EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info, IDisposable restoreContext) : IGlPlatformSurfaceRenderingSession
        {
            private readonly IDisposable _restoreContext = restoreContext;

            public IGlContext Context { get; } = context;

            public PixelSize Size => info.Size;

            public double Scaling => info.Scaling;

            public bool IsYFlipped => false;

            public void Dispose()
            {
                context.GlInterface.Flush();
                PInvoke.SwapBuffers(new HDC(dc));
                _restoreContext.Dispose();
            }
        }
    }
}