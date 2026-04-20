using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectWrite;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.Graphics.Imaging;
using Avalonia.Direct2D1.Interop.DXGI;
using W32D2D = Windows.Win32.Graphics.Direct2D;
using W32D2DC = Windows.Win32.Graphics.Direct2D.Common;

namespace Avalonia.Direct2D1.Interop.Direct2D1;

public enum FactoryType
{
    SingleThreaded = 0,
    MultiThreaded = 1
}

public enum DebugLevel
{
    None = 0,
    Error = 1,
    Warning = 2,
    Information = 3
}

public enum AlphaMode
{
    Unknown = 0,
    Premultiplied = 1,
    Straight = 2,
    Ignore = 3
}

public enum ExtendMode
{
    Clamp = 0,
    Wrap = 1,
    Mirror = 2
}

public enum LineJoin
{
    Miter = 0,
    Bevel = 1,
    Round = 2,
    MiterOrBevel = 3
}

public enum CapStyle
{
    Flat = 0,
    Square = 1,
    Round = 2,
    Triangle = 3
}

public enum DashStyle
{
    Solid = 0,
    Dash = 1,
    Dot = 2,
    DashDot = 3,
    DashDotDot = 4,
    Custom = 5
}

public enum BitmapInterpolationMode
{
    NearestNeighbor = 0,
    Linear = 1
}

public enum InterpolationMode
{
    NearestNeighbor = 0,
    Linear = 1,
    Cubic = 2,
    MultiSampleLinear = 3,
    Anisotropic = 4,
    HighQualityCubic = 5
}

public enum CompositeMode
{
    SourceOver = 0,
    DestinationOver = 1,
    SourceIn = 2,
    DestinationIn = 3,
    SourceOut = 4,
    DestinationOut = 5,
    SourceAtop = 6,
    DestinationAtop = 7,
    Xor = 8,
    Plus = 9
}

public enum AntialiasMode
{
    PerPrimitive = 0,
    Aliased = 1
}

public enum TextAntialiasMode
{
    Default = 0,
    Cleartype = 1,
    Grayscale = 2,
    Aliased = 3
}

public enum CompatibleRenderTargetOptions
{
    None = 0
}

[Flags]
public enum BitmapOptions
{
    None = 0,
    Target = 1,
    CannotDraw = 2,
    CpuRead = 4,
    GdiCompatible = 8
}

public enum DeviceContextOptions
{
    None = 0,
    EnableMultithreadedOptimizations = 1
}

public enum FigureBegin
{
    Filled = 0,
    Hollow = 1
}

public enum FigureEnd
{
    Open = 0,
    Closed = 1
}

public enum FillMode
{
    Alternate = 0,
    Winding = 1
}

public enum SweepDirection
{
    CounterClockwise = 0,
    Clockwise = 1
}

public enum ArcSize
{
    Small = 0,
    Large = 1
}

public enum CombineMode
{
    Union = 0,
    Intersect = 1,
    Xor = 2,
    Exclude = 3
}

public struct PixelFormat
{
    public Avalonia.Direct2D1.Interop.DXGI.Format Format;
    public AlphaMode AlphaMode;
}

public struct RenderTargetProperties
{
    public PixelFormat PixelFormat;
    public float DpiX;
    public float DpiY;
}

public struct BitmapProperties1
{
    public BitmapProperties1(PixelFormat pixelFormat, int pixelWidth, int pixelHeight, BitmapOptions options)
    {
        PixelFormat = pixelFormat;
        DpiX = 96;
        DpiY = 96;
        BitmapOptions = options;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
    }

    public PixelFormat PixelFormat;
    public float DpiX;
    public float DpiY;
    public BitmapOptions BitmapOptions;
    public int PixelWidth;
    public int PixelHeight;
}

public struct BrushProperties
{
    public float Opacity;
    public Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 Transform;
}

public struct BitmapBrushProperties
{
    public ExtendMode ExtendModeX;
    public ExtendMode ExtendModeY;
    public BitmapInterpolationMode InterpolationMode;
}

public struct BitmapBrushProperties1
{
    public ExtendMode ExtendModeX;
    public ExtendMode ExtendModeY;
    public InterpolationMode InterpolationMode;
}

public struct GradientStop
{
    public float Position;
    public Avalonia.Direct2D1.Interop.Mathematics.RawColor4 Color;
}

public struct LinearGradientBrushProperties
{
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 StartPoint;
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 EndPoint;
}

public struct RadialGradientBrushProperties
{
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Center;
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 GradientOriginOffset;
    public float RadiusX;
    public float RadiusY;
}

public struct StrokeStyleProperties
{
    public CapStyle StartCap;
    public CapStyle EndCap;
    public CapStyle DashCap;
    public LineJoin LineJoin;
    public float MiterLimit;
    public DashStyle DashStyle;
    public float DashOffset;
}

public struct Ellipse
{
    public Ellipse(Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point, float radiusX, float radiusY)
    {
        Point = point;
        RadiusX = radiusX;
        RadiusY = radiusY;
    }

    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Point;
    public float RadiusX;
    public float RadiusY;
}

public struct RoundedRectangle
{
    public Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF Rect;
    public float RadiusX;
    public float RadiusY;
}

public struct ArcSegment
{
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Point;
    public Avalonia.Direct2D1.Interop.Size2F Size;
    public float RotationAngle;
    public SweepDirection SweepDirection;
    public ArcSize ArcSize;
}

public struct BezierSegment
{
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Point1;
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Point2;
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Point3;
}

public struct QuadraticBezierSegment
{
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Point1;
    public Avalonia.Direct2D1.Interop.Mathematics.RawVector2 Point2;
}

public struct LayerParameters
{
    public Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF ContentBounds;
    public Geometry? GeometricMask;
    public AntialiasMode MaskAntialiasMode;
    public Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 MaskTransform;
    public float Opacity;
    public Brush? OpacityBrush;
}

