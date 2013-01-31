/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using Common;

namespace OmegaEngine
{

    #region Enumerations
    /// <seealso cref="Engine.LogFrame"/>
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

    // This file contains code for measuring the framerate
    partial class Engine
    {
        #region Constants
        /// <summary>
        /// How many frames to consider for <see cref="Fps"/> calculation
        /// </summary>
        private const long FpsResetLimit = 30;
        #endregion

        #region Variables
        private readonly Stopwatch _frameTimer = new Stopwatch();
        private readonly Stopwatch _fpsTimer = new Stopwatch();

        private FrameLog _frameLogMode;
        private string _frameLogFile;

        private long _fpsFrames;
        private double _lastFrameTime;
        #endregion

        #region Properties
        /// <summary>
        /// Frames per second - auto-calculated by the engine
        /// </summary>
        public float Fps { get; private set; }

        /// <summary>
        /// A string representing the current framerate in this format: "FPS: 12.34" (two decimals)
        /// </summary>
        public string FpsText { get { return string.Format("FPS {0:0.00}", Fps); } }

        /// <summary>
        /// Average milliseconds per frame - auto-calculated by the engine
        /// </summary>
        public float FrameMs { get; private set; }

        /// <summary>
        /// How many frames the engine has rendered since it was started
        /// </summary>
        public long TotalFrames { get; private set; }

        /// <summary>
        /// How many seconds game time have elapsed since the first frame was drawn
        /// </summary>
        internal double TotalTime { get; private set; }

        /// <summary>
        /// How many seconds game time have elapsed since the last frame was drawn
        /// </summary>
        internal double ThisFrameTime { get; private set; }
        #endregion

        //--------------------//

        #region Frame log
        /// <summary>
        /// Logs a complete CPU frame for performance profiling
        /// </summary>
        /// <param name="filename">The path of the file to store the log in</param>
        public void LogFrameCpu(string filename)
        {
            LogFrame(filename, FrameLog.CpuOnly);
        }

        /// <summary>
        /// Logs a complete CPU/GPU frame for performance profiling
        /// </summary>
        /// <param name="filename">The path of the file to store the log in</param>
        public void LogFrameCpuGpu(string filename)
        {
            LogFrame(filename, FrameLog.CpuGpu);
        }

        private void LogFrame(string filename, FrameLog logMode)
        {
            #region Sanity checks
            if (filename == null) throw new ArgumentNullException("filename");
            #endregion

            // Backup PostRender delegate and deactiavte it
            Action postRender = PreRender;
            PreRender = null;

            // Setup frame for logging and render it
            _frameLogMode = logMode;
            _frameLogFile = filename;
            Render(0, true);

            // Restore PostRender delegate
            PreRender = postRender;

            Log.Info("Frame-log created");
        }
        #endregion
    }
}
