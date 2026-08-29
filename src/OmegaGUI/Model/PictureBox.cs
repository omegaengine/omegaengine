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
using OmegaEngine.Foundation.Storage;
using OmegaGUI.Render;

namespace OmegaGUI.Model;

[Cloneable]
public partial class PictureBox : Control
{
    #region Properties
    /// <summary>
    /// The texture slot in <see cref="Model.Dialog.DialogRender"/> this control's texture is loaded into once generated
    /// </summary>
    private uint _textureNumber;

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
    public bool TextureFileValid => !string.IsNullOrEmpty(_textureFile) && ContentManager.FileExists("GUI/Textures", _textureFile);

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
        // Reserve a texture slot; the actual texture is (re-)loaded into it by UpdateTexture(), including on later changes
        _textureNumber = Parent.CustomTexture++;

        var fill = new Element();
        fill.TextureColor.Initialize(Render.Dialog.WhiteColorValue); // UpdateTexture() below applies Alpha on top once States exist

        // Add control to dialog
        UpdateLayout();
        DXControl = Parent.DialogRender.AddPictureBox(0, EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, fill);
        ControlModel.IsVisible = IsVisible;
        ControlModel.IsEnabled = IsEnabled;

        // Setup event hooks
        SetupMouseEvents();

        UpdateTexture();
    }

    /// <summary>
    /// (Re-)loads <see cref="TextureFile"/> into the reserved texture slot and applies it to the rendered element,
    /// e.g. after (re-)generating or after <see cref="TextureFile"/>/<see cref="TextureLocation"/>/<see cref="TextureSize"/> is changed at runtime.
    /// </summary>
    private void UpdateTexture()
    {
        if (DXControl == null) return; // Not generated yet; Generate() will call this itself once it is

        if (TextureFileValid)
        {
            Parent.DialogRender.SetTexture(_textureNumber, _textureFile);
            DXControl[0].SetTexture(_textureNumber, new(_textureLocation, _textureSize));
        }
        else
        {
            // No (valid) texture to show; point at a dummy region of the dialog's default texture
            // (an empty region would cause a division by zero when scaling the sprite)
            DXControl[0].SetTexture(0, new(0, 0, 1, 1));
        }

        // Element.SetTexture() re-initializes the color blend states, so Alpha needs to be re-applied afterwards
        ApplyAlpha();
    }

    /// <summary>
    /// Applies <see cref="Alpha"/> to the rendered element, keeping the control fully transparent while there is no valid <see cref="TextureFile"/>.
    /// </summary>
    private void ApplyAlpha()
    {
        if (DXControl == null) return; // Not generated yet; Generate() will apply this itself once it is

        DXControl[0].TextureColor.States[(int)ControlState.Normal].Alpha = TextureFileValid ? (float)_alpha / 255 : 0;
    }
    #endregion
}
