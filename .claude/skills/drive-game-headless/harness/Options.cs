using System.Globalization;
using SlimDX;

namespace FrameOfReference.ScenarioTool;

/// <summary>
/// The parsed command line.
/// </summary>
public sealed class Options
{
    public const string Usage = """
        Usage: Scenario <base> <name> [options]

        Builds a savegame for a test run from an existing one, through the World model rather than by
        editing XML, so templates are really resolved and the result loads like any other save.

          <base>   A .FrameOfReferenceSave file, or the name of a slot in the Debug data folder.
          <name>   The slot to write in the Debug data folder. Resume is refused, as is any slot that
                   already exists unless --force is given: those are the user's saves.

        Applied in this order:
          --light-phase <0-4>           Set the time of day (0 = dawn, 1 = noon, 2 = dusk, 3 = midnight).
          --time-warp <factor>          Set how much faster than real time the game runs.
          --camera <x>,<y>[:<radius>[:<degrees>]]
                                        Point the camera at map coordinates. Radius and rotation default to
                                        the save's camera. The game locks the camera onto the player's entity
                                        instead, if there is one.
          --script <name>               Run a named script from Scripts.cs; repeatable. Scripts take their
                                        settings from --set.
          --set <key>=<value>           A setting for the scripts; repeatable.
          --position <entity>:<x>,<y>   Place a named entity at map coordinates; repeatable.
          --player <entity>             Last: hand the player control of a named entity, and take it away
                                        from all others.

          --force                       Overwrite an existing slot (never Resume).
          --data <dir>                  The data folder to read slots from and write to. Defaults to the
                                        Debug build's.
          --content <dir>               The game's content folder. Found automatically.
          --mod <dir>                   A mod folder overlaid on the content folder, so its templates
                                        resolve: a path, or a name in the Debug build's Mods folder. Pass
                                        the same mod to Start-Game -Mod.
          --list-scripts                List the scripts and exit.
          --help                        Show this text.
        """;

    public string? Base { get; private set; }
    public string? Name { get; private set; }
    public float? LightPhase { get; private set; }
    public double? TimeWarp { get; private set; }
    public (Vector2 Position, double? Radius, double? Rotation)? Camera { get; private set; }
    public List<string> Scripts { get; } = [];
    public Dictionary<string, string> Settings { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<(string Entity, Vector2 Position)> Positions { get; } = [];
    public string? Player { get; private set; }
    public bool Force { get; private set; }
    public string? DataDir { get; private set; }
    public string? ContentDir { get; private set; }
    public string? ModDir { get; private set; }
    public bool ListScripts { get; private set; }
    public bool ShowHelp { get; private set; }

    /// <exception cref="ArgumentException">The command line is malformed.</exception>
    public static Options Parse(IReadOnlyList<string> args)
    {
        var options = new Options();
        List<string> positional = [];

        for (int i = 0; i < args.Count; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "--light-phase": options.LightPhase = float.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--time-warp": options.TimeWarp = double.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--camera": options.Camera = ParseCamera(Value()); break;
                case "--script": options.Scripts.Add(Value()); break;
                case "--set": AddSetting(Value()); break;
                case "--position": options.Positions.Add(ParsePosition(Value())); break;
                case "--player": options.Player = Value(); break;
                case "--force": options.Force = true; break;
                case "--data": options.DataDir = Value(); break;
                case "--content": options.ContentDir = Value(); break;
                case "--mod": options.ModDir = Value(); break;
                case "--list-scripts": options.ListScripts = true; break;
                case "--help" or "-h" or "/?": options.ShowHelp = true; break;
                default:
                    if (arg.StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException($"Unknown option '{arg}'.");
                    positional.Add(arg);
                    break;
            }

            string Value()
                => ++i < args.Count ? args[i] : throw new ArgumentException($"'{arg}' needs a value.");
        }

        if (options.ShowHelp || options.ListScripts) return options;
        if (positional.Count != 2) throw new ArgumentException("Expected <base> and <name>.");
        options.Base = positional[0];
        options.Name = positional[1];
        return options;

        void AddSetting(string value)
        {
            int split = value.IndexOf('=');
            if (split <= 0) throw new ArgumentException($"--set needs <key>=<value>, not '{value}'.");
            options.Settings[value[..split]] = value[(split + 1)..];
        }
    }

    /// <summary>
    /// Parses map coordinates written as <c>&lt;x&gt;,&lt;y&gt;</c>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is malformed.</exception>
    public static Vector2 ParseCoordinates(string value, string option)
    {
        string[] parts = value.Split(',');
        if (parts.Length != 2) throw new ArgumentException($"{option} needs <x>,<y>, not '{value}'.");
        return new(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture));
    }

    private static (Vector2, double?, double?) ParseCamera(string value)
    {
        string[] parts = value.Split(':');
        if (parts.Length > 3) throw new ArgumentException($"--camera needs <x>,<y>[:<radius>[:<degrees>]], not '{value}'.");
        return (
            ParseCoordinates(parts[0], "--camera"),
            parts.Length > 1 ? (double?)double.Parse(parts[1], CultureInfo.InvariantCulture) : null,
            parts.Length > 2 ? (double?)double.Parse(parts[2], CultureInfo.InvariantCulture) : null);
    }

    private static (string, Vector2) ParsePosition(string value)
    {
        int split = value.LastIndexOf(':');
        if (split <= 0) throw new ArgumentException($"--position needs <entity>:<x>,<y>, not '{value}'.");
        return (value[..split], ParseCoordinates(value[(split + 1)..], "--position"));
    }
}
