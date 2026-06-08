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
/// A custom vertex format that stores position, normals, binormals, tangents and texture coordinates.
/// Using this format hints the engine that all necessary data is already present.
/// </summary>
/// <param name="position">The position of the vertex in entity-space</param>
/// <param name="normal">The normal of the vertex in entity-space</param>
/// <param name="binormal">The binormal of the vertex in entity-space</param>
/// <param name="tangent">The tangent of the vertex in entity-space</param>
/// <param name="tu">The U-component of the texture coordinates</param>
/// <param name="tv">The V-component of the texture coordinates</param>
[StructLayout(LayoutKind.Sequential)]
public struct PositionNormalBinormalTangentTextured(Vector3 position, Vector3 normal, Vector3 binormal, Vector3 tangent, float tu, float tv)
{
    /// <summary>The position of the vertex in entity-space</summary>
    public Vector3 Position = position;

    /// <summary>The normal of the vertex in entity-space</summary>
    public Vector3 Normal = normal;

    /// <summary>The U-component of the texture coordinates</summary>
    public float Tu = tu;

    /// <summary>The V-component of the texture coordinates</summary>
    public float Tv = tv;

    /// <summary>The binormal of the vertex in entity-space</summary>
    public Vector3 Binormal = binormal;

    /// <summary>The tangent of the vertex in entity-space</summary>
    public Vector3 Tangent = tangent;

    public override string ToString() => $"{nameof(PositionNormalBinormalTangentTextured)}(position={Position}, normal={Normal}, tu={Tu}, tv={Tv}, binormal={Binormal}, tangent={Tangent})";

    /// <summary>
    /// Returns an array describing the usage of the vertex fields
    /// </summary>
    public static VertexElement[] GetVertexElements() =>
    [
        // Position
        new(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
        // Normal
        new(0, sizeof(float) * 3, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Normal, 0),
        // Tu, Tv
        new(0, sizeof(float) * 6, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0),
        // Binormal
        new(0, sizeof(float) * 8, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Binormal, 0),
        // Tangent
        new(0, sizeof(float) * 11, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Tangent, 0),
        // End
        VertexElement.VertexDeclarationEnd
    ];
}
