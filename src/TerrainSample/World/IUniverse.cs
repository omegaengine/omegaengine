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
using System.IO;
using System.Xml.Serialization;
using TerrainSample.World.Positionables;

namespace TerrainSample.World
{
    /// <summary>
    /// A common base for all <see cref="Universe{TCoordinates}"/> types.
    /// </summary>
    public interface IUniverse
    {
        /// <summary>
        /// The map file this world was loaded from.
        /// </summary>
        /// <remarks>Is not serialized/stored, is set by whatever method loads the universe.</remarks>
        [XmlIgnore, Browsable(false)]
        string SourceFile { get; set; }

        /// <summary>
        /// Updates the <see cref="Universe{TCoordinates}"/> and all <see cref="Positionable{TCoordinates}"/>s in it.
        /// </summary>
        /// <param name="elapsedTime">How much game time in seconds has elapsed since this method was last called.</param>
        /// <remarks>This is usually called by <see cref="Session.Update"/>.</remarks>
        void Update(double elapsedTime);

        /// <summary>
        /// Recalculates all paths stored in <see cref="TerrainEntity.PathControl"/>.
        /// </summary>
        /// <remarks>This needs to be called when new obstacles have appeared or when a savegame was loaded (which does not store paths).</remarks>
        void RecalcPaths();

        /// <summary>
        /// Saves this <see cref="Universe{TCoordinates}"/> in a compressed XML file (map file).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        void Save(string path);

        /// <summary>
        /// Overwrites the map file this <see cref="Universe{TCoordinates}"/> was loaded from with the changed data.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        void Save();

        /// <summary>
        /// Saves this <see cref="Universe{TCoordinates}"/> in an uncompressed XML file.
        /// </summary>
        /// <param name="path">The file to save in</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        void SaveXml(string path);
    }
}
