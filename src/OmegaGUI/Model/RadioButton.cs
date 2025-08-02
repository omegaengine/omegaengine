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
using System.Xml.Serialization;

namespace OmegaGUI.Model;

/// <summary>
/// Radio button control
/// </summary>
public class RadioButton : CheckBox
{
    #region Variables
    /// <summary>
    /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
    /// </summary>
    private Render.RadioButton _radioButton;
    #endregion

    #region Properties
    private uint _groupID;

    /// <summary>
    /// The ID of the radio button group - no auto-update
    /// </summary>
    [Description("The ID of the radio button group"), Category("Design"),]
    public uint GroupID
    {
        get => _groupID;
        set
        {
            _groupID = value;
            NeedsUpdate();
        }
    }

    /// <summary>
    /// The text displayed on the control
    /// </summary>
    [DefaultValue(""), Description("The text displayed on the control"), Category("Appearance")]
    [XmlAttribute]
    public override string Text
    {
        get => ControlText;
        set
        {
            ControlText = value;
            if (_radioButton != null) _radioButton.SetText(Parent.GetLocalized(ControlText));
        }
    }

    /// <summary>
    /// Is this control currently checked?
    /// </summary>
    [DefaultValue(false), Description("Is this control currently checked?"), Category("Appearance")]
    [XmlAttribute]
    public override bool Checked
    {
        get => _radioButton?.IsChecked ?? IsChecked;
        set
        {
            IsChecked = value;
            if (_radioButton != null) _radioButton.IsChecked = value;
        }
    }
    #endregion

    #region Constructor
    public RadioButton()
    {
        Size = new(140, 20);
    }
    #endregion

    #region Generate
    internal override void Generate()
    {
        // Add control to dialog
        UpdateLayout();
        DXControl = _radioButton =
            Parent.DialogRender.AddRadioButton(0, _groupID, Parent.GetLocalized(ControlText), EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, IsChecked, Hotkey, Default);
        ControlModel.IsVisible = IsVisible;
        ControlModel.IsEnabled = IsEnabled;

        // Setup event hooks
        SetupMouseEvents();
        if (!string.IsNullOrEmpty(OnClick))
            _radioButton.Click += delegate { Parent.RaiseEvent(OnClick, Name + "_Click"); };
        if (!string.IsNullOrEmpty(OnChanged))
        {
            // Note: Don't auto-update isChecked value since changing one RadioButton will effect others as well
            _radioButton.Changed += delegate { Parent.RaiseEvent(OnChanged, Name + "_Changed"); };
        }
    }
    #endregion
}