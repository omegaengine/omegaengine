/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using NanoByte.Common;
using NanoByte.Common.Dispatch;

namespace AlphaFramework.World.Positionables;

/// <summary>
/// An object that can be positioned in the game world.
/// </summary>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
public abstract class Positionable<TCoordinates> : ICloneable, IChangeNotify<Positionable<TCoordinates>>
    where TCoordinates : struct
{
    #region Events
    /// <summary>
    /// Occurs when a property relevant for rendering has changed.
    /// </summary>
    [Description("Occurs when a property relevant for rendering has changed.")]
    public event Action<Positionable<TCoordinates>> Changed;

    /// <summary>
    /// To be called when a property relevant for rendering has changed.
    /// </summary>
    protected virtual void OnChanged() => Changed?.Invoke(this);

    /// <summary>
    /// Occurs when a property has changed that requires visual representations to rebuilt from scratch (usually a template).
    /// </summary>
    [Description("Occurs when a property changed that requires visual representations to rebuilt from scratch (usually a template).")]
    public event Action<Positionable<TCoordinates>> ChangedRebuild;

    /// <summary>
    /// To be called when a property has changed, that requires visual representations to be rebuilt from scratch (usually a template).
    /// </summary>
    protected void OnChangedRebuild() => ChangedRebuild?.Invoke(this);
    #endregion

    #region Properties
    /// <summary>
    /// Used for identification in scripts, debugging, etc.
    /// </summary>
    [XmlAttribute, Description("Used for identification in scripts, debugging, etc.")]
    public string Name { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        string value = GetType().Name;
        if (!string.IsNullOrEmpty(Name))
            value += ": " + Name;
        return value;
    }

    private TCoordinates _position;

    /// <summary>
    /// The <see cref="Positionable{TCoordinates}"/>'s position.
    /// </summary>
    [Description("The entity's position on the terrain.")]
    public TCoordinates Position { get => _position; set => value.To(ref _position, OnChanged); }
    #endregion

    //--------------------//

    #region Clone
    /// <summary>
    /// Creates a deep copy of this <see cref="Positionable{TCoordinates}"/>.
    /// </summary>
    /// <returns>The cloned <see cref="Positionable{TCoordinates}"/>.</returns>
    public virtual Positionable<TCoordinates> Clone()
    {
        var clonedPositionable = (Positionable<TCoordinates>)MemberwiseClone();

        // Don't clone event handlers
        clonedPositionable.Changed = null;
        clonedPositionable.ChangedRebuild = null;

        return clonedPositionable;
    }

    object ICloneable.Clone() => Clone();
    #endregion
}
