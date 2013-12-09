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
using World.Config;
using World.Pathfinding;
using World.Positionables;

namespace World
{
    /// <summary>
    /// Represents a game world (but not a running game). It is equivalent to the content of a map file.
    /// </summary>
    /// <typeparam name="TCoordinates">Coordinate data type (2D, 3D, ...)</typeparam>
    public abstract class Universe<TCoordinates> : IUniverse
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
        [Browsable(false)]
        [XmlIgnore] // XML serialization configuration is configured in sub-type
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

        /// <inheritdoc/>
        [XmlIgnore, Browsable(false)]
        public string SourceFile { get; set; }
        #endregion

        //--------------------//

        #region Update
        /// <inheritdoc/>
        public virtual void Update(double elapsedTime)
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

        /// <inheritdoc/>
        public void RecalcPaths()
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
        /// <inheritdoc/>
        public abstract void Save(string path);

        /// <inheritdoc/>
        public void Save()
        {
            // Determine the original filename to overweite
            Save(Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile));
        }

        /// <inheritdoc/>
        public void SaveXml(string path)
        {
            this.SaveXml(path, IgnoreMemeber);
        }
        #endregion
    }
}
