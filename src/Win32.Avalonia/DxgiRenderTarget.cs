using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;

namespace Win32.Avalonia;

internal sealed class DxgiRenderTarget : EglPlatformSurfaceRenderTargetBase
{
    private const uint DxgiUsageRenderTargetOutput = 0x00000020;
    private const uint DxgiSwapChainFlagAllowTearing = 0x00000800;

    private static readonly Guid Id3D11Texture2DGuid = new("6F15AAF2-D208-4E89-9AB4-489535D34F9C");

    private readonly EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo _window;
    private readonly DxgiConnection _connection;
    private readonly IDxgiDeviceCom _dxgiDevice;
    private readonly IDxgiFactory2Com _dxgiFactory;
    private readonly IDxgiSwapChain1Com _swapChain;
    private readonly nint _dxgiDevicePointer;
    private readonly nint _dxgiFactoryPointer;
    private readonly nint _swapChainPointer;
    private readonly uint _swapChainFlags;
    private readonly MethodInfo _getDirect3DDevice;
    private readonly MethodInfo _wrapDirect3D11Texture;

    private nint _renderTexture;
    private EglSurface? _surface;
    private PixelSize _currentSize;

    public DxgiRenderTarget(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo window, EglContext context, DxgiConnection connection)
        : base(context)
    {
        _window = window;
        _connection = connection;
        _currentSize = Normalize(window.Size);

        var displayType = context.Display.GetType();
        _getDirect3DDevice = displayType.GetMethod("GetDirect3DDevice", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"{displayType.FullName} does not expose GetDirect3DDevice().");
        _wrapDirect3D11Texture = displayType.GetMethod(
                "WrapDirect3D11Texture",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                types: [typeof(IntPtr), typeof(int), typeof(int), typeof(int), typeof(int)],
                modifiers: null)
            ?? throw new InvalidOperationException($"{displayType.FullName} does not expose WrapDirect3D11Texture(IntPtr,int,int,int,int).");

        var d3dDevice = (IntPtr?)_getDirect3DDevice.Invoke(context.Display, null)
            ?? throw new InvalidOperationException("ANGLE display did not return a D3D device.");

        _dxgiDevicePointer = QueryInterface(d3dDevice, DxgiNative.IdxgiDevice);
        _dxgiDevice = GeneratedComHelpers.ConvertToManaged<IDxgiDeviceCom>(_dxgiDevicePointer)
            ?? throw new InvalidOperationException("Unable to query IDXGIDevice from ANGLE D3D device.");

        if (_dxgiDevice.GetAdapter(out var adapterPointer) < 0 || adapterPointer == nint.Zero)
        {
            throw new InvalidOperationException("Unable to retrieve DXGI adapter.");
        }

        try
        {
            var adapter = GeneratedComHelpers.ConvertToManaged<IDxgiAdapterCom>(adapterPointer)
                ?? throw new InvalidOperationException("Unable to retrieve managed DXGI adapter.");

            if (adapter.GetParent(DxgiNative.IdxgiFactory2, out var factoryPointer) < 0 || factoryPointer == nint.Zero)
            {
                throw new InvalidOperationException("Unable to retrieve DXGI factory.");
            }

            _dxgiFactoryPointer = factoryPointer;
            _dxgiFactory = GeneratedComHelpers.ConvertToManaged<IDxgiFactory2Com>(_dxgiFactoryPointer)
                ?? throw new InvalidOperationException("Unable to retrieve managed DXGI factory.");
            _swapChainFlags = DxgiSwapChainFlagAllowTearing;

            var size = Normalize(window.Size);
            var swapChainDesc = new DxgiSwapChainDesc1
            {
                Width = (uint)size.Width,
                Height = (uint)size.Height,
                Format = DxgiFormat.B8G8R8A8Unorm,
                Stereo = false,
                SampleDesc = new DxgiSampleDesc { Count = 1, Quality = 0 },
                BufferUsage = DxgiUsageRenderTargetOutput,
                BufferCount = 2,
                Scaling = DxgiScaling.Stretch,
                SwapEffect = DxgiSwapEffect.FlipDiscard,
                AlphaMode = DxgiAlphaMode.Ignore,
                Flags = _swapChainFlags,
            };

            if (_dxgiFactory.CreateSwapChainForHwnd(_dxgiDevicePointer, window.Handle, in swapChainDesc, nint.Zero, nint.Zero, out var swapChainPointer) < 0
                || swapChainPointer == nint.Zero)
            {
                throw new InvalidOperationException("Unable to create DXGI swap chain.");
            }

            _swapChainPointer = swapChainPointer;
            _swapChain = GeneratedComHelpers.ConvertToManaged<IDxgiSwapChain1Com>(_swapChainPointer)
                ?? throw new InvalidOperationException("Unable to retrieve managed DXGI swap chain.");
            _ = _dxgiFactory.MakeWindowAssociation(window.Handle, (uint)(DxgiWindowAssociationFlags.NoAltEnter | DxgiWindowAssociationFlags.NoPrintScreen));
        }
        finally
        {
            Marshal.Release(adapterPointer);
        }
    }

    public override IGlPlatformSurfaceRenderingSession BeginDrawCore(IRenderTarget.RenderTargetSceneInfo sceneInfo)
    {
        var success = false;
        var contextLock = Context.EnsureCurrent();

        try
        {
            var size = Normalize(_window.Size);
            if (size != _currentSize)
            {
                ReleaseSurface();
                Marshal.ThrowExceptionForHR(_swapChain.ResizeBuffers(2, (uint)size.Width, (uint)size.Height, DxgiFormat.B8G8R8A8Unorm, _swapChainFlags));
                _currentSize = size;
            }

            if (_renderTexture == nint.Zero)
            {
                Marshal.ThrowExceptionForHR(_swapChain.GetBuffer(0, Id3D11Texture2DGuid, out _renderTexture));
            }

            _surface ??= (EglSurface?)_wrapDirect3D11Texture.Invoke(Context.Display, [_renderTexture, 0, 0, size.Width, size.Height])
                ?? throw new InvalidOperationException("Unable to wrap DXGI backbuffer as EGL surface.");

            var session = base.BeginDraw(_surface, size, _window.Scaling, onFinish: OnFinish, isYFlipped: true);
            success = true;
            return session;
        }
        finally
        {
            if (!success)
            {
                contextLock.Dispose();
            }
        }

        void OnFinish()
        {
            _ = _connection;
            Marshal.ThrowExceptionForHR(_swapChain.Present(0, 0));
            contextLock.Dispose();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        ReleaseSurface();
        Release(_swapChainPointer);
        Release(_dxgiFactoryPointer);
        Release(_dxgiDevicePointer);
    }

    private void ReleaseSurface()
    {
        _surface?.Dispose();
        _surface = null;

        if (_renderTexture != nint.Zero)
        {
            Marshal.Release(_renderTexture);
            _renderTexture = nint.Zero;
        }
    }

    private static PixelSize Normalize(PixelSize size)
        => new(Math.Max(1, size.Width), Math.Max(1, size.Height));

    private static nint QueryInterface(nint instance, Guid guid)
    {
        Marshal.ThrowExceptionForHR(Marshal.QueryInterface(instance, in guid, out var result));
        return result;
    }

    private static void Release(nint instance)
    {
        if (instance != nint.Zero)
        {
            Marshal.Release(instance);
        }
    }
}