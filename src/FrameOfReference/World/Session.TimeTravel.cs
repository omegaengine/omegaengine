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
using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World;
using NanoByte.Common.Utils;

namespace FrameOfReference.World
{
    partial class Session
    {
        /// <summary>
        /// Indicates whwther a <see cref="TimeTravel"/> request is currently being processed.
        /// </summary>
        [Browsable(false), XmlIgnore]
        public bool TimeTravelInProgress { get; set; }

        private double _timeTravelStart, _timeTravelTarget;

        private double _timeTravelElapsed;

        private const double TimeTravelSpeedFactor = 1 / 30.0;

        /// <summary>
        /// Like <see cref="UpdateTo"/>, but interpolates between the current and the target time smoothly.
        /// </summary>
        /// <param name="gameTime">The target value for <see cref="UniverseBase{TCoordinates}.GameTime"/>.</param>
        public void TimeTravel(double gameTime)
        {
            _timeTravelStart = Universe.GameTime;
            _timeTravelTarget = gameTime;
            _timeTravelElapsed = 0;
            TimeTravelInProgress = true;
        }

        private double UpdateTimeTravel(double elapsedRealTime)
        {
            _timeTravelElapsed += elapsedRealTime;
            double timeTravelDuration = Math.Abs(_timeTravelTarget - _timeTravelStart) * TimeTravelSpeedFactor;
            double intermediateTarget = MathUtils.InterpolateTrigonometric(_timeTravelElapsed / timeTravelDuration, _timeTravelStart, _timeTravelTarget);

            double gameTimeDelta = intermediateTarget - Universe.GameTime;
            UpdateDeterministic(gameTimeDelta);

            if (_timeTravelElapsed > timeTravelDuration)
            {
                TimeTravelInProgress = false;
                _timeTravelStart = _timeTravelTarget = _timeTravelElapsed = 0;
            }

            return gameTimeDelta;
        }
    }
}
