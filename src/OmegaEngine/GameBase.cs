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
using System.IO;
using System.Threading;
using System.Windows.Forms;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Native;
using OmegaEngine.Input;
using OmegaEngine.Values;
using SlimDX;

namespace OmegaEngine;

/// <summary>
/// Automatically provides an <see cref="Engine"/> instance with a fullscreen-capable window, render loop, input handling, etc.
/// </summary>
/// <remarks>
///   <para>By using this class, you don't need to create a window, set up <see cref="InputProvider"/>s, provide access to <see cref="DebugConsole"/>, etc. yourself.</para>
///   <para>You should override at least <see cref="Initialize"/> and <see cref="Render"/>. This corresponds to the template method pattern.</para>
/// </remarks>
public abstract partial class GameBase : IDisposable
{
    #region Variables
    /// <summary>Contains a reference to the <see cref="DebugConsole"/> while it is open</summary>
    private DebugConsole _debugConsole;

    /// <summary>
    /// The internal form used as the Engine render-targets
    /// </summary>
    protected readonly GameForm Form;
    #endregion

    #region Properties
    /// <summary>
    /// Has the game been shutdown?
    /// </summary>
    [Browsable(false)]
    public bool Disposed { get; private set; }

    /// <summary>
    /// The <see cref="Engine"/> instance used by the game.
    /// Warning! Only set after <see cref="Control.Show"/> has been called
    /// </summary>
    [LuaHide]
    public Engine Engine { get; private set; }

    /// <summary>
    /// Is the game in fullscreen mode?
    /// </summary>
    public bool Fullscreen { get; private set; }

    private bool _loading;

    /// <summary>
    /// Indicates the game is currently loading something and the user must wait.
    /// </summary>
    /// <remarks>On Windows 7 and newer this will cause a neat taskbar animation.</remarks>
    public bool Loading
    {
        get => _loading;
        set =>
            // Show taskbar animation on Windows 7 or newer
            value.To(ref _loading, () =>
                WindowsTaskbar.SetProgressState(Form.Handle, value
                    ? WindowsTaskbar.ProgressBarState.Indeterminate
                    : WindowsTaskbar.ProgressBarState.NoProgress));
    }

    /// <summary>
    /// A default <see cref="Input.KeyboardInputProvider"/> hooked up to the <see cref="Form"/>.
    /// </summary>
    public KeyboardInputProvider KeyboardInputProvider { get; }

    /// <summary>
    /// A default <see cref="Input.MouseInputProvider"/> hooked up to the <see cref="Form"/>.
    /// </summary>
    public MouseInputProvider MouseInputProvider { get; }

    /// <summary>
    /// A default <see cref="Input.TouchInputProvider"/> hooked up to the <see cref="Form"/>.
    /// </summary>
    public TouchInputProvider TouchInputProvider { get; }
    #endregion

    #region Constructor
    /// <summary>
    /// Sets up the <see cref="Form"/>.
    /// Call <see cref="ToWindowed"/> or <see cref="ToFullscreen"/> and <see cref="Run"/> afterwards.
    /// </summary>
    /// <param name="name">The name of the application for the title bar</param>
    /// <param name="icon">The icon of the application for the title bar</param>
    /// <param name="background">A background image for the window while loading</param>
    /// <param name="stretch">Stretch <paramref name="background"/> to fit the screen? (<c>false</c> will center it instead)</param>
    protected GameBase(string name, Icon? icon = null, Image? background = null, bool stretch = false)
    {
        // Setup render-target form
        Form = new()
        {
            Text = name, Icon = icon,
            BackgroundImage = background, BackgroundImageLayout = stretch ? ImageLayout.Stretch : ImageLayout.Center, BackColor = Color.Black,
            MinimumSize = new(800, 600), Size = new(800, 600), MaximizeBox = false
        };

        // Setup event hooks
        Form.Shown += Form_Shown;
        Form.KeyDown += delegate(object _, KeyEventArgs e)
        {
            // Ctrl + Shift + Alt + D = Debug console
            if (e.Control && e.Alt && e.Shift && e.KeyCode == Keys.D)
                Debug();
        };

        // Start tracking input
        KeyboardInputProvider = new(Form);
        MouseInputProvider = new(Form);
        TouchInputProvider = new(Form);
    }
    #endregion

    //--------------------//

    #region Start/Stop/Crash
    /// <summary>
    /// Shows the window and runs the render loop until <see cref="Exit"/> is called.
    /// </summary>
    [LuaHide]
    public virtual void Run()
    {
        #region Sanity checks
        if (Disposed) throw new ObjectDisposedException(ToString());
        #endregion

        // Start the window message loop
        Application.Run(Form);
    }

    /// <summary>
    /// Stops the render loop and closes the window
    /// </summary>
    public void Exit()
    {
        Form.Close();
    }

