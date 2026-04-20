using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Fonts;
using Xunit;

namespace Avalonia.Direct2D1.UnitTests.Media
{
    public class FontManagerImplTests
    {
        [Fact]
        public void Should_Create_Typeface_From_Fallback()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Direct2D1TestServices.Initialize();

            var typeface = new Typeface(new FontFamily("A, B, Arial"));
            var glyphTypeface = typeface.GlyphTypeface;

            Assert.Equal("Arial", glyphTypeface.FamilyName);
        }

        [Fact]
        public void Should_Create_Typeface_From_Fallback_Bold()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Direct2D1TestServices.Initialize();

            var typeface = new Typeface(new FontFamily("A, B, Arial"), weight: FontWeight.Bold);
            var glyphTypeface = typeface.GlyphTypeface;

            Assert.Equal("Arial", glyphTypeface.FamilyName);
            Assert.Equal(FontWeight.Bold, glyphTypeface.Weight);
            Assert.Equal(FontStyle.Normal, glyphTypeface.Style);
        }

        [Fact]
        public void Should_Create_Typeface_For_Unknown_Font()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Direct2D1TestServices.Initialize();

            var glyphTypeface = new Typeface(new FontFamily("Unknown")).GlyphTypeface;
            var defaultName = FontManager.Current.DefaultFontFamily.Name;

            Assert.Equal(defaultName, glyphTypeface.FamilyName);
        }

        [Fact]
        public void Should_Create_Typeface_From_Stream()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            var fontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            var fontPath = Path.Combine(fontsPath, "arial.ttf");

            if (!File.Exists(fontPath))
            {
                return;
            }

            Direct2D1TestServices.Initialize();

            using var stream = File.OpenRead(fontPath);
            var fontCollection = new TestFontCollection();

            Assert.True(fontCollection.TryAddGlyphTypeface(stream, out var glyphTypeface));
            Assert.NotNull(glyphTypeface);
            Assert.False(string.IsNullOrWhiteSpace(glyphTypeface.FamilyName));
        }

        private sealed class TestFontCollection : FontCollectionBase
        {
            public override Uri Key { get; } = new("fonts:unit-test");
        }
    }
}
