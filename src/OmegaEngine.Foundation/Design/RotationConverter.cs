/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using NanoByte.Common.Values.Design;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Foundation.Design;

internal class RotationConverter : ValueTypeConverter<Rotation>
{
    /// <inheritdoc/>
    protected override int NoArguments => 3;

    /// <inheritdoc/>
    protected override ConstructorInfo GetConstructor() => typeof(Rotation).GetConstructor([typeof(float), typeof(float), typeof(float)])!;

    /// <inheritdoc/>
    protected override object[] GetArguments(Rotation value) => [value.Yaw, value.Pitch, value.Roll];

    /// <inheritdoc/>
    protected override string[] GetValues(Rotation value, ITypeDescriptorContext? context, CultureInfo culture)
    {
        var floatConverter = TypeDescriptor.GetConverter(typeof(float));
        return
        [
            floatConverter.ConvertToString(context, culture, value.Yaw) ?? "",
            floatConverter.ConvertToString(context, culture, value.Pitch) ?? "",
            floatConverter.ConvertToString(context, culture, value.Roll) ?? ""
        ];
    }

    /// <inheritdoc/>
    protected override Rotation GetObject(string[] values, CultureInfo culture)
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (culture == null) throw new ArgumentNullException(nameof(culture));
        #endregion

        return new(
            Convert.ToSingle(values[0], culture),
            Convert.ToSingle(values[1], culture),
            Convert.ToSingle(values[2], culture));
    }

    /// <inheritdoc/>
    protected override Rotation GetObject(IDictionary propertyValues)
    {
        #region Sanity checks
        if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
        #endregion

        return new(
            (float)propertyValues[nameof(Rotation.Yaw)]!,
            (float)propertyValues[nameof(Rotation.Pitch)]!,
            (float)propertyValues[nameof(Rotation.Roll)]!);
    }
}
