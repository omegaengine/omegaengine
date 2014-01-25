/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.IO;
using Common;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Graphics.VertexDecl;
using Resources = OmegaEngine.Properties.Resources;

#if NETFX4
using System.Threading.Tasks;
#endif

namespace OmegaEngine.Graphics.Renderables
{
    partial class Terrain
    {
        #region Build mesh
        /// <summary>
        /// Creates a new textured model from a vertex and an index array.
        /// The <see cref="PrimitiveType"/> is <see cref="PrimitiveType.TriangleList"/>.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to create the mesh in</param>
        /// <param name="size">The size of the terrain</param>
        /// <param name="stretchH">A factor by which all horizontal distances are multiplied</param>
        /// <param name="stretchV">A factor by which all vertical distances are multiplied</param>
        /// <param name="heightMap">The height values of the terrain in a 2D array.
        ///   Array size = Terrain size</param>
        /// <param name="lightRiseAngleMap">The minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded in a 2D array.
        ///   Array size = Terrain size; may be <see langword="null"/> for no shadowing</param>
        /// <param name="lightSetAngleMap">The maximum vertical angle (0 = 90°, 255 = 180°) which a directional light must not exceed to be not occluded in a 2D array.
        ///   Array size = Terrain size; may be <see langword="null"/> for no shadowing</param>
        /// <param name="textureMap">The texture values of the terrain in a 2D array.
        ///   Array size = Terrain size / 3</param>
        /// <param name="lighting">Shall this mesh be prepared for lighting? (calculate normal vectors, make shaders support lighting, ...)</param>
        /// <param name="blockSize">How many points in X and Y direction shall one block for culling be?</param>
        /// <param name="subsetShaders">Shaders for all subsets the mesh was split into</param>
        /// <param name="subsetBoundingBoxes">Bounding boxes for all subsets the mesh was split into</param>
        /// <returns>The model that was created</returns>
        private static Mesh BuildMesh(Engine engine, Size size, float stretchH, float stretchV, byte[,] heightMap, byte[,] lightRiseAngleMap, byte[,] lightSetAngleMap, byte[,] textureMap, bool lighting, int blockSize, out SurfaceShader[] subsetShaders, out BoundingBox[] subsetBoundingBoxes)
        {
            #region Sanity checks
            if (heightMap.GetLength(0) != size.Width || heightMap.GetLength(1) != size.Height)
                throw new ArgumentException(Resources.WrongHeightMapSize, "heightMap");

            if (lightRiseAngleMap != null && (lightRiseAngleMap.GetLength(0) != size.Width || lightRiseAngleMap.GetLength(1) != size.Height))
                throw new ArgumentException(Resources.WrongAngleMapSize, "lightRiseAngleMap");

            if (lightSetAngleMap != null && (lightSetAngleMap.GetLength(0) != size.Width || lightSetAngleMap.GetLength(1) != size.Height))
                throw new ArgumentException(Resources.WrongAngleMapSize, "lightSetAngleMap");

            if ((textureMap.GetLength(0)) * 3 != size.Width || (textureMap.GetLength(1)) * 3 != size.Height)
                throw new ArgumentException(Resources.WrongTextureMapSize, "textureMap");
            #endregion

            using (new TimedLogEvent("Building terrain mesh"))
            {
                var vertexes = GenerateVertexes(size, stretchH, stretchV, heightMap, lightRiseAngleMap, lightSetAngleMap, textureMap);

                int[] attributes;
                ushort[] subsetTextureMasks;
                var indexes = GenerateIndexes(size, stretchH, stretchV, blockSize, vertexes, out attributes, out subsetTextureMasks, out subsetBoundingBoxes);

                subsetShaders = LoadShaders(engine, lighting, subsetTextureMasks);
                return CompileMesh(engine, vertexes, indexes, attributes, lighting);
            }
        }

