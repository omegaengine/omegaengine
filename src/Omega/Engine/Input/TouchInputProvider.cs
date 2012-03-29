/*
 * Copyright 2006-2012 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using Common.Controls;

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
        /// Starts monitoring and processing Touch events receieved by a specififc control.
        /// </summary>
        /// <param name="control">The control receiving the touch events.</param>
        public TouchInputProvider(ITouchControl control)
        {
            #region Sanity checks
            if (control == null) throw new ArgumentNullException("control");
            #endregion

            _control = control;

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
