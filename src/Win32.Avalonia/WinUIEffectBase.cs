using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Linq;

namespace Win32.Avalonia;

internal static class D2DEffects
{
    public static readonly Guid Blend = new(0x81C5B77B, 0x13F8, 0x4CDD, 0xAD, 0x20, 0xC8, 0x90, 0x54, 0x7A, 0xC6, 0x5D);
    public static readonly Guid Flood = new(0x61C23C20, 0xAE69, 0x4D8E, 0x94, 0xCF, 0x50, 0x07, 0x8D, 0xF6, 0x38, 0xF2);
    public static readonly Guid GaussianBlur = new(0x1FEB6D69, 0x2FE6, 0x4AC9, 0x8C, 0x58, 0x1D, 0x7F, 0x93, 0xE7, 0xA6, 0xA5);
    public static readonly Guid Opacity = new("811d79a4-de28-4454-8094-c64685f8bd4c");
}

internal abstract class WinRTInspectableBase : IInspectableCom
{
    public int GetIids(out ulong iidCount, out nint iids)
    {
        var interfaces = GetType()
            .GetInterfaces()
            .Where(interfaceType => interfaceType.IsInterface && Attribute.IsDefined(interfaceType, typeof(GeneratedComInterfaceAttribute)))
            .Distinct()
            .ToArray();

        iidCount = (ulong)interfaces.Length;
        if (interfaces.Length == 0)
        {
            iids = nint.Zero;
            return 0;
        }

        var guidSize = Marshal.SizeOf<Guid>();
        iids = Marshal.AllocCoTaskMem(guidSize * interfaces.Length);
        for (var index = 0; index < interfaces.Length; index++)
        {
            Marshal.StructureToPtr(interfaces[index].GUID, iids + (index * guidSize), false);
        }

        return 0;
    }

    public int GetRuntimeClassName(out nint className)
    {
        className = WinRTNativeMethods.CreateString(GetType().FullName ?? GetType().Name);
        return 0;
    }

    public int GetTrustLevel(out WinRTTrustLevel trustLevel)
    {
        trustLevel = WinRTTrustLevel.BaseTrust;
        return 0;
    }
}

internal abstract class WinUIEffectBase : WinRTInspectableBase, IGraphicsEffectCom, IGraphicsEffectSourceCom, IGraphicsEffectD2D1InteropCom, IDisposable
{
    private const int EInvalidArg = unchecked((int)0x80070057);
    private const int ENotImpl = unchecked((int)0x80004001);

    private nint[]? _sources;

    protected WinUIEffectBase(params IGraphicsEffectSourceCom[] sources)
    {
        if (sources.Length == 0)
        {
            return;
        }

        _sources = new nint[sources.Length];
        for (var index = 0; index < sources.Length; index++)
        {
            _sources[index] = GeneratedComHelpers.ConvertToUnmanaged(sources[index]);
        }
    }

    public abstract Guid EffectId { get; }

    protected abstract IPropertyValueCom? CreateProperty(uint index);

    protected abstract uint PropertyCount { get; }

    public void Dispose()
    {
        if (_sources is null)
        {
            return;
        }

        foreach (var source in _sources)
        {
            GeneratedComHelpers.Free<IGraphicsEffectSourceCom>(source);
        }

        _sources = null;
    }

    public int GetName(out nint name)
    {
        name = nint.Zero;
        return 0;
    }

    public int SetName(nint name) => 0;

    public int GetEffectId(out Guid id)
    {
        id = EffectId;
        return 0;
    }

    public int GetNamedPropertyMapping(nint name, out uint index, out WinRTGraphicsEffectPropertyMapping mapping)
    {
        index = 0;
        mapping = WinRTGraphicsEffectPropertyMapping.Unknown;
        return ENotImpl;
    }

    public int GetPropertyCount(out uint count)
    {
        count = PropertyCount;
        return 0;
    }

    public int GetProperty(uint index, out nint value)
    {
        var propertyValue = CreateProperty(index);
        value = propertyValue is null ? nint.Zero : GeneratedComHelpers.ConvertToUnmanaged(propertyValue);
        return 0;
    }

    public int GetSource(uint index, out nint source)
    {
        if (_sources is null || index >= _sources.Length)
        {
            source = nint.Zero;
            return EInvalidArg;
        }

        source = _sources[index];
        Marshal.AddRef(source);
        return 0;
    }

    public int GetSourceCount(out uint count)
    {
        count = (uint)(_sources?.Length ?? 0);
        return 0;
    }
}

[GeneratedComClass]
internal sealed partial class WinRTPropertyValue : WinRTInspectableBase, IPropertyValueCom
{
    private const int ENotImpl = unchecked((int)0x80004001);

    private readonly float[]? _singleArray;

    public WinRTPropertyValue(float value)
    {
        Type = WinRTPropertyType.Single;
        Single = value;
    }

