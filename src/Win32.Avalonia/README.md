# Win32.Avalonia

`Win32.Avalonia` is the new standalone Win32 windowing package for this repository.

Current status:

- owns the public Win32 bootstrap entry point: `UseWin32Avalonia()`
- owns a package-local `Win32PlatformOptions` surface that mirrors the upstream Win32 backend contract
- seeds generated Win32 interop through `Microsoft.Windows.CsWin32`
- owns the Win32 bootstrap, dispatcher registration, locator bindings, and compositor initialization path
- owns the lightweight Win32 service slice used during bootstrap: hidden message window, keyboard modifier device, platform settings, screen enumeration, mounted-volume polling, tray/icon plumbing, and the top-level graphics router
- owns the Vulkan platform-graphics path in `Win32GlManager`
- carries a source-owned WGL stack that is now patched into legacy window instances so local WGL activation can run without the upstream OpenGL type checks
- owns local OLE clipboard and drag-source implementations built on generated COM interop, plus a source-owned DXGI COM and vblank-timer foundation
- still routes composition-backed DXGI/DirectComposition/WinUI paths through the compatibility bridge until the concrete window/composition slice is owned locally
- currently bridges unported concrete Win32 internals from `Avalonia.Win32` through a cached compatibility layer while those files are re-owned here

Porting goals:

- full feature parity with Avalonia's existing Win32 backend responsibilities: windows, popups, input, screens, tray, drag-drop, storage, native menus, dispatcher, and composition paths
- keep the platform-graphics seam explicit so renderer backends can interop with D3D11, ANGLE, WGL/OpenGL, and Vulkan on Windows
- keep the graphics abstraction backend-neutral enough that future Metal-backed presentation on macOS can use the same core renderer interop expectations
- move Win32 interop to generated APIs instead of growing manual bindings or ad-hoc code generation

Planned migration slices:

1. bootstrap, dispatcher, and lifetime services
2. `WindowImpl`, popup, embedding, and non-client/DPI behavior
3. input, clipboard, drag-drop, and storage provider services
4. tray, screen enumeration, native menu export, and notifications
5. composition and GPU interop paths: redirection surface, DirectComposition, WinUI composition, ANGLE, WGL, Vulkan

Until those slices are ported, this package should be treated as the owning public facade and migration point, not yet the final internal implementation.