/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using LuaInterface;
using OmegaEngine.Foundation.Light;
using SlimDX;

namespace OmegaEngine.Graphics.Renderables;

/// <summary>
/// A set of information about a particle in a particle system
/// </summary>
[XmlInclude(typeof(Color)), XmlInclude(typeof(Color4))]
public class CpuParticleParameters
{
    #region Constants
    /// <summary>
    /// This value for <see cref="LifeTime"/> is a flag for infinite life
    /// </summary>
    internal const float InfiniteFlag = -32768;
    #endregion

    #region Properties
    /// <summary>
    /// How many seconds this particle will exist.
    /// Set to 0 never create. Set to <see cref="InfiniteFlag"/> for infinite.
    /// </summary>
    [DefaultValue(0f), Description("How many seconds this particle will exist; 0 = never create; -32768 for infinite")]
    public float LifeTime { get; set; }

    /// <summary>
    /// How much the velocity will be reduced in one second as a value between 0 and 1
    /// </summary>
    [DefaultValue(0f), Description("How much the velocity will be reduced in one second as a value between 0 and 1")]
    public float Friction { get; set; }

    /// <summary>
    /// The size of the particle
    /// </summary>
    [DefaultValue(0f), Description("The size of the particle")]
    public float Size { get; set; }

    /// <summary>
    /// How much the particle will grow per second
    /// </summary>
    [DefaultValue(0f), Description("How much the particle will grow per second")]
    public float DeltaSize { get; set; }

    /// <summary>
    /// The color of the particle
    /// </summary>
    /// <remarks>Is not serialized/stored, <see cref="Color4"/> is used for that.</remarks>
    [XmlIgnore, LuaHide, Description("The color of the particle")]
    public Color Color { get; set; } = Color.White;

    /// <summary>Used for XML serialization.</summary>
    /// <seealso cref="Color"/>
    [XmlElement("Color"), LuaHide, Browsable(false)]
    // ReSharper disable UnusedMember.Global
    public XColor Color4
    {
        get => Color;
        set => Color = value;
    }

    // ReSharper restore UnusedMember.Global

    /// <summary>
    /// How much the particle gets darker per second
    /// </summary>
    [DefaultValue(0f), Description("How much the particle gets darker per second")]
    public float DeltaColor { get; set; }
    #endregion

    #region Clone
    /// <summary>
    /// Creates a plain copy of the this particle system parameter set
    /// </summary>
    internal CpuParticleParameters CloneParameters()
    {
        return (CpuParticleParameters)MemberwiseClone();
    }
    #endregion
}
