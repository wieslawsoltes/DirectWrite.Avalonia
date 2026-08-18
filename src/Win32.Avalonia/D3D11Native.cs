using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static class D3D11Native
{
    public static readonly Guid Id3D11Device = new("DB6F6DDB-AC77-4E88-8253-819DF9BBF140");
    public static readonly Guid Id3D11Device1 = new("A04BFB29-08EF-43D6-A49C-A9BDBDCBE686");
    public static readonly Guid Id3D11Texture2D = new("6F15AAF2-D208-4E89-9AB4-489535D34F9C");

    public const int SdkVersion = 7;
    public const int DriverTypeUnknown = 0;
    public const int EglPlatformAngleAngle = 0x3202;
    public const int EglPlatformAngleTypeAngle = 0x3203;
    public const int EglPlatformAngleTypeD3D11Angle = 0x3208;
    public const int EglPlatformDeviceExt = 0x313F;
    public const int EglD3D11DeviceAngle = 0x33A1;
    public const int EglD3DTextureAngle = 0x33A3;
    public const int EglTextureOffsetXAngle = 0x3490;
    public const int EglTextureOffsetYAngle = 0x3491;
    public const int EglFlexibleSurfaceCompatibilitySupportedAngle = 0x33A6;

    [DllImport("d3d11.dll", EntryPoint = "D3D11CreateDevice")]
    public static extern int D3D11CreateDevice(
        nint adapter,
        int driverType,
        nint software,
        uint flags,
        int[] featureLevels,
        uint featureLevelsCount,
        uint sdkVersion,
        out nint device,
        out int featureLevel,
        out nint immediateContext);
}