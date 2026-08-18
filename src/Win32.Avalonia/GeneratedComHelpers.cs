using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace Win32.Avalonia;

internal static unsafe class GeneratedComHelpers
{
    public static nint ConvertToUnmanaged<T>(T? value)
        where T : class
        => value is null ? nint.Zero : (nint)ComInterfaceMarshaller<T>.ConvertToUnmanaged(value);

    public static T? ConvertToManaged<T>(nint value)
        where T : class
        => value == nint.Zero ? null : ComInterfaceMarshaller<T>.ConvertToManaged((void*)value);

    public static void Free<T>(nint value)
        where T : class
    {
        if (value != nint.Zero)
        {
            ComInterfaceMarshaller<T>.Free((void*)value);
        }
    }

    public static T QueryInterface<T>(nint sourcePointer, out nint interfacePointer)
        where T : class
    {
        var interfaceId = typeof(T).GUID;
        var hr = Marshal.QueryInterface(sourcePointer, in interfaceId, out interfacePointer);
        if (hr < 0 || interfacePointer == nint.Zero)
        {
            Marshal.ThrowExceptionForHR(hr);
        }

        return ConvertToManaged<T>(interfacePointer)
            ?? throw new InvalidOperationException($"Unable to wrap queried COM interface {typeof(T).FullName}.");
    }
}

internal abstract class GeneratedComWrapper<T> : IDisposable
    where T : class
{
    private nint _nativePointer;
    private bool _disposed;

    protected GeneratedComWrapper(T native)
    {
        Native = native;
        _nativePointer = GeneratedComHelpers.ConvertToUnmanaged(native);
    }

    protected T Native { get; private set; }

    protected nint NativePointer => _nativePointer;

    public nint DangerousGetPointer()
        => _nativePointer;

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_nativePointer != nint.Zero)
        {
            GeneratedComHelpers.Free<T>(_nativePointer);
            _nativePointer = nint.Zero;
        }

        if (disposing)
        {
            Native = null!;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~GeneratedComWrapper()
    {
        Dispose(false);
    }
}

internal sealed class GeneratedComValue<T>(T native) : GeneratedComWrapper<T>(native)
    where T : class;