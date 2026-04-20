using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Direct2D1.Interop;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using FontMetrics = Avalonia.Media.FontMetrics;
using FontSimulations = Avalonia.Media.FontSimulations;
using GlyphMetrics = Avalonia.Media.GlyphMetrics;

namespace Avalonia.Direct2D1.Media;

internal class GlyphTypefaceImpl : IPlatformTypeface, ITextShaperTypeface
{
    private bool _isDisposed;
    private readonly IDisposable? _resourceOwner;

    public GlyphTypefaceImpl(DWriteFont font, IDisposable? resourceOwner = null)
    {
        _resourceOwner = resourceOwner;
        Font = font;
        FontFace = font.CreateFontFace();

        var fontMetrics = FontFace.Metrics;

        Metrics = new FontMetrics
        {
            DesignEmHeight = fontMetrics.DesignUnitsPerEm,
            Ascent = fontMetrics.Ascent,
            Descent = fontMetrics.Descent,
            LineGap = fontMetrics.LineGap,
            UnderlinePosition = fontMetrics.UnderlinePosition,
            UnderlineThickness = fontMetrics.UnderlineThickness,
            StrikethroughPosition = fontMetrics.StrikethroughPosition,
            StrikethroughThickness = fontMetrics.StrikethroughThickness,
            IsFixedPitch = false
        };

        FamilyName = font.FamilyName;
        Weight = (FontWeight)font.Weight;
        Style = (FontStyle)font.Style;
        Stretch = (FontStretch)font.Stretch;
    }

    private static uint SwapBytes(uint x)
    {
        x = (x >> 16) | (x << 16);
        return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
    }

    public DWriteFont Font { get; }

    public DWriteFontFace FontFace { get; }

    public FontMetrics Metrics { get; }

    public FontSimulations FontSimulations => FontSimulations.None;

    public string FamilyName { get; }

    public FontWeight Weight { get; }

    public FontStyle Style { get; }

    public FontStretch Stretch { get; }

    public ushort GetGlyph(uint codepoint)
    {
        return FontFace.GetGlyphIndices(new[] { codepoint })[0];
    }

    public bool TryGetGlyph(uint codepoint, out ushort glyph)
    {
        glyph = GetGlyph(codepoint);
        return glyph != 0;
    }

    public ushort[] GetGlyphs(ReadOnlySpan<uint> codepoints)
    {
        var codePoints = new uint[codepoints.Length];
        codepoints.CopyTo(codePoints);
        return FontFace.GetGlyphIndices(codePoints);
    }

    public int GetGlyphAdvance(ushort glyph)
    {
        return GetGlyphAdvances(new[] { glyph })[0];
    }

    public int[] GetGlyphAdvances(ReadOnlySpan<ushort> glyphs)
    {
        var glyphIndices = new ushort[glyphs.Length];
        glyphs.CopyTo(glyphIndices);
        var metrics = FontFace.GetDesignGlyphMetrics(glyphIndices, isSideways: false);
        var advances = new int[glyphIndices.Length];

        for (var i = 0; i < metrics.Length; i++)
        {
            advances[i] = unchecked((int)metrics[i].AdvanceWidth);
        }

        return advances;
    }

    public bool TryGetGlyphMetrics(ushort glyph, out GlyphMetrics metrics)
    {
        metrics = default;
        var designMetrics = FontFace.GetDesignGlyphMetrics(new[] { glyph }, isSideways: false);

        if (designMetrics.Length == 0)
        {
            return false;
        }

        var glyphMetrics = designMetrics[0];
        var width = (int)glyphMetrics.AdvanceWidth - glyphMetrics.LeftSideBearing - glyphMetrics.RightSideBearing;
        var height = (int)glyphMetrics.AdvanceHeight - glyphMetrics.TopSideBearing - glyphMetrics.BottomSideBearing;

        metrics = new GlyphMetrics
        {
            XBearing = glyphMetrics.LeftSideBearing,
            YBearing = glyphMetrics.VerticalOriginY - glyphMetrics.TopSideBearing,
            Width = (ushort)Math.Abs(width),
            Height = (ushort)Math.Abs(height)
        };

        return true;
    }

    private void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (!disposing)
        {
            return;
        }

        FontFace.Dispose();
        Font.Dispose();
        _resourceOwner?.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool TryGetTable(OpenTypeTag tag, out ReadOnlyMemory<byte> table)
    {
        table = default;

        if (!FontFace.TryGetFontTable(SwapBytes((uint)tag), out var data, out var size, out var tableContext))
        {
            return false;
        }

        try
        {
            var bytes = new byte[size];
            Marshal.Copy(data, bytes, 0, bytes.Length);
            table = bytes;
            return true;
        }
        finally
        {
            FontFace.ReleaseFontTable(tableContext);
        }
    }

    public bool TryGetStream([NotNullWhen(true)] out Stream? stream)
    {
        stream = null;
        return false;
    }
}
