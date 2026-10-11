using System.Globalization;
using FrameOfReference.World;
using FrameOfReference.World.Positionables;

namespace FrameOfReference.ScenarioTool;

/// <summary>
/// Named set-up steps for whatever the command-line options do not cover. Add one here when a test run needs it,
/// and keep the ones that are likely to be wanted again.
/// </summary>
/// <remarks>
/// A script changes the session directly, as when setting up a map: nothing is running yet that could react.
/// </remarks>
public static class Scripts
{
    public static readonly IReadOnlyDictionary<string, (string Description, Action<Session, Settings> Run)> All
        = new Dictionary<string, (string, Action<Session, Settings>)>(StringComparer.OrdinalIgnoreCase)
        {
            ["add-entity"] = (
                "Adds an entity to the map. Settings: template (required; a name from content/World/EntityTemplates.xml), "
                + "position (required; <x>,<y> in map coordinates), name (none; needed for --position and --player), "
                + "rotation (0; degrees)",
                AddEntity)
        };

    private static void AddEntity(Session session, Settings settings)
    {
        string? name = settings.Get("name");
        if (name != null && session.Universe.GetEntity(name) != null)
            throw new ArgumentException($"The save already has an entity named '{name}'. Pick another name.");

        var entity = new Entity
        {
            Name = name,
            TemplateName = settings.Required("template"),
            Position = Options.ParseCoordinates(settings.Required("position"), "position"),
            Rotation = float.Parse(settings.Get("rotation") ?? "0", CultureInfo.InvariantCulture)
        };
        session.Universe.Positionables.Add(entity);
    }
}

/// <summary>
/// The <c>--set</c> values passed to the scripts.
/// </summary>
public sealed class Settings(IReadOnlyDictionary<string, string> values)
{
    public string? Get(string key) => values.TryGetValue(key, out string? value) ? value : null;

    public string Required(string key)
        => Get(key) ?? throw new ArgumentException($"This script needs --set {key}=<value>.");
}
