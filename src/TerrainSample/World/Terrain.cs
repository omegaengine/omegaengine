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
using Common.Utils;
using SlimDX;
using Resources = World.Properties.Resources;

namespace World
{
    /// <summary>
    /// This class contains a height-map-based Terrain including texturing and pathfinding data.
    /// </summary>
    /// <remarks>
    ///   The positive X-axis points towards east (the direction from which light sources in the sky rise),
    ///   the negative X-axis points towards west (the direction in which light sources in the sky set),
    ///   the positive Y-axis points towards north and
    ///   the negative Y-axis points towards south.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The contained height-map bitmap will automatically be disposed after saving")]
    public sealed partial class Terrain
    {
        #region Properties

        #region Size
        private TerrainSize _size;

        /// <summary>
        /// The size of the terrain.
        /// </summary>
        [Description("The size of the terrain.")]
        public TerrainSize Size
        {
            get { return _size; }
            set
            {
                #region Sanity checks
                int resultX, resultY;
                Math.DivRem(value.X, 3, out resultX);
                Math.DivRem(value.Y, 3, out resultY);
                if (value.X == 0 || value.Y == 0 || resultX != 0 || resultY != 0)
                    throw new InvalidOperationException(Resources.TerrainSizeMultipleOfThree);
                #endregion

                value.To(ref _size, delegate
                { // If the size has changed, the height and texture-maps have become invalid and need to be cleared
                    HeightMap = null;
                    TextureMap = null;
                });
            }
        }

        /// <summary>
        /// The world coordinates of the center of the <see cref="Terrain"/>.
        /// </summary>
        [Browsable(false)]
        public Vector2 Center { get { return new Vector2(_size.X * _size.StretchH / 2f, _size.Y * _size.StretchH / 2f); } }
        #endregion

