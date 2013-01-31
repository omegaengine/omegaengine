/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Drawing.Design;
using System.Xml.Serialization;

namespace OmegaGUI.Model
{
    /// <summary>
    /// CheckBox control
    /// </summary>
    public class CheckBox : ButtonBase
    {
        #region Variables
        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        private Render.CheckBox _checkbox;
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
                if (_checkbox != null) _checkbox.SetText(Parent.GetLocalized(ControlText));
            }
        }

        protected bool IsChecked;

        /// <summary>
        /// Is this control currently checked?
        /// </summary>
        [DefaultValue(false), Description("Is this control currently checked?"), Category("Appearance")]
        [XmlAttribute]
        public virtual bool Checked
        {
            get { return IsChecked; }
            set
            {
                IsChecked = value;
                if (_checkbox != null) _checkbox.IsChecked = value;
            }
        }

        #region Events
        /// <summary>
        /// A script to run when the control's value has changed
        /// </summary>
        [DefaultValue(""), Description("A script to run when the control's value has changed"), Category("Events"), Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string OnChanged { get; set; }
        #endregion

        #endregion

        #region Constructor
        public CheckBox()
        {
            Size = new Size(140, 20);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            // Add control to dialog
            UpdateLayout();
            DXControl = _checkbox =
                Parent.DialogModel.AddCheckBox(0, Parent.GetLocalized(ControlText), EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, Checked, Hotkey, Default);
            ControlModel.IsVisible = IsVisible;
            ControlModel.IsEnabled = IsEnabled;

            // Setup event hooks
            SetupMouseEvents();
            if (!string.IsNullOrEmpty(OnClick))
                _checkbox.Click += delegate { Parent.RaiseEvent(OnClick, Name + "_Click"); };
            if (!string.IsNullOrEmpty(OnChanged))
            {
                _checkbox.Changed += delegate
                {
                    IsChecked = _checkbox.IsChecked;
                    Parent.RaiseEvent(OnChanged, Name + "_Changed");
                };
            }
        }
        #endregion
    }
}
