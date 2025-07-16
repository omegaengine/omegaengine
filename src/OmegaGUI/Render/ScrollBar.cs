/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Drawing;
using NanoByte.Common.Native;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render
{
    /// <summary>
    /// A scroll bar control
    /// </summary>
    public class ScrollBar : Control
    {
        public const int TrackLayer = 0;
        public const int UpButtonLayer = 1;
        public const int DownButtonLayer = 2;
        public const int ThumbLayer = 3;
        protected const int MinimumThumbSize = 8;

        #region Instance Data
        protected bool showingThumb = true;
        protected Rectangle upButtonRect;
        protected Rectangle downButtonRect;
        protected Rectangle trackRect;
        protected Rectangle thumbRect;
        protected int m_position; // Position of the first displayed item
        protected int pgSize = 1; // How many items are displayable in one page
        protected int start; // First item
        protected int end = 1; // The index after the last item
        private int thumbOffsetY;
        private bool isDragging;
        #endregion

        /// <summary>
        /// Creates a new instance of the scroll bar class
        /// </summary>
        public ScrollBar(Dialog parent) : base(parent)
        {
            ctrlType = ControlType.Scrollbar;
        }

        /// <summary>
        /// Update all of the rectangles
        /// </summary>
        protected override void UpdateRectangles()
        {
            // Get the bounding box first
            base.UpdateRectangles();

            // Make sure buttons are square
            upButtonRect = new(boundingBox.Location,
                new(boundingBox.Width, boundingBox.Width));

            downButtonRect = new(boundingBox.Left, boundingBox.Bottom - boundingBox.Width,
                boundingBox.Width, boundingBox.Width);

            trackRect = new(upButtonRect.Left, upButtonRect.Bottom,
                upButtonRect.Width, downButtonRect.Top - upButtonRect.Bottom);

            thumbRect = upButtonRect;

            UpdateThumbRectangle();
        }

        /// <summary>
        /// Position of the track
        /// </summary>
        public int TrackPosition
        {
            get => m_position;
            set
            {
                m_position = value;
                Cap();
                UpdateThumbRectangle();
            }
        }

        /// <summary>
        /// Size of a 'page'
        /// </summary>
        public int PageSize
        {
            get => pgSize;
            set
            {
                pgSize = value;
                Cap();
                UpdateThumbRectangle();
            }
        }

        /// <summary>Clips position at boundaries</summary>
        protected void Cap()
        {
            if (m_position < start || end - start <= pgSize)
                m_position = start;
            else if (m_position + pgSize > end)
                m_position = end - pgSize;
        }

        /// <summary>Compute the dimension of the scroll thumb</summary>
        protected void UpdateThumbRectangle()
        {
            if (end - start > pgSize)
            {
                int thumbHeight = Math.Max(trackRect.Height * pgSize / (end - start), MinimumThumbSize);
                int maxPosition = end - start - pgSize;
                thumbRect.Location = new(thumbRect.Left,
                    trackRect.Top + (m_position - start) * (trackRect.Height - thumbHeight) / maxPosition);
                thumbRect.Size = new(thumbRect.Width, thumbHeight);
                showingThumb = true;
            }
            else
            {
                // No content to scroll
                thumbRect.Height = 0;
                showingThumb = false;
            }
        }

        /// <summary>Scrolls by delta items.  A positive value scrolls down, while a negative scrolls down</summary>
        public void Scroll(int delta)
        {
            // Perform scroll
            m_position += delta;
            // Cap position
            Cap();
            // Update thumb rectangle
            UpdateThumbRectangle();
        }

        /// <summary>Scrolls by to position.</summary>
        public void ScrollTo(int position)
        {
            // Perform scroll
            m_position = position;
            // Cap position
            Cap();
            // Update thumb rectangle
            UpdateThumbRectangle();
        }

        /// <summary>Shows an item</summary>
        public void ShowItem(int index)
        {
            // Cap the index
            if (index < 0)
                index = 0;

            if (index >= end)
                index = end - 1;

            // Adjust the position to show this item
            if (m_position > index)
                m_position = index;
            else if (m_position + pgSize <= index)
                m_position = index - pgSize + 1;

            // Update thumbs again
            UpdateThumbRectangle();
        }

        /// <summary>Sets the track range</summary>
        public void SetTrackRange(int startRange, int endRange)
        {
            start = startRange;
            end = endRange;
            Cap();
            UpdateThumbRectangle();
        }

        /// <summary>Render the scroll bar control</summary>
        public override void Render(Device device, float elapsedTime)
        {
            var state = ControlState.Normal;
            if (IsVisible == false)
                state = ControlState.Hidden;
            else if ((IsEnabled == false) || (showingThumb == false))
                state = ControlState.Disabled;
            else if (isMouseOver)
                state = ControlState.MouseOver;
            else if (hasFocus)
                state = ControlState.Focus;

            float blendRate = (state == ControlState.Pressed) ? 0.0f : 0.8f;

            // Background track layer
            Element e = elementList[TrackLayer];

            // Blend current color
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, trackRect);

            // Up arrow
            e = elementList[UpButtonLayer];
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, upButtonRect);

            // Down arrow
            e = elementList[DownButtonLayer];
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, downButtonRect);

            // Thumb button
            e = elementList[ThumbLayer];
            e.TextureColor.Blend(state, elapsedTime, blendRate);
            parentDialog.DrawSprite(e, thumbRect);
        }

        /// <summary>Stores data for a dropdown list item</summary>
        public override bool HandleMouse(WindowMessage msg, Point pt, IntPtr wParam, IntPtr lParam)
        {
            if (!IsEnabled || !IsVisible)
                return false;

            switch (msg)
            {
                case WindowMessage.LeftButtonDoubleClick:
                case WindowMessage.LeftButtonDown:
                {
                    parentDialog.DialogManager.Target.Capture = true;

                    // Check for on up button
                    if (upButtonRect.Contains(pt))
                    {
                        if (m_position > start)
                            --m_position;
                        UpdateThumbRectangle();
                        return true;
                    }

                    // Check for on down button
                    if (downButtonRect.Contains(pt))
                    {
                        if (m_position + pgSize < end)
                            ++m_position;
                        UpdateThumbRectangle();
                        return true;
                    }

                    // Check for click on thumb
                    if (thumbRect.Contains(pt))
                    {
                        isDragging = true;
                        thumbOffsetY = pt.Y - thumbRect.Top;
                        return true;
                    }

                    // check for click on track
                    if (thumbRect.Left <= pt.X && thumbRect.Right > pt.X)
                    {
                        if (thumbRect.Top > pt.Y && trackRect.Top <= pt.Y)
                        {
                            Scroll(-(pgSize - 1));
                            return true;
                        }
                        if (thumbRect.Bottom <= pt.Y && trackRect.Bottom > pt.Y)
                        {
                            Scroll(pgSize - 1);
                            return true;
                        }
                    }

                    break;
                }
                case WindowMessage.LeftButtonUp:
                {
                    isDragging = false;
                    parentDialog.DialogManager.Target.Capture = false;
                    UpdateThumbRectangle();
                    break;
                }

                case WindowMessage.MouseMove:
                {
                    if (isDragging)
                    {
                        // Calculate new bottom and top of thumb rect
                        int bottom = thumbRect.Bottom + (pt.Y - thumbOffsetY - thumbRect.Top);
                        int top = pt.Y - thumbOffsetY;
                        thumbRect = new(thumbRect.Left, top, thumbRect.Width, bottom - top);
                        if (thumbRect.Top < trackRect.Top)
                            thumbRect.Offset(0, trackRect.Top - thumbRect.Top);
                        else if (thumbRect.Bottom > trackRect.Bottom)
                            thumbRect.Offset(0, trackRect.Bottom - thumbRect.Bottom);

                        // Compute first item index based on thumb position
                        int maxFirstItem = end - start - pgSize; // Largest possible index for first item
                        int maxThumb = trackRect.Height - thumbRect.Height; // Largest possible thumb position

                        m_position = start + (thumbRect.Top - trackRect.Top +
                                              maxThumb / (maxFirstItem * 2)) * // Shift by half a row to avoid last row covered
                                     maxFirstItem / maxThumb;

                        return true;
                    }
                    break;
                }
            }

            // Was not handled
            return false;
        }
    }
}
