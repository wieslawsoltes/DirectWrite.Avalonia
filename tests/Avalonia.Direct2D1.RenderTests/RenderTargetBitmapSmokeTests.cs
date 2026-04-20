using Avalonia;
using Avalonia.Direct2D1;
using Avalonia.Media.Imaging;
using Xunit;

namespace Avalonia.Direct2D1.RenderTests;

public class RenderTargetBitmapSmokeTests
{
    [Fact]
    public void Should_Create_Render_Target_Bitmap()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Direct2D1Platform.Initialize();

        using var bitmap = new RenderTargetBitmap(new PixelSize(32, 24), new Vector(96, 96));

        Assert.Equal(new PixelSize(32, 24), bitmap.PixelSize);
    }

    [Fact]
    public void Should_Create_Writeable_Bitmap()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        Direct2D1Platform.Initialize();

        using var bitmap = new WriteableBitmap(
            new PixelSize(16, 8),
            new Vector(96, 96),
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul);

        Assert.Equal(new PixelSize(16, 8), bitmap.PixelSize);
    }
}
