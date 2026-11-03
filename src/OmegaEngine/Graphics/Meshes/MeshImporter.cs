/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using NanoByte.Common.Storage;
using OmegaEngine.Graphics.VertexDecl;
using SharpAssimp;
using Vortice.Direct3D9;
using Vortice.Mathematics;
using AssimpMesh = SharpAssimp.Mesh;
using AssimpScene = SharpAssimp.Scene;
using PrimitiveType = SharpAssimp.PrimitiveType;

namespace OmegaEngine.Graphics.Meshes;

/// <summary>
/// A material as loaded from a model file.
/// </summary>
/// <param name="Material">The lighting properties.</param>
/// <param name="TextureFileName">The diffuse texture, relative to the model file; <c>null</c> if there is none.</param>
internal sealed record ImportedMaterial(Material9 Material, string? TextureFileName);

/// <summary>
/// A node in the frame hierarchy of an animated model.
/// </summary>
/// <param name="Name">The name of the frame, referenced by bones and animations.</param>
/// <param name="Parent">The index of the parent frame; <c>-1</c> for the root.</param>
/// <param name="Transform">The transform relative to the parent frame in the bind pose.</param>
internal sealed record ImportedFrame(string Name, int Parent, Matrix4x4 Transform);

/// <summary>
/// A mesh attached to a frame of an animated model.
/// </summary>
/// <param name="FrameIndex">The index of the frame the mesh is attached to.</param>
/// <param name="Mesh">The mesh in its bind pose, with a single subset.</param>
/// <param name="Material">The material of the mesh.</param>
/// <param name="Skin">The skinning information; <c>null</c> for rigid meshes.</param>
internal sealed record ImportedContainer(int FrameIndex, Mesh Mesh, ImportedMaterial Material, SkinInfo? Skin);

/// <summary>
/// An animated model with a frame hierarchy.
/// </summary>
/// <param name="Frames">The frames, parents before their children.</param>
/// <param name="Containers">The meshes attached to the frames.</param>
/// <param name="Animations">The animations, in the order of the file.</param>
/// <param name="BoundingSphere">A bounding sphere surrounding all meshes in the bind pose.</param>
internal sealed record ImportedHierarchy(IReadOnlyList<ImportedFrame> Frames, IReadOnlyList<ImportedContainer> Containers, IReadOnlyList<AnimationClip> Animations, BoundingSphere BoundingSphere);

/// <summary>
/// Loads model files using Assimp.
/// </summary>
internal static class MeshImporter
{
    /// <summary>The content type model files are loaded from.</summary>
    private const string ContentType = "Meshes";

    /// <summary>Converts to Direct3D's left-handed coordinate system and triangulates, so the result matches what D3DX loaded from .X files.</summary>
    private const PostProcessSteps Steps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.SortByPrimitiveType
                                         | PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipUVs | PostProcessSteps.FlipWindingOrder;

    /// <summary>
    /// Loads a model file and merges all its meshes into one, with one subset per material.
    /// </summary>
    /// <param name="device">The device to draw the mesh on.</param>
    /// <param name="id">The ID of the model file in the <c>Meshes</c> content directory.</param>
    /// <exception cref="InvalidDataException">The file does not contain a valid model.</exception>
    public static (Mesh Mesh, ImportedMaterial[] Materials) LoadStatic(IDirect3DDevice9 device, string id)
    {
        var scene = Import(id);

        var parts = new List<(AssimpMesh Mesh, Matrix4x4 Transform)>();
        void Walk(Node node, Matrix4x4 parentTransform)
        {
            var transform = ToMatrix(node.Transform) * parentTransform;
            foreach (int meshIndex in node.MeshIndices)
            {
                var mesh = scene.Meshes[meshIndex];
                if (mesh.PrimitiveType.HasFlag(PrimitiveType.Triangle)) parts.Add((mesh, transform));
            }
            foreach (var child in node.Children) Walk(child, transform);
        }
        Walk(scene.RootNode, Matrix4x4.Identity);
        if (parts.Count == 0) throw new InvalidDataException($"{id} contains no triangle meshes.");

        var result = Build(device, parts, mesh => mesh.MaterialIndex);
        return (result, scene.Materials.Select(ToMaterial).ToArray());
    }

