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
using Common.Controls;

namespace World
{
    /// <summary>
    /// A set of data used as a prototype for constructing new objects at runtime.
    /// </summary>
    /// <typeparam name="T">The derived class used for the clone return value.</typeparam>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for INamed sorting")]
    public abstract class Template<T> : INamed<T>, IHighlightColor, ICloneable where T : Template<T>
    {
        #region Properties
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
        #endregion

        //--------------------//

        #region Comparison
        /// <inheritdoc/>
        int IComparable<T>.CompareTo(T other)
        {
            return string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <typeparamref name="T"/>.
        /// </summary>
        /// <returns>The cloned <typeparamref name="T"/>.</returns>
        public virtual T Clone()
        {
            // Perform initial shallow copy
            return (T)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