        private static PositionMultiTextured[] GenerateVertexes(Size size, float stretchH, float stretchV, byte[,] heightMap, byte[,] lightRiseAngleMap, byte[,] lightSetAngleMap, byte[,] textureMap)
        {
            var vertexes = new PositionMultiTextured[size.Width * size.Height];

            // Only use light angle-maps if both are available
            bool useAngleMaps = (lightRiseAngleMap != null && lightSetAngleMap != null);

#if NETFX4
            Parallel.For(0, size.Width, x =>
#else
            for (int x = 0; x < size.Width; x++)
#endif
            {
                for (int y = 0; y < size.Height; y++)
                {
                    #region Texture blending
                    var texWeights = new float[16];

                    // Perform integer division (texture map has 1/3 of height map accuracy) but keep remainders
                    int xRemainder;
                    int xCoord = Math.DivRem(x, 3, out xRemainder);
                    int yRemainder;
                    int yCoord = Math.DivRem(y, 3, out yRemainder);

                    // Use remainders to determine intermediate blending levels for vertexes
                    switch (xRemainder)
                    {
                        case 0:
                            TextureBlendHelper(textureMap, xCoord, yCoord, texWeights, 0.25f);
                            TextureBlendHelper(textureMap, xCoord - 1, yCoord, texWeights, 0.25f);
                            break;
                        case 1:
                            TextureBlendHelper(textureMap, xCoord, yCoord, texWeights, 0.5f);
                            break;
                        case 2:
                            TextureBlendHelper(textureMap, xCoord, yCoord, texWeights, 0.25f);
                            TextureBlendHelper(textureMap, xCoord + 1, yCoord, texWeights, 0.25f);
                            break;
                    }
                    switch (yRemainder)
                    {
                        case 0:
                            TextureBlendHelper(textureMap, xCoord, yCoord, texWeights, 0.25f);
                            TextureBlendHelper(textureMap, xCoord, yCoord - 1, texWeights, 0.25f);
                            break;
                        case 1:
                            TextureBlendHelper(textureMap, xCoord, yCoord, texWeights, 0.5f);
                            break;
                        case 2:
                            TextureBlendHelper(textureMap, xCoord, yCoord, texWeights, 0.25f);
                            TextureBlendHelper(textureMap, xCoord, yCoord + 1, texWeights, 0.25f);
                            break;
                    }
                    #endregion

                    // Light rise angles: 0 = 0°, 255 = 90°, default = 0°
                    float lightRiseAngle = useAngleMaps ? (float)(lightRiseAngleMap[x, y] / 255.0 * Math.PI / 2) : 0;

                    // Light set angles: 0 = 90°, 255 = 180°, default = 180°
                    float lightSetAngle = useAngleMaps ? (float)((lightSetAngleMap[x, y] / 255.0 + 1) * Math.PI / 2) : (float)Math.PI;

                    // Generate vertex using 2D coords, stretch factors (and tex-coords based on them)
                    // Map X = Engine +X
                    // Map Y = Engine -Z
                    // Map height = Engine +Y
                    vertexes[x + size.Width * y] = new PositionMultiTextured(
                        new Vector3(x * stretchH, heightMap[x, y] * stretchV, -y * stretchH),
                        x * stretchH / 500f, y * stretchH / 500f,
                        lightRiseAngle, lightSetAngle,
                        texWeights, Color.White);
                }
            }
#if NETFX4
            );
#endif
            return vertexes;
        }

