using System.Collections.ObjectModel;
using Avalonia.Controls.Platform;

namespace Win32.Avalonia;

internal sealed class WindowsMountedVolumeInfoProvider : IMountedVolumeInfoProvider
{
    public IDisposable Listen(ObservableCollection<MountedVolumeInfo> mountedDrives)
        => new WindowsMountedVolumeInfoListener(mountedDrives);
}