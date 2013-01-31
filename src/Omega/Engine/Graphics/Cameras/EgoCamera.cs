/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine.Graphics.Cameras
{
    /// <summary>
    /// A classic ego-shooter camera (2D Terrain-locked movement, two-axis look rotation).
    /// </summary>
    public class EgoCamera : MatrixCamera
    {
        // ToDo: Implement 

        //--------------------//

        #region View control
        /// <inheritdoc/>
        public override void PerspectiveChange(float panX, float panY, float rotation, float zoom)
        {
            // ToDo: Implement 
        }
        #endregion
    }
}