[NativeInterface(typeof(W32D2D.ID2D1Factory))]
public class Factory : CppObject
{
    internal Factory(W32D2D.ID2D1Factory native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1Factory Native => GetNative<W32D2D.ID2D1Factory>();
}

[NativeInterface(typeof(W32D2D.ID2D1Factory1))]
public sealed class Factory1 : Factory
{
    public Factory1(FactoryType factoryType, DebugLevel debugLevel)
        : this(Create(factoryType, debugLevel))
    {
    }

    internal Factory1(W32D2D.ID2D1Factory1 native)
        : base(native)
    {
    }

    internal new W32D2D.ID2D1Factory1 Native => GetNative<W32D2D.ID2D1Factory1>();

    private static W32D2D.ID2D1Factory1 Create(FactoryType factoryType, DebugLevel debugLevel)
    {
        var options = new W32D2D.D2D1_FACTORY_OPTIONS
        {
            debugLevel = (W32D2D.D2D1_DEBUG_LEVEL)(int)debugLevel
        };
        var iid = typeof(W32D2D.ID2D1Factory1).GUID;

        PInvoke.D2D1CreateFactory(
            (W32D2D.D2D1_FACTORY_TYPE)(int)factoryType,
            in iid,
            options,
            out var factory).ThrowOnFailure();

        return (W32D2D.ID2D1Factory1)factory;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1Device))]
public sealed class Device : CppObject
{
    public Device(Factory1 factory, Avalonia.Direct2D1.Interop.DXGI.Device1 dxgiDevice)
        : this(Create(factory, dxgiDevice))
    {
    }

    internal Device(W32D2D.ID2D1Device native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1Device Native => GetNative<W32D2D.ID2D1Device>();

    private static W32D2D.ID2D1Device Create(Factory1 factory, Avalonia.Direct2D1.Interop.DXGI.Device1 dxgiDevice)
    {
        factory.Native.CreateDevice(dxgiDevice.Native, out var device);
        return device;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1Resource))]
public abstract class Resource : CppObject
{
    protected Resource(object native)
        : base(native)
    {
    }
}

[NativeInterface(typeof(W32D2D.ID2D1Brush))]
public class Brush : Resource
{
    internal Brush(W32D2D.ID2D1Brush native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1Brush Native => GetNative<W32D2D.ID2D1Brush>();

    public float Opacity
    {
        get => Native.GetOpacity();
        set => Native.SetOpacity(value);
    }

    public Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 Transform
    {
        get
        {
            Native.GetTransform(out var transform);
            return transform.ToRaw();
        }
        set => Native.SetTransform(value.ToWin32());
    }
}

[NativeInterface(typeof(W32D2D.ID2D1SolidColorBrush))]
public sealed class SolidColorBrush : Brush
{
    public SolidColorBrush(RenderTarget renderTarget, Avalonia.Direct2D1.Interop.Mathematics.RawColor4 color, BrushProperties properties)
        : this(Create(renderTarget, color, properties))
    {
    }

    internal SolidColorBrush(W32D2D.ID2D1SolidColorBrush native)
        : base(native)
    {
    }

    private static W32D2D.ID2D1SolidColorBrush Create(RenderTarget renderTarget, Avalonia.Direct2D1.Interop.Mathematics.RawColor4 color, BrushProperties properties)
    {
        var nativeColor = color.ToWin32();
        var nativeProperties = properties.ToWin32();
        renderTarget.Native.CreateSolidColorBrush(in nativeColor, nativeProperties, out var brush);
        return brush;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1GradientStopCollection))]
public sealed class GradientStopCollection : Resource
{
    public GradientStopCollection(RenderTarget renderTarget, GradientStop[] gradientStops, ExtendMode extendMode)
        : this(Create(renderTarget, gradientStops, extendMode))
    {
    }

    internal GradientStopCollection(W32D2D.ID2D1GradientStopCollection native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1GradientStopCollection Native => GetNative<W32D2D.ID2D1GradientStopCollection>();

    private static W32D2D.ID2D1GradientStopCollection Create(RenderTarget renderTarget, GradientStop[] gradientStops, ExtendMode extendMode)
    {
        renderTarget.Native.CreateGradientStopCollection(
            gradientStops.ToWin32(),
            W32D2D.D2D1_GAMMA.D2D1_GAMMA_2_2,
            extendMode.ToWin32(),
            out var collection);
        return collection;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1LinearGradientBrush))]
public sealed class LinearGradientBrush : Brush
{
    public LinearGradientBrush(RenderTarget renderTarget, LinearGradientBrushProperties properties, BrushProperties brushProperties, GradientStopCollection stops)
        : this(Create(renderTarget, properties, brushProperties, stops))
    {
    }

    internal LinearGradientBrush(W32D2D.ID2D1LinearGradientBrush native)
        : base(native)
    {
    }

    private static W32D2D.ID2D1LinearGradientBrush Create(RenderTarget renderTarget, LinearGradientBrushProperties properties, BrushProperties brushProperties, GradientStopCollection stops)
    {
        renderTarget.Native.CreateLinearGradientBrush(properties.ToWin32(), brushProperties.ToWin32(), stops.Native, out var brush);
        return brush;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1RadialGradientBrush))]
public sealed class RadialGradientBrush : Brush
{
    public RadialGradientBrush(RenderTarget renderTarget, RadialGradientBrushProperties properties, BrushProperties brushProperties, GradientStopCollection stops)
        : this(Create(renderTarget, properties, brushProperties, stops))
    {
    }

    internal RadialGradientBrush(W32D2D.ID2D1RadialGradientBrush native)
        : base(native)
    {
    }

    private static W32D2D.ID2D1RadialGradientBrush Create(RenderTarget renderTarget, RadialGradientBrushProperties properties, BrushProperties brushProperties, GradientStopCollection stops)
    {
        renderTarget.Native.CreateRadialGradientBrush(properties.ToWin32(), brushProperties.ToWin32(), stops.Native, out var brush);
        return brush;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1BitmapBrush))]
public class BitmapBrush : Brush
{
    public BitmapBrush(RenderTarget renderTarget, Bitmap bitmap, BitmapBrushProperties bitmapBrushProperties, BrushProperties brushProperties)
        : this(Create(renderTarget, bitmap, bitmapBrushProperties, brushProperties))
    {
    }

