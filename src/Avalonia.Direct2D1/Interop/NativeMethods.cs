using System;
using System.Runtime.InteropServices;

namespace Avalonia.Direct2D1.Interop;

internal static partial class NativeMethods
{
    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [LibraryImport("user32.dll")]
    public static partial uint GetDpiForWindow(IntPtr hWnd);

    public static unsafe void CopyMemory(IntPtr destination, IntPtr source, nuint byteCount)
    {
        Buffer.MemoryCopy((void*)source, (void*)destination, byteCount, byteCount);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
