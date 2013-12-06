/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Common.Utils;
using SlimDX;

namespace World
{
    /// <summary>
    /// This class represents a complete game world (but not a running game).
    /// It is equivalent to the content of a map file.
    /// </summary>
    public sealed partial class Universe
    {
        #region Events
        /// <summary>
        /// Occurs when <see cref="Skybox"/> was changed.
        /// </summary>
        [Description("Occurs when Skybox was changed")]
        public event Action SkyboxChanged;
        #endregion

        #region Properties
        private readonly PositionableCollection _positionables = new PositionableCollection();

        /// <summary>
        /// A collection of all <see cref="Positionable"/>s in this <see cref="Universe"/>.
        /// </summary>
        [Browsable(false)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        [XmlElement(typeof(Entity)), XmlElement(typeof(Water)), XmlElement(typeof(Waypoint)), XmlElement(typeof(BenchmarkPoint)), XmlElement(typeof(Memo))]
        public PositionableCollection Positionables { get { return _positionables; } }

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
        public CameraState Camera { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public Universe()
        {
            LightPhaseSpeedFactor = 1;
        }

        /// <summary>
        /// Creates a new <see cref="Universe"/> with a terrain.
        /// </summary>
        /// <param name="terrain">The terrain for the new <see cref="Universe"/>.</param>
        public Universe(Terrain terrain) : this()
        {
            _terrain = terrain;
        }
        #endregion

        //--------------------//

        #region Path finding
        /// <summary>
        /// Moves an <see cref="Entity"/> to a new position using basic pathfinding.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be moved.</param>
        /// <param name="target">The terrain position to move <paramref name="entity"/> to.</param>
        /// <remarks>The actual movement occurs whenever <see cref="Update"/> is called.</remarks>
        public void MoveEntity(Entity entity, Vector2 target)
        {
            #region Sanity checks
            if (entity == null) throw new ArgumentNullException("entity");
            #endregion

            // Get path and cancel if none was found
            var pathNodes = Terrain.Pathfinder.FindPathPlayer(entity.Position * (1.0f / Terrain.Size.StretchH), target * (1.0f / Terrain.Size.StretchH));
            if (pathNodes == null)
            {
                entity.PathControl = null;
                return;
            }

            // Store path data in entity
            var pathLeader = new PathLeader {ID = 0, Target = target};
            foreach (Vector2 node in pathNodes)
                pathLeader.PathNodes.Push(node * Terrain.Size.StretchH);
            entity.PathControl = pathLeader;
        }

        /// <summary>
        /// Recalculates all paths stored in <see cref="Entity.PathControl"/>.
        /// </summary>
        /// <remarks>This needs to be called when new obstacles have appeared or when a savegame was loaded (which does not store paths).</remarks>
        internal void RecalcPaths()
        {
            foreach (Entity entity in Positionables.Entities)
            {
                var pathLeader = entity.PathControl as PathLeader;
                if (pathLeader != null)
                    MoveEntity(entity, pathLeader.Target);
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates the <see cref="Universe"/> and all <see cref="Positionable"/>s in it.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
        /// <remarks>This is usually called by <see cref="Session.Update"/>.</remarks>
        internal void Update(double elapsedTime)
        {
            LightPhase += (float)(elapsedTime / 40 * LightPhaseSpeedFactor);

            foreach (Entity entity in Positionables.Entities)
                entity.UpdatePosition(elapsedTime);
        }
        #endregion
    }
}