    /// <summary>
    /// Loads a model file with its frame hierarchy and animations.
    /// </summary>
    /// <param name="device">The device to draw the meshes on.</param>
    /// <param name="id">The ID of the model file in the <c>Meshes</c> content directory.</param>
    /// <exception cref="InvalidDataException">The file does not contain a valid model.</exception>
    public static ImportedHierarchy LoadHierarchy(IDirect3DDevice9 device, string id)
    {
        var scene = Import(id);
        var materials = scene.Materials.Select(ToMaterial).ToArray();

        var frames = new List<ImportedFrame>();
        var frameIndexes = new Dictionary<string, int>();
        var containers = new List<ImportedContainer>();
        var points = new List<Vector3>();
        try
        {
            void Walk(Node node, int parent, Matrix4x4 parentTransform)
            {
                int index = frames.Count;
                var local = ToMatrix(node.Transform);
                var combined = local * parentTransform;
                frames.Add(new(node.Name ?? "", parent, local));
                if (!string.IsNullOrEmpty(node.Name)) frameIndexes[node.Name] = index;

                foreach (int meshIndex in node.MeshIndices)
                {
                    var assimpMesh = scene.Meshes[meshIndex];
                    if (!assimpMesh.PrimitiveType.HasFlag(PrimitiveType.Triangle)) continue;

                    var mesh = Build(device, [(assimpMesh, Matrix4x4.Identity)], _ => 0);
                    var material = assimpMesh.MaterialIndex < materials.Length ? materials[assimpMesh.MaterialIndex] : new(DefaultMaterial, null);
                    containers.Add(new(index, mesh, material, ToSkinInfo(assimpMesh)));
                    points.AddRange(mesh.GetPoints().Select(x => Vector3.Transform(x, combined)));
                }

                foreach (var child in node.Children) Walk(child, index, combined);
            }
            Walk(scene.RootNode, -1, Matrix4x4.Identity);

            var animations = scene.Animations.Select(x => ToAnimationClip(x, frameIndexes)).ToList();
            var boundingSphere = points.Count == 0 ? default : BoundingSphere.CreateFromPoints(points.ToArray());
            return new(frames, containers, animations, boundingSphere);
        }
        catch
        {
            foreach (var container in containers) container.Mesh.Dispose();
            throw;
        }
    }

