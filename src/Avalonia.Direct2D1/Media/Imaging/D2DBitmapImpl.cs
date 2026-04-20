using System;
using System.IO;
using Avalonia.Direct2D1.Interop.Direct2D1;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.Graphics.Imaging;
using Windows.Win32.Graphics.Imaging.D2D;

namespace Avalonia.Direct2D1.Media
{
    /// <summary>
    /// A Direct2D Bitmap implementation that uses a GPU memory bitmap as its image.
    /// </summary>
    internal class D2DBitmapImpl : BitmapImpl
    {
        protected readonly Bitmap _direct2DBitmap;

        /// <summary>
        /// Initialize a new instance of the <see cref="BitmapImpl"/> class
        /// with a bitmap backed by GPU memory.
        /// </summary>
        /// <param name="d2DBitmap">The GPU bitmap.</param>
        /// <remarks>
        /// This bitmap must be either from the same render target,
        /// or if the render target is a <see cref="Avalonia.Direct2D1.Interop.Direct2D1.DeviceContext"/>,
        /// the device associated with this context, to be renderable.
        /// </remarks>
        public D2DBitmapImpl(Bitmap d2DBitmap)
        {
            _direct2DBitmap = d2DBitmap ?? throw new ArgumentNullException(nameof(d2DBitmap));
        }

        public override Vector Dpi => new Vector(96, 96);
        public override PixelSize PixelSize => _direct2DBitmap.PixelSize.ToAvalonia();

        public override void Dispose()
        {
            base.Dispose();
            _direct2DBitmap.Dispose();
        }

        public override OptionalDispose<Bitmap> GetDirect2DBitmap(Avalonia.Direct2D1.Interop.Direct2D1.RenderTarget target)
        {
            return new OptionalDispose<Bitmap>(_direct2DBitmap, false);
        }

        public override void Save(Stream stream, int? quality = null)
        {
            if (_direct2DBitmap is Bitmap1 bitmap1)
            {
                using var encoder = new Avalonia.Direct2D1.Interop.WIC.PngBitmapEncoder(Direct2D1Platform.ImagingFactory, stream);
                using var frame = new Avalonia.Direct2D1.Interop.WIC.BitmapFrameEncode(encoder);
                frame.Initialize();

                Direct2D1Platform.ImagingFactory.Native.CreateImageEncoder(
                    Direct2D1Platform.Direct2D1Device.Native,
                    out IWICImageEncoder imageEncoder);

                var parameters = new WICImageParameters
                {
                    PixelFormat = new D2D1_PIXEL_FORMAT
                    {
                        format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                        alphaMode = D2D1_ALPHA_MODE.D2D1_ALPHA_MODE_PREMULTIPLIED
                    },
                    DpiX = (float)Dpi.X,
                    DpiY = (float)Dpi.Y,
                    Top = 0,
                    Left = 0,
                    PixelWidth = (uint)PixelSize.Width,
                    PixelHeight = (uint)PixelSize.Height
                };

                unsafe
                {
                    var imageParameters = parameters;
                    imageEncoder.WriteFrame(bitmap1.NativeBitmap, frame.Native, &imageParameters);
                }
                frame.Commit();
                encoder.Commit();
                return;
            }

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
