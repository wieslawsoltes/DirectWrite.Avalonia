using System;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Imaging;
using Windows.Win32.Graphics.Imaging.D2D;
using Windows.Win32.System.Com.StructuredStorage;
using W32WIC = Windows.Win32.Graphics.Imaging;
using W32WICD2D = Windows.Win32.Graphics.Imaging.D2D;

namespace Avalonia.Direct2D1.Interop.WIC;

[Flags]
public enum BitmapLockFlags
{
    Read = 0x1,
    Write = 0x2
}

public enum DecodeOptions
{
    MetadataCacheOnDemand = 0,
    MetadataCacheOnLoad = 1,
    CacheOnDemand = MetadataCacheOnDemand,
    CacheOnLoad = MetadataCacheOnLoad
}

public enum BitmapCreateCacheOption
{
    NoCache = 0,
    CacheOnDemand = 1,
    CacheOnLoad = 2
}

public enum BitmapInterpolationMode
{
    NearestNeighbor = 0,
    Linear = 1,
    Cubic = 2,
    Fant = 3,
    HighQualityCubic = 4
}

public static class PixelFormat
{
    public static Guid Format16bppBGR565 => PInvoke.GUID_WICPixelFormat16bppBGR565;
    public static Guid Format32bppPBGRA => PInvoke.GUID_WICPixelFormat32bppPBGRA;
    public static Guid Format32bppBGRA => PInvoke.GUID_WICPixelFormat32bppBGRA;
    public static Guid Format32bppPRGBA => PInvoke.GUID_WICPixelFormat32bppPRGBA;
    public static Guid Format32bppRGBA => PInvoke.GUID_WICPixelFormat32bppRGBA;
    public static Guid Format32bppRGB => PInvoke.GUID_WICPixelFormat32bppRGB;
}

public readonly struct DataRectangle
{
    public DataRectangle(IntPtr dataPointer, int pitch)
    {
        DataPointer = dataPointer;
        Pitch = pitch;
    }

    public IntPtr DataPointer { get; }

    public int Pitch { get; }
}

[NativeInterface(typeof(W32WIC.IWICBitmapSource))]
public class BitmapSource : ComObject
{
    internal BitmapSource(W32WIC.IWICBitmapSource native)
        : base(native)
    {
    }

    internal W32WIC.IWICBitmapSource Native => GetNative<W32WIC.IWICBitmapSource>();

    public Avalonia.Direct2D1.Interop.Size2 Size
    {
        get
        {
            Native.GetSize(out var width, out var height);
            return new Avalonia.Direct2D1.Interop.Size2((int)width, (int)height);
        }
    }

    public Guid PixelFormat
    {
        get
        {
            Native.GetPixelFormat(out var pixelFormat);
            return pixelFormat;
        }
    }
}

[NativeInterface(typeof(W32WIC.IWICBitmapFrameDecode))]
public sealed class BitmapFrameDecode : BitmapSource
{
    internal BitmapFrameDecode(W32WIC.IWICBitmapFrameDecode native)
        : base(native)
    {
    }

    internal W32WIC.IWICBitmapFrameDecode NativeFrame => GetNative<W32WIC.IWICBitmapFrameDecode>();
}

[NativeInterface(typeof(W32WIC.IWICBitmapScaler))]
public sealed class BitmapScaler : BitmapSource
{
    public BitmapScaler(ImagingFactory factory)
        : this(Create(factory))
    {
    }

    internal BitmapScaler(W32WIC.IWICBitmapScaler native)
        : base(native)
    {
    }

    internal W32WIC.IWICBitmapScaler NativeScaler => GetNative<W32WIC.IWICBitmapScaler>();

    public void Initialize(BitmapSource source, int width, int height, BitmapInterpolationMode interpolationMode)
    {
        NativeScaler.Initialize(source.Native, (uint)width, (uint)height, interpolationMode.ToWin32());
    }

    private static W32WIC.IWICBitmapScaler Create(ImagingFactory factory)
    {
        factory.Native.CreateBitmapScaler(out var scaler);
        return scaler;
    }
}

[NativeInterface(typeof(W32WIC.IWICFormatConverter))]
public sealed class FormatConverter : BitmapSource
{
    public FormatConverter(ImagingFactory factory)
        : this(Create(factory))
    {
    }

    internal FormatConverter(W32WIC.IWICFormatConverter native)
        : base(native)
    {
    }

    internal W32WIC.IWICFormatConverter NativeConverter => GetNative<W32WIC.IWICFormatConverter>();

