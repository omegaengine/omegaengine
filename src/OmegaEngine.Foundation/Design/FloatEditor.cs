/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace OmegaEngine.Foundation.Design;

/// <summary>
/// Abstract base class for drop-down <see cref="PropertyGrid"/> editors that can be associated with <c>float</c> or <c>double</c> properties.
/// </summary>
public abstract class FloatEditor : UITypeEditor
{
    /// <inheritdoc/>
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext? context) => UITypeEditorEditStyle.DropDown;

    /// <inheritdoc/>
    public override object EditValue(ITypeDescriptorContext? context, IServiceProvider? provider, object? value)
    {
        #region Sanity checks
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        if (value == null) throw new ArgumentNullException(nameof(value));
        #endregion

        if (!TryGetDouble(value, out double doubleValue) || provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService editorService) return value;

        var range = context.PropertyDescriptor?.Attributes.OfType<FloatRangeAttribute>().FirstOrDefault();
        double newValue = (range == null)
            ? EditValue(doubleValue, editorService)
            : EditValue(doubleValue, range, editorService);
        if (value is float)
            return (float)newValue;
        else
            return newValue;
    }

    /// <summary>
    /// Displays the UI to edit the <c>float</c> value.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="editorService">The editor service used to display the dropdown control.</param>
    /// <returns>The value set by the user.</returns>
    protected abstract double EditValue(double value, IWindowsFormsEditorService editorService);

    /// <summary>
    /// Displays the UI to edit the <c>float</c> value.
    /// </summary>
    /// <param name="value">The current value.</param>
    /// <param name="range">The range of valid values the user can select.</param>
    /// <param name="editorService">The editor service used to display the dropdown control.</param>
    /// <returns>The value set by the user.</returns>
    protected abstract double EditValue(double value, FloatRangeAttribute range, IWindowsFormsEditorService editorService);

    /// <summary>
    /// Interprets <paramref name="value"/> as a <c>double</c> if it is a <c>float</c> or <c>double</c>.
    /// </summary>
    protected bool TryGetDouble(object? value, out double doubleValue)
    {
        switch (value)
        {
            case double d:
                doubleValue = d;
                return true;
            case float f:
                doubleValue = f;
                return true;
            default:
                doubleValue = 0;
                return false;
        }
    }
}
