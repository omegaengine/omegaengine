/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
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
        public float Height { get { return _height; } set { value.To(ref _height, OnChanged); } }

        /// <summary>
        /// The maximum depth an <see cref="EntityBase{TCoordinates,TTemplate}"/> can walk into this water.
        /// </summary>
        [XmlAttribute, DefaultValue(0f), Description("The maximum depth a entity can walk into this water")]
        public float TraversableDepth { get; set; }
        #endregion
    }
}
