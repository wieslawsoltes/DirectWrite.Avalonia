using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using global::Avalonia.Win32.DirectX;

namespace Win32.Avalonia;

internal sealed class WinUiCompositedWindowSurface : IDirect3D11TexturePlatformSurface, IDisposable, ICompositionEffectsSurface
{
    private readonly WinUiCompositionShared _shared;
    private readonly EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo _info;
    private BlurEffect _blurEffect;
    private WinUiCompositedWindow? _window;

    public WinUiCompositedWindowSurface(WinUiCompositionShared shared, EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info)
    {
        _shared = shared;
        _info = info;
    }

    public IDirect3D11TextureRenderTarget CreateRenderTarget(IPlatformGraphicsContext context, nint d3dDevice)
    {
        var cornerRadius = AvaloniaLocator.Current.GetService<Win32PlatformOptions>()?.WinUICompositionBackdropCornerRadius;
        _window ??= new WinUiCompositedWindow(_info, _shared, cornerRadius);
        _window.SetBlur(_blurEffect);
        return new WinUiCompositedWindowRenderTarget(context, _window, d3dDevice, _shared);
    }

    public void Dispose()
    {
        _window?.Dispose();
        _window = null;
    }

    public bool IsBlurSupported(BlurEffect effect) => effect switch
    {
        BlurEffect.None => true,
        BlurEffect.Acrylic => Win32Platform.WindowsVersion >= WinUiCompositionShared.MinAcrylicVersion,
        BlurEffect.MicaLight => Win32Platform.WindowsVersion >= WinUiCompositionShared.MinHostBackdropVersion,
        BlurEffect.MicaDark => Win32Platform.WindowsVersion >= WinUiCompositionShared.MinHostBackdropVersion,
        _ => false,
    };

    public void SetBlur(BlurEffect effect)
    {
        _blurEffect = effect;
        _window?.SetBlur(effect);
    }
}

internal sealed class WinUiCompositedWindowRenderTarget : IDirect3D11TextureRenderTarget
{
    private readonly IPlatformGraphicsContext _context;
    private readonly WinUiCompositedWindow _window;
    private readonly nint _graphicsDevicePointer;
    private readonly nint _graphicsDevice2Pointer;
    private readonly nint _drawingSurfacePointer;
    private readonly nint _surfacePointer;
    private readonly nint _surfaceInteropPointer;
    private readonly ICompositionGraphicsDevice2Com _graphicsDevice2;
    private readonly ICompositionSurfaceCom _surface;
    private readonly ICompositionDrawingSurfaceInteropCom _surfaceInterop;
    private PixelSize _size;
    private bool _lost;

    public WinUiCompositedWindowRenderTarget(IPlatformGraphicsContext context, WinUiCompositedWindow window, nint d3dDevice, WinUiCompositionShared shared)
    {
        _context = context;
        _window = window;

        var compositorInterop = GeneratedComHelpers.QueryInterface<ICompositorInteropCom>(shared.CompositorPointer, out var compositorInteropPointer);
        try
        {
            var hr = compositorInterop.CreateGraphicsDevice(d3dDevice, out _graphicsDevicePointer);
            if (hr < 0 || _graphicsDevicePointer == 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            _graphicsDevice2 = GeneratedComHelpers.QueryInterface<ICompositionGraphicsDevice2Com>(_graphicsDevicePointer, out _graphicsDevice2Pointer);

            hr = _graphicsDevice2.CreateDrawingSurface2(default, WinRTDirectXPixelFormat.B8G8R8A8UIntNormalized, WinRTDirectXAlphaMode.Premultiplied, out _drawingSurfacePointer);
            if (hr < 0 || _drawingSurfacePointer == 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            _surface = GeneratedComHelpers.QueryInterface<ICompositionSurfaceCom>(_drawingSurfacePointer, out _surfacePointer);
            _surfaceInterop = GeneratedComHelpers.QueryInterface<ICompositionDrawingSurfaceInteropCom>(_drawingSurfacePointer, out _surfaceInteropPointer);
        }
        finally
        {
            GeneratedComHelpers.Free<ICompositorInteropCom>(compositorInteropPointer);
        }
    }

    public PlatformRenderTargetState State => _context.IsLost || _lost ? PlatformRenderTargetState.Corrupted : PlatformRenderTargetState.Ready;

    public void Dispose()
    {
        GeneratedComHelpers.Free<ICompositionDrawingSurfaceInteropCom>(_surfaceInteropPointer);
        GeneratedComHelpers.Free<ICompositionSurfaceCom>(_surfacePointer);
        GeneratedComHelpers.Free<ICompositionDrawingSurfaceCom>(_drawingSurfacePointer);
        GeneratedComHelpers.Free<ICompositionGraphicsDevice2Com>(_graphicsDevice2Pointer);
        GeneratedComHelpers.Free<IInspectableCom>(_graphicsDevicePointer);
    }

    public IDirect3D11TextureRenderTargetRenderSession BeginDraw()
    {
        if (State.IsCorrupted)
        {
            throw new RenderTargetCorruptedException();
        }

        var transaction = _window.BeginTransaction();
        var needsEndDraw = false;

        try
        {
            var size = _window.WindowInfo.Size;
            var scaling = _window.WindowInfo.Scaling;
            _window.ResizeIfNeeded(size);
            _window.SetSurface(_surfacePointer);

            try
            {
                if (_size != size)
                {
                    var resizeHr = _surfaceInterop.Resize(new WinRTPoint { X = size.Width, Y = size.Height });
                    if (resizeHr < 0)
                    {
                        Marshal.ThrowExceptionForHR(resizeHr);
                    }

                    _size = size;
                }

                var hr = _surfaceInterop.BeginDraw(0, in DCompositionNative.IIdD3D11Texture2D, out var texturePointer, out var offset);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                needsEndDraw = true;
                var session = new Session(_surfaceInterop, texturePointer, transaction, _size, new PixelPoint(offset.X, offset.Y), scaling);
                transaction = null!;
                return session;
            }
            catch (Exception ex)
            {
                _lost = true;
                throw new RenderTargetCorruptedException(ex);
            }
        }
        finally
        {
            if (transaction is not null)
            {
                if (needsEndDraw)
                {
                    _surfaceInterop.EndDraw();
                }

                transaction.Dispose();
            }
        }
    }

    private sealed class Session : IDirect3D11TextureRenderTargetRenderSession
    {
        private readonly ICompositionDrawingSurfaceInteropCom _surfaceInterop;
        private readonly nint _texturePointer;
        private readonly IDisposable _transaction;

        public Session(ICompositionDrawingSurfaceInteropCom surfaceInterop, nint texturePointer, IDisposable transaction, PixelSize size, PixelPoint offset, double scaling)
        {
            _surfaceInterop = surfaceInterop;
            _texturePointer = texturePointer;
            _transaction = transaction;
            Size = size;
            Offset = offset;
            Scaling = scaling;
        }

        public nint D3D11Texture2D => _texturePointer;

        public PixelSize Size { get; }

        public PixelPoint Offset { get; }

        public double Scaling { get; }

        public void Dispose()
        {
            try
            {
                if (_texturePointer != 0)
                {
                    Marshal.Release(_texturePointer);
                }

                _surfaceInterop.EndDraw();
            }
            finally
            {
                _transaction.Dispose();
            }
        }
    }
}