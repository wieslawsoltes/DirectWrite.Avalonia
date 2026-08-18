using Avalonia;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using global::Avalonia.Win32.DirectX;

namespace Win32.Avalonia;

internal sealed class DirectCompositedWindowSurface : IDirect3D11TexturePlatformSurface, IDisposable
{
    private readonly EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo _info;
    private readonly DirectCompositionShared _shared;
    private DirectCompositedWindow? _window;

    public DirectCompositedWindowSurface(DirectCompositionShared shared, EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info)
    {
        _shared = shared;
        _info = info;
    }

    public IDirect3D11TextureRenderTarget CreateRenderTarget(IPlatformGraphicsContext context, nint d3dDevice)
    {
        _window ??= new DirectCompositedWindow(_info, _shared);
        return new DirectCompositedWindowRenderTarget(context, d3dDevice, _shared, _window);
    }

    public void Dispose()
    {
        _window?.Dispose();
        _window = null;
    }
}

internal sealed class DirectCompositedWindowRenderTarget : IDirect3D11TextureRenderTarget
{
    private readonly IPlatformGraphicsContext _context;
    private readonly DirectCompositedWindow _window;
    private readonly IDCompositionVirtualSurfaceCom _surface;
    private readonly nint _surfacePointer;
    private bool _lost;
    private PixelSize _size;

    public DirectCompositedWindowRenderTarget(
        IPlatformGraphicsContext context,
        nint d3dDevice,
        DirectCompositionShared shared,
        DirectCompositedWindow window)
    {
        _context = context;
        _window = window;

        var hr = shared.Device.CreateSurfaceFactory(d3dDevice, out var surfaceFactoryPointer);
        if (hr < 0 || surfaceFactoryPointer == 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        try
        {
            var surfaceFactory = GeneratedComHelpers.ConvertToManaged<IDCompositionSurfaceFactoryCom>(surfaceFactoryPointer)
                ?? throw new InvalidOperationException("Unable to wrap the DirectComposition surface factory.");

            hr = surfaceFactory.CreateVirtualSurface(1, 1, DxgiFormat.B8G8R8A8Unorm, DxgiAlphaMode.Premultiplied, out _surfacePointer);
            if (hr < 0 || _surfacePointer == 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            _surface = GeneratedComHelpers.ConvertToManaged<IDCompositionVirtualSurfaceCom>(_surfacePointer)
                ?? throw new InvalidOperationException("Unable to wrap the DirectComposition virtual surface.");
        }
        finally
        {
            GeneratedComHelpers.Free<IDCompositionSurfaceFactoryCom>(surfaceFactoryPointer);
        }
    }

    public PlatformRenderTargetState State => _context.IsLost || _lost ? PlatformRenderTargetState.Corrupted : PlatformRenderTargetState.Ready;

    public void Dispose()
        => GeneratedComHelpers.Free<IDCompositionVirtualSurfaceCom>(_surfacePointer);

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

            if (_size != size)
            {
                var resizeHr = _surface.Resize((uint)size.Width, (uint)size.Height);
                if (resizeHr < 0)
                {
                    Marshal.ThrowExceptionForHR(resizeHr);
                }

                _size = size;
            }

            _window.SetSurface(_surfacePointer);

            var rect = new RECT { right = size.Width, bottom = size.Height };
            nint texturePointer;
            DCompositionPoint offset;

            try
            {
                var hr = _surface.BeginDraw(in rect, in DCompositionNative.IIdD3D11Texture2D, out texturePointer, out offset);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
            catch (Exception ex)
            {
                _lost = true;
                throw new RenderTargetCorruptedException(ex);
            }

            needsEndDraw = true;
            var session = new Session(_surface, texturePointer, transaction, size, new PixelPoint(offset.X, offset.Y), scaling);
            transaction = null!;
            return session;
        }
        finally
        {
            if (transaction is not null)
            {
                if (needsEndDraw)
                {
                    _surface.EndDraw();
                }

                transaction.Dispose();
            }
        }
    }

    private sealed class Session : IDirect3D11TextureRenderTargetRenderSession
    {
        private readonly IDCompositionSurfaceCom _surface;
        private readonly nint _texturePointer;
        private readonly IDisposable _transaction;

        public Session(
            IDCompositionSurfaceCom surface,
            nint texturePointer,
            IDisposable transaction,
            PixelSize size,
            PixelPoint offset,
            double scaling)
        {
            _surface = surface;
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

                _surface.EndDraw();
            }
            finally
            {
                _transaction.Dispose();
            }
        }
    }
}