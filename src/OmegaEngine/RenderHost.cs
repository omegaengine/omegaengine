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
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using OmegaEngine.Foundation;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Input;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine;

/// <summary>
/// Automatically provides an <see cref="Engine"/> instance with a fullscreen-capable window, render loop, input handling, etc.
/// </summary>
/// <remarks>
///   <para>By using this class, you don't need to create a window, set up <see cref="InputProvider"/>s, provide access to <see cref="DebugConsole"/>, etc. yourself.</para>
///   <para>You should override at least <see cref="Initialize"/> and <see cref="Render"/>. This corresponds to the template method pattern.</para>
/// </remarks>
public partial class RenderHost : IRenderHost, IDisposable
{
    /// <summary>Contains a reference to the <see cref="DebugConsole"/> while it is open</summary>
    private DebugConsole? _debugConsole;

    /// <summary>
    /// The internal form used as the Engine render-targets
    /// </summary>
    protected readonly RenderForm Form;

    /// <inheritdoc/>
    public KeyboardInputProvider KeyboardInputProvider { get; }

    /// <inheritdoc/>
    public MouseInputProvider MouseInputProvider { get; }

    /// <inheritdoc/>
    public TouchInputProvider TouchInputProvider { get; }

    /// <summary>
    /// Has the app been shutdown?
    /// </summary>
    [Browsable(false)]
    public bool Disposed { get; private set; }

    private Engine? _engine;

    /// <summary>
    /// The <see cref="Engine"/> instance.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="Run"/> has not been called yet.</exception>
    [LuaHide]
    public Engine Engine => _engine ?? throw new InvalidOperationException($"{nameof(Run)} has not been called yet.");

    /// <summary>
    /// Is the app in fullscreen mode?
    /// </summary>
    public bool Fullscreen { get; private set; }

    private bool _loading;

    /// <summary>
    /// Indicates the app is currently loading something and the user must wait.
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
    /// Sets up the <see cref="Form"/>.
    /// Call <see cref="ToWindowed"/> or <see cref="ToFullscreen"/> and <see cref="Run"/> afterwards.
    /// </summary>
    /// <param name="name">The name of the application for the title bar</param>
    /// <param name="icon">The icon of the application for the title bar</param>
    /// <param name="background">A background image for the window while loading</param>
    /// <param name="stretch">Stretch <paramref name="background"/> to fit the screen? (<c>false</c> will center it instead)</param>
    protected RenderHost(string name, Icon? icon = null, Image? background = null, bool stretch = false)
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

    /// <summary>
    /// To be called after the window is ready and the <see cref="Engine"/> needs to be set up
    /// </summary>
    /// <returns><c>true</c> if the initialization worked, <c>false</c> if it failed and the app must be closed</returns>
    protected virtual bool Initialize()
    {
        using (new TimedLogEvent("Initialize engine"))
        {
            // Initialize engine
            try
            {
                _engine = new(Form, BuildEngineConfig(fullscreen: false)); // No fullscreen while loading
                ApplyGraphicsSettings();
            }
            #region Error handling
            catch (NotSupportedException ex)
            {
                Log.Error($"{ex}\n{ex.InnerException}");
                Msg.Inform(Form, Resources.BadGraphics, MsgSeverity.Error);
                Exit();
                return false;
            }
            catch (Direct3D9Exception ex) when (ex is Direct3D9NotFoundException or Direct3DX9NotFoundException)
            {
                Msg.Inform(Form, Resources.DirectXMissing, MsgSeverity.Error);
                Exit();
                return false;
            }
            catch (SlimDXException ex) when (ex is Direct3D9Exception or SlimDX.DirectSound.DirectSoundException)
            {
                Msg.Inform(Form, ex.Message, MsgSeverity.Error);
                Exit();
                return false;
            }
            #endregion

            if (Engine.Capabilities.MaxShaderModel < TerrainShader.MinShaderModel)
            {
                Log.Error($"No support for Pixel Shader {TerrainShader.MinShaderModel}");
                Msg.Inform(Form, $"{Resources.BadGraphics}\n{string.Format(Resources.MinimumShaderModel, TerrainShader.MinShaderModel)}", MsgSeverity.Warn);
                Exit();
                return false;
            }
        }

        return true;
    }

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
    ~RenderHost()
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
            _engine?.Dispose();
            Form.Dispose();
            _debugConsole?.Dispose();

