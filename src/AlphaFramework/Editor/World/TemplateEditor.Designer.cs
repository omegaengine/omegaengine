namespace AlphaFramework.Editor.World
{
    partial class TemplateEditor<T>
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
            this.splitVertical = new System.Windows.Forms.SplitContainer();
            this.panelList = new System.Windows.Forms.Panel();
            this.groupTemplate = new System.Windows.Forms.GroupBox();
            this.toolStripTemplate = new System.Windows.Forms.ToolStrip();
            this.buttonAdd = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonRemove = new System.Windows.Forms.ToolStripButton();
            this.buttonRename = new System.Windows.Forms.ToolStripButton();
            this.buttonCopy = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonExport = new System.Windows.Forms.ToolStripButton();
            this.propertyGridTemplate = new System.Windows.Forms.PropertyGrid();
            this.timerRender = new System.Windows.Forms.Timer(this.components);
            this.dialogExportXml = new System.Windows.Forms.SaveFileDialog();
            this.splitVertical.Panel1.SuspendLayout();
            this.splitVertical.SuspendLayout();
            this.panelList.SuspendLayout();
            this.groupTemplate.SuspendLayout();
            this.toolStripTemplate.SuspendLayout();
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
            // splitVertical
            // 
            this.splitVertical.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitVertical.Location = new System.Drawing.Point(0, 0);
            this.splitVertical.Name = "splitVertical";
            // 
            // splitVertical.Panel1
            // 
            this.splitVertical.Panel1.Controls.Add(this.panelList);
            this.splitVertical.Panel1MinSize = 150;
            this.splitVertical.Size = new System.Drawing.Size(692, 538);
            this.splitVertical.SplitterDistance = 150;
            this.splitVertical.TabIndex = 1;
            // 
            // panelList
            // 
            this.panelList.Controls.Add(this.groupTemplate);
            this.panelList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelList.Location = new System.Drawing.Point(0, 0);
            this.panelList.Name = "panelList";
            this.panelList.Size = new System.Drawing.Size(150, 538);
            this.panelList.TabIndex = 0;
            // 
            // groupTemplate
            // 
            this.groupTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupTemplate.Controls.Add(this.toolStripTemplate);
            this.groupTemplate.Controls.Add(this.propertyGridTemplate);
            this.groupTemplate.Location = new System.Drawing.Point(6, 3);
            this.groupTemplate.Name = "groupTemplate";
            this.groupTemplate.Size = new System.Drawing.Size(140, 528);
            this.groupTemplate.TabIndex = 0;
            this.groupTemplate.TabStop = false;
            this.groupTemplate.Text = "Template";
            // 
            // toolStripTemplate
            // 
            this.toolStripTemplate.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStripTemplate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAdd,
            this.toolStripSeparator1,
            this.buttonRemove,
            this.buttonRename,
            this.buttonCopy,
            this.toolStripSeparator2,
            this.buttonExport});
            this.toolStripTemplate.Location = new System.Drawing.Point(6, 16);
            this.toolStripTemplate.Name = "toolStripTemplate";
            this.toolStripTemplate.Size = new System.Drawing.Size(170, 25);
            this.toolStripTemplate.TabIndex = 0;
            // 
            // buttonAdd
            // 
            this.buttonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAdd.Image = global::AlphaFramework.Editor.Properties.Resources.CreateButton;
            this.buttonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(23, 22);
            this.buttonAdd.Text = "Add new...";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonRemove
            // 
            this.buttonRemove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Image = global::AlphaFramework.Editor.Properties.Resources.DeleteButton;
            this.buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(23, 22);
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonRename
            // 
            this.buttonRename.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRename.Enabled = false;
            this.buttonRename.Image = global::AlphaFramework.Editor.Properties.Resources.RenameButton;
            this.buttonRename.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRename.Name = "buttonRename";
            this.buttonRename.Size = new System.Drawing.Size(23, 22);
            this.buttonRename.Text = "Rename...";
            this.buttonRename.Click += new System.EventHandler(this.buttonRename_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonCopy.Enabled = false;
            this.buttonCopy.Image = global::AlphaFramework.Editor.Properties.Resources.CopyCopy;
            this.buttonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(23, 22);
            this.buttonCopy.Text = "Copy...";
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonExport
            // 
            this.buttonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonExport.Enabled = false;
            this.buttonExport.Image = global::AlphaFramework.Editor.Properties.Resources.ImportExportButton;
            this.buttonExport.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(23, 22);
            this.buttonExport.Text = "Export...";
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // propertyGridTemplate
            // 
            this.propertyGridTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridTemplate.Location = new System.Drawing.Point(6, 355);
            this.propertyGridTemplate.Name = "propertyGridTemplate";
            this.propertyGridTemplate.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridTemplate.Size = new System.Drawing.Size(128, 168);
            this.propertyGridTemplate.TabIndex = 2;
            this.propertyGridTemplate.ToolbarVisible = false;
            // 
            // timerRender
            // 
            this.timerRender.Enabled = true;
            this.timerRender.Interval = 33;
            // 
            // dialogExportXml
            // 
            this.dialogExportXml.Filter = "XML file (*.xml)|*.xml|All files (*.*)|*.*";
            this.dialogExportXml.RestoreDirectory = true;
            this.dialogExportXml.FileOk += new System.ComponentModel.CancelEventHandler(this.dialogExportXml_FileOk);
            // 
            // TemplateEditor
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.splitVertical);
            this.Name = "TemplateEditor";
            this.NameUI = "Editor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.buttonRedo, 0);
            this.Controls.SetChildIndex(this.buttonUndo, 0);
            this.Controls.SetChildIndex(this.splitVertical, 0);
            this.splitVertical.Panel1.ResumeLayout(false);
            this.splitVertical.ResumeLayout(false);
            this.panelList.ResumeLayout(false);
            this.groupTemplate.ResumeLayout(false);
            this.groupTemplate.PerformLayout();
            this.toolStripTemplate.ResumeLayout(false);
            this.toolStripTemplate.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>The vertical splitter separating the template list from the template editor.</summary>
        protected System.Windows.Forms.SplitContainer splitVertical;
        private System.Windows.Forms.Panel panelList;
        private System.Windows.Forms.GroupBox groupTemplate;
        private System.Windows.Forms.Timer timerRender;
        private System.Windows.Forms.PropertyGrid propertyGridTemplate;
        private System.Windows.Forms.ToolStrip toolStripTemplate;
        private System.Windows.Forms.ToolStripButton buttonAdd;
        private System.Windows.Forms.ToolStripButton buttonRemove;
        private System.Windows.Forms.ToolStripButton buttonCopy;
        private System.Windows.Forms.ToolStripButton buttonRename;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton buttonExport;
        private System.Windows.Forms.SaveFileDialog dialogExportXml;
    }
}
