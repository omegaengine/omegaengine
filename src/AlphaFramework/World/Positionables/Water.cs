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
using System.Xml.Serialization;
using AlphaFramework.World.Terrains;
using Common.Utils;
using Common.Values;
using SlimDX;

namespace AlphaFramework.World.Positionables
{
    /// <summary>
    /// A water plane spanning a certain part of the <see cref="Terrain{TTemplate}"/>.
    /// </summary>
    public class Water : Positionable<Vector2>
    {
        #region Events
        /// <summary>
        /// Occurs when <see cref="Size"/> has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when the Size property has changed.")]
        public event Action<Water> SizeChanged;

        /// <summary>
        /// To be called when <see cref="Size"/> has changed.
        /// </summary>
        protected void OnSizeChanged()
        {
            if (SizeChanged != null) SizeChanged(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The position of origin for this water in the engine coordinate system.
        /// </summary>
        /// <remarks>
        /// World X = Engine +X<br />
        /// World Y = Engine -Z<br />
        /// World height = Engine +Y
        /// </remarks>
        [Browsable(false)]
        public DoubleVector3 EnginePosition { get { return new DoubleVector3(Position.X, Height, -Position.Y); } }

        private Vector2 _size;

        /// <summary>
        /// The size of the area of the <see cref="ITerrain"/> this water plane spans.
        /// </summary>
        [Description("The size of the area of the terrain this water plane spans.")]
        public Vector2 Size { get { return _size; } set { value.To(ref _size, OnSizeChanged); } }

        private float _height;

        /// <summary>
        /// The height of the water above reference zero.
        /// </summary>
        [XmlAttribute, DefaultValue(0f), Description("The height of the water above reference zero.")]
        public float Height { get { return _height; } set { value.To(ref _height, OnRenderPropertyChanged); } }

        /// <summary>
        /// The maximum depth an <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/> can walk into this water.
        /// </summary>
        [XmlAttribute, DefaultValue(0f), Description("The maximum depth a entity can walk into this water")]
        public float TraversableDepth { get; set; }
        #endregion
    }
}
