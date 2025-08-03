/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace OmegaEngine.Values.Design;

/// <summary>
/// An editor that can be associated with <c>float</c> properties with values between 0 and 3 to provide a <see cref="TrackBar"/> interface.
/// </summary>
public class SliderEditor : FloatEditor
{
    /// <inheritdoc/>
    protected override float EditValue(float value, IWindowsFormsEditorService editorService)
        => EditValue(value, new(0, 10), editorService);

    /// <inheritdoc/>
    protected override float EditValue(float value, FloatRangeAttribute range, IWindowsFormsEditorService editorService)
    {
        #region Sanity checks
        if (editorService == null) throw new ArgumentNullException(nameof(editorService));
        if (range == null) throw new ArgumentNullException(nameof(range));
        #endregion

        // Scale up by factor 40 and clamp within [minimum,maximum]
        var trackBar = new TrackBar
        {
            TickFrequency = 40,
            Minimum = (int)(range.Minimum * 40),
            Maximum = (int)(range.Maximum * 40),
        };
        trackBar.Value = ((int)(value * 40)).Clamp(trackBar.Minimum, trackBar.Maximum);

        editorService.DropDownControl(trackBar);
        return trackBar.Value / 40f;
    }
}
