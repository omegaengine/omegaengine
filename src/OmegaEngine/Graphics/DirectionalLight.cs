/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using SlimDX;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// A light source that has no position and shines in one direction.
    /// </summary>
    public sealed class DirectionalLight : LightSource
    {
        #region Properties
        private Vector3 _direction;

        /// <summary>
        /// The direction of the light source
        /// </summary>
        [Description("The direction of the light source"), Category("Layout")]
        public Vector3 Direction { get { return _direction; } set { _direction = Vector3.Normalize(value); } }
        #endregion
    }
}
