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
/// A fixed-function vertex format that stores position, normals and texture coordinates.
/// Using this format hints the engine that tangents (and maybe normals) still need to be calculated.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionNormalTextured
{
    #region Constants
    /// <summary>
    /// The fixed-function format of this vertex structure.
    /// </summary>
    public const VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;

    /// <summary>
    /// The length of this vertex structure in bytes.
    /// </summary>
    public const int StrideSize = 8 * 4;
    #endregion

    #region Variables
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position;

    /// <summary>The normal of the vertex in entity-space</summary>
    public Vector3 Normal;

    /// <summary>The U-component of the texture coordinates</summary>
    public float Tu;

    /// <summary>The V-component of the texture coordinates</summary>
    public float Tv;

    // ReSharper restore FieldCanBeMadeReadOnly.Global
    // ReSharper restore MemberCanBePrivate.Global
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new position, textured vertex with normal information
    /// </summary>
    /// <param name="position">The position of the vertex in entity-space</param>
    /// <param name="normal">The normal of the vertex in entity-space</param>
    /// <param name="tu">The U-component of the texture coordinates</param>
    /// <param name="tv">The V-component of the texture coordinates</param>
    public PositionNormalTextured(Vector3 position, Vector3 normal, float tu, float tv)
    {
        Position = position;
        Normal = normal;
        Tu = tu;
        Tv = tv;
    }
    #endregion

    #region ToString
    public override string ToString() => $"PositionNormalTextured(position={Position}, normal={Normal}, tu={Tu}, tv={Tv})";
    #endregion
}
