/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.Editor.World.Commands;

/// <summary>
/// Adds one or more <see cref="Positionable{TCoordinates}"/>ies to a <see cref="CoordinateUniverse{TCoordinates}"/>.
/// </summary>
/// <param name="universe">The <see cref="CoordinateUniverse{TCoordinates}"/> to add to.</param>
/// <param name="entities">The <see cref="Positionable{TCoordinates}"/>ies to add.</param>
public class AddPositionables<TCoordinates>(CoordinateUniverse<TCoordinates> universe, IEnumerable<Positionable<TCoordinates>> entities)
    : AddRemovePositionables<TCoordinates>(universe, entities)
    where TCoordinates : struct
{
    /// <summary>
    /// Adds the <see cref="Positionable{TCoordinates}"/> to the <see cref="CoordinateUniverse{TCoordinates}"/>
    /// </summary>
    protected override void OnExecute() => AddPositionables();

    /// <summary>
    /// Removes the <see cref="Positionable{TCoordinates}"/> from the <see cref="CoordinateUniverse{TCoordinates}"/> again
    /// </summary>
    protected override void OnUndo() => RemovePositionables();
}
