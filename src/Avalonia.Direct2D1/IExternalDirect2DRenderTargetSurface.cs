using Avalonia.Platform.Surfaces;

namespace Avalonia.Direct2D1
{
    public interface IExternalDirect2DRenderTargetSurface : IPlatformRenderSurface
    {
        Avalonia.Direct2D1.Interop.Direct2D1.RenderTarget GetOrCreateRenderTarget();
        void DestroyRenderTarget();
        void BeforeDrawing();
        void AfterDrawing();
    }
}
