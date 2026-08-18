using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Logging;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform.Surfaces;
using Avalonia.Rendering;

namespace Win32.Avalonia;

internal sealed class DxgiConnection : IRenderTimer, IWindowsSurfaceFactory
{
    private const string LogArea = "DXGI";

    private readonly object _syncLock;
    private readonly AutoResetEvent _wakeEvent = new(false);
    private Action<TimeSpan>? _tick;
    private IDxgiOutputCom? _output;
    private bool _stopped = true;
    private Stopwatch? _stopwatch;

    public DxgiConnection(object syncLock)
    {
        _syncLock = syncLock;
    }

    public bool RunsInBackground => true;

    public bool RequiresNoRedirectionBitmap => false;

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

    public static bool TryCreateAndRegister()
    {
        try
        {
            var taskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                try
                {
                    var connection = new DxgiConnection(new object());
                    AvaloniaLocator.CurrentMutable.Bind<IWindowsSurfaceFactory>().ToConstant(connection);
                    AvaloniaLocator.CurrentMutable.Bind<IRenderLoop>().ToConstant(RenderLoop.FromTimer(connection));
                    taskSource.SetResult(true);
                    connection.RunLoop();
                }
                catch (Exception ex)
                {
                    taskSource.SetException(ex);
                }
            })
            {
                IsBackground = true,
                Name = "DxgiRenderTimerLoop",
            };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return taskSource.Task.Result;
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, LogArea)?.Log(null, "Unable to register DXGI timer: {0}", ex);
            return false;
        }
    }

    public IPlatformRenderSurface CreateSurface(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info)
        => new DxgiSwapchainWindow(this, info);

    private void RunLoop()
    {
        _stopwatch = Stopwatch.StartNew();
        TryGetBestOutputToWaitOn();

        while (true)
        {
            try
            {
                if (_stopped)
                {
                    _wakeEvent.WaitOne();
                }

                lock (_syncLock)
                {
                    if (_output is not null)
                    {
                        try
                        {
                            _output.WaitForVBlank();
                        }
                        catch (Exception ex)
                        {
                            Logger.TryGet(LogEventLevel.Error, LogArea)?.Log(this, "Failed to wait for vblank: {0}", ex);
                            _output = null;
                            TryGetBestOutputToWaitOn();
                        }
                    }
                    else
                    {
                        DxgiNative.DwmFlush();
                    }

                    _tick?.Invoke(_stopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea)?.Log(this, "DXGI timer loop failed: {0}", ex);
            }
        }
    }

    private void TryGetBestOutputToWaitOn()
    {
        try
        {
            var result = DxgiNative.CreateDxgiFactory(DxgiNative.IdxgiFactory, out var factoryPointer);
            if (result < 0 || factoryPointer == nint.Zero)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            var factory = GeneratedComHelpers.ConvertToManaged<IDxgiFactoryCom>(factoryPointer)!;
            for (uint adapterIndex = 0; factory.EnumAdapters(adapterIndex, out var adapterPointer) == 0; adapterIndex++)
            {
                var adapter = GeneratedComHelpers.ConvertToManaged<IDxgiAdapterCom>(adapterPointer)!;
                for (uint outputIndex = 0; adapter.EnumOutputs(outputIndex, out var outputPointer) == 0; outputIndex++)
                {
                    if (GeneratedComHelpers.ConvertToManaged<IDxgiOutputCom>(outputPointer) is { } output)
                    {
                        _output = output;
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, LogArea)?.Log(this, "Failed to determine DXGI output: {0}", ex);
        }
    }
}