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
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using FrameOfReference.World.Positionables;
using FrameOfReference.World.Templates;
using NanoByte.Common.Collections;
using SlimDX;

namespace FrameOfReference.World
{
    /// <summary>
    /// Represents a world with a height-map based <see cref="Terrain"/>.
    /// </summary>
    public sealed partial class Universe : UniverseBase<Vector2>
    {
        private readonly MonitoredCollection<Positionable<Vector2>> _positionables = [];

        /// <inheritdoc/>
        [Browsable(false)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        [XmlElement(typeof(Entity)), XmlElement(typeof(Water)),
         XmlElement(typeof(Waypoint)), XmlElement(typeof(Trigger)),
         XmlElement(typeof(CameraState<Vector2>), ElementName = "CameraState"), XmlElement(typeof(BenchmarkPoint<Vector2>), ElementName = "BenchmarkPoint")]
        public override MonitoredCollection<Positionable<Vector2>> Positionables => _positionables;

        /// <summary>
        /// The <see cref="Terrain"/> on which <see cref="Entity"/>s are placed.
        /// </summary>
        /// <exception cref="InvalidOperationException">The <see cref="Terrain"/> could not be properly loaded from the file.</exception>
        /// <remarks>Is not serialized/stored, <see cref="TerrainSerialize"/> is used for that.</remarks>
        [XmlIgnore, Browsable(false)]
        public Terrain<TerrainTemplate> Terrain
        {
            get
            {
                if (TerrainSerialize != null && SourceFile != null && !TerrainSerialize.DataLoaded) LoadTerrainData();
                return TerrainSerialize;
            }
            set { TerrainSerialize = value; }
        }

        /// <summary>
        /// Creates a new <see cref="Universe"/> with a terrain.
        /// </summary>
        /// <param name="terrain">The terrain for the new <see cref="Universe"/>.</param>
        public Universe(Terrain<TerrainTemplate> terrain)
        {
            TerrainSerialize = terrain;
        }

        #region Update
        /// <inheritdoc/>
        public override void Update(double elapsedGameTime)
        {
            WrapWaypoints();
            base.Update(elapsedGameTime);
            LightPhase += (float)(elapsedGameTime * LightPhaseSpeedFactor);
        }

        /// <inheritdoc/>
        protected override void Update(IUpdateable updateable, double elapsedGameTime)
        {
            var entity = updateable as Entity;
            if (entity != null) HandleWaypoints(entity, elapsedGameTime);

            base.Update(updateable, elapsedGameTime);
        }
        #endregion

        #region Entities
        /// <summary>
        /// Retrieves an <see cref="Entity"/> from <see cref="Positionables"/> by its name.
        /// </summary>
        /// <returns>The first matching <see cref="Entity"/>; <c>null</c> if there is no match.</returns>
        public Entity GetEntity(string name)
        {
            return _positionables.OfType<Entity>().FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Retrieves an <see cref="Trigger"/> from <see cref="Positionables"/> by its name.
        /// </summary>
        /// <returns>The first matching <see cref="Trigger"/>; <c>null</c> if there is no match.</returns>
        public Trigger GetTrigger(string name)
        {
            return _positionables.OfType<Trigger>().FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Retrieves an <see cref="CameraState{TCoordinates}"/> from <see cref="Positionables"/> by its name.
        /// </summary>
        /// <returns>The first matching <see cref="CameraState{TCoordinates}"/>; <c>null</c> if there is no match.</returns>
        public CameraState<Vector2> GetCamera(string name)
        {
            return _positionables.OfType<CameraState<Vector2>>().FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Makes a player-controlled <see cref="Entity"/> move towards a <paramref name="target"/>.
        /// </summary>
        public void PlayerMove(Entity entity, Vector2 target)
        {
            #region Sanity checks
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            #endregion

            if (!entity.IsPlayerControlled || entity.TemplateData.Movement == null) return;

            entity.Waypoints.Add(new() {TargetEntity = entity.Name, ActivationTime = GameTime, Position = target, OriginPosition = entity.Position});
            entity.ActiveWaypointIndex = entity.Waypoints.Count - 1;

            StartMoving(entity, target);
        }

        /// <summary>
        /// Turns a specific <see cref="Entity"/> into a player-controlled character.
        /// </summary>
        public void MakePlayerControlled(string name)
        {
            var entity = GetEntity(name);

            // Remove any recorded paths
            entity.Waypoints.RemoveAll(x => (x.ArrivalTimeSpecified ? x.ArrivalTime : x.ActivationTime) >= GameTime);
            entity.CurrentPath = null;

            // Reactivates all associated triggers
            foreach (var trigger in Positionables.OfType<Trigger>()
                .Where(x => x.TargetEntity == name && x.DueTime > GameTime))
                trigger.WasTriggered = false;

            entity.IsPlayerControlled = true;
        }

        /// <summary>
        /// Turns all <see cref="Entity"/>s into NPCs.
        /// </summary>
        public void MakeAllNpc()
        {
            foreach (var entity in Positionables.OfType<Entity>().Where(x => x.IsPlayerControlled))
            {
                entity.IsPlayerControlled = false;

                // Prevent characters from finishing last pathfinding command after player control was removed
                foreach (var openWaypoint in entity.Waypoints.Where(x => !x.ArrivalTimeSpecified))
                {
                    openWaypoint.ArrivalTime = GameTime;
                    openWaypoint.Position = entity.Position;
                }
            }
        }
        #endregion
    }
}
