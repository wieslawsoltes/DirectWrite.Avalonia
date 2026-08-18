using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia;

internal static unsafe class WglGdiResourceManager
{
    private sealed class GetDcOp(nint window, TaskCompletionSource<nint> result)
    {
        public nint Window { get; } = window;
        public TaskCompletionSource<nint> Result { get; } = result;
    }

    private sealed class ReleaseDcOp(nint window, nint dc, TaskCompletionSource<object?> result)
    {
        public nint Window { get; } = window;
        public nint Dc { get; } = dc;
        public TaskCompletionSource<object?> Result { get; } = result;
    }

    private sealed class CreateWindowOp(TaskCompletionSource<nint> result)
    {
        public TaskCompletionSource<nint> Result { get; } = result;
    }

    private sealed class DestroyWindowOp(nint window, TaskCompletionSource<object?> result)
    {
        public nint Window { get; } = window;
        public TaskCompletionSource<object?> Result { get; } = result;
    }

    private static readonly Queue<object> s_queue = new();
    private static readonly AutoResetEvent s_event = new(false);
    private static readonly string s_className = $"Win32.Avalonia.WglWindow-{Guid.NewGuid()}";

    static WglGdiResourceManager()
    {
        var windowClass = new WNDCLASSEXW
        {
            cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
            hInstance = PInvoke.GetModuleHandle((PCWSTR)null),
            style = WNDCLASS_STYLES.CS_OWNDC,
        };

        fixed (char* className = s_className)
        {
            windowClass.lpszClassName = className;
            windowClass.lpfnWndProc = &WndProc;
            PInvoke.RegisterClassEx(windowClass);
        }

        var thread = new Thread(Worker)
        {
            IsBackground = true,
            Name = "Win32 OpenGL HDC manager",
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    public static nint CreateOffscreenWindow()
    {
        var completion = new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (s_queue)
        {
            s_queue.Enqueue(new CreateWindowOp(completion));
        }

        s_event.Set();
        return completion.Task.Result;
    }

    public static nint GetDc(nint window)
    {
        var completion = new TaskCompletionSource<nint>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (s_queue)
        {
            s_queue.Enqueue(new GetDcOp(window, completion));
        }

        s_event.Set();
        return completion.Task.Result;
    }

    public static void ReleaseDc(nint window, nint dc)
    {
        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (s_queue)
        {
            s_queue.Enqueue(new ReleaseDcOp(window, dc, completion));
        }

        s_event.Set();
        completion.Task.Wait();
    }

    public static void DestroyWindow(nint window)
    {
        var completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (s_queue)
        {
            s_queue.Enqueue(new DestroyWindowOp(window, completion));
        }

        s_event.Set();
        completion.Task.Wait();
    }

    private static void Worker()
    {
        while (true)
        {
            s_event.WaitOne();

            object? job = null;
            lock (s_queue)
            {
                if (s_queue.Count > 0)
                {
                    job = s_queue.Dequeue();
                }
            }

            switch (job)
            {
                case GetDcOp getDc:
                    getDc.Result.TrySetResult((nint)PInvoke.GetDC(new HWND(getDc.Window)).Value);
                    break;
                case ReleaseDcOp releaseDc:
                    PInvoke.ReleaseDC(new HWND(releaseDc.Window), new HDC(releaseDc.Dc));
                    releaseDc.Result.TrySetResult(null);
                    break;
                case CreateWindowOp createWindow:
                {
                    var hwnd = PInvoke.CreateWindowEx(
                        0,
                        s_className,
                        string.Empty,
                        WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                        0,
                        0,
                        1,
                        1,
                        new HWND(nint.Zero),
                        null,
                        null,
                        null);
                    createWindow.Result.TrySetResult((nint)hwnd.Value);
                    break;
                }
                case DestroyWindowOp destroyWindow:
                    PInvoke.DestroyWindow(new HWND(destroyWindow.Window));
                    destroyWindow.Result.TrySetResult(null);
                    break;
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
    private static LRESULT WndProc(HWND hWnd, uint msg, WPARAM wParam, LPARAM lParam)
        => PInvoke.DefWindowProc(hWnd, msg, wParam, lParam);
}