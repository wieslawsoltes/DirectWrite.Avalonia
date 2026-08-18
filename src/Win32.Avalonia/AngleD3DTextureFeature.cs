using System.Runtime.InteropServices;
using global::Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using global::Avalonia.Win32.DirectX;

namespace Win32.Avalonia;

internal sealed class AngleD3DTextureFeature : IGlPlatformSurfaceRenderTargetFactory
{
    public bool CanRenderToSurface(IGlContext context, IPlatformRenderSurface surface)
        => context is EglContext { Display: AngleWin32EglDisplay }
           && surface is IDirect3D11TexturePlatformSurface;

    public IGlPlatformSurfaceRenderTarget CreateRenderTarget(IGlContext context, IPlatformRenderSurface surface)
    {
        var eglContext = (EglContext)context;
        var angleDisplay = (AngleWin32EglDisplay)eglContext.Display;
        var textureSurface = (IDirect3D11TexturePlatformSurface)surface;

        try
        {
            var target = textureSurface.CreateRenderTarget(context, angleDisplay.GetDirect3DDevice());
            return new RenderTargetWrapper(eglContext, angleDisplay, target);
        }
        catch (COMException ex) when (ex.HResult.IsDeviceLostError())
        {
            eglContext.NotifyContextLost();
            throw;
        }
    }

    private sealed class RenderTargetWrapper : EglPlatformSurfaceRenderTargetBase
    {
        private readonly AngleWin32EglDisplay _angleDisplay;
        private readonly IDirect3D11TextureRenderTarget _target;

        public RenderTargetWrapper(EglContext context, AngleWin32EglDisplay angleDisplay, IDirect3D11TextureRenderTarget target)
            : base(context)
        {
            _angleDisplay = angleDisplay;
            _target = target;
        }

        public override PlatformRenderTargetState State
            => base.IsCorrupted ? PlatformRenderTargetState.Corrupted : _target.State;

        public override IGlPlatformSurfaceRenderingSession BeginDrawCore(IRenderTarget.RenderTargetSceneInfo sceneInfo)
        {
            var success = false;
            var contextLock = Context.EnsureCurrent();
            IDirect3D11TextureRenderTargetRenderSession? session = null;
            EglSurface? surface = null;

            try
            {
                session = _target.BeginDraw();
                surface = _angleDisplay.WrapDirect3D11Texture(
                    session.D3D11Texture2D,
                    session.Offset.X,
                    session.Offset.Y,
                    session.Size.Width,
                    session.Size.Height);

                var result = BeginDraw(surface, session.Size, session.Scaling, () =>
                {
                    using (contextLock)
                    using (session)
                    using (surface)
                    {
                    }
                }, true);

                success = true;
                return result;
            }
            catch (RenderTargetCorruptedException ex) when (ex.InnerException is COMException com && com.HResult.IsDeviceLostError())
            {
                Context.NotifyContextLost();
                throw;
            }
            finally
            {
                if (!success)
                {
                    using (contextLock)
                    using (session)
                    using (surface)
                    {
                    }
                }
            }
        }

        public override void Dispose()
        {
            _target.Dispose();
            base.Dispose();
        }
    }
}