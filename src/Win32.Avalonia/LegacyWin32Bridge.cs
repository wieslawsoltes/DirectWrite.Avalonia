using System.Reflection;
using Avalonia;
using global::Avalonia.Win32;
using Avalonia.Input.Platform;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

namespace Win32.Avalonia;

internal sealed class LegacyWin32Bridge
{
    public static LegacyWin32Bridge Instance { get; } = new();

    private readonly Assembly _assembly;
    private readonly MethodInfo _angleGraphicsTryCreate;
    private readonly MethodInfo _directCompositionIsSupported;
    private readonly MethodInfo _directCompositionTryCreateAndRegister;
    private readonly MethodInfo _legacyGlManagerInitialize;
    private readonly FieldInfo _legacyWin32OptionsField;
    private readonly FieldInfo _legacyWin32CompositorField;
    private readonly MethodInfo _legacyWin32SetDpiAwareness;
    private readonly Type _legacyWindowImplType;
    private readonly FieldInfo _legacyWindowGlSurfaceField;
    private readonly FieldInfo _legacyWindowInstancesField;
    private readonly MethodInfo _winUiCompositionIsSupported;
    private readonly MethodInfo _winUiCompositionTryCreateAndRegister;

    private LegacyWin32Bridge()
    {
        _assembly = typeof(global::Avalonia.Win32ApplicationExtensions).Assembly;
        var legacyWin32PlatformType = GetRequiredType("Avalonia.Win32.Win32Platform");
        _legacyWindowImplType = GetRequiredType("Avalonia.Win32.WindowImpl");
        _legacyWin32OptionsField = legacyWin32PlatformType.GetField(
            "s_options",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.Win32Platform.s_options.");
        _legacyWin32CompositorField = legacyWin32PlatformType.GetField(
            "s_compositor",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.Win32Platform.s_compositor.");
        _legacyWin32SetDpiAwareness = legacyWin32PlatformType.GetMethod(
            "SetDpiAwareness",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.Win32Platform.SetDpiAwareness.");

        _angleGraphicsTryCreate = GetRequiredType("Avalonia.Win32.OpenGl.Angle.AngleWin32PlatformGraphicsFactory").GetMethod(
            "TryCreate",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.OpenGl.Angle.AngleWin32PlatformGraphicsFactory.TryCreate.");

        var directCompositionConnectionType = GetRequiredType("Avalonia.Win32.DComposition.DirectCompositionConnection");
        _directCompositionIsSupported = directCompositionConnectionType.GetMethod(
            "IsSupported",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.DComposition.DirectCompositionConnection.IsSupported.");
        _directCompositionTryCreateAndRegister = directCompositionConnectionType.GetMethod(
            "TryCreateAndRegister",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.DComposition.DirectCompositionConnection.TryCreateAndRegister.");

        _legacyGlManagerInitialize = GetRequiredType("Avalonia.Win32.Win32GlManager").GetMethod(
            "Initialize",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.Win32GlManager.Initialize.");
        _legacyWindowGlSurfaceField = _legacyWindowImplType.GetField(
            "_glSurface",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.WindowImpl._glSurface.");
        _legacyWindowInstancesField = _legacyWindowImplType.GetField(
            "s_instances",
            BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.WindowImpl.s_instances.");

        var winUiCompositorConnectionType = GetRequiredType("Avalonia.Win32.WinRT.Composition.WinUiCompositorConnection");
        _winUiCompositionIsSupported = winUiCompositorConnectionType.GetMethod(
            "IsSupported",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.WinRT.Composition.WinUiCompositorConnection.IsSupported.");
        _winUiCompositionTryCreateAndRegister = winUiCompositorConnectionType.GetMethod(
            "TryCreateAndRegister",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate Avalonia.Win32.WinRT.Composition.WinUiCompositorConnection.TryCreateAndRegister.");

    }

    public void SetLegacyOptions(global::Avalonia.Win32PlatformOptions options)
        => _legacyWin32OptionsField.SetValue(null, options);

    public void ApplyLegacyDpiAwareness()
        => _legacyWin32SetDpiAwareness.Invoke(null, null);

    public void SetLegacyCompositor(Compositor compositor)
        => _legacyWin32CompositorField.SetValue(null, compositor);

    public void InitializeLegacyPlatformState(global::Avalonia.Win32PlatformOptions options, Compositor compositor)
    {
        SetLegacyOptions(options);
        SetLegacyCompositor(compositor);
    }

    public IPlatformGraphics? InitializePlatformGraphics()
        => (IPlatformGraphics?)_legacyGlManagerInitialize.Invoke(null, null);

    public IPlatformGraphics? InitializeAnglePlatformGraphics(AngleOptions? options)
        => (IPlatformGraphics?)_angleGraphicsTryCreate.Invoke(null, [options]);

    public bool IsDirectCompositionSupported()
        => (bool)(_directCompositionIsSupported.Invoke(null, null)
            ?? throw new InvalidOperationException("DirectCompositionConnection.IsSupported returned null."));

    public bool TryInitializeDirectComposition()
        => (bool)(_directCompositionTryCreateAndRegister.Invoke(null, null)
            ?? throw new InvalidOperationException("DirectCompositionConnection.TryCreateAndRegister returned null."));

    public IClipboardImpl CreateClipboardImpl()
        => (IClipboardImpl)CreateInstance("Avalonia.Win32.ClipboardImpl");

    public IPlatformDragSource CreatePlatformDragSource()
        => (IPlatformDragSource)CreateInstance("Avalonia.Win32.DragSource");

    public bool IsWinUiCompositionSupported()
        => (bool)(_winUiCompositionIsSupported.Invoke(null, null)
            ?? throw new InvalidOperationException("WinUiCompositorConnection.IsSupported returned null."));

    public bool TryInitializeWinUiComposition()
        => (bool)(_winUiCompositionTryCreateAndRegister.Invoke(null, null)
            ?? throw new InvalidOperationException("WinUiCompositorConnection.TryCreateAndRegister returned null."));

    public void PatchLocalWglWindowSurfaces()
    {
        if (AvaloniaLocator.Current.GetService<IPlatformGraphics>() is not WglPlatformOpenGlInterface)
        {
            return;
        }

        if (_legacyWindowInstancesField.GetValue(null) is not System.Collections.IEnumerable windows)
        {
            return;
        }

        foreach (var window in windows)
        {
            if (window is IWindowImpl windowImpl)
            {
                PatchLocalWglSurface(windowImpl);
            }
        }
    }

    private void PatchLocalWglSurface(IWindowImpl window)
    {
        if (AvaloniaLocator.Current.GetService<IPlatformGraphics>() is not WglPlatformOpenGlInterface)
        {
            return;
        }

        if (!_legacyWindowImplType.IsInstanceOfType(window))
        {
            return;
        }

        if (_legacyWindowGlSurfaceField.GetValue(window) is not null)
        {
            return;
        }

        if (window is EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info)
        {
            _legacyWindowGlSurfaceField.SetValue(window, new WglGlPlatformSurface(info));
        }
    }

    private object CreateInstance(string typeName, params object?[] args)
    {
        var type = GetRequiredType(typeName);
        return Activator.CreateInstance(
                   type,
                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                   binder: null,
                   args: args,
                   culture: null)
               ?? throw new InvalidOperationException($"Unable to create instance of {typeName}.");
    }

    private Type GetRequiredType(string fullName)
        => _assembly.GetType(fullName, throwOnError: true, ignoreCase: false)
           ?? throw new InvalidOperationException($"Unable to locate type {fullName}.");
}