/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Common.Utils;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// CheckBox control
    /// </summary>
    [CLSCompliant(false)]
    public class CheckBox : Button
    {
        public const int BoxLayer = 0;
        public const int CheckLayer = 1;

        #region Event code
        public event EventHandler Changed;

        /// <summary>Create new button instance</summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        protected void RaiseChangedEvent(CheckBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire both the changed and clicked event
            RaiseClickEvent(sender, wasTriggeredByUser);
            if (Changed != null)
                Changed(sender, EventArgs.Empty);
        }
        #endregion

        protected Rectangle buttonRect;
        protected Rectangle textRect;
        protected bool isBoxChecked;

        /// <summary>
        /// Create new checkbox instance
        /// </summary>
        public CheckBox(Dialog parent) : base(parent)
        {
            ctrlType = ControlType.CheckBox;
        }

        /// <summary>
        /// Checked property
        /// </summary>
        public virtual bool IsChecked { get { return isBoxChecked; } set { SetCheckedInternal(value, false); } }

        /// <summary>
        /// Sets the checked state and fires the event if necessary
        /// </summary>
        protected virtual void SetCheckedInternal(bool ischecked, bool fromInput)
        {
            isBoxChecked = ischecked;
            RaiseChangedEvent(this, fromInput);
        }

        /// <summary>
        /// Override hotkey to fire event
        /// </summary>
        public override void OnHotKey()
        {
            SetCheckedInternal(!isBoxChecked, true);
        }

        /// <summary>
        /// Does the control contain the point?
        /// </summary>
        public override bool ContainsPoint(Point pt)
        {
            return (boundingBox.Contains(pt) || buttonRect.Contains(pt));
        }

        /// <summary>
        /// Update the rectangles
        /// </summary>
        protected override void UpdateRectangles()
        {
            // Update base first
            base.UpdateRectangles();

            // Update the two rects
            buttonRect = boundingBox;
            buttonRect = new Rectangle(boundingBox.Location,
                new Size(boundingBox.Height, boundingBox.Height));

            textRect = boundingBox;
            textRect.Offset((int)(1.25f * buttonRect.Width), 0);
        }

        /// <summary>
        /// Render the checkbox control
        /// </summary>
        public override void Render(Device device, float elapsedTime)
        {
            var state = ControlState.Normal;
            if (IsVisible == false)
                state = ControlState.Hidden;
            else if (IsEnabled == false)
                state = ControlState.Disabled;
            else if (isPressed)
                state = ControlState.Pressed;
            else if (isMouseOver)
                state = ControlState.MouseOver;
            else if (hasFocus)
                state = ControlState.Focus;

            Element e = elementList[BoxLayer];
            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite/text of checkbox
            parentDialog.DrawSprite(e, buttonRect);
            parentDialog.DrawText(textData, e, textRect);

            if (!isBoxChecked)
                state = ControlState.Hidden;

            e = elementList[CheckLayer];
            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite of checkbox
            parentDialog.DrawSprite(e, buttonRect);
        }

        /// <summary>
        /// Handle the keyboard for the checkbox
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
                        if (isPressed)
                        {
                            isPressed = false;
                            SetCheckedInternal(!isBoxChecked, true);
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Handle mouse messages from the checkbox
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
                        if ((!hasFocus) && (parentDialog.IsUsingKeyboardInput))
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

                        // Button click
                        if (ContainsPoint(pt))
                            SetCheckedInternal(!isBoxChecked, true);

                        return true;
                    }
                }
                    break;
            }

            return false;
        }
    }
}
