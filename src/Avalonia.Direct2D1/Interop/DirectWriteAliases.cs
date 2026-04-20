global using DWRITE_FACTORY_TYPE = Windows.Win32.Graphics.DirectWrite.DWRITE_FACTORY_TYPE;
global using DWRITE_FONT_METRICS = Windows.Win32.Graphics.DirectWrite.DWRITE_FONT_METRICS;
global using DWRITE_FONT_STRETCH = Windows.Win32.Graphics.DirectWrite.DWRITE_FONT_STRETCH;
global using DWRITE_FONT_STYLE = Windows.Win32.Graphics.DirectWrite.DWRITE_FONT_STYLE;
global using DWRITE_GLYPH_METRICS = Windows.Win32.Graphics.DirectWrite.DWRITE_GLYPH_METRICS;
global using DWRITE_GLYPH_OFFSET = Windows.Win32.Graphics.DirectWrite.DWRITE_GLYPH_OFFSET;
global using DWRITE_MEASURING_MODE = Windows.Win32.Graphics.DirectWrite.DWRITE_MEASURING_MODE;
global using DWRITE_READING_DIRECTION = Windows.Win32.Graphics.DirectWrite.DWRITE_READING_DIRECTION;
global using DWRITE_SCRIPT_ANALYSIS = Windows.Win32.Graphics.DirectWrite.DWRITE_SCRIPT_ANALYSIS;
global using DWRITE_SCRIPT_SHAPES = Windows.Win32.Graphics.DirectWrite.DWRITE_SCRIPT_SHAPES;
global using DWRITE_SHAPING_GLYPH_PROPERTIES = Windows.Win32.Graphics.DirectWrite.DWRITE_SHAPING_GLYPH_PROPERTIES;
global using DWRITE_SHAPING_TEXT_PROPERTIES = Windows.Win32.Graphics.DirectWrite.DWRITE_SHAPING_TEXT_PROPERTIES;
global using IDWriteFactory = Windows.Win32.Graphics.DirectWrite.IDWriteFactory;
global using IDWriteFont = Windows.Win32.Graphics.DirectWrite.IDWriteFont;
global using IDWriteFontCollection = Windows.Win32.Graphics.DirectWrite.IDWriteFontCollection;
global using IDWriteFontCollectionLoader = Windows.Win32.Graphics.DirectWrite.IDWriteFontCollectionLoader;
global using IDWriteFontFace = Windows.Win32.Graphics.DirectWrite.IDWriteFontFace;
global using IDWriteFontFile = Windows.Win32.Graphics.DirectWrite.IDWriteFontFile;
global using IDWriteFontFileEnumerator = Windows.Win32.Graphics.DirectWrite.IDWriteFontFileEnumerator;
global using IDWriteFontFileLoader = Windows.Win32.Graphics.DirectWrite.IDWriteFontFileLoader;
global using IDWriteFontFileStream = Windows.Win32.Graphics.DirectWrite.IDWriteFontFileStream;
global using IDWriteFontFamily = Windows.Win32.Graphics.DirectWrite.IDWriteFontFamily;
global using IDWriteFontList = Windows.Win32.Graphics.DirectWrite.IDWriteFontList;
global using IDWriteLocalizedStrings = Windows.Win32.Graphics.DirectWrite.IDWriteLocalizedStrings;
global using IDWriteNumberSubstitution = Windows.Win32.Graphics.DirectWrite.IDWriteNumberSubstitution;
global using IDWriteTextAnalysisSink = Windows.Win32.Graphics.DirectWrite.IDWriteTextAnalysisSink;
global using IDWriteTextAnalysisSource = Windows.Win32.Graphics.DirectWrite.IDWriteTextAnalysisSource;
global using IDWriteTextAnalyzer = Windows.Win32.Graphics.DirectWrite.IDWriteTextAnalyzer;

namespace Windows.Win32.Graphics.DirectWrite;

public partial struct DWRITE_SCRIPT_ANALYSIS
{
    public ushort Script
    {
        readonly get => script;
        set => script = value;
    }

    public DWRITE_SCRIPT_SHAPES Shapes
    {
        readonly get => shapes;
        set => shapes = value;
    }
}

public partial struct DWRITE_FONT_METRICS
{
    public ushort DesignUnitsPerEm
    {
        readonly get => designUnitsPerEm;
        set => designUnitsPerEm = value;
    }

    public ushort Ascent
    {
        readonly get => ascent;
        set => ascent = value;
    }

    public ushort Descent
    {
        readonly get => descent;
        set => descent = value;
    }

    public short LineGap
    {
        readonly get => lineGap;
        set => lineGap = value;
    }

    public ushort CapHeight
    {
        readonly get => capHeight;
        set => capHeight = value;
    }

    public ushort XHeight
    {
        readonly get => xHeight;
        set => xHeight = value;
    }

    public short UnderlinePosition
    {
        readonly get => underlinePosition;
        set => underlinePosition = value;
    }

    public ushort UnderlineThickness
    {
        readonly get => underlineThickness;
        set => underlineThickness = value;
    }

    public short StrikethroughPosition
    {
        readonly get => strikethroughPosition;
        set => strikethroughPosition = value;
    }

    public ushort StrikethroughThickness
    {
        readonly get => strikethroughThickness;
        set => strikethroughThickness = value;
    }
}

public partial struct DWRITE_GLYPH_METRICS
{
    public int LeftSideBearing
    {
        readonly get => leftSideBearing;
        set => leftSideBearing = value;
    }

    public uint AdvanceWidth
    {
        readonly get => advanceWidth;
        set => advanceWidth = value;
    }

    public int RightSideBearing
    {
        readonly get => rightSideBearing;
        set => rightSideBearing = value;
    }

    public int TopSideBearing
    {
        readonly get => topSideBearing;
        set => topSideBearing = value;
    }

    public uint AdvanceHeight
    {
        readonly get => advanceHeight;
        set => advanceHeight = value;
    }

    public int BottomSideBearing
    {
        readonly get => bottomSideBearing;
        set => bottomSideBearing = value;
    }

    public int VerticalOriginY
    {
        readonly get => verticalOriginY;
        set => verticalOriginY = value;
    }
}

public partial struct DWRITE_GLYPH_OFFSET
{
    public float AdvanceOffset
    {
        readonly get => advanceOffset;
        set => advanceOffset = value;
    }

    public float AscenderOffset
    {
        readonly get => ascenderOffset;
        set => ascenderOffset = value;
    }
}
