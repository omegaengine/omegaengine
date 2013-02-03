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
using Common.Storage;
using ICSharpCode.SharpZipLib.Zip;
using LuaInterface;
using Core;

namespace World
{
    /// <summary>
    /// Represents a game session (i.e. a game actually being played).
    /// It is equivalent to the content of a savegame.
    /// </summary>
    public sealed class Session
    {
        #region Constants
        /// <summary>
        /// The file extensions when this class is stored as a file.
        /// </summary>
        public const string FileExt = "." + GeneralSettings.AppNameShort + "Save";

        /// <summary>
        /// Used for encrypting serialized versions of this class.
        /// </summary>
        /// <remarks>This provides only very basic protection against savegame tampering.</remarks>
        private const string EncryptionKey = "Session";
        #endregion

        #region Properties
        /// <summary>
        /// The current state of the game world.
        /// </summary>
        [LuaHide]
        public Universe Universe { get; set; }

        /// <summary>
        /// The filename of the map file the <see cref="Universe"/> was loaded from.
        /// </summary>
        public string MapSourceFile { get; set; }

        /// <summary>
        /// Total elapsed real time in seconds.
        /// </summary>
        public double RealTime { get; set; }

        /// <summary>
        /// Total elapsed game time in seconds.
        /// </summary>
        public double GameTime { get; set; }

        /// <summary>
        /// The factor by which <see cref="GameTime"/> (not <see cref="RealTime"/>) progression should be multiplied.
        /// </summary>
        /// <remarks>This multiplication is not done in <see cref="Update"/>!</remarks>
        [DefaultValue(1f)]
        public float TimeWarpFactor { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public Session()
        {}

        /// <summary>
        /// Creates a new game session based upon a given <see cref="Universe"/>
        /// </summary>
        /// <param name="baseUniverse">The universe to base the new game session on</param>
        public Session(Universe baseUniverse) : this()
        {
            #region Sanity checks
            if (baseUniverse == null) throw new ArgumentNullException("baseUniverse");
            #endregion

            // Note: This path is only taken when creating a new session, loading a savegame does not go through here!

            Universe = baseUniverse;
            TimeWarpFactor = 1;

            // Transfer map name from universe to session, because it will persist there
            MapSourceFile = baseUniverse.SourceFile;
        }
        #endregion

        //--------------------//

        #region Update
        /// <summary>
        /// Updates the <see cref="Session"/>, the contained <see cref="Universe"/> and all <see cref="Positionable"/>s in it.
        /// </summary>
        /// <param name="elapsedRealTime">How much real time in seconds has elapsed since this method was last called.</param>
        /// <param name="elapsedGameTime">How much game time in seconds has elapsed since this method was last called.</param>
        /// <remarks>This needs to be called as a part of the render loop.</remarks>
        public void Update(double elapsedRealTime, double elapsedGameTime)
        {
            RealTime += elapsedRealTime;
            GameTime += elapsedGameTime;

            // ToDo: Add some game logic here

            // Split Universe update into multiple steps to increase accuracy
            while (elapsedGameTime > Settings.Current.General.UniversePredictSecs)
            {
                Universe.Update(Settings.Current.General.UniversePredictSecs);
                elapsedGameTime -= Settings.Current.General.UniversePredictSecs;
            }
            if (elapsedGameTime > 0) Universe.Update(elapsedGameTime);
        }
        #endregion

        #region Storage
        /// <summary>
        /// Loads a <see cref="Session"/> from a encrypted XML file (savegame).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Session"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurred while deserializing the XML data.</exception>
        public static Session Load(string path)
        {
            // Load the file
            Session session;
            try
            {
                session = XmlStorage.LoadXmlZip<Session>(path, EncryptionKey, null);
            }
                #region Error handling
            catch (ZipException ex)
            {
                throw new IOException(ex.Message, ex);
            }
            #endregion

            var universe = session.Universe;

            // Restore the orginal map filename
            universe.SourceFile = session.MapSourceFile;

            // Perform updates to regenerate data lost in the savegame
            universe.Update(0);
            universe.RecalcPaths();

            return session;
        }

        /// <summary>
        /// Saves this <see cref="Session"/> in an encrypted XML file (savegame).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurred while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            this.SaveXmlZip(path, EncryptionKey, null);
        }
        #endregion
    }
}
