/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Xml.Serialization;
using NanoByte.Common.Values;
using NanoByte.Common.Values.Design;
using OmegaEngine.Values;
using OmegaEngine.Values.Design;

namespace OmegaGUI.Model
{

    #region Enumerations
    /// <seealso cref="Control.AlignHorizontal"/>
    public enum HorizontalMode
    {
        /// <summary>X-values are interpreted as right-ward distance from the left border</summary>
        FromLeft,

        /// <summary>X-values are interpreted as left-ward distance from the right border</summary>
        FromRight,

        /// <summary>X-values are interpreted as right-ward distance from the center</summary>
        Center
    }

    /// <seealso cref="Control.AlignVertical"/>
    public enum VerticalMode
    {
        /// <summary>Y-values are interpreted as down-ward distance from the top border</summary>
        FromTop,

        /// <summary>Y-values are interpreted as up-ward distance from the bottom border</summary>
        FromBottom,

        /// <summary>Y-values are interpreted as down-ward distance from the center</summary>
        Center
    }
    #endregion

    /// <summary>
    /// Abstract base class for all control views
    /// </summary>
    public abstract class Control : ICloneable
    {
        #region Variables
        /// <summary>
        /// The dialog containing this control
        /// </summary>
        internal Dialog Parent;

        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        protected Render.Control DXControl;

        /// <summary>
        /// The actual location of the control on the dialog factoring in <see cref="AlignHorizontal"/> and <see cref="AlignVertical"/>
        /// </summary>
        protected Point EffectiveLocation;

        /// <summary>
        /// The actual size of the control on the dialog factoring in scaling due to <see cref="Dialog.Scale"/>
        /// </summary>
        protected Size EffectiveSize;
        #endregion

        #region Properties
        protected void NeedsUpdate()
        {
            if (Parent != null) Parent.NeedsUpdate = true;
        }

        #region Name
        /// <summary>
        /// Unique name for identifying this control
        /// </summary>
        [XmlAttribute, Description("Unique name for identifying this control"), Category("Design")]
        public string Name { get; set; }

        public override string ToString()
        {
            string value = GetType().Name;
            if (!string.IsNullOrEmpty(Name))
                value += ": " + Name;
            return value;
        }
        #endregion

        #region Behaviour
        private bool _isDefault;

        /// <summary>
        /// Is this the default control on the dialog? - no auto-update
        /// </summary>
        [XmlAttribute, DefaultValue(false), Description("Is this the default control on the dialog?"), Category("Behavior")]
        public bool Default
        {
            get => _isDefault;
            set
            {
                _isDefault = value;
                NeedsUpdate();
            }
        }

        protected bool IsEnabled = true;

        /// <summary>
        /// Is this control currently active?
        /// </summary>
        [XmlAttribute, DefaultValue(true), Description("Is this control currently active?"), Category("Behavior")]
        public bool Enabled
        {
            get => DXControl?.IsEnabled ?? IsEnabled;
            set
            {
                IsEnabled = value;
                if (DXControl != null) DXControl.IsEnabled = value;
            }
        }

        protected bool IsVisible = true;

        /// <summary>
        /// Is this control currently visible?
        /// </summary>
        [XmlAttribute, DefaultValue(true), Description("Is this control currently visible?"), Category("Behavior")]
        public bool Visible
        {
            get => DXControl?.IsVisible ?? IsVisible;
            set
            {
                IsVisible = value;
                if (DXControl != null) DXControl.IsVisible = value;
            }
        }
        #endregion

        #region Layout
        private Point _location;

        /// <summary>
        /// The location of the control on the dialog
        /// </summary>
        [Description("The location of the control on the dialog"), Category("Layout")]
        public Point Location
        {
            get => _location;
            set
            {
                _location = value;
                UpdateLayout();
            }
        }

        private Size _size = new(32, 32);

        /// <summary>
        /// The size of the control on the dialog
        /// </summary>
        [Description("The size of the control on the dialog"), Category("Layout")]
        public Size Size
        {
            get => _size;
            set
            {
                _size = value;
                UpdateLayout();
            }
        }

        private HorizontalMode _alignHorizontal;

        /// <summary>
        /// How to handle the X values of Location and Size
        /// </summary>
        [DefaultValue(HorizontalMode.FromLeft), Description("How to handle the X values of Location and Size"), Category("Layout")]
        public HorizontalMode AlignHorizontal
        {
            get => _alignHorizontal;
            set
            {
                _alignHorizontal = value;
                UpdateLayout();
            }
        }

