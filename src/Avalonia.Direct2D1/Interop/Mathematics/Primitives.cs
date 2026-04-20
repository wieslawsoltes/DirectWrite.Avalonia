using System.Runtime.InteropServices;

namespace Avalonia.Direct2D1.Interop.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public struct RawColor4
{
    public RawColor4(float r, float g, float b, float a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public float R;
    public float G;
    public float B;
    public float A;
}

[StructLayout(LayoutKind.Sequential)]
public struct RawVector2
{
    public float X;
    public float Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct RawRectangleF
{
    public RawRectangleF(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public float Left;
    public float Top;
    public float Right;
    public float Bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct RawMatrix3x2
{
    public RawMatrix3x2(float m11, float m12, float m21, float m22, float m31, float m32)
    {
        M11 = m11;
        M12 = m12;
        M21 = m21;
        M22 = m22;
        M31 = m31;
        M32 = m32;
    }

    public float M11;
    public float M12;
    public float M21;
    public float M22;
    public float M31;
    public float M32;
}
