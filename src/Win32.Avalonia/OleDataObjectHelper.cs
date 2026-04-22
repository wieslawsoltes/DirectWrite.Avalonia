using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Logging;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace Win32.Avalonia;

internal static unsafe class OleDataObjectHelper
{
    private const int GmemMoveable = 0x0002;
    private const int GmemZeroinit = 0x0040;

    public static FormatEtc ToFormatEtc(ushort formatId)
        => new()
        {
            cfFormat = formatId,
            dwAspect = OleAspect.Content,
            lindex = -1,
            tymed = OleTymed.HGlobal,
        };

    public static object? TryGet(IOleDataObject oleDataObject, DataFormat format)
    {
        if (TryGetContainedFormat(oleDataObject, format) is not { } formatId)
        {
            return null;
        }

        var medium = new StgMedium();
        var formatEtc = ToFormatEtc(formatId);
        if (oleDataObject.GetData(&formatEtc, &medium) != OleHResults.SOk)
        {
            return null;
        }

        try
        {
            if (medium.unionmember == nint.Zero || medium.tymed != OleTymed.HGlobal)
            {
                return null;
            }

            return ReadDataFromHGlobal(format, medium.unionmember);
        }
        finally
        {
            OleNative.ReleaseStgMedium(ref medium);
        }
    }

    public static uint WriteDataToHGlobal(IDataTransfer dataTransfer, DataFormat format, ref nint hGlobal)
    {
        if (DataFormat.Text.Equals(format))
        {
            return WriteStringToHGlobal(ref hGlobal, dataTransfer.TryGetValue(DataFormat.Text) ?? string.Empty);
        }

        if (DataFormat.File.Equals(format))
        {
            var files = dataTransfer.TryGetValues(DataFormat.File) ?? [];
            var fileNames = files.Select(GetLocalPath).OfType<string>();
            return WriteFileNamesToHGlobal(ref hGlobal, fileNames);
        }

        if (ClipboardFormatRegistry.PngSystemDataFormat.Equals(format) || ClipboardFormatRegistry.PngMimeDataFormat.Equals(format))
        {
            if (dataTransfer.TryGetValue(DataFormat.Bitmap) is { } bitmap)
            {
                using var stream = new MemoryStream();
                bitmap.Save(stream);
                return WriteBytesToHGlobal(ref hGlobal, stream.ToArray());
            }

            return OleHResults.DvEFormatEtc;
        }

        if (format is DataFormat<string> stringFormat)
        {
            return dataTransfer.TryGetValue(stringFormat) is { } value
                ? WriteStringToHGlobal(ref hGlobal, value)
                : OleHResults.DvEFormatEtc;
        }

        if (format is DataFormat<byte[]> bytesFormat)
        {
            return dataTransfer.TryGetValue(bytesFormat) is { } value
                ? WriteBytesToHGlobal(ref hGlobal, value)
                : OleHResults.DvEFormatEtc;
        }

        Logger.TryGet(LogEventLevel.Warning, "Win32")?.Log(null, "Unsupported OLE data format {Format}", format);
        return OleHResults.DvEFormatEtc;
    }

    private static ushort? TryGetContainedFormat(IOleDataObject oleDataObject, DataFormat format)
    {
        if (DataFormat.Bitmap.Equals(format))
        {
            foreach (var imageFormat in new[] { ClipboardFormatRegistry.PngMimeDataFormat, ClipboardFormatRegistry.PngSystemDataFormat })
            {
                if (TryGetContainedFormatCore(oleDataObject, imageFormat) is { } imageFormatId)
                {
                    return imageFormatId;
                }
            }

            return null;
        }

        return TryGetContainedFormatCore(oleDataObject, format);
    }

    private static ushort? TryGetContainedFormatCore(IOleDataObject oleDataObject, DataFormat format)
    {
        var formatId = ClipboardFormatRegistry.GetOrAddFormat(format);
        var formatEtc = ToFormatEtc(formatId);
        return oleDataObject.QueryGetData(&formatEtc) == OleHResults.SOk ? formatId : null;
    }

    private static object? ReadDataFromHGlobal(DataFormat format, nint hGlobal)
    {
        if (DataFormat.Text.Equals(format))
        {
            return ReadStringFromHGlobal(hGlobal);
        }

        if (DataFormat.File.Equals(format))
        {
            return ReadFileNamesFromHGlobal(hGlobal)
                .Select(CreateStorageItem)
                .Where(item => item is not null)
                .Cast<IStorageItem>()
                .ToArray();
        }

        if (DataFormat.Bitmap.Equals(format))
        {
            var bytes = ReadBytesFromHGlobal(hGlobal);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }

        if (ClipboardFormatRegistry.PngSystemDataFormat.Equals(format) || ClipboardFormatRegistry.PngMimeDataFormat.Equals(format))
        {
            return ReadBytesFromHGlobal(hGlobal);
        }

