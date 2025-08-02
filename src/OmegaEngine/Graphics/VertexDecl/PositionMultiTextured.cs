/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.VertexDecl;

/// <summary>
/// A fixed-function vertex format that stores position, shadow information, texture blending weights and texture coordinates.
/// Using this format hints the engine that normals and tangents still need to be calculated.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct PositionMultiTextured
{
    #region Constants
    /// <summary>
    /// The length of this vertex structure in bytes.
    /// </summary>
    public const int StrideSize = 27 * 4;
    #endregion

    #region Variables
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position;

    /// <summary>The U-component of the texture coordinates</summary>
    public float Tu;

    /// <summary>The V-component of the texture coordinates</summary>
    public float Tv;

    /// <summary>The angles at which the global light source occlusion begins and ends</summary>
    public Vector4 OcclusionIntervals;

    /// <summary>Texture blending weights</summary>
    public Vector4 TexWeights1, TexWeights2, TexWeights3, TexWeights4;

    /// <summary>A color by which the texture will be multiplied</summary>
    public Color4 Color;

    // ReSharper restore FieldCanBeMadeReadOnly.Global
    // ReSharper restore MemberCanBePrivate.Global
    #endregion

    #region Properties
    /// <summary>The X-component of the position of the vertex in entity-space</summary>
    public float X { get => Position.X; set => Position.X = value; }

    /// <summary>The Y-component of the position of the vertex in entity-space</summary>
    public float Y { get => Position.Y; set => Position.Y = value; }

    /// <summary>The Z-component of the position of the vertex in entity-space</summary>
    public float Z { get => Position.Z; set => Position.Z = value; }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new multi-textured vertex
    /// </summary>
    /// <param name="position">The position of the vertex in entity-space</param>
    /// <param name="tu">The U-component of the texture coordinates</param>
    /// <param name="tv">The V-component of the texture coordinates</param>
    /// <param name="occlusionIntervals">The angles at which the global light source occlusion begins and ends</param>
    /// <param name="texWeights">A 16-element array of texture blending weight</param>
    /// <param name="color">A color by which the texture will be multiplied</param>
    public PositionMultiTextured(Vector3 position, float tu, float tv, Vector4 occlusionIntervals, float[] texWeights, Color4 color)
    {
        #region Sanity checks
        if (texWeights == null) throw new ArgumentNullException(nameof(texWeights));
        if (texWeights.Length != 16)
            throw new ArgumentException(string.Format(Resources.WrongTexArrayLength, "16"), nameof(texWeights));
        #endregion

        Position = position;
        Tu = tu;
        Tv = tv;
        OcclusionIntervals = occlusionIntervals;
        TexWeights1 = new(texWeights[0], texWeights[1], texWeights[2], texWeights[3]);
        TexWeights2 = new(texWeights[4], texWeights[5], texWeights[6], texWeights[7]);
        TexWeights3 = new(texWeights[8], texWeights[9], texWeights[10], texWeights[11]);
        TexWeights4 = new(texWeights[12], texWeights[13], texWeights[14], texWeights[15]);
        Color = color;
    }
    #endregion

    #region ToString
    public override string ToString()
    {
        return "PositionMultiTextured(position=" + Position + ", " +
               "tu=" + Tu + ", " + "tv=" + Tv + ", color=" + Color + ", " +
               "texWeights=" + TexWeights1 + TexWeights2 + TexWeights3 + TexWeights4 + ")";
    }
    #endregion

    //--------------------//

    #region Vertex declaration
    /// <summary>
    /// Returns an array describing the usage of the vertex fields
    /// </summary>
    public static VertexElement[] GetVertexElements()
    {
        return
        [
            // Position
            new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default,
                DeclarationUsage.Position, 0),
            // Tu, Tv
            new VertexElement(0, sizeof(float) * 3, DeclarationType.Float2, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 0),
            // OcclusionIntervals
            new VertexElement(0, sizeof(float) * 5, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 1),
            // TexWeights1
            new VertexElement(0, sizeof(float) * 9, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 2),
            // TexWeights2
            new VertexElement(0, sizeof(float) * 13, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 3),
            // TexWeights3
            new VertexElement(0, sizeof(float) * 17, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 4),
            // TexWeights4
            new VertexElement(0, sizeof(float) * 21, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.TextureCoordinate, 5),
            // Color
            new VertexElement(0, sizeof(float) * 25, DeclarationType.Float4, DeclarationMethod.Default,
                DeclarationUsage.Color, 0),
            // End
            VertexElement.VertexDeclarationEnd
        ];
    }
    #endregion
}