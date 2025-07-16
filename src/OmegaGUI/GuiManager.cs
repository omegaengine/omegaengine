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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Native;
using OmegaEngine;

namespace OmegaGUI
{
    /// <summary>
    /// Maintains lists of all <see cref="DialogRenderer"/>s
    /// </summary>
    public sealed class GuiManager : IDisposable
    {
        #region Variables
        private readonly Engine _engine;

        private readonly List<DialogRenderer> _normalDialogs = [];
        private readonly List<DialogRenderer> _modalDialogs = [];
        private readonly List<Lua> _pendingLuaDisposes = [];

        private Stopwatch _timer;
        private float _timeSinceLastUpdate;

        /// <summary>
        /// Manages shared resources of all <see cref="OmegaGUI.Render.Dialog"/>s.
        /// </summary>
        internal Render.DialogManager DialogManager { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Sets up the GUI system for rendering with the engine.
        /// </summary>
        /// <param name="engine">The <see cref="Engine"/> to render in.</param>
        public GuiManager(Engine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            DialogManager = new(engine);
            engine.ExtraRender += Render;
        }
        #endregion

        //--------------------//

        #region Open
        /// <summary>
        /// Adds a normal <see cref="DialogRenderer"/> to the GUI system that shares user-input with all other <see cref="DialogRenderer"/>s.
        /// </summary>
        /// <param name="dialog">The <see cref="DialogRenderer"/> to add.</param>
        internal void AddNormal(DialogRenderer dialog)
        {
            _normalDialogs.Add(dialog);
        }

        /// <summary>
        /// Adds a modal <see cref="DialogRenderer"/> to the GUI system that locks all other <see cref="DialogRenderer"/>s while it is active.
        /// </summary>
        /// <param name="dialog">The <see cref="DialogRenderer"/> to add.</param>
        internal void AddModal(DialogRenderer dialog)
        {
            _modalDialogs.Add(dialog);
        }
        #endregion

        #region Close
        /// <summary>
        /// Removes/closes an open <see cref="DialogRenderer"/>.
        /// </summary>
        /// <param name="dialog">The <see cref="DialogRenderer"/> to close.</param>
        internal void Remove(DialogRenderer dialog)
        {
            dialog.DialogRender.Refresh();
            dialog.Dispose();
            _normalDialogs.Remove(dialog);
            _modalDialogs.Remove(dialog);
        }

        /// <summary>
        /// Closes all open <see cref="DialogRenderer"/>s.
        /// </summary>
        [LuaGlobal(Description = "Closes all open dialogs.")]
        public void CloseAll()
        {
            _normalDialogs.ForEach(dialog => dialog.Dispose());
            _normalDialogs.Clear();

            _modalDialogs.ForEach(dialog => dialog.Dispose());
            _modalDialogs.Clear();
        }

        /// <summary>
        /// Closes all open <see cref="DialogRenderer"/>s and resets the GUI system (i.e. clears all its caches).
        /// </summary>
        [LuaGlobal(Description = "Closes all open dialogs and resets the GUI system (i.e. clears all its caches).")]
        public void Reset()
        {
            CloseAll();
            DialogManager.Dispose();
            DialogManager = new(_engine);
        }
        #endregion

        #region Update
        /// <summary>
        /// Invokes <see cref="DialogRenderer.Update"/> on all open <see cref="DialogRenderer"/>s.
        /// </summary>
        [LuaGlobal(Description = "Invokes the OnUpdate event on all open dialogs.")]
        public void Update()
        {
            _normalDialogs.ForEach(dialog => dialog.Update());
            _modalDialogs.ForEach(dialog => dialog.Update());

            _timeSinceLastUpdate = 0;
        }
        #endregion

        //--------------------//

        #region Render
        /// <summary>
        /// Called by <see cref="Engine.ExtraRender"/> to render the GUI
        /// </summary>
        private void Render()
        {
            #region Timer
            if (_timer == null) _timer = Stopwatch.StartNew();
            var elapsedTime = (float)_timer.Elapsed.TotalSeconds;
            _timer.Reset();
            _timer.Start();

            // Run update scripts once per second
            _timeSinceLastUpdate += elapsedTime;
            if (_timeSinceLastUpdate >= 1) Update();
            #endregion

            using (new ProfilerEvent("Render GUI"))
            {
                foreach (var dialog in _normalDialogs.Where(dialog => dialog.DialogModel.Visible))
                    dialog.DialogRender.OnRender(dialog.DialogModel.Animate ? elapsedTime : 1);
                foreach (var dialog in _modalDialogs.Where(dialog => dialog.DialogModel.Visible))
                    dialog.DialogRender.OnRender(dialog.DialogModel.Animate ? elapsedTime : 1);
                if (DialogManager.MessageBox.Visible)
                    DialogManager.MessageBox.OnRender(elapsedTime);
            }

            var readyToRemove = _pendingLuaDisposes.FindAll(lua => !lua.IsExecuting);
            foreach (var lua in readyToRemove)
            {
                lua.Dispose();
                _pendingLuaDisposes.Remove(lua);
            }
        }
        #endregion

        #region Message handling
        /// <summary>
        /// Handles Windows Messages for the GUI
        /// </summary>
        /// <param name="m">The message to handle</param>
        /// <returns><c>true</c> if the message was handled and no further processing is necessary</returns>
        public bool OnMsgProc(Message m)
        {
            // Exclusive input handling for MessageBox
            if (DialogManager.MessageBox != null && DialogManager.MessageBox.Visible)
            {
                return DialogManager.MessageBox.
                    MessageProc(m.HWnd, (WindowMessage)m.Msg, m.WParam, m.LParam);
            }

            // Exclusive input handling for last modal dialog
            if (_modalDialogs.Count > 0)
            {
                return _modalDialogs[_modalDialogs.Count - 1].DialogRender.
                    MessageProc(m.HWnd, (WindowMessage)m.Msg, m.WParam, m.LParam);
            }

            // Copy dialog list to an array first to prevent exceptions if dialogs are removed
            var currentGuis = new DialogRenderer[_normalDialogs.Count];
            _normalDialogs.CopyTo(currentGuis, 0);

            // Pass input to dialogs for handling from last to first
            for (int i = currentGuis.Length - 1; i >= 0; i--)
            {
                if (currentGuis[i].DialogRender.MessageProc(m.HWnd, (WindowMessage)m.Msg, m.WParam, m.LParam))
                {
                    // Input has been handled, no further processing
                    return true;
                }
            }

            // Input was not handled
            return false;
        }
        #endregion

        //--------------------//

        #region Queues
        /// <summary>
        /// Queues a Lua controller for disposal as soon as it stops executing its current script
        /// </summary>
        /// <param name="lua">The object to be disposed</param>
        internal void QueueLuaDispose(Lua lua)
        {
            _pendingLuaDisposes.Add(lua);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Deactivates the <see cref="OmegaGUI"/> system and unhooks it from the <see cref="Engine"/>
        /// </summary>
        public void Dispose()
        {
            // Unhook engine events
            _engine.ExtraRender -= Render;

            if (DialogManager != null)
            {
                CloseAll();
                DialogManager.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        ~GuiManager()
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
