using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Xunit;

namespace Avalonia.Direct2D1.UnitTests.Media
{
    public class TextShaperImplTests
    {
        [Fact]
        public void Should_Create_Current_TextShaper()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Direct2D1TestServices.Initialize();

            var textShaper = TextShaper.Current;

            Assert.NotNull(textShaper);
        }

        [Fact]
        public void Should_Shape_Simple_Text()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Direct2D1TestServices.Initialize();

            var glyphTypeface = new Typeface(new FontFamily("Arial")).GlyphTypeface;
            using var shapedBuffer = TextShaper.Current.ShapeText(
                "Hello".AsMemory(),
                new TextShaperOptions(glyphTypeface, 12));

            Assert.Equal(5, shapedBuffer.Length);

            foreach (var glyphInfo in shapedBuffer)
            {
                Assert.NotEqual(0, glyphInfo.GlyphIndex);
                Assert.InRange(glyphInfo.GlyphCluster, 0, 4);
            }
        }
    }
}
