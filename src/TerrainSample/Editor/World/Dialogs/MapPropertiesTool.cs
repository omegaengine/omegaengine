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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Common.Undo;
using World;

namespace AlphaEditor.World.Dialogs
{
    /// <summary>
    /// Allows the user to modify the properties of a <see cref="TerrainUniverse"/>.
    /// </summary>
    /// <remarks>This is a non-modal floating toolbox window. Communication is handled via events (<see cref="ExecuteCommand"/>).</remarks>
    public sealed partial class MapPropertiesTool : Form
    {
        #region Events
        /// <summary>
        /// Occurs when a command is to be executed by the owning tab.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when a command is to be executed by the owning tab.")]
        public event Action<IUndoCommand> ExecuteCommand;

        private void OnExecuteCommand(IUndoCommand command)
        {
            if (ExecuteCommand != null) ExecuteCommand(command);
        }
        #endregion

        #region Variables
        private TerrainUniverse _universe;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new map properties tool window.
        /// </summary>
        /// <param name="universe">The map data to modify.</param>
        public MapPropertiesTool(TerrainUniverse universe)
        {
            InitializeComponent();

            UpdateUniverse(universe);
        }
        #endregion

        //--------------------//

        /// <summary>
        /// Updates the <see cref="TerrainUniverse"/> object being represented by this window.
        /// </summary>
        /// <param name="universe">The new <see cref="TerrainUniverse"/> object. If it is the same object as the old one, cached values will be refreshed.</param>
        public void UpdateUniverse(TerrainUniverse universe)
        {
            _universe = universe;
            propertyGridUniverse.SelectedObject = universe;
        }

        private void propertyGridUniverse_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            // Add undo-entry for changed property
            OnExecuteCommand(new PropertyChangedCommand(_universe, e));
        }
    }
}
