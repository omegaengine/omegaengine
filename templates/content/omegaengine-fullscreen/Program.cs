using System;
using System.Windows.Forms;
using OmegaEngine.Foundation.Storage;

namespace Template.Fullscreen;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        ContentManager.LoadArchives();
        using (var game = new Game()) game.Run();
        ContentManager.CloseArchives();
    }
}