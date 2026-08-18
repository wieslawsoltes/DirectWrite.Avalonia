using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static class OleInterfaceIds
{
    public static readonly Guid IEnumFormatEtc = new("00000103-0000-0000-C000-000000000046");
    public static readonly Guid IDataObject = new("0000010E-0000-0000-C000-000000000046");
    public static readonly Guid IDropSource = new("00000121-0000-0000-C000-000000000046");
    public static readonly Guid IDropTarget = new("00000122-0000-0000-C000-000000000046");
}

internal static class OleHResults
{
    public const int SOk = 0;
    public const int SFalse = 1;
    public const int NotImpl = unchecked((int)0x80004001);
    public const int OleEAdviseNotSupported = unchecked((int)0x80040003);
    public const uint DvEFormatEtc = 0x80040064;
    public const uint DvETymed = 0x80040069;
    public const uint DvEDvaspect = 0x8004006B;
    public const uint StgEMediumFull = 0x80030070;
    public const uint CorEObjectDisposed = 0x80131622;
}

[Flags]
internal enum OleDropEffect : uint
{
    None = 0,
    Copy = 1,
    Move = 2,
    Link = 4,
    Scroll = 0x80000000,
}

internal enum OleDataDirection : uint
{
    Get = 1,
    Set = 2,
}

internal enum OleAspect : uint
{
    Content = 1,
    Thumbnail = 2,
    Icon = 4,
    DocPrint = 8,
}

[Flags]
internal enum OleTymed : uint
{
    Null = 0,
    HGlobal = 1,
    File = 2,
    IStream = 4,
    IStorage = 8,
    Gdi = 16,
    MfPict = 32,
    EnhMf = 64,
}

internal enum ClipboardStandardFormat : ushort
{
    Bitmap = 2,
    Dib = 8,
    UnicodeText = 13,
    HDrop = 15,
    DibV5 = 17,
}

[StructLayout(LayoutKind.Sequential)]
internal struct FormatEtc
{
    public ushort cfFormat;
    public nint ptd;
    public OleAspect dwAspect;
    public int lindex;
    public OleTymed tymed;
}

[StructLayout(LayoutKind.Sequential)]
internal struct StgMedium
{
    public OleTymed tymed;
    public nint unionmember;
    public nint pUnkForRelease;
}