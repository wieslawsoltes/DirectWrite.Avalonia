using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Win32.Avalonia;

internal enum WinRTTrustLevel
{
    BaseTrust,
    PartialTrust,
    FullTrust,
}

internal enum WinRTPropertyType
{
    Empty = 0,
    UInt8 = 1,
    Int16 = 2,
    UInt16 = 3,
    Int32 = 4,
    UInt32 = 5,
    Int64 = 6,
    UInt64 = 7,
    Single = 8,
    Double = 9,
    Char16 = 10,
    Boolean = 11,
    String = 12,
    Inspectable = 13,
    DateTime = 14,
    TimeSpan = 15,
    Guid = 16,
    Point = 17,
    Size = 18,
    Rect = 19,
    OtherType = 20,
    UInt8Array = 1025,
    Int16Array = 1026,
    UInt16Array = 1027,
    Int32Array = 1028,
    UInt32Array = 1029,
    Int64Array = 1030,
    UInt64Array = 1031,
    SingleArray = 1032,
    DoubleArray = 1033,
    Char16Array = 1034,
    BooleanArray = 1035,
    StringArray = 1036,
    InspectableArray = 1037,
    DateTimeArray = 1038,
    TimeSpanArray = 1039,
    GuidArray = 1040,
    PointArray = 1041,
    SizeArray = 1042,
    RectArray = 1043,
    OtherTypeArray = 1044,
}

internal enum WinRTGraphicsEffectPropertyMapping
{
    Unknown,
    Direct,
    VectorX,
    VectorY,
    VectorZ,
    VectorW,
    RectToVector4,
    RadiansToDegrees,
    ColorMatrixAlphaMode,
    ColorToVector3,
    ColorToVector4,
}

internal enum WinRTAsyncStatus
{
    Started = 0,
    Completed = 1,
    Canceled = 2,
    Error = 3,
}

[Flags]
internal enum WinRTCompositionBatchTypes
{
    None = 0x0,
    Animation = 0x1,
    Effect = 0x2,
    InfiniteAnimation = 0x4,
    AllAnimations = 0x5,
}

internal enum WinRTDirectXAlphaMode
{
    Unspecified,
    Premultiplied,
    Straight,
    Ignore,
}

internal enum WinRTDirectXPixelFormat
{
    Unknown = 0,
    B8G8R8A8UIntNormalized = 87,
}

internal enum WinRTCompositionBitmapInterpolationMode
{
    NearestNeighbor,
    Linear,
    MagLinearMinLinearMipLinear,
    MagLinearMinLinearMipNearest,
    MagLinearMinNearestMipLinear,
    MagLinearMinNearestMipNearest,
    MagNearestMinLinearMipLinear,
    MagNearestMinLinearMipNearest,
    MagNearestMinNearestMipLinear,
    MagNearestMinNearestMipNearest,
}

internal enum WinRTCompositionStretch
{
    None,
    Fill,
    Uniform,
    UniformToFill,
}

