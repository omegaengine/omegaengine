/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine.Graphics.Cameras;

/// <summary>
/// A freely rotateable and moveable camera, like flying through space.
/// </summary>
public class SpaceCamera : QuaternionCamera
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
