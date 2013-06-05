/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Undo;

namespace Common.StructureEditor
{
    /// <summary>
    /// Provides an interface to a control that edits a single element.
    /// </summary>
    /// <typeparam name="T">The type of element to edit.</typeparam>
    public interface IEditorControl<T> : IDisposable where T : class
    {
        /// <summary>
        /// The element to be edited.
        /// </summary>
        T Element { get; set; }

        /// <summary>
        /// An optional undo system to use for editing.
        /// </summary>
        ICommandExecutor CommandExecutor { get; set; }
    }
}