    internal BitmapBrush(W32D2D.ID2D1BitmapBrush native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1BitmapBrush NativeBitmapBrush => GetNative<W32D2D.ID2D1BitmapBrush>();

    private static W32D2D.ID2D1BitmapBrush Create(RenderTarget renderTarget, Bitmap bitmap, BitmapBrushProperties bitmapBrushProperties, BrushProperties brushProperties)
    {
        renderTarget.Native.CreateBitmapBrush(bitmap.Native, bitmapBrushProperties.ToWin32(), brushProperties.ToWin32(), out var brush);
        return brush;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1BitmapBrush1))]
public sealed class BitmapBrush1 : Brush
{
    public BitmapBrush1(DeviceContext renderTarget, Bitmap bitmap, BitmapBrushProperties1 bitmapBrushProperties)
        : this(Create(renderTarget, bitmap, bitmapBrushProperties))
    {
    }

    internal BitmapBrush1(W32D2D.ID2D1BitmapBrush1 native)
        : base(native)
    {
    }

    private static W32D2D.ID2D1BitmapBrush1 Create(DeviceContext renderTarget, Bitmap bitmap, BitmapBrushProperties1 bitmapBrushProperties)
    {
        renderTarget.Native.CreateBitmapBrush(bitmap.Native, bitmapBrushProperties.ToWin32(), null, out var brush);
        return brush;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1StrokeStyle))]
public sealed class StrokeStyle : Resource
{
    public StrokeStyle(Factory factory, StrokeStyleProperties properties, float[] dashes)
        : this(Create(factory, properties, dashes))
    {
    }

    internal StrokeStyle(W32D2D.ID2D1StrokeStyle native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1StrokeStyle Native => GetNative<W32D2D.ID2D1StrokeStyle>();

    private static W32D2D.ID2D1StrokeStyle Create(Factory factory, StrokeStyleProperties properties, float[] dashes)
    {
        factory.Native.CreateStrokeStyle(properties.ToWin32(), dashes, out var strokeStyle);
        return strokeStyle;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1RenderTarget))]
public class RenderTarget : Resource
{
    internal RenderTarget(W32D2D.ID2D1RenderTarget native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1RenderTarget Native => GetNative<W32D2D.ID2D1RenderTarget>();

    public Avalonia.Direct2D1.Interop.Size2 PixelSize => Native.GetPixelSize().ToInterop();

    public Avalonia.Direct2D1.Interop.Size2F DotsPerInch
    {
        get
        {
            Native.GetDpi(out var dpiX, out var dpiY);
            return new Avalonia.Direct2D1.Interop.Size2F(dpiX, dpiY);
        }
        set => Native.SetDpi(value.Width, value.Height);
    }

    public Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 Transform
    {
        get
        {
            Native.GetTransform(out var transform);
            return transform.ToRaw();
        }
        set => Native.SetTransform(value.ToWin32());
    }

    public AntialiasMode AntialiasMode
    {
        get => Native.GetAntialiasMode().ToCompat();
        set => Native.SetAntialiasMode(value.ToWin32());
    }

    public TextAntialiasMode TextAntialiasMode
    {
        get => Native.GetTextAntialiasMode().ToCompat();
        set => Native.SetTextAntialiasMode(value.ToWin32());
    }

    public virtual void BeginDraw()
    {
        Native.BeginDraw();
    }

    public virtual unsafe void EndDraw()
    {
        Native.EndDraw().ThrowOnFailure();
    }

    public void Clear(Avalonia.Direct2D1.Interop.Mathematics.RawColor4? color = null)
    {
        Native.Clear(color?.ToWin32());
    }

    public void DrawLine(Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point0, Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point1, Brush brush, float strokeWidth, StrokeStyle? strokeStyle = null)
    {
        Native.DrawLine(point0.ToWin32(), point1.ToWin32(), brush.Native, strokeWidth, strokeStyle?.Native!);
    }

    public void DrawRectangle(Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF rect, Brush brush, float strokeWidth, StrokeStyle? strokeStyle = null)
    {
        var nativeRect = rect.ToWin32();
        Native.DrawRectangle(in nativeRect, brush.Native, strokeWidth, strokeStyle?.Native!);
    }

    public void FillRectangle(Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF rect, Brush brush)
    {
        var nativeRect = rect.ToWin32();
        Native.FillRectangle(in nativeRect, brush.Native);
    }

    public void DrawRoundedRectangle(RoundedRectangle rect, Brush brush, float strokeWidth, StrokeStyle? strokeStyle = null)
    {
        var nativeRect = rect.ToWin32();
        Native.DrawRoundedRectangle(in nativeRect, brush.Native, strokeWidth, strokeStyle?.Native!);
    }

    public void FillRoundedRectangle(RoundedRectangle rect, Brush brush)
    {
        var nativeRect = rect.ToWin32();
        Native.FillRoundedRectangle(in nativeRect, brush.Native);
    }

    public void DrawEllipse(Ellipse ellipse, Brush brush, float strokeWidth, StrokeStyle? strokeStyle = null)
    {
        var nativeEllipse = ellipse.ToWin32();
        Native.DrawEllipse(in nativeEllipse, brush.Native, strokeWidth, strokeStyle?.Native!);
    }

    public void FillEllipse(Ellipse ellipse, Brush brush)
    {
        var nativeEllipse = ellipse.ToWin32();
        Native.FillEllipse(in nativeEllipse, brush.Native);
    }

    public void DrawGeometry(Geometry geometry, Brush brush, float strokeWidth, StrokeStyle? strokeStyle = null)
    {
        Native.DrawGeometry(geometry.Native, brush.Native, strokeWidth, strokeStyle?.Native!);
    }

    public void FillGeometry(Geometry geometry, Brush brush, Brush? opacityBrush = null)
    {
        Native.FillGeometry(geometry.Native, brush.Native, opacityBrush?.Native!);
    }

    public void FillOpacityMask(Bitmap opacityMask, Brush brush, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF? destinationRectangle, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF? sourceRectangle)
    {
        Native.FillOpacityMask(opacityMask.Native, brush.Native, W32D2D.D2D1_OPACITY_MASK_CONTENT.D2D1_OPACITY_MASK_CONTENT_GRAPHICS, destinationRectangle?.ToWin32(), sourceRectangle?.ToWin32());
    }

    public void DrawBitmap(Bitmap bitmap, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF? destinationRectangle, float opacity, BitmapInterpolationMode interpolationMode, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF? sourceRectangle)
    {
        Native.DrawBitmap(bitmap.Native, destinationRectangle?.ToWin32(), opacity, interpolationMode.ToWin32(), sourceRectangle?.ToWin32());
    }

    public void DrawGlyphRun(Avalonia.Direct2D1.Interop.Mathematics.RawVector2 baselineOrigin, in DWRITE_GLYPH_RUN glyphRun, Brush foregroundBrush, DWRITE_MEASURING_MODE measuringMode)
    {
        Native.DrawGlyphRun(baselineOrigin.ToWin32(), in glyphRun, foregroundBrush.Native, measuringMode);
    }

    public void PushAxisAlignedClip(Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF clipRect, AntialiasMode antialiasMode)
    {
        var nativeClipRect = clipRect.ToWin32();
        Native.PushAxisAlignedClip(in nativeClipRect, antialiasMode.ToWin32());
    }

    public void PopAxisAlignedClip()
    {
        Native.PopAxisAlignedClip();
    }

    public void PushLayer(ref LayerParameters layerParameters, Layer layer)
    {
        var nativeParameters = layerParameters.ToWin32();
        Native.PushLayer(in nativeParameters, layer.Native);
    }

    public void PopLayer()
    {
        Native.PopLayer();
    }

    public void CreateCompatibleRenderTarget(Avalonia.Direct2D1.Interop.Size2F size, CompatibleRenderTargetOptions options, out BitmapRenderTarget renderTarget)
    {
        var nativeSize = size.ToWin32();
        Native.CreateCompatibleRenderTarget(nativeSize, null, null, options.ToWin32(), out var nativeTarget);
        renderTarget = new BitmapRenderTarget(nativeTarget);
    }

    public void CreateLayer(out Layer layer)
    {
        Native.CreateLayer(default(W32D2DC.D2D_SIZE_F?), out var nativeLayer);
        layer = new Layer(nativeLayer);
    }
}

[NativeInterface(typeof(W32D2D.ID2D1DeviceContext))]
public sealed class DeviceContext : RenderTarget
{
    public DeviceContext(Device device, DeviceContextOptions options)
        : this(Create(device, options))
    {
    }