        private static int[] GenerateIndexes(Size size, float stretchH, float stretchV, int blockSize, PositionMultiTextured[] vertexes, out int[] attributes, out ushort[] subsetTextureMasks, out BoundingBox[] subsetBoundingBoxes)
        {
            // Calculate number of blocks to divide the indexes into
            int remainder;
            int blocksX = Math.DivRem(size.Width - 1, blockSize, out remainder);
            if (remainder > 0) blocksX++;
            int blocksY = Math.DivRem(size.Height - 1, blockSize, out remainder);
            if (remainder > 0) blocksY++;

            // Create output arrays
            var indexes = new int[(size.Width - 1) * (size.Height - 1) * 6];
            var attributesOut = new int[(size.Width - 1) * (size.Height - 1) * 2];
            var subsetTextureMasksOut = new ushort[blocksX * blocksY];
            var subsetBoundingBoxesOut = new BoundingBox[blocksX * blocksY];

#if NETFX4
            Parallel.For(0, blocksX, xBlock =>
#else
            for (int xBlock = 0; xBlock < blocksX; xBlock++)
#endif
            {
                // Calculate range of block in X direction
                int startX = xBlock * blockSize;
                int endX = startX + blockSize;
                if (endX > size.Width - 1) endX = size.Width - 1;

                for (int yBlock = 0; yBlock < blocksY; yBlock++)
                {
                    // Calculate range of block in Y direction
                    int startY = yBlock * blockSize;
                    int endY = startY + blockSize;
                    if (endY > size.Height - 1) endY = size.Height - 1;
                    int subsetCount = xBlock * blocksY + yBlock;

                    #region Generate indexes
                    byte blockMaxHeight = 0, blockMinHeight = 255;
                    for (int yVert = startY; yVert < endY; yVert++)
                    {
                        for (int xVert = startX; xVert < endX; xVert++)
                        {
                            int vertexCount = xVert + yVert * (size.Width - 1);

                            // Add the vertex numbers to the index array
                            // Keep track of the highest and lowest height value that has occurred in this block/subset so far
                            // Determine which textures are used in this block/subset
                            int indexCount = vertexCount * 6;
                            indexes[indexCount++] = IndexHelper((xVert + 0) + size.Width * (yVert + 0), vertexes, stretchV, ref blockMaxHeight, ref blockMinHeight, ref subsetTextureMasksOut[subsetCount]);
                            indexes[indexCount++] = IndexHelper((xVert + 1) + size.Width * (yVert + 0), vertexes, stretchV, ref blockMaxHeight, ref blockMinHeight, ref subsetTextureMasksOut[subsetCount]);
                            indexes[indexCount++] = IndexHelper((xVert + 0) + size.Width * (yVert + 1), vertexes, stretchV, ref blockMaxHeight, ref blockMinHeight, ref subsetTextureMasksOut[subsetCount]);
                            indexes[indexCount++] = IndexHelper((xVert + 1) + size.Width * (yVert + 0), vertexes, stretchV, ref blockMaxHeight, ref blockMinHeight, ref subsetTextureMasksOut[subsetCount]);
                            indexes[indexCount++] = IndexHelper((xVert + 1) + size.Width * (yVert + 1), vertexes, stretchV, ref blockMaxHeight, ref blockMinHeight, ref subsetTextureMasksOut[subsetCount]);
                            indexes[indexCount] = IndexHelper((xVert + 0) + size.Width * (yVert + 1), vertexes, stretchV, ref blockMaxHeight, ref blockMinHeight, ref subsetTextureMasksOut[subsetCount]);

                            // Assign the current subset number to both faces (triangles) of the current square
                            int faceCount = vertexCount * 2;
                            attributesOut[faceCount++] = subsetCount;
                            attributesOut[faceCount] = subsetCount;
                        }
                    }
                    #endregion

                    // Generate a bounding box using the minimum and maximum heights
                    subsetBoundingBoxesOut[subsetCount] = new BoundingBox(
                        new Vector3(xBlock * blockSize * stretchH, blockMinHeight * stretchV, -yBlock * blockSize * stretchH),
                        new Vector3((xBlock + 1) * blockSize * stretchH, blockMaxHeight * stretchV, -(yBlock + 1) * blockSize * stretchH));
                }
            }
#if NETFX4
            );
#endif

            attributes = attributesOut;
            subsetTextureMasks = subsetTextureMasksOut;
            subsetBoundingBoxes = subsetBoundingBoxesOut;
            return indexes;
        }

        private static SurfaceShader[] LoadShaders(Engine engine, bool lighting, ushort[] textureMasks)
        {
            var shaders = new SurfaceShader[textureMasks.Length];

            // Load the dynamic multitexturing shader if supported
            if (TerrainShader.MinShaderModel <= engine.Capabilities.MaxShaderModel)
            {
#if NETFX4
    // Remove any duplicates and then generate all required shaders in parallel
                var textureMaskSet = new HashSet<ushort>();
                textureMaskSet.AddAll(textureMasks);
                Parallel.ForEach(textureMaskSet, textureMask => engine.GetTerrainShader(lighting, textureMask));
#endif

                for (int i = 0; i < textureMasks.Length; i++)
                    shaders[i] = engine.GetTerrainShader(lighting, textureMasks[i]);
            }

            return shaders;
        }

