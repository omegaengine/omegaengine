/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Templates;
using Common.Utils;
using Common.Values;
using LuaInterface;
using SlimDX;
using Resources = AlphaFramework.World.Properties.Resources;

namespace AlphaFramework.World.Terrains
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
    /// <typeparam name="TTemplate">The specific type of <see cref="Template{T}"/> to use for storing information about terrain types.</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The contained height-map bitmap will automatically be disposed after saving")]
    public sealed partial class Terrain<TTemplate> : ITerrain
        where TTemplate : Template<TTemplate>
    {
        #region Properties

        #region Size
        private TerrainSize _size;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        [Browsable(false)]
        public Vector2 Center { get { return new Vector2(_size.X * _size.StretchH / 2f, _size.Y * _size.StretchH / 2f); } }
        #endregion

        #region Terrain templates
        /// <summary>
        /// The <typeparamref name="TTemplate"/>s available for usage in this <see cref="ITerrain"/>.
        /// </summary>
        [XmlIgnore]
        public readonly TTemplate[] Templates = new TTemplate[16];

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
                        ? Template<TTemplate>.All[value[i]]
                        : null;
                }
            }
        }
        #endregion

        #region Height-map
        private byte[,] _heightMap;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        [XmlIgnore]
        public bool LightAngleMapsSet { get { return LightRiseAngleMap != null && LightSetAngleMap != null; } }

        /// <inheritdoc/>
        [XmlAttribute, DefaultValue(false)]
        public bool LightAngleMapsOutdated { get; set; }
        #endregion

        #region Texture-map
        private byte[,] _textureMap;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        [Browsable(false)]
        public bool DataLoaded { get { return _heightMap != null && _textureMap != null; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public Terrain()
        {}

        /// <summary>
        /// Creates a new terrain. It is completely flat and has only one texture initially.
        /// </summary>
        /// <param name="size">The size of the terrain to create.</param>
        public Terrain(TerrainSize size)
        {
            Size = size;
            _heightMap = new byte[size.X, size.Y];
            _textureMap = new byte[size.X / 3, size.Y / 3];

            // Try to use "Grass" as the default Terrain type
            try
            {
                Templates[0] = Template<TTemplate>.All["Grass"];
            }
            catch (KeyNotFoundException)
            {}
        }
        #endregion

        //--------------------//

        #region Coordinates
        /// <summary>
        /// Converts a position in world coordinates to the engine entity space coordinate system.
        /// </summary>
        /// <param name="coordinates">The coordinates of the point in engine world space to get information for.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates lie outside the range of the <see cref="Terrain{TTemplate}"/>.</exception>
        public DoubleVector3 ToEngineCoords(Vector2 coordinates)
        {
            // Note: This is only required for lookups in the height-map, not for actually unstretching the coordinates to be returned
            Vector2 unstrechedCoords = coordinates * (1 / _size.StretchH);

            if (unstrechedCoords.X > HeightMap.GetLength(0) - 1 || unstrechedCoords.Y > HeightMap.GetLength(1) - 1 ||
                unstrechedCoords.X < 0 || unstrechedCoords.Y < 0)
                throw new ArgumentOutOfRangeException("coordinates", Resources.CoordinatesNotInRange);

            // Snap X values to bounding whole values
            var xPos0 = (int)Math.Floor(unstrechedCoords.X);
            var xPos1 = (int)Math.Ceiling(unstrechedCoords.X);
            float xPosDiff = unstrechedCoords.X - xPos0;

            // Snap Y values to bounding whole values
            var yPos0 = (int)Math.Floor(unstrechedCoords.Y);
            var yPos1 = (int)Math.Ceiling(unstrechedCoords.Y);
            float yPosDiff = unstrechedCoords.Y - yPos0;

            float height, xHeightDiff, yHeightDiff;
            // Determine one which of the two possible triangles of the map square the point is located
            if (xPosDiff + yPosDiff < 1)
            { // Top left triangle
                xHeightDiff = HeightMap[xPos1, yPos0] - HeightMap[xPos0, yPos0];
                yHeightDiff = HeightMap[xPos0, yPos1] - HeightMap[xPos0, yPos0];
                height = HeightMap[xPos0, yPos0] +
                         ((xHeightDiff * xPosDiff) + (yHeightDiff * yPosDiff));
            }
            else
            { // Bottom right triangle
                xHeightDiff = HeightMap[xPos1, yPos1] - HeightMap[xPos0, yPos1];
                yHeightDiff = HeightMap[xPos1, yPos1] - HeightMap[xPos1, yPos0];
                height = HeightMap[xPos1, yPos1] -
                         ((xHeightDiff * (1 - xPosDiff)) + (yHeightDiff * (1 - yPosDiff)));
            }

            return new DoubleVector3(
                coordinates.X, // World X = Engine +X
                height * _size.StretchV, // World height = Engine +Y
                -coordinates.Y); // World Y = Engine -Z
        }
        #endregion

        #region Pathfinding
        /// <summary>
        /// The pathfinding engine used on this terrain.
        /// Is <see langword="null"/> until <see cref="SetupPathfinding"/> has been called.
        /// </summary>
        [XmlIgnore]
        public IPathfinder<Vector2> Pathfinder { get; private set; }

        /// <summary>
        /// Initializes the pathfinding engine.
        /// </summary>
        /// <param name="entities">The <see cref="Positionable{TCoordinates}"/>s to consider for obstacles.</param>
        /// <remarks>Is automatically called on first access to the terrain.</remarks>
        [LuaHide]
        public void SetupPathfinding(IEnumerable<Positionable<Vector2>> entities)
        {
            #region Sanity checks
            if (entities == null) throw new ArgumentNullException("entities");
            #endregion

            var blockedMap = new bool[_size.X, _size.Y];

            foreach (var water in entities.OfType<Water>())
            {
                var xStart = (int)Math.Floor(water.Position.X / _size.StretchH);
                var xEnd = (int)Math.Ceiling((water.Position.X + water.Size.X) / _size.StretchH);
                var yStart = (int)Math.Floor(water.Position.Y / _size.StretchH);
                var yEnd = (int)Math.Ceiling((water.Position.Y + water.Size.Y) / _size.StretchH);

                if (xEnd > Size.X - 1) xEnd = Size.X - 1;
                if (yEnd > Size.Y - 1) yEnd = Size.Y - 1;

                for (int x = xStart; x <= xEnd; x++)
                {
                    for (int y = yStart; y <= yEnd; y++)
                        blockedMap[x, y] = (HeightMap[x, y] * Size.StretchV) < water.Height - water.TraversableDepth;
                }
            }

            // TODO: Block on too steep terrain

            Pathfinder = new SimplePathfinder(blockedMap);
        }
        #endregion

        #region Terrain template
        /// <summary>
        /// Determines the <typeparamref name="TTemplate"/> effective at specific coordinates.
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
        /// <inheritdoc/>
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
