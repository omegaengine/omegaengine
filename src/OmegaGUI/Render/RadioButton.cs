/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Drawing;
using System.Windows.Forms;
using Common.Utils;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Radio button control
    /// </summary>
    [CLSCompliant(false)]
    public class RadioButton : CheckBox
    {
        protected uint buttonGroupIndex;

        /// <summary>
        /// Create new radio button instance
        /// </summary>
        public RadioButton(Dialog parent) : base(parent)
        {
            ctrlType = ControlType.RadioButton;
        }

        /// <summary>
        /// Button Group property
        /// </summary>
        public uint ButtonGroup { get { return buttonGroupIndex; } set { buttonGroupIndex = value; } }

        /// <summary>
        /// Sets the check state and potentially clears the group
        /// </summary>
        public void SetChecked(bool ischecked, bool clear)
        {
            SetCheckedInternal(ischecked, clear, false);
        }

        /// <summary>
        /// Sets the checked state and fires the event if necessary
        /// </summary>
        protected virtual void SetCheckedInternal(bool ischecked, bool clearGroup, bool fromInput)
        {
            isBoxChecked = ischecked;
            RaiseChangedEvent(this, fromInput);
        }

        /// <summary>
        /// Override hotkey to fire event
        /// </summary>
        public override void OnHotKey()
        {
            SetCheckedInternal(true, true);
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
                            parentDialog.ClearRadioButtonGroup(buttonGroupIndex);
                            isBoxChecked = !isBoxChecked;

                            RaiseChangedEvent(this, true);
                        }
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Handle mouse messages from the radio button
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
                        {
                            parentDialog.ClearRadioButtonGroup(buttonGroupIndex);
                            isBoxChecked = !isBoxChecked;

                            RaiseChangedEvent(this, true);
                        }

                        return true;
                    }
                }
                    break;
            }

            return false;
        }
    }
}