    public void Initialize(BitmapSource source, Guid destinationFormat)
    {
        NativeConverter.Initialize(
            source.Native,
            in destinationFormat,
            W32WIC.WICBitmapDitherType.WICBitmapDitherTypeNone,
            null!,
            0,
            W32WIC.WICBitmapPaletteType.WICBitmapPaletteTypeCustom);
    }

    private static W32WIC.IWICFormatConverter Create(ImagingFactory factory)
    {
        factory.Native.CreateFormatConverter(out var converter);
        return converter;
    }
}

[NativeInterface(typeof(W32WIC.IWICBitmap))]
public sealed class Bitmap : BitmapSource
{
    public Bitmap(ImagingFactory factory, int width, int height, Guid pixelFormat, BitmapCreateCacheOption cacheOption)
        : this(Create(factory, width, height, pixelFormat, cacheOption))
    {
    }

    public Bitmap(ImagingFactory factory, BitmapSource source, BitmapCreateCacheOption cacheOption)
        : this(Create(factory, source, cacheOption))
    {
    }

    internal Bitmap(W32WIC.IWICBitmap native)
        : base(native)
    {
    }

    internal W32WIC.IWICBitmap NativeBitmap => GetNative<W32WIC.IWICBitmap>();

    public void SetResolution(double dpiX, double dpiY)
    {
        NativeBitmap.SetResolution(dpiX, dpiY);
    }

    public BitmapLock Lock(BitmapLockFlags flags)
    {
        var size = Size;
        var rect = new W32WIC.WICRect
        {
            X = 0,
            Y = 0,
            Width = size.Width,
            Height = size.Height
        };

        NativeBitmap.Lock(in rect, (uint)flags, out var bitmapLock);
        return new BitmapLock(bitmapLock);
    }

    private static W32WIC.IWICBitmap Create(ImagingFactory factory, int width, int height, Guid pixelFormat, BitmapCreateCacheOption cacheOption)
    {
        factory.Native.CreateBitmap((uint)width, (uint)height, in pixelFormat, cacheOption.ToWin32(), out var bitmap);
        return bitmap;
    }

    private static W32WIC.IWICBitmap Create(ImagingFactory factory, BitmapSource source, BitmapCreateCacheOption cacheOption)
    {
        factory.Native.CreateBitmapFromSource(source.Native, cacheOption.ToWin32(), out var bitmap);
        return bitmap;
    }
}

[NativeInterface(typeof(W32WIC.IWICBitmapLock))]
public sealed class BitmapLock : ComObject
{
    internal BitmapLock(W32WIC.IWICBitmapLock native)
        : base(native)
    {
    }

    private W32WIC.IWICBitmapLock Native => GetNative<W32WIC.IWICBitmapLock>();

    public Avalonia.Direct2D1.Interop.Size2 Size
    {
        get
        {
            Native.GetSize(out var width, out var height);
            return new Avalonia.Direct2D1.Interop.Size2((int)width, (int)height);
        }
    }

    public int Stride
    {
        get
        {
            Native.GetStride(out var stride);
            return (int)stride;
        }
    }

    public unsafe DataRectangle Data
    {
        get
        {
            byte* dataPointer;
            Native.GetDataPointer(out _, &dataPointer);
            return new DataRectangle((IntPtr)dataPointer, Stride);
        }
    }
}

[NativeInterface(typeof(W32WIC.IWICBitmapDecoder))]
public sealed class BitmapDecoder : ComObject
{
    private readonly string? _temporaryFilePath;

    public BitmapDecoder(ImagingFactory factory, string fileName, DecodeOptions decodeOptions)
        : this(factory.Native.CreateDecoderFromFilename(fileName, null, GENERIC_ACCESS_RIGHTS.GENERIC_READ, decodeOptions.ToWin32()), null)
    {
    }

    public BitmapDecoder(ImagingFactory factory, Stream stream, DecodeOptions decodeOptions)
        : this(factory, CreateTemporaryFile(stream), decodeOptions, ownsTemporaryFile: true)
    {
    }

    private BitmapDecoder(ImagingFactory factory, string fileName, DecodeOptions decodeOptions, bool ownsTemporaryFile)
        : this(factory.Native.CreateDecoderFromFilename(fileName, null, GENERIC_ACCESS_RIGHTS.GENERIC_READ, decodeOptions.ToWin32()), ownsTemporaryFile ? fileName : null)
    {
    }

