using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Avalonia.Direct2D1.Media;
using Avalonia.Direct2D1.Media.Imaging;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using Avalonia.Platform.Surfaces;
using GlyphRun = Avalonia.Media.GlyphRun;
using SharpDX.Mathematics.Interop;

namespace Avalonia
{
    public static class Direct2DApplicationExtensions
    {
        public static AppBuilder UseDirect2D1(this AppBuilder builder, Direct2D1.Direct2D1Options? options = null)
        {
            if (options != null)
            {
                builder.With(options);
            }

            builder
                .UseTextShapingSubsystem(Direct2D1.Direct2D1Platform.InitializeTextShaping, "DirectWrite")
                .UseRenderingSubsystem(Direct2D1.Direct2D1Platform.Initialize, "Direct2D1");
            return builder;
        }
    }
}

namespace Avalonia.Direct2D1
{
    internal class Direct2D1Platform : IPlatformRenderInterface
    {
        private static readonly Direct2D1Platform s_instance = new Direct2D1Platform();
        internal static Direct2D1Options Options { get; private set; } = new Direct2D1Options();

        public static SharpDX.Direct3D11.Device Direct3D11Device { get; private set; } = null!;

        public static SharpDX.Direct2D1.Factory1 Direct2D1Factory { get; private set; } = null!;

        public static SharpDX.Direct2D1.Device Direct2D1Device { get; private set; } = null!;

        public static SharpDX.DirectWrite.Factory1 DirectWriteFactory { get; private set; } = null!;

        public static SharpDX.DirectWrite.TextAnalyzer DirectWriteTextAnalyzer { get; private set; } = null!;

        public static SharpDX.WIC.ImagingFactory ImagingFactory { get; private set; } = null!;

        public static SharpDX.DXGI.Device1 DxgiDevice { get; private set; } = null!;

        private static readonly object s_initLock = new object();
        private static bool s_initialized = false;

        private static Direct2D1Options ResolveOptions()
        {
            return AvaloniaLocator.Current.GetService<Direct2D1Options>() ?? new Direct2D1Options();
        }

        private static SharpDX.Direct2D1.Factory1 CreateFactory(Direct2D1Options options)
        {
            if (options.EnableDiagnostics)
            {
                try
                {
                    return new SharpDX.Direct2D1.Factory1(
                        SharpDX.Direct2D1.FactoryType.MultiThreaded,
                        SharpDX.Direct2D1.DebugLevel.Information);
                }
                catch
                {
                    // Some systems don't have the debug layer installed. Retry without it.
                }
            }

#if DEBUG
            if (Debugger.IsAttached)
            {
                try
                {
                    return new SharpDX.Direct2D1.Factory1(
                        SharpDX.Direct2D1.FactoryType.MultiThreaded,
                        SharpDX.Direct2D1.DebugLevel.Error);
                }
                catch
                {
                    // ignore, retry below without the debug layer
                }
            }
#endif

            return new SharpDX.Direct2D1.Factory1(
                SharpDX.Direct2D1.FactoryType.MultiThreaded,
                SharpDX.Direct2D1.DebugLevel.None);
        }

        private static SharpDX.Direct3D11.Device CreateD3D11Device(
            Direct2D1Options options,
            SharpDX.Direct3D.FeatureLevel[] featureLevels)
        {
            var flags = SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport |
                        SharpDX.Direct3D11.DeviceCreationFlags.VideoSupport;

            if (options.EnableDiagnostics)
            {
                try
                {
                    return CreateD3D11Device(options, featureLevels, flags | SharpDX.Direct3D11.DeviceCreationFlags.Debug);
                }
                catch (SharpDX.SharpDXException)
                {
                    // Some systems don't have the Direct3D debug layer installed. Retry without it.
                }
            }

            return CreateD3D11Device(options, featureLevels, flags);
        }

        private static SharpDX.Direct3D11.Device CreateD3D11Device(
            Direct2D1Options options,
            SharpDX.Direct3D.FeatureLevel[] featureLevels,
            SharpDX.Direct3D11.DeviceCreationFlags flags)
        {
            if (options.UseHardwareAcceleration)
            {
                try
                {
                    return new SharpDX.Direct3D11.Device(
                        SharpDX.Direct3D.DriverType.Hardware,
                        flags,
                        featureLevels);
                }
                catch (SharpDX.SharpDXException) when (options.UseWarpFallback)
                {
                    // Fall back to WARP when requested.
                }
            }

            if (!options.UseHardwareAcceleration || options.UseWarpFallback)
            {
                return new SharpDX.Direct3D11.Device(
                    SharpDX.Direct3D.DriverType.Warp,
                    flags,
                    featureLevels);
            }

            throw new InvalidOperationException(
                "Failed to create a Direct3D11 hardware device and WARP fallback is disabled.");
        }

