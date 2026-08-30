/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using AlphaFramework.World.Positionables;
using AlphaFramework.World.Templates;
using SlimDX;

namespace AlphaFramework.World.Components;

/// <summary>
/// Controls how an <see cref="EntityBase{TCoordinates,TTemplate}"/> shall be rendered.
/// </summary>
/// <seealso cref="EntityTemplateBase{TSelf}.Render"/>
[Cloneable]
public abstract partial class Render : ICloneable
{
    /// <inheritdoc/>
    public override string ToString() => GetType().Name;

    /// <summary>
    /// How this component is to be shifted before rendering.
    /// </summary>
    [Description("How this component is to be shifted before rendering.")]
    [DefaultValue(typeof(Vector3), "0,0,0")]
    public Vector3 Shift { get; set; }

    /// <summary>Used for XML serialization.</summary>
    public bool ShouldSerializeShift() => Shift != default;
}
