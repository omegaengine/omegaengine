/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World;
using AlphaFramework.World.Components;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using FrameOfReference.World.Templates;
using NanoByte.Common.Utils;
using NanoByte.Common.Values;
using NanoByte.Common.Values.Design;
using SlimDX;

namespace FrameOfReference.World.Positionables
{
    /// <summary>
    /// An entity that can be placed on a <see cref="Terrain{TTemplate}"/>.
    /// </summary>
    public sealed class Entity : EntityBase<Vector2, EntityTemplate>
    {
        private float _rotation;

        /// <summary>
        /// The horizontal rotation of the view direction in degrees.
        /// </summary>
        [DefaultValue(0f), Description("The horizontal rotation of the view direction in degrees.")]
        [Editor(typeof(AngleEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public float Rotation { get { return _rotation; } set { value.To(ref _rotation, OnChanged); } }

        /// <summary>
        /// <see langword="true"/> if this entity is controlled by a human player, <see langword="false"/> if it is controlled by the computer.
        /// </summary>
        [XmlAttribute, DefaultValue(false), Description("true if this entity is controlled by a human player, false if it is controlled by the computer.")]
        public bool IsPlayerControlled { get; set; }

        private int _activeWaypointIndex = -1;

        /// <summary>
        /// The <see cref="Waypoints"/> index of the <see cref="Waypoint"/> this entity is currently moving towards; -1 for no <see cref="Waypoint"/>.
        /// </summary>
        [XmlAttribute, DefaultValue(-1), Browsable(false)]
        public int ActiveWaypointIndex { get { return _activeWaypointIndex; } set { _activeWaypointIndex = value; } }

        private readonly List<Waypoint> _waypoints = new List<Waypoint>();

        /// <summary>
        /// The <see cref="Waypoint"/>s associated with this entity ordered by <see cref="Waypoint.ActivationTime"/>.
        /// </summary>
        [XmlElement, Browsable(false)]
        public List<Waypoint> Waypoints { get { return _waypoints; } }

        //--------------------//

        /// <inheritdoc/>
        public override void Update(double elapsedTime)
        {
            if (CurrentPath != null)
            {
                if (elapsedTime < 0)
                {
                    UpdatePathfinding(-elapsedTime);
                    Rotation += 180;
                }
                else UpdatePathfinding(elapsedTime);
            }
        }

        /// <summary>
        /// Determines the currently active <see cref="Waypoints"/> entry.
        /// </summary>
        /// <param name="gameTime">The current <see cref="UniverseBase{TCoordinates}.GameTime"/> value.</param>
        /// <returns>The currently active <see cref="Waypoint"/>; <see langword="null"/> if none is active.</returns>
        public int GetCurrentWaypointIndex(double gameTime)
        {
            int index = _waypoints.FindLastIndex(x => x.ActivationTime <= gameTime);
            if (index == -1 || (Waypoints[index].ArrivalTimeSpecified && Waypoints[index].ArrivalTime <= gameTime)) return -1;
            return index;
        }

        #region Collision
        /// <summary>
        /// Determines whether a certain point collides with this entity (based on its <see cref="Collision{TCoordinates}"/> component).
        /// </summary>
        /// <param name="point">The point to check for collision in world space.</param>
        /// <returns><see langword="true"/> if the <paramref name="point"/> does collide with this entity, <see langword="false"/> otherwise.</returns>
        public bool CollisionTest(Vector2 point)
        {
            // With no valid collision control all collision checks always fail
            if (TemplateData.Collision == null) return false;

            // Convert position from world space to entity space and transmit rotation
            var shiftedPoint = point - Position;
            return TemplateData.Collision.CollisionTest(shiftedPoint, Rotation);
        }

        /// <summary>
        /// Determines whether a certain area collides with this entity (based on its <see cref="Collision{TCoordinates}"/> component).
        /// </summary>
        /// <param name="area">The area to check for collision in world space.</param>
        /// <returns><see langword="true"/> if the <paramref name="area"/> does collide with this entity, <see langword="false"/> otherwise.</returns>
        public bool CollisionTest(Quadrangle area)
        {
            // With no valid collision control all collision checks always fail
            if (TemplateData.Collision == null) return false;

            // Convert position from world space to entity space and transmit rotation
            var shiftedArea = area.Offset(-Position);
            return TemplateData.Collision.CollisionTest(shiftedArea, Rotation);
        }
        #endregion

        #region Path finding
        /// <summary>
        /// Perform movements queued up in pathfinding.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called. Must be positive!</param>
        private void UpdatePathfinding(double elapsedTime)
        {
            bool loop;
            Vector2 posDifference;
            do
            {
                // Get the position of the next target node
                Vector2 nextNodePos = CurrentPath.PathNodes.Peek();

                // Calculate the difference between the current position and the target
                posDifference = nextNodePos - Position;
                float differenceLength = posDifference.Length();

                // Calculate how much of the distance should be walked in this interval
                var movementFactor = (float)(elapsedTime * TemplateData.Movement.Speed / differenceLength);

                if (movementFactor >= 1)
                { // This move will skip past the current node
                    // Remove the node from the list
                    CurrentPath.PathNodes.Dequeue();

                    // Subtract the amount of time the rest of the distance to the node would have taken
                    elapsedTime -= differenceLength / TemplateData.Movement.Speed;

                    if (CurrentPath.PathNodes.Count == 0)
                    { // No further nodes, go to final target
                        // Calculate the difference for the rotation calculation below
                        posDifference = CurrentPath.Target - Position;

                        // Move the entity
                        Position = CurrentPath.Target;

                        // Prevent further calls of this method
                        CurrentPath = null;

                        loop = false;
                    }
                    else
                    { // Continue with next node
                        loop = true;
                    }
                }
                else
                { // We need to move a part of the way to the next node
                    // Move the entity
                    Position += posDifference * movementFactor;

                    loop = false;
                }
            } while (loop);

            // Make the entity face the direction it is walking in
            Rotation = ((float)Math.Atan2(posDifference.Y, posDifference.X)).RadianToDegree() - 90;
        }
        #endregion
    }
}
