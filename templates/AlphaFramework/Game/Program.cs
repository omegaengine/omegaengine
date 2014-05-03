using System;
using System.IO;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Storage.SlimDX;

namespace Game
{
    internal static class Program
    {
        /// <summary>
        /// The arguments this application was launched with.
        /// </summary>
        public static Arguments Args { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            Args = new Arguments(args);

            if (!DetermineContentDirs()) return;
            ContentManager.LoadArchives();
            using (var game = new Game())
                game.Run();
            ContentManager.CloseArchives();
        }

        /// <summary>
        /// Determines the data directories used by <see cref="ContentManager"/> and displays error messages if a directory could not be found.
        /// </summary>
        /// <returns><see langword="true"/> if all directories were located successfully; <see langword="false"/> if something went wrong.</returns>
        /// <remarks>The <see cref="ContentManager.ModDir"/> is also handled based on <see cref="Args"/>.</remarks>
        private static bool DetermineContentDirs()
        {
            try
            {
                // Base
                string baseDirPath = Environment.GetEnvironmentVariable("OMEGAENGINE_BASE_DIR");
                if (!string.IsNullOrEmpty(baseDirPath)) ContentManager.BaseDir = new DirectoryInfo(baseDirPath);

                // Mod
                if (Args.Contains("mod"))
                    ContentManager.ModDir = new DirectoryInfo(Path.Combine(Path.Combine(Locations.InstallBase, "Mods"), Args["mod"]));
                else
                {
                    string modDirPath = Environment.GetEnvironmentVariable("OMEGAENGINE_MOD_DIR");
                    if (!string.IsNullOrEmpty(modDirPath)) ContentManager.ModDir = new DirectoryInfo(modDirPath);
                }
                if (ContentManager.ModDir != null) Log.Info("Load mod from: " + ContentManager.ModDir);
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
    }
}
