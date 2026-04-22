using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;

namespace Win32.Avalonia;

internal sealed class Win32AngleEglInterface : EglInterface
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate nint EglCreateDeviceAngleDelegate(int deviceType, nint nativeDevice, nint attribs);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate void EglReleaseDeviceAngleDelegate(nint device);

    [DllImport("av_libGLESv2.dll", CharSet = CharSet.Ansi)]
    private static extern nint EGL_GetProcAddress(string proc);

    private readonly EglCreateDeviceAngleDelegate _createDeviceAngle;
    private readonly EglReleaseDeviceAngleDelegate _releaseDeviceAngle;

    public Win32AngleEglInterface()
        : this(LoadAngle())
    {
    }

    private Win32AngleEglInterface(Func<string, nint> getProcAddress)
        : base(getProcAddress)
    {
        _createDeviceAngle = GetProcDelegate<EglCreateDeviceAngleDelegate>(getProcAddress, "eglCreateDeviceANGLE");
        _releaseDeviceAngle = GetProcDelegate<EglReleaseDeviceAngleDelegate>(getProcAddress, "eglReleaseDeviceANGLE");
    }

    public nint CreateDeviceANGLE(int deviceType, nint nativeDevice, int[]? attribs)
    {
        if (attribs is null)
        {
            return _createDeviceAngle(deviceType, nativeDevice, nint.Zero);
        }

        var handle = GCHandle.Alloc(attribs, GCHandleType.Pinned);
        try
        {
            return _createDeviceAngle(deviceType, nativeDevice, handle.AddrOfPinnedObject());
        }
        finally
        {
            handle.Free();
        }
    }

    public void ReleaseDeviceANGLE(nint device)
        => _releaseDeviceAngle(device);

    private static Func<string, nint> LoadAngle()
    {
        if (OperatingSystem.IsWindows())
        {
            var displayProc = EGL_GetProcAddress("eglGetPlatformDisplayEXT");
            if (displayProc == nint.Zero)
            {
                throw new OpenGlException("libegl.dll doesn't have eglGetPlatformDisplayEXT entry point");
            }

            return EGL_GetProcAddress;
        }

        throw new PlatformNotSupportedException();
    }

    private static T GetProcDelegate<T>(Func<string, nint> getProcAddress, string name)
        where T : Delegate
    {
        var address = getProcAddress(name);
        if (address == nint.Zero)
        {
            throw new OpenGlException($"ANGLE does not expose {name}");
        }

        return Marshal.GetDelegateForFunctionPointer<T>(address);
    }
}