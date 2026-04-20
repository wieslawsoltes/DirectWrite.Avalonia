using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia.Direct2D1.Interop;

internal static partial class NativeMethods
{
    [DllImport("dwrite.dll", EntryPoint = "DWriteCreateFactory")]
    public static extern int DWriteCreateFactory(
        DWRITE_FACTORY_TYPE factoryType,
        in Guid iid,
        out IntPtr factory);
}

internal static class HResult
{
    internal const int S_OK = 0;
    internal const int E_FAIL = unchecked((int)0x80004005);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int ERROR_INSUFFICIENT_BUFFER = unchecked((int)0x8007007A);

    public static void ThrowIfFailed(int hresult)
    {
        if (hresult < 0)
        {
            Marshal.ThrowExceptionForHR(hresult);
        }
    }
}

internal static class ComReleaser
{
    public static void Release(object? comObject)
    {
        if (comObject is not null && Marshal.IsComObject(comObject))
        {
            Marshal.ReleaseComObject(comObject);
        }
    }
}

internal enum DWRITE_FACTORY_TYPE
{
    SHARED = 0,
    ISOLATED = 1,
}

internal enum DWRITE_FONT_STYLE
{
    NORMAL = 0,
    OBLIQUE = 1,
    ITALIC = 2,
}

internal enum DWRITE_FONT_STRETCH
{
    UNDEFINED = 0,
    ULTRA_CONDENSED = 1,
    EXTRA_CONDENSED = 2,
    CONDENSED = 3,
    SEMI_CONDENSED = 4,
    NORMAL = 5,
    SEMI_EXPANDED = 6,
    EXPANDED = 7,
    EXTRA_EXPANDED = 8,
    ULTRA_EXPANDED = 9,
}

internal enum DWRITE_SCRIPT_SHAPES
{
    DEFAULT = 0,
    NO_VISUAL = 1,
}

internal enum DWRITE_READING_DIRECTION
{
    LEFT_TO_RIGHT = 0,
    RIGHT_TO_LEFT = 1,
    TOP_TO_BOTTOM = 2,
    BOTTOM_TO_TOP = 3,
}

internal enum DWRITE_MEASURING_MODE
{
    NATURAL = 0,
    GDI_CLASSIC = 1,
    GDI_NATURAL = 2,
}

