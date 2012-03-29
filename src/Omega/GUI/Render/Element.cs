/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2012 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Contains all the display tweakables for a control
    /// </summary>
    [CLSCompliant(false)]
    public class Element : ICloneable
    {
        #region Instance Data
        public uint TextureIndex; // Index of the texture for this Element 
        public uint FontIndex; // Index of the font for this Element 
        public DrawTextFormat textFormat; // The Format argument to draw text

        public Rectangle textureRect; // Bounding rectangle of this element on the composite texture

        public BlendColor TextureColor;
        public BlendColor FontColor;
        #endregion

        #region Textures
        /// <summary>
        /// Set the texture
        /// </summary>
        public void SetTexture(uint tex, Rectangle texRect, Color4 defaultTextureColor)
        {
            // Store data
            TextureIndex = tex;
            textureRect = texRect;
            TextureColor.Initialize(defaultTextureColor);
        }

        /// <summary>
        /// Set the texture
        /// </summary>
        public void SetTexture(uint tex, Rectangle texRect)
        {
            SetTexture(tex, texRect, Dialog.WhiteColorValue);
        }
        #endregion

        #region Fonts
        /// <summary>Set the font</summary>
        public void SetFont(uint font, Color4 defaultFontColor, DrawTextFormat format)
        {
            // Store data
            FontIndex = font;
            textFormat = format | DrawTextFormat.ExpandTabs | DrawTextFormat.WordBreak;
            FontColor.Initialize(defaultFontColor);
        }
        #endregion

        #region Refresh
        /// <summary>
        /// Refresh this element
        /// </summary>
        public void Refresh()
        {
            if (TextureColor.States != null)
                TextureColor.Current = TextureColor.States[(int)ControlState.Hidden];
            if (FontColor.States != null)
                FontColor.Current = FontColor.States[(int)ControlState.Hidden];
        }
        #endregion

        #region ICloneable Members
        /// <summary>Clone an object</summary>
        public Element Clone()
        {
            var e = new Element
            {
                TextureIndex = TextureIndex,
                FontIndex = FontIndex,
                textFormat = textFormat,
                textureRect = textureRect,
                TextureColor = TextureColor,
                FontColor = FontColor
            };

            return e;
        }

        /// <summary>Clone an object</summary>
        object ICloneable.Clone()
        {
            throw new NotSupportedException("Use the strongly typed clone.");
        }
        #endregion
    }
}
