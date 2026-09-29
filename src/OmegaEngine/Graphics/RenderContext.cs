/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics;

/// <summary>
/// The kind of output a <see cref="View"/> renders its <see cref="Scene"/> into.
/// </summary>
internal enum RenderPass
{
    /// <summary>The regular appearance of the scene.</summary>
    Scene,

    /// <summary>The glow colors without any lighting, for <see cref="GlowView"/>s.</summary>
    Glow,

    /// <summary>The depth (Z-buffer) values as grayscale colors, for <see cref="DepthView"/>s.</summary>
    Depth
}

/// <summary>
/// Information about the render pass a <see cref="Renderable"/> is being rendered in.
/// </summary>
/// <param name="camera">Supplies information for the view transformation.</param>
/// <param name="pass">The kind of output being rendered.</param>
/// <param name="lights">A delegate that will be called to get lighting information; <c>null</c> if lighting is disabled.</param>
internal readonly struct RenderContext(Camera camera, RenderPass pass = RenderPass.Scene, GetEffectiveLights? lights = null)
{
    /// <summary>
    /// Supplies information for the view transformation.
    /// </summary>
    public Camera Camera { get; } = camera;

    /// <summary>
    /// The kind of output being rendered.
    /// </summary>
    public RenderPass Pass { get; } = pass;

    /// <summary>
    /// A delegate that will be called to get lighting information; <c>null</c> if lighting is disabled.
    /// </summary>
    public GetEffectiveLights? Lights { get; } = lights;

    /// <summary>
    /// Indicates whether lighting is enabled.
    /// </summary>
    public bool Lighting => Lights != null;

    /// <summary>
    /// Determines the surface effect to actually apply in this render pass.
    /// </summary>
    /// <param name="configured">The <see cref="PositionableRenderable.SurfaceEffect"/> configured for the renderable.</param>
    /// <param name="shaderAvailable">Indicates whether a <see cref="Shaders.SurfaceShader"/> is available for <see cref="SurfaceEffect.Shader"/>.</param>
    public SurfaceEffect GetSurfaceEffect(SurfaceEffect configured, bool shaderAvailable)
        => GetSurfaceEffect(configured, Pass, Lighting, shaderAvailable);

    /// <summary>
    /// Determines the surface effect to actually apply in a render pass.
    /// </summary>
    /// <param name="configured">The <see cref="PositionableRenderable.SurfaceEffect"/> configured for the renderable.</param>
    /// <param name="pass">The kind of output being rendered.</param>
    /// <param name="lighting">Indicates whether lighting is enabled.</param>
    /// <param name="shaderAvailable">Indicates whether a <see cref="Shaders.SurfaceShader"/> is available for <see cref="SurfaceEffect.Shader"/>.</param>
    /// <remarks>
    /// <see cref="RenderPass.Glow"/> and <see cref="RenderPass.Depth"/> override the <paramref name="configured"/> effect.
    /// Otherwise lit effects fall back to <see cref="SurfaceEffect.Plain"/> if lighting is disabled and <see cref="SurfaceEffect.Shader"/> falls back to <see cref="SurfaceEffect.FixedFunction"/> if there is no shader.
    /// </remarks>
    public static SurfaceEffect GetSurfaceEffect(SurfaceEffect configured, RenderPass pass, bool lighting, bool shaderAvailable)
        => pass switch
        {
            RenderPass.Glow => SurfaceEffect.Glow,
            RenderPass.Depth => SurfaceEffect.Depth,
            _ => configured switch
            {
                SurfaceEffect.FixedFunction or SurfaceEffect.Shader when !lighting => SurfaceEffect.Plain,
                SurfaceEffect.Shader when !shaderAvailable => SurfaceEffect.FixedFunction,
                _ => configured
            }
        };
}
