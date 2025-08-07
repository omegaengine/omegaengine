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
using NanoByte.Common;

namespace FrameOfReference.World.Config;

/// <summary>
/// Stores display settings (resolution, etc.). Changes here require the engine to be reset.
/// </summary>
/// <seealso cref="Settings.Display"/>
public sealed class DisplaySettings
{
    /// <summary>
    /// Occurs when a setting in this group is changed.
    /// </summary>
    [Description("Occurs when a setting in this group is changed.")]
    public event Action Changed = () => {};

    private Size _resolution = Screen.PrimaryScreen!.Bounds.Size;

    /// <summary>
    /// The monitor resolution for fullscreen mode
    /// </summary>
    [Description("The monitor resolution for fullscreen mode")]
    public Size Resolution
    {
        get => _resolution;
        set
        {
            if (value == Size.Empty)
                value = Screen.PrimaryScreen!.Bounds.Size;
            value.To(ref _resolution, Changed);
        }
    }

    /// <summary>
    /// The monitor resolution for fullscreen mode - as a string followed with an x between the components
    /// </summary>
    [XmlIgnore, Browsable(false)]
    public string ResolutionText
    {
        #region Scripting-friendly interface
        get { return Resolution.Width + "x" + _resolution.Height; }
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
        #endregion
    }

    private Size _windowSize = new(1024, 768);

    /// <summary>
    /// The size of the render window
    /// </summary>
    [Description("The size of the render window")]
    public Size WindowSize
    {
        get => _windowSize;
        set
        {
            if (value == Size.Empty)
                value = new(1024, 768);
            value.To(ref _windowSize, Changed);
        }
    }

    private int _antiAliasing;

    /// <summary>
    /// The level of anti aliasing to use
    /// </summary>
    [Description("The level of anti aliasing to use")]
    public int AntiAliasing { get => _antiAliasing; set => value.To(ref _antiAliasing, Changed); }

    /// <summary>
    /// The level of anti aliasing to use - as a string followed by an x
    /// </summary>
    [XmlIgnore, Browsable(false)]
    public string AntiAliasingText
    {
        #region Scripting-friendly interface
        get { return AntiAliasing + "x"; }
        set
        {
            if (string.IsNullOrEmpty(value)) return;

            AntiAliasing = int.TryParse(value.Replace("x", ""), out int temp) ? temp : 0;
        }
        #endregion
    }

    private bool _fullscreen = true;

    /// <summary>
    /// Run game in fullscreen mode
    /// </summary>
    [Description("Run game in fullscreen mode")]
    public bool Fullscreen { get => _fullscreen; set => value.To(ref _fullscreen, Changed); }

    private bool _vSync;

    /// <summary>
    /// Synchronize the framerate with the monitor's refresh rate
    /// </summary>
    [Description("Synchronize the framerate with the monitor's refresh rate")]
    public bool VSync { get => _vSync; set => value.To(ref _vSync, Changed); }
}
