using System;
using System.Runtime.InteropServices;
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
        private const int BackBufferCount = 2;
        private const uint DxgiErrorInvalidCall = 0x887A0001;
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
            var size = NormalizeSize(GetWindowSize());
            var dpi = GetWindowDpi();
            var sizeChanged = size != _savedSize;
            var dpiChanged = dpi != _savedDpi;

            if (sizeChanged || dpiChanged)
            {
                _savedSize = size;
                _savedDpi = dpi;

                Resize(sizeChanged);
            }
            else if (_deviceContext == null)
            {
                CreateDeviceContext();
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

        private void Resize(bool sizeChanged)
        {
            DisposeDeviceContext();

            if (sizeChanged && _swapChain is not null)
            {
                try
                {
                    _swapChain.ResizeBuffers(
                        BackBufferCount,
                        _savedSize.Width,
                        _savedSize.Height,
                        Format.B8G8R8A8_UNorm,
                        SwapChainFlags.None);
                }
                catch (COMException ex) when ((uint)ex.HResult == DxgiErrorInvalidCall)
                {
                    // DXGI still sees a live back-buffer reference somewhere; recreate the swap chain instead.
                    _swapChain.Dispose();
                    _swapChain = null;
                }
            }

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
                BufferCount = BackBufferCount,
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

        private static Size2 NormalizeSize(Size2 size)
        {
            return new Size2(
                Math.Max(size.Width, 1),
                Math.Max(size.Height, 1));
        }

        protected abstract SwapChain1 CreateSwapChain(Avalonia.Direct2D1.Interop.DXGI.Factory2 dxgiFactory, SwapChainDescription1 swapChainDesc);

        protected abstract Size2F GetWindowDpi();

        protected abstract Size2 GetWindowSize();
    }
}
