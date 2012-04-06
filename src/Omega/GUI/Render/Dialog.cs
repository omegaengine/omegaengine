/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2012 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Common.Utils;
using OmegaEngine;
using SlimDX;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.VertexDecl;
using Resources = OmegaGUI.Properties.Resources;

namespace OmegaGUI.Render
{
    /// <summary>
    /// A dialog model containing DirectX-based controls
    /// </summary>
    [CLSCompliant(false)]
    public class Dialog
    {
        #region Constants
        public const int WheelDelta = 120;
        public static readonly Color4 WhiteColorValue = new Color4(1.0f, 1.0f, 1.0f);
        public static readonly Color4 TransparentWhite = new Color4(0.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Color4 BlackColorValue = new Color4(0.0f, 0.0f, 0.0f);
        private static Control controlFocus; // The control which has focus
        private static Control controlMouseOver; // The control which is hovered over
        private static Control controlMouseDown; // The control which the mouse was pressed on

        private static double timeRefresh;

        /// <summary>Set the static refresh time</summary>
        public static void SetRefreshTime(float time)
        {
            timeRefresh = time;
        }
        #endregion

        #region Instance Data
        private readonly DialogManager dialogManager;
        internal DialogManager DialogManager { get { return dialogManager; } }

        // Vertex information
        private TransformedColoredTextured[] dialogVertexes, captionVertexes;

        // Timing
        private double timeLastRefresh;

        // Control/Elements
        private readonly List<Control> controlList = new List<Control>();
        private readonly List<ElementHolder> defaultElementList = new List<ElementHolder>();

        // Captions
        private string caption;
        private int captionHeight = 20;
        private Element captionElement;
        public bool IsMinimized;

        // Dialog information
        private int dialogX, dialogY, width, height;
        // Colors
        private Color4 topLeftColor, topRightColor, bottomLeftColor, bottomRightColor, captionColor;

        // Fonts/Textures
        private readonly List<int> textureList = new List<int>(); // Index into texture cache
        private readonly List<int> fontList = new List<int>(); // Index into font cache

        // Dialogs
        private readonly Dialog nextDialog;
        private readonly Dialog prevDialog;

        // User Input control
        public bool IsUsingNonUserEvents;
        public bool IsUsingKeyboardInput;
        public bool IsUsingMouseInput;

        // Constructor parameters
        private readonly Color4 defaultTextColor;
        private readonly string defaultTexture;
        private readonly string defaultFont;
        private uint defaultFontSize;
        #endregion

        #region Simple Properties/Methods
        public uint DefaultFontSize
        {
            get { return defaultFontSize; }
            set
            {
                defaultFontSize = value;
                SetFont(0, defaultFont, value, FontWeight.Normal);
            }
        }

        /// <summary>The dialog's location</summary>
        public Point Location
        {
            get { return new Point(dialogX, dialogY); }
            set
            {
                dialogX = value.X;
                dialogY = value.Y;
                UpdateVertexes();
            }
        }

        public event EventHandler Resize;

        /// <summary>Called to set dialog's size</summary>
        public void SetSize(int w, int h)
        {
            width = w;
            height = h;
            UpdateVertexes();
            if (Resize != null) Resize(this, EventArgs.Empty);
        }

        /// <summary>Dialogs width</summary>
        public int Width { get { return width; } set { width = value; } }

        /// <summary>Dialogs height</summary>
        public int Height { get { return height; } set { height = value; } }

        /// <summary>Called to set dialog's caption</summary>
        public void SetCaptionText(string text)
        {
            caption = text;
        }

        /// <summary>The height of the caption bar at the top of the dialog</summary>
        public int CaptionHeight
        {
            get { return captionHeight; }
            set
            {
                captionHeight = value;
                UpdateVertexes();
            }
        }

        /// <summary>Called to set dialog's border colors</summary>
        public void SetBackgroundColors(Color4 topLeft, Color4 topRight, Color4 bottomLeft, Color4 bottomRight)
        {
            topLeftColor = topLeft;
            topRightColor = topRight;
            bottomLeftColor = bottomLeft;
            bottomRightColor = bottomRight;
            UpdateVertexes();
        }

        /// <summary>Called to set dialog's border colors</summary>
        public void SetBackgroundColors(Color4 allCorners)
        {
            SetBackgroundColors(allCorners, allCorners, allCorners, allCorners);
        }

        public void SetCaptionColor(Color4 color)
        {
            captionColor = color;
            UpdateVertexes();
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new instance of the dialog class
        /// </summary>
        /// <param name="manager">The <see cref="DialogManager"/> instance that provides the resources for rendering of this dialog</param>
        /// <param name="defaultTextColor">The default color of text on this dialog</param>
        /// <param name="defaultTexture">The texture file containing the controls</param>
        /// <param name="defaultFont">The name of default font for text on controls</param>
        /// <param name="defaultFontSize">The default font size</param>
        public Dialog(DialogManager manager, Color4 defaultTextColor, string defaultTexture, string defaultFont, uint defaultFontSize)
        {
            dialogManager = manager;

            topLeftColor = topRightColor = bottomLeftColor = bottomRightColor = new Color4();

            nextDialog = this; // Only one dialog
            prevDialog = this; // Only one dialog

            IsUsingMouseInput = true;

            this.defaultTextColor = defaultTextColor;
            this.defaultTexture = defaultTexture;
            this.defaultFont = defaultFont;
            this.defaultFontSize = defaultFontSize;
            InitializeDefaultElements();
        }

        /// <summary>
        /// Create a new instance of the dialog class
        /// </summary>
        /// <param name="manager">The <see cref="DialogManager"/> instance that provides the resources for rendering of this dialog</param>
        /// <param name="defaultTextColor">The default color of text on this dialog</param>
        public Dialog(DialogManager manager, Color4 defaultTextColor)
            : this(manager, defaultTextColor, "base.png", "Arial", 16)
        {}

        /// <summary>
        /// Create a new instance of the dialog class
        /// </summary>
        /// <param name="manager">The <see cref="DialogManager"/> instance that provides the resources for rendering of this dialog</param>
        public Dialog(DialogManager manager) : this(manager, WhiteColorValue)
        {}
        #endregion

        #region Default
        /// <summary>
        /// Initialize the default elements for this dialog
        /// </summary>
        private void InitializeDefaultElements()
        {
            SetTexture(0, defaultTexture);
            SetFont(0, defaultFont, defaultFontSize, FontWeight.Normal);

            // ReSharper disable BitwiseOperatorOnEnumWihtoutFlags
            //-------------------------------------
            // Element for the caption
            //-------------------------------------
            captionElement = new Element();
            captionElement.SetFont(0, defaultTextColor, DrawTextFormat.Center | DrawTextFormat.VerticalCenter);
            captionElement.SetTexture(0, Rectangle.FromLTRB(17, 269, 241, 287));
            captionElement.TextureColor.States[(int)ControlState.Normal] = WhiteColorValue;
            captionElement.FontColor.States[(int)ControlState.Normal] = defaultTextColor;
            // Pre-blend as we don't need to transition the state
            captionElement.TextureColor.Blend(ControlState.Normal, 10.0f);
            captionElement.FontColor.Blend(ControlState.Normal, 10.0f);

            var e = new Element();

            //-------------------------------------
            // Label
            //-------------------------------------
            e.SetFont(0, defaultTextColor, DrawTextFormat.Left | DrawTextFormat.VerticalCenter);
            e.FontColor.States[(int)ControlState.Disabled] = new Color4(0.75f, 0.75f, 0.75f, 0.75f);
            SetDefaultElement(ControlType.Label, (int)TextAlign.Left, e);

            e.SetFont(0, defaultTextColor, DrawTextFormat.Center | DrawTextFormat.VerticalCenter);
            e.FontColor.States[(int)ControlState.Disabled] = new Color4(0.75f, 0.75f, 0.75f, 0.75f);
            SetDefaultElement(ControlType.Label, (int)TextAlign.Center, e);

            e.SetFont(0, defaultTextColor, DrawTextFormat.Right | DrawTextFormat.VerticalCenter);
            e.FontColor.States[(int)ControlState.Disabled] = new Color4(0.75f, 0.75f, 0.75f, 0.75f);
            SetDefaultElement(ControlType.Label, (int)TextAlign.Right, e);

            //-------------------------------------
            // Button - Button
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(0, 0, 136, 54));
            e.SetFont(0, defaultTextColor, DrawTextFormat.Center | DrawTextFormat.VerticalCenter);
            e.TextureColor.States[(int)ControlState.Normal] = new Color4(0.55f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Pressed] = new Color4(0.85f, 1.0f, 1.0f, 1.0f);
            e.FontColor.States[(int)ControlState.MouseOver] = BlackColorValue;
            // Assign the element
            SetDefaultElement(ControlType.Button, Button.ButtonLayer, e);

            //-------------------------------------
            // Button - Fill Layer
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(136, 0, 252, 54), TransparentWhite);
            e.TextureColor.States[(int)ControlState.MouseOver] = new Color4(0.6f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Pressed] = new Color4(0, 0, 0, 0.25f);
            e.TextureColor.States[(int)ControlState.Focus] = new Color4(0.05f, 1.0f, 1.0f, 1.0f);
            // Assign the element
            SetDefaultElement(ControlType.Button, Button.FillLayer, e);

            //-------------------------------------
            // CheckBox - Box
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(0, 54, 27, 81));
            e.SetFont(0, defaultTextColor, DrawTextFormat.Left | DrawTextFormat.VerticalCenter);
            e.FontColor.States[(int)ControlState.Disabled] = new Color4(0.8f, 0.8f, 0.8f, 0.8f);
            e.TextureColor.States[(int)ControlState.Normal] = new Color4(0.55f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Focus] = new Color4(0.8f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Pressed] = WhiteColorValue;
            // Assign the element
            SetDefaultElement(ControlType.CheckBox, CheckBox.BoxLayer, e);

