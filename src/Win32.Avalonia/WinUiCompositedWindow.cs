using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.OpenGL.Egl;

namespace Win32.Avalonia;

internal sealed class WinUiCompositedWindow : IDisposable
{
    private readonly WinUiCompositionShared _shared;
    private readonly nint _blurPointer;
    private readonly nint _contentBrushPointer;
    private readonly nint _desktopWindowTargetPointer;
    private readonly nint _geometryPointer;
    private readonly nint _micaDarkPointer;
    private readonly nint _micaLightPointer;
    private readonly nint _targetPointer;
    private readonly nint _surfaceBrushPointer;
    private readonly nint _visualPointer;
    private PixelSize _size;

    public WinUiCompositedWindow(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info, WinUiCompositionShared shared, float? backdropCornerRadius)
    {
        WindowInfo = info;
        _shared = shared;

        var hr = shared.DesktopInterop.CreateDesktopWindowTarget(info.Handle, 0, out _desktopWindowTargetPointer);
        if (hr < 0 || _desktopWindowTargetPointer == 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        Target = GeneratedComHelpers.QueryInterface<ICompositionTargetCom>(_desktopWindowTargetPointer, out _targetPointer);

        hr = shared.Compositor.CreateContainerVisual(out var containerPointer);
        if (hr < 0 || containerPointer == nint.Zero)
        {
            Dispose();
            Marshal.ThrowExceptionForHR(hr);
        }

        try
        {
            var container = GeneratedComHelpers.ConvertToManaged<IContainerVisualCom>(containerPointer)
                ?? throw new InvalidOperationException("Unable to wrap the WinUI container visual.");
            var containerVisual = GeneratedComHelpers.QueryInterface<IVisualCom>(containerPointer, out var containerVisualPointer);

            try
            {
                var containerVisual2 = GeneratedComHelpers.QueryInterface<IVisual2Com>(containerPointer, out var containerVisual2Pointer);
                var childrenPointer = nint.Zero;

                try
                {
                    hr = containerVisual2.SetRelativeSizeAdjustment(new WinRTVector2 { X = 1f, Y = 1f });
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    hr = Target.SetRoot(containerVisualPointer);
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    hr = container.GetChildren(out childrenPointer);
                    if (hr < 0 || childrenPointer == nint.Zero)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    var children = GeneratedComHelpers.ConvertToManaged<IVisualCollectionCom>(childrenPointer)
                        ?? throw new InvalidOperationException("Unable to wrap the WinUI visual collection.");

                    if (shared.BlurBrushPointer != nint.Zero)
                    {
                        _blurPointer = WinUiCompositionUtils.CreateBrushVisual(shared.CompositorPointer, shared.BlurBrushPointer, visible: false);
                        BlurVisual = GeneratedComHelpers.ConvertToManaged<IVisualCom>(_blurPointer)
                            ?? throw new InvalidOperationException("Unable to wrap the acrylic blur visual.");
                    }

                    if (shared.MicaBrushLightPointer != nint.Zero)
                    {
                        _micaLightPointer = WinUiCompositionUtils.CreateBrushVisual(shared.CompositorPointer, shared.MicaBrushLightPointer, visible: false);
                        MicaLightVisual = GeneratedComHelpers.ConvertToManaged<IVisualCom>(_micaLightPointer)
                            ?? throw new InvalidOperationException("Unable to wrap the light mica visual.");
                    }

                    if (shared.MicaBrushDarkPointer != nint.Zero)
                    {
                        _micaDarkPointer = WinUiCompositionUtils.CreateBrushVisual(shared.CompositorPointer, shared.MicaBrushDarkPointer, visible: false);
                        MicaDarkVisual = GeneratedComHelpers.ConvertToManaged<IVisualCom>(_micaDarkPointer)
                            ?? throw new InvalidOperationException("Unable to wrap the dark mica visual.");
                    }

                    _geometryPointer = WinUiCompositionUtils.CreateRoundedClipGeometry(
                        shared.CompositorPointer,
                        backdropCornerRadius,
                        _blurPointer,
                        _micaLightPointer,
                        _micaDarkPointer);

                    if (_geometryPointer != nint.Zero)
                    {
                        RoundedRectangleGeometry = GeneratedComHelpers.ConvertToManaged<ICompositionRoundedRectangleGeometryCom>(_geometryPointer)
                            ?? throw new InvalidOperationException("Unable to wrap the rounded clip geometry.");
                    }

                    if (_micaLightPointer != nint.Zero)
                    {
                        hr = children.InsertAtTop(_micaLightPointer);
                        if (hr < 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }
                    }

                    if (_micaDarkPointer != nint.Zero)
                    {
                        hr = children.InsertAtTop(_micaDarkPointer);
                        if (hr < 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }
                    }

                    if (_blurPointer != nint.Zero)
                    {
                        hr = children.InsertAtTop(_blurPointer);
                        if (hr < 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }
                    }

                    hr = shared.Compositor.CreateSurfaceBrush(out _surfaceBrushPointer);
                    if (hr < 0 || _surfaceBrushPointer == nint.Zero)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    SurfaceBrush = GeneratedComHelpers.ConvertToManaged<ICompositionSurfaceBrushCom>(_surfaceBrushPointer)
                        ?? throw new InvalidOperationException("Unable to wrap the WinUI composition surface brush.");
                    GeneratedComHelpers.QueryInterface<ICompositionBrushCom>(_surfaceBrushPointer, out _contentBrushPointer);

                    hr = shared.Compositor.CreateSpriteVisual(out var spriteVisualPointer);
                    if (hr < 0 || spriteVisualPointer == nint.Zero)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    try
                    {
                        var spriteVisual = GeneratedComHelpers.ConvertToManaged<ISpriteVisualCom>(spriteVisualPointer)
                            ?? throw new InvalidOperationException("Unable to wrap the WinUI sprite visual.");
                        Visual = GeneratedComHelpers.QueryInterface<IVisualCom>(spriteVisualPointer, out _visualPointer);

                        hr = spriteVisual.SetBrush(_contentBrushPointer);
                        if (hr < 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }

                        hr = children.InsertAtTop(_visualPointer);
                        if (hr < 0)
                        {
                            Marshal.ThrowExceptionForHR(hr);
                        }
                    }
                    finally
                    {
                        GeneratedComHelpers.Free<ISpriteVisualCom>(spriteVisualPointer);
                    }
                }
                finally
                {
                    GeneratedComHelpers.Free<IVisualCollectionCom>(childrenPointer);
                    GeneratedComHelpers.Free<IVisual2Com>(containerVisual2Pointer);
                }
            }
            finally
            {
                GeneratedComHelpers.Free<IVisualCom>(containerVisualPointer);
            }
        }
        finally
        {
            GeneratedComHelpers.Free<IContainerVisualCom>(containerPointer);
        }
    }

    public EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo WindowInfo { get; }

    private IVisualCom? BlurVisual { get; }

    private IVisualCom? MicaDarkVisual { get; }

    private IVisualCom? MicaLightVisual { get; }

    private ICompositionRoundedRectangleGeometryCom? RoundedRectangleGeometry { get; }

    private ICompositionTargetCom Target { get; }

    private IVisualCom Visual { get; }

    private ICompositionSurfaceBrushCom SurfaceBrush { get; }

    public void Dispose()
    {
        GeneratedComHelpers.Free<ICompositionBrushCom>(_contentBrushPointer);
        GeneratedComHelpers.Free<ICompositionSurfaceBrushCom>(_surfaceBrushPointer);
        GeneratedComHelpers.Free<ICompositionRoundedRectangleGeometryCom>(_geometryPointer);
        GeneratedComHelpers.Free<IVisualCom>(_micaDarkPointer);
        GeneratedComHelpers.Free<IVisualCom>(_micaLightPointer);
        GeneratedComHelpers.Free<IVisualCom>(_blurPointer);
        GeneratedComHelpers.Free<IVisualCom>(_visualPointer);
        GeneratedComHelpers.Free<ICompositionTargetCom>(_targetPointer);
        GeneratedComHelpers.Free<IInspectableCom>(_desktopWindowTargetPointer);
    }

    public IDisposable BeginTransaction()
    {
        Monitor.Enter(_shared.SyncRoot);
        return new ActionDisposable(() => Monitor.Exit(_shared.SyncRoot));
    }

    public void SetBlur(BlurEffect blurEffect)
    {
        lock (_shared.SyncRoot)
        {
            if (BlurVisual is not null)
            {
                var hr = BlurVisual.SetIsVisible(
                    blurEffect == BlurEffect.Acrylic
                    || (blurEffect == BlurEffect.MicaLight && MicaLightVisual is null)
                    || (blurEffect == BlurEffect.MicaDark && MicaDarkVisual is null)
                        ? 1
                        : 0);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            if (MicaLightVisual is not null)
            {
                var hr = MicaLightVisual.SetIsVisible(blurEffect == BlurEffect.MicaLight ? 1 : 0);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            if (MicaDarkVisual is not null)
            {
                var hr = MicaDarkVisual.SetIsVisible(blurEffect == BlurEffect.MicaDark ? 1 : 0);
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
        }
    }

    public void ResizeIfNeeded(PixelSize size)
    {
        lock (_shared.SyncRoot)
        {
            if (_size == size)
            {
                return;
            }

            var hr = Visual.SetSize(new WinRTVector2 { X = size.Width, Y = size.Height });
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            if (RoundedRectangleGeometry is not null)
            {
                hr = RoundedRectangleGeometry.SetSize(new WinRTVector2 { X = size.Width, Y = size.Height });
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            _size = size;
        }
    }

    public void SetSurface(nint surfacePointer)
    {
        var hr = SurfaceBrush.SetSurface(surfacePointer);
        if (hr < 0)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
    }

    private sealed class ActionDisposable(Action dispose) : IDisposable
    {
        private Action? _dispose = dispose;

        public void Dispose()
            => Interlocked.Exchange(ref _dispose, null)?.Invoke();
    }
}