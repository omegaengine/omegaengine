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

using System.Xml.Serialization;
using Common.Collections;
using Common.Storage;

namespace TerrainSample.World.Templates
{
    /// <summary>
    /// A string-keyed dictionary of <see cref="Template{T}"/>es that can be XML serialized.
    /// </summary>
    [XmlType("TemplateList")]
    public class TemplateCollection<T> : NamedCollection<T> where T : Template<T>
    {
        #region Storage
        /// <summary>
        /// Loads a <see cref="TemplateCollection{T}"/> from an XML file via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the file to load from.</param>
        /// <returns>The loaded <see cref="TemplateCollection{T}"/>.</returns>
        public static TemplateCollection<T> FromContent(string id)
        {
            using (var stream = ContentManager.GetFileStream("World", id))
                return XmlStorage.LoadXml<TemplateCollection<T>>(stream);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override NamedCollection<T> Clone()
        {
            var newCollection = new TemplateCollection<T>();
            foreach (T entry in this)
                newCollection.Add(entry.Clone());

            return newCollection;
        }
        #endregion
    }
}
