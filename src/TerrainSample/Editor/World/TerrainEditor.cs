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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AlphaEditor.Properties;
using Common;
using Common.Controls;
using Common.Storage;
using World;
using World.Templates;

namespace AlphaEditor.World
{
    /// <summary>
    /// Allows the user to edit <see cref="TerrainTemplate"/>s
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance", Justification = "The designer-shim class doesn't add any complexity, it only prevents the WinForms designer from getting confused")]
    public partial class TerrainEditor : TerrainEditorDesignerShim
    {
        #region Constructor
        /// <summary>
        /// Creates a new terrain editor.
        /// </summary>
        public TerrainEditor()
        {
            InitializeComponent();

            // Hard-coded filename
            FilePath = TemplateManager.TerrainFileName;
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <inheritdoc />
        protected override void OnUpdate()
        {
            // Enable button only when a class is selected
            buttonBrowse.Enabled = textPath.Enabled = (TemplateList.SelectedEntry != null);

            UpdateTexturePreview();

            base.OnUpdate();
        }

        /// <summary>
        /// Called when the user wants a new <see cref="TerrainTemplate"/> to be added.
        /// </summary>
        protected override void OnNewTemplate()
        {
            string newName = InputBox.Show(this, "Terrain Class Name", Resources.EnterTemplateName);

            #region Error handling
            if (string.IsNullOrEmpty(newName)) return;
            if (Templates.Contains(newName))
            {
                Msg.Inform(this, Resources.NameInUse, MsgSeverity.Warn);
                return;
            }
            #endregion

            var itemClass = new TerrainTemplate {Name = newName};
            Templates.Add(itemClass);
            OnChange();
            OnUpdate();

            // Select the new entry
            TemplateList.SelectedEntry = itemClass;
        }
        #endregion

        #region Texture preview
        private void UpdateTexturePreview()
        {
            if (TemplateList.SelectedEntry != null && !string.IsNullOrEmpty(TemplateList.SelectedEntry.Texture))
            {
                textPath.Text = TemplateList.SelectedEntry.Texture;
                errorProvider.SetError(textPath, null); // Reset any previous error messages

                // Preview the texture
                try
                {
                    if (pictureTexture.Image != null) pictureTexture.Image.Dispose();
                    using (Stream stream = ContentManager.GetFileStream("Textures/Terrain", TemplateList.SelectedEntry.Texture))
                        pictureTexture.Image = Image.FromStream(stream);
                }
                    #region Error handling
                catch (ArgumentException)
                {
                    // GDI+ unsupported file format - no reason to worry
                    pictureTexture.Image = null;
                }
                catch (IOException ex)
                {
                    pictureTexture.Image = null;
                    errorProvider.SetError(textPath, ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    pictureTexture.Image = null;
                    errorProvider.SetError(textPath, ex.Message);
                }
                #endregion
            }
            else
            {
                textPath.Text = "";
                pictureTexture.Image = null;
            }
        }
        #endregion

        //--------------------//

        #region Buttons
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            // Get a terrain texture file path
            string path;
            if (!FileSelectorDialog.TryGetPath("Textures/Terrain", ".*", out path)) return;

            // Apply the texture file
            TemplateList.SelectedEntry.Texture = path;
            OnChange();

            OnUpdate();
        }
        #endregion

        #region Path TextBox
        private void textPath_Leave(object sender, EventArgs e)
        {
            if (TemplateList.SelectedEntry != null && TemplateList.SelectedEntry.Texture != textPath.Text)
            {
                // Apply the texture file
                TemplateList.SelectedEntry.Texture = textPath.Text;
                OnChange();

                OnUpdate();
            }
        }

        private void textPath_KeyDown(object sender, KeyEventArgs e)
        {
            // Pretend we are leaving the text box when the Enter key is pressed
            if (e.KeyCode == Keys.Enter)
                textPath_Leave(sender, e);
        }
        #endregion
    }
}
