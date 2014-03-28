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
using Common.Collections;
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

        /// <summary>
        /// Update time left over from the last <see cref="Update"/> call due to the fixed update step size.
        /// </summary>
        private double _leftoverGameTime;

        /// <summary>
        /// Fixed step size for updates in seconds. Makes updates deterministic.
        /// </summary>
        private const float UpdateStepSize = 0.015f;

        /// <inheritdoc/>
        public override void Update(double elapsedGameTime)
        {
            _leftoverGameTime += elapsedGameTime;

            while (_leftoverGameTime >= UpdateStepSize)
            {
                base.Update(UpdateStepSize);
                LightPhase += UpdateStepSize * LightPhaseSpeedFactor;
                _leftoverGameTime -= UpdateStepSize;
            }
        }
    }
}
