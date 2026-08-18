using Avalonia.Platform.Storage;

namespace Win32.Avalonia;

internal abstract class LocalStorageItem(FileSystemInfo fileSystemInfo) : IStorageItem
{
    protected FileSystemInfo FileSystemInfo { get; } = fileSystemInfo;

    public string Name => FileSystemInfo.Name;

    public Uri Path => new(FileSystemInfo.FullName);

    public bool CanBookmark => true;

    public Task<StorageItemProperties> GetBasicPropertiesAsync()
        => Task.FromResult(FileSystemInfo.Exists
            ? new StorageItemProperties(
                FileSystemInfo is FileInfo fileInfo ? (ulong)fileInfo.Length : 0,
                FileSystemInfo.CreationTimeUtc,
                FileSystemInfo.LastWriteTimeUtc)
            : new StorageItemProperties());

    public Task<string?> SaveBookmarkAsync()
        => Task.FromResult<string?>(FileSystemInfo.FullName);

    public Task<IStorageFolder?> GetParentAsync()
        => Task.FromResult<IStorageFolder?>(GetParent());

    public Task DeleteAsync()
    {
        if (FileSystemInfo is DirectoryInfo directoryInfo)
        {
            directoryInfo.Delete(true);
        }
        else
        {
            FileSystemInfo.Delete();
        }

        return Task.CompletedTask;
    }

    public Task<IStorageItem?> MoveAsync(IStorageFolder destination)
    {
        if (!destination.Path.IsFile)
        {
            var targetPath = System.IO.Path.Combine(destination.Path.LocalPath, FileSystemInfo.Name);
            if (FileSystemInfo is DirectoryInfo directory)
            {
                directory.MoveTo(targetPath);
                return Task.FromResult<IStorageItem?>(new LocalStorageFolder(new DirectoryInfo(targetPath)));
            }

            ((FileInfo)FileSystemInfo).MoveTo(targetPath);
            return Task.FromResult<IStorageItem?>(new LocalStorageFile(new FileInfo(targetPath)));
        }

        return Task.FromResult<IStorageItem?>(null);
    }

    public void Dispose()
    {
    }

    private LocalStorageFolder? GetParent()
        => FileSystemInfo switch
        {
            DirectoryInfo { Parent: { } parent } => new LocalStorageFolder(parent),
            FileInfo { Directory: { } directory } => new LocalStorageFolder(directory),
            _ => null,
        };
}

internal sealed class LocalStorageFile(FileInfo fileInfo) : LocalStorageItem(fileInfo), IStorageFile
{
    private FileInfo FileInfo => (FileInfo)FileSystemInfo;

    public Task<Stream> OpenReadAsync()
        => Task.FromResult<Stream>(FileInfo.OpenRead());

    public Task<Stream> OpenWriteAsync()
        => Task.FromResult<Stream>(new FileStream(FileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Write));
}

internal sealed class LocalStorageFolder(DirectoryInfo directoryInfo) : LocalStorageItem(directoryInfo), IStorageFolder
{
    private DirectoryInfo DirectoryInfo => (DirectoryInfo)FileSystemInfo;

    public async IAsyncEnumerable<IStorageItem> GetItemsAsync()
    {
        foreach (var directory in DirectoryInfo.EnumerateDirectories())
        {
            yield return new LocalStorageFolder(directory);
            await Task.Yield();
        }

        foreach (var file in DirectoryInfo.EnumerateFiles())
        {
            yield return new LocalStorageFile(file);
            await Task.Yield();
        }
    }

    public Task<IStorageFolder?> GetFolderAsync(string name)
    {
        var path = System.IO.Path.Combine(DirectoryInfo.FullName, name);
        return Task.FromResult<IStorageFolder?>(Directory.Exists(path) ? new LocalStorageFolder(new DirectoryInfo(path)) : null);
    }

    public Task<IStorageFile?> GetFileAsync(string name)
    {
        var path = System.IO.Path.Combine(DirectoryInfo.FullName, name);
        return Task.FromResult<IStorageFile?>(File.Exists(path) ? new LocalStorageFile(new FileInfo(path)) : null);
    }

    public Task<IStorageFile?> CreateFileAsync(string name)
    {
        var path = System.IO.Path.Combine(DirectoryInfo.FullName, name);
        using var stream = File.Create(path);
        return Task.FromResult<IStorageFile?>(new LocalStorageFile(new FileInfo(path)));
    }

    public Task<IStorageFolder?> CreateFolderAsync(string name)
        => Task.FromResult<IStorageFolder?>(new LocalStorageFolder(DirectoryInfo.CreateSubdirectory(name)));
}