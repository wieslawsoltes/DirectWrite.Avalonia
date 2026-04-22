using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Input;
using Avalonia.Utilities;

namespace Win32.Avalonia;

internal static class ClipboardFormatRegistry
{
    private const string AppPrefix = "avn-app-fmt:";
    private static readonly List<(DataFormat Format, ushort Id)> s_formats = [];

    public static readonly DataFormat PngSystemDataFormat = DataFormat.CreateBytesPlatformFormat("PNG");
    public static readonly DataFormat PngMimeDataFormat = DataFormat.CreateBytesPlatformFormat("image/png");
    public static readonly DataFormat HBitmapDataFormat = DataFormat.CreateBytesPlatformFormat("CF_BITMAP");
    public static readonly DataFormat DibDataFormat = DataFormat.CreateBytesPlatformFormat("CF_DIB");
    public static readonly DataFormat DibV5DataFormat = DataFormat.CreateBytesPlatformFormat("CF_DIBV5");

    public static readonly DataFormat[] ImageFormats = [PngMimeDataFormat, PngSystemDataFormat, DibDataFormat, DibV5DataFormat, HBitmapDataFormat];

    static ClipboardFormatRegistry()
    {
        AddDataFormat(DataFormat.Text, (ushort)ClipboardStandardFormat.UnicodeText);
        AddDataFormat(DataFormat.File, (ushort)ClipboardStandardFormat.HDrop);
        AddDataFormat(DibDataFormat, (ushort)ClipboardStandardFormat.Dib);
        AddDataFormat(DibV5DataFormat, (ushort)ClipboardStandardFormat.DibV5);
        AddDataFormat(HBitmapDataFormat, (ushort)ClipboardStandardFormat.Bitmap);
    }

    public static DataFormat GetOrAddFormat(ushort id)
    {
        lock (s_formats)
        {
            for (var index = 0; index < s_formats.Count; index++)
            {
                if (s_formats[index].Id == id)
                {
                    return s_formats[index].Format;
                }
            }

            var systemName = OleNative.GetClipboardFormatName(id) ?? Enum.GetName(typeof(ClipboardStandardFormat), id) ?? $"Unknown_Format_{id}";
            var format = DataFormat.FromSystemName<byte[]>(systemName, AppPrefix);
            AddDataFormat(format, id);
            return format;
        }
    }

    public static ushort GetOrAddFormat(DataFormat format)
    {
        Debug.Assert(format != DataFormat.Bitmap);

        lock (s_formats)
        {
            for (var index = 0; index < s_formats.Count; index++)
            {
                if (s_formats[index].Format.Equals(format))
                {
                    return s_formats[index].Id;
                }
            }

            var systemName = format.ToSystemName(AppPrefix);
            var registered = OleNative.RegisterClipboardFormat(systemName);
            if (registered == 0)
            {
                throw new Win32Exception();
            }

            var id = (ushort)registered;
            AddDataFormat(format, id);
            return id;
        }
    }

    private static void AddDataFormat(DataFormat format, ushort id)
        => s_formats.Add((format, id));
}