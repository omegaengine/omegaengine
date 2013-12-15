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
using Common.Utils;

namespace TerrainSample.World.Config
{
    /// <summary>
    /// Stores general game settings (UI language, difficulty level, etc.).
    /// </summary>
    /// <seealso cref="Settings.General"/>
    public sealed class GeneralSettings
    {
        #region Constants
        /// <summary>
        /// The complete name of the application
        /// </summary>
        public const string AppName = "Terrain Sample Game";

        /// <summary>
        /// The name of the application without whitespaces (used for AppModel IDs, etc.)
        /// </summary>
        public const string AppNameGrid = "TerrainSampleGame";

        /// <summary>
        /// The short version of the application name (as used by the EXE name)
        /// </summary>
        public const string AppNameShort = "TerrainSample";
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a setting in this group is changed.
        /// </summary>
        [Description("Occurs when a setting in this group is changed.")]
        public event Action Changed;

        private void OnChanged()
        {
            if (Changed != null) Changed();
            if (Settings.AutoSave && Settings.Current != null && Settings.Current.General == this) Settings.SaveCurrent();
        }
        #endregion

        private string _contentDir;

        /// <summary>
        /// Path to the directory to load the game content from
        /// </summary>
        [DefaultValue(""), Description("Path to the directory to load the game content from")]
        public string ContentDir { get { return _contentDir; } set { value.To(ref _contentDir, OnChanged); } }

        private string _language;

        /// <summary>
        /// The current game language
        /// </summary>
        [DefaultValue(""), Description("The current game language")]
        public string Language { get { return _language; } set { value.To(ref _language, OnChanged); } }

        private int _universePredictSecs = 5;

        /// <summary>
        /// How many seconds at a time a universe can be predicted - higher value = better performance, lower value = better accuracy
        /// </summary>
        [DefaultValue(5), Description("How many seconds at a time a universe can be predicted - higher value = better performance, lower value = better accuracy")]
        public int UniversePredictSecs { get { return _universePredictSecs; } set { value.To(ref _universePredictSecs, OnChanged); } }
    }
}