using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static partial class TrayNative
{
    internal enum NotifyIconMessage : uint
    {
        Add = 0,
        Modify = 1,
        Delete = 2,
    }

    internal static class NotifyIconFlags
    {
        public const uint Message = 0x00000001;
        public const uint Icon = 0x00000002;
        public const uint Tip = 0x00000004;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal unsafe struct NotifyIconData
    {
        public uint cbSize;
        public nint hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public nint hIcon;
        public fixed char szTip[128];
        public uint dwState;
        public uint dwStateMask;
        public fixed char szInfo[256];
        public uint uTimeoutOrVersion;
        public fixed char szInfoTitle[64];
        public uint dwInfoFlags;
        public Guid guidItem;
        public nint hBalloonIcon;

        public void SetToolTip(string? text)
        {
            fixed (char* tooltip = szTip)
            {
                var span = new Span<char>(tooltip, 128);
                span.Clear();
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                text.AsSpan(0, Math.Min(text.Length, 127)).CopyTo(span);
            }
        }
    }

    [LibraryImport("shell32.dll", EntryPoint = "Shell_NotifyIconW", SetLastError = true)]
    internal static partial int Shell_NotifyIcon(NotifyIconMessage message, ref NotifyIconData data);
}