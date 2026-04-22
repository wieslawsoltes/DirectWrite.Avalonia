using global::Avalonia;

namespace Win32.Avalonia;

public static class Win32ApplicationExtensions
{
    public static AppBuilder UseWin32Avalonia(this AppBuilder builder, Win32PlatformOptions? options = null)
    {
        options ??= new Win32PlatformOptions();

        return builder
            .With(options)
            .With(options.ToAvaloniaOptions())
            .UseStandardRuntimePlatformSubsystem()
            .UseWindowingSubsystem(
                () => Win32Platform.Initialize(
                    global::Avalonia.AvaloniaLocator.Current.GetService<Win32PlatformOptions>() ?? options),
                "Win32.Avalonia");
    }
}