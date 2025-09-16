/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;

namespace OmegaEngine;

partial class RenderHost
{
    /// <summary>
    /// Stops the render loop and closes the window
    /// </summary>
    public void Exit() => Form.Close();

    /// <summary>
    /// Sets up the <see cref="System.Windows.Forms.Form"/> for fullscreen display
    /// </summary>
    protected void ToFullscreen()
    {
        // Cannot show debug-console in fullscreen-mode
        _debugConsole?.Close();

        // Make the window borderless
        Form.FormBorderStyle = FormBorderStyle.None;

        // Place the window in the top-left corner
        Form.StartPosition = FormStartPosition.Manual;
        Form.Location = new(0, 0);

        // Make the window fill the entire screen (no need to maximize the window)
        Form.Size = Screen.PrimaryScreen.Bounds.Size;
        Fullscreen = true;
    }

    /// <summary>
    /// Sets up the <see cref="System.Windows.Forms.Form"/> for normal windowed (non-fullscreen) mode
    /// </summary>
    /// <param name="size">The window size</param>
    protected void ToWindowed(Size size)
    {
        // Restore the default window border
        Form.FormBorderStyle = FormBorderStyle.Sizable;
        Form.TopMost = false;
        Application.DoEvents(); // Ensure window control box comes back after fullscreen mode

        // Update the window size
        Form.ClientSize = size;

        // Move the window away from the top-left corner if we're coming from fullscreen-mode
        if (Fullscreen) Form.Location = new(20, 20);

        Fullscreen = false;
    }

    /// <summary>
    /// Updates the current status of the loading process
    /// </summary>
    /// <param name="message">The new message to display</param>
    protected void UpdateStatus(string message)
    {
        Form.LoadingLabel.Text = message;
        Form.LoadingLabel.Refresh();
    }

    /// <summary>
    /// An internal Windows Form to use as <see cref="Engine"/> render target with mouse+keyboard event handling
    /// </summary>
    /// <seealso cref="RenderHost.Form"/>
    protected class RenderForm : TouchForm
    {
        /// <summary>
        /// Occurs when a window message is to be processed
        /// </summary>
        [Description("Occurs when a window message is to be processed")]
        public event MessageEventHandler? WindowMessage;

        /// <summary>True once <see cref="RenderHost.Dispose()"/> has been called</summary>
        private bool _isDisposed;

        /// <summary>
        /// A <see cref="Label"/> for displaying loading messages during startup
        /// </summary>
        internal readonly Label LoadingLabel = new()
        {
            Text = "Loading...",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Bottom,
            Size = new(0, 100),
            Font = new("Arial", 26.25f, FontStyle.Regular, GraphicsUnit.Point, 0),
            ForeColor = Color.White
        };

        public RenderForm()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            Disposed += delegate { _isDisposed = true; };
            Controls.Add(LoadingLabel);
        }

        protected override void WndProc(ref Message m)
        {
            if (WindowMessage != null)
            {
                // Allow external handling of window messages
                if (WindowMessage(m))
                {
                    m.Result = new(1); // Indicate to Windows that the message was handled
                    return; // Suppress any additional event handlers that might be registered further along the line
                }
            }

            // Previous event handler may have caused the form to dispose
            if (_isDisposed) return;

            // Invoke any registered event handlers, default actions, etc.
            base.WndProc(ref m);
        }
    }

    private void Form_Shown(object sender, EventArgs e)
    {
        // Start rendering once WinForms has completed all startup operations
        Form.Refresh();

        Loading = true;

        // Only proceed if the Initialize method succeeds
        if (Initialize())
        {
            // Remove the loading-background before rendering starts
            Form.LoadingLabel.Visible = false;
            if (Form.BackgroundImage != null)
            {
                Form.BackgroundImage.Dispose();
                Form.BackgroundImage = null;
            }

            // Keep the timer outside of the event handling
            var timer = Stopwatch.StartNew();

            // Hook the render loop into the application's idle event
            Application.Idle += delegate
            {
                while (WinFormsUtils.AppIdle && Form.Visible)
                {
                    // Don't waste too much CPU time if the render window isn't active
                    if (Form.WindowState == FormWindowState.Minimized)
                        Thread.Sleep(500);

                    // Start the timer over
                    double elapsedTime = timer.Elapsed.TotalSeconds;
                    timer.Reset();
                    timer.Start();

                    // Loop the Render method
                    Render(elapsedTime);
                }
            };

            Loading = false;
        }
    }
}
