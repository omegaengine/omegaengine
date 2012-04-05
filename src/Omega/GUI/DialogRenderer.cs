/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using Common;
using Common.Storage;
using Common.Utils;
using LuaInterface;
using OmegaEngine;

namespace OmegaGUI
{
    /// <summary>
    /// Manages the interface between <see cref="Render.Dialog"/> and <see cref="Model.Dialog"/>
    /// </summary>
    public class DialogRenderer : IDisposable
    {
        #region Variables
        private readonly GuiManager _dialogManager;

        private Point _location;
        #endregion

        #region Properties
        /// <summary>
        /// Has this dialog been disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        /// <summary>
        /// Text value to make it easier to identify a particular dialog
        /// </summary>
        public string Name { get; set; }

        public override string ToString()
        {
            string value = GetType().Name;
            if (!string.IsNullOrEmpty(Name))
                value += ": " + Name;
            return value;
        }

        /// <summary>
        /// The dialog model used for rendering
        /// </summary>
        [CLSCompliant(false)]
        public Render.Dialog DialogRender { get; private set; }

        /// <summary>
        /// The dialog view the dialog model is based on
        /// </summary>
        [CLSCompliant(false)]
        public Model.Dialog DialogModel { get; private set; }

        /// <summary>
        /// The <see cref="LuaInterface.Lua"/> interpreter used to execute event scripts
        /// </summary>
        [CLSCompliant(false)]
        public Lua Lua { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new dialog from an XML file
        /// </summary>
        /// <param name="manager">The DialogManager interfaces dialog view representation and actual rendering</param>
        /// <param name="filename">The filename of the XML file to load</param>
        /// <param name="location">The location of the dialog on the screen</param>
        public DialogRenderer(GuiManager manager, string filename, Point location)
            : this(manager, Model.Dialog.FromContent(filename), location, true)
        {
            Log.Info("Loading GUI dialog: " + filename);
            Name = filename;
        }

        /// <summary>
        /// Creates a new dialog from a <see cref="Model.Dialog"/>
        /// </summary>
        /// <param name="manager">The DialogManager interfaces dialog view representation and actual rendering</param>
        /// <param name="dialogView">The dialog to render</param>
        /// <param name="location">The location of the dialog on the screen</param>
        /// <param name="enableLua">Enable Lua scripting support for this dialog?</param>
        [CLSCompliant(false)]
        public DialogRenderer(GuiManager manager, Model.Dialog dialogView, Point location, bool enableLua)
        {
            _dialogManager = manager;
            DialogModel = dialogView;
            DialogRender = dialogView.GenerateDialog(_dialogManager.ModelManager);
            _location = location;

            LayoutHelper();
            _dialogManager.ModelManager.Engine.DeviceReset += LayoutHelper;

            if (enableLua)
            {
                Lua = LuaBuilder.Default();

                #region Register Lua variables and functions
                // Register all controls as direct variables when possible
                foreach (var control in dialogView.Controls)
                {
                    if (!string.IsNullOrEmpty(control.Name))
                    {
                        try
                        {
                            Lua[control.Name] = control;
                        }
                        catch (LuaException)
                        {}
                    }
                }

                Lua["Dialog"] = this;

                LuaRegistrationHelper.Enumeration<Render.MsgBoxType>(Lua);
                LuaRegistrationHelper.Enumeration<Render.MsgBoxResult>(Lua);
                #endregion

                dialogView.ScriptFired += LuaExecute;
            }
        }
        #endregion

        //--------------------//

        #region Layout
        private void LayoutHelper()
        {
            Engine engine = DialogRender.DialogManager.Engine;
            if (engine == null || engine.Disposed) return;
            Size renderSize = engine.RenderSize;

            if (DialogModel.Fullscreen && DialogModel.Size != Size.Empty)
            {
                DialogRender.Location = new Point();

                // Setup fullscreen layout
                DialogRender.SetSize(renderSize.Width, renderSize.Height);

                // Automatically scale the dialog to fit the current monitor resolution
                float width = (float)renderSize.Width / DialogModel.Size.Width;
                float height = (float)renderSize.Height / DialogModel.Size.Height;
                DialogModel.AutoScale = width < height ? width : height;
            }
            else
            {
                DialogRender.Location = new Point(_location.X, _location.Y);

                if (DialogModel.Size == Size.Empty)
                    DialogRender.SetSize(renderSize.Width, renderSize.Height);
                else
                    DialogRender.SetSize(DialogModel.Size.Width, DialogModel.Size.Height);

                DialogModel.AutoScale = 1;
            }
        }
        #endregion

        #region Control
        /// <summary>
        /// Show the dialog
        /// </summary>
        public void Show()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            _dialogManager.AddNormal(this);
            LuaExecute(DialogModel.OnShow, "Dialog_Show");
            Update();
        }

