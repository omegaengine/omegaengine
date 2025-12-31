using System;
using System.Diagnostics;
using System.Text;
using System.Xml;
using NanoByte.Common;
using SlimDX.Direct3D9;

namespace OmegaEngine;

/// <summary>
/// Tracks the performance/speed of the <see cref="Engine"/>.
/// </summary>
public sealed class EnginePerformance(Device device, Action renderPure)
{
    /// <summary>
    /// How many frames to consider for <see cref="Fps"/> calculation
    /// </summary>
    private const long FpsResetLimit = 30;

    private readonly Stopwatch _frameTimer = new();
    private readonly Stopwatch _fpsTimer = new();
    private long _framesCounted;

    /// <summary>
    /// Frames per second; auto-calculated by the engine.
    /// </summary>
    public float Fps { get; private set; }

    /// <summary>
    /// Average milliseconds per frame - auto-calculated by the engine.
    /// </summary>
    public float FrameMs { get; private set; }

    /// <summary>
    /// How many frames the engine has rendered since it was started.
    /// </summary>
    public long TotalFrames { get; private set; }

    /// <summary>
    /// How many seconds of time have elapsed since the last frame started drawing.
    /// </summary>
    internal double LastFrameTime { get; private set; }

    internal void Reset() => _fpsTimer.Reset();

    /// <summary>
    /// Logs a complete frame for performance profiling.
    /// </summary>
    /// <param name="path">The path of the file to store the log in.</param>
    /// <param name="stallGpu">Stall the GPU after each draw call, to measure their individual impact.</param>
    public void LogFrame(string path, bool stallGpu = true)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        #endregion

        Profiler.LogXml = new();

        if (stallGpu)
        {
            Profiler.DeviceQuery = new(device, QueryType.Event);
            Profiler.AddEvent("Forcing render pipeline stalls to log GPU performance");
        }

        renderPure();

        // Write to disk
        using (var writer = new XmlTextWriter(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)) {Formatting = Formatting.Indented})
            Profiler.LogXml.Save(writer);

        // Clean up
        Profiler.LogXml = null;
        if (Profiler.DeviceQuery != null)
        {
            Profiler.DeviceQuery.Dispose();
            Profiler.DeviceQuery = null;
        }

        Log.Info("Frame-log created");
    }

    /// <summary>
    /// To be called at the start of a new frame. Keeps track of the frame rate.
    /// </summary>
    internal void OnNewFrame()
    {
        TotalFrames++;

        if (_frameTimer.IsRunning)
        {
            LastFrameTime = _frameTimer.Elapsed.TotalSeconds;
            _frameTimer.Reset();
        }
        _frameTimer.Start();

        if (_fpsTimer.IsRunning)
        {
            _framesCounted++;
            if (_framesCounted > FpsResetLimit)
            {
                _fpsTimer.Stop();

                Fps = (float)(_framesCounted / _fpsTimer.Elapsed.TotalSeconds);
                FrameMs = (float)(_fpsTimer.Elapsed.TotalMilliseconds / _framesCounted);
                _framesCounted = 0;

                _fpsTimer.Reset();
            }
        }
        _fpsTimer.Start();
    }
}
