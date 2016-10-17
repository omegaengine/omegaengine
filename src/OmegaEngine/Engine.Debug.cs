/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using NanoByte.Common;
using SlimDX.Direct3D9;
using OmegaEngine.Graphics.Renderables;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine
{
    // This file contains methods for debugging the engine
    partial class Engine
    {
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
                DeviceLost?.Invoke();
                DeviceReset?.Invoke();
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
                DeviceLost?.Invoke();
                DeviceReset?.Invoke();

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
            _debugForm?.Close();
        }
        #endregion
    }
}
