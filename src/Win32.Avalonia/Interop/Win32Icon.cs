using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia.Interop;

internal unsafe sealed class Win32Icon : IDisposable
{
    private DestroyIconSafeHandle? _handle;

    public Win32Icon(Bitmap bitmap, PixelPoint hotSpot = default)
    {
        _handle = CreateIcon(bitmap, hotSpot);
    }

    public nint Handle => _handle?.DangerousGetHandle() ?? nint.Zero;

    private static DestroyIconSafeHandle CreateIcon(Bitmap bitmap, PixelPoint hotSpot)
    {
        var mainBitmap = CreateHBitmap(bitmap);
        if (mainBitmap.IsNull)
        {
            throw new Win32Exception();
        }

        var alphaBitmap = AlphaToMask(bitmap);

        try
        {
            if (alphaBitmap.IsNull)
            {
                throw new Win32Exception();
            }

            var info = new ICONINFO
            {
                fIcon = false,
                xHotspot = (uint)hotSpot.X,
                yHotspot = (uint)hotSpot.Y,
                hbmMask = alphaBitmap,
                hbmColor = mainBitmap
            };

            var icon = PInvoke.CreateIconIndirect(info);
            if (icon.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return icon;
        }
        finally
        {
            PInvoke.DeleteObject(new HGDIOBJ(mainBitmap.Value));
            PInvoke.DeleteObject(new HGDIOBJ(alphaBitmap.Value));
        }
    }

    private static HBITMAP CreateHBitmap(Bitmap source)
    {
        using var framebuffer = AllocateFramebuffer(source.PixelSize, PixelFormats.Bgra8888, AlphaFormat.Unpremul);
        source.CopyPixels(framebuffer);
        return PInvoke.CreateBitmap(source.PixelSize.Width, source.PixelSize.Height, 1, 32, framebuffer.Address.ToPointer());
    }

    private static HBITMAP AlphaToMask(Bitmap source)
    {
        using var alphaMaskBuffer = AllocateFramebuffer(source.PixelSize, PixelFormats.BlackWhite, AlphaFormat.Opaque);

        if (source.AlphaFormat == AlphaFormat.Opaque)
        {
            Unsafe.InitBlock((void*)alphaMaskBuffer.Address, 0xff, (uint)(alphaMaskBuffer.RowBytes * alphaMaskBuffer.Size.Height));
        }
        else
        {
            using var argbBuffer = AllocateFramebuffer(source.PixelSize, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
            source.CopyPixels(argbBuffer);

            var sourcePixels = (byte*)argbBuffer.Address;
            var destinationPixels = (byte*)alphaMaskBuffer.Address;

            for (var y = 0; y < argbBuffer.Size.Height; y++)
            {
                for (var x = 0; x < argbBuffer.Size.Width; x++)
                {
                    if (sourcePixels[x * 4 + 3] == 0)
                    {
                        destinationPixels[x / 8] |= (byte)(1 << (x % 8));
                    }
                }

                sourcePixels += argbBuffer.RowBytes;
                destinationPixels += alphaMaskBuffer.RowBytes;
            }
        }

        return PInvoke.CreateBitmap(alphaMaskBuffer.Size.Width, alphaMaskBuffer.Size.Height, 1, 1, alphaMaskBuffer.Address.ToPointer());
    }

    private static LockedFramebuffer AllocateFramebuffer(PixelSize size, PixelFormat format, AlphaFormat alphaFormat)
    {
        if (size.Width < 1 || size.Height < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        var stride = (size.Width * format.BitsPerPixel + 7) / 8;
        var data = Marshal.AllocHGlobal(size.Height * stride);
        if (data == IntPtr.Zero)
        {
            throw new OutOfMemoryException();
        }

        return new LockedFramebuffer(data, size, stride, new Vector(96, 96), format, alphaFormat, () => Marshal.FreeHGlobal(data));
    }

    public void Dispose()
    {
        if (_handle is not null)
        {
            _handle.Dispose();
            _handle = null;
        }

        GC.SuppressFinalize(this);
    }

    ~Win32Icon()
    {
        Dispose();
    }
}