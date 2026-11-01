using System.Globalization;
using SlimDX;

namespace FrameOfReference.ScenarioTool;

/// <summary>
/// The parsed command line.
/// </summary>
public sealed class Options
{
    public const string Usage = """
        Usage: Scenario <base> [<name>] [options]
               Scenario --new <map> [<name>] [options]

        Builds a savegame for a test run from an existing one or a new game, through the World model
        rather than by editing XML, so templates are really resolved and the result loads like any other
        save.

          <base>        A .FrameOfReferenceSave file, or the name of a slot in the Debug data folder.
          --new <map>   Start from a new game on a map instead, as the main menu (Tutorial) or the game's
                        /map does: a name in the content's (or the mod's) World/Maps, or a
                        .FrameOfReferenceMap file.
          <name>        The slot to write in the Debug data folder. Resume is refused, as is any slot that
                        already exists unless --force is given: those are the user's saves. Without it,
                        nothing is written: the steps below run and the result is only shown.

        Applied in this order:
          --light-phase <phase>         Set the time of day (0 = dawn, 1 = noon, 2 = dusk, 3 = midnight,
                                        fractions in between; wraps around at 4).
          --time-warp <factor>          Set how much faster than real time the game runs.
          --camera <x>,<y>[:<radius>[:<degrees>]]
                                        Point the camera at map coordinates. Radius and rotation default to
                                        the save's camera. The game locks the camera onto the player's entity
                                        instead, if there is one.
          --script <name>               Run a named script from Scripts.cs; repeatable. Scripts take their
                                        settings from --set.
          --set <key>=<value>           A setting for the --script it follows, or for every script if it
                                        comes before the first --script; repeatable. A script's own
                                        settings override the shared ones.
          --position <entity>:<x>,<y>   Place a named entity at map coordinates; repeatable.
          --player <entity>             Last: hand the player control of a named entity, and take it away
                                        from all others.

          --force                       Overwrite an existing slot (never Resume).
          --data <dir>                  The data folder to read slots from and write to. Defaults to the
                                        Debug build's.
          --content <dir>               The game's content folder. Found automatically.
          --mod <dir>                   A mod folder overlaid on the content folder, so its templates and
                                        maps resolve: a path, or a name in the Debug build's Mods folder.
                                        Pass the same mod to Invoke-GameRun -Mod.
          --list-scripts                List the scripts and exit.
          --help                        Show this text.
        """;

    public string? Base { get; private set; }
    public string? NewMap { get; private set; }
    /// <summary>The slot to write, or <c>null</c> to write nothing.</summary>
    public string? Name { get; private set; }
    public float? LightPhase { get; private set; }
    public double? TimeWarp { get; private set; }
    public (Vector2 Position, double? Radius, double? Rotation)? Camera { get; private set; }
    /// <summary>The scripts to run, in order, each with the settings that apply to it.</summary>
    public List<(string Name, Dictionary<string, string> Settings)> Scripts { get; } = [];

    /// <summary>Settings given before the first --script, shared by all scripts.</summary>
    private readonly Dictionary<string, string> _sharedSettings = new(StringComparer.OrdinalIgnoreCase);
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
                case "--new": options.NewMap = Value(); break;
                case "--light-phase": options.LightPhase = float.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--time-warp": options.TimeWarp = double.Parse(Value(), CultureInfo.InvariantCulture); break;
                case "--camera": options.Camera = ParseCamera(Value()); break;
                case "--script": options.Scripts.Add((Value(), new(options._sharedSettings, StringComparer.OrdinalIgnoreCase))); break;
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
        if (options.NewMap == null)
        {
            if (positional.Count == 0) throw new ArgumentException("Expected <base> or --new <map>.");
            options.Base = positional[0];
            positional.RemoveAt(0);
        }
        if (positional.Count > 1) throw new ArgumentException($"Unexpected argument '{positional[1]}'.");
        options.Name = positional.FirstOrDefault();
        return options;

        void AddSetting(string value)
        {
            int split = value.IndexOf('=');
            if (split <= 0) throw new ArgumentException($"--set needs <key>=<value>, not '{value}'.");
            var settings = options.Scripts is [.., var (_, current)] ? current : options._sharedSettings;
            settings[value[..split]] = value[(split + 1)..];
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
