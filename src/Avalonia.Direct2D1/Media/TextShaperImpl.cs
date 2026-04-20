using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;
using DWriteFontFeature = SharpDX.DirectWrite.FontFeature;
using DWriteFontFeatureTag = SharpDX.DirectWrite.FontFeatureTag;
using DWriteGlyphOffset = SharpDX.DirectWrite.GlyphOffset;
using DWriteNumberSubstitution = SharpDX.DirectWrite.NumberSubstitution;
using DWriteReadingDirection = SharpDX.DirectWrite.ReadingDirection;
using DWriteScriptAnalysis = SharpDX.DirectWrite.ScriptAnalysis;
using DWriteShapingGlyphProperties = SharpDX.DirectWrite.ShapingGlyphProperties;
using DWriteShapingTextProperties = SharpDX.DirectWrite.ShapingTextProperties;
using DWriteTextAnalysisSink = SharpDX.DirectWrite.TextAnalysisSink;
using DWriteTextAnalysisSource = SharpDX.DirectWrite.TextAnalysisSource;

namespace Avalonia.Direct2D1.Media
{
    internal class TextShaperImpl : ITextShaperImpl
    {
        public ShapedBuffer ShapeText(ReadOnlyMemory<char> text, TextShaperOptions options)
        {
            if (text.Length == 0)
            {
                return new ShapedBuffer(text, 0, options.GlyphTypeface, options.FontRenderingEmSize, options.BidiLevel);
            }

            if (options.GlyphTypeface.PlatformTypeface is not GlyphTypefaceImpl typeface)
            {
                throw new NotSupportedException("The provided GlyphTypeface is not supported by this text shaper.");
            }

            var usedCulture = options.Culture ?? CultureInfo.CurrentCulture;
            var textString = text.ToString();
            var analysisSource = new TextAnalysisSourceImpl(textString, usedCulture, options.BidiLevel);
            var analysisSink = new TextAnalysisSinkImpl(textString.Length, options.BidiLevel);
            var analyzer = Direct2D1Platform.DirectWriteTextAnalyzer;

            analyzer.AnalyzeScript(analysisSource, 0, textString.Length, analysisSink);
            analyzer.AnalyzeBidi(analysisSource, 0, textString.Length, analysisSink);
            analyzer.AnalyzeNumberSubstitution(analysisSource, 0, textString.Length, analysisSink);

            var runShapes = new List<RunShapeResult>();
            var totalGlyphCount = 0;

            foreach (var run in analysisSink.GetRuns())
            {
                if (run.Length == 0)
                {
                    continue;
                }

                var shapedRun = ShapeRun(analyzer, typeface, textString, run, options, usedCulture);
                totalGlyphCount += shapedRun.GlyphCount;
                runShapes.Add(shapedRun);
            }

            var shapedBuffer = new ShapedBuffer(
                text,
                totalGlyphCount,
                options.GlyphTypeface,
                options.FontRenderingEmSize,
                options.BidiLevel);

            var bufferIndex = 0;

            foreach (var run in runShapes)
            {
                for (var i = 0; i < run.GlyphCount; i++)
                {
                    var glyphIndex = (ushort)run.GlyphIndices[i];
                    var glyphCluster = run.GlyphClusters[i];
                    var glyphAdvance = run.GlyphAdvances[i] + options.LetterSpacing;
                    var glyphOffset = new Vector(
                        run.GlyphOffsets[i].AdvanceOffset,
                        -run.GlyphOffsets[i].AscenderOffset);

                    if ((uint)glyphCluster < (uint)textString.Length && textString[glyphCluster] == '\t')
                    {
                        glyphIndex = typeface.GetGlyph(' ');

                        if (options.IncrementalTabWidth > 0)
                        {
                            glyphAdvance = options.IncrementalTabWidth;
                        }
                        else if (options.GlyphTypeface.TryGetHorizontalGlyphAdvance(glyphIndex, out var advance))
                        {
                            glyphAdvance = 4 * advance * (options.FontRenderingEmSize / options.GlyphTypeface.Metrics.DesignEmHeight);
                        }
                    }

                    shapedBuffer[bufferIndex++] =
                        new Avalonia.Media.TextFormatting.GlyphInfo(glyphIndex, glyphCluster, glyphAdvance, glyphOffset);
                }
            }

            return shapedBuffer;
        }

