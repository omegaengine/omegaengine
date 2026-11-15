/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using OmegaEngine.Assets;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// A post-screen shader that creates "scratched film" effect.
/// </summary>
public class PostScratchedFilmShader : PostShader
{
    #region Variables
    private XTexture? _noiseTexture;
    #endregion

    #region Properties
    /// <summary>
    /// The horizontal speed of the scratches
    /// </summary>
    [DefaultValue(0.03f), Description("The horizontal speed of the scratches")]
    public float Speed { get; set => value.To(ref field, () => SetShaderParameter("Speed", value)); } = 0.03f;

    /// <summary>
    /// The vertical speed of the scratches
    /// </summary>
    [DefaultValue(0.02f), Description("The vertical speed of the scratches")]
    public float Speed2 { get; set => value.To(ref field, () => SetShaderParameter("Speed2", value)); } = 0.02f;

    /// <summary>
    /// The number of scratches
    /// </summary>
    [DefaultValue(0.65f), Description("The number of scratches")]
    public float ScratchIntensity { get; set => value.To(ref field, () => SetShaderParameter("ScratchIntensity", value)); } = 0.65f;

    /// <summary>
    /// The size of the scratches
    /// </summary>
    [DefaultValue(0.0075f), Description("The size of the scratches")]
    public float ScratchWidth { get; set => value.To(ref field, () => SetShaderParameter("IS", value)); } = 0.0075f;
    #endregion

    #region Engine
    /// <inheritdoc/>
    protected override void OnEngineSet()
    {
        LoadShaderFile("Post_ScratchedFilm.fxo");

        // Load noise texture
        _noiseTexture = XTexture.Get(Engine, "Shaders/Noise128.dds", srgb: false);
        _noiseTexture.HoldReference();
        Effect.SetTexture("Noise2DTex", _noiseTexture);

        base.OnEngineSet();
    }
    #endregion

    #region Dispose
    /// <inheritdoc/>
    protected override void OnDispose()
    {
        try
        {
            _noiseTexture?.ReleaseReference();
        }
        finally
        {
            base.OnDispose();
        }
    }
    #endregion
}
