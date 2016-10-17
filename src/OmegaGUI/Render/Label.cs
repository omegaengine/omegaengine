/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Label text control
    /// </summary>
    [CLSCompliant(false)]
    public class Label : Control
    {
        protected string textData; // Window text

        public TextAlign TextAlign { get; set; }

        /// <summary>
        /// Create a new instance of a static text control
        /// </summary>
        public Label(Dialog parent) : base(parent)
        {
            ctrlType = ControlType.Label;
            textData = string.Empty;
        }

        /// <summary>
        /// Render this control
        /// </summary>
        public override void Render(Device device, float elapsedTime)
        {
            if (!IsVisible)
                return; // Nothing to do here

            var state = ControlState.Normal;
            if (!IsEnabled)
                state = ControlState.Disabled;

            // Blend the element colors
            Element e = elementList[(int)TextAlign];
            e.FontColor.Blend(state, elapsedTime);

            // Render with a shadow
            parentDialog.DrawText(textData, e, boundingBox, true);
        }

        /// <summary>
        /// Return a copy of the string
        /// </summary>
        public string GetTextCopy()
        {
            return string.Copy(textData);
        }

        /// <summary>
        /// Sets the updated text for this control
        /// </summary>
        public void SetText(string newText)
        {
            textData = newText;
        }
    }
}
