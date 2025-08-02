/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaGUI.Render;

/// <summary>
/// A filled rectangle used to optically group controls together
/// </summary>
public class GroupBox : Control
{
    public Color4 BorderColor, FillColor;

    /// <summary>Create new group box instance</summary>
    public GroupBox(Dialog parent, Color4 borderColor, Color4 fillColor) : base(parent)
    {
        ctrlType = ControlType.GroupBox;
        BorderColor = borderColor;
        FillColor = fillColor;
    }

    /// <summary>Render the picture box</summary>
    public override void Render(Device device, float elapsedTime)
    {
        if (IsVisible)
        {
            parentDialog.DrawRectangle(boundingBox, FillColor, true);
            parentDialog.DrawRectangle(boundingBox, BorderColor, false);
        }
    }
}