    internal DeviceContext(W32D2D.ID2D1DeviceContext native)
        : base(native)
    {
    }

    internal new W32D2D.ID2D1DeviceContext Native => GetNative<W32D2D.ID2D1DeviceContext>();

    public Bitmap1 Target
    {
        set => Native.SetTarget(value.NativeBitmap);
    }

    public Bitmap1? GetTarget()
    {
        Native.GetTarget(out var image);
        return image is null ? null : new Bitmap1((W32D2D.ID2D1Bitmap1)image);
    }

    public W32D2D.D2D1_PRIMITIVE_BLEND PrimitiveBlend
    {
        get => Native.GetPrimitiveBlend();
        set => Native.SetPrimitiveBlend(value);
    }

    public void DrawBitmap(Bitmap bitmap, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF? destinationRectangle, float opacity, InterpolationMode interpolationMode, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF? sourceRectangle, object? perspectiveTransform)
    {
        Native.DrawBitmap(bitmap.Native, destinationRectangle?.ToWin32(), opacity, interpolationMode.ToWin32(), sourceRectangle?.ToWin32(), null);
    }

    private static W32D2D.ID2D1DeviceContext Create(Device device, DeviceContextOptions options)
    {
        device.Native.CreateDeviceContext((W32D2D.D2D1_DEVICE_CONTEXT_OPTIONS)(int)options, out var context);
        return context;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1Bitmap))]
public class Bitmap : Resource
{
    internal Bitmap(W32D2D.ID2D1Bitmap native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1Bitmap Native => GetNative<W32D2D.ID2D1Bitmap>();

    public Avalonia.Direct2D1.Interop.Size2 PixelSize => Native.GetPixelSize().ToInterop();
}

[NativeInterface(typeof(W32D2D.ID2D1Bitmap1))]
public sealed class Bitmap1 : Bitmap
{
    public Bitmap1(DeviceContext deviceContext, Avalonia.Direct2D1.Interop.DXGI.Surface surface, BitmapProperties1 properties)
        : this(Create(deviceContext, surface, properties))
    {
    }

    internal Bitmap1(W32D2D.ID2D1Bitmap1 native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1Bitmap1 NativeBitmap => GetNative<W32D2D.ID2D1Bitmap1>();

    public static Bitmap FromWicBitmap(RenderTarget renderTarget, Avalonia.Direct2D1.Interop.WIC.BitmapSource bitmapSource)
    {
        if (renderTarget is DeviceContext deviceContext)
        {
            deviceContext.Native.CreateBitmapFromWicBitmap(bitmapSource.Native, default(W32D2D.D2D1_BITMAP_PROPERTIES1_unmanaged?), out var bitmap);
            return new Bitmap1(bitmap);
        }

        renderTarget.Native.CreateBitmapFromWicBitmap(bitmapSource.Native, default(W32D2D.D2D1_BITMAP_PROPERTIES?), out var bitmap0);
        return new Bitmap(bitmap0);
    }

    private static W32D2D.ID2D1Bitmap1 Create(DeviceContext deviceContext, Avalonia.Direct2D1.Interop.DXGI.Surface surface, BitmapProperties1 properties)
    {
        var nativeProperties = properties.ToWin32();
        deviceContext.Native.CreateBitmapFromDxgiSurface(surface.Native, nativeProperties, out var bitmap);
        return bitmap;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1BitmapRenderTarget))]
public sealed class BitmapRenderTarget : RenderTarget
{
    public BitmapRenderTarget(RenderTarget renderTarget, CompatibleRenderTargetOptions options, Avalonia.Direct2D1.Interop.Size2F size)
        : this(Create(renderTarget, options, size))
    {
    }

