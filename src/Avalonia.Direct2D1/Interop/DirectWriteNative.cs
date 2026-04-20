using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D.Common;
using Avalonia.Direct2D1.Interop.Direct2D1;

namespace Avalonia.Direct2D1.Interop;

internal static class DirectWriteErrorCodes
{
    internal const int ErrorInsufficientBuffer = unchecked((int)0x8007007A);
}

internal static unsafe class GeneratedComHelpers
{
    public static IntPtr ConvertToUnmanaged<T>(T value)
        where T : class
    {
        return value is null ? IntPtr.Zero : (IntPtr)ComInterfaceMarshaller<T>.ConvertToUnmanaged(value);
    }

    public static T ConvertToManaged<T>(IntPtr value)
        where T : class
    {
        return ComInterfaceMarshaller<T>.ConvertToManaged((void*)value)!;
    }

    public static void Free<T>(IntPtr value)
        where T : class
    {
        if (value != IntPtr.Zero)
        {
            ComInterfaceMarshaller<T>.Free((void*)value);
        }
    }
}

internal abstract class GeneratedComWrapper<T> : IDisposable
    where T : class
{
    private IntPtr _nativePointer;
    private bool _disposed;

    protected GeneratedComWrapper(T native)
    {
        Native = native ?? throw new ArgumentNullException(nameof(native));
        _nativePointer = GeneratedComHelpers.ConvertToUnmanaged(native);
    }

    protected T Native { get; private set; }

    protected IntPtr NativePointer => _nativePointer;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_nativePointer != IntPtr.Zero)
        {
            GeneratedComHelpers.Free<T>(_nativePointer);
            _nativePointer = IntPtr.Zero;
        }

        if (disposing)
        {
            Native = null!;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

internal sealed class DWriteFactory : GeneratedComWrapper<IDWriteFactory>
{
    private DWriteFactory(IDWriteFactory native)
        : base(native)
    {
    }

    public static DWriteFactory CreateShared()
    {
        var iid = typeof(IDWriteFactory).GUID;
        PInvoke.DWriteCreateFactory(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED, in iid, out object factory).ThrowOnFailure();
        return new DWriteFactory((IDWriteFactory)factory);
    }

    public DWriteFontCollection GetSystemFontCollection(bool checkForUpdates)
    {
        Native.GetSystemFontCollection(out var fontCollection, checkForUpdates);
        return new DWriteFontCollection(fontCollection);
    }

    public DWriteCustomFontCollection CreateCustomFontCollection(IReadOnlyList<Stream> fontStreams)
    {
        var loader = new DWriteResourceFontLoader(this, fontStreams);

        try
        {
            Native.CreateCustomFontCollection(loader, loader.CollectionKey, out var fontCollection);
            return new DWriteCustomFontCollection(new DWriteFontCollection(fontCollection), loader);
        }
        catch
        {
            loader.Dispose();
            throw;
        }
    }

    public DWriteTextAnalyzer CreateTextAnalyzer()
    {
        Native.CreateTextAnalyzer(out var analyzer);
        return new DWriteTextAnalyzer(analyzer);
    }

    internal new IDWriteFactory Native => base.Native;
}

internal sealed class DWriteFontCollection : GeneratedComWrapper<IDWriteFontCollection>
{
    public DWriteFontCollection(IDWriteFontCollection native)
        : base(native)
    {
    }

    public uint FontFamilyCount => Native.GetFontFamilyCount();

    public bool FindFamilyName(string familyName, out uint index)
    {
        Native.FindFamilyName(familyName, out index, out var exists);
        return exists;
    }

    public DWriteFontFamily GetFontFamily(uint index)
    {
        Native.GetFontFamily(index, out var family);
        return new DWriteFontFamily(family);
    }
}

internal sealed class DWriteFontList : GeneratedComWrapper<IDWriteFontList>
{
    public DWriteFontList(IDWriteFontList native)
        : base(native)
    {
    }

    public uint FontCount => Native.GetFontCount();

    public DWriteFont GetFont(uint index)
    {
        Native.GetFont(index, out var font);
        return new DWriteFont(font);
    }
}

internal sealed class DWriteFontFamily : GeneratedComWrapper<IDWriteFontFamily>
{
    public DWriteFontFamily(IDWriteFontFamily native)
        : base(native)
    {
    }

    public uint FontCount => Native.GetFontCount();

    public string FamilyName
    {
        get
        {
            Native.GetFamilyNames(out var names);

            using var localizedStrings = new DWriteLocalizedStrings(names);
            return localizedStrings.GetString(0);
        }
    }

    public DWriteFont GetFont(uint index)
    {
        Native.GetFont(index, out var font);
        return new DWriteFont(font);
    }

    public DWriteFont GetFirstMatchingFont(int weight, DWRITE_FONT_STRETCH stretch, DWRITE_FONT_STYLE style)
    {
        Native.GetFirstMatchingFont((Windows.Win32.Graphics.DirectWrite.DWRITE_FONT_WEIGHT)weight, stretch, style, out var font);
        return new DWriteFont(font);
    }

    public DWriteFontList GetMatchingFonts(int weight, DWRITE_FONT_STRETCH stretch, DWRITE_FONT_STYLE style)
    {
        Native.GetMatchingFonts((Windows.Win32.Graphics.DirectWrite.DWRITE_FONT_WEIGHT)weight, stretch, style, out var fontList);
        return new DWriteFontList(fontList);
    }
}

internal sealed class DWriteLocalizedStrings : GeneratedComWrapper<IDWriteLocalizedStrings>
{
    public DWriteLocalizedStrings(IDWriteLocalizedStrings native)
        : base(native)
    {
    }

    public string GetString(uint index)
    {
        Native.GetStringLength(index, out var length);

        Span<char> buffer = length + 1 <= 256
            ? stackalloc char[(int)length + 1]
            : new char[(int)length + 1];

        Native.GetString(index, buffer);
        return buffer.Slice(0, (int)length).ToString();
    }
}

internal sealed class DWriteFont : GeneratedComWrapper<IDWriteFont>
{
    public DWriteFont(IDWriteFont native)
        : base(native)
    {
    }

    public int Weight => (int)Native.GetWeight();

    public DWRITE_FONT_STYLE Style => Native.GetStyle();

    public DWRITE_FONT_STRETCH Stretch => Native.GetStretch();

    public string FamilyName
    {
        get
        {
            Native.GetFontFamily(out var family);

            using var fontFamily = new DWriteFontFamily(family);
            return fontFamily.FamilyName;
        }
    }

    public bool HasCharacter(uint codepoint)
    {
        Native.HasCharacter(codepoint, out var exists);
        return exists;
    }

    public DWriteFontFace CreateFontFace()
    {
        Native.CreateFontFace(out var fontFace);
        return new DWriteFontFace(fontFace);
    }
}

internal sealed class DWriteFontFace : GeneratedComWrapper<IDWriteFontFace>
{
    public DWriteFontFace(IDWriteFontFace native)
        : base(native)
    {
    }

    internal new IDWriteFontFace Native => base.Native;

    internal unsafe Windows.Win32.Graphics.DirectWrite.IDWriteFontFace_unmanaged* NativeFontFacePointer =>
        (Windows.Win32.Graphics.DirectWrite.IDWriteFontFace_unmanaged*)NativePointer;

    public DWRITE_FONT_METRICS Metrics
    {
        get
        {
            Native.GetMetrics(out var metrics);
            return metrics;
        }
    }

    public ushort[] GetGlyphIndices(uint[] codePoints)
    {
        var glyphs = new ushort[codePoints.Length];
        Native.GetGlyphIndices(codePoints, glyphs);
        return glyphs;
    }

    public DWRITE_GLYPH_METRICS[] GetDesignGlyphMetrics(ushort[] glyphIndices, bool isSideways)
    {
        var metrics = new DWRITE_GLYPH_METRICS[glyphIndices.Length];
        Native.GetDesignGlyphMetrics(glyphIndices, metrics, isSideways);
        return metrics;
    }

    public unsafe bool TryGetFontTable(uint openTypeTableTag, out IntPtr tableData, out uint tableSize, out IntPtr tableContext)
    {
        Native.TryGetFontTable(openTypeTableTag, out void* tableDataPtr, out tableSize, out void* tableContextPtr, out var exists);

        tableData = (IntPtr)tableDataPtr;
        tableContext = (IntPtr)tableContextPtr;
        return exists;
    }

    public unsafe void ReleaseFontTable(IntPtr tableContext)
    {
        Native.ReleaseFontTable((void*)tableContext);
    }

    public void GetGlyphRunOutline(
        float emSize,
        ushort[] glyphIndices,
        bool isSideways,
        bool isRightToLeft,
        GeometrySink geometrySink)
    {
        Native.GetGlyphRunOutline(
            emSize,
            glyphIndices,
            default(ReadOnlySpan<float>),
            default(ReadOnlySpan<DWRITE_GLYPH_OFFSET>),
            isSideways,
            isRightToLeft,
            geometrySink.Native);
    }
}

internal sealed class DWriteTextAnalyzer : GeneratedComWrapper<IDWriteTextAnalyzer>
{
    public DWriteTextAnalyzer(IDWriteTextAnalyzer native)
        : base(native)
    {
    }

    internal new IDWriteTextAnalyzer Native => base.Native;
}
