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
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace AlphaFramework.World.Terrains;

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
    /// <exception cref="InvalidOperationException">The height-map size is incorrect.</exception>
    /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadHeightMap(Stream)"/>.</remarks>
    [XmlIgnore, Browsable(false)]
    ByteGrid? HeightMap { get; set; }

    /// <summary>
    /// Direct access to the internal occlusion interval map array. Handle with care; clone when necessary!
    /// </summary>
    /// <exception cref="InvalidOperationException">The size is incorrect.</exception>
    /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadOcclusionIntervalMap(System.IO.Stream)"/>.</remarks>
    [XmlIgnore, Browsable(false)]
    ByteVector4Grid? OcclusionIntervalMap { get; set; }

    /// <summary>
    /// Indicates that the data stored in <see cref="OcclusionIntervalMap"/> is outdated and should be recalculated using <see cref="OcclusionIntervalMapGenerator"/>.
    /// </summary>
    [XmlAttribute, DefaultValue(false)]
    bool OcclusionIntervalMapOutdated { get; set; }

    /// <summary>
    /// Direct access to the internal height-map array. Handle with care; clone when necessary!
    /// </summary>
    /// <exception cref="InvalidOperationException">The height-map size is incorrect.</exception>
    /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadTextureMap(string)"/>.</remarks>
    [XmlIgnore, Browsable(false)]
    NibbleGrid? TextureMap { get; set; }

    /// <summary>
    /// Was the minimum necessary data for the terrain  (<see cref="HeightMap"/> and <see cref="TextureMap"/>) loaded already?
    /// </summary>
    [Browsable(false), XmlIgnore]
    [MemberNotNullWhen(true, nameof(HeightMap))]
    [MemberNotNullWhen(true, nameof(TextureMap))]
    bool DataLoaded { get; }

    /// <summary>
    /// Marks untraversable slopes in a pathfinding "obstruction map".
    /// </summary>
    /// <param name="obstructionMap">The existing pathfinding "obstruction map" to mark the untraversable slopes in.</param>
    /// <param name="maxTraversableSlope">The maximum slope to consider traversable.</param>
    [LuaHide]
    void MarkUntraversableSlopes(bool[,] obstructionMap, int maxTraversableSlope);

    /// <summary>
    /// Loads data for <see cref="ITerrain.HeightMap"/> from a stream.
    /// </summary>
    /// <param name="stream">The stream to read the height-map from.</param>
    /// <exception cref="IOException">The height-map size is incorrect.</exception>
    [LuaHide]
    void LoadHeightMap(Stream stream);

    /// <summary>
    /// Loads data for <see cref="ITerrain.HeightMap"/> from a file.
    /// </summary>
    /// <param name="path">The path of the PNG file to load the height-map from.</param>
    /// <exception cref="IOException">The texture-map size is incorrect.</exception>
    void LoadHeightMap(string path);

    /// <summary>
    /// Loads data for <see cref="ITerrain.OcclusionIntervalMap"/> from a stream.
    /// </summary>
    /// <param name="stream">The stream to read the occlusion interval map from.</param>
    /// <exception cref="IOException">The occlusion interval map size is incorrect.</exception>
    [LuaHide]
    void LoadOcclusionIntervalMap(Stream stream);

    /// <summary>
    /// Loads data for <see cref="ITerrain.OcclusionIntervalMap"/> from a file.
    /// </summary>
    /// <param name="path">The path of the PNG file to load the occlusion interval map from.</param>
    /// <exception cref="IOException">The texture-map size is incorrect.</exception>
    void LoadOcclusionIntervalMap(string path);

    /// <summary>
    /// Loads data for <see cref="ITerrain.TextureMap"/> from a stream.
    /// </summary>
    /// <param name="stream">The stream to read the texture-map from.</param>
    /// <exception cref="IOException">The texture-map size is incorrect.</exception>
    [LuaHide]
    void LoadTextureMap(Stream stream);

    /// <summary>
    /// Loads data for <see cref="ITerrain.TextureMap"/> from a file.
    /// </summary>
    /// <param name="path">The path of the PNG file to load the texture-map from.</param>
    /// <exception cref="IOException">The texture-map size is incorrect.</exception>
    void LoadTextureMap(string path);
}
