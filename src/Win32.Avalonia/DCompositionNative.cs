using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static class DCompositionNative
{
    public static readonly Guid IIdCompositionDesktopDevice = new("5F4633FE-1E08-4CB8-8C75-CE24333F5602");
    public static readonly Guid IIdD3D11Texture2D = new("6F15AAF2-D208-4E89-9AB4-489535D34F9C");
    public static readonly Version MinDirectCompositionVersion = new(6, 3);

    [DllImport("dcomp.dll", ExactSpelling = true)]
    public static extern int DCompositionCreateDevice2(nint renderingDevice, in Guid interfaceId, out nint dcompositionDevice);
}