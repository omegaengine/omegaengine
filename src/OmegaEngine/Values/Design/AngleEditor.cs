/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace OmegaEngine.Values.Design;

/// <summary>
/// An editor that can be associated with <c>float</c> properties representing angles between 0 and 360 degrees. Uses <see cref="AngleControl"/>.
/// </summary>
/// <seealso cref="FloatRangeAttribute"/>
public class AngleEditor : FloatEditor
{
    /// <inheritdoc/>
    protected override float EditValue(float value, IWindowsFormsEditorService editorService)
    {
        #region Sanity checks
        if (editorService == null) throw new ArgumentNullException(nameof(editorService));
        #endregion

        var angleControl = new AngleControl {Angle = value};
        editorService.DropDownControl(angleControl);
        return angleControl.Angle;
    }

    /// <inheritdoc/>
    protected override float EditValue(float value, FloatRangeAttribute range, IWindowsFormsEditorService editorService)
    {
        #region Sanity checks
        if (editorService == null) throw new ArgumentNullException(nameof(editorService));
        if (range == null) throw new ArgumentNullException(nameof(range));
        #endregion

        var angleControl = new AngleControl {Angle = value, Range = range};
        editorService.DropDownControl(angleControl);
        return angleControl.Angle;
    }

    /// <inheritdoc/>
    public override bool GetPaintValueSupported(ITypeDescriptorContext context) => true;

    /// <inheritdoc/>
    public override void PaintValue(PaintValueEventArgs e)
    {
        #region Sanity checks
        if (e == null) throw new ArgumentNullException(nameof(e));
        #endregion

        // Draw background
        e.Graphics.FillRectangle(new SolidBrush(Color.DarkBlue), e.Bounds);

        // Draw circle
        e.Graphics.FillEllipse(new SolidBrush(Color.White), e.Bounds.X + 3, e.Bounds.Y + 1, e.Bounds.Width - 6, e.Bounds.Height - 2);

        if (!(e.Value is float)) return;

        // Draw angle line
        var center = new Point(e.Bounds.Width / 2 + e.Bounds.X, e.Bounds.Height / 2 + e.Bounds.Y);
        float angle = ((float)e.Value).DegreeToRadian();
        var endPoint = new Point(
            center.X + (int)(center.X * Math.Sin(angle)),
            center.Y + (int)(center.Y * -Math.Cos(angle)));
        e.Graphics.DrawLine(new(new SolidBrush(Color.Red), 1), center, endPoint);
    }
}
