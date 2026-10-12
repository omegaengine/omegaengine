# Dispose

All classes in the <xref:OmegaEngine> namespace implementing the  `IDisposable` interface must be `.Dispose()`ed manually.

Unlike other .NET objects you can not rely on the garbage collection to cleanup left-over resources here. This is because of circular references caused by [event hooks](#device-reset) as well as the [asset caching system](xref:OmegaEngine.Assets#cache).

If you forget a `.Dispose()` this may trigger an exception (in Debug mode) or a log entry (in Release mode) at a non-deterministic point in time.

## Leak reporting

<xref:OmegaEngine.RenderHost> assumes it is the only user of Direct3D in the process. It enables the object tracking of [SharpGen](https://github.com/SharpGenTools/SharpGenTools), on which Vortice and the engine's D3DX wrappers are built. When it is disposed it therefore checks whether any DirectX objects are still alive and reports each one as a log entry:

```
ERROR: Object of type OmegaEngine.D3DX.Mesh was not disposed. Set OMEGAENGINE_TRACK_OBJECTS=1 to also log the stack trace of the object creation.
	Total of 10 objects still alive.
```

Such leftovers are usually a symptom rather than a cause: a <xref:OmegaEngine.Graphics.Renderables.Renderable> that was never `.Dispose()`ed keeps the DirectX objects and the [cached assets](xref:OmegaEngine.Assets#cache) it holds alive. Watch out for the accompanying `References were not properly released for ...` entries from the <xref:OmegaEngine.Assets.CacheManager>; they name the asset files involved and are often easier to trace back to your own code.

> [!NOTE]
> <xref:OmegaEngine.EngineElement> silently skips `OnDispose()` if its <xref:OmegaEngine.EngineElement.Engine> was never set. An element that allocates DirectX resources (e.g. via `Model.Sphere()`) but is never added to a <xref:OmegaEngine.Graphics.Scene> that is in turn added to <xref:OmegaEngine.Engine.Views> will therefore leak.

### Creation stack traces

To find out where a leaked object came from, set the environment variable `OMEGAENGINE_TRACK_OBJECTS` to `1` before starting the application:

```powershell
$env:OMEGAENGINE_TRACK_OBJECTS = "1"
```

This records a stack trace for every COM object that is created. The leak report then includes that stack trace:

```
ERROR: Object of type OmegaEngine.D3DX.Mesh was not disposed. Stack trace of object creation:
	   ...
	   at OmegaEngine.D3DX.Mesh..ctor(IDirect3DDevice9 device, Int32 faceCount, Int32 vertexCount, MeshFlags options, VertexFormat format)
	   at OmegaEngine.Graphics.TexturedMesh.Sphere(...)
	   ...
```

Capturing a stack trace per COM object is expensive, so this is off by default (except in Debug builds). Objects that were garbage-collected without being disposed no longer show up in the report, even though their COM references were never released.

## Device reset

The engine automatically resets the DirectX device when it needs to be recreated due to resolution changes, switching to or from fullscreen mode, a desktop display mode change, etc..

All resources live in `Pool.Default` and are lost on a device reset, so every one of them must be released and recreated manually:

  * A delegate registered at the <xref:OmegaEngine.Engine.DeviceLost> event must release the resource using `.Dispose()`. This event fires *before* the device is reset.
  * A delegate registered at the <xref:OmegaEngine.Engine.DeviceReset> event must recreate everything released on <xref:OmegaEngine.Engine.DeviceLost>.
