/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;
using System.Xml.Serialization;

namespace AlphaFramework.World.Terrains
{
    /// <summary>
    /// Contains information about the size of a <see cref="ITerrain"/>.
    /// </summary>
    /// <seealso cref="ITerrain.Size"/>
    [XmlInclude(typeof(Size))]
    public struct TerrainSize
    {
        #region Properties
        /// <summary>
        /// The size of the <see cref="ITerrain"/> along the X-axis.
        /// </summary>
        [XmlAttribute]
        public int X { get; set; }

        /// <summary>
        /// The size of the <see cref="ITerrain"/> along the Y-axis.
        /// </summary>
        [XmlAttribute]
        public int Y { get; set; }

        /// <summary>
        /// A factor by which the <see cref="ITerrain"/> is horizontally stretched.
        /// </summary>
        [XmlAttribute]
        public float StretchH { get; set; }

        /// <summary>
        /// A factor by which the <see cref="ITerrain"/> is vertically stretched.
        /// </summary>
        [XmlAttribute]
        public float StretchV { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="ITerrain"/> point information struct.
        /// </summary>
        /// <param name="x">The size along the x-axis.</param>
        /// <param name="y">The size along the y-axis.</param>
        /// <param name="stretchH">A factor by which the terrain is horizontally stretched.</param>
        /// <param name="stretchV">A factor by which the terrain is vertically stretched.</param>
        public TerrainSize(int x, int y, float stretchH = 1, float stretchV = 1) : this()
        {
            X = x;
            Y = y;
            StretchH = stretchH;
            StretchV = stretchV;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Convert <see cref="Size"/> into <see cref="TerrainSize"/>.
        /// </summary>
        public static implicit operator TerrainSize(Size size)
        {
            return new TerrainSize(size.Width, size.Height);
        }

        /// <summary>
        /// Convert <see cref="TerrainSize"/> into <see cref="Size"/>.
        /// </summary>
        public static implicit operator Size(TerrainSize size)
        {
            return new Size(size.X, size.Y);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "X: " + X + "\nY: " + Y + "\nStretchH: " + StretchH + "\nStretchV: " + StretchV;
        }
        #endregion
    }
}
