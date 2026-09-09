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

using System.Drawing;
using OmegaEngine.Foundation.Storage;
using OmegaGUI.Render;

namespace OmegaGUI.Model;

/// <summary>
/// Manages a custom texture loaded from <c>GUI/Textures</c> into a dedicated slot of a <see cref="Render.Dialog"/>
/// and applied to a single <see cref="Element"/> layer of a <see cref="Render.Control"/>.
/// </summary>
/// <remarks>Shared mechanism behind <see cref="PictureBox.TextureFile"/> and <see cref="Button.ImageFile"/>. The persisted values (file name, sub-region, alpha) stay on the owning control; this class only performs the slot reservation and the transfer into the render element.</remarks>
/// <param name="elementIndex">The index of the <see cref="Element"/> layer within the target control this texture is applied to.</param>
internal sealed class ControlTexture(uint elementIndex)
{
    private Dialog? _dialog;
    private Render.Control? _target;

    /// <summary>The texture slot in <see cref="Model.Dialog.DialogRender"/> this texture is loaded into once <see cref="Generate"/> has run.</summary>
    private uint _textureNumber;

    /// <summary>
    /// Is <paramref name="file"/> the name of a texture file that exists in <c>GUI/Textures</c>?
    /// </summary>
    public static bool IsValid(string file)
        => !string.IsNullOrEmpty(file) && ContentManager.FileExists("GUI/Textures", file);

    /// <summary>
    /// Reserves a texture slot and ensures the target element layer exists. Call from the owning control's <c>Generate()</c> once its <see cref="Render.Control"/> has been created.
    /// </summary>
    public void Generate(Dialog dialog, Render.Control target)
    {
        _dialog = dialog;
        _target = target;
        _textureNumber = dialog.CustomTexture++;

        // Make sure the layer exists before Apply() addresses it
        target[elementIndex] = new Element();
    }

    /// <summary>
    /// (Re-)loads <paramref name="file"/> into the reserved slot and points the target element layer at the
    /// <paramref name="location"/>/<paramref name="size"/> sub-region with the given <paramref name="alpha"/>.
    /// While there is no valid <paramref name="file"/> the layer is kept fully transparent.
    /// </summary>
    public void Apply(string file, Point location, Size size, byte alpha)
    {
        if (_target == null || _dialog?.DialogRender == null) return;

        if (IsValid(file))
        {
            _dialog.DialogRender.SetTexture(_textureNumber, file);
            _target[elementIndex].SetTexture(_textureNumber, new(location, size));
        }
        else
        {
            // No (valid) texture to show; point at a dummy region of the dialog's default texture
            // (an empty region would cause a division by zero when scaling the sprite)
            _target[elementIndex].SetTexture(0, new(0, 0, 1, 1));
        }

        // Element.SetTexture() re-initializes the color blend states, so the alpha needs to be re-applied afterwards
        ApplyAlpha(file, alpha);
    }

    /// <summary>
    /// Applies <paramref name="alpha"/> to the target element layer without reloading the texture,
    /// keeping the layer fully transparent while there is no valid <paramref name="file"/>.
    /// </summary>
    public void ApplyAlpha(string file, byte alpha)
    {
        if (_target == null) return;

        float value = IsValid(file) ? (float)alpha / 255 : 0;

        // Cover every non-hidden state, not just Normal: otherwise a hovered/pressed/disabled control
        // would reveal the stretched dummy region (or jump to a different opacity) as its state changes.
        var states = _target[elementIndex].TextureColor.States;
        states[(int)ControlState.Normal].Alpha = value;
        states[(int)ControlState.Disabled].Alpha = value;
        states[(int)ControlState.Focus].Alpha = value;
        states[(int)ControlState.MouseOver].Alpha = value;
        states[(int)ControlState.Pressed].Alpha = value;
    }
}
