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
using AlphaFramework.World.Positionables;
using Common.Collections;
using Common.Storage;
using Common.Utils;

namespace AlphaFramework.World
{
    /// <summary>
    /// A common base for game worlds (but not a running game). Equivalent to the content of a map file.
    /// </summary>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    public abstract class UniverseBase<TCoordinates> : IUniverse
        where TCoordinates : struct
    {
        /// <summary>
        /// Total elapsed game time in seconds.
        /// </summary>
        public double GameTime { get; set; }

        private float _timeWarpFactor = 1;
        /// <summary>
        /// The factor by which <see cref="GameTime"/> progression should be multiplied in relation to real time.
        /// </summary>
        /// <remarks>This multiplication is not done by <see cref="Update"/>!</remarks>
        [DefaultValue(1f)]
        public float TimeWarpFactor { get { return _timeWarpFactor; } set { _timeWarpFactor = value; } }

        /// <summary>
        /// A collection of all <see cref="Positionable{TCoordinates}"/>s in this <see cref="UniverseBase{TCoordinates}"/>.
        /// </summary>
        // Note: Can not use ICollection<T> interface with XML Serialization
        [Browsable(false)]
        [XmlIgnore] // XML serialization configuration is configured in sub-type
        public abstract MonitoredCollection<Positionable<TCoordinates>> Positionables { get; }

        private string _skybox;

        /// <summary>
        /// The name of the skybox to use for this map; may be <see langword="null"/> or empty.
        /// </summary>
        [DefaultValue(""), Category("Background"), Description("The name of the skybox to use for this map; may be null or empty.")]
        public string Skybox { get { return _skybox; } set { value.To(ref _skybox, SkyboxChanged); } }

        /// <summary>
        /// Occurs when <see cref="Skybox"/> was changed.
        /// </summary>
        [Description("Occurs when Skybox was changed")]
        public event Action SkyboxChanged;

        /// <summary>
        /// The position and direction of the camera in the game.
        /// </summary>
        /// <remarks>This is updated only when leaving the game, not continuously.</remarks>
        [Browsable(false)]
        public CameraState<TCoordinates> Camera { get; set; }

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        public string SourceFile { get; set; }

        /// <inheritdoc/>
        public virtual void Update(double elapsedGameTime)
        {
            GameTime += elapsedGameTime;

            foreach (var entity in Positionables.OfType<IUpdateable>())
                entity.Update(elapsedGameTime);
        }

        /// <inheritdoc/>
        public abstract void Save(string path);

        /// <inheritdoc/>
        public void Save()
        {
            // Determine the original filename to overweite
            Save(Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile));
        }
    }
}
