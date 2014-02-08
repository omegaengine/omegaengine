/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using LuaInterface;
using SlimDX;

namespace AlphaFramework.World.Terrains
{
    /// <summary>
    /// A common base for all <see cref="ITerrain"/> types.
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
        /// Direct access to the internal occlusion end map array. Handle with care; clone when necessary!
        /// </summary>
        /// <remarks>A light rise angles is the minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadOcclusionEndMap(System.IO.Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        byte[,] OcclusionEndMap { get; set; }

        /// <summary>
        /// Direct access to the internal occlusion begin map array. Handle with care; clone when necessary!
        /// </summary>
        /// <remarks>A light rise set is the maximum vertical angle (0 = 90°, 255 = 180°) which a directional light must not exceed to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadOcclusionBeginMap(System.IO.Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        byte[,] OcclusionBeginMap { get; set; }

        /// <summary>
        /// Indicates whether <see cref="OcclusionEndMap"/> and <see cref="OcclusionBeginMap"/> have been calculated yet.
        /// </summary>
        [XmlIgnore]
        bool OcclusionIntervalMapSet { get; }

        /// <summary>
        /// Indicates that the data stored in <see cref="OcclusionEndMap"/> and <see cref="OcclusionBeginMap"/> is outdated and should be recalculated using <see cref="OcclusionIntervalMapGenerator"/>.
        /// </summary>
        [XmlAttribute, DefaultValue(false)]
        bool OcclusionIntervalMapOutdated { get; set; }

        /// <summary>
        /// Direct access to the internal height-map array. Handle with care; clone when necessary!
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
        /// Loads data for <see cref="ITerrain.HeightMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the height-map from.</param>
        /// <exception cref="IOException">Thrown if the height-map size is incorrect.</exception>
        [LuaHide]
        void LoadHeightMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="ITerrain.HeightMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the height-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadHeightMap(string path);

        /// <summary>
        /// Loads data for <see cref="ITerrain.OcclusionEndMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the occlusion end map from.</param>
        /// <exception cref="IOException">Thrown if the occlusion end map size is incorrect.</exception>
        [LuaHide]
        void LoadOcclusionEndMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="ITerrain.OcclusionEndMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the occlusion end map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadOcclusionEndMap(string path);

        /// <summary>
        /// Loads data for <see cref="ITerrain.OcclusionBeginMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the occlusion begin map from.</param>
        /// <exception cref="IOException">Thrown if the occlusion begin map size is incorrect.</exception>
        [LuaHide]
        void LoadOcclusionBeginMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="ITerrain.OcclusionBeginMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the occlusion begin map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadOcclusionBeginMap(string path);

        /// <summary>
        /// Loads data for <see cref="ITerrain.TextureMap"/> from a stream.
        /// </summary>
        /// <param name="stream">The stream to read the texture-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        [LuaHide]
        void LoadTextureMap(Stream stream);

        /// <summary>
        /// Loads data for <see cref="ITerrain.TextureMap"/> from a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to load the texture-map from.</param>
        /// <exception cref="IOException">Thrown if the texture-map size is incorrect.</exception>
        void LoadTextureMap(string path);

        /// <summary>
        /// Prepares <see cref="ITerrain.HeightMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the height-map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveHeightMapDelegate();

        /// <summary>
        /// Saves data from <see cref="ITerrain.HeightMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the height-map in.</param>
        void SaveHeightMap(string path);

        /// <summary>
        /// Prepares <see cref="ITerrain.OcclusionEndMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the occlusion end map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveOcclusionEndMapDelegate();

        /// <summary>
        /// Saves data from <see cref="ITerrain.OcclusionEndMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the angle-map in.</param>
        void SaveOcclusionEndMap(string path);

        /// <summary>
        /// Prepares <see cref="ITerrain.OcclusionBeginMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the occlusion begin map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveOcclusionBeginMapDelegate();

        /// <summary>
        /// Saves data from <see cref="ITerrain.OcclusionBeginMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the angle-map in.</param>
        void SaveOcclusionBeginMap(string path);

        /// <summary>
        /// Prepares <see cref="ITerrain.TextureMap"/> so that it can be stored in a <see cref="Stream"/> later on.
        /// </summary>
        /// <returns>A delegate to be called when the <see cref="Stream"/> for writing the texture-map is ready.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Creates a new delegate on each call")]
        [LuaHide]
        Action<Stream> GetSaveTextureMapDelegate();

        /// <summary>
        /// Saves data from <see cref="ITerrain.TextureMap"/> in a file.
        /// </summary>
        /// <param name="path">The path of the PNG file to store the texture-map in.</param>
        void SaveTextureMap(string path);
    }
}
