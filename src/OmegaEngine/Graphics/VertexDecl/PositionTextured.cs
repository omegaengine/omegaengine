/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.VertexDecl;

/// <summary>
/// A fixed-function vertex format that stores position and texture coordinates.
/// Using this format hints the engine that normals and tangents still need to be calculated.
/// </summary>
/// <param name="position">The position of the vertex in entity-space</param>
/// <param name="tu">The U-component of the texture coordinates</param>
/// <param name="tv">The V-component of the texture coordinates</param>
[StructLayout(LayoutKind.Sequential)]
public struct PositionTextured(Vector3 position, float tu, float tv)
{
    /// <summary>
    /// The fixed-function format of this vertex structure.
    /// </summary>
    public const VertexFormat Format = VertexFormat.Position | VertexFormat.Texture1;

    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position = position;

    /// <summary>The U-component of the texture coordinates</summary>
    public float Tu = tu;

    /// <summary>The V-component of the texture coordinates</summary>
    public float Tv = tv;

    /// <summary>
    /// Creates a new positioned, textured vertex
    /// </summary>
    /// <param name="xvalue">The X-component of the position of the vertex in entity-space</param>
    /// <param name="yvalue">The Y-component of the position of the vertex in entity-space</param>
    /// <param name="zvalue">The Z-component of the position of the vertex in entity-space</param>
    /// <param name="tu">The U-component of the texture coordinates</param>
    /// <param name="tv">The V-component of the texture coordinates</param>
    public PositionTextured(float xvalue, float yvalue, float zvalue, float tu, float tv)
        : this(new(xvalue, yvalue, zvalue), tu, tv)
    {}

    public override string ToString() => $"{nameof(PositionTextured)}(position={Position}, tu={Tu}, tv={Tv})";
}
