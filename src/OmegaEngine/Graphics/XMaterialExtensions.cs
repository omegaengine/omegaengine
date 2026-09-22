using OmegaEngine.Foundation.Light;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// Extension methods for <see cref="XMaterial"/>.
/// </summary>
public static class XMaterialExtensions
{
    /// <summary>
    /// Converts an <see cref="XMaterial"/> to a <see cref="Material"/>.
    /// </summary>
    /// <remarks>The colors are linearized, because the fixed-function pipeline shades in linear space and gamma-encodes on output.</remarks>
    public static Material ToD3DMaterial(this XMaterial material)
        => new()
        {
            Ambient = material.Ambient.SrgbToLinear(),
            Diffuse = material.Diffuse.SrgbToLinear(),
            Specular = material.Specular.SrgbToLinear(),
            Power = material.SpecularPower,
            Emissive = material.Emissive.SrgbToLinear()
        };

    private static readonly Color4 _unset = new(1, 0, 0, 0);

    /// <summary>
    /// Converts a <see cref="Material"/> to an <see cref="XMaterial"/>.
    /// </summary>
    public static XMaterial ToXMaterial(this Material material)
        => new(Diffuse: material.Diffuse.ToColor())
        {
            Ambient = material.Ambient == _unset ? XMaterial.Default.Ambient : material.Ambient.ToColor(),
            Specular = material.Specular == _unset ? XMaterial.Default.Specular : material.Specular.ToColor(),
            SpecularPower = material.Power == 1 ? XMaterial.Default.SpecularPower : material.Power,
            Emissive = material.Emissive.ToColor()
        };
}
