using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls.Platform;
using Avalonia.Logging;
using Avalonia.Threading;

namespace Win32.Avalonia;

internal sealed class WindowsMountedVolumeInfoListener : IDisposable
{
    private readonly IDisposable _subscription;
    private readonly ObservableCollection<MountedVolumeInfo> _mountedDrives;
    private bool _disposed;

    public WindowsMountedVolumeInfoListener(ObservableCollection<MountedVolumeInfo> mountedDrives)
    {
        _mountedDrives = mountedDrives;
        _subscription = DispatcherTimer.Run(Poll, TimeSpan.FromSeconds(1));
        Poll();
    }

    private bool Poll()
    {
        var mountVolInfos = DriveInfo.GetDrives()
            .Where(drive =>
            {
                try
                {
                    _ = drive.IsReady;
                    _ = drive.TotalSize;
                    return drive.IsReady;
                }
                catch (Exception exception)
                {
                    Logger.TryGet(LogEventLevel.Warning, LogArea.Control)
                        ?.Log(this, $"Error in Windows drive enumeration: {exception.Message}");
                    return false;
                }
            })
            .Select(drive => new MountedVolumeInfo
            {
                VolumeLabel = string.IsNullOrWhiteSpace(drive.VolumeLabel)
                    ? drive.RootDirectory.FullName
                    : $"{drive.VolumeLabel} ({drive.Name})",
                VolumePath = drive.RootDirectory.FullName,
                VolumeSizeBytes = (ulong)drive.TotalSize,
            })
            .ToArray();

        if (_mountedDrives.SequenceEqual(mountVolInfos))
        {
            return true;
        }

        _mountedDrives.Clear();
        foreach (var info in mountVolInfos)
        {
            _mountedDrives.Add(info);
        }

        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _subscription.Dispose();
        _disposed = true;
    }
}