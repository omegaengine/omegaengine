# Shaders

Shaders are small pieces of code executed directly on the graphics card. They govern how vertexes are transformed and how each individual pixel color is calculated.

The engine stores shaders in the High Level Shader Format (HLSL) and supports the [DirectX Standard Annotations and Semantics](#semantics). It can dynamically generate shaders at runtime and apply pixel shaders to the entire scene.

The HLSL code is stored in `.fx` files in the `Shader` subdirectory of the Engine's source directory. They are compiled to `.fxo` files via a post-build script. Each HLSL file usually has a corresponding C# class in the <xref:OmegaEngine.Graphics.Shaders> namespace.

## Semantics

The [DirectX Standard Annotations and Semantics (DXSAS)](https://docs.microsoft.com/en-us/windows/desktop/direct3d9/dx9-graphics-reference-effects-dxsas) provide a way to add metadata to HLSL code that automates the integration between shader code and the engine. By specifying things such as value bindings for transform matrices shaders can more easily be exchanged between tools such as [FX Composer](http://developer.nvidia.com/fx-composer) and different graphics engines.

The engine supports a subset of DXSAS 0.8.
