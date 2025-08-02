/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SlimDX.Direct3D9;
using OmegaEngine.Assets;

namespace OmegaEngine.Graphics;

/// <summary>
/// An object that can provide a <see cref="Texture"/> for rendering.
/// </summary>
/// <remarks>
/// This provides a common interface for static (unchanging) <see cref="XTexture"/> assets and dynamic <see cref="RenderTarget"/>s.
/// </remarks>
public interface ITextureProvider : IReferenceCount
{
    /// <summary>
    /// The <see cref="SlimDX.Direct3D9.Texture"/> this object represents.
    /// </summary>
    Texture Texture { get; }
}
