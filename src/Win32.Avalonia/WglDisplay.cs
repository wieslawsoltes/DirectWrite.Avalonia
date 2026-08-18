using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.Threading;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;

namespace Win32.Avalonia;

internal static unsafe class WglDisplay
{
    private static bool? s_initialized;
    private static nint s_bootstrapContext;
    private static nint s_bootstrapWindow;
    private static nint s_bootstrapDc;
    private static PIXELFORMATDESCRIPTOR s_defaultPfd;
    private static int s_defaultPixelFormat;
    private static delegate* unmanaged[Stdcall]<nint, int*, float*, int, int*, int*, int> s_wglChoosePixelFormatArb;
    private static delegate* unmanaged[Stdcall]<nint, nint, int*, nint> s_wglCreateContextAttribsArb;

    private static bool Initialize() => s_initialized ??= InitializeCore();

    private static bool InitializeCore()
    {
        Dispatcher.UIThread.VerifyAccess();

        s_bootstrapWindow = WglGdiResourceManager.CreateOffscreenWindow();
        s_bootstrapDc = WglGdiResourceManager.GetDc(s_bootstrapWindow);
        s_defaultPfd = new PIXELFORMATDESCRIPTOR
        {
            nSize = (ushort)Marshal.SizeOf<PIXELFORMATDESCRIPTOR>(),
            nVersion = 1,
            dwFlags = PFD_FLAGS.PFD_DRAW_TO_WINDOW | PFD_FLAGS.PFD_SUPPORT_OPENGL | PFD_FLAGS.PFD_DOUBLEBUFFER,
            iPixelType = PFD_PIXEL_TYPE.PFD_TYPE_RGBA,
            cColorBits = 32,
            cDepthBits = 24,
            cStencilBits = 8,
            iLayerType = PFD_LAYER_TYPE.PFD_MAIN_PLANE,
        };

        s_defaultPixelFormat = PInvoke.ChoosePixelFormat(new HDC(s_bootstrapDc), s_defaultPfd);
        if (s_defaultPixelFormat == 0 || !PInvoke.SetPixelFormat(new HDC(s_bootstrapDc), s_defaultPixelFormat, s_defaultPfd))
        {
            return false;
        }

        s_bootstrapContext = WglNative.CreateContext(s_bootstrapDc);
        if (s_bootstrapContext == nint.Zero || !WglNative.MakeCurrent(s_bootstrapDc, s_bootstrapContext))
        {
            return false;
        }

        var createContextProc = WglNative.GetProcAddress("wglCreateContextAttribsARB");
        var choosePixelFormatProc = WglNative.GetProcAddress("wglChoosePixelFormatARB");
        if (createContextProc == nint.Zero || choosePixelFormatProc == nint.Zero)
        {
            WglNative.MakeCurrent(nint.Zero, nint.Zero);
            return false;
        }

        s_wglCreateContextAttribsArb = (delegate* unmanaged[Stdcall]<nint, nint, int*, nint>)createContextProc;
        s_wglChoosePixelFormatArb = (delegate* unmanaged[Stdcall]<nint, int*, float*, int, int*, int*, int>)choosePixelFormatProc;

        var attributes = stackalloc int[]
        {
            WglConsts.WglDrawToWindowArb, 1,
            WglConsts.WglAccelerationArb, WglConsts.WglFullAccelerationArb,
            WglConsts.WglSupportOpenGlArb, 1,
            WglConsts.WglDoubleBufferArb, 1,
            WglConsts.WglPixelTypeArb, WglConsts.WglTypeRgbaArb,
            WglConsts.WglColorBitsArb, 32,
            WglConsts.WglAlphaBitsArb, 8,
            WglConsts.WglDepthBitsArb, 0,
            WglConsts.WglStencilBitsArb, 0,
            0,
        };
        var formats = stackalloc int[1];
        var formatCount = 0;
        if (s_wglChoosePixelFormatArb(s_bootstrapDc, attributes, null, 1, formats, &formatCount) != 0 && formatCount != 0)
        {
            PInvoke.DescribePixelFormat(new HDC(s_bootstrapDc), formats[0], out var pfd);
            s_defaultPfd = pfd;
            s_defaultPixelFormat = formats[0];
        }

        WglNative.MakeCurrent(nint.Zero, nint.Zero);
        return true;
    }

    public static WglContext? CreateContext(IEnumerable<GlVersion> versions, IGlContext? share)
    {
        if (!Initialize())
        {
            return null;
        }

        var sharedContext = share as WglContext;
        using var _ = new WglRestoreContext(s_bootstrapDc, s_bootstrapContext, null);

        var window = WglGdiResourceManager.CreateOffscreenWindow();
        var dc = WglGdiResourceManager.GetDc(window);
        if (!PInvoke.SetPixelFormat(new HDC(dc), s_defaultPixelFormat, s_defaultPfd))
        {
            WglGdiResourceManager.ReleaseDc(window, dc);
            WglGdiResourceManager.DestroyWindow(window);
            return null;
        }

        var contextAttributes = stackalloc int[8];
        foreach (var version in versions)
        {
            if (version.Type != GlProfileType.OpenGL)
            {
                continue;
            }

            nint context;
            using (sharedContext?.Lock())
            {
                var profileMask = WglConsts.WglContextCoreProfileBitArb;
                if (version.IsCompatibilityProfile && (version.Major > 3 || version.Major == 3 && version.Minor >= 2))
                {
                    profileMask = WglConsts.WglContextCompatibilityProfileBitArb;
                }

                contextAttributes[0] = WglConsts.WglContextMajorVersionArb;
                contextAttributes[1] = version.Major;
                contextAttributes[2] = WglConsts.WglContextMinorVersionArb;
                contextAttributes[3] = version.Minor;
                contextAttributes[4] = WglConsts.WglContextProfileMaskArb;
                contextAttributes[5] = profileMask;
                contextAttributes[6] = 0;
                contextAttributes[7] = 0;
                context = s_wglCreateContextAttribsArb(dc, sharedContext?.Handle ?? nint.Zero, contextAttributes);
            }

            if (context != nint.Zero)
            {
                return new WglContext(sharedContext, version, context, window, dc, s_defaultPixelFormat, s_defaultPfd);
            }
        }

        WglGdiResourceManager.ReleaseDc(window, dc);
        WglGdiResourceManager.DestroyWindow(window);
        return null;
    }
}