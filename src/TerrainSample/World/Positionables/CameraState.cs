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

using System.ComponentModel;
using Common.Values.Design;

namespace TerrainSample.World.Positionables
{
    /// <summary>
    /// Stores the position and direction of the camera in the game.
    /// </summary>
    /// <seealso cref="Universe{TCoordinates}.Camera"/>
    /// <typeparam name="TCoordinates">Coordinate data type (2D, 3D, ...)</typeparam>
    public class CameraState<TCoordinates> : Positionable<TCoordinates>
        where TCoordinates : struct
    {
        /// <summary>
        /// The camera's distance from the focused position.
        /// </summary>
        [Description("The camera's distance from the focused position.")]
        public float Radius { get; set; }

        /// <summary>
        /// The horizontal rotation of the view direction in degrees.
        /// </summary>
        [DefaultValue(0f), Description("The horizontal rotation of the view direction in degrees.")]
        [Editor(typeof(AngleEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public float Rotation { get; set; }
    }
}
