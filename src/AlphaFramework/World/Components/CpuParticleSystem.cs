/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.Templates;

namespace AlphaFramework.World.Components;

/// <summary>
/// Represents a particle system whose particles are tracked by the CPU.
/// </summary>
/// <seealso cref="EntityTemplateBase{TSelf}.Render"/>
public class CpuParticleSystem : ParticleSystem
{
    /// <summary>
    /// Controls whether particles are tracked relative to the particle system instead of world space.
    /// </summary>
    [DefaultValue(false), Description("Controls whether particles are tracked relative to the particle system instead of world space.")]
    [XmlAttribute]
    public bool LocalSpace { get; set; }
}
