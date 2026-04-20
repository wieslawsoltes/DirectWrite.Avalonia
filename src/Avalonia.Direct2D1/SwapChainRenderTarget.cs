using Avalonia.Direct2D1.Media;
using Avalonia.Direct2D1.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Direct2D1.Interop;
using Avalonia.Direct2D1.Interop.Direct2D1;
using Avalonia.Direct2D1.Interop.DXGI;

namespace Avalonia.Direct2D1
{   
    internal abstract class SwapChainRenderTarget : IRenderTarget, ILayerFactory
    {
        private Size2 _savedSize;
        private Size2F _savedDpi;
        private DeviceContext? _deviceContext;
        private Bitmap1? _targetBitmap;
        private SwapChain1? _swapChain;

        /// <summary>
        /// Creates a drawing context for a rendering session.
        /// </summary>
        /// <returns>An <see cref="Avalonia.Platform.IDrawingContextImpl"/>.</returns>
        public Avalonia.Platform.RenderTargetProperties Properties => new()
        {
            RetainsPreviousFrameContents = false,
            IsSuitableForDirectRendering = true
        };

        public IDrawingContextImpl CreateDrawingContext(bool useScaledDrawing)
        {
            var size = GetWindowSize();
            var dpi = GetWindowDpi();

            if (size != _savedSize || dpi != _savedDpi)
            {
                _savedSize = size;
                _savedDpi = dpi;

                Resize();
            }

            return new DrawingContextImpl(this, _deviceContext!, useScaledDrawing, _swapChain);
        }

        public IDrawingContextImpl CreateDrawingContext(IRenderTarget.RenderTargetSceneInfo sceneInfo,
            out RenderTargetDrawingContextProperties properties)
        {
            properties = default;

            return CreateDrawingContext(useScaledDrawing: false);
        }

        public bool IsCorrupted => false;

        public IDrawingContextLayerImpl CreateLayer(Size size)
        {
            if (_deviceContext == null)
            {
                CreateDeviceContext();
            }

            return D2DRenderTargetBitmapImpl.CreateCompatible(_deviceContext!, size);
        }

        public void Dispose()
        {
            DisposeDeviceContext();
            _swapChain?.Dispose();
            _swapChain = null;
        }

        private void Resize()
        {
            DisposeDeviceContext();

            _swapChain?.ResizeBuffers(0, 0, 0, Format.Unknown, SwapChainFlags.None);

            CreateDeviceContext();
        }

        private void CreateSwapChain()
        {
            var swapChainDescription = new SwapChainDescription1
            {
                Width = _savedSize.Width,
                Height = _savedSize.Height,
                Format = Format.B8G8R8A8_UNorm,
                SampleDescription = new SampleDescription
                {
                    Count = 1,
                    Quality = 0,
                },
                Usage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipDiscard,
                AlphaMode = Avalonia.Direct2D1.Interop.DXGI.AlphaMode.Ignore,
            };

            using (var dxgiAdapter = Direct2D1Platform.DxgiDevice.Adapter)
            using (var dxgiFactory = dxgiAdapter.GetParent<Avalonia.Direct2D1.Interop.DXGI.Factory2>())
            {
                _swapChain = CreateSwapChain(dxgiFactory, swapChainDescription);
            }
        }

        private void CreateDeviceContext()
        {
            _deviceContext = new DeviceContext(
                Direct2D1Platform.Direct2D1Device,
                DeviceContextOptions.EnableMultithreadedOptimizations)
            {
                DotsPerInch = _savedDpi
            };

            if (_swapChain == null)
            {
                CreateSwapChain();
            }

            using (var dxgiBackBuffer = _swapChain!.GetBackBuffer<Surface>(0))
            {
                _targetBitmap = new Bitmap1(
                _deviceContext,
                dxgiBackBuffer,
                new BitmapProperties1(
                    new Avalonia.Direct2D1.Interop.Direct2D1.PixelFormat
                    {
                        AlphaMode = Avalonia.Direct2D1.Interop.Direct2D1.AlphaMode.Premultiplied,
                        Format = Format.B8G8R8A8_UNorm
                    },
                    _savedSize.Width,
                    _savedSize.Height,
                    BitmapOptions.Target | BitmapOptions.CannotDraw));

                _deviceContext.Target = _targetBitmap;
            }
        }

        private void DisposeDeviceContext()
        {
            if (_deviceContext is not null)
            {
                _deviceContext.Native.SetTarget(null!);
            }

            _targetBitmap?.Dispose();
            _targetBitmap = null;

            _deviceContext?.Dispose();
            _deviceContext = null;
        }

        protected abstract SwapChain1 CreateSwapChain(Avalonia.Direct2D1.Interop.DXGI.Factory2 dxgiFactory, SwapChainDescription1 swapChainDesc);

        protected abstract Size2F GetWindowDpi();

        protected abstract Size2 GetWindowSize();
    }
}
