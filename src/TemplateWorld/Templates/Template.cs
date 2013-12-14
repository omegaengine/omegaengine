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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Xml.Serialization;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Storage;
using TemplateWorld.Properties;

namespace TemplateWorld.Templates
{
    /// <summary>
    /// A set of data used as a prototype for constructing new objects at runtime.
    /// </summary>
    /// <typeparam name="TSelf">The type of the class itself.</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for INamed sorting")]
    public abstract class Template<TSelf> : INamed<TSelf>, IHighlightColor, ICloneable where TSelf : Template<TSelf>
    {
        /// <summary>
        /// The name of this class. Used in map files as a reference. Must be unique and is case-sensitive!
        /// </summary>
        // Mark as read only for PropertyGrid, since direct renames would confuse NamedCollection<T>
        [XmlAttribute, ReadOnly(true), Description("The name of this class. Used in map files as a reference. Must be unique and is case-sensitive!")]
        public string Name { get; set; }

        /// <summary>
        /// The color to highlight this class with in list representations. <see cref="Color.Empty"/> for no highlighting.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public Color HighlightColor { get; private set; }

        /// <summary>
        /// A short English description of this class for developers.
        /// </summary>
        [Description("A short English description of this class for developers.")]
        public string Description { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            string value = GetType().Name;
            if (!string.IsNullOrEmpty(Name))
                value += ": " + Name;
            return value;
        }

        //--------------------//

        #region Comparison
        /// <inheritdoc/>
        int IComparable<TSelf>.CompareTo(TSelf other)
        {
            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <typeparamref name="TSelf"/>.
        /// </summary>
        /// <returns>The cloned <typeparamref name="TSelf"/>.</returns>
        public virtual TSelf Clone()
        {
            // Perform initial shallow copy
            return (TSelf)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// The XML file <see cref="Template{TSelf}"/> instances are stored in.
        /// </summary>
        public static string FileName { get { return typeof(TSelf).Name + "s.xml"; } }

        private static NamedCollection<TSelf> _all;

        /// <summary>
        /// A list of all loaded <see cref="Template{TSelf}"/>s.
        /// </summary>
        /// <seealso cref="LoadAll"/>
        public static NamedCollection<TSelf> All
        {
            get
            {
                if (_all == null) throw new InvalidOperationException(string.Format(Resources.NotLoaded, FileName));

                return _all;
            }
        }

        /// <summary>
        /// Loads the list of <see cref="Template{TSelf}"/>s from <seealso cref="FileName"/>.
        /// </summary>
        public static void LoadAll()
        {
            using (var stream = ContentManager.GetFileStream("World", FileName))
                _all = XmlStorage.LoadXml<NamedCollection<TSelf>>(stream);
        }
        #endregion
    }
}
