using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.OpenGL;

namespace Win32.Avalonia;

internal sealed class WglRestoreContext : IDisposable
{
    private readonly object? _monitor;
    private readonly nint _oldDc;
    private readonly nint _oldContext;

    public WglRestoreContext(nint dc, nint context, object? monitor, bool takeMonitor = true)
    {
        _monitor = monitor;
        _oldDc = WglNative.GetCurrentDc();
        _oldContext = WglNative.GetCurrentContext();

        if (monitor is not null && takeMonitor)
        {
            Monitor.Enter(monitor);
        }

        if (!WglNative.MakeCurrent(dc, context))
        {
            if (monitor is not null && takeMonitor)
            {
                Monitor.Exit(monitor);
            }

            throw new OpenGlException($"Unable to make the context current: {Marshal.GetLastPInvokeError()}");
        }
    }

    public void Dispose()
    {
        if (!WglNative.MakeCurrent(_oldDc, _oldContext))
        {
            WglNative.MakeCurrent(nint.Zero, nint.Zero);
        }

        if (_monitor is not null)
        {
            Monitor.Exit(_monitor);
        }
    }
}