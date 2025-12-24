/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.Editor.World.Commands;

/// <summary>
/// Adds one or more <see cref="Positionable{TCoordinates}"/>ies to a <see cref="CoordinateUniverse{TCoordinates}"/>.
/// </summary>
public class AddPositionables<TCoordinates> : AddRemovePositionables<TCoordinates>
    where TCoordinates : struct
{
    #region Constructor
    /// <summary>
    /// Creates a new command for adding one or more <see cref="Positionable{TCoordinates}"/>ies to a <see cref="CoordinateUniverse{TCoordinates}"/>.
    /// </summary>
    /// <param name="universe">The <see cref="CoordinateUniverse{TCoordinates}"/> to add to.</param>
    /// <param name="entities">The <see cref="Positionable{TCoordinates}"/>ies to add.</param>
    /// <param name="callback">Optional callback to be invoked after adding entities (e.g., to update pathfinding).</param>
    public AddPositionables(CoordinateUniverse<TCoordinates> universe, IEnumerable<Positionable<TCoordinates>> entities, Action? callback = null)
        : base(universe, entities, callback)
    {}
    #endregion

    //--------------------//

    #region Execute
    /// <summary>
    /// Adds the <see cref="Positionable{TCoordinates}"/> to the <see cref="CoordinateUniverse{TCoordinates}"/>
    /// </summary>
    protected override void OnExecute()
    {
        AddPositionables();
    }
    #endregion

    #region Undo
    /// <summary>
    /// Removes the <see cref="Positionable{TCoordinates}"/> from the <see cref="CoordinateUniverse{TCoordinates}"/> again
    /// </summary>
    protected override void OnUndo()
    {
        RemovePositionables();
    }
    #endregion
}
