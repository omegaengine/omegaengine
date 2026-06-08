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
/// A fixed-function vertex format that stores a transformed position, color and texture coordinates.
/// </summary>
/// <param name="position">The position of the vertex in screen-space</param>
/// <param name="rhw">The reciprocal of homogeneous W (the depth-value)</param>
/// <param name="color">A color by which the texture will be multiplied</param>
/// <param name="tu">The U-component of the texture coordinates</param>
/// <param name="tv">The V-component of the texture coordinates</param>
[StructLayout(LayoutKind.Sequential)]
public struct TransformedColoredTextured(Vector3 position, float rhw, int color, float tu, float tv)
{
    /// <summary>
    /// The fixed-function format of this vertex structure.
    /// </summary>
    public const VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture1;

    /// <summary>The position of the vertex in screen-space</summary>
    public Vector3 Position = position;

    /// <summary>The reciprocal of homogeneous W (the depth-value)</summary>
    public float Rhw = rhw;

    /// <summary>A color by which the texture will be multiplied</summary>
    public int Color = color;

    /// <summary>The U-component of the texture coordinates</summary>
    public float Tu = tu;

    /// <summary>The V-component of the texture coordinates</summary>
    public float Tv = tv;

    /// <summary>
    /// Creates a new transformed, colored and textured vertex
    /// </summary>
    /// <param name="xvalue">The X-component of the position of the vertex in screen-space</param>
    /// <param name="yvalue">The Y-component of the position of the vertex in screen-space</param>
    /// <param name="zvalue">The Z-component of the position of the vertex in screen-space</param>
    /// <param name="rhw">The reciprocal of homogeneous W (the depth-value)</param>
    /// <param name="color">A color by which the texture will be multiplied</param>
    /// <param name="tu">The U-component of the texture coordinates</param>
    /// <param name="tv">The V-component of the texture coordinates</param>
    public TransformedColoredTextured(float xvalue, float yvalue, float zvalue, float rhw, int color, float tu, float tv)
        : this(new(xvalue, yvalue, zvalue), rhw, color, tu, tv)
    {}

    public override string ToString() => $"{nameof(TransformedColoredTextured)}(position={Position}, rhw={Rhw}, color={Color}, tu={Tu}, tv={Tv})";
}
