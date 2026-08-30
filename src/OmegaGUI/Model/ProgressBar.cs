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

using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using OmegaEngine.Foundation.Light;
using SlimDX;

namespace OmegaGUI.Model;

/// <summary>
/// ProgressBar control
/// </summary>
[Cloneable]
public partial class ProgressBar : Control
{
    #region Variables
    /// <summary>
    /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
    /// </summary>
    [IgnoreClone]
    private Render.ProgressBar? _progressBar;
    #endregion

    #region Properties
    private float _min;

    /// <summary>
    /// The lower end of the range displayed by the bar
    /// </summary>
    [XmlAttribute, DefaultValue(0f), Description("The lower end of the range displayed by the bar"), Category("Behavior")]
    public float Min
    {
        get => _min;
        set
        {
            _min = value;
            if (_progressBar != null) _progressBar.Min = value;
        }
    }

    private float _max = 1;

    /// <summary>
    /// The upper end of the range displayed by the bar
    /// </summary>
    [XmlAttribute, DefaultValue(1f), Description("The upper end of the range displayed by the bar"), Category("Behavior")]
    public float Max
    {
        get => _max;
        set
        {
            _max = value;
            if (_progressBar != null) _progressBar.Max = value;
        }
    }

    private float _value;

    /// <summary>
    /// The current value of the control
    /// </summary>
    [XmlAttribute, DefaultValue(0f), Description("The current value of the control"), Category("Appearance")]
    public float Value
    {
        get => _value;
        set
        {
            _value = value;
            if (_progressBar != null) _progressBar.Value = value;
        }
    }

    private bool _reversed;

    /// <summary>
    /// Fill the bar from right to left instead of from left to right
    /// </summary>
    [XmlAttribute, DefaultValue(false), Description("Fill the bar from right to left instead of from left to right"), Category("Appearance")]
    public bool Reversed
    {
        get => _reversed;
        set
        {
            _reversed = value;
            if (_progressBar != null) _progressBar.Reversed = value;
        }
    }

    /// <summary>Used for XML serialization.</summary>
    public XColor ColorFill = Color.White;

    /// <summary>
    /// The color of the bar when <see cref="Value"/> is at <see cref="Max"/>
    /// </summary>
    [XmlIgnore, DefaultValue(typeof(Color), "White"), Description("The color of the bar when the value is at its maximum"), Category("Appearance")]
    public Color FillColor
    {
        get => ColorFill;
        set
        {
            ColorFill = value;
            if (_progressBar != null) _progressBar.FillColor = ColorFill.ToColor4();
        }
    }

    /// <summary>Used for XML serialization.</summary>
    public XColor ColorFillLow;

    /// <summary>Used for XML serialization.</summary>
    public bool ShouldSerializeColorFillLow() => ColorFillLow != default(XColor);

    /// <summary>
    /// The color of the bar when <see cref="Value"/> is at <see cref="Min"/>; leave transparent to always use <see cref="FillColor"/>
    /// </summary>
    [XmlIgnore, DefaultValue(typeof(Color), "0,0,0,0"), Description("The color of the bar when the value is at its minimum; leave transparent to always use the fill color"), Category("Appearance")]
    public Color FillColorLow
    {
        get => ColorFillLow;
        set
        {
            ColorFillLow = value;
            if (_progressBar != null) _progressBar.FillColorLow = ColorFillLow.ToColor4();
        }
    }

    /// <summary>Used for XML serialization.</summary>
    public XColor ColorBackground;

    /// <summary>Used for XML serialization.</summary>
    public bool ShouldSerializeColorBackground() => ColorBackground != default(XColor);

    /// <summary>
    /// A custom tint for the area behind the bar; leave transparent to use the default state colors
    /// </summary>
    [XmlIgnore, DefaultValue(typeof(Color), "0,0,0,0"), Description("A custom tint for the area behind the bar; leave transparent to use the default state colors"), Category("Appearance")]
    public Color BackgroundColor
    {
        get => ColorBackground;
        set
        {
            ColorBackground = value;
            ApplyBackgroundColor();
        }
    }
    #endregion

    #region Constructor
    public ProgressBar()
    {
        Size = new(150, 24);
    }
    #endregion

    #region Generate
    internal override void Generate()
    {
        // Add control to dialog
        UpdateLayout();
        DXControl = _progressBar =
            Parent.DialogRender.AddProgressBar(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height);
        _progressBar.Min = _min;
        _progressBar.Max = _max;
        _progressBar.Value = _value;
        _progressBar.Reversed = _reversed;
        _progressBar.FillColor = ColorFill.ToColor4();
        _progressBar.FillColorLow = ColorFillLow.ToColor4();
        ApplyBackgroundColor();
        ControlModel.IsVisible = IsVisible;
        ControlModel.IsEnabled = IsEnabled;

        // Setup event hooks
        SetupMouseEvents();
    }

    /// <summary>
    /// (Re-)applies <see cref="BackgroundColor"/> to the control's rendered elements, e.g. after (re-)generating
    /// </summary>
    private void ApplyBackgroundColor()
    {
        if (_progressBar == null || ColorBackground.A == 0) return;

        var element = _progressBar[Render.ProgressBar.BackgroundLayer];

        // Copy the state colors before modifying them, since the array is shared with the dialog's default elements
        var states = (Color4[])element.TextureColor.States.Clone();
        states[(int)Render.ControlState.Normal] = ColorBackground.ToColor4();
        element.TextureColor.States = states;
    }
    #endregion
}
