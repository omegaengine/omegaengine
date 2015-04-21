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
using System.Windows.Forms;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Controls;

namespace OmegaEngine
{
    /// <summary>
    /// Provides a generic debug console powered by the <see cref="Log"/> system and Lua scripting.
    /// </summary>
    public partial class DebugConsole : Form
    {
        #region Variables
        private string _lastCommand;
        private readonly Lua _lua;
        #endregion

        #region Constructor
        public DebugConsole(Lua lua)
        {
            InitializeComponent();

            // Prepare Lua interpreter
            _lua = lua;
            LuaRegistrationHelper.TaggedStaticMethods(_lua, typeof(InspectionForm));

            // Keep the text-box in sync with the Log while the window is open
            HandleCreated += delegate { Log.Handler += LogHandler; };
            HandleDestroyed += delegate { Log.Handler -= LogHandler; };
        }
        #endregion

        #region Startup
        private void DebugForm_Load(object sender, EventArgs e)
        {
            // Copy Lua globals list to auto-complete collection
            var autoComplete = new AutoCompleteStringCollection();
            foreach (string global in _lua.Globals)
                autoComplete.Add(global);

            // Set text-box to use auto-complete collection
            inputBox.AutoCompleteCustomSource = autoComplete;
        }

        private void DebugForm_Shown(object sender, EventArgs e)
        {
            UpdateLog();
            inputBox.Focus();
        }

        private void LogHandler(LogSeverity severity, string message)
        {
            UpdateLog();
        }
        #endregion

        //--------------------//

        #region Update
        /// <summary>
        /// Updates the on-screen representation of the <see cref="Log"/>.
        /// </summary>
        private void UpdateLog()
        {
            outputBox.Invoke(new Action(delegate
            { // Update the text-box display
                outputBox.Text = Log.Content.Trim();
                outputBox.Select(outputBox.Text.Length, 0);
                outputBox.ScrollToCaret();
                Application.DoEvents();
            }));
        }
        #endregion

        #region Run
        private void runButton_Click(object sender, EventArgs e)
        {
            string command = inputBox.Text.Trim();
            if (string.IsNullOrEmpty(command)) return;

            if (!inputBox.AutoCompleteCustomSource.Contains(command))
                inputBox.AutoCompleteCustomSource.Add(command);
            _lastCommand = command;
            Log.Debug("> " + command);

            try
            {
                // Execute the command and capture its result
                if (command.Contains("=")) _lua.DoString(command);
                else _lua.DoString("DebugResult = " + command);
                if (_lua["DebugResult"] != null)
                {
                    // Output the result as a string if possible
                    string result = _lua["DebugResult"].ToString();
                    if (!string.IsNullOrEmpty(result))
                        Log.Debug("==> " + result);
                    _lua["DebugResult"] = null;
                }
            }
            catch (LuaScriptException ex)
            {
                // Unwrap .NET exceptions inside Lua exceptions
                string message = ex.IsNetException ? ex.InnerException.Message : ex.Message;

                // Output exception message
                Log.Debug("==> " + message);
            }

            inputBox.Text = "";
        }
        #endregion

        #region Retreive last command
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up && string.IsNullOrEmpty(inputBox.Text))
                inputBox.Text = _lastCommand;
        }
        #endregion
    }
}
