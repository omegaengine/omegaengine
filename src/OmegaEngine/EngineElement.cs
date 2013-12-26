/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common;
using OmegaEngine.Properties;

namespace OmegaEngine
{
    /// <summary>
    /// A common base class for all objects that need an <see cref="Engine"/> instance.
    /// </summary>
    public abstract class EngineElement : IDisposable
    {
        #region Children
        private readonly List<EngineElement> _children = new List<EngineElement>();

        /// <summary>
        /// Registers a child <see cref="EngineElement"/> for automatic <see cref="Engine"/> setting and <see cref="Dispose"/> calling.
        /// </summary>
        /// <param name="element">The <see cref="EngineElement"/> to register. Silently ignores <see langword="null"/>.</param>
        protected void RegisterChild(EngineElement element)
        {
            if (element == null) return;

            if (IsEngineSet) element.Engine = Engine;
            _children.Add(element);
        }

        /// <summary>
        /// Unregisters a child <see cref="EngineElement"/> (opposite of <see cref="RegisterChild"/>).
        /// </summary>
        /// <param name="element">The <see cref="EngineElement"/> to unregister. Silently ignores <see langword="null"/>.</param>
        protected void UnregisterChild(EngineElement element)
        {
            if (element == null) return;

            _children.Remove(element);
        }
        #endregion

        #region Engine
        private Engine _engine;

        /// <summary>
        /// The <see cref="Engine"/> instance used by this object. Must be set before using the object. May not be changed once it has been set!
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when trying to read the engine before it has been set.</exception>
        public Engine Engine
        {
            get
            {
                if (!IsEngineSet) throw new InvalidOperationException(Resources.EngineNotSetYet);
                return _engine;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (Disposed) throw new ObjectDisposedException("EngineElement");

                if (IsEngineSet)
                {
                    if (value == _engine) return;
                    else throw new InvalidOperationException(Resources.EngineCannotChange);
                }

                _engine = value;
                OnEngineSet();
            }
        }

        /// <summary>
        /// <see langword="true"/> if the <see cref="Engine"/> has been set.
        /// </summary>
        public bool IsEngineSet { get { return _engine != null; } }

        /// <summary>
        /// Hook that is calld when <see cref="Engine"/> is set for the first time.
        /// </summary>
        protected virtual void OnEngineSet()
        {
            foreach (var element in _children)
                element.Engine = Engine;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Indicates whether this object has been disposed and can therefore no longer be used.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Using an alternative OnDispose() pattern")]
        public void Dispose()
        {
            if (Disposed) return;

            if (IsEngineSet && !_engine.Disposed)
                OnDispose();

            Disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Hook that is called when the object needs to dispose its internal resources.
        /// </summary>
        protected virtual void OnDispose()
        {
            foreach (var element in _children)
                element.Dispose();
        }

        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Using an alternative OnDispose() pattern")]
        [SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations", Justification = "Only for debugging, not present in Release code")]
        ~EngineElement()
        {
            // This block will only be executed on Garbage Collection, not by manual disposal
            Log.Error("Forgot to call Dispose on " + this);

#if DEBUG
            throw new InvalidOperationException("Forgot to call Dispose on " + this);
#endif
        }
        #endregion
    }
}
