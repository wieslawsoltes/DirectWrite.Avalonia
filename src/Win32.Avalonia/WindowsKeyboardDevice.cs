using Avalonia.Input;
using Windows.Win32;

namespace Win32.Avalonia;

internal sealed class WindowsKeyboardDevice : KeyboardDevice
{
    private const int VkLMenu = 0xA4;
    private const int VkRMenu = 0xA5;
    private const int VkLControl = 0xA2;
    private const int VkRControl = 0xA3;
    private const int VkLShift = 0xA0;
    private const int VkRShift = 0xA1;
    private const int VkLWin = 0x5B;
    private const int VkRWin = 0x5C;

    public static WindowsKeyboardDevice Instance { get; } = new();

    public unsafe RawInputModifiers Modifiers
    {
        get
        {
            Span<byte> keyStates = stackalloc byte[256];
            fixed (byte* keyStatePtr = keyStates)
            {
                PInvoke.GetKeyboardState(keyStatePtr);
            }

            var result = RawInputModifiers.None;

            if (((keyStates[VkLMenu] | keyStates[VkRMenu]) & 0x80) != 0)
            {
                result |= RawInputModifiers.Alt;
            }

            if (((keyStates[VkLControl] | keyStates[VkRControl]) & 0x80) != 0)
            {
                result |= RawInputModifiers.Control;
            }

            if (((keyStates[VkLShift] | keyStates[VkRShift]) & 0x80) != 0)
            {
                result |= RawInputModifiers.Shift;
            }

            if (((keyStates[VkLWin] | keyStates[VkRWin]) & 0x80) != 0)
            {
                result |= RawInputModifiers.Meta;
            }

            return result;
        }
    }

    private WindowsKeyboardDevice()
    {
    }
}