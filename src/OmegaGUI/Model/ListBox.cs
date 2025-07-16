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
using System.Linq;
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
    /// List box control
    /// </summary>
    public class ListBox : Control
    {
        #region Variables
        /// <summary>
        /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
        /// </summary>
        private Render.ListBox _listBox;
        #endregion

        #region Properties
        private ListBoxStyle _style;

        /// <summary>
        /// The style of the list box
        /// </summary>
        [DefaultValue(ListBoxStyle.SingleSelection), Description("The style of the list box"), Category("Behavior")]
        public ListBoxStyle Style
        {
            get => _listBox?.Style ?? _style;
            set
            {
                _style = value;
                if (_listBox != null) _listBox.Style = value;
            }
        }

        /// <summary>
        /// A list of strings selectable in the control
        /// </summary>
        [Description("A list of strings selectable in the control"), Category("Data"), MergableProperty(false),
         Editor("System.Windows.Forms.Design.ListControlStringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [XmlElement("Item")]
        public MonitoredCollection<string> Items { get; } = [];

        private string _selectedItem;

        /// <summary>
        /// The currently selected item in the control
        /// </summary>
        [DefaultValue(""), Description("The currently selected item in the control"), Category("Data"), MergableProperty(false)]
        public string SelectedItem
        {
            get
            {
                if (_listBox == null) return _selectedItem;

                try
                {
                    ListItem item = _listBox.GetSelectedItem();
                    return string.IsNullOrEmpty(item.ItemTag) ? item.ItemText : item.ItemTag;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return "";
                }
            }
            set
            {
                _selectedItem = value;
                if (_listBox != null) LoadItems();
            }
        }

        #region Events
        /// <summary>
        /// A Lua script to execute when the user selects an entry
        /// </summary>
        [DefaultValue(""), Description("A Lua script to execute when the user selects an entry"), Category("Events"), FileType("Lua")]
        [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
        public string OnSelection { get; set; }

        /// <summary>
        /// A Lua script to execute when the user double-clicks onto an entry
        /// </summary>
        [DefaultValue(""), Description("A Lua script to execute when the user double-clicks onto an entry"), Category("Events"), FileType("Lua")]
        [Editor(typeof(CodeEditor), typeof(UITypeEditor))]
        public string OnDoubleClick { get; set; }
        #endregion

        #endregion

        #region Constructor
        public ListBox()
        {
            Items.Changed += delegate { if (_listBox != null) LoadItems(); };

            Size = new(150, 100);
        }
        #endregion

        #region Generate
        internal override void Generate()
        {
            // Add control to dialog
            UpdateLayout();
            DXControl = _listBox =
                Parent.DialogRender.AddListBox(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, _style);
            ControlModel.IsVisible = IsVisible;
            ControlModel.IsEnabled = IsEnabled;

            // Add items to ListBox
            LoadItems();

            // Setup event hooks
            SetupMouseEvents();
            if (!string.IsNullOrEmpty(OnSelection))
                _listBox.Selection += delegate { Parent.RaiseEvent(OnSelection, Name + "_Selection"); };
            if (!string.IsNullOrEmpty(OnDoubleClick))
                _listBox.DoubleClick += delegate { Parent.RaiseEvent(OnDoubleClick, Name + "_DoubleClick"); };
        }

        private void LoadItems()
        {
            _listBox.Clear();
            foreach (string item in Items.Where(x => !string.IsNullOrEmpty(x)))
                _listBox.AddItem(Parent.GetLocalized(item), item, null);

            if (!string.IsNullOrEmpty(_selectedItem))
            {
                try
                {
                    _listBox.SetSelected(Parent.GetLocalized(_selectedItem));
                }
                catch (InvalidOperationException)
                {}
            }
        }
        #endregion
    }
}
