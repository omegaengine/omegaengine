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
using Common.Storage;
using Common.Utils;
using OmegaEngine;

namespace Editor
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
            Application.EnableVisualStyles();

            Args = new Arguments(args);

            UpdateLocale();

            // The user might want to come back here multiple times, in order to switch the mod
            while (Restart)
            {
                Restart = false;

                // Ask user to select mod, cancel if an exception occurred
                Application.Run(new ModSelectorForm(allowEditMain: true));

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
        /// The language currently used by the application.
        /// </summary>
        public static string Language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        /// <summary>
        /// Updates the localization used by the application.
        /// </summary>
        public static void UpdateLocale()
        {
            // Propagate selected language to other assemblies
            Resources.Culture = Engine.ResourceCulture = OmegaGUI.Model.Dialog.ResourceCulture = new CultureInfo(Language);

            // Create specific culture for thread
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(Resources.Culture.Name);
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
            Process.Start(new ProcessStartInfo(Path.Combine(Locations.InstallBase, "Game.exe"), param));
        }
    }
}
