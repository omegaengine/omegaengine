/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace OmegaEngine.Collections
{
    /// <summary>
    /// An interface items must implement to be addable to <see cref="Pool{T}"/>. Poolable items directly store a reference to their successor.
    /// </summary>
    /// <typeparam name="T">The type of items to store in <see cref="Pool{T}"/>.</typeparam>
    public interface IPoolable<T> where T : class, IPoolable<T>
    {
        /// <summary>
        /// A reference to the next element in the <see cref="Pool{T}"/> chain.
        /// </summary>
        T NextElement { get; set; }
    }
}
