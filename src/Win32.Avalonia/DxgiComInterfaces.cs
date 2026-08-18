using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;

namespace Win32.Avalonia;

[GeneratedComInterface]
[Guid("7B7166EC-21C7-44AE-B21A-C9AE321AE369")]
internal partial interface IDxgiFactoryCom
{
    [PreserveSig]
    int EnumAdapters(uint adapterIndex, out nint adapter);
}

[GeneratedComInterface]
[Guid("770AAE78-F26F-4DBA-A829-253C83D1B387")]
internal partial interface IDxgiFactory1Com
{
    [PreserveSig]
    int EnumAdapters1(uint adapterIndex, out nint adapter);
}

[GeneratedComInterface]
[Guid("50C83A1C-E072-4C48-87B0-3630FA36A6D0")]
internal partial interface IDxgiFactory2Com
{
    [PreserveSig]
    int CreateSwapChainForHwnd(nint device, nint hwnd, in DxgiSwapChainDesc1 desc, nint fullscreenDesc, nint restrictToOutput, out nint swapChain);

    [PreserveSig]
    int MakeWindowAssociation(nint windowHandle, uint flags);
}

[GeneratedComInterface]
[Guid("54EC77FA-1377-44E6-8C32-88FD5F44C84C")]
internal partial interface IDxgiDeviceCom
{
    [PreserveSig]
    int GetAdapter(out nint adapter);
}

[GeneratedComInterface]
[Guid("2411E7E1-12AC-4CCF-BD14-9798E8534DC0")]
internal partial interface IDxgiAdapterCom
{
    [PreserveSig]
    int EnumOutputs(uint outputIndex, out nint output);

    [PreserveSig]
    int GetParent(in Guid interfaceId, out nint parent);
}

[GeneratedComInterface]
[Guid("29038F61-3839-4626-91FD-086879011A05")]
internal partial interface IDxgiAdapter1Com
{
    [PreserveSig]
    int GetDesc1(out DxgiAdapterDesc1 desc);
}

[GeneratedComInterface]
[Guid("AE02EEDB-C735-4690-8D52-5A8DC20213AA")]
internal partial interface IDxgiOutputCom
{
    void GetDesc(out DxgiOutputDesc desc);
    void WaitForVBlank();
}

[GeneratedComInterface]
[Guid("035F3AB4-482E-4E50-B41F-8A7F8BD8960B")]
internal partial interface IDxgiResourceCom
{
    [PreserveSig]
    int GetSharedHandle(out nint sharedHandle);
}

[GeneratedComInterface]
[Guid("9D8E1289-D7B3-465F-8126-250E349AF85D")]
internal partial interface IDxgiKeyedMutexCom
{
    [PreserveSig]
    int AcquireSync(uint key, uint milliseconds);

    [PreserveSig]
    int ReleaseSync(uint key);
}

internal enum DxgiModeRotation
{
    Unspecified,
    Identity,
    Rotate90,
    Rotate180,
    Rotate270,
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal unsafe struct DxgiOutputDesc
{
    public fixed char DeviceName[32];
    public RECT DesktopCoordinates;
    [MarshalAs(UnmanagedType.Bool)]
    public bool AttachedToDesktop;
    public DxgiModeRotation Rotation;
    public nint Monitor;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal unsafe struct DxgiAdapterDesc1
{
    public fixed char Description[128];
    public uint VendorId;
    public uint DeviceId;
    public uint SubSysId;
    public uint Revision;
    public nuint DedicatedVideoMemory;
    public nuint DedicatedSystemMemory;
    public nuint SharedSystemMemory;
    public long AdapterLuid;
    public uint Flags;

    public override string ToString()
    {
        fixed (char* description = Description)
        {
            return new string(description);
        }
    }
}

[GeneratedComInterface]
[Guid("790A45F7-0D42-4876-983A-0A55CFE6F4AA")]
internal partial interface IDxgiSwapChain1Com
{
    [PreserveSig]
    int Present(uint syncInterval, uint flags);

    [PreserveSig]
    int GetBuffer(uint buffer, in Guid interfaceId, out nint surface);

    [PreserveSig]
    int ResizeBuffers(uint bufferCount, uint width, uint height, DxgiFormat newFormat, uint flags);
}

internal enum DxgiFormat
{
    Unknown = 0,
    R8G8B8A8Unorm = 28,
    B8G8R8A8Unorm = 87,
}

internal enum DxgiScaling
{
    Stretch = 0,
    None = 1,
    AspectRatioStretch = 2,
}

internal enum DxgiSwapEffect
{
    Discard = 0,
    Sequential = 1,
    FlipSequential = 3,
    FlipDiscard = 4,
}

internal enum DxgiAlphaMode
{
    Unspecified = 0,
    Premultiplied = 1,
    Straight = 2,
    Ignore = 3,
}

[Flags]
internal enum DxgiWindowAssociationFlags : uint
{
    None = 0,
    NoWindowChanges = 1,
    NoAltEnter = 2,
    NoPrintScreen = 4,
}

[StructLayout(LayoutKind.Sequential)]
internal struct DxgiSampleDesc
{
    public uint Count;
    public uint Quality;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DxgiSwapChainDesc1
{
    public uint Width;
    public uint Height;
    public DxgiFormat Format;
    [MarshalAs(UnmanagedType.Bool)]
    public bool Stereo;
    public DxgiSampleDesc SampleDesc;
    public uint BufferUsage;
    public uint BufferCount;
    public DxgiScaling Scaling;
    public DxgiSwapEffect SwapEffect;
    public DxgiAlphaMode AlphaMode;
    public uint Flags;
}