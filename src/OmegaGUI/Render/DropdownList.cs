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
using OmegaEngine;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// Dropdown list control
    /// </summary>
    public class DropdownList : Button
    {
        public const int MainLayer = 0;
        public const int ComboButtonLayer = 1;
        public const int DropdownLayer = 2;
        public const int SelectionLayer = 3;

        #region Event code
        public event EventHandler Changed;

        /// <summary>Create new button instance</summary>
        protected void RaiseChangedEvent(DropdownList sender, bool wasTriggeredByUser)
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

        private bool isScrollBarInit;

        #region Instance data
        protected int selectedIndex;
        protected int focusedIndex;
        protected int dropHeight;
        protected ScrollBar scrollbarControl;
        protected int scrollWidth;
        protected bool isComboOpen;
        protected Rectangle textRect;
        protected Rectangle buttonRect;
        protected Rectangle dropDownRect;
        protected Rectangle dropDownTextRect;

        protected List<ListItem> itemList = [];
        #endregion

        /// <summary>Create new dropdown list control</summary>
        public DropdownList(Dialog parent)
            : base(parent)
        {
            ctrlType = ControlType.DropdownList;

            // Create the scrollbar control too
            scrollbarControl = new(parent);

            // Set some default items
            dropHeight = 100;
            scrollWidth = 16;
            selectedIndex = -1;
            focusedIndex = -1;
        }

        /// <summary>Update the rectangles for the dropdown list control</summary>
        protected override void UpdateRectangles()
        {
            // Get bounding box
            base.UpdateRectangles();

            // Update the bounding box for the items
            buttonRect = new(boundingBox.Right - boundingBox.Height, boundingBox.Top,
                boundingBox.Height, boundingBox.Height);

            textRect = boundingBox;
            textRect.Size = new(textRect.Width - buttonRect.Width, textRect.Height);

            dropDownRect = textRect;
            dropDownRect.Offset(0, (int)(0.9f * textRect.Height));
            dropDownRect.Size = new(dropDownRect.Width - scrollWidth, dropDownRect.Height + dropHeight);

            // Scale it down slightly
            Point loc = dropDownRect.Location;
            Size size = dropDownRect.Size;

            loc.X += (int)(0.1f * dropDownRect.Width);
            loc.Y += (int)(0.1f * dropDownRect.Height);
            size.Width -= (2 * (int)(0.1f * dropDownRect.Width));
            size.Height -= (2 * (int)(0.1f * dropDownRect.Height));

            dropDownTextRect = new(loc, size);

            // Update the scroll bars rects too
            scrollbarControl.SetLocation(dropDownRect.Right, dropDownRect.Top + 2);
            scrollbarControl.SetSize(scrollWidth, dropDownRect.Height - 2);
            FontNode fNode = parentDialog.DialogManager.GetFontNode(
                (int)(elementList[2]).FontIndex);
            if (fNode is { Height: > 0 })
            {
                scrollbarControl.PageSize = (int)(dropDownTextRect.Height / fNode.Height);

                // The selected item may have been scrolled off the page.
                // Ensure that it is in page again.
                scrollbarControl.ShowItem(selectedIndex);
            }
        }

        /// <summary>Sets the drop height of this control</summary>
        public void SetDropHeight(int height)
        {
            dropHeight = height;
            UpdateRectangles();
        }

        /// <summary>Sets the scroll bar width of this control</summary>
        public void SetScrollbarWidth(int width)
        {
            scrollWidth = width;
            UpdateRectangles();
        }

        /// <summary>Can this control have focus</summary>
        public override bool CanHaveFocus => (IsVisible && IsEnabled);

        /// <summary>Number of items current in the list</summary>
        public int NumberItems => itemList.Count;

        /// <summary>Indexer for items in the list</summary>
        public ListItem this[int index] => itemList[index];

        /// <summary>Initialize the scrollbar control here</summary>
        public override void OnInitialize()
        {
            parentDialog.InitializeControl(scrollbarControl);
        }

        /// <summary>Called when focus leaves the control</summary>
        public override void OnFocusOut()
        {
            // Call base first
            base.OnFocusOut();
            isComboOpen = false;
        }

        /// <summary>Called when the control's hotkey is pressed</summary>
        public override void OnHotKey()
        {
            if (isComboOpen)
                return; // Nothing to do yet

            if (selectedIndex == -1)
                return; // Nothing selected

            selectedIndex++;
            if (selectedIndex >= itemList.Count)
                selectedIndex = 0;

            focusedIndex = selectedIndex;
            RaiseChangedEvent(this, true);
        }

        /// <summary>Called when the control needs to handle the keyboard</summary>
        public override bool HandleKeyboard(WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            const uint RepeatMask = (0x40000000);

            if (!IsEnabled || !IsVisible)
                return false;

            // Let the scroll bar have a chance to handle it first
            if (scrollbarControl.HandleKeyboard(msg, wParam, lParam))
                return true;

            switch (msg)
            {
                case WindowMessage.KeyDown:
                {
                    switch ((Keys)wParam.ToInt32())
                    {
                        case Keys.Return:
                        {
                            if (isComboOpen)
                            {
                                if (selectedIndex != focusedIndex)
                                {
                                    selectedIndex = focusedIndex;
                                    RaiseChangedEvent(this, true);
                                }
                                isComboOpen = false;

                                if (!Parent.IsUsingKeyboardInput)
                                    Dialog.ClearFocus();

                                return true;
                            }
                            break;
                        }
                        case Keys.F4:
                        {
                            // Filter out auto repeats
                            if ((lParam.ToInt32() & RepeatMask) != 0)
                                return true;

                            isComboOpen = !isComboOpen;
                            if (!isComboOpen)
                            {
                                RaiseChangedEvent(this, true);

                                if (!Parent.IsUsingKeyboardInput)
                                    Dialog.ClearFocus();
                            }

                            return true;
                        }
                        case Keys.Left:
                        case Keys.Up:
                        {
                            if (focusedIndex > 0)
                            {
                                focusedIndex--;
                                selectedIndex = focusedIndex;
                                if (!isComboOpen)
                                    RaiseChangedEvent(this, true);
                            }
                            return true;
                        }
                        case Keys.Right:
                        case Keys.Down:
                        {
                            if (focusedIndex + 1 < NumberItems)
                            {
                                focusedIndex++;
                                selectedIndex = focusedIndex;
                                if (!isComboOpen)
                                    RaiseChangedEvent(this, true);
                            }
                            return true;
                        }
                    }
                    break;
                }
            }

            return false;
        }

        /// <summary>Called when the control should handle the mouse</summary>
        public override bool HandleMouse(WindowMessage msg, Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false; // Nothing to do

            // Let the scroll bar handle it first
            if (scrollbarControl.HandleMouse(msg, pt, wParam, lParam))
                return true;

            unchecked
            {
                // Ok, scrollbar didn't handle it, move on
                switch (msg)
                {
                    case WindowMessage.MouseMove:
                    {
                        if (isComboOpen && dropDownRect.Contains(pt))
                        {
                            // Determine which item has been selected
                            for (int i = 0; i < itemList.Count; i++)
                            {
                                ListItem cbi = itemList[i];
                                if (cbi.IsItemVisible && cbi.ItemRect.Contains(pt))
                                    focusedIndex = i;
                            }
                            return true;
                        }
                        break;
                    }
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

                            // Toggle dropdown
                            if (hasFocus)
                            {
                                isComboOpen = !isComboOpen;
                                if (!isComboOpen)
                                {
                                    if (!parentDialog.IsUsingKeyboardInput)
                                        Dialog.ClearFocus();
                                }
                            }

                            return true;
                        }

                        // Perhaps this click is within the dropdown
                        if (isComboOpen && dropDownRect.Contains(pt))
                        {
                            // Determine which item has been selected
                            for (int i = scrollbarControl.TrackPosition; i < itemList.Count; i++)
                            {
                                ListItem cbi = itemList[i];
                                if (cbi.IsItemVisible && cbi.ItemRect.Contains(pt))
                                {
                                    selectedIndex = focusedIndex = i;
                                    RaiseChangedEvent(this, true);

                                    isComboOpen = false;

                                    if (!parentDialog.IsUsingKeyboardInput)
                                        Dialog.ClearFocus();

                                    break;
                                }
                            }
                            return true;
                        }
                        // Mouse click not on main control or in dropdown, fire an event if needed
                        if (isComboOpen)
                        {
                            focusedIndex = selectedIndex;
                            RaiseChangedEvent(this, true);
                            isComboOpen = false;
                        }

                        // Make sure the control is no longer 'pressed'
                        isPressed = false;

                        // Release focus if appropriate
                        if (!parentDialog.IsUsingKeyboardInput)
                            Dialog.ClearFocus();

                        break;
                    }
                    case WindowMessage.LeftButtonUp:
                    {
                        if (isPressed && ContainsPoint(pt))
                        {
                            // Button click
                            isPressed = false;
                            parentDialog.DialogManager.Target.Capture = false;
                            return true;
                        }
                        break;
                    }
                    case WindowMessage.MouseWheel:
                    {
                        int zdelta = MathUtils.HiWord((uint)wParam.ToInt32()) / Dialog.WheelDelta;
                        if (isComboOpen)
                            scrollbarControl.Scroll(-zdelta * SystemInformation.MouseWheelScrollLines);
                        else
                        {
                            if (zdelta > 0)
                            {
                                if (focusedIndex > 0)
                                {
                                    focusedIndex--;
                                    selectedIndex = focusedIndex;
                                    if (!isComboOpen)
                                        RaiseChangedEvent(this, true);
                                }
                            }
                            else
                            {
                                if (focusedIndex + 1 < NumberItems)
                                {
                                    focusedIndex++;
                                    selectedIndex = focusedIndex;
                                    if (!isComboOpen)
                                        RaiseChangedEvent(this, true);
                                }
                            }
                        }
                        return true;
                    }
                }
            }

            // Didn't handle it
            return false;
        }

        /// <summary>Called when the control should be rendered</summary>
        public override void Render(Device device, float elapsedTime)
        {
            var state = ControlState.Normal;
            if (!isComboOpen)
                state = ControlState.Hidden;

            // Dropdown box
            Element e = elementList[DropdownLayer];

            // If we have not initialized the scroll bar page size,
            // do that now.
            if (!isScrollBarInit)
            {
                FontNode fNode = parentDialog.DialogManager.GetFontNode((int)e.FontIndex);
                if (fNode is { Height: > 0 })
                    scrollbarControl.PageSize = (int)(dropDownTextRect.Height / fNode.Height);
                else
                    scrollbarControl.PageSize = dropDownTextRect.Height;

                isScrollBarInit = true;
            }

            if (isComboOpen)
            {
                using (new ProfilerEvent(() => "Render GUI control: " + scrollbarControl))
                    scrollbarControl.Render(device, elapsedTime);
            }

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime);
            e.FontColor.Blend(state, elapsedTime);
            parentDialog.DrawSprite(e, dropDownRect);

            // Selection outline
            Element selectionElement = elementList[SelectionLayer];
            selectionElement.TextureColor.Current = e.TextureColor.Current;
            selectionElement.FontColor.Current = selectionElement.FontColor.States[(int)ControlState.Normal];

            FontNode font = parentDialog.DialogManager.GetFontNode((int)e.FontIndex);
            int currentY = dropDownTextRect.Top;
            int remainingHeight = dropDownTextRect.Height;

            for (int i = scrollbarControl.TrackPosition; i < itemList.Count; i++)
            {
                ListItem cbi = itemList[i];

                // Make sure there's room left in the dropdown
                remainingHeight -= (int)font.Height;
                if (remainingHeight < 0)
                {
                    // Not visible, store that item
                    cbi.IsItemVisible = false;
                    itemList[i] = cbi; // Store this back in list
                    continue;
                }

                cbi.ItemRect = new(dropDownTextRect.Left, currentY,
                    dropDownTextRect.Width, (int)font.Height);
                cbi.IsItemVisible = true;
                currentY += (int)font.Height;
                itemList[i] = cbi; // Store this back in list

                if (isComboOpen)
                {
                    if (focusedIndex == i)
                    {
                        var rect = new Rectangle(
                            dropDownRect.Left, cbi.ItemRect.Top - 2, dropDownRect.Width,
                            cbi.ItemRect.Height + 4);
                        parentDialog.DrawSprite(selectionElement, rect);
                        parentDialog.DrawText(cbi.ItemText, selectionElement, cbi.ItemRect);
                    }
                    else
                        parentDialog.DrawText(cbi.ItemText, e, cbi.ItemRect);
                }
            }

            int offsetX = 0;
            int offsetY = 0;

            state = ControlState.Normal;
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

            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            // Button
            e = elementList[ComboButtonLayer];

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);

            Rectangle windowRect = buttonRect;
            windowRect.Offset(offsetX, offsetY);
            // Draw sprite
            parentDialog.DrawSprite(e, windowRect);

            if (isComboOpen)
                state = ControlState.Pressed;

            // Main text box
            e = elementList[MainLayer];

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            e.FontColor.Blend(state, elapsedTime, blendRate);

            // Draw sprite
            parentDialog.DrawSprite(e, textRect);

            if (selectedIndex >= 0 && selectedIndex < itemList.Count)
            {
                try
                {
                    ListItem cbi = itemList[selectedIndex];
                    parentDialog.DrawText(cbi.ItemText, e, new(
                        new(textRect.Left + 5, textRect.Top),
                        new(textRect.Width - 10, textRect.Height)));
                }
                catch (ArgumentOutOfRangeException)
                {} // Ignore
            }
        }

        #region Item Controlling methods
        /// <summary>Adds an item to the dropdown list control</summary>
        public void AddItem(string text, string tag, object data)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            // Create a new item and add it
            var newitem = new ListItem {ItemText = text, ItemTag = tag, ItemData = data};
            itemList.Add(newitem);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);

            // If this is the only item in the list, it should be selected
            if (NumberItems == 1)
            {
                selectedIndex = 0;
                focusedIndex = 0;
                RaiseChangedEvent(this, false);
            }
        }

        /// <summary>Removes an item at a particular index</summary>
        public void RemoveAt(int index)
        {
            // Remove the item
            itemList.RemoveAt(index);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);

            if (selectedIndex >= itemList.Count)
                selectedIndex = itemList.Count - 1;
        }

        /// <summary>Removes all items from the control</summary>
        public void Clear()
        {
            // clear the list
            itemList.Clear();

            // Update scroll bar and index
            scrollbarControl.SetTrackRange(0, 1);
            focusedIndex = selectedIndex = -1;
        }

        /// <summary>Determines whether this control contains an item</summary>
        public bool ContainsItem(string text, int start)
        {
            return (FindItem(text, start) != -1);
        }

        /// <summary>Determines whether this control contains an item</summary>
        public bool ContainsItem(string text)
        {
            return ContainsItem(text, 0);
        }

        /// <summary>Gets the data for the selected item</summary>
        public object GetSelectedData()
        {
            if (selectedIndex < 0)
                return null; // Nothing selected

            ListItem cbi = itemList[selectedIndex];
            return cbi.ItemData;
        }

        /// <summary>Gets the selected item</summary>
        public ListItem GetSelectedItem()
        {
            if (selectedIndex < 0)
                throw new InvalidOperationException("No item selected.");

            return itemList[selectedIndex];
        }

        /// <summary>Gets the data for an item</summary>
        public object GetItemData(string text)
        {
            int i = FindItem(text);
            if (i == -1)
                return null; // no item

            ListItem cbi = itemList[i];
            return cbi.ItemData;
        }

        /// <summary>Finds an item in the list and returns the index</summary>
        protected int FindItem(string text, int start)
        {
            if (string.IsNullOrEmpty(text))
                return -1;

            for (int i = start; i < itemList.Count; i++)
            {
                ListItem cbi = itemList[i];
                if (string.Compare(cbi.ItemText, text, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return i;
            }

            // Never found it
            return -1;
        }

        /// <summary>Finds an item in the list and returns the index</summary>
        protected int FindItem(string text)
        {
            return FindItem(text, 0);
        }

        /// <summary>Sets the selected item by index</summary>
        public void SetSelected(int index)
        {
            if (index >= NumberItems)
                throw new ArgumentOutOfRangeException(nameof(index), "There are not enough items in the list to select this index.");

            focusedIndex = selectedIndex = index;
            RaiseChangedEvent(this, false);
        }

        /// <summary>Sets the selected item by text</summary>
        public void SetSelected(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            int i = FindItem(text);
            if (i == -1)
                throw new InvalidOperationException("This item could not be found.");

            focusedIndex = selectedIndex = i;
            RaiseChangedEvent(this, false);
        }
        #endregion
    }
}
