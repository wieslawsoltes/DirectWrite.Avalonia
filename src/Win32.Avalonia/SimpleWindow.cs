using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia;

internal unsafe sealed class SimpleWindow : IDisposable
{
    private const int CwUseDefault = unchecked((int)0x80000000);
    private const uint WmDestroy = 0x0002;

    internal delegate nint WindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    private readonly WindowProc? _wndProc;
    private static readonly string s_className = "Win32.Avalonia.SimpleWindow-" + Guid.NewGuid();
    private static readonly ConcurrentDictionary<nint, SimpleWindow> s_instances = new();

    internal sealed class Options
    {
        public WINDOW_EX_STYLE ExtendedStyle { get; init; }
        public WINDOW_STYLE Style { get; init; } = WINDOW_STYLE.WS_OVERLAPPEDWINDOW | WINDOW_STYLE.WS_CLIPCHILDREN;
        public nint Parent { get; init; }
        public int X { get; init; } = CwUseDefault;
        public int Y { get; init; } = CwUseDefault;
        public int Width { get; init; } = CwUseDefault;
        public int Height { get; init; } = CwUseDefault;
    }

    static SimpleWindow()
    {
        var wndClassEx = new WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
            hInstance = PInvoke.GetModuleHandle((PCWSTR)null),
            style = WNDCLASS_STYLES.CS_OWNDC | WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
        };

        fixed (char* className = s_className)
        {
            wndClassEx.lpszClassName = className;
            wndClassEx.lpfnWndProc = &WndProc;
            PInvoke.RegisterClassEx(wndClassEx);
        }
    }

    internal SimpleWindow(WindowProc? wndProc)
        : this(wndProc, new Options())
    {
    }

    internal SimpleWindow(WindowProc? wndProc, Options options)
    {
        _wndProc = wndProc;
        var hwnd = PInvoke.CreateWindowEx(
            options.ExtendedStyle,
            s_className,
            string.Empty,
            options.Style,
            options.X,
            options.Y,
            options.Width,
            options.Height,
            new HWND(options.Parent),
            null,
            null,
            null);

        if (hwnd.IsNull)
        {
            throw new Win32Exception();
        }

        Handle = (nint)hwnd.Value;
        s_instances.TryAdd(Handle, this);
    }

    public nint Handle { get; private set; }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    private static LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
    {
        s_instances.TryGetValue((nint)hWnd.Value, out var window);

        if (msg == WmDestroy)
        {
            s_instances.TryRemove((nint)hWnd.Value, out _);
        }

        if (window?._wndProc?.Invoke(hWnd, msg, (nint)wParam.Value, lParam.Value) is { } result)
        {
            return new LRESULT(result);
        }

        return PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        if (Handle != nint.Zero)
        {
            s_instances.TryRemove(Handle, out _);
            PInvoke.DestroyWindow(new HWND(Handle));
            Handle = nint.Zero;
        }
    }
}