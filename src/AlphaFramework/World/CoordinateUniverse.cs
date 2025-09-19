/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using NanoByte.Common.Collections;

namespace AlphaFramework.World;

/// <summary>
/// A common base for game worlds with objects in a coordinate system.
/// </summary>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
public abstract class CoordinateUniverse<TCoordinates> : UniverseBase
    where TCoordinates : struct
{
    /// <summary>
    /// A collection of all <see cref="Positionable{TCoordinates}"/>s in this <see cref="CoordinateUniverse{TCoordinates}"/>.
    /// </summary>
    // Note: Can not use ICollection<T> interface with XML Serialization
    [Browsable(false)]
    [XmlIgnore] // XML serialization configuration is configured in sub-type
    public abstract MonitoredCollection<Positionable<TCoordinates>> Positionables { get; }

    /// <summary>
    /// The pathfinding engine used to navigate <see cref="Positionables"/>.
    /// </summary>
    [Browsable(false), XmlIgnore]
    public IPathfinder<TCoordinates>? Pathfinder { get; set; }

    /// <inheritdoc/>
    public override void Update(double elapsedGameTime)
    {
        base.Update(elapsedGameTime);

        foreach (var updateable in Positionables.OfType<IUpdateable>())
            Update(updateable, elapsedGameTime);
    }

    /// <summary>
    /// Updates a single <see cref="IUpdateable"/>.
    /// </summary>
    protected virtual void Update(IUpdateable updateable, double elapsedGameTime)
    {
        #region Sanity checks
        if (updateable == null) throw new ArgumentNullException(nameof(updateable));
        #endregion

        updateable.Update(elapsedGameTime);
    }
}
