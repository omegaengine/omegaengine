/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using World;

namespace AlphaEditor.World.Commands
{
    /// <summary>
    /// Loads new texture-map data into a <see cref="Terrain"/>.
    /// </summary>
    internal class ImportTextureMap : ImportMap
    {
        #region Constructor
        /// <summary>
        /// Creates a new command for loading texture-map data into a <see cref="Terrain"/>.
        /// </summary>
        /// <param name="terrain">The <see cref="Terrain"/> to load new texture-map data into.</param>
        /// <param name="fileName">The file to load the texture-map data from.</param>
        /// <param name="refreshHandler">Called when the <see cref="Terrain"/> needs to be reset.</param>
        public ImportTextureMap(Terrain terrain, string fileName, Action refreshHandler) : base(terrain, fileName, refreshHandler)
        {}
        #endregion

        //--------------------//

        #region Terrain access
        /// <summary>
        /// Points to <see cref="Terrain.TextureMap"/>
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This property provides direct access to the underlying array without any cloning involved")]
        protected override byte[,] MapData { get { return _terrain.TextureMap; } set { _terrain.TextureMap = value; } }

        /// <summary>
        /// Loads the texture-map data from a file into the <see cref="Terrain"/>
        /// </summary>
        protected override void LoadMap()
        {
            _terrain.LoadTextureMap(_fileName);
        }
        #endregion
    }
}
