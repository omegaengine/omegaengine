/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using LuaInterface;
using SlimDX;

namespace TemplateWorld.Terrains
{
    /// <summary>
    /// A common base for all <see cref="Terrain{TTemplate}"/> types.
    /// </summary>
    public interface ITerrain
    {
        /// <summary>
        /// The size of the terrain.
        /// </summary>
        [Description("The size of the terrain.")]
        TerrainSize Size { get; set; }

        /// <summary>
        /// The world coordinates of the center of the terrain.
        /// </summary>
        [Browsable(false)]
        Vector2 Center { get; }

        /// <summary>
        /// Direct access to the internal height-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the height-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadHeightMap(Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        byte[,] HeightMap { get; set; }

        /// <summary>
        /// Direct access to the internal light rise angle-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <remarks>A light rise angles is the minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the angle-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadLightRiseAngleMap(Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        byte[,] LightRiseAngleMap { get; set; }

        /// <summary>
        /// Direct access to the internal light set angle-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <remarks>A light rise set is the maximum vertical angle (0 = 90°, 255 = 180°) which a directional light must not exceed to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the angle-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadLightSetAngleMap(Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        byte[,] LightSetAngleMap { get; set; }

        /// <summary>
        /// Indicates whether <see cref="LightRiseAngleMap"/> and <see cref="LightSetAngleMap"/> have been calculated yet.
        /// </summary>
        [XmlIgnore]
        bool LightAngleMapsSet { get; }

        /// <summary>
        /// Indicates that the data stored in <see cref="LightRiseAngleMap"/> and <see cref="LightSetAngleMap"/> is outdated and should be recalculated using <see cref="LightAngleMapGenerator"/>.
        /// </summary>
        [XmlAttribute, DefaultValue(false)]
        bool LightAngleMapsOutdated { get; set; }

        /// <summary>
        /// Direct access to the internal texture-map arrayDirect access to the internal height-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the height-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadTextureMap(string)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        byte[,] TextureMap { get; set; }

        /// <summary>
        /// Was the minimum necessary data for the terrain  (<see cref="HeightMap"/> and <see cref="TextureMap"/>) loaded already?
        /// </summary>
        [Browsable(false)]
        bool DataLoaded { get; }

        /// <summary>
        /// Automatically generate <see cref="Terrain{TTemplate}.LightRiseAngleMap"/> and <see cref="Terrain{TTemplate}.LightSetAngleMap"/> based on <see cref="Terrain{TTemplate}.HeightMap"/>.
        /// </summary>
        /// <remarks>This method interally uses a <see cref="LightAngleMapGenerator"/> instance.</remarks>
        void GenerateLightAngleMaps();

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.HeightMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the height-map from.</param>
        /// <exception cref="IOException">Thrown if the height-map size is incorrect.</exception>
        [LuaHide]
        void LoadHeightMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.HeightMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the height-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadHeightMap(string path);

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.LightRiseAngleMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the light rise angle-map from.</param>
        /// <exception cref="IOException">Thrown if the light rise angle-map size is incorrect.</exception>
        [LuaHide]
        void LoadLightRiseAngleMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.LightRiseAngleMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the light rise angle-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadLightRiseAngleMap(string path);

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.LightSetAngleMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the light set angle-map from.</param>
        /// <exception cref="IOException">Thrown if the light set angle-map size is incorrect.</exception>
        [LuaHide]
        void LoadLightSetAngleMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.LightSetAngleMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the light set angle-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadLightSetAngleMap(string path);

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.TextureMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the texture-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        [LuaHide]
        void LoadTextureMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="Terrain{TTemplate}.TextureMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the texture-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadTextureMap(string path);

        /// <summary>
        /// Prepares <see cref="Terrain{TTemplate}.HeightMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the height-map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveHeightMapDelegate();

        /// <summary>
        /// Saves data from <see cref="Terrain{TTemplate}.HeightMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the height-map in.</param>
        void SaveHeightMap(string path);

        /// <summary>
        /// Prepares <see cref="Terrain{TTemplate}.LightRiseAngleMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the angle-map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveLightRiseAngleMapDelegate();

        /// <summary>
        /// Saves data from <see cref="Terrain{TTemplate}.LightRiseAngleMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the angle-map in.</param>
        void SaveLightRiseAngleMap(string path);

        /// <summary>
        /// Prepares <see cref="Terrain{TTemplate}.LightSetAngleMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the angle-map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveLightSetAngleMapDelegate();

        /// <summary>
        /// Saves data from <see cref="Terrain{TTemplate}.LightSetAngleMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the angle-map in.</param>
        void SaveLightSetAngleMap(string path);

        /// <summary>
        /// Prepares <see cref="Terrain{TTemplate}.TextureMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the texture-map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveTextureMapDelegate();

        /// <summary>
        /// Saves data from <see cref="Terrain{TTemplate}.TextureMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the texture-map in.</param>
        void SaveTextureMap(string path);
    }
}
