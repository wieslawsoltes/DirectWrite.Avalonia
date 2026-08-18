using System.Runtime.InteropServices;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace Win32.Avalonia;

internal sealed class OleDataTransfer : IDataTransfer, IAsyncDataTransfer
{
    private readonly DataTransfer _dataTransfer;

    public OleDataTransfer(IOleDataObject dataObject)
    {
        _dataTransfer = BuildDataTransfer(dataObject);
    }

    public IReadOnlyList<DataFormat> Formats => _dataTransfer.Formats;

    public IReadOnlyList<DataTransferItem> Items => _dataTransfer.Items;

    IReadOnlyList<IDataTransferItem> IDataTransfer.Items => Items;

    IReadOnlyList<IAsyncDataTransferItem> IAsyncDataTransfer.Items => Items;

    public void Dispose()
    {
    }

    private static DataTransfer BuildDataTransfer(IOleDataObject dataObject)
    {
        var result = new DataTransfer();
        var formats = GetFormats(dataObject);
        if (formats.Count == 0)
        {
            return result;
        }

        List<DataFormat>? nonFileFormats = null;
        var supportsBitmap = false;

        foreach (var format in formats)
        {
            if (DataFormat.File.Equals(format))
            {
                if (OleDataObjectHelper.TryGet(dataObject, format) is IEnumerable<IStorageItem> files)
                {
                    foreach (var file in files)
                    {
                        result.Add(DataTransferItem.CreateFile(file));
                    }
                }

                continue;
            }

            if (ClipboardFormatRegistry.PngMimeDataFormat.Equals(format) || ClipboardFormatRegistry.PngSystemDataFormat.Equals(format))
            {
                supportsBitmap = true;
            }

            (nonFileFormats ??= []).Add(format);
        }

        if (supportsBitmap)
        {
            (nonFileFormats ??= []).Add(DataFormat.Bitmap);
        }

        if (nonFileFormats is not null)
        {
            var item = new DataTransferItem();
            foreach (var format in nonFileFormats.Distinct())
            {
                switch (format)
                {
                    case DataFormat<string> stringFormat:
                        item.Set(stringFormat, () => (string?)OleDataObjectHelper.TryGet(dataObject, stringFormat));
                        break;
                    case DataFormat<byte[]> bytesFormat:
                        item.Set(bytesFormat, () => (byte[]?)OleDataObjectHelper.TryGet(dataObject, bytesFormat));
                        break;
                    case DataFormat<Bitmap> bitmapFormat when DataFormat.Bitmap.Equals(bitmapFormat):
                        item.Set(bitmapFormat, () => (Bitmap?)OleDataObjectHelper.TryGet(dataObject, bitmapFormat));
                        break;
                }
            }

            if (item.Formats.Count > 0)
            {
                result.Add(item);
            }
        }

        return result;
    }

    private static unsafe List<DataFormat> GetFormats(IOleDataObject dataObject)
    {
        var formats = new List<DataFormat>();
        if (dataObject.EnumFormatEtc((int)OleDataDirection.Get, out var enumFormat) != OleHResults.SOk || enumFormat is null)
        {
            return formats;
        }

        while (true)
        {
            var fetched = 0u;
            var formatEtc = default(FormatEtc);
            var result = enumFormat.Next(1, &formatEtc, &fetched);
            if (result != OleHResults.SOk || fetched == 0)
            {
                break;
            }

            if (formatEtc.ptd != nint.Zero)
            {
                Marshal.FreeCoTaskMem(formatEtc.ptd);
            }

            formats.Add(ClipboardFormatRegistry.GetOrAddFormat(formatEtc.cfFormat));
        }

        return formats;
    }
}