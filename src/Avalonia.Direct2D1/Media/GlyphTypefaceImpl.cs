using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using SharpDX.DirectWrite;
using FontMetrics = Avalonia.Media.FontMetrics;
using FontSimulations = Avalonia.Media.FontSimulations;
using GlyphMetrics = Avalonia.Media.GlyphMetrics;

namespace Avalonia.Direct2D1.Media
{
    internal class GlyphTypefaceImpl : IPlatformTypeface, ITextShaperTypeface
    {
        private bool _isDisposed;

        public GlyphTypefaceImpl(SharpDX.DirectWrite.Font font)
        {
            DWFont = font;

            FontFace = new FontFace(DWFont).QueryInterface<FontFace1>();
            var fontMetrics = FontFace.Metrics;

            Metrics = new FontMetrics
            {
                DesignEmHeight = (ushort)fontMetrics.DesignUnitsPerEm,
                Ascent = fontMetrics.Ascent,
                Descent = fontMetrics.Descent,
                LineGap = fontMetrics.LineGap,
                UnderlinePosition = fontMetrics.UnderlinePosition,
                UnderlineThickness = fontMetrics.UnderlineThickness,
                StrikethroughPosition = fontMetrics.StrikethroughPosition,
                StrikethroughThickness = fontMetrics.StrikethroughThickness,
                IsFixedPitch = FontFace.IsMonospacedFont
            };

            FamilyName = DWFont.FontFamily.FamilyNames.GetString(0);

            Weight = (Avalonia.Media.FontWeight)DWFont.Weight;

            Style = (Avalonia.Media.FontStyle)DWFont.Style;

            Stretch = (Avalonia.Media.FontStretch)DWFont.Stretch;
        }

        private static uint SwapBytes(uint x)
        {
            x = (x >> 16) | (x << 16);

            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }

        public SharpDX.DirectWrite.Font DWFont { get; }

        public FontFace1 FontFace { get; }

        public FontMetrics Metrics { get; }

        public FontSimulations FontSimulations => FontSimulations.None;

        public string FamilyName { get; }

        public Avalonia.Media.FontWeight Weight { get; }

        public Avalonia.Media.FontStyle Style { get; }

        public Avalonia.Media.FontStretch Stretch { get; }

        /// <inheritdoc cref="GlyphTypeface"/>
        public ushort GetGlyph(uint codepoint)
        {
            return unchecked((ushort)FontFace.GetGlyphIndices(new[] { (int)codepoint })[0]);
        }

        public bool TryGetGlyph(uint codepoint, out ushort glyph)
        {
            glyph = GetGlyph(codepoint);

            return glyph != 0;
        }

        /// <inheritdoc cref="GlyphTypeface"/>
        public ushort[] GetGlyphs(ReadOnlySpan<uint> codepoints)
        {
            var codePoints = new int[codepoints.Length];

            for (var i = 0; i < codepoints.Length; i++)
            {
                codePoints[i] = (int)codepoints[i];
            }

            var glyphIndices = FontFace.GetGlyphIndices(codePoints);
            var glyphs = new ushort[glyphIndices.Length];

            for (var i = 0; i < glyphIndices.Length; i++)
            {
                glyphs[i] = unchecked((ushort)glyphIndices[i]);
            }

            return glyphs;
        }

        /// <inheritdoc cref="GlyphTypeface"/>
        public int GetGlyphAdvance(ushort glyph)
        {
            return GetGlyphAdvances(new[] { glyph })[0];
        }

        /// <inheritdoc cref="GlyphTypeface"/>
        public int[] GetGlyphAdvances(ReadOnlySpan<ushort> glyphs)
        {
            var glyphIndices = new short[glyphs.Length];
            var glyphAdvances = new int[glyphs.Length];

            for (var i = 0; i < glyphs.Length; i++)
            {
                glyphIndices[i] = unchecked((short)glyphs[i]);
            }

            FontFace.GetDesignGlyphAdvances(glyphIndices.Length, glyphIndices, glyphAdvances, false);

            return glyphAdvances;
        }

        public bool TryGetGlyphMetrics(ushort glyph, out GlyphMetrics metrics)
        {
            metrics = default;
            var designMetrics = FontFace.GetDesignGlyphMetrics(new[] { unchecked((short)glyph) }, false);

            if (designMetrics.Length == 0)
                return false;

            var glyphMetrics = designMetrics[0];
            var width = glyphMetrics.AdvanceWidth - glyphMetrics.LeftSideBearing - glyphMetrics.RightSideBearing;
            var height = glyphMetrics.AdvanceHeight - glyphMetrics.TopSideBearing - glyphMetrics.BottomSideBearing;

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

            FontFace?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool TryGetTable(OpenTypeTag tag, out ReadOnlyMemory<byte> table)
        {
            table = default;
            if (FontFace.TryGetFontTable((int)SwapBytes((uint)tag), out var data, out var tableContext))
            {
                try
                {
                    var bytes = new byte[data.Size];
                    Marshal.Copy(data.Pointer, bytes, 0, bytes.Length);
                    table = bytes;
                    return true;
                }
                finally
                {
                    FontFace.ReleaseFontTable(tableContext);
                }
            }

            return false;
        }

        public bool TryGetStream([NotNullWhen(true)] out Stream? stream)
        {
            stream = null;
            return false;
        }
    }
}
