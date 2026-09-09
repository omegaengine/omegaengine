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
using System.Windows.Forms;
using System.Xml.Serialization;

namespace OmegaGUI.Model;

/// <summary>
/// Button control
/// </summary>
[Cloneable]
public partial class Button : ButtonBase
{
    #region Variables
    /// <summary>
    /// The <see cref="OmegaGUI.Render"/> control used for actual rendering
    /// </summary>
    [IgnoreClone]
    private Render.Button? _button;

    /// <summary>Handles loading <see cref="ImageFile"/> and transferring it into the button's image layer.</summary>
    [IgnoreClone]
    private readonly ControlTexture _image = new(Render.Button.ImageLayer);
    #endregion

    #region Properties
    /// <summary>
    /// The text displayed on the control
    /// </summary>
    [DefaultValue(""), Description("The text displayed on the control"), Category("Appearance")]
    [XmlAttribute]
    public override string Text
    {
        get => ControlText;
        set
        {
            ControlText = value;
            _button?.SetText(Parent.GetLocalized(ControlText));
        }
    }

    private string _customStyle;

    /// <summary>
    /// A custom style for this button - no auto-update
    /// </summary>
    [Description("A custom style for this button"), Category("Appearance")]
    public string CustomStyle
    {
        get => _customStyle;
        set
        {
            _customStyle = value;
            NeedsUpdate();
        }
    }

    [Description("Is the specified style name valid?"), Category("Appearance")]
    public bool StyleValid => Parent?.GetButtonStyle(_customStyle) is { TextureFileValid: true };

    private string _imageFile;

    /// <summary>
    /// The file containing a custom image to display on the button, drawn underneath any <see cref="Text"/>
    /// </summary>
    /// <remarks>Assigning the value this already has is a no-op, so scripts can set this on every frame without reloading the texture each time.</remarks>
    [DefaultValue(""), Description("The file containing a custom image to display on the button, drawn underneath any text"), Category("Appearance")]
    public string ImageFile
    {
        get => _imageFile;
        set
        {
            if (value == _imageFile) return;
            _imageFile = value;
            UpdateImage();
        }
    }

    public bool ShouldSerializeImageFile() => !string.IsNullOrEmpty(_imageFile);

    [Description("Is the specified image file name valid?"), Category("Appearance")]
    public bool ImageFileValid => ControlTexture.IsValid(_imageFile);

    private Point _imageLocation = new(0, 0);

    /// <summary>
    /// The upper left corner of the area in the image file to use
    /// </summary>
    [DefaultValue(typeof(Point), "0,0"), Description("The upper left corner of the area in the image file to use"), Category("Appearance")]
    public Point ImageLocation
    {
        get => _imageLocation;
        set
        {
            if (value == _imageLocation) return;
            _imageLocation = value;
            UpdateImage();
        }
    }

    public bool ShouldSerializeImageLocation() => _imageLocation != default;

    private Size _imageSize = new(256, 256);

    /// <summary>
    /// The distance to the lower right corner of the area in the image file to use
    /// </summary>
    [Description("The distance to the lower right corner of the area in the image file to use"), Category("Appearance")]
    public Size ImageSize
    {
        get => _imageSize;
        set
        {
            if (value == _imageSize) return;
            _imageSize = value;
            UpdateImage();
        }
    }

    public bool ShouldSerializeImageSize() => !string.IsNullOrEmpty(_imageFile);

    private Padding _imagePadding;

    /// <summary>
    /// The space between the button's edges and its <see cref="ImageFile"/>, in unscaled dialog units
    /// </summary>
    [DefaultValue(typeof(Padding), "0,0,0,0"), Description("The space between the button's edges and its image, in unscaled dialog units"), Category("Appearance")]
    public Padding ImagePadding
    {
        get => _imagePadding;
        set
        {
            _imagePadding = value;
            UpdateImage();
        }
    }

    public bool ShouldSerializeImagePadding() => _imagePadding != default;

    private byte _imageAlpha = 255;

    /// <summary>
    /// The level of transparency of the image from 0 (invisible) to 255 (solid)
    /// </summary>
    [DefaultValue((byte)255), Description("The level of transparency of the image from 0 (invisible) to 255 (solid)"), Category("Appearance")]
    public byte ImageAlpha
    {
        get => _imageAlpha;
        set
        {
            _imageAlpha = value;
            _image.ApplyAlpha(_imageFile, _imageAlpha);
        }
    }
    #endregion

    #region Constructor
    public Button()
    {
        Size = new(140, 40);
    }
    #endregion

    #region Generate
    internal override void Generate()
    {
        // Add control to dialog
        UpdateLayout();
        DXControl = _button =
            Parent.DialogRender.AddButton(0, Parent.GetLocalized(ControlText), EffectiveLocation.X, EffectiveLocation.Y, EffectiveSize.Width, EffectiveSize.Height, Hotkey, Default);
        DXControl.IsVisible = IsVisible;
        DXControl.IsEnabled = IsEnabled;
        DXControl.IsEnabled = IsEnabled;

        if (StyleValid)
        {
            ButtonStyle style = Parent.GetButtonStyle(_customStyle);
            _button[Render.Button.ButtonLayer] = style.ButtonElement;
            _button[Render.Button.FillLayer] = style.ButtonElement;
        }

        // Setup event hooks
        SetupMouseEvents();
        if (!string.IsNullOrEmpty(OnClick))
            _button.Click += delegate { Parent.RaiseEvent(OnClick, $"{Name}_Click"); };

        // Reserve a texture slot for the image layer and (re-)load the image into it, including on later changes
        _image.Generate(Parent, _button);
        UpdateImage();
    }

    /// <summary>
    /// (Re-)loads <see cref="ImageFile"/> into the reserved texture slot and applies it (with <see cref="ImageLocation"/>/<see cref="ImageSize"/>/<see cref="ImagePadding"/>) to the button's image layer,
    /// e.g. after (re-)generating, after one of those properties is changed at runtime, or after a scale change.
    /// </summary>
    private void UpdateImage()
    {
        if (_button == null) return; // Not generated yet; Generate() will call this itself once it is

        float scale = Parent.EffectiveScale;
        _button.ImagePadding = new Padding(
            (int)(_imagePadding.Left * scale),
            (int)(_imagePadding.Top * scale),
            (int)(_imagePadding.Right * scale),
            (int)(_imagePadding.Bottom * scale));

        _image.Apply(_imageFile, _imageLocation, _imageSize, _imageAlpha);
    }

    /// <inheritdoc/>
    protected override void OnLayoutUpdated() => UpdateImage();
    #endregion
}
