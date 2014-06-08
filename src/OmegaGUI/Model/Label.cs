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
using OmegaGUI.Render;

namespace OmegaGUI.Model
{
    /// <summary>
    /// Label text control
    /// </summary>
    public class Label : Control
    {
        #region Variables
        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        private Render.Label _staticText;
        #endregion

        #region Properties
        protected string ControlText = "";

        /// <summary>
        /// The text displayed on the control
        /// </summary>
        [DefaultValue(""), Description("The text displayed on the control"), Category("Appearance")]
        [XmlAttribute]
        public virtual string Text
        {
            get { return ControlText; }
            set
            {
                ControlText = value;
                if (_staticText != null) _staticText.SetText(Parent.GetLocalized(ControlText));
            }
        }

        private TextAlign _textAlign;

        /// <summary>
        /// How the text is to be aligned - only effective for Label
        /// </summary>
        [DefaultValue(TextAlign.Left), Description("How the text is to be aligned - only effective for Label"), Category("Appearance")]
        public TextAlign TextAlign
        {
            get { return _textAlign; }
            set
            {
                _textAlign = value;
                if (_staticText != null) _staticText.TextAlign = value;
            }
        }
        #endregion

        #region Constructor
        public Label()
        {
            Size = new Size(100, 20);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            // Add control to dialog
            UpdateLayout();
            DXControl = _staticText =
                Parent.DialogRender.AddStatic(0, Parent.GetLocalized(ControlText), EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, Default);
            ControlModel.IsVisible = IsVisible;
            ControlModel.IsEnabled = IsEnabled;
            _staticText.TextAlign = _textAlign;

            // Setup event hooks
            SetupMouseEvents();
        }
        #endregion
    }
}
