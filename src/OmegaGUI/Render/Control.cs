/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NanoByte.Common.Native;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Abstract base class for all control models
    /// </summary>
    public abstract class Control
    {
        public override string ToString()
        {
            return GetType().Name;
        }

        #region Instance data
        protected Dialog parentDialog; // Parent container
        public uint index; // Index within the control list
        public bool IsDefault;

        // Protected members
        protected object localUserData; // User specificied data
        protected bool visible = true;
        protected bool isMouseOver;
        protected bool hasFocus;
        protected int controlId; // ID Number
        protected ControlType ctrlType; // Control type, set in constructor
        protected Keys ctrlHotKey; // Controls hotkey
        protected bool enabled = true;
        protected Rectangle boundingBox; // Rectangle defining the active region of the control

        protected int controlX, controlY, width, height; // Size, scale, and positioning members

        protected List<Element> elementList = []; // All display elements
        #endregion

        public event EventHandler MouseEnter , MouseExit;

        /// <summary>Initialize the control</summary>
        public virtual void OnInitialize()
        {}

        // Nothing to do here

        /// <summary>Render the control</summary>
        public abstract void Render(Device device, float elapsedTime);

        /// <summary>Message Handler</summary>
        public virtual bool MsgProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            return false;
        }

        // Nothing to do here

        /// <summary>Handle the keyboard data</summary>
        public virtual bool HandleKeyboard(WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            return false;
        }

        // Nothing to do here

        /// <summary>Handle the mouse data</summary>
        public virtual bool HandleMouse(WindowMessage msg, Point pt, IntPtr wParam, IntPtr lParam)
        {
            return false;
        }

        // Nothing to do here

        /// <summary>User specified data</summary>
        public object UserData { get => localUserData; set => localUserData = value; }

        /// <summary>The parent dialog of this control</summary>
        public Dialog Parent => parentDialog;

        /// <summary>Can the control have focus</summary>
        public virtual bool CanHaveFocus => false;

        /// <summary>Called when control gets focus</summary>
        public virtual void OnFocusIn()
        {
            hasFocus = true;
        }

        /// <summary>Called when control loses focus</summary>
        public virtual void OnFocusOut()
        {
            hasFocus = false;
        }

        /// <summary>Called when mouse goes over the control</summary>
        public virtual void OnMouseEnter()
        {
            isMouseOver = true;

            if (MouseEnter != null) MouseEnter(this, EventArgs.Empty);
        }

        /// <summary>Called when mouse leaves the control</summary>
        public virtual void OnMouseExit()
        {
            isMouseOver = false;

            if (MouseExit != null) MouseExit(this, EventArgs.Empty);
        }

        /// <summary>Called when the control's hotkey is hit</summary>
        public virtual void OnHotKey()
        {}

        // Nothing to do here
        /// <summary>Does the control contain this point</summary>
        public virtual bool ContainsPoint(Point pt)
        {
            return boundingBox.Contains(pt);
        }

        /// <summary>Is the control enabled</summary>
        public virtual bool IsEnabled { get => enabled; set => enabled = value; }

        /// <summary>Is the control visible</summary>
        public virtual bool IsVisible { get => visible; set => visible = value; }

        /// <summary>Type of the control</summary>
        public virtual ControlType ControlType => ctrlType;

        /// <summary>Unique ID of the control</summary>
        public virtual int ID { get => controlId; set => controlId = value; }

        /// <summary>Called to set control's location</summary>
        public virtual void SetLocation(int x, int y)
        {
            controlX = x;
            controlY = y;
            UpdateRectangles();
        }

        /// <summary>Called to set control's size</summary>
        public virtual void SetSize(int w, int h)
        {
            width = w;
            height = h;
            UpdateRectangles();
        }

        /// <summary>The controls hotkey</summary>
        public virtual Keys Hotkey { get => ctrlHotKey; set => ctrlHotKey = value; }

        /// <summary>
        /// Index for the elements this control has access to
        /// </summary>
        public Element this[uint elementIndex]
        {
            get => elementList[(int)elementIndex];
            set
            {
                if (value == null)
                    throw new InvalidOperationException("You cannot set a null element.");

                // Is the collection big enough?
                for (var i = (uint)elementList.Count; i <= elementIndex; i++)
                {
                    // Add a new one
                    elementList.Add(new());
                }
                // Update the data (with a clone)
                elementList[(int)elementIndex] = value.Clone();
            }
        }

        /// <summary>
        /// Create a new instance of a control
        /// </summary>
        protected Control(Dialog parent)
        {
            parentDialog = parent;
        }

        /// <summary>
        /// Refreshes the control
        /// </summary>
        public virtual void Refresh()
        {
            isMouseOver = false;
            hasFocus = false;
            foreach (var element in elementList) element.Refresh();
        }

        /// <summary>
        /// Updates the rectangles
        /// </summary>
        protected virtual void UpdateRectangles()
        {
            boundingBox = new(controlX, controlY, width, height);
        }
    }
}
