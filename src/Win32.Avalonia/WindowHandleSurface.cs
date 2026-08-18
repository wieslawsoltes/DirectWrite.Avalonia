using Avalonia;
using Avalonia.Platform;

namespace Win32.Avalonia;

internal sealed class WindowHandleSurface(Func<nint> handle, Func<PixelSize> size, Func<double> scaling) : INativePlatformHandleSurface
{
    private readonly Func<nint> _handle = handle;
    private readonly Func<PixelSize> _size = size;
    private readonly Func<double> _scaling = scaling;

    public nint Handle => _handle();

    IntPtr IPlatformHandle.Handle => Handle;

    public string? HandleDescriptor => "HWND";

    public PixelSize Size => _size();

    public double Scaling => _scaling();
}