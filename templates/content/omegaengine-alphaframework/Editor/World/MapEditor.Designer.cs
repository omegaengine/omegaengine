namespace Template.AlphaFramework.Editor.World
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.renderPanel = new Template.AlphaFramework.Editor.RenderPanel();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.listBox = new System.Windows.Forms.ListBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Size = new System.Drawing.Size(720, 540);
            this.splitContainer.SplitterDistance = 480;
            this.splitContainer.SplitterWidth = 4;
            this.splitContainer.TabIndex = 0;
            //
            // splitContainer.Panel1
            //
            this.splitContainer.Panel1.Controls.Add(this.renderPanel);
            //
            // splitContainer.Panel2 (add order: Fill control first, then Top controls stack above)
            //
            this.splitContainer.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer.Panel2.Controls.Add(this.listBox);
            this.splitContainer.Panel2.Controls.Add(this.buttonPanel);
            //
            // renderPanel
            //
            this.renderPanel.AutoRender = true;
            this.renderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderPanel.Location = new System.Drawing.Point(0, 0);
            this.renderPanel.Name = "renderPanel";
            this.renderPanel.Size = new System.Drawing.Size(480, 540);
            this.renderPanel.TabIndex = 0;
            //
            // propertyGrid
            //
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.TabIndex = 2;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            //
            // listBox
            //
            this.listBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.listBox.FormattingEnabled = true;
            this.listBox.IntegralHeight = false;
            this.listBox.Name = "listBox";
            this.listBox.Size = new System.Drawing.Size(236, 200);
            this.listBox.TabIndex = 1;
            this.listBox.SelectedIndexChanged += new System.EventHandler(this.listBox_SelectedIndexChanged);
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.buttonRemove);
            this.buttonPanel.Controls.Add(this.buttonAdd);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(236, 28);
            this.buttonPanel.TabIndex = 0;
            //
            // buttonRemove
            //
            this.buttonRemove.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonRemove.Location = new System.Drawing.Point(60, 0);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(60, 28);
            this.buttonRemove.TabIndex = 1;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            //
            // buttonAdd
            //
            this.buttonAdd.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonAdd.Location = new System.Drawing.Point(0, 0);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(60, 28);
            this.buttonAdd.TabIndex = 0;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            //
            // MapEditor
            //
            this.Controls.Add(this.splitContainer);
            this.Name = "MapEditor";
            this.Size = new System.Drawing.Size(720, 540);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private Template.AlphaFramework.Editor.RenderPanel renderPanel;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
    }
}
