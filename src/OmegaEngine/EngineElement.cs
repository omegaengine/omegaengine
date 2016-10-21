/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using NanoByte.Common;
using OmegaEngine.Properties;

namespace OmegaEngine
{
    /// <summary>
    /// A common base class for all objects that need an <see cref="Engine"/> instance.
    /// </summary>
    public abstract class EngineElement : IDisposable
    {
        #region Children
        /// <summary>
        /// Registers a child <see cref="EngineElement"/> for automatic <see cref="Engine"/> setting and <see cref="Dispose"/> calling.
        /// </summary>
        /// <param name="element">The <see cref="EngineElement"/> to register. Silently ignores <c>null</c>.</param>
        /// <param name="autoDispose">Controls whether the <paramref name="element"/> is automatically disposed when <see cref="Dispose"/> is called.</param>
        protected void RegisterChild(EngineElement element, bool autoDispose = true)
        {
            if (element == null) return;

            if (autoDispose) _toDispose.Add(element);
            if (IsEngineSet) element.Engine = Engine;
            _toSetEngine.Add(element);
        }

        /// <summary>
        /// Unregisters a child <see cref="EngineElement"/> (opposite of <see cref="RegisterChild"/>).
        /// </summary>
        /// <param name="element">The <see cref="EngineElement"/> to unregister. Silently ignores <c>null</c>.</param>
        protected void UnregisterChild(EngineElement element)
        {
            if (element == null) return;

            _toSetEngine.Remove(element);
            _toDispose.Remove(element);
        }
        #endregion

        #region Engine
        private Engine _engine;
        private readonly List<EngineElement> _toSetEngine = new List<EngineElement>();

        /// <summary>
        /// The <see cref="Engine"/> instance used by this object. Must be set before using the object. May not be changed once it has been set!
        /// </summary>
        /// <exception cref="InvalidOperationException">Trying to read the engine before it has been set.</exception>
        [Browsable(false)]
        public Engine Engine
        {
            get
            {
                if (!IsEngineSet) throw new InvalidOperationException(Resources.EngineNotSetYet);
                if (IsDisposed) throw new ObjectDisposedException(ToString());
                return _engine;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (IsDisposed) throw new ObjectDisposedException(ToString());

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
        /// <c>true</c> if the <see cref="Engine"/> has been set.
        /// </summary>
        [Browsable(false)]
        public bool IsEngineSet => _engine != null;

        /// <summary>
        /// Hook that is calld when <see cref="Engine"/> is set for the first time.
        /// </summary>
        protected virtual void OnEngineSet()
        {
            foreach (var element in _toSetEngine)
                element.Engine = Engine;
        }
        #endregion

        #region Dispose
        private readonly List<EngineElement> _toDispose = new List<EngineElement>();

        /// <summary>
        /// Indicates whether this object has been disposed and can therefore no longer be used.
        /// </summary>
        [Browsable(false)]
        public bool IsDisposed { get; private set; }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Using an alternative OnDispose() pattern")]
        public void Dispose()
        {
            if (IsDisposed) return;
            if (IsEngineSet && !_engine.IsDisposed)
                OnDispose();
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        /// Hook that is called when the object needs to dispose its internal resources.
        /// </summary>
        protected virtual void OnDispose()
        {
            foreach (var element in _toDispose)
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