    /// <summary>
    /// Crashes the game for testing purposes
    /// </summary>
    [LuaGlobal]
    public static void Crash()
    {
        throw new InvalidOperationException("The Crash function was called");
    }
    #endregion

    #region Startup
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
    #endregion

    #region Mode
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
    #endregion

    #region Input handling
    /// <summary>
    /// Calls <see cref="InputProvider.AddReceiver"/> for all default <see cref="InputProvider"/>s.
    /// </summary>
    /// <param name="receiver">The object to receive the commands.</param>
    public void AddInputReceiver(IInputReceiver receiver)
    {
        KeyboardInputProvider.AddReceiver(receiver);
        MouseInputProvider.AddReceiver(receiver);
        TouchInputProvider.AddReceiver(receiver);
    }

    /// <summary>
    /// Calls <see cref="InputProvider.RemoveReceiver"/> for all default <see cref="InputProvider"/>s.
    /// </summary>
    /// <param name="receiver">The object to no longer receive the commands.</param>
    public void RemoveInputReceiver(IInputReceiver receiver)
    {
        KeyboardInputProvider.RemoveReceiver(receiver);
        MouseInputProvider.RemoveReceiver(receiver);
        TouchInputProvider.RemoveReceiver(receiver);
    }
    #endregion

    //--------------------//

    #region Debug
    /// <summary>
    /// Called when the debug form is to be displayed
    /// </summary>
    [LuaHide]
    public virtual void Debug()
    {
        // Exit fullscreen mode to allow a normal window to be displayed
        if (Fullscreen) ToWindowed(Form.ClientSize);

        // Only create a new form if there isn't already one open
        if (_debugConsole == null)
        {
            _debugConsole = new(NewLua());
            _debugConsole.Text = Application.ProductName + @" " + _debugConsole.Text;

            // Remove the reference as soon the form is closed
            _debugConsole.FormClosed += delegate
            {
                _debugConsole.Dispose();
                _debugConsole = null;
            };
        }

        _debugConsole.Show();
    }
    #endregion

    #region Lua references
    /// <summary>
    /// Creates a new <see cref="Lua"/> instance with commonly used objects preloaded.
    /// </summary>
    [LuaHide]
    public virtual Lua NewLua()
    {
        var lua = new Lua();
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(Log));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(StringUtils));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(MathUtils));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(VectorMath));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(RandomUtils));
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(ColorUtils));

        lua["temp"] = Path.GetTempPath() + Path.DirectorySeparatorChar;
        lua["Pi"] = Math.PI;

        lua["Game"] = this;
        lua["Engine"] = Engine;

        // Import .NET constructors
        ImportConstructor(lua, typeof(Point));
        ImportConstructor(lua, typeof(Size));
        ImportConstructor(lua, typeof(Rectangle));
        ImportConstructor(lua, typeof(Color3));
        ImportConstructor(lua, typeof(Color4));
        ImportConstructor(lua, typeof(Vector2));
        ImportConstructor(lua, typeof(Vector3));
        ImportConstructor(lua, typeof(Vector4));
        ImportConstructor(lua, typeof(Quaternion));
        ImportConstructor(lua, typeof(Vector2Ray));
        ImportConstructor(lua, typeof(DoubleVector3));
        ImportConstructor(lua, typeof(XColor));

        LuaRegistrationHelper.TaggedInstanceMethods(lua, this);
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(GameBase));

        return lua;
    }

    private static void ImportConstructor(Lua lua, Type type)
    {
        lua.DoString("luanet.load_assembly(\"" + type.Assembly.GetName() + "\")");
        lua.DoString(type.Name + " = luanet.import_type(\"" + type.FullName + "\")");
    }
    #endregion

    //--------------------//

    #region Dispose
    /// <summary>
    /// Dispose the <see cref="Engine"/>, the internal windows form, etc.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Dispose(true);
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    /// <inheritdoc/>
    ~GameBase()
    {
        Dispose(false);
    }

    /// <summary>
    /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
    /// </summary>
    /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
    protected virtual void Dispose(bool disposing)
    {
        // Unlock the mouse cursor
        Cursor.Clip = new();

        if (disposing)
        { // This block will only be executed on manual disposal, not by Garbage Collection
            Engine?.Dispose();
            Form?.Dispose();
            _debugConsole?.Dispose();

            // Stop tracking input
            KeyboardInputProvider?.Dispose();
            MouseInputProvider?.Dispose();
            TouchInputProvider?.Dispose();

            // Assume this is the only usage of SlimDX in the entire process (true for games, not for editors)
            if (ObjectTable.Objects.Count > 0)
            {
                string leaks = ObjectTable.ReportLeaks();
                Log.Error(leaks);
#if DEBUG
                throw new InvalidOperationException(leaks);
#endif
            }
        }
        else
        { // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
            throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
        }
    }
    #endregion
}
