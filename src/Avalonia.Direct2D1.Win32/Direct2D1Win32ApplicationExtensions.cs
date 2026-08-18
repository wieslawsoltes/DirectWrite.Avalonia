using Win32.Avalonia;

namespace Avalonia.Direct2D1.Win32;

public static class Direct2D1Win32ApplicationExtensions
{
    public static AppBuilder UseWin32Direct2D1(this AppBuilder builder, Win32Direct2D1PlatformOptions? options = null)
    {
        options ??= new Win32Direct2D1PlatformOptions();

        return builder
            .With(options.Direct2D1)
            .UseWin32Avalonia(options.ToWin32AvaloniaOptions())
            .UseDirect2D1();
    }
}
