using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Win32.Avalonia;

[GeneratedComInterface]
[Guid("DB6F6DDB-AC77-4E88-8253-819DF9BBF140")]
internal partial interface ID3D11DeviceCom
{
    [PreserveSig]
    int CreateTexture2D(in D3D11Texture2DDesc desc, nint initialData, out nint texture2D);

    [PreserveSig]
    int OpenSharedResource(nint handle, in Guid returnedInterface, out nint resource);

    [PreserveSig]
    int GetDeviceRemovedReason();
}

[GeneratedComInterface]
[Guid("A04BFB29-08EF-43D6-A49C-A9BDBDCBE686")]
internal partial interface ID3D11Device1Com
{
    [PreserveSig]
    int OpenSharedResource1(nint handle, in Guid returnedInterface, out nint resource);
}

internal enum D3D11Usage : uint
{
    Default = 0,
}

[Flags]
internal enum D3D11BindFlags : uint
{
    ShaderResource = 0x8,
    RenderTarget = 0x20,
}

[Flags]
internal enum D3D11ResourceMiscFlags : uint
{
    SharedKeyedMutex = 0x100,
}

[StructLayout(LayoutKind.Sequential)]
internal struct D3D11Texture2DDesc
{
    public uint Width;
    public uint Height;
    public uint MipLevels;
    public uint ArraySize;
    public DxgiFormat Format;
    public DxgiSampleDesc SampleDesc;
    public D3D11Usage Usage;
    public D3D11BindFlags BindFlags;
    public uint CPUAccessFlags;
    public D3D11ResourceMiscFlags MiscFlags;
}