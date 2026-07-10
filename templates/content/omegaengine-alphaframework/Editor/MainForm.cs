using AlphaFramework.Editor;
using NanoByte.Common;
using NanoByte.Common.Controls;
using Template.AlphaFramework.Editor.World;
using Template.AlphaFramework.Presentation.Config;
using Template.AlphaFramework.World;
using Resources = AlphaFramework.Editor.Properties.Resources;

namespace Template.AlphaFramework.Editor;

/// <summary>
/// The main window of the editor, container for the editor tabs.
/// </summary>
public sealed partial class MainForm : MainFormBase
{
    /// <inheritdoc/>
    public MainForm()
    {
        InitializeComponent();

        switch (Settings.Current.General.Language)
        {
            case "de":
                menuLanguageGerman.Checked = true;
                break;
            default:
                menuLanguageEnglish.Checked = true;
                break;
        }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        // Open any map files passed as command-line arguments
        foreach (string file in Environment.GetCommandLineArgs().Where(file => file.EndsWith(Constants.MapFileExt, StringComparison.OrdinalIgnoreCase)))
            AddTab(new MapEditor(file, false));
    }

    private void toolMap_Click(object sender, EventArgs e)
        => OpenFileTab("World/Maps", Constants.MapFileExt, (path, overwrite) => new MapEditor(path, overwrite));

    /// <inheritdoc/>
    protected override void Restart()
    {
        Program.Restart = true;
        Close();
    }

    /// <inheritdoc/>
    protected override void LaunchGame(params string[] arguments)
        => Program.LaunchGame(arguments);

    /// <inheritdoc/>
    protected override void ChangeLanguage(string language)
    {
        // Set language and propagate change
        Settings.Current.General.Language = language;
        Program.UpdateLocale();

        // Refresh tab to update language-related stuff
        CurrentTab?.Open(this);

        // Inform user he/she needs to close the Main Form to refresh all locales
        if (Msg.YesNo(this, Resources.CloseModForLangChange, MsgSeverity.Info, Resources.CloseModForLangChangeYes, Resources.CloseModForLangChangeNo))
            Restart();
    }
}
