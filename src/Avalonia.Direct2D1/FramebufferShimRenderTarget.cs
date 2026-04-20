#nullable enable

using System;
using Avalonia.Direct2D1.Interop;
using Avalonia.Direct2D1.Media;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using SharpDX.WIC;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace Avalonia.Direct2D1
{
    class FramebufferShimRenderTarget : IRenderTarget
    {
        private IFramebufferRenderTarget? _target;

        public FramebufferShimRenderTarget(IFramebufferPlatformSurface surface)
        {
            _target = surface.CreateFramebufferRenderTarget();
        }

        public RenderTargetProperties Properties => new()
        {
            RetainsPreviousFrameContents = _target?.RetainsFrameContents == true,
            IsSuitableForDirectRendering = true
        };

        public PlatformRenderTargetState PlatformRenderTargetState =>
            _target?.State ?? PlatformRenderTargetState.Disposed;

        public void Dispose()
        {
            _target?.Dispose();
            _target = null;
        }

        public IDrawingContextImpl CreateDrawingContext(bool useScaledDrawing)
        {
            return CreateDrawingContext(new IRenderTarget.RenderTargetSceneInfo(default, 1), out _);
        }

        public IDrawingContextImpl CreateDrawingContext(IRenderTarget.RenderTargetSceneInfo sceneInfo,
            out RenderTargetDrawingContextProperties properties)
        {
            if (_target == null)
                throw new ObjectDisposedException(nameof(FramebufferShimRenderTarget));
            var locked = _target.Lock(sceneInfo, out var lockProperties);
            if (locked.Format == PixelFormat.Rgb565)
            {
                locked.Dispose();
                throw new ArgumentException("Unsupported pixel format: " + locked.Format);
            }

            properties = new RenderTargetDrawingContextProperties
            {
                PreviousFrameIsRetained = lockProperties.PreviousFrameIsRetained
            };

            return new FramebufferShim(locked)
                .CreateDrawingContext(useScaledDrawing: false);
        }

        public bool IsCorrupted => false;

        class FramebufferShim : WicRenderTargetBitmapImpl
        {
            private readonly ILockedFramebuffer _target;

            public FramebufferShim(ILockedFramebuffer target) : 
                base(target.Size, target.Dpi, target.Format)
            {
                _target = target;
            }
            
            public override IDrawingContextImpl CreateDrawingContext(bool useScaledDrawing)
            {
                return base.CreateDrawingContext(useScaledDrawing, () =>
                {
                    using (var l = WicImpl.Lock(BitmapLockFlags.Read))
                    {
                        for (var y = 0; y < _target.Size.Height; y++)
                        {
                            NativeMethods.CopyMemory(
                                (_target.Address + _target.RowBytes * y),
                                (l.Data.DataPointer + l.Stride * y),
                                (UIntPtr)Math.Min(l.Stride, _target.RowBytes));
                        }
                    }
                    Dispose();
                    _target.Dispose();
                });
            }
        }
    }
}
