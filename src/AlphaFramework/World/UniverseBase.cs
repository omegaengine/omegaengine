/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        #region Events
        /// <summary>
        /// Occurs when <see cref="Skybox"/> was changed.
        /// </summary>
        [Description("Occurs when Skybox was changed")]
        public event Action SkyboxChanged;
        #endregion

        #region Properties
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
        /// The position and direction of the camera in the game.
        /// </summary>
        /// <remarks>This is updated only when leaving the game, not continuously.</remarks>
        [Browsable(false)]
        public CameraState<TCoordinates> Camera { get; set; }

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        public string SourceFile { get; set; }
        #endregion

        //--------------------//

        #region Update
        /// <inheritdoc/>
        public virtual void Update(double elapsedTime)
        {
            foreach (var entity in Positionables.OfType<IUpdateable>())
                entity.Update(elapsedTime);
        }
        #endregion

        //--------------------//

        #region Storage
        /// <inheritdoc/>
        public abstract void Save(string path);

        /// <inheritdoc/>
        public void Save()
        {
            // Determine the original filename to overweite
            Save(Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile));
        }
        #endregion
    }
}
