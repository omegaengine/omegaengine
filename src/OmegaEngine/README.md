# OmegaEngine

**OmegaEngine** is a general-purpose 3D graphics engine for .NET Framework and DirectX 9. It is designed to be:

- light-weight (compiled binaries with external libraries < 4MB),
- modular (use only the parts you need for your project) and
- gameplay-agnostic (also suitable for visualization projects, etc.).

The complementary [OmegaGUI](https://www.nuget.org/packages/OmegaGUI), [AlphaFramework](https://www.nuget.org/packages/AlphaFramework.World) and [AlphaEditor](https://www.nuget.org/packages/AlphaEditor) help you build games using OmegaEngine.

[Documentation](https://docs.omegaengine.de/)

## First steps

Create a WinForms project targeting .NET Framework 4.7.2 or newer with the platform set to `x86`.
The engine renders with Direct3D 9Ex, so it requires a graphics driver providing it (Windows Vista or newer).
Add a reference to the NuGet package `OmegaEngine`. Then add the following code to render a textured sphere:

```csharp
var engine = new Engine(this, new EngineConfig { TargetSize = ClientSize });
var scene = new Scene
{
    Positionables = { Model.Sphere(engine, XTexture.Get(engine, "flag.png")) }
};
var view = new View(scene, new ArcballCamera()) { BackgroundColor = Color.CornflowerBlue };
engine.Views.Add(view);

Paint += delegate { engine.Render(); };
```

You additionally need to ensure these native dependencies are installed or bundled with your application:

- [Visual C++ 2010 Redistributable x86](https://www.microsoft.com/en-us/download/details.aspx?id=26999)
- [DirectX June 2010 Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=8109)

## Related packages

- [OmegaEngine.Foundation](https://www.nuget.org/packages/OmegaEngine.Foundation) provides rendering-agnostic infrastructure like storage and data structures.
- [OmegaGUI](https://www.nuget.org/packages/OmegaGUI) adds a GUI toolkit with an XML file format and Lua scripting.
- [AlphaFramework.World](https://www.nuget.org/packages/AlphaFramework.World) provides a basis for engine-agnostic models of game worlds.
- [AlphaFramework.Presentation](https://www.nuget.org/packages/AlphaFramework.Presentation) provides a basis for presenters that visualize game worlds using OmegaEngine.
- [AlphaEditor](https://www.nuget.org/packages/AlphaEditor) is a toolkit for creating editors for games based on AlphaFramework.