    internal BitmapRenderTarget(W32D2D.ID2D1BitmapRenderTarget native)
        : base(native)
    {
    }

    private new W32D2D.ID2D1BitmapRenderTarget Native => GetNative<W32D2D.ID2D1BitmapRenderTarget>();

    public Bitmap Bitmap
    {
        get
        {
            Native.GetBitmap(out var bitmap);
            return new Bitmap(bitmap);
        }
    }

    private static W32D2D.ID2D1BitmapRenderTarget Create(RenderTarget renderTarget, CompatibleRenderTargetOptions options, Avalonia.Direct2D1.Interop.Size2F size)
    {
        var nativeSize = size.ToWin32();
        renderTarget.Native.CreateCompatibleRenderTarget(nativeSize, null, null, options.ToWin32(), out var bitmapRenderTarget);
        return bitmapRenderTarget;
    }
}

public sealed class WicRenderTarget : RenderTarget
{
    public WicRenderTarget(Factory1 factory, Avalonia.Direct2D1.Interop.WIC.Bitmap target, RenderTargetProperties properties)
        : base(Create(factory, target, properties))
    {
    }

    private static W32D2D.ID2D1RenderTarget Create(Factory1 factory, Avalonia.Direct2D1.Interop.WIC.Bitmap target, RenderTargetProperties properties)
    {
        factory.Native.CreateWicBitmapRenderTarget(target.NativeBitmap, properties.ToWin32(), out var renderTarget);
        return renderTarget;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1Layer))]
public sealed class Layer : Resource
{
    public Layer(RenderTarget renderTarget)
        : this(Create(renderTarget))
    {
    }

    internal Layer(W32D2D.ID2D1Layer native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1Layer Native => GetNative<W32D2D.ID2D1Layer>();

    private static W32D2D.ID2D1Layer Create(RenderTarget renderTarget)
    {
        renderTarget.Native.CreateLayer(default(W32D2DC.D2D_SIZE_F?), out var layer);
        return layer;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1Geometry))]
public class Geometry : Resource
{
    internal Geometry(W32D2D.ID2D1Geometry native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1Geometry Native => GetNative<W32D2D.ID2D1Geometry>();

    public Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF GetBounds()
    {
        Native.GetBounds(default(W32D2DC.D2D_MATRIX_3X2_F?), out var bounds);
        return bounds.ToRaw();
    }

    public Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF GetWidenedBounds(float strokeWidth)
    {
        Native.GetWidenedBounds(strokeWidth, null!, null, 0.25f, out var bounds);
        return bounds.ToRaw();
    }

    public void Widen(float strokeWidth, StrokeStyle? strokeStyle, float flatteningTolerance, GeometrySink geometrySink)
    {
        Native.Widen(strokeWidth, strokeStyle?.Native!, default(W32D2DC.D2D_MATRIX_3X2_F?), flatteningTolerance, geometrySink.Native);
    }

    public bool FillContainsPoint(Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point)
    {
        Native.FillContainsPoint(point.ToWin32(), default(W32D2DC.D2D_MATRIX_3X2_F?), 0.25f, out var contains);
        return contains;
    }

    public void Combine(Geometry other, CombineMode combineMode, GeometrySink geometrySink)
    {
        Native.CombineWithGeometry(other.Native, combineMode.ToWin32(), default(W32D2DC.D2D_MATRIX_3X2_F?), 0.25f, geometrySink.Native);
    }

    public bool StrokeContainsPoint(Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point, float strokeWidth)
    {
        Native.StrokeContainsPoint(point.ToWin32(), strokeWidth, null!, default(W32D2DC.D2D_MATRIX_3X2_F?), 0.25f, out var contains);
        return contains;
    }

    public float ComputeLength(object? worldTransform, float flatteningTolerance)
    {
        Native.ComputeLength(default(W32D2DC.D2D_MATRIX_3X2_F?), flatteningTolerance, out var length);
        return length;
    }

    public void ComputePointAtLength(float length, object? worldTransform, float flatteningTolerance, out Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point)
    {
        Native.ComputePointAtLength(length, default(W32D2DC.D2D_MATRIX_3X2_F?), flatteningTolerance, out var nativePoint, out _);
        point = nativePoint.ToRaw();
    }
}

[NativeInterface(typeof(W32D2D.ID2D1RectangleGeometry))]
public sealed class RectangleGeometry : Geometry
{
    public RectangleGeometry(Factory1 factory, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF rectangle)
        : this(Create(factory, rectangle))
    {
    }

    internal RectangleGeometry(W32D2D.ID2D1RectangleGeometry native)
        : base(native)
    {
    }

    private static W32D2D.ID2D1RectangleGeometry Create(Factory1 factory, Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF rectangle)
    {
        var nativeRectangle = rectangle.ToWin32();
        factory.Native.CreateRectangleGeometry(in nativeRectangle, out var geometry);
        return geometry;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1EllipseGeometry))]
public sealed class EllipseGeometry : Geometry
{
    public EllipseGeometry(Factory1 factory, Ellipse ellipse)
        : this(Create(factory, ellipse))
    {
    }

    internal EllipseGeometry(W32D2D.ID2D1EllipseGeometry native)
        : base(native)
    {
    }

    private static W32D2D.ID2D1EllipseGeometry Create(Factory1 factory, Ellipse ellipse)
    {
        var nativeEllipse = ellipse.ToWin32();
        factory.Native.CreateEllipseGeometry(in nativeEllipse, out var geometry);
        return geometry;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1GeometryGroup))]
public sealed class GeometryGroup : Geometry
{
    public GeometryGroup(Factory1 factory, FillMode fillMode, Geometry[] geometries)
        : this(Create(factory, fillMode, geometries))
    {
    }

    internal GeometryGroup(W32D2D.ID2D1GeometryGroup native)
        : base(native)
    {
    }

