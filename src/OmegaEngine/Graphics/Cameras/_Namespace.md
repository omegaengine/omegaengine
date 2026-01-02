---
uid: OmegaEngine.Graphics.Cameras
summary: Cameras in OmegaEngine determine the perspective from which a <xref:OmegaEngine.Graphics.Scene> is displayed in a <xref:OmegaEngine.Graphics.View>.
---
Cameras manage:

- **Position and orientation** - The camera's location and direction in 3D space
- **Projection parameters** - Field of view, near/far clipping planes, aspect ratio
- **View frustum** - Automatic calculation for culling objects outside the camera's view
- **Input handling** - Integration with [input system](xref:OmegaEngine.Input) for interactive camera control

You can pick from several [built-in camera types](#classes) to suit your needs.

## Floating coordinate system

The positions of [renderables](xref:OmegaEngine.Graphics.Renderables), [light sources](xref:OmegaEngine.Graphics.LightSources), etc. are stored with 64-bit precision in OmegaEngine. However, they must be converted to 32-bit coordinates for rendering with DirectX. To mitigate the resulting loss of precision, OmegaEngine uses an internal floating coordinate system.

The origin of this coordinate system is defined by the camera's <xref:OmegaEngine.Graphics.Cameras.Camera.FloatingOrigin> property. This value is subtracted from each 64-bit position before conversion to 32-bit and submission to DirectX.

Most camera types automatically adjust `FloatingOrigin` once the camera moves more than <xref:OmegaEngine.Graphics.Cameras.Camera.FloatingOriginMaxDistance> units away from it. This keeps the coordinate origin close to the camera, ensuring higher precision for objects near the viewer.

> [!TIP]
> To pick a suitable value for `FloatingOriginMaxDistance`, you can use this formula:  
> $FloatingOriginMaxDistance=DesiredMaxError*2^{23}$
>
> For example, if 1 world unit corresponds to 1 meter and you want to achieve a precision of 1 millimeter:  
> $FloatingOriginMaxDistance=0.001*2^{23}\approx8388$

## API
