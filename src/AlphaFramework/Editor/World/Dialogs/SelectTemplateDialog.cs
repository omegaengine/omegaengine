/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Forms;
using AlphaFramework.Editor.Properties;
using AlphaFramework.World.Templates;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;

namespace AlphaFramework.Editor.World.Dialogs
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
        /// The name of the <see cref="Template{T}"/> the user selected; <c>null</c> if none.
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

            buttonOK.Enabled = false;
            _templateList.SelectionConfirmed += delegate
            {
                DialogResult = DialogResult.OK;
                OnOKClicked();
                Close();
            };
            _templateList.SelectedEntryChanged += delegate { buttonOK.Enabled = (_templateList.SelectedEntry != null); };
            Controls.Add(_templateList);

            _templateList.Nodes = templates;
        }
        #endregion
    }
}
