using System;
using System.Runtime.InteropServices;

namespace Avalonia.Direct2D1.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Size2
    {
        public Size2(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width;
        public int Height;

        public static bool operator ==(Size2 left, Size2 right) =>
            left.Width == right.Width && left.Height == right.Height;

        public static bool operator !=(Size2 left, Size2 right) => !(left == right);

        public override bool Equals(object? obj) =>
            obj is Size2 other && this == other;

        public override int GetHashCode() => HashCode.Combine(Width, Height);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Size2F
    {
        public Size2F(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public float Width;
        public float Height;

        public static bool operator ==(Size2F left, Size2F right) =>
            left.Width.Equals(right.Width) && left.Height.Equals(right.Height);

        public static bool operator !=(Size2F left, Size2F right) => !(left == right);

        public override bool Equals(object? obj) =>
            obj is Size2F other && this == other;

        public override int GetHashCode() => HashCode.Combine(Width, Height);
    }
}

namespace Avalonia.Direct2D1.Interop.DXGI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SampleDescription
    {
        public int Count;
        public int Quality;
    }

    public enum Format : uint
    {
        Unknown = 0,
        B8G8R8A8_UNorm = 87
    }

    [Flags]
    public enum Usage : uint
    {
        ShaderInput = 0x10,
        RenderTargetOutput = 0x20,
        BackBuffer = 0x40,
        Shared = 0x80,
        ReadOnly = 0x100,
        DiscardOnPresent = 0x200,
        UnorderedAccess = 0x400
    }

    public enum SwapEffect
    {
        Discard = 0,
        Sequential = 1,
        FlipSequential = 3,
        FlipDiscard = 4
    }

    [Flags]
    public enum SwapChainFlags : uint
    {
        None = 0
    }

    [Flags]
    public enum PresentFlags : uint
    {
        None = 0
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SwapChainDescription1
    {
        public int Width;
        public int Height;
        public Format Format;
        public bool Stereo;
        public SampleDescription SampleDescription;
        public Usage Usage;
        public int BufferCount;
        public Scaling Scaling;
        public SwapEffect SwapEffect;
        public AlphaMode AlphaMode;
        public SwapChainFlags Flags;
    }

    public enum Scaling
    {
        Stretch = 0,
        None = 1,
        AspectRatioStretch = 2
    }

    public enum AlphaMode
    {
        Unspecified = 0,
        Premultiplied = 1,
        Straight = 2,
        Ignore = 3
    }
}
