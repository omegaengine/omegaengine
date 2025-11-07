/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing;
using OmegaEngine.Assets;
using OmegaEngine.Foundation.Light;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// A combination of textures and lighting parameters.
/// </summary>
/// <param name="color">The color of the material when hit by diffuse light (normal lighting).</param>
/// <remarks><see cref="XMaterial"/> instances are auto-generated when <see cref="XMesh"/>es are loaded.</remarks>
public struct XMaterial(Color color)
{
    /// <summary>
    /// The diffuse textures maps
    /// </summary>
    public readonly ITextureProvider?[] DiffuseMaps = new ITextureProvider[16];

    /// <summary>
    /// Indicates whether this material uses textures
    /// </summary>
    public bool IsTextured => DiffuseMaps is [not null, ..] || EmissiveMap != null;

    /// <summary>
    /// A plain white untextured material
    /// </summary>
    public static XMaterial DefaultMaterial => new(Color.White);

    /// <summary>
    /// A DirectX material with the color information from this <see cref="XMaterial"/>
    /// </summary>
    public Material D3DMaterial => new() {Ambient = Ambient, Diffuse = Diffuse, Specular = Specular, Power = SpecularPower, Emissive = Emissive};

    /// <summary>
    /// The color of the material when lit by ambient/background light (always active)
    /// </summary>
    [DefaultValue(typeof(Color), "White")]
    public Color Ambient { get; set; } = Color.White;

    /// <summary>
    /// The color of the material when hit by diffuse light (normal lighting)
    /// </summary>
    [DefaultValue(typeof(Color), "White")]
    public Color Diffuse { get; set; } = color;

    /// <summary>
    /// The color of specular (shiny) highlights on the material surface
    /// </summary>
    [DefaultValue(typeof(Color), "Gray")]
    public Color Specular { get; set; } = Color.Gray;

    /// <summary>
    /// The sharpness of specular highlights (lower value = sharper)
    /// </summary>
    [DefaultValue(1f)]
    public float SpecularPower { get; set; } = 1;

    /// <summary>
    /// The color of the light this material emits by itself
    /// </summary>
    [DefaultValue(typeof(Color), "Black")]
    public Color Emissive { get; set; } = Color.Black;

    /// <summary>
    /// A shader-specific parameter controlling the application of emissive lighting (usually how much emissive light is muted by diffuse and specular light)
    /// </summary>
    [DefaultValue(0f)]
    public float EmissiveFactor { get; set; } = 0;

    /// <summary>
    /// The color of this material glows in, bleeding over edges
    /// </summary>
    [DefaultValue(typeof(Color), "Black")]
    public Color Glow { get; set; } = Color.Black;

    /// <summary>
    /// The normal map (i.e. bump map)
    /// </summary>
    public ITextureProvider? NormalMap { get; set; }

    /// <summary>
    /// The height map (an optional addition to the normal map)
    /// </summary>
    public ITextureProvider? HeightMap { get; set; }

    /// <summary>
    /// Indicates whether this material requires TBN (tangent, bitangent, normal) vectors
    /// </summary>
    public bool NeedsTBN => NormalMap != null || HeightMap != null;

    /// <summary>
    /// The specular map (which spots are "shiny")
    /// </summary>
    public ITextureProvider? SpecularMap { get; set; }

    private ITextureProvider? _emissiveMap;

    /// <summary>
    /// The emissive (which spots are self-shining without an external light source)
    /// </summary>
    public ITextureProvider? EmissiveMap
    {
        readonly get => _emissiveMap;
        set
        {
            _emissiveMap = value;

            // Default color to white when texture is set, as a convenience
            if (Emissive.EqualsIgnoreAlpha(Color.Black) && value != null)
                Emissive = Color.White;
        }
    }

    private ITextureProvider? _glowMap;

    /// <summary>
    /// The glow map (which spots glow, bleeding over edges)
    /// </summary>
    public ITextureProvider? GlowMap
    {
        readonly get => _glowMap;
        set
        {
            _glowMap = value;

            // Default color to white when texture is set, as a convenience
            if (Glow.EqualsIgnoreAlpha(Color.Black) && value != null)
                Glow = Color.White;
        }
    }

    public XMaterial(ITextureProvider? diffuse) : this(Color.White)
    {
        DiffuseMaps = new ITextureProvider[16];
        DiffuseMaps[0] = diffuse;
    }

    public static implicit operator XMaterial(XTexture texture) => new(texture);

    /// <summary>
    /// Calls <see cref="Asset.HoldReference"/> for all contained <see cref="XTexture"/>s.
    /// </summary>
    public readonly void HoldReference()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (DiffuseMaps != null)
        {
            foreach (var texture in DiffuseMaps)
                texture?.HoldReference();
        }

        NormalMap?.HoldReference();
        HeightMap?.HoldReference();
        SpecularMap?.HoldReference();
        EmissiveMap?.HoldReference();
    }

    /// <summary>
    /// Calls <see cref="Asset.ReleaseReference"/> for all contained <see cref="XTexture"/>s.
    /// </summary>
    public void ReleaseReference()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (DiffuseMaps != null)
        {
            foreach (var texture in DiffuseMaps)
                texture?.ReleaseReference();
        }

        NormalMap?.ReleaseReference();
        HeightMap?.ReleaseReference();
        SpecularMap?.ReleaseReference();
        EmissiveMap?.ReleaseReference();
    }
}
