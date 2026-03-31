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
    /// Controls whether particles are tracked in world space (<c>true</c>) or relative to the particle system (<c>false</c>).
    /// When <c>false</c>, moving the particle system moves all existing particles along with it.
    /// </summary>
    [Description("Controls whether particles are tracked in world space or relative to the particle system.")]
    [XmlAttribute]
    public bool WorldSpace { get; set; } = true;
}