[StructLayout(LayoutKind.Sequential)]
internal struct DWRITE_SCRIPT_ANALYSIS
{
    public ushort Script;
    public DWRITE_SCRIPT_SHAPES Shapes;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DWRITE_FONT_METRICS
{
    public ushort DesignUnitsPerEm;
    public ushort Ascent;
    public ushort Descent;
    public short LineGap;
    public ushort CapHeight;
    public ushort XHeight;
    public short UnderlinePosition;
    public ushort UnderlineThickness;
    public short StrikethroughPosition;
    public ushort StrikethroughThickness;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DWRITE_GLYPH_METRICS
{
    public int LeftSideBearing;
    public uint AdvanceWidth;
    public int RightSideBearing;
    public int TopSideBearing;
    public uint AdvanceHeight;
    public int BottomSideBearing;
    public int VerticalOriginY;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DWRITE_SHAPING_TEXT_PROPERTIES
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DWRITE_SHAPING_GLYPH_PROPERTIES
{
    public ushort Value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DWRITE_GLYPH_OFFSET
{
    public float AdvanceOffset;
    public float AscenderOffset;
}

[StructLayout(LayoutKind.Sequential)]
internal struct D2D_POINT_2F
{
    public float X;
    public float Y;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct DWRITE_GLYPH_RUN_NATIVE
{
    public IntPtr FontFace;
    public float FontEmSize;
    public uint GlyphCount;
    public ushort* GlyphIndices;
    public float* GlyphAdvances;
    public DWRITE_GLYPH_OFFSET* GlyphOffsets;
    [MarshalAs(UnmanagedType.Bool)]
    public bool IsSideways;
    public uint BidiLevel;
}

[ComImport]
[Guid("B859EE5A-D838-4B5B-A2E8-1ADC7D93DB48")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFactory
{
    [PreserveSig] int GetSystemFontCollection([MarshalAs(UnmanagedType.Bool)] bool checkForUpdates, [MarshalAs(UnmanagedType.Interface)] out IDWriteFontCollection fontCollection);
    [PreserveSig] int CreateCustomFontCollection([MarshalAs(UnmanagedType.Interface)] IDWriteFontCollectionLoader collectionLoader, IntPtr collectionKey, uint collectionKeySize, [MarshalAs(UnmanagedType.Interface)] out IDWriteFontCollection fontCollection);
    [PreserveSig] int RegisterFontCollectionLoader([MarshalAs(UnmanagedType.Interface)] IDWriteFontCollectionLoader fontCollectionLoader);
    [PreserveSig] int UnregisterFontCollectionLoader([MarshalAs(UnmanagedType.Interface)] IDWriteFontCollectionLoader fontCollectionLoader);
    [PreserveSig] int CreateFontFileReference([MarshalAs(UnmanagedType.LPWStr)] string filePath, IntPtr lastWriteTime, [MarshalAs(UnmanagedType.Interface)] out IDWriteFontFile fontFile);
    [PreserveSig] int CreateCustomFontFileReference(IntPtr fontFileReferenceKey, uint fontFileReferenceKeySize, [MarshalAs(UnmanagedType.Interface)] IDWriteFontFileLoader fontFileLoader, [MarshalAs(UnmanagedType.Interface)] out IDWriteFontFile fontFile);
    [PreserveSig] int CreateFontFace();
    [PreserveSig] int CreateRenderingParams();
    [PreserveSig] int CreateMonitorRenderingParams();
    [PreserveSig] int CreateCustomRenderingParams();
    [PreserveSig] int RegisterFontFileLoader([MarshalAs(UnmanagedType.Interface)] IDWriteFontFileLoader fontFileLoader);
    [PreserveSig] int UnregisterFontFileLoader([MarshalAs(UnmanagedType.Interface)] IDWriteFontFileLoader fontFileLoader);
    [PreserveSig] int CreateTextFormat();
    [PreserveSig] int CreateTypography();
    [PreserveSig] int GetGdiInterop();
    [PreserveSig] int CreateTextLayout();
    [PreserveSig] int CreateGdiCompatibleTextLayout();
    [PreserveSig] int CreateEllipsisTrimmingSign();
    [PreserveSig] int CreateTextAnalyzer([MarshalAs(UnmanagedType.Interface)] out IDWriteTextAnalyzer textAnalyzer);
}

[ComImport]
[Guid("CCA920E4-52F0-492B-BFA8-29C72EE0A468")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontCollectionLoader
{
    [PreserveSig] int CreateEnumeratorFromKey(
        [MarshalAs(UnmanagedType.Interface)] IDWriteFactory factory,
        IntPtr collectionKey,
        uint collectionKeySize,
        [MarshalAs(UnmanagedType.Interface)] out IDWriteFontFileEnumerator? fontFileEnumerator);
}

[ComImport]
[Guid("727CAD4E-D6AF-4C9E-8A08-D695B11CAA49")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontFileLoader
{
    [PreserveSig] int CreateStreamFromKey(
        IntPtr fontFileReferenceKey,
        uint fontFileReferenceKeySize,
        [MarshalAs(UnmanagedType.Interface)] out IDWriteFontFileStream? fontFileStream);
}

[ComImport]
[Guid("72755049-5FF7-435D-8348-4BE97CFA6C7C")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontFileEnumerator
{
    [PreserveSig] int MoveNext([MarshalAs(UnmanagedType.Bool)] out bool hasCurrentFile);
    [PreserveSig] int GetCurrentFontFile([MarshalAs(UnmanagedType.Interface)] out IDWriteFontFile? fontFile);
}

[ComImport]
[Guid("6D4865FE-0AB8-4D91-8F62-5DD6BE34A3E0")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontFileStream
{
    [PreserveSig] int ReadFileFragment(out IntPtr fragmentStart, ulong fileOffset, ulong fragmentSize, out IntPtr fragmentContext);
    void ReleaseFileFragment(IntPtr fragmentContext);
    [PreserveSig] int GetFileSize(out ulong fileSize);
    [PreserveSig] int GetLastWriteTime(out ulong lastWriteTime);
}

[ComImport]
[Guid("739D886A-CEF5-47DC-8769-1A8B41BEBBB0")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontFile
{
}

[ComImport]
[Guid("A84CEE02-3EEA-4EEE-A827-87C1A02A0FCC")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontCollection
{
    uint GetFontFamilyCount();
    [PreserveSig] int GetFontFamily(uint index, [MarshalAs(UnmanagedType.Interface)] out IDWriteFontFamily fontFamily);
    [PreserveSig] int FindFamilyName([MarshalAs(UnmanagedType.LPWStr)] string familyName, out uint index, [MarshalAs(UnmanagedType.Bool)] out bool exists);
    [PreserveSig] int GetFontFromFontFace([MarshalAs(UnmanagedType.Interface)] IDWriteFontFace fontFace, [MarshalAs(UnmanagedType.Interface)] out IDWriteFont font);
}

[ComImport]
[Guid("1A0D8438-1D97-4EC1-AEF9-A2FB86ED6ACB")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontList
{
    [PreserveSig] int GetFontCollection([MarshalAs(UnmanagedType.Interface)] out IDWriteFontCollection fontCollection);
    uint GetFontCount();
    [PreserveSig] int GetFont(uint index, [MarshalAs(UnmanagedType.Interface)] out IDWriteFont font);
}

[ComImport]
[Guid("DA20D8EF-812A-4C43-9802-62EC4ABD7ADF")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontFamily
{
    [PreserveSig] int GetFontCollection([MarshalAs(UnmanagedType.Interface)] out IDWriteFontCollection fontCollection);
    uint GetFontCount();
    [PreserveSig] int GetFont(uint index, [MarshalAs(UnmanagedType.Interface)] out IDWriteFont font);
    [PreserveSig] int GetFamilyNames([MarshalAs(UnmanagedType.Interface)] out IDWriteLocalizedStrings names);
    [PreserveSig] int GetFirstMatchingFont(int weight, DWRITE_FONT_STRETCH stretch, DWRITE_FONT_STYLE style, [MarshalAs(UnmanagedType.Interface)] out IDWriteFont matchingFont);
    [PreserveSig] int GetMatchingFonts(int weight, DWRITE_FONT_STRETCH stretch, DWRITE_FONT_STYLE style, [MarshalAs(UnmanagedType.Interface)] out IDWriteFontList matchingFonts);
}

[ComImport]
[Guid("08256209-099A-4B34-B86D-C22B110E7771")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteLocalizedStrings
{
    uint GetCount();
    [PreserveSig] int FindLocaleName([MarshalAs(UnmanagedType.LPWStr)] string localeName, out uint index, [MarshalAs(UnmanagedType.Bool)] out bool exists);
    [PreserveSig] int GetLocaleNameLength(uint index, out uint length);
    [PreserveSig] int GetLocaleName(uint index, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder localeName, uint size);
    [PreserveSig] int GetStringLength(uint index, out uint length);
    [PreserveSig] int GetString(uint index, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder @string, uint size);
}

[ComImport]
[Guid("ACD16696-8C14-4F5D-877E-FE3FC1D32737")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFont
{
    [PreserveSig] int GetFontFamily([MarshalAs(UnmanagedType.Interface)] out IDWriteFontFamily fontFamily);
    int GetWeight();
    DWRITE_FONT_STRETCH GetStretch();
    DWRITE_FONT_STYLE GetStyle();
    [return: MarshalAs(UnmanagedType.Bool)] bool IsSymbolFont();
    [PreserveSig] int GetFaceNames([MarshalAs(UnmanagedType.Interface)] out IDWriteLocalizedStrings names);
    [PreserveSig] int GetInformationalStrings();
    int GetSimulations();
    void GetMetrics(out DWRITE_FONT_METRICS metrics);
    [PreserveSig] int HasCharacter(uint unicodeValue, [MarshalAs(UnmanagedType.Bool)] out bool exists);
    [PreserveSig] int CreateFontFace([MarshalAs(UnmanagedType.Interface)] out IDWriteFontFace fontFace);
}

[ComImport]
[Guid("5F49804D-7024-4D43-BFA9-D25984F53849")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteFontFace
{
    int GetType();
    [PreserveSig] int GetFiles();
    uint GetIndex();
    int GetSimulations();
    [return: MarshalAs(UnmanagedType.Bool)] bool IsSymbolFont();
    void GetMetrics(out DWRITE_FONT_METRICS fontFaceMetrics);
    ushort GetGlyphCount();
    void GetDesignGlyphMetrics([In] ushort[] glyphIndices, uint glyphCount, [Out] DWRITE_GLYPH_METRICS[] glyphMetrics, [MarshalAs(UnmanagedType.Bool)] bool isSideways);
    void GetGlyphIndices([In] uint[] codePoints, uint codePointCount, [Out] ushort[] glyphIndices);
    [return: MarshalAs(UnmanagedType.Bool)]
    bool TryGetFontTable(uint openTypeTableTag, out IntPtr tableData, out uint tableSize, out IntPtr tableContext, [MarshalAs(UnmanagedType.Bool)] out bool exists);
    void ReleaseFontTable(IntPtr tableContext);
    void GetGlyphRunOutline(
        float emSize,
        [In] ushort[] glyphIndices,
        IntPtr glyphAdvances,
        IntPtr glyphOffsets,
        uint glyphCount,
        [MarshalAs(UnmanagedType.Bool)] bool isSideways,
        [MarshalAs(UnmanagedType.Bool)] bool isRightToLeft,
        IntPtr geometrySink);
}

[ComImport]
[Guid("B7E6163E-7F46-43B4-84B3-E4E6249C365D")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteTextAnalyzer
{
    [PreserveSig] int AnalyzeScript([MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource analysisSource, uint textPosition, uint textLength, [MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink analysisSink);
    [PreserveSig] int AnalyzeBidi([MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource analysisSource, uint textPosition, uint textLength, [MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink analysisSink);
    [PreserveSig] int AnalyzeNumberSubstitution([MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource analysisSource, uint textPosition, uint textLength, [MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink analysisSink);
    [PreserveSig] int AnalyzeLineBreakpoints([MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSource analysisSource, uint textPosition, uint textLength, [MarshalAs(UnmanagedType.Interface)] IDWriteTextAnalysisSink analysisSink);
    [PreserveSig]
    int GetGlyphs(
        [MarshalAs(UnmanagedType.LPWStr)] string textString,
        uint textLength,
        [MarshalAs(UnmanagedType.Interface)] IDWriteFontFace fontFace,
        [MarshalAs(UnmanagedType.Bool)] bool isSideways,
        [MarshalAs(UnmanagedType.Bool)] bool isRightToLeft,
        ref DWRITE_SCRIPT_ANALYSIS scriptAnalysis,
        [MarshalAs(UnmanagedType.LPWStr)] string localeName,
        [MarshalAs(UnmanagedType.Interface)] IDWriteNumberSubstitution? numberSubstitution,
        IntPtr features,
        IntPtr featureRangeLengths,
        uint featureRanges,
        uint maxGlyphCount,
        [Out] ushort[] clusterMap,
        [Out] DWRITE_SHAPING_TEXT_PROPERTIES[] textProps,
        [Out] ushort[] glyphIndices,
        [Out] DWRITE_SHAPING_GLYPH_PROPERTIES[] glyphProps,
        out uint actualGlyphCount);

    [PreserveSig]
    int GetGlyphPlacements(
        [MarshalAs(UnmanagedType.LPWStr)] string textString,
        [In] ushort[] clusterMap,
        [In] DWRITE_SHAPING_TEXT_PROPERTIES[] textProps,
        uint textLength,
        [In] ushort[] glyphIndices,
        [In] DWRITE_SHAPING_GLYPH_PROPERTIES[] glyphProps,
        uint glyphCount,
        [MarshalAs(UnmanagedType.Interface)] IDWriteFontFace fontFace,
        float fontEmSize,
        [MarshalAs(UnmanagedType.Bool)] bool isSideways,
        [MarshalAs(UnmanagedType.Bool)] bool isRightToLeft,
        ref DWRITE_SCRIPT_ANALYSIS scriptAnalysis,
        [MarshalAs(UnmanagedType.LPWStr)] string localeName,
        IntPtr features,
        IntPtr featureRangeLengths,
        uint featureRanges,
        [Out] float[] glyphAdvances,
        [Out] DWRITE_GLYPH_OFFSET[] glyphOffsets);
}

[ComImport]
[Guid("14885CC9-BAB0-4F90-B6ED-5C366A2CD03D")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteNumberSubstitution
{
}

[ComImport]
[Guid("688E1A58-5094-47C8-ADC8-FBCEA60AE92B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteTextAnalysisSource
{
    [PreserveSig] int GetTextAtPosition(uint textPosition, out IntPtr textString, out uint textLength);
    [PreserveSig] int GetTextBeforePosition(uint textPosition, out IntPtr textString, out uint textLength);
    [PreserveSig] int GetParagraphReadingDirection(out DWRITE_READING_DIRECTION readingDirection);
    [PreserveSig] int GetLocaleName(uint textPosition, out uint textLength, out IntPtr localeName);
    [PreserveSig] int GetNumberSubstitution(uint textPosition, out uint textLength, [MarshalAs(UnmanagedType.Interface)] out IDWriteNumberSubstitution? numberSubstitution);
}

[ComImport]
[Guid("5810CD44-0CA0-4701-B3FA-BEC5182AE4F6")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDWriteTextAnalysisSink
{
    [PreserveSig] int SetScriptAnalysis(uint textPosition, uint textLength, ref DWRITE_SCRIPT_ANALYSIS scriptAnalysis);
    [PreserveSig] int SetLineBreakpoints(uint textPosition, uint textLength, IntPtr lineBreakpoints);
    [PreserveSig] int SetBidiLevel(uint textPosition, uint textLength, byte explicitLevel, byte resolvedLevel);
    [PreserveSig] int SetNumberSubstitution(uint textPosition, uint textLength, [MarshalAs(UnmanagedType.Interface)] IDWriteNumberSubstitution? numberSubstitution);
}

[ComImport]
[Guid("2CD90694-12E2-11DC-9FED-001143A055F9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ID2D1RenderTargetNative
{
    [PreserveSig] int GetFactory(out IntPtr factory);
    [PreserveSig] int CreateBitmap();
    [PreserveSig] int CreateBitmapFromWicBitmap();
    [PreserveSig] int CreateSharedBitmap();
    [PreserveSig] int CreateBitmapBrush();
    [PreserveSig] int CreateSolidColorBrush();
    [PreserveSig] int CreateGradientStopCollection();
    [PreserveSig] int CreateLinearGradientBrush();
    [PreserveSig] int CreateRadialGradientBrush();
    [PreserveSig] int CreateCompatibleRenderTarget();
    [PreserveSig] int CreateLayer();
    [PreserveSig] int CreateMesh();
    [PreserveSig] int DrawLine();
    [PreserveSig] int DrawRectangle();
    [PreserveSig] int FillRectangle();
    [PreserveSig] int DrawRoundedRectangle();
    [PreserveSig] int FillRoundedRectangle();
    [PreserveSig] int DrawEllipse();
    [PreserveSig] int FillEllipse();
    [PreserveSig] int DrawGeometry();
    [PreserveSig] int FillGeometry();
    [PreserveSig] int FillMesh();
    [PreserveSig] int FillOpacityMask();
    [PreserveSig] int DrawBitmap();
    [PreserveSig] int DrawText();
    [PreserveSig] int DrawTextLayout();
    void DrawGlyphRun(D2D_POINT_2F baselineOrigin, ref DWRITE_GLYPH_RUN_NATIVE glyphRun, IntPtr foregroundBrush, DWRITE_MEASURING_MODE measuringMode);
}

internal sealed class DWriteFactory : IDisposable
{
    private readonly IDWriteFactory _native;

    private DWriteFactory(IDWriteFactory native)
    {
        _native = native;
    }

    public static DWriteFactory CreateShared()
    {
        var iid = typeof(IDWriteFactory).GUID;
        HResult.ThrowIfFailed(NativeMethods.DWriteCreateFactory(DWRITE_FACTORY_TYPE.SHARED, in iid, out var factoryPtr));
        var factory = (IDWriteFactory)Marshal.GetObjectForIUnknown(factoryPtr);
        Marshal.Release(factoryPtr);
        return new DWriteFactory(factory);
    }

    public DWriteFontCollection GetSystemFontCollection(bool checkForUpdates)
    {
        HResult.ThrowIfFailed(_native.GetSystemFontCollection(checkForUpdates, out var fontCollection));
        return new DWriteFontCollection(fontCollection);
    }

    public unsafe DWriteCustomFontCollection CreateCustomFontCollection(IReadOnlyList<Stream> fontStreams)
    {
        var loader = new DWriteResourceFontLoader(this, fontStreams);

        try
        {
            fixed (byte* collectionKeyPtr = loader.CollectionKey)
            {
                HResult.ThrowIfFailed(_native.CreateCustomFontCollection(
                    loader,
                    (IntPtr)collectionKeyPtr,
                    (uint)loader.CollectionKey.Length,
                    out var fontCollection));

                return new DWriteCustomFontCollection(new DWriteFontCollection(fontCollection), loader);
            }
        }
        catch
        {
            loader.Dispose();
            throw;
        }
    }

    public DWriteTextAnalyzer CreateTextAnalyzer()
    {
        HResult.ThrowIfFailed(_native.CreateTextAnalyzer(out var analyzer));
        return new DWriteTextAnalyzer(analyzer);
    }

    internal IDWriteFactory Native => _native;

    public void Dispose()
    {
        ComReleaser.Release(_native);
    }
}

internal sealed class DWriteFontCollection : IDisposable
{
    private readonly IDWriteFontCollection _native;

    public DWriteFontCollection(IDWriteFontCollection native)
    {
        _native = native;
    }

    public uint FontFamilyCount => _native.GetFontFamilyCount();

    public bool FindFamilyName(string familyName, out uint index)
    {
        HResult.ThrowIfFailed(_native.FindFamilyName(familyName, out index, out var exists));
        return exists;
    }

    public DWriteFontFamily GetFontFamily(uint index)
    {
        HResult.ThrowIfFailed(_native.GetFontFamily(index, out var family));
        return new DWriteFontFamily(family);
    }

    public void Dispose()
    {
        ComReleaser.Release(_native);
    }
}

internal sealed class DWriteFontList : IDisposable
{
    private readonly IDWriteFontList _native;

    public DWriteFontList(IDWriteFontList native)
    {
        _native = native;
    }

    public uint FontCount => _native.GetFontCount();

    public DWriteFont GetFont(uint index)
    {
        HResult.ThrowIfFailed(_native.GetFont(index, out var font));
        return new DWriteFont(font);
    }

    public void Dispose()
    {
        ComReleaser.Release(_native);
    }
}

internal sealed class DWriteFontFamily : IDisposable
{
    private readonly IDWriteFontFamily _native;

    public DWriteFontFamily(IDWriteFontFamily native)
    {
        _native = native;
    }

    public uint FontCount => _native.GetFontCount();

    public string FamilyName
    {
        get
        {
            HResult.ThrowIfFailed(_native.GetFamilyNames(out var names));
            try
            {
                return new DWriteLocalizedStrings(names).GetString(0);
            }
            finally
            {
                ComReleaser.Release(names);
            }
        }
    }

    public DWriteFont GetFont(uint index)
    {
        HResult.ThrowIfFailed(_native.GetFont(index, out var font));
        return new DWriteFont(font);
    }

    public DWriteFont GetFirstMatchingFont(int weight, DWRITE_FONT_STRETCH stretch, DWRITE_FONT_STYLE style)
    {
        HResult.ThrowIfFailed(_native.GetFirstMatchingFont(weight, stretch, style, out var font));
        return new DWriteFont(font);
    }

    public DWriteFontList GetMatchingFonts(int weight, DWRITE_FONT_STRETCH stretch, DWRITE_FONT_STYLE style)
    {
        HResult.ThrowIfFailed(_native.GetMatchingFonts(weight, stretch, style, out var fontList));
        return new DWriteFontList(fontList);
    }

    public void Dispose()
    {
        ComReleaser.Release(_native);
    }
}

internal sealed class DWriteLocalizedStrings
{
    private readonly IDWriteLocalizedStrings _native;

    public DWriteLocalizedStrings(IDWriteLocalizedStrings native)
    {
        _native = native;
    }

    public string GetString(uint index)
    {
        HResult.ThrowIfFailed(_native.GetStringLength(index, out var length));
        var builder = new StringBuilder((int)length + 1);
        HResult.ThrowIfFailed(_native.GetString(index, builder, length + 1));
        return builder.ToString();
    }
}

internal sealed class DWriteFont : IDisposable
{
    private readonly IDWriteFont _native;

    public DWriteFont(IDWriteFont native)
    {
        _native = native;
    }

    public int Weight => _native.GetWeight();
    public DWRITE_FONT_STYLE Style => _native.GetStyle();
    public DWRITE_FONT_STRETCH Stretch => _native.GetStretch();

    public string FamilyName
    {
        get
        {
            HResult.ThrowIfFailed(_native.GetFontFamily(out var family));
            try
            {
                return new DWriteFontFamily(family).FamilyName;
            }
            finally
            {
                ComReleaser.Release(family);
            }
        }
    }

    public bool HasCharacter(uint codepoint)
    {
        HResult.ThrowIfFailed(_native.HasCharacter(codepoint, out var exists));
        return exists;
    }

    public DWriteFontFace CreateFontFace()
    {
        HResult.ThrowIfFailed(_native.CreateFontFace(out var fontFace));
        return new DWriteFontFace(fontFace);
    }

    public void Dispose()
    {
        ComReleaser.Release(_native);
    }
}

internal sealed class DWriteFontFace : IDisposable
{
    private readonly IDWriteFontFace _native;

    public DWriteFontFace(IDWriteFontFace native)
    {
        _native = native;
    }

    public IDWriteFontFace Native => _native;

    public DWRITE_FONT_METRICS Metrics
    {
        get
        {
            _native.GetMetrics(out var metrics);
            return metrics;
        }
    }

    public ushort[] GetGlyphIndices(uint[] codePoints)
    {
        var glyphs = new ushort[codePoints.Length];
        _native.GetGlyphIndices(codePoints, (uint)codePoints.Length, glyphs);
        return glyphs;
    }

    public DWRITE_GLYPH_METRICS[] GetDesignGlyphMetrics(ushort[] glyphIndices, bool isSideways)
    {
        var metrics = new DWRITE_GLYPH_METRICS[glyphIndices.Length];
        _native.GetDesignGlyphMetrics(glyphIndices, (uint)glyphIndices.Length, metrics, isSideways);
        return metrics;
    }

    public bool TryGetFontTable(uint openTypeTableTag, out IntPtr tableData, out uint tableSize, out IntPtr tableContext)
    {
        return _native.TryGetFontTable(openTypeTableTag, out tableData, out tableSize, out tableContext, out var exists) && exists;
    }

    public void ReleaseFontTable(IntPtr tableContext)
    {
        _native.ReleaseFontTable(tableContext);
    }

    public void GetGlyphRunOutline(float emSize, ushort[] glyphIndices, bool isSideways, bool isRightToLeft, IntPtr geometrySink)
    {
        _native.GetGlyphRunOutline(emSize, glyphIndices, IntPtr.Zero, IntPtr.Zero, (uint)glyphIndices.Length, isSideways, isRightToLeft, geometrySink);
    }

    public void Dispose()
    {
        ComReleaser.Release(_native);
    }
}

internal sealed class DWriteTextAnalyzer : IDisposable
{
    private readonly IDWriteTextAnalyzer _native;

    public DWriteTextAnalyzer(IDWriteTextAnalyzer native)
    {
        _native = native;
    }

    public IDWriteTextAnalyzer Native => _native;

    public void Dispose()
    {
        ComReleaser.Release(_native);
    }
}

internal static class D2D1TextInterop
{
    public static unsafe void DrawGlyphRun(
        IntPtr renderTarget,
        D2D_POINT_2F baselineOrigin,
        IntPtr fontFace,
        float fontEmSize,
        ReadOnlySpan<ushort> glyphIndices,
        ReadOnlySpan<float> glyphAdvances,
        ReadOnlySpan<DWRITE_GLYPH_OFFSET> glyphOffsets,
        uint bidiLevel,
        IntPtr foregroundBrush)
    {
        var native = (ID2D1RenderTargetNative)Marshal.GetObjectForIUnknown(renderTarget);

        try
        {
            fixed (ushort* glyphIndicesPtr = glyphIndices)
            fixed (float* glyphAdvancesPtr = glyphAdvances)
            fixed (DWRITE_GLYPH_OFFSET* glyphOffsetsPtr = glyphOffsets)
            {
                var glyphRun = new DWRITE_GLYPH_RUN_NATIVE
                {
                    FontFace = fontFace,
                    FontEmSize = fontEmSize,
                    GlyphCount = (uint)glyphIndices.Length,
                    GlyphIndices = glyphIndicesPtr,
                    GlyphAdvances = glyphAdvancesPtr,
                    GlyphOffsets = glyphOffsetsPtr,
                    IsSideways = false,
                    BidiLevel = bidiLevel
                };

                native.DrawGlyphRun(
                    baselineOrigin,
                    ref glyphRun,
                    foregroundBrush,
                    DWRITE_MEASURING_MODE.NATURAL);
            }
        }
        finally
        {
            ComReleaser.Release(native);
        }
    }
}
