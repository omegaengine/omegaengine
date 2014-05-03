using System;
using System.Windows.Forms;
using NanoByte.Common.Storage.SlimDX;

namespace $safeprojectname$
{
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
}
