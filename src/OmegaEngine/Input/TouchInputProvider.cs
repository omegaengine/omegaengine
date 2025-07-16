/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using NanoByte.Common.Controls;

namespace OmegaEngine.Input
{
    /// <summary>
    /// Processes touch events into higher-level navigational commands.
    /// </summary>
    /// <remarks>Complex manipulations with combined panning, rotating and zooming are possible.</remarks>
    public class TouchInputProvider : InputProvider
    {
        #region Variables
        /// <summary>The control receiving the keyboard events.</summary>
        private readonly ITouchControl _control;
        #endregion

        #region Constructor
        /// <summary>
        /// Starts monitoring and processing Touch events received by a specific control.
        /// </summary>
        /// <param name="control">The control receiving the touch events.</param>
        public TouchInputProvider(ITouchControl control)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));

            // Start tracking input events
            _control.TouchDown += TouchDown;
            _control.TouchMove += TouchMove;
            _control.TouchUp += TouchUp;
        }
        #endregion

        //--------------------//

        #region Event handlers
        private void TouchDown(object sender, TouchEventArgs e)
        {
            // ToDo: Implement
        }

        private void TouchMove(object sender, TouchEventArgs e)
        {
            // ToDo: Implement
        }

        private void TouchUp(object sender, TouchEventArgs e)
        {
            // ToDo: Implement
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                // Stop tracking input events
                _control.TouchDown -= TouchDown;
                _control.TouchMove -= TouchMove;
                _control.TouchUp -= TouchUp;
            }
        }
        #endregion
    }
}
