using System.Windows.Forms;

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

        using var game = new Game();
        game.Run();
    }
}
