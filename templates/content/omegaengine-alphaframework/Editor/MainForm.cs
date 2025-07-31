using AlphaFramework.Editor;
using NanoByte.Common.Controls;
using Resources = AlphaFramework.Editor.Properties.Resources;

namespace Template.AlphaFramework.Editor
{
    /// <summary>
    /// The main window of the editor, container for the editor tabs.
    /// </summary>
    public sealed partial class MainForm : MainFormBase
    {
        public MainForm()
        {
            InitializeComponent();

            switch (Program.Language)
            {
                case "de":
                    menuLanguageGerman.Checked = true;
                    break;
                default:
                    menuLanguageEnglish.Checked = true;
                    break;
            }
        }

        protected override void Restart()
        {
            Program.Restart = true;
            Close();
        }

        protected override void LaunchGame(string arguments)
        {
            Program.LaunchGame(arguments);
        }

        protected override void ChangeLanguage(string language)
        {
            // Set language and propagate change
            Program.Language = language;
            Program.UpdateLocale();

            // Refresh tab to update language-related stuff
            if (CurrentTab != null) CurrentTab.Open(this);

            // Inform user he/she needs to close the Main Form to refresh all locales
            if (Msg.YesNo(this, Resources.CloseModForLangChange, MsgSeverity.Info, Resources.CloseModForLangChangeYes, Resources.CloseModForLangChangeNo))
                Restart();
        }
    }
}
