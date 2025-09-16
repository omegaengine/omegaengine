using System.Globalization;
using System.Windows.Forms;
using AlphaFramework.Presentation;
using AlphaFramework.World.Properties;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Storage;
using NanoByte.Common.Values;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using OmegaGUI.Model;
using Template.AlphaFramework.Presentation.Config;

namespace Template.AlphaFramework;

static class Program
{
    /// <summary>
    /// The arguments this application was launched with.
    /// </summary>
    public static Arguments Args { get; private set; } = null!;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();

        Args = new(args);

        Settings.LoadCurrent();
        UpdateLocale();
        Settings.SaveCurrent();
        Settings.EnableAutoSave();

        // Setup content sources
        try
        {
            // Base
            if (Settings.Current.General.ContentDir is {} contentDir)
                ContentManager.BaseDir = new(Path.Combine(Locations.InstallBase, contentDir));

            // Mod
            if (Args["mod"] is {} mod)
                ContentManager.ModDir = new(Path.Combine(Path.Combine(Locations.InstallBase, "Mods"), mod));
            if (ContentManager.ModDir != null) Log.Info($"Load mod from: {ContentManager.ModDir}");
        }
        #region Error handling
        catch (Exception ex) when (ex is ArgumentException or DirectoryNotFoundException)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
            return;
        }
        #endregion

        using var game = new Game(Settings.Current);
        game.Run();
    }

    /// <summary>
    /// Updates the localization used by the application
    /// </summary>
    public static void UpdateLocale()
    {
        Settings.Current.General.Language ??= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var language = CultureInfo.CreateSpecificCulture(Settings.Current.General.Language);
        Languages.SetUI(language);
        Resources.Culture = Engine.ResourceCulture = Dialog.ResourceCulture = language;
    }
}
