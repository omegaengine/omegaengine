/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;
using Common.Controls;

namespace OmegaEngine
{

    #region Delegates
    /// <seealso cref="GameBase.GameForm.WindowMessage"/>
    /// <param name="m">The window message</param>
    public delegate bool MessageEventHandler(Message m);
    #endregion

    partial class GameBase
    {
        /// <summary>
        /// An internal Windows Form to use as <see cref="Engine"/> render target with mouse+keyboard event handling
        /// </summary>
        /// <seealso cref="GameBase.Form"/>
        protected class GameForm : TouchForm
        {
            #region Events
            /// <summary>
            /// Occurs when a window message is to processed
            /// </summary>
            [Description("Occurs when a window message is to processed")]
            public event MessageEventHandler WindowMessage;
            #endregion

            #region Variables
            /// <summary>True once <see cref="GameBase.Dispose()"/> has been called</summary>
            private bool _isDisposed;

            /// <summary>
            /// A <see cref="Label"/> for displaying loading messages during startup
            /// </summary>
            internal readonly Label LoadingLabel = new Label
            {
                Text = "Loading...",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Size = new Size(0, 100),
                Font = new Font("Arial", 26.25f, FontStyle.Regular, GraphicsUnit.Point, 0),
                ForeColor = Color.White
            };
            #endregion

            #region Constructor
            public GameForm()
            {
                SetStyle(ControlStyles.UserPaint, true);
                SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                Disposed += delegate { _isDisposed = true; };
                Controls.Add(LoadingLabel);
            }
            #endregion

            //--------------------//

            #region Window messages
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            protected override void WndProc(ref Message m)
            {
                if (WindowMessage != null)
                {
                    // Allow external handling of window messages
                    if (WindowMessage(m))
                    {
                        m.Result = new IntPtr(1); // Indicate to Windows that the message was handled
                        return; // Suppress any additional event handlers that might be registered further along the line
                    }
                }

                // Previous event handler may have caused the form to dispose
                if (_isDisposed) return;

                // Invoke any registered event handlers, default actions, etc.
                base.WndProc(ref m);
            }
            #endregion
        }
    }
}