        internal static void InitializeDirect2D()
        {
            lock (s_initLock)
            {
                if (s_initialized)
                {
                    return;
                }

                Options = ResolveOptions();
                Direct2D1Factory = CreateFactory(Options);

                using (var factory = new SharpDX.DirectWrite.Factory())
                {
                    DirectWriteFactory = factory.QueryInterface<SharpDX.DirectWrite.Factory1>();
                }

                DirectWriteTextAnalyzer = new SharpDX.DirectWrite.TextAnalyzer(DirectWriteFactory);

                ImagingFactory = new SharpDX.WIC.ImagingFactory();

                var featureLevels = new[]
                {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                    SharpDX.Direct3D.FeatureLevel.Level_9_3,
                    SharpDX.Direct3D.FeatureLevel.Level_9_2,
                    SharpDX.Direct3D.FeatureLevel.Level_9_1,
                };

                Direct3D11Device = CreateD3D11Device(Options, featureLevels);

                DxgiDevice = Direct3D11Device.QueryInterface<SharpDX.DXGI.Device1>();

                Direct2D1Device = new SharpDX.Direct2D1.Device(Direct2D1Factory, DxgiDevice);

                s_initialized = true;
            }
        }

        public static void InitializeTextShaping()
        {
            InitializeDirect2D();
            AvaloniaLocator.CurrentMutable
                .Bind<IFontManagerImpl>().ToConstant(new FontManagerImpl())
                .Bind<ITextShaperImpl>().ToConstant(new TextShaperImpl());
            SharpDX.Configuration.EnableReleaseOnFinalizer = true;
        }

        public static void Initialize()
        {
            InitializeTextShaping();
            AvaloniaLocator.CurrentMutable
                .Bind<IPlatformRenderInterface>().ToConstant(s_instance);
        }

        private IRenderTarget CreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces)
        {
            foreach (var s in surfaces)
            {
                if (s is INativePlatformHandleSurface nativeWindow)
                {
                    if (nativeWindow.HandleDescriptor != "HWND")
                    {
                        throw new NotSupportedException("Don't know how to create a Direct2D1 renderer from " +
                                                        nativeWindow.HandleDescriptor);
                    }

                    return new HwndRenderTarget(nativeWindow);
                }
                if (s is IExternalDirect2DRenderTargetSurface external)
                {
                    return new ExternalRenderTarget(external);
                }

                if (s is IFramebufferPlatformSurface fb)
                {
                    return new FramebufferShimRenderTarget(fb);
                }
            }
            throw new NotSupportedException("Don't know how to create a Direct2D1 renderer from any of provided surfaces");
        }

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(PixelSize size, Vector dpi)
        {
            return new WicRenderTargetBitmapImpl(size, dpi);
        }

        public IWriteableBitmapImpl CreateWriteableBitmap(PixelSize size, Vector dpi, PixelFormat format, AlphaFormat alphaFormat)
        {
            return new WriteableWicBitmapImpl(size, dpi, format, alphaFormat);
        }

        public IGeometryImpl CreateEllipseGeometry(Rect rect) => new EllipseGeometryImpl(rect);
        public IGeometryImpl CreateLineGeometry(Point p1, Point p2) => new LineGeometryImpl(p1, p2);
        public IGeometryImpl CreateRectangleGeometry(Rect rect) => new RectangleGeometryImpl(rect);
        public IStreamGeometryImpl CreateStreamGeometry() => new StreamGeometryImpl();
        public IGeometryImpl CreateGeometryGroup(FillRule fillRule, IReadOnlyList<IGeometryImpl> children) => new GeometryGroupImpl(fillRule, children);
        public IGeometryImpl CreateCombinedGeometry(GeometryCombineMode combineMode, IGeometryImpl g1, IGeometryImpl g2) => new CombinedGeometryImpl(combineMode, g1, g2);
        public IGlyphRunImpl CreateGlyphRun(GlyphTypeface glyphTypeface, double fontRenderingEmSize, IReadOnlyList<GlyphInfo> glyphInfos, Point baselineOrigin)
        {
            return new GlyphRunImpl(glyphTypeface, fontRenderingEmSize, glyphInfos, baselineOrigin);
        }

        class D2DApi : IPlatformRenderInterfaceContext
        {
            private readonly Direct2D1Platform _platform;

            public D2DApi(Direct2D1Platform platform)
            {
                _platform = platform;
            }
            public object? TryGetFeature(Type featureType) => null;

