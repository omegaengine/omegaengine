using System;
using System.Diagnostics;
using System.Text;
using System.Xml;
using NanoByte.Common;
using SlimDX.Direct3D9;

namespace OmegaEngine;

#region Enumerations
/// <seealso cref="EnginePerformance.LogFrame"/>
public enum FrameLog
{
    /// <summary>Do not create a log of the frame</summary>
    Off,

    /// <summary>Create a log of the frame without stalling the GPU pipeline, measuring only the CPU load</summary>
    CpuOnly,

    /// <summary>Create a log of the frame stalling the GPU pipeline, measuring the CPU and GPU load</summary>
    CpuGpu
}
#endregion

/// <summary>
/// Tracks the performance/speed of the <see cref="Engine"/>.
/// </summary>
public sealed class EnginePerformance
{
    #region Constants
    /// <summary>
    /// How many frames to consider for <see cref="Fps"/> calculation
    /// </summary>
    private const long FpsResetLimit = 30;
    #endregion

    #region Variables
    private readonly Stopwatch _frameTimer = new();
    private readonly Stopwatch _fpsTimer = new();
    private long _framesCounted;

    private FrameLog _frameLogMode;
    private string _frameLogFile;
    #endregion

    #region Properties
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
    #endregion

    #region Dependencies
    private readonly Device _device;
    private readonly Action _renderPure;

    internal EnginePerformance(Device device, Action renderPure)
    {
        _device = device;
        _renderPure = renderPure;
    }

    internal void Reset()
    {
        _fpsTimer.Reset();
    }
    #endregion

    //--------------------//

    #region Log frame
    /// <summary>
    /// Logs a complete frame for performance profiling.
    /// </summary>
    /// <param name="path">The path of the file to store the log in.</param>
    /// <param name="logMode">Controls how the frame log is captured.</param>
    public void LogFrame(string path, FrameLog logMode = FrameLog.CpuGpu)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
        #endregion

        _frameLogMode = logMode;
        _frameLogFile = path;
        _renderPure();

        Log.Info("Frame-log created");
    }
    #endregion

    #region Handlers
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

    /// <summary>
    /// To be called before rendering begins.
    /// </summary>
    internal void BeforeRender()
    {
        if (_frameLogMode == FrameLog.Off) return;

        // Setup log XML document
        Profiler.LogXml = new();

        // Log the GPU time as well
        if (_frameLogMode == FrameLog.CpuGpu)
        {
            Profiler.DeviceQuery = new(_device, QueryType.Event);
            Profiler.AddEvent("Forcing render pipeline stalls to log GPU performance");
        }
    }

    /// <summary>
    /// To be called after rendering ends.
    /// </summary>
    internal void AfterRender()
    {
        if (_frameLogMode == FrameLog.Off) return;
        _frameLogMode = FrameLog.Off;

        // Write to disk
        var writer = new XmlTextWriter(_frameLogFile, new UTF8Encoding(false)) {Formatting = Formatting.Indented};
        Profiler.LogXml.Save(writer);
        writer.Close();

        // Clean up
        Profiler.LogXml = null;
        if (Profiler.DeviceQuery != null)
        {
            Profiler.DeviceQuery.Dispose();
            Profiler.DeviceQuery = null;
        }
    }
    #endregion
}