            // Stop tracking input
            KeyboardInputProvider.Dispose();
            MouseInputProvider.Dispose();
            TouchInputProvider.Dispose();

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
            Log.Error($"Forgot to call Dispose on {this}");
#if DEBUG
            throw new InvalidOperationException($"Forgot to call Dispose on {this}");
#endif
        }
    }

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
    /// Called when the next frame needs to be rendered.
    /// </summary>
    /// <param name="elapsedTime">The number of seconds that have passed since this method was last called.</param>
    protected virtual void Render(double elapsedTime)
    {
        // Use the form's WindowState to determine whether fullscreen mode was intended
        if (Form.WindowState == FormWindowState.Maximized)
        {
            // Switch the form's WindowState to normal, otherwise there will be ugly render bugs
            Form.WindowState = FormWindowState.Normal;

            // Now switch the Direct3D device to real fullscreen mode
            Log.Info("Switch to real fullscreen mode");
            ResetEngine();
        }

        Engine.Render(elapsedTime);

        // Start new song after last one has stopped
        Engine.Music.Update();
    }

    /// <summary>
    /// Resets the <see cref="Engine"/>
    /// </summary>
    [LuaGlobal(Description = "Resets the graphics engine")]
    protected virtual void ResetEngine()
    {
        Engine.Config = BuildEngineConfig(Fullscreen);
        Engine.Render();
    }

    /// <summary>
    /// Called to generate an <see cref="EngineConfig"/> based on external settings
    /// </summary>
    /// <param name="fullscreen">Shall the configuration be generated for fullscreen mode?</param>
    protected virtual EngineConfig BuildEngineConfig(bool fullscreen) => new()
    {
        Fullscreen = fullscreen,
        TargetSize = fullscreen ? Screen.PrimaryScreen.Bounds.Size : Form.ClientSize
    };

    /// <summary>
    /// Called when graphics settings from an external source need to be applied to the <see cref="Engine"/>
    /// </summary>
    protected virtual void ApplyGraphicsSettings()
    {}

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
        lua["Engine"] = Engine;
        lua["Host"] = this;

        // Import .NET constructors
        ImportConstructor(typeof(Point));
        ImportConstructor(typeof(Size));
        ImportConstructor(typeof(Rectangle));
        ImportConstructor(typeof(Color3));
        ImportConstructor(typeof(Color4));
        ImportConstructor(typeof(Vector2));
        ImportConstructor(typeof(Vector3));
        ImportConstructor(typeof(Vector4));
        ImportConstructor(typeof(Quaternion));
        ImportConstructor(typeof(Vector2Ray));
        ImportConstructor(typeof(DoubleVector3));
        ImportConstructor(typeof(XColor));

        LuaRegistrationHelper.TaggedInstanceMethods(lua, this);
        LuaRegistrationHelper.TaggedStaticMethods(lua, typeof(RenderHost));

        return lua;

        void ImportConstructor(Type type)
        {
            lua.DoString($"luanet.load_assembly(\"{type.Assembly.GetName()}\")");
            lua.DoString($"{type.Name} = luanet.import_type(\"{type.FullName}\")");
        }
    }

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
            _debugConsole.Text = $@"{Application.ProductName} {_debugConsole.Text}";

            // Remove the reference as soon the form is closed
            _debugConsole.FormClosed += delegate
            {
                _debugConsole.Dispose();
                _debugConsole = null;
            };
        }

        _debugConsole?.Show();
    }

    /// <summary>
    /// Crashes the app for testing purposes
    /// </summary>
    [LuaGlobal]
    public static void Crash()
        => throw new InvalidOperationException("The Crash function was called");
}
