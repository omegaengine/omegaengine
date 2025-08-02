/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Native;
using OmegaEngine;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// List box control
    /// </summary>
    public class ListBox : Control
    {
        public const int MainLayer = 0;
        public const int SelectionLayer = 1;

        #region Event code
        public event EventHandler DoubleClick;
        public event EventHandler Selection;

        /// <summary>Raises the double click event</summary>
        protected void RaiseDoubleClickEvent(ListBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire the event
            if (DoubleClick != null)
                DoubleClick(sender, EventArgs.Empty);
        }

        /// <summary>Raises the selection event</summary>
        protected void RaiseSelectionEvent(ListBox sender, bool wasTriggeredByUser)
        {
            // Discard events triggered programatically if these types of events haven't been
            // enabled
            if (!Parent.IsUsingNonUserEvents && !wasTriggeredByUser)
                return;

            // Fire the event
            if (Selection != null)
                Selection(sender, EventArgs.Empty);
        }
        #endregion

        #region Instance data
        private bool isScrollBarInit;
        protected Rectangle textRect; // Text rendering bound
        protected Rectangle selectionRect; // Selection box bound
        protected ScrollBar scrollbarControl;
        protected int scrollWidth = 16;
        protected int margin = 5;
        protected int border = 6;
        protected int textHeight; // Height of a single line of text
        protected int selectedIndex = -1;
        protected int selectedStarted;
        protected bool isDragging;
        protected ListBoxStyle ctrlStyle = ListBoxStyle.SingleSelection;

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "There will never be any need for a different collection type")]
        protected List<ListItem> itemList = [];
        #endregion

        /// <summary>Create a new list box control</summary>
        public ListBox(Dialog parent) : base(parent)
        {
            ctrlType = ControlType.ListBox;

            // Create the scrollbar control too
            scrollbarControl = new(parent);
        }

        /// <summary>Update the rectangles for the list box control</summary>
        protected override void UpdateRectangles()
        {
            // Get bounding box
            base.UpdateRectangles();

            // Calculate the size of the selection rectangle
            selectionRect = boundingBox;
            selectionRect.Width -= scrollWidth;
            selectionRect.Inflate(-border, -border);
            textRect = selectionRect;
            textRect.Inflate(-margin, 0);

            // Update the scroll bars rects too
            scrollbarControl.SetLocation(boundingBox.Right - scrollWidth, boundingBox.Top);
            scrollbarControl.SetSize(scrollWidth, height);
            FontNode fNode = parentDialog.DialogManager.GetFontNode((int)elementList[0].FontIndex);
            if (fNode is { Height: > 0 })
            {
                scrollbarControl.PageSize = (int)(textRect.Height / fNode.Height);

                // The selected item may have been scrolled off the page.
                // Ensure that it is in page again.
                scrollbarControl.ShowItem(selectedIndex);
            }
        }

        /// <summary>Sets the scroll bar width of this control</summary>
        public void SetScrollbarWidth(int width)
        {
            scrollWidth = width;
            UpdateRectangles();
        }

        /// <summary>Can this control have focus</summary>
        public override bool CanHaveFocus => (IsVisible && IsEnabled);

        /// <summary>Sets the style of the listbox</summary>
        public ListBoxStyle Style { get => ctrlStyle; set => ctrlStyle = value; }

        /// <summary>Number of items current in the list</summary>
        public int NumberItems => itemList.Count;

        /// <summary>Indexer for items in the list</summary>
        public ListItem this[int index] => itemList[index];

        /// <summary>Initialize the scrollbar control here</summary>
        public override void OnInitialize()
        {
            parentDialog.InitializeControl(scrollbarControl);
        }

        /// <summary>Called when the control needs to handle the keyboard</summary>
        public override bool HandleKeyboard(WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
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
                        case Keys.Up:
                        case Keys.Down:
                        case Keys.Next:
                        case Keys.Prior:
                        case Keys.Home:
                        case Keys.End:
                        {
                            // If no items exists, do nothing
                            if (itemList.Count == 0)
                                return true;

                            int oldSelected = selectedIndex;

                            // Adjust selectedIndex
                            switch ((Keys)wParam.ToInt32())
                            {
                                case Keys.Up:
                                    --selectedIndex;
                                    break;
                                case Keys.Down:
                                    ++selectedIndex;
                                    break;
                                case Keys.Next:
                                    selectedIndex += scrollbarControl.PageSize - 1;
                                    break;
                                case Keys.Prior:
                                    selectedIndex -= scrollbarControl.PageSize - 1;
                                    break;
                                case Keys.Home:
                                    selectedIndex = 0;
                                    break;
                                case Keys.End:
                                    selectedIndex = itemList.Count - 1;
                                    break;
                            }

                            // Clamp the item
                            if (selectedIndex < 0)
                                selectedIndex = 0;
                            if (selectedIndex >= itemList.Count)
                                selectedIndex = itemList.Count - 1;

                            // Did the selection change?
                            if (oldSelected != selectedIndex)
                            {
                                if (ctrlStyle == ListBoxStyle.MultiSelection)
                                {
                                    // Clear all selection
                                    for (int i = 0; i < itemList.Count; i++)
                                    {
                                        ListItem lbi = itemList[i];
                                        lbi.IsItemSelected = false;
                                        itemList[i] = lbi;
                                    }

                                    // Is shift being held down?
                                    bool shiftDown = WinFormsUtils.IsKeyDown(Keys.ShiftKey);

                                    if (shiftDown)
                                    {
                                        // Select all items from the start selection to current selected index
                                        int end = Math.Max(selectedStarted, selectedIndex);
                                        for (int i = Math.Min(selectedStarted, selectedIndex); i <= end; ++i)
                                        {
                                            ListItem lbi = itemList[i];
                                            lbi.IsItemSelected = true;
                                            itemList[i] = lbi;
                                        }
                                    }
                                    else
                                    {
                                        ListItem lbi = itemList[selectedIndex];
                                        lbi.IsItemSelected = true;
                                        itemList[selectedIndex] = lbi;

                                        // Update selection start
                                        selectedStarted = selectedIndex;
                                    }
                                }
                                else // Update selection start
                                    selectedStarted = selectedIndex;

                                // adjust scrollbar
                                scrollbarControl.ShowItem(selectedIndex);
                                RaiseSelectionEvent(this, true);
                            }
                        }
                            return true;
                    }
                    break;
                }
            }

            return false;
        }

        /// <summary>Called when the control should handle the mouse</summary>
        public override bool HandleMouse(WindowMessage msg, Point pt, IntPtr wParam, IntPtr lParam)
        {
            const int ShiftModifier = 0x0004;
            const int ControlModifier = 0x0008;

            if (!IsEnabled || !IsVisible)
                return false; // Nothing to do

            // First acquire focus
            if (msg == WindowMessage.LeftButtonDown)
            {
                if (!hasFocus)
                    Dialog.RequestFocus(this);
            }

            // Let the scroll bar handle it first
            if (scrollbarControl.HandleMouse(msg, pt, wParam, lParam))
                return true;

            // Ok, scrollbar didn't handle it, move on
            switch (msg)
            {
                case WindowMessage.LeftButtonDoubleClick:
                case WindowMessage.LeftButtonDown:
                {
                    // Check for clicks in the text area
                    if (itemList.Count > 0 && selectionRect.Contains(pt))
                    {
                        // Compute the index of the clicked item
                        int clicked;
                        if (textHeight > 0)
                            clicked = scrollbarControl.TrackPosition + (pt.Y - textRect.Top) / textHeight;
                        else
                            clicked = -1;

                        // Only proceed if the click falls ontop of an item
                        if (clicked >= scrollbarControl.TrackPosition &&
                            clicked < itemList.Count &&
                            clicked < scrollbarControl.TrackPosition + scrollbarControl.PageSize)
                        {
                            parentDialog.DialogManager.Target.Capture = true;
                            isDragging = true;

                            // If this is a double click, fire off an event and exit
                            // since the first click would have taken care of the selection
                            // updating.
                            if (msg == WindowMessage.LeftButtonDoubleClick)
                            {
                                RaiseDoubleClickEvent(this, true);
                                return true;
                            }

                            selectedIndex = clicked;
                            if ((wParam.ToInt32() & ShiftModifier) == 0)
                                selectedStarted = selectedIndex; // Shift isn't down

                            // If this is a multi-selection listbox, update per-item
                            // selection data.
                            if (ctrlStyle == ListBoxStyle.MultiSelection)
                            {
                                // Determine behavior based on the state of Shift and Ctrl
                                ListItem selectedItem = itemList[selectedIndex];
                                if ((wParam.ToInt32() & (ShiftModifier | ControlModifier)) == ControlModifier)
                                {
                                    // Control click, reverse the selection
                                    selectedItem.IsItemSelected = !selectedItem.IsItemSelected;
                                    itemList[selectedIndex] = selectedItem;
                                }
                                else if ((wParam.ToInt32() & (ShiftModifier | ControlModifier)) == ShiftModifier)
                                {
                                    // Shift click. Set the selection for all items
                                    // from last selected item to the current item.
                                    // Clear everything else.
                                    int begin = Math.Min(selectedStarted, selectedIndex);
                                    int end = Math.Max(selectedStarted, selectedIndex);

                                    // Unselect everthing before the beginning
                                    for (int i = 0; i < begin; ++i)
                                    {
                                        ListItem lb = itemList[i];
                                        lb.IsItemSelected = false;
                                        itemList[i] = lb;
                                    }
                                    // unselect everything after the end
                                    for (int i = end + 1; i < itemList.Count; ++i)
                                    {
                                        ListItem lb = itemList[i];
                                        lb.IsItemSelected = false;
                                        itemList[i] = lb;
                                    }

                                    // Select everything between
                                    for (int i = begin; i <= end; ++i)
                                    {
                                        ListItem lb = itemList[i];
                                        lb.IsItemSelected = true;
                                        itemList[i] = lb;
                                    }
                                }
                                else if ((wParam.ToInt32() & (ShiftModifier | ControlModifier)) == (ShiftModifier | ControlModifier))
                                {
                                    // Control-Shift-click.

                                    // The behavior is:
                                    //   Set all items from selectedStarted to selectedIndex to
                                    //     the same state as selectedStarted, not including selectedIndex.
                                    //   Set selectedIndex to selected.
                                    int begin = Math.Min(selectedStarted, selectedIndex);
                                    int end = Math.Max(selectedStarted, selectedIndex);

                                    // The two ends do not need to be set here.
                                    bool isLastSelected = itemList[selectedStarted].IsItemSelected;

                                    for (int i = begin + 1; i < end; ++i)
                                    {
                                        ListItem lb = itemList[i];
                                        lb.IsItemSelected = isLastSelected;
                                        itemList[i] = lb;
                                    }

                                    selectedItem.IsItemSelected = true;
                                    itemList[selectedIndex] = selectedItem;

                                    // Restore selectedIndex to the previous value
                                    // This matches the Windows behavior

                                    selectedIndex = selectedStarted;
                                }
                                else
                                {
                                    // Simple click.  Clear all items and select the clicked
                                    // item.
                                    for (int i = 0; i < itemList.Count; ++i)
                                    {
                                        ListItem lb = itemList[i];
                                        lb.IsItemSelected = false;
                                        itemList[i] = lb;
                                    }
                                    selectedItem.IsItemSelected = true;
                                    itemList[selectedIndex] = selectedItem;
                                }
                            } // End of multi-selection case
                            RaiseSelectionEvent(this, true);
                        }
                        return true;
                    }
                    break;
                }
                case WindowMessage.LeftButtonUp:
                {
                    parentDialog.DialogManager.Target.Capture = false;
                    isDragging = false;

                    if (selectedIndex != -1)
                    {
                        // Set all items between selectedStarted and selectedIndex to
                        // the same state as selectedStarted
                        int end = Math.Max(selectedStarted, selectedIndex);
                        for (int i = Math.Min(selectedStarted, selectedIndex) + 1; i < end; ++i)
                        {
                            ListItem lb = itemList[i];
                            lb.IsItemSelected = itemList[selectedStarted].IsItemSelected;
                            itemList[i] = lb;
                        }
                        ListItem lbs = itemList[selectedIndex];
                        lbs.IsItemSelected = itemList[selectedStarted].IsItemSelected;
                        itemList[selectedIndex] = lbs;

                        // If selectedStarted and selectedIndex are not the same,
                        // the user has dragged the mouse to make a selection.
                        // Notify the application of this.
                        if (selectedIndex != selectedStarted)
                            RaiseSelectionEvent(this, true);
                    }
                    break;
                }
                case WindowMessage.MouseWheel:
                {
                    int lines = SystemInformation.MouseWheelScrollLines;
                    int scrollAmount = MathUtils.HiWord((uint)wParam.ToInt32()) / Dialog.WheelDelta * lines;
                    scrollbarControl.Scroll(-scrollAmount);
                    break;
                }

                case WindowMessage.MouseMove:
                {
                    if (isDragging)
                    {
                        // compute the index of the item below the cursor
                        int itemIndex = -1;
                        if (textHeight > 0)
                            itemIndex = scrollbarControl.TrackPosition + (pt.Y - textRect.Top) / textHeight;

                        // Only proceed if the cursor is on top of an item
                        if (itemIndex >= scrollbarControl.TrackPosition &&
                            itemIndex < itemList.Count &&
                            itemIndex < scrollbarControl.TrackPosition + scrollbarControl.PageSize)
                        {
                            selectedIndex = itemIndex;
                            RaiseSelectionEvent(this, true);
                        }
                        else if (itemIndex < scrollbarControl.TrackPosition)
                        {
                            // User drags the mouse above window top
                            scrollbarControl.Scroll(-1);
                            selectedIndex = scrollbarControl.TrackPosition;
                            RaiseSelectionEvent(this, true);
                        }
                        else if (itemIndex >= scrollbarControl.TrackPosition + scrollbarControl.PageSize)
                        {
                            // User drags the mouse below the window bottom
                            scrollbarControl.Scroll(1);
                            selectedIndex = Math.Min(itemList.Count, scrollbarControl.TrackPosition + scrollbarControl.PageSize - 1);
                            RaiseSelectionEvent(this, true);
                        }
                    }
                    break;
                }
            }

            // Didn't handle it
            return false;
        }

        /// <summary>Called when the control should be rendered</summary>
        public override void Render(Device device, float elapsedTime)
        {
            if (!IsVisible)
                return; // Nothing to render

            Element e = elementList[MainLayer];

            // Blend current color
            e.TextureColor.Blend(ControlState.Normal, elapsedTime);
            e.FontColor.Blend(ControlState.Normal, elapsedTime);

            Element selectedElement = elementList[SelectionLayer];

            // Blend current color
            selectedElement.TextureColor.Blend(ControlState.Normal, elapsedTime);
            selectedElement.FontColor.Blend(ControlState.Normal, elapsedTime);

            parentDialog.DrawSprite(e, boundingBox);

            // Render the text
            if (itemList.Count > 0)
            {
                // Find out the height of a single line of text
                Rectangle rc = textRect;
                Rectangle sel = selectionRect;
                rc.Height = (int)(parentDialog.DialogManager.GetFontNode((int)e.FontIndex).Height);
                textHeight = rc.Height;

                // If we have not initialized the scroll bar page size,
                // do that now.
                if (!isScrollBarInit)
                {
                    if (textHeight > 0)
                        scrollbarControl.PageSize = textRect.Height / textHeight;
                    else
                        scrollbarControl.PageSize = textRect.Height;

                    isScrollBarInit = true;
                }
                rc.Width = textRect.Width;
                for (int i = scrollbarControl.TrackPosition; i < itemList.Count; ++i)
                {
                    if (rc.Bottom > textRect.Bottom)
                        break;

                    ListItem lb = itemList[i];

                    // Determine if we need to render this item with the
                    // selected element.
                    bool isSelectedStyle = false;

                    if ((selectedIndex == i) && (ctrlStyle == ListBoxStyle.SingleSelection))
                        isSelectedStyle = true;
                    else if (ctrlStyle == ListBoxStyle.MultiSelection)
                    {
                        if (isDragging && ((i >= selectedIndex && i < selectedStarted) ||
                                           (i <= selectedIndex && i > selectedStarted)))
                        {
                            ListItem selStart = itemList[selectedStarted];
                            isSelectedStyle = selStart.IsItemSelected;
                        }
                        else
                            isSelectedStyle = lb.IsItemSelected;
                    }

                    // Now render the text
                    if (isSelectedStyle)
                    {
                        sel.Location = new(sel.Left, rc.Top);
                        sel.Height = rc.Height;
                        parentDialog.DrawSprite(selectedElement, sel);
                        parentDialog.DrawText(lb.ItemText, selectedElement, rc);
                    }
                    else
                        parentDialog.DrawText(lb.ItemText, e, rc);

                    rc.Offset(0, textHeight);
                }
            }

            // Render the scrollbar finally
            using (new ProfilerEvent(() => "Render GUI control: " + scrollbarControl))
                scrollbarControl.Render(device, elapsedTime);
        }

        #region Item Controlling methods
        /// <summary>Adds an item to the list box control</summary>
        public void AddItem(string text, string tag, object data)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            // Create a new item and add it
            var newitem = new ListItem {ItemText = text, ItemTag = tag, ItemData = data, IsItemSelected = false};
            itemList.Add(newitem);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);
        }

        /// <summary>Inserts an item to the list box control</summary>
        public void InsertItem(int index, string text, object data)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            // Create a new item and insert it
            var newitem = new ListItem {ItemText = text, ItemData = data, IsItemSelected = false};
            itemList.Insert(index, newitem);

            // Update the scrollbar with the new range
            scrollbarControl.SetTrackRange(0, itemList.Count);
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

            RaiseSelectionEvent(this, true);
        }

        /// <summary>Removes all items from the control</summary>
        public void Clear()
        {
            // clear the list
            itemList.Clear();

            // Update scroll bar and index
            scrollbarControl.SetTrackRange(0, 1);
            selectedIndex = -1;
        }

        /// <summary>
        /// For single-selection listbox, returns the index of the selected item.
        /// For multi-selection, returns the first selected item after the previousSelected position.
        /// To search for the first selected item, the app passes -1 for previousSelected.  For
        /// subsequent searches, the app passes the returned index back to GetSelectedIndex as.
        /// previousSelected.
        /// Returns -1 on error or if no item is selected.
        /// </summary>
        public int GetSelectedIndex(int previousSelected)
        {
            if (previousSelected < -1)
                return -1;

            if (ctrlStyle == ListBoxStyle.MultiSelection)
            {
                // Multiple selections enabled.  Search for the next item with the selected flag
                for (int i = previousSelected + 1; i < itemList.Count; ++i)
                {
                    ListItem lbi = itemList[i];
                    if (lbi.IsItemSelected)
                        return i;
                }

                return -1;
            }

            // Single selection
            return selectedIndex;
        }

        /// <summary>Gets the selected item</summary>
        public ListItem GetSelectedItem(int previousSelected)
        {
            return itemList[GetSelectedIndex(previousSelected)];
        }

        /// <summary>Gets the selected item</summary>
        public ListItem GetSelectedItem()
        {
            return GetSelectedItem(-1);
        }

        /// <summary>Sets the border and margin sizes</summary>
        public void SetBorder(int borderSize, int marginSize)
        {
            border = borderSize;
            margin = marginSize;
            UpdateRectangles();
        }

        /// <summary>Sets the selected item by text</summary>
        public void SetSelected(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));

            for (int i = 0; i < itemList.Count; i++)
                if (itemList[i].ItemText == text) SelectItem(i);
        }

        /// <summary>Selects this item</summary>
        public void SelectItem(int newIndex)
        {
            if (itemList.Count == 0)
                return; // If no items exist there's nothing to do

            int oldSelected = selectedIndex;

            // Select the new item
            selectedIndex = newIndex;

            // Clamp the item
            if (selectedIndex < 0)
                selectedIndex = 0;
            if (selectedIndex > itemList.Count)
                selectedIndex = itemList.Count - 1;

            // Did the selection change?
            if (oldSelected != selectedIndex)
            {
                if (ctrlStyle == ListBoxStyle.MultiSelection)
                {
                    ListItem lbi = itemList[selectedIndex];
                    lbi.IsItemSelected = true;
                    itemList[selectedIndex] = lbi;
                }

                // Update selection start
                selectedStarted = selectedIndex;

                // adjust scrollbar
                scrollbarControl.ShowItem(selectedIndex);
            }
            RaiseSelectionEvent(this, true);
        }
        #endregion
    }

    #region Structs
    /// <summary>
    /// Style of the list box
    /// </summary>
    public enum ListBoxStyle
    {
        SingleSelection,
        MultiSelection,
    }
    #endregion
}
