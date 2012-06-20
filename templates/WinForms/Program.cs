﻿using System;
using System.Windows.Forms;
using Common.Storage;

namespace WinForms
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
            Application.Run(new MainForm());
            ContentManager.CloseArchives();
        }
    }
}
