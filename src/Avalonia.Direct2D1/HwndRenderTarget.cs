using Avalonia.Platform;
using Avalonia.Direct2D1.Interop;
using Avalonia.Direct2D1.Interop.DXGI;

namespace Avalonia.Direct2D1
{
    class HwndRenderTarget : SwapChainRenderTarget
    {
        private readonly IPlatformHandle _window;

        public HwndRenderTarget(IPlatformHandle window)
        {
            _window = window;
        }

        protected override SwapChain1 CreateSwapChain(Factory2 dxgiFactory, SwapChainDescription1 swapChainDesc)
        {
            return new SwapChain1(dxgiFactory, Direct2D1Platform.DxgiDevice, _window.Handle, ref swapChainDesc);
        }

        protected override Size2F GetWindowDpi()
        {
            var dpi = NativeMethods.GetDpiForWindow(_window.Handle);
            if (dpi != 0)
            {
                return new Size2F(dpi, dpi);
            }

            return new Size2F(96, 96);
        }

        protected override Size2 GetWindowSize()
        {
            NativeMethods.GetClientRect(_window.Handle, out var rc);
            return new Size2(rc.Right - rc.Left, rc.Bottom - rc.Top);
        }
    }
}
