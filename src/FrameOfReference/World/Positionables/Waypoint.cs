/*
 * Copyright 2006-2014 Bastian Eicher
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

using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.Positionables;
using NanoByte.Common.Utils;
using SlimDX;

namespace FrameOfReference.World.Positionables
{
    /// <summary>
    /// A marker used to control automated <see cref="Positionables.Entity"/> movement.
    /// </summary>
    public class Waypoint : Positionable<Vector2>
    {
        private string _targetEntity;

        /// <summary>
        /// The name of the <see cref="Positionables.Entity"/> this waypoint is for.
        /// </summary>
        [XmlAttribute, Description("The name of the Entity this waypoint is for.")]
        public string TargetEntity { get { return _targetEntity; } set { value.To(ref _targetEntity, OnChanged); } }

        private double _activationTime;

        /// <summary>
        /// The <see cref="AlphaFramework.World.UniverseBase{T}.GameTime"/> when <see cref="Positionables.Entity"/>s start walking towards this waypoint.
        /// </summary>
        [DefaultValue(0.0), Description("The GameTime when Entities start walking towards this waypoint.")]
        public double ActivationTime { get { return _activationTime; } set { value.To(ref _activationTime, OnChanged); } }

        #region Auto-set
        /// <inheritdoc/>
        protected override void OnChanged()
        {
            base.OnChanged();
            ArrivalTimeSpecified = OriginPositionSpecified = false;
        }

        private double _arrivalTime;

        /// <summary>
        /// The <see cref="AlphaFramework.World.UniverseBase{T}.GameTime"/> when <see cref="Positionables.Entity"/>s reach this waypoint.
        /// Set automatically by <see cref="Universe.HandleWaypoints"/>.
        /// </summary>
        [ReadOnly(true)]
        public double ArrivalTime
        {
            get { return _arrivalTime; }
            set
            {
                _arrivalTime = value;
                ArrivalTimeSpecified = true;
            }
        }

        /// <summary>
        /// Indicates whether <see cref="ArrivalTime"/> has been set.
        /// </summary>
        [Browsable(false), XmlIgnore]
        public bool ArrivalTimeSpecified { get; set; }

        private Vector2 _originPosition;

        /// <summary>
        /// The position where an <see cref="Positionables.Entity"/> walking towards this waypoint started off.
        /// Set automatically by <see cref="Universe.HandleWaypoints"/>.
        /// </summary>
        [ReadOnly(true)]
        public Vector2 OriginPosition
        {
            get { return _originPosition; }
            set
            {
                _originPosition = value;
                OriginPositionSpecified = true;
            }
        }

        /// <summary>
        /// Indicates whether <see cref="OriginPosition"/> has been set.
        /// </summary>
        [Browsable(false), XmlIgnore]
        public bool OriginPositionSpecified { get; set; }
        #endregion
    }
}
