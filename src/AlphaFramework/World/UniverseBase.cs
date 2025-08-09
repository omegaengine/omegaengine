/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AlphaFramework.World.Paths;
using AlphaFramework.World.Positionables;
using NanoByte.Common;
using NanoByte.Common.Collections;
using OmegaEngine.Foundation.Storage;

namespace AlphaFramework.World;

/// <summary>
/// A common base for game worlds (but not a running game). Equivalent to the content of a map file.
/// </summary>
/// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
public abstract class UniverseBase<TCoordinates> : IUniverse
    where TCoordinates : struct
{
    #region Events
    private void OnSkyboxChanged()
    {
        SkyboxChanged?.Invoke();
    }

    /// <summary>
    /// Occurs when <see cref="Skybox"/> was changed.
    /// </summary>
    [Description("Occurs when Skybox was changed")]
    public event Action? SkyboxChanged;
    #endregion

    /// <summary>
    /// Total elapsed game time in seconds.
    /// </summary>
    [DefaultValue(0.0), Category("Gameplay"), Description("Total elapsed game time in seconds.")]
    public double GameTime { get; set; }

    /// <summary>
    /// A collection of all <see cref="Positionable{TCoordinates}"/>s in this <see cref="UniverseBase{TCoordinates}"/>.
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

    private string? _skybox;

    /// <summary>
    /// The name of the skybox to use for this map; may be <c>null</c> or empty.
    /// </summary>
    [DefaultValue(""), Category("Effects"), Description("The name of the skybox to use for this map; may be null or empty.")]
    public string? Skybox { get => _skybox; set => value.To(ref _skybox, OnSkyboxChanged); }

    /// <summary>
    /// The current position and direction of the camera in the game.
    /// </summary>
    /// <remarks>This is updated only when leaving the game, not continuously.</remarks>
    [Browsable(false)]
    public CameraState<TCoordinates>? CurrentCamera { get; set; }

    /// <inheritdoc/>
    [XmlIgnore, Browsable(false)]
    public string SourceFile { get; set; } = null!;

    /// <inheritdoc/>
    public virtual void Update(double elapsedGameTime)
    {
        GameTime += elapsedGameTime;

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

    /// <inheritdoc/>
    public abstract void Save(string path);

    /// <inheritdoc/>
    public void Save()
    {
        // Determine the original filename to overwrite
        Save(Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile));
    }
}