        private VerticalMode _alignVertical;

        /// <summary>
        /// How to handle the Y values of Location and Size
        /// </summary>
        [DefaultValue(VerticalMode.FromTop), Description("How to handle the Y values of Location and Size"), Category("Layout")]
        public VerticalMode AlignVertical
        {
            get => _alignVertical;
            set
            {
                _alignVertical = value;
                UpdateLayout();
            }
        }

        /// <summary>
        /// The effective area on the window were this control is rendered
        /// </summary>
        [Browsable(false)]
        public Rectangle DrawBox => new(EffectiveLocation, EffectiveSize);
        #endregion

        #region Events
        /// <summary>
        /// A Lua script to execute when the mouse enters the area of the control
        /// </summary>
        [DefaultValue(""), Description("A Lua script to execute when the mouse enters the area of the control"), Category("Events"), FileType("Lua")]
        [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
        public string OnMouseEnter { get; set; }

        /// <summary>
        /// A Lua script to execute when the mouse leaves the area of the control
        /// </summary>
        [DefaultValue(""), Description("A Lua script to execute when the mouse leaves the area of the control"), Category("Events"), FileType("Lua")]
        [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
        public string OnMouseExit { get; set; }
        #endregion

        [Browsable(false)]
        public Render.Control ControlModel => DXControl;
        #endregion

        #region Generate
        /// <summary>
        /// Generates a control model from this control view
        /// </summary>
        internal abstract void Generate();

        protected void SetupMouseEvents()
        {
            if (!string.IsNullOrEmpty(OnMouseEnter))
                DXControl.MouseEnter += delegate { Parent.RaiseEvent(OnMouseEnter, Name + "_MouseEnter"); };

            if (!string.IsNullOrEmpty(OnMouseExit))
                DXControl.MouseExit += delegate { Parent.RaiseEvent(OnMouseExit, Name + "_MousExit"); };
        }
        #endregion

        #region Update Layout
        /// <summary>
        /// Updates the control's location and size
        /// </summary>
        internal void UpdateLayout()
        {
            if (Parent != null)
            {
                EffectiveSize.Width = (int)(_size.Width * Parent.EffectiveScale);
                EffectiveSize.Height = (int)(_size.Height * Parent.EffectiveScale);

                var scaledLocation = new Point(
                    (int)(_location.X * Parent.EffectiveScale),
                    (int)(_location.Y * Parent.EffectiveScale));

                EffectiveLocation = _location;

                if (Parent.DialogRender != null)
                {
                    switch (_alignHorizontal)
                    {
                        case HorizontalMode.FromLeft:
                            EffectiveLocation.X = scaledLocation.X;
                            break;
                        case HorizontalMode.FromRight:
                            EffectiveLocation.X = Parent.DialogRender.Width - EffectiveSize.Width - scaledLocation.X;
                            break;
                        case HorizontalMode.Center:
                            EffectiveLocation.X = (Parent.DialogRender.Width - EffectiveSize.Width) / 2 + scaledLocation.X;
                            break;
                    }

                    switch (_alignVertical)
                    {
                        case VerticalMode.FromTop:
                            EffectiveLocation.Y = scaledLocation.Y;
                            break;
                        case VerticalMode.FromBottom:
                            EffectiveLocation.Y = Parent.DialogRender.Height - EffectiveSize.Height - scaledLocation.Y;
                            break;
                        case VerticalMode.Center:
                            EffectiveLocation.Y = (Parent.DialogRender.Height - EffectiveSize.Height) / 2 + scaledLocation.Y;
                            break;
                    }
                }

                EffectiveLocation.X += (int)(Parent.Shift.X * Parent.EffectiveScale);
                EffectiveLocation.Y += (int)(Parent.Shift.Y * Parent.EffectiveScale);

                if (DXControl != null)
                {
                    DXControl.SetLocation(EffectiveLocation.X, EffectiveLocation.Y);
                    DXControl.SetSize(EffectiveSize.Width, EffectiveSize.Height);
                }
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this control.
        /// You need to call <see cref="Generate"/> on it before it can be used for rendering.
        /// </summary>
        /// <returns>The cloned control</returns>
        public Control Clone()
        {
            return (Control)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