    #region Import
    /// <summary>
    /// Imports a model file via Assimp.
    /// </summary>
    private static AssimpScene Import(string id)
    {
        using var context = new AssimpContext();
        using var ioSystem = new ContentIOSystem();
        context.SetIOSystem(ioSystem);
        try
        {
            var scene = context.ImportFile(id, Steps);
            if (scene is not {HasMeshes: true}) throw new InvalidDataException($"{id} contains no meshes.");
            return scene;
        }
        catch (AssimpException ex)
        {
            throw new InvalidDataException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Lets Assimp open the model file and any files it references (e.g. material libraries or binary buffers) via the <see cref="Foundation.Storage.ContentManager"/>.
    /// </summary>
    private sealed class ContentIOSystem : IOSystem
    {
        public override IOStream OpenFile(string pathToFile, FileIOMode fileMode)
        {
            string id = pathToFile.Replace('\\', '/');
            while (id.StartsWith("./", StringComparison.Ordinal)) id = id[2..];
            id = id.ToNativePath();

            Stream? stream = null;
            if (fileMode is FileIOMode.Read or FileIOMode.ReadBinary or FileIOMode.ReadText && Foundation.Storage.ContentManager.FileExists(ContentType, id))
                stream = Foundation.Storage.ContentManager.GetFileStream(ContentType, id);
            return new ContentIOStream(stream, pathToFile, fileMode);
        }
    }

    private sealed class ContentIOStream(Stream? stream, string pathToFile, FileIOMode fileMode) : IOStream(pathToFile, fileMode)
    {
        public override bool IsValid => stream != null;

        public override long Read(byte[] dataRead, long count) => stream?.Read(dataRead, 0, (int)count) ?? 0;

        public override long Write(byte[] dataToWrite, long count) => 0;

        public override ReturnCode Seek(long offset, Origin seekOrigin)
        {
            if (stream is not {CanSeek: true}) return ReturnCode.Failure;
            stream.Seek(offset, seekOrigin switch
            {
                Origin.Current => SeekOrigin.Current,
                Origin.End => SeekOrigin.End,
                _ => SeekOrigin.Begin
            });
            return ReturnCode.Success;
        }

        public override long GetPosition() => stream?.Position ?? 0;

        public override long GetFileSize() => stream?.Length ?? 0;

        public override void Flush() {}

        protected override void Dispose(bool disposing)
        {
            if (disposing) stream?.Dispose();
            base.Dispose(disposing);
        }
    }
    #endregion

    #region Meshes
    /// <summary>
    /// Merges Assimp meshes into a single mesh.
    /// </summary>
    /// <param name="device">The device to draw the mesh on.</param>
    /// <param name="parts">The meshes to merge and the transform to apply to each.</param>
    /// <param name="getSubset">Determines the subset for the faces of each mesh.</param>
    private static Mesh Build(IDirect3DDevice9 device, IReadOnlyList<(AssimpMesh Mesh, Matrix4x4 Transform)> parts, Func<AssimpMesh, int> getSubset)
    {
        bool normals = parts.Any(x => x.Mesh.HasNormals);
        bool texCoords = parts.Any(x => x.Mesh.HasTextureCoords(0));
        bool colors = parts.Any(x => x.Mesh.HasVertexColors(0));
        bool tangents = parts.All(x => x.Mesh.HasNormals && x.Mesh.HasTextureCoords(0) && x.Mesh.HasTangentBasis);

        // Use the engine's tangent-carrying format directly if the file provides everything it needs
        var declaration = tangents ? PositionNormalTangentBinormalTextured.GetVertexElements() : BuildDeclaration(normals, colors, texCoords);
        int positionOffset = declaration.Find(DeclarationUsage.Position)!.Value.Offset;
        int? normalOffset = declaration.Find(DeclarationUsage.Normal)?.Offset;
        int? colorOffset = declaration.Find(DeclarationUsage.Color)?.Offset;
        int? texCoordOffset = declaration.Find(DeclarationUsage.TextureCoordinate)?.Offset;
        int? tangentOffset = declaration.Find(DeclarationUsage.Tangent)?.Offset;
        int? binormalOffset = declaration.Find(DeclarationUsage.Binormal)?.Offset;

        int vertexCount = parts.Sum(x => x.Mesh.VertexCount);
        int faceCount = parts.Sum(x => x.Mesh.Faces.Count(face => face.IndexCount == 3));
        var mesh = new Mesh(device, faceCount, vertexCount, declaration);
        var vertexes = mesh.VertexData;
        int stride = mesh.BytesPerVertex;
        var indexes = new int[faceCount * 3];
        var attributes = new int[faceCount];

        int vertexBase = 0, faceIndex = 0;
        foreach (var (part, transform) in parts)
        {
            Matrix4x4.Invert(transform, out var inverse);
            var normalTransform = Matrix4x4.Transpose(inverse);

            for (int i = 0; i < part.VertexCount; i++)
            {
                var vertex = vertexes.Slice((vertexBase + i) * stride, stride);
                Write(vertex, positionOffset, Vector3.Transform(part.Vertices[i], transform));
                if (normalOffset is {} n && part.HasNormals)
                    Write(vertex, n, NormalizeOrZero(Vector3.TransformNormal(part.Normals[i], normalTransform)));
                if (tangentOffset is {} t && binormalOffset is {} b)
                {
                    Write(vertex, t, NormalizeOrZero(Vector3.TransformNormal(part.Tangents[i], transform)));
                    Write(vertex, b, NormalizeOrZero(Vector3.TransformNormal(part.BiTangents[i], transform)));
                }
                if (texCoordOffset is {} uv && part.HasTextureCoords(0))
                {
                    var texCoord = part.TextureCoordinateChannels[0][i];
                    Write(vertex, uv, new Vector2(texCoord.X, texCoord.Y));
                }
                if (colorOffset is {} c)
                    Write(vertex, c, part.HasVertexColors(0) ? ToD3DColor(part.VertexColorChannels[0][i]) : 0xFFFFFFFF);
            }

            int subset = getSubset(part);
            foreach (var face in part.Faces)
            {
                if (face.IndexCount != 3) continue;
                for (int corner = 0; corner < 3; corner++)
                    indexes[faceIndex * 3 + corner] = vertexBase + face.Indices[corner];
                attributes[faceIndex++] = subset;
            }

            vertexBase += part.VertexCount;
        }

        mesh.InvalidateVertexes();
        mesh.WriteIndexBuffer(indexes);
        mesh.WriteAttributeBuffer(attributes);
        return mesh;
    }

    /// <summary>
    /// Builds a vertex declaration in the order a fixed-function vertex format would use.
    /// </summary>
    private static VertexElement[] BuildDeclaration(bool normals, bool colors, bool texCoords)
    {
        var format = VertexFormat.Position;
        if (normals) format |= VertexFormat.Normal;
        if (colors) format |= VertexFormat.Diffuse;
        if (texCoords) format |= VertexFormat.Texture1;
        return VertexElements.FromFormat(format);
    }

    /// <summary>
    /// Packs a color into a <c>D3DCOLOR</c> (ARGB).
    /// </summary>
    private static uint ToD3DColor(Vector4 color)
    {
        static uint Channel(float value) => (uint)Math.Clamp(MathF.Round(value * 255), 0, 255);
        return (Channel(color.W) << 24) | (Channel(color.X) << 16) | (Channel(color.Y) << 8) | Channel(color.Z);
    }

    private static void Write<T>(Span<byte> vertex, int offset, T value) where T : unmanaged
        => MemoryMarshal.Write(vertex[offset..], in value);

    private static Vector3 NormalizeOrZero(Vector3 vector)
        => vector == default ? default : Vector3.Normalize(vector);

    /// <summary>
    /// Converts an Assimp matrix (column vectors) to a <see cref="System.Numerics"/> matrix (row vectors).
    /// </summary>
    private static Matrix4x4 ToMatrix(Matrix4x4 matrix) => Matrix4x4.Transpose(matrix);
    #endregion

    #region Materials
    private static readonly Material9 DefaultMaterial = new()
    {
        Diffuse = new() {R = 1, G = 1, B = 1, A = 1},
        Ambient = new() {A = 1},
        Specular = new() {A = 1},
        Emissive = new() {A = 1},
        Power = 1
    };

    /// <summary>
    /// Converts an Assimp material to the equivalent of what D3DX loaded from .X files.
    /// </summary>
    private static ImportedMaterial ToMaterial(Material material)
    {
        static Colorvalue ToColorvalue(Vector4 color) => new() {R = color.X, G = color.Y, B = color.Z, A = color.W};

        var result = DefaultMaterial;
        if (material.HasColorDiffuse) result.Diffuse = ToColorvalue(material.ColorDiffuse);
        if (material.HasColorAmbient) result.Ambient = ToColorvalue(material.ColorAmbient);
        if (material.HasColorSpecular) result.Specular = ToColorvalue(material.ColorSpecular);
        if (material.HasColorEmissive) result.Emissive = ToColorvalue(material.ColorEmissive);
        if (material.HasShininess) result.Power = material.Shininess;
        if (material.HasOpacity) result.Diffuse.A *= material.Opacity;

        // Embedded textures (referenced as "*0", "*1", ...) are not supported
        string? texture = material.HasTextureDiffuse ? material.TextureDiffuse.FilePath : null;
        if (string.IsNullOrEmpty(texture) || texture.StartsWith('*')) texture = null;

        return new(result, texture);
    }
    #endregion

    #region Animation
    /// <summary>
    /// Extracts the bones and their influences from an Assimp mesh.
    /// </summary>
    /// <returns>The skinning information; <c>null</c> if the mesh has no bones.</returns>
    private static SkinInfo? ToSkinInfo(AssimpMesh mesh)
    {
        if (!mesh.HasBones) return null;

        var bones = mesh.Bones;
        return new(
            mesh.VertexCount,
            bones.Select(x => x.Name).ToArray(),
            bones.Select(x => ToMatrix(x.OffsetMatrix)).ToArray(),
            bones.SelectMany((bone, index) => bone.VertexWeights.Select(weight => (index, weight.VertexID, weight.Weight))));
    }

    /// <summary>
    /// Converts an Assimp animation, with key times in seconds.
    /// </summary>
    /// <param name="animation">The animation to convert.</param>
    /// <param name="frameIndexes">Maps frame names to their indexes.</param>
    private static AnimationClip ToAnimationClip(Animation animation, Dictionary<string, int> frameIndexes)
    {
        double ticksPerSecond = animation.TicksPerSecond > 0 ? animation.TicksPerSecond : 25;

        AnimationKeys<T> Keys<T>(IEnumerable<(double Time, T Value)> keys) where T : struct
        {
            var array = keys.ToArray();
            return new(array.Select(x => x.Time / ticksPerSecond).ToArray(), array.Select(x => x.Value).ToArray());
        }

        var channels = new List<AnimationChannel>();
        foreach (var channel in animation.NodeAnimationChannels)
        {
            if (!frameIndexes.TryGetValue(channel.NodeName, out int frameIndex)) continue;
            channels.Add(new(
                frameIndex,
                Keys(channel.ScalingKeys.Select(x => (x.Time, x.Value))),
                Keys(channel.RotationKeys.Select(x => (x.Time, x.Value))),
                Keys(channel.PositionKeys.Select(x => (x.Time, x.Value)))));
        }

        return new(animation.Name ?? "", animation.DurationInTicks / ticksPerSecond, channels);
    }
    #endregion
}
