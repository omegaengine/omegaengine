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
using System.IO;
using System.Windows.Forms;
using AlphaFramework.Editor.Properties;
using NanoByte.Common;
using NanoByte.Common.Controls;
using SlimDX.Direct3D9;

namespace AlphaFramework.Editor
{
    /// <summary>
    /// A base class for all editor windows that can be displayed as tabs
    /// </summary>
    /// <remarks>Call <see cref="OnChange"/> after each change to make sure the save and undo buttons are enabled</remarks>
    public abstract class Tab : UserControl
    {
        #region Events
        /// <summary>
        /// Occurs after the tab was closed
        /// </summary>
        [Description("Occurs after the tab was closed")]
        public event EventHandler TabClosed;
        #endregion

        #region Variables
        private bool _startupDone;

        // ReSharper disable InconsistentNaming
        /// <summary><c>true</c> if an existing file supposed to be overwritten when <see cref="SaveFile"/> is called.</summary>
        protected bool _overwrite;

        /// <summary>The complete (rooted) path to load and/or save the file from/to</summary>
        protected string _fullPath;

        // ReSharper restore InconsistentNaming

        /// <summary>
        /// True when unsaved changes to the content exist
        /// </summary>
        protected bool Changed;

        /// <summary>
        /// Provides a surface for displaying toast messages to the user.
        /// </summary>
        protected IToastProvider ToastProvider;
        #endregion

        #region Properties
        /// <summary>
        /// The user-friendly name of this editor
        /// </summary>
        [Category("Design"), Description("The user-friendly name of this editor")]
        public string NameUI { get; set; }

        /// <summary>
        /// The path to the file in this editor
        /// </summary>
        [Browsable(false)]
        public string FilePath { get; protected set; }
        #endregion

