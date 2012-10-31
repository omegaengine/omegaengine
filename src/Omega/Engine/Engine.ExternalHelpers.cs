/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Drawing;
using Common;
using Common.Utils;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.VertexDecl;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine
{
    // This file contains helper methods that can be called from outside the Engine
    partial class Engine
    {
        #region Draw rectangle outline
        /// <summary>
        /// Draws a 2D colored rectangle outline
        /// </summary>
        /// <param name="rectangle">The rectangle to draw</param>
        /// <param name="color">The color to draw the rectangle in</param>
        public void DrawRectangleOutline(Rectangle rectangle, Color color)
        {
            var vertexes = new[]
            {
                new TransformedColored(rectangle.Left, rectangle.Top, 0, 1, color.ToArgb()),
                new TransformedColored(rectangle.Right, rectangle.Top, 0, 1, color.ToArgb()),
                new TransformedColored(rectangle.Right, rectangle.Bottom, 0, 1, color.ToArgb()),
                new TransformedColored(rectangle.Left, rectangle.Bottom, 0, 1, color.ToArgb())
            };
            Device.VertexFormat = TransformedColored.Format;
            SetTexture(null);
            Device.DrawIndexedUserPrimitives(PrimitiveType.LineStrip, 0, 5, 4,
                // ToDo: Properly determine vertex stride
                new[] {0, 1, 2, 3, 0}, Format.Index32, vertexes, 20);
        }
        #endregion

        //--------------------//

        #region Numeric interpolation
        /// <summary>
        /// Automatically interpolates between two numeric values while rendering
        /// </summary>
        /// <param name="start">The value to start off with</param>
        /// <param name="target">The value to end up at</param>
        /// <param name="time">The time for complete transition in seconds</param>
        /// <param name="trigonometric"><see false="true"/> smooth (trigonometric) and <see langword="false"/> for linear interpolation</param>
        /// <param name="callback">The delegate to call for with the updated interpolated value each frame</param>
        public void Interpolate(double start, double target, double time, bool trigonometric, Action<double> callback)
        {
            #region Sanity checks
            if (callback == null) throw new ArgumentNullException("callback");
            #endregion

            bool negative = start > target;

            Stopwatch interpolationTimer = null;
            Action interpolate = null;
            interpolate = delegate
            {
                double value;

                if (interpolationTimer == null)
                {
                    // Start timer the first time this delegate gets called;
                    interpolationTimer = Stopwatch.StartNew();
                    value = start;
                }
                else
                {
                    // Calc the interpolated value based on the elapsed time
                    double interpolationTime = interpolationTimer.Elapsed.TotalSeconds;
                    if (trigonometric) value = MathUtils.InterpolateTrigonometric(interpolationTime / time, start, target);
                    else value = interpolationTime / time * (target - start) + start;

                    // Don't shoot past the target
                    if ((negative && value < target) || (!negative && value > target)) value = target;
                }

                // Return the value to the delegate
                callback(value);

                // Delegate removes itself from PostRender once Max value has been reached
                // ReSharper disable AccessToModifiedClosure
                // ReSharper disable CompareOfFloatsByEqualityOperator
                if (value == target) PreRender -= interpolate;
                // ReSharper restore CompareOfFloatsByEqualityOperator
                // ReSharper restore AccessToModifiedClosure
            };

            // Hook interpolation delegate into PostRender sequence
            PreRender += interpolate;
        }
        #endregion

        #region Fading
        /// <summary>
        /// Fades in the screen from total black in one second
        /// </summary>
        public void FadeIn()
        {
            Interpolate(255, 0, 1, true, value => FadeLevel = (int)value);
            FadeExtra = true;
        }

        /// <summary>
        /// Dims in the screen down
        /// </summary>
        public void DimDown()
        {
            Interpolate(FadeLevel, 80, 1, true, value => FadeLevel = (int)value);
            FadeExtra = false;
        }

        /// <summary>
        /// Dims in the screen back up
        /// </summary>
        public void DimUp()
        {
            Interpolate(FadeLevel, 0, 1, true, value => FadeLevel = (int)value);
            FadeExtra = false;
        }
        #endregion

        //--------------------//

        #region Screenshot
        /// <summary>
        /// Saves the current screen at it's current resolution
        /// </summary>
        /// <param name="filename">The file name to save the screenshot too</param>
        public void Screenshot(string filename)
        {
            Screenshot(filename, RenderSize);
        }

        /// <summary>
        /// Saves the current screen at a scaled resolution as JPG
        /// </summary>
        /// <param name="filename">The file name to save the screenshot too</param>
        /// <param name="width">The width to scale the resolution down too</param>
        /// <param name="height">The height to scale the resolution down too</param>
        public void Screenshot(string filename, int width, int height)
        {
            Screenshot(filename, new Size(width, height));
        }

        /// <summary>
        /// Saves the current screen at a scaled resolution
        /// </summary>
        /// <param name="filename">The file name to save the screenshot too</param>
        /// <param name="size">The size to scale the resolution down too</param>
        public void Screenshot(string filename, Size size)
        {
            #region Sanity checks
            if (filename == null) throw new ArgumentNullException("filename");
            #endregion

            if (size.Width > RenderSize.Width || size.Height > RenderSize.Height)
                throw new ArgumentException(Resources.InvalidScreenshotSize, "size");

            // Backup PostRender delegate and deactiavte it
            Action postRender = PreRender;
            PreRender = null;

            var sizeBackup = new Size();
            if (size != RenderSize)
            { // The size needs to be changed
                // Backup current size and replace it with new size
                sizeBackup = RenderSize;
                RenderSize = size;

                // Update everything hooked to the engine
                if (DeviceLost != null) DeviceLost();
                if (DeviceReset != null) DeviceReset();
            }

            // Render one frame for screenshot
            Render(0, true);

            // Copy the BackBuffer to the file
            Surface.ToFile(BackBuffer, filename, ImageFileFormat.Jpg, new Rectangle(new Point(), size));
            Log.Info("Screenshot created");

            if (sizeBackup != Size.Empty)
            { // The size was changed
                // Restore the original viewport
                RenderSize = sizeBackup;

                // Update everything hooked to the engine
                if (DeviceLost != null) DeviceLost();
                if (DeviceReset != null) DeviceReset();

                // Render one frame to restore original configuration
                Render(0, true);
            }

            // Restore PostRender delegate
            PreRender = postRender;
        }
        #endregion

        #region Debug interface
        /// <summary>Contains a reference to the <see cref="DebugForm"/> while it is open</summary>
        private DebugForm _debugForm;

        /// <summary>
        /// Displays a debug interface for the engine, allowing easy manipulation of <see cref="Views"/>, <see cref="Renderable"/>, etc.
        /// </summary>
        public void Debug()
        {
            if (!PresentParams.Windowed)
                throw new InvalidOperationException(Resources.NoDebugInFullscreen);

            // Only create a new form if there isn't already one open
            if (_debugForm == null)
            {
                _debugForm = new DebugForm(this);

                // Remove the reference as soon the form is closed
                _debugForm.FormClosed += delegate { _debugForm = null; };
            }

            _debugForm.Show();
        }

        /// <summary>
        /// Closes the windows displayed by <see cref="Debug"/>
        /// </summary>
        public void DebugClose()
        {
            if (_debugForm != null) _debugForm.Close();
        }
        #endregion
    }
}
