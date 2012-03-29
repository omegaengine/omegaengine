/*
 * Copyright 2006-2012 Bastian Eicher
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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using World.EntityComponents;

namespace World
{
    /// <summary>
    /// A collection of components used as a prototype for constructing new <see cref="Entity"/>s. Defines the behavior and look for a certain class of <see cref="Entity"/>.
    /// </summary>
    /// <seealso cref="TemplateManager.EntityTemplates"/>
    public sealed class EntityTemplate : Template<EntityTemplate>
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
        /// Creates a deep copy of this <see cref="EntityTemplate"/>.
        /// </summary>
        /// <returns>The cloned <see cref="EntityTemplate"/>.</returns>
        public override EntityTemplate Clone()
        {
            // Perform initial shallow copy
            var newClass = base.Clone();

            // Replace contained lists with deep copies
            newClass._renderControls = new Collection<RenderControl>();
            foreach (RenderControl renderControl in RenderControls)
                newClass.RenderControls.Add(renderControl.Clone());
            if (MovementControl != null) newClass.MovementControl = MovementControl.Clone();

            return newClass;
        }
        #endregion
    }
}
