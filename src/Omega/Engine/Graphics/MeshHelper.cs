/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using Common;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.VertexDecl;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// Provides helper methods for optimizing <see cref="Mesh"/>es, calculating normal vectors, adding textures, etc.
    /// </summary>
    internal static class MeshHelper
    {
        #region Vertex declaration helpers
        /// <summary>
        /// Compares to vertex declaration arrays to determine whether they are equivalent
        /// </summary>
        /// <param name="baseDecl">The original declaration to use for a comparison base (must not contain excess data)</param>
        /// <param name="testDecl">The other declaration to test against (may contain excess data)</param>
        /// <returns><see langword="true"/> if <paramref name="testDecl"/> is equivalent to <paramref name="baseDecl"/>, <see langword="false"/>.</returns>
        private static bool CompareDecl(VertexElement[] baseDecl, VertexElement[] testDecl)
        {
            #region Sanity checks
            if (baseDecl == null) throw new ArgumentNullException("baseDecl");
            if (testDecl == null) throw new ArgumentNullException("testDecl");
            #endregion

            // Cancel if the number of vertex elements is too low (too high is ok, there is and end marker to trim off excess data)
            if (testDecl.Length < baseDecl.Length) return false;

            // Cancel as soon as one vertex declaration doesn't match
            return !baseDecl.Where((t, i) => !testDecl[i].Equals(t)).Any();

            // If we reached here, everything is identical
        }

        /// <summary>
        /// Expands the declaration of a mesh to include tangent and binormal information
        /// </summary>
        /// <param name="device">The <see cref="Device"/> containing the mesh</param>
        /// <param name="mesh">The mesh to be manipulated</param>
        /// <param name="hadNormals">Did the original declaration already contain normals?</param>
        /// <param name="hadTangents">Did the original declaration already contain tangents?</param>
        /// <returns><see langword="true"/> if the mesh declaration was expanded, <see langword="false"/> if the declaration didn't need to be changed</returns>
        private static bool ExpandDeclaration(Device device, ref Mesh mesh, out bool hadNormals, out bool hadTangents)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (mesh == null) throw new ArgumentNullException("mesh");
            #endregion

            // Check if vertex declaration of mesh already is already ok
            var decl = mesh.GetDeclaration();
            if (CompareDecl(PositionNormalBinormalTangentTextured.GetVertexElements(), decl) ||
                CompareDecl(PositionNormalMultiTextured.GetVertexElements(), decl))
            {
                hadNormals = true;
                hadTangents = true;
                return false;
            }

            #region Find existing info
            hadNormals = false;
            hadTangents = false;

            for (int i = 0; i < 6 && i < decl.Length; i++)
            {
                if (decl[i].Usage == DeclarationUsage.Normal)
                {
                    hadNormals = true;
                    break;
                }
                if (decl[i].Usage == DeclarationUsage.Tangent)
                {
                    hadTangents = true;
                    break;
                }
            }
            #endregion

            // Select the appropriate new vertex format
            decl = CompareDecl(PositionMultiTextured.GetVertexElements(), decl) ?
                                                                                    PositionNormalMultiTextured.GetVertexElements() : PositionNormalBinormalTangentTextured.GetVertexElements();

            // Clone the mesh to change the vertex format
            Mesh tempMesh = mesh.Clone(device, mesh.CreationOptions, decl);
            mesh.Dispose();
            mesh = tempMesh;

            return true;
        }
        #endregion

        //--------------------//

        #region Optimize
        /// <summary>
        /// Performs an in-place optimization of a <see cref="Mesh"/>.
        /// </summary>
        /// <param name="mesh">The <see cref="Mesh"/> to be optimized.</param>
        public static void Optimize(Mesh mesh)
        {
            using (new TimedLogEvent("Optimized mesh"))
            {
                const MeshOptimizeFlags flags = MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.AttributeSort;

                mesh.GenerateAdjacency(0);
                mesh.OptimizeInPlace(flags);
            }
        }
        #endregion

        #region Generate normals
        /// <summary>
        /// Generate normals if not present and convert into TangentVertex format for shaders.
        /// Tangent data is left empty
        /// </summary>
        /// <param name="device">The <see cref="Device"/> containing the mesh</param>
        /// <param name="mesh">The mesh to be manipulated</param>
        public static void GenerateNormals(Device device, ref Mesh mesh)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (mesh == null) throw new ArgumentNullException("mesh");
            #endregion

            bool hadNormals, hadTangents;
            if (!ExpandDeclaration(device, ref mesh, out hadNormals, out hadTangents)) return;

            #region Check existing info
            bool gotMilkErmTexCoords = false;
            bool gotValidNormals = true;
            var vertexes = BufferHelper.ReadVertexBuffer<PositionNormalBinormalTangentTextured>(mesh);

            // Check all vertexes
            for (int num = 0; num < vertexes.Length; num++)
            {
                // We need at least 1 texture coordinate different from (0, 0)
                if (vertexes[num].Tu != 0.0f ||
                    vertexes[num].Tv != 0.0f)
                    gotMilkErmTexCoords = true;

                // All normals and tangents must be valid, otherwise generate them below
                if (vertexes[num].Normal == default(Vector3))
                    gotValidNormals = false;
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
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
            #endregion

            // Assume meshes with propper normal data also have been optimized for rendering
            if (!hadNormals)
            {
                using (new TimedLogEvent("Computed normals"))
                    mesh.ComputeNormals();

                Optimize(mesh);
            }
        }
        #endregion

        #region Generate normals and tangents
        /// <summary>
        /// Generate normals and tangents if not present and convert into TangentVertex format for shaders.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> containing the mesh</param>
        /// <param name="mesh">The mesh to be manipulated</param>
        /// <param name="weldVertexes">Weld vertexes before generating tangents.
        /// Useful for organic objects, stones, trees, etc. (anything with a lot of round surfaces).
        /// If a lot of single faces are not connected on the texture (e.g. rockets, buildings, etc.) do not use.</param>
        public static void GenerateNormalsAndTangents(Device device, ref Mesh mesh, bool weldVertexes)
        {
            #region Sanity checks
            if (device == null) throw new ArgumentNullException("device");
            if (mesh == null) throw new ArgumentNullException("mesh");
            #endregion

            bool hadNormals, hadTangents;
            if (!ExpandDeclaration(device, ref mesh, out hadNormals, out hadTangents)) return;
            var decl = mesh.GetDeclaration();

            #region Check existing info
            bool gotMilkErmTexCoords = false;
            bool gotValidNormals = true;
            bool gotValidTangents = true;
            var vertexes = BufferHelper.ReadVertexBuffer<PositionNormalBinormalTangentTextured>(mesh);

            // Check all vertexes
            for (int num = 0; num < vertexes.Length; num++)
            {
                // We need at least 1 texture coordinate different from (0, 0)
                if (vertexes[num].Tu != 0.0f ||
                    vertexes[num].Tv != 0.0f)
                    gotMilkErmTexCoords = true;

                // All normals and tangents must be valid, otherwise generate them below
                if (vertexes[num].Normal == default(Vector3))
                    gotValidNormals = false;
                if (vertexes[num].Tangent == default(Vector3))
                    gotValidTangents = false;

                // If we found valid texture coordinates and no normals or tangents,
                // there isn't anything left to check here
                if (gotMilkErmTexCoords && !gotValidNormals && !gotValidTangents)
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
            }
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
            #endregion

            if (!hadNormals)
            {
                using (new TimedLogEvent("Computed normals"))
                    mesh.ComputeNormals();
            }

            if (weldVertexes)
            {
                // Reduce amount of vertexes
                var weldEpsilons = new WeldEpsilons {Position = 0.0001f, Normal = 0.0001f};
                mesh.WeldVertices(WeldFlags.WeldPartialMatches, weldEpsilons);

                if (!hadTangents)
                {
                    #region Compute tangents
                    using (new TimedLogEvent("Computed tangents"))
                    {
                        // If the vertexes for a smoothend point exist several times the
                        // DirectX ComputeTangent method is not able to treat them all the
                        // same way.
                        // To circumvent this, we collapse all vertexes in a cloned mesh
                        // even if the texture coordinates don't fit. Then we copy the
                        // generated tangents back to the original mesh vertexes (duplicating
                        // the tangents for vertexes at the same point with the same normals
                        // if required). This happens usually with models exported from 3DSMax.

                        // Clone mesh just for tangent generation
                        Mesh dummyTangentGenerationMesh = mesh.Clone(device, mesh.CreationOptions, decl);

                        // Reuse weldEpsilons, just change the TextureCoordinates, which we don't care about anymore
                        weldEpsilons.TextureCoordinate1 = 1;
                        weldEpsilons.TextureCoordinate2 = 1;
                        weldEpsilons.TextureCoordinate3 = 1;
                        weldEpsilons.TextureCoordinate4 = 1;
                        weldEpsilons.TextureCoordinate5 = 1;
                        weldEpsilons.TextureCoordinate6 = 1;
                        weldEpsilons.TextureCoordinate7 = 1;
                        weldEpsilons.TextureCoordinate8 = 1;
                        // Rest of the weldEpsilons values can stay 0, we don't use them
                        dummyTangentGenerationMesh.WeldVertices(WeldFlags.WeldPartialMatches, weldEpsilons);

                        // Compute tangents
                        if (!CompareDecl(PositionNormalMultiTextured.GetVertexElements(), decl))
                            dummyTangentGenerationMesh.ComputeTangent(0, 0, 0, false);
                        var tangentVertexes = BufferHelper.ReadVertexBuffer<PositionNormalBinormalTangentTextured>(dummyTangentGenerationMesh);
                        dummyTangentGenerationMesh.Dispose();

                        // Copy generated tangents back
                        vertexes = BufferHelper.ReadVertexBuffer<PositionNormalBinormalTangentTextured>(mesh);
                        for (int num = 0; num < vertexes.Length; num++)
                        {
                            // Search for tangent vertex with the exact same position and normal.
                            for (int tangentVertexNum = 0; tangentVertexNum < tangentVertexes.Length; tangentVertexNum++)
                            {
                                if (vertexes[num].Position == tangentVertexes[tangentVertexNum].Position && vertexes[num].Normal == tangentVertexes[tangentVertexNum].Normal)
                                {
                                    // Copy the tangent over
                                    vertexes[num].Tangent = tangentVertexes[tangentVertexNum].Tangent;
                                    // No more checks required, proceed with next vertex
                                    break;
                                }
                            }
                        }
                        BufferHelper.WriteVertexBuffer(mesh, vertexes);
                    }
                    #endregion
                }
            }
            else
            {
                if (!hadTangents && CompareDecl(PositionNormalMultiTextured.GetVertexElements(), decl))
                {
                    using (new TimedLogEvent("Computed tangents"))
                        mesh.ComputeTangent(0, 0, D3DX.Default, false);
                }
            }

            Optimize(mesh);
        }
        #endregion
    }
}
