/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.EntityComponents;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.World.Templates
{
    /// <summary>
    /// A common base for entity templates (collection of components used as a prototype for constructing new entities). Defines the behavior and look for a certain class of <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/>.
    /// </summary>
    public abstract class EntityTemplateBase<TSelf> : Template<TSelf> where TSelf : EntityTemplateBase<TSelf>
    {
        private Collection<RenderControl> _renderControls = new Collection<RenderControl>();

        /// <summary>
        /// Controls how this class of entities shall be rendered.
        /// </summary>
        [Browsable(false)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        [XmlElement(typeof(TestSphere)), XmlElement(typeof(StaticMesh)), XmlElement(typeof(AnimatedMesh)), XmlElement(typeof(CpuParticleSystem)), XmlElement(typeof(GpuParticleSystem)), XmlElement(typeof(LightSource))]
        public Collection<RenderControl> RenderControls { get { return _renderControls; } }

        /// <summary>
        /// Controls the basic movement parameters.
        /// </summary>
        [Browsable(false)]
        public MovementControl MovementControl { get; set; }

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="EntityTemplateBase{TSelf}"/>.
        /// </summary>
        /// <returns>The cloned <see cref="EntityTemplateBase{TSelf}"/>.</returns>
        public override TSelf Clone()
        {
            // Perform initial shallow copy
            var newClass = base.Clone();

            // Replace contained lists with deep copies
            newClass._renderControls = new Collection<RenderControl>();
            foreach (RenderControl renderControl in RenderControls)
                newClass.RenderControls.Add(renderControl.Clone());

            return newClass;
        }
        #endregion
    }
}
