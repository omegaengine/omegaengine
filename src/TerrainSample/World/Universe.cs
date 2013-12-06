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
using System.Reflection;
using System.Xml.Serialization;
using Common.Storage;
using Common.Utils;
using Core;
using World.Pathfinding;
using World.Positionables;

namespace World
{
    /// <summary>
    /// Represents a game world (but not a running game). It is equivalent to the content of a map file.
    /// </summary>
    /// <typeparam name="TCoordinates">Coordinate data type (2D, 3D, ...)</typeparam>
    public abstract class Universe<TCoordinates>
        where TCoordinates : struct
    {
        #region Constants
        /// <summary>
        /// The file extensions when this class is stored as a file.
        /// </summary>
        public const string FileExt = "." + GeneralSettings.AppNameShort + "Map";

        /// <summary>
        /// Don't save or load the <see cref="Entity{TCoordinates}.TemplateData"/> in map files - that's only sensible in savegames.
        /// Instead <see cref="Entity{TCoordinates}.TemplateName"/> is used.
        /// </summary>
        // ReSharper disable once StaticFieldInGenericType
        protected static readonly MemberInfo IgnoreMemeber = typeof(Entity<TCoordinates>).GetProperty("TemplateData");
        #endregion

        #region Events
        /// <summary>
        /// Occurs when <see cref="Skybox"/> was changed.
        /// </summary>
        [Description("Occurs when Skybox was changed")]
        public event Action SkyboxChanged;
        #endregion

        #region Properties
        /// <summary>
        /// A collection of all <see cref="Positionable{TCoordinates}"/>s in this <see cref="Universe{TCoordinates}"/>.
        /// </summary>
        // Note: Can not use ICollection<T> interface with XML Serialization
        [XmlIgnore]
        public abstract PositionableCollection<TCoordinates> Positionables { get; }

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

        /// <summary>
        /// The map file this world was loaded from.
        /// </summary>
        /// <remarks>Is not serialized/stored, is set by whatever method loads the <see cref="Universe{TCoordinates}"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        public string SourceFile { get; set; }
        #endregion

        //--------------------//

        #region Update
        /// <summary>
        /// Updates the <see cref="Universe{TCoordinates}"/> and all <see cref="Positionable{TCoordinates}"/>s in it.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
        /// <remarks>This is usually called by <see cref="Session.Update"/>.</remarks>
        internal virtual void Update(double elapsedTime)
        {
            foreach (var entity in Positionables.Entities)
                entity.UpdatePosition(elapsedTime);
        }
        #endregion

        #region Path finding
        /// <summary>
        /// Moves an <see cref="Entity{TCoordinates}"/> to a new position using pathfinding.
        /// </summary>
        /// <param name="entity">The <see cref="Entity{TCoordinates}"/> to be moved.</param>
        /// <param name="target">The terrain position to move <paramref name="entity"/> to.</param>
        /// <remarks>The actual movement occurs whenever <see cref="Update"/> is called.</remarks>
        public abstract void MoveEntity(Entity<TCoordinates> entity, TCoordinates target);

        /// <summary>
        /// Recalculates all paths stored in <see cref="Entity2D.PathControl"/>.
        /// </summary>
        /// <remarks>This needs to be called when new obstacles have appeared or when a savegame was loaded (which does not store paths).</remarks>
        internal void RecalcPaths()
        {
            foreach (var entity in Positionables.Entities)
            {
                var pathLeader = entity.PathControl as PathLeader<TCoordinates>;
                if (pathLeader != null)
                    MoveEntity(entity, pathLeader.Target);
            }
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Saves this <see cref="Universe{TCoordinates}"/> in a compressed XML file (map file).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public abstract void Save(string path);

        /// <summary>
        /// Overwrites the map file this <see cref="Universe{TCoordinates}"/> was loaded from with the changed data.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save()
        {
            // Determine the original filename to overweite
            Save(Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile));
        }

        /// <summary>
        /// Saves this <see cref="Universe{TCoordinates}"/> in an uncompressed XML file.
        /// </summary>
        /// <param name="path">The file to save in</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void SaveXml(string path)
        {
            this.SaveXml(path, IgnoreMemeber);
        }
        #endregion
    }
}
