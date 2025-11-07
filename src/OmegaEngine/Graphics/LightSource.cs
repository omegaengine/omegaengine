/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.ComponentModel;
using System.Drawing;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics;

/// <summary>
/// A light source that illuminates <see cref="PositionableRenderable"/>s in a <see cref="Scene"/>.
/// </summary>
/// <seealso cref="Scene.Lights"/>
public abstract class LightSource
{
    /// <summary>
    /// Text value to make it easier to identify a particular camera
    /// </summary>
    [Description("Text value to make it easier to identify a particular light source"), Category("Design")]
    public string? Name { get; set; }

    public override string ToString()
    {
        string value = GetType().Name;
        if (!string.IsNullOrEmpty(Name))
            value += $": {Name}";
        return value;
    }

    /// <summary>
    /// Shall the light source affect its surroundings?
    /// </summary>
    [Description("Shall the light source affect its enabled?"), Category("Behavior")]
    public bool Enabled { get; set; } = true;

    private Color _diffuse = Color.White.DropAlpha();

    /// <summary>
    /// The diffuse color this light source emits
    /// </summary>
    [Description("The diffuse color this light source emits"), Category("Appearance")]
    public Color Diffuse { get => _diffuse; set => _diffuse = value.DropAlpha(); }

    private Color _specular = Color.Gray.DropAlpha();

    /// <summary>
    /// The specular color this light source emits
    /// </summary>
    [Description("The specular color this light source emits"), Category("Appearance")]
    public Color Specular { get => _specular; set => _specular = value.DropAlpha(); }

    private Color _ambient = Color.Black.DropAlpha();

    /// <summary>
    /// The ambient color this light source emits
    /// </summary>
    [Description("The ambient color this light source emits"), Category("Appearance")]
    public Color Ambient { get => _ambient; set => _ambient = value.DropAlpha(); }
}
