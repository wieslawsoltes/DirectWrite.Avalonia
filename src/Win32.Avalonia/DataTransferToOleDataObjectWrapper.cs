using Avalonia.Input;
using System.Runtime.InteropServices.Marshalling;

namespace Win32.Avalonia;

internal sealed class DataTransferToOleDataObjectWrapper(IDataTransfer dataTransfer)
    : GeneratedComWrapper<IOleDataObject>(new DataTransferToOleDataObject(dataTransfer))
{
    private DataTransferToOleDataObject DataObject => (DataTransferToOleDataObject)Native;

    public bool IsDisposed => DataObject.DataTransfer is null;

    public IDataTransfer? DataTransfer => DataObject.DataTransfer;

    public nint DataObjectPointer => NativePointer;

    public void ReleaseDataTransfer() => DataObject.ReleaseDataTransfer();
}

[GeneratedComClass]
internal unsafe partial class DataTransferToOleDataObject : IOleDataObject
{
    [GeneratedComClass]
    private sealed unsafe partial class FormatEnumerator : IOleEnumFormatEtc
    {
        private readonly FormatEtc[] _formats;
        private uint _current;

        public FormatEnumerator(ushort[] formatIds)
        {
            _formats = formatIds.Select(OleDataObjectHelper.ToFormatEtc).ToArray();
        }

        private FormatEnumerator(FormatEtc[] formats, uint current)
        {
            _formats = formats;
            _current = current;
        }

        public uint Next(uint celt, FormatEtc* elements, uint* fetched)
        {
            if (elements is null)
            {
                return OleHResults.DvEFormatEtc;
            }

            if (celt != 1 && fetched is null)
            {
                return OleHResults.DvEFormatEtc;
            }

            uint count = 0;
            while (count < celt && _current < _formats.Length)
            {
                elements[count] = _formats[_current];
                _current++;
                count++;
            }

            if (fetched is not null)
            {
                *fetched = count;
            }

            return count == celt ? OleHResults.SOk : (uint)OleHResults.SFalse;
        }

        public uint Skip(uint celt)
        {
            _current = Math.Min((uint)_formats.Length, _current + celt);
            return _current < _formats.Length ? OleHResults.SOk : (uint)OleHResults.SFalse;
        }

        public int Reset()
        {
            _current = 0;
            return OleHResults.SOk;
        }

        public int Clone(out IOleEnumFormatEtc? enumFormat)
        {
            enumFormat = new FormatEnumerator(_formats, _current);
            return OleHResults.SOk;
        }
    }

    private readonly ushort[] _formatIds;

    public DataTransferToOleDataObject(IDataTransfer dataTransfer)
    {
        DataTransfer = dataTransfer;
        _formatIds = CalculateFormatIds();
    }

    public IDataTransfer? DataTransfer { get; private set; }

    public uint GetData(FormatEtc* format, StgMedium* medium)
    {
        if (!ValidateFormat(format, out var result, out var dataFormat))
        {
            return result;
        }

        *medium = default;
        medium->tymed = OleTymed.HGlobal;
        return OleDataObjectHelper.WriteDataToHGlobal(DataTransfer!, dataFormat!, ref medium->unionmember);
    }

    public uint GetDataHere(FormatEtc* format, StgMedium* medium)
    {
        if (!ValidateFormat(format, out var result, out var dataFormat))
        {
            return result;
        }

        if (medium->unionmember == nint.Zero)
        {
            return OleHResults.StgEMediumFull;
        }

        return OleDataObjectHelper.WriteDataToHGlobal(DataTransfer!, dataFormat!, ref medium->unionmember);
    }

    public uint QueryGetData(FormatEtc* format)
        => ValidateFormat(format, out var result, out _) ? OleHResults.SOk : result;

    public int GetCanonicalFormatEtc(FormatEtc* formatIn, FormatEtc* formatOut)
        => OleHResults.NotImpl;

    public uint SetData(FormatEtc* format, StgMedium* medium, bool release)
        => unchecked((uint)OleHResults.NotImpl);

    public int EnumFormatEtc(int direction, out IOleEnumFormatEtc? enumFormat)
    {
        enumFormat = null;
        if (DataTransfer is null || direction != (int)OleDataDirection.Get)
        {
            return OleHResults.NotImpl;
        }

        enumFormat = new FormatEnumerator(_formatIds);
        return OleHResults.SOk;
    }

    public int DAdvise(FormatEtc* format, int advf, nint adviseSink, out int connection)
    {
        connection = 0;
        return OleHResults.SOk;
    }

    public int DUnadvise(int connection)
        => OleHResults.OleEAdviseNotSupported;

    public int EnumDAdvise(out nint enumAdvise)
    {
        enumAdvise = nint.Zero;
        return OleHResults.OleEAdviseNotSupported;
    }

    public void ReleaseDataTransfer()
    {
        DataTransfer?.Dispose();
        DataTransfer = null;
    }

    private bool ValidateFormat(FormatEtc* format, out uint result, out DataFormat? dataFormat)
    {
        dataFormat = null;
        if (format is null || (format->tymed & OleTymed.HGlobal) == 0)
        {
            result = OleHResults.DvETymed;
            return false;
        }

        if (format->dwAspect != OleAspect.Content)
        {
            result = OleHResults.DvEDvaspect;
            return false;
        }

        if (DataTransfer is null)
        {
            result = OleHResults.CorEObjectDisposed;
            return false;
        }

        if (Array.IndexOf(_formatIds, format->cfFormat) < 0)
        {
            result = OleHResults.DvEFormatEtc;
            return false;
        }

        dataFormat = ClipboardFormatRegistry.GetOrAddFormat(format->cfFormat);
        result = OleHResults.SOk;
        return true;
    }

    private ushort[] CalculateFormatIds()
    {
        if (DataTransfer is null)
        {
            return [];
        }

        var ids = new List<ushort>(DataTransfer.Formats.Count);
        foreach (var format in DataTransfer.Formats)
        {
            if (DataFormat.Bitmap.Equals(format))
            {
                ids.Add(ClipboardFormatRegistry.GetOrAddFormat(ClipboardFormatRegistry.PngMimeDataFormat));
                ids.Add(ClipboardFormatRegistry.GetOrAddFormat(ClipboardFormatRegistry.PngSystemDataFormat));
            }
            else
            {
                ids.Add(ClipboardFormatRegistry.GetOrAddFormat(format));
            }
        }

        return ids.ToArray();
    }
}