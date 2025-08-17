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
using SlimDX;

namespace AlphaFramework.World.Components;

/// <summary>
/// Controls how an <see cref="EntityBase{TCoordinates,TTemplate}"/> shall be rendered.
/// </summary>
/// <seealso cref="EntityTemplateBase{TSelf}.Render"/>
public abstract class Render : ICloneable
{
    /// <inheritdoc/>
    public override string ToString() => GetType().Name;

    /// <summary>
    /// How this component is to be shifted before rendering.
    /// </summary>
    [Description("How this component is to be shifted before rendering.")]
    public Vector3 Shift { get; set; }

    /// <summary>
    /// Indicates whether <see cref="Shift"/> has been set to a non-default value.
    /// </summary>
    [Browsable(false), XmlIgnore]
    public bool ShiftSpecified { get => Shift != default; set { if (!value) Shift = default; } }

    //--------------------//

    #region Clone
    /// <summary>
    /// Creates a shallow copy of this <see cref="Render"/>
    /// </summary>
    /// <returns>The cloned <see cref="Render"/>.</returns>
    public Render Clone()
    {
        // Perform initial shallow copy
        return (Render)MemberwiseClone();
    }

    object ICloneable.Clone() => Clone();
    #endregion
}
