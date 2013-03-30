using System;
using System.Xml.Serialization;
using Common.Utils;
using Common.Values;

namespace World
{
    /// <summary>
    /// Describes an eliptic orbit of a body around another in 3D-space
    /// </summary>
    public class Orbit
    {
        #region Variables
        private Entity _orbitCenterBody;
        private double _radius, _theta;
        #endregion

        #region Properties
        /// <summary>
        /// The name of the body the orbit is going around - read-only, used for XML serialazation
        /// </summary>
        [XmlAttribute]
        public string OrbitCenter { get; set; }

        /// <summary>
        /// The orbiting body's minimum distance to the central body in m
        /// </summary>
        public double Periapsis { get; set; }

        /// <summary>
        /// The numeric eccentricity of the orbit
        /// </summary>
        public double Eccentricity { get; set; }

        /// <summary>
        /// The angle in degrees between the orbit's plane and the reference plane in degrees
        /// </summary>
        public double Inclination { get; set; }

        /// <summary>
        /// Shall the rotation be turned around to go clock-wise?
        /// </summary>
        [XmlAttribute]
        public bool Clockwise { get; set; }

        /// <summary>
        /// The body the orbit is going around - may be null if position wasn't calculated yet!
        /// </summary>
        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        public Entity OrbitCenterBody { get { return _orbitCenterBody; } }

        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        /// <summary>
        /// The current distance from the orbit's center - read-only, auto-updated
        /// </summary>
        // ReSharper disable ConvertToAutoPropertyWithPrivateSetter
        public double Radius { get { return _radius; } }

        // ReSharper restore ConvertToAutoPropertyWithPrivateSetter

        /// <summary>
        /// The angle in degrees pointing towards the body's current orbital position - auto-updated
        /// </summary>
        public double Theta { get { return _theta.RadianToDegree(); } set { _theta = value.DegreeToRadian(); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public Orbit()
        {}

        /// <summary>
        /// Sets up a new orbit
        /// </summary>
        /// <param name="orbitCenter">The name of the body the orbit is going around </param>
        /// <param name="periapsis">The orbiting body's distance to the central body in m</param>
        /// <param name="eccentricity">The numeric eccentricity of the orbit</param>
        /// <param name="inclination">The angle in degrees between the orbit's plane and the reference plane</param>
        /// <param name="theta">The angle in degrees pointing towards the body's intial orbital position</param>
        /// <param name="clockwise">Shall the rotation be turned around to go clock-wise?</param>
        public Orbit(string orbitCenter, double periapsis, double eccentricity, double inclination, double theta, bool clockwise)
        {
            OrbitCenter = orbitCenter;
            Periapsis = periapsis;
            Eccentricity = eccentricity;
            Inclination = inclination;
            Theta = theta;
            Clockwise = clockwise;
        }

        /// <summary>
        /// Sets up a new orbit
        /// </summary>
        /// <param name="orbitCenter">The name of the body the orbit is going around </param>
        /// <param name="periapsis">The orbiting body's distance to the central body in m</param>
        /// <param name="eccentricity">The numeric eccentricity of the orbit</param>
        /// <param name="inclination">The angle in degrees between the orbit's plane and the reference plane</param>
        /// <param name="theta">The angle in degrees pointing towards the body's intial orbital position</param>
        public Orbit(string orbitCenter, double periapsis, double eccentricity, double inclination, double theta)
            : this(orbitCenter, periapsis, eccentricity, inclination, theta, false)
        {}
        #endregion

        //--------------------//

        #region Update
        /// <summary>
        /// Calculates the position the body should currently have (Inaccurate for large ElapsedTime values!)
        /// </summary>
        /// <param name="elapsedTime">The number of seconds that have passed since this function was last called</param>
        /// <param name="currentUniverse">The universe containing the bodies</param>
        /// <returns>The position in 3D-space the body now has</returns>
        internal DoubleVector3 UpdatePosition(double elapsedTime, Universe currentUniverse)
        {
            // Get the orbit center object
            if (_orbitCenterBody == null) _orbitCenterBody = (Entity)currentUniverse.Positionables[OrbitCenter];

            // Calculate current position within 3D-space
            DoubleVector3 position = CalcPosition(_theta) + OrbitCenterBody.Position;

            // Update angular velocity
            double v = 0; // Universe.GravAcc(OrbitCenterBody.Mass, (double)Radius);

            // Update angle
            _theta += elapsedTime * v / Radius;

            while (_theta >= 2 * Math.PI) _theta -= 2 * Math.PI;

            return position;
        }
        #endregion

        #region Calculate
        private DoubleVector3 CalcPosition(double theta)
        {
            double inclinationAngle = Inclination.DegreeToRadian();

            // Calculate current position in 3D-space
            _radius = Periapsis * (1 + Eccentricity) / (1 + Eccentricity * Math.Cos(theta));
            double x = Radius * Math.Cos(theta) * Math.Cos(inclinationAngle);
            double y = Radius * Math.Sin(theta);
            double z = Math.Sin(inclinationAngle) * Math.Cos(theta);

            // Turn around rotation for clock-wise
            return Clockwise ? new DoubleVector3(-x, -z, -y) : new DoubleVector3(x, z, y);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public Orbit Clone()
        {
            return new Orbit(OrbitCenter, Periapsis, Eccentricity, Inclination, Theta, Clockwise);
        }
        #endregion
    }
}
