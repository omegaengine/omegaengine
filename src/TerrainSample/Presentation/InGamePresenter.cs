/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Collections.Generic;
using System.Linq;
using AlphaFramework.World.Positionables;
using Common;
using OmegaEngine;
using OmegaEngine.Graphics;
using SlimDX;
using TerrainSample.World;
using TerrainSample.World.Positionables;

namespace TerrainSample.Presentation
{
    /// <summary>
    /// Main in-game interaction
    /// </summary>
    public sealed class InGamePresenter : InteractivePresenter
    {
        /// <summary>
        /// Creates a new presenter for the actual running game
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The universe to display</param>
        public InGamePresenter(Engine engine, Universe universe) : base(engine, universe)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException("engine");
            if (universe == null) throw new ArgumentNullException("universe");
            #endregion

            // Restore previous camera position (or default to center of terrain)
            var mainCamera = CreateCamera(universe.Camera);

            View = new View(Scene, mainCamera) {Name = "InGame", BackgroundColor = universe.FogColor};
        }

        /// <inheritdoc/>
        public override void HookIn()
        {
            base.HookIn();

            SwitchMusicTheme("Game", immediate: true);
        }

        /// <inheritdoc/>
        public override void HookOut()
        {
            PrepareSave();

            base.HookOut();
        }

        /// <summary>
        /// Writes back data to <see cref="Universe"/> so that state gets stored in savegames.
        /// </summary>
        public void PrepareSave()
        {
            Universe.Camera = CameraState;
        }

        /// <inheritdoc/>
        protected override void MovePositionables(IEnumerable<Positionable<Vector2>> positionables, Vector2 target)
        {
            #region Sanity checks
            if (positionables == null) throw new ArgumentNullException("positionables");
            #endregion

            if (positionables.OfType<Entity>().Contains(Universe.PlayerEntity))
                Universe.PathfindEntity(Universe.PlayerEntity, target);
        }
    }
}
