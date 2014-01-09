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
using System.Xml.Serialization;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.World
{
    /// <summary>
    /// A common base for all <see cref="UniverseBase{TCoordinates}"/> types.
    /// </summary>
    public interface IUniverse
    {
        /// <summary>
        /// The map file this world was loaded from.
        /// </summary>
        /// <remarks>Is not serialized/stored, is set by whatever method loads the universe.</remarks>
        [XmlIgnore, Browsable(false)]
        string SourceFile { get; set; }

        /// <summary>
        /// Updates the <see cref="UniverseBase{TCoordinates}"/> and all <see cref="Positionable{TCoordinates}"/>s in it.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
        void Update(double elapsedTime);

        /// <summary>
        /// Saves this <see cref="UniverseBase{TCoordinates}"/> in a compressed XML file (map file).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        void Save(string path);

        /// <summary>
        /// Overwrites the map file this <see cref="UniverseBase{TCoordinates}"/> was loaded from with the changed data.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        void Save();
    }
}
