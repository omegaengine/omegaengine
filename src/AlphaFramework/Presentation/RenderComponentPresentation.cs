using AlphaFramework.World.Components;
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.LightSources;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using LightSource = AlphaFramework.World.Components.LightSource;
using CpuParticleSystem = AlphaFramework.World.Components.CpuParticleSystem;

namespace AlphaFramework.Presentation;

/// <summary>
/// Converts <see cref="Render"/> components into <see cref="PositionableRenderable"/>s.
/// </summary>
public static class RenderComponentPresentation
{
    /// <summary>
    /// Creates a <see cref="PointLight"/> from a <see cref="LightSource"/> component.
    /// </summary>
    public static PointLight ToPresentation(this LightSource component, string? name = null)
        => new()
        {
            Name = name,
            Attenuation = component.Attenuation,
            Diffuse = component.Color,
            Shift = component.Shift
        };

    /// <summary>
    /// Creates a <see cref="Model"/> from a <see cref="Mesh"/> component.
    /// </summary>
    /// <returns>The presentation; <c>null</c> if the component configuration is incomplete.</returns>
    public static Model? ToPresentation(this Mesh component, Engine engine, string? name = null)
    {
        if (string.IsNullOrEmpty(component.Filename)) return null;

        var presentation = new Model(XMesh.Get(engine, component.Filename)) {Name = name};
        ApplyProperties(component, presentation);
        return presentation;
    }

    /// <summary>
    /// Creates a <see cref="AnimatedModel"/> from an <see cref="AnimatedMesh"/> component.
    /// </summary>
    /// <returns>The presentation; <c>null</c> if the component configuration is incomplete.</returns>
    public static AnimatedModel? ToPresentation(this AnimatedMesh component, Engine engine, string? name = null)
    {
        if (string.IsNullOrEmpty(component.Filename)) return null;

        var presentation = new AnimatedModel(XAnimatedMesh.Get(engine, component.Filename)) {Name = name};
        ApplyProperties(component, presentation);
        return presentation;
    }

    private static void ApplyProperties(Mesh component, PositionableRenderable presentation)
    {
        presentation.PreTransform = Matrix.Scaling(component.Scale, component.Scale, component.Scale) *
                                    Matrix.RotationYawPitchRoll(
                                        component.RotationY.DegreeToRadian(),
                                        component.RotationX.DegreeToRadian(),
                                        component.RotationZ.DegreeToRadian()) *
                                    Matrix.Translation(component.Shift);
        presentation.Alpha = component.Alpha;
        presentation.Pickable = component.Pickable;
        presentation.RenderIn = (OmegaEngine.Graphics.Renderables.ViewType)component.RenderIn;
        presentation.ShadowCaster = component.ShadowCaster;
        presentation.ShadowReceiver = component.ShadowReceiver;
    }

    /// <summary>
    /// Creates a <see cref="Model"/> from a <see cref="TestSphere"/> component.
    /// </summary>
    public static Model ToPresentation(this TestSphere component, Engine engine, string? name = null)
    {
        var presentation = Model.Sphere(engine, XTexture.Get(engine, component.Texture), component.Radius, component.Slices, component.Stacks);
        presentation.Name = name;
        presentation.PreTransform = Matrix.Translation(component.Shift);
        presentation.Alpha = component.Alpha;
        return presentation;
    }

    /// <summary>
    /// Creates a <see cref="OmegaEngine.Graphics.Renderables.CpuParticleSystem"/> from a <see cref="CpuParticleSystem"/> component.
    /// </summary>
    /// <returns>The presentation; <c>null</c> if the component configuration is incomplete.</returns>
    public static OmegaEngine.Graphics.Renderables.CpuParticleSystem? ToPresentation(this CpuParticleSystem component, string? name = null)
    {
        if (string.IsNullOrEmpty(component.Filename)) return null;

        return new()
        {
            Name = name,
            Preset = CpuParticlePreset.FromContent(component.Filename),
            LocalSpace = component.LocalSpace,
            PreTransform = Matrix.Translation(component.Shift)
        };
    }
}
