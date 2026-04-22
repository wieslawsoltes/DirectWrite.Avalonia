using System.Runtime.InteropServices;
using System.ComponentModel;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using static Avalonia.OpenGL.Egl.EglConsts;

namespace Win32.Avalonia;

internal sealed class AngleWin32EglDisplay : EglDisplay
{
    private readonly bool _flexibleSurfaceSupported;
    private readonly nint _d3dDevicePointer;

    protected override bool DisplayLockIsSharedWithContexts => true;

    private AngleWin32EglDisplay(nint display, Win32AngleEglInterface egl, EglDisplayOptions options, nint d3dDevicePointer)
        : base(display, options)
    {
        _d3dDevicePointer = d3dDevicePointer;
        var extensions = egl.QueryString(display, EGL_EXTENSIONS);
        _flexibleSurfaceSupported = extensions?.Contains("EGL_ANGLE_flexible_surface_compatibility") ?? false;
    }

    public byte[]? DeviceLuid { get; private init; }

    public static AngleWin32EglDisplay CreateD3D11Display(Win32AngleEglInterface egl)
    {
        var featureLevels = new[]
        {
            0xb100,
            0xb000,
            0xa100,
            0xa000,
            0x9300,
            0x9200,
            0x9100,
        };

        var chosenAdapterPointer = nint.Zero;
        byte[]? chosenAdapterLuid = null;

        var factoryResult = DxgiNative.CreateDxgiFactory1(DxgiNative.IdxgiFactory1, out var factoryPointer);
        if (factoryResult < 0 || factoryPointer == nint.Zero)
        {
            Marshal.ThrowExceptionForHR(factoryResult);
        }

        try
        {
            var factory = GeneratedComHelpers.ConvertToManaged<IDxgiFactory1Com>(factoryPointer)
                ?? throw new InvalidOperationException("Unable to query IDXGIFactory1.");

            var selectionCallback = AvaloniaLocator.Current.GetService<Win32PlatformOptions>()?.GraphicsAdapterSelectionCallback;
            var applyArmAdrenoBlacklist = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

            var adapters = new List<(nint Pointer, PlatformGraphicsDeviceAdapterDescription Description)>();
            for (uint adapterIndex = 0; factory.EnumAdapters1(adapterIndex, out var adapterPointer) == 0; adapterIndex++)
            {
                if (adapterPointer == nint.Zero)
                {
                    continue;
                }

                var adapter = GeneratedComHelpers.ConvertToManaged<IDxgiAdapter1Com>(adapterPointer)
                    ?? throw new InvalidOperationException("Unable to query IDXGIAdapter1.");
                if (adapter.GetDesc1(out var desc) < 0)
                {
                    Marshal.Release(adapterPointer);
                    throw new InvalidOperationException("Unable to query DXGI adapter description.");
                }

                adapters.Add((
                    adapterPointer,
                    new PlatformGraphicsDeviceAdapterDescription
                    {
                        Description = desc.ToString().TrimEnd('\0').ToLowerInvariant(),
                        DeviceLuid = BitConverter.GetBytes(desc.AdapterLuid),
                    }));
            }

            if (adapters.Count == 0)
            {
                throw new OpenGlException("No adapters found.");
            }

            if (applyArmAdrenoBlacklist && adapters.Count > 1)
            {
                for (var index = adapters.Count - 1; index >= 0; index--)
                {
                    if (adapters[index].Description.Description?.Contains("adreno", StringComparison.Ordinal) == true)
                    {
                        Marshal.Release(adapters[index].Pointer);
                        adapters.RemoveAt(index);
                    }
                }
            }

            if (adapters.Count == 0)
            {
                throw new OpenGlException("No compatible adapters found.");
            }

            var chosenAdapterIndex = selectionCallback is null
                ? 0
                : selectionCallback(adapters.Select(static adapter => adapter.Description).ToArray());
            if (chosenAdapterIndex < 0 || chosenAdapterIndex >= adapters.Count)
            {
                throw new InvalidOperationException($"GraphicsAdapterSelectionCallback returned invalid adapter index {chosenAdapterIndex}.");
            }

            chosenAdapterPointer = adapters[chosenAdapterIndex].Pointer;
            chosenAdapterLuid = adapters[chosenAdapterIndex].Description.DeviceLuid;

            for (var index = 0; index < adapters.Count; index++)
            {
                if (index != chosenAdapterIndex)
                {
                    Marshal.Release(adapters[index].Pointer);
                }
            }
        }
        finally
        {
            Marshal.Release(factoryPointer);
        }

        var hr = D3D11Native.D3D11CreateDevice(
            chosenAdapterPointer,
            D3D11Native.DriverTypeUnknown,
            nint.Zero,
            0,
            featureLevels,
            (uint)featureLevels.Length,
            D3D11Native.SdkVersion,
            out var d3dDevice,
            out _,
            out var immediateContext);

        if (chosenAdapterPointer != nint.Zero)
        {
            Marshal.Release(chosenAdapterPointer);
        }

        if (immediateContext != nint.Zero)
        {
            Marshal.Release(immediateContext);
        }

        if (hr < 0)
        {
            throw new Win32Exception(hr, "Unable to create D3D11 device for ANGLE.");
        }

        if (d3dDevice == nint.Zero)
        {
            throw new OpenGlException("Unable to create D3D11 Device");
        }

        var device = GeneratedComHelpers.ConvertToManaged<ID3D11DeviceCom>(d3dDevice)
            ?? throw new InvalidOperationException("Unable to query ID3D11Device for ANGLE.");

        var angleDevice = nint.Zero;
        var display = nint.Zero;

        void Cleanup()
        {
            if (angleDevice != nint.Zero)
            {
                egl.ReleaseDeviceANGLE(angleDevice);
            }

            if (d3dDevice != nint.Zero)
            {
                Marshal.Release(d3dDevice);
            }
        }

        var success = false;
        try
        {
            angleDevice = egl.CreateDeviceANGLE(D3D11Native.EglD3D11DeviceAngle, d3dDevice, null);
            if (angleDevice == nint.Zero)
            {
                throw OpenGlException.GetFormattedException("eglCreateDeviceANGLE", egl);
            }

            display = egl.GetPlatformDisplayExt(D3D11Native.EglPlatformDeviceExt, angleDevice, null);
            if (display == nint.Zero)
            {
                throw OpenGlException.GetFormattedException("eglGetPlatformDisplayEXT", egl);
            }

            var options = new EglDisplayOptions
            {
                DisposeCallback = Cleanup,
                Egl = egl,
                ContextLossIsDisplayLoss = true,
                DeviceLostCheckCallback = () => device.GetDeviceRemovedReason() != 0,
                GlVersions = AvaloniaLocator.Current.GetService<global::Avalonia.Win32.AngleOptions>()?.GlProfiles,
            };

            var result = new AngleWin32EglDisplay(display, egl, options, d3dDevice)
            {
                DeviceLuid = chosenAdapterLuid,
            };
            success = true;
            return result;
        }
        finally
        {
            if (!success)
            {
                if (display != nint.Zero)
                {
                    egl.Terminate(display);
                }

                Cleanup();
            }
        }
    }

    public nint GetDirect3DDevice() => _d3dDevicePointer;

    public unsafe EglSurface WrapDirect3D11Texture(nint handle)
    {
        var attrs = stackalloc[] { EGL_NONE, EGL_NONE };
        return CreatePBufferFromClientBuffer(D3D11Native.EglD3DTextureAngle, handle, attrs);
    }

    public unsafe EglSurface WrapDirect3D11Texture(nint handle, int offsetX, int offsetY, int width, int height)
    {
        var attrs = stackalloc[]
        {
            EGL_WIDTH, width,
            EGL_HEIGHT, height,
            D3D11Native.EglTextureOffsetXAngle, offsetX,
            D3D11Native.EglTextureOffsetYAngle, offsetY,
            _flexibleSurfaceSupported ? D3D11Native.EglFlexibleSurfaceCompatibilitySupportedAngle : EGL_NONE,
            EGL_TRUE,
            EGL_NONE,
        };

        return CreatePBufferFromClientBuffer(D3D11Native.EglD3DTextureAngle, handle, attrs);
    }
}