/*
 * Copyright 2006-2014 Bastian Eicher
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
using LuaInterface;
using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Storage;
using OmegaGUI.Model;

namespace OmegaGUI
{
    /// <summary>
    /// Displays a <see cref="Dialog"/> using <see cref="Render.Dialog"/>.
    /// </summary>
    public sealed class DialogRenderer : IDisposable
    {
        #region Variables
        private readonly GuiManager _manager;
        private Point _location;
        private readonly Lua _lua;
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
        /// The dialog to be displayed
        /// </summary>
        public Dialog DialogModel { get; }

        /// <summary>
        /// The rendering system used to display <see cref="DialogModel"/>
        /// </summary>
        public Render.Dialog DialogRender { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new dialog renderer from an XML file
        /// </summary>
        /// <param name="manager">The <see cref="GuiManager"/> used to interface with the <see cref="Engine"/></param>
        /// <param name="filename">The filename of the XML file to load</param>
        /// <param name="location">The location of the dialog on the screen</param>
        /// <param name="lua">The scripting engine to execute event handlers.</param>
        public DialogRenderer(GuiManager manager, string filename, Point location = new(), Lua lua = null)
            : this(manager, Dialog.FromContent(filename), location: location, lua: lua)
        {
            Log.Info("Loading GUI dialog: " + filename);
            Name = filename;
        }

        /// <summary>
        /// Creates a new dialog renderer for a <see cref="Model.Dialog"/>
        /// </summary>
        /// <param name="manager">The <see cref="GuiManager"/> used to interface with the <see cref="Engine"/></param>
        /// <param name="dialog">The dialog to be displayed</param>
        /// <param name="location">The location of the dialog on the screen</param>
        /// <param name="lua">The scripting engine to execute event handlers.</param>
        public DialogRenderer(GuiManager manager, Dialog dialog, Point location = new(), Lua lua = null)
        {
            _manager = manager;
            DialogModel = dialog;
            DialogRender = dialog.GenerateRender(_manager.DialogManager);
            _location = location;
            _lua = lua;

            LayoutHelper();
            _manager.DialogManager.Engine.DeviceReset += LayoutHelper;

            if (lua != null)
            {
                #region Register Lua variables and functions
                // Register all controls as direct variables when possible
                foreach (var control in dialog.Controls)
                {
                    if (!string.IsNullOrEmpty(control.Name))
                    {
                        try
                        {
                            _lua[control.Name] = control;
                        }
                        catch (LuaException)
                        {}
                    }
                }

                _lua["Me"] = this;

                LuaRegistrationHelper.Enumeration<Render.MsgBoxType>(_lua);
                LuaRegistrationHelper.Enumeration<Render.MsgBoxResult>(_lua);
                #endregion

                dialog.ScriptFired += LuaExecute;
            }
        }
        #endregion

        //--------------------//

        #region Layout
        private void LayoutHelper()
        {
            Engine engine = DialogRender.DialogManager.Engine;
            if (engine == null || engine.IsDisposed) return;
            Size renderSize = engine.RenderSize;

            if (DialogModel.Fullscreen && DialogModel.Size != Size.Empty)
            {
                DialogRender.Location = new();

                // Setup fullscreen layout
                DialogRender.SetSize(renderSize.Width, renderSize.Height);

                // Automatically scale the dialog to fit the current monitor resolution
                float width = (float)renderSize.Width / DialogModel.Size.Width;
                float height = (float)renderSize.Height / DialogModel.Size.Height;
                DialogModel.AutoScale = width < height ? width : height;
            }
            else
            {
                DialogRender.Location = new(_location.X, _location.Y);

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

            _manager.AddNormal(this);
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

            _manager.AddModal(this);
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

            _manager.Remove(this);
        }
        #endregion

        //--------------------//

        #region Lua execute
        private void LuaExecute(string script, string source)
        {
            #region Sanity checks
            if (_lua == null || string.IsNullOrEmpty(script)) return;
            #endregion

            try
            {
                _lua.DoString(script);
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
                _lua.DoString(streamReader.ReadToEnd());
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Gets the first <see cref="Model.Control"/> in this <see cref="DialogRenderer"/> with the specified <paramref name="name"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">An element with the specified key does not exist in the dictionary.</exception>
        // Note: Keep this in addition to the DialogRender index accessor for Lua access
        public Control GetControl(string name)
        {
            return DialogModel[name];
        }

        /// <summary>
        /// Changes the dialog's on-screen position.
        /// </summary>
        // Note: Keep this in addition to the DialogModel property for Lua access
        public void SetLocation(int x, int y)
        {
            DialogRender.Location = new(x, y);
        }
        #endregion

        #region MsgBox
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
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Lua is queued for delayed disposal")]
        public void Dispose()
        {
            if (Disposed) return;

            if (DialogRender != null)
            {
                // Ensure clean shutdown
                DialogRender.RemoveAllControls();

                if (DialogRender.DialogManager.Engine != null)
                    DialogRender.DialogManager.Engine.DeviceReset -= LayoutHelper;
            }
            if (_lua != null) _manager.QueueLuaDispose(_lua);

            GC.SuppressFinalize(this);
            Disposed = true;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        ~DialogRenderer()
        {
            // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
            throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
        }
        #endregion
    }
}
