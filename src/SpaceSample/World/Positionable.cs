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
using Common;
using Common.Values;

namespace World
{
    /// <summary>
    /// An object that can be positioned in space.
    /// </summary>
    public abstract class Positionable : ICloneable
    {
        #region Events
        /// <summary>
        /// Occurs when a property relevant for rendering has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when a property relevant for rendering has changed.")]
        public event Action<Positionable> RenderPropertyChanged;

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

        private DoubleVector3 _position;

        /// <summary>
        /// The <see cref="Positionable"/>'s position in space.
        /// </summary>
        [Description("The entity's position on the terrain.")]
        public DoubleVector3 Position { get { return _position; } set { UpdateHelper.Do(ref _position, value, OnRenderPropertyChanged); } }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Positionable"/>.
        /// </summary>
        /// <returns>The cloned <see cref="Positionable"/>.</returns>
        public virtual Positionable Clone()
        {
            var clonedPositionable = (Positionable)MemberwiseClone();

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
