using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static class WinUiCompositionUtils
{
    public static nint CreateMicaBackdropBrush(nint compositorPointer, float color, float opacity)
    {
        if (Win32Platform.WindowsVersion < WinUiCompositionShared.MinHostBackdropVersion)
        {
            return nint.Zero;
        }

        var tint = new[] { color / 255f, color / 255f, color / 255f, 1f };
        var factory = WinRTNativeMethods.GetActivationFactory<ICompositionEffectSourceParameterFactoryCom>(
            "Windows.UI.Composition.CompositionEffectSourceParameter",
            out var factoryPointer);

        try
        {
            var tintBrushPointer = CreateCompositionBrush(compositorPointer, new OpacityEffect(1.0f, new ColorSourceEffect(tint)));
            try
            {
                var luminosityBrushPointer = CreateCompositionBrush(compositorPointer, new OpacityEffect(opacity, new ColorSourceEffect(tint)));
                try
                {
                    var wallpaperBrushPointer = CreateBlurredWallpaperBackdropBrush(compositorPointer);
                    if (wallpaperBrushPointer == nint.Zero)
                    {
                        return nint.Zero;
                    }

                    try
                    {
                        using var backgroundName = new WinRTNativeMethods.HStringInterop("Background");
                        using var foregroundName = new WinRTNativeMethods.HStringInterop("Foreground");
                        var backgroundSource = CreateParameterSource(factory, backgroundName.Handle, out var backgroundSourcePointer);
                        var foregroundSource = CreateParameterSource(factory, foregroundName.Handle, out var foregroundSourcePointer);

                        try
                        {
                            var luminosityBlendBrushPointer = CreateEffectBrush(
                                compositorPointer,
                                new BlendEffect(23, backgroundSource, foregroundSource));

                            try
                            {
                                var luminosityBlendBrush = GeneratedComHelpers.ConvertToManaged<ICompositionEffectBrushCom>(luminosityBlendBrushPointer)
                                    ?? throw new InvalidOperationException("Unable to wrap the luminosity blend brush.");

                                var hr = luminosityBlendBrush.SetSourceParameter(backgroundName.Handle, wallpaperBrushPointer);
                                if (hr < 0)
                                {
                                    Marshal.ThrowExceptionForHR(hr);
                                }

                                hr = luminosityBlendBrush.SetSourceParameter(foregroundName.Handle, luminosityBrushPointer);
                                if (hr < 0)
                                {
                                    Marshal.ThrowExceptionForHR(hr);
                                }

                                GeneratedComHelpers.QueryInterface<ICompositionBrushCom>(luminosityBlendBrushPointer, out var luminosityBlendCompositionPointer);
                                try
                                {
                                    var colorBlendBrushPointer = CreateEffectBrush(
                                        compositorPointer,
                                        new BlendEffect(22, backgroundSource, foregroundSource));

                                    try
                                    {
                                        var colorBlendBrush = GeneratedComHelpers.ConvertToManaged<ICompositionEffectBrushCom>(colorBlendBrushPointer)
                                            ?? throw new InvalidOperationException("Unable to wrap the color blend brush.");

                                        hr = colorBlendBrush.SetSourceParameter(backgroundName.Handle, luminosityBlendCompositionPointer);
                                        if (hr < 0)
                                        {
                                            Marshal.ThrowExceptionForHR(hr);
                                        }

                                        hr = colorBlendBrush.SetSourceParameter(foregroundName.Handle, tintBrushPointer);
                                        if (hr < 0)
                                        {
                                            Marshal.ThrowExceptionForHR(hr);
                                        }

                                        GeneratedComHelpers.QueryInterface<ICompositionBrushCom>(colorBlendBrushPointer, out var micaBrushPointer);
                                        return micaBrushPointer;
                                    }
                                    finally
                                    {
                                        GeneratedComHelpers.Free<ICompositionEffectBrushCom>(colorBlendBrushPointer);
                                    }
                                }
                                finally
                                {
                                    GeneratedComHelpers.Free<ICompositionBrushCom>(luminosityBlendCompositionPointer);
                                }
                            }
                            finally
                            {
                                GeneratedComHelpers.Free<ICompositionEffectBrushCom>(luminosityBlendBrushPointer);
                            }
                        }
                        finally
                        {
                            GeneratedComHelpers.Free<IGraphicsEffectSourceCom>(foregroundSourcePointer);
                            GeneratedComHelpers.Free<IGraphicsEffectSourceCom>(backgroundSourcePointer);
                        }
                    }
                    finally
                    {
                        GeneratedComHelpers.Free<ICompositionBrushCom>(wallpaperBrushPointer);
                    }
                }
                finally
                {
                    GeneratedComHelpers.Free<ICompositionBrushCom>(luminosityBrushPointer);
                }
            }
            finally
            {
                GeneratedComHelpers.Free<ICompositionBrushCom>(tintBrushPointer);
            }
        }
        finally
        {
            GeneratedComHelpers.Free<ICompositionEffectSourceParameterFactoryCom>(factoryPointer);
        }
    }

    public static nint CreateAcrylicBlurBackdropBrush(nint compositorPointer)
    {
        var factory = WinRTNativeMethods.GetActivationFactory<ICompositionEffectSourceParameterFactoryCom>(
            "Windows.UI.Composition.CompositionEffectSourceParameter",
            out var factoryPointer);

        try
        {
            using var backdropName = new WinRTNativeMethods.HStringInterop("backdrop");
            var backgroundSource = CreateParameterSource(factory, backdropName.Handle, out var backgroundSourcePointer);

            try
            {
                var blurEffect = new WinUIGaussianBlurEffect(backgroundSource);
                using (blurEffect)
                {
                    var effectBrushPointer = CreateEffectBrush(compositorPointer, blurEffect);
                    try
                    {
                        var effectBrush = GeneratedComHelpers.ConvertToManaged<ICompositionEffectBrushCom>(effectBrushPointer)
                            ?? throw new InvalidOperationException("Unable to wrap the acrylic effect brush.");

                        var backdropBrushPointer = CreateBackdropBrush(compositorPointer);
                        try
                        {
                            var hr = effectBrush.SetSourceParameter(backdropName.Handle, backdropBrushPointer);
                            if (hr < 0)
                            {
                                Marshal.ThrowExceptionForHR(hr);
                            }

                            GeneratedComHelpers.QueryInterface<ICompositionBrushCom>(effectBrushPointer, out var compositionBrushPointer);
                            return compositionBrushPointer;
                        }
                        finally
                        {
                            GeneratedComHelpers.Free<ICompositionBrushCom>(backdropBrushPointer);
                        }
                    }
                    finally
                    {
                        GeneratedComHelpers.Free<ICompositionEffectBrushCom>(effectBrushPointer);
                    }
                }
            }
            finally
            {
                GeneratedComHelpers.Free<IGraphicsEffectSourceCom>(backgroundSourcePointer);
            }
        }
        finally
        {
            GeneratedComHelpers.Free<ICompositionEffectSourceParameterFactoryCom>(factoryPointer);
        }
    }

    public static nint CreateRoundedClipGeometry(nint compositorPointer, float? backdropCornerRadius, params nint[] visualPointers)
    {
        if (!backdropCornerRadius.HasValue)
        {
            return nint.Zero;
        }

        var compositor5 = GeneratedComHelpers.QueryInterface<ICompositor5Com>(compositorPointer, out var compositor5Pointer);
        var compositor6 = GeneratedComHelpers.QueryInterface<ICompositor6Com>(compositorPointer, out var compositor6Pointer);

        try
        {
            var hr = compositor5.CreateRoundedRectangleGeometry(out var geometryPointer);
            if (hr < 0 || geometryPointer == nint.Zero)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            try
            {
                var geometry = GeneratedComHelpers.ConvertToManaged<ICompositionRoundedRectangleGeometryCom>(geometryPointer)
                    ?? throw new InvalidOperationException("Unable to wrap the rounded rectangle geometry.");

                hr = geometry.SetCornerRadius(new WinRTVector2 { X = backdropCornerRadius.Value, Y = backdropCornerRadius.Value });
                if (hr < 0)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                GeneratedComHelpers.QueryInterface<ICompositionGeometryCom>(geometryPointer, out var compositionGeometryPointer);
                try
                {
                    hr = compositor6.CreateGeometricClipWithGeometry(compositionGeometryPointer, out var clipPointer);
                    if (hr < 0 || clipPointer == nint.Zero)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    try
                    {
                        foreach (var visualPointer in visualPointers)
                        {
                            if (visualPointer == nint.Zero)
                            {
                                continue;
                            }

                            var visual = GeneratedComHelpers.ConvertToManaged<IVisualCom>(visualPointer)
                                ?? throw new InvalidOperationException("Unable to wrap the clipped visual.");
                            hr = visual.SetClip(clipPointer);
                            if (hr < 0)
                            {
                                Marshal.ThrowExceptionForHR(hr);
                            }
                        }
                    }
                    finally
                    {
                        GeneratedComHelpers.Free<ICompositionClipCom>(clipPointer);
                    }
                }
                finally
                {
                    GeneratedComHelpers.Free<ICompositionGeometryCom>(compositionGeometryPointer);
                }

                return geometryPointer;
            }
            catch
            {
                GeneratedComHelpers.Free<ICompositionRoundedRectangleGeometryCom>(geometryPointer);
                throw;
            }
        }
        finally
        {
            GeneratedComHelpers.Free<ICompositor6Com>(compositor6Pointer);
            GeneratedComHelpers.Free<ICompositor5Com>(compositor5Pointer);
        }
    }

    public static nint CreateBrushVisual(nint compositorPointer, nint compositionBrushPointer, bool visible)
    {
        var compositor = GeneratedComHelpers.ConvertToManaged<ICompositorCom>(compositorPointer)
            ?? throw new InvalidOperationException("Unable to wrap the WinUI compositor.");

        var hr = compositor.CreateSpriteVisual(out var spriteVisualPointer);
        if (hr < 0 || spriteVisualPointer == nint.Zero)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        try
        {
            var spriteVisual = GeneratedComHelpers.ConvertToManaged<ISpriteVisualCom>(spriteVisualPointer)
                ?? throw new InvalidOperationException("Unable to wrap the sprite visual.");
            var visual = GeneratedComHelpers.QueryInterface<IVisualCom>(spriteVisualPointer, out var visualPointer);

            try
            {
                var visual2 = GeneratedComHelpers.QueryInterface<IVisual2Com>(spriteVisualPointer, out var visual2Pointer);
                try
                {
                    hr = spriteVisual.SetBrush(compositionBrushPointer);
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    hr = visual.SetIsVisible(visible ? 1 : 0);
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    hr = visual2.SetRelativeSizeAdjustment(new WinRTVector2 { X = 1f, Y = 1f });
                    if (hr < 0)
                    {
                        Marshal.ThrowExceptionForHR(hr);
                    }

                    return visualPointer;
                }
                finally
                {
                    GeneratedComHelpers.Free<IVisual2Com>(visual2Pointer);
                }
            }
            catch
            {
                GeneratedComHelpers.Free<IVisualCom>(visualPointer);
                throw;
            }
        }
        finally
        {
            GeneratedComHelpers.Free<ISpriteVisualCom>(spriteVisualPointer);
        }
    }

    private static nint CreateBackdropBrush(nint compositorPointer)
    {
        nint backdropBrushPointer;
        if (Win32Platform.WindowsVersion >= WinUiCompositionShared.MinHostBackdropVersion)
        {
            var compositor3 = GeneratedComHelpers.QueryInterface<ICompositor3Com>(compositorPointer, out var compositor3Pointer);
            try
            {
                var hr = compositor3.CreateHostBackdropBrush(out backdropBrushPointer);
                if (hr < 0 || backdropBrushPointer == nint.Zero)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
            finally
            {
                GeneratedComHelpers.Free<ICompositor3Com>(compositor3Pointer);
            }
        }
        else
        {
            var compositor2 = GeneratedComHelpers.QueryInterface<ICompositor2Com>(compositorPointer, out var compositor2Pointer);
            try
            {
                var hr = compositor2.CreateBackdropBrush(out backdropBrushPointer);
                if (hr < 0 || backdropBrushPointer == nint.Zero)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }
            finally
            {
                GeneratedComHelpers.Free<ICompositor2Com>(compositor2Pointer);
            }
        }

        try
        {
            GeneratedComHelpers.QueryInterface<ICompositionBrushCom>(backdropBrushPointer, out var compositionBrushPointer);
            return compositionBrushPointer;
        }
        finally
        {
            GeneratedComHelpers.Free<ICompositionBackdropBrushCom>(backdropBrushPointer);
        }
    }

    private static nint CreateBlurredWallpaperBackdropBrush(nint compositorPointer)
    {
        var compositorWithWallpaperBrush = GeneratedComHelpers.QueryInterface<ICompositorWithBlurredWallpaperBackdropBrushCom>(
            compositorPointer,
            out var compositorWithWallpaperBrushPointer);

        try
        {
            var hr = compositorWithWallpaperBrush.TryCreateBlurredWallpaperBackdropBrush(out var backdropBrushPointer);
            if (hr < 0 || backdropBrushPointer == nint.Zero)
            {
                return nint.Zero;
            }

            try
            {
                GeneratedComHelpers.QueryInterface<ICompositionBrushCom>(backdropBrushPointer, out var compositionBrushPointer);
                return compositionBrushPointer;
            }
            finally
            {
                GeneratedComHelpers.Free<ICompositionBackdropBrushCom>(backdropBrushPointer);
            }
        }
        finally
        {
            GeneratedComHelpers.Free<ICompositorWithBlurredWallpaperBackdropBrushCom>(compositorWithWallpaperBrushPointer);
        }
    }

    private static IGraphicsEffectSourceCom CreateParameterSource(
        ICompositionEffectSourceParameterFactoryCom factory,
        nint nameHandle,
        out nint sourcePointer)
    {
        var hr = factory.Create(nameHandle, out var parameterPointer);
        if (hr < 0 || parameterPointer == nint.Zero)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        try
        {
            return GeneratedComHelpers.QueryInterface<IGraphicsEffectSourceCom>(parameterPointer, out sourcePointer);
        }
        finally
        {
            GeneratedComHelpers.Free<IInspectableCom>(parameterPointer);
        }
    }

    private static nint CreateCompositionBrush(nint compositorPointer, IGraphicsEffectCom effect)
    {
        var effectBrushPointer = CreateEffectBrush(compositorPointer, effect);
        try
        {
            GeneratedComHelpers.QueryInterface<ICompositionBrushCom>(effectBrushPointer, out var compositionBrushPointer);
            return compositionBrushPointer;
        }
        finally
        {
            GeneratedComHelpers.Free<ICompositionEffectBrushCom>(effectBrushPointer);
        }
    }

    private static nint CreateEffectBrush(nint compositorPointer, IGraphicsEffectCom effect)
    {
        var compositor = GeneratedComHelpers.ConvertToManaged<ICompositorCom>(compositorPointer)
            ?? throw new InvalidOperationException("Unable to wrap the WinUI compositor.");

        var effectPointer = GeneratedComHelpers.ConvertToUnmanaged(effect);
        try
        {
            var hr = compositor.CreateEffectFactory(effectPointer, out var effectFactoryPointer);
            if (hr < 0 || effectFactoryPointer == nint.Zero)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            try
            {
                var effectFactory = GeneratedComHelpers.ConvertToManaged<ICompositionEffectFactoryCom>(effectFactoryPointer)
                    ?? throw new InvalidOperationException("Unable to wrap the effect factory.");
                hr = effectFactory.CreateBrush(out var effectBrushPointer);
                if (hr < 0 || effectBrushPointer == nint.Zero)
                {
                    Marshal.ThrowExceptionForHR(hr);
                }

                return effectBrushPointer;
            }
            finally
            {
                GeneratedComHelpers.Free<ICompositionEffectFactoryCom>(effectFactoryPointer);
            }
        }
        finally
        {
            GeneratedComHelpers.Free<IGraphicsEffectCom>(effectPointer);
        }
    }
}