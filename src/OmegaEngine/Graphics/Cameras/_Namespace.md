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

## API
