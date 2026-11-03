/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using Vortice.Direct3D9;
using OmegaEngine.Graphics.Meshes;
using OmegaEngine.Graphics.VertexDecl;

namespace OmegaEngine.Graphics;

/// <summary>
/// Utility method for textured <see cref="Mesh"/>es.
/// </summary>
public static class TexturedMeshUtils
{
    /// <summary>
    /// Compares to vertex declaration arrays to determine whether they are equivalent
    /// </summary>
    /// <param name="baseDecl">The original declaration to use for a comparison base (must not contain excess data)</param>
    /// <param name="testDecl">The other declaration to test against (may contain excess data)</param>
    /// <returns><c>true</c> if <paramref name="testDecl"/> is equivalent to <paramref name="baseDecl"/>, <c>false</c>.</returns>
    private static bool CompareDecl(VertexElement[] baseDecl, VertexElement[] testDecl)
    {
        #region Sanity checks
        if (baseDecl == null) throw new ArgumentNullException(nameof(baseDecl));
        if (testDecl == null) throw new ArgumentNullException(nameof(testDecl));
        #endregion

        // Cancel if the number of vertex elements is too low (too high is ok, there is and end marker to trim off excess data)
        if (testDecl.Length < baseDecl.Length) return false;

        // Cancel as soon as one vertex declaration doesn't match
        return !baseDecl.Where((t, i) => !testDecl[i].Equals(t)).Any();
    }

