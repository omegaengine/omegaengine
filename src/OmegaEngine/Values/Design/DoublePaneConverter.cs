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
using SlimDX;

namespace OmegaEngine.Values.Design
{
    internal class DoublePlaneConverter : ValueTypeConverter<DoublePlane>
    {
        /// <inheritdoc/>
        protected override int NoArguments => 6;

        /// <inheritdoc/>
        protected override ConstructorInfo GetConstructor() => typeof(DoublePlane).GetConstructor([
            typeof(double),
            typeof(double),
            typeof(double),
            typeof(float),
            typeof(float),
            typeof(float)
        ]);

        /// <inheritdoc/>
        protected override object[] GetArguments(DoublePlane value) =>
        [
            value.Point.X,
            value.Point.Y,
            value.Point.Z,
            value.Normal.X,
            value.Normal.Y,
            value.Normal.Z
        ];

        /// <inheritdoc/>
        protected override string[] GetValues(DoublePlane value, ITypeDescriptorContext context, CultureInfo culture)
        {
            var doubleConverter = TypeDescriptor.GetConverter(typeof(double));
            var floatConverter = TypeDescriptor.GetConverter(typeof(float));
            return
            [
                doubleConverter.ConvertToString(context, culture, value.Point.X),
                doubleConverter.ConvertToString(context, culture, value.Point.Y),
                doubleConverter.ConvertToString(context, culture, value.Point.Z),
                floatConverter.ConvertToString(context, culture, value.Normal.X),
                floatConverter.ConvertToString(context, culture, value.Normal.Y),
                floatConverter.ConvertToString(context, culture, value.Normal.Z)
            ];
        }

        /// <inheritdoc/>
        protected override DoublePlane GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            #endregion

            return new(
                new(Convert.ToDouble(values[0], culture), Convert.ToDouble(values[1], culture), Convert.ToDouble(values[2], culture)),
                new(Convert.ToSingle(values[3], culture), Convert.ToSingle(values[4], culture), Convert.ToSingle(values[5], culture)));
        }

        /// <inheritdoc/>
        protected override DoublePlane GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
            #endregion

            return new((DoubleVector3)propertyValues["Point"], (Vector3)propertyValues["Normal"]);
        }
    }
}
