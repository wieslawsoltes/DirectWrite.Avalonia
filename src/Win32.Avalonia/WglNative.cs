using System.Runtime.InteropServices;
using System.Text;

namespace Win32.Avalonia;

internal static unsafe class WglNative
{
    private static readonly nint s_openGl32 = NativeLibrary.Load("opengl32.dll");
    private static readonly delegate* unmanaged[Stdcall]<nint, nint> s_wglCreateContext =
        (delegate* unmanaged[Stdcall]<nint, nint>)NativeLibrary.GetExport(s_openGl32, "wglCreateContext");
    private static readonly delegate* unmanaged[Stdcall]<nint, int> s_wglDeleteContext =
        (delegate* unmanaged[Stdcall]<nint, int>)NativeLibrary.GetExport(s_openGl32, "wglDeleteContext");
    private static readonly delegate* unmanaged[Stdcall]<nint, nint, int> s_wglMakeCurrent =
        (delegate* unmanaged[Stdcall]<nint, nint, int>)NativeLibrary.GetExport(s_openGl32, "wglMakeCurrent");
    private static readonly delegate* unmanaged[Stdcall]<byte*, nint> s_wglGetProcAddress =
        (delegate* unmanaged[Stdcall]<byte*, nint>)NativeLibrary.GetExport(s_openGl32, "wglGetProcAddress");
    private static readonly delegate* unmanaged[Stdcall]<nint> s_wglGetCurrentContext =
        (delegate* unmanaged[Stdcall]<nint>)NativeLibrary.GetExport(s_openGl32, "wglGetCurrentContext");
    private static readonly delegate* unmanaged[Stdcall]<nint> s_wglGetCurrentDc =
        (delegate* unmanaged[Stdcall]<nint>)NativeLibrary.GetExport(s_openGl32, "wglGetCurrentDC");

    public static nint OpenGl32Handle => s_openGl32;

    public static nint CreateContext(nint dc) => s_wglCreateContext(dc);

    public static bool DeleteContext(nint context) => s_wglDeleteContext(context) != 0;

    public static bool MakeCurrent(nint dc, nint context) => s_wglMakeCurrent(dc, context) != 0;

    public static nint GetCurrentContext() => s_wglGetCurrentContext();

    public static nint GetCurrentDc() => s_wglGetCurrentDc();

    public static nint GetProcAddress(string name)
    {
        var bytes = Encoding.ASCII.GetBytes(name + '\0');
        fixed (byte* namePointer = bytes)
        {
            return s_wglGetProcAddress(namePointer);
        }
    }

    public static bool TryGetModuleExport(string name, out nint address)
        => NativeLibrary.TryGetExport(s_openGl32, name, out address);
}