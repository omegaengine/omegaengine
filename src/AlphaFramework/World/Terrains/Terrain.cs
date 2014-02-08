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
using System.Xml.Serialization;
using AlphaFramework.World.Templates;
using Common.Utils;
using Common.Values;
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

        #region Occlusion interval map
        private byte[,] _occlusionEndMap;

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] OcclusionEndMap
        {
            get { return _occlusionEndMap; }
            set
            {
                #region Sanity checks
                if (value != null)
                {
                    if (value.GetUpperBound(0) + 1 != _size.X || value.GetUpperBound(1) + 1 != _size.Y)
                        throw new InvalidOperationException(Resources.HeightMapSizeEqualTerrain);
                }
                #endregion

                _occlusionEndMap = value;
            }
        }

        private byte[,] _occlusionBeginMap;

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "For performance reasons this property provides direct access to the underlying array without any cloning involved")]
        public byte[,] OcclusionBeginMap
        {
            get { return _occlusionBeginMap; }
            set
            {
                #region Sanity checks
                if (value != null)
                {
                    if (value.GetUpperBound(0) + 1 != _size.X || value.GetUpperBound(1) + 1 != _size.Y)
                        throw new InvalidOperationException(Resources.HeightMapSizeEqualTerrain);
                }
                #endregion

                _occlusionBeginMap = value;
            }
        }

        /// <inheritdoc/>
        [XmlIgnore]
        public bool OcclusionIntervalMapSet { get { return OcclusionEndMap != null && OcclusionBeginMap != null; } }

        /// <inheritdoc/>
        [XmlAttribute, DefaultValue(false)]
        public bool OcclusionIntervalMapOutdated { get; set; }
        #endregion

        #region Texture-map
        /// <summary>
        /// The <typeparamref name="TTemplate"/>s available for usage in this <see cref="ITerrain"/>.
        /// </summary>
        [XmlIgnore]
        public readonly TTemplate[] Templates = new TTemplate[16];

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
    }
}