    private static W32D2D.ID2D1GeometryGroup Create(Factory1 factory, FillMode fillMode, Geometry[] geometries)
    {
        var nativeGeometries = new W32D2D.ID2D1Geometry[geometries.Length];

        for (var i = 0; i < geometries.Length; i++)
        {
            nativeGeometries[i] = geometries[i].Native;
        }

        factory.Native.CreateGeometryGroup(fillMode.ToWin32(), nativeGeometries, out var group);
        return group;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1PathGeometry))]
public class PathGeometry : Geometry
{
    public PathGeometry(Factory1 factory)
        : this(Create(factory))
    {
    }

    internal PathGeometry(W32D2D.ID2D1PathGeometry native)
        : base(native)
    {
    }

    private new W32D2D.ID2D1PathGeometry Native => GetNative<W32D2D.ID2D1PathGeometry>();

    public GeometrySink Open()
    {
        Native.Open(out var sink);
        return new GeometrySink(sink);
    }

    public void Stream(GeometrySink sink)
    {
        Native.Stream(sink.Native);
    }

    private static W32D2D.ID2D1PathGeometry Create(Factory1 factory)
    {
        W32D2D.ID2D1PathGeometry geometry;
        factory.Native.CreatePathGeometry(out geometry);
        return geometry;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1TransformedGeometry))]
public sealed class TransformedGeometry : Geometry
{
    private readonly Geometry _sourceGeometry;

    public TransformedGeometry(Factory1 factory, Geometry sourceGeometry, Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 transform)
        : this(Create(factory, sourceGeometry, transform))
    {
        _sourceGeometry = sourceGeometry;
    }

    internal TransformedGeometry(W32D2D.ID2D1TransformedGeometry native)
        : base(native)
    {
        native.GetSourceGeometry(out var sourceGeometry);
        _sourceGeometry = new Geometry(sourceGeometry);
    }

    private new W32D2D.ID2D1TransformedGeometry Native => GetNative<W32D2D.ID2D1TransformedGeometry>();

    public Geometry SourceGeometry => _sourceGeometry;

    public Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 Transform
    {
        get
        {
            Native.GetTransform(out var transform);
            return transform.ToRaw();
        }
    }

    private static W32D2D.ID2D1TransformedGeometry Create(Factory1 factory, Geometry sourceGeometry, Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 transform)
    {
        var nativeTransform = transform.ToWin32();
        factory.Native.CreateTransformedGeometry(sourceGeometry.Native, in nativeTransform, out var geometry);
        return geometry;
    }
}

[NativeInterface(typeof(W32D2D.ID2D1GeometrySink))]
public sealed class GeometrySink : CppObject
{
    internal GeometrySink(W32D2D.ID2D1GeometrySink native)
        : base(native)
    {
    }

    internal W32D2D.ID2D1GeometrySink Native => GetNative<W32D2D.ID2D1GeometrySink>();

    public void AddArc(ArcSegment arc)
    {
        var nativeArc = arc.ToWin32();
        Native.AddArc(in nativeArc);
    }

    public void BeginFigure(Avalonia.Direct2D1.Interop.Mathematics.RawVector2 startPoint, FigureBegin figureBegin)
    {
        Native.BeginFigure(startPoint.ToWin32(), figureBegin.ToWin32());
    }

    public void AddBezier(BezierSegment bezier)
    {
        var nativeBezier = bezier.ToWin32();
        Native.AddBezier(in nativeBezier);
    }

    public void AddQuadraticBezier(QuadraticBezierSegment bezier)
    {
        var nativeBezier = bezier.ToWin32();
        Native.AddQuadraticBezier(in nativeBezier);
    }

    public void AddLine(Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point)
    {
        Native.AddLine(point.ToWin32());
    }

    public void EndFigure(FigureEnd figureEnd)
    {
        Native.EndFigure(figureEnd.ToWin32());
    }

    public void SetFillMode(FillMode fillMode)
    {
        Native.SetFillMode(fillMode.ToWin32());
    }

    public void Close()
    {
        Native.Close();
    }
}

internal static class Direct2DConversions
{
    public static W32D2D.D2D1_EXTEND_MODE ToWin32(this ExtendMode extendMode) =>
        extendMode switch
        {
            ExtendMode.Clamp => W32D2D.D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP,
            ExtendMode.Wrap => W32D2D.D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_WRAP,
            _ => W32D2D.D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_MIRROR
        };

    public static W32D2D.D2D1_CAP_STYLE ToWin32(this CapStyle capStyle) =>
        (W32D2D.D2D1_CAP_STYLE)(int)capStyle;

    public static W32D2D.D2D1_LINE_JOIN ToWin32(this LineJoin lineJoin) =>
        (W32D2D.D2D1_LINE_JOIN)(int)lineJoin;

    public static W32D2D.D2D1_DASH_STYLE ToWin32(this DashStyle dashStyle) =>
        (W32D2D.D2D1_DASH_STYLE)(int)dashStyle;

    public static W32D2D.D2D1_BITMAP_INTERPOLATION_MODE ToWin32(this BitmapInterpolationMode interpolationMode) =>
        interpolationMode switch
        {
            BitmapInterpolationMode.NearestNeighbor => W32D2D.D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_NEAREST_NEIGHBOR,
            _ => W32D2D.D2D1_BITMAP_INTERPOLATION_MODE.D2D1_BITMAP_INTERPOLATION_MODE_LINEAR
        };

    public static W32D2D.D2D1_INTERPOLATION_MODE ToWin32(this InterpolationMode interpolationMode) =>
        interpolationMode switch
        {
            InterpolationMode.NearestNeighbor => W32D2D.D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_NEAREST_NEIGHBOR,
            InterpolationMode.Linear => W32D2D.D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_LINEAR,
            InterpolationMode.MultiSampleLinear => W32D2D.D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_MULTI_SAMPLE_LINEAR,
            InterpolationMode.HighQualityCubic => W32D2D.D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC,
            InterpolationMode.Cubic => W32D2D.D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_CUBIC,
            _ => W32D2D.D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_ANISOTROPIC
        };