    internal BitmapDecoder(W32WIC.IWICBitmapDecoder native, string? temporaryFilePath)
        : base(native)
    {
        _temporaryFilePath = temporaryFilePath;
    }

    private W32WIC.IWICBitmapDecoder Native => GetNative<W32WIC.IWICBitmapDecoder>();

    public BitmapFrameDecode GetFrame(int index)
    {
        Native.GetFrame((uint)index, out var frame);
        return new BitmapFrameDecode(frame);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (_temporaryFilePath is { } temporaryFilePath && File.Exists(temporaryFilePath))
        {
            File.Delete(temporaryFilePath);
        }
    }

    private static string CreateTemporaryFile(Stream stream)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.img");

        using var file = File.Create(path);
        stream.CopyTo(file);
        return path;
    }
}

[NativeInterface(typeof(W32WICD2D.IWICImagingFactory2))]
public sealed class ImagingFactory : ComObject
{
    public ImagingFactory()
        : this(Create())
    {
    }

    internal ImagingFactory(W32WICD2D.IWICImagingFactory2 native)
        : base(native)
    {
    }

    internal W32WICD2D.IWICImagingFactory2 Native => GetNative<W32WICD2D.IWICImagingFactory2>();

    private static W32WICD2D.IWICImagingFactory2 Create()
    {
        var iid = typeof(W32WICD2D.IWICImagingFactory2).GUID;
        PInvoke.CoCreateInstance(
            in PInvoke.CLSID_WICImagingFactory2,
            null!,
            Windows.Win32.System.Com.CLSCTX.CLSCTX_INPROC_SERVER,
            in iid,
            out var factory).ThrowOnFailure();
        return (W32WICD2D.IWICImagingFactory2)factory;
    }
}

public sealed class PngBitmapEncoder : IDisposable
{
    private readonly Stream _output;
    private readonly string _temporaryFilePath;
    private readonly W32WIC.IWICBitmapEncoder _native;
    private readonly W32WIC.IWICStream _stream;
    private bool _committed;

    public PngBitmapEncoder(ImagingFactory factory, Stream output)
    {
        _output = output;
        _temporaryFilePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.png");
        var vendor = Guid.Empty;
        _native = factory.Native.CreateEncoder(in PInvoke.GUID_ContainerFormatPng, in vendor);
        factory.Native.CreateStream(out _stream);
        _stream.InitializeFromFilename(_temporaryFilePath, unchecked((uint)GENERIC_ACCESS_RIGHTS.GENERIC_WRITE));
        _native.Initialize(_stream, W32WIC.WICBitmapEncoderCacheOption.WICBitmapEncoderNoCache);
    }

    internal W32WIC.IWICBitmapEncoder Native => _native;

    public void Commit()
    {
        _native.Commit();
        _stream.Commit(0);
        _output.Position = 0;

        using var file = File.OpenRead(_temporaryFilePath);
        file.CopyTo(_output);
        _committed = true;
    }

    public void Dispose()
    {
        if (!_committed && File.Exists(_temporaryFilePath))
        {
            File.Delete(_temporaryFilePath);
        }
    }
}

public sealed class BitmapFrameEncode : IDisposable
{
    private readonly W32WIC.IWICBitmapFrameEncode _native;

    public BitmapFrameEncode(PngBitmapEncoder encoder)
    {
        IPropertyBag2 options = null!;
        encoder.Native.CreateNewFrame(out _native, ref options);
    }

    public void Initialize()
    {
        _native.Initialize(null!);
    }

    internal W32WIC.IWICBitmapFrameEncode Native => _native;

    public unsafe void WriteSource(BitmapSource source)
    {
        _native.WriteSource(source.Native, default(W32WIC.WICRect*));
    }

    public void Commit()
    {
        _native.Commit();
    }

    public void Dispose()
    {
    }
}

internal static class WicConversions
{
    public static W32WIC.WICDecodeOptions ToWin32(this DecodeOptions decodeOptions) =>
        (W32WIC.WICDecodeOptions)(int)decodeOptions;

    public static W32WIC.WICBitmapCreateCacheOption ToWin32(this BitmapCreateCacheOption cacheOption) =>
        (W32WIC.WICBitmapCreateCacheOption)(int)cacheOption;

    public static W32WIC.WICBitmapInterpolationMode ToWin32(this BitmapInterpolationMode interpolationMode) =>
        (W32WIC.WICBitmapInterpolationMode)(int)interpolationMode;
}
