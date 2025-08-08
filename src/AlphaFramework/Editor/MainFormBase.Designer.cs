namespace AlphaFramework.Editor
{
    partial class MainFormBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFormBase));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolParticleSystem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolGui = new System.Windows.Forms.ToolStripSplitButton();
            this.toolGuiEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolGuiLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.toolGuiSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolGuiCutscene = new System.Windows.Forms.ToolStripMenuItem();
            this.tabStrip = new System.Windows.Forms.ToolStrip();
            this.tabPanel = new System.Windows.Forms.Panel();
            this.labelToast = new System.Windows.Forms.Label();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileCloseTab = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSwitchTab = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileModProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFilePackageMod = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileTestInGame = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuFileSwitchMod = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuEditDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLanguage = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLanguageEnglish = new System.Windows.Forms.ToolStripMenuItem();
            this.menuLanguageGerman = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.timerToastReset = new System.Windows.Forms.Timer(this.components);
            this.toolStrip.SuspendLayout();
            this.tabPanel.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolParticleSystem,
            this.toolGui});
            resources.ApplyResources(this.toolStrip, "toolStrip");
            this.toolStrip.Name = "toolStrip";
            // 
            // toolParticleSystem
            // 
            resources.ApplyResources(this.toolParticleSystem, "toolParticleSystem");
            this.toolParticleSystem.Name = "toolParticleSystem";
            this.toolParticleSystem.Click += new System.EventHandler(this.toolParticleSystem_Click);
            // 
            // toolGui
            // 
            this.toolGui.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolGuiEditor,
            this.toolGuiLanguage,
            this.toolGuiSeparator,
            this.toolGuiCutscene});
            resources.ApplyResources(this.toolGui, "toolGui");
            this.toolGui.Name = "toolGui";
            this.toolGui.ButtonClick += new System.EventHandler(this.toolGuiEditor_Click);
            // 
            // toolGuiEditor
            // 
            resources.ApplyResources(this.toolGuiEditor, "toolGuiEditor");
            this.toolGuiEditor.Name = "toolGuiEditor";
            this.toolGuiEditor.Click += new System.EventHandler(this.toolGuiEditor_Click);
            // 
            // toolGuiLanguage
            // 
            resources.ApplyResources(this.toolGuiLanguage, "toolGuiLanguage");
            this.toolGuiLanguage.Name = "toolGuiLanguage";
            this.toolGuiLanguage.Click += new System.EventHandler(this.toolGuiLanguage_Click);
            // 
            // toolGuiSeparator
            // 
            this.toolGuiSeparator.Name = "toolGuiSeparator";
            resources.ApplyResources(this.toolGuiSeparator, "toolGuiSeparator");
            // 
            // toolGuiCutscene
            // 
            resources.ApplyResources(this.toolGuiCutscene, "toolGuiCutscene");
            this.toolGuiCutscene.Name = "toolGuiCutscene";
            this.toolGuiCutscene.Click += new System.EventHandler(this.toolGuiCutscene_Click);
            // 
            // tabStrip
            // 
            resources.ApplyResources(this.tabStrip, "tabStrip");
            this.tabStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tabStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.tabStrip.Name = "tabStrip";
            // 
            // tabPanel
            // 
            this.tabPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tabPanel.Controls.Add(this.labelToast);
            resources.ApplyResources(this.tabPanel, "tabPanel");
            this.tabPanel.Name = "tabPanel";
            // 
            // labelToast
            // 
            resources.ApplyResources(this.labelToast, "labelToast");
            this.labelToast.AutoEllipsis = true;
            this.labelToast.BackColor = System.Drawing.SystemColors.ControlLight;
            this.labelToast.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelToast.Image = global::AlphaFramework.Editor.Properties.Resources.Information;
            this.labelToast.Name = "labelToast";
            this.labelToast.Click += new System.EventHandler(this.labelToast_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuEdit,
            this.menuLanguage,
            this.menuHelp});
            resources.ApplyResources(this.menuStrip, "menuStrip");
            this.menuStrip.Name = "menuStrip";
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileCloseTab,
            this.menuFileSwitchTab,
            this.menuFileSave,
            this.menuFileSeparator1,
            this.menuFileModProperties,
            this.menuFilePackageMod,
            this.menuFileTestInGame,
            this.menuFileSeparator2,
            this.menuFileSwitchMod,
            this.menuFileExit});
            this.menuFile.Name = "menuFile";
            resources.ApplyResources(this.menuFile, "menuFile");
            // 
            // menuFileCloseTab
            // 
            resources.ApplyResources(this.menuFileCloseTab, "menuFileCloseTab");
            this.menuFileCloseTab.Name = "menuFileCloseTab";
            this.menuFileCloseTab.Click += new System.EventHandler(this.menuFileCloseTab_Click);
            // 
            // menuFileSwitchTab
            // 
            resources.ApplyResources(this.menuFileSwitchTab, "menuFileSwitchTab");
            this.menuFileSwitchTab.Name = "menuFileSwitchTab";
            this.menuFileSwitchTab.Click += new System.EventHandler(this.menuFileSwitchTab_Click);
            // 
            // menuFileSave
            // 
            resources.ApplyResources(this.menuFileSave, "menuFileSave");
            this.menuFileSave.Image = global::AlphaFramework.Editor.Properties.Resources.SaveButton;
            this.menuFileSave.Name = "menuFileSave";
            this.menuFileSave.Click += new System.EventHandler(this.menuFileSave_Click);
            // 
            // menuFileSeparator1
            // 
            this.menuFileSeparator1.Name = "menuFileSeparator1";
            resources.ApplyResources(this.menuFileSeparator1, "menuFileSeparator1");
            // 
            // menuFileModProperties
            // 
            resources.ApplyResources(this.menuFileModProperties, "menuFileModProperties");
            this.menuFileModProperties.Name = "menuFileModProperties";
            this.menuFileModProperties.Click += new System.EventHandler(this.menuFileModProperties_Click);
            // 
            // menuFilePackageMod
            // 
            resources.ApplyResources(this.menuFilePackageMod, "menuFilePackageMod");
            this.menuFilePackageMod.Name = "menuFilePackageMod";
            this.menuFilePackageMod.Click += new System.EventHandler(this.menuFilePackageMod_Click);
            // 
            // menuFileTestInGame
            // 
            this.menuFileTestInGame.Image = global::AlphaFramework.Editor.Properties.Resources.RunButton;
            this.menuFileTestInGame.Name = "menuFileTestInGame";
            resources.ApplyResources(this.menuFileTestInGame, "menuFileTestInGame");
            this.menuFileTestInGame.Click += new System.EventHandler(this.menuFileTestInGame_Click);
            // 
            // menuFileSeparator2
            // 
            this.menuFileSeparator2.Name = "menuFileSeparator2";
            resources.ApplyResources(this.menuFileSeparator2, "menuFileSeparator2");
            // 
            // menuFileSwitchMod
            // 
            this.menuFileSwitchMod.Name = "menuFileSwitchMod";
            resources.ApplyResources(this.menuFileSwitchMod, "menuFileSwitchMod");
            this.menuFileSwitchMod.Click += new System.EventHandler(this.menuFileSwitchMod_Click);
            // 
            // menuFileExit
            // 
            this.menuFileExit.Name = "menuFileExit";
            resources.ApplyResources(this.menuFileExit, "menuFileExit");
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // menuEdit
            // 
            this.menuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuEditUndo,
            this.menuEditRedo,
            this.menuEditSeparator1,
            this.menuEditDelete});
            this.menuEdit.Name = "menuEdit";
            resources.ApplyResources(this.menuEdit, "menuEdit");
            // 
            // menuEditUndo
            // 
            resources.ApplyResources(this.menuEditUndo, "menuEditUndo");
            this.menuEditUndo.Image = global::AlphaFramework.Editor.Properties.Resources.UndoButton;
            this.menuEditUndo.Name = "menuEditUndo";
            this.menuEditUndo.Click += new System.EventHandler(this.menuEditUndo_Click);
            // 
            // menuEditRedo
            // 
            resources.ApplyResources(this.menuEditRedo, "menuEditRedo");
            this.menuEditRedo.Image = global::AlphaFramework.Editor.Properties.Resources.RedoButton;
            this.menuEditRedo.Name = "menuEditRedo";
            this.menuEditRedo.Click += new System.EventHandler(this.menuEditRedo_Click);
            // 
            // menuEditSeparator1
            // 
            this.menuEditSeparator1.Name = "menuEditSeparator1";
            resources.ApplyResources(this.menuEditSeparator1, "menuEditSeparator1");
            // 
            // menuEditDelete
            // 
            resources.ApplyResources(this.menuEditDelete, "menuEditDelete");
            this.menuEditDelete.Image = global::AlphaFramework.Editor.Properties.Resources.DeleteButton;
            this.menuEditDelete.Name = "menuEditDelete";
            this.menuEditDelete.Click += new System.EventHandler(this.menuEditDelete_Click);
            // 
            // menuLanguage
            // 
            this.menuLanguage.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuLanguageEnglish,
            this.menuLanguageGerman});
            this.menuLanguage.Name = "menuLanguage";
            resources.ApplyResources(this.menuLanguage, "menuLanguage");
            // 
            // menuLanguageEnglish
            // 
            this.menuLanguageEnglish.Name = "menuLanguageEnglish";
            resources.ApplyResources(this.menuLanguageEnglish, "menuLanguageEnglish");
            this.menuLanguageEnglish.Click += new System.EventHandler(this.menuLanguageEnglish_Click);
            // 
            // menuLanguageGerman
            // 
            this.menuLanguageGerman.Name = "menuLanguageGerman";
            resources.ApplyResources(this.menuLanguageGerman, "menuLanguageGerman");
            this.menuLanguageGerman.Click += new System.EventHandler(this.menuLanguageGerman_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHelpAbout});
            this.menuHelp.Name = "menuHelp";
            resources.ApplyResources(this.menuHelp, "menuHelp");
            // 
            // menuHelpAbout
            // 
            this.menuHelpAbout.Name = "menuHelpAbout";
            resources.ApplyResources(this.menuHelpAbout, "menuHelpAbout");
            this.menuHelpAbout.Click += new System.EventHandler(this.menuHelpAbout_Click);
            // 
            // timerToastReset
            // 
            this.timerToastReset.Interval = 5000;
            this.timerToastReset.Tick += new System.EventHandler(this.timerToastReset_Tick);
            // 
            // MainFormBase
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabPanel);
            this.Controls.Add(this.tabStrip);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Load += new System.EventHandler(this.MainFormBase_Load);
            this.Name = "MainFormBase";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.tabPanel.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuFileSave;
        private System.Windows.Forms.ToolStripMenuItem menuFileSwitchMod;
        protected System.Windows.Forms.ToolStripMenuItem menuFileModProperties;
        private System.Windows.Forms.ToolStripSeparator menuFileSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuFileTestInGame;
        protected System.Windows.Forms.ToolStripMenuItem menuFilePackageMod;
        private System.Windows.Forms.ToolStripSeparator menuFileSeparator2;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem menuEdit;
        private System.Windows.Forms.ToolStripMenuItem menuEditUndo;
        private System.Windows.Forms.ToolStripMenuItem menuEditRedo;
        private System.Windows.Forms.ToolStripSeparator menuEditSeparator1;
        private System.Windows.Forms.ToolStripMenuItem menuEditDelete;
        private System.Windows.Forms.ToolStripMenuItem menuLanguage;
        protected System.Windows.Forms.ToolStripMenuItem menuLanguageEnglish;
        protected System.Windows.Forms.ToolStripMenuItem menuLanguageGerman;
        private System.Windows.Forms.ToolStripSplitButton toolGui;
        private System.Windows.Forms.ToolStripMenuItem toolGuiLanguage;
        private System.Windows.Forms.ToolStripMenuItem toolGuiCutscene;
        private System.Windows.Forms.ToolStripMenuItem toolParticleSystem;
        private System.Windows.Forms.ToolStripMenuItem toolGuiEditor;
        private System.Windows.Forms.Panel tabPanel;
        private System.Windows.Forms.ToolStrip tabStrip;
        private System.Windows.Forms.ToolStripSeparator toolGuiSeparator;
        private System.Windows.Forms.ToolStripMenuItem menuFileCloseTab;
        private System.Windows.Forms.ToolStripMenuItem menuFileSwitchTab;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuHelpAbout;
        private System.Windows.Forms.Label labelToast;
        private System.Windows.Forms.Timer timerToastReset;
        protected System.Windows.Forms.ToolStrip toolStrip;
    }
}