[StructLayout(LayoutKind.Sequential)]
internal struct WinRTPoint
{
    public int X;
    public int Y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WinRTSize
{
    public int X;
    public int Y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WinRTVector2
{
    public float X;
    public float Y;
}

[GeneratedComInterface]
[Guid("AF86E2E0-B12D-4C6A-9C5A-D7AA65101E90")]
internal partial interface IInspectableCom
{
    [PreserveSig]
    int GetIids(out ulong iidCount, out nint iids);

    [PreserveSig]
    int GetRuntimeClassName(out nint className);

    [PreserveSig]
    int GetTrustLevel(out WinRTTrustLevel trustLevel);
}

[GeneratedComInterface]
[Guid("A4ED5C81-76C9-40BD-8BE6-B1D90FB20AE7")]
internal partial interface IAsyncActionCompletedHandlerCom
{
    [PreserveSig]
    int Invoke(IAsyncActionCom? asyncInfo, WinRTAsyncStatus asyncStatus);
}

[GeneratedComInterface]
[Guid("5A648006-843A-4DA9-865B-9D26E5DFAD7B")]
internal partial interface IAsyncActionCom : IInspectableCom
{
    [PreserveSig]
    int SetCompleted(IAsyncActionCompletedHandlerCom? handler);

    [PreserveSig]
    int GetCompleted(out IAsyncActionCompletedHandlerCom? completedHandler);

    [PreserveSig]
    int GetResults();
}

[GeneratedComInterface]
[Guid("B403CA50-7F8C-4E83-985F-CC45060036D8")]
internal partial interface ICompositorCom : IInspectableCom
{
    [PreserveSig] int CreateColorKeyFrameAnimation(out nint result);
    [PreserveSig] int CreateColorBrush(out nint result);
    [PreserveSig] int CreateColorBrushWithColor(nint color, out nint result);
    [PreserveSig] int CreateContainerVisual(out nint result);
    [PreserveSig] int CreateCubicBezierEasingFunction(in WinRTVector2 controlPoint1, in WinRTVector2 controlPoint2, out nint result);
    [PreserveSig] int CreateEffectFactory(nint graphicsEffect, out nint result);
    [PreserveSig] int CreateEffectFactoryWithProperties(nint graphicsEffect, nint animatableProperties, out nint result);
    [PreserveSig] int CreateExpressionAnimation(out nint result);
    [PreserveSig] int CreateExpressionAnimationWithExpression(nint expression, out nint result);
    [PreserveSig] int CreateInsetClip(out nint result);
    [PreserveSig] int CreateInsetClipWithInsets(float leftInset, float topInset, float rightInset, float bottomInset, out nint result);
    [PreserveSig] int CreateLinearEasingFunction(out nint result);
    [PreserveSig] int CreatePropertySet(out nint result);
    [PreserveSig] int CreateQuaternionKeyFrameAnimation(out nint result);
    [PreserveSig] int CreateScalarKeyFrameAnimation(out nint result);
    [PreserveSig] int CreateScopedBatch(WinRTCompositionBatchTypes batchType, out nint result);
    [PreserveSig] int CreateSpriteVisual(out nint result);
    [PreserveSig] int CreateSurfaceBrush(out nint result);
    [PreserveSig] int CreateSurfaceBrushWithSurface(nint surface, out nint result);
    [PreserveSig] int CreateTargetForCurrentView(out nint result);
    [PreserveSig] int CreateVector2KeyFrameAnimation(out nint result);
    [PreserveSig] int CreateVector3KeyFrameAnimation(out nint result);
    [PreserveSig] int CreateVector4KeyFrameAnimation(out nint result);
    [PreserveSig] int GetCommitBatch(WinRTCompositionBatchTypes batchType, out nint result);
}

[GeneratedComInterface]
[Guid("735081DC-5E24-45DA-A38F-E32CC349A9A0")]
internal partial interface ICompositor2Com : IInspectableCom
{
    [PreserveSig] int CreateAmbientLight(out nint result);
    [PreserveSig] int CreateAnimationGroup(out nint result);
    [PreserveSig] int CreateBackdropBrush(out nint result);
    [PreserveSig] int CreateDistantLight(out nint result);
    [PreserveSig] int CreateDropShadow(out nint result);
    [PreserveSig] int CreateImplicitAnimationCollection(out nint result);
    [PreserveSig] int CreateLayerVisual(out nint result);
    [PreserveSig] int CreateMaskBrush(out nint result);
    [PreserveSig] int CreateNineGridBrush(out nint result);
    [PreserveSig] int CreatePointLight(out nint result);
    [PreserveSig] int CreateSpotLight(out nint result);
    [PreserveSig] int CreateStepEasingFunction(out nint result);
    [PreserveSig] int CreateStepEasingFunctionWithStepCount(int stepCount, out nint result);
}

[GeneratedComInterface]
[Guid("C9DD8EF0-6EB1-4E3C-A658-675D9C64D4AB")]
internal partial interface ICompositor3Com : IInspectableCom
{
    [PreserveSig] int CreateHostBackdropBrush(out nint result);
}

[GeneratedComInterface]
[Guid("0D8FB190-F122-5B8D-9FDD-543B0D8EB7F3")]
internal partial interface ICompositorWithBlurredWallpaperBackdropBrushCom : IInspectableCom
{
    [PreserveSig] int TryCreateBlurredWallpaperBackdropBrush(out nint result);
}

[GeneratedComInterface]
[Guid("48EA31AD-7FCD-4076-A79C-90CC4B852C9B")]
internal partial interface ICompositor5Com : IInspectableCom
{
    [PreserveSig] int GetComment(out nint value);
    [PreserveSig] int SetComment(nint value);
    [PreserveSig] int GetGlobalPlaybackRate(out float value);
    [PreserveSig] int SetGlobalPlaybackRate(float value);
    [PreserveSig] int CreateBounceScalarAnimation(out nint result);
    [PreserveSig] int CreateBounceVector2Animation(out nint result);
    [PreserveSig] int CreateBounceVector3Animation(out nint result);
    [PreserveSig] int CreateContainerShape(out nint result);
    [PreserveSig] int CreateEllipseGeometry(out nint result);
    [PreserveSig] int CreateLineGeometry(out nint result);
    [PreserveSig] int CreatePathGeometry(out nint result);
    [PreserveSig] int CreatePathGeometryWithPath(nint path, out nint result);
    [PreserveSig] int CreatePathKeyFrameAnimation(out nint result);
    [PreserveSig] int CreateRectangleGeometry(out nint result);
    [PreserveSig] int CreateRoundedRectangleGeometry(out nint result);
    [PreserveSig] int CreateShapeVisual(out nint result);
    [PreserveSig] int CreateSpriteShape(out nint result);
    [PreserveSig] int CreateSpriteShapeWithGeometry(nint geometry, out nint result);
    [PreserveSig] int CreateViewBox(out nint result);
    [PreserveSig] int RequestCommitAsync(out nint operation);
}

[GeneratedComInterface]
[Guid("7A38B2BD-CEC8-4EEB-830F-D8D07AEDEBC3")]
internal partial interface ICompositor6Com : IInspectableCom
{
    [PreserveSig] int CreateGeometricClip(out nint result);
    [PreserveSig] int CreateGeometricClipWithGeometry(nint geometry, out nint result);
}

[GeneratedComInterface]
[Guid("25297D5C-3AD4-4C9C-B5CF-E36A38512330")]
internal partial interface ICompositorInteropCom
{
    [PreserveSig]
    int CreateCompositionSurfaceForHandle(nint swapChain, out nint result);

    [PreserveSig]
    int CreateCompositionSurfaceForSwapChain(nint swapChain, out nint result);

    [PreserveSig]
    int CreateGraphicsDevice(nint renderingDevice, out nint result);
}

[GeneratedComInterface]
[Guid("29E691FA-4567-4DCA-B319-D0F207EB6807")]
internal partial interface ICompositorDesktopInteropCom
{
    [PreserveSig]
    int CreateDesktopWindowTarget(nint hwndTarget, int isTopmost, out nint result);

    [PreserveSig]
    int EnsureOnThread(uint threadId);
}

[GeneratedComInterface]
[Guid("08E05581-1AD1-4F97-9757-402D76E4233B")]
internal partial interface ISpriteVisualCom : IInspectableCom
{
    [PreserveSig]
    int GetBrush(out nint value);

    [PreserveSig]
    int SetBrush(nint value);
}

[GeneratedComInterface]
[Guid("FD04E6E3-FE0C-4C3C-AB19-A07601A576EE")]
internal partial interface ICompositionDrawingSurfaceInteropCom
{
    [PreserveSig]
    int BeginDraw(nint updateRect, in Guid interfaceId, out nint updateObject, out DCompositionPoint updateOffset);

    [PreserveSig]
    int EndDraw();

    [PreserveSig]
    int Resize(WinRTPoint sizePixels);

    [PreserveSig]
    int Scroll(nint scrollRect, nint clipRect, int offsetX, int offsetY);

    [PreserveSig]
    int ResumeDraw();

    [PreserveSig]
    int SuspendDraw();
}

[GeneratedComInterface]
[Guid("0FB8BDF6-C0F0-4BCC-9FB8-084982490D7D")]
internal partial interface ICompositionGraphicsDevice2Com : IInspectableCom
{
    [PreserveSig]
    int CreateDrawingSurface2(WinRTSize sizePixels, WinRTDirectXPixelFormat pixelFormat, WinRTDirectXAlphaMode alphaMode, out nint result);
}

[GeneratedComInterface]
[Guid("1527540D-42C7-47A6-A408-668F79A90DFB")]
internal partial interface ICompositionSurfaceCom : IInspectableCom
{
}

[GeneratedComInterface]
[Guid("A166C300-FAD0-4D11-9E67-E433162FF49E")]
internal partial interface ICompositionDrawingSurfaceCom : IInspectableCom
{
}

[GeneratedComInterface]
[Guid("1CCD2A52-CFC7-4ACE-9983-146BB8EB6A3C")]
internal partial interface ICompositionClipCom : IInspectableCom
{
}

[GeneratedComInterface]
[Guid("02F6BC74-ED20-4773-AFE6-D49B4A93DB32")]
internal partial interface IContainerVisualCom : IInspectableCom
{
    [PreserveSig]
    int GetChildren(out nint value);
}

[GeneratedComInterface]
[Guid("AD016D79-1E4C-4C0D-9C29-83338C87C162")]
internal partial interface ICompositionSurfaceBrushCom : IInspectableCom
{
    [PreserveSig] int GetBitmapInterpolationMode(out WinRTCompositionBitmapInterpolationMode value);
    [PreserveSig] int SetBitmapInterpolationMode(WinRTCompositionBitmapInterpolationMode value);
    [PreserveSig] int GetHorizontalAlignmentRatio(out float value);
    [PreserveSig] int SetHorizontalAlignmentRatio(float value);
    [PreserveSig] int GetStretch(out WinRTCompositionStretch value);
    [PreserveSig] int SetStretch(WinRTCompositionStretch value);
    [PreserveSig] int GetSurface(out nint value);
    [PreserveSig] int SetSurface(nint value);
    [PreserveSig] int GetVerticalAlignmentRatio(out float value);
    [PreserveSig] int SetVerticalAlignmentRatio(float value);
}

[GeneratedComInterface]
[Guid("AB0D7608-30C0-40E9-B568-B60A6BD1FB46")]
internal partial interface ICompositionBrushCom : IInspectableCom
{
}

[GeneratedComInterface]
[Guid("BE5624AF-BA7E-4510-9850-41C0B4FF74DF")]
internal partial interface ICompositionEffectFactoryCom : IInspectableCom
{
    [PreserveSig] int CreateBrush(out nint result);
    [PreserveSig] int GetExtendedError(out int value);
    [PreserveSig] int GetLoadStatus(out int value);
}

[GeneratedComInterface]
[Guid("BF7F795E-83CC-44BF-A447-3E3C071789EC")]
internal partial interface ICompositionEffectBrushCom : IInspectableCom
{
    [PreserveSig] int GetSourceParameter(nint name, out nint result);
    [PreserveSig] int SetSourceParameter(nint name, nint source);
}

[GeneratedComInterface]
[Guid("C5ACAE58-3898-499E-8D7F-224E91286A5D")]
internal partial interface ICompositionBackdropBrushCom : IInspectableCom
{
}

[GeneratedComInterface]
[Guid("B3D9F276-ABA3-4724-ACF3-D0397464DB1C")]
internal partial interface ICompositionEffectSourceParameterFactoryCom : IInspectableCom
{
    [PreserveSig] int Create(nint name, out nint instance);
}

[GeneratedComInterface]
[Guid("117E202D-A859-4C89-873B-C2AA566788E3")]
internal partial interface IVisualCom : IInspectableCom
{
    [PreserveSig] int GetAnchorPoint(out WinRTVector2 value);
    [PreserveSig] int SetAnchorPoint(WinRTVector2 value);
    [PreserveSig] int GetBackfaceVisibility(out int value);
    [PreserveSig] int SetBackfaceVisibility(int value);
    [PreserveSig] int GetBorderMode(out int value);
    [PreserveSig] int SetBorderMode(int value);
    [PreserveSig] int GetCenterPoint(out nint value);
    [PreserveSig] int SetCenterPoint(nint value);
    [PreserveSig] int GetClip(out nint value);
    [PreserveSig] int SetClip(nint value);
    [PreserveSig] int GetCompositeMode(out int value);
    [PreserveSig] int SetCompositeMode(int value);
    [PreserveSig] int GetIsVisible(out int value);
    [PreserveSig] int SetIsVisible(int value);
    [PreserveSig] int GetOffset(out nint value);
    [PreserveSig] int SetOffset(nint value);
    [PreserveSig] int GetOpacity(out float value);
    [PreserveSig] int SetOpacity(float value);
    [PreserveSig] int GetOrientation(out nint value);
    [PreserveSig] int SetOrientation(nint value);
    [PreserveSig] int GetParent(out nint value);
    [PreserveSig] int GetRotationAngle(out float value);
    [PreserveSig] int SetRotationAngle(float value);
    [PreserveSig] int GetRotationAngleInDegrees(out float value);
    [PreserveSig] int SetRotationAngleInDegrees(float value);
    [PreserveSig] int GetRotationAxis(out nint value);
    [PreserveSig] int SetRotationAxis(nint value);
    [PreserveSig] int GetScale(out nint value);
    [PreserveSig] int SetScale(nint value);
    [PreserveSig] int GetSize(out WinRTVector2 value);
    [PreserveSig] int SetSize(WinRTVector2 value);
    [PreserveSig] int GetTransformMatrix(out nint value);
    [PreserveSig] int SetTransformMatrix(nint value);
}

[GeneratedComInterface]
[Guid("3052B611-56C3-4C3E-8BF3-F6E1AD473F06")]
internal partial interface IVisual2Com : IInspectableCom
{
    [PreserveSig] int GetParentForTransform(out nint value);
    [PreserveSig] int SetParentForTransform(nint value);
    [PreserveSig] int GetRelativeOffsetAdjustment(out nint value);
    [PreserveSig] int SetRelativeOffsetAdjustment(nint value);
    [PreserveSig] int GetRelativeSizeAdjustment(out WinRTVector2 value);
    [PreserveSig] int SetRelativeSizeAdjustment(WinRTVector2 value);
}

[GeneratedComInterface]
[Guid("8B745505-FD3E-4A98-84A8-E949468C6BCB")]
internal partial interface IVisualCollectionCom : IInspectableCom
{
    [PreserveSig] int GetCount(out int value);
    [PreserveSig] int InsertAbove(nint newChild, nint sibling);
    [PreserveSig] int InsertAtBottom(nint newChild);
    [PreserveSig] int InsertAtTop(nint newChild);
    [PreserveSig] int InsertBelow(nint newChild, nint sibling);
    [PreserveSig] int Remove(nint child);
    [PreserveSig] int RemoveAll();
}

[GeneratedComInterface]
[Guid("8770C822-1D50-4B8B-B013-7C9A0E46935F")]
internal partial interface ICompositionRoundedRectangleGeometryCom : IInspectableCom
{
    [PreserveSig] int GetCornerRadius(out WinRTVector2 value);
    [PreserveSig] int SetCornerRadius(WinRTVector2 value);
    [PreserveSig] int GetOffset(out WinRTVector2 value);
    [PreserveSig] int SetOffset(WinRTVector2 value);
    [PreserveSig] int GetSize(out WinRTVector2 value);
    [PreserveSig] int SetSize(WinRTVector2 value);
}

[GeneratedComInterface]
[Guid("E985217C-6A17-4207-ABD8-5FD3DD612A9D")]
internal partial interface ICompositionGeometryCom : IInspectableCom
{
    [PreserveSig] int GetTrimEnd(out float value);
    [PreserveSig] int SetTrimEnd(float value);
    [PreserveSig] int GetTrimOffset(out float value);
    [PreserveSig] int SetTrimOffset(float value);
    [PreserveSig] int GetTrimStart(out float value);
    [PreserveSig] int SetTrimStart(float value);
}

[GeneratedComInterface]
[Guid("CB51C0CE-8FE6-4636-B202-861FAA07D8F3")]
internal partial interface IGraphicsEffectCom : IInspectableCom
{
    [PreserveSig] int GetName(out nint name);
    [PreserveSig] int SetName(nint name);
}

[GeneratedComInterface]
[Guid("2D8F9DDC-4339-4EB9-9216-F9DEB75658A2")]
internal partial interface IGraphicsEffectSourceCom : IInspectableCom
{
}

[GeneratedComInterface]
[Guid("2FC57384-A068-44D7-A331-30982FCF7177")]
internal partial interface IGraphicsEffectD2D1InteropCom
{
    [PreserveSig] int GetEffectId(out Guid id);
    [PreserveSig] int GetNamedPropertyMapping(nint name, out uint index, out WinRTGraphicsEffectPropertyMapping mapping);
    [PreserveSig] int GetPropertyCount(out uint count);
    [PreserveSig] int GetProperty(uint index, out nint value);
    [PreserveSig] int GetSource(uint index, out nint source);
    [PreserveSig] int GetSourceCount(out uint count);
}

[GeneratedComInterface]
[Guid("4BD682DD-7554-40E9-9A9B-82654EDE7E62")]
internal partial interface IPropertyValueCom : IInspectableCom
{
    [PreserveSig] int GetType(out WinRTPropertyType value);
    [PreserveSig] int GetIsNumericScalar(out byte value);
    [PreserveSig] int GetUInt8(out byte value);
    [PreserveSig] int GetInt16(out short value);
    [PreserveSig] int GetUInt16(out ushort value);
    [PreserveSig] int GetInt32(out int value);
    [PreserveSig] int GetUInt32(out uint value);
    [PreserveSig] int GetInt64(out long value);
    [PreserveSig] int GetUInt64(out ulong value);
    [PreserveSig] int GetSingle(out float value);
    [PreserveSig] int GetDouble(out double value);
    [PreserveSig] int GetChar16(out char value);
    [PreserveSig] int GetBoolean(out byte value);
    [PreserveSig] int GetString(out nint value);
    [PreserveSig] int GetGuid(out Guid value);
    [PreserveSig] int GetDateTime(out nint value);
    [PreserveSig] int GetTimeSpan(out nint value);
    [PreserveSig] int GetPoint(out nint value);
    [PreserveSig] int GetSize(out nint value);
    [PreserveSig] int GetRect(out nint value);
    [PreserveSig] int GetUInt8Array(out uint valueSize, out nint value);
    [PreserveSig] int GetInt16Array(out uint valueSize, out nint value);
    [PreserveSig] int GetUInt16Array(out uint valueSize, out nint value);
    [PreserveSig] int GetInt32Array(out uint valueSize, out nint value);
    [PreserveSig] int GetUInt32Array(out uint valueSize, out nint value);
    [PreserveSig] int GetInt64Array(out uint valueSize, out nint value);
    [PreserveSig] int GetUInt64Array(out uint valueSize, out nint value);
    [PreserveSig] int GetSingleArray(out uint valueSize, out nint value);
    [PreserveSig] int GetDoubleArray(out uint valueSize, out nint value);
    [PreserveSig] int GetChar16Array(out uint valueSize, out nint value);
    [PreserveSig] int GetBooleanArray(out uint valueSize, out nint value);
    [PreserveSig] int GetStringArray(out uint valueSize, out nint value);
    [PreserveSig] int GetInspectableArray(out uint valueSize, out nint value);
    [PreserveSig] int GetGuidArray(out uint valueSize, out nint value);
    [PreserveSig] int GetDateTimeArray(out uint valueSize, out nint value);
    [PreserveSig] int GetTimeSpanArray(out uint valueSize, out nint value);
    [PreserveSig] int GetPointArray(out uint valueSize, out nint value);
    [PreserveSig] int GetSizeArray(out uint valueSize, out nint value);
    [PreserveSig] int GetRectArray(out uint valueSize, out nint value);
}

[GeneratedComInterface]
[Guid("A1BEA8BA-D726-4663-8129-6B5E7927FFA6")]
internal partial interface ICompositionTargetCom : IInspectableCom
{
    [PreserveSig]
    int GetRoot(out nint value);

    [PreserveSig]
    int SetRoot(nint value);
}