            //-------------------------------------
            // CheckBox - Check
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(27, 54, 54, 81));
            // Assign the element
            SetDefaultElement(ControlType.CheckBox, CheckBox.CheckLayer, e);

            //-------------------------------------
            // RadioButton - Box
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(54, 54, 81, 81));
            e.SetFont(0, defaultTextColor, DrawTextFormat.Left | DrawTextFormat.VerticalCenter);
            e.FontColor.States[(int)ControlState.Disabled] = new Color4(0.8f, 0.8f, 0.8f, 0.8f);
            e.TextureColor.States[(int)ControlState.Normal] = new Color4(0.55f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Focus] = new Color4(0.8f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Pressed] = WhiteColorValue;
            // Assign the element
            SetDefaultElement(ControlType.RadioButton, CheckBox.BoxLayer, e);

            //-------------------------------------
            // RadioButton - Check
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(81, 54, 108, 81));
            // Assign the element
            SetDefaultElement(ControlType.RadioButton, CheckBox.CheckLayer, e);

            //-------------------------------------
            // DropdownList - Main
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(7, 81, 247, 123));
            e.SetFont(0, defaultTextColor, DrawTextFormat.Left | DrawTextFormat.VerticalCenter);
            e.TextureColor.States[(int)ControlState.Normal] = new Color4(0.55f, 0.8f, 0.8f, 0.8f);
            e.TextureColor.States[(int)ControlState.Focus] = new Color4(0.6f, 0.95f, 0.95f, 0.95f);
            e.TextureColor.States[(int)ControlState.Disabled] = new Color4(0.25f, 0.8f, 0.8f, 0.8f);
            e.FontColor.States[(int)ControlState.MouseOver] = new Color4(0, 0, 0);
            e.FontColor.States[(int)ControlState.Pressed] = new Color4(0, 0, 0);
            e.FontColor.States[(int)ControlState.Disabled] = new Color4(0.8f, 0.8f, 0.8f, 0.8f);
            // Assign the element
            SetDefaultElement(ControlType.DropdownList, DropdownList.MainLayer, e);

            //-------------------------------------
            // DropdownList - Button
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(98, 189, 151, 238));
            e.TextureColor.States[(int)ControlState.Normal] = new Color4(0.55f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Pressed] = new Color4(0.55f, 0.55f, 0.55f);
            e.TextureColor.States[(int)ControlState.Focus] = new Color4(0.75f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Disabled] = new Color4(0.25f, 1.0f, 1.0f, 1.0f);
            // Assign the element
            SetDefaultElement(ControlType.DropdownList, DropdownList.ComboButtonLayer, e);

            //-------------------------------------
            // DropdownList - Dropdown
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(13, 123, 241, 160));
            e.SetFont(0, BlackColorValue, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.DropdownList, DropdownList.DropdownLayer, e);

            //-------------------------------------
            // DropdownList - Selection
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(12, 163, 239, 183));
            e.SetFont(0, defaultTextColor, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.DropdownList, DropdownList.SelectionLayer, e);

            //-------------------------------------
            // Slider - Track
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(1, 187, 93, 228));
            e.TextureColor.States[(int)ControlState.Normal] = new Color4(0.55f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Focus] = new Color4(0.75f, 1.0f, 1.0f, 1.0f);
            e.TextureColor.States[(int)ControlState.Disabled] = new Color4(0.25f, 1.0f, 1.0f, 1.0f);
            // Assign the element
            SetDefaultElement(ControlType.Slider, Slider.TrackLayer, e);

            //-------------------------------------
            // Slider - Button
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(151, 193, 192, 234));
            // Assign the element
            SetDefaultElement(ControlType.Slider, Slider.ButtonLayer, e);

            //-------------------------------------
            // Scrollbar - Track
            //-------------------------------------
            const int scrollBarStartX = 196;
            const int scrollBarStartY = 191;
            e.SetTexture(0, Rectangle.FromLTRB(scrollBarStartX + 0, scrollBarStartY + 21, scrollBarStartX + 22, scrollBarStartY + 32));
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.TrackLayer, e);

            //-------------------------------------
            // Scrollbar - Up Arrow
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(scrollBarStartX + 0, scrollBarStartY + 1, scrollBarStartX + 22, scrollBarStartY + 21));
            e.TextureColor.States[(int)ControlState.Disabled] = new Color4(0.8f, 0.8f, 0.8f);
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.UpButtonLayer, e);

            //-------------------------------------
            // Scrollbar - Down Arrow
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(scrollBarStartX + 0, scrollBarStartY + 32, scrollBarStartX + 22, scrollBarStartY + 53));
            e.TextureColor.States[(int)ControlState.Disabled] = new Color4(0.8f, 0.8f, 0.8f);
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.DownButtonLayer, e);

            //-------------------------------------
            // Scrollbar - Button
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(220, 192, 238, 234));
            // Assign the element
            SetDefaultElement(ControlType.Scrollbar, ScrollBar.ThumbLayer, e);

            //-------------------------------------
            // TextBox
            //-------------------------------------
            // Element assignment:
            //   0 - text area
            //   1 - top left border
            //   2 - top border
            //   3 - top right border
            //   4 - left border
            //   5 - right border
            //   6 - lower left border
            //   7 - lower border
            //   8 - lower right border
            SetFont(1, "Arial", 18, FontWeight.Normal);
            e.SetFont(1, BlackColorValue, DrawTextFormat.Left | DrawTextFormat.Top);

            // Assign the styles
            e.SetTexture(0, Rectangle.FromLTRB(14, 90, 241, 113));
            SetDefaultElement(ControlType.TextBox, TextBox.TextLayer, e);
            e.SetTexture(0, Rectangle.FromLTRB(8, 82, 14, 90));
            SetDefaultElement(ControlType.TextBox, TextBox.TopLeftBorder, e);
            e.SetTexture(0, Rectangle.FromLTRB(14, 82, 241, 90));
            SetDefaultElement(ControlType.TextBox, TextBox.TopBorder, e);
            e.SetTexture(0, Rectangle.FromLTRB(241, 82, 246, 90));
            SetDefaultElement(ControlType.TextBox, TextBox.TopRightBorder, e);
            e.SetTexture(0, Rectangle.FromLTRB(8, 90, 14, 113));
            SetDefaultElement(ControlType.TextBox, TextBox.LeftBorder, e);
            e.SetTexture(0, Rectangle.FromLTRB(241, 90, 246, 113));
            SetDefaultElement(ControlType.TextBox, TextBox.RightBorder, e);
            e.SetTexture(0, Rectangle.FromLTRB(8, 113, 14, 121));
            SetDefaultElement(ControlType.TextBox, TextBox.LowerLeftBorder, e);
            e.SetTexture(0, Rectangle.FromLTRB(14, 113, 241, 121));
            SetDefaultElement(ControlType.TextBox, TextBox.LowerBorder, e);
            e.SetTexture(0, Rectangle.FromLTRB(241, 113, 246, 121));
            SetDefaultElement(ControlType.TextBox, TextBox.LowerRightBorder, e);

            //-------------------------------------
            // Listbox - Main
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(13, 123, 241, 160));
            e.SetFont(0, BlackColorValue, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.ListBox, ListBox.MainLayer, e);

            //-------------------------------------
            // Listbox - Selection
            //-------------------------------------
            e.SetTexture(0, Rectangle.FromLTRB(16, 166, 240, 183));
            e.SetFont(0, defaultTextColor, DrawTextFormat.Left | DrawTextFormat.Top);
            // Assign the element
            SetDefaultElement(ControlType.ListBox, ListBox.SelectionLayer, e);
            // ReSharper restore BitwiseOperatorOnEnumWihtoutFlags
        }
        #endregion

        #region Remove
        /// <summary>Removes all controls from this dialog</summary>
        public void RemoveAllControls()
        {
            controlList.Clear();
            if ((controlFocus != null) && (controlFocus.Parent == this))
                controlFocus = null;
            controlMouseOver = null;
        }
        #endregion

        #region Clear
        /// <summary>Clears the radio button group</summary>
        public void ClearRadioButtonGroup(uint groupIndex)
        {
            // Find all radio buttons with the given group number
            foreach (Control control in controlList)
            {
                var radioButton = (RadioButton)control;
                // Clear the radio button checked setting
                if (radioButton != null && radioButton.ButtonGroup == groupIndex)
                    radioButton.SetChecked(false, false);
            }
        }
        #endregion

        #region Message handling
        private bool isDragging;
        private Point lastMouseLocation;

        private static Point MouseLocationHelper(IntPtr lParam)
        {
            return new Point(
                MathUtils.LoWord((uint)lParam.ToInt32()),
                MathUtils.HiWord((uint)lParam.ToInt32()));
        }

        /// <summary>
        /// Handle messages for this dialog
        /// </summary>
        /// <returns><see langword="true"/> if the message was handled and no further processing is necessary</returns>
        public bool MessageProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            #region Caption dragging
            if (!string.IsNullOrEmpty(caption) && lParam.ToInt32() > 0) // Don't drag into negative areas
            {
                if (msg == WindowMessage.LeftButtonDown)
                { // Start dragging
                    Point mouseLocation = MouseLocationHelper(lParam);

                    // Is in caption area?
                    if (mouseLocation.X >= dialogX && mouseLocation.X < dialogX + width &&
                        mouseLocation.Y >= dialogY && mouseLocation.Y < dialogY + captionHeight)
                    {
                        isDragging = true;
                        lastMouseLocation = new Point(mouseLocation.X, mouseLocation.Y);
                        WindowsUtils.SetCapture(hWnd);

                        // Click was handled
                        return true;
                    }
                }
                else if ((msg == WindowMessage.MouseMove) && isDragging)
                { // Update dragging
                    Point mouseLocation = MouseLocationHelper(lParam);

                    Location = new Point(
                        Location.X + (mouseLocation.X - lastMouseLocation.X),
                        Location.Y + (mouseLocation.Y - lastMouseLocation.Y));

                    lastMouseLocation = mouseLocation;
                }
                else if ((msg == WindowMessage.LeftButtonUp) && isDragging)
                { // Finish dragging
                    Point mouseLocation = MouseLocationHelper(lParam);

                    if (isDragging)
                    {
                        WindowsUtils.ReleaseCapture();
                        isDragging = false;
                        Location = new Point(
                            Location.X + (mouseLocation.X - lastMouseLocation.X),
                            Location.Y + (mouseLocation.Y - lastMouseLocation.Y));

                        // Click was handled
                        return true;
                    }
                }
                else if (msg == WindowMessage.LeftButtonDoubleClick)
                { // Toggle minimized state
                    Point mouseLocation = MouseLocationHelper(lParam);

                    // Is in caption area?
                    if (mouseLocation.X >= dialogX && mouseLocation.X < dialogX + width &&
                        mouseLocation.Y >= dialogY && mouseLocation.Y < dialogY + captionHeight)
                    {
                        IsMinimized = !IsMinimized;

                        // Click was handled
                        return true;
                    }
                }
            }
            #endregion

            // If the dialog is minimized, don't send any messages to controls.
            if (IsMinimized)
                return false;

            // If a control is in focus and is enabled, then give it the first chance at handling the message
            if (controlFocus != null && controlFocus.Parent == this && controlFocus.IsEnabled)
            {
                // If the control MsgProc handles it, then we don't
                if (controlFocus.MsgProc(hWnd, msg, wParam, lParam))
                    return true;
            }

            switch (msg)
            {
                    #region Focus handling
                case WindowMessage.ActivateApplication:
                {
                    if (controlFocus != null &&
                        controlFocus.Parent == this &&
                            controlFocus.IsEnabled)
                    {
                        if (wParam != IntPtr.Zero)
                            controlFocus.OnFocusIn();
                        else
                            controlFocus.OnFocusOut();
                    }
                    break;
                }
                    #endregion

                    #region Keyboard messages
                case WindowMessage.KeyDown:
                case WindowMessage.SystemKeyDown:
                case WindowMessage.KeyUp:
                case WindowMessage.SystemKeyUp:
                {
                    // If a control is in focus, it belongs to this dialog, and it's enabled, then give
                    // it the first chance at handling the message.
                    if (controlFocus != null &&
                        controlFocus.Parent == this &&
                            controlFocus.IsEnabled)
                    {
                        // If the control MsgProc handles it, then we don't.
                        if (controlFocus.HandleKeyboard(msg, wParam, lParam))
                            return true;
                    }

                    // Not yet handled, see if this matches a control's hotkey
                    if (msg == WindowMessage.KeyUp)
                    {
                        foreach (Control control in controlList)
                        {
                            // Was the hotkey hit?
                            if (control.Hotkey == (Keys)wParam.ToInt32())
                            {
                                control.OnHotKey();
                                return true;
                            }
                        }
                    }
                    if (msg == WindowMessage.KeyDown)
                    {
                        // If keyboard input is not enabled, this message should be ignored
                        if (!IsUsingKeyboardInput)
                            return false;

                        var key = (Keys)wParam.ToInt32();
                        switch (key)
                        {
                            case Keys.Right:
                            case Keys.Down:
                                if (controlFocus != null)
                                {
                                    OnCycleFocus(true);
                                    return true;
                                }
                                break;
                            case Keys.Left:
                            case Keys.Up:
                                if (controlFocus != null)
                                {
                                    OnCycleFocus(false);
                                    return true;
                                }
                                break;
                            case Keys.Tab:
                                if (controlFocus == null)
                                    FocusDefaultControl();
                                else
                                {
                                    bool shiftDown = WindowsUtils.IsKeyDown(Keys.ShiftKey);

                                    OnCycleFocus(!shiftDown);
                                }
                                return true;
                        }
                    }
                    break;
                }
                    #endregion

                    #region Mouse messages
                case WindowMessage.MouseMove:
                case WindowMessage.MouseWheel:
                case WindowMessage.LeftButtonUp:
                case WindowMessage.LeftButtonDown:
                case WindowMessage.LeftButtonDoubleClick:
                case WindowMessage.RightButtonUp:
                case WindowMessage.RightButtonDown:
                case WindowMessage.RightButtonDoubleClick:
                case WindowMessage.MiddleButtonUp:
                case WindowMessage.MiddleButtonDown:
                case WindowMessage.MiddleButtonDoubleClick:
                case WindowMessage.XButtonUp:
                case WindowMessage.XButtonDown:
                case WindowMessage.XButtonDoubleClick:
                {
                    // If not accepting mouse input, return false to indicate the message should still 
                    // be handled by the application (usually to move the camera).
                    if (!IsUsingMouseInput)
                        return false;

                    // Current mouse position
                    Point mousePoint;
                    unchecked
                    {
                        mousePoint = new Point(
                            MathUtils.LoWord((uint)lParam.ToInt32()),
                            MathUtils.HiWord((uint)lParam.ToInt32()));
                    }

                    // Offset mouse point
                    mousePoint.X -= dialogX;
                    mousePoint.Y -= dialogY;

                    // If a control is in focus, it belongs to this dialog, and it's enabled, then give
                    // it the first chance at handling the message.
                    if (controlFocus != null &&
                        controlFocus.Parent == this &&
                            controlFocus.IsEnabled)
                    {
                        // If the control MsgProc handles it, then we don't.
                        if (controlFocus.HandleMouse(msg, mousePoint, wParam, lParam))
                            return true;
                    }

                    // Not yet handled, see if the mouse is over any controls
                    Control control = GetControlAtPoint(mousePoint);
                    if ((control != null) && (control.IsEnabled))
                    {
                        // Let the control handle the mouse if it wants (and return true if it handles it)
                        if (control.HandleMouse(msg, mousePoint, wParam, lParam))
                            return true;
                    }
                    else
                    {
                        // Mouse not over any controls in this dialog, if there was a control which had focus it just lost it
                        if (msg == WindowMessage.LeftButtonDown && controlFocus != null && controlFocus.Parent == this)
                        {
                            controlFocus.OnFocusOut();
                            controlFocus = null;
                        }
                    }

                    // Still not handled, hand this off to the dialog. Return false to indicate the
                    // message should still be handled by the application (usually to move the camera).
                    if (msg == WindowMessage.MouseMove)
                    {
                        OnMouseMove(mousePoint);
                        return false;
                    }
                    break;
                }
                    #endregion
            }

            // Didn't handle this message
            return false;
        }

        #region Mouse move
        /// <summary>
        /// Handle mouse moves
        /// </summary>
        private void OnMouseMove(Point pt)
        {
            // If the mouse was previously hovering over a control, it's either
            // still over the control or has left
            if (controlMouseDown != null)
            {
                // If another dialog owns this control then let that dialog handle it
                if (controlMouseDown.Parent != this)
                    return;

                // If the same control is still under the mouse, nothing needs to be done
                if (controlMouseDown.ContainsPoint(pt))
                    return;

                // Mouse has moved outside the control, notify the control and continue
                controlMouseDown.OnMouseExit();
                controlMouseDown = null;
            }

            // Figure out which control the mouse is over now
            Control control = GetControlAtPoint(pt);
            if (controlMouseOver != control)
            {
                if (controlMouseOver != null)
                {
                    controlMouseOver.OnMouseExit();
                    controlMouseOver = null;
                }

                controlMouseOver = control;

                if (controlMouseOver != null)
                    controlMouseOver.OnMouseEnter();
            }
        }
        #endregion

        #endregion

        #region Focus
        /// <summary>
        /// Request that this control has focus
        /// </summary>
        public static void RequestFocus(Control control)
        {
            if (control == null) throw new ArgumentNullException("control");

            if (controlFocus == control)
                return; // Already does

            if (!control.CanHaveFocus)
                return; // Can't have focus

            if (controlFocus != null)
                controlFocus.OnFocusOut();

            // Set the control focus now
            control.OnFocusIn();
            controlFocus = control;
        }

        /// <summary>
        /// Clears focus of the dialog
        /// </summary>
        public static void ClearFocus()
        {
            if (controlFocus != null)
            {
                controlFocus.OnFocusOut();
                controlFocus = null;
            }
        }

        /// <summary>
        /// Cycles focus to the next available control
        /// </summary>
        private void OnCycleFocus(bool forward)
        {
            // This should only be handled by the dialog which owns the focused control, and 
            // only if a control currently has focus
            if (controlFocus == null || controlFocus.Parent != this)
                return;

            Control control = controlFocus;
            // Go through a bunch of controls
            for (int i = 0; i < 0xffff; i++)
            {
                control = (forward) ? GetNextControl(control) : GetPreviousControl(control);

                // If we've gone in a full circle, focus won't change
                if (control == controlFocus)
                    return;

                // If the dialog accepts keyboard input and the control can have focus then
                // move focus
                if (control.Parent.IsUsingKeyboardInput && control.CanHaveFocus)
                {
                    controlFocus.OnFocusOut();
                    controlFocus = control;
                    controlFocus.OnFocusIn();
                    return;
                }
            }

            throw new InvalidOperationException("Multiple dialogs are improperly chained together.");
        }

        /// <summary>
        /// Gets the next control
        /// </summary>
        private static Control GetNextControl(Control control)
        {
            int index = (int)control.index + 1;

            Dialog dialog = control.Parent;

            // Cycle through dialogs in the loop to find the next control.
            // If only one control exists in all looped dialogs it will
            // be the returned 'next' control.
            while (index >= dialog.controlList.Count)
            {
                dialog = dialog.nextDialog;
                index = 0;
            }

            return dialog.controlList[index];
        }

        /// <summary>
        /// Gets the previous control
        /// </summary>
        private static Control GetPreviousControl(Control control)
        {
            int index = (int)control.index - 1;

            Dialog dialog = control.Parent;

            // Cycle through dialogs in the loop to find the next control.
            // If only one control exists in all looped dialogs it will
            // be the returned 'previous' control.
            while (index < 0)
            {
                dialog = dialog.prevDialog ?? control.Parent;
                index = dialog.controlList.Count - 1;
            }

            return dialog.controlList[index];
        }

        /// <summary>
        /// Sets focus to the default control of a dialog
        /// </summary>
        private void FocusDefaultControl()
        {
            // Check for a default control in this dialog
            foreach (Control control in controlList)
            {
                if (control.IsDefault)
                {
                    // Remove focus from the current control
                    ClearFocus();

                    // Give focus to the default control
                    controlFocus = control;
                    controlFocus.OnFocusIn();
                    return;
                }
            }
        }
        #endregion

        #region Controls Methods/Properties
        /// <summary>Returns the control located at a point (if one exists)</summary>
        public Control GetControlAtPoint(Point pt)
        {
            // Loop through controls backwards
            for (int i = controlList.Count - 1; i >= 0; i--)
            {
                if (controlList[i].IsEnabled && controlList[i].IsVisible && controlList[i].ContainsPoint(pt))
                    return controlList[i];
            }

            return null;
        }
        #endregion

        #region Default Elements
        /// <summary>
        /// Sets the default element
        /// </summary>
        public void SetDefaultElement(ControlType ctype, uint index, Element e)
        {
            // If this element already exists, just update it
            for (int i = 0; i < defaultElementList.Count; i++)
            {
                ElementHolder holder = defaultElementList[i];
                if ((holder.ControlType == ctype) &&
                    (holder.ElementIndex == index))
                {
                    // Found it, update it
                    holder.Element = e.Clone();
                    defaultElementList[i] = holder;
                    return;
                }
            }

            // Couldn't find it, add a new entry
            var newEntry = new ElementHolder {ControlType = ctype, ElementIndex = index, Element = e.Clone()};

            // Add it now
            defaultElementList.Add(newEntry);
        }

        /// <summary>
        /// Gets the default element
        /// </summary>
        public Element GetDefaultElement(ControlType ctype, uint index)
        {
            for (int i = 0; i < defaultElementList.Count; i++)
            {
                ElementHolder holder = defaultElementList[i];
                if ((holder.ControlType == ctype) &&
                    (holder.ElementIndex == index))
                {
                    // Found it, return it
                    return holder.Element;
                }
            }
            return null;
        }
        #endregion

        #region Texture/Font Resources
        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public void SetFont(uint index, string faceName, uint fontHeight, FontWeight weight)
        {
            // Make sure the list is at least big enough to hold this index
            for (var i = (uint)fontList.Count; i <= index; i++)
                fontList.Add(-1);

            int fontIndex = dialogManager.AddFont(faceName, fontHeight, weight);
            fontList[(int)index] = fontIndex;
        }

        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public FontNode GetFont(uint index)
        {
            return dialogManager.GetFontNode(fontList[(int)index]);
        }

        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public void SetTexture(uint index, string filename)
        {
            // Make sure the list is at least big enough to hold this index
            for (var i = (uint)textureList.Count; i <= index; i++)
                textureList.Add(-1);

            int textureIndex = dialogManager.AddTexture(filename);
            textureList[(int)index] = textureIndex;
        }

        /// <summary>
        /// Shared resource access. Indexed fonts and textures are shared among
        /// all the controls.
        /// </summary>
        public TextureNode GetTexture(uint index)
        {
            return dialogManager.GetTextureNode(textureList[(int)index]);
        }
        #endregion

        #region Control Creation
        /// <summary>
        /// Initializes a control
        /// </summary>
        public void InitializeControl(Control control)
        {
            if (control == null) throw new ArgumentNullException("control");

            // Set the index
            control.index = (uint)controlList.Count;

            // Look for a default element entires
            for (int i = 0; i < defaultElementList.Count; i++)
            {
                // Find any elements for this control
                ElementHolder holder = defaultElementList[i];
                if (holder.ControlType == control.ControlType)
                    control[holder.ElementIndex] = holder.Element;
            }

            // Initialize the control
            control.OnInitialize();
        }

        /// <summary>
        /// Adds a control to the dialog
        /// </summary>
        public void AddControl(Control control)
        {
            // Initialize the control first
            InitializeControl(control);

            // Add this to the control list
            controlList.Add(control);
        }

        /// <summary>Adds a static text control to the dialog</summary>
        public Label AddStatic(int id, string text, int x, int y, int w, int h, bool isDefault)
        {
            // First create the static
            var s = new Label(this);

            // Now call the add control method
            AddControl(s);

            // Set the properties of the static now
            s.ID = id;
            s.SetText(text);
            s.SetLocation(x, y);
            s.SetSize(w, h);
            s.IsDefault = isDefault;

            return s;
        }

        /// <summary>Adds a static text control to the dialog</summary>
        public Label AddStatic(int id, string text, int x, int y, int w, int h)
        {
            return AddStatic(id, text, x, y, w, h, false);
        }

        /// <summary>Adds a picture box control to the dialog</summary>
        public PictureBox AddPictureBox(int id, int x, int y, int w, int h, Element fill)
        {
            // First create the static
            var s = new PictureBox(this, fill);

            // Now call the add control method
            AddControl(s);

            // Set the properties of the static now
            s.ID = id;
            s.SetLocation(x, y);
            s.SetSize(w, h);

            return s;
        }

        /// <summary>Adds a group box control to the dialog</summary>
        public GroupBox AddGroupBox(int id, int x, int y, int w, int h, Color4 borderColor, Color4 fillCo)
        {
            // First create the static
            var s = new GroupBox(this, borderColor, fillCo);

            // Now call the add control method
            AddControl(s);

            // Set the properties of the static now
            s.ID = id;
            s.SetLocation(x, y);
            s.SetSize(w, h);

            return s;
        }

        /// <summary>Adds a button control to the dialog</summary>
        public Button AddButton(int id, string text, int x, int y, int w, int h, Keys hotkey, bool isDefault)
        {
            // First create the button
            var b = new Button(this);

            // Now call the add control method
            AddControl(b);

            // Set the properties of the button now
            b.ID = id;
            b.SetText(text);
            b.SetLocation(x, y);
            b.SetSize(w, h);
            b.Hotkey = hotkey;
            b.IsDefault = isDefault;

            return b;
        }

        /// <summary>Adds a button control to the dialog</summary>
        public Button AddButton(int id, string text, int x, int y, int w, int h)
        {
            return AddButton(id, text, x, y, w, h, 0, false);
        }

        /// <summary>Adds a checkbox to the dialog</summary>
        public CheckBox AddCheckBox(int id, string text, int x, int y, int w, int h, bool ischecked, Keys hotkey, bool isDefault)
        {
            // First create the checkbox
            var c = new CheckBox(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetText(text);
            c.SetLocation(x, y);
            c.SetSize(w, h);
            c.Hotkey = hotkey;
            c.IsDefault = isDefault;
            c.IsChecked = ischecked;

            return c;
        }

        /// <summary>Adds a checkbox control to the dialog</summary>
        public CheckBox AddCheckBox(int id, string text, int x, int y, int w, int h, bool ischecked)
        {
            return AddCheckBox(id, text, x, y, w, h, ischecked, 0, false);
        }

        /// <summary>Adds a radiobutton to the dialog</summary>
        public RadioButton AddRadioButton(int id, uint groupId, string text, int x, int y, int w, int h, bool ischecked, Keys hotkey, bool isDefault)
        {
            // First create the RadioButton
            var c = new RadioButton(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.ButtonGroup = groupId;
            c.SetText(text);
            c.SetLocation(x, y);
            c.SetSize(w, h);
            c.Hotkey = hotkey;
            c.IsDefault = isDefault;
            c.IsChecked = ischecked;

            return c;
        }

        /// <summary>Adds a radio button control to the dialog</summary>
        public RadioButton AddRadioButton(int id, uint groupId, string text, int x, int y, int w, int h, bool ischecked)
        {
            return AddRadioButton(id, groupId, text, x, y, w, h, ischecked, 0, false);
        }

        /// <summary>Adds a dropdown list control to the dialog</summary>
        public DropdownList AddDropdownList(int id, int x, int y, int w, int h, Keys hotkey, bool isDefault)
        {
            // First create the list
            var c = new DropdownList(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetLocation(x, y);
            c.SetSize(w, h);
            c.Hotkey = hotkey;
            c.IsDefault = isDefault;

            return c;
        }

        /// <summary>Adds a dropdown list control to the dialog</summary>
        public DropdownList AddDropdownList(int id, int x, int y, int w, int h)
        {
            return AddDropdownList(id, x, y, w, h, 0, false);
        }

        /// <summary>Adds a slider control to the dialog</summary>
        public Slider AddSlider(int id, int x, int y, int w, int h, int min, int max, int initialValue, bool isDefault)
        {
            // First create the slider
            var c = new Slider(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetLocation(x, y);
            c.SetSize(w, h);
            c.IsDefault = isDefault;
            c.SetRange(min, max);
            c.Value = initialValue;

            return c;
        }

        /// <summary>Adds a slider control to the dialog</summary>
        public Slider AddSlider(int id, int x, int y, int w, int h)
        {
            return AddSlider(id, x, y, w, h, 0, 100, 50, false);
        }

        /// <summary>Adds a scroll bar control to the dialog</summary>
        public ScrollBar AddScrollBar(int id, int x, int y, int w, int h, int min, int max, int initialValue, bool isDefault)
        {
            // First create the slider
            var c = new ScrollBar(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetLocation(x, y);
            c.SetSize(w, h);
            c.IsDefault = isDefault;
            c.SetTrackRange(min, max);
            c.TrackPosition = initialValue;

            return c;
        }

        /// <summary>Adds a scroll bar control to the dialog</summary>
        public ScrollBar AddScrollBar(int id, int x, int y, int w, int h)
        {
            return AddScrollBar(id, x, y, w, h, 0, 100, 50, false);
        }

        /// <summary>Adds a listbox control to the dialog</summary>
        public ListBox AddListBox(int id, int x, int y, int w, int h, ListBoxStyle style)
        {
            // First create the listbox
            var c = new ListBox(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the button now
            c.ID = id;
            c.SetLocation(x, y);
            c.SetSize(w, h);
            c.Style = style;

            return c;
        }

        /// <summary>Adds a listbox control to the dialog</summary>
        public ListBox AddListBox(int id, int x, int y, int w, int h)
        {
            return AddListBox(id, x, y, w, h, ListBoxStyle.SingleSelection);
        }

        /// <summary>Adds an edit box control to the dialog</summary>
        public TextBox AddTextBox(int id, string text, int x, int y, int w, int h, bool isDefault)
        {
            // First create the editbox
            var c = new TextBox(this);

            // Now call the add control method
            AddControl(c);

            // Set the properties of the static now
            c.ID = id;
            c.Text = text ?? string.Empty;
            c.SetLocation(x, y);
            c.SetSize(w, h);
            c.IsDefault = isDefault;

            return c;
        }

        /// <summary>Adds an edit box control to the dialog</summary>
        public TextBox AddTextBox(int id, string text, int x, int y, int w, int h)
        {
            return AddTextBox(id, text, x, y, w, h, false);
        }
        #endregion

        #region Update
        /// <summary>Render the dialog</summary>
        private void UpdateVertexes()
        {
            dialogVertexes = new[]
            {
                new TransformedColoredTextured(dialogX - 1, dialogY - 1, 0.5f, 1.0f, topLeftColor.ToArgb(), 0.0f, 0.5f),
                new TransformedColoredTextured(dialogX + width, dialogY - 1, 0.5f, 1.0f, topRightColor.ToArgb(), 1.0f, 0.5f),
                new TransformedColoredTextured(dialogX + width, dialogY + height, 0.5f, 1.0f, bottomRightColor.ToArgb(), 1.0f, 1.0f),
                new TransformedColoredTextured(dialogX - 1, dialogY + height, 0.5f, 1.0f, bottomLeftColor.ToArgb(), 0.0f, 1.0f)
            };

            captionVertexes = new[]
            {
                new TransformedColoredTextured(dialogX + 5, dialogY + 5, 0.5f, 1.0f, captionColor.ToArgb(), 0.0f, 0.5f),
                new TransformedColoredTextured(dialogX + width - 5, dialogY + 5, 0.5f, 1.0f, captionColor.ToArgb(), 1.0f, 0.5f),
                new TransformedColoredTextured(dialogX + width - 5, dialogY + captionHeight - 5, 0.5f, 1.0f, captionColor.ToArgb(), 1.0f, 1.0f),
                new TransformedColoredTextured(dialogX + 5, dialogY + captionHeight - 5, 0.5f, 1.0f, captionColor.ToArgb(), 0.0f, 1.0f)
            };
        }
        #endregion

        #region Drawing methods
        /// <summary>Render the dialog</summary>
        public void OnRender(float elapsedTime)
        {
            // See if the dialog needs to be refreshed
            if (timeLastRefresh < timeRefresh)
            {
                timeLastRefresh = WindowsUtils.AbsoluteTime;
                Refresh();
            }

            using (new ProfilerEvent("Render GUI dialog"))
            {
                Device device = dialogManager.Device;

                using (new ProfilerEvent("Prepare Device for GUI rendering"))
                {
                    // Set up a state block here and restore it when finished drawing all the controls
                    dialogManager.StateBlock.Capture();

                    // Set some render/texture states
                    device.SetRenderState(RenderState.AlphaBlendEnable, true);
                    device.SetRenderState(RenderState.SourceBlend, (int)Blend.SourceAlpha);
                    device.SetRenderState(RenderState.DestinationBlend, (int)Blend.InverseSourceAlpha);
                    device.SetRenderState(RenderState.AlphaTestEnable, false);
                    device.SetRenderState(RenderState.ZEnable, false);
                    device.SetTextureStageState(0, TextureStage.ColorOperation, (int)TextureOperation.SelectArg2);
                    device.SetTextureStageState(0, TextureStage.ColorArg2, (int)TextureArgument.Diffuse);
                    device.SetTextureStageState(0, TextureStage.AlphaOperation, (int)TextureOperation.SelectArg1);
                    device.SetTextureStageState(0, TextureStage.AlphaArg1, (int)TextureArgument.Diffuse);
                    // Clear vertex/pixel shader
                    device.VertexShader = null;
                    device.PixelShader = null;

                    // Render if not minimized
                    device.VertexFormat = TransformedColoredTextured.Format;
                    if (!IsMinimized) device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, dialogVertexes);
                    if (!string.IsNullOrEmpty(caption)) device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, captionVertexes);

                    // Reset states
                    device.SetTextureStageState(0, TextureStage.ColorOperation, (int)TextureOperation.Modulate);
                    device.SetTextureStageState(0, TextureStage.ColorArg1, (int)TextureArgument.Texture);
                    device.SetTextureStageState(0, TextureStage.ColorArg2, (int)TextureArgument.Diffuse);
                    device.SetTextureStageState(0, TextureStage.AlphaOperation, (int)TextureOperation.Modulate);
                    device.SetTextureStageState(0, TextureStage.AlphaArg1, (int)TextureArgument.Texture);
                    device.SetTextureStageState(0, TextureStage.AlphaArg2, (int)TextureArgument.Diffuse);

                    device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);

                    // Set the texture up, and begin the sprite
                    TextureNode tNode = GetTexture(0);
                    device.SetTexture(0, tNode.Texture);
                    dialogManager.Sprite.Begin(SpriteFlags.DoNotSaveState);

                    // Render the caption if it's enabled.
                    if (!string.IsNullOrEmpty(caption))
                    {
                        // DrawSprite will offset the rect down by
                        // captionHeight, so adjust the rect higher
                        // here to negate the effect.
                        var rect = new Rectangle(0, 0, width, captionHeight);
                        DrawSprite(captionElement, rect);
                        rect.Offset(5, 0); // Make a left margin
                        DrawText(
                            caption + ((IsMinimized) ? " (" + Resources.Minimized + ")" : null),
                            captionElement, rect, true);
                    }
                }

                // If the dialog is minimized, skip rendering its controls.
                if (!IsMinimized)
                {
                    using (new ProfilerEvent("Render GUI controls"))
                    {
                        foreach (Control control in controlList)
                        {
                            // Focused control is drawn last
                            if (control == controlFocus)
                                continue;

                            // ReSharper disable AccessToModifiedClosure
                            using (new ProfilerEvent(() => "Render " + control))
                                control.Render(device, elapsedTime);
                            // ReSharper restore AccessToModifiedClosure
                        }

                        // Render the focus control if necessary
                        if (controlFocus != null && controlFocus.Parent == this)
                        {
                            using (new ProfilerEvent(() => "Render " + controlFocus))
                                controlFocus.Render(device, elapsedTime);
                        }
                    }
                }

                using (new ProfilerEvent("Restore original Device state"))
                {
                    // End the sprite and apply the stateblock
                    dialogManager.Sprite.End();
                    dialogManager.StateBlock.Apply();
                }
            }
        }

        /// <summary>
        /// Refresh the dialog
        /// </summary>
        public void Refresh()
        {
            using (new ProfilerEvent("Refresh GUI dialog"))
            {
                // Reset the controls
                controlFocus = null;
                controlMouseDown = null;
                controlMouseOver = null;

                // Refresh any controls
                controlList.ForEach(control => control.Refresh());

                if (IsUsingKeyboardInput) FocusDefaultControl();
            }
        }

        /// <summary>Draw's some text</summary>
        public void DrawText(string text, Element element, Rectangle rect, bool shadow)
        {
            if (element == null) throw new ArgumentNullException("element");

            // No need to draw fully transparant layers
            if (element.FontColor.Current.Alpha == 0)
                return; // Nothing to do

            Rectangle screenRect = rect;
            screenRect.Offset(dialogX, dialogY);

            // Set the identity transform
            dialogManager.Sprite.Transform = Matrix.Identity;

            // Get the font node here
            FontNode fNode = GetFont(element.FontIndex);
            if (shadow)
            {
                // Render the text shadowed
                Rectangle shadowRect = screenRect;
                shadowRect.Offset(1, 1);

                // Black shadow for bright text, gray shadow for dark text
                Color shadowColor =
                    (element.FontColor.Current.Red + element.FontColor.Current.Green + element.FontColor.Current.Blue > 1.5f) ?
                                                                                                                                  Color.Black : Color.Gray;

                // ToDo: Optimize performance
                fNode.Font.DrawString(dialogManager.Sprite, text, shadowRect, element.textFormat, shadowColor);
            }

            fNode.Font.DrawString(dialogManager.Sprite, text,
                screenRect, element.textFormat, element.FontColor.Current.ToArgb());
        }

        /// <summary>Draw a sprite</summary>
        public void DrawSprite(Element element, Rectangle rect)
        {
            if (element == null) throw new ArgumentNullException("element");

            // No need to draw fully transparant layers
            if (element.TextureColor.Current.Alpha == 0)
                return; // Nothing to do

            Rectangle texRect = element.textureRect;
            Rectangle screenRect = rect;
            screenRect.Offset(dialogX, dialogY);

            // Get the texture
            TextureNode tNode = GetTexture(element.TextureIndex);
            float scaleX = screenRect.Width / (float)texRect.Width;
            float scaleY = screenRect.Height / (float)texRect.Height;

            // Set the scaling transform
            dialogManager.Sprite.Transform = Matrix.Scaling(scaleX, scaleY, 1.0f);

            // Calculate the position
            var pos = new Vector3(screenRect.Left, screenRect.Top, 0.0f);
            pos.X /= scaleX;
            pos.Y /= scaleY;

            // Finally draw the sprite
            dialogManager.Sprite.Draw(tNode.Texture, texRect, new Vector3(), pos, element.TextureColor.Current);
        }

        /// <summary>Draw's some text</summary>
        public void DrawText(string text, Element element, Rectangle rect)
        {
            if (element == null) throw new ArgumentNullException("element");

            DrawText(text, element, rect, false);
        }

        /// <summary>Draw a rectangle</summary>
        public void DrawRectangle(Rectangle rect, Color4 color, bool filled)
        {
            // Offset the rectangle
            rect.Offset(dialogX, dialogY);

            // Get the integer value of the color
            int realColor = color.ToArgb();
            // Create some vertexes
            TransformedColoredTextured[] vertexes =
                {
                    new TransformedColoredTextured(rect.Left - 0.5f, rect.Top - 0.5f, 0.5f, 1.0f, realColor, 0, 0),
                    new TransformedColoredTextured(rect.Right - 0.5f, rect.Top - 0.5f, 0.5f, 1.0f, realColor, 0, 0),
                    new TransformedColoredTextured(rect.Right - 0.5f, rect.Bottom - 0.5f, 0.5f, 1.0f, realColor, 0, 0),
                    new TransformedColoredTextured(rect.Left - 0.5f, rect.Bottom - 0.5f, 0.5f, 1.0f, realColor, 0, 0)
                };

            // Get the device
            Device device = dialogManager.Device;

            // Since we're doing our own drawing here, we need to flush the sprites
            dialogManager.Sprite.Flush();
            // Preserve the devices current vertex declaration
            using (VertexDeclaration decl = device.VertexDeclaration)
            {
                // Set the vertex format
                device.VertexFormat = TransformedColoredTextured.Format;

                // Set some texture states
                device.SetTextureStageState(0, TextureStage.ColorOperation, (int)TextureOperation.SelectArg2);
                device.SetTextureStageState(0, TextureStage.AlphaOperation, (int)TextureOperation.SelectArg2);

                // Draw the rectangle
                if (filled) device.DrawUserPrimitives(PrimitiveType.TriangleFan, 2, vertexes);
                else
                {
                    device.DrawIndexedUserPrimitives(PrimitiveType.LineStrip, 0, 5, 4,
                        new short[] {0, 1, 2, 3, 0}, Format.Index16, vertexes, TransformedColoredTextured.StrideSize);
                }

                // Reset some texture states
                device.SetTextureStageState(0, TextureStage.ColorOperation, (int)TextureOperation.Modulate);
                device.SetTextureStageState(0, TextureStage.AlphaOperation, (int)TextureOperation.Modulate);

                // Restore the vertex declaration
                device.VertexDeclaration = decl;
            }
        }
        #endregion
    }
}
