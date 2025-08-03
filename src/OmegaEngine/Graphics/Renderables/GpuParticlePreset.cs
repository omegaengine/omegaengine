/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using NanoByte.Common;
using SlimDX;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Storage;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A set of information about a particle system as a whole
/// </summary>
[XmlRoot("GpuParticlePreset")] // Suppress XMLSchema declarations (no inheritance used for properties)
public class GpuParticlePreset : ICloneable
{
    #region Variables
    private string _particleTexture = "Flame.tga";

    /// <summary>
    /// Flag indicating that <see cref="ParticleShader.ParticleTexture"/> has changed and needs to be reloaded
    /// </summary>
    internal bool TextureDirty;
    #endregion

    #region Properties
    /// <summary>
    /// The largest distance from the emitter at which particle shall be spawned
    /// </summary>
    [DefaultValue(1f), Description("The largest distance from the emitter at which particle shall be spawned")]
    public float SpawnRadius { get; set; } = 1;

    /// <summary>
    /// The largest distance from the emitter particles can travel before dying
    /// </summary>
    [DefaultValue(80f), Description("The largest distance from the emitter particles can travel before dying")]
    public float SystemHeight { get; set; } = 80;

    /// <summary>
    /// The direction and speed with which the particles move
    /// </summary>
    [Description("The direction and speed with which the particles move")]
    public Vector3 Movement { get; set; } = new(0, 0.48f, 0);

    /// <summary>
    /// How to spread the particles
    /// </summary>
    [DefaultValue(20f), Description("How to spread the particles")]
    public float ParticleSpread { get; set; } = 20;

    /// <summary>
    /// The size of the particles
    /// </summary>
    [DefaultValue(7.8f), Description("The size of the particles")]
    public float ParticleSize { get; set; } = 7.8f;

    /// <summary>
    /// The shape of the particles
    /// </summary>
    [DefaultValue(0.38f), Description("The shape of the particles")]
    public float ParticleShape { get; set; } = 0.38f;

    /// <summary>
    /// The ID of the texture to use for color lookup
    /// </summary>
    [DefaultValue("Flame.tga"), Description("The ID of the texture to use for color lookup")]
    public string ParticleTexture { get => _particleTexture; set => value.To(ref _particleTexture, ref TextureDirty); }
    #endregion

    //--------------------//

    #region Storage
    /// <summary>
    /// Loads a preset from an XML file via the <see cref="ContentManager"/>.
    /// </summary>
    /// <param name="id">The ID of the XML file to load</param>
    /// <returns>The loaded preset</returns>
    /// <exception cref="FileNotFoundException">The specified file could not be found.</exception>
    /// <exception cref="IOException">There was an error reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidOperationException">A problem occurred while deserializing the XML data.</exception>
    public static GpuParticlePreset FromContent(string id)
    {
        using var stream = ContentManager.GetFileStream("Graphics/GpuParticleSystem", id);
        return XmlStorage.LoadXml<GpuParticlePreset>(stream);
    }
    #endregion

    #region Clone
    /// <summary>
    /// Creates a deep copy of the this particle system preset
    /// </summary>
    /// <returns>The cloned preset</returns>
    public GpuParticlePreset Clone()
    {
        var newPreset = (GpuParticlePreset)MemberwiseClone();
        return newPreset;
    }

    object ICloneable.Clone() => Clone();
    #endregion
}
