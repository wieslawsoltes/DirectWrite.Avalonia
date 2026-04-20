using System;
using System.Collections.Concurrent;
using System.Linq;
using Avalonia.Direct2D1.Interop;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Avalonia.Platform;
using FontFamily = Avalonia.Media.FontFamily;

namespace Avalonia.Direct2D1.Media;

internal static class Direct2D1FontCollectionCache
{
    private static readonly ConcurrentDictionary<FontFamilyKey, DWriteCustomFontCollection> s_cachedCollections;
    internal static readonly DWriteFontCollection InstalledFontCollection;

    static Direct2D1FontCollectionCache()
    {
        s_cachedCollections = new ConcurrentDictionary<FontFamilyKey, DWriteCustomFontCollection>();
        InstalledFontCollection = Direct2D1Platform.DirectWriteFactory.GetSystemFontCollection(checkForUpdates: false);
    }

    public static DWriteFont GetFont(Typeface typeface)
    {
        var fontFamily = typeface.FontFamily;
        var fontCollection = GetOrAddFontCollection(fontFamily);

        foreach (var name in fontFamily.FamilyNames)
        {
            if (fontCollection.FindFamilyName(name, out var index))
            {
                using var family = fontCollection.GetFontFamily(index);
                return family.GetFirstMatchingFont(
                    (int)typeface.Weight,
                    (DWRITE_FONT_STRETCH)typeface.Stretch,
                    (DWRITE_FONT_STYLE)typeface.Style);
            }
        }

        InstalledFontCollection.FindFamilyName("Segoe UI", out var defaultIndex);

        using var defaultFamily = InstalledFontCollection.GetFontFamily(defaultIndex);

        return defaultFamily.GetFirstMatchingFont(
            (int)typeface.Weight,
            (DWRITE_FONT_STRETCH)typeface.Stretch,
            (DWRITE_FONT_STYLE)typeface.Style);
    }

    private static DWriteFontCollection GetOrAddFontCollection(FontFamily fontFamily)
    {
        return fontFamily.Key is null
            ? InstalledFontCollection
            : s_cachedCollections.GetOrAdd(fontFamily.Key, CreateFontCollection).FontCollection;
    }

    private static DWriteCustomFontCollection CreateFontCollection(FontFamilyKey key)
    {
        var source = key.BaseUri != null ? new Uri(key.BaseUri, key.Source) : key.Source;
        var assets = Direct2D1FontFamilyLoader.LoadFontAssets(source).ToArray();
        var assetLoader = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
        var streams = assets.Select(asset => assetLoader.Open(asset)).ToArray();

        try
        {
            return Direct2D1Platform.DirectWriteFactory.CreateCustomFontCollection(streams);
        }
        finally
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }
    }
}
