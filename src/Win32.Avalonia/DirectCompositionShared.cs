namespace Win32.Avalonia;

internal sealed class DirectCompositionShared : IDisposable
{
    private readonly nint _devicePointer;

    public DirectCompositionShared(nint devicePointer)
    {
        _devicePointer = devicePointer;
        Device = GeneratedComHelpers.ConvertToManaged<IDCompositionDesktopDeviceCom>(devicePointer)
            ?? throw new InvalidOperationException("Unable to wrap the DirectComposition desktop device.");
    }

    public object SyncRoot { get; } = new();

    public IDCompositionDesktopDeviceCom Device { get; }

    public void Dispose()
        => GeneratedComHelpers.Free<IDCompositionDesktopDeviceCom>(_devicePointer);
}