        private static RunShapeResult ShapeRun(
            SharpDX.DirectWrite.TextAnalyzer analyzer,
            GlyphTypefaceImpl typeface,
            string text,
            TextAnalysisRun run,
            TextShaperOptions options,
            CultureInfo culture)
        {
            var runText = text.Substring(run.Start, run.Length);
            var features = CreateFeatureRanges(options.FontFeatures, run.Length);
            var maxGlyphCount = GetMaxGlyphCount(run.Length);

            while (true)
            {
                var clusterMap = new short[run.Length];
                var textProps = new DWriteShapingTextProperties[run.Length];
                var glyphIndices = new short[maxGlyphCount];
                var glyphProps = new DWriteShapingGlyphProperties[maxGlyphCount];

                try
                {
                    analyzer.GetGlyphs(
                        runText,
                        run.Length,
                        typeface.FontFace,
                        false,
                        run.IsRightToLeft,
                        run.ScriptAnalysis,
                        culture.Name,
                        run.NumberSubstitution,
                        features?.Features,
                        features?.FeatureRangeLengths,
                        features?.FeatureRanges ?? 0,
                        clusterMap,
                        textProps,
                        glyphIndices,
                        glyphProps,
                        out var actualGlyphCount);

                    Array.Resize(ref glyphIndices, actualGlyphCount);
                    Array.Resize(ref glyphProps, actualGlyphCount);

                    var glyphAdvances = new float[actualGlyphCount];
                    var glyphOffsets = new DWriteGlyphOffset[actualGlyphCount];

                    analyzer.GetGlyphPlacements(
                        runText,
                        clusterMap,
                        textProps,
                        run.Length,
                        glyphIndices,
                        glyphProps,
                        actualGlyphCount,
                        typeface.FontFace,
                        (float)options.FontRenderingEmSize,
                        false,
                        run.IsRightToLeft,
                        run.ScriptAnalysis,
                        culture.Name,
                        features?.Features,
                        features?.FeatureRangeLengths,
                        glyphAdvances,
                        glyphOffsets);

                    return new RunShapeResult(
                        actualGlyphCount,
                        glyphIndices,
                        glyphAdvances,
                        glyphOffsets,
                        BuildGlyphClusters(clusterMap, actualGlyphCount, run.Start));
                }
                catch (SharpDX.SharpDXException) when (maxGlyphCount < run.Length * 8 + 32)
                {
                    maxGlyphCount *= 2;
                }
            }
        }

        private static int[] BuildGlyphClusters(short[] clusterMap, int glyphCount, int runStart)
        {
            var clusterStarts = new SortedDictionary<int, int>();

            for (var charIndex = 0; charIndex < clusterMap.Length; charIndex++)
            {
                var glyphStart = clusterMap[charIndex];

                if (!clusterStarts.ContainsKey(glyphStart))
                {
                    clusterStarts[glyphStart] = runStart + charIndex;
                }
            }

            var glyphClusters = new int[glyphCount];
            var orderedStarts = new List<KeyValuePair<int, int>>(clusterStarts);

            for (var i = 0; i < orderedStarts.Count; i++)
            {
                var start = orderedStarts[i].Key;
                var end = i + 1 < orderedStarts.Count ? orderedStarts[i + 1].Key : glyphCount;

                for (var glyphIndex = start; glyphIndex < end && glyphIndex < glyphCount; glyphIndex++)
                {
                    glyphClusters[glyphIndex] = orderedStarts[i].Value;
                }
            }

            return glyphClusters;
        }

        private static FeatureRange? CreateFeatureRanges(IReadOnlyList<FontFeature>? fontFeatures, int textLength)
        {
            if (fontFeatures is null || fontFeatures.Count == 0)
            {
                return null;
            }

            var features = new DWriteFontFeature[fontFeatures.Count];

            for (var i = 0; i < fontFeatures.Count; i++)
            {
                var feature = fontFeatures[i];
                features[i] = new DWriteFontFeature(ToDirectWriteTag(feature.Tag), feature.Value);
            }

            return new FeatureRange(new[] { features }, new[] { textLength }, 1);
        }

        private static DWriteFontFeatureTag ToDirectWriteTag(string tag)
        {
            if (tag.Length != 4)
            {
                throw new InvalidOperationException($"OpenType feature tag '{tag}' must contain exactly four characters.");
            }

            var value =
                (uint)tag[0] |
                ((uint)tag[1] << 8) |
                ((uint)tag[2] << 16) |
                ((uint)tag[3] << 24);

            return (DWriteFontFeatureTag)value;
        }

        private static int GetMaxGlyphCount(int textLength)
        {
            return Math.Max(16, textLength * 3 / 2 + 16);
        }

        public ITextShaperTypeface CreateTypeface(GlyphTypeface glyphTypeface)
        {
            return glyphTypeface.PlatformTypeface as GlyphTypefaceImpl
                ?? throw new NotSupportedException("The provided GlyphTypeface is not supported by this text shaper.");
        }

        private sealed class TextAnalysisSourceImpl : SharpDX.CallbackBase, DWriteTextAnalysisSource
        {
            private readonly string _text;
            private readonly string _localeName;

            public TextAnalysisSourceImpl(string text, CultureInfo culture, sbyte bidiLevel)
            {
                _text = text;
                _localeName = culture.Name;
                ReadingDirection = (bidiLevel & 1) == 0
                    ? DWriteReadingDirection.LeftToRight
                    : DWriteReadingDirection.RightToLeft;
            }

