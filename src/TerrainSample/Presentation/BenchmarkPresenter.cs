/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Drawing;
using System.IO;
using Common;
using Common.Info;
using Common.Utils;
using Common.Storage;
using Core;
using ICSharpCode.SharpZipLib.Zip;
using OmegaEngine;
using OmegaEngine.Graphics;
using World;

namespace Presentation
{
    /// <summary>
    /// Performs rendering for the benchmark mode
    /// </summary>
    public sealed class BenchmarkPresenter : Presenter
    {
        #region Constants
        /// <summary>
        /// How long a single test-case should be rendered in seconds
        /// </summary>
        private const int TestCaseTime = 5;
        #endregion

        #region Variables
        private readonly string
            _resultDir = Locations.GetSaveDataPath(GeneralSettings.AppName, false, "Benchmark"),
            _resultFile = Locations.GetSaveDataPath(GeneralSettings.AppName, true, GeneralSettings.AppName + " Benchmark.zip");

        private readonly Action<string> _callback;

        private readonly Statistics _statistics;
        private int _testCaseCounter;
        private Stopwatch _testCaseTimer;
        private long _lastTotalFrames;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new benchmark presenter
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The universe to display</param>
        /// <param name="callback">A delegate to execute after the benchmark is complete with the path of the result file</param>
        public BenchmarkPresenter(Engine engine, Universe universe, Action<string> callback) : base(engine, universe)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (universe == null) throw new ArgumentNullException("universe");
            if (callback == null) throw new ArgumentNullException("callback");
            #endregion

            _callback = callback;

            // Make sure an empty directory is available for the benchmark results
            if (Directory.Exists(_resultDir)) Directory.Delete(_resultDir, true);
            Directory.CreateDirectory(_resultDir);

            _statistics = new Statistics(AppInfo.Current.Version.ToString(), Engine.Version.ToString(), universe);

            // Target camera on first BenchmarkPoint
            var mainCamera = CreateCamera(_statistics.TestCases.Length > 0 ? _statistics.TestCases[0].Target : null);

            View = new View(engine, Scene, mainCamera) {Name = "Benchmark", BackgroundColor = universe.FogColor};
        }
        #endregion

        //--------------------//

        #region Engine Hook-in
        /// <inheritdoc />
        public override void HookIn()
        {
            base.HookIn();

            // Perform additional actions after each frame
            Engine.PostRender += PostRender;
        }

        /// <inheritdoc />
        public override void HookOut()
        {
            // Stop additional actions after each frame
            Engine.PostRender -= PostRender;

            base.HookOut();
        }
        #endregion

        //--------------------//

        #region PostRender hook
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
            // Apply settings
            Settings.Current.Display.Resolution = Settings.Current.Display.WindowSize =
                _statistics.TestCases[_testCaseCounter].HighRes ? new Size(1024, 768) : new Size(800, 600);
            Settings.Current.Display.AntiAliasing = _statistics.TestCases[_testCaseCounter].AntiAliasing ? 2 : 0;
            Settings.Current.Graphics.Anisotropic = MathUtils.CheckFlag((byte)_statistics.TestCases[_testCaseCounter].GraphicsSettings, (byte)TestGraphicsSettings.Anisotropic);
            Settings.Current.Graphics.DoubleSampling = MathUtils.CheckFlag((byte)_statistics.TestCases[_testCaseCounter].GraphicsSettings, (byte)TestGraphicsSettings.DoubleSampling);
            Settings.Current.Graphics.PostScreenEffects = MathUtils.CheckFlag((byte)_statistics.TestCases[_testCaseCounter].GraphicsSettings, (byte)TestGraphicsSettings.PostScreenEffects);
            Settings.Current.Graphics.WaterEffects = _statistics.TestCases[_testCaseCounter].WaterEffects;
            Settings.Current.Graphics.ParticleSystemQuality = _statistics.TestCases[_testCaseCounter].ParticleSystemQuality;

            // Set camera to new test-case target
            View.Camera = CreateCamera(_statistics.TestCases[_testCaseCounter].Target);

            Log.Info("Start test-case #" + _testCaseCounter);

            // Start the timer
            _lastTotalFrames = Engine.TotalFrames;
            _testCaseTimer = Stopwatch.StartNew();
        }
        #endregion

        #region Finish test-case
        /// <summary>
        /// Called by <see cref="PostRender"/> when a <see cref="TestCase"/> has run long enough
        /// </summary>
        private void FinishTestCase(double elapsedTime)
        {
            long frames = Engine.TotalFrames - _lastTotalFrames;

            // Store results
            _statistics.TestCases[_testCaseCounter].AverageFps = (float)(frames / elapsedTime);
            _statistics.TestCases[_testCaseCounter].AverageFrameMs = (float)(elapsedTime / frames);

            // Log a frame with all details
            Engine.LogFrameCpuGpu(Path.Combine(_resultDir, "test-case" + _testCaseCounter + ".xml"));

            if (_statistics.TestCases[_testCaseCounter].Screenshot)
                Engine.Screenshot(Path.Combine(_resultDir, "test-case" + _testCaseCounter + ".jpg"), new Size(640, 480));

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
            XmlStorage.Save(Path.Combine(_resultDir, "hardware.xml"), Engine.Hardware);

            // Store test-case results in an XML file
            XmlStorage.Save(Path.Combine(_resultDir, "statistics.xml"), _statistics);

            // Copy log-file to benchmark directory
            using (StreamWriter writer = File.CreateText(Path.Combine(_resultDir, "log.txt")))
                writer.Write(Log.Content);

            // Package benchmark directory into a single ZIP file
            var fastZip = new FastZip();
            fastZip.CreateZip(_resultFile, _resultDir, true, null);
            Directory.Delete(_resultDir, true);

            // Submit results
            _callback(_resultFile);
        }
        #endregion
    }
}
