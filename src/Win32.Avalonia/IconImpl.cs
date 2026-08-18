using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Win32.Avalonia.Interop;

namespace Win32.Avalonia;

internal sealed class IconImpl : IWindowIconImpl, IDisposable
{
    private readonly byte[] _iconData;

    public IconImpl(Stream icon)
    {
        using var memoryStream = new MemoryStream();
        icon.CopyTo(memoryStream);
        _iconData = memoryStream.ToArray();
    }

    public Win32Icon LoadSmallIcon(double scaleFactor)
    {
        using var bitmap = LoadBitmap();
        return new Win32Icon(bitmap);
    }

    public Win32Icon LoadBigIcon(double scaleFactor)
    {
        using var bitmap = LoadBitmap();
        return new Win32Icon(bitmap);
    }

    private Bitmap LoadBitmap()
    {
        using var memoryStream = new MemoryStream(_iconData, writable: false);
        return new Bitmap(memoryStream);
    }

    public void Save(Stream outputStream)
    {
        outputStream.Write(_iconData, 0, _iconData.Length);
    }

    public void Dispose()
    {
    }
}