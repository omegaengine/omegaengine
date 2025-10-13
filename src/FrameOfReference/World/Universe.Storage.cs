/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AlphaFramework.World.Properties;
using AlphaFramework.World.Terrains;
using FrameOfReference.World.Positionables;
using FrameOfReference.World.Templates;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine.Foundation.Storage;

namespace FrameOfReference.World;

partial class Universe
{
    /// <summary>
    /// The file extensions when this class is stored as a file.
    /// </summary>
    public const string FileExt = $".{Constants.AppNameShort}Map";

    /// <summary>
    /// Base-constructor for XML serialization. Do not call manually!
    /// </summary>
    public Universe()
    {}

    /// <summary>
    /// Loads a <see cref="Universe"/> from a compressed XML file (map file).
    /// </summary>
    /// <param name="path">The file to load from.</param>
    /// <returns>The loaded <see cref="Universe"/>.</returns>
    /// <exception cref="IOException">A problem occurred while reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidOperationException">A problem occurred while deserializing the XML data.</exception>
    public static Universe Load(string path)
    {
        // Load the core data but without terrain data yet (that is delay-loaded)
        var universe = XmlZipStorage.LoadXmlZip<Universe>(path);
        universe.SourceFile = path;
        return universe;
    }

    /// <summary>
    /// Loads a <see cref="Universe"/> from the game content source via the <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="id">The ID of the <see cref="Universe"/> to load.</param>
    /// <returns>The loaded <see cref="Universe"/>.</returns>
    public static Universe FromContent(string id)
    {
        Log.Info($"Loading map: {id}");

        using var stream = ContentManager.GetFileStream("World/Maps", id);
        var universe = XmlZipStorage.LoadXmlZip<Universe>(stream);
        universe.SourceFile = id;
        return universe;
    }

    /// <summary>Used for XML serialization.</summary>
    [XmlElement("Terrain"), LuaHide, Browsable(false)]
    public Terrain<TerrainTemplate> TerrainSerialize { get; set; } = null!;

    /// <summary>
    /// Performs the deferred loading of <see cref="Terrain"/> data.
    /// </summary>
    private void LoadTerrainData()
    {
        if (SourceFile == null) return;

        // Load the data
        using (var stream = ContentManager.GetFileStream("World/Maps", SourceFile))
        {
            XmlZipStorage.LoadXmlZip<Universe>(stream, additionalFiles:
            [
                // Callbacks for loading terrain data
                new("height.png", TerrainSerialize.LoadHeightMap),
                new("texture.png", TerrainSerialize.LoadTextureMap),
                new("occlusion.png", TerrainSerialize.LoadOcclusionIntervalMap)
            ]);
        }

        if (!TerrainSerialize.DataLoaded) throw new InvalidOperationException(Resources.TerrainDataNotLoaded);

        InitializePathfinding();
    }

    /// <inheritdoc/>
    public override void Save(string path)
    {
        UnwrapWaypoints();
        using (Entity.MaskTemplateData())
            this.SaveXmlZip(path, additionalFiles: GetEmbeddedFiles().ToArray());

        SourceFile = path;

        IEnumerable<EmbeddedFile> GetEmbeddedFiles()
        {
            if (Terrain.HeightMap is {} heightMap)
                yield return new("height.png", 0, heightMap.Save);
            if (Terrain.TextureMap is {} textureMap)
                yield return new("texture.png", 0, textureMap.Save);
            if (Terrain.OcclusionIntervalMap is {} occlusionIntervalMap)
                yield return new("occlusion.png", 0, occlusionIntervalMap.Save);
        }
    }
}
