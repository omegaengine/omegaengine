using OmegaEngine.Foundation.Light;
using System.Numerics;
using Vortice.Mathematics;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Extension methods for <see cref="XMaterial"/>.
/// </summary>
public static class XMaterialExtensions
{
    /// <summary>
    /// Converts an <see cref="XMaterial"/> to a <see cref="Material9"/>.
    /// </summary>
    /// <remarks>The colors are linearized, because the fixed-function pipeline shades in linear space and gamma-encodes on output.</remarks>
    public static Material9 ToD3DMaterial(this XMaterial material)
        => new()
        {
            Ambient = material.Ambient.SrgbToLinear().ToColorvalue(),
            Diffuse = material.Diffuse.SrgbToLinear().ToColorvalue(),
            Specular = material.Specular.SrgbToLinear().ToColorvalue(),
            Power = material.SpecularPower,
            Emissive = material.Emissive.SrgbToLinear().ToColorvalue()
        };

    private static readonly Color4 _unset = new(0, 0, 0, 1);

    /// <summary>
    /// Converts a <see cref="Material9"/> to an <see cref="XMaterial"/>.
    /// </summary>
    public static XMaterial ToXMaterial(this Material9 material)
        => new(Diffuse: material.Diffuse.ToColor4().ToColor())
        {
            Ambient = material.Ambient.ToColor4() == _unset ? XMaterial.Default.Ambient : material.Ambient.ToColor4().ToColor(),
            Specular = material.Specular.ToColor4() == _unset ? XMaterial.Default.Specular : material.Specular.ToColor4().ToColor(),
            SpecularPower = material.Power == 1 ? XMaterial.Default.SpecularPower : material.Power,
            Emissive = material.Emissive.ToColor4().ToColor()
        };
}
