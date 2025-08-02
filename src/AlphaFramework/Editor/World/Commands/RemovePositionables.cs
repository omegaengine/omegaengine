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
/// Removes one or more <see cref="Positionable{TCoordinates}"/>s from a <see cref="IUniverse"/>.
/// </summary>
public class RemovePositionables<TCoordinates> : AddRemovePositionables<TCoordinates>
    where TCoordinates : struct
{
    #region Constructor
    /// <summary>
    /// Creates a new command for removing one or more <see cref="Positionable{TCoordinates}"/>s from a <see cref="IUniverse"/>.
    /// </summary>
    /// <param name="universe">The <see cref="UniverseBase{TCoordinates}"/> to remove from.</param>
    /// <param name="entities">The <see cref="Positionable{TCoordinates}"/>s to remove.</param>
    public RemovePositionables(UniverseBase<TCoordinates> universe, IEnumerable<Positionable<TCoordinates>> entities)
        : base(universe, entities)
    {}
    #endregion

    //--------------------//

    #region Execute
    /// <summary>
    /// Removes the <see cref="Positionable{TCoordinates}"/> from the <see cref="IUniverse"/>
    /// </summary>
    protected override void OnExecute()
    {
        RemovePositionables();
    }
    #endregion

    #region Undo
    /// <summary>
    /// Adds the <see cref="Positionable{TCoordinates}"/> back to the <see cref="IUniverse"/>
    /// </summary>
    protected override void OnUndo()
    {
        AddPositionables();
    }
    #endregion
}
