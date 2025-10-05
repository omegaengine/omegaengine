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
/// Not supported!
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionColoredTextured
{
    #region Constants
    /// <summary>
    /// The fixed-function format of this vertex structure.
    /// </summary>
    public const VertexFormat Format = VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture1;

    /// <summary>
    /// The length of this vertex structure in bytes.
    /// </summary>
    public const int StrideSize = 6 * 4;
    #endregion

    #region Variables
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position;

    /// <summary>A color by which the texture will be multiplied</summary>
    public int Color;

    /// <summary>The U-component of the texture coordinates</summary>
    public float Tu;

    /// <summary>The V-component of the texture coordinates</summary>
    public float Tv;

    // ReSharper restore FieldCanBeMadeReadOnly.Global
    // ReSharper restore MemberCanBePrivate.Global
    #endregion

    #region ToString
    public override string ToString() => $"PositionColoredTextured(position={Position}, color={Color}, tu={Tu}, tv={Tv})";
    #endregion
}