            public string GetTextAtPosition(int textPosition)
            {
                return textPosition >= _text.Length ? string.Empty : _text.Substring(textPosition);
            }

            public string GetTextBeforePosition(int textPosition)
            {
                return textPosition <= 0 ? string.Empty : _text.Substring(0, textPosition);
            }

            public DWriteReadingDirection ReadingDirection { get; }

            public string GetLocaleName(int textPosition, out int textLength)
            {
                textLength = _text.Length - textPosition;
                return _localeName;
            }

            public DWriteNumberSubstitution GetNumberSubstitution(int textPosition, out int textLength)
            {
                textLength = _text.Length - textPosition;
                return null!;
            }
        }

        private sealed class TextAnalysisSinkImpl : SharpDX.CallbackBase, DWriteTextAnalysisSink
        {
            private readonly DWriteScriptAnalysis[] _scripts;
            private readonly byte[] _bidiLevels;
            private readonly DWriteNumberSubstitution?[] _numberSubstitutions;
            private readonly sbyte _paragraphBidiLevel;

            public TextAnalysisSinkImpl(int textLength, sbyte paragraphBidiLevel)
            {
                _scripts = new DWriteScriptAnalysis[textLength];
                _bidiLevels = new byte[textLength];
                _numberSubstitutions = new DWriteNumberSubstitution?[textLength];
                _paragraphBidiLevel = paragraphBidiLevel;

                for (var i = 0; i < _bidiLevels.Length; i++)
                {
                    _bidiLevels[i] = unchecked((byte)paragraphBidiLevel);
                }
            }

            public void SetScriptAnalysis(int textPosition, int textLength, DWriteScriptAnalysis scriptAnalysis)
            {
                for (var i = textPosition; i < textPosition + textLength && i < _scripts.Length; i++)
                {
                    _scripts[i] = scriptAnalysis;
                }
            }

            public void SetLineBreakpoints(int textPosition, int textLength, SharpDX.DirectWrite.LineBreakpoint[] lineBreakpoints)
            {
            }

            public void SetBidiLevel(int textPosition, int textLength, byte explicitLevel, byte resolvedLevel)
            {
                for (var i = textPosition; i < textPosition + textLength && i < _bidiLevels.Length; i++)
                {
                    _bidiLevels[i] = resolvedLevel;
                }
            }

            public void SetNumberSubstitution(int textPosition, int textLength, DWriteNumberSubstitution numberSubstitution)
            {
                for (var i = textPosition; i < textPosition + textLength && i < _numberSubstitutions.Length; i++)
                {
                    _numberSubstitutions[i] = numberSubstitution;
                }
            }

            public IReadOnlyList<TextAnalysisRun> GetRuns()
            {
                if (_scripts.Length == 0)
                {
                    return Array.Empty<TextAnalysisRun>();
                }

                var runs = new List<TextAnalysisRun>();
                var runStart = 0;

                for (var i = 1; i < _scripts.Length; i++)
                {
                    if (!SameRun(runStart, i))
                    {
                        runs.Add(CreateRun(runStart, i - runStart));
                        runStart = i;
                    }
                }

                runs.Add(CreateRun(runStart, _scripts.Length - runStart));
                return runs;
            }

            private bool SameRun(int left, int right)
            {
                return _scripts[left].Script == _scripts[right].Script &&
                       _scripts[left].Shapes == _scripts[right].Shapes &&
                       _bidiLevels[left] == _bidiLevels[right] &&
                       ReferenceEquals(_numberSubstitutions[left], _numberSubstitutions[right]);
            }

            private TextAnalysisRun CreateRun(int start, int length)
            {
                var bidiLevel = _bidiLevels[start];
                if (bidiLevel == 0 && (_paragraphBidiLevel & 1) == 1)
                {
                    bidiLevel = unchecked((byte)_paragraphBidiLevel);
                }

                return new TextAnalysisRun(
                    start,
                    length,
                    _scripts[start],
                    bidiLevel,
                    _numberSubstitutions[start]);
            }
        }

        private readonly record struct TextAnalysisRun(
            int Start,
            int Length,
            DWriteScriptAnalysis ScriptAnalysis,
            byte BidiLevel,
            DWriteNumberSubstitution? NumberSubstitution)
        {
            public bool IsRightToLeft => (BidiLevel & 1) != 0;
        }

        private readonly record struct FeatureRange(
            DWriteFontFeature[][] Features,
            int[] FeatureRangeLengths,
            int FeatureRanges);

        private readonly record struct RunShapeResult(
            int GlyphCount,
            short[] GlyphIndices,
            float[] GlyphAdvances,
            DWriteGlyphOffset[] GlyphOffsets,
            int[] GlyphClusters);
    }
}
