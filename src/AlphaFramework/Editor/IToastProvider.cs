/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace AlphaFramework.Editor;

/// <summary>
/// Provides a surface for displaying toas messages to the user.
/// </summary>
public interface IToastProvider
{
    /// <summary>
    /// Displays a new toast messages to the user. Any exisiting messages are replaced.
    /// </summary>
    /// <param name="message">The message text to be displayed.</param>
    /// <remarks>A toast message is a message that pops up from a corner of the screen or window and vanishes after a few seconds without the user needing to confirm it.</remarks>
    void ShowToast(string message);
}
