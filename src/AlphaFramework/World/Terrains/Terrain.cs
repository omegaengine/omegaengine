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
using System.Xml.Serialization;
using AlphaFramework.World.Templates;
using LuaInterface;
using NanoByte.Common;
using OmegaEngine.Values;
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
                Math.DivRem(value.X, 3, out int resultX);
                Math.DivRem(value.Y, 3, out int resultY);
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
        public Vector2 Center => new(_size.X * _size.StretchH / 2f, _size.Y * _size.StretchH / 2f);
        #endregion

        #region Height-map
        private ByteGrid _heightMap;

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        public ByteGrid HeightMap
        {
            get => _heightMap;
            set
            {
                if (value != null)
                {
                    if (value.Width != _size.X || value.Height != _size.Y)
                        throw new InvalidOperationException(Resources.HeightMapSizeEqualTerrain);
                }

                _heightMap = value;
            }
        }
        #endregion

        #region Occlusion interval map
        private ByteVector4Grid _occlusionIntervalMap;

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        public ByteVector4Grid OcclusionIntervalMap
        {
            get => _occlusionIntervalMap;
            set
            {
                if (value != null)
                {
                    if (value.Width != _size.X || value.Height != _size.Y)
                        throw new InvalidOperationException(Resources.HeightMapSizeEqualTerrain);
                }

                _occlusionIntervalMap = value;
            }
        }

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

        private NibbleGrid _textureMap;

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        public NibbleGrid TextureMap
        {
            get => _textureMap;
            set
            {
                if (value != null)
                {
                    if (value.Width != _size.X / 3 || value.Height != _size.Y / 3)
                        throw new InvalidOperationException(Resources.TextureMapSizeThirdOfTerrain);
                }

                _textureMap = value;
            }
        }
        #endregion

        /// <inheritdoc/>
        [Browsable(false), XmlIgnore]
        public bool DataLoaded => _heightMap != null && _textureMap != null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new terrain. It is completely flat and has only one texture initially.
        /// </summary>
        /// <param name="size">The size of the terrain to create.</param>
        public Terrain(TerrainSize size)
        {
            Size = size;
            _heightMap = new(size.X, size.Y);
            _textureMap = new(size.X / 3, size.Y / 3);

            // Try to use "Grass" as the default Terrain type
            try
            {
                Templates[0] = Template<TTemplate>.All["Grass"];
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warn(ex);
            }
            catch (InvalidOperationException ex)
            {
                Log.Warn(ex);
            }
        }
        #endregion

        //--------------------//

        #region Coordinates
        /// <summary>
        /// Converts a position in world coordinates to the engine entity space coordinate system.
        /// </summary>
        /// <param name="coordinates">The coordinates of the point in engine world space to get information for.</param>
        public DoubleVector3 ToEngineCoords(Vector2 coordinates)
        {
            // Note: This is only required for lookups in the height-map, not for actually unstretching the coordinates to be returned
            Vector2 unstrechedCoords = coordinates * (1 / _size.StretchH);

            var height = HeightMap.SampledRead(unstrechedCoords.X, unstrechedCoords.Y);

            return new(
                coordinates.X, // World X = Engine +X
                height * _size.StretchV, // World height = Engine +Y
                -coordinates.Y); // World Y = Engine -Z
        }
        #endregion

        #region Slope
        /// <inheritdoc/>
        [LuaHide]
        public void MarkUntraversableSlopes(bool[,] obstructionMap, int maxTraversableSlope)
        {
            #region Sanity checks
            if (obstructionMap == null) throw new ArgumentNullException(nameof(obstructionMap));
            if (obstructionMap.GetLength(0) != Size.X || obstructionMap.GetLength(1) != Size.Y) throw new ArgumentException("Obstruction map size does not match terrain size.", nameof(obstructionMap));
            #endregion

            for (int x = 0; x < obstructionMap.GetLength(0); x++)
            {
                for (int y = 0; y < obstructionMap.GetLength(1); y++)
                {
                    obstructionMap[x, y] |=
                        (GetSlope(x, y, 0, 1) > maxTraversableSlope) ||
                        (GetSlope(x, y, 1, 0) > maxTraversableSlope) ||
                        (GetSlope(x, y, 1, 1) > maxTraversableSlope) ||
                        (GetSlope(x, y, 0, -1) > maxTraversableSlope) ||
                        (GetSlope(x, y, -1, 0) > maxTraversableSlope) ||
                        (GetSlope(x, y, -1, -1) > maxTraversableSlope);
                }
            }
        }

        private int GetSlope(int x, int y, int xDiff, int yDiff)
        {
            return Math.Abs(
                HeightMap.ClampedRead(x + xDiff, y + yDiff) -
                HeightMap.ClampedRead(x, y));
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
