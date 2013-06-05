/*
 * Copyright 2006-2013 Bastian Eicher
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

using System.Windows.Forms;
using AlphaEditor.Properties;
using Common.Collections;
using Common.Controls;
using World;

namespace AlphaEditor.World.Dialogs
{
    /// <summary>
    /// Dialog for selecting <see cref="Template{T}"/>es (with a preview pane).
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Template{T}"/>es to select.</typeparam>
    /// <remarks>This dialog is a modal dialog.Communication is handled via the <see cref="DialogResult"/> and properties (<see cref="SelectedTemplate"/>)</remarks>
    public sealed class SelectTemplateDialog<T> : OKCancelDialog where T : Template<T>
    {
        #region Variables
        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<T> _templateList = new FilteredTreeView<T>
        {
            Location = new System.Drawing.Point(12, 12),
            Size = new System.Drawing.Size(262, 209),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            TabIndex = 1
        };
        #endregion

        #region Properties
        /// <summary>
        /// The name of the <see cref="Template{T}"/> the user selected; <see langword="null"/> if none.
        /// </summary>
        public string SelectedTemplate { get { return (_templateList.SelectedEntry == null ? null : _templateList.SelectedEntry.Name); } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Template{T}"/> selection dialog
        /// </summary>
        /// <param name="templates">The list of <see cref="Template{T}"/>es to choose from</param>
        public SelectTemplateDialog(NamedCollection<T> templates)
        {
            Text = Resources.TemplateSelection;
            _templateList.SelectionConfirmed += delegate
            {
                DialogResult = DialogResult.OK;
                OnOKClicked();
                Close();
            };
            Controls.Add(_templateList);

            _templateList.Nodes = templates;
        }
        #endregion
    }
}
