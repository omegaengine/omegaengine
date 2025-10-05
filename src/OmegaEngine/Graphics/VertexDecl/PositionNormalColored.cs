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
[StructLayout(LayoutKind.Sequential)]
public struct PositionNormalColored
{
    #region Constants
    /// <summary>
    /// The fixed-function format of this vertex structure.
    /// </summary>
    public const VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Diffuse;

    /// <summary>
    /// The length of this vertex structure in bytes.
    /// </summary>
    public const int StrideSize = 7 * 4;
    #endregion

    #region Variables
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position;

    /// <summary>The normal of the vertex in entity-space</summary>
    public Vector3 Normal;

    /// <summary>The color of the vertex</summary>
    public int Color;

    // ReSharper restore FieldCanBeMadeReadOnly.Global
    // ReSharper restore MemberCanBePrivate.Global
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new positioned, colored vertex with normal information
    /// </summary>
    /// <param name="position">The position of the vertex in entity-space</param>
    /// <param name="normal">The normal of the vertex in entity-space</param>
    /// <param name="color">The color of the vertex</param>
    public PositionNormalColored(Vector3 position, Vector3 normal, int color)
    {
        Position = position;
        Normal = normal;
        Color = color;
    }
    #endregion

    #region ToString
    public override string ToString() => $"PositionNormalColored(position={Position}, normal={Normal}, color={Color})";
    #endregion
}
