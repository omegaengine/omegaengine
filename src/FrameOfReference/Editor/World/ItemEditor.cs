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

using AlphaFramework.Editor.Properties;
using AlphaFramework.World.Templates;
using FrameOfReference.World.Templates;
using NanoByte.Common.Controls;

namespace FrameOfReference.Editor.World
{
    /// <summary>
    /// Allows the user to edit <see cref="ItemTemplate"/>s
    /// </summary>
    public partial class ItemEditor : ItemEditorDesignerShim
    {
        #region Constructor
        /// <summary>
        /// Creates a new item editor.
        /// </summary>
        public ItemEditor()
        {
            InitializeComponent();

            // Hard-coded filename
            FilePath = Template<ItemTemplate>.FileName;
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <summary>
        /// Called when the user wants a new <see cref="ItemTemplate"/> to be added.
        /// </summary>
        protected override void OnNewTemplate()
        {
            string newName = InputBox.Show(this, "Item Template Name", Resources.EnterTemplateName);

            #region Error handling
            if (string.IsNullOrEmpty(newName)) return;
            if (Content.Contains(newName))
            {
                Msg.Inform(this, Resources.NameInUse, MsgSeverity.Warn);
                return;
            }
            #endregion

            var itemClass = new ItemTemplate {Name = newName};
            Content.Add(itemClass);
            OnChange();
            OnUpdate();

            // Select the new entry
            TemplateList.SelectedEntry = itemClass;
        }
        #endregion
    }
}
