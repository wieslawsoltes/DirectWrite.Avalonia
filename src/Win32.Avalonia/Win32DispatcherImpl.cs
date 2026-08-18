using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Win32.Avalonia;

internal unsafe sealed class Win32DispatcherImpl : IControlledDispatcherImpl
{
    private const uint QueueStatusInput = 0x0007;
    private const uint QueueStatusEvent = 0x02000;
    private const uint QueueStatusPostMessage = 0x0008;
    private const uint MessageWaitInputAvailable = 0x0004;
    private const uint WmDispatchWorkItem = 0x0400;

    private readonly nint _messageWindow;
    private static Thread? s_uiThread;
    private readonly Stopwatch _clock = Stopwatch.StartNew();

    public Win32DispatcherImpl(nint messageWindow)
    {
        _messageWindow = messageWindow;
        s_uiThread = Thread.CurrentThread;
    }

    public bool CurrentThreadIsLoopThread => s_uiThread == Thread.CurrentThread;

    internal const int SignalW = unchecked((int)0xdeadbeaf);
    internal const int SignalL = unchecked((int)0x12345678);

    public event Action? Signaled;
    public event Action? Timer;

    public void Signal()
    {
        PInvoke.PostMessage(
            new HWND(_messageWindow),
            WmDispatchWorkItem,
            new WPARAM(unchecked((nuint)(uint)SignalW)),
            new LPARAM(SignalL));
    }

    public void DispatchWorkItem() => Signaled?.Invoke();

    public void FireTimer() => Timer?.Invoke();

    public void UpdateTimer(long? dueTimeInMs)
    {
        if (dueTimeInMs is null)
        {
            PInvoke.KillTimer(new HWND(_messageWindow), (nuint)Win32Platform.TimerIdDispatcher);
            return;
        }

        var interval = unchecked((uint)Math.Min(int.MaxValue - 10, Math.Max(1, dueTimeInMs.Value - Now)));
        PInvoke.SetTimer(new HWND(_messageWindow), (nuint)Win32Platform.TimerIdDispatcher, interval, null);
    }

    public bool CanQueryPendingInput => true;

    public bool HasPendingInput
        => PInvoke.MsgWaitForMultipleObjectsEx(
               0,
               null,
               0,
               (QUEUE_STATUS_FLAGS)(QueueStatusInput | QueueStatusEvent | QueueStatusPostMessage),
               (MSG_WAIT_FOR_MULTIPLE_OBJECTS_EX_FLAGS)MessageWaitInputAvailable) == 0;

    public void RunLoop(CancellationToken cancellationToken)
    {
        var result = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            result = PInvoke.GetMessage(out var msg, new HWND(nint.Zero), 0, 0).Value;

            if (result <= 0)
            {
                break;
            }

            PInvoke.TranslateMessage(msg);
            PInvoke.DispatchMessage(msg);
        }

        if (result < 0)
        {
            Marshal.GetLastWin32Error();
        }
    }

    public long Now => _clock.ElapsedMilliseconds;
}