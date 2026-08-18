using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;

namespace Win32.Avalonia;

[StructLayout(LayoutKind.Sequential)]
internal struct DCompositionPoint
{
    public int X;
    public int Y;
}

[GeneratedComInterface]
[Guid("75F6468D-1B8E-447C-9BC6-75FEA80B5B25")]
internal partial interface IDCompositionDevice2Com
{
    [PreserveSig]
    int Commit();

    [PreserveSig]
    int WaitForCommitCompletion();

    [PreserveSig]
    int GetFrameStatistics(nint statistics);

    [PreserveSig]
    int CreateVisual(out nint visual);

    [PreserveSig]
    int CreateSurfaceFactory(nint renderingDevice, out nint surfaceFactory);

    [PreserveSig]
    int CreateSurface(uint width, uint height, DxgiFormat pixelFormat, DxgiAlphaMode alphaMode, out nint surface);

    [PreserveSig]
    int CreateVirtualSurface(uint initialWidth, uint initialHeight, DxgiFormat pixelFormat, DxgiAlphaMode alphaMode, out nint virtualSurface);

    [PreserveSig]
    int CreateTranslateTransform(out nint translateTransform);

    [PreserveSig]
    int CreateScaleTransform(out nint scaleTransform);

    [PreserveSig]
    int CreateRotateTransform(out nint rotateTransform);

    [PreserveSig]
    int CreateSkewTransform(out nint skewTransform);

    [PreserveSig]
    int CreateMatrixTransform(out nint matrixTransform);

    [PreserveSig]
    int CreateTransformGroup(nint transforms, int elements, out nint transformGroup);

    [PreserveSig]
    int CreateTranslateTransform3D(out nint translateTransform3D);

    [PreserveSig]
    int CreateScaleTransform3D(out nint scaleTransform3D);

    [PreserveSig]
    int CreateRotateTransform3D(out nint rotateTransform3D);

    [PreserveSig]
    int CreateMatrixTransform3D(out nint matrixTransform3D);

    [PreserveSig]
    int CreateTransform3DGroup(nint transforms3D, int elements, out nint transform3DGroup);

    [PreserveSig]
    int CreateEffectGroup(out nint effectGroup);

    [PreserveSig]
    int CreateRectangleClip(out nint clip);

    [PreserveSig]
    int CreateAnimation(out nint animation);
}

[GeneratedComInterface]
[Guid("5F4633FE-1E08-4CB8-8C75-CE24333F5602")]
internal partial interface IDCompositionDesktopDeviceCom : IDCompositionDevice2Com
{
    [PreserveSig]
    int CreateTargetForHwnd(nint hwnd, [MarshalAs(UnmanagedType.Bool)] bool topmost, out nint target);

    [PreserveSig]
    int CreateSurfaceFromHandle(nint handle, out nint surface);

    [PreserveSig]
    int CreateSurfaceFromHwnd(nint hwnd, out nint surface);
}

[GeneratedComInterface]
[Guid("4D93059D-097B-4651-9A60-F0F25116E2F3")]
internal partial interface IDCompositionVisualCom
{
    [PreserveSig]
    int SetOffsetXAnimation(nint animation);

    [PreserveSig]
    int SetOffsetX(float offsetX);

    [PreserveSig]
    int SetOffsetYAnimation(nint animation);

    [PreserveSig]
    int SetOffsetY(float offsetY);

    [PreserveSig]
    int SetTransform(nint transform);

    [PreserveSig]
    int SetTransformMatrix(nint matrix);

    [PreserveSig]
    int SetTransformParent(nint visual);

    [PreserveSig]
    int SetEffect(nint effect);

    [PreserveSig]
    int SetBitmapInterpolationMode(int interpolationMode);

    [PreserveSig]
    int SetBorderMode(int borderMode);

    [PreserveSig]
    int SetClip(nint clip);

    [PreserveSig]
    int SetClipRect(nint rect);

    [PreserveSig]
    int SetContent(nint content);

    [PreserveSig]
    int AddVisual(nint visual, int insertAbove, nint referenceVisual);

    [PreserveSig]
    int RemoveVisual(nint visual);

    [PreserveSig]
    int RemoveAllVisuals();

    [PreserveSig]
    int SetCompositeMode(int compositeMode);
}

[GeneratedComInterface]
[Guid("EACDD04C-117E-4E17-88F4-D1B12B0E3D89")]
internal partial interface IDCompositionTargetCom
{
    [PreserveSig]
    int SetRoot(nint visual);
}

[GeneratedComInterface]
[Guid("E334BC12-3937-4E02-85EB-FCF4EB30D2C8")]
internal partial interface IDCompositionSurfaceFactoryCom
{
    [PreserveSig]
    int CreateSurface(uint width, uint height, DxgiFormat pixelFormat, DxgiAlphaMode alphaMode, out nint surface);

    [PreserveSig]
    int CreateVirtualSurface(uint initialWidth, uint initialHeight, DxgiFormat pixelFormat, DxgiAlphaMode alphaMode, out nint virtualSurface);
}

[GeneratedComInterface]
[Guid("BB8A4953-2C99-4F5A-96F5-4819027FA3AC")]
internal partial interface IDCompositionSurfaceCom
{
    [PreserveSig]
    int BeginDraw(in RECT updateRect, in Guid interfaceId, out nint updateObject, out DCompositionPoint updateOffset);

    [PreserveSig]
    int EndDraw();

    [PreserveSig]
    int SuspendDraw();

    [PreserveSig]
    int ResumeDraw();

    [PreserveSig]
    int Scroll(nint scrollRect, nint clipRect, int offsetX, int offsetY);
}

[GeneratedComInterface]
[Guid("AE471C51-5F53-4A24-8D3E-D0C39C30B3F0")]
internal partial interface IDCompositionVirtualSurfaceCom : IDCompositionSurfaceCom
{
    [PreserveSig]
    int Resize(uint width, uint height);

    [PreserveSig]
    int Trim(nint rectangles, int count);
}