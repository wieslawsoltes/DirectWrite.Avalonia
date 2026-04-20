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
                Assert.True(glyphInfo.GlyphAdvance > 0);
            }
        }

        [Fact]
        public void Should_Create_TextLayout_With_Positive_Metrics()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            Direct2D1TestServices.Initialize();

            using var layout = new TextLayout(
                "Direct2D1 text rendering",
                Typeface.Default,
                24,
                Brushes.Black);

            Assert.True(layout.Width > 0);
            Assert.True(layout.Height > 0);
            Assert.True(layout.TextLines.Count > 0);
            Assert.True(layout.TextLines[0].Width > 0);
        }
    }
}
