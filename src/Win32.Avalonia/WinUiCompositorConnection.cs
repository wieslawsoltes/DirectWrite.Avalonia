using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Avalonia;
using Avalonia.Logging;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Rendering;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace Win32.Avalonia;

internal sealed partial class WinUiCompositorConnection : IRenderTimer, IWindowsSurfaceFactory
{
    private const string LogArea = "WinUIComposition";

    private readonly WinUiCompositionShared _shared;
    private readonly AutoResetEvent _wakeEvent = new(false);
    private bool _stopped = true;
    private Action<TimeSpan>? _tick;

    private WinUiCompositorConnection(WinUiCompositionShared shared)
    {
        _shared = shared;
    }

    public bool RunsInBackground => true;

    public bool RequiresNoRedirectionBitmap => true;

    public Action<TimeSpan>? Tick
    {
        get => _tick;
        set
        {
            _tick = value;
            _stopped = value is null;
            if (value is not null)
            {
                _wakeEvent.Set();
            }
        }
    }

    public static bool IsSupported()
        => Win32Platform.WindowsVersion >= WinUiCompositionShared.MinWinCompositionVersion;

    public static bool TryCreateAndRegister()
    {
        if (!IsSupported())
        {
            Logger.TryGet(LogEventLevel.Warning, LogArea)?.Log(
                null,
                "Unable to initialize WinUI composition: Windows {0} is required. Current version is {1}.",
                WinUiCompositionShared.MinWinCompositionVersion,
                Win32Platform.WindowsVersion);
            return false;
        }

        try
        {
            var taskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                WinUiCompositorConnection? connection = null;

                try
                {
                    WinRTNativeMethods.CreateDispatcherQueueController(new WinRTNativeMethods.DispatcherQueueOptions
                    {
                        Size = Marshal.SizeOf<WinRTNativeMethods.DispatcherQueueOptions>(),
                        ThreadType = WinRTNativeMethods.DispatcherQueueThreadType.Current,
                        ApartmentType = WinRTNativeMethods.DispatcherQueueThreadApartmentType.ComNone,
                    });

                    var compositorPointer = WinRTNativeMethods.ActivateInstance("Windows.UI.Composition.Compositor");
                    connection = new WinUiCompositorConnection(new WinUiCompositionShared(compositorPointer));
                    AvaloniaLocator.CurrentMutable.Bind<IWindowsSurfaceFactory>().ToConstant(connection);
                    AvaloniaLocator.CurrentMutable.Bind<IRenderLoop>().ToConstant(RenderLoop.FromTimer(connection));
                    taskSource.SetResult(true);
                }
                catch (Exception ex)
                {
                    taskSource.SetException(ex);
                    return;
                }

                connection.RunLoop();
            })
            {
                IsBackground = true,
                Name = "WinUiCompositionRenderTimerLoop",
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return taskSource.Task.Result;
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, LogArea)?.Log(null, "Unable to initialize WinUI composition: {0}", ex);
            return false;
        }
    }

    public IPlatformRenderSurface CreateSurface(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info)
        => new WinUiCompositedWindowSurface(_shared, info);

    private void RunLoop()
    {
        using var exitSource = new CancellationTokenSource();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => exitSource.Cancel();

        var handler = new RunLoopHandler(this);
        handler.Start();

        while (!exitSource.IsCancellationRequested && PInvoke.GetMessage(out var message, new HWND(0), 0, 0) > 0)
        {
            lock (_shared.SyncRoot)
            {
                PInvoke.DispatchMessage(message);
            }
        }
    }

    [GeneratedComClass]
    private sealed partial class RunLoopHandler : IAsyncActionCompletedHandlerCom
    {
        private readonly WinUiCompositorConnection _parent;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private IAsyncActionCom? _currentCommit;
        private nint _currentCommitPointer;

        public RunLoopHandler(WinUiCompositorConnection parent)
        {
            _parent = parent;
        }

        public void Start() => ScheduleNextCommit();

        public int Invoke(IAsyncActionCom? asyncInfo, WinRTAsyncStatus asyncStatus)
        {
            OnCommitCompleted();
            return 0;
        }

        private void OnCommitCompleted()
        {
            if (_currentCommitPointer != 0)
            {
                GeneratedComHelpers.Free<IAsyncActionCom>(_currentCommitPointer);
                _currentCommitPointer = 0;
                _currentCommit = null;
            }

            _parent._tick?.Invoke(_stopwatch.Elapsed);
            ScheduleNextCommit();

            if (_parent._stopped)
            {
                _parent._wakeEvent.WaitOne();
            }
        }

        private void ScheduleNextCommit()
        {
            lock (_parent._shared.SyncRoot)
            {
                var hr = _parent._shared.Compositor5.RequestCommitAsync(out _currentCommitPointer);
                if (hr < 0 || _currentCommitPointer == 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                _currentCommit = GeneratedComHelpers.ConvertToManaged<IAsyncActionCom>(_currentCommitPointer)
                    ?? throw new InvalidOperationException("Unable to wrap the WinUI commit action.");

                hr = _currentCommit.SetCompleted(this);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }
    }
}