    public static W32D2D.D2D1_ANTIALIAS_MODE ToWin32(this AntialiasMode antialiasMode) =>
        antialiasMode == AntialiasMode.Aliased
            ? W32D2D.D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED
            : W32D2D.D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE;

    public static AntialiasMode ToCompat(this W32D2D.D2D1_ANTIALIAS_MODE antialiasMode) =>
        antialiasMode == W32D2D.D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED
            ? AntialiasMode.Aliased
            : AntialiasMode.PerPrimitive;

    public static W32D2D.D2D1_TEXT_ANTIALIAS_MODE ToWin32(this TextAntialiasMode textAntialiasMode) =>
        (W32D2D.D2D1_TEXT_ANTIALIAS_MODE)(int)textAntialiasMode;

    public static TextAntialiasMode ToCompat(this W32D2D.D2D1_TEXT_ANTIALIAS_MODE textAntialiasMode) =>
        (TextAntialiasMode)(int)textAntialiasMode;

    public static W32D2D.D2D1_COMPATIBLE_RENDER_TARGET_OPTIONS ToWin32(this CompatibleRenderTargetOptions options) =>
        (W32D2D.D2D1_COMPATIBLE_RENDER_TARGET_OPTIONS)(int)options;

    public static W32D2D.D2D1_BITMAP_OPTIONS ToWin32(this BitmapOptions options) =>
        (W32D2D.D2D1_BITMAP_OPTIONS)(int)options;

    public static W32D2D.Common.D2D1_FILL_MODE ToWin32(this FillMode fillMode) =>
        fillMode == FillMode.Winding
            ? W32D2D.Common.D2D1_FILL_MODE.D2D1_FILL_MODE_WINDING
            : W32D2D.Common.D2D1_FILL_MODE.D2D1_FILL_MODE_ALTERNATE;

    public static W32D2DC.D2D1_FIGURE_BEGIN ToWin32(this FigureBegin figureBegin) =>
        (W32D2DC.D2D1_FIGURE_BEGIN)(int)figureBegin;

    public static W32D2DC.D2D1_FIGURE_END ToWin32(this FigureEnd figureEnd) =>
        (W32D2DC.D2D1_FIGURE_END)(int)figureEnd;

    public static W32D2D.D2D1_SWEEP_DIRECTION ToWin32(this SweepDirection sweepDirection) =>
        (W32D2D.D2D1_SWEEP_DIRECTION)(int)sweepDirection;

    public static W32D2D.D2D1_ARC_SIZE ToWin32(this ArcSize arcSize) =>
        (W32D2D.D2D1_ARC_SIZE)(int)arcSize;

    public static W32D2D.D2D1_COMBINE_MODE ToWin32(this CombineMode combineMode) =>
        (W32D2D.D2D1_COMBINE_MODE)(int)combineMode;

    public static W32D2D.Common.D2D_RECT_F ToWin32(this Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF rectangle) =>
        new()
        {
            left = rectangle.Left,
            top = rectangle.Top,
            right = rectangle.Right,
            bottom = rectangle.Bottom
        };

    public static Avalonia.Direct2D1.Interop.Mathematics.RawRectangleF ToRaw(this W32D2D.Common.D2D_RECT_F rectangle) =>
        new(rectangle.left, rectangle.top, rectangle.right, rectangle.bottom);

    public static W32D2D.Common.D2D_POINT_2F ToWin32(this Avalonia.Direct2D1.Interop.Mathematics.RawVector2 point) =>
        new()
        {
            x = point.X,
            y = point.Y
        };

    public static Avalonia.Direct2D1.Interop.Mathematics.RawVector2 ToRaw(this W32D2D.Common.D2D_POINT_2F point) =>
        new() { X = point.x, Y = point.y };

    public static W32D2D.Common.D2D_SIZE_F ToWin32(this Avalonia.Direct2D1.Interop.Size2F size) =>
        new()
        {
            width = size.Width,
            height = size.Height
        };

    public static Avalonia.Direct2D1.Interop.Size2 ToInterop(this W32D2D.Common.D2D_SIZE_U size) =>
        new((int)size.width, (int)size.height);

    public static W32D2D.Common.D2D_MATRIX_3X2_F ToWin32(this Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 matrix) =>
        new()
        {
            Anonymous = new W32D2D.Common.D2D_MATRIX_3X2_F._Anonymous_e__Union
            {
                Anonymous1 = new W32D2D.Common.D2D_MATRIX_3X2_F._Anonymous_e__Union._Anonymous1_e__Struct
                {
                    m11 = matrix.M11,
                    m12 = matrix.M12,
                    m21 = matrix.M21,
                    m22 = matrix.M22,
                    dx = matrix.M31,
                    dy = matrix.M32
                }
            }
        };

    public static Avalonia.Direct2D1.Interop.Mathematics.RawMatrix3x2 ToRaw(this W32D2D.Common.D2D_MATRIX_3X2_F matrix) =>
        new(
            matrix.Anonymous.Anonymous1.m11,
            matrix.Anonymous.Anonymous1.m12,
            matrix.Anonymous.Anonymous1.m21,
            matrix.Anonymous.Anonymous1.m22,
            matrix.Anonymous.Anonymous1.dx,
            matrix.Anonymous.Anonymous1.dy);

    public static W32D2DC.D2D1_COLOR_F ToWin32(this Avalonia.Direct2D1.Interop.Mathematics.RawColor4 color) =>
        new()
        {
            r = color.R,
            g = color.G,
            b = color.B,
            a = color.A
        };

    public static W32D2DC.D2D1_PIXEL_FORMAT ToWin32(this PixelFormat pixelFormat) =>
        new()
        {
            format = pixelFormat.Format.ToWin32(),
            alphaMode = (W32D2DC.D2D1_ALPHA_MODE)(int)pixelFormat.AlphaMode
        };

