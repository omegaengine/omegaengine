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

using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;
using Common.Storage;

namespace OmegaGUI.Model
{
    /// <summary>
    /// A wrapper around an <see cref="XmlDictionary"/> used to store localized strings.
    /// </summary>
    [XmlRoot("locale")]
    public sealed class LocaleFile
    {
        #region Constants
        /// <summary>
        /// The file extensions when this class is stored as a file.
        /// </summary>
        public const string FileExt = ".locale";
        #endregion

        #region Properties
        private readonly XmlDictionary _entries;

        /// <summary>
        /// The collection of entries to be stored.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [XmlElement("entry")]
        public XmlDictionary Entries { get { return _entries; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public LocaleFile()
        {
            _entries = new XmlDictionary();
        }

        /// <summary>
        /// Creates a new wrapper for a collection.
        /// </summary>
        /// <param name="entries">The collection of entries to be stored.</param>
        private LocaleFile(XmlDictionary entries)
        {
            _entries = entries;
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a localization table from an XML file via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the file to load from.</param>
        /// <returns>>The loaded table.</returns>
        public static XmlDictionary FromContent(string id)
        {
            using (var stream = ContentManager.GetFileStream("GUI/Language", id))
                return XmlStorage.Load<LocaleFile>(stream).Entries;
        }

        /// <summary>
        /// Loads a localization table from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded table.</returns>
        public static XmlDictionary Load(string path)
        {
            return XmlStorage.Load<LocaleFile>(path).Entries;
        }

        /// <summary>
        /// Loads a localization table from an XML file if possible.
        /// </summary>
        /// <param name="language">The language to load.</param>
        /// <returns>The loaded table or <see langword="null"/> if not found.</returns>
        public static XmlDictionary LoadLang(string language)
        {
            if (ContentManager.FileExists("GUI/Language", language + FileExt, true))
                return FromContent(language + FileExt);
            if (ContentManager.FileExists("GUI/Language", "English" + FileExt, true))
                return FromContent("English" + FileExt);
            return ContentManager.FileExists("GUI/Language", "German" + FileExt, true) ?
                                                                                           FromContent("German" + FileExt) : null;
        }

        /// <summary>
        /// Saves a localization table in an XML file.
        /// </summary>
        /// <param name="path">The file to save in</param>
        /// <param name="entries">The collection of entries to be stored.</param>
        public static void Save(string path, XmlDictionary entries)
        {
            XmlStorage.Save(path, new LocaleFile(entries));
        }
        #endregion
    }
}
