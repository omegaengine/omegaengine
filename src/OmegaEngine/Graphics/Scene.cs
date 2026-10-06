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
    /// When a <see cref="PositionableRenderable"/> is farther than this distance from the <see cref="Camera"/>, it is instead pulled in logarithmically beyond this distance, with corresponding scaling applied to preserve its apparent size (angular diameter). <c>null</c> to disable forced perspective.
    /// </summary>
    /// <remarks>
    /// <para>While enabled, renderables are not culled by <see cref="Camera.FarClip"/>.</para>
    /// <para>The surface is rendered at <c>d + r * (1 - exp(-d * ln(x / d) / r))</c>, where <c>d</c> is this distance, <c>x</c> the distance it actually has and <c>r</c> the room left up to <see cref="Camera.FarClip"/>. This is close to <c>d * (1 + ln(x / d))</c> while far short of <see cref="Camera.FarClip"/>, but approaches it instead of exceeding it. This keeps renderables at clearly different distances in order and squeezes even very large distances into the depth range up to <see cref="Camera.FarClip"/>.</para>
    /// <para>The order is not guaranteed for renderables whose depth ranges overlap: each subtree is scaled as a whole by a factor measured to its own nearest surface, so its far parts may end up behind another renderable that is actually behind them.</para>
    /// <para>Leave enough room between this value and <see cref="Camera.FarClip"/> of every <see cref="View"/> showing this scene, since that is the depth range the pulled-in renderables are spread across.</para>
    /// </remarks>
    public float? ForcedPerspectiveDistance { get; set; }
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
        // The caster spheres are still read at query time rather than snapshotted, since they are camera-dependent (billboarding, auto-scaling) and may change between views or during a view's render pass.
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
