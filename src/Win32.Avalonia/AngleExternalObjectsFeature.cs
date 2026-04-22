using System.Runtime.InteropServices;
using global::Avalonia;
using global::Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

namespace Win32.Avalonia;

internal sealed class AngleExternalObjectsFeature : IGlContextExternalObjectsFeature, IDisposable
{
    private static readonly IReadOnlyList<PlatformGraphicsExternalImageFormat> SupportedFormats =
        [PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm];

    private readonly EglContext _context;
    private readonly nint _devicePointer;
    private readonly ID3D11DeviceCom _device;
    private readonly nint _device1Pointer;
    private readonly ID3D11Device1Com? _device1;

    public AngleExternalObjectsFeature(EglContext context)
    {
        _context = context;

        var angleDisplay = (AngleWin32EglDisplay)context.Display;
        _devicePointer = QueryInterface(angleDisplay.GetDirect3DDevice(), D3D11Native.Id3D11Device);
        _device = GeneratedComHelpers.ConvertToManaged<ID3D11DeviceCom>(_devicePointer)
            ?? throw new InvalidOperationException("Unable to query ID3D11Device from the ANGLE display.");

        _device1Pointer = TryQueryInterface(_devicePointer, D3D11Native.Id3D11Device1);
        if (_device1Pointer != nint.Zero)
        {
            _device1 = GeneratedComHelpers.ConvertToManaged<ID3D11Device1Com>(_device1Pointer)
                ?? throw new InvalidOperationException("Unable to query ID3D11Device1 from the ANGLE display.");
        }

        DeviceLuid = angleDisplay.DeviceLuid;

        SupportedImportableExternalImageTypes = _device1 is null
            ? [KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureGlobalSharedHandle]
            :
            [
                KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureGlobalSharedHandle,
                KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle,
            ];
        SupportedExportableExternalImageTypes = SupportedImportableExternalImageTypes;
    }

    public IReadOnlyList<string> SupportedImportableExternalImageTypes { get; }

    public IReadOnlyList<string> SupportedExportableExternalImageTypes { get; }

    public IReadOnlyList<string> SupportedImportableExternalSemaphoreTypes { get; } = Array.Empty<string>();

    public IReadOnlyList<string> SupportedExportableExternalSemaphoreTypes { get; } = Array.Empty<string>();

    public byte[]? DeviceLuid { get; }

    public byte[]? DeviceUuid => null;

    public IReadOnlyList<PlatformGraphicsExternalImageFormat> GetSupportedFormatsForExternalMemoryType(string type)
        => SupportedImportableExternalImageTypes.Contains(type) ? SupportedFormats : Array.Empty<PlatformGraphicsExternalImageFormat>();

    public IGlExportableExternalImageTexture CreateImage(string type, PixelSize size, PlatformGraphicsExternalImageFormat format)
    {
        if (!SupportedExportableExternalImageTypes.Contains(type))
        {
            throw new NotSupportedException($"Unsupported external memory type: {type}");
        }

        if (format != PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm)
        {
            throw new NotSupportedException("Unsupported external memory format.");
        }

        using (_context.EnsureCurrent())
        {
            var desc = new D3D11Texture2DDesc
            {
                Width = (uint)size.Width,
                Height = (uint)size.Height,
                ArraySize = 1,
                MipLevels = 1,
                Format = DxgiFormat.R8G8B8A8Unorm,
                SampleDesc = new DxgiSampleDesc { Count = 1, Quality = 0 },
                Usage = D3D11Usage.Default,
                CPUAccessFlags = 0,
                MiscFlags = D3D11ResourceMiscFlags.SharedKeyedMutex,
                BindFlags = D3D11BindFlags.RenderTarget | D3D11BindFlags.ShaderResource,
            };

            Marshal.ThrowExceptionForHR(_device.CreateTexture2D(in desc, nint.Zero, out var texturePointer));
            return new AngleExternalMemoryD3D11ExportedTexture2D(_context, texturePointer, desc, format);
        }
    }

    public IGlExportableExternalImageTexture CreateSemaphore(string type)
        => throw new NotSupportedException();

    public IGlExternalImageTexture ImportImage(IPlatformHandle handle, PlatformGraphicsExternalImageProperties properties)
    {
        if (!SupportedImportableExternalImageTypes.Contains(handle.HandleDescriptor))
        {
            throw new NotSupportedException($"Unsupported external memory type: {handle.HandleDescriptor}");
        }

        using (_context.EnsureCurrent())
        {
            nint texturePointer;
            if (handle.HandleDescriptor == KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureGlobalSharedHandle)
            {
                Marshal.ThrowExceptionForHR(_device.OpenSharedResource(handle.Handle, D3D11Native.Id3D11Texture2D, out texturePointer));
            }
            else if (_device1 is not null)
            {
                Marshal.ThrowExceptionForHR(_device1.OpenSharedResource1(handle.Handle, D3D11Native.Id3D11Texture2D, out texturePointer));
            }
            else
            {
                throw new NotSupportedException("D3D11 NT shared handles require ID3D11Device1 support.");
            }

            return new AngleExternalMemoryD3D11Texture2D(_context, texturePointer, properties);
        }
    }

    public IGlExternalSemaphore ImportSemaphore(IPlatformHandle handle)
        => throw new NotSupportedException();

    public CompositionGpuImportedImageSynchronizationCapabilities GetSynchronizationCapabilities(string imageHandleType)
    {
        if (imageHandleType == KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureGlobalSharedHandle
            || imageHandleType == KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle)
        {
            return CompositionGpuImportedImageSynchronizationCapabilities.KeyedMutex;
        }

        return default;
    }

    public void Dispose()
    {
        if (_device1Pointer != nint.Zero)
        {
            Marshal.Release(_device1Pointer);
        }

        if (_devicePointer != nint.Zero)
        {
            Marshal.Release(_devicePointer);
        }
    }

    private static nint QueryInterface(nint instance, Guid iid)
    {
        Marshal.ThrowExceptionForHR(Marshal.QueryInterface(instance, in iid, out var result));
        return result;
    }

    private static nint TryQueryInterface(nint instance, Guid iid)
    {
        var hr = Marshal.QueryInterface(instance, in iid, out var result);
        return hr >= 0 ? result : nint.Zero;
    }
}