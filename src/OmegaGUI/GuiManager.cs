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
using System.Linq;
using System.Windows.Forms;
using NLua;
using NanoByte.Common;
using NanoByte.Common.Native;
using OmegaEngine;

namespace OmegaGUI;

/// <summary>
/// Maintains lists of all <see cref="DialogPresenter"/>s
/// </summary>
public sealed class GuiManager : IDisposable
{
    #region Variables
    private readonly Engine _engine;

    private readonly List<DialogPresenter> _normalDialogs = [];
    private readonly List<DialogPresenter> _modalDialogs = [];
    private readonly List<Lua> _pendingLuaDisposes = [];

    private Stopwatch? _timer;
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
    /// Adds a normal <see cref="DialogPresenter"/> to the GUI system that shares user-input with all other <see cref="DialogPresenter"/>s.
    /// </summary>
    /// <param name="dialog">The <see cref="DialogPresenter"/> to add.</param>
    internal void AddNormal(DialogPresenter dialog)
    {
        _normalDialogs.Add(dialog);
    }

    /// <summary>
    /// Adds a modal <see cref="DialogPresenter"/> to the GUI system that locks all other <see cref="DialogPresenter"/>s while it is active.
    /// </summary>
    /// <param name="dialog">The <see cref="DialogPresenter"/> to add.</param>
    internal void AddModal(DialogPresenter dialog)
    {
        _modalDialogs.Add(dialog);
    }
    #endregion

    #region Close
    /// <summary>
    /// Removes/closes an open <see cref="DialogPresenter"/>.
    /// </summary>
    /// <param name="dialog">The <see cref="DialogPresenter"/> to close.</param>
    internal void Remove(DialogPresenter dialog)
    {
        dialog.Render.Refresh();
        dialog.Dispose();
        _normalDialogs.Remove(dialog);
        _modalDialogs.Remove(dialog);
    }

    /// <summary>
    /// Closes all open <see cref="DialogPresenter"/>s.
    /// </summary>
    [LuaMember]
    public void CloseAll()
    {
        _normalDialogs.ForEach(dialog => dialog.Dispose());
        _normalDialogs.Clear();

        _modalDialogs.ForEach(dialog => dialog.Dispose());
        _modalDialogs.Clear();
    }

    /// <summary>
    /// Closes all open <see cref="DialogPresenter"/>s and resets the GUI system (i.e. clears all its caches).
    /// </summary>
    [LuaMember]
    public void Reset()
    {
        CloseAll();
        DialogManager.Dispose();
        DialogManager = new(_engine);
    }
    #endregion

    #region Update
    /// <summary>
    /// Invokes <see cref="DialogPresenter.Update"/> on all open <see cref="DialogPresenter"/>s.
    /// </summary>
    [LuaMember]
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
            foreach (var dialog in _normalDialogs.Where(dialog => dialog.Model.Visible))
                dialog.Render.OnRender(dialog.Model.Animate ? elapsedTime : 1);
            foreach (var dialog in _modalDialogs.Where(dialog => dialog.Model.Visible))
                dialog.Render.OnRender(dialog.Model.Animate ? elapsedTime : 1);
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
    /// <summary>Is a left-click gesture that started on a GUI control currently in progress?</summary>
    private bool _clickInProgress;

    /// <summary>
    /// Handles Windows Messages for the GUI
    /// </summary>
    /// <param name="m">The message to handle</param>
    /// <returns><c>true</c> if the message was handled and no further processing is necessary</returns>
    public bool OnMsgProc(Message m)
    {
        var msg = (WindowMessage)m.Msg;
        bool handled = Dispatch(m, msg);

        // Track the entire click gesture: once a click has started on a control, the rest of the
        // gesture must not leak through to the application, even if no dialog handles the
        // individual messages. This is tracked here rather than in the dialogs themselves because
        // event handlers may open or close dialogs in the middle of a gesture.
        switch (msg)
        {
            case WindowMessage.LeftButtonDown:
            case WindowMessage.LeftButtonDoubleClick:
                _clickInProgress = handled;
                break;

            case WindowMessage.LeftButtonUp:
                if (_clickInProgress)
                {
                    // The click started on a control, so its mouse-up belongs to the GUI as well
                    _clickInProgress = false;
                    return true;
                }
                break;

            case WindowMessage.MouseMove:
                // The button-up that should have ended the click may never have reached us
                if (_clickInProgress && !Control.MouseButtons.HasFlag(MouseButtons.Left))
                    _clickInProgress = false;

                // Drags that started on a control are not passed on to the application
                if (_clickInProgress) return true;
                break;
        }

        return handled;
    }

    /// <summary>
    /// Passes a Windows Message to the <see cref="DialogPresenter"/>s for handling
    /// </summary>
    private bool Dispatch(Message m, WindowMessage msg)
    {
        // Exclusive input handling for last modal dialog
        if (_modalDialogs.Count > 0)
            return _modalDialogs[^1].Render.MessageProc(m.HWnd, msg, m.WParam, m.LParam);

        // Copy dialog list to an array first to prevent exceptions if dialogs are removed
        var currentGuis = new DialogPresenter[_normalDialogs.Count];
        _normalDialogs.CopyTo(currentGuis, 0);

        // Pass input to dialogs for handling from last to first
        for (int i = currentGuis.Length - 1; i >= 0; i--)
        {
            if (currentGuis[i].Render.MessageProc(m.HWnd, msg, m.WParam, m.LParam))
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

        CloseAll();
        DialogManager.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    ~GuiManager()
    {
        // This block will only be executed on Garbage Collection, not by manual disposal
        Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
        throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
    }
    #endregion
}
