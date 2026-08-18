using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Logging;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using Avalonia.Rendering;

namespace Win32.Avalonia;

internal sealed class DirectCompositionConnection : IRenderTimer, IWindowsSurfaceFactory
{
    private const string LogArea = "DirectComposition";

    private readonly DirectCompositionShared _shared;
    private readonly AutoResetEvent _wakeEvent = new(false);
    private Action<TimeSpan>? _tick;
    private bool _stopped = true;

    private DirectCompositionConnection(DirectCompositionShared shared)
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
        => Win32Platform.WindowsVersion >= DCompositionNative.MinDirectCompositionVersion;

    public static bool TryCreateAndRegister()
    {
        if (!IsSupported())
        {
            Logger.TryGet(LogEventLevel.Warning, LogArea)?.Log(
                null,
                "Unable to initialize DirectComposition: Windows {0} is required. Current version is {1}.",
                DCompositionNative.MinDirectCompositionVersion,
                Win32Platform.WindowsVersion);
            return false;
        }

        try
        {
            var taskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                DirectCompositionConnection? connection = null;

                try
                {
                    var hr = DCompositionNative.DCompositionCreateDevice2(0, DCompositionNative.IIdCompositionDesktopDevice, out var devicePointer);
                    if (hr < 0 || devicePointer == 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    connection = new DirectCompositionConnection(new DirectCompositionShared(devicePointer));
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
                Name = "DirectCompositionRenderTimerLoop",
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return taskSource.Task.Result;
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, LogArea)?.Log(null, "Unable to initialize DirectComposition: {0}", ex);
            return false;
        }
    }

    public IPlatformRenderSurface CreateSurface(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info)
        => new DirectCompositedWindowSurface(_shared, info);

    private void RunLoop()
    {
        using var exitSource = new CancellationTokenSource();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => exitSource.Cancel();
        var stopwatch = Stopwatch.StartNew();

        while (!exitSource.IsCancellationRequested)
        {
            try
            {
                if (_stopped)
                {
                    WaitHandle.WaitAny([_wakeEvent, exitSource.Token.WaitHandle]);
                }

                var hr = _shared.Device.WaitForCommitCompletion();
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                _tick?.Invoke(stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea)?.Log(this, "DirectComposition render loop failed: {0}", ex);
            }
        }
    }
}