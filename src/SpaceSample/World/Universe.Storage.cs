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
using System.Reflection;
using System.Xml.Serialization;
using Common;
using Common.Storage;
using Core;

namespace World
{
    partial class Universe
    {
        #region Constants
        /// <summary>
        /// The file extensions when this class is stored as a file.
        /// </summary>
        public const string FileExt = "." + GeneralSettings.AppNameShort + "Map";
        #endregion

        #region Variables
        /// <summary>
        /// Don't save or load the <see cref="Entity.TemplateData"/> in map files - that's only sensible in savegames.
        /// Instead <see cref="Entity.TemplateName"/> is used.
        /// </summary>
        private static readonly MemberInfo _ignoreMemeber = typeof(Entity).GetProperty("TemplateData");
        #endregion

        #region Properties
        /// <summary>
        /// The map file this world was loaded from.
        /// </summary>
        /// <remarks>Is not serialized/stored, is set by <see cref="FromContent"/>, <see cref="Session.Load"/> or whatever method loads <see cref="Universe"/>.</remarks>
        [XmlIgnore, Browsable(false)]
        public string SourceFile { get; set; }
        #endregion

        //--------------------//

        #region Content
        /// <summary>
        /// Loads a <see cref="Universe"/> from the game content source via the <see cref="ContentManager"/>.
        /// </summary>
        /// <param name="id">The ID of the <see cref="Universe"/> to load.</param>
        /// <returns>The loaded <see cref="Universe"/>.</returns>
        public static Universe FromContent(string id)
        {
            Log.Info("Loading map: " + id);

            Universe universe;
            using (var stream = ContentManager.GetFileStream("World/Maps", id))
                universe = XmlStorage.FromZip<Universe>(stream, null, null, _ignoreMemeber);
            universe.SourceFile = id;
            universe.Update(0);

            return universe;
        }
        #endregion

        #region Load
        /// <summary>
        /// Loads a <see cref="Universe"/> from a compressed XML file (map file).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Universe"/>.</returns>
        public static Universe Load(string path)
        {
            // Load the file
            var universe = XmlStorage.FromZip<Universe>(path, null, null, _ignoreMemeber);

            // Store the orginal map filename
            universe.SourceFile = path;

            // Perform updates to initialize basic data
            universe.Update(0);

            return universe;
        }

        /// <summary>
        /// Loads a <see cref="Universe"/> from an uncompressed XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Universe"/>.</returns>
        public static Universe LoadXml(string path)
        {
            return XmlStorage.Load<Universe>(path, _ignoreMemeber);
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves this <see cref="Universe"/> in a compressed XML file (map file).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        public void Save(string path)
        {
            // Save the data
            XmlStorage.ToZip(path, this, null, null, _ignoreMemeber);

            SourceFile = path;
        }

        /// <summary>
        /// Overwrites the map file this <see cref="Universe"/> was loaded from with the changed data.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save()
        {
            // Determine the original filename to overweite
            Save(Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile));
        }

        /// <summary>
        /// Saves this <see cref="Universe"/> in an uncompressed XML file.
        /// </summary>
        /// <param name="path">The file to save in</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void SaveXml(string path)
        {
            XmlStorage.Save(path, this, _ignoreMemeber);
        }
        #endregion
    }
}
