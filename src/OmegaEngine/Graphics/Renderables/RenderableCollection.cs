/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A collection of <see cref="PositionableRenderable"/>s that form one level of a render hierarchy.
/// </summary>
/// <param name="owner">The <see cref="PositionableRenderable"/> whose children this collection holds; <c>null</c> for the roots of a <see cref="Scene"/>.</param>
internal sealed class RenderableCollection(PositionableRenderable? owner = null) : EngineElementCollection<PositionableRenderable>
{
    /// <summary>
    /// The <see cref="PositionableRenderable"/> whose children this collection holds; <c>null</c> for the roots of a <see cref="Scene"/>.
    /// </summary>
    public PositionableRenderable? Owner => owner;

    /// <inheritdoc/>
    protected override void OnAdding(PositionableRenderable element)
    {
        #region Sanity checks
        if (owner != null && (element == owner || owner.IsDescendantOf(element)))
            throw new ArgumentException($"Adding {element} below {owner} would create a cycle in the render hierarchy.", nameof(element));
        #endregion

        // Elements live in exactly one collection, so adding is also a detach from the previous one
        element.Container?.Remove(element);
        element.Container = this;

        // Camera effects are leaf-only, so gaining a child changes the owner's own transform
        owner?.MarkLocalTransformDirty();
    }

    /// <inheritdoc/>
    protected override void OnRemoved(PositionableRenderable element)
    {
        if (element.Container == this) element.Container = null;

        owner?.MarkLocalTransformDirty();
    }
}
