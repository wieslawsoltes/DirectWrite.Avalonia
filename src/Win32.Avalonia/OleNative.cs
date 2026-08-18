using System.Runtime.InteropServices;
using System.Text;

namespace Win32.Avalonia;

internal static unsafe partial class OleNative
{
    [LibraryImport("ole32.dll", EntryPoint = "OleInitialize")]
    public static partial int OleInitialize(nint reserved);

    [LibraryImport("ole32.dll", EntryPoint = "RegisterDragDrop")]
    public static partial int RegisterDragDrop(nint hwnd, nint target);

    [LibraryImport("ole32.dll", EntryPoint = "RevokeDragDrop")]
    public static partial int RevokeDragDrop(nint hwnd);

    [LibraryImport("ole32.dll", EntryPoint = "DoDragDrop")]
    public static partial int DoDragDrop(nint dataObject, nint dropSource, int allowedEffects, out int finalEffect);

    [LibraryImport("ole32.dll", EntryPoint = "OleSetClipboard")]
    public static partial int OleSetClipboard(nint dataObject);

    [LibraryImport("ole32.dll", EntryPoint = "OleGetClipboard")]
    public static partial int OleGetClipboard(out nint dataObject);

    [LibraryImport("ole32.dll", EntryPoint = "OleIsCurrentClipboard")]
    public static partial int OleIsCurrentClipboard(nint dataObject);

    [LibraryImport("ole32.dll", EntryPoint = "OleFlushClipboard")]
    public static partial int OleFlushClipboard();

    [LibraryImport("ole32.dll", EntryPoint = "ReleaseStgMedium")]
    public static partial void ReleaseStgMedium(ref StgMedium medium);

    [LibraryImport("user32.dll", EntryPoint = "OpenClipboard", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool OpenClipboard(nint owner);

    [LibraryImport("user32.dll", EntryPoint = "CloseClipboard", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool CloseClipboard();

    [LibraryImport("user32.dll", EntryPoint = "EmptyClipboard", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool EmptyClipboard();

    [LibraryImport("kernel32.dll", EntryPoint = "GlobalLock", SetLastError = true)]
    public static partial nint GlobalLock(nint hMem);

    [LibraryImport("kernel32.dll", EntryPoint = "GlobalUnlock", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GlobalUnlock(nint hMem);

    [LibraryImport("kernel32.dll", EntryPoint = "GlobalSize", SetLastError = true)]
    public static partial nuint GlobalSize(nint hMem);

    [LibraryImport("kernel32.dll", EntryPoint = "GlobalAlloc", SetLastError = true)]
    public static partial nint GlobalAlloc(int flags, nuint bytes);

    [LibraryImport("shell32.dll", EntryPoint = "DragQueryFileW", SetLastError = true)]
    public static partial uint DragQueryFile(nint drop, uint index, char* fileName, uint fileNameCount);

    [LibraryImport("user32.dll", EntryPoint = "GetClipboardFormatNameW", SetLastError = true)]
    private static partial int GetClipboardFormatNameCore(uint format, char* buffer, int capacity);

    [LibraryImport("user32.dll", EntryPoint = "RegisterClipboardFormatW", SetLastError = true)]
    private static partial uint RegisterClipboardFormatCore(char* format);

    public static string? GetClipboardFormatName(uint format)
    {
        Span<char> buffer = stackalloc char[260];
        fixed (char* bufferPointer = buffer)
        {
            var length = GetClipboardFormatNameCore(format, bufferPointer, buffer.Length);
            return length > 0 ? new string(buffer[..length]) : null;
        }
    }

    public static uint RegisterClipboardFormat(string format)
    {
        var chars = format.ToCharArray();
        fixed (char* formatPointer = chars)
        {
            return RegisterClipboardFormatCore(formatPointer);
        }
    }
}