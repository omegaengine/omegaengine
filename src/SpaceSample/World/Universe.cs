using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Common.Utils;

namespace World
{
    /// <summary>
    /// This class represents a complete game world (but not a running game).
    /// It is equivalent to the content of a map file.
    /// </summary>
    public sealed partial class Universe
    {
        #region Events
        /// <summary>
        /// Occurs when <see cref="Skybox"/> was changed.
        /// </summary>
        [Description("Occurs when Skybox was changed")]
        public event Action SkyboxChanged;
        #endregion

        #region Constants
        /// <summary>
        /// The number of meters in an Astronomical Unit (AU)
        /// </summary>
        public const float AUInMeters = 149597870691f;

        /// <summary>
        /// A value by which all sizes and distances are mutliplied for easier rendering
        /// </summary>
        public const float ScalingFactor = 1e-5f;
        #endregion

        #region Properties
        private readonly PositionableCollection _positionables = new PositionableCollection();

        /// <summary>
        /// A collection of all <see cref="Positionable"/>s in this <see cref="Universe"/>.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Entity)), XmlElement(typeof(BenchmarkPoint)), XmlElement(typeof(Memo))]
        // Note: Can not use ICollection<T> interface with XML Serialization
            public PositionableCollection Positionables
        {
            get { return _positionables; }
        }

        /// <summary>
        /// The position and direction of the camera in the game.
        /// </summary>
        /// <remarks>This is updated only when leaving the game, not continuously.</remarks>
        [Browsable(false)]
        public CameraState Camera { get; set; }

        private string _skybox;

        /// <summary>
        /// The name of the skybox to use for this map; may be <see langword="null"/> or empty.
        /// </summary>
        [DefaultValue(""), Category("Background"), Description("The name of the skybox to use for this map; may be null or empty.")]
        public string Skybox { get { return _skybox; } set { value.To(ref _skybox, SkyboxChanged); } }
        #endregion

        //--------------------//

        #region Gravity helper
        /// <summary>
        /// Calculates the acceleration caused by gravity
        /// </summary>
        /// <param name="mass">The mass of the accelerating (larger) body in kg</param>
        /// <param name="radius">The distance between the two bodies in m</param>
        /// <returns>The accleration in m/s²</returns>
        public static double GravAcc(double mass, double radius)
        {
            // Universal gravitational constant
            const double gamma = 6.673e-11f;

            return Math.Sqrt(gamma * mass / radius);
        }
        #endregion

        //--------------------//

        #region Path finding
        /// <summary>
        /// Recalculates all stored paths.
        /// </summary>
        /// <remarks>This needs to be called when new obstacles have appeared or when a savegame was loaded (which doesn't store the complete path).</remarks>
        internal void RecalcPaths()
        {
            // ToDo: Implement
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates the <see cref="Universe"/> and all <see cref="Entity"/>s in it.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since the last update.</param>
        public void Update(double elapsedTime)
        {
            // ToDo: Implement
        }
        #endregion
    }
}
