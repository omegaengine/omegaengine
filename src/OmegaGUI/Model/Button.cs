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

namespace OmegaGUI.Model
{
    /// <summary>
    /// Button control
    /// </summary>
    public class Button : ButtonBase
    {
        #region Variables
        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        private Render.Button _button;
        #endregion

        #region Properties
        /// <summary>
        /// The text displayed on the control
        /// </summary>
        [DefaultValue(""), Description("The text displayed on the control"), Category("Appearance")]
        [XmlAttribute]
        public override string Text
        {
            get { return ControlText; }
            set
            {
                ControlText = value;
                if (_button != null) _button.SetText(Parent.GetLocalized(ControlText));
            }
        }

        private string _customStyle;

        /// <summary>
        /// A custom style for this button - no auto-update
        /// </summary>
        [Description("A custom style for this button"), Category("Appearance")]
        public string CustomStyle
        {
            get { return _customStyle; }
            set
            {
                _customStyle = value;
                NeedsUpdate();
            }
        }

        [Description("Is the specified style name valid?"), Category("Appearance")]
        public bool StyleValid
        {
            get
            {
                if (Parent != null)
                {
                    ButtonStyle style = Parent.GetButtonStyle(_customStyle);
                    return style != null && style.TextureFileValid;
                }
                return false;
            }
        }
        #endregion

        #region Constructor
        public Button()
        {
            Size = new Size(140, 40);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            // Add control to dialog
            UpdateLayout();
            DXControl = _button =
                Parent.DialogRender.AddButton(0, Parent.GetLocalized(ControlText), EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, Hotkey, Default);
            DXControl.IsVisible = IsVisible;
            DXControl.IsEnabled = IsEnabled;
            DXControl.IsEnabled = IsEnabled;

            if (StyleValid)
            {
                ButtonStyle style = Parent.GetButtonStyle(_customStyle);
                _button[Render.Button.ButtonLayer] = style.ButtonElement;
                _button[Render.Button.FillLayer] = style.ButtonElement;
            }

            // Setup event hooks
            SetupMouseEvents();
            if (!string.IsNullOrEmpty(OnClick))
                _button.Click += delegate { Parent.RaiseEvent(OnClick, Name + "_Click"); };
        }
        #endregion
    }
}
