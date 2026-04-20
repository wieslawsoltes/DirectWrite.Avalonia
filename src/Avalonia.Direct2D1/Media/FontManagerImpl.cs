using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Avalonia.Direct2D1.Interop;
using Avalonia.Media;
using Avalonia.Platform;
using FontFamily = Avalonia.Media.FontFamily;
using FontStretch = Avalonia.Media.FontStretch;
using FontStyle = Avalonia.Media.FontStyle;
using FontWeight = Avalonia.Media.FontWeight;

namespace Avalonia.Direct2D1.Media;

internal class FontManagerImpl : IFontManagerImpl
{
    public string GetDefaultFontFamilyName()
    {
        return "Segoe UI";
    }

    public string[] GetInstalledFontFamilyNames(bool checkForUpdates = false)
    {
        var familyCount = Direct2D1FontCollectionCache.InstalledFontCollection.FontFamilyCount;
        var fontFamilies = new string[(int)familyCount];

        for (uint i = 0; i < familyCount; i++)
        {
            using var family = Direct2D1FontCollectionCache.InstalledFontCollection.GetFontFamily(i);
            fontFamilies[(int)i] = family.FamilyName;
        }

        return fontFamilies;
    }

    public bool TryMatchCharacter(
        int codepoint,
        FontStyle fontStyle,
        FontWeight fontWeight,
        FontStretch fontStretch,
        string? familyName,
        CultureInfo? culture,
        [NotNullWhen(returnValue: true)] out IPlatformTypeface? platformTypeface)
    {
        if (!string.IsNullOrWhiteSpace(familyName)
            && TryCreateGlyphTypeface(familyName, fontStyle, fontWeight, fontStretch, out platformTypeface)
            && ((GlyphTypefaceImpl)platformTypeface).Font.HasCharacter((uint)codepoint))
        {
            return true;
        }

        var familyCount = Direct2D1FontCollectionCache.InstalledFontCollection.FontFamilyCount;

        for (uint i = 0; i < familyCount; i++)
        {
            using var family = Direct2D1FontCollectionCache.InstalledFontCollection.GetFontFamily(i);
            using var fonts = family.GetMatchingFonts((int)fontWeight, (DWRITE_FONT_STRETCH)fontStretch, (DWRITE_FONT_STYLE)fontStyle);

            if (fonts.FontCount == 0)
            {
                continue;
            }

            var font = fonts.GetFont(0);

            if (!font.HasCharacter((uint)codepoint))
            {
                font.Dispose();
                continue;
            }

            platformTypeface = new GlyphTypefaceImpl(font);
            return true;
        }

        platformTypeface = null;
        return false;
    }

    public bool TryCreateGlyphTypeface(
        string familyName,
        FontStyle style,
        FontWeight weight,
        FontStretch stretch,
        [NotNullWhen(returnValue: true)] out IPlatformTypeface? platformTypeface)
    {
        var systemFonts = Direct2D1FontCollectionCache.InstalledFontCollection;

        if (familyName == FontFamily.DefaultFontFamilyName)
        {
            familyName = "Segoe UI";
        }

        if (systemFonts.FindFamilyName(familyName, out var index))
        {
            using var family = systemFonts.GetFontFamily(index);
            var font = family.GetFirstMatchingFont((int)weight, (DWRITE_FONT_STRETCH)stretch, (DWRITE_FONT_STYLE)style);

            platformTypeface = new GlyphTypefaceImpl(font);
            return true;
        }

        platformTypeface = null;
        return false;
    }

    public bool TryCreateGlyphTypeface(
        Stream stream,
        FontSimulations fontSimulations,
        [NotNullWhen(returnValue: true)] out IPlatformTypeface? platformTypeface)
    {
        platformTypeface = null;

        var customFontCollection = Direct2D1Platform.DirectWriteFactory.CreateCustomFontCollection([stream]);

        try
        {
            if (customFontCollection.FontCollection.FontFamilyCount == 0)
            {
                customFontCollection.Dispose();
                return false;
            }

            using var fontFamily = customFontCollection.FontCollection.GetFontFamily(0);

            if (fontFamily.FontCount == 0)
            {
                customFontCollection.Dispose();
                return false;
            }

            var font = fontFamily.GetFont(0);
            platformTypeface = new GlyphTypefaceImpl(font, customFontCollection);
            return true;
        }
        catch
        {
            customFontCollection.Dispose();
            throw;
        }
    }

    public bool TryGetFamilyTypefaces(string familyName, [NotNullWhen(true)] out IReadOnlyList<Typeface>? familyTypefaces)
    {
        familyTypefaces = null;

        var systemFonts = Direct2D1FontCollectionCache.InstalledFontCollection;

        if (!systemFonts.FindFamilyName(familyName, out var index))
        {
            return false;
        }

        using var family = systemFonts.GetFontFamily(index);
        var typefaces = new List<Typeface>((int)family.FontCount);

        for (uint i = 0; i < family.FontCount; i++)
        {
            using var font = family.GetFont(i);
            typefaces.Add(new Typeface(
                familyName,
                (FontStyle)font.Style,
                (FontWeight)font.Weight,
                (FontStretch)font.Stretch));
        }

        familyTypefaces = typefaces;
        return true;
    }
}
