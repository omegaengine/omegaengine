using System.Windows.Forms;
using AlphaFramework.Presentation;
using NanoByte.Common.Storage;
using OmegaEngine.Foundation.Storage;

namespace Template.AlphaFramework;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();

        var arguments = new Arguments(args);

        if (arguments["mod"] is {} mod)
            ContentManager.ModDir = new(Path.Combine(Path.Combine(Locations.InstallBase, "Mods"), mod));

        using var game = new Game();
        game.Run();
    }
}
