namespace AlphaEditor.Gui
{
    partial class GuiEditor
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
                if (_guiManager != null) _guiManager.Dispose();
                if (_dialogRenderer != null) _dialogRenderer.Dispose();
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
            this.splitVertical = new System.Windows.Forms.SplitContainer();
            this.panelList = new System.Windows.Forms.Panel();
            this.splitHorizontal = new System.Windows.Forms.SplitContainer();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonAdd = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonRemove = new System.Windows.Forms.ToolStripButton();
            this.buttonCopy = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonMoveUp = new System.Windows.Forms.ToolStripButton();
            this.buttonMoveDown = new System.Windows.Forms.ToolStripButton();
            this.listBox = new System.Windows.Forms.ListBox();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.renderPanel = new OmegaEngine.RenderPanel();
            this.splitVertical.Panel1.SuspendLayout();
            this.splitVertical.Panel2.SuspendLayout();
            this.splitVertical.SuspendLayout();
            this.panelList.SuspendLayout();
            this.splitHorizontal.Panel1.SuspendLayout();
            this.splitHorizontal.Panel2.SuspendLayout();
            this.splitHorizontal.SuspendLayout();
            this.toolStrip.SuspendLayout();
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
            // 
            // splitVertical.Panel2
            // 
            this.splitVertical.Panel2.Controls.Add(this.renderPanel);
            this.splitVertical.Size = new System.Drawing.Size(692, 538);
            this.splitVertical.SplitterDistance = 150;
            this.splitVertical.TabIndex = 0;
            // 
            // panelList
            // 
            this.panelList.Controls.Add(this.splitHorizontal);
            this.panelList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelList.Location = new System.Drawing.Point(0, 0);
            this.panelList.Name = "panelList";
            this.panelList.Size = new System.Drawing.Size(150, 538);
            this.panelList.TabIndex = 1;
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
            this.splitHorizontal.Panel1.Controls.Add(this.toolStrip);
            this.splitHorizontal.Panel1.Controls.Add(this.listBox);
            this.splitHorizontal.Panel1.Padding = new System.Windows.Forms.Padding(5, 0, 5, 2);
            this.splitHorizontal.Panel1MinSize = 64;
            // 
            // splitHorizontal.Panel2
            // 
            this.splitHorizontal.Panel2.Controls.Add(this.propertyGrid);
            this.splitHorizontal.Panel2.Padding = new System.Windows.Forms.Padding(5, 2, 5, 5);
            this.splitHorizontal.Panel2MinSize = 128;
            this.splitHorizontal.Size = new System.Drawing.Size(150, 538);
            this.splitHorizontal.SplitterDistance = 190;
            this.splitHorizontal.TabIndex = 1;
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonAdd,
            this.toolStripSeparator1,
            this.buttonRemove,
            this.buttonCopy,
            this.toolStripSeparator2,
            this.buttonMoveUp,
            this.buttonMoveDown});
            this.toolStrip.Location = new System.Drawing.Point(5, 9);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(139, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // buttonAdd
            // 
            this.buttonAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonAdd.Image = global::AlphaEditor.Properties.Resources.CreateButton;
            this.buttonAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(23, 22);
            this.buttonAdd.Text = "Add control...";
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
            this.buttonRemove.Image = global::AlphaEditor.Properties.Resources.DeleteButton;
            this.buttonRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(23, 22);
            this.buttonRemove.Text = "Remove control";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonCopy.Enabled = false;
            this.buttonCopy.Image = global::AlphaEditor.Properties.Resources.CopyCopy;
            this.buttonCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(23, 22);
            this.buttonCopy.Text = "Copy control";
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonMoveUp
            // 
            this.buttonMoveUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonMoveUp.Enabled = false;
            this.buttonMoveUp.Image = global::AlphaEditor.Properties.Resources.UpButton;
            this.buttonMoveUp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonMoveUp.Name = "buttonMoveUp";
            this.buttonMoveUp.Size = new System.Drawing.Size(23, 22);
            this.buttonMoveUp.Text = "Move control up";
            this.buttonMoveUp.Click += new System.EventHandler(this.buttonMoveUp_Click);
            // 
            // buttonMoveDown
            // 
            this.buttonMoveDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonMoveDown.Enabled = false;
            this.buttonMoveDown.Image = global::AlphaEditor.Properties.Resources.DownButton;
            this.buttonMoveDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonMoveDown.Name = "buttonMoveDown";
            this.buttonMoveDown.Size = new System.Drawing.Size(23, 22);
            this.buttonMoveDown.Text = "Move control down";
            this.buttonMoveDown.Click += new System.EventHandler(this.buttonMoveDown_Click);
            // 
            // listBox
            // 
            this.listBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox.FormattingEnabled = true;
            this.listBox.Location = new System.Drawing.Point(5, 37);
            this.listBox.Name = "listBox";
            this.listBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBox.Size = new System.Drawing.Size(140, 147);
            this.listBox.TabIndex = 1;
            this.listBox.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            // 
            // propertyGrid
            // 
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(5, 2);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(140, 337);
            this.propertyGrid.TabIndex = 1;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // renderPanel
            // 
            this.renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderPanel.Location = new System.Drawing.Point(0, 0);
            this.renderPanel.Name = "renderPanel";
            this.renderPanel.Size = new System.Drawing.Size(538, 538);
            this.renderPanel.TabIndex = 0;
            this.renderPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseDown);
            this.renderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseMove);
            this.renderPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.renderPanel_MouseUp);
            // 
            // GuiEditor
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.splitVertical);
            this.Name = "GuiEditor";
            this.NameUI = "GUI Editor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.buttonRedo, 0);
            this.Controls.SetChildIndex(this.buttonUndo, 0);
            this.Controls.SetChildIndex(this.splitVertical, 0);
            this.splitVertical.Panel1.ResumeLayout(false);
            this.splitVertical.Panel2.ResumeLayout(false);
            this.splitVertical.ResumeLayout(false);
            this.panelList.ResumeLayout(false);
            this.splitHorizontal.Panel1.ResumeLayout(false);
            this.splitHorizontal.Panel1.PerformLayout();
            this.splitHorizontal.Panel2.ResumeLayout(false);
            this.splitHorizontal.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitVertical;
        private System.Windows.Forms.Panel panelList;
        private System.Windows.Forms.SplitContainer splitHorizontal;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton buttonAdd;
        private System.Windows.Forms.ToolStripButton buttonRemove;
        private System.Windows.Forms.ToolStripButton buttonCopy;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton buttonMoveUp;
        private System.Windows.Forms.ToolStripButton buttonMoveDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private OmegaEngine.RenderPanel renderPanel;
    }
}
