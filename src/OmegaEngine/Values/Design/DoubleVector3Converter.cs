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

namespace OmegaEngine.Values.Design
{
    internal class DoubleVector3Converter : ValueTypeConverter<DoubleVector3>
    {
        /// <inheritdoc/>
        protected override int NoArguments => 3;

        /// <inheritdoc/>
        protected override ConstructorInfo GetConstructor() => typeof(DoubleVector3).GetConstructor([typeof(double), typeof(double), typeof(double)]);

        /// <inheritdoc/>
        protected override object[] GetArguments(DoubleVector3 value) => [value.X, value.Y];

        /// <inheritdoc/>
        protected override string[] GetValues(DoubleVector3 value, ITypeDescriptorContext context, CultureInfo culture)
        {
            var doubleConverter = TypeDescriptor.GetConverter(typeof(double));
            return
            [
                doubleConverter.ConvertToString(context, culture, value.X),
                doubleConverter.ConvertToString(context, culture, value.Y),
                doubleConverter.ConvertToString(context, culture, value.Z)
            ];
        }

        /// <inheritdoc/>
        protected override DoubleVector3 GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            #endregion

            return new(Convert.ToDouble(values[0], culture), Convert.ToDouble(values[1], culture), Convert.ToDouble(values[2], culture));
        }

        /// <inheritdoc/>
        protected override DoubleVector3 GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
            #endregion

            return new((double)propertyValues["X"], (double)propertyValues["Y"], (double)propertyValues["Z"]);
        }
    }
}
