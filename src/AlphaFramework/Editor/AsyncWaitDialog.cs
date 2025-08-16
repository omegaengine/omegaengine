/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common.Native;

namespace AlphaFramework.Editor;

/// <summary>
/// Displays a "Loading..." dialog box in a separate GUI thread for cases where the main message pump is blocked.
/// </summary>
public sealed partial class AsyncWaitDialog : Form
{
    #region Variables
    private readonly Thread _thread;

    /// <summary>A barrier that blocks threads until the window handle is ready.</summary>
    private readonly ManualResetEvent _handleReady = new(initialState: false);
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new asynchrnous waiting dialog.
    /// </summary>
    /// <param name="title">The title of th dialog to display.</param>
    /// <param name="icon">The icon for the dialog to display in the task bar; can be <c>null</c>.</param>
    public AsyncWaitDialog([NotNull, Localizable(true)] string title, [CanBeNull] Icon icon = null)
    {
        InitializeComponent();

        Text = title;
        Icon = icon;

        HandleCreated += delegate { _handleReady.Set(); };
        HandleDestroyed += delegate { _handleReady.Reset(); };

        _thread = new(() => Application.Run(this));
    }
    #endregion

    #region Event handlers
    private void AsyncWaitDialog_Shown(object sender, EventArgs e)
        => WindowsTaskbar.SetProgressState(Handle, WindowsTaskbar.ProgressBarState.Indeterminate);

    private void AsyncWaitDialog_FormClosing(object sender, FormClosingEventArgs e)
        => WindowsTaskbar.SetProgressState(Handle, WindowsTaskbar.ProgressBarState.NoProgress);
    #endregion

    #region Control
    /// <summary>
    /// Starts a new message pump with this dialog in a new thread.
    /// </summary>
    public void Start()
    {
        _thread.Start();
        _handleReady.WaitOne();
        Application.DoEvents();
    }

    /// <summary>
    /// Closes the dialog and stops the separate message pump.
    /// </summary>
    public void Stop()
    {
        Invoke(new Action(Close));
        _thread.Join();
    }
    #endregion
}
