using System;
using Windows.Win32.Graphics.Dxgi;
using W32DXGI = Windows.Win32.Graphics.Dxgi;
using W32DXGIC = Windows.Win32.Graphics.Dxgi.Common;

namespace Avalonia.Direct2D1.Interop.DXGI;

public abstract class DxgiObject : ComObject
{
    protected DxgiObject(object native)
        : base(native)
    {
    }

    public unsafe T GetParent<T>()
        where T : ComObject
    {
        var iid = ComObject.GetNativeInterfaceType(typeof(T)).GUID;
        GetNative<W32DXGI.IDXGIObject>().GetParent(&iid, out var parent);
        return ComObjectFactory.Create<T>(parent);
    }
}

[NativeInterface(typeof(W32DXGI.IDXGIDevice1))]
public sealed class Device1 : DxgiObject
{
    internal Device1(W32DXGI.IDXGIDevice1 native)
        : base(native)
    {
    }

    internal W32DXGI.IDXGIDevice1 Native => GetNative<W32DXGI.IDXGIDevice1>();

    public Adapter Adapter
    {
        get
        {
            Native.GetAdapter(out var adapter);
            return new Adapter(adapter);
        }
    }
}

[NativeInterface(typeof(W32DXGI.IDXGIAdapter))]
public sealed class Adapter : DxgiObject
{
    internal Adapter(W32DXGI.IDXGIAdapter native)
        : base(native)
    {
    }

    internal W32DXGI.IDXGIAdapter Native => GetNative<W32DXGI.IDXGIAdapter>();
}

[NativeInterface(typeof(W32DXGI.IDXGIFactory2))]
public sealed class Factory2 : DxgiObject
{
    internal Factory2(W32DXGI.IDXGIFactory2 native)
        : base(native)
    {
    }

    internal W32DXGI.IDXGIFactory2 Native => GetNative<W32DXGI.IDXGIFactory2>();
}

[NativeInterface(typeof(W32DXGI.IDXGISurface))]
public sealed class Surface : DxgiObject
{
    internal Surface(W32DXGI.IDXGISurface native)
        : base(native)
    {
    }

    internal W32DXGI.IDXGISurface Native => GetNative<W32DXGI.IDXGISurface>();
}

[NativeInterface(typeof(W32DXGI.IDXGISwapChain1))]
public sealed class SwapChain1 : DxgiObject
{
    public unsafe SwapChain1(Factory2 factory, Device1 device, IntPtr hwnd, ref SwapChainDescription1 description)
        : this(Create(factory, device, hwnd, description))
    {
    }

    internal SwapChain1(W32DXGI.IDXGISwapChain1 native)
        : base(native)
    {
    }

    internal W32DXGI.IDXGISwapChain1 Native => GetNative<W32DXGI.IDXGISwapChain1>();

    public unsafe void Present(int syncInterval, PresentFlags flags)
    {
        Native.Present1((uint)syncInterval, flags.ToWin32(), default(W32DXGI.DXGI_PRESENT_PARAMETERS*)).ThrowOnFailure();
    }

    public void ResizeBuffers(int bufferCount, int width, int height, Format format, SwapChainFlags flags)
    {
        Native.ResizeBuffers((uint)bufferCount, (uint)width, (uint)height, format.ToWin32(), flags.ToWin32());
    }

    public unsafe T GetBackBuffer<T>(int index)
        where T : ComObject
    {
        var iid = ComObject.GetNativeInterfaceType(typeof(T)).GUID;
        Native.GetBuffer((uint)index, &iid, out var buffer);
        return ComObjectFactory.Create<T>(buffer);
    }

    private static unsafe W32DXGI.IDXGISwapChain1 Create(Factory2 factory, Device1 device, IntPtr hwnd, SwapChainDescription1 description)
    {
        var nativeDescription = description.ToWin32();
        factory.Native.CreateSwapChainForHwnd(
            device.NativeComObject,
            new Windows.Win32.Foundation.HWND(hwnd),
            &nativeDescription,
            null,
            null!,
            out var swapChain);
        return swapChain;
    }
}

internal static class DxgiConversions
{
    public static W32DXGIC.DXGI_FORMAT ToWin32(this Format format) =>
        (W32DXGIC.DXGI_FORMAT)(uint)format;

    public static W32DXGI.DXGI_SWAP_CHAIN_FLAG ToWin32(this SwapChainFlags flags) =>
        (W32DXGI.DXGI_SWAP_CHAIN_FLAG)(uint)flags;

    public static W32DXGI.DXGI_PRESENT ToWin32(this PresentFlags flags) =>
        (W32DXGI.DXGI_PRESENT)(uint)flags;

    public static W32DXGI.DXGI_SCALING ToWin32(this Scaling scaling) =>
        (W32DXGI.DXGI_SCALING)(int)scaling;

    public static W32DXGI.DXGI_SWAP_EFFECT ToWin32(this SwapEffect effect) =>
        (W32DXGI.DXGI_SWAP_EFFECT)(int)effect;

    public static W32DXGIC.DXGI_ALPHA_MODE ToWin32(this AlphaMode alphaMode) =>
        (W32DXGIC.DXGI_ALPHA_MODE)(int)alphaMode;

    public static W32DXGIC.DXGI_SAMPLE_DESC ToWin32(this SampleDescription sampleDescription) =>
        new()
        {
            Count = (uint)sampleDescription.Count,
            Quality = (uint)sampleDescription.Quality
        };

    public static W32DXGI.DXGI_SWAP_CHAIN_DESC1 ToWin32(this SwapChainDescription1 description) =>
        new()
        {
            Width = (uint)description.Width,
            Height = (uint)description.Height,
            Format = description.Format.ToWin32(),
            Stereo = description.Stereo,
            SampleDesc = description.SampleDescription.ToWin32(),
            BufferUsage = (W32DXGI.DXGI_USAGE)(uint)description.Usage,
            BufferCount = (uint)description.BufferCount,
            Scaling = description.Scaling.ToWin32(),
            SwapEffect = description.SwapEffect.ToWin32(),
            AlphaMode = description.AlphaMode.ToWin32(),
            Flags = description.Flags.ToWin32()
        };
}
