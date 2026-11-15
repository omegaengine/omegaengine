/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Xml.Serialization;
using OmegaEngine;

namespace AlphaFramework.Presentation.Config;

/// <summary>
/// Stores display settings (resolution, etc.). Changes here require the engine to be reset.
/// </summary>
/// <seealso cref="SettingsBase.Display"/>
public sealed class DisplaySettings
{
    /// <summary>
    /// Occurs when a setting in this group is changed.
    /// </summary>
    [Description("Occurs when a setting in this group is changed.")]
    public event Action Changed = () => {};

    /// <summary>
    /// The monitor resolution for fullscreen mode
    /// </summary>
    [Description("The monitor resolution for fullscreen mode")]
    public Size Resolution
    {
        get;
        set
        {
            if (value == Size.Empty)
                value = Screen.PrimaryScreen!.Bounds.Size;
            value.To(ref field, Changed);
        }
    } = Size.Empty;

    /// <summary>
    /// The monitor resolution for fullscreen mode as a string in the format <c>WIDTHxHEIGHT</c>
    /// </summary>
    [XmlIgnore, Browsable(false)]
    public string ResolutionText
    {
        get => $"{Resolution.Width}x{Resolution.Height}";
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                string[] components = value.Split('x');
                Resolution = new(
                    int.Parse(components[0], CultureInfo.InvariantCulture),
                    int.Parse(components[1], CultureInfo.InvariantCulture));
            }
        }
    }

    /// <summary>
    /// The size of the render window
    /// </summary>
    [Description("The size of the render window")]
    public Size WindowSize
    {
        get;
        set
        {
            if (value == Size.Empty)
                value = new(1024, 768);
            value.To(ref field, Changed);
        }
    } = new(1024, 768);

    /// <summary>
    /// The level of anti aliasing to use
    /// </summary>
    [Description("The level of anti aliasing to use")]
    public int AntiAliasing { get; set => value.To(ref field, Changed); } = 2;

    /// <summary>
    /// The level of anti aliasing to use - as a string followed by an x
    /// </summary>
    [XmlIgnore, Browsable(false)]
    public string AntiAliasingText
    {
        #region Scripting-friendly interface
        get => $"{AntiAliasing}x";
        set
        {
            if (string.IsNullOrEmpty(value)) return;

            AntiAliasing = int.TryParse(value.Replace("x", ""), out int temp) ? temp : 0;
        }
        #endregion
    }

    /// <summary>
    /// Run game in fullscreen mode
    /// </summary>
    [Description("Run game in fullscreen mode")]
    public bool Fullscreen { get; set => value.To(ref field, Changed); } = true;

    /// <summary>
    /// Synchronize the framerate with the monitor's refresh rate
    /// </summary>
    [Description("Synchronize the framerate with the monitor's refresh rate")]
    public bool VSync { get; set => value.To(ref field, Changed); } = true;

    /// <summary>
    /// Generates <see cref="EngineConfig"/> from the settings.
    /// </summary>
    /// <param name="windowClientSize">The client size if running in windowed mode; <c>null</c> if running in fullscreen mode.</param>
    public EngineConfig ToEngineConfig(Size? windowClientSize)
    {
        if (!EngineCapabilities.CheckResolution(0, Resolution.Width, Resolution.Height))
            Resolution = Screen.PrimaryScreen.Bounds.Size;
        if (!EngineCapabilities.CheckAA(0, AntiAliasing))
            AntiAliasing = 0;

        return new()
        {
            Fullscreen = windowClientSize == null,
            VSync = VSync,
            TargetSize = windowClientSize ?? Resolution,
            AntiAliasing = AntiAliasing
        };
    }
}
