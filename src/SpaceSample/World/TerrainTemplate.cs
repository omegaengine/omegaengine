using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using Common.Utils;
using Common.Values;
using LuaInterface;

namespace World
{
    /// <summary>
    /// Defines a type of terrain (texture, effects on units, etc.).
    /// </summary>
    /// <seealso cref="TemplateManager.TerrainTemplates"/>
    public sealed class TerrainTemplate : Template<TerrainTemplate>
    {
        /// <summary>
        /// The filename of the ground texture.
        /// </summary>
        [XmlAttribute, Browsable(false)]
        public string Texture { get; set; }

        private Color _color = Color.Black;

        /// <summary>
        /// The mini-map color for this terrain type. Should be unique.
        /// </summary>
        /// <remarks>Is not serialized/stored, <see cref="ColorValue"/> is used for that.</remarks>
        [XmlIgnore, LuaHide, Description("The mini-map color for this terrain type. Should be unique.")]
        public Color Color { get { return _color; } set { _color = Color.FromArgb(255, value); } }

        // Drop alpha-value

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Color"/>
        [XmlElement("Color"), LuaHide, Browsable(false)]
        public XColor ColorValue { get { return Color; } set { Color = Color.FromArgb(value.R, value.G, value.B); } }

        // Drop alpha-value

        private float _movementAbility = 1;

        /// <summary>
        /// How good can units walk on this ground? 0=not at all; 1=with full speed
        /// </summary>
        [DefaultValue(1f), Description("How good can units walk on this ground? 0=not at all; 1=with full speed")]
        public float MovementAbility { get { return _movementAbility; } set { _movementAbility = MathUtils.Clamp(value, 0, 1); } }
    }
}
