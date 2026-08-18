using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace Win32.Avalonia;

internal static class WindowNative
{
    public const int GwlStyle = -16;
    public const int GwlExStyle = -20;
    public const int GwlHwndParent = -8;
    public const int GclpHCursor = -12;
    public const int IconSmall = 0;
    public const int IconBig = 1;
    public const int IdcArrow = 32512;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern nint SetWindowLongPtr64(nint hwnd, int index, nint newLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", SetLastError = true)]
    public static extern int GetWindowLong(nint hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    public static extern int SetWindowLong(nint hwnd, int index, int newLong);

    public static nint GetWindowLongPtr(nint hwnd, int index)
        => IntPtr.Size == 8 ? GetWindowLongPtr64(hwnd, index) : new nint(GetWindowLong(hwnd, index));

    public static nint SetWindowLongPtr(nint hwnd, int index, nint newLong)
        => IntPtr.Size == 8 ? SetWindowLongPtr64(hwnd, index, newLong) : new nint(SetWindowLong(hwnd, index, newLong.ToInt32()));

    [DllImport("user32.dll", EntryPoint = "GetClassLongPtrW", SetLastError = true)]
    private static extern nint GetClassLongPtr64(nint hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetClassLongPtrW", SetLastError = true)]
    private static extern nint SetClassLongPtr64(nint hwnd, int index, nint newLong);

    public static nint GetClassLongPtr(nint hwnd, int index)
        => IntPtr.Size == 8 ? GetClassLongPtr64(hwnd, index) : new nint(GetWindowLong(hwnd, index));

    public static nint SetClassLongPtr(nint hwnd, int index, nint newLong)
        => IntPtr.Size == 8 ? SetClassLongPtr64(hwnd, index, newLong) : new nint(SetWindowLong(hwnd, index, newLong.ToInt32()));

    [DllImport("user32.dll", EntryPoint = "GetWindowRect", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(nint hwnd, out RECT rect);

    [DllImport("user32.dll", EntryPoint = "EnableWindow", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnableWindow(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool enabled);

    [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowText(nint hwnd, string? text);

    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(nint hwnd);

    [DllImport("user32.dll", EntryPoint = "LoadCursorW", SetLastError = true)]
    public static extern nint LoadCursor(nint instance, nint cursorName);

    [DllImport("user32.dll", EntryPoint = "ReleaseCapture", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ReleaseCapture();

    [DllImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    public static extern nint SendMessage(nint hwnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll", EntryPoint = "SetCursor", SetLastError = true)]
    public static extern nint SetCursor(nint cursor);

    [DllImport("user32.dll", EntryPoint = "BeginPaint", SetLastError = true)]
    public static extern nint BeginPaint(nint hwnd, out PaintStruct paintStruct);

    [DllImport("user32.dll", EntryPoint = "EndPaint", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EndPaint(nint hwnd, ref PaintStruct paintStruct);

    [StructLayout(LayoutKind.Sequential)]
    public struct PaintStruct
    {
        public nint hdc;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fErase;
        public RECT rcPaint;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fRestore;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Reserved;
    }
}