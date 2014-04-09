/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;
using AlphaFramework.World.Terrains;
using NanoByte.Common.Controls;
using NanoByte.Common.Utils;

namespace AlphaFramework.Editor.World.Dialogs
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
            var size = new TerrainSize(126, 126, stretchH: 30);
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
