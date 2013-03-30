using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Utils;
using Common.Values.Design;

namespace World
{
    /// <summary>
    /// A <see cref="Positionable"/> in space whose behaviour and graphical representation is controlled by <see cref="World.EntityComponents"/>.
    /// </summary>
    public class Entity : Positionable
    {
        #region Events
        /// <summary>
        /// Occurs when <see cref="TemplateData"/> is about to change.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when TemplateData is about to change.")]
        public event Action<Entity> TemplateChanging;

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
        public event Action<Entity> TemplateChanged;

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
        /// The <see cref="EntityTemplate"/> controlling the behavior and look for this <see cref="Entity"/>.
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

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Entity"/>.
        /// </summary>
        /// <returns>The cloned <see cref="Entity"/>.</returns>
        public virtual Entity CloneEntity()
        {
            var clonedEntity = (Entity)base.Clone();

            // Don't clone event handlers
            clonedEntity.TemplateChanged = null;
            clonedEntity.TemplateChanging = null;

            clonedEntity._template = _template.Clone();

            return clonedEntity;
        }

        /// <inheritdoc />
        public override Positionable Clone()
        {
            return CloneEntity();
        }
        #endregion
    }
}
