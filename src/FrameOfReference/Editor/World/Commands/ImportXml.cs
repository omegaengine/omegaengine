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
using AlphaFramework.Editor.World.Commands;
using FrameOfReference.World;

namespace FrameOfReference.Editor.World.Commands
{
    /// <summary>
    /// Loads new XML data into a <see cref="Universe"/>.
    /// </summary>
    public class ImportXml : ImportXmlBase<Universe>
    {
        /// <summary>
        /// Creates a new command for loading XML data into a <see cref="Universe"/>.
        /// </summary>
        /// <param name="getUniverse">Called to get the current <see cref="Universe"/> in the editor.</param>
        /// <param name="setUniverse">Called to change the current <see cref="Universe"/> in the editor.</param>
        /// <param name="xmlData">The XML string to parse.</param>
        /// <param name="refreshHandler">Called when the presenter needs to be reset.</param>
        public ImportXml(Func<Universe> getUniverse, Action<Universe> setUniverse, string xmlData, Action refreshHandler)
            : base(getUniverse, setUniverse, xmlData, refreshHandler)
        {}

        /// <inheritdoc/>
        protected override void TransferNonXmlData(Universe oldUniverse, Universe newUniverse)
        {
            #region Sanity checks
            if (oldUniverse == null) throw new ArgumentNullException("oldUniverse");
            if (newUniverse == null) throw new ArgumentNullException("newUniverse");
            #endregion

            newUniverse.Terrain.OcclusionIntervalMapOutdated = oldUniverse.Terrain.OcclusionIntervalMapOutdated = true;
            newUniverse.Terrain.HeightMap = oldUniverse.Terrain.HeightMap;
            newUniverse.Terrain.TextureMap = oldUniverse.Terrain.TextureMap;
        }
    }
}
