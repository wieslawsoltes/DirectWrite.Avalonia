using System.ComponentModel;
using System.Threading;
using Avalonia.Threading;

namespace Win32.Avalonia;

internal sealed class OleContext
{
    private static OleContext? s_current;

    public static OleContext? Current
    {
        get
        {
            if (!IsValidOleThread())
            {
                return null;
            }

            return s_current ??= new OleContext();
        }
    }

    private OleContext()
    {
        var result = OleNative.OleInitialize(nint.Zero);
        if (result != OleHResults.SOk && result != OleHResults.SFalse)
        {
            throw new Win32Exception(result, "Failed to initialize OLE.");
        }
    }

    private static bool IsValidOleThread()
        => Dispatcher.UIThread.CheckAccess() && Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;
}