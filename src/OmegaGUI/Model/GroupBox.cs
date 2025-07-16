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
using NanoByte.Common.Values;
using OmegaEngine.Values;

namespace OmegaGUI.Model
{
    /// <summary>
    /// GroupBox control
    /// </summary>
    public class GroupBox : Control
    {
        #region Variables
        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        private Render.GroupBox _groupBox;
        #endregion

        #region Properties
        public XColor ColorBorder = Color.Black, ColorFill = Color.FromArgb(128, 128, 128, 128);

        /// <summary>
        /// The color of the border of the group box
        /// </summary>
        [XmlIgnore, Description("The color of the border of the group box"), Category("Appearance")]
        public Color BorderColor
        {
            get { return (Color)ColorBorder; }
            set
            {
                ColorBorder = value;
                if (_groupBox != null)
                    _groupBox.BorderColor = ColorBorder.ToColorValue();
            }
        }

        /// <summary>
        /// The background color of the group box
        /// </summary>
        [XmlIgnore, Description("The background color of the group box"), Category("Appearance")]
        public Color FillColor
        {
            get { return (Color)ColorFill; }
            set
            {
                ColorFill = value;
                if (_groupBox != null)
                    _groupBox.FillColor = ColorFill.ToColorValue();
            }
        }
        #endregion

        #region Constructor
        public GroupBox()
        {
            Size = new(200, 100);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            // Add control to dialog
            UpdateLayout();
            DXControl = _groupBox =
                Parent.DialogRender.AddGroupBox(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, ColorBorder.ToColorValue(), ColorFill.ToColorValue());
            ControlModel.IsVisible = IsVisible;
            ControlModel.IsEnabled = IsEnabled;

            // Setup event hooks
            SetupMouseEvents();
        }
        #endregion
    }
}
