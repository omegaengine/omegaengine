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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AlphaFramework.Editor;
using AlphaFramework.Editor.Properties;
using Common;
using Common.Controls;
using Common.Storage;
using Common.Storage.SlimDX;
using Common.Utils;
using FrameOfReference.World.Config;
using OmegaEngine;

namespace FrameOfReference.Editor
{
    internal static class Program
    {
        /// <summary>
        /// The arguments this application was launched with.
        /// </summary>
        public static Arguments Args { get; private set; }

        /// <summary>
        /// Shall the application start from the beginning again?
        /// </summary>
        public static bool Restart = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "GC.Collect is only called after the main form closes, when a lot of long-lived objects have turned into garbage")]
        private static void Main(string[] args)
        {
            WindowsUtils.SetCurrentProcessAppID(Application.CompanyName + "." + GeneralSettings.AppNameShort + ".AlphaEditor");
            ModInfo.FileExt = "." + GeneralSettings.AppNameShort + "Mod";

            Application.EnableVisualStyles();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            ErrorReportForm.SetupMonitoring(new Uri("http://omegaengine.de/error-report/?app=" + GeneralSettings.AppNameShort));

            // Allow setup to detect running instances
            AppMutex.Create(GeneralSettings.AppName + " Editor");

            Args = new Arguments(args);

            Settings.LoadCurrent();
            UpdateLocale();
            Settings.SaveCurrent();

            if (!DetermineContentDirs()) return;

            if (Settings.Current.Editor.ShowWelcomeMessage)
            {
                Restart = false; // Will be set to true again, if the user clicks "Continue"
                Application.Run(new WelcomeForm());
            }

            // The user might want to come back here multiple times, in order to switch the mod
            while (Restart)
            {
                Restart = false;

                // Ask user to select mod, cancel if an exception occurred
                Application.Run(new ModSelectorForm(Settings.Current.Editor.EditBase, Settings.Current.Editor.RecentMods));

                // Exit if the user didn't select anything
                if (ContentManager.ModDir == null && !ModInfo.MainGame) break;

                // Load the archives, run the main editor, cancel if an exception occurred, always unload the archives
                if (!LoadArchives()) break;
                try
                {
                    Application.Run(new MainForm());
                }
                finally
                {
                    ContentManager.CloseArchives();
                }

                // Prepare for next selection
                ModInfo.MainGame = false;
                ContentManager.ModDir = null;

                // After the MainForm has closed a lot of garbage will be left in Generation 2.
                // We should run Garbage Collection now, so we don't keep on wasting a large chunk of memory.
                GC.Collect();
            }
        }

        /// <summary>
        /// Updates the localization used by the application
        /// </summary>
        public static void UpdateLocale()
        {
            // Query Windows to get the default language
            if (string.IsNullOrEmpty(Settings.Current.General.Language))
                Settings.Current.General.Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            // Propagate selected language to other assemblies
            Resources.Culture = Engine.ResourceCulture = OmegaGUI.Model.Dialog.ResourceCulture = new CultureInfo(Settings.Current.General.Language);

            // Create specific culture for thread
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(Resources.Culture.Name);
        }

        /// <summary>
        /// Determines the data directories used by <see cref="ContentManager"/> and displays error messages if a directory could not be found.
        /// </summary>
        /// <returns><see langword="true"/> if all directories were located successfully; <see langword="false"/> if something went wrong.</returns>
        /// <remarks>The <see cref="ContentManager.ModDir"/> is not handled yet.</remarks>
        private static bool DetermineContentDirs()
        {
            try
            {
                if (!string.IsNullOrEmpty(Settings.Current.General.ContentDir))
                    ContentManager.BaseDir = new DirectoryInfo(Path.Combine(Locations.InstallBase, Settings.Current.General.ContentDir));
                else
                {
                    string baseDirPath = Environment.GetEnvironmentVariable("OMEGAENGINE_BASE_DIR");
                    if (!string.IsNullOrEmpty(baseDirPath)) ContentManager.BaseDir = new DirectoryInfo(baseDirPath);
                }
            }
                #region Error handling
            catch (ArgumentException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Calls <see cref="ContentManager.LoadArchives"/> and displays error messages if something went wrong.
        /// </summary>
        /// <returns><see langword="true"/> if all archives were loaded successfully; <see langword="false"/> if something went wrong.</returns>
        private static bool LoadArchives()
        {
            try
            {
                ContentManager.LoadArchives();
            }
                #region Error handling
            catch (IOException)
            {
                ContentManager.CloseArchives();
                Msg.Inform(null, Resources.FailedReadArchives, MsgSeverity.Error);
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                ContentManager.CloseArchives();
                Msg.Inform(null, Resources.FailedReadArchives, MsgSeverity.Error);
                return false;
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Launches the main game with the currently active mod (arguments automatically set).
        /// </summary>
        /// <param name="arguments">Additional arguments to be passed; may be <see langword="null"/>.</param>
        /// <exception cref="Win32Exception">Thrown if the game executable could not be launched.</exception>
        /// <exception cref="BadImageFormatException">Thrown if the game executable is damaged.</exception>
        internal static void LaunchGame(string arguments)
        {
            string param = "";

            // Make sure the current mod is loaded
            if (ContentManager.ModDir != null) param += " /mod " + "\"" + ContentManager.ModDir.FullName.TrimEnd(Path.DirectorySeparatorChar) + "\"";

            // Add additional arguments
            if (!string.IsNullOrEmpty(arguments)) param += " " + arguments;

            // Launch the game
            Process.Start(new ProcessStartInfo(Path.Combine(Locations.InstallBase, GeneralSettings.AppNameShort + ".exe"), param));
        }
    }
}
