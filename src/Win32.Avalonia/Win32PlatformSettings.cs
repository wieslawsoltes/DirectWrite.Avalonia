using Avalonia.Input;
using Avalonia.Platform;

namespace Win32.Avalonia;

internal sealed class Win32PlatformSettings : DefaultPlatformSettings
{
    private PlatformColorValues? _lastColorValues;

    public override PlatformColorValues GetColorValues()
    {
        _lastColorValues ??= base.GetColorValues();
        return _lastColorValues ?? base.GetColorValues();
    }

    internal void OnColorValuesChanged()
    {
        var oldColorValues = _lastColorValues;
        var colorValues = GetColorValues();

        if (oldColorValues != colorValues)
        {
            OnColorValuesChanged(colorValues);
        }
    }
}