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
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Terrains;
using Common.Collections;
using Common.Utils;
using SlimDX;
using TerrainSample.World.Positionables;
using TerrainSample.World.Templates;

namespace TerrainSample.World
{
    /// <summary>
    /// Represents a world with a height-map based <see cref="Terrain"/>.
    /// </summary>
    public sealed partial class Universe : UniverseBase<Vector2>
    {
        private readonly MonitoredCollection<Positionable<Vector2>> _positionables = new MonitoredCollection<Positionable<Vector2>>();

        /// <inheritoc/>
        [Browsable(false)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        [XmlElement(typeof(Entity)), XmlElement(typeof(Water)), XmlElement(typeof(Waypoint)),
         XmlElement(typeof(BenchmarkPoint<Vector2>), ElementName = "BenchmarkPoint"),
         XmlElement(typeof(Memo<Vector2>), ElementName = "Memo")]
        public override MonitoredCollection<Positionable<Vector2>> Positionables { get { return _positionables; } }

        /// <summary>
        /// Retrieves an <see cref="Entity"/> from <see cref="Positionables"/> by its name.
        /// </summary>
        /// <returns>The first matching <see cref="Entity"/>; <see langword="null"/> if there is no match.</returns>
        public Entity GetEntity(string name)
        {
            return _positionables.OfType<Entity>().FirstOrDefault(x => x.Name == name);
        }

        private Terrain<TerrainTemplate> _terrain;

        /// <summary>
        /// The <see cref="Terrain"/> on which <see cref="EntityBase{TCoordinates,TTemplate}"/>s are placed.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="Terrain"/> could not be properly loaded from the file.</exception>
        /// <remarks>Is not serialized/stored, <see cref="TerrainSerialize"/> is used for that.</remarks>
        [Browsable(false)]
        public Terrain<TerrainTemplate> Terrain
        {
            get
            {
                if (_terrain != null && SourceFile != null && !_terrain.DataLoaded) LoadTerrainData();
                return _terrain;
            }
        }

        private int _maxTraversableSlope = 10;

        /// <summary>
        /// The maximum slope the <see cref="IPathfinder{TCoordinates}"/> considers traversable.
        /// </summary>
        [DefaultValue(10), Category("Gameplay"), Description("The maximum slope the Pathfinder considers traversable.")]
        public int MaxTraversableSlope { get { return _maxTraversableSlope; } set { Math.Abs(value).To(ref _maxTraversableSlope, SetupPathfinding); } }

        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public Universe()
        {}

        /// <summary>
        /// Creates a new <see cref="Universe"/> with a terrain.
        /// </summary>
        /// <param name="terrain">The terrain for the new <see cref="Universe"/>.</param>
        public Universe(Terrain<TerrainTemplate> terrain)
        {
            _terrain = terrain;
        }

        /// <inheritdoc/>
        public override void Update(double elapsedGameTime)
        {
            base.Update(elapsedGameTime);
            LightPhase += (float)(elapsedGameTime * LightPhaseSpeedFactor);
        }

        /// <inheritdoc/>
        protected override void Update(IUpdateable updateable, double elapsedGameTime)
        {
            var entity = updateable as Entity;

            // Recalculate paths lost due to XML serialization
            if (entity != null && entity.PathControl != null) StartMoving(entity, entity.PathControl.Target);

            base.Update(updateable, elapsedGameTime);
        }

        /// <summary>
        /// Makes a player-controlled <see cref="Entity"/> move towards a <paramref name="target"/>.
        /// </summary>
        public void PlayerMove(Entity entity, Vector2 target)
        {
            if (entity.IsNpc) return;

            StartMoving(entity, target);
        }
    }
}
