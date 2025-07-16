/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AlphaFramework.World;
using AlphaFramework.World.Positionables;
using LuaInterface;
using NanoByte.Common;
using NanoByte.Common.Dispatch;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using LightSource = OmegaEngine.Graphics.LightSource;

namespace AlphaFramework.Presentation
{
    /// <summary>
    /// Uses the <see cref="Engine"/> to present an <see cref="IUniverse"/> game world.
    /// </summary>
    /// <typeparam name="TUniverse">The type of <see cref="IUniverse"/> to present.</typeparam>
    /// <typeparam name="TCoordinates">Data type for storing position coordinates of objects in the game world.</typeparam>
    public abstract class PresenterBase<TUniverse, TCoordinates> : IDisposable
        where TUniverse : UniverseBase<TCoordinates>
        where TCoordinates : struct
    {
        /// <summary>
        /// Creates a new presenter.
        /// </summary>
        /// <param name="engine">The engine to use for rendering.</param>
        /// <param name="universe">The game world to present.</param>
        protected PresenterBase(Engine engine, TUniverse universe)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            if (universe == null) throw new ArgumentNullException(nameof(universe));
            #endregion

            Engine = engine;
            Universe = universe;

            Scene = new();
            RenderablesSync = new(Universe.Positionables, Scene.Positionables);
            LightsSync = new(Universe.Positionables, Scene.Lights);
        }

        #region Model-View
        /// <summary>
        /// Maps between <see cref="UniverseBase{TCoordinates}.Positionables"/> and <see cref="OmegaEngine.Graphics.Scene.Positionables"/>.
        /// </summary>
        protected readonly ModelViewSync<Positionable<TCoordinates>, PositionableRenderable> RenderablesSync;

        /// <summary>
        /// Maps between <see cref="UniverseBase{TCoordinates}.Positionables"/> and <see cref="OmegaEngine.Graphics.Scene.Lights"/>.
        /// </summary>
        protected readonly ModelViewSync<Positionable<TCoordinates>, LightSource> LightsSync;

        /// <summary>
        /// The <see cref="Engine"/> reference to use for rendering operations
        /// </summary>
        protected readonly Engine Engine;

        /// <summary>
        /// The engine view used to display the <see cref="Scene"/>
        /// </summary>
        public View View { get; protected set; }

        /// <summary>
        /// The engine scene containing the graphical representations of <see cref="Positionable{TCoordinates}"/>s
        /// </summary>
        protected readonly Scene Scene;

        /// <summary>
        /// The game world to present.
        /// </summary>
        [LuaHide]
        public TUniverse Universe { get; }
        #endregion

        #region Initialize
        /// <summary>
        /// Was <see cref="Initialize"/> already called?
        /// </summary>
        public bool Initialized { get; protected set; }

        /// <summary>
        /// Generate <see cref="Terrain"/> and <see cref="Renderable"/>s from <see cref="UniverseBase{TCoordinates}.Positionables"/> and keeps everything in sync using events
        /// </summary>
        /// <exception cref="FileNotFoundException">A required <see cref="Asset"/> file could not be found.</exception>
        /// <exception cref="IOException">There was an error reading an <see cref="Asset"/> file.</exception>
        /// <exception cref="InvalidDataException">An <see cref="Asset"/> file contains invalid data.</exception>
        /// <remarks>Should be called before <see cref="HookIn"/> is used</remarks>
        public virtual void Initialize()
        {
            RegisterRenderablesSync();
            RenderablesSync.Initialize();
            LightsSync.Initialize();

            Initialized = true;
        }

        /// <summary>
        /// Hook to configure <see cref="RenderablesSync"/> and <see cref="LightsSync"/>.
        /// </summary>
        protected virtual void RegisterRenderablesSync()
        {}
        #endregion

        #region Engine hook-in
        /// <summary>
        /// Hooks the <see cref="View"/> into <see cref="OmegaEngine.Engine.Views"/>
        /// </summary>
        /// <remarks>Will internally call <see cref="Initialize"/> first, if you didn't</remarks>
        public virtual void HookIn()
        {
            if (Disposed) throw new ObjectDisposedException(ToString());
            if (!Initialized) Initialize();

            Engine.Views.Add(View);
        }

        /// <summary>
        /// Hooks the <see cref="View"/> out of <see cref="OmegaEngine.Engine.Views"/>
        /// </summary>
        public virtual void HookOut()
        {
            Engine.Views.Remove(View);
        }
        #endregion

        #region Dimming
        /// <summary>
        /// Dims in the screen down.
        /// </summary>
        public virtual void DimDown()
        {
            Engine.DimDown();
        }

        /// <summary>
        /// Dims in the screen back up.
        /// </summary>
        public virtual void DimUp()
        {
            Engine.DimUp();
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <summary>
        /// Was this presenter already disposed?
        /// </summary>
        [Browsable(false)]
        public bool Disposed { get; private set; }

        /// <summary>
        /// Removes the <see cref="Universe"/> hooks setup by <see cref="Initialize"/> and disposes all created <see cref="View"/>s, <see cref="Scene"/>s, <see cref="PositionableRenderable"/>s, etc.
        /// </summary>
        public void Dispose()
        {
            if (Disposed) return;
            Dispose(true);
            GC.SuppressFinalize(this);
            Disposed = true;
        }

        /// <inheritdoc/>
        ~PresenterBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// To be called by <see cref="IDisposable.Dispose"/> and the object destructor.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called manually and not by the garbage collector.</param>
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            { // This block will only be executed on manual disposal, not by Garbage Collection
                Log.Info("Disposing presenter");

                RenderablesSync.Dispose();
                LightsSync.Dispose();

                Scene?.Dispose();
                View?.Dispose();
            }
            else
            { // This block will only be executed on Garbage Collection, not by manual disposal
                Log.Error("Forgot to call Dispose on " + this);
#if DEBUG
                throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
            }
        }
        #endregion
    }
}
