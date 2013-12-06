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
using Common.Utils;
using Common.Values;
using Common.Values.Design;
using World.EntityComponents;

namespace World
{
    /// <summary>
    /// A <see cref="Positionable{TCoordinates}"/> whose behaviour and graphical representation is controlled by <see cref="World.EntityComponents"/>.
    /// </summary>
    /// <typeparam name="TCoordinates">Coordinate data type (2D, 3D, ...)</typeparam>
    public abstract class Entity<TCoordinates> : Positionable<TCoordinates>
        where TCoordinates : struct
    {
        #region Events
        /// <summary>
        /// Occurs when <see cref="TemplateData"/> is about to change.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when TemplateData is about to change.")]
        public event Action<Entity<TCoordinates>> TemplateChanging;

        /// <summary>
        /// To be called when <see cref="TemplateData"/> is about to change.
        /// </summary>
        protected void OnTemplateChanging()
        {
            if (TemplateChanging != null) TemplateChanging(this);
        }

        /// <summary>
        /// Occurs when <see cref="TemplateData"/> has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when TemplateData has changed.")]
        public event Action<Entity<TCoordinates>> TemplateChanged;

        /// <summary>
        /// To be called when <see cref="TemplateData"/> has changed.
        /// </summary>
        protected void OnTemplateChanged()
        {
            if (TemplateChanged != null) TemplateChanged(this);
        }
        #endregion

        #region Properties
        private string _className;

        /// <summary>
        /// The name of the <see cref="EntityTemplate"/>.
        /// </summary>
        /// <remarks>
        /// Setting this will overwrite <see cref="TemplateData"/> with a new clone of the appropriate <see cref="EntityTemplate"/>.
        /// This is serialized/stored in map files. It is also serialized/stored in savegames, but the value is ignored there (due to the attribute order)!
        /// </remarks>
        [XmlAttribute("Template"), Description("The name of the entity template")]
        public string TemplateName
        {
            get { return _className; }
            set
            {
                // Create copy of the class so run-time modifications for individual entities are possible
                TemplateData = TemplateManager.GetEntityTemplate(value).Clone();

                // Only set the new name once the according class was successfully located
                _className = value;
            }
        }

        private EntityTemplate _template;

        /// <summary>
        /// The <see cref="EntityTemplate"/> controlling the behavior and look for this <see cref="Entity{TCoordinates}"/>.
        /// </summary>
        /// <remarks>
        /// This is always a clone of the original <see cref="EntityTemplate"/>s.
        /// This is serialized/stored in savegames but not in map files!
        /// </remarks>
        [Browsable(false)]
        public EntityTemplate TemplateData
        {
            get { return _template; }
            set
            {
                OnTemplateChanging();

                // Backup the original value (might be needed for restore on exception) and then set the new value
                var oldValue = _template;
                _template = value;

                try
                {
                    OnTemplateChanged();
                }
                    #region Error handling
                catch (Exception)
                {
                    // Restore the original value, trigger a render update for that and then pass on the exception for handling
                    _template = oldValue;
                    OnTemplateChanged();
                    throw;
                }
                #endregion
            }
        }

        /// <summary>
        /// Controls how this <see cref="Entity{TCoordinates}"/> will move along a path generated by pathfinding.
        /// </summary>
        public abstract PathControl<TCoordinates> PathControl { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            string value = base.ToString();
            if (!string.IsNullOrEmpty(_className))
                value += " (" + _className + ")";
            return value;
        }

        private float _rotation;

        /// <summary>
        /// The horizontal rotation of the view direction in degrees.
        /// </summary>
        [DefaultValue(0f), Description("The horizontal rotation of the view direction in degrees.")]
        [EditorAttribute(typeof(AngleEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public float Rotation { get { return _rotation; } set { value.To(ref _rotation, OnRenderPropertyChanged); } }
        #endregion

        //--------------------//

        #region Collision
        /// <summary>
        /// Determines whether a certain point collides with this entity (based on its <see cref="CollisionControl{TCoordinates}"/> component).
        /// </summary>
        /// <param name="point">The point to check for collision in world space.</param>
        /// <returns><see langword="true"/> if the <paramref name="point"/> does collide with this entity, <see langword="false"/> otherwise.</returns>
        public abstract bool CollisionTest(TCoordinates point);

        /// <summary>
        /// Determines whether a certain area collides with this entity (based on its <see cref="CollisionControl{TCoordinates}"/> component).
        /// </summary>
        /// <param name="area">The area to check for collision in world space.</param>
        /// <returns><see langword="true"/> if the <paramref name="area"/> does collide with this entity, <see langword="false"/> otherwise.</returns>
        public abstract bool CollisionTest(Quadrangle area);
        #endregion

        #region Path finding
        /// <summary>
        /// Returns a list of positions that outline this <see cref="Entity{TCoordinates}"/>s <see cref="CollisionControl{TCoordinates}"/>.
        /// </summary>
        /// <returns>Positions in world space for use by the pathfinding system.</returns>
        public abstract TCoordinates[] GetPathFindingOutline();

        /// <summary>
        /// Perform movements queued up in <see cref="World.PathControl{TCoordinates}"/>.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
        public abstract void UpdatePosition(double elapsedTime);
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Entity{TCoordinates}"/>.
        /// </summary>
        /// <returns>The cloned <see cref="Entity{TCoordinates}"/>.</returns>
        public virtual Entity<TCoordinates> CloneEntity()
        {
            var clonedEntity = (Entity<TCoordinates>)base.Clone();

            // Don't clone event handlers
            clonedEntity.TemplateChanged = null;
            clonedEntity.TemplateChanging = null;

            clonedEntity._template = _template.Clone();

            return clonedEntity;
        }

        /// <inheritdoc />
        public override Positionable<TCoordinates> Clone()
        {
            return CloneEntity();
        }
        #endregion
    }
}
