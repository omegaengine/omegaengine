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

internal class Vector2RayConverter : ValueTypeConverter<Vector2Ray>
{
    /// <inheritdoc/>
    protected override int NoArguments => 4;

    /// <inheritdoc/>
    protected override ConstructorInfo GetConstructor() => typeof(Vector2Ray).GetConstructor([
        typeof(float),
        typeof(float),
        typeof(float),
        typeof(float)
    ])!;

    /// <inheritdoc/>
    protected override object[] GetArguments(Vector2Ray value) =>
    [
        value.Origin.X,
        value.Origin.Y,
        value.Direction.X,
        value.Direction.Y
    ];

    /// <inheritdoc/>
    protected override string[] GetValues(Vector2Ray value, ITypeDescriptorContext? context, CultureInfo culture)
    {
        var floatConverter = TypeDescriptor.GetConverter(typeof(float));
        return
        [
            floatConverter.ConvertToString(context, culture, value.Origin.X) ?? "",
            floatConverter.ConvertToString(context, culture, value.Origin.Y) ?? "",
            floatConverter.ConvertToString(context, culture, value.Direction.X) ?? "",
            floatConverter.ConvertToString(context, culture, value.Direction.Y) ?? ""
        ];
    }

    /// <inheritdoc/>
    protected override Vector2Ray GetObject(string[] values, CultureInfo culture)
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (culture == null) throw new ArgumentNullException(nameof(culture));
        #endregion

        return new(
            new(Convert.ToSingle(values[0], culture), Convert.ToSingle(values[1], culture)),
            new(Convert.ToSingle(values[2], culture), Convert.ToSingle(values[3], culture)));
    }

    /// <inheritdoc/>
    protected override Vector2Ray GetObject(IDictionary propertyValues)
    {
        #region Sanity checks
        if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
        #endregion

        return new((Vector2)propertyValues["Point"]!, (Vector2)propertyValues["Direction"]!);
    }
}
