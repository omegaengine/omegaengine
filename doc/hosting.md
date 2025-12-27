# Hosting

OmegaEngine provides several hosting classes that handle window management, engine initialization, and render loop coordination. Choose the appropriate hosting class based on your application type.

## Hosting Classes

### <xref:OmegaEngine.RenderHost>

**RenderHost** is a full-featured application host that provides a complete windowed application with:

- **Window management** - Creates and manages a render-capable form with fullscreen support
- **Engine initialization** - Automatically creates and configures the <xref:OmegaEngine.Engine>
- **Render loop** - Handles the main application loop and frame rendering
- **Input handling** - Sets up <xref:OmegaEngine.Input>
- **Debug console** - Provides access to debugging features

**Use when:** Building a standalone game or application that needs its own window.

TODO: Check that sample code compiles

```csharp
public class MyApp : RenderHost
{
    public MyApp() : base("My Application", icon: Resources.AppIcon)
    { }

    protected override bool Initialize()
    {
        if (!base.Initialize()) return false;

        var view = new View(new Scene(), new FreeFlyCamera());
        Engine.Views.Add(view);

        return true;
    }

    protected override void Render()
    {
        // Custom rendering logic here
        base.Render();
    }
}

static void Main()
{
    using var app = new MyApp();
    app.ToWindowed();
    app.Run();
}
```

### <xref:AlphaFramework.Presentation.GameBase>

**GameBase** extends `RenderHost` with game-specific features:

- **Settings management** - Loads and saves game configuration
- **GUI system** - Integrates OmegaGUI via <xref:OmegaGUI.GuiManager>

**Use when:** Building a complete game using <xref:AlphaFramework>.

### <xref:OmegaEngine.RenderPanel>

**RenderPanel** is a Windows Forms UserControl that hosts the engine within a larger application:

- **Embeddable** - Can be placed in forms with other controls
- **Optional render loop** - Choose between automatic rendering or manual control
- **Input handling** - Sets up <xref:OmegaEngine.Input>

**Use when:** Integrating 3D rendering into a tools application, level editor, or any app with a complex UI.

TODO: Check that sample code compiles

```csharp
// In your Form designer
var renderPanel = new RenderPanel
{
    AutoRender = true,
    AutoRenderInterval = 33 // ~30 FPS
};
this.Controls.Add(renderPanel);

// Initialize the engine
renderPanel.Setup();

var view = new View(new Scene(), new ArcballCamera());
renderPanel.Engine.Views.Add(view);

// Load and display a model
var model = Model.FromContent(renderPanel.Engine, "Models/Viewer.x");
view.Scene.Renderables.Add(model);
```

RenderPanel is ideal for:
- **Level editors** - Embed a 3D viewport alongside property grids and toolbars
- **Model viewers** - Preview 3D assets in a compact UI
- **Dashboard applications** - Show live 3D visualization as part of a monitoring interface

## Comparison

| Class | Best For | Window Type | GUI Support |
|-------|----------|-------------|-------------|
| <xref:OmegaEngine.RenderHost> | Standalone apps | Full window | Manual |
| <xref:AlphaFramework.Presentation.GameBase> | Complete games | Full window | OmegaGUI |
| <xref:OmegaEngine.RenderPanel> | Tools, editors | Embedded control | External (WinForms) |