        private static Mesh CompileMesh(Engine engine, PositionMultiTextured[] vertexes, int[] indexes, int[] attributes, bool lighting)
        {
            var mesh = new Mesh(engine.Device, indexes.Length / 3, vertexes.Length, MeshFlags.Managed | MeshFlags.Use32Bit, PositionMultiTextured.GetVertexElements());
            BufferHelper.WriteVertexBuffer(mesh, vertexes);
            BufferHelper.WriteIndexBuffer(mesh, indexes);

            // Add subset data to mesh
            mesh.LockAttributeBuffer(LockFlags.None).WriteRange(attributes);
            mesh.UnlockAttributeBuffer();

            if (lighting) MeshHelper.GenerateNormals(engine.Device, ref mesh);
            //else MeshHelper.Optimize(mesh);

            return mesh;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Takes a value from a 2D byte array and uses it as an index for a float array. Then adds a numeric value to the float array.
        /// Does nothing if the index is out of bounds.
        /// </summary>
        /// <param name="sourceArray">The byte array to take the index from</param>
        /// <param name="x">The 1st index for the 2D byte array</param>
        /// <param name="y">The 2nd index for the 2D byte array</param>
        /// <param name="targetArray">The float array to add the value to</param>
        /// <param name="addValue">The value to add</param>
        private static void TextureBlendHelper(byte[,] sourceArray, int x, int y, float[] targetArray, float addValue)
        {
            if (x < sourceArray.GetLength(0) && x >= 0 && y < sourceArray.GetLength(1) && y >= 0) // Prevent index overflows
                targetArray[sourceArray[x, y]] += addValue;
        }

        /// <summary>
        /// Looks up an index in a vertex array and extracts the height and texture data from there.
        /// </summary>
        /// <param name="index">The index within <paramref name="array"/> to look up</param>
        /// <param name="array">The vertex array to perform the lookup in</param>
        /// <param name="factor">A factor by which the <paramref name="array"/> elements are to be diveded before handling them</param>
        /// <param name="maxHeight">A height value that is increased if a higher value is found in the lookup</param>
        /// <param name="minHeight">A height value that is decreased if a lower value is found in the lookup</param>
        /// <param name="textureMask">A bitmask that is updated to indicate which textures are used.</param>
        /// <returns>Passed the <paramref name="index"/> back out.</returns>
        private static int IndexHelper(int index, PositionMultiTextured[] array, float factor, ref byte maxHeight, ref byte minHeight, ref ushort textureMask)
        {
            float height = array[index].Position.Y / factor;
            if (height > maxHeight) maxHeight = (byte)height;
            if (height < minHeight) minHeight = (byte)height;

            #region Texture lookup
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (array[index].TexWeights1.X != 0) textureMask |= 1 << 0;
            if (array[index].TexWeights1.Y != 0) textureMask |= 1 << 1;
            if (array[index].TexWeights1.Z != 0) textureMask |= 1 << 2;
            if (array[index].TexWeights1.W != 0) textureMask |= 1 << 3;
            if (array[index].TexWeights2.X != 0) textureMask |= 1 << 4;
            if (array[index].TexWeights2.Y != 0) textureMask |= 1 << 5;
            if (array[index].TexWeights2.Z != 0) textureMask |= 1 << 6;
            if (array[index].TexWeights2.W != 0) textureMask |= 1 << 7;
            if (array[index].TexWeights3.X != 0) textureMask |= 1 << 8;
            if (array[index].TexWeights3.Y != 0) textureMask |= 1 << 9;
            if (array[index].TexWeights3.Z != 0) textureMask |= 1 << 10;
            if (array[index].TexWeights3.W != 0) textureMask |= 1 << 11;
            if (array[index].TexWeights4.X != 0) textureMask |= 1 << 12;
            if (array[index].TexWeights4.Y != 0) textureMask |= 1 << 13;
            if (array[index].TexWeights4.Z != 0) textureMask |= 1 << 14;
            if (array[index].TexWeights4.W != 0) textureMask |= 1 << 15;
            // ReSharper restore CompareOfFloatsByEqualityOperator
            #endregion

            return index;
        }
        #endregion

        //--------------------//

        #region Build material
        /// <summary>
        /// Builds a material with a set of textures.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to load the textures into</param>
        /// <param name="textures">An array with a maximum of 16 texture names</param>
        /// <returns>The material that was created</returns>
        /// <exception cref="FileNotFoundException">Thrown if on of the specified texture files could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading one of the texture files.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to one of the texture files is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if one of the texture files does not contain a valid texture.</exception>
        private static XMaterial BuildMaterial(Engine engine, string[] textures)
        {
            var material = new XMaterial(Color.White) {Specular = Color.Black};
            for (int i = 0; i < textures.Length; i++)
            {
                string id = textures[i];
                material.DiffuseMaps[i] = XTexture.Get(engine, id);
            }
            return material;
        }
        #endregion
    }
}
