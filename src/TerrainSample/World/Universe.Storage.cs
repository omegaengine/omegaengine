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
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Common;
using Common.Storage;
using Core;
using ICSharpCode.SharpZipLib.Zip;
using LuaInterface;
using SlimDX;
using World.Positionables;
using World.Terrains;
using Resources = World.Properties.Resources;

namespace World
{
    partial class Universe
    {
        #region Constants
        /// <summary>
        /// The file extensions when this class is stored as a file.
        /// </summary>
        public const string FileExt = "." + GeneralSettings.AppNameShort + "Map";
        #endregion

        #region Variables
        /// <summary>
        /// Don't save or load the <see cref="Entity{TCoordinates}.TemplateData"/> in map files - that's only sensible in savegames.
        /// Instead <see cref="Entity{TCoordinates}.TemplateName"/> is used.
        /// </summary>
        private static readonly MemberInfo _ignoreMemeber = typeof(Entity<Vector2>).GetProperty("TemplateData");
        #endregion

        #region Properties
        /// <summary>
        /// The map file this world was loaded from.
        /// </summary>
        /// <remarks>Is not serialized/stored, is set by <see cref="FromContent"/>, <see cref="Session.Load"/> or whatever method loads <see cref="Universe"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        public string SourceFile { get; set; }

        private Terrain _terrain;

        /// <summary>
        /// The <see cref="Terrain"/> on which <see cref="Entity{TCoordinates}"/>s are placed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="Terrain"/> could not be properly loaded from the file.</exception>
        /// <remarks>Is not serialized/stored, <see cref="TerrainSerialize"/> is used for that.</remarks>
        [Browsable(false)]
        public Terrain Terrain
        {
            get
            {
                if (_terrain != null && SourceFile != null && !_terrain.DataLoaded)
                {
                    #region Delay-load terrain
                    // Setup callbacks for loading terrain data
                    var additionalFiles = new[]
                    {
                        new EmbeddedFile("height.png", _terrain.LoadHeightMap),
                        new EmbeddedFile("texture.png", _terrain.LoadTextureMap),
                        new EmbeddedFile("lightRiseAngle.png", _terrain.LoadLightRiseAngleMap),
                        new EmbeddedFile("lightSetAngle.png", _terrain.LoadLightSetAngleMap)
                    };

                    // Load the data
                    using (var stream = ContentManager.GetFileStream("World/Maps", SourceFile))
                        XmlStorage.LoadXmlZip<Universe>(stream, null, additionalFiles, _ignoreMemeber);

                    if (!_terrain.DataLoaded) throw new InvalidOperationException(Resources.TerrainDataNotLoaded);

                    using (new TimedLogEvent("Setup pathfinding"))
                        _terrain.SetupPathfinding(Positionables);
                    #endregion
                }
                return _terrain;
            }
        }

        /// <summary>Used for XML serialization.</summary>
        [XmlElement("Terrain"), LuaHide, Browsable(false)]
        public Terrain TerrainSerialize { get { return _terrain; } set { _terrain = value; } }
        #endregion

        //--------------------//

        #region Content
        /// <summary>
        /// Loads a <see cref="Universe"/> from the game content source via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="Universe"/> to load.</param>
        /// <returns>The loaded <see cref="Universe"/>.</returns>
        public static Universe FromContent(string id)
        {
            Log.Info("Loading map: " + id);

            Universe universe;
            using (var stream = ContentManager.GetFileStream("World/Maps", id))
                universe = XmlStorage.LoadXmlZip<Universe>(stream, null, null, _ignoreMemeber);
            universe.SourceFile = id;
            universe.Update(0);

            return universe;
        }
        #endregion

        #region Load
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
            Universe universe;
            try
            {
                universe = XmlStorage.LoadXmlZip<Universe>(path, null, null, _ignoreMemeber);
            }
                #region Error handling
            catch (ZipException ex)
            {
                throw new IOException(ex.Message, ex);
            }
            #endregion

            // Store the orginal map filename
            universe.SourceFile = path;

            // Perform updates to initialize basic data
            universe.Update(0);

            return universe;
        }

        /// <summary>
        /// Loads a <see cref="Universe"/> from an uncompressed XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Universe"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurred while deserializing the XML data.</exception>
        public static Universe LoadXml(string path)
        {
            return XmlStorage.LoadXml<Universe>(path, _ignoreMemeber);
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves this <see cref="Universe"/> in a compressed XML file (map file).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            #region Sanity checks
            if (_terrain == null) throw new InvalidOperationException("No terrain data!");
            #endregion

            // Encode the terrain data and setup callbacks to save it
            EmbeddedFile[] additionalFiles;
            if (_terrain.LightRiseAngleMap != null && _terrain.LightSetAngleMap != null)
            {
                // Only attempt to store light angle-maps if they actually exists
                additionalFiles = new[]
                {
                    new EmbeddedFile("height.png", 0, Terrain.GetSaveHeightMapDelegate()),
                    new EmbeddedFile("texture.png", 0, Terrain.GetSaveTextureMapDelegate()),
                    new EmbeddedFile("lightRiseAngle.png", 0, Terrain.GetSaveLightRiseAngleMapDelegate()),
                    new EmbeddedFile("lightSetAngle.png", 0, Terrain.GetSaveLightSetAngleMapDelegate())
                };
            }
            else
            {
                additionalFiles = new[]
                {
                    new EmbeddedFile("height.png", 0, Terrain.GetSaveHeightMapDelegate()),
                    new EmbeddedFile("texture.png", 0, Terrain.GetSaveTextureMapDelegate())
                };
            }

            // Save the data
            this.SaveXmlZip(path, null, additionalFiles, _ignoreMemeber);

            SourceFile = path;
        }

        /// <summary>
        /// Overwrites the map file this <see cref="Universe"/> was loaded from with the changed data.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save()
        {
            // Determine the original filename to overweite
            Save(Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile));
        }

        /// <summary>
        /// Saves this <see cref="Universe"/> in an uncompressed XML file.
        /// </summary>
        /// <param name="path">The file to save in</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void SaveXml(string path)
        {
            this.SaveXml(path, _ignoreMemeber);
        }
        #endregion
    }
}
