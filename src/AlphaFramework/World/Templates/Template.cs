/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using AlphaFramework.World.Properties;
using NanoByte.Common;
using NanoByte.Common.Collections;
using OmegaEngine.Storage;

namespace AlphaFramework.World.Templates;

/// <summary>
/// A set of data used as a prototype for constructing new objects at runtime.
/// </summary>
/// <typeparam name="TSelf">The type of the class itself.</typeparam>
public abstract class Template<TSelf> : INamed, IHighlightColor, ICloneable, IComparable<TSelf> where TSelf : Template<TSelf>
{
    /// <summary>
    /// The name of this class. Used in map files as a reference. Must be unique and is case-sensitive!
    /// </summary>
    // Mark as read only for PropertyGrid, since direct renames would confuse NamedCollection<T>
    [XmlAttribute, ReadOnly(true), Description("The name of this class. Used in map files as a reference. Must be unique and is case-sensitive!")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The color to highlight this class with in list representations. <see cref="Color.Empty"/> for no highlighting.
    /// </summary>
    [XmlIgnore, Browsable(false)]
    public Color HighlightColor { get; protected set; }

    /// <summary>
    /// A short English description of this class for developers.
    /// </summary>
    [Description("A short English description of this class for developers.")]
    public string? Description { get; set; }

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
    int IComparable<TSelf>.CompareTo(TSelf? other)
    {
        return string.Compare(Name, other?.Name, StringComparison.OrdinalIgnoreCase);
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

    object ICloneable.Clone() => Clone();
    #endregion

    //--------------------//

    #region Storage
    /// <summary>
    /// The XML file <see cref="Template{TSelf}"/> instances are stored in.
    /// </summary>
    public static string FileName => typeof(TSelf).Name + "s.xml";

    private static NamedCollection<TSelf>? _all;

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
    /// Loads the list of <see cref="Template{TSelf}"/>s from <see cref="FileName"/>.
    /// </summary>
    public static void LoadAll()
    {
        using var stream = ContentManager.GetFileStream("World", FileName);
        _all = XmlStorage.LoadXml<NamedCollection<TSelf>>(stream);
    }
    #endregion
}
