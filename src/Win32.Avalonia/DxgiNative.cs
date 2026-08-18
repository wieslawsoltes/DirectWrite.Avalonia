using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static partial class DxgiNative
{
    public static readonly Guid IdxgiFactory = new("7B7166EC-21C7-44AE-B21A-C9AE321AE369");
    public static readonly Guid IdxgiFactory1 = new("770AAE78-F26F-4DBA-A829-253C83D1B387");
    public static readonly Guid IdxgiFactory2 = new("50C83A1C-E072-4C48-87B0-3630FA36A6D0");
    public static readonly Guid IdxgiAdapter = new("2411E7E1-12AC-4CCF-BD14-9798E8534DC0");
    public static readonly Guid IdxgiAdapter1 = new("29038F61-3839-4626-91FD-086879011A05");
    public static readonly Guid IdxgiDevice = new("54EC77FA-1377-44E6-8C32-88FD5F44C84C");
    public static readonly Guid IdxgiOutput = new("AE02EEDB-C735-4690-8D52-5A8DC20213AA");
    public static readonly Guid IdxgiResource = new("035F3AB4-482E-4E50-B41F-8A7F8BD8960B");
    public static readonly Guid IdxgiKeyedMutex = new("9D8E1289-D7B3-465F-8126-250E349AF85D");
    public static readonly Guid IdxgiSwapChain1 = new("790A45F7-0D42-4876-983A-0A55CFE6F4AA");

    [LibraryImport("dxgi.dll", EntryPoint = "CreateDXGIFactory")]
    public static partial int CreateDxgiFactory(in Guid interfaceId, out nint factory);

    [LibraryImport("dxgi.dll", EntryPoint = "CreateDXGIFactory1")]
    public static partial int CreateDxgiFactory1(in Guid interfaceId, out nint factory);

    [LibraryImport("dwmapi.dll", EntryPoint = "DwmFlush")]
    public static partial int DwmFlush();
}