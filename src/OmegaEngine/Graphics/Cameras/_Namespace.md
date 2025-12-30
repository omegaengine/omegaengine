---
uid: OmegaEngine.Graphics.Cameras
summary: Cameras in OmegaEngine determine the perspective from which a <xref:OmegaEngine.Graphics.Scene> is displayed in a <xref:OmegaEngine.Graphics.View>.
---
All cameras inherit from <xref:OmegaEngine.Graphics.Cameras.Camera>, which provides:

- **Position and orientation** - The camera's location and direction in 3D space
- **Projection parameters** - Field of view, near/far clipping planes, aspect ratio
- **View frustum** - Automatic calculation for culling objects outside the camera's view
- **Input handling** - Optional integration with [input system](xref:OmegaEngine.Input) for interactive camera control

## Floating coordinate system

The <xref:OmegaEngine.Graphics.Cameras.Camera.PositionBase> property acts as a floating coordinate system that addresses precision limitations when working with large worlds.

Graphics hardware processes coordinates as 32-bit floats, which lose precision at large values. To maintain accuracy:

- The camera's <xref:OmegaEngine.Graphics.Cameras.Camera.Position> is stored as a 64-bit double-precision vector
- `PositionBase` is automatically adjusted to stay near the camera position
- Before rendering, all object positions are offset by subtracting `PositionBase`, converting them to 32-bit coordinates relative to the camera
- This keeps effective coordinate values small and precise, even in worlds spanning millions of units

## Camera types

### <xref:OmegaEngine.Graphics.Cameras.FreeFlyCamera>

A camera with free 6-degrees-of-freedom movement, like flying in space.

**Use when:** Building tools, debugging, or games with free-flying movement (space sims, spectator modes).

### <xref:OmegaEngine.Graphics.Cameras.FirstPersonCamera>

A first-person camera that walks on a surface with gravity. Movement is constrained to the XZ plane with rotation around the Y-axis.

**Use when:** Creating first-person or third-person games with ground-based movement.

### <xref:OmegaEngine.Graphics.Cameras.StrategyCamera>

An overhead camera perfect for real-time strategy games. Allows panning across the map and zooming in/out.

**Use when:** Building RTS games, tactical games, or top-down views.

### <xref:OmegaEngine.Graphics.Cameras.ArcballCamera>

A camera that orbits around a target point. Ideal for inspecting objects from all angles.

**Use when:** Creating model viewers, character inspection screens, or turntable presentations.

### <xref:OmegaEngine.Graphics.Cameras.TransitionCamera>

A camera that smoothly transitions between two camera states over time. This is useful for cinematic camera movements or smooth transitions between different viewpoints.

**Use when:** Creating smooth camera animations, cutscenes, or transitioning between gameplay cameras.

> [!TIP]
> Use <xref:OmegaEngine.Graphics.View.TransitionCameraTo(OmegaEngine.Graphics.Cameras.Camera,OmegaEngine.Graphics.Cameras.TransitionCameraOptions)> to smoothly transition from the current camera to a target camera:
> 
> ```csharp
> // Transition to a new camera position over 2 seconds
> var targetCamera = new FreeFlyCamera { Position = new(100, 50, 0) };
> view.TransitionCameraTo(targetCamera, options: new(Duration: TimeSpan.FromSeconds(2)));
> ```

## API
