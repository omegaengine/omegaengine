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
using System.ComponentModel;
using System.Windows.Forms;
using Common.Controls;
using Common.Utils;
using TerrainSample.World.Terrains;

namespace TerrainSample.Editor.World.Dialogs
{
    /// <summary>
    /// Displays and edits a <see cref="TerrainSize"/>. Can also be used to create a new <see cref="TerrainSize"/>.
    /// </summary>
    /// <remarks>This dialog is a modal dialog. Communication is handled via the static methods (<see cref="Create"/>, <see cref="Edit"/>).</remarks>
    public sealed partial class TerrainSizeDialog : OKCancelDialog
    {
        #region Constructor
        private TerrainSizeDialog()
        {
            InitializeComponent();
            Shown += delegate { WindowsUtils.SetForegroundWindow(this); };
        }
        #endregion

        //--------------------//

        #region Get input
        /// <summary>
        /// Get a new <see cref="TerrainSize"/>.
        /// </summary>
        /// <returns>The new <see cref="TerrainSize"/>.</returns>
        public static TerrainSize Create()
        {
            var size = new TerrainSize(126, 126, 30, 1);
            Edit(ref size);
            return size;
        }

        /// <summary>
        /// Modify an existing <see cref="TerrainSize"/>.
        /// </summary>
        /// <param name="size">The <see cref="TerrainSize"/> to modify</param>
        /// <exception cref="OperationCanceledException">Thrown when the user clicked the cancel button.</exception>
        public static void Edit(ref TerrainSize size)
        {
            var dialog = new TerrainSizeDialog
            {
                // Display existing data in dialog
                numericX = {Value = size.X},
                numericY = {Value = size.Y},
                numericStretchH = {Value = ((decimal)size.StretchH)},
                numericStretchV = {Value = ((decimal)size.StretchV)}
            };

            if (dialog.ShowDialog() == DialogResult.Cancel)
                throw new OperationCanceledException();

            // Get new data from dialog
            size.X = (int)dialog.numericX.Value;
            size.Y = (int)dialog.numericY.Value;
            size.StretchH = (float)dialog.numericStretchH.Value;
            size.StretchV = (float)dialog.numericStretchV.Value;
        }
        #endregion

        #region Numeric validation
        private void numericSize_Validating(object sender, CancelEventArgs e)
        {
            // Make sure this message came from a NumericUpDown control 
            var numericControl = sender as NumericUpDown;
            if (numericControl == null) return;

            // Round values to a multiple of 3
            int roundValue = (int)Math.Round(numericControl.Value / 3) * 3;
            // ReSharper disable RedundantCheckBeforeAssignment
            if (numericControl.Value != roundValue)
                numericControl.Value = roundValue;
            // ReSharper restore RedundantCheckBeforeAssignment
        }
        #endregion
    }
}
