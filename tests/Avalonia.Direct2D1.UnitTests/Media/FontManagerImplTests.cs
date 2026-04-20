using Avalonia;
using Avalonia.Media;
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
    }
}
