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
/// A fixed-function vertex format that stores position, normals and color.
/// Using this format hints the engine that that lighting is to be used.
/// </summary>
/// <param name="position">The position of the vertex in entity-space</param>
/// <param name="normal">The normal of the vertex in entity-space</param>
/// <param name="color">The color of the vertex</param>
[StructLayout(LayoutKind.Sequential)]
public struct PositionNormalColored(Vector3 position, Vector3 normal, int color)
{
    /// <summary>
    /// The fixed-function format of this vertex structure.
    /// </summary>
    public const VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Diffuse;

    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position = position;

    /// <summary>The normal of the vertex in entity-space</summary>
    public Vector3 Normal = normal;

    /// <summary>The color of the vertex</summary>
    public int Color = color;

    public override string ToString() => $"{nameof(PositionNormalColored)}(position={Position}, normal={Normal}, color={Color})";
}