        /// <summary>
        /// Show the dialog with exclusive mouse handling
        /// </summary>
        public void ShowModal()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            _dialogManager.AddModal(this);
            LuaExecute(DialogModel.OnShow, "Dialog_Show");
            Update();
        }

        /// <summary>
        /// Update the dialog output
        /// </summary>
        public void Update()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            LuaExecute(DialogModel.OnUpdate, "Dialog_Update");
        }

        /// <summary>
        /// Closes the dialog
        /// </summary>
        public void Close()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            #endregion

            _dialogManager.Remove(this);
        }
        #endregion

        //--------------------//

        #region Lua execute
        private void LuaExecute(string script, string source)
        {
            #region Sanity checks
            if (Lua == null || string.IsNullOrEmpty(script)) return;
            #endregion

            try
            {
                Lua.DoString(script);
            }
            catch (LuaScriptException ex)
            {
                // Prepend additional source information and then rethrow the exception
                ex.Source = Name + ":" + source + ":" + ex.Source;
                throw;
            }
        }
        #endregion

        #region Lua import
        /// <summary>
        /// Imports an external Lua script into the GUI's Lua instance
        /// </summary>
        /// <param name="filename">The Lua file to import</param>
        public void ImportLua(string filename)
        {
            using (var stream = ContentManager.GetFileStream("GUI", filename))
            using (var streamReader = new StreamReader(stream))
                Lua.DoString(streamReader.ReadToEnd());
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the first <see cref="Model.Control"/> in this <see cref="DialogRenderer"/> with the specified <paramref name="name"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">An element with the specified key does not exist in the dictionary.</exception>
        // Note: Keep this in addition to the DialogView index accessor for Lua access
        public Model.Control GetControl(string name)
        {
            return DialogModel[name];
        }

        /// <summary>
        /// Changes the dialog's on-screen position.
        /// </summary>
        // Note: Keep this in addition to the DialogModel property for Lua access
        public void SetLocation(int x, int y)
        {
            DialogRender.Location = new Point(x, y);
        }
        #endregion

        #region MsgBox
        [CLSCompliant(false)]
        public void MsgBox(string text)
        {
            MsgBox(text, null);
        }

        public void MsgBox(string text, Action<Render.MsgBoxResult> callback)
        {
            MsgBox(text, Render.MsgBoxType.OK, callback);
        }

        public void MsgBox(string text, Render.MsgBoxType type, Action<Render.MsgBoxResult> callback)
        {
            DialogModel.MsgBox(text, type, callback);
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Removes the <see cref="Engine"/> hooks and queues the <see cref="LuaInterface.Lua"/> interpreter for disposal
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~DialogRenderer()
        {
            Dispose(false);
        }

        /// <summary>
        /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if called manually and not by the garbage collector.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Lua is queued for delayed disposal")]
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed) return; // Don't try to dispose more than once

            if (DialogRender != null)
            {
                // Ensure clean shutdown
                DialogRender.RemoveAllControls();

                if (DialogRender.DialogManager.Engine != null)
                    DialogRender.DialogManager.Engine.DeviceReset -= LayoutHelper;
            }
            if (Lua != null) _dialogManager.QueueLuaDispose(Lua);

            Disposed = true;
        }
        #endregion
    }
}