    /// <summary>
    /// Expands the declaration of a mesh to include tangent and binormal information
    /// </summary>
    /// <param name="mesh">The mesh to be manipulated</param>
    /// <param name="hadNormals">Did the original declaration already contain normals?</param>
    /// <param name="hadTangents">Did the original declaration already contain tangents?</param>
    /// <returns><c>true</c> if the mesh declaration was expanded, <c>false</c> if the declaration didn't need to be changed</returns>
    private static bool ExpandDeclaration(ref Mesh mesh, out bool hadNormals, out bool hadTangents)
    {
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));

        // Check if vertex declaration of mesh already is already ok
        var decl = mesh.GetDeclaration();
        if (CompareDecl(PositionNormalTangentBinormalTextured.GetVertexElements(), decl))
        {
            hadNormals = true;
            hadTangents = true;
            return false;
        }
        if (CompareDecl(PositionNormalMultiTextured.GetVertexElements(), decl))
        {
            hadNormals = true;
            hadTangents = false;
            return false;
        }

        hadNormals = false;
        hadTangents = false;

        for (int i = 0; i < decl.Length && (!hadNormals || !hadTangents); i++)
        {
            if (decl[i].Usage == DeclarationUsage.Normal) hadNormals = true;
            else if (decl[i].Usage == DeclarationUsage.Tangent) hadTangents = true;
        }

        // Select the appropriate new vertex format
        decl = CompareDecl(PositionMultiTextured.GetVertexElements(), decl) ?
            PositionNormalMultiTextured.GetVertexElements() : PositionNormalTangentBinormalTextured.GetVertexElements();

        // Clone the mesh to change the vertex format
        Mesh tempMesh = mesh.Clone(decl);
        mesh.Dispose();
        mesh = tempMesh;

        return true;
    }

    /// <summary>
    /// Performs an in-place optimization of a <see cref="Mesh"/>.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/> to be optimized.</param>
    private static void Optimize(Mesh mesh)
    {
        using (new TimedLogEvent("Optimized mesh"))
            mesh.Optimize();
    }

    /// <summary>
    /// Generate normal vectors if not present and convert into a normal-carrying format for shaders.
    /// Tangent and binormal data is left empty.
    /// </summary>
    /// <param name="mesh">The mesh to be manipulated</param>
    /// <remarks>Normal vectors are required for lighting.</remarks>
    public static void GenerateNormals(ref Mesh mesh)
    {
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));

        if (!ExpandDeclaration(ref mesh, out bool hadNormals, out _)) return;

        if (CompareDecl(PositionNormalMultiTextured.GetVertexElements(), mesh.GetDeclaration()))
        {
            using (new TimedLogEvent("Computed normals"))
                mesh.ComputeNormals();
            Optimize(mesh);
            return;
        }

        bool gotMilkErmTexCoords = false;
        bool gotValidNormals = false;
        var vertexes = mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>();

        // Check all vertexes
        for (int num = 0; num < vertexes.Length; num++)
        {
            // We need at least 1 texture coordinate different from (0, 0)
            if (vertexes[num].Tu != 0.0f ||
                vertexes[num].Tv != 0.0f)
                gotMilkErmTexCoords = true;

            // A single non-zero value proves the normals were actually filled in (see GenerateTBN)
            if (vertexes[num].Normal != default)
                gotValidNormals = true;

            if (gotMilkErmTexCoords && gotValidNormals) break;
        }

        // If declaration had normals, but we found no valid normals,
        // set hadNormals to false and generate valid normals (see below)
        if (!gotValidNormals)
            hadNormals = false;

        // Generate dummy texture coordinates
        if (!gotMilkErmTexCoords)
        {
            for (int num = 0; num < vertexes.Length; num++)
            {
                vertexes[num].Tu = -0.75f + vertexes[num].Position.X / 2.0f;
                vertexes[num].Tv = +0.75f - vertexes[num].Position.Y / 2.0f;
            }
        }
        mesh.WriteVertexBuffer(vertexes);

        // Assume meshes with proper normal data also have been optimized for rendering
        if (!hadNormals)
        {
            using (new TimedLogEvent("Computed normals"))
                mesh.ComputeNormals();

            Optimize(mesh);
        }
    }

    /// <summary>
    /// Generate TBN (tangent, binormal and normal) vectors if not present and convert into <see cref="PositionNormalTangentBinormalTextured"/> format for shaders.
    /// </summary>
    /// <param name="mesh">The mesh to be manipulated</param>
    /// <param name="weldVertexes">Weld vertexes before generating tangents and share tangents between vertexes that only differ in their texture coordinates.
    /// Useful for organic objects, stones, trees, etc. (anything with a lot of round surfaces).
    /// If a lot of single faces are not connected on the texture (e.g. rockets, buildings, etc.) do not use.</param>
    /// <remarks>TBN vectors are required for normal mapping.</remarks>
    public static void GenerateTBN(ref Mesh mesh, bool weldVertexes = false)
    {
        #region Sanity checks
        if (mesh == null) throw new ArgumentNullException(nameof(mesh));
        #endregion

        bool expanded = ExpandDeclaration(ref mesh, out bool hadNormals, out bool hadTangents);

        // This format can not carry tangents, so there is nothing more we can do here
        if (CompareDecl(PositionNormalMultiTextured.GetVertexElements(), mesh.GetDeclaration()))
        {
            if (!hadNormals)
            {
                using (new TimedLogEvent("Computed normals"))
                    mesh.ComputeNormals();
                Optimize(mesh);
            }
            return;
        }

        bool gotMilkErmTexCoords = false;
        bool gotValidNormals = false;
        bool gotValidTangents = false;
        var vertexes = mesh.ReadVertexBuffer<PositionNormalTangentBinormalTextured>();

        // Check all vertexes
        for (int num = 0; num < vertexes.Length; num++)
        {
            // We need at least 1 texture coordinate different from (0, 0)
            if (vertexes[num].Tu != 0.0f ||
                vertexes[num].Tv != 0.0f)
                gotMilkErmTexCoords = true;

            // A single non-zero value proves the data was actually filled in. Do not require every vertex to be
            // non-zero: degenerate faces and vertexes with zero-area texture coordinates legitimately end up at zero,
            // no matter who computes them, so regenerating would not help.
            if (vertexes[num].Normal != default)
                gotValidNormals = true;
            if (vertexes[num].Tangent != default)
                gotValidTangents = true;

            // Once everything is proven present there isn't anything left to check here
            if (gotMilkErmTexCoords && gotValidNormals && gotValidTangents)
                break;
        }

        // If declaration had normals, but we found no valid normals,
        // set hadNormals to false and generate valid normals (see below)
        if (!gotValidNormals)
            hadNormals = false;
        // Same check for tangents
        if (!gotValidTangents)
            hadTangents = false;

        // Generate dummy texture coordinates
        if (!gotMilkErmTexCoords)
        {
            for (int num = 0; num < vertexes.Length; num++)
            {
                vertexes[num].Tu = -0.75f + vertexes[num].Position.X / 2.0f;
                vertexes[num].Tv = +0.75f - vertexes[num].Position.Y / 2.0f;
            }
            mesh.WriteVertexBuffer(vertexes);
        }

        if (!hadNormals)
        {
            using (new TimedLogEvent("Computed normals"))
                mesh.ComputeNormals();
        }

        if (!hadTangents)
        {
            using (new TimedLogEvent("Computed tangents"))
            {
                // Welding is only useful to improve the quality of tangents we generate ourselves.
                // Vertexes at the same point with the same normal also share tangents, even if their texture coordinates differ.
                // This avoids seams where a smooth surface is split on the texture, which happens usually with models exported from 3DSMax.
                if (weldVertexes) mesh.WeldVertexes(positionEpsilon: 0.0001f, normalEpsilon: 0.0001f);
                mesh.ComputeTangents(mergeSeams: weldVertexes);
            }
        }

        // Assume meshes that already came with complete data have also been optimized for rendering
        if (!expanded && hadNormals && hadTangents && gotMilkErmTexCoords) return;

        Optimize(mesh);
    }
}
