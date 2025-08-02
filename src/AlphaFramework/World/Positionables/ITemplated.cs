/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace AlphaFramework.World.Positionables;

/// <summary>
/// An interface to elements that are based on a named template.
/// </summary>
public interface ITemplated
{
    /// <summary>
    /// The name of the template.
    /// </summary>
    string TemplateName { get; set; }
}
