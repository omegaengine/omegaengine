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
using System.Drawing;
using Resources = OmegaGUI.Properties.Resources;

namespace OmegaGUI.Model
{
    /// <summary>
    /// A scroll bar control
    /// </summary>
    public class ScrollBar : Slider
    {
        #region Variables
        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        private Render.ScrollBar _scrollbar;
        #endregion

        #region Properties
        /// <summary>
        /// The current value of the control
        /// </summary>
        [DefaultValue(0), Description("The current value of the control"), Category("Appearance")]
        public override int Value
        {
            get => _scrollbar?.TrackPosition ?? ControlValue;
            set
            {
                if (value < Min || value > Max)
                    throw new InvalidOperationException(Resources.ValueOutOfRange);
                ControlValue = value;
                if (_scrollbar != null) _scrollbar.ScrollTo(value);
            }
        }
        #endregion

        #region Constructor
        public ScrollBar()
        {
            Size = new(20, 150);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            // Add control to dialog
            UpdateLayout();
            DXControl = _scrollbar =
                Parent.DialogRender.AddScrollBar(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, Min, Max, ControlValue, Default);
            ControlModel.IsVisible = IsVisible;
            ControlModel.IsEnabled = IsEnabled;

            // Setup event hooks
            SetupMouseEvents();
        }
        #endregion
    }
}
