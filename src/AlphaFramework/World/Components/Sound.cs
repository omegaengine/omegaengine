/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Templates;
using OmegaEngine.Foundation.Geometry;
using SlimDX;

namespace AlphaFramework.World.Components;

/// <summary>
/// Makes an <see cref="EntityBase{TCoordinates,TTemplate}"/> emit a positioned 3D sound.
/// </summary>
/// <seealso cref="EntityTemplateBase{TSelf}.Sound"/>
public class Sound : ICloneable
{
    /// <inheritdoc/>
    public override string ToString() => GetType().Name + (string.IsNullOrEmpty(Filename) ? "" : $": {Filename}");

    /// <summary>
    /// The filename of the sound file to play.
    /// </summary>
    [DefaultValue(""), Description("The filename of the sound file to play.")]
    [XmlAttribute]
    public string? Filename { get; set; }

    /// <summary>
    /// The playback volume as a factor (0 = silent, 1 = normal).
    /// </summary>
    [DefaultValue(1f), Description(" The playback volume as a factor (0 = silent, 1 = normal).")]
    [XmlAttribute]
    public float Volume { get; set; } = 1;

    /// <summary>
    /// Factors describing the attenuation of sound intensity over distance.
    /// </summary>
    [Description("Factors describing the attenuation of sound intensity over distance.")]
    public Attenuation Attenuation { get; set; } = Attenuation.None;

    /// <summary>
    /// A positional offset relative to the entity's origin.
    /// </summary>
    [Description("A positional offset relative to the entity's origin.")]
    public Vector3 Shift { get; set; }

    /// <summary>
    /// Indicates whether <see cref="Shift"/> has been set to a non-default value.
    /// </summary>
    [Browsable(false), XmlIgnore]
    public bool ShiftSpecified { get => Shift != default; set { if (!value) Shift = default; } }

    //--------------------//

    #region Clone
    /// <summary>
    /// Creates a shallow copy of this <see cref="Sound"/>
    /// </summary>
    /// <returns>The cloned <see cref="Sound"/>.</returns>
    public Sound Clone()
    {
        // Perform initial shallow copy
        return (Sound)MemberwiseClone();
    }

    object ICloneable.Clone() => Clone();
    #endregion
}
