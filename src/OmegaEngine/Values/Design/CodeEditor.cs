/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace OmegaEngine.Values.Design;

/// <summary>
/// An editor that can be associated with <c>string</c> properties. Uses <see cref="TextEditorControl"/>.
/// </summary>
/// <seealso cref="FileTypeAttribute"/>
public class CodeEditor : UITypeEditor
{
    /// <inheritdoc/>
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

    /// <inheritdoc/>
    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
        #region Sanity checks
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        #endregion

        if (provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService editorService) return value;

        var editorControl = new TextEditorControl {Text = value as string, Dock = DockStyle.Fill};
        var form = new Form
        {
            FormBorderStyle = FormBorderStyle.SizableToolWindow,
            ShowInTaskbar = false,
            Controls = {editorControl}
        };

        if (context.PropertyDescriptor?.Attributes.OfType<FileTypeAttribute>().FirstOrDefault() is {} fileType)
        {
            editorControl.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategy(fileType.FileType);
            form.Text = fileType.FileType + " Editor";
        }
        else form.Text = "Editor";

        editorService.ShowDialog(form);
        return editorControl.Text;
    }
}
