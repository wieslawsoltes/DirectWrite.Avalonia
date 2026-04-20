using System;
using Windows.Win32;
using W32D3D = Windows.Win32.Graphics.Direct3D;
using W32D3D11 = Windows.Win32.Graphics.Direct3D11;

namespace Avalonia.Direct2D1.Interop.Direct3D11;

[Flags]
public enum DeviceCreationFlags
{
    None = 0,
    SingleThreaded = 0x1,
    Debug = 0x2,
    SwitchToRef = 0x4,
    PreventInternalThreadingOptimizations = 0x8,
    BgraSupport = 0x20,
    Debuggable = 0x40,
    PreventAlteringLayerSettingsFromRegistry = 0x80,
    DisableGpuTimeout = 0x100,
    VideoSupport = 0x800
}

[NativeInterface(typeof(W32D3D11.ID3D11Device))]
public sealed class Device : CppObject
{
    public Device(Avalonia.Direct2D1.Interop.Direct3D.DriverType driverType, DeviceCreationFlags flags, Avalonia.Direct2D1.Interop.Direct3D.FeatureLevel[] featureLevels)
        : this(Create(driverType, flags, featureLevels))
    {
    }

    internal Device(W32D3D11.ID3D11Device native)
        : base(native)
    {
    }

    internal W32D3D11.ID3D11Device Native => GetNative<W32D3D11.ID3D11Device>();

    private static W32D3D11.ID3D11Device Create(Avalonia.Direct2D1.Interop.Direct3D.DriverType driverType, DeviceCreationFlags flags, Avalonia.Direct2D1.Interop.Direct3D.FeatureLevel[] featureLevels)
    {
        var nativeFeatureLevels = new W32D3D.D3D_FEATURE_LEVEL[featureLevels.Length];

        for (var i = 0; i < featureLevels.Length; i++)
        {
            nativeFeatureLevels[i] = (W32D3D.D3D_FEATURE_LEVEL)(uint)featureLevels[i];
        }

        PInvoke.D3D11CreateDevice(
            pAdapter: null!,
            DriverType: (W32D3D.D3D_DRIVER_TYPE)(int)driverType,
            Software: default,
            Flags: (W32D3D11.D3D11_CREATE_DEVICE_FLAG)(uint)flags,
            pFeatureLevels: nativeFeatureLevels,
            SDKVersion: 7u,
            out var device,
            out _,
            out _).ThrowOnFailure();

        return device;
    }
}
