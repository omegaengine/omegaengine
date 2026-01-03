---
uid: OmegaGUI
summary: OmegaGUI is a GUI toolkit for OmegaEngine.
---
> [!NOTE]
> NuGet package: [OmegaGUI](https://www.nuget.org/packages/OmegaGUI/)
TODO: Describe OmegaGUI as texture-based 2D GUI toolkit with Lua scripting support.

TODO: Explain how <xref:OmegaGUI.GuiManager>, <xref:OmegaGUI.DialogRenderer>, <xref:OmegaGUI.Model> and <xref:OmegaGUI.Render> interact.

TODO: Add mermaid diagram

## XML storage

Load GUI definitions from XML files in the <xref:OmegaGUI.Model> namespace:

```csharp
// Load dialog from XML
var dialog = XmlStorage.LoadXml<DialogModel>("GUI/MainMenu.xml");

// Convert to renderable dialog
var dialogRenderer = new DialogRenderer(guiManager, dialog.ToRenderable(), lua: myLuaInstance);
dialogRenderer.Show();
```

> [!TIP]
> [AlphaEditor](xref:AlphaFramework.Editor) provides a WYSIWYG editor for creating and editing GUI dialogs visually.

## Scripting

TODO: Explain event handling with Lua scripts

## Localization

TODO: Explain how localization in OmegaGUI works

## Theming

TODO: Explain that controls are rendered using texture atlas loaded via [storage system](xref:OmegaEngine.Foundation.Storage) from `GUI/Textures/base.png`.

TODO: Convert to markdown table:

Control							Left, Top, Right, Bottom
=======							========================
Button							0, 0, 136, 54
Button - Fill Layer				136, 0, 252, 54
CheckBox - Box					0, 54, 27, 81
CheckBox - Check				27, 54, 54, 81
RadioButton - Box				54, 54, 81, 81
RadioButton - Check				81, 54, 108, 81
DropdownList - Main				7, 81, 247, 123
DropdownList - Button			98, 189, 151, 238
DropdownList - Dropdown			13, 123, 241, 160
DropdownList - Selection		12, 163, 239, 183
Slider - Track					1, 187, 93, 228
Slider - Button					151, 193, 192, 234
Scrollbar - Track				196, 212, 218, 223
Scrollbar - Up Arrow			196, 192, 218, 212
Scrollbar - Down Arrow			196, 223, 218, 244
Scrollbar - Button				220, 192, 238, 234
TextBox - Text area				14, 90, 241, 113
TextBox - Top left border		8, 82, 14, 90
TextBox - Top border			14, 82, 241, 90
TextBox - Top right border		241, 82, 246, 90
TextBox - Left border			8, 90, 14, 113
TextBox - Right border			241, 90, 246, 113
TextBox - Lower left border		8, 113, 14, 121
TextBox - Lower border			14, 113, 241, 121
TextBox - Lower right border	241, 113, 246, 121
Listbox - Main					13, 123, 241, 160
Listbox - Selection				16, 166, 240, 183
