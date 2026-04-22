using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static class DwmNative
{
    public const int UseHostBackdropBrush = 17;
    public const int UseImmersiveDarkMode = 20;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int valueSize);

    public static bool TrySetWindowAttribute(nint hwnd, int attribute, int value)
        => DwmSetWindowAttribute(hwnd, attribute, ref value, sizeof(int)) == 0;
}