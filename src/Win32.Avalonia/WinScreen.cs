using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Win32.Avalonia;

internal sealed unsafe class WinScreen(nint hMonitor) : PlatformScreen(new PlatformHandle(hMonitor, "HMonitor"))
{
    public void Refresh()
    {
        var info = new MONITORINFO
        {
            cbSize = (uint)Marshal.SizeOf<MONITORINFO>(),
        };

        PInvoke.GetMonitorInfo(new HMONITOR(hMonitor), &info);
        IsPrimary = info.dwFlags == 1;
        Bounds = new PixelRect(info.rcMonitor.left, info.rcMonitor.top,
            info.rcMonitor.right - info.rcMonitor.left,
            info.rcMonitor.bottom - info.rcMonitor.top);
        WorkingArea = new PixelRect(info.rcWork.left, info.rcWork.top,
            info.rcWork.right - info.rcWork.left,
            info.rcWork.bottom - info.rcWork.top);
        Scaling = 1.0;
        CurrentOrientation = ScreenOrientation.None;
    }
}