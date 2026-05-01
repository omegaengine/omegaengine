/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine;

/// <summary>
/// Entity that needs to have state reset before each frame.
/// </summary>
internal interface IFrameResettable
{
    /// <summary>
    /// Resets state in preparation for the next frame.
    /// </summary>
    void FrameReset();
}
