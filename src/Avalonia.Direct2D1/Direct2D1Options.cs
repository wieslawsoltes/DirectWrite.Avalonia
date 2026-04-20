namespace Avalonia.Direct2D1;

public sealed class Direct2D1Options
{
    public bool UseHardwareAcceleration { get; set; } = true;

    public bool UseWarpFallback { get; set; } = true;

    public bool EnableTextAntialiasing { get; set; } = true;

    public bool EnableTextHinting { get; set; } = true;

    public long? MaxResourceBytes { get; set; }

    public bool EnableDiagnostics { get; set; }
}
