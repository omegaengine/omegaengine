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
using System.Xml.Serialization;
using OmegaEngine.Storage;
using OmegaEngine.Values;
using SlimDX.Direct3D9;
using OmegaGUI.Render;

namespace OmegaGUI.Model;

/// <summary>
/// Represents a custom style for a GUI button
/// </summary>
public class ButtonStyle : ICloneable
{
    #region Variables
    /// <summary>
    /// The dialog containing this button style
    /// </summary>
    internal Dialog Parent;

    public XColor
        ButtonColorNormal = new(1.0f, 1.0f, 1.0f, 0.55f),
        ButtonColorPressed = new(1.0f, 1.0f, 1.0f, 0.85f),
        TextColorNormal = Color.White,
        TextColorMouseOver = Color.Black;

    public XColor
        FillColorMouseOver = new(1.0f, 1.0f, 1.0f, 0.6f),
        FillColorPressed = new(0, 0, 0, 0.25f),
        FillColorFocus = new(1.0f, 1.0f, 1.0f, 0.05f);

    internal Element ButtonElement, FillElement;
    #endregion

    #region Properties
    /// <summary>
    /// Unique name for identifying this button style
    /// </summary>
    [XmlAttribute, Description("Unique name for identifying this button style"), Category("Design")]
    public string Name { get; set; }

    public override string ToString()
    {
        string value = GetType().Name;
        if (!string.IsNullOrEmpty(Name))
            value += ": " + Name;
        return value;
    }

    #region Texture file
    /// <summary>
    /// The file containing the texture for this button style
    /// </summary>
    [Description("The file containing the texture for this button style"), Category("Appearance")]
    public string TextureFile { get; set; } = "base.png";

    [Description("Is the specified texture file name valid?"), Category("Appearance")]
    public bool TextureFileValid => !string.IsNullOrEmpty(TextureFile) && ContentManager.FileExists("GUI/Textures", TextureFile);
    #endregion

    #region Button layer
    /// <summary>
    /// The upper left corner of the area in the texture file to use for the button layer
    /// </summary>
    [Description("The upper left corner of the area in the texture file to use for the button layer"), Category("Button layer")]
    public Point ButtonTextureLocation { get; set; } = new(0, 0);

    /// <summary>
    /// The distance to the lower right corner of the area in the texture file to use for the button layer
    /// </summary>
    [Description("The distance to the lower right corner of the area in the texture file to use for the button layer"), Category("Button layer")]
    public Size ButtonTextureSize { get; set; } = new(136, 54);

    [XmlIgnore, Category("Button layer"), Description("The color of the button layer in its normal state")]
    public Color ButtonNormalColor { get => (Color)ButtonColorNormal; set => ButtonColorNormal = value; }

    [XmlIgnore, Category("Button layer"), Description("The color of the button layer while the button is pressed")]
    public Color ButtonPressedColor { get => (Color)ButtonColorPressed; set => ButtonColorPressed = value; }

    [XmlIgnore, Category("Button layer"), Description("The color of the text while the mouse is over the button")]
    public Color TextNormalColor { get => (Color)TextColorNormal; set => TextColorNormal = value; }

    [XmlIgnore, Category("Button layer"), Description("The color of the text while the mouse is over the button")]
    public Color TextMouseOverColor { get => (Color)TextColorMouseOver; set => TextColorMouseOver = value; }
    #endregion

    #region Fill layer
    /// <summary>
    /// The upper left corner of the area in the texture file to use for the fill layer
    /// </summary>
    [Description("The upper left corner of the area in the texture file to use for the fill layer"), Category("Fill layer")]
    public Point FillTextureLocation { get; set; } = new(136, 0);

    /// <summary>
    /// The distance to the lower right corner of the area in the texture file to use for the fill layer
    /// </summary>
    [Description("The distance to the lower right corner of the area in the texture file to use for the fill layer"), Category("Fill layer")]
    public Size FillTextureSize { get; set; } = new(252, 54);

    [XmlIgnore, Category("Fill layer"), Description("The color of the fill layer in its normal state")]
    public Color FillMouseOverColor { get => (Color)FillColorMouseOver; set => FillColorMouseOver = value; }

    [XmlIgnore, Category("Fill layer"), Description("The color of the fill layer while the button is pressed")]
    public Color FillPressedColor { get => (Color)FillColorPressed; set => FillColorPressed = value; }

    [XmlIgnore, Category("Fill layer"), Description("The color of the fill layer while the button is focused")]
    public Color FillFocusColor { get => (Color)FillColorFocus; set => FillColorFocus = value; }
    #endregion

    #endregion

    #region Generate
    internal void Generate()
    {
        if (!TextureFileValid) return;

        // Load custom texture
        uint textureNumber = Parent.CustomTexture++;
        Parent.DialogRender.SetTexture(textureNumber, TextureFile);

        ButtonElement = new();
        ButtonElement.SetTexture(textureNumber, new(ButtonTextureLocation, ButtonTextureSize));
        ButtonElement.SetFont(0, TextColorNormal.ToColorValue(), DrawTextFormat.Center | DrawTextFormat.VerticalCenter);
        ButtonElement.TextureColor.States[(int)ControlState.Normal] = ButtonColorNormal.ToColorValue();
        ButtonElement.TextureColor.States[(int)ControlState.Pressed] = ButtonColorPressed.ToColorValue();
        ButtonElement.FontColor.States[(int)ControlState.Normal] = TextColorNormal.ToColorValue();
        ButtonElement.FontColor.States[(int)ControlState.MouseOver] = TextColorMouseOver.ToColorValue();

        FillElement = new();
        FillElement.SetTexture(textureNumber, new(FillTextureLocation, FillTextureSize), Render.Dialog.TransparentWhite);
        FillElement.TextureColor.States[(int)ControlState.MouseOver] = FillColorMouseOver.ToColorValue();
        FillElement.TextureColor.States[(int)ControlState.Pressed] = FillColorPressed.ToColorValue();
        FillElement.TextureColor.States[(int)ControlState.Focus] = FillColorFocus.ToColorValue();
    }
    #endregion

    #region Clone
    /// <summary>
    /// Creates a shallow copy of this button style.
    /// You need to call <see cref="Generate"/> on it before it can be used for rendering.
    /// </summary>
    /// <returns>The cloned button style</returns>
    public ButtonStyle Clone()
    {
        return (ButtonStyle)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion
}
