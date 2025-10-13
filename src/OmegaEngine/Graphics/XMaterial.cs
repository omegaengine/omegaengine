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
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics;

/// <summary>
/// A combination of textures and lighting parameters.
/// </summary>
/// <remarks><see cref="XMaterial"/> instances are auto-generated when <see cref="XMesh"/>es are loaded.</remarks>
public struct XMaterial
{
    /// <summary>
    /// The diffuse textures maps
    /// </summary>
    public readonly ITextureProvider?[] DiffuseMaps;

    /// <summary>
    /// Indicates whether this material uses textures
    /// </summary>
    public bool IsTextured => DiffuseMaps is [not null, ..];

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
    public Color Ambient { get; set; }

    /// <summary>
    /// The color of the material when hit by diffuse light (normal lighting)
    /// </summary>
    [DefaultValue(typeof(Color), "White")]
    public Color Diffuse { get; set; }

    /// <summary>
    /// The color of specular (shiny) highlights on the material surface
    /// </summary>
    [DefaultValue(typeof(Color), "Gray")]
    public Color Specular { get; set; }

    /// <summary>
    /// The sharpness of specular highlights (lower value = sharper)
    /// </summary>
    [DefaultValue(1f)]
    public float SpecularPower { get; set; }

    /// <summary>
    /// The color of the light this material emits by itself - doubles as glow color
    /// </summary>
    [DefaultValue(typeof(Color), "Black")]
    public Color Emissive { get; set; }

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

    /// <summary>
    /// The emissive (self-shining, without light source) - doubles as the glow map
    /// </summary>
    public ITextureProvider? EmissiveMap { get; set; }

    public XMaterial(Color color) : this()
    {
        DiffuseMaps = new ITextureProvider[16];
        NormalMap = null;
        HeightMap = null;
        SpecularMap = null;
        EmissiveMap = null;

        Diffuse = color;
        Ambient = Color.White;
        Specular = Color.Gray;
        SpecularPower = 1;
        Emissive = Color.Black;
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
    public void HoldReference()
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
