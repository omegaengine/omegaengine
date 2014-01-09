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
using Common.Utils;

namespace AlphaFramework.World.Positionables
{
    /// <summary>
    /// An object that can be positioned in the game world.
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    public abstract class Positionable<TCoordinates> : ICloneable
        where TCoordinates : struct
    {
        #region Events
        /// <summary>
        /// Occurs when a property relevant for rendering has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when a property relevant for rendering has changed.")]
        public event Action<Positionable<TCoordinates>> RenderPropertyChanged;

        /// <summary>
        /// To be called when a property relevant for rendering has changed.
        /// </summary>
        protected void OnRenderPropertyChanged()
        {
            if (RenderPropertyChanged != null) RenderPropertyChanged(this);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Used for identification in scripts, debugging, etc.
        /// </summary>
        [XmlAttribute, Description("Used for identification in scripts, debugging, etc.")]
        public string Name { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            string value = GetType().Name;
            if (!string.IsNullOrEmpty(Name))
                value += ": " + Name;
            return value;
        }

        private TCoordinates _position;

        /// <summary>
        /// The <see cref="Positionable{TCoordinates}"/>'s position.
        /// </summary>
        [Description("The entity's position on the terrain.")]
        public TCoordinates Position { get { return _position; } set { value.To(ref _position, OnRenderPropertyChanged); } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Positionable{TCoordinates}"/>.
        /// </summary>
        /// <returns>The cloned <see cref="Positionable{TCoordinates}"/>.</returns>
        public virtual Positionable<TCoordinates> Clone()
        {
            var clonedPositionable = (Positionable<TCoordinates>)MemberwiseClone();

            // Don't clone event handlers
            clonedPositionable.RenderPropertyChanged = null;

            return clonedPositionable;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
