# OmegaEngine project templates

The OmegaEngine project templates help you create C# projects that use OmegaEngine, OmegaGUI and AlphaFramework.

These templates use the [dotnet new](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new) command. To install them, run:

    dotnet new install OmegaEngine.Templates

## WinForms

A Windows Forms application that embeds a surface with 3D rendering via the OmegaEngine.

    mkdir MyApp
    cd MyApp
    dotnet new omegaengine-winforms

## Fullscreen

A fullscreen application with 3D rendering via the OmegaEngine.

    mkdir MyApp
    cd MyApp
    dotnet new omegaengine-fullscreen

## AlphaFramework

The AlphaFramework supplies a common base for building and rendering game worlds as well as an IDE-like Editor. The OmegaEngine provides the actual rendering functionality.

    mkdir MyGame
    cd MyGame
    dotnet new omegaengine-alphaframework
