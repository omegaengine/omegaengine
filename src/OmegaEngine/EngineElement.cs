/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using Common;

namespace OmegaEngine
{
    /// <summary>
    /// A common base class for all objects that need an <see cref="Engine"/> instance.
    /// </summary>
    public abstract class EngineElement : IDisposable
    {
        #region Engine
        private Engine _engine;

        /// <summary>
        /// The <see cref="Engine"/> instance used by this object. Must be set before using the object. May not be changed once it has been set!
        /// </summary>
        public Engine Engine
        {
            get { return _engine; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (Disposed) throw new ObjectDisposedException("EngineElement");

                if (_engine != null)
                {
                    if (value == _engine) return;
                    else throw new InvalidOperationException("Engine can not be changed again once it has been set!");
                }

                _engine = value;
                OnEngineSet();
            }
        }

        /// <summary>
        /// Hook that is calld when <see cref="Engine"/> is set for the first time.
        /// </summary>
        protected virtual void OnEngineSet()
        {}
        #endregion

        #region Dispose
        /// <summary>
        /// Indicates whether this object has been disposed and can therefore no longer be used.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Disposed) return;

            if (_engine != null && !_engine.Disposed)
            {
                Log.Info("Disposing " + this);
                OnDispose();
            }

            Disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Hook that is called when the object needs to dispose its internal resources.
        /// </summary>
        protected virtual void OnDispose()
        {}

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
