using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.OpenGL;

namespace Win32.Avalonia;

internal sealed class WglContext : IGlContext
{
    private readonly object _lock = new();
    private readonly WglContext? _sharedWith;
    private readonly nint _context;
    private readonly nint _window;
    private readonly nint _dc;
    private readonly int _pixelFormat;
    private readonly PIXELFORMATDESCRIPTOR _formatDescriptor;

    public WglContext(WglContext? sharedWith, GlVersion version, nint context, nint window, nint dc, int pixelFormat, PIXELFORMATDESCRIPTOR formatDescriptor)
    {
        Version = version;
        _sharedWith = sharedWith;
        _context = context;
        _window = window;
        _dc = dc;
        _pixelFormat = pixelFormat;
        _formatDescriptor = formatDescriptor;
        StencilSize = formatDescriptor.cStencilBits;

        using (MakeCurrent())
        {
            GlInterface = new GlInterface(version, proc =>
            {
                var extensionProc = WglNative.GetProcAddress(proc);
                if (extensionProc != nint.Zero)
                {
                    return extensionProc;
                }

                return WglNative.TryGetModuleExport(proc, out var moduleProc) ? moduleProc : nint.Zero;
            });
        }
    }

    public nint Handle => _context;

    public GlVersion Version { get; }

    public GlInterface GlInterface { get; }

    public int SampleCount => 0;

    public int StencilSize { get; }

    public bool IsLost { get; private set; }

    public void Dispose()
    {
        WglNative.DeleteContext(_context);
        WglGdiResourceManager.ReleaseDc(_window, _dc);
        WglGdiResourceManager.DestroyWindow(_window);
        IsLost = true;
    }

    public IDisposable EnsureCurrent() => MakeCurrent();

    public IDisposable MakeCurrent()
    {
        LegacyWin32Bridge.Instance.PatchLocalWglWindowSurfaces();

        if (IsLost)
        {
            throw new PlatformGraphicsContextLostException();
        }

        if (WglNative.GetCurrentContext() == _context && WglNative.GetCurrentDc() == _dc)
        {
            return NoopDisposable.Instance;
        }

        return new WglRestoreContext(_dc, _context, _lock);
    }

    internal IDisposable Lock()
    {
        Monitor.Enter(_lock);
        return new MonitorExitDisposable(_lock);
    }

    public nint CreateConfiguredDeviceContext(nint window)
    {
        var dc = WglGdiResourceManager.GetDc(window);
        var descriptor = _formatDescriptor;
        PInvoke.SetPixelFormat(new HDC(dc), _pixelFormat, descriptor);
        return dc;
    }

    public IDisposable MakeCurrent(nint dc) => new WglRestoreContext(dc, _context, _lock);

    public bool IsSharedWith(IGlContext context)
    {
        var wglContext = (WglContext)context;
        return wglContext == this
               || wglContext._sharedWith == this
               || _sharedWith == context
               || _sharedWith is not null && _sharedWith == wglContext._sharedWith;
    }

    public bool CanCreateSharedContext => true;

    public IGlContext? CreateSharedContext(IEnumerable<GlVersion>? preferredVersions = null)
    {
        var versions = preferredVersions?.Append(Version).ToArray() ?? [Version];
        return WglDisplay.CreateContext(versions, _sharedWith ?? this);
    }

    public object? TryGetFeature(Type featureType) => null;

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }

    private sealed class MonitorExitDisposable(object monitor) : IDisposable
    {
        private object? _monitor = monitor;

        public void Dispose()
        {
            var monitor = Interlocked.Exchange(ref _monitor, null);
            if (monitor is not null)
            {
                Monitor.Exit(monitor);
            }
        }
    }
}