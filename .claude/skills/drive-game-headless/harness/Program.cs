using System.Text;
using FrameOfReference.ScenarioTool;
using FrameOfReference.World;
using FrameOfReference.World.Positionables;
using FrameOfReference.World.Templates;
using OmegaEngine.Foundation.Storage;

Console.OutputEncoding = Encoding.UTF8;

const string SavegameExt = ".FrameOfReferenceSave";

Options options;
try
{
    options = Options.Parse(args);
}
catch (Exception ex) when (ex is ArgumentException or FormatException)
{
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine(Options.Usage);
    return 1;
}

if (options.ShowHelp)
{
    Console.WriteLine(Options.Usage);
    return 0;
}

if (options.ListScripts)
{
    foreach (var (name, (description, _)) in Scripts.All)
        Console.WriteLine($"{name}\n  {description}\n");
    return 0;
}

string contentDir = options.ContentDir ?? FindContentDir();
string dataDir = options.DataDir ?? Path.Combine(contentDir, "..", "artifacts", "Debug", "net10.0-windows", "data");

string basePath = File.Exists(options.Base) ? options.Base! : Path.Combine(dataDir, options.Base + SavegameExt);
if (!File.Exists(basePath))
{
    Console.Error.WriteLine($"No savegame at '{options.Base}' or '{basePath}'.");
    return 1;
}

string slot = options.Name!;
if (slot.Equals("Resume", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine($"'{slot}' is the slot the game itself writes. Pick another name.");
    return 1;
}
string outPath = Path.Combine(dataDir, slot + SavegameExt);
if (File.Exists(outPath) && !options.Force)
{
    Console.Error.WriteLine($"{outPath} already exists and may be one of the user's saves. Pick another name, or pass --force if it is yours.");
    return 1;
}

XmlStorageConfig.Apply();
ContentManager.BaseDir = new DirectoryInfo(contentDir);
if (options.ModDir is {} mod)
{
    // Like the game's /mod: a path, or a name in the Mods folder next to the data folder
    string modDir = Directory.Exists(mod) ? mod : Path.Combine(dataDir, "..", "Mods", mod);
    if (!Directory.Exists(modDir))
    {
        Console.Error.WriteLine($"No mod directory at '{mod}' or '{Path.GetFullPath(modDir)}'.");
        return 1;
    }
    ContentManager.ModDir = new DirectoryInfo(modDir);
}
EntityTemplate.LoadAll();
TerrainTemplate.LoadAll();

Session session;
try
{
    session = Session.Load(basePath);
    Apply(session);
}
catch (InvalidDataException ex) when (ex.GetBaseException() is KeyNotFoundException missing)
{
    // A template the save references is not in the content (or mod) folder
    Console.Error.WriteLine(missing.Message);
    Console.Error.WriteLine(options.ModDir == null
        ? "If the save was made with a mod, pass it with --mod."
        : "Is it the mod the save was made with?");
    return 1;
}
catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException or FormatException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

Directory.CreateDirectory(dataDir);
session.Save(outPath);
Console.WriteLine($"Wrote {Path.GetFullPath(outPath)}");
Describe(Session.Load(outPath));
return 0;

void Apply(Session session)
{
    var universe = session.Universe;

    if (options.LightPhase is {} lightPhase) universe.LightPhase = lightPhase;
    if (options.TimeWarp is {} timeWarp) session.TimeWarpFactor = timeWarp;
    if (options.Camera is var (position, radius, rotation))
    {
        universe.CurrentCamera = new()
        {
            Position = position,
            Radius = radius ?? universe.CurrentCamera?.Radius ?? 1500,
            Rotation = rotation ?? universe.CurrentCamera?.Rotation ?? 0
        };
    }

    var settings = new Settings(options.Settings);
    foreach (string script in options.Scripts)
    {
        if (!Scripts.All.TryGetValue(script, out var entry)) throw new ArgumentException($"No script named '{script}'. Run --list-scripts.");
        entry.Run(session, settings);
    }

    foreach (var (entity, target) in options.Positions)
        (universe.GetEntity(entity) ?? throw new KeyNotFoundException($"No entity named '{entity}'.")).Position = target;

    if (options.Player is {} player)
    {
        if (universe.GetEntity(player) == null) throw new KeyNotFoundException($"No entity named '{player}'.");
        universe.MakeAllNpc();
        universe.MakePlayerControlled(player);
    }
}

// Read back from the file, so this shows what the game will load
void Describe(Session session)
{
    var universe = session.Universe;
    Console.WriteLine($"  Map:        {session.MapSourceFile}");
    Console.WriteLine($"  Game time:  {universe.GameTime:N0} s");
    Console.WriteLine($"  Light:      phase {universe.LightPhase:0.00} (0 = dawn, 1 = noon, 2 = dusk, 3 = midnight)");
    Console.WriteLine($"  Time warp:  {session.TimeWarpFactor}");
    if (universe.CurrentCamera is {} camera)
        Console.WriteLine($"  Camera:     at {camera.Position.X:0},{camera.Position.Y:0}, radius {camera.Radius:0}, rotation {camera.Rotation:0}°");
    foreach (var entity in universe.Positionables.OfType<Entity>())
        Console.WriteLine($"  Entity:     {entity.Name ?? "(unnamed)"} ({entity.TemplateName}) at {entity.Position.X:0},{entity.Position.Y:0}{(entity.IsPlayerControlled ? ", player-controlled" : "")}");
}

// Walks up from the executable for the repository's content directory.
static string FindContentDir()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory != null)
    {
        string candidate = Path.Combine(directory.FullName, "content");
        if (Directory.Exists(Path.Combine(candidate, "World", "Maps"))) return candidate;
        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not find the game's content directory; pass --content.");
}
