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

using System.Drawing;
using System.Xml.Serialization;

namespace World.Terrains
{
    /// <summary>
    /// Contains information about the size of a <see cref="Terrain"/>.
    /// </summary>
    /// <seealso cref="Terrain.Size"/>
    [XmlInclude(typeof(Size))]
    public struct TerrainSize
    {
        #region Properties
        /// <summary>
        /// The size of the <see cref="Terrain"/> along the X-axis.
        /// </summary>
        [XmlAttribute]
        public int X { get; set; }

        /// <summary>
        /// The size of the <see cref="Terrain"/> along the Y-axis.
        /// </summary>
        [XmlAttribute]
        public int Y { get; set; }

        /// <summary>
        /// A factor by which the <see cref="Terrain"/> is horizontally stretched.
        /// </summary>
        [XmlAttribute]
        public float StretchH { get; set; }

        /// <summary>
        /// A factor by which the <see cref="Terrain"/> is vertically stretched.
        /// </summary>
        [XmlAttribute]
        public float StretchV { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Terrain"/> point information struct.
        /// </summary>
        /// <param name="x">The size along the x-axis.</param>
        /// <param name="y">The size along the y-axis.</param>
        /// <param name="stretchH">A factor by which the terrain is horizontally stretched.</param>
        /// <param name="stretchV">A factor by which the terrain is vertically stretched.</param>
        public TerrainSize(int x, int y, float stretchH, float stretchV) : this()
        {
            X = x;
            Y = y;
            StretchH = stretchH;
            StretchV = stretchV;
        }

        /// <summary>
        /// Creates a new <see cref="Terrain"/> point information struct.
        /// </summary>
        /// <param name="x">The size along the x-axis.</param>
        /// <param name="y">The size along the y-axis.</param>
        public TerrainSize(int x, int y) : this(x, y, 1, 1)
        {}
        #endregion

        //--------------------//

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
