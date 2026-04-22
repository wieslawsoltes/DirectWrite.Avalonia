namespace Win32.Avalonia;

internal static class OffscreenParentWindow
{
    private static readonly SimpleWindow s_simpleWindow = new(null);

    public static nint Handle => s_simpleWindow.Handle;
}