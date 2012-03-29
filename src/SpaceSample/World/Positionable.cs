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
