using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Win32.Avalonia;

internal unsafe sealed class ScreenImpl : ScreensBase<nint, WinScreen>
{
    protected override int GetScreenCount() => GetAllDisplayMonitorHandlers().Count;

    protected override IReadOnlyList<nint> GetAllScreenKeys() => GetAllDisplayMonitorHandlers();

    public static List<nint> GetAllDisplayMonitorHandlers()
    {
        var screens = new List<nint>();
        var gcHandle = GCHandle.Alloc(screens);
        try
        {
            PInvoke.EnumDisplayMonitors(default, null, &EnumDisplayMonitorsCallback, (LPARAM)GCHandle.ToIntPtr(gcHandle));
        }
        finally
        {
            gcHandle.Free();
        }

        return screens;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        static BOOL EnumDisplayMonitorsCallback(HMONITOR monitor, HDC hdcMonitor, RECT* lprcMonitor, LPARAM dwData)
        {
            if (GCHandle.FromIntPtr(dwData).Target is List<nint> screens)
            {
                screens.Add((nint)monitor.Value);
                return true;
            }

            return false;
        }
    }

    protected override WinScreen CreateScreenFromKey(nint key) => new(key);

    protected override void ScreenChanged(WinScreen screen) => screen.Refresh();

    protected override Screen? ScreenFromTopLevelCore(ITopLevelImpl topLevel)
    {
        if (topLevel.Handle?.Handle is { } handle)
        {
            return ScreenFromHwnd(handle);
        }

        return null;
    }

    protected override Screen? ScreenFromPointCore(PixelPoint point)
    {
        foreach (var screen in AllScreens)
        {
            if (screen.Bounds.Contains(point))
            {
                return screen;
            }
        }

        return null;
    }

    protected override Screen? ScreenFromRectCore(PixelRect rect)
    {
        foreach (var screen in AllScreens)
        {
            if (screen.Bounds.Intersects(rect))
            {
                return screen;
            }
        }

        return null;
    }

    public WinScreen? ScreenFromHMonitor(nint hmonitor)
        => TryGetScreen(hmonitor, out var screen) ? screen : null;

    public WinScreen? ScreenFromHwnd(nint hwnd, MONITOR_FROM_FLAGS flags = MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONULL)
    {
        var monitor = PInvoke.MonitorFromWindow(new HWND(hwnd), flags);
        return ScreenFromHMonitor((nint)monitor.Value);
    }
}