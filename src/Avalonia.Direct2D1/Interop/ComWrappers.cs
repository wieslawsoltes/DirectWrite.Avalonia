using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Avalonia.Direct2D1.Interop;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class NativeInterfaceAttribute : Attribute
{
    public NativeInterfaceAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }

    public Type InterfaceType { get; }
}

internal static class Configuration
{
    public static bool EnableReleaseOnFinalizer { get; set; }
}

public class DirectXException : COMException
{
    public DirectXException(int result)
        : base(result.ToString("X8"), result)
    {
        ResultCode = result;
    }

    public int ResultCode { get; }
}

public abstract class ComObject : IDisposable
{
    private bool _disposed;
    private readonly Type _nativeInterfaceType;

    protected ComObject(object native)
    {
        NativeObject = native ?? throw new ArgumentNullException(nameof(native));
        _nativeInterfaceType = GetNativeInterfaceType(GetType());
        NativePointer = ComMarshaller.ConvertToUnmanaged(_nativeInterfaceType, NativeObject);
    }

    public IntPtr NativePointer { get; private set; }

    protected object NativeObject { get; private set; }

    internal object NativeComObject => NativeObject;

    public T QueryInterface<T>()
        where T : ComObject
    {
        var interfaceType = GetNativeInterfaceType(typeof(T));
        var iid = interfaceType.GUID;

        Marshal.QueryInterface(NativePointer, in iid, out var ptr);

        try
        {
            var native = ComMarshaller.ConvertToManaged(interfaceType, ptr);
            return (T)Activator.CreateInstance(
                typeof(T),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                binder: null,
                args: new[] { native },
                culture: null)!;
        }
        finally
        {
            if (ptr != IntPtr.Zero)
            {
                ComMarshaller.Free(interfaceType, ptr);
            }
        }
    }

    protected static void CheckError(int result)
    {
        if (result < 0)
        {
            throw new DirectXException(result);
        }
    }

    protected static TInterface As<TInterface>(object value)
        where TInterface : class
    {
        return (TInterface)value;
    }

    protected TInterface GetNative<TInterface>()
        where TInterface : class
    {
        return As<TInterface>(NativeObject);
    }

    private static Type GetNativeInterfaceType(Type wrapperType)
    {
        var attribute = wrapperType.GetCustomAttribute<NativeInterfaceAttribute>();

        if (attribute is null)
        {
            throw new InvalidOperationException($"Missing {nameof(NativeInterfaceAttribute)} on {wrapperType.FullName}.");
        }

        return attribute.InterfaceType;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            NativeObject = null!;
        }

        if (NativePointer != IntPtr.Zero)
        {
            ComMarshaller.Free(_nativeInterfaceType, NativePointer);
            NativePointer = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ComObject()
    {
        if (Configuration.EnableReleaseOnFinalizer)
        {
            Dispose(false);
        }
    }
}

public abstract class CppObject : ComObject
{
    protected CppObject(object native)
        : base(native)
    {
    }
}

internal static unsafe class ComMarshaller
{
    public static IntPtr ConvertToUnmanaged(Type interfaceType, object managed)
    {
        var marshallerType = typeof(ComInterfaceMarshaller<>).MakeGenericType(interfaceType);
        var convert = marshallerType.GetMethod(nameof(ComInterfaceMarshaller<object>.ConvertToUnmanaged), BindingFlags.Public | BindingFlags.Static);
        var result = convert?.Invoke(null, new[] { managed });

        return result is null ? IntPtr.Zero : (IntPtr)Pointer.Unbox(result);
    }

    public static object ConvertToManaged(Type interfaceType, IntPtr unmanaged)
    {
        var marshallerType = typeof(ComInterfaceMarshaller<>).MakeGenericType(interfaceType);
        var convert = marshallerType.GetMethod(nameof(ComInterfaceMarshaller<object>.ConvertToManaged), BindingFlags.Public | BindingFlags.Static);

        return convert?.Invoke(null, new[] { Pointer.Box((void*)unmanaged, typeof(void*)) }) ?? throw new InvalidOperationException($"Unable to convert {interfaceType.FullName} to managed COM object.");
    }

    public static void Free(Type interfaceType, IntPtr unmanaged)
    {
        var marshallerType = typeof(ComInterfaceMarshaller<>).MakeGenericType(interfaceType);
        var free = marshallerType.GetMethod(nameof(ComInterfaceMarshaller<object>.Free), BindingFlags.Public | BindingFlags.Static);
        free?.Invoke(null, new[] { Pointer.Box((void*)unmanaged, typeof(void*)) });
    }
}

internal static class ComObjectFactory
{
    public static T Create<T>(object native)
        where T : ComObject
    {
        return (T)Activator.CreateInstance(
            typeof(T),
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            binder: null,
            args: new[] { native },
            culture: null)!;
    }
}
