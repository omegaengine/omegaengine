using AlphaFramework.Editor;
using Common;
using Resources = AlphaFramework.Editor.Properties.Resources;

namespace $safeprojectname$
{
    partial class MainForm : MainFormBase
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

        /// <summary>
        /// Launches the main game with the currently active mod (arguments automatically set).
        /// </summary>
        protected override void LaunchGame(string arguments)
        {
            // TODO: Implement
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
