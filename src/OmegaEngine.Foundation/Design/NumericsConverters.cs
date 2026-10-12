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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using NanoByte.Common.Values.Design;

namespace OmegaEngine.Foundation.Design;

/// <summary>
/// Registers <see cref="TypeConverter"/>s for <see cref="System.Numerics"/> types, which do not come with any.
/// </summary>
/// <remarks>Needed for PropertyGrids and for <see cref="DefaultValueAttribute"/>s parsing values from strings.</remarks>
internal static class NumericsConverters
{
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries", Justification = "Converters must be in place before any attribute referencing these types is evaluated.")]
    internal static void Register()
    {
        TypeDescriptor.AddAttributes(typeof(Vector2), new TypeConverterAttribute(typeof(Vector2Converter)));
        TypeDescriptor.AddAttributes(typeof(Vector3), new TypeConverterAttribute(typeof(Vector3Converter)));
        TypeDescriptor.AddAttributes(typeof(Vector4), new TypeConverterAttribute(typeof(Vector4Converter)));
        TypeDescriptor.AddAttributes(typeof(Quaternion), new TypeConverterAttribute(typeof(QuaternionConverter)));
    }
}

internal class Vector2Converter() : FloatFieldsConverter<Vector2>(nameof(Vector2.X), nameof(Vector2.Y));

internal class Vector3Converter() : FloatFieldsConverter<Vector3>(nameof(Vector3.X), nameof(Vector3.Y), nameof(Vector3.Z));

internal class Vector4Converter() : FloatFieldsConverter<Vector4>(nameof(Vector4.X), nameof(Vector4.Y), nameof(Vector4.Z), nameof(Vector4.W));

internal class QuaternionConverter() : FloatFieldsConverter<Quaternion>(nameof(Quaternion.X), nameof(Quaternion.Y), nameof(Quaternion.Z), nameof(Quaternion.W));

/// <summary>
/// Converts structs made up of public <see cref="float"/> fields, with a constructor taking the fields in order.
/// </summary>
internal abstract class FloatFieldsConverter<T>(params string[] fieldNames) : ValueTypeConverter<T>
    where T : struct
{
    private readonly FieldInfo[] _fields = fieldNames.Select(name => typeof(T).GetField(name)!).ToArray();

    /// <inheritdoc/>
    protected override int NoArguments => _fields.Length;

    /// <inheritdoc/>
    protected override ConstructorInfo GetConstructor() => typeof(T).GetConstructor(_fields.Select(_ => typeof(float)).ToArray())!;

    /// <inheritdoc/>
    protected override object[] GetArguments(T value) => _fields.Select(field => field.GetValue(value)!).ToArray();

    /// <inheritdoc/>
    protected override string[] GetValues(T value, ITypeDescriptorContext? context, CultureInfo culture)
    {
        var floatConverter = TypeDescriptor.GetConverter(typeof(float));
        return _fields.Select(field => floatConverter.ConvertToString(context, culture, field.GetValue(value)) ?? "").ToArray();
    }

    /// <inheritdoc/>
    protected override T GetObject(string[] values, CultureInfo culture)
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        if (culture == null) throw new ArgumentNullException(nameof(culture));
        #endregion

        return (T)GetConstructor().Invoke(values.Select(x => (object)Convert.ToSingle(x, culture)).ToArray());
    }

    /// <inheritdoc/>
    protected override T GetObject(IDictionary propertyValues)
    {
        #region Sanity checks
        if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));
        #endregion

        return (T)GetConstructor().Invoke(_fields.Select(field => propertyValues[field.Name]!).ToArray());
    }

    /// <inheritdoc/>
    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext? context, object value, Attribute[]? attributes)
        => new(_fields.Select(field => (PropertyDescriptor)new FieldPropertyDescriptor(field)).ToArray());

    /// <summary>
    /// Exposes a field as a property, since PropertyGrids only show properties.
    /// </summary>
    private sealed class FieldPropertyDescriptor(FieldInfo field) : PropertyDescriptor(field.Name, [])
    {
        public override Type ComponentType => field.DeclaringType!;
        public override Type PropertyType => field.FieldType;
        public override bool IsReadOnly => false;
        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) {}
        public override bool ShouldSerializeValue(object component) => true;
        public override object? GetValue(object? component) => field.GetValue(component);
        public override void SetValue(object? component, object? value) => field.SetValue(component, value);
    }
}
