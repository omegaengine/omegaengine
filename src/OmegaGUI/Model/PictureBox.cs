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
using Common.Storage;
using Common.Storage.SlimDX;
using OmegaGUI.Render;

namespace OmegaGUI.Model
{
    public class PictureBox : Control
    {
        #region Properties
        private string _textureFile;

        /// <summary>
        /// The file containing the texture for this picture box - no auto-update
        /// </summary>
        [Description("The file containing the texture for this picture box"), Category("Appearance")]
        public string TextureFile
        {
            get { return _textureFile; }
            set
            {
                _textureFile = value;
                NeedsUpdate();
            }
        }

        [Description("Is the specified texture file name valid?"), Category("Appearance")]
        public bool TextureFileValid { get { return !string.IsNullOrEmpty(_textureFile) && ContentManager.FileExists("GUI/Textures", _textureFile); } }

        private Point _textureLocation = new Point(0, 0);

        /// <summary>
        /// The upper left corner of the area in the texture file to use - no auto-update
        /// </summary>
        [Description("The upper left corner of the area in the texture file to use"), Category("Appearance")]
        public Point TextureLocation
        {
            get { return _textureLocation; }
            set
            {
                _textureLocation = value;
                NeedsUpdate();
            }
        }

        private Size _textureSize = new Size(256, 256);

        /// <summary>
        /// The distance to the lower right corner of the area in the texture file to use - no auto-update
        /// </summary>
        [Description("The distance to the lower right corner of the area in the texture file to use"), Category("Appearance")]
        public Size TextureSize
        {
            get { return _textureSize; }
            set
            {
                _textureSize = value;
                NeedsUpdate();
            }
        }

        private byte _alpha = 255;

        /// <summary>
        /// The level of transparency from 0 (invisible) to 255 (solid)
        /// </summary>
        [DefaultValue((byte)0), Description("The level of transparency from 0 (invisible) to 255 (solid)"), Category("Appearance")]
        public byte Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                if (ControlModel != null)
                    ControlModel[0].TextureColor.States[(int)ControlState.Normal].Alpha = (float)_alpha / 255;
            }
        }
        #endregion

        #region Constructor
        public PictureBox()
        {
            Size = new Size(120, 60);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            if (!TextureFileValid) return;

            // Load custom texture
            uint textureNumber = Parent.CustomTexture++;
            Parent.DialogModel.SetTexture(textureNumber, _textureFile);

            var fill = new Element();
            fill.SetTexture(textureNumber, new Rectangle(_textureLocation, _textureSize));
            fill.TextureColor.States[(int)ControlState.Normal] = Render.Dialog.WhiteColorValue;
            fill.TextureColor.States[(int)ControlState.Normal].Alpha = (float)_alpha / 255;

            // Add control to dialog
            UpdateLayout();
            DXControl = Parent.DialogModel.AddPictureBox(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, fill);
            ControlModel.IsVisible = IsVisible;
            ControlModel.IsEnabled = IsEnabled;

            // Setup event hooks
            SetupMouseEvents();
        }
        #endregion
    }
}
