/*
 * Copyright Microsoft Cooperation
 * Modifications Copyright 2006-2014 Bastian Eicher
 *
 * This code is based on sample code from the DiretX SDK and as such is placed
 * under the Microsoft Public License.
 */

namespace OmegaGUI.Render;

/// <summary>
/// How to align text within a control
/// </summary>
public enum TextAlign
{
    Left,
    Center,
    Right
}

/// <summary>
/// Predefined control types
/// </summary>
public enum ControlType
{
    Label,
    GroupBox,
    Button,
    CheckBox,
    RadioButton,
    DropdownList,
    Slider,
    ListBox,
    TextBox,
    Scrollbar,
    PictureBox,
}

/// <summary>
/// Possible states of a control
/// </summary>
public enum ControlState
{
    Normal,
    Disabled,
    Hidden,
    Focus,
    MouseOver,
    Pressed,
    LastState // Should always be last
}
