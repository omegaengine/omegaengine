namespace TerrainSample.Editor.World
{
    partial class EntityEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxTest = new System.Windows.Forms.GroupBox();
            this.buttonDebug = new System.Windows.Forms.Button();
            this.checkBoundingSphere = new System.Windows.Forms.CheckBox();
            this.checkNormalMapping = new System.Windows.Forms.CheckBox();
            this.checkBoundingBox = new System.Windows.Forms.CheckBox();
            this.checkWireframe = new System.Windows.Forms.CheckBox();
            this.propertyGridUniverse = new System.Windows.Forms.PropertyGrid();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageRender = new System.Windows.Forms.TabPage();
            this.toolStripRender = new System.Windows.Forms.ToolStrip();
            this.buttonBrowseRender = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonAddRender = new System.Windows.Forms.ToolStripButton();
            this.buttonRemoveRender = new System.Windows.Forms.ToolStripButton();
            this.comboRender = new System.Windows.Forms.ComboBox();
            this.propertyGridRender = new System.Windows.Forms.PropertyGrid();
            this.tabPageCollision = new System.Windows.Forms.TabPage();
            this.labelCollision = new System.Windows.Forms.Label();
            this.toolStripCollision = new System.Windows.Forms.ToolStrip();
            this.buttonAddCollision = new System.Windows.Forms.ToolStripButton();
            this.buttonRemoveCollision = new System.Windows.Forms.ToolStripButton();
            this.propertyGridCollision = new System.Windows.Forms.PropertyGrid();
            this.tabPageMovement = new System.Windows.Forms.TabPage();
            this.labelMovement = new System.Windows.Forms.Label();
            this.toolStripMovement = new System.Windows.Forms.ToolStrip();
            this.buttonAddMovement = new System.Windows.Forms.ToolStripButton();
            this.buttonRemoveMovement = new System.Windows.Forms.ToolStripButton();
            this.propertyGridMovement = new System.Windows.Forms.PropertyGrid();
            this.splitSettings = new System.Windows.Forms.SplitContainer();
            this.splitRender = new System.Windows.Forms.SplitContainer();
            this.renderPanel = new OmegaEngine.RenderPanel();
            this.buttonOrthographicView = new System.Windows.Forms.Button();
            this.buttonNormalView = new System.Windows.Forms.Button();
            this.splitVertical.Panel2.SuspendLayout();
            this.splitVertical.SuspendLayout();
            this.groupBoxTest.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageRender.SuspendLayout();
            this.toolStripRender.SuspendLayout();
            this.tabPageCollision.SuspendLayout();
            this.toolStripCollision.SuspendLayout();
            this.tabPageMovement.SuspendLayout();
            this.toolStripMovement.SuspendLayout();
            this.splitSettings.Panel1.SuspendLayout();
            this.splitSettings.Panel2.SuspendLayout();
            this.splitSettings.SuspendLayout();
            this.splitRender.Panel1.SuspendLayout();
            this.splitRender.Panel2.SuspendLayout();
            this.splitRender.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitVertical
            // 
            // 
            // splitVertical.Panel2
            // 
            this.splitVertical.Panel2.Controls.Add(this.splitRender);
            // 
            // buttonUndo
            // 
            this.buttonUndo.Location = new System.Drawing.Point(697, 517);
            // 
            // buttonRedo
            // 
            this.buttonRedo.Location = new System.Drawing.Point(697, 496);
            // 
            // groupBoxTest
            // 
            this.groupBoxTest.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTest.Controls.Add(this.buttonDebug);
            this.groupBoxTest.Controls.Add(this.checkBoundingSphere);
            this.groupBoxTest.Controls.Add(this.checkNormalMapping);
            this.groupBoxTest.Controls.Add(this.checkBoundingBox);
            this.groupBoxTest.Controls.Add(this.checkWireframe);
            this.groupBoxTest.Controls.Add(this.propertyGridUniverse);
            this.groupBoxTest.Location = new System.Drawing.Point(3, 3);
            this.groupBoxTest.Name = "groupBoxTest";
            this.groupBoxTest.Size = new System.Drawing.Size(228, 194);
            this.groupBoxTest.TabIndex = 2;
            this.groupBoxTest.TabStop = false;
            this.groupBoxTest.Text = "Test settings   (will not be saved)";
            // 
            // buttonDebug
            // 
            this.buttonDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDebug.Location = new System.Drawing.Point(178, 170);
            this.buttonDebug.Name = "buttonDebug";
            this.buttonDebug.Size = new System.Drawing.Size(50, 23);
            this.buttonDebug.TabIndex = 3;
            this.buttonDebug.Text = "&Debug";
            this.buttonDebug.UseVisualStyleBackColor = true;
            this.buttonDebug.Click += new System.EventHandler(this.buttonDebug_Click);
            // 
            // checkBoundingSphere
            // 
            this.checkBoundingSphere.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoundingSphere.AutoSize = true;
            this.checkBoundingSphere.Location = new System.Drawing.Point(6, 176);
            this.checkBoundingSphere.Name = "checkBoundingSphere";
            this.checkBoundingSphere.Size = new System.Drawing.Size(106, 17);
            this.checkBoundingSphere.TabIndex = 6;
            this.checkBoundingSphere.Text = "Bounding &sphere";
            this.checkBoundingSphere.UseVisualStyleBackColor = true;
            this.checkBoundingSphere.Click += new System.EventHandler(this.checkBoundingSphere_Click);
            // 
            // checkNormalMapping
            // 
            this.checkNormalMapping.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkNormalMapping.AutoSize = true;
            this.checkNormalMapping.Checked = true;
            this.checkNormalMapping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkNormalMapping.Location = new System.Drawing.Point(6, 153);
            this.checkNormalMapping.Name = "checkNormalMapping";
            this.checkNormalMapping.Size = new System.Drawing.Size(102, 17);
            this.checkNormalMapping.TabIndex = 4;
            this.checkNormalMapping.Text = "&Normal mapping";
            this.checkNormalMapping.UseVisualStyleBackColor = true;
            this.checkNormalMapping.Click += new System.EventHandler(this.checkBoxNormalMap_CheckedChanged);
            // 
            // checkBoundingBox
            // 
            this.checkBoundingBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoundingBox.AutoSize = true;
            this.checkBoundingBox.Location = new System.Drawing.Point(118, 176);
            this.checkBoundingBox.Name = "checkBoundingBox";
            this.checkBoundingBox.Size = new System.Drawing.Size(55, 17);
            this.checkBoundingBox.TabIndex = 7;
            this.checkBoundingBox.Text = "... &box";
            this.checkBoundingBox.UseVisualStyleBackColor = true;
            this.checkBoundingBox.Click += new System.EventHandler(this.checkBoundingBox_Click);
            // 
            // checkWireframe
            // 
            this.checkWireframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkWireframe.AutoSize = true;
            this.checkWireframe.Location = new System.Drawing.Point(118, 153);
            this.checkWireframe.Name = "checkWireframe";
            this.checkWireframe.Size = new System.Drawing.Size(74, 17);
            this.checkWireframe.TabIndex = 5;
            this.checkWireframe.Text = "&Wireframe";
            this.checkWireframe.UseVisualStyleBackColor = true;
            this.checkWireframe.Click += new System.EventHandler(this.checkWireframe_CheckedChanged);
            // 
            // propertyGridUniverse
            // 
            this.propertyGridUniverse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridUniverse.Location = new System.Drawing.Point(6, 19);
            this.propertyGridUniverse.Name = "propertyGridUniverse";
            this.propertyGridUniverse.Size = new System.Drawing.Size(214, 128);
            this.propertyGridUniverse.TabIndex = 0;
            this.propertyGridUniverse.ToolbarVisible = false;
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageRender);
            this.tabControl.Controls.Add(this.tabPageCollision);
            this.tabControl.Controls.Add(this.tabPageMovement);
            this.tabControl.Location = new System.Drawing.Point(3, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(294, 194);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageRender
            // 
            this.tabPageRender.Controls.Add(this.toolStripRender);
            this.tabPageRender.Controls.Add(this.comboRender);
            this.tabPageRender.Controls.Add(this.propertyGridRender);
            this.tabPageRender.Location = new System.Drawing.Point(4, 22);
            this.tabPageRender.Name = "tabPageRender";
            this.tabPageRender.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRender.Size = new System.Drawing.Size(286, 168);
            this.tabPageRender.TabIndex = 0;
            this.tabPageRender.Text = "Render Control";
            this.tabPageRender.UseVisualStyleBackColor = true;
            // 
            // toolStripRender
            // 
            this.toolStripRender.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStripRender.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripRender.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonBrowseRender,
            this.toolStripSeparator,
            this.buttonAddRender,
            this.buttonRemoveRender});
            this.toolStripRender.Location = new System.Drawing.Point(199, 0);
            this.toolStripRender.Name = "toolStripRender";
            this.toolStripRender.Size = new System.Drawing.Size(87, 25);
            this.toolStripRender.TabIndex = 1;
            this.toolStripRender.Text = "toolStrip1";
            // 
            // buttonBrowseRender
            // 
            this.buttonBrowseRender.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonBrowseRender.Enabled = false;
            this.buttonBrowseRender.Image = global::AlphaFramework.Editor.Properties.Resources.SearchButton;
            this.buttonBrowseRender.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonBrowseRender.Name = "buttonBrowseRender";
            this.buttonBrowseRender.Size = new System.Drawing.Size(23, 22);
            this.buttonBrowseRender.Text = "Browse files...";
            this.buttonBrowseRender.Click += new System.EventHandler(this.buttonBrowseRender_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonAddRender
            // 
            this.buttonAddRender.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAddRender.Enabled = false;
            this.buttonAddRender.Image = global::AlphaFramework.Editor.Properties.Resources.CreateButton;
            this.buttonAddRender.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddRender.Name = "buttonAddRender";
            this.buttonAddRender.Size = new System.Drawing.Size(23, 22);
            this.buttonAddRender.Text = "Add new render control...";
            this.buttonAddRender.Click += new System.EventHandler(this.buttonAddRender_Click);
            // 
            // buttonRemoveRender
            // 
            this.buttonRemoveRender.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRemoveRender.Enabled = false;
            this.buttonRemoveRender.Image = global::AlphaFramework.Editor.Properties.Resources.DeleteButton;
            this.buttonRemoveRender.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemoveRender.Name = "buttonRemoveRender";
            this.buttonRemoveRender.Size = new System.Drawing.Size(23, 22);
            this.buttonRemoveRender.Text = "Remove render control";
            this.buttonRemoveRender.Click += new System.EventHandler(this.buttonRemoveRender_Click);
            // 
            // comboRender
            // 
            this.comboRender.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboRender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRender.FormattingEnabled = true;
            this.comboRender.Location = new System.Drawing.Point(0, 0);
            this.comboRender.Name = "comboRender";
            this.comboRender.Size = new System.Drawing.Size(196, 21);
            this.comboRender.TabIndex = 0;
            this.comboRender.SelectedIndexChanged += new System.EventHandler(this.comboRender_SelectedIndexChanged);
            // 
            // propertyGridRender
            // 
            this.propertyGridRender.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridRender.Location = new System.Drawing.Point(0, 22);
            this.propertyGridRender.Name = "propertyGridRender";
            this.propertyGridRender.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridRender.Size = new System.Drawing.Size(286, 146);
            this.propertyGridRender.TabIndex = 2;
            this.propertyGridRender.ToolbarVisible = false;
            this.propertyGridRender.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridRender_PropertyValueChanged);
            // 
            // tabPageCollision
            // 
            this.tabPageCollision.Controls.Add(this.labelCollision);
            this.tabPageCollision.Controls.Add(this.toolStripCollision);
            this.tabPageCollision.Controls.Add(this.propertyGridCollision);
            this.tabPageCollision.Location = new System.Drawing.Point(4, 22);
            this.tabPageCollision.Name = "tabPageCollision";
            this.tabPageCollision.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCollision.Size = new System.Drawing.Size(310, 168);
            this.tabPageCollision.TabIndex = 2;
            this.tabPageCollision.Text = "Collision Control";
            this.tabPageCollision.UseVisualStyleBackColor = true;
            // 
            // labelCollision
            // 
            this.labelCollision.AutoSize = true;
            this.labelCollision.Location = new System.Drawing.Point(6, 3);
            this.labelCollision.Name = "labelCollision";
            this.labelCollision.Size = new System.Drawing.Size(0, 13);
            this.labelCollision.TabIndex = 0;
            // 
            // toolStripCollision
            // 
            this.toolStripCollision.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStripCollision.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripCollision.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddCollision,
            this.buttonRemoveCollision});
            this.toolStripCollision.Location = new System.Drawing.Point(252, 0);
            this.toolStripCollision.Name = "toolStripCollision";
            this.toolStripCollision.Size = new System.Drawing.Size(58, 25);
            this.toolStripCollision.TabIndex = 1;
            this.toolStripCollision.Text = "toolStrip1";
            // 
            // buttonAddCollision
            // 
            this.buttonAddCollision.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAddCollision.Enabled = false;
            this.buttonAddCollision.Image = global::AlphaFramework.Editor.Properties.Resources.CreateButton;
            this.buttonAddCollision.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddCollision.Name = "buttonAddCollision";
            this.buttonAddCollision.Size = new System.Drawing.Size(23, 22);
            this.buttonAddCollision.Text = "Add new collision control...";
            this.buttonAddCollision.Click += new System.EventHandler(this.buttonAddCollision_Click);
            // 
            // buttonRemoveCollision
            // 
            this.buttonRemoveCollision.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRemoveCollision.Enabled = false;
            this.buttonRemoveCollision.Image = global::AlphaFramework.Editor.Properties.Resources.DeleteButton;
            this.buttonRemoveCollision.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemoveCollision.Name = "buttonRemoveCollision";
            this.buttonRemoveCollision.Size = new System.Drawing.Size(23, 22);
            this.buttonRemoveCollision.Text = "Remove collision control";
            this.buttonRemoveCollision.Click += new System.EventHandler(this.buttonRemoveCollision_Click);
            // 
            // propertyGridCollision
            // 
            this.propertyGridCollision.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridCollision.Location = new System.Drawing.Point(0, 22);
            this.propertyGridCollision.Name = "propertyGridCollision";
            this.propertyGridCollision.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridCollision.Size = new System.Drawing.Size(310, 116);
            this.propertyGridCollision.TabIndex = 2;
            this.propertyGridCollision.ToolbarVisible = false;
            this.propertyGridCollision.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridCollision_PropertyValueChanged);
            // 
            // tabPageMovement
            // 
            this.tabPageMovement.Controls.Add(this.labelMovement);
            this.tabPageMovement.Controls.Add(this.toolStripMovement);
            this.tabPageMovement.Controls.Add(this.propertyGridMovement);
            this.tabPageMovement.Location = new System.Drawing.Point(4, 22);
            this.tabPageMovement.Name = "tabPageMovement";
            this.tabPageMovement.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMovement.Size = new System.Drawing.Size(310, 168);
            this.tabPageMovement.TabIndex = 1;
            this.tabPageMovement.Text = "Movement Control";
            this.tabPageMovement.UseVisualStyleBackColor = true;
            // 
            // labelMovement
            // 
            this.labelMovement.AutoSize = true;
            this.labelMovement.Location = new System.Drawing.Point(6, 3);
            this.labelMovement.Name = "labelMovement";
            this.labelMovement.Size = new System.Drawing.Size(0, 13);
            this.labelMovement.TabIndex = 0;
            // 
            // toolStripMovement
            // 
            this.toolStripMovement.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStripMovement.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripMovement.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAddMovement,
            this.buttonRemoveMovement});
            this.toolStripMovement.Location = new System.Drawing.Point(252, 0);
            this.toolStripMovement.Name = "toolStripMovement";
            this.toolStripMovement.Size = new System.Drawing.Size(58, 25);
            this.toolStripMovement.TabIndex = 1;
            this.toolStripMovement.Text = "toolStrip1";
            // 
            // buttonAddMovement
            // 
            this.buttonAddMovement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAddMovement.Enabled = false;
            this.buttonAddMovement.Image = global::AlphaFramework.Editor.Properties.Resources.CreateButton;
            this.buttonAddMovement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAddMovement.Name = "buttonAddMovement";
            this.buttonAddMovement.Size = new System.Drawing.Size(23, 22);
            this.buttonAddMovement.Text = "Add new movement control...";
            this.buttonAddMovement.Click += new System.EventHandler(this.buttonAddMovement_Click);
            // 
            // buttonRemoveMovement
            // 
            this.buttonRemoveMovement.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRemoveMovement.Enabled = false;
            this.buttonRemoveMovement.Image = global::AlphaFramework.Editor.Properties.Resources.DeleteButton;
            this.buttonRemoveMovement.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemoveMovement.Name = "buttonRemoveMovement";
            this.buttonRemoveMovement.Size = new System.Drawing.Size(23, 22);
            this.buttonRemoveMovement.Text = "Remove movement control";
            this.buttonRemoveMovement.Click += new System.EventHandler(this.buttonRemoveMovement_Click);
            // 
            // propertyGridMovement
            // 
            this.propertyGridMovement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridMovement.Location = new System.Drawing.Point(0, 22);
            this.propertyGridMovement.Name = "propertyGridMovement";
            this.propertyGridMovement.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridMovement.Size = new System.Drawing.Size(310, 116);
            this.propertyGridMovement.TabIndex = 2;
            this.propertyGridMovement.ToolbarVisible = false;
            this.propertyGridMovement.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridMovement_PropertyValueChanged);
            // 
            // splitSettings
            // 
            this.splitSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitSettings.Location = new System.Drawing.Point(0, 0);
            this.splitSettings.Name = "splitSettings";
            // 
            // splitSettings.Panel1
            // 
            this.splitSettings.Panel1.Controls.Add(this.tabControl);
            // 
            // splitSettings.Panel2
            // 
            this.splitSettings.Panel2.Controls.Add(this.groupBoxTest);
            this.splitSettings.Size = new System.Drawing.Size(538, 200);
            this.splitSettings.SplitterDistance = 300;
            this.splitSettings.TabIndex = 4;
            // 
            // splitRender
            // 
            this.splitRender.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitRender.Location = new System.Drawing.Point(0, 0);
            this.splitRender.Name = "splitRender";
            this.splitRender.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitRender.Panel1
            // 
            this.splitRender.Panel1.Controls.Add(this.splitSettings);
            // 
            // splitRender.Panel2
            // 
            this.splitRender.Panel2.Controls.Add(this.renderPanel);
            this.splitRender.Size = new System.Drawing.Size(538, 538);
            this.splitRender.SplitterDistance = 200;
            this.splitRender.TabIndex = 5;
            // 
            // renderPanel
            // 
            this.renderPanel.AutoRender = true;
            this.renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderPanel.Location = new System.Drawing.Point(0, 0);
            this.renderPanel.Name = "renderPanel";
            this.renderPanel.Size = new System.Drawing.Size(538, 334);
            this.renderPanel.TabIndex = 0;
            // 
            // buttonOrthographicView
            // 
            this.buttonOrthographicView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOrthographicView.Location = new System.Drawing.Point(692, 469);
            this.buttonOrthographicView.Name = "buttonOrthographicView";
            this.buttonOrthographicView.Size = new System.Drawing.Size(23, 21);
            this.buttonOrthographicView.TabIndex = 1;
            this.buttonOrthographicView.Text = "O";
            this.buttonOrthographicView.UseVisualStyleBackColor = true;
            this.buttonOrthographicView.Click += new System.EventHandler(this.buttonOrthographicView_Click);
            // 
            // buttonNormalView
            // 
            this.buttonNormalView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNormalView.Location = new System.Drawing.Point(692, 442);
            this.buttonNormalView.Name = "buttonNormalView";
            this.buttonNormalView.Size = new System.Drawing.Size(23, 21);
            this.buttonNormalView.TabIndex = 0;
            this.buttonNormalView.Text = "N";
            this.buttonNormalView.UseVisualStyleBackColor = true;
            this.buttonNormalView.Click += new System.EventHandler(this.buttonNormalView_Click);
            // 
            // EntityEditor
            // 
            this.Controls.Add(this.buttonNormalView);
            this.Controls.Add(this.buttonOrthographicView);
            this.Name = "EntityEditor";
            this.NameUI = "Entity Editor";
            this.Size = new System.Drawing.Size(718, 538);
            this.splitVertical.Panel2.ResumeLayout(false);
            this.splitVertical.ResumeLayout(false);
            this.groupBoxTest.ResumeLayout(false);
            this.groupBoxTest.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageRender.ResumeLayout(false);
            this.tabPageRender.PerformLayout();
            this.toolStripRender.ResumeLayout(false);
            this.toolStripRender.PerformLayout();
            this.tabPageCollision.ResumeLayout(false);
            this.tabPageCollision.PerformLayout();
            this.toolStripCollision.ResumeLayout(false);
            this.toolStripCollision.PerformLayout();
            this.tabPageMovement.ResumeLayout(false);
            this.tabPageMovement.PerformLayout();
            this.toolStripMovement.ResumeLayout(false);
            this.toolStripMovement.PerformLayout();
            this.splitSettings.Panel1.ResumeLayout(false);
            this.splitSettings.Panel2.ResumeLayout(false);
            this.splitSettings.ResumeLayout(false);
            this.splitRender.Panel1.ResumeLayout(false);
            this.splitRender.Panel2.ResumeLayout(false);
            this.splitRender.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxTest;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageRender;
        private System.Windows.Forms.ToolStrip toolStripRender;
        private System.Windows.Forms.ToolStripButton buttonBrowseRender;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
        private System.Windows.Forms.ToolStripButton buttonAddRender;
        private System.Windows.Forms.ToolStripButton buttonRemoveRender;
        private System.Windows.Forms.ComboBox comboRender;
        private System.Windows.Forms.PropertyGrid propertyGridRender;
        private System.Windows.Forms.TabPage tabPageMovement;
        private System.Windows.Forms.Button buttonDebug;
        private System.Windows.Forms.CheckBox checkWireframe;
        private System.Windows.Forms.PropertyGrid propertyGridUniverse;
        private System.Windows.Forms.SplitContainer splitSettings;
        private System.Windows.Forms.SplitContainer splitRender;
        private System.Windows.Forms.ToolStrip toolStripMovement;
        private System.Windows.Forms.ToolStripButton buttonAddMovement;
        private System.Windows.Forms.ToolStripButton buttonRemoveMovement;
        private System.Windows.Forms.PropertyGrid propertyGridMovement;
        private System.Windows.Forms.TabPage tabPageCollision;
        private System.Windows.Forms.ToolStrip toolStripCollision;
        private System.Windows.Forms.ToolStripButton buttonAddCollision;
        private System.Windows.Forms.ToolStripButton buttonRemoveCollision;
        private System.Windows.Forms.PropertyGrid propertyGridCollision;
        private System.Windows.Forms.Label labelCollision;
        private System.Windows.Forms.Label labelMovement;
        private System.Windows.Forms.CheckBox checkNormalMapping;
        private OmegaEngine.RenderPanel renderPanel;
        private System.Windows.Forms.CheckBox checkBoundingSphere;
        private System.Windows.Forms.CheckBox checkBoundingBox;
        private System.Windows.Forms.Button buttonOrthographicView;
        private System.Windows.Forms.Button buttonNormalView;
    }
}
