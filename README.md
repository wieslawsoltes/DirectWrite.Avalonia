# Direct2D1.Avalonia

Standalone Windows `Direct2D1 + DirectWrite` rendering backend for Avalonia, packaged outside the main Avalonia repository.

This repository currently targets `Avalonia 12.0.1` and `net10.0-windows10.0.19041.0`.

Current Win32 integration defaults to `Win32RenderingMode.Software` with `Win32CompositionMode.RedirectionSurface`. Composition-backed `AngleEgl` modes are not yet wired into the standalone package.
