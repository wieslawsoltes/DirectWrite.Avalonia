using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Win32.Avalonia;

[GeneratedComInterface]
[Guid("00000103-0000-0000-C000-000000000046")]
internal unsafe partial interface IOleEnumFormatEtc
{
    [PreserveSig]
    uint Next(uint celt, FormatEtc* elements, uint* fetched);

    [PreserveSig]
    uint Skip(uint celt);

    [PreserveSig]
    int Reset();

    [PreserveSig]
    int Clone(out IOleEnumFormatEtc? enumFormat);
}

[GeneratedComInterface]
[Guid("0000010E-0000-0000-C000-000000000046")]
internal unsafe partial interface IOleDataObject
{
    [PreserveSig]
    uint GetData(FormatEtc* format, StgMedium* medium);

    [PreserveSig]
    uint GetDataHere(FormatEtc* format, StgMedium* medium);

    [PreserveSig]
    uint QueryGetData(FormatEtc* format);

    [PreserveSig]
    int GetCanonicalFormatEtc(FormatEtc* formatIn, FormatEtc* formatOut);

    [PreserveSig]
    uint SetData(FormatEtc* format, StgMedium* medium, [MarshalAs(UnmanagedType.Bool)] bool release);

    [PreserveSig]
    int EnumFormatEtc(int direction, out IOleEnumFormatEtc? enumFormat);

    [PreserveSig]
    int DAdvise(FormatEtc* format, int advf, nint adviseSink, out int connection);

    [PreserveSig]
    int DUnadvise(int connection);

    [PreserveSig]
    int EnumDAdvise(out nint enumAdvise);
}

[GeneratedComInterface]
[Guid("00000121-0000-0000-C000-000000000046")]
internal partial interface IOleDropSource
{
    [PreserveSig]
    int QueryContinueDrag([MarshalAs(UnmanagedType.Bool)] bool escapePressed, int keyState);

    [PreserveSig]
    int GiveFeedback(OleDropEffect effect);
}