using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Avalonia.Direct2D1.Interop;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Platform;

namespace Avalonia.Direct2D1.Media;

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
        using var analysisSource = new TextAnalysisSourceImpl(textString, usedCulture, options.BidiLevel);
        var analysisSink = new TextAnalysisSinkImpl(textString.Length, options.BidiLevel);
        var analyzer = Direct2D1Platform.DirectWriteTextAnalyzer.Native;

        HResult.ThrowIfFailed(analyzer.AnalyzeScript(analysisSource, 0, (uint)textString.Length, analysisSink));
        HResult.ThrowIfFailed(analyzer.AnalyzeBidi(analysisSource, 0, (uint)textString.Length, analysisSink));
        HResult.ThrowIfFailed(analyzer.AnalyzeNumberSubstitution(analysisSource, 0, (uint)textString.Length, analysisSink));

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
                var glyphIndex = run.GlyphIndices[i];
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

                shapedBuffer[bufferIndex++] = new GlyphInfo(glyphIndex, glyphCluster, glyphAdvance, glyphOffset);
            }
        }

        return shapedBuffer;
    }

    private static RunShapeResult ShapeRun(
        IDWriteTextAnalyzer analyzer,
        GlyphTypefaceImpl typeface,
        string text,
        TextAnalysisRun run,
        TextShaperOptions options,
        CultureInfo culture)
    {
        var runText = text.Substring(run.Start, run.Length);
        var maxGlyphCount = GetMaxGlyphCount(run.Length);

        while (true)
        {
            var clusterMap = new ushort[run.Length];
            var textProps = new DWRITE_SHAPING_TEXT_PROPERTIES[run.Length];
            var glyphIndices = new ushort[maxGlyphCount];
            var glyphProps = new DWRITE_SHAPING_GLYPH_PROPERTIES[maxGlyphCount];

            var scriptAnalysis = run.ScriptAnalysis;

            var hr = analyzer.GetGlyphs(
                runText,
                (uint)run.Length,
                typeface.FontFace.Native,
                isSideways: false,
                isRightToLeft: run.IsRightToLeft,
                ref scriptAnalysis,
                culture.Name,
                run.NumberSubstitution,
                IntPtr.Zero,
                IntPtr.Zero,
                0,
                (uint)maxGlyphCount,
                clusterMap,
                textProps,
                glyphIndices,
                glyphProps,
                out var actualGlyphCount);

            if (hr == HResult.ERROR_INSUFFICIENT_BUFFER && maxGlyphCount < run.Length * 8 + 32)
            {
                maxGlyphCount *= 2;
                continue;
            }

            HResult.ThrowIfFailed(hr);

            Array.Resize(ref glyphIndices, (int)actualGlyphCount);
            Array.Resize(ref glyphProps, (int)actualGlyphCount);

            var glyphAdvances = new float[actualGlyphCount];
            var glyphOffsets = new DWRITE_GLYPH_OFFSET[actualGlyphCount];

            HResult.ThrowIfFailed(analyzer.GetGlyphPlacements(
                runText,
                clusterMap,
                textProps,
                (uint)run.Length,
                glyphIndices,
                glyphProps,
                actualGlyphCount,
                typeface.FontFace.Native,
                (float)options.FontRenderingEmSize,
                isSideways: false,
                isRightToLeft: run.IsRightToLeft,
                ref scriptAnalysis,
                culture.Name,
                IntPtr.Zero,
                IntPtr.Zero,
                0,
                glyphAdvances,
                glyphOffsets));

            return new RunShapeResult(
                (int)actualGlyphCount,
                glyphIndices,
                glyphAdvances,
                glyphOffsets,
                BuildGlyphClusters(clusterMap, (int)actualGlyphCount, run.Start));
        }
    }

    private static int[] BuildGlyphClusters(ushort[] clusterMap, int glyphCount, int runStart)
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

    private static int GetMaxGlyphCount(int textLength)
    {
        return Math.Max(16, textLength * 3 / 2 + 16);
    }

    public ITextShaperTypeface CreateTypeface(GlyphTypeface glyphTypeface)
    {
        return glyphTypeface.PlatformTypeface as GlyphTypefaceImpl
            ?? throw new NotSupportedException("The provided GlyphTypeface is not supported by this text shaper.");
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class TextAnalysisSourceImpl : IDWriteTextAnalysisSource, IDisposable
    {
        private readonly string _text;
        private readonly GCHandle _textHandle;
        private readonly GCHandle _localeHandle;

        public TextAnalysisSourceImpl(string text, CultureInfo culture, sbyte bidiLevel)
        {
            _text = text;
            _textHandle = GCHandle.Alloc(_text, GCHandleType.Pinned);
            var locale = culture.Name.Length == 0 ? CultureInfo.InvariantCulture.Name : culture.Name;
            _localeHandle = GCHandle.Alloc(locale, GCHandleType.Pinned);
            ReadingDirection = (bidiLevel & 1) == 0
                ? DWRITE_READING_DIRECTION.LEFT_TO_RIGHT
                : DWRITE_READING_DIRECTION.RIGHT_TO_LEFT;
        }

        public DWRITE_READING_DIRECTION ReadingDirection { get; }

        public int GetTextAtPosition(uint textPosition, out IntPtr textString, out uint textLength)
        {
            if (textPosition >= _text.Length)
            {
                textString = IntPtr.Zero;
                textLength = 0;
                return HResult.S_OK;
            }

            textString = _textHandle.AddrOfPinnedObject() + (int)textPosition * sizeof(char);
            textLength = (uint)(_text.Length - (int)textPosition);
            return HResult.S_OK;
        }

        public int GetTextBeforePosition(uint textPosition, out IntPtr textString, out uint textLength)
        {
            if (textPosition == 0)
            {
                textString = IntPtr.Zero;
                textLength = 0;
                return HResult.S_OK;
            }

            textString = _textHandle.AddrOfPinnedObject();
            textLength = Math.Min(textPosition, (uint)_text.Length);
            return HResult.S_OK;
        }

        public int GetParagraphReadingDirection(out DWRITE_READING_DIRECTION readingDirection)
        {
            readingDirection = ReadingDirection;
            return HResult.S_OK;
        }

        public int GetLocaleName(uint textPosition, out uint textLength, out IntPtr localeName)
        {
            localeName = _localeHandle.AddrOfPinnedObject();
            textLength = textPosition >= _text.Length ? 0 : (uint)(_text.Length - (int)textPosition);
            return HResult.S_OK;
        }

        public int GetNumberSubstitution(uint textPosition, out uint textLength, out IDWriteNumberSubstitution? numberSubstitution)
        {
            textLength = textPosition >= _text.Length ? 0 : (uint)(_text.Length - (int)textPosition);
            numberSubstitution = null;
            return HResult.S_OK;
        }

        public void Dispose()
        {
            if (_textHandle.IsAllocated)
            {
                _textHandle.Free();
            }

            if (_localeHandle.IsAllocated)
            {
                _localeHandle.Free();
            }
        }
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    private sealed class TextAnalysisSinkImpl : IDWriteTextAnalysisSink
    {
        private readonly DWRITE_SCRIPT_ANALYSIS[] _scripts;
        private readonly byte[] _bidiLevels;
        private readonly IDWriteNumberSubstitution?[] _numberSubstitutions;
        private readonly sbyte _paragraphBidiLevel;

        public TextAnalysisSinkImpl(int textLength, sbyte paragraphBidiLevel)
        {
            _scripts = new DWRITE_SCRIPT_ANALYSIS[textLength];
            _bidiLevels = new byte[textLength];
            _numberSubstitutions = new IDWriteNumberSubstitution?[textLength];
            _paragraphBidiLevel = paragraphBidiLevel;

            for (var i = 0; i < _bidiLevels.Length; i++)
            {
                _bidiLevels[i] = unchecked((byte)paragraphBidiLevel);
            }
        }

        public int SetScriptAnalysis(uint textPosition, uint textLength, ref DWRITE_SCRIPT_ANALYSIS scriptAnalysis)
        {
            for (var i = (int)textPosition; i < textPosition + textLength && i < _scripts.Length; i++)
            {
                _scripts[i] = scriptAnalysis;
            }

            return HResult.S_OK;
        }

        public int SetLineBreakpoints(uint textPosition, uint textLength, IntPtr lineBreakpoints)
        {
            return HResult.S_OK;
        }

        public int SetBidiLevel(uint textPosition, uint textLength, byte explicitLevel, byte resolvedLevel)
        {
            for (var i = (int)textPosition; i < textPosition + textLength && i < _bidiLevels.Length; i++)
            {
                _bidiLevels[i] = resolvedLevel;
            }

            return HResult.S_OK;
        }

        public int SetNumberSubstitution(uint textPosition, uint textLength, IDWriteNumberSubstitution? numberSubstitution)
        {
            for (var i = (int)textPosition; i < textPosition + textLength && i < _numberSubstitutions.Length; i++)
            {
                _numberSubstitutions[i] = numberSubstitution;
            }

            return HResult.S_OK;
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
        DWRITE_SCRIPT_ANALYSIS ScriptAnalysis,
        byte BidiLevel,
        IDWriteNumberSubstitution? NumberSubstitution)
    {
        public bool IsRightToLeft => (BidiLevel & 1) != 0;
    }

    private readonly record struct RunShapeResult(
        int GlyphCount,
        ushort[] GlyphIndices,
        float[] GlyphAdvances,
        DWRITE_GLYPH_OFFSET[] GlyphOffsets,
        int[] GlyphClusters);
}