    public WinRTPropertyValue(uint value)
    {
        Type = WinRTPropertyType.UInt32;
        UInt32 = value;
    }

    public WinRTPropertyValue(float[] value)
    {
        Type = WinRTPropertyType.SingleArray;
        _singleArray = value;
    }

    public WinRTPropertyType Type { get; }

    public uint UInt32 { get; }

    public float Single { get; }

    public int GetType(out WinRTPropertyType value)
    {
        value = Type;
        return 0;
    }

    public int GetIsNumericScalar(out byte value)
    {
        value = (byte)(Type is WinRTPropertyType.UInt32 or WinRTPropertyType.Single ? 1 : 0);
        return 0;
    }

    public int GetUInt8(out byte value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetInt16(out short value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetUInt16(out ushort value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetInt32(out int value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetUInt32(out uint value)
    {
        value = UInt32;
        return 0;
    }

    public int GetInt64(out long value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetUInt64(out ulong value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetSingle(out float value)
    {
        value = Single;
        return 0;
    }

    public int GetDouble(out double value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetChar16(out char value)
    {
        value = default;
        return ENotImpl;
    }

    public int GetBoolean(out byte value)
    {
        value = 0;
        return ENotImpl;
    }

    public int GetString(out nint value)
    {
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetGuid(out Guid value)
    {
        value = default;
        return ENotImpl;
    }

    public int GetDateTime(out nint value)
    {
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetTimeSpan(out nint value)
    {
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetPoint(out nint value)
    {
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetSize(out nint value)
    {
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetRect(out nint value)
    {
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetUInt8Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetInt16Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetUInt16Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetInt32Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetUInt32Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetInt64Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetUInt64Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetSingleArray(out uint valueSize, out nint value)
    {
        if (_singleArray is null)
        {
            valueSize = 0;
            value = nint.Zero;
            return ENotImpl;
        }

        valueSize = (uint)_singleArray.Length;
        value = Marshal.AllocCoTaskMem(sizeof(float) * _singleArray.Length);
        Marshal.Copy(_singleArray, 0, value, _singleArray.Length);
        return 0;
    }

    public int GetDoubleArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetChar16Array(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetBooleanArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetStringArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetInspectableArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetGuidArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetDateTimeArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetTimeSpanArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetPointArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetSizeArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }

    public int GetRectArray(out uint valueSize, out nint value)
    {
        valueSize = 0;
        value = nint.Zero;
        return ENotImpl;
    }
}

[GeneratedComClass]
internal sealed partial class BlendEffect(int mode, params IGraphicsEffectSourceCom[] sources) : WinUIEffectBase(sources)
{
    private readonly int _mode = mode;

    public override Guid EffectId => D2DEffects.Blend;

    protected override uint PropertyCount => 1;

    protected override IPropertyValueCom? CreateProperty(uint index)
        => index == 0 ? new WinRTPropertyValue((uint)_mode) : null;
}

[GeneratedComClass]
internal sealed partial class OpacityEffect(float opacity, params IGraphicsEffectSourceCom[] sources) : WinUIEffectBase(sources)
{
    private readonly float _opacity = opacity;

    public override Guid EffectId => D2DEffects.Opacity;

    protected override uint PropertyCount => 1;

    protected override IPropertyValueCom? CreateProperty(uint index)
        => index == 0 ? new WinRTPropertyValue(_opacity) : null;
}

[GeneratedComClass]
internal sealed partial class ColorSourceEffect(float[] color) : WinUIEffectBase()
{
    private readonly float[] _color = color;

    public override Guid EffectId => D2DEffects.Flood;

    protected override uint PropertyCount => 1;

    protected override IPropertyValueCom? CreateProperty(uint index)
        => index == 0 ? new WinRTPropertyValue(_color) : null;
}

[GeneratedComClass]
internal sealed partial class WinUIGaussianBlurEffect(IGraphicsEffectSourceCom source) : WinUIEffectBase(source)
{
    private enum D2D1GaussianBlurOptimization
    {
        Balanced = 1,
    }

    private enum D2D1BorderMode
    {
        Hard = 1,
    }

    private enum D2D1GaussianBlurProp
    {
        StandardDeviation,
        Optimization,
        BorderMode,
    }

    public override Guid EffectId => D2DEffects.GaussianBlur;

    protected override uint PropertyCount => 3;

    protected override IPropertyValueCom? CreateProperty(uint index)
        => (D2D1GaussianBlurProp)index switch
        {
            D2D1GaussianBlurProp.StandardDeviation => new WinRTPropertyValue(30.0f),
            D2D1GaussianBlurProp.Optimization => new WinRTPropertyValue((uint)D2D1GaussianBlurOptimization.Balanced),
            D2D1GaussianBlurProp.BorderMode => new WinRTPropertyValue((uint)D2D1BorderMode.Hard),
            _ => null,
        };
}