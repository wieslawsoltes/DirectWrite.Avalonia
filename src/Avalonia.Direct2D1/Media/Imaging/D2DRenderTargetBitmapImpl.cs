using System;
using System.IO;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Utilities;
using Avalonia.Direct2D1.Interop;
using Avalonia.Direct2D1.Interop.Direct2D1;
using D2DBitmap = Avalonia.Direct2D1.Interop.Direct2D1.Bitmap;

namespace Avalonia.Direct2D1.Media.Imaging
{
    internal class D2DRenderTargetBitmapImpl : D2DBitmapImpl, IDrawingContextLayerImpl, ILayerFactory
    {
        private readonly BitmapRenderTarget _renderTarget;

        public D2DRenderTargetBitmapImpl(BitmapRenderTarget renderTarget)
            : base(renderTarget.Bitmap)
        {
            _renderTarget = renderTarget;
        }

        public static D2DRenderTargetBitmapImpl CreateCompatible(
            Avalonia.Direct2D1.Interop.Direct2D1.RenderTarget renderTarget,
            Size size)
        {
            var bitmapRenderTarget = new BitmapRenderTarget(
                renderTarget,
                CompatibleRenderTargetOptions.None,
                new Size2F((float)size.Width, (float)size.Height));
            return new D2DRenderTargetBitmapImpl(bitmapRenderTarget);
        }

        public IDrawingContextImpl CreateDrawingContext(bool useScaledDrawing)
        {
            return new DrawingContextImpl( this, _renderTarget, useScaledDrawing, 
                null, () => Version++);
        }

        public IDrawingContextImpl CreateDrawingContext()
        {
            return CreateDrawingContext(true);
        }

        public bool IsCorrupted => false;

        public void Blit(IDrawingContextImpl context) => throw new NotSupportedException();

        public bool CanBlit => false;

        public IDrawingContextLayerImpl CreateLayer(Size size)
        {
            return CreateCompatible(_renderTarget, size);
        }

        public override void Dispose()
        {
            base.Dispose();
            _renderTarget.Dispose();
        }

        public override void Save(Stream stream, int? quality = null)
        {
            using (var wic = new WicRenderTargetBitmapImpl(PixelSize, Dpi))
            {
                using (var dc = wic.CreateDrawingContext(true, null))
                {
                    dc.DrawBitmap(
                        this,
                        1,
                        new Rect(PixelSize.ToSizeWithDpi(Dpi.X)),
                        new Rect(PixelSize.ToSizeWithDpi(Dpi.X)));
                }

                wic.Save(stream);
            }
        }
    }
}
