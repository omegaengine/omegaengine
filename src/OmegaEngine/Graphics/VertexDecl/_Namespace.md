---
uid: OmegaEngine.Graphics.VertexDecl
summary: Structs mapped to DirectX custom vertex formats (for storing additional information per vertex).
---
Each struct in this namespace corresponds to a specific DirectX vertex declaration, specifying which attributes (position, normal, texture coordinates, color, etc.) are packed together for each vertex.

These types are used internally by OmegaEngine when building geometry buffers for models, terrain, and particle systems. You typically do not need to work with them directly unless you are implementing custom <xref:OmegaEngine.Graphics.Renderables>.
