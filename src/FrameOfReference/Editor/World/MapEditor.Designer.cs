using NanoByte.Common.Controls;

namespace FrameOfReference.Editor.World
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
            this.buttonDebug = new System.Windows.Forms.Button();
            this.checkWireframe = new System.Windows.Forms.CheckBox();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonTestInGame = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripImportExport = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonImportHeightMap = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonExportHeightMap = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripImportExportSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonImportTextureMap = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonExportTextureMap = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonNew = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonNewEntity = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonNewWaypoint = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonNewWater = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonNewCameraState = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonNewBenchmarkPoint = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCopy = new System.Windows.Forms.ToolStripButton();
            this.buttonRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMapProperties = new System.Windows.Forms.ToolStripSplitButton();
            this.buttonCameraStatupPerspective = new System.Windows.Forms.ToolStripMenuItem();
            this.dialogExportHeightMap = new System.Windows.Forms.SaveFileDialog();
            this.dialogImportHeightMap = new System.Windows.Forms.OpenFileDialog();
            this.dialogExportTextureMap = new System.Windows.Forms.SaveFileDialog();
            this.dialogImportTextureMap = new System.Windows.Forms.OpenFileDialog();
            this.splitVertical = new System.Windows.Forms.SplitContainer();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPagePositionables = new System.Windows.Forms.TabPage();
            this.positionablesTab = new System.Windows.Forms.SplitContainer();
            this.checkCameraState = new System.Windows.Forms.CheckBox();
            this.textSearch = new NanoByte.Common.Controls.HintTextBox();
            this.checkMemo = new System.Windows.Forms.CheckBox();
            this.checkBenchmarkPoint = new System.Windows.Forms.CheckBox();
            this.checkWater = new System.Windows.Forms.CheckBox();
            this.checkWaypoint = new System.Windows.Forms.CheckBox();
            this.checkEntity = new System.Windows.Forms.CheckBox();
            this.listBoxPositionables = new System.Windows.Forms.ListBox();
            this.tabControlEntities = new System.Windows.Forms.TabControl();
            this.tabPageProperties = new System.Windows.Forms.TabPage();
            this.propertyGridPositionable = new System.Windows.Forms.PropertyGrid();
            this.tabPageTemplate = new System.Windows.Forms.TabPage();
            this.tabPageHeight = new System.Windows.Forms.TabPage();
            this.upDownHeightNoiseFrequency = new System.Windows.Forms.NumericUpDown();
            this.upDownHeightStrength = new System.Windows.Forms.NumericUpDown();
            this.groupHeightShape = new System.Windows.Forms.GroupBox();
            this.radioHeightShapeSquare = new System.Windows.Forms.RadioButton();
            this.radioHeightShapeCircle = new System.Windows.Forms.RadioButton();
            this.upDownHeightSize = new System.Windows.Forms.NumericUpDown();
            this.labelHeightShapeSize = new System.Windows.Forms.Label();
            this.labelHeightStrength = new System.Windows.Forms.Label();
            this.radioHeightSmooth = new System.Windows.Forms.RadioButton();
            this.radioHeightNoise = new System.Windows.Forms.RadioButton();
            this.radioHeightRaise = new System.Windows.Forms.RadioButton();
            this.radioHeightPlateau = new System.Windows.Forms.RadioButton();
            this.radioHeightLower = new System.Windows.Forms.RadioButton();
            this.tabPageTexture = new System.Windows.Forms.TabPage();
            this.groupTextureShape = new System.Windows.Forms.GroupBox();
            this.radioTextureShapeSquare = new System.Windows.Forms.RadioButton();
            this.radioTextureShapeCircle = new System.Windows.Forms.RadioButton();
            this.upDownTextureSize = new System.Windows.Forms.NumericUpDown();
            this.labelTerrainSize = new System.Windows.Forms.Label();
            this.splitHorizontal = new System.Windows.Forms.SplitContainer();
            this.renderPanel = new OmegaEngine.RenderPanel();
            this.xmlEditor = new NanoByte.Common.Controls.LiveEditor();
            this.toolStrip.SuspendLayout();
            this.splitVertical.Panel1.SuspendLayout();
            this.splitVertical.Panel2.SuspendLayout();
            this.splitVertical.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPagePositionables.SuspendLayout();
            this.positionablesTab.Panel1.SuspendLayout();
            this.positionablesTab.Panel2.SuspendLayout();
            this.positionablesTab.SuspendLayout();
            this.tabControlEntities.SuspendLayout();
            this.tabPageProperties.SuspendLayout();
            this.tabPageHeight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHeightNoiseFrequency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHeightStrength)).BeginInit();
            this.groupHeightShape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHeightSize)).BeginInit();
            this.tabPageTexture.SuspendLayout();
            this.groupTextureShape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownTextureSize)).BeginInit();
            this.splitHorizontal.Panel1.SuspendLayout();
            this.splitHorizontal.Panel2.SuspendLayout();
            this.splitHorizontal.SuspendLayout();
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
            this.toolStrip.Size = new System.Drawing.Size(469, 25);
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
            this.buttonImportHeightMap,
            this.buttonExportHeightMap,
            this.toolStripImportExportSeparator2,
            this.buttonImportTextureMap,
            this.buttonExportTextureMap});
            this.toolStripImportExport.Image = ((System.Drawing.Image)(resources.GetObject("toolStripImportExport.Image")));
            this.toolStripImportExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripImportExport.Name = "toolStripImportExport";
            this.toolStripImportExport.Size = new System.Drawing.Size(116, 22);
            this.toolStripImportExport.Text = "&Import / Export";
            // 
            // buttonImportHeightMap
            // 
            this.buttonImportHeightMap.Image = ((System.Drawing.Image)(resources.GetObject("buttonImportHeightMap.Image")));
            this.buttonImportHeightMap.Name = "buttonImportHeightMap";
            this.buttonImportHeightMap.Size = new System.Drawing.Size(178, 22);
            this.buttonImportHeightMap.Text = "Import &height-map";
            this.buttonImportHeightMap.Click += new System.EventHandler(this.buttonImportHeightMap_Click);
            // 
            // buttonExportHeightMap
            // 
            this.buttonExportHeightMap.Image = ((System.Drawing.Image)(resources.GetObject("buttonExportHeightMap.Image")));
            this.buttonExportHeightMap.Name = "buttonExportHeightMap";
            this.buttonExportHeightMap.Size = new System.Drawing.Size(178, 22);
            this.buttonExportHeightMap.Text = "Export h&eight-map";
            this.buttonExportHeightMap.Click += new System.EventHandler(this.buttonExportHeightMap_Click);
            // 
            // toolStripImportExportSeparator2
            // 
            this.toolStripImportExportSeparator2.Name = "toolStripImportExportSeparator2";
            this.toolStripImportExportSeparator2.Size = new System.Drawing.Size(175, 6);
            // 
            // buttonImportTextureMap
            // 
            this.buttonImportTextureMap.Image = ((System.Drawing.Image)(resources.GetObject("buttonImportTextureMap.Image")));
            this.buttonImportTextureMap.Name = "buttonImportTextureMap";
            this.buttonImportTextureMap.Size = new System.Drawing.Size(178, 22);
            this.buttonImportTextureMap.Text = "Import &texture-map";
            this.buttonImportTextureMap.Click += new System.EventHandler(this.buttonImportTextureMap_Click);
            // 
            // buttonExportTextureMap
            // 
            this.buttonExportTextureMap.Image = ((System.Drawing.Image)(resources.GetObject("buttonExportTextureMap.Image")));
            this.buttonExportTextureMap.Name = "buttonExportTextureMap";
            this.buttonExportTextureMap.Size = new System.Drawing.Size(178, 22);
            this.buttonExportTextureMap.Text = "Export te&xture-map";
            this.buttonExportTextureMap.Click += new System.EventHandler(this.buttonExportTextureMap_Click);
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
            this.buttonNewWater,
            this.buttonNewWaypoint,
            this.buttonNewCameraState,
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
            // buttonNewWaypoint
            // 
            this.buttonNewWaypoint.Name = "buttonNewWaypoint";
            this.buttonNewWaypoint.Size = new System.Drawing.Size(165, 22);
            this.buttonNewWaypoint.Text = "&Waypoint";
            this.buttonNewWaypoint.Click += new System.EventHandler(this.buttonNewWaypoint_Click);
            // 
            // buttonNewWater
            // 
            this.buttonNewWater.Name = "buttonNewWater";
            this.buttonNewWater.Size = new System.Drawing.Size(165, 22);
            this.buttonNewWater.Text = "&Water";
            this.buttonNewWater.Click += new System.EventHandler(this.buttonNewWater_Click);
            // 
            // buttonNewCameraState
            // 
            this.buttonNewCameraState.Name = "buttonNewCameraState";
            this.buttonNewCameraState.Size = new System.Drawing.Size(165, 22);
            this.buttonNewCameraState.Text = "&Camera state";
            this.buttonNewCameraState.Click += new System.EventHandler(this.buttonNewCameraState_Click);
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
            this.buttonCopy.Text = "Copy";
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
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonMapProperties
            // 
            this.buttonMapProperties.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonCameraStatupPerspective});
            this.buttonMapProperties.Image = ((System.Drawing.Image)(resources.GetObject("buttonMapProperties.Image")));
            this.buttonMapProperties.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonMapProperties.Name = "buttonMapProperties";
            this.buttonMapProperties.Size = new System.Drawing.Size(119, 22);
            this.buttonMapProperties.Text = "Map properties";
            this.buttonMapProperties.ButtonClick += new System.EventHandler(this.buttonMapProperties_ButtonClick);
            // 
            // buttonCameraStatupPerspective
            // 
            this.buttonCameraStatupPerspective.Name = "buttonCameraStatupPerspective";
            this.buttonCameraStatupPerspective.Size = new System.Drawing.Size(235, 22);
            this.buttonCameraStatupPerspective.Text = "Set camera &startup perspective";
            this.buttonCameraStatupPerspective.Click += new System.EventHandler(this.buttonCameraStatupPerspective_Click);
            // 
            // dialogExportHeightMap
            // 
            this.dialogExportHeightMap.Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*";
            this.dialogExportHeightMap.RestoreDirectory = true;
            this.dialogExportHeightMap.FileOk += new System.ComponentModel.CancelEventHandler(this.dialogExportHeightMap_FileOk);
            // 
            // dialogImportHeightMap
            // 
            this.dialogImportHeightMap.Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*";
            this.dialogImportHeightMap.RestoreDirectory = true;
            this.dialogImportHeightMap.FileOk += new System.ComponentModel.CancelEventHandler(this.dialogImportHeightMap_FileOk);
            // 
            // dialogExportTextureMap
            // 
            this.dialogExportTextureMap.Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*";
            this.dialogExportTextureMap.RestoreDirectory = true;
            this.dialogExportTextureMap.FileOk += new System.ComponentModel.CancelEventHandler(this.dialogExportTextureMap_FileOk);
            // 
            // dialogImportTextureMap
            // 
            this.dialogImportTextureMap.Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*";
            this.dialogImportTextureMap.RestoreDirectory = true;
            this.dialogImportTextureMap.FileOk += new System.ComponentModel.CancelEventHandler(this.dialogImportTextureMap_FileOk);
            // 
            // splitVertical
            // 
            this.splitVertical.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitVertical.Location = new System.Drawing.Point(6, 28);
            this.splitVertical.Name = "splitVertical";
            // 
            // splitVertical.Panel1
            // 
            this.splitVertical.Panel1.Controls.Add(this.tabControl);
            this.splitVertical.Panel1MinSize = 140;
            // 
            // splitVertical.Panel2
            // 
            this.splitVertical.Panel2.Controls.Add(this.splitHorizontal);
            this.splitVertical.Size = new System.Drawing.Size(685, 504);
            this.splitVertical.SplitterDistance = 173;
            this.splitVertical.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPagePositionables);
            this.tabControl.Controls.Add(this.tabPageHeight);
            this.tabControl.Controls.Add(this.tabPageTexture);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(173, 504);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.UpdatePaintingStatus);
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
            this.positionablesTab.Panel1.Controls.Add(this.checkCameraState);
            this.positionablesTab.Panel1.Controls.Add(this.textSearch);
            this.positionablesTab.Panel1.Controls.Add(this.checkMemo);
            this.positionablesTab.Panel1.Controls.Add(this.checkBenchmarkPoint);
            this.positionablesTab.Panel1.Controls.Add(this.checkWater);
            this.positionablesTab.Panel1.Controls.Add(this.checkWaypoint);
            this.positionablesTab.Panel1.Controls.Add(this.checkEntity);
            this.positionablesTab.Panel1.Controls.Add(this.listBoxPositionables);
            this.positionablesTab.Panel1MinSize = 69;
            // 
            // positionablesTab.Panel2
            // 
            this.positionablesTab.Panel2.Controls.Add(this.tabControlEntities);
            this.positionablesTab.Panel2MinSize = 150;
            this.positionablesTab.Size = new System.Drawing.Size(159, 472);
            this.positionablesTab.SplitterDistance = 240;
            this.positionablesTab.TabIndex = 0;
            // 
            // checkCameraState
            // 
            this.checkCameraState.AutoSize = true;
            this.checkCameraState.Checked = true;
            this.checkCameraState.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkCameraState.Location = new System.Drawing.Point(0, 58);
            this.checkCameraState.Name = "checkCameraState";
            this.checkCameraState.Size = new System.Drawing.Size(62, 17);
            this.checkCameraState.TabIndex = 4;
            this.checkCameraState.Text = "Camera";
            this.checkCameraState.UseVisualStyleBackColor = true;
            this.checkCameraState.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
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
            this.checkMemo.Location = new System.Drawing.Point(75, 40);
            this.checkMemo.Name = "checkMemo";
            this.checkMemo.Size = new System.Drawing.Size(55, 17);
            this.checkMemo.TabIndex = 3;
            this.checkMemo.Text = "Memo";
            this.checkMemo.UseVisualStyleBackColor = true;
            this.checkMemo.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
            // 
            // checkBenchmarkPoint
            // 
            this.checkBenchmarkPoint.AutoSize = true;
            this.checkBenchmarkPoint.Checked = true;
            this.checkBenchmarkPoint.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBenchmarkPoint.Location = new System.Drawing.Point(75, 58);
            this.checkBenchmarkPoint.Name = "checkBenchmarkPoint";
            this.checkBenchmarkPoint.Size = new System.Drawing.Size(80, 17);
            this.checkBenchmarkPoint.TabIndex = 5;
            this.checkBenchmarkPoint.Text = "Benchmark";
            this.checkBenchmarkPoint.UseVisualStyleBackColor = true;
            this.checkBenchmarkPoint.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
            // 
            // checkWater
            // 
            this.checkWater.AutoSize = true;
            this.checkWater.Checked = true;
            this.checkWater.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkWater.Location = new System.Drawing.Point(75, 22);
            this.checkWater.Name = "checkWater";
            this.checkWater.Size = new System.Drawing.Size(55, 17);
            this.checkWater.TabIndex = 1;
            this.checkWater.Text = "Water";
            this.checkWater.UseVisualStyleBackColor = true;
            this.checkWater.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
            // 
            // checkWaypoint
            // 
            this.checkWaypoint.AutoSize = true;
            this.checkWaypoint.Checked = true;
            this.checkWaypoint.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkWaypoint.Location = new System.Drawing.Point(0, 40);
            this.checkWaypoint.Name = "checkWaypoint";
            this.checkWaypoint.Size = new System.Drawing.Size(71, 17);
            this.checkWaypoint.TabIndex = 2;
            this.checkWaypoint.Text = "Waypoint";
            this.checkWaypoint.UseVisualStyleBackColor = true;
            this.checkWaypoint.CheckedChanged += new System.EventHandler(this.PositionablesFilterEvent);
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
            this.listBoxPositionables.Location = new System.Drawing.Point(0, 78);
            this.listBoxPositionables.Name = "listBoxPositionables";
            this.listBoxPositionables.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxPositionables.Size = new System.Drawing.Size(159, 160);
            this.listBoxPositionables.Sorted = true;
            this.listBoxPositionables.TabIndex = 6;
            this.listBoxPositionables.SelectedIndexChanged += new System.EventHandler(this.listBoxPositionables_SelectedIndexChanged);
            this.listBoxPositionables.DoubleClick += new System.EventHandler(this.listBoxPositionables_DoubleClick);
            // 
            // tabControlEntities
            // 
            this.tabControlEntities.Controls.Add(this.tabPageProperties);
            this.tabControlEntities.Controls.Add(this.tabPageTemplate);
            this.tabControlEntities.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlEntities.Location = new System.Drawing.Point(0, 0);
            this.tabControlEntities.Name = "tabControlEntities";
            this.tabControlEntities.SelectedIndex = 0;
            this.tabControlEntities.Size = new System.Drawing.Size(159, 228);
            this.tabControlEntities.TabIndex = 0;
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
            // tabPageHeight
            // 
            this.tabPageHeight.AutoScroll = true;
            this.tabPageHeight.Controls.Add(this.upDownHeightNoiseFrequency);
            this.tabPageHeight.Controls.Add(this.upDownHeightStrength);
            this.tabPageHeight.Controls.Add(this.groupHeightShape);
            this.tabPageHeight.Controls.Add(this.labelHeightStrength);
            this.tabPageHeight.Controls.Add(this.radioHeightSmooth);
            this.tabPageHeight.Controls.Add(this.radioHeightNoise);
            this.tabPageHeight.Controls.Add(this.radioHeightRaise);
            this.tabPageHeight.Controls.Add(this.radioHeightPlateau);
            this.tabPageHeight.Controls.Add(this.radioHeightLower);
            this.tabPageHeight.Location = new System.Drawing.Point(4, 22);
            this.tabPageHeight.Name = "tabPageHeight";
            this.tabPageHeight.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageHeight.Size = new System.Drawing.Size(165, 478);
            this.tabPageHeight.TabIndex = 2;
            this.tabPageHeight.Text = "Height";
            this.tabPageHeight.UseVisualStyleBackColor = true;
            // 
            // upDownHeightNoiseFrequency
            // 
            this.upDownHeightNoiseFrequency.DecimalPlaces = 2;
            this.upDownHeightNoiseFrequency.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.upDownHeightNoiseFrequency.Location = new System.Drawing.Point(73, 75);
            this.upDownHeightNoiseFrequency.Maximum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.upDownHeightNoiseFrequency.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.upDownHeightNoiseFrequency.Name = "upDownHeightNoiseFrequency";
            this.upDownHeightNoiseFrequency.Size = new System.Drawing.Size(42, 20);
            this.upDownHeightNoiseFrequency.TabIndex = 4;
            this.upDownHeightNoiseFrequency.Value = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            // 
            // upDownHeightStrength
            // 
            this.upDownHeightStrength.Location = new System.Drawing.Point(6, 142);
            this.upDownHeightStrength.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.upDownHeightStrength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.upDownHeightStrength.Name = "upDownHeightStrength";
            this.upDownHeightStrength.Size = new System.Drawing.Size(40, 20);
            this.upDownHeightStrength.TabIndex = 7;
            this.upDownHeightStrength.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // groupHeightShape
            // 
            this.groupHeightShape.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupHeightShape.Controls.Add(this.radioHeightShapeSquare);
            this.groupHeightShape.Controls.Add(this.radioHeightShapeCircle);
            this.groupHeightShape.Controls.Add(this.upDownHeightSize);
            this.groupHeightShape.Controls.Add(this.labelHeightShapeSize);
            this.groupHeightShape.Location = new System.Drawing.Point(6, 177);
            this.groupHeightShape.Name = "groupHeightShape";
            this.groupHeightShape.Size = new System.Drawing.Size(153, 74);
            this.groupHeightShape.TabIndex = 8;
            this.groupHeightShape.TabStop = false;
            this.groupHeightShape.Text = "Shap&e";
            // 
            // radioHeightShapeSquare
            // 
            this.radioHeightShapeSquare.AutoSize = true;
            this.radioHeightShapeSquare.Location = new System.Drawing.Point(79, 45);
            this.radioHeightShapeSquare.Name = "radioHeightShapeSquare";
            this.radioHeightShapeSquare.Size = new System.Drawing.Size(59, 17);
            this.radioHeightShapeSquare.TabIndex = 3;
            this.radioHeightShapeSquare.Text = "Square";
            this.radioHeightShapeSquare.UseVisualStyleBackColor = true;
            this.radioHeightShapeSquare.CheckedChanged += new System.EventHandler(this.UpdatePaintingStatus);
            // 
            // radioHeightShapeCircle
            // 
            this.radioHeightShapeCircle.AutoSize = true;
            this.radioHeightShapeCircle.Checked = true;
            this.radioHeightShapeCircle.Location = new System.Drawing.Point(6, 45);
            this.radioHeightShapeCircle.Name = "radioHeightShapeCircle";
            this.radioHeightShapeCircle.Size = new System.Drawing.Size(51, 17);
            this.radioHeightShapeCircle.TabIndex = 2;
            this.radioHeightShapeCircle.TabStop = true;
            this.radioHeightShapeCircle.Text = "Circle";
            this.radioHeightShapeCircle.UseVisualStyleBackColor = true;
            this.radioHeightShapeCircle.CheckedChanged += new System.EventHandler(this.UpdatePaintingStatus);
            // 
            // upDownHeightSize
            // 
            this.upDownHeightSize.Location = new System.Drawing.Point(39, 19);
            this.upDownHeightSize.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.upDownHeightSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.upDownHeightSize.Name = "upDownHeightSize";
            this.upDownHeightSize.Size = new System.Drawing.Size(40, 20);
            this.upDownHeightSize.TabIndex = 1;
            this.upDownHeightSize.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.upDownHeightSize.ValueChanged += new System.EventHandler(this.UpdatePaintingStatus);
            // 
            // labelHeightShapeSize
            // 
            this.labelHeightShapeSize.AutoSize = true;
            this.labelHeightShapeSize.Location = new System.Drawing.Point(3, 21);
            this.labelHeightShapeSize.Name = "labelHeightShapeSize";
            this.labelHeightShapeSize.Size = new System.Drawing.Size(30, 13);
            this.labelHeightShapeSize.TabIndex = 0;
            this.labelHeightShapeSize.Text = "Size:";
            // 
            // labelHeightStrength
            // 
            this.labelHeightStrength.AutoSize = true;
            this.labelHeightStrength.Location = new System.Drawing.Point(3, 126);
            this.labelHeightStrength.Name = "labelHeightStrength";
            this.labelHeightStrength.Size = new System.Drawing.Size(50, 13);
            this.labelHeightStrength.TabIndex = 6;
            this.labelHeightStrength.Text = "Stren&gth:";
            // 
            // radioHeightSmooth
            // 
            this.radioHeightSmooth.AutoSize = true;
            this.radioHeightSmooth.Location = new System.Drawing.Point(6, 52);
            this.radioHeightSmooth.Name = "radioHeightSmooth";
            this.radioHeightSmooth.Size = new System.Drawing.Size(61, 17);
            this.radioHeightSmooth.TabIndex = 2;
            this.radioHeightSmooth.Text = "Smoot&h";
            this.radioHeightSmooth.UseVisualStyleBackColor = true;
            // 
            // radioHeightNoise
            // 
            this.radioHeightNoise.AutoSize = true;
            this.radioHeightNoise.Location = new System.Drawing.Point(6, 75);
            this.radioHeightNoise.Name = "radioHeightNoise";
            this.radioHeightNoise.Size = new System.Drawing.Size(52, 17);
            this.radioHeightNoise.TabIndex = 3;
            this.radioHeightNoise.Text = "No&ise";
            this.radioHeightNoise.UseVisualStyleBackColor = true;
            // 
            // radioHeightRaise
            // 
            this.radioHeightRaise.AutoSize = true;
            this.radioHeightRaise.Checked = true;
            this.radioHeightRaise.Location = new System.Drawing.Point(6, 6);
            this.radioHeightRaise.Name = "radioHeightRaise";
            this.radioHeightRaise.Size = new System.Drawing.Size(52, 17);
            this.radioHeightRaise.TabIndex = 0;
            this.radioHeightRaise.TabStop = true;
            this.radioHeightRaise.Text = "&Raise";
            this.radioHeightRaise.UseVisualStyleBackColor = true;
            // 
            // radioHeightPlateau
            // 
            this.radioHeightPlateau.AutoSize = true;
            this.radioHeightPlateau.Location = new System.Drawing.Point(6, 97);
            this.radioHeightPlateau.Name = "radioHeightPlateau";
            this.radioHeightPlateau.Size = new System.Drawing.Size(61, 17);
            this.radioHeightPlateau.TabIndex = 5;
            this.radioHeightPlateau.Text = "Pl&ateau";
            this.radioHeightPlateau.UseVisualStyleBackColor = true;
            // 
            // radioHeightLower
            // 
            this.radioHeightLower.AutoSize = true;
            this.radioHeightLower.Location = new System.Drawing.Point(6, 29);
            this.radioHeightLower.Name = "radioHeightLower";
            this.radioHeightLower.Size = new System.Drawing.Size(54, 17);
            this.radioHeightLower.TabIndex = 1;
            this.radioHeightLower.Text = "L&ower";
            this.radioHeightLower.UseVisualStyleBackColor = true;
            // 
            // tabPageTexture
            // 
            this.tabPageTexture.Controls.Add(this.groupTextureShape);
            this.tabPageTexture.Location = new System.Drawing.Point(4, 22);
            this.tabPageTexture.Name = "tabPageTexture";
            this.tabPageTexture.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTexture.Size = new System.Drawing.Size(165, 478);
            this.tabPageTexture.TabIndex = 3;
            this.tabPageTexture.Text = "Texture";
            this.tabPageTexture.UseVisualStyleBackColor = true;
            // 
            // groupTextureShape
            // 
            this.groupTextureShape.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupTextureShape.Controls.Add(this.radioTextureShapeSquare);
            this.groupTextureShape.Controls.Add(this.radioTextureShapeCircle);
            this.groupTextureShape.Controls.Add(this.upDownTextureSize);
            this.groupTextureShape.Controls.Add(this.labelTerrainSize);
            this.groupTextureShape.Location = new System.Drawing.Point(6, 276);
            this.groupTextureShape.Name = "groupTextureShape";
            this.groupTextureShape.Size = new System.Drawing.Size(153, 74);
            this.groupTextureShape.TabIndex = 9;
            this.groupTextureShape.TabStop = false;
            this.groupTextureShape.Text = "Shap&e";
            // 
            // radioTextureShapeSquare
            // 
            this.radioTextureShapeSquare.AutoSize = true;
            this.radioTextureShapeSquare.Location = new System.Drawing.Point(79, 45);
            this.radioTextureShapeSquare.Name = "radioTextureShapeSquare";
            this.radioTextureShapeSquare.Size = new System.Drawing.Size(59, 17);
            this.radioTextureShapeSquare.TabIndex = 3;
            this.radioTextureShapeSquare.Text = "Square";
            this.radioTextureShapeSquare.UseVisualStyleBackColor = true;
            this.radioTextureShapeSquare.CheckedChanged += new System.EventHandler(this.UpdatePaintingStatus);
            // 
            // radioTextureShapeCircle
            // 
            this.radioTextureShapeCircle.AutoSize = true;
            this.radioTextureShapeCircle.Checked = true;
            this.radioTextureShapeCircle.Location = new System.Drawing.Point(6, 45);
            this.radioTextureShapeCircle.Name = "radioTextureShapeCircle";
            this.radioTextureShapeCircle.Size = new System.Drawing.Size(51, 17);
            this.radioTextureShapeCircle.TabIndex = 2;
            this.radioTextureShapeCircle.TabStop = true;
            this.radioTextureShapeCircle.Text = "Circle";
            this.radioTextureShapeCircle.UseVisualStyleBackColor = true;
            this.radioTextureShapeCircle.CheckedChanged += new System.EventHandler(this.UpdatePaintingStatus);
            // 
            // upDownTextureSize
            // 
            this.upDownTextureSize.Increment = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.upDownTextureSize.Location = new System.Drawing.Point(39, 19);
            this.upDownTextureSize.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.upDownTextureSize.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.upDownTextureSize.Name = "upDownTextureSize";
            this.upDownTextureSize.Size = new System.Drawing.Size(40, 20);
            this.upDownTextureSize.TabIndex = 1;
            this.upDownTextureSize.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.upDownTextureSize.ValueChanged += new System.EventHandler(this.UpdatePaintingStatus);
            // 
            // labelTerrainSize
            // 
            this.labelTerrainSize.AutoSize = true;
            this.labelTerrainSize.Location = new System.Drawing.Point(3, 21);
            this.labelTerrainSize.Name = "labelTerrainSize";
            this.labelTerrainSize.Size = new System.Drawing.Size(30, 13);
            this.labelTerrainSize.TabIndex = 0;
            this.labelTerrainSize.Text = "Size:";
            // 
            // splitHorizontal
            // 
            this.splitHorizontal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitHorizontal.Location = new System.Drawing.Point(0, 0);
            this.splitHorizontal.Name = "splitHorizontal";
            this.splitHorizontal.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitHorizontal.Panel1
            // 
            this.splitHorizontal.Panel1.Controls.Add(this.renderPanel);
            // 
            // splitHorizontal.Panel2
            // 
            this.splitHorizontal.Panel2.Controls.Add(this.xmlEditor);
            this.splitHorizontal.Size = new System.Drawing.Size(508, 504);
            this.splitHorizontal.SplitterDistance = 345;
            this.splitHorizontal.TabIndex = 0;
            // 
            // renderPanel
            // 
            this.renderPanel.AutoRender = true;
            this.renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderPanel.Location = new System.Drawing.Point(0, 0);
            this.renderPanel.Name = "renderPanel";
            this.renderPanel.Size = new System.Drawing.Size(508, 345);
            this.renderPanel.TabIndex = 1;
            // 
            // xmlEditor
            // 
            this.xmlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xmlEditor.Location = new System.Drawing.Point(0, 0);
            this.xmlEditor.Name = "xmlEditor";
            this.xmlEditor.Size = new System.Drawing.Size(508, 155);
            this.xmlEditor.TabIndex = 0;
            this.xmlEditor.ContentChanged += new System.Action<string>(this.xmlEditor_ContentChanged);
            // 
            // MapEditor
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.splitVertical);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.checkWireframe);
            this.Controls.Add(this.buttonDebug);
            this.Name = "MapEditor";
            this.NameUI = "Map Editor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.buttonDebug, 0);
            this.Controls.SetChildIndex(this.checkWireframe, 0);
            this.Controls.SetChildIndex(this.toolStrip, 0);
            this.Controls.SetChildIndex(this.splitVertical, 0);
            this.Controls.SetChildIndex(this.buttonRedo, 0);
            this.Controls.SetChildIndex(this.buttonUndo, 0);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.splitVertical.Panel1.ResumeLayout(false);
            this.splitVertical.Panel2.ResumeLayout(false);
            this.splitVertical.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPagePositionables.ResumeLayout(false);
            this.positionablesTab.Panel1.ResumeLayout(false);
            this.positionablesTab.Panel1.PerformLayout();
            this.positionablesTab.Panel2.ResumeLayout(false);
            this.positionablesTab.ResumeLayout(false);
            this.tabControlEntities.ResumeLayout(false);
            this.tabPageProperties.ResumeLayout(false);
            this.tabPageHeight.ResumeLayout(false);
            this.tabPageHeight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHeightNoiseFrequency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHeightStrength)).EndInit();
            this.groupHeightShape.ResumeLayout(false);
            this.groupHeightShape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownHeightSize)).EndInit();
            this.tabPageTexture.ResumeLayout(false);
            this.groupTextureShape.ResumeLayout(false);
            this.groupTextureShape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.upDownTextureSize)).EndInit();
            this.splitHorizontal.Panel1.ResumeLayout(false);
            this.splitHorizontal.Panel2.ResumeLayout(false);
            this.splitHorizontal.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonDebug;
        private System.Windows.Forms.CheckBox checkWireframe;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripDropDownButton toolStripImportExport;
        private System.Windows.Forms.ToolStripMenuItem buttonExportHeightMap;
        private System.Windows.Forms.ToolStripMenuItem buttonImportHeightMap;
        private System.Windows.Forms.ToolStripSeparator toolStripImportExportSeparator2;
        private System.Windows.Forms.ToolStripMenuItem buttonImportTextureMap;
        private System.Windows.Forms.ToolStripMenuItem buttonExportTextureMap;
        private System.Windows.Forms.ToolStripButton buttonTestInGame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem buttonNewBenchmarkPoint;
        private System.Windows.Forms.ToolStripButton buttonRemove;
        private System.Windows.Forms.ToolStripButton buttonCopy;
        private System.Windows.Forms.OpenFileDialog dialogImportHeightMap;
        private System.Windows.Forms.SaveFileDialog dialogExportHeightMap;
        private System.Windows.Forms.OpenFileDialog dialogImportTextureMap;
        private System.Windows.Forms.SaveFileDialog dialogExportTextureMap;
        private System.Windows.Forms.SplitContainer splitVertical;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPagePositionables;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton buttonNew;
        private System.Windows.Forms.ToolStripMenuItem buttonNewEntity;
        private System.Windows.Forms.SplitContainer positionablesTab;
        private System.Windows.Forms.ListBox listBoxPositionables;
        private System.Windows.Forms.TabControl tabControlEntities;
        private System.Windows.Forms.TabPage tabPageProperties;
        private System.Windows.Forms.TabPage tabPageTemplate;
        private System.Windows.Forms.PropertyGrid propertyGridPositionable;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.CheckBox checkMemo;
        private System.Windows.Forms.CheckBox checkBenchmarkPoint;
        private System.Windows.Forms.CheckBox checkWaypoint;
        private System.Windows.Forms.CheckBox checkEntity;
        private HintTextBox textSearch;
        private System.Windows.Forms.CheckBox checkWater;
        private System.Windows.Forms.TabPage tabPageHeight;
        private System.Windows.Forms.ToolStripSplitButton buttonMapProperties;
        private System.Windows.Forms.ToolStripMenuItem buttonCameraStatupPerspective;
        private System.Windows.Forms.GroupBox groupHeightShape;
        private System.Windows.Forms.RadioButton radioHeightSmooth;
        private System.Windows.Forms.RadioButton radioHeightNoise;
        private System.Windows.Forms.RadioButton radioHeightPlateau;
        private System.Windows.Forms.RadioButton radioHeightLower;
        private System.Windows.Forms.RadioButton radioHeightRaise;
        private System.Windows.Forms.RadioButton radioHeightShapeSquare;
        private System.Windows.Forms.RadioButton radioHeightShapeCircle;
        private System.Windows.Forms.NumericUpDown upDownHeightSize;
        private System.Windows.Forms.Label labelHeightShapeSize;
        private System.Windows.Forms.NumericUpDown upDownHeightStrength;
        private System.Windows.Forms.Label labelHeightStrength;
        private System.Windows.Forms.NumericUpDown upDownHeightNoiseFrequency;
        private System.Windows.Forms.TabPage tabPageTexture;
        private System.Windows.Forms.GroupBox groupTextureShape;
        private System.Windows.Forms.RadioButton radioTextureShapeSquare;
        private System.Windows.Forms.RadioButton radioTextureShapeCircle;
        private System.Windows.Forms.NumericUpDown upDownTextureSize;
        private System.Windows.Forms.Label labelTerrainSize;
        private System.Windows.Forms.SplitContainer splitHorizontal;
        private OmegaEngine.RenderPanel renderPanel;
        private LiveEditor xmlEditor;
        private System.Windows.Forms.ToolStripMenuItem buttonNewWaypoint;
        private System.Windows.Forms.ToolStripMenuItem buttonNewWater;
        private System.Windows.Forms.ToolStripMenuItem buttonNewCameraState;
        private System.Windows.Forms.CheckBox checkCameraState;
    }
}