    public static W32D2D.D2D1_RENDER_TARGET_PROPERTIES ToWin32(this RenderTargetProperties properties) =>
        new()
        {
            pixelFormat = properties.PixelFormat.ToWin32(),
            dpiX = properties.DpiX,
            dpiY = properties.DpiY,
            type = W32D2D.D2D1_RENDER_TARGET_TYPE.D2D1_RENDER_TARGET_TYPE_DEFAULT,
            usage = W32D2D.D2D1_RENDER_TARGET_USAGE.D2D1_RENDER_TARGET_USAGE_NONE,
            minLevel = W32D2D.D2D1_FEATURE_LEVEL.D2D1_FEATURE_LEVEL_DEFAULT
        };

    public static W32D2D.D2D1_BITMAP_PROPERTIES1_unmanaged ToWin32(this BitmapProperties1 properties) =>
        new()
        {
            pixelFormat = properties.PixelFormat.ToWin32(),
            dpiX = properties.DpiX,
            dpiY = properties.DpiY,
            bitmapOptions = properties.BitmapOptions.ToWin32(),
            colorContext = null
        };

    public static W32D2D.D2D1_BRUSH_PROPERTIES ToWin32(this BrushProperties properties) =>
        new()
        {
            opacity = properties.Opacity,
            transform = properties.Transform.ToWin32()
        };

    public static W32D2D.D2D1_BITMAP_BRUSH_PROPERTIES ToWin32(this BitmapBrushProperties properties) =>
        new()
        {
            extendModeX = properties.ExtendModeX.ToWin32(),
            extendModeY = properties.ExtendModeY.ToWin32(),
            interpolationMode = properties.InterpolationMode.ToWin32()
        };

    public static W32D2D.D2D1_BITMAP_BRUSH_PROPERTIES1 ToWin32(this BitmapBrushProperties1 properties) =>
        new()
        {
            extendModeX = properties.ExtendModeX.ToWin32(),
            extendModeY = properties.ExtendModeY.ToWin32(),
            interpolationMode = properties.InterpolationMode.ToWin32()
        };

    public static W32D2DC.D2D1_GRADIENT_STOP[] ToWin32(this GradientStop[] gradientStops)
    {
        var result = new W32D2DC.D2D1_GRADIENT_STOP[gradientStops.Length];

        for (var i = 0; i < gradientStops.Length; i++)
        {
            result[i] = new W32D2DC.D2D1_GRADIENT_STOP
            {
                position = gradientStops[i].Position,
                color = gradientStops[i].Color.ToWin32()
            };
        }

        return result;
    }

    public static W32D2D.D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES ToWin32(this LinearGradientBrushProperties properties) =>
        new()
        {
            startPoint = properties.StartPoint.ToWin32(),
            endPoint = properties.EndPoint.ToWin32()
        };

    public static W32D2D.D2D1_RADIAL_GRADIENT_BRUSH_PROPERTIES ToWin32(this RadialGradientBrushProperties properties) =>
        new()
        {
            center = properties.Center.ToWin32(),
            gradientOriginOffset = properties.GradientOriginOffset.ToWin32(),
            radiusX = properties.RadiusX,
            radiusY = properties.RadiusY
        };

    public static W32D2D.D2D1_STROKE_STYLE_PROPERTIES ToWin32(this StrokeStyleProperties properties) =>
        new()
        {
            startCap = properties.StartCap.ToWin32(),
            endCap = properties.EndCap.ToWin32(),
            dashCap = properties.DashCap.ToWin32(),
            lineJoin = properties.LineJoin.ToWin32(),
            miterLimit = properties.MiterLimit,
            dashStyle = properties.DashStyle.ToWin32(),
            dashOffset = properties.DashOffset
        };

    public static W32D2D.D2D1_ELLIPSE ToWin32(this Ellipse ellipse) =>
        new()
        {
            point = ellipse.Point.ToWin32(),
            radiusX = ellipse.RadiusX,
            radiusY = ellipse.RadiusY
        };

    public static W32D2D.D2D1_ROUNDED_RECT ToWin32(this RoundedRectangle roundedRectangle) =>
        new()
        {
            rect = roundedRectangle.Rect.ToWin32(),
            radiusX = roundedRectangle.RadiusX,
            radiusY = roundedRectangle.RadiusY
        };

    public static W32D2D.D2D1_ARC_SEGMENT ToWin32(this ArcSegment arcSegment) =>
        new()
        {
            point = arcSegment.Point.ToWin32(),
            size = arcSegment.Size.ToWin32(),
            rotationAngle = arcSegment.RotationAngle,
            sweepDirection = arcSegment.SweepDirection.ToWin32(),
            arcSize = arcSegment.ArcSize.ToWin32()
        };

    public static W32D2DC.D2D1_BEZIER_SEGMENT ToWin32(this BezierSegment bezierSegment) =>
        new()
        {
            point1 = bezierSegment.Point1.ToWin32(),
            point2 = bezierSegment.Point2.ToWin32(),
            point3 = bezierSegment.Point3.ToWin32()
        };

    public static W32D2D.D2D1_QUADRATIC_BEZIER_SEGMENT ToWin32(this QuadraticBezierSegment bezierSegment) =>
        new()
        {
            point1 = bezierSegment.Point1.ToWin32(),
            point2 = bezierSegment.Point2.ToWin32()
        };

    public static unsafe W32D2D.D2D1_LAYER_PARAMETERS ToWin32(this LayerParameters parameters) =>
        new()
        {
            contentBounds = parameters.ContentBounds.ToWin32(),
            geometricMask = parameters.GeometricMask is null
                ? null
                : (W32D2D.ID2D1Geometry_unmanaged*)parameters.GeometricMask.NativePointer,
            maskAntialiasMode = parameters.MaskAntialiasMode.ToWin32(),
            maskTransform = parameters.MaskTransform.ToWin32(),
            opacity = parameters.Opacity,
            opacityBrush = parameters.OpacityBrush is null
                ? null
                : (W32D2D.ID2D1Brush_unmanaged*)parameters.OpacityBrush.NativePointer,
            layerOptions = W32D2D.D2D1_LAYER_OPTIONS.D2D1_LAYER_OPTIONS_NONE
        };
}
