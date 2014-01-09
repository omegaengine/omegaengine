/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Drawing;

namespace OmegaEngine.Graphics.Renderables
{
    /// <summary>
    /// A set of information about a particle in a particle system stored as a struct
    /// </summary>
    internal struct CpuParticleParametersStruct
    {
        /// <summary>
        /// How many seconds this particle will exist.
        /// Set to 0 never create. Set to <see cref="CpuParticleParameters.InfiniteFlag"/> for infinite
        /// </summary>
        public float LifeTime;

        /// <summary>
        /// How much the velocity will be reduced in one second as a value between 0 and 1
        /// </summary>
        public float Friction;

        /// <summary>
        /// The size of the particle
        /// </summary>
        public float Size;

        /// <summary>
        /// How much the particle will grow per second
        /// </summary>
        public float DeltaSize;

        /// <summary>
        /// The color of the particle
        /// </summary>
        public Color Color;

        /// <summary>
        /// How much the particle gets darker per second
        /// </summary>
        public float DeltaColor;
    }
}
