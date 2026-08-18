using Avalonia.Input;

namespace Win32.Avalonia;

internal sealed class BlockingAsyncDataTransfer(IAsyncDataTransfer asyncDataTransfer) : IDataTransfer, IAsyncDataTransfer
{
    private readonly IAsyncDataTransfer _asyncDataTransfer = asyncDataTransfer;
    private BlockingAsyncDataTransferItem[]? _items;

    public IReadOnlyList<DataFormat> Formats => _asyncDataTransfer.Formats;

    public IReadOnlyList<BlockingAsyncDataTransferItem> Items => _items ??= CreateItems();

    IReadOnlyList<IDataTransferItem> IDataTransfer.Items => Items;

    IReadOnlyList<IAsyncDataTransferItem> IAsyncDataTransfer.Items => _asyncDataTransfer.Items;

    public void Dispose() => _asyncDataTransfer.Dispose();

    private BlockingAsyncDataTransferItem[] CreateItems()
    {
        var asyncItems = _asyncDataTransfer.Items;
        var items = new BlockingAsyncDataTransferItem[asyncItems.Count];
        for (var index = 0; index < asyncItems.Count; index++)
        {
            items[index] = new BlockingAsyncDataTransferItem(asyncItems[index]);
        }

        return items;
    }
}

internal sealed class BlockingAsyncDataTransferItem(IAsyncDataTransferItem asyncItem) : IDataTransferItem
{
    private readonly IAsyncDataTransferItem _asyncItem = asyncItem;

    public IReadOnlyList<DataFormat> Formats => _asyncItem.Formats;

    public object? TryGetRaw(DataFormat format)
        => _asyncItem.TryGetRawAsync(format).GetAwaiter().GetResult();
}