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
using NanoByte.Common.Collections;
using NanoByte.Common.Values;
using NanoByte.Common.Values.Design;
using OmegaEngine.Values;
using OmegaEngine.Values.Design;
using OmegaGUI.Render;

namespace OmegaGUI.Model
{
    /// <summary>
    /// Combo box control
    /// </summary>
    public class DropdownList : ButtonBase
    {
        #region Variables
        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        private Render.DropdownList _dropdownList;
        #endregion

        #region Properties
        /// <summary>
        /// A list of strings selectable in the control
        /// </summary>
        [Description("A list of strings selectable in the control"), Category("Data"), MergableProperty(false),
         Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [XmlElement("Item")]
        public MonitoredCollection<string> Items { get; } = new MonitoredCollection<string>();

        private string _selectedItem;

        /// <summary>
        /// The currently selected item in the control
        /// </summary>
        [DefaultValue(""), Description("The currently selected item in the control"), Category("Data"), MergableProperty(false)]
        public string SelectedItem
        {
            get
            {
                if (_dropdownList == null) return _selectedItem;

                try
                {
                    ListItem item = _dropdownList.GetSelectedItem();
                    return string.IsNullOrEmpty(item.ItemTag) ? item.ItemText : item.ItemTag;
                }
                catch (InvalidOperationException)
                {
                    return "";
                }
            }
            set
            {
                _selectedItem = value;
                if (!string.IsNullOrEmpty(value) && _dropdownList != null)
                    _dropdownList.SetSelected(Parent.GetLocalized(value));
            }
        }

        #region Events
        /// <summary>
        /// A Lua script to execute when the control's value has changed
        /// </summary>
        [DefaultValue(""), Description("A Lua script to execute when the control's value has changed"), Category("Events"), FileType("Lua")]
        [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
        public string OnChanged { get; set; }
        #endregion

        #endregion

        #region Constructor
        public DropdownList()
        {
            Items.Changed += delegate { if (_dropdownList != null) LoadItems(); };

            Size = new Size(140, 30);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            // Add control to dialog
            UpdateLayout();
            DXControl = _dropdownList =
                Parent.DialogRender.AddDropdownList(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, Hotkey, Default);
            ControlModel.IsVisible = IsVisible;
            ControlModel.IsEnabled = IsEnabled;

            // Add items to DropdownList
            LoadItems();

            // Setup event hooks
            SetupMouseEvents();
            if (!string.IsNullOrEmpty(OnClick))
                _dropdownList.Click += delegate { Parent.RaiseEvent(OnClick, Name + "_Click"); };
            if (!string.IsNullOrEmpty(OnChanged))
                _dropdownList.Changed += delegate { Parent.RaiseEvent(OnChanged, Name + "_Changed"); };
        }

        private void LoadItems()
        {
            _dropdownList.Clear();
            foreach (string item in Items)
                if (!string.IsNullOrEmpty(item)) _dropdownList.AddItem(Parent.GetLocalized(item), item, null);

            if (!string.IsNullOrEmpty(_selectedItem))
            {
                try
                {
                    _dropdownList.SetSelected(Parent.GetLocalized(_selectedItem));
                }
                catch (InvalidOperationException)
                {}
            }
        }
        #endregion
    }
}
