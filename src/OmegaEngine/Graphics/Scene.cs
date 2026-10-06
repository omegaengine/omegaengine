/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace OmegaEngine.Graphics;

/// <summary>
/// Gets the effective light sources for a specific location (in range and potentially shadowed).
/// </summary>
/// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
/// <param name="shadowing">Whether to apply shadowing.</param>
/// <seealso cref="Scene.GetEffectiveLights"/>
public delegate IReadOnlyList<LightSource> GetEffectiveLights(BoundingSphere boundingSphere, bool shadowing);

/// <summary>
/// Represents a scene that can be viewed by a <see cref="Camera"/>.
/// </summary>
/// <remarks>Multiple <see cref="View"/>s can share one <see cref="Scene"/>.</remarks>
/// <seealso cref="View.Scene"/>
public sealed class Scene : EngineElement
{
    #region Properties
    private readonly RenderableCollection _positionables = new();

    /// <summary>
    /// The root <see cref="PositionableRenderable"/>s of this scene. Each of them may carry a tree of <see cref="PositionableRenderable.Children"/>.
    /// </summary>
    /// <remarks>
    /// <para>A renderable can only be part of one scene or parent at a time. Adding it here removes it from wherever it was before.</para>
    /// <para>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</para>
    /// </remarks>
    public ICollection<PositionableRenderable> Positionables => _positionables;

    /// <summary>
    /// The current <see cref="Skybox"/> for this scene
    /// </summary>
    /// <remarks>Will be disposed when <see cref="EngineElement.Dispose"/> is called.</remarks>
    public Skybox? Skybox
    {
        get => _skybox;
        set
        {
            UnregisterChild(_skybox);
            RegisterChild(_skybox = value);
        }
    }

    // Order is not important, duplicate entries are not allowed
    private readonly HashSet<LightSource> _lights = [];
    private Skybox? _skybox;

    /// <summary>
    /// All light sources affecting the entities in this scene
    /// </summary>
    public ICollection<LightSource> Lights => _lights;

    /// <summary>
    /// Renderables farther than this from the <see cref="Camera"/> are pulled in logarithmically and scaled down to preserve their apparent size. <c>null</c> to disable.
    /// </summary>
    /// <remarks>
    /// <para>Serves a similar purpose as a logarithmic depth buffer: very distant renderables fit within <see cref="Camera.FarClip"/> without sacrificing depth buffer precision. However, it scales entire top-level subtrees on the CPU instead of remapping depth per pixel.</para>
    /// <para>While enabled, renderables are not culled by <see cref="Camera.FarClip"/>. Leave enough room between this value and <see cref="Camera.FarClip"/>, since that is the depth range pulled-in renderables are spread across.</para>
    /// </remarks>
    public float? DistanceCompressionStart { get; set; }
    #endregion

    #region Constructor
    public Scene()
    {
        RegisterChild(_positionables);
    }
    #endregion

    #region Get effective lighting
    /// <summary>
    /// Gets the effective light sources for a specific location (in range and potentially shadowed).
    /// </summary>
    /// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
    /// <param name="shadowing">Whether to apply shadowing.</param>
    internal IReadOnlyList<LightSource> GetEffectiveLights(BoundingSphere boundingSphere, bool shadowing)
    {
        var lights = GetLights(boundingSphere);
        if (shadowing && Engine.Effects.Shadows)
            ApplyShadows(lights, boundingSphere);
        return lights;
    }

    /// <summary>
    /// Gets the light sources that are in range of a specific location.
    /// </summary>
    /// <param name="boundingSphere">The position and optional radius in floating world space of the target being lit.</param>
    private List<LightSource> GetLights(BoundingSphere boundingSphere)
    {
        var lights = new List<LightSource>(capacity: _lights.Count);
        foreach (var light in _lights)
        {
            switch (light)
            {
                case DirectionalLight:
                    lights.Add(light);
                    break;
                case PointLight point when point.IsInRange(boundingSphere):
                    lights.Add(point.RenderAsDirectional ? point.AsDirectional(boundingSphere.Center) : point);
                    break;
            }
        }
        return lights;
    }

    /// <summary>
    /// Applies shadows to light sources.
    /// </summary>
    /// <param name="lights">The set of light source to be modified.</param>
    /// <param name="receiverSphere">The bounding sphere of the shadow receiver in world space.</param>
    private void ApplyShadows(List<LightSource> lights, BoundingSphere receiverSphere)
    {
        if (receiverSphere.Radius == 0 || lights.Count == 0) return;

        // Iterates the flat caster list instead of the whole render hierarchy.
        // The caster spheres are still read at query time rather than snapshotted, since they are camera-dependent (billboarding, minimum screen size) and may change between views or during a view's render pass.
        foreach (var caster in GetShadowCasters())
        {
            if (caster.WorldBoundingSphere is { Radius: > 0.0001f } casterSphere && casterSphere != receiverSphere)
            {
                for (int i = 0; i < lights.Count; i++)
                    lights[i] = lights[i].GetShadowed(receiverSphere, casterSphere);
            }
        }
    }

    /// <summary>
    /// All <see cref="PositionableRenderable.ShadowCaster"/>s in the render hierarchy, in depth-first pre-order.
    /// </summary>
    /// <remarks>Includes bodies that are hidden or outside any view frustum, since those can still cast shadows into view.</remarks>
    private readonly List<PositionableRenderable> _shadowCasters = [];

    /// <summary>
    /// The <see cref="RenderableCollection.ShadowCastersVersion"/> <see cref="_shadowCasters"/> was collected for; <c>null</c> if never collected.
    /// </summary>
    private uint? _shadowCastersVersion;

    /// <summary>
    /// Returns <see cref="_shadowCasters"/>, recollecting it only if the render hierarchy or any <see cref="PositionableRenderable.ShadowCaster"/> flag changed since the last call.
    /// </summary>
    private List<PositionableRenderable> GetShadowCasters()
    {
        if (_shadowCastersVersion != _positionables.ShadowCastersVersion)
        {
            _shadowCasters.Clear();
            CollectShadowCasters(_positionables);
            _shadowCastersVersion = _positionables.ShadowCastersVersion;
        }
        return _shadowCasters;
    }

    private void CollectShadowCasters(RenderableCollection bodies)
    {
        foreach (var body in bodies)
        {
            if (body.ShadowCaster) _shadowCasters.Add(body);
            CollectShadowCasters(body.ChildCollection);
        }
    }
    #endregion
}