            public void Dispose()
            {
            }

            public IRenderTarget CreateRenderTarget(IEnumerable<IPlatformRenderSurface> surfaces) => _platform.CreateRenderTarget(surfaces);

            public IDrawingContextLayerImpl CreateOffscreenRenderTarget(PixelSize pixelSize, Vector scaling, bool enableTextAntialiasing) =>
                new WicRenderTargetBitmapImpl(pixelSize, scaling * 96);

            public bool IsLost => false;
            public IReadOnlyDictionary<Type, object> PublicFeatures { get; } = new Dictionary<Type, object>();
            public PixelSize? MaxOffscreenRenderTargetPixelSize => null;
        }

        public IPlatformRenderInterfaceContext CreateBackendContext(IPlatformGraphicsContext? graphicsContext) =>
            new D2DApi(this);

        public IGeometryImpl BuildGlyphRunGeometry(GlyphRun glyphRun)
        {
            if (glyphRun.GlyphTypeface.PlatformTypeface is not GlyphTypefaceImpl glyphTypeface)
            {
                throw new InvalidOperationException("PlatformImpl can't be null.");
            }

            var pathGeometry = new SharpDX.Direct2D1.PathGeometry(Direct2D1Factory);

            using (var sink = pathGeometry.Open())
            {
                var glyphInfos = glyphRun.GlyphInfos;
                var glyphs = new short[glyphInfos.Count];

                for (int i = 0; i < glyphInfos.Count; i++)
                {
                    glyphs[i] = (short)glyphInfos[i].GlyphIndex;
                }

                glyphTypeface.FontFace.GetGlyphRunOutline((float)glyphRun.FontRenderingEmSize, glyphs, null, null, false, !glyphRun.IsLeftToRight, sink);

                sink.Close();
            }

            var (baselineOriginX, baselineOriginY) = glyphRun.BaselineOrigin;

            var transformedGeometry = new SharpDX.Direct2D1.TransformedGeometry(
                Direct2D1Factory,
                pathGeometry,
                new RawMatrix3x2(1.0f, 0.0f, 0.0f, 1.0f, (float)baselineOriginX, (float)baselineOriginY));

            return new TransformedGeometryWrapper(transformedGeometry);
        }

        private class TransformedGeometryWrapper : GeometryImpl
        {
            public TransformedGeometryWrapper(SharpDX.Direct2D1.TransformedGeometry geometry) : base(geometry)
            {

            }
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(string fileName)
        {
            return new WicBitmapImpl(fileName);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(Stream stream)
        {
            return new WicBitmapImpl(stream);
        }

        public IWriteableBitmapImpl LoadWriteableBitmapToWidth(Stream stream, int width,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WriteableWicBitmapImpl(stream, width, true, interpolationMode);
        }

        public IWriteableBitmapImpl LoadWriteableBitmapToHeight(Stream stream, int height,
            BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WriteableWicBitmapImpl(stream, height, false, interpolationMode);
        }

        public IWriteableBitmapImpl LoadWriteableBitmap(string fileName)
        {
            return new WriteableWicBitmapImpl(fileName);
        }

        public IWriteableBitmapImpl LoadWriteableBitmap(Stream stream)
        {
            return new WriteableWicBitmapImpl(stream);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmapToWidth(Stream stream, int width, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WicBitmapImpl(stream, width, true, interpolationMode);
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmapToHeight(Stream stream, int height, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            return new WicBitmapImpl(stream, height, false, interpolationMode);
        }

        /// <inheritdoc />
        public IBitmapImpl ResizeBitmap(IBitmapImpl bitmapImpl, PixelSize destinationSize, BitmapInterpolationMode interpolationMode = BitmapInterpolationMode.HighQuality)
        {
            // https://github.com/sharpdx/SharpDX/issues/959 blocks implementation.
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IBitmapImpl LoadBitmap(PixelFormat format, AlphaFormat alphaFormat, IntPtr data, PixelSize size, Vector dpi, int stride)
        {
            return new WicBitmapImpl(format, alphaFormat, data, size, dpi, stride);
        }

        public bool SupportsIndividualRoundRects => false;

        public AlphaFormat DefaultAlphaFormat => AlphaFormat.Premul;

        public PixelFormat DefaultPixelFormat => PixelFormat.Bgra8888;
        public bool IsSupportedBitmapPixelFormat(PixelFormat format) =>
            format == PixelFormats.Bgra8888 
            || format == PixelFormats.Rgba8888;

        public bool SupportsRegions => false;
        public IPlatformRenderInterfaceRegion CreateRegion() => throw new NotSupportedException();
    }
}
