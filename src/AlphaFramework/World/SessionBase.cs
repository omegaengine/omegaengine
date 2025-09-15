/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using LuaInterface;

namespace AlphaFramework.World;

/// <summary>
/// A common base for game sessions (i.e. a game actually being played).
/// </summary>
/// <typeparam name="TUniverse">The specific type of <see cref="IUniverse"/> stored in the session.</typeparam>
public abstract class SessionBase<TUniverse>
    where TUniverse : class, IUniverse
{
    /// <summary>
    /// The current state of the game world.
    /// </summary>
    [LuaHide]
    public TUniverse Universe { get; set; }

    /// <summary>
    /// The filename of the map file the <see cref="Universe"/> was loaded from.
    /// </summary>
    public string? MapSourceFile { get; set; }

    /// <summary>
    ///  Base-constructor for XML serialization. Do not call manually!
    /// </summary>
    protected SessionBase()
    {
        Universe = null!;
    }

    /// <summary>
    /// Creates a new game session based upon a given universe
    /// </summary>
    /// <param name="baseUniverse">The universe to base the new game session on.</param>
    protected SessionBase(TUniverse baseUniverse)
    {
        Universe = baseUniverse ?? throw new ArgumentNullException(nameof(baseUniverse));

        // Transfer map name from universe to session, because it will persist there
        MapSourceFile = baseUniverse.SourceFile;
    }

    #region Update
    /// <summary>
    /// The factor by which <see cref="UniverseBase{TCoordinates}.GameTime"/> progression should be multiplied in relation to real time.
    /// </summary>
    [DefaultValue(1.0)]
    public double TimeWarpFactor { get; set; } = 1;

    /// <summary>
    /// Updates the underlying <see cref="Universe"/>.
    /// </summary>
    /// <param name="elapsedRealTime">How much real time in seconds has elapsed since this method was last called.</param>
    /// <returns>The elapsed game time (real time multiplied by <see cref="TimeWarpFactor"/>)</returns>
    public virtual double Update(double elapsedRealTime)
    {
        double elapsedGameTime = (elapsedRealTime * TimeWarpFactor);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (elapsedGameTime != 0) Universe.Update(elapsedGameTime);

        return elapsedGameTime;
    }
    #endregion
}
