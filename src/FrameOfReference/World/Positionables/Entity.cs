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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World;
using AlphaFramework.World.Components;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using FrameOfReference.World.Templates;
using NanoByte.Common;
using OmegaEngine.Foundation.Design;
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace FrameOfReference.World.Positionables;

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
    public float Rotation { get => _rotation; set => value.To(ref _rotation, OnChanged); }

    /// <summary>
    /// <c>true</c> if this entity is controlled by a human player, <c>false</c> if it is controlled by the computer.
    /// </summary>
    [XmlAttribute, DefaultValue(false), Description("true if this entity is controlled by a human player, false if it is controlled by the computer.")]
    public bool IsPlayerControlled { get; set; }

    /// <summary>
    /// The <see cref="Waypoints"/> index of the <see cref="Waypoint"/> this entity is currently moving towards; -1 for no <see cref="Waypoint"/>.
    /// </summary>
    [XmlAttribute, DefaultValue(-1), Browsable(false)]
    public int ActiveWaypointIndex { get; set; } = -1;

    /// <summary>
    /// The <see cref="Waypoint"/>s associated with this entity ordered by <see cref="Waypoint.ActivationTime"/>.
    /// </summary>
    [XmlElement, Browsable(false)]
    public List<Waypoint> Waypoints { get; } = [];

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
    /// <param name="gameTime">The current <see cref="IUniverse.GameTime"/> value.</param>
    /// <returns>The currently active <see cref="Waypoint"/>; <c>null</c> if none is active.</returns>
    public int GetCurrentWaypointIndex(double gameTime)
    {
        int index = Waypoints.FindLastIndex(x => x.ActivationTime <= gameTime);
        if (index == -1 || (Waypoints[index].ArrivalTimeSpecified && Waypoints[index].ArrivalTime <= gameTime)) return -1;
        return index;
    }

    #region Collision
    /// <summary>
    /// Determines whether a certain point collides with this entity (based on its <see cref="Collision{TCoordinates}"/> component).
    /// </summary>
    /// <param name="point">The point to check for collision in world space.</param>
    /// <returns><c>true</c> if the <paramref name="point"/> does collide with this entity, <c>false</c> otherwise.</returns>
    public bool CollisionTest(Vector2 point)
    {
        // With no valid collision control all collision checks always fail
        if (TemplateData?.Collision == null) return false;

        // Convert position from world space to entity space and transmit rotation
        var shiftedPoint = point - Position;
        return TemplateData.Collision.CollisionTest(shiftedPoint, Rotation);
    }

    /// <summary>
    /// Determines whether a certain area collides with this entity (based on its <see cref="Collision{TCoordinates}"/> component).
    /// </summary>
    /// <param name="area">The area to check for collision in world space.</param>
    /// <returns><c>true</c> if the <paramref name="area"/> does collide with this entity, <c>false</c> otherwise.</returns>
    public bool CollisionTest(Quadrangle area)
    {
        // With no valid collision control all collision checks always fail
        if (TemplateData?.Collision == null) return false;

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
        if (TemplateData?.Movement is not {} movement) return;

        bool loop;
        Vector2 posDifference;
        do
        {
            if (CurrentPath == null) return;

            // Get the position of the next target node
            Vector2 nextNodePos = CurrentPath.PathNodes.Peek();

            // Calculate the difference between the current position and the target
            posDifference = nextNodePos - Position;
            float differenceLength = posDifference.Length();

            // Calculate how much of the distance should be walked in this interval
            var movementFactor = (float)(elapsedTime * movement.Speed / differenceLength);

            if (movementFactor >= 1)
            { // This move will skip past the current node
                // Remove the node from the list
                CurrentPath.PathNodes.Dequeue();

                // Subtract the amount of time the rest of the distance to the node would have taken
                elapsedTime -= differenceLength / movement.Speed;

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

        // Make the entity face the direction it is walking in (unless it's the last step)
        if (CurrentPath != null)
            Rotation = ((float)Math.Atan2(posDifference.Y, posDifference.X)).RadianToDegree() - 90;
    }
    #endregion
}
