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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using FrameOfReference.World;
using FrameOfReference.World.Config;
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common;
using NanoByte.Common.Info;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using Locations = NanoByte.Common.Storage.Locations;

namespace FrameOfReference.Presentation;

/// <summary>
/// Performs rendering for the benchmark mode
/// </summary>
public sealed class BenchmarkPresenter : Presenter
{
    /// <summary>
    /// How long a single test-case should be rendered in seconds
    /// </summary>
    private const int TestCaseTime = 5;

    /// <summary>
    /// Creates a new benchmark presenter
    /// </summary>
    /// <param name="engine">The engine to use for rendering</param>
    /// <param name="universe">The universe to display</param>
    /// <param name="callback">A delegate to execute after the benchmark is complete with the path of the result file</param>
    [SetsRequiredMembers]
    public BenchmarkPresenter(Engine engine, Universe universe, Action<string> callback) : base(engine, universe)
    {
        #region Sanity checks
        if (engine == null) throw new ArgumentNullException(nameof(engine));
        if (universe == null) throw new ArgumentNullException(nameof(universe));
        #endregion

        _callback = callback ?? throw new ArgumentNullException(nameof(callback));

        // Make sure an empty directory is available for the benchmark results
        if (Directory.Exists(_resultDir)) Directory.Delete(_resultDir, true);
        Directory.CreateDirectory(_resultDir);

        _statistics = new(AppInfo.Current.Version ?? "", Engine.Version.ToString(), universe);

        // Target camera on first BenchmarkPoint
        var mainCamera = CreateCamera(_statistics.TestCases.Length > 0 ? _statistics.TestCases[0].Target : null);

        View = new(Scene, mainCamera) {Name = "Benchmark", BackgroundColor = universe.FogColor};
    }

    #region Variables
    private readonly string
        _resultDir = Locations.GetSaveDataPath(GeneralSettings.AppName, false, "Benchmark"),
        _resultFile = Locations.GetSaveDataPath(GeneralSettings.AppName, true, $"{GeneralSettings.AppName} Benchmark.zip");

    private readonly Action<string> _callback;

    private readonly Statistics _statistics;
    private int _testCaseCounter;
    private Stopwatch? _testCaseTimer;
    private long _lastTotalFrames;
    #endregion

    #region PostRender hook
    /// <inheritdoc/>
    public override void HookIn()
    {
        base.HookIn();

        // Perform additional actions after each frame
        Engine.PostRender += PostRender;
    }

    /// <inheritdoc/>
    public override void HookOut()
    {
        // Stop additional actions after each frame
        Engine.PostRender -= PostRender;

        base.HookOut();
    }

    /// <summary>
    /// Is called after each frame has finished rendering
    /// </summary>
    private void PostRender()
    {
        if (_testCaseCounter == _statistics.TestCases.Length)
        { // All test-cases have been handled
            BenchmarkComplete();
        }
        else if (_testCaseTimer == null)
        { // The last test-case has been handled, ready for the next one
            NextTestCase();
        }
        else
        { // If target time has been passed, store actual time and then stop timer
            var elapsedTime = _testCaseTimer.Elapsed;
            if (elapsedTime.TotalSeconds >= TestCaseTime)
            {
                _testCaseTimer = null;
                FinishTestCase(elapsedTime.TotalSeconds);
            }
        }
    }
    #endregion

    #region Next test-case
    /// <summary>
    /// Called by <see cref="PostRender"/> when the next <see cref="TestCase"/> needs to be prepared
    /// </summary>
    private void NextTestCase()
    {
        var testCase = _statistics.TestCases[_testCaseCounter];

        // Apply settings
        Settings.Current.Display.Resolution = Settings.Current.Display.WindowSize =
            testCase.HighRes ? new(1024, 768) : new Size(800, 600);
        Settings.Current.Display.AntiAliasing = testCase.AntiAliasing ? 2 : 0;
        Settings.Current.Graphics.Anisotropic = testCase.GraphicsSettings.HasFlag(TestGraphicsSettings.Anisotropic);
        Settings.Current.Graphics.DoubleSampling = testCase.GraphicsSettings.HasFlag(TestGraphicsSettings.DoubleSampling);
        Settings.Current.Graphics.PostScreenEffects = testCase.GraphicsSettings.HasFlag(TestGraphicsSettings.PostScreenEffects);
        Settings.Current.Graphics.WaterEffects = testCase.WaterEffects;

        // Set camera to new test-case target
        View.Camera = CreateCamera(testCase.Target);

        Log.Info($"Start test-case #{_testCaseCounter}");

        // Start the timer
        _lastTotalFrames = Engine.Performance.TotalFrames;
        _testCaseTimer = Stopwatch.StartNew();
    }
    #endregion

    #region Finish test-case
    /// <summary>
    /// Called by <see cref="PostRender"/> when a <see cref="TestCase"/> has run long enough
    /// </summary>
    private void FinishTestCase(double elapsedTime)
    {
        long frames = Engine.Performance.TotalFrames - _lastTotalFrames;

        // Store results
        _statistics.TestCases[_testCaseCounter].AverageFps = (float)(frames / elapsedTime);
        _statistics.TestCases[_testCaseCounter].AverageFrameMs = (float)(elapsedTime / frames);

        // Log a frame with all details
        Engine.Performance.LogFrame(Path.Combine(_resultDir, $"test-case{_testCaseCounter}.xml"));

        if (_statistics.TestCases[_testCaseCounter].Screenshot)
            Engine.Screenshot(Path.Combine(_resultDir, $"test-case{_testCaseCounter}.jpg"), new(640, 480));

        // Prepare next test-case
        _testCaseCounter++;
    }
    #endregion

    #region Benchmark complete
    /// <summary>
    /// Called by <see cref="PostRender"/> when all <see cref="TestCase"/>s have been run
    /// </summary>
    private void BenchmarkComplete()
    {
        // Stop rendering
        HookOut();

        Log.Info("Benchmark complete - packaging results");

        // Store hardware information in an XML file
        Engine.Capabilities.Hardware.SaveXml(Path.Combine(_resultDir, "hardware.xml"));

        // Store test-case results in an XML file
        _statistics.SaveXml(Path.Combine(_resultDir, "statistics.xml"));

        // Copy log-file to benchmark directory
        using (StreamWriter writer = File.CreateText(Path.Combine(_resultDir, "log.txt")))
            writer.Write(Log.GetBuffer());

        // Package benchmark directory into a single ZIP file
        var fastZip = new FastZip();
        fastZip.CreateZip(_resultFile, _resultDir, true, null);
        Directory.Delete(_resultDir, true);

        // Submit results
        _callback(_resultFile);
    }
    #endregion
}
