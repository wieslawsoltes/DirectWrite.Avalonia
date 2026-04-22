using System.Runtime.InteropServices;
using global::Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using static Avalonia.OpenGL.Egl.EglConsts;
using static Avalonia.OpenGL.GlConsts;

namespace Win32.Avalonia;

internal class AngleExternalMemoryD3D11Texture2D : IGlExternalImageTexture
{
    private readonly EglContext _context;
    private nint _texturePointer;
    private readonly nint _keyedMutexPointer;
    private readonly IDxgiKeyedMutexCom _keyedMutex;
    private EglSurface? _eglSurface;

    public unsafe AngleExternalMemoryD3D11Texture2D(EglContext context, nint texturePointer, PlatformGraphicsExternalImageProperties properties)
    {
        _context = context;
        _texturePointer = texturePointer;
        _keyedMutexPointer = QueryInterface(texturePointer, DxgiNative.IdxgiKeyedMutex);
        _keyedMutex = GeneratedComHelpers.ConvertToManaged<IDxgiKeyedMutexCom>(_keyedMutexPointer)
            ?? throw new InvalidOperationException("Unable to query IDXGIKeyedMutex from the D3D11 texture.");
        Properties = properties;

        InternalFormat = GL_RGBA8;

        var attrs = stackalloc[]
        {
            EGL_WIDTH, properties.Width,
            EGL_HEIGHT, properties.Height,
            EGL_TEXTURE_FORMAT, EGL_TEXTURE_RGBA,
            EGL_TEXTURE_TARGET, EGL_TEXTURE_2D,
            EGL_TEXTURE_INTERNAL_FORMAT_ANGLE, GL_RGBA,
            EGL_NONE, EGL_NONE,
            EGL_NONE,
        };

        _eglSurface = _context.Display.CreatePBufferFromClientBuffer(D3D11Native.EglD3DTextureAngle, texturePointer, attrs);

        var gl = _context.GlInterface;
        int textureId = 0;
        gl.GenTextures(1, &textureId);
        TextureId = textureId;
        gl.BindTexture(GL_TEXTURE_2D, TextureId);

        if (_context.Display.EglInterface.BindTexImage(_context.Display.Handle, _eglSurface.DangerousGetHandle(), EGL_BACK_BUFFER) == 0)
        {
            throw OpenGlException.GetFormattedException("eglBindTexImage", _context.Display.EglInterface);
        }
    }

    public void Dispose()
    {
        if (!_context.IsLost && TextureId != 0)
        {
            using (_context.EnsureCurrent())
            {
                _context.GlInterface.DeleteTexture(TextureId);
            }
        }

        TextureId = 0;

        _eglSurface?.Dispose();
        _eglSurface = null;

        if (_keyedMutexPointer != nint.Zero)
        {
            Marshal.Release(_keyedMutexPointer);
        }

        if (_texturePointer != nint.Zero)
        {
            Marshal.Release(_texturePointer);
            _texturePointer = nint.Zero;
        }
    }

    public void AcquireKeyedMutex(uint key)
        => Marshal.ThrowExceptionForHR(_keyedMutex.AcquireSync(key, int.MaxValue));

    public void ReleaseKeyedMutex(uint key)
        => Marshal.ThrowExceptionForHR(_keyedMutex.ReleaseSync(key));

    public int TextureId { get; private set; }

    public int InternalFormat { get; }

    public int TextureType => GL_TEXTURE_2D;

    public PlatformGraphicsExternalImageProperties Properties { get; }

    private static nint QueryInterface(nint instance, Guid iid)
    {
        Marshal.ThrowExceptionForHR(Marshal.QueryInterface(instance, in iid, out var result));
        return result;
    }
}

internal sealed class AngleExternalMemoryD3D11ExportedTexture2D : AngleExternalMemoryD3D11Texture2D, IGlExportableExternalImageTexture
{
    public AngleExternalMemoryD3D11ExportedTexture2D(
        EglContext context,
        nint texturePointer,
        D3D11Texture2DDesc desc,
        PlatformGraphicsExternalImageFormat format)
        : this(
            context,
            texturePointer,
            GetHandle(texturePointer),
            new PlatformGraphicsExternalImageProperties
            {
                Width = (int)desc.Width,
                Height = (int)desc.Height,
                Format = format,
            })
    {
    }

    private AngleExternalMemoryD3D11ExportedTexture2D(
        EglContext context,
        nint texturePointer,
        IPlatformHandle handle,
        PlatformGraphicsExternalImageProperties properties)
        : base(context, texturePointer, properties)
    {
        Handle = handle;
    }

    public IPlatformHandle Handle { get; }

    public IPlatformHandle GetHandle() => Handle;

    private static IPlatformHandle GetHandle(nint texturePointer)
    {
        var resourcePointer = QueryInterface(texturePointer, DxgiNative.IdxgiResource);

        try
        {
            var resource = GeneratedComHelpers.ConvertToManaged<IDxgiResourceCom>(resourcePointer)
                ?? throw new InvalidOperationException("Unable to query IDXGIResource from the D3D11 texture.");

            if (resource.GetSharedHandle(out var sharedHandle) < 0 || sharedHandle == nint.Zero)
            {
                throw new InvalidOperationException("Unable to retrieve a D3D11 shared handle.");
            }

            return new PlatformHandle(sharedHandle, KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureGlobalSharedHandle);
        }
        finally
        {
            Marshal.Release(resourcePointer);
        }
    }

    private static nint QueryInterface(nint instance, Guid iid)
    {
        Marshal.ThrowExceptionForHR(Marshal.QueryInterface(instance, in iid, out var result));
        return result;
    }
}