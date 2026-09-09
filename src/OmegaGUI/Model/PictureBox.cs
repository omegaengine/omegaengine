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

using System.ComponentModel;
using System.Drawing;
using OmegaGUI.Render;

namespace OmegaGUI.Model;

[Cloneable]
public partial class PictureBox : Control
{
    #region Properties
    /// <summary>Handles loading the texture and transferring it into the rendered element.</summary>
    [IgnoreClone]
    private readonly ControlTexture _texture = new(0);

    private string _textureFile;

    /// <summary>
    /// The file containing the texture for this picture box
    /// </summary>
    /// <remarks>Assigning the value this already has is a no-op, so scripts can set this on every frame without reloading the texture each time.</remarks>
    [Description("The file containing the texture for this picture box"), Category("Appearance")]
    public string TextureFile
    {
        get => _textureFile;
        set
        {
            if (value == _textureFile) return;
            _textureFile = value;
            UpdateTexture();
        }
    }

    [Description("Is the specified texture file name valid?"), Category("Appearance")]
    public bool TextureFileValid => ControlTexture.IsValid(_textureFile);

    private Point _textureLocation = new(0, 0);

    /// <summary>
    /// The upper left corner of the area in the texture file to use
    /// </summary>
    [Description("The upper left corner of the area in the texture file to use"), Category("Appearance")]
    public Point TextureLocation
    {
        get => _textureLocation;
        set
        {
            if (value == _textureLocation) return;
            _textureLocation = value;
            UpdateTexture();
        }
    }

    private Size _textureSize = new(256, 256);

    /// <summary>
    /// The distance to the lower right corner of the area in the texture file to use
    /// </summary>
    [Description("The distance to the lower right corner of the area in the texture file to use"), Category("Appearance")]
    public Size TextureSize
    {
        get => _textureSize;
        set
        {
            if (value == _textureSize) return;
            _textureSize = value;
            UpdateTexture();
        }
    }

    private byte _alpha = 255;

    /// <summary>
    /// The level of transparency from 0 (invisible) to 255 (solid)
    /// </summary>
    [DefaultValue((byte)255), Description("The level of transparency from 0 (invisible) to 255 (solid)"), Category("Appearance")]
    public byte Alpha
    {
        get => _alpha;
        set
        {
            _alpha = value;
            ApplyAlpha();
        }
    }
    #endregion

    #region Constructor
    public PictureBox()
    {
        Size = new(120, 60);
    }
    #endregion

    #region Generate
    internal override void Generate()
    {
        // Add control to dialog
        UpdateLayout();
        DXControl = Parent.DialogRender.AddPictureBox(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, new Element());
        ControlModel.IsVisible = IsVisible;
        ControlModel.IsEnabled = IsEnabled;

        // Setup event hooks
        SetupMouseEvents();

        // Reserve a texture slot and (re-)load the texture into it, including on later changes
        _texture.Generate(Parent, DXControl);
        UpdateTexture();
    }

    /// <summary>
    /// (Re-)loads <see cref="TextureFile"/> into the reserved texture slot and applies it to the rendered element,
    /// e.g. after (re-)generating or after <see cref="TextureFile"/>/<see cref="TextureLocation"/>/<see cref="TextureSize"/> is changed at runtime.
    /// </summary>
    private void UpdateTexture()
    {
        if (DXControl == null) return; // Not generated yet; Generate() will call this itself once it is

        _texture.Apply(_textureFile, _textureLocation, _textureSize, _alpha);
    }

    /// <summary>
    /// Applies <see cref="Alpha"/> to the rendered element, keeping the control fully transparent while there is no valid <see cref="TextureFile"/>.
    /// </summary>
    private void ApplyAlpha()
    {
        if (DXControl == null) return; // Not generated yet; Generate() will apply this itself once it is

        _texture.ApplyAlpha(_textureFile, _alpha);
    }
    #endregion
}
