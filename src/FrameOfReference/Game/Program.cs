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
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using AlphaFramework.Presentation;
using FrameOfReference.Properties;
using FrameOfReference.World.Config;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Values;
using OmegaEngine;
using OmegaEngine.Storage;
using OmegaEngine.Values;
using OmegaGUI.Model;

namespace FrameOfReference;

internal static class Program
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
        WindowsUtils.SetCurrentProcessAppID(Application.CompanyName + "." + GeneralSettings.AppNameShort);

        Application.EnableVisualStyles();
        ErrorReportForm.SetupMonitoring(new Uri("https://omegaengine.de/error-report/?app=" + GeneralSettings.AppNameShort));

#if !DEBUG
            // Prevent multiple instances from running simultaneously
            WindowsMutex.Create(GeneralSettings.AppName, out bool alreadyRunning);
            if (alreadyRunning)
            {
                Msg.Inform(null, Resources.AlreadyRunning, MsgSeverity.Warn);
                return;
            }
#endif

        Args = new(args);

        Settings.LoadCurrent();
        UpdateLocale();
        Settings.SaveCurrent();

        // Show additional warning before actually starting the game
        if (Args.Contains("launchWarn") && !Args.Contains("benchmark"))
            if (!Msg.OkCancel(null, Resources.ReadyToLaunch, MsgSeverity.Info, Resources.ReadyToLaunchContinue)) return;

        // Handle benchmark mode
        if (Args.Contains("benchmark"))
        {
            if (!Msg.OkCancel(null, Resources.BenchmarkInfo, MsgSeverity.Info, Resources.BenchmarkInfoContinue)) return;
            ConfigureSettingsForBenchmark();
        }

        if (!DetermineContentDirs()) return;
        if (!LoadArchives()) return;
        using (var game = new Game())
            game.Run();
        ContentManager.CloseArchives();
    }

    /// <summary>
    /// Updates the localization used by the application
    /// </summary>
    public static void UpdateLocale()
    {
        if (string.IsNullOrEmpty(Settings.Current.General.Language))
            Settings.Current.General.Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        var language = CultureInfo.CreateSpecificCulture(Settings.Current.General.Language);
        Languages.SetUI(language);
        Resources.Culture = Engine.ResourceCulture = Dialog.ResourceCulture = language;
    }

    /// <summary>
    /// Normalizes <see cref="Settings"/> for comparable benchmark results. Original settings are preserved on-disk.
    /// </summary>
    private static void ConfigureSettingsForBenchmark()
    {
        Settings.AutoSave = false;
        Settings.Current.Sound.PlayMusic = false;
        Settings.Current.Graphics.Fading = false;
        Settings.Current.Graphics.WaterEffects = WaterEffectsType.None;
        Settings.Current.Graphics.ParticleSystemQuality = Quality.Low;
        Settings.Current.Display.VSync = false;
        Settings.Current.Display.Resolution = Settings.Current.Display.WindowSize = new(800, 600);
#if !DEBUG
            Settings.Current.Display.Fullscreen = true;
#endif
    }

    /// <summary>
    /// Determines the data directories used by <see cref="ContentManager"/> and displays error messages if a directory could not be found.
    /// </summary>
    /// <returns><c>true</c> if all directories were located successfully; <c>false</c> if something went wrong.</returns>
    /// <remarks>The <see cref="ContentManager.ModDir"/> is also handled based on <see cref="Args"/>.</remarks>
    private static bool DetermineContentDirs()
    {
        try
        {
            // Base
            if (!string.IsNullOrEmpty(Settings.Current.General.ContentDir))
                ContentManager.BaseDir = new(Path.Combine(Locations.InstallBase, Settings.Current.General.ContentDir));

            // Mod
            if (Args["mod"] is {} mod)
                ContentManager.ModDir = new(Path.Combine(Path.Combine(Locations.InstallBase, "Mods"), mod));
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

    /// <summary>
    /// Calls <see cref="ContentManager.LoadArchives"/> and displays error messages if something went wrong.
    /// </summary>
    /// <returns><c>true</c> if all archives were loaded successfully; <c>false</c> if something went wrong.</returns>
    private static bool LoadArchives()
    {
        try
        {
            ContentManager.LoadArchives();
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Error(Resources.FailedReadArchives, ex);
            ContentManager.CloseArchives();
            Msg.Inform(null, Resources.FailedReadArchives + Environment.NewLine + ex.Message, MsgSeverity.Error);
            return false;
        }
        #endregion

        return true;
    }
}
