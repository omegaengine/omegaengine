/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Drawing.Design;
using OmegaEngine.Foundation.Design;
using Resources = OmegaGUI.Properties.Resources;

namespace OmegaGUI.Model;

/// <summary>
/// Slider control
/// </summary>
public class Slider : Control
{
    #region Variables
    /// <summary>
    /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
    /// </summary>
    private Render.Slider? _slider;
    #endregion

    #region Properties
    private int _min;

    /// <summary>
    /// Minimum value for control  - no auto-update
    /// </summary>
    [DefaultValue(0), Description("Minimum value for control"), Category("Behavior")]
    public int Min
    {
        get => _min;
        set
        {
            if (value >= Max)
                throw new InvalidOperationException(Resources.MinMustBeSmallerThanMax);
            _min = value;
            NeedsUpdate();
        }
    }

    private int _max = 10;

    /// <summary>
    /// Maximum value for control - no auto-update
    /// </summary>
    [DefaultValue(10), Description("Maximum value for control"), Category("Behavior")]
    public int Max
    {
        get => _max;
        set
        {
            if (value <= Min)
                throw new InvalidOperationException(Resources.MaxMustBeLargerThanMin);
            _max = value;
            NeedsUpdate();
        }
    }

    protected int ControlValue;

    /// <summary>
    /// The current value of the control
    /// </summary>
    [DefaultValue(0), Description("The current value of the control"), Category("Appearance")]
    public virtual int Value
    {
        get => ControlValue;
        set
        {
            if (value < Min || value > Max)
                throw new InvalidOperationException(Resources.ValueOutOfRange);
            ControlValue = value;
            if (_slider != null) _slider.Value = value;
        }
    }

    #region Events
    /// <summary>
    /// A Lua script to execute when the control's value has changed
    /// </summary>
    [DefaultValue(""), Description("A Lua script to execute when the control's value has changed"), Category("Events"), FileType("Lua")]
    [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
    public string? OnChanged { get; set; }
    #endregion

    #endregion

    #region Constructor
    public Slider()
    {
        Size = new(150, 32);
    }
    #endregion

    #region Generate
    internal override void Generate()
    {
        // Add control to dialog
        UpdateLayout();
        DXControl = _slider =
            Parent.DialogRender.AddSlider(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, _min, _max, Value, Default);
        ControlModel.IsVisible = IsVisible;
        ControlModel.IsEnabled = IsEnabled;

        // Setup event hooks
        SetupMouseEvents();
        if (!string.IsNullOrEmpty(OnChanged))
        {
            _slider.Changed += delegate
            {
                ControlValue = _slider.Value;
                Parent.RaiseEvent(OnChanged, $"{Name}_Changed");
            };
        }
    }
    #endregion
}
