using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Win32.Avalonia;

internal unsafe sealed class FramebufferManager : IFramebufferPlatformSurface, IDisposable
{
    private const int BytesPerPixel = 4;

    private readonly nint _hwnd;
    private readonly object _syncLock = new();
    private FramebufferData? _framebufferData;

    public FramebufferManager(nint hwnd)
    {
        _hwnd = hwnd;
    }

    public ILockedFramebuffer Lock()
    {
        Monitor.Enter(_syncLock);

        try
        {
            PInvoke.GetClientRect(new HWND(_hwnd), out var rect);
            var width = Math.Max(1, rect.right - rect.left);
            var height = Math.Max(1, rect.bottom - rect.top);

            if (_framebufferData is null || _framebufferData.Value.Size.Width != width || _framebufferData.Value.Size.Height != height)
            {
                _framebufferData?.Dispose();
                _framebufferData = new FramebufferData(width, height);
            }

            var framebuffer = _framebufferData.Value;
            return new LockedFramebuffer(
                framebuffer.Address,
                framebuffer.Size,
                framebuffer.RowBytes,
                GetCurrentDpi(),
                PixelFormats.Bgra8888,
                AlphaFormat.Premul,
                DrawAndUnlock);
        }
        catch
        {
            Monitor.Exit(_syncLock);
            throw;
        }
    }

    public IFramebufferRenderTarget CreateFramebufferRenderTarget() => new FuncFramebufferRenderTarget(Lock);

    public void Dispose()
    {
        lock (_syncLock)
        {
            _framebufferData?.Dispose();
            _framebufferData = null;
        }
    }

    private void DrawAndUnlock()
    {
        try
        {
            if (_framebufferData is not { } framebuffer)
            {
                return;
            }

            var hdc = PInvoke.GetDC(new HWND(_hwnd));
            if (hdc.IsNull)
            {
                return;
            }

            try
            {
                var bitmapInfo = framebuffer.BitmapInfo;
                FramebufferNative.SetDIBitsToDevice(
                    (nint)hdc.Value,
                    0,
                    0,
                    (uint)framebuffer.Size.Width,
                    (uint)framebuffer.Size.Height,
                    0,
                    0,
                    0,
                    (uint)framebuffer.Size.Height,
                    framebuffer.Address,
                        ref bitmapInfo,
                    0);
            }
            finally
            {
                PInvoke.ReleaseDC(new HWND(_hwnd), hdc);
            }
        }
        finally
        {
            Monitor.Exit(_syncLock);
        }
    }

    private Vector GetCurrentDpi()
    {
        var dpi = PInvoke.GetDpiForWindow(new HWND(_hwnd));
        if (dpi == 0)
        {
            dpi = 96;
        }

        return new Vector(dpi, dpi);
    }

    private readonly struct FramebufferData : IDisposable
    {
        public FramebufferData(int width, int height)
        {
            Size = new PixelSize(width, height);
            RowBytes = width * BytesPerPixel;
            Address = Marshal.AllocHGlobal(RowBytes * height);
            BitmapInfo = new FramebufferNative.BitmapInfo
            {
                Header = new FramebufferNative.BitmapInfoHeader
                {
                    Size = (uint)Marshal.SizeOf<FramebufferNative.BitmapInfoHeader>(),
                    Width = width,
                    Height = -height,
                    Planes = 1,
                    BitCount = 32,
                    Compression = 0,
                    SizeImage = (uint)(RowBytes * height),
                }
            };
        }

        public nint Address { get; }

        public PixelSize Size { get; }

        public int RowBytes { get; }

        public FramebufferNative.BitmapInfo BitmapInfo { get; }

        public void Dispose()
        {
            if (Address != nint.Zero)
            {
                Marshal.FreeHGlobal(Address);
            }
        }
    }

    private static class FramebufferNative
    {
        [DllImport("gdi32.dll", EntryPoint = "SetDIBitsToDevice", SetLastError = true)]
        public static extern int SetDIBitsToDevice(
            nint hdc,
            int xDest,
            int yDest,
            uint width,
            uint height,
            int xSrc,
            int ySrc,
            uint startScan,
            uint scanLines,
            nint bits,
            ref BitmapInfo bitmapInfo,
            uint colorUse);

        [StructLayout(LayoutKind.Sequential)]
        public struct BitmapInfoHeader
        {
            public uint Size;
            public int Width;
            public int Height;
            public ushort Planes;
            public ushort BitCount;
            public uint Compression;
            public uint SizeImage;
            public int XPelsPerMeter;
            public int YPelsPerMeter;
            public uint ClrUsed;
            public uint ClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BitmapInfo
        {
            public BitmapInfoHeader Header;
            public uint Colors;
        }
    }
}