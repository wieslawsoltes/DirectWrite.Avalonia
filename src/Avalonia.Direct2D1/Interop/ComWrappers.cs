using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using D2DInterop = Avalonia.Direct2D1.Interop.Direct2D1;
using D3D11Interop = Avalonia.Direct2D1.Interop.Direct3D11;
using DXGIInterop = Avalonia.Direct2D1.Interop.DXGI;
using WICInterop = Avalonia.Direct2D1.Interop.WIC;
using W32D2D = Windows.Win32.Graphics.Direct2D;
using W32D3D11 = Windows.Win32.Graphics.Direct3D11;
using W32DXGI = Windows.Win32.Graphics.Dxgi;
using W32WIC = Windows.Win32.Graphics.Imaging;
using W32WICD2D = Windows.Win32.Graphics.Imaging.D2D;

namespace Avalonia.Direct2D1.Interop;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
internal sealed class NativeInterfaceAttribute : Attribute
{
    public NativeInterfaceAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }

    public Type InterfaceType { get; }
}

public class DirectXException : COMException
{
    public DirectXException(int result)
        : base(result.ToString("X8"), result)
    {
        ResultCode = result;
    }

    public int ResultCode { get; }
}

public abstract class ComObject : IDisposable
{
    private bool _disposed;
    private readonly Type _nativeInterfaceType;

    protected ComObject(object native)
    {
        NativeObject = native ?? throw new ArgumentNullException(nameof(native));
        _nativeInterfaceType = ComTypeRegistry.GetNativeInterfaceType(GetType());
        NativePointer = ComMarshaller.ConvertToUnmanaged(_nativeInterfaceType, NativeObject);
    }

    public IntPtr NativePointer { get; private set; }

    protected object NativeObject { get; private set; }

    internal object NativeComObject => NativeObject;

