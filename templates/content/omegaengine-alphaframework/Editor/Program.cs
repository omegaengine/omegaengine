using System.ComponentModel;
using System.Globalization;
using AlphaFramework.Editor;
using AlphaFramework.Editor.Properties;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using NanoByte.Common.Values;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using Template.AlphaFramework.Presentation.Config;

namespace Template.AlphaFramework.Editor;

static class Program
{
    /// <summary>
    /// Shall the application start from the beginning again?
    /// </summary>
    public static bool Restart = true;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        XmlStorageConfig.Apply();

        Settings.LoadCurrent();
        UpdateLocale();
        Settings.SaveCurrent();
        Settings.EnableAutoSave();

        // Setup content sources
        try
        {
            if (Settings.Current.General.ContentDir is {} contentDir)
                ContentManager.BaseDir = new DirectoryInfo(Path.Combine(Locations.InstallBase, contentDir));
        }
        #region Error handling
        catch (Exception ex) when (ex is ArgumentException or DirectoryNotFoundException)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
            return;
        }
        #endregion

        // The user might want to come back here multiple times, in order to switch the mod
        while (Restart)
        {
            Restart = false;

            // Ask user to select mod, cancel if an exception occurred
            Application.Run(new ModSelectorForm(Settings.Current.Editor));

            // Exit if the user didn't select anything
            if (ContentManager.ModDir == null && !ModInfo.MainGame) break;

            Application.Run(new MainForm());

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
        Settings.Current.General.Language ??= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var language = CultureInfo.CreateSpecificCulture(Settings.Current.General.Language);
        Languages.SetUI(language);
        Resources.Culture = Engine.ResourceCulture = OmegaGUI.Model.Dialog.ResourceCulture = language;
    }

    /// <summary>
    /// Launches the main game with the currently active mod (arguments automatically set).
    /// </summary>
    /// <param name="arguments">Additional arguments to be passed.</param>
    /// <exception cref="Win32Exception">The game executable could not be launched.</exception>
    /// <exception cref="BadImageFormatException">The game executable is damaged.</exception>
    internal static void LaunchGame(params string[] arguments)
    {
        // Make sure the current mod is loaded
        if (ContentManager.ModDir != null) arguments = ["/mod", ContentManager.ModDir.FullName.TrimEnd(Path.DirectorySeparatorChar), ..arguments];

        // Launch the game
        ProcessUtils.Start(Path.Combine(Locations.InstallBase, "Template.AlphaFramework.exe"), arguments);
    }
}
