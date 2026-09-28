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

    /// <summary>
    /// Incremented whenever the set of <see cref="PositionableRenderable.ShadowCaster"/>s in the hierarchy rooted at this collection may have changed.
    /// </summary>
    /// <remarks>Only maintained for the roots of a <see cref="Scene"/> (<see cref="Owner"/> is <c>null</c>); changes further down are forwarded there.</remarks>
    public uint ShadowCastersVersion { get; private set; }

    /// <summary>
    /// Signals that bodies were added or removed at any depth below this collection or that a body's <see cref="PositionableRenderable.ShadowCaster"/> flag changed.
    /// </summary>
    public void MarkShadowCastersDirty()
    {
        if (owner == null) unchecked { ShadowCastersVersion++; }
        else owner.Container?.MarkShadowCastersDirty();
    }

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
        MarkShadowCastersDirty();
    }

    /// <inheritdoc/>
    protected override void OnRemoved(PositionableRenderable element)
    {
        if (element.Container == this) element.Container = null;

        owner?.MarkLocalTransformDirty();
        MarkShadowCastersDirty();
    }
}
