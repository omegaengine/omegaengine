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
using SlimDX;

namespace OmegaEngine.Foundation.Design;

internal class Vector3RayConverter : ValueTypeConverter<Vector3Ray>
{
    /// <inheritdoc/>
    protected override int NoArguments => 4;

    /// <inheritdoc/>
    protected override ConstructorInfo GetConstructor() => typeof(Vector3Ray).GetConstructor([
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float)
    ])!;

    /// <inheritdoc/>
    protected override object[] GetArguments(Vector3Ray value) =>
    [
        value.Position.X,
        value.Position.Y,
        value.Position.Z,
        value.Direction.X,
        value.Direction.Y,
        value.Direction.Z
    ];

    /// <inheritdoc/>
    protected override string[] GetValues(Vector3Ray value, ITypeDescriptorContext? context, CultureInfo culture)
    {
        var floatConverter = TypeDescriptor.GetConverter(typeof(float));
        return
        [
            floatConverter.ConvertToString(context, culture, value.Position.X) ?? "",
            floatConverter.ConvertToString(context, culture, value.Position.Y) ?? "",
            floatConverter.ConvertToString(context, culture, value.Position.Z) ?? "",
            floatConverter.ConvertToString(context, culture, value.Direction.X) ?? "",
            floatConverter.ConvertToString(context, culture, value.Direction.Y) ?? "",
            floatConverter.ConvertToString(context, culture, value.Direction.Z) ?? ""
        ];
    }

    /// <inheritdoc/>
    protected override Vector3Ray GetObject(string[] values, CultureInfo culture)
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (culture == null) throw new ArgumentNullException(nameof(culture));
        #endregion

        return new(
            new(Convert.ToSingle(values[0], culture), Convert.ToSingle(values[1], culture), Convert.ToSingle(values[2], culture)),
            new(Convert.ToSingle(values[3], culture), Convert.ToSingle(values[4], culture), Convert.ToSingle(values[5], culture)));
    }

    /// <inheritdoc/>
    protected override Vector3Ray GetObject(IDictionary propertyValues)
    {
        #region Sanity checks
        if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
        #endregion

        return new((Vector3)propertyValues["Point"]!, (Vector3)propertyValues["Direction"]!);
    }
}
