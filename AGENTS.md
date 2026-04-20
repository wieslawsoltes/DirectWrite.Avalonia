# AGENTS.md

## Purpose

This repository hosts a standalone Windows rendering backend for Avalonia based on:

- Direct2D1 for drawing
- DirectWrite for fonts, shaping, glyph metrics, and glyph rendering
- DXGI + D3D11 for GPU-backed surfaces and presentation
- WIC for bitmap decode, encode, readback, and writeable bitmap support

The backend must ship outside the Avalonia repository as NuGet packages and remain compatible with the exact Avalonia version it is pinned to.

## Package Identity

- Core package: `Direct2D1.Avalonia`
- Win32 bootstrap package: `Direct2D1.Avalonia.Win32`
- Do not use an `Avalonia.*` package prefix. That namespace is reserved on NuGet.

## Target Framework And Version Pinning

- Target framework is `net10.0-windows10.0.19041.0`.
- Windows targeting is mandatory.
- Avalonia is pinned exactly to `12.0.1`.
- The package uses Avalonia private APIs and must keep the exact pinned Avalonia dependency contract.
- If Avalonia is upgraded, update:
  - [Directory.Build.props](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/Directory.Build.props)
  - [Directory.Packages.props](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/Directory.Packages.props)
  - any private API compatibility code

## Source References

When implementing or reviewing changes, use these upstream codebases as references:

- Avalonia Skia backend architecture:
  - `/Users/wieslawsoltes/GitHub/Avalonia/src/Skia/Avalonia.Skia`
- Avalonia Win32 and DXGI hosting behavior:
  - `/Users/wieslawsoltes/GitHub/Avalonia/src/Windows/Avalonia.Win32`
- Out-of-repo Avalonia private API packaging patterns:
  - `/Users/wieslawsoltes/GitHub/VelloSharp`

Skia is the structural reference for backend responsibilities. Avalonia.Win32 is the behavioral reference for HWND, DXGI, swap chain, DPI, and resize semantics.

## Repository Layout

- [src/Avalonia.Direct2D1](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1)
  Core renderer, text, interop, bitmap, geometry, and render-target implementation.
- [src/Avalonia.Direct2D1.Win32](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1.Win32)
  Win32 bootstrap extensions and platform options.
- [samples/ControlCatalog.Direct2D1](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/samples/ControlCatalog.Direct2D1)
  Manual verification sample.
- [tests/Avalonia.Direct2D1.UnitTests](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/tests/Avalonia.Direct2D1.UnitTests)
  Backend unit coverage.
- [tests/Avalonia.Direct2D1.RenderTests](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/tests/Avalonia.Direct2D1.RenderTests)
  Render-target smoke and regression coverage.

## Public Entry Points

- `UseDirect2D1(this AppBuilder, Direct2D1Options? options = null)`
  - implemented in [Direct2D1Platform.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Direct2D1Platform.cs)
- `UseWin32Direct2D1(this AppBuilder, Win32Direct2D1PlatformOptions? options = null)`
  - implemented in [Direct2D1Win32ApplicationExtensions.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1.Win32/Direct2D1Win32ApplicationExtensions.cs)

These are the supported public bootstrap APIs. Do not add parallel builder entry points without a strong reason.

## Non-Negotiable Technical Decisions

### Interop

- Do not introduce SharpDX back into this repository.
- Do not introduce MicroCom as the primary interop layer.
- Prefer official generated Win32 interop via `Microsoft.Windows.CsWin32`.
- Keep runtime COM wrapper activation explicit and reflection-free.
- Do not use `Activator`, runtime type scanning, or reflection-based COM dispatch in hot paths.
- Do not add new handwritten COM ABI/vtable code when generated Win32 interfaces can be used.

The current wrapper layer in [ComWrappers.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Interop/ComWrappers.cs) exists to adapt generated Win32 COM interfaces to backend wrapper types without reflection. Future work should simplify toward more generated interop, not away from it.

### Text System

- Do not require `UseHarfBuzz()` for this backend.
- Text shaping must be backed by DirectWrite.
- `IFontManagerImpl` and `ITextShaperImpl` must be supplied by this backend during `UseDirect2D1()`.
- The backend must render text through DirectWrite glyph runs, not through a HarfBuzz fallback path.

Relevant files:

- [TextShaperImpl.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Media/TextShaperImpl.cs)
- [GlyphTypefaceImpl.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Media/GlyphTypefaceImpl.cs)
- [FontManagerImpl.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Media/FontManagerImpl.cs)
- [DirectWriteNative.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Interop/DirectWriteNative.cs)

### Performance And GPU Usage

- Default backend behavior should use GPU-backed D3D11 hardware acceleration.
- WARP fallback is allowed and currently supported through `Direct2D1Options.UseWarpFallback`.
- Do not silently degrade to CPU-only rasterization if hardware initialization fails unexpectedly.
- Presentation, swap chain, and device-context paths must avoid redundant allocations and unnecessary per-frame resource churn.

Be precise about terminology:

- Current supported Avalonia Win32 host mode is `Win32RenderingMode.Software` with `Win32CompositionMode.RedirectionSurface`.
- That host integration limitation does not mean the backend itself should be CPU rasterized.
- The backend should still prefer D3D11 hardware devices internally unless options explicitly disable that.

## Current Implementation Status

The repository is a functioning baseline, not final parity with all original goals.

Current state:

- Standalone packages build and pack.
- DirectWrite-backed shaping and font loading are wired into the backend.
- Generated Win32 interop is used with explicit wrapper adaptation.
- Reflection-based COM activation has been removed.
- SharpDX should not be used as an active dependency path for new work.
- Sample app uses `FluentTheme` for visible controls.

