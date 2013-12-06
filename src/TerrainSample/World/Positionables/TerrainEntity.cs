/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.ComponentModel;
using System.Xml.Serialization;
using Common.Utils;
using Common.Values;
using SlimDX;
using World.Pathfinding;
using World.Terrains;

namespace World.Positionables
{
    /// <summary>
    /// An <see cref="Entity{TCoordinates}"/> that can be placed on a <see cref="Terrain"/>.
    /// </summary>
    public class TerrainEntity : Entity<Vector2>
    {
        #region Properties
        /// <inheritdoc/>
        [Browsable(false)]
        [XmlElement(typeof(PathLeader<Vector2>)), XmlElement(typeof(PathFollower<Vector2>))]
        public override PathControl<Vector2> PathControl { get; set; }
        #endregion

        //--------------------//

        #region Collision
        /// <inheritdoc/>
        public override bool CollisionTest(Vector2 point)
        {
            // With no valid collision control all collision checks always fail
            if (TemplateData.CollisionControl == null) return false;

            // Convert position from world space to entity space and transmit rotation
            var shiftedPoint = point - Position;
            return TemplateData.CollisionControl.CollisionTest(shiftedPoint, Rotation);
        }

        /// <inheritdoc/>
        public override bool CollisionTest(Quadrangle area)
        {
            // With no valid collision control all collision checks always fail
            if (TemplateData.CollisionControl == null) return false;

            // Convert position from world space to entity space and transmit rotation
            var shiftedArea = area.Offset(-Position);
            return TemplateData.CollisionControl.CollisionTest(shiftedArea, Rotation);
        }
        #endregion

        #region Path finding
        /// <inheritdoc/>
        public override Vector2[] GetPathFindingOutline()
        {
            // With no valid collision control the outline is empty
            if (TemplateData.CollisionControl == null) return new Vector2[0];

            // Transmit rotation
            Vector2[] outline = TemplateData.CollisionControl.GetPathFindingOutline(Rotation);

            // Convert positions from entity space to world space
            for (int i = 0; i < outline.Length; i++)
                outline[i] += Position;

            return outline;
        }

        /// <inheritdoc/>
        public override void UpdatePosition(double elapsedTime)
        {
            #region Sanity checks
            if (TemplateData.MovementControl == null) return;
            #endregion

            var leader = PathControl as PathLeader<Vector2>;
            if (leader != null && leader.PathNodes.Count > 0)
            { // This entity performs its own pathfinding

                #region Leader
                bool loop;
                Vector2 posDifference;
                do
                {
                    // Get the position of the next target node
                    Vector2 nextNodePos = leader.PathNodes.Peek();

                    // Calculate the difference between the current position and the target
                    posDifference = nextNodePos - Position;
                    float differenceLength = posDifference.Length();

                    // Calculate how much of the distance should be walked in this interval
                    var movementFactor = (float)(elapsedTime * TemplateData.MovementControl.Speed / differenceLength);

                    if (movementFactor >= 1)
                    { // This move will skip past the current node
                        // Remove the node from the list
                        leader.PathNodes.Pop();

                        // Subtract the amount of time the rest of the distance to the node would have taken
                        elapsedTime -= differenceLength / TemplateData.MovementControl.Speed;

                        if (leader.PathNodes.Count == 0)
                        { // No further nodes, go to final target
                            // Calculate the difference for the rotation calculation below
                            posDifference = leader.Target - Position;

                            // Move the entity
                            Position = leader.Target;

                            // Prevent further calls of this method
                            PathControl = null;

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
                #endregion
            }
            else
            {
                var follower = PathControl as PathFollower<Vector2>;
                if (follower != null)
                { // This entity follows another entity for pathfinding

                    #region Follower
                    // ToDo: Implement
                    #endregion
                }
            }
        }
        #endregion
    }
}
