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
/// A fixed-function vertex format that stores position and color.
/// Using this format hints the engine that no lighting is to be used.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionColored
{
    /// <summary>
    /// The fixed-function format of this vertex structure.
    /// </summary>
    public const VertexFormat Format = VertexFormat.Position | VertexFormat.Diffuse;

    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position;

    /// <summary>The color of the vertex</summary>
    public int Color;

    /// <summary>
    /// Creates a new positioned, colored vertex
    /// </summary>
    /// <param name="position">The position of the vertex in entity-space</param>
    /// <param name="color">The color of the vertex</param>
    public PositionColored(Vector3 position, int color)
    {
        Position = position;
        Color = color;
    }

    /// <summary>
    /// Creates a new positioned, colored vertex
    /// </summary>
    /// <param name="xvalue">The X-component of the position of the vertex in entity-space</param>
    /// <param name="yvalue">The Y-component of the position of the vertex in entity-space</param>
    /// <param name="zvalue">The Z-component of the position of the vertex in entity-space</param>
    /// <param name="color">The color of the vertex</param>
    public PositionColored(float xvalue, float yvalue, float zvalue, int color)
        : this(new(xvalue, yvalue, zvalue), color)
    {}

    public override string ToString() => $"{nameof(PositionColored)}(position={Position}, color={Color})";
}
