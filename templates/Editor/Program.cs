using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using AlphaFramework.Editor;
using AlphaFramework.Editor.Properties;
using Common;
using Common.Storage;
using OmegaEngine;
using OmegaGUI.Model;

namespace $safeprojectname$
{
    internal static class Program
    {
        /// <summary>
        /// Shall the application start from the beginning again?
        /// </summary>
        public static bool Restart = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Justification = "GC.Collect is only called after the main form closes, when a lot of long-lived objects have turned into garbage")]
        private static void Main()
        {
            Application.EnableVisualStyles();
            UpdateLocale();

            // The user might want to come back here multiple times, in order to switch the mod
            while (Restart)
            {
                Restart = false;

                // Ask user to select mod, cancel if an exception occurred
                Application.Run(new ModSelectorForm(true, new List<string>()));

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
            Resources.Culture = Engine.ResourceCulture = Dialog.ResourceCulture = new CultureInfo(Language);

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
    }
}
