namespace TerrainSample.Editor
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolUniverse = new System.Windows.Forms.ToolStripSplitButton();
            this.toolUniverseEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolUniverseSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolUniverseEntityEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolUniverseItemEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.toolUniverseTerrainEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.Insert(0, this.toolUniverse);
            // 
            // toolUniverse
            // 
            this.toolUniverse.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolUniverseEditor,
            this.toolUniverseSeparator,
            this.toolUniverseEntityEditor,
            this.toolUniverseItemEditor,
            this.toolUniverseTerrainEditor});
            resources.ApplyResources(this.toolUniverse, "toolUniverse");
            this.toolUniverse.Name = "toolUniverse";
            this.toolUniverse.ButtonClick += new System.EventHandler(this.toolUniverseEditor_Click);
            // 
            // toolUniverseEditor
            // 
            resources.ApplyResources(this.toolUniverseEditor, "toolUniverseEditor");
            this.toolUniverseEditor.Name = "toolUniverseEditor";
            this.toolUniverseEditor.Click += new System.EventHandler(this.toolUniverseEditor_Click);
            // 
            // toolUniverseSeparator
            // 
            this.toolUniverseSeparator.Name = "toolUniverseSeparator";
            resources.ApplyResources(this.toolUniverseSeparator, "toolUniverseSeparator");
            // 
            // toolUniverseEntityEditor
            // 
            resources.ApplyResources(this.toolUniverseEntityEditor, "toolUniverseEntityEditor");
            this.toolUniverseEntityEditor.Name = "toolUniverseEntityEditor";
            this.toolUniverseEntityEditor.Click += new System.EventHandler(this.toolUniverseEntityEditor_Click);
            // 
            // toolUniverseItemEditor
            // 
            resources.ApplyResources(this.toolUniverseItemEditor, "toolUniverseItemEditor");
            this.toolUniverseItemEditor.Name = "toolUniverseItemEditor";
            this.toolUniverseItemEditor.Click += new System.EventHandler(this.toolUniverseItemEditor_Click);
            // 
            // toolUniverseTerrainEditor
            // 
            resources.ApplyResources(this.toolUniverseTerrainEditor, "toolUniverseTerrainEditor");
            this.toolUniverseTerrainEditor.Name = "toolUniverseTerrainEditor";
            this.toolUniverseTerrainEditor.Click += new System.EventHandler(this.toolUniverseTerrainEditor_Click);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripSplitButton toolUniverse;
        private System.Windows.Forms.ToolStripMenuItem toolUniverseEditor;
        private System.Windows.Forms.ToolStripMenuItem toolUniverseTerrainEditor;
        private System.Windows.Forms.ToolStripMenuItem toolUniverseEntityEditor;
        private System.Windows.Forms.ToolStripMenuItem toolUniverseItemEditor;
        private System.Windows.Forms.ToolStripSeparator toolUniverseSeparator;
    }
}