        #region Terrain templates
        /// <summary>
        /// The <see cref="TerrainTemplate"/>s available for usage in this <see cref="Terrain"/>.
        /// </summary>
        [XmlIgnore]
        public readonly TerrainTemplate[] Templates = new TerrainTemplate[16];

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Templates"/>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Used for XML serialization")]
        [XmlElement("Template"), Browsable(false)]
        public string[] TemplateNames
        {
            get
            {
                var templateNames = new string[Templates.Length];
                for (int i = 0; i < Templates.Length; i++)
                    templateNames[i] = (Templates[i] == null) ? "" : Templates[i].Name;
                return templateNames;
            }
            set
            {
                for (int i = 0; i < Templates.Length; i++)
                {
                    Templates[i] = (value != null && i < value.Length && !string.IsNullOrEmpty(value[i]))
                        ? TemplateManager.GetTerrainTemplate(value[i])
                        : null;
                }
            }
        }
        #endregion

        #region Height-map
        private byte[,] _heightMap;

        /// <summary>
        /// Direct access to the internal height-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the height-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadHeightMap(Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] HeightMap
        {
            get { return _heightMap; }
            set
            {
                #region Sanity checks
                if (value != null)
                {
                    if (value.GetUpperBound(0) + 1 != _size.X || value.GetUpperBound(1) + 1 != _size.Y)
                        throw new InvalidOperationException(Resources.HeightMapSizeEqualTerrain);
                }
                #endregion

                _heightMap = value;
            }
        }
        #endregion

        #region Light angle-map
        private byte[,] _lightRiseAngleMap;

        /// <summary>
        /// Direct access to the internal light rise angle-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <remarks>A light rise angles is the minimum vertical angle (0 = 0°, 255 = 90°) which a directional light must achieve to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the angle-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadLightRiseAngleMap(Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] LightRiseAngleMap
        {
            get { return _lightRiseAngleMap; }
            set
            {
                #region Sanity checks
                if (value != null)
                {
                    if (value.GetUpperBound(0) + 1 != _size.X || value.GetUpperBound(1) + 1 != _size.Y)
                        throw new InvalidOperationException(Resources.HeightMapSizeEqualTerrain);
                }
                #endregion

                _lightRiseAngleMap = value;
            }
        }

        private byte[,] _lightSetAngleMap;

        /// <summary>
        /// Direct access to the internal light set angle-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <remarks>A light rise set is the maximum vertical angle (0 = 90°, 255 = 180°) which a directional light must not exceed to be not occluded.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the angle-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadLightSetAngleMap(Stream)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] LightSetAngleMap
        {
            get { return _lightSetAngleMap; }
            set
            {
                #region Sanity checks
                if (value != null)
                {
                    if (value.GetUpperBound(0) + 1 != _size.X || value.GetUpperBound(1) + 1 != _size.Y)
                        throw new InvalidOperationException(Resources.HeightMapSizeEqualTerrain);
                }
                #endregion

                _lightSetAngleMap = value;
            }
        }

        /// <summary>
        /// Indicates that the data stored in <see cref="LightRiseAngleMap"/> and <see cref="LightSetAngleMap"/> is outdated and should be recalculated using <see cref="LightAngleMapGenerator"/>.
        /// </summary>
        [XmlAttribute, DefaultValue(false)]
        public bool LightAngleMapsOutdated { get; set; }
        #endregion

        #region Texture-map
        private byte[,] _textureMap;

        /// <summary>
        /// Direct access to the internal texture-map arrayDirect access to the internal height-map array. Handle with care; clone when necessary!
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the height-map size is incorrect.</exception>
        /// <remarks>Is not serialized/stored, is loaded by <see cref="LoadTextureMap(string)"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] TextureMap
        {
            get { return _textureMap; }
            set
            {
                #region Sanity checks
                if (value != null)
                {
                    if (value.GetUpperBound(0) + 1 != _size.X / 3 || value.GetUpperBound(1) + 1 != _size.Y / 3)
                        throw new InvalidOperationException(Resources.TextureMapSizeThirdOfTerrain);
                }
                #endregion

                _textureMap = value;
            }
        }
        #endregion

        /// <summary>
        /// Was the minimum necessary data for the <see cref="Terrain"/>  (<see cref="HeightMap"/> and <see cref="TextureMap"/>) loaded already?
        /// </summary>
        [Browsable(false)]
        internal bool DataLoaded { get { return _heightMap != null && _textureMap != null; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public Terrain()
        {}

        /// <summary>
        /// Creates a new <see cref="Terrain"/>.  It is completely flat and has only one texture initially.
        /// </summary>
        /// <param name="size">The size of the <see cref="Terrain"/> to create.</param>
        public Terrain(TerrainSize size)
        {
            Size = size;
            _heightMap = new byte[size.X,size.Y];
            _textureMap = new byte[size.X / 3,size.Y / 3];

            // Try to use "Grass" as the default Terrain type
            try
            {
                Templates[0] = TemplateManager.GetTerrainTemplate("Grass");
            }
            catch (InvalidOperationException)
            {}
        }
        #endregion

        //--------------------//

        #region Terrain class
        /// <summary>
        /// Determines the <see cref="TerrainTemplate"/> effective at specific coordinates.
        /// </summary>
        /// <param name="coordinates">The world coordinates to check.</param>
        public byte GetTerrainIndex(Vector2 coordinates)
        {
            return _textureMap[
                (int)(coordinates.X / _size.StretchV),
                (int)(coordinates.Y / _size.StretchV)];
        }
        #endregion

        #region Light angle maps
        /// <summary>
        /// Automatically generate <see cref="LightRiseAngleMap"/> and <see cref="LightSetAngleMap"/> based on <see cref="HeightMap"/>.
        /// </summary>
        /// <remarks>This method interally uses a <see cref="LightAngleMapGenerator"/> instance.</remarks>
        public void GenerateLightAngleMaps()
        {
            #region Sanity checks
            if (!DataLoaded) throw new InvalidOperationException(Resources.TerrainDataNotLoaded);
            #endregion

            var generator = new LightAngleMapGenerator(Size, HeightMap);
            generator.RunSync();

            // Replace the old angle-maps
            _lightRiseAngleMap = generator.LightRiseAngleMap;
            _lightSetAngleMap = generator.LightSetAngleMap;
        }
        #endregion
    }
}
