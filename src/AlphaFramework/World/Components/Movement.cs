/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.World.Components
{
    /// <summary>
    /// Controls the basic movement parameters.
    /// </summary>
    public class Movement : ICloneable
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>
        /// How many units the <see cref="EntityBase{TCoordinates,TTemplate}"/> can walk per second.
        /// </summary>
        [XmlAttribute, DefaultValue(200f), Description("How many units the entity can walk per second.")]
        public float Speed { get; set; } = 200;

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this <see cref="Movement"/>
        /// </summary>
        /// <returns>The cloned <see cref="Movement"/>.</returns>
        public Movement Clone()
        {
            // Perform initial shallow copy
            return (Movement)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
