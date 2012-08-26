/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2012 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    [CLSCompliant(false)]
    public class PictureBox : Control
    {
        /// <summary>Create new picture box instance</summary>
        public PictureBox(Dialog parent, Element fill) : base(parent)
        {
            ctrlType = ControlType.PictureBox;
            this[0] = fill;
        }

        /// <summary>Render the picture box</summary>
        public override void Render(Device device, float elapsedTime)
        {
            var state = ControlState.Normal;
            if (IsVisible == false)
                state = ControlState.Hidden;
            else if (IsEnabled == false)
                state = ControlState.Disabled;
            else if (isMouseOver)
                state = ControlState.MouseOver;
            else if (hasFocus)
                state = ControlState.Focus;

            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            Element e = elementList[0];

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, boundingBox);
        }
    }
}
