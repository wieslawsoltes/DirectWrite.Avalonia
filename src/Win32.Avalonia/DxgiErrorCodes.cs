namespace Win32.Avalonia;

internal enum DxgiError : uint
{
    DeviceHung = 0x887A0006,
    DeviceRemoved = 0x887A0005,
    DeviceReset = 0x887A0007,
    NotCurrentlyAvailable = 0x887A0022,
}

internal static class DxgiErrorExtensions
{
    public static bool IsDeviceLostError(this int hr)
    {
        var error = unchecked((DxgiError)(uint)hr);
        return error is DxgiError.DeviceRemoved
            or DxgiError.DeviceHung
            or DxgiError.DeviceReset
            or DxgiError.NotCurrentlyAvailable;
    }
}