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
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using AlphaFramework.World.Properties;
using AlphaFramework.World.Terrains;
using FrameOfReference.World.Config;
using FrameOfReference.World.Positionables;
using FrameOfReference.World.Templates;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Storage.SlimDX;
using EmbeddedFile = NanoByte.Common.Storage.EmbeddedFile;

namespace FrameOfReference.World
{
    partial class Universe
    {
        /// <summary>
        /// The file extensions when this class is stored as a file.
        /// </summary>
        public const string FileExt = "." + GeneralSettings.AppNameShort + "Map";

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
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurred while deserializing the XML data.</exception>
        public static Universe Load(string path)
        {
            // Load the core data but without terrain data yet (that is delay-loaded)
            var universe = XmlStorage.LoadXmlZip<Universe>(path);
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
            Log.Info("Loading map: " + id);

            using (var stream = ContentManager.GetFileStream("World/Maps", id))
            {
                var universe = XmlStorage.LoadXmlZip<Universe>(stream);
                universe.SourceFile = id;
                return universe;
            }
        }

        /// <summary>Used for XML serialization.</summary>
        [XmlElement("Terrain"), LuaHide, Browsable(false)]
        public Terrain<TerrainTemplate> TerrainSerialize { get { return _terrain; } set { _terrain = value; } }

        /// <summary>
        /// Performs the deferred loading of <see cref="Terrain"/> data.
        /// </summary>
        private void LoadTerrainData()
        {
            // Load the data
            using (var stream = ContentManager.GetFileStream("World/Maps", SourceFile))
            {
                XmlStorage.LoadXmlZip<Universe>(stream, additionalFiles: new[]
                {
                    // Callbacks for loading terrain data
                    new EmbeddedFile("height.png", _terrain.LoadHeightMap),
                    new EmbeddedFile("texture.png", _terrain.LoadTextureMap),
                    new EmbeddedFile("occlusion.png", _terrain.LoadOcclusionIntervalMap)
                });
            }

            if (!_terrain.DataLoaded) throw new InvalidOperationException(Resources.TerrainDataNotLoaded);

            using (new TimedLogEvent("Initialize pathfinding"))
                InitializePathfinding();
        }

        /// <inheritdoc/>
        public override void Save(string path)
        {
            UnwrapWaypoints();
            using (Entity.MaskTemplateData())
            {
                if (Terrain.OcclusionIntervalMap == null)
                {
                    this.SaveXmlZip(path, additionalFiles: new[]
                    {
                        new EmbeddedFile("height.png", 0, Terrain.HeightMap.Save),
                        new EmbeddedFile("texture.png", 0, Terrain.TextureMap.Save)
                    });
                }
                else
                {
                    this.SaveXmlZip(path, additionalFiles: new[]
                    {
                        new EmbeddedFile("height.png", 0, Terrain.HeightMap.Save),
                        new EmbeddedFile("texture.png", 0, Terrain.TextureMap.Save),
                        new EmbeddedFile("occlusion.png", 0, Terrain.OcclusionIntervalMap.Save)
                    });
                }
            }

            SourceFile = path;
        }
    }
}
