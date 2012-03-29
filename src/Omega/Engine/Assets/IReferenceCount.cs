/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine.Assets
{
    /// <summary>
    /// Represents an object that tracks whether it is still needed by increasing and decreasing a reference counter.
    /// </summary>
    public interface IReferenceCount
    {
        /// <summary>
        /// Increments the reference count by one.
        /// </summary>
        void HoldReference();

        /// <summary>
        /// Decrements the reference count by one.
        /// </summary>
        void ReleaseReference();
    }
}
