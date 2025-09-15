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
using FrameOfReference.Presentation.Config;
using FrameOfReference.Properties;
using FrameOfReference.World;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Values;
using OmegaEngine;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Foundation.Storage;
using OmegaGUI.Model;

namespace FrameOfReference;

public static class Program
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
        WindowsUtils.SetCurrentProcessAppID($"{Application.CompanyName}.{Universe.AppNameShort}");

        Application.EnableVisualStyles();
        ErrorReportForm.SetupMonitoring(new Uri($"https://omegaengine.de/error-report/?app={Universe.AppNameShort}"));

#if !DEBUG
        // Prevent multiple instances from running simultaneously
        WindowsMutex.Create(Universe.AppName, out bool alreadyRunning);
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
        else Settings.EnableAutoSave();

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

    /// <summary>
    /// Normalizes <see cref="Settings"/> for comparable benchmark results.
    /// </summary>
    private static void ConfigureSettingsForBenchmark()
    {
        Settings.Current.Graphics.Fading = false;
        Settings.Current.Graphics.WaterEffects = WaterEffectsType.None;
        Settings.Current.Display.VSync = false;
        Settings.Current.Display.Resolution = Settings.Current.Display.WindowSize = new(1024, 768);
#if !DEBUG
        Settings.Current.Display.Fullscreen = true;
#endif
    }
}
