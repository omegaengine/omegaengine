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
using System.Xml.Serialization;
using Common;
using Common.Collections;
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;
using LuaInterface;
using SlimDX;
using TerrainSample.World.Pathfinding;
using TerrainSample.World.Positionables;
using TerrainSample.World.Terrains;
using Resources = TerrainSample.World.Properties.Resources;

namespace TerrainSample.World
{
    /// <summary>
    /// Represents a world with a height-map based <see cref="Terrain"/>.
    /// </summary>
    public sealed partial class TerrainUniverse : Universe<Vector2>
    {
        #region Properties
        private readonly MonitoredCollection<Positionable<Vector2>> _positionables = new MonitoredCollection<Positionable<Vector2>>();

        /// <inheritoc/>
        [Browsable(false)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        [XmlElement(typeof(TerrainEntity)), XmlElement(typeof(Water)), XmlElement(typeof(Waypoint<Vector2>)), XmlElement(typeof(BenchmarkPoint<Vector2>)), XmlElement(typeof(Memo<Vector2>))]
        public override MonitoredCollection<Positionable<Vector2>> Positionables { get { return _positionables; } }

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
                if (_terrain != null && SourceFile != null && !_terrain.DataLoaded) LoadTerrainData();
                return _terrain;
            }
        }

        /// <summary>Used for XML serialization.</summary>
        [XmlElement("Terrain"), LuaHide, Browsable(false)]
        public Terrain TerrainSerialize { get { return _terrain; } set { _terrain = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public TerrainUniverse()
        {
            LightPhaseSpeedFactor = 1;
        }

        /// <summary>
        /// Creates a new <see cref="TerrainUniverse"/> with a terrain.
        /// </summary>
        /// <param name="terrain">The terrain for the new <see cref="TerrainUniverse"/>.</param>
        public TerrainUniverse(Terrain terrain)
        {
            _terrain = terrain;
        }
        #endregion

        //--------------------//

        #region Update
        /// <inheritdoc/>
        public override void Update(double elapsedTime)
        {
            LightPhase += (float)(elapsedTime / 40 * LightPhaseSpeedFactor);

            base.Update(elapsedTime);
        }
        #endregion

        #region Path finding
        /// <inheritdoc/>
        public override void MoveEntity(Entity<Vector2> entity, Vector2 target)
        {
            #region Sanity checks
            if (entity == null) throw new ArgumentNullException("entity");
            #endregion

            // Get path and cancel if none was found
            var pathNodes = Terrain.Pathfinder.FindPathPlayer(GetScaledPosition(entity.Position), GetScaledPosition(target));
            if (pathNodes == null)
            {
                entity.PathControl = null;
                return;
            }

            // Store path data in entity
            var pathLeader = new PathLeader<Vector2> {ID = 0, Target = target};
            foreach (var node in pathNodes)
                pathLeader.PathNodes.Push(node * Terrain.Size.StretchH);
            entity.PathControl = pathLeader;
        }

        /// <summary>
        /// Applies <see cref="TerrainSize"/> scaling to a position.
        /// </summary>
        private Vector2 GetScaledPosition(Vector2 position)
        {
            return position * (1.0f / Terrain.Size.StretchH);
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="TerrainUniverse"/> from the game content source via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="TerrainUniverse"/> to load.</param>
        /// <returns>The loaded <see cref="TerrainUniverse"/>.</returns>
        public static TerrainUniverse FromContent(string id)
        {
            Log.Info("Loading map: " + id);

            TerrainUniverse universe;
            using (var stream = ContentManager.GetFileStream("World/Maps", id))
                universe = XmlStorage.LoadXmlZip<TerrainUniverse>(stream);
            universe.SourceFile = id;
            universe.Update(0);

            return universe;
        }

        /// <summary>
        /// Loads a <see cref="TerrainUniverse"/> from a compressed XML file (map file).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="TerrainUniverse"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurred while deserializing the XML data.</exception>
        public static TerrainUniverse Load(string path)
        {
            // Load the core data but without terrain data yet (that is delay-loaded)
            TerrainUniverse universe;
            try
            {
                universe = XmlStorage.LoadXmlZip<TerrainUniverse>(path);
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
        /// Performs the deferred loading of <see cref="Terrain"/> data.
        /// </summary>
        private void LoadTerrainData()
        {
            // Load the data
            using (var stream = ContentManager.GetFileStream("World/Maps", SourceFile))
            {
                XmlStorage.LoadXmlZip<TerrainUniverse>(stream, additionalFiles: new[]
                {
                    // Callbacks for loading terrain data
                    new EmbeddedFile("height.png", _terrain.LoadHeightMap),
                    new EmbeddedFile("texture.png", _terrain.LoadTextureMap),
                    new EmbeddedFile("lightRiseAngle.png", _terrain.LoadLightRiseAngleMap),
                    new EmbeddedFile("lightSetAngle.png", _terrain.LoadLightSetAngleMap)
                });
            }

            if (!_terrain.DataLoaded) throw new InvalidOperationException(Resources.TerrainDataNotLoaded);

            using (new TimedLogEvent("Setup pathfinding"))
                _terrain.SetupPathfinding(Positionables);
        }

        /// <inheritdoc/>
        public override void Save(string path)
        {
            using (Entity<Vector2>.MaskTemplateData())
            {
                if (_terrain.LightAngleMapsSet)
                {
                    this.SaveXmlZip(path, additionalFiles: new[]
                    {
                        new EmbeddedFile("height.png", 0, Terrain.GetSaveHeightMapDelegate()),
                        new EmbeddedFile("texture.png", 0, Terrain.GetSaveTextureMapDelegate()),
                        new EmbeddedFile("lightRiseAngle.png", 0, Terrain.GetSaveLightRiseAngleMapDelegate()),
                        new EmbeddedFile("lightSetAngle.png", 0, Terrain.GetSaveLightSetAngleMapDelegate())
                    });
                }
                else
                {
                    this.SaveXmlZip(path, additionalFiles: new[]
                    {
                        new EmbeddedFile("height.png", 0, Terrain.GetSaveHeightMapDelegate()),
                        new EmbeddedFile("texture.png", 0, Terrain.GetSaveTextureMapDelegate())
                    });
                }
            }

            SourceFile = path;
        }
        #endregion
    }
}
