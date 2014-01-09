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
using OmegaGUI.Model;

namespace AlphaFramework.Editor.Gui.Dialogs
{
    /// <summary>
    /// Allows the user to add a new <see cref="Control"/> to a <see cref="Dialog"/>.
    /// </summary>
    /// <remarks>This is a non-modal floating toolbox window. Communication is handled via events (<see cref="NewControl"/>).</remarks>
    public sealed partial class AddControlTool : System.Windows.Forms.Form
    {
        #region Events
        /// <summary>
        /// Occurs when a new control is to be added.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs when a new control is to be added.")]
        public event Action<Control> NewControl;

        private void OnNewControl(Control control)
        {
            if (NewControl != null) NewControl(control);
        }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public AddControlTool()
        {
            InitializeComponent();
        }
        #endregion

        //--------------------//

        #region Buttons
        private void buttonOK_Click(object sender, EventArgs e)
        {
            switch (typeBox.Text)
            {
                case "Button":
                    OnNewControl(new Button());
                    break;
                case "CheckBox":
                    OnNewControl(new CheckBox());
                    break;
                case "DropdownList":
                    OnNewControl(new DropdownList());
                    break;
                case "GroupBox":
                    OnNewControl(new GroupBox());
                    break;
                case "Label":
                    OnNewControl(new Label());
                    break;
                case "ListBox":
                    OnNewControl(new ListBox());
                    break;
                case "PictureBox":
                    OnNewControl(new PictureBox());
                    break;
                case "RadioButton":
                    OnNewControl(new RadioButton());
                    break;
                case "ScrollBar":
                    OnNewControl(new ScrollBar());
                    break;
                case "Slider":
                    OnNewControl(new Slider());
                    break;
                case "TextBox":
                    OnNewControl(new TextBox());
                    break;
            }

            Close();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
