/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.VertexDecl
{
    /// <summary>
    /// A fixed-function vertex format that stores a transformed position and color.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TransformedColored
    {
        #region Constants
        /// <summary>
        /// The fixed-function format of this vertex structure.
        /// </summary>
        public const VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse;

        /// <summary>
        /// The length of this vertex structure in bytes.
        /// </summary>
        public const int StrideSize = 5 * 4;
        #endregion

        #region Variables
        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        /// <summary>The position of the vertex in screen-space</summary>
        public Vector3 Position;

        /// <summary>The reciprocal of homogeneous W (the depth-value)</summary>
        public float Rhw;

        /// <summary>The color of the vertex</summary>
        public int Color;

        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore MemberCanBePrivate.Global
        #endregion

        #region Properties
        /// <summary>The X-component of the position of the vertex in screen-space</summary>
        public float X { get => Position.X; set => Position.X = value; }

        /// <summary>The Y-component of the position of the vertex in screen-space</summary>
        public float Y { get => Position.Y; set => Position.Y = value; }

        /// <summary>The Z-component of the position of the vertex in screen-space</summary>
        public float Z { get => Position.Z; set => Position.Z = value; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new transformed, colored vertex
        /// </summary>
        /// <param name="position">The position of the vertex in screen-space</param>
        /// <param name="rhw">The reciprocal of homogeneous W (the depth-value)</param>
        /// <param name="color">The color of the vertex</param>
        public TransformedColored(Vector3 position, float rhw, int color)
        {
            Position = position;
            Rhw = rhw;
            Color = color;
        }

        /// <summary>
        /// Creates a new transformed, colored vertex
        /// </summary>
        /// <param name="xvalue">The X-component of the position of the vertex in screen-space</param>
        /// <param name="yvalue">The Y-component of the position of the vertex in screen-space</param>
        /// <param name="zvalue">The Z-component of the position of the vertex in screen-space</param>
        /// <param name="rhw">The reciprocal of homogeneous W (the depth-value)</param>
        /// <param name="color">The color of the vertex</param>
        public TransformedColored(float xvalue, float yvalue, float zvalue, float rhw, int color)
            : this(new(xvalue, yvalue, zvalue), rhw, color)
        {}
        #endregion

        #region ToString
        public override string ToString()
        {
            return "TransformedColored(position=" + Position + ", rhw=" + Rhw + ", " +
                   "color=" + Color + ")";
        }
        #endregion
    }
}
