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
using System.Drawing.Design;
using System.Xml.Serialization;
using OmegaEngine.Values;
using OmegaEngine.Values.Design;

namespace OmegaGUI.Model;

/// <summary>
/// A basic edit box
/// </summary>
public class TextBox : Label
{
    #region Variables
    /// <summary>
    /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
    /// </summary>
    private Render.TextBox _editBox;
    #endregion

    #region Properties
    /// <summary>
    /// The text entered in the control
    /// </summary>
    [Description("The text entered in the control"), Category("Appearance")]
    [XmlAttribute]
    public override string Text
    {
        get => ControlText;
        set
        {
            ControlText = value;
            if (_editBox != null) _editBox.Text = value;
        }
    }

    #region Events
    /// <summary>
    /// A Lua script to execute when the user presses the ENTER key
    /// </summary>
    [DefaultValue(""), Description("A Lua script to execute when the user presses the ENTER key"), Category("Events"), FileType("Lua")]
    [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
    public string OnEnter { get; set; }

    /// <summary>
    /// A Lua script to execute when the control's value has changed
    /// </summary>
    [DefaultValue(""), Description("A Lua script to execute when the control's value has changed"), Category("Events"), FileType("Lua")]
    [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
    public string OnChanged { get; set; }
    #endregion

    #endregion

    #region Constructor
    public TextBox()
    {
        Size = new(150, 35);
    }
    #endregion

    #region Generate
    internal override void Generate()
    {
        // Add control to dialog
        UpdateLayout();
        DXControl = _editBox =
            Parent.DialogRender.AddTextBox(0, Parent.GetLocalized(ControlText), EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, Default);
        ControlModel.IsVisible = IsVisible;
        ControlModel.IsEnabled = IsEnabled;

        // Setup event hooks
        SetupMouseEvents();
        if (!string.IsNullOrEmpty(OnEnter))
            _editBox.Enter += delegate { Parent.RaiseEvent(OnEnter, Name + "_Enter"); };
        if (!string.IsNullOrEmpty(OnChanged))
        {
            _editBox.Changed += delegate
            {
                ControlText = _editBox.Text;
                Parent.RaiseEvent(OnChanged, Name + "_Changed");
            };
        }
    }
    #endregion
}