        if (format is DataFormat<string>)
        {
            return ReadStringFromHGlobal(hGlobal);
        }

        if (format is DataFormat<byte[]>)
        {
            return ReadBytesFromHGlobal(hGlobal);
        }

        return null;
    }

    private static LocalStorageItem? CreateStorageItem(string path)
    {
        if (Directory.Exists(path))
        {
            return new LocalStorageFolder(new DirectoryInfo(path));
        }

        return File.Exists(path) ? new LocalStorageFile(new FileInfo(path)) : null;
    }

    private static string? GetLocalPath(IStorageItem item)
        => item.Path.IsAbsoluteUri ? item.Path.LocalPath : null;

    private static string? ReadStringFromHGlobal(nint hGlobal)
    {
        var source = OleNative.GlobalLock(hGlobal);
        try
        {
            return Marshal.PtrToStringUni(source);
        }
        finally
        {
            OleNative.GlobalUnlock(hGlobal);
        }
    }

    private static byte[] ReadBytesFromHGlobal(nint hGlobal)
    {
        var source = OleNative.GlobalLock(hGlobal);
        try
        {
            var size = checked((int)OleNative.GlobalSize(hGlobal));
            var data = new byte[size];
            Marshal.Copy(source, data, 0, size);
            return data;
        }
        finally
        {
            OleNative.GlobalUnlock(hGlobal);
        }
    }

    private static List<string> ReadFileNamesFromHGlobal(nint hGlobal)
    {
        var fileCount = OleNative.DragQueryFile(hGlobal, 0xFFFFFFFF, null, 0);
        var files = new List<string>((int)fileCount);
        for (uint index = 0; index < fileCount; index++)
        {
            var length = OleNative.DragQueryFile(hGlobal, index, null, 0);
            var buffer = new char[length + 1];
            fixed (char* bufferPointer = buffer)
            {
                if (OleNative.DragQueryFile(hGlobal, index, bufferPointer, (uint)buffer.Length) == length)
                {
                    files.Add(new string(buffer, 0, (int)length));
                }
            }
        }

        return files;
    }

    private static uint WriteStringToHGlobal(ref nint hGlobal, string data)
    {
        var requiredSize = checked((nuint)((data.Length + 1) * sizeof(char)));
        if (!EnsureGlobalBuffer(ref hGlobal, requiredSize, out var destination))
        {
            return OleHResults.StgEMediumFull;
        }

        try
        {
            Span<char> span = new((void*)destination, (int)(requiredSize / sizeof(char)));
            data.AsSpan().CopyTo(span);
            span[data.Length] = '\0';
            return OleHResults.SOk;
        }
        finally
        {
            OleNative.GlobalUnlock(hGlobal);
        }
    }

    private static uint WriteFileNamesToHGlobal(ref nint hGlobal, IEnumerable<string> fileNames)
    {
        var joined = string.Join('\0', fileNames) + "\0\0";
        var dropFiles = new DropFiles
        {
            pFiles = (uint)Marshal.SizeOf<DropFiles>(),
            fWide = 1,
        };
        var requiredSize = checked((nuint)(Marshal.SizeOf<DropFiles>() + joined.Length * sizeof(char)));
        if (!EnsureGlobalBuffer(ref hGlobal, requiredSize, out var destination))
        {
            return OleHResults.StgEMediumFull;
        }

        try
        {
            var bytes = new Span<byte>((void*)destination, (int)requiredSize);
            MemoryMarshal.Write(bytes, in dropFiles);
            var chars = MemoryMarshal.Cast<byte, char>(bytes[Marshal.SizeOf<DropFiles>()..]);
            joined.AsSpan().CopyTo(chars);
            return OleHResults.SOk;
        }
        finally
        {
            OleNative.GlobalUnlock(hGlobal);
        }
    }

    private static uint WriteBytesToHGlobal(ref nint hGlobal, ReadOnlySpan<byte> data)
    {
        var requiredSize = checked((nuint)data.Length);
        if (!EnsureGlobalBuffer(ref hGlobal, requiredSize, out var destination))
        {
            return OleHResults.StgEMediumFull;
        }

        try
        {
            data.CopyTo(new Span<byte>((void*)destination, data.Length));
            return OleHResults.SOk;
        }
        finally
        {
            OleNative.GlobalUnlock(hGlobal);
        }
    }

    private static bool EnsureGlobalBuffer(ref nint hGlobal, nuint requiredSize, out nint destination)
    {
        if (hGlobal == nint.Zero)
        {
            hGlobal = OleNative.GlobalAlloc(GmemMoveable | GmemZeroinit, requiredSize);
        }

        destination = nint.Zero;
        if (hGlobal == nint.Zero || OleNative.GlobalSize(hGlobal) < requiredSize)
        {
            return false;
        }

        destination = OleNative.GlobalLock(hGlobal);
        return destination != nint.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DropFiles
    {
        public uint pFiles;
        public int x;
        public int y;
        public int fNc;
        public int fWide;
    }
}