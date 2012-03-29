/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2012 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using Common.Utils;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Button control
    /// </summary>
    [CLSCompliant(false)]
    public class Button : Label
    {
        public const int ButtonLayer = 0;
        public const int FillLayer = 1;
        protected bool isPressed;

        #region Event code
        public event EventHandler Click;

        /// <summary>Create new button instance</summary>
        protected void RaiseClickEvent(Button sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            if (Click != null)
                Click(sender, EventArgs.Empty);
        }
        #endregion

        /// <summary>Create new button instance</summary>
        public Button(Dialog parent) : base(parent)
        {
            ctrlType = ControlType.Button;
        }

        /// <summary>Can the button have focus</summary>
        public override bool CanHaveFocus { get { return IsVisible && IsEnabled; } }

        /// <summary>The hotkey for this button was pressed</summary>
        public override void OnHotKey()
        {
            RaiseClickEvent(this, true);
        }

        /// <summary>
        /// Will handle the keyboard strokes
        /// </summary>
        public override bool HandleKeyboard(WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch (msg)
            {
                case WindowMessage.KeyDown:
                    if ((Keys)wParam.ToInt32() == Keys.Space)
                    {
                        isPressed = true;
                        return true;
                    }
                    break;
                case WindowMessage.KeyUp:
                    if ((Keys)wParam.ToInt32() == Keys.Space)
                    {
                        isPressed = false;
                        RaiseClickEvent(this, true);

                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Handle mouse messages from the buttons
        /// </summary>
        public override bool HandleMouse(WindowMessage msg, Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch (msg)
            {
                case WindowMessage.LeftButtonDoubleClick:
                case WindowMessage.LeftButtonDown:
                {
                    if (ContainsPoint(pt))
                    {
                        // Pressed while inside the control
                        isPressed = true;
                        parentDialog.DialogManager.Target.Capture = true;
                        if (!hasFocus)
                            Dialog.RequestFocus(this);

                        return true;
                    }
                }
                    break;
                case WindowMessage.LeftButtonUp:
                {
                    if (isPressed)
                    {
                        isPressed = false;
                        parentDialog.DialogManager.Target.Capture = false;
                        if (!parentDialog.IsUsingKeyboardInput)
                            Dialog.ClearFocus();

                        // Button click
                        if (ContainsPoint(pt))
                            RaiseClickEvent(this, true);
                    }
                }
                    break;
            }

            return false;
        }

        /// <summary>Render the button</summary>
        public override void Render(Device device, float elapsedTime)
        {
            int offsetX = 0;
            int offsetY = 0;

            ControlState state = ControlState.Normal;
            if (IsVisible == false)
                state = ControlState.Hidden;
            else if (IsEnabled == false)
                state = ControlState.Disabled;
            else if (isPressed)
            {
                state = ControlState.Pressed;
                offsetX = 1;
                offsetY = 2;
            }
            else if (isMouseOver)
            {
                state = ControlState.MouseOver;
                offsetX = -1;
                offsetY = -2;
            }
            else if (hasFocus)
                state = ControlState.Focus;

            // Background fill layer
            Element e = elementList[ButtonLayer];
            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            Rectangle buttonRect = boundingBox;
            buttonRect.Offset(offsetX, offsetY);

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite/text of button
            parentDialog.DrawSprite(e, buttonRect);
            parentDialog.DrawText(textData, e, buttonRect);

            // Main button
            e = elementList[FillLayer];

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            parentDialog.DrawSprite(e, buttonRect);
            parentDialog.DrawText(textData, e, buttonRect);
        }
    }
}