    public T QueryInterface<T>()
        where T : ComObject
    {
        var interfaceType = ComTypeRegistry.GetNativeInterfaceType(typeof(T));
        var iid = interfaceType.GUID;

        Marshal.QueryInterface(NativePointer, in iid, out var ptr);

        try
        {
            var native = ComMarshaller.ConvertToManaged(interfaceType, ptr);
            return ComObjectFactory.Create<T>(native);
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                ComMarshaller.Free(interfaceType, ptr);
            }
        }
    }

    protected static void CheckError(int result)
    {
        if (result < 0)
        {
            throw new DirectXException(result);
        }
    }

    protected static TInterface As<TInterface>(object value)
        where TInterface : class
    {
        return (TInterface)value;
    }

    protected TInterface GetNative<TInterface>()
        where TInterface : class
    {
        return As<TInterface>(NativeObject);
    }

    internal static Type GetNativeInterfaceType(Type wrapperType) => ComTypeRegistry.GetNativeInterfaceType(wrapperType);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            NativeObject = null!;
        }

        if (NativePointer != IntPtr.Zero)
        {
            ComMarshaller.Free(_nativeInterfaceType, NativePointer);
            NativePointer = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

public abstract class CppObject : ComObject
{
    protected CppObject(object native)
        : base(native)
    {
    }
}

internal readonly record struct ComInterfaceAdapter(
    Func<object, IntPtr> ConvertToUnmanaged,
    Func<IntPtr, object> ConvertToManaged,
    Action<IntPtr> Free);

internal static unsafe class ComMarshaller
{
    private static readonly Dictionary<Type, ComInterfaceAdapter> s_adapters = new()
    {
        [typeof(W32DXGI.IDXGIDevice1)] = CreateAdapter<W32DXGI.IDXGIDevice1>(),
        [typeof(W32DXGI.IDXGIAdapter)] = CreateAdapter<W32DXGI.IDXGIAdapter>(),
        [typeof(W32DXGI.IDXGIFactory2)] = CreateAdapter<W32DXGI.IDXGIFactory2>(),
        [typeof(W32DXGI.IDXGISurface)] = CreateAdapter<W32DXGI.IDXGISurface>(),
        [typeof(W32DXGI.IDXGISwapChain1)] = CreateAdapter<W32DXGI.IDXGISwapChain1>(),
        [typeof(W32D3D11.ID3D11Device)] = CreateAdapter<W32D3D11.ID3D11Device>(),
        [typeof(W32D2D.ID2D1Factory)] = CreateAdapter<W32D2D.ID2D1Factory>(),
        [typeof(W32D2D.ID2D1Factory1)] = CreateAdapter<W32D2D.ID2D1Factory1>(),
        [typeof(W32D2D.ID2D1Device)] = CreateAdapter<W32D2D.ID2D1Device>(),
        [typeof(W32D2D.ID2D1Resource)] = CreateAdapter<W32D2D.ID2D1Resource>(),
        [typeof(W32D2D.ID2D1Brush)] = CreateAdapter<W32D2D.ID2D1Brush>(),
        [typeof(W32D2D.ID2D1SolidColorBrush)] = CreateAdapter<W32D2D.ID2D1SolidColorBrush>(),
        [typeof(W32D2D.ID2D1GradientStopCollection)] = CreateAdapter<W32D2D.ID2D1GradientStopCollection>(),
        [typeof(W32D2D.ID2D1LinearGradientBrush)] = CreateAdapter<W32D2D.ID2D1LinearGradientBrush>(),
        [typeof(W32D2D.ID2D1RadialGradientBrush)] = CreateAdapter<W32D2D.ID2D1RadialGradientBrush>(),
        [typeof(W32D2D.ID2D1BitmapBrush)] = CreateAdapter<W32D2D.ID2D1BitmapBrush>(),
        [typeof(W32D2D.ID2D1BitmapBrush1)] = CreateAdapter<W32D2D.ID2D1BitmapBrush1>(),
        [typeof(W32D2D.ID2D1StrokeStyle)] = CreateAdapter<W32D2D.ID2D1StrokeStyle>(),
        [typeof(W32D2D.ID2D1RenderTarget)] = CreateAdapter<W32D2D.ID2D1RenderTarget>(),
        [typeof(W32D2D.ID2D1DeviceContext)] = CreateAdapter<W32D2D.ID2D1DeviceContext>(),
        [typeof(W32D2D.ID2D1Bitmap)] = CreateAdapter<W32D2D.ID2D1Bitmap>(),
        [typeof(W32D2D.ID2D1Bitmap1)] = CreateAdapter<W32D2D.ID2D1Bitmap1>(),
        [typeof(W32D2D.ID2D1BitmapRenderTarget)] = CreateAdapter<W32D2D.ID2D1BitmapRenderTarget>(),
        [typeof(W32D2D.ID2D1Layer)] = CreateAdapter<W32D2D.ID2D1Layer>(),
        [typeof(W32D2D.ID2D1Geometry)] = CreateAdapter<W32D2D.ID2D1Geometry>(),
        [typeof(W32D2D.ID2D1RectangleGeometry)] = CreateAdapter<W32D2D.ID2D1RectangleGeometry>(),
        [typeof(W32D2D.ID2D1EllipseGeometry)] = CreateAdapter<W32D2D.ID2D1EllipseGeometry>(),
        [typeof(W32D2D.ID2D1GeometryGroup)] = CreateAdapter<W32D2D.ID2D1GeometryGroup>(),
        [typeof(W32D2D.ID2D1PathGeometry)] = CreateAdapter<W32D2D.ID2D1PathGeometry>(),
        [typeof(W32D2D.ID2D1TransformedGeometry)] = CreateAdapter<W32D2D.ID2D1TransformedGeometry>(),
        [typeof(W32D2D.ID2D1GeometrySink)] = CreateAdapter<W32D2D.ID2D1GeometrySink>(),
        [typeof(W32WIC.IWICBitmapSource)] = CreateAdapter<W32WIC.IWICBitmapSource>(),
        [typeof(W32WIC.IWICBitmapFrameDecode)] = CreateAdapter<W32WIC.IWICBitmapFrameDecode>(),
        [typeof(W32WIC.IWICBitmapScaler)] = CreateAdapter<W32WIC.IWICBitmapScaler>(),
        [typeof(W32WIC.IWICFormatConverter)] = CreateAdapter<W32WIC.IWICFormatConverter>(),
        [typeof(W32WIC.IWICBitmap)] = CreateAdapter<W32WIC.IWICBitmap>(),
        [typeof(W32WIC.IWICBitmapLock)] = CreateAdapter<W32WIC.IWICBitmapLock>(),
        [typeof(W32WIC.IWICBitmapDecoder)] = CreateAdapter<W32WIC.IWICBitmapDecoder>(),
        [typeof(W32WICD2D.IWICImagingFactory2)] = CreateAdapter<W32WICD2D.IWICImagingFactory2>(),
    };

    private static ComInterfaceAdapter CreateAdapter<T>()
        where T : class
    {
        return new ComInterfaceAdapter(
            managed => managed is null ? IntPtr.Zero : (IntPtr)ComInterfaceMarshaller<T>.ConvertToUnmanaged((T)managed),
            unmanaged => ComInterfaceMarshaller<T>.ConvertToManaged((void*)unmanaged)
                ?? throw new InvalidOperationException($"Unable to convert {typeof(T).FullName} to a managed COM object."),
            unmanaged =>
            {
                if (unmanaged != IntPtr.Zero)
                {
                    ComInterfaceMarshaller<T>.Free((void*)unmanaged);
                }
            });
    }

    public static IntPtr ConvertToUnmanaged(Type interfaceType, object managed)
    {
        return GetAdapter(interfaceType).ConvertToUnmanaged(managed);
    }

    public static object ConvertToManaged(Type interfaceType, IntPtr unmanaged)
    {
        return GetAdapter(interfaceType).ConvertToManaged(unmanaged);
    }

    public static void Free(Type interfaceType, IntPtr unmanaged)
    {
        GetAdapter(interfaceType).Free(unmanaged);
    }

    private static ComInterfaceAdapter GetAdapter(Type interfaceType)
    {
        if (s_adapters.TryGetValue(interfaceType, out var adapter))
        {
            return adapter;
        }

        throw new InvalidOperationException($"Unsupported COM interface type '{interfaceType.FullName}'.");
    }
}

internal static class ComTypeRegistry
{
    private static readonly Dictionary<Type, Type> s_nativeInterfaceTypes = new()
    {
        [typeof(DXGIInterop.Device1)] = typeof(W32DXGI.IDXGIDevice1),
        [typeof(DXGIInterop.Adapter)] = typeof(W32DXGI.IDXGIAdapter),
        [typeof(DXGIInterop.Factory2)] = typeof(W32DXGI.IDXGIFactory2),
        [typeof(DXGIInterop.Surface)] = typeof(W32DXGI.IDXGISurface),
        [typeof(DXGIInterop.SwapChain1)] = typeof(W32DXGI.IDXGISwapChain1),
        [typeof(D3D11Interop.Device)] = typeof(W32D3D11.ID3D11Device),
        [typeof(D2DInterop.Factory)] = typeof(W32D2D.ID2D1Factory),
        [typeof(D2DInterop.Factory1)] = typeof(W32D2D.ID2D1Factory1),
        [typeof(D2DInterop.Device)] = typeof(W32D2D.ID2D1Device),
        [typeof(D2DInterop.Resource)] = typeof(W32D2D.ID2D1Resource),
        [typeof(D2DInterop.Brush)] = typeof(W32D2D.ID2D1Brush),
        [typeof(D2DInterop.SolidColorBrush)] = typeof(W32D2D.ID2D1SolidColorBrush),
        [typeof(D2DInterop.GradientStopCollection)] = typeof(W32D2D.ID2D1GradientStopCollection),
        [typeof(D2DInterop.LinearGradientBrush)] = typeof(W32D2D.ID2D1LinearGradientBrush),
        [typeof(D2DInterop.RadialGradientBrush)] = typeof(W32D2D.ID2D1RadialGradientBrush),
        [typeof(D2DInterop.BitmapBrush)] = typeof(W32D2D.ID2D1BitmapBrush),
        [typeof(D2DInterop.BitmapBrush1)] = typeof(W32D2D.ID2D1BitmapBrush1),
        [typeof(D2DInterop.StrokeStyle)] = typeof(W32D2D.ID2D1StrokeStyle),
        [typeof(D2DInterop.RenderTarget)] = typeof(W32D2D.ID2D1RenderTarget),
        [typeof(D2DInterop.DeviceContext)] = typeof(W32D2D.ID2D1DeviceContext),
        [typeof(D2DInterop.Bitmap)] = typeof(W32D2D.ID2D1Bitmap),
        [typeof(D2DInterop.Bitmap1)] = typeof(W32D2D.ID2D1Bitmap1),
        [typeof(D2DInterop.BitmapRenderTarget)] = typeof(W32D2D.ID2D1BitmapRenderTarget),
        [typeof(D2DInterop.Layer)] = typeof(W32D2D.ID2D1Layer),
        [typeof(D2DInterop.Geometry)] = typeof(W32D2D.ID2D1Geometry),
        [typeof(D2DInterop.RectangleGeometry)] = typeof(W32D2D.ID2D1RectangleGeometry),
        [typeof(D2DInterop.EllipseGeometry)] = typeof(W32D2D.ID2D1EllipseGeometry),
        [typeof(D2DInterop.GeometryGroup)] = typeof(W32D2D.ID2D1GeometryGroup),
        [typeof(D2DInterop.PathGeometry)] = typeof(W32D2D.ID2D1PathGeometry),
        [typeof(D2DInterop.TransformedGeometry)] = typeof(W32D2D.ID2D1TransformedGeometry),
        [typeof(D2DInterop.GeometrySink)] = typeof(W32D2D.ID2D1GeometrySink),
        [typeof(WICInterop.BitmapSource)] = typeof(W32WIC.IWICBitmapSource),
        [typeof(WICInterop.BitmapFrameDecode)] = typeof(W32WIC.IWICBitmapFrameDecode),
        [typeof(WICInterop.BitmapScaler)] = typeof(W32WIC.IWICBitmapScaler),
        [typeof(WICInterop.FormatConverter)] = typeof(W32WIC.IWICFormatConverter),
        [typeof(WICInterop.Bitmap)] = typeof(W32WIC.IWICBitmap),
        [typeof(WICInterop.BitmapLock)] = typeof(W32WIC.IWICBitmapLock),
        [typeof(WICInterop.BitmapDecoder)] = typeof(W32WIC.IWICBitmapDecoder),
        [typeof(WICInterop.ImagingFactory)] = typeof(W32WICD2D.IWICImagingFactory2),
    };

    public static Type GetNativeInterfaceType(Type wrapperType)
    {
        for (var current = wrapperType; current is not null; current = current.BaseType)
        {
            if (s_nativeInterfaceTypes.TryGetValue(current, out var interfaceType))
            {
                return interfaceType;
            }
        }

        throw new InvalidOperationException($"Unsupported COM wrapper type '{wrapperType.FullName}'.");
    }
}

internal static class ComObjectFactory
{
    private static readonly Dictionary<Type, Func<object, ComObject>> s_wrapperFactories = new()
    {
        [typeof(DXGIInterop.Device1)] = native => new DXGIInterop.Device1((W32DXGI.IDXGIDevice1)native),
        [typeof(DXGIInterop.Adapter)] = native => new DXGIInterop.Adapter((W32DXGI.IDXGIAdapter)native),
        [typeof(DXGIInterop.Factory2)] = native => new DXGIInterop.Factory2((W32DXGI.IDXGIFactory2)native),
        [typeof(DXGIInterop.Surface)] = native => new DXGIInterop.Surface((W32DXGI.IDXGISurface)native),
        [typeof(DXGIInterop.SwapChain1)] = native => new DXGIInterop.SwapChain1((W32DXGI.IDXGISwapChain1)native),
        [typeof(D3D11Interop.Device)] = native => new D3D11Interop.Device((W32D3D11.ID3D11Device)native),
        [typeof(D2DInterop.Factory)] = native => new D2DInterop.Factory((W32D2D.ID2D1Factory)native),
        [typeof(D2DInterop.Factory1)] = native => new D2DInterop.Factory1((W32D2D.ID2D1Factory1)native),
        [typeof(D2DInterop.Device)] = native => new D2DInterop.Device((W32D2D.ID2D1Device)native),
        [typeof(D2DInterop.SolidColorBrush)] = native => new D2DInterop.SolidColorBrush((W32D2D.ID2D1SolidColorBrush)native),
        [typeof(D2DInterop.GradientStopCollection)] = native => new D2DInterop.GradientStopCollection((W32D2D.ID2D1GradientStopCollection)native),
        [typeof(D2DInterop.LinearGradientBrush)] = native => new D2DInterop.LinearGradientBrush((W32D2D.ID2D1LinearGradientBrush)native),
        [typeof(D2DInterop.RadialGradientBrush)] = native => new D2DInterop.RadialGradientBrush((W32D2D.ID2D1RadialGradientBrush)native),
        [typeof(D2DInterop.BitmapBrush)] = native => new D2DInterop.BitmapBrush((W32D2D.ID2D1BitmapBrush)native),
        [typeof(D2DInterop.BitmapBrush1)] = native => new D2DInterop.BitmapBrush1((W32D2D.ID2D1BitmapBrush1)native),
        [typeof(D2DInterop.StrokeStyle)] = native => new D2DInterop.StrokeStyle((W32D2D.ID2D1StrokeStyle)native),
        [typeof(D2DInterop.RenderTarget)] = native => new D2DInterop.RenderTarget((W32D2D.ID2D1RenderTarget)native),
        [typeof(D2DInterop.DeviceContext)] = native => new D2DInterop.DeviceContext((W32D2D.ID2D1DeviceContext)native),
        [typeof(D2DInterop.Bitmap)] = native => new D2DInterop.Bitmap((W32D2D.ID2D1Bitmap)native),
        [typeof(D2DInterop.Bitmap1)] = native => new D2DInterop.Bitmap1((W32D2D.ID2D1Bitmap1)native),
        [typeof(D2DInterop.BitmapRenderTarget)] = native => new D2DInterop.BitmapRenderTarget((W32D2D.ID2D1BitmapRenderTarget)native),
        [typeof(D2DInterop.Layer)] = native => new D2DInterop.Layer((W32D2D.ID2D1Layer)native),
        [typeof(D2DInterop.Geometry)] = native => new D2DInterop.Geometry((W32D2D.ID2D1Geometry)native),
        [typeof(D2DInterop.RectangleGeometry)] = native => new D2DInterop.RectangleGeometry((W32D2D.ID2D1RectangleGeometry)native),
        [typeof(D2DInterop.EllipseGeometry)] = native => new D2DInterop.EllipseGeometry((W32D2D.ID2D1EllipseGeometry)native),
        [typeof(D2DInterop.GeometryGroup)] = native => new D2DInterop.GeometryGroup((W32D2D.ID2D1GeometryGroup)native),
        [typeof(D2DInterop.PathGeometry)] = native => new D2DInterop.PathGeometry((W32D2D.ID2D1PathGeometry)native),
        [typeof(D2DInterop.TransformedGeometry)] = native => new D2DInterop.TransformedGeometry((W32D2D.ID2D1TransformedGeometry)native),
        [typeof(D2DInterop.GeometrySink)] = native => new D2DInterop.GeometrySink((W32D2D.ID2D1GeometrySink)native),
        [typeof(WICInterop.BitmapSource)] = native => new WICInterop.BitmapSource((W32WIC.IWICBitmapSource)native),
        [typeof(WICInterop.BitmapFrameDecode)] = native => new WICInterop.BitmapFrameDecode((W32WIC.IWICBitmapFrameDecode)native),
        [typeof(WICInterop.BitmapScaler)] = native => new WICInterop.BitmapScaler((W32WIC.IWICBitmapScaler)native),
        [typeof(WICInterop.FormatConverter)] = native => new WICInterop.FormatConverter((W32WIC.IWICFormatConverter)native),
        [typeof(WICInterop.Bitmap)] = native => new WICInterop.Bitmap((W32WIC.IWICBitmap)native),
        [typeof(WICInterop.BitmapLock)] = native => new WICInterop.BitmapLock((W32WIC.IWICBitmapLock)native),
        [typeof(WICInterop.BitmapDecoder)] = native => new WICInterop.BitmapDecoder((W32WIC.IWICBitmapDecoder)native, temporaryFilePath: null),
        [typeof(WICInterop.ImagingFactory)] = native => new WICInterop.ImagingFactory((W32WICD2D.IWICImagingFactory2)native),
    };

    public static T Create<T>(object native)
        where T : ComObject
    {
        if (s_wrapperFactories.TryGetValue(typeof(T), out var factory))
        {
            return (T)factory(native);
        }

        throw new MissingMethodException($"Constructor on type '{typeof(T).FullName}' not found.");
    }
}
