using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static class WinRTNativeMethods
{
    public static readonly Version MinWinCompositionVersion = new(10, 0, 17134);

    [StructLayout(LayoutKind.Sequential)]
    internal struct DispatcherQueueOptions
    {
        public int Size;
        public DispatcherQueueThreadType ThreadType;
        public DispatcherQueueThreadApartmentType ApartmentType;
    }

    internal enum DispatcherQueueThreadApartmentType
    {
        ComNone = 0,
        ComAsta = 1,
        ComSta = 2,
    }

    internal enum DispatcherQueueThreadType
    {
        Dedicated = 1,
        Current = 2,
    }

    internal enum RoInitType
    {
        SingleThreaded = 0,
        MultiThreaded = 1,
    }

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", PreserveSig = false, CallingConvention = CallingConvention.StdCall)]
    private static extern nint WindowsCreateString([MarshalAs(UnmanagedType.LPWStr)] string sourceString, int length);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern unsafe char* WindowsGetStringRawBuffer(nint hstring, uint* length);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", PreserveSig = false, CallingConvention = CallingConvention.StdCall)]
    internal static extern void WindowsDeleteString(nint hstring);

    [DllImport("coremessaging.dll", PreserveSig = false)]
    internal static extern nint CreateDispatcherQueueController(DispatcherQueueOptions options);

    [DllImport("combase.dll", PreserveSig = false)]
    private static extern void RoInitialize(RoInitType initType);

    [DllImport("combase.dll", PreserveSig = false)]
    private static extern nint RoActivateInstance(nint activatableClassId);

    [DllImport("combase.dll", PreserveSig = false)]
    private static extern void RoGetActivationFactory(nint activatableClassId, in Guid iid, out nint factory);

    private static bool s_initialized;

    public static nint ActivateInstance(string fullName)
    {
        EnsureRoInitialized();
        using var hString = new HStringInterop(fullName);
        return RoActivateInstance(hString.Handle);
    }

    public static nint CreateString(string value)
        => WindowsCreateString(value, value.Length);

    public static void EnsureRoInitialized()
    {
        if (s_initialized)
        {
            return;
        }

        RoInitialize(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA
            ? RoInitType.SingleThreaded
            : RoInitType.MultiThreaded);
        s_initialized = true;
    }

    public static T GetActivationFactory<T>(string fullName, out nint factoryPointer)
        where T : class
    {
        EnsureRoInitialized();
        using var hString = new HStringInterop(fullName);
        var interfaceId = typeof(T).GUID;
        RoGetActivationFactory(hString.Handle, in interfaceId, out factoryPointer);
        return GeneratedComHelpers.ConvertToManaged<T>(factoryPointer)
            ?? throw new InvalidOperationException($"Unable to wrap the WinRT activation factory for {fullName}.");
    }

    internal sealed class HStringInterop : IDisposable
    {
        private nint _handle;

        public HStringInterop(string value)
        {
            _handle = WindowsCreateString(value, value.Length);
        }

        public nint Handle => _handle;

        public void Dispose()
        {
            if (_handle != 0)
            {
                WindowsDeleteString(_handle);
                _handle = 0;
            }
        }
    }
}