        #region Constructor
        /// <inheritdoc/>
        protected Tab()
        {
            Size = new(720, 540);

            Load += delegate
            {
                BorderStyle = BorderStyle.FixedSingle;
                BackColor = SystemColors.Control;
            };

            #region Close Button
            var buttonClose = new Button
            {
                Name = "closeButton",
                FlatStyle = FlatStyle.Flat,
                Text = @"X",
                Font = new("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(224, 118, 92),
                TabStop = false,
                Size = new(22, 22),
                Top = (-1)
            };
            buttonClose.Click += delegate { RequestClose(); };

            // Prevent button from getting focus
            buttonClose.TabIndex = int.MaxValue;
            buttonClose.GotFocus += delegate { FocusDefaultControl(); };

            Controls.Add(buttonClose);
            Resize += delegate { buttonClose.Left = Width - buttonClose.Width - 1; };
            #endregion

            #region Save Button
            var saveButton = new Button
            {
                Name = "closeButton",
                FlatStyle = FlatStyle.Flat,
                Image = Resources.SaveButton,
                BackColor = Color.White,
                TabStop = false,
                Size = new(22, 22),
                Top = (buttonClose.Top + buttonClose.Height - 1)
            };
            saveButton.Click += delegate { SaveFile(); };

            // Prevent button from getting focus
            saveButton.TabIndex = int.MaxValue;
            saveButton.GotFocus += delegate { FocusDefaultControl(); };

            Controls.Add(saveButton);
            Resize += delegate { saveButton.Left = Width - saveButton.Width - 1; };
            #endregion
        }

        /// <summary>
        /// Set the focus back to the default control on the tab
        /// </summary>
        protected void FocusDefaultControl()
        {
            foreach (Control control in Controls)
            {
                if (control.TabIndex == 0)
                {
                    control.Focus();
                    return;
                }
            }
        }
        #endregion

        //--------------------//

        #region Handlers
        /// <summary>
        /// Called on startup to load the content for this tab.
        /// </summary>
        /// <exception cref="NotSupportedException">An unsupported graphics card feature is used.</exception>
        /// <exception cref="FileNotFoundException">A file could not be located.</exception>
        /// <exception cref="IOException">There was a problem reading a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to a file was denied.</exception>
        /// <exception cref="InvalidOperationException">There was a problem inside the engine.</exception>
        /// <exception cref="InvalidDataException">A file contained invalid data.</exception>
        protected virtual void OnInitialize()
        {}

        /// <summary>
        /// Called when the content of this tab is to be saved to a file.
        /// </summary>
        /// <exception cref="ArgumentException">The file path is invalid.</exception>
        /// <exception cref="NotSupportedException">The file path is invalid.</exception>
        /// <exception cref="IOException">There was a problem writing a file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to a file was denied.</exception>
        protected virtual void OnSaveFile()
        {
            Changed = false;
        }

        /// <summary>
        /// Hook to delete the currently selected object in this tab
        /// </summary>
        protected virtual void OnDelete()
        {}

        /// <summary>
        /// Called on startup, content updates and tab switch to refresh any on-screen displays
        /// </summary>
        protected virtual void OnUpdate()
        {}

        /// <summary>
        /// Mark the content of this tab as changed (needs to be saved)
        /// </summary>
        protected virtual void OnChange()
        {
            Changed = true;
        }

        /// <summary>
        /// Called when the tab needs to shutdown and close
        /// </summary>
        protected virtual void OnClose()
        {
            Dispose();
            TabClosed?.Invoke(this, EventArgs.Empty);

            Log.Info("Tab closed: " + FilePath);
        }
        #endregion

        #region Open
        /// <summary>
        /// Opens the tab.
        /// </summary>
        /// <param name="toastProvider">Provides a surface for displaying toast messages to the user.</param>
        /// <exception cref="OperationCanceledException">The user has been informed of an error and has confirmed the resulting cancellation.</exception>
        public void Open(IToastProvider toastProvider)
        {
            ToastProvider = toastProvider;

            Show();
            if (!_startupDone)
            {
                Application.DoEvents();

                // Load the file data for the tab
                try
                {
                    OnInitialize();
                }
                    #region Error handling
                catch (OperationCanceledException)
                {
                    OnClose();
                    throw new OperationCanceledException();
                }
                    //catch (ArgumentException)
                    //{
                    //    Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Error);
                    //    OnClose();
                    //    throw new OperationCanceledException();
                    //}
                catch (NotSupportedException)
                {
                    Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (FileNotFoundException ex)
                {
                    Msg.Inform(this, Resources.FileNotFound + "\n" + ex.FileName, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (IOException ex)
                {
                    Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (InvalidDataException ex)
                {
                    Msg.Inform(this, Resources.FileDamaged + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (Direct3D9NotFoundException)
                {
                    Msg.Inform(this, Resources.DirectXMissing, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (Direct3DX9NotFoundException)
                {
                    Msg.Inform(this, Resources.DirectXMissing, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (Direct3D9Exception ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                catch (SlimDX.DirectSound.DirectSoundException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    OnClose();
                    throw new OperationCanceledException();
                }
                #endregion
            }

            // Initialize/update the data display
            try
            {
                OnUpdate();
            }
                #region Error handling
            catch (NotSupportedException)
            {
                Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Error);
                ForceClose();
                throw new OperationCanceledException();
            }
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, Resources.FileNotFound + "\n" + ex.FileName, MsgSeverity.Error);
                ForceClose();
                throw new OperationCanceledException();
            }
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Error);
                ForceClose();
                throw new OperationCanceledException();
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.FileNotLoadable + "\n" + ex.Message, MsgSeverity.Error);
                ForceClose();
                throw new OperationCanceledException();
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, Resources.FileDamaged + "\n" + ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                ForceClose();
                throw new OperationCanceledException();
            }
            #endregion

            // Replace the "Loading" message with the actual title
            Text = NameUI + @": " + Path.GetFileName(FilePath);

            _startupDone = true;
        }
        #endregion

        #region Hide
        /// <summary>
        /// Hides this tab (so that another can be brought to the foreground)
        /// </summary>
        public new void Hide()
        {
            if (!Visible) return;
            Visible = false;
        }
        #endregion

        //--------------------//

        #region Close
        /// <summary>
        /// Requests the tab to close, displaying warnings about unsaved data, etc. allowing the user to cancel.
        /// </summary>
        /// <returns><c>true</c> if the tab was closed, <c>false</c> if the user canceled the process</returns>
        public bool RequestClose()
        {
            if (Changed)
            {
                switch (Msg.YesNoCancel(this, Resources.SaveChanges, MsgSeverity.Warn, Resources.SaveChangesYes, Resources.SaveChangesNo))
                {
                    case DialogResult.Yes:
                        if (SaveFile())
                        {
                            OnClose();
                            return true;
                        }
                        return false;
                    case DialogResult.No:
                        OnClose();
                        return true;
                    default:
                        return false;
                }
            }

            OnClose();
            return true;
        }

        /// <summary>
        /// Forces the tab to close, displaying warnings about unsaved data, etc. but not allowing the user to cancel.
        /// </summary>
        protected void ForceClose()
        {
            if (Changed)
            {
                if (Msg.YesNo(this, Resources.SaveChanges, MsgSeverity.Warn, Resources.SaveChangesYes, Resources.SaveChangesNo))
                    SaveFile();
            }

            OnClose();
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the content of this tab to a file - error-handling included!
        /// </summary>
        /// <returns><c>true</c> if file was saved, <c>false</c> if an error occurred</returns>
        public bool SaveFile()
        {
            try
            {
                OnSaveFile();
            }
                #region Error handling
                //catch (ArgumentException)
                //{
                //    Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Warn);
                //    return false;
                //}
            catch (NotSupportedException)
            {
                Msg.Inform(this, Resources.InvalidFilePath, MsgSeverity.Warn);
                return false;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Warn);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, Resources.FileNotSavable + "\n" + ex.Message, MsgSeverity.Warn);
                return false;
            }
            #endregion

            ToastProvider.ShowToast(Resources.SavedFile);

            return true;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete the currently selected object in this tab
        /// </summary>
        public void Delete()
        {
            OnDelete();
        }
        #endregion

        //--------------------//

        #region Undo
        /// <summary>
        /// Hook to undo the last change
        /// </summary>
        public virtual void Undo()
        {}
        #endregion

        #region Redo
        /// <summary>
        /// Hook to redo the last undone change
        /// </summary>
        public virtual void Redo()
        {}
        #endregion
    }
}
