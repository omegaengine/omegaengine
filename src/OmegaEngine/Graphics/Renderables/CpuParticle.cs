/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Values;
using OmegaEngine.Collections;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Values;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A particle used by the <see cref="CpuParticleSystem"/>
    /// </summary>
    internal class CpuParticle : IPoolable<CpuParticle>, IPositionable
    {
        #region Properties
        /// <summary>
        /// A reference to the next element in the <see cref="Pool{T}"/> chain.
        /// </summary>
        CpuParticle IPoolable<CpuParticle>.NextElement { get; set; }

        #region Flags
        /// <summary>
        /// Is this particle currently alive/visible?
        /// </summary>
        public bool Alive { get; set; }

        /// <summary>
        /// Is this particle currently in its second life?
        /// </summary>
        public bool SecondLife { get; set; }
        #endregion

        #region Data
        /// <summary>
        /// The particle's position in world space
        /// </summary>
        public DoubleVector3 Position { get; set; }

        /// <summary>
        /// The movement of this particle in one second
        /// </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>
        /// The current color of this particle
        /// </summary>
        public Color4 Color { get; set; }

        /// <summary>
        /// The current size of the particle
        /// </summary>
        public float Size { get; private set; }
        #endregion

        #region Parameters
        /// <summary>
        /// The initial configuration of this particle
        /// </summary>
        public CpuParticleParametersStruct Parameters1;

        /// <summary>
        /// The configuration this particle will take in its "second life"
        /// </summary>
        public CpuParticleParametersStruct Parameters2;
        #endregion

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new particle
        /// </summary>
        /// <param name="position">The initial position of the particle</param>
        /// <param name="parameters1">The initial configuration of this particle</param>
        /// <param name="parameters2">The configuration this particle will take in its "second life"</param>
        public CpuParticle(DoubleVector3 position, CpuParticleParametersStruct parameters1, CpuParticleParametersStruct parameters2)
        {
            Alive = true;
            Position = position;
            Color = new(parameters1.Color);

            Parameters1 = parameters1;
            Parameters2 = parameters2;
        }
        #endregion

        //--------------------//

        #region Update
        /// <summary>
        /// Update the status of the particle
        /// </summary>
        /// <param name="elapsedTime">The number of seconds elapsed</param>
        internal void Update(float elapsedTime)
        {
            if (SecondLife)
            {
                Parameters2.LifeTime -= elapsedTime;

                if (Parameters2.LifeTime <= 0)
                { // Handle dead particles
                    Alive = false;
                }
            }
                // ReSharper disable once CompareOfFloatsByEqualityOperator
            else if (Parameters1.LifeTime != -32768)
            {
                Parameters1.LifeTime -= elapsedTime;

                if (Parameters1.LifeTime <= 0)
                {
                    if (Parameters2.LifeTime > 0)
                    { // Handle second-life particles
                        SecondLife = true;
                    }
                    else
                    { // Handle dead particles
                        Alive = false;
                        return;
                    }
                }
            }

            // Apply friction
            Velocity -= Velocity * (SecondLife ? Parameters2.Friction : Parameters1.Friction) * elapsedTime;

            // Update position
            Position += Velocity * elapsedTime;

            // Update size
            if (SecondLife)
            {
                Parameters2.Size += Parameters2.DeltaSize * elapsedTime;
                if (Parameters2.Size < 0) Parameters2.Size = 0;
            }
            else
            {
                Parameters1.Size += Parameters1.DeltaSize * elapsedTime;
                if (Parameters1.Size < 0) Parameters1.Size = 0;
            }
            Size = SecondLife ? Parameters2.Size : Parameters1.Size;

            // Update color
            float currentDeltaColor = (SecondLife ? Parameters2.DeltaColor : Parameters1.DeltaColor) * elapsedTime;
            Color = new(
                (Color.Red - currentDeltaColor).Clamp(),
                (Color.Green - currentDeltaColor).Clamp(),
                (Color.Blue - currentDeltaColor).Clamp());
        }
        #endregion

        #region Render
        /// <summary>
        /// Called internally when this particle needs to be rendered
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to use to render this particle</param>
        /// <param name="camera">Supplies information for the view transformation</param>
        internal void Render(Engine engine, Camera camera)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (camera == null) throw new ArgumentNullException(nameof(camera));
            #endregion

            // Calculate the world transform
            engine.State.WorldTransform =
                Matrix.Scaling(new(Size)) *
                camera.SphericalBillboard *
                Matrix.Translation(Position.ApplyOffset(camera.PositionBase));

            // Set the particle color
            var material = new Material {Emissive = Color};
            engine.Device.Material = material;

            // Render the particle using the externally set stream source
            engine.Device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }
        #endregion
    }
}
