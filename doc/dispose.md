# Dispose

All classes in the <xref:OmegaEngine> namespace implementing the  `IDisposable` interface must be `.Dispose()`ed manually.

Unlike other .NET objects you can not rely on the garbage collection to cleanup left-over resources here. This is because of circular references caused by [event hooks](#lost-device) as well as the [asset](assets.md) management system's caching feature.

If you forget a `.Dispose()` this may trigger an exception (in Debug mode) or a log entry (in Release mode) at a non-deterministic point in time.

## Lost device

The engine automatically restores a DirectX device if it is lost due to resolution changes, minimizing a fullscreen application, etc..

To reduce the amount of required manual reloading resources are stored in `Pool.Managed` whenever possible.

When this is not possible:

  * A delegate registered at the <xref:OmegaEngine.Engine.DeviceLost> event must release the resource using `.Dispose()`.
  * A delegate registered at the <xref:OmegaEngine.Engine.DeviceReset> event must reload the resource.
