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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;
using World.Properties;

namespace World.Positionables
{
    /// <summary>
    /// Stores a collection of <see cref="Positionable{TCoordinates}"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    public class PositionableCollection<TCoordinates> : MonitoredCollection<Positionable<TCoordinates>>
        where TCoordinates : struct
    {
        #region Variables
        // Preserver order, duplicate entries are not allowed
        private readonly IList<Entity<TCoordinates>> _entities = new C5.HashedLinkedList<Entity<TCoordinates>>();
        #endregion

        #region Properties
        private readonly ReadOnlyCollection<Entity<TCoordinates>> _entitiesReadonly;

        /// <summary>
        /// A read-only sub-collection of all <see cref="Entity{TCoordinates}"/>s within this collection.
        /// </summary>
        /// <remarks>When <see cref="MonitoredCollection{T}.Removing"/> is raised corresponding <see cref="Entity{TCoordinates}"/>s have already been removed from this list.</remarks>
        [XmlIgnore]
        public ICollection<Entity<TCoordinates>> Entities { get { return _entitiesReadonly; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="Positionable{TCoordinates}"/> collection
        /// </summary>
        public PositionableCollection()
        {
            // Create a read-only wrapper to prevent outside modification
            _entitiesReadonly = new ReadOnlyCollection<Entity<TCoordinates>>(_entities);
        }
        #endregion

        //--------------------//

        #region Hooks
        /// <inheritdoc />
        protected override void InsertItem(int index, Positionable<TCoordinates> item)
        {
            // Make sure we don't track operations that are bound to fail
            if (MaxElements == 0 || Count != MaxElements)
            {
                // Keep track of bodies in an additional sub-collection
                var entity = item as Entity<TCoordinates>;
                if (entity != null) _entities.Add(entity);
            }

            base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            // Keep track of bodies in an additional sub-collection
            var entity = Items[index] as Entity<TCoordinates>;
            if (entity != null) _entities.Remove(entity);

            base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            // Keep track of bodies in an additional sub-collection
            _entities.Clear();

            base.ClearItems();
        }
        #endregion

        #region Access
        /// <summary>
        /// Gets the <see cref="Positionable{TCoordinates}"/> with the specified <paramref name="name"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">An element with the specified key does not exist in the dictionary.</exception>
        public Positionable<TCoordinates> this[string name]
        {
            get
            {
                #region Sanity checks
                if (name == null) throw new ArgumentNullException("name");
                #endregion

                return this.First(positionable => positionable.Name == name,
                    noneException: () => new KeyNotFoundException(Resources.EntryNotFound));
            }
        }
        #endregion
    }
}