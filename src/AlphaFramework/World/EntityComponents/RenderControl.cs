/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Templates;
using SlimDX;

namespace AlphaFramework.World.EntityComponents
{
    /// <summary>
    /// Controls how an <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/> shall be rendered.
    /// </summary>
    /// <seealso cref="EntityTemplateBase{TSelf}.RenderControls"/>
    public abstract class RenderControl : ICloneable
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>
        /// How this component is to be shifted before rendering.
        /// </summary>
        [Description("How this component is to be shifted before rendering.")]
        public Vector3 Shift { get; set; }

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this <see cref="RenderControl"/>
        /// </summary>
        /// <returns>The cloned <see cref="RenderControl"/>.</returns>
        public RenderControl Clone()
        {
            // Perform initial shallow copy
            return (RenderControl)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
