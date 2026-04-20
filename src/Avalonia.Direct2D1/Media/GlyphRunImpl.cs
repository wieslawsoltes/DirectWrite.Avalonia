using System;
using System.Collections.Generic;
using Avalonia.Direct2D1.Interop;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;

#nullable enable

namespace Avalonia.Direct2D1.Media;

internal class GlyphRunImpl : IGlyphRunImpl
{
    private readonly GlyphTypefaceImpl _glyphTypefaceImpl;
    private readonly GlyphTypeface _glyphTypeface;

    private readonly ushort[] _glyphIndices;
    private readonly float[] _glyphAdvances;
    private readonly DWRITE_GLYPH_OFFSET[] _glyphOffsets;

    public GlyphRunImpl(GlyphTypeface glyphTypeface, double fontRenderingEmSize, IReadOnlyList<GlyphInfo> glyphInfos, Point baselineOrigin)
    {
        _glyphTypeface = glyphTypeface;
        _glyphTypefaceImpl = glyphTypeface.PlatformTypeface as GlyphTypefaceImpl
            ?? throw new NotSupportedException("GlyphTypeface must be backed by GlyphTypefaceImpl.");

        FontRenderingEmSize = fontRenderingEmSize;
        BaselineOrigin = baselineOrigin;

        var glyphCount = glyphInfos.Count;
        _glyphIndices = new ushort[glyphCount];

        for (var i = 0; i < glyphCount; i++)
        {
            _glyphIndices[i] = glyphInfos[i].GlyphIndex;
        }

        _glyphAdvances = new float[glyphCount];

        var width = 0.0;

        for (var i = 0; i < glyphCount; i++)
        {
            var advance = glyphInfos[i].GlyphAdvance;
            width += advance;
            _glyphAdvances[i] = (float)advance;
        }

        _glyphOffsets = new DWRITE_GLYPH_OFFSET[glyphCount];

        var runBounds = new Rect();
        var currentX = 0.0;
        var scale = fontRenderingEmSize / glyphTypeface.Metrics.DesignEmHeight;

        for (var i = 0; i < glyphCount; i++)
        {
            var (x, y) = glyphInfos[i].GlyphOffset;

            _glyphOffsets[i] = new DWRITE_GLYPH_OFFSET
            {
                AdvanceOffset = (float)x,
                AscenderOffset = (float)y
            };

            if (_glyphTypefaceImpl.TryGetGlyphMetrics(glyphInfos[i].GlyphIndex, out var metrics))
            {
                var ybearing = metrics.YBearing;
                var height = (double)metrics.Height;
                var xOffset = metrics.XBearing * scale;
                var xWidth = xOffset > 0 ? xOffset : 0;
                var xBearing = xOffset < 0 ? xOffset : 0;
                runBounds = runBounds.Union(new Rect(
                    currentX + xBearing,
                    baselineOrigin.Y + ybearing,
                    xWidth + metrics.Width * scale,
                    height * scale));
            }

            currentX += glyphInfos[i].GlyphAdvance;
        }

        Bounds = runBounds.Translate(new Vector(baselineOrigin.X, 0));
    }

    internal GlyphTypeface GlyphTypeface => _glyphTypeface;
    internal DWriteFontFace FontFace => _glyphTypefaceImpl.FontFace;
    internal ushort[] GlyphIndices => _glyphIndices;
    internal float[] GlyphAdvances => _glyphAdvances;
    internal DWRITE_GLYPH_OFFSET[] GlyphOffsets => _glyphOffsets;

    public double FontRenderingEmSize { get; }

    public Point BaselineOrigin { get; }

    public Rect Bounds { get; }

    public IReadOnlyList<float> GetIntersections(float lowerBound, float upperBound) => Array.Empty<float>();

    public void Dispose()
    {
    }
}
