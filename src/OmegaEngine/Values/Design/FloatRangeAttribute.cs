/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using OmegaEngine.Properties;

namespace OmegaEngine.Values.Design;

/// <summary>
/// Stores the minimum and maximum values allowed for a float field or property.
/// Controls the behaviour of <see cref="AngleEditor"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class FloatRangeAttribute : Attribute
{
    /// <summary>
    /// The minimum value the field or property may have.
    /// </summary>
    public float Minimum { get; }

    /// <summary>
    /// The maximum value the field or property may have.
    /// </summary>
    public float Maximum { get; }

    /// <summary>
    /// Creates a new float range attribute.
    /// </summary>
    /// <param name="minimum">The minimum value the field or property may have.</param>
    /// <param name="maximum">The maximum value the field or property may have.</param>
    public FloatRangeAttribute(float minimum, float maximum)
    {
        #region Sanity checks
        if (minimum > maximum) throw new ArgumentException(Resources.MinLargerMax, nameof(minimum));
        #endregion

        Minimum = minimum;
        Maximum = maximum;
    }
}
