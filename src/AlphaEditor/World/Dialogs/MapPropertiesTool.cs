/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using Common.Undo;
using TemplateWorld;

namespace AlphaEditor.World.Dialogs
{
    /// <summary>
    /// Allows the user to modify the properties of a <see cref="IUniverse"/>.
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
        private IUniverse _universe;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new map properties tool window.
        /// </summary>
        /// <param name="universe">The map data to modify.</param>
        public MapPropertiesTool(IUniverse universe)
        {
            InitializeComponent();

            UpdateUniverse(universe);
        }
        #endregion

        //--------------------//

        /// <summary>
        /// Updates the <see cref="IUniverse"/> object being represented by this window.
        /// </summary>
        /// <param name="universe">The new <see cref="IUniverse"/> object. If it is the same object as the old one, cached values will be refreshed.</param>
        public void UpdateUniverse(IUniverse universe)
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
