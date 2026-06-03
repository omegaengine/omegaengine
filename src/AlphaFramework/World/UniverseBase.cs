/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using NanoByte.Common;
using NanoByte.Common.Storage;
using OmegaEngine.Foundation.Storage;

namespace AlphaFramework.World;

/// <summary>
/// A common base for game worlds (but not a running game). Equivalent to the content of a map file.
/// </summary>
public abstract class UniverseBase : IUniverse
{
    private string? _skybox;

    private void OnSkyboxChanged() => SkyboxChanged?.Invoke();

    /// <summary>
    /// Occurs when <see cref="Skybox"/> was changed.
    /// </summary>
    public event Action? SkyboxChanged;

    /// <summary>
    /// The name of the skybox to use for this map; may be <c>null</c> or empty.
    /// </summary>
    [DefaultValue(""), Category("Effects"), Description("The name of the skybox to use for this map; may be null or empty.")]
    public string? Skybox { get => _skybox; set => value.To(ref _skybox, OnSkyboxChanged); }

    /// <inheritdoc/>
    [DefaultValue(0.0), Category("Gameplay"), Description("Total elapsed game time in seconds.")]
    public double GameTime { get; set; }

    /// <inheritdoc/>
    public virtual void Update(double elapsedGameTime)
    {
        GameTime += elapsedGameTime;
    }

    /// <inheritdoc/>
    [XmlIgnore, Browsable(false)]
    public string? SourceFile { get; protected set; }

    /// <inheritdoc/>
    public virtual void Save(string path)
    {
        this.SaveXml(path);
        SourceFile = path;
    }

    /// <summary>
    /// Saves the universe back to the file it was loaded from.
    /// </summary>
    /// <exception cref="InvalidOperationException">Universe was not loaded from a file.</exception>
    public void Save()
    {
        if (SourceFile == null) throw new InvalidOperationException("Universe was not loaded from a file.");

        string path = Path.IsPathRooted(SourceFile) ? SourceFile : ContentManager.CreateFilePath("World/Maps", SourceFile);
        Save(path);
    }

    /// <inheritdoc/>
    public virtual void PostLoad(string? path)
    {
        SourceFile = path;
    }
}
