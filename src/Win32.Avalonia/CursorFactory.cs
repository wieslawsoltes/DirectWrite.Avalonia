using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Win32.Avalonia.Interop;

namespace Win32.Avalonia;

internal sealed class CursorFactory : ICursorFactory
{
    private static readonly Dictionary<StandardCursorType, int> CursorTypeMapping = new()
    {
        { StandardCursorType.None, 0 },
        { StandardCursorType.AppStarting, 32650 },
        { StandardCursorType.Arrow, 32512 },
        { StandardCursorType.Cross, 32515 },
        { StandardCursorType.Hand, 32649 },
        { StandardCursorType.Help, 32651 },
        { StandardCursorType.Ibeam, 32513 },
        { StandardCursorType.No, 32648 },
        { StandardCursorType.SizeAll, 32646 },
        { StandardCursorType.UpArrow, 32516 },
        { StandardCursorType.SizeNorthSouth, 32645 },
        { StandardCursorType.SizeWestEast, 32644 },
        { StandardCursorType.Wait, 32514 },
        { StandardCursorType.TopSide, 32645 },
        { StandardCursorType.BottomSide, 32645 },
        { StandardCursorType.LeftSide, 32644 },
        { StandardCursorType.RightSide, 32644 },
        { StandardCursorType.TopLeftCorner, 32642 },
        { StandardCursorType.BottomRightCorner, 32642 },
        { StandardCursorType.TopRightCorner, 32643 },
        { StandardCursorType.BottomLeftCorner, 32643 },
        { StandardCursorType.DragMove, 32516 },
        { StandardCursorType.DragCopy, 32516 },
        { StandardCursorType.DragLink, 32516 },
    };

    private static readonly Dictionary<StandardCursorType, CursorImpl> Cache = new();

    public static CursorFactory Instance { get; } = new();

    static CursorFactory()
    {
        LoadModuleCursor(StandardCursorType.DragMove, "ole32.dll", 2);
        LoadModuleCursor(StandardCursorType.DragCopy, "ole32.dll", 3);
        LoadModuleCursor(StandardCursorType.DragLink, "ole32.dll", 4);
    }

    private CursorFactory()
    {
    }

    public ICursorImpl CreateCursor(Bitmap cursor, PixelPoint hotSpot)
        => new CursorImpl(new Win32Icon(cursor, hotSpot));

    public ICursorImpl GetCursor(StandardCursorType cursorType)
    {
        if (!Cache.TryGetValue(cursorType, out var cursor))
        {
            cursor = new CursorImpl(WindowNative.LoadCursor(nint.Zero, new nint(CursorTypeMapping[cursorType])));
            Cache.Add(cursorType, cursor);
        }

        return cursor;
    }

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint GetModuleHandle(string moduleName);

    private static void LoadModuleCursor(StandardCursorType cursorType, string module, int id)
    {
        var moduleHandle = GetModuleHandle(module);
        if (moduleHandle == nint.Zero)
        {
            return;
        }

        var cursorHandle = WindowNative.LoadCursor(moduleHandle, new nint(id));
        if (cursorHandle != nint.Zero)
        {
            Cache[cursorType] = new CursorImpl(cursorHandle);
        }
    }
}

internal sealed class CursorImpl : ICursorImpl, IPlatformHandle
{
    private const string CursorHandleType = "HCURSOR";

    private Win32Icon? _icon;

    public CursorImpl(Win32Icon icon)
        : this(icon.Handle)
    {
        _icon = icon;
    }

    public CursorImpl(nint handle)
    {
        Handle = handle;
    }

    public nint Handle { get; private set; }

    public string HandleDescriptor => CursorHandleType;

    public void Dispose()
    {
        if (_icon is null)
        {
            return;
        }

        _icon.Dispose();
        _icon = null;
        Handle = nint.Zero;
    }
}