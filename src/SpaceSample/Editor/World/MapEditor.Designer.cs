namespace AlphaEditor.World
{
    partial class MapEditor
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
            if (disposing)
            {
                if (_presenter != null) _presenter.Dispose();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapEditor));
            this.dialogExportXml = new System.Windows.Forms.SaveFileDialog();
            this.dialogImportXml = new System.Windows.Forms.OpenFileDialog();
            this.buttonDebug = new System.Windows.Forms.Button();
            this.checkWireframe = new System.Windows.Forms.CheckBox();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonTestInGame = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripImportExport = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonImportXML = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonExportXml = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonNew = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonNewEntity = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonNewBenchmarkPoint = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCopy = new System.Windows.Forms.ToolStripButton();
            this.buttonRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMapProperties = new System.Windows.Forms.ToolStripButton();
            this.splitRender = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPagePositionables = new System.Windows.Forms.TabPage();
            this.positionablesTab = new System.Windows.Forms.SplitContainer();
            this.textSearch = new Common.Controls.HintTextBox();
            this.checkMemo = new System.Windows.Forms.CheckBox();
            this.checkBenchmarkPoint = new System.Windows.Forms.CheckBox();
            this.checkEntity = new System.Windows.Forms.CheckBox();
            this.listBoxPositionables = new System.Windows.Forms.ListBox();
            this.tabEntities = new System.Windows.Forms.TabControl();
            this.tabPageProperties = new System.Windows.Forms.TabPage();
            this.propertyGridPositionable = new System.Windows.Forms.PropertyGrid();
            this.tabPageTemplate = new System.Windows.Forms.TabPage();
            this.renderPanel = new OmegaEngine.RenderPanel();
            this.toolStrip.SuspendLayout();
            this.splitRender.Panel1.SuspendLayout();
            this.splitRender.Panel2.SuspendLayout();
            this.splitRender.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPagePositionables.SuspendLayout();
            this.positionablesTab.Panel1.SuspendLayout();
            this.positionablesTab.Panel2.SuspendLayout();
            this.positionablesTab.SuspendLayout();
            this.tabEntities.SuspendLayout();
            this.tabPageProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonUndo
            // 
            this.buttonUndo.Location = new System.Drawing.Point(697, 517);
            // 
            // buttonRedo
            // 
            this.buttonRedo.Location = new System.Drawing.Point(697, 496);
            // 
            // dialogExportXml
            // 
            this.dialogExportXml.Filter = "XML file (*.xml)|*.xml|All files (*.*)|*.*";
            this.dialogExportXml.RestoreDirectory = true;
            this.dialogExportXml.FileOk += new System.ComponentModel.CancelEventHandler(this.dialogExportXml_FileOk);
            // 
            // dialogImportXml
            // 
            this.dialogImportXml.Filter = "XML file (*.xml)|*.xml|All files (*.*)|*.*";
            this.dialogImportXml.RestoreDirectory = true;
            this.dialogImportXml.FileOk += new System.ComponentModel.CancelEventHandler(this.dialogImportXml_FileOk);
            // 
            // buttonDebug
            // 
            this.buttonDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDebug.Location = new System.Drawing.Point(617, 3);
            this.buttonDebug.Name = "buttonDebug";
            this.buttonDebug.Size = new System.Drawing.Size(74, 23);
            this.buttonDebug.TabIndex = 3;
            this.buttonDebug.Text = "&Debug";
            this.buttonDebug.UseVisualStyleBackColor = true;
            this.buttonDebug.Click += new System.EventHandler(this.buttonDebug_Click);
            // 
            // checkWireframe
            // 
            this.checkWireframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkWireframe.AutoSize = true;
            this.checkWireframe.Location = new System.Drawing.Point(537, 7);
            this.checkWireframe.Name = "checkWireframe";
            this.checkWireframe.Size = new System.Drawing.Size(74, 17);
            this.checkWireframe.TabIndex = 2;
            this.checkWireframe.Text = "&Wireframe";
            this.checkWireframe.UseVisualStyleBackColor = true;
            this.checkWireframe.CheckedChanged += new System.EventHandler(this.checkWireframe_CheckedChanged);
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonTestInGame,
            this.toolStripSeparator1,
            this.toolStripImportExport,
            this.toolStripSeparator2,
            this.buttonNew,
            this.buttonCopy,
            this.buttonRemove,
            this.toolStripSeparator3,
            this.buttonMapProperties});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(488, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip1";
            // 
            // buttonTestInGame
            // 
            this.buttonTestInGame.Image = ((System.Drawing.Image)(resources.GetObject("buttonTestInGame.Image")));
            this.buttonTestInGame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonTestInGame.Name = "buttonTestInGame";
            this.buttonTestInGame.Size = new System.Drawing.Size(98, 22);
            this.buttonTestInGame.Text = "&Test In-Game";
            this.buttonTestInGame.Click += new System.EventHandler(this.buttonTestInGame_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripImportExport
            // 
            this.toolStripImportExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonImportXML,
            this.buttonExportXml});
            this.toolStripImportExport.Image = ((System.Drawing.Image)(resources.GetObject("toolStripImportExport.Image")));
            this.toolStripImportExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripImportExport.Name = "toolStripImportExport";
            this.toolStripImportExport.Size = new System.Drawing.Size(116, 22);
            this.toolStripImportExport.Text = "&Import / Export";
            // 
            // buttonImportXML
            // 
            this.buttonImportXML.Image = ((System.Drawing.Image)(resources.GetObject("buttonImportXML.Image")));
            this.buttonImportXML.Name = "buttonImportXML";
            this.buttonImportXML.Size = new System.Drawing.Size(166, 22);
            this.buttonImportXML.Text = "Import from &XML";
            this.buttonImportXML.Click += new System.EventHandler(this.buttonImportXml_Click);
            // 
            // buttonExportXml
            // 
            this.buttonExportXml.Image = ((System.Drawing.Image)(resources.GetObject("buttonExportXml.Image")));
            this.buttonExportXml.Name = "buttonExportXml";
            this.buttonExportXml.Size = new System.Drawing.Size(166, 22);
            this.buttonExportXml.Text = "Export to X&ML";
            this.buttonExportXml.Click += new System.EventHandler(this.buttonExportXml_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonNew
            // 
            this.buttonNew.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNewEntity,
            this.buttonNewBenchmarkPoint});
            this.buttonNew.Image = ((System.Drawing.Image)(resources.GetObject("buttonNew.Image")));
            this.buttonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(60, 22);
            this.buttonNew.Text = "&New";
            // 
            // buttonNewEntity
            // 
            this.buttonNewEntity.Name = "buttonNewEntity";
            this.buttonNewEntity.Size = new System.Drawing.Size(165, 22);
            this.buttonNewEntity.Text = "&Entity";
            this.buttonNewEntity.Click += new System.EventHandler(this.buttonNewEntity_Click);
            // 
            // buttonNewBenchmarkPoint
            // 
            this.buttonNewBenchmarkPoint.Name = "buttonNewBenchmarkPoint";
            this.buttonNewBenchmarkPoint.Size = new System.Drawing.Size(165, 22);
            this.buttonNewBenchmarkPoint.Text = "&Benchmark point";
            this.buttonNewBenchmarkPoint.Click += new System.EventHandler(this.buttonNewBenchmarkPoint_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonCopy.Enabled = false;
            this.buttonCopy.Image = ((System.Drawing.Image)(resources.GetObject("buttonCopy.Image")));
            this.buttonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(23, 22);
            this.buttonCopy.Text = "Copy class...";
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemove.Image")));
            this.buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(23, 22);
            this.buttonRemove.Text = "Remove class";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonMapProperties
            // 
            this.buttonMapProperties.Image = ((System.Drawing.Image)(resources.GetObject("buttonMapProperties.Image")));
            this.buttonMapProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonMapProperties.Name = "buttonMapProperties";
            this.buttonMapProperties.Size = new System.Drawing.Size(107, 22);
            this.buttonMapProperties.Text = "Map properties";
            this.buttonMapProperties.Click += new System.EventHandler(this.buttonMapProperties_ButtonClick);
            // 
            // splitRender
            // 
            this.splitRender.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitRender.Location = new System.Drawing.Point(6, 28);
            this.splitRender.Name = "splitRender";
            // 
            // splitRender.Panel1
            // 
            this.splitRender.Panel1.Controls.Add(this.tabControl);
            this.splitRender.Panel1MinSize = 140;
            // 
            // splitRender.Panel2
            // 
            this.splitRender.Panel2.Controls.Add(this.renderPanel);
            this.splitRender.Size = new System.Drawing.Size(685, 504);
            this.splitRender.SplitterDistance = 173;
            this.splitRender.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPagePositionables);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(173, 504);
            this.tabControl.TabIndex = 0;
            // 
            // tabPagePositionables
            // 
            this.tabPagePositionables.Controls.Add(this.positionablesTab);
            this.tabPagePositionables.Location = new System.Drawing.Point(4, 22);
            this.tabPagePositionables.Name = "tabPagePositionables";
            this.tabPagePositionables.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePositionables.Size = new System.Drawing.Size(165, 478);
            this.tabPagePositionables.TabIndex = 1;
            this.tabPagePositionables.Text = "Positionables";
            this.tabPagePositionables.UseVisualStyleBackColor = true;
            // 
            // positionablesTab
            // 
            this.positionablesTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionablesTab.Location = new System.Drawing.Point(3, 3);
            this.positionablesTab.Name = "positionablesTab";
            this.positionablesTab.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // positionablesTab.Panel1
            // 
            this.positionablesTab.Panel1.Controls.Add(this.textSearch);
            this.positionablesTab.Panel1.Controls.Add(this.checkMemo);
            this.positionablesTab.Panel1.Controls.Add(this.checkBenchmarkPoint);
            this.positionablesTab.Panel1.Controls.Add(this.checkEntity);
            this.positionablesTab.Panel1.Controls.Add(this.listBoxPositionables);
            this.positionablesTab.Panel1MinSize = 69;
            // 
            // positionablesTab.Panel2
            // 
            this.positionablesTab.Panel2.Controls.Add(this.tabEntities);
            this.positionablesTab.Panel2MinSize = 150;
            this.positionablesTab.Size = new System.Drawing.Size(159, 472);
            this.positionablesTab.SplitterDistance = 240;
            this.positionablesTab.TabIndex = 0;
            // 
            // textSearch
            // 
            this.textSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textSearch.HintText = "Search";
            this.textSearch.Location = new System.Drawing.Point(0, 0);
            this.textSearch.Name = "textSearch";
            this.textSearch.ShowClearButton = true;
            this.textSearch.Size = new System.Drawing.Size(159, 20);
            this.textSearch.TabIndex = 0;
            this.textSearch.TextChanged += new System.EventHandler(this.PositionablesFilterEvent);
            // 
            // checkMemo
            // 
            this.checkMemo.AutoSize = true;
            this.checkMemo.Checked = true;
            this.checkMemo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkMemo.Location = new System.Drawing.Point(0, 40);
            this.checkMemo.Name = "checkMemo";
            this.checkMemo.Size = new System.Drawing.Size(55, 17);
            this.checkMemo.TabIndex = 2;
            this.checkMemo.Text = "Memo";
            this.checkMemo.UseVisualStyleBackColor = true;
            this.checkMemo.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
            // 
            // checkBenchmarkPoint
            // 
            this.checkBenchmarkPoint.AutoSize = true;
            this.checkBenchmarkPoint.Checked = true;
            this.checkBenchmarkPoint.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBenchmarkPoint.Location = new System.Drawing.Point(75, 22);
            this.checkBenchmarkPoint.Name = "checkBenchmarkPoint";
            this.checkBenchmarkPoint.Size = new System.Drawing.Size(80, 17);
            this.checkBenchmarkPoint.TabIndex = 1;
            this.checkBenchmarkPoint.Text = "Benchmark";
            this.checkBenchmarkPoint.UseVisualStyleBackColor = true;
            this.checkBenchmarkPoint.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
            // 
            // checkEntity
            // 
            this.checkEntity.AutoSize = true;
            this.checkEntity.Checked = true;
            this.checkEntity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEntity.Location = new System.Drawing.Point(0, 22);
            this.checkEntity.Name = "checkEntity";
            this.checkEntity.Size = new System.Drawing.Size(52, 17);
            this.checkEntity.TabIndex = 0;
            this.checkEntity.Text = "Entity";
            this.checkEntity.UseVisualStyleBackColor = true;
            this.checkEntity.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
            // 
            // listBoxPositionables
            // 
            this.listBoxPositionables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPositionables.FormattingEnabled = true;
            this.listBoxPositionables.Location = new System.Drawing.Point(0, 65);
            this.listBoxPositionables.Name = "listBoxPositionables";
            this.listBoxPositionables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxPositionables.Size = new System.Drawing.Size(159, 173);
            this.listBoxPositionables.Sorted = true;
            this.listBoxPositionables.TabIndex = 3;
            this.listBoxPositionables.SelectedIndexChanged += new System.EventHandler(this.listBoxPositionables_SelectedIndexChanged);
            // 
            // tabEntities
            // 
            this.tabEntities.Controls.Add(this.tabPageProperties);
            this.tabEntities.Controls.Add(this.tabPageTemplate);
            this.tabEntities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabEntities.Location = new System.Drawing.Point(0, 0);
            this.tabEntities.Name = "tabEntities";
            this.tabEntities.SelectedIndex = 0;
            this.tabEntities.Size = new System.Drawing.Size(159, 228);
            this.tabEntities.TabIndex = 0;
            // 
            // tabPageProperties
            // 
            this.tabPageProperties.Controls.Add(this.propertyGridPositionable);
            this.tabPageProperties.Location = new System.Drawing.Point(4, 22);
            this.tabPageProperties.Name = "tabPageProperties";
            this.tabPageProperties.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageProperties.Size = new System.Drawing.Size(151, 202);
            this.tabPageProperties.TabIndex = 0;
            this.tabPageProperties.Text = "Properties";
            this.tabPageProperties.UseVisualStyleBackColor = true;
            // 
            // propertyGridPositionable
            // 
            this.propertyGridPositionable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridPositionable.Location = new System.Drawing.Point(3, 3);
            this.propertyGridPositionable.Margin = new System.Windows.Forms.Padding(0);
            this.propertyGridPositionable.Name = "propertyGridPositionable";
            this.propertyGridPositionable.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridPositionable.Size = new System.Drawing.Size(145, 196);
            this.propertyGridPositionable.TabIndex = 2;
            this.propertyGridPositionable.ToolbarVisible = false;
            this.propertyGridPositionable.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridPositionable_PropertyValueChanged);
            // 
            // tabPageTemplate
            // 
            this.tabPageTemplate.Location = new System.Drawing.Point(4, 22);
            this.tabPageTemplate.Name = "tabPageTemplate";
            this.tabPageTemplate.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTemplate.Size = new System.Drawing.Size(151, 202);
            this.tabPageTemplate.TabIndex = 1;
            this.tabPageTemplate.Text = "Template";
            this.tabPageTemplate.UseVisualStyleBackColor = true;
            // 
            // renderPanel
            // 
            this.renderPanel.AutoRender = true;
            this.renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderPanel.Location = new System.Drawing.Point(0, 0);
            this.renderPanel.Name = "renderPanel";
            this.renderPanel.Size = new System.Drawing.Size(508, 504);
            this.renderPanel.TabIndex = 0;
            // 
            // MapEditor
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.splitRender);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.checkWireframe);
            this.Controls.Add(this.buttonDebug);
            this.Name = "MapEditor";
            this.NameUI = "Map Editor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.buttonDebug, 0);
            this.Controls.SetChildIndex(this.checkWireframe, 0);
            this.Controls.SetChildIndex(this.toolStrip, 0);
            this.Controls.SetChildIndex(this.splitRender, 0);
            this.Controls.SetChildIndex(this.buttonRedo, 0);
            this.Controls.SetChildIndex(this.buttonUndo, 0);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.splitRender.Panel1.ResumeLayout(false);
            this.splitRender.Panel2.ResumeLayout(false);
            this.splitRender.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPagePositionables.ResumeLayout(false);
            this.positionablesTab.Panel1.ResumeLayout(false);
            this.positionablesTab.Panel1.PerformLayout();
            this.positionablesTab.Panel2.ResumeLayout(false);
            this.positionablesTab.ResumeLayout(false);
            this.tabEntities.ResumeLayout(false);
            this.tabPageProperties.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonDebug;
        private System.Windows.Forms.CheckBox checkWireframe;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripDropDownButton toolStripImportExport;
        private System.Windows.Forms.ToolStripMenuItem buttonExportXml;
        private System.Windows.Forms.ToolStripMenuItem buttonImportXML;
        private System.Windows.Forms.ToolStripButton buttonTestInGame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem buttonNewBenchmarkPoint;
        private System.Windows.Forms.ToolStripButton buttonRemove;
        private System.Windows.Forms.ToolStripButton buttonCopy;
        private System.Windows.Forms.OpenFileDialog dialogImportXml;
        private System.Windows.Forms.SaveFileDialog dialogExportXml;
        private System.Windows.Forms.SplitContainer splitRender;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPagePositionables;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton buttonNew;
        private System.Windows.Forms.ToolStripMenuItem buttonNewEntity;
        private System.Windows.Forms.SplitContainer positionablesTab;
        private System.Windows.Forms.ListBox listBoxPositionables;
        private System.Windows.Forms.TabControl tabEntities;
        private System.Windows.Forms.TabPage tabPageProperties;
        private System.Windows.Forms.TabPage tabPageTemplate;
        private System.Windows.Forms.PropertyGrid propertyGridPositionable;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.CheckBox checkMemo;
        private System.Windows.Forms.CheckBox checkBenchmarkPoint;
        private System.Windows.Forms.CheckBox checkEntity;
        private Common.Controls.HintTextBox textSearch;
        private System.Windows.Forms.ToolStripButton buttonMapProperties;
        private OmegaEngine.RenderPanel renderPanel;
    }
}
