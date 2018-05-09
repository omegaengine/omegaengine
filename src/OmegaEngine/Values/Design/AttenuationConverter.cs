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
    internal class AttenuationConverter : ValueTypeConverter<Attenuation>
    {
        /// <inheritdoc/>
        protected override int NoArguments => 3;

        /// <inheritdoc/>
        protected override ConstructorInfo GetConstructor() => typeof(Attenuation).GetConstructor(new[] {typeof(float), typeof(float), typeof(float)});

        /// <inheritdoc/>
        protected override object[] GetArguments(Attenuation value) => new object[] {value.Constant, value.Linear, value.Quadratic};

        /// <inheritdoc/>
        protected override string[] GetValues(Attenuation value, ITypeDescriptorContext context, CultureInfo culture)
        {
            var floatConverter = TypeDescriptor.GetConverter(typeof(float));
            return new[]
            {
                floatConverter.ConvertToString(context, culture, value.Constant),
                floatConverter.ConvertToString(context, culture, value.Linear),
                floatConverter.ConvertToString(context, culture, value.Quadratic)
            };
        }

        /// <inheritdoc/>
        protected override Attenuation GetObject(string[] values, CultureInfo culture)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (culture == null) throw new ArgumentNullException(nameof(culture));
            #endregion

            return new Attenuation(Convert.ToSingle(values[0], culture), Convert.ToSingle(values[1], culture), Convert.ToSingle(values[2], culture));
        }

        /// <inheritdoc/>
        protected override Attenuation GetObject(IDictionary propertyValues)
        {
            #region Sanity checks
            if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
            #endregion

            return new Attenuation((float)propertyValues["Constant"], (float)propertyValues["Linear"], (float)propertyValues["Quadratic"]);
        }
    }
}