Current constraints:

- Win32 standalone validation currently allows only `Win32RenderingMode.Software`.
- Win32 standalone validation currently allows only `Win32CompositionMode.RedirectionSurface`.
- Composition-backed `AngleEgl`, `Wgl`, `Vulkan`, `DirectComposition`, and `WinUIComposition` integration are not finished in this package.
- Runtime validation still needs Windows execution for any change touching swap chains, text rendering, GPU device creation, or presentation.

## Rendering Rules

### Drawing Context

- Keep [DrawingContextImpl.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Media/DrawingContextImpl.cs) aligned with Avalonia’s expected `IDrawingContextImpl` semantics.
- Apply render options deterministically when a drawing context starts and whenever text options change.
- Be careful with text antialiasing on alpha-backed surfaces:
  - grayscale antialiasing is the safe default for unspecified text rendering on composition-compatible targets
  - do not assume ClearType is always valid

### Glyph Rendering

- `DrawGlyphRun` must use correct DirectWrite glyph run layout, baseline origin, and font face.
- If text disappears, inspect:
  - glyph advances
  - glyph indices
  - Direct2D text antialias mode
  - font face pointer lifetime
  - alpha mode and target bitmap configuration

### Swap Chain And Resize

- Use explicit `ResizeBuffers` arguments: buffer count, width, height, and format.
- Do not use `ResizeBuffers(0, 0, 0, Format.Unknown, SwapChainFlags.None)` in resize hot paths.
- Do not resize DXGI buffers for a DPI-only change.
- Normalize zero-sized client areas to at least `1x1` before recreating GPU resources.
- If DXGI returns `DXGI_ERROR_INVALID_CALL` during resize, recreate the swap chain instead of crashing.
- Always detach the D2D target before disposing a target bitmap or resizing the swap chain.

Relevant file:

- [SwapChainRenderTarget.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/SwapChainRenderTarget.cs)

### Bitmaps And WIC

- Keep GPU bitmap and WIC bitmap responsibilities separate.
- Prefer WIC for file and stream encoding or decoding.
- Avoid extra `QueryInterface` churn around bitmap creation.
- Be explicit about alpha format and pixel format conversions.

## Text-Specific Rules

- Shaped glyphs must have positive advances for ordinary Latin text.
- `TextLayout` creation for simple visible text should yield positive width and height.
- Font fallback and custom font collection loading must continue to work through DirectWrite.
- Custom font collection lifetime must be owned correctly by the typeface or higher-level cache.

When changing shaping code, add or update tests in:

- [TextShaperImplTests.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/tests/Avalonia.Direct2D1.UnitTests/Media/TextShaperImplTests.cs)
- [FontManagerImplTests.cs](/Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/tests/Avalonia.Direct2D1.UnitTests/Media/FontManagerImplTests.cs)

## Coding Rules For Future Agents

- Prefer adapting Avalonia.Skia backend structure over inventing new backend abstractions.
- Keep interop wrappers explicit and small.
- Avoid reflection in runtime code.
- Avoid broad refactors during bug fixes unless they are required to remove an unsafe architectural choice.
- Preserve deterministic disposal for COM, DXGI, D2D, D3D11, and WIC resources.
- Be conservative with GPU resource lifetime changes. Resize bugs are usually ownership bugs.
- Do not reintroduce HarfBuzz as a requirement for backend text shaping.
- Do not reintroduce SharpDX because it seems convenient for one API call.

## Validation Checklist

For renderer changes, run as many of these as are relevant:

- `dotnet build /Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1/Direct2D1.Avalonia.csproj -c Debug --no-restore`
- `dotnet build /Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/src/Avalonia.Direct2D1.Win32/Direct2D1.Avalonia.Win32.csproj -c Debug --no-restore`
- `dotnet build /Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/samples/ControlCatalog.Direct2D1/ControlCatalog.Direct2D1.csproj -c Debug --no-restore`
- `dotnet test /Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/tests/Avalonia.Direct2D1.UnitTests/Avalonia.Direct2D1.UnitTests.csproj -c Debug --no-build`
- `dotnet test /Users/wieslawsoltes/GitHub/DirectWrite.Avalonia/tests/Avalonia.Direct2D1.RenderTests/Avalonia.Direct2D1.RenderTests.csproj -c Debug --no-build`

For any change touching presentation, swap chains, text rendering, or D3D11 device creation, also run the sample on Windows and verify:

- text is visible
- controls are visible
- resize does not throw
- DPI changes do not corrupt the surface
- repeated open/close cycles do not leak or crash

## Packaging And Release Rules

- Keep package IDs as `Direct2D1.Avalonia` and `Direct2D1.Avalonia.Win32`.
- Keep the README and package metadata aligned with the real implementation status.
- Do not claim support for unsupported Win32 composition modes.
- Do not claim full parity with Avalonia.Skia unless the missing presentation and regression gaps are actually closed.

## Commit Discipline

When the user asks for commits:

- commit granularly
- separate architecture, bug fix, tests, and sample changes when practical
- do not mix packaging renames with rendering fixes unless unavoidable
- push only after build and relevant tests pass

## What Future Work Should Prioritize

Highest value next steps:

- finish stable Windows runtime validation for text, resize, and present paths
- complete composition-backed Win32 surface modes
- improve render regression coverage with true Direct2D1 goldens generated on Windows
- reduce remaining interop adaptation complexity where official generated projections can replace custom glue cleanly
