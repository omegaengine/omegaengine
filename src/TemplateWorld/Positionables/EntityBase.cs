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
using Common.Collections;
using TemplateWorld.Paths;
using TemplateWorld.Templates;

namespace TemplateWorld.Positionables
{
    /// <summary>
    /// A common base class for <see cref="Positionable{TCoordinates}"/> whose behaviour and graphical representation is controlled by components.
    /// </summary>
    /// <typeparam name="TSelf">The type of the class itself.</typeparam>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    /// <typeparam name="TTemplate">The specific type of <see cref="EntityTemplateBase{TSelf}"/> to use as a component container.</typeparam>
    public abstract class EntityBase<TSelf, TCoordinates, TTemplate> : Positionable<TCoordinates>, ITemplateName, IUpdateable
        where TSelf : EntityBase<TSelf, TCoordinates, TTemplate>
        where TCoordinates : struct
        where TTemplate : EntityTemplateBase<TTemplate>
    {
        #region Events
        /// <summary>
        /// Occurs when <see cref="TemplateData"/> is about to change.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when TemplateData is about to change.")]
        public event Action<TSelf> TemplateChanging;

        /// <summary>
        /// To be called when <see cref="TemplateData"/> is about to change.
        /// </summary>
        protected void OnTemplateChanging()
        {
            if (TemplateChanging != null) TemplateChanging((TSelf)this);
        }

        /// <summary>
        /// Occurs when <see cref="TemplateData"/> has changed.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when TemplateData has changed.")]
        public event Action<TSelf> TemplateChanged;

        /// <summary>
        /// To be called when <see cref="TemplateData"/> has changed.
        /// </summary>
        protected void OnTemplateChanged()
        {
            if (TemplateChanged != null) TemplateChanged((TSelf)this);
        }
        #endregion

        #region Properties
        private string _templateName;

        /// <summary>
        /// The name of the <typeparamref name="TTemplate"/>.
        /// </summary>
        /// <remarks>
        /// Setting this will overwrite <see cref="TemplateData"/> with a new clone of the appropriate <typeparamref name="TTemplate"/>.
        /// This is serialized/stored in map files. It is also serialized/stored in savegames, but the value is ignored there (due to the attribute order)!
        /// </remarks>
        [XmlAttribute("Template"), Description("The name of the entity template")]
        public string TemplateName
        {
            get { return _templateName; }
            set
            {
                // Create copy of the class so run-time modifications for individual entities are possible
                TemplateData = Template<TTemplate>.All[value].Clone();

                // Only set the new name once the according class was successfully located
                _templateName = value;
            }
        }

        private TTemplate _template;

        /// <summary>
        /// The <typeparamref name="TTemplate"/> controlling the behavior and look for this <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/>.
        /// </summary>
        /// <remarks>
        /// This is always a clone of the original <typeparamref name="TTemplate"/>.
        /// This is serialized/stored in savegames but not in map files!
        /// </remarks>
        [Browsable(false)]
        public TTemplate TemplateData
        {
            get { return _templateDataMasked ? null : _template; }
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
        /// Controls how this <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/> will move along a path generated by pathfinding.
        /// </summary>
        [Browsable(false)]
        [XmlIgnore] // XML serialization configuration is configured in sub-type
        public abstract PathControl<TCoordinates> PathControl { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            string value = base.ToString();
            if (!string.IsNullOrEmpty(_templateName))
                value += " (" + _templateName + ")";
            return value;
        }
        #endregion

        #region Masking
        // ReSharper disable once StaticFieldInGenericType
        private static bool _templateDataMasked;

        private struct TemplateDataMasking : IDisposable
        {
            public void Dispose()
            {
                _templateDataMasked = false;
            }
        }

        /// <summary>
        /// Makes all <see cref="TemplateData"/> values return <see langword="null"/> until <see cref="IDisposable.Dispose"/> is called on the returned object. This is not thread-safe!
        /// </summary>
        public static IDisposable MaskTemplateData()
        {
            _templateDataMasked = true;
            return new TemplateDataMasking();
        }
        #endregion

        //--------------------//

        #region Pathfinding
        /// <summary>
        /// Perform movements queued up in pathfinding.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
        public virtual void Update(double elapsedTime)
        {
            if (PathControl != null)
            {
                new PerTypeDispatcher<PathControl<TCoordinates>>(ignoreMissing: false)
                {
                    (PathLeader<TCoordinates> leader) => UpdatePath(leader, elapsedTime),
                    (PathFollower<TCoordinates> follower) => UpdatePath(follower, elapsedTime),
                }.Dispatch(PathControl);
            }
        }

        /// <summary>
        /// Handles movement controlled by a <see cref="PathLeader{TCoordinates}"/>.
        /// </summary>
        protected abstract void UpdatePath(PathLeader<TCoordinates> leader, double elapsedTime);

        /// <summary>
        /// Handles movement controlled by a <see cref="PathFollower{TCoordinates}"/>.
        /// </summary>
        protected abstract void UpdatePath(PathFollower<TCoordinates> follower, double elapsedTime);
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/>.
        /// </summary>
        /// <returns>The cloned <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/>.</returns>
        public virtual TSelf CloneEntity()
        {
            var clonedEntity = (TSelf)base.Clone();

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
