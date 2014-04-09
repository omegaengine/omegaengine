using NanoByte.Common.Controls;

namespace OmegaEngine
{
    partial class DebugForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            this.mainContainer = new System.Windows.Forms.SplitContainer();
            this.viewContainer = new System.Windows.Forms.SplitContainer();
            this.logFrameButton = new System.Windows.Forms.Button();
            this.dumpViewButton = new System.Windows.Forms.Button();
            this.cameraLabel = new System.Windows.Forms.Label();
            this.cameraPropertyGrid = new ResettablePropertyGrid();
            this.viewLabel = new System.Windows.Forms.Label();
            this.viewListBox = new System.Windows.Forms.ListBox();
            this.viewSubContainer = new System.Windows.Forms.SplitContainer();
            this.viewPropertyGrid = new ResettablePropertyGrid();
            this.shaderPropertyGrid = new ResettablePropertyGrid();
            this.shaderLabel = new System.Windows.Forms.Label();
            this.shaderListBox = new System.Windows.Forms.ListBox();
            this.sceneContainer = new System.Windows.Forms.SplitContainer();
            this.entityContainer = new System.Windows.Forms.SplitContainer();
            this.renderableLabel = new System.Windows.Forms.Label();
            this.renderableListBox = new System.Windows.Forms.ListBox();
            this.renderablePropertyGrid = new ResettablePropertyGrid();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.removeButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.lightLabel = new System.Windows.Forms.Label();
            this.lightListBox = new System.Windows.Forms.ListBox();
            this.lightPropertyGrid = new ResettablePropertyGrid();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.dumpViewSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.logFrameSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.mainContainer.Panel1.SuspendLayout();
            this.mainContainer.Panel2.SuspendLayout();
            this.mainContainer.SuspendLayout();
            this.viewContainer.Panel1.SuspendLayout();
            this.viewContainer.Panel2.SuspendLayout();
            this.viewContainer.SuspendLayout();
            this.viewSubContainer.Panel1.SuspendLayout();
            this.viewSubContainer.Panel2.SuspendLayout();
            this.viewSubContainer.SuspendLayout();
            this.sceneContainer.Panel1.SuspendLayout();
            this.sceneContainer.Panel2.SuspendLayout();
            this.sceneContainer.SuspendLayout();
            this.entityContainer.Panel1.SuspendLayout();
            this.entityContainer.Panel2.SuspendLayout();
            this.entityContainer.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainContainer
            // 
            this.mainContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainContainer.Location = new System.Drawing.Point(9, 9);
            this.mainContainer.Name = "mainContainer";
            // 
            // mainContainer.Panel1
            // 
            this.mainContainer.Panel1.Controls.Add(this.viewContainer);
            this.mainContainer.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            // 
            // mainContainer.Panel2
            // 
            this.mainContainer.Panel2.Controls.Add(this.sceneContainer);
            this.mainContainer.Panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.mainContainer.Size = new System.Drawing.Size(874, 629);
            this.mainContainer.SplitterDistance = 466;
            this.mainContainer.TabIndex = 0;
            // 
            // viewContainer
            // 
            this.viewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewContainer.Location = new System.Drawing.Point(0, 0);
            this.viewContainer.Name = "viewContainer";
            // 
            // viewContainer.Panel1
            // 
            this.viewContainer.Panel1.Controls.Add(this.logFrameButton);
            this.viewContainer.Panel1.Controls.Add(this.dumpViewButton);
            this.viewContainer.Panel1.Controls.Add(this.cameraLabel);
            this.viewContainer.Panel1.Controls.Add(this.cameraPropertyGrid);
            this.viewContainer.Panel1.Controls.Add(this.viewLabel);
            this.viewContainer.Panel1.Controls.Add(this.viewListBox);
            this.viewContainer.Panel1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            // 
            // viewContainer.Panel2
            // 
            this.viewContainer.Panel2.Controls.Add(this.viewSubContainer);
            this.viewContainer.Panel2.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.viewContainer.Size = new System.Drawing.Size(463, 629);
            this.viewContainer.SplitterDistance = 218;
            this.viewContainer.TabIndex = 0;
            // 
            // logFrameButton
            // 
            this.logFrameButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.logFrameButton.Location = new System.Drawing.Point(139, 0);
            this.logFrameButton.Name = "logFrameButton";
            this.logFrameButton.Size = new System.Drawing.Size(71, 23);
            this.logFrameButton.TabIndex = 3;
            this.logFrameButton.Text = "&Log Frame";
            this.logFrameButton.UseVisualStyleBackColor = true;
            this.logFrameButton.Click += new System.EventHandler(this.logFrameButton_Click);
            // 
            // dumpViewButton
            // 
            this.dumpViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dumpViewButton.Enabled = false;
            this.dumpViewButton.Location = new System.Drawing.Point(139, 130);
            this.dumpViewButton.Name = "dumpViewButton";
            this.dumpViewButton.Size = new System.Drawing.Size(71, 23);
            this.dumpViewButton.TabIndex = 2;
            this.dumpViewButton.Text = "&Dump View";
            this.dumpViewButton.UseVisualStyleBackColor = true;
            this.dumpViewButton.Click += new System.EventHandler(this.dumpViewButton_Click);
            // 
            // cameraLabel
            // 
            this.cameraLabel.AutoSize = true;
            this.cameraLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cameraLabel.Location = new System.Drawing.Point(6, 160);
            this.cameraLabel.Name = "cameraLabel";
            this.cameraLabel.Size = new System.Drawing.Size(53, 13);
            this.cameraLabel.TabIndex = 4;
            this.cameraLabel.Text = "Camera:";
            // 
            // cameraPropertyGrid
            // 
            this.cameraPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cameraPropertyGrid.Location = new System.Drawing.Point(6, 176);
            this.cameraPropertyGrid.Name = "cameraPropertyGrid";
            this.cameraPropertyGrid.Size = new System.Drawing.Size(204, 453);
            this.cameraPropertyGrid.TabIndex = 5;
            // 
            // viewLabel
            // 
            this.viewLabel.AutoSize = true;
            this.viewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.viewLabel.Location = new System.Drawing.Point(6, 5);
            this.viewLabel.Name = "viewLabel";
            this.viewLabel.Size = new System.Drawing.Size(87, 13);
            this.viewLabel.TabIndex = 0;
            this.viewLabel.Text = "Engine Views:";
            // 
            // viewListBox
            // 
            this.viewListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.viewListBox.FormattingEnabled = true;
            this.viewListBox.Location = new System.Drawing.Point(6, 23);
            this.viewListBox.Name = "viewListBox";
            this.viewListBox.Size = new System.Drawing.Size(204, 121);
            this.viewListBox.TabIndex = 1;
            this.viewListBox.SelectedIndexChanged += new System.EventHandler(this.viewListBox_SelectedIndexChanged);
            // 
            // viewSubContainer
            // 
            this.viewSubContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewSubContainer.Location = new System.Drawing.Point(3, 0);
            this.viewSubContainer.Name = "viewSubContainer";
            this.viewSubContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // viewSubContainer.Panel1
            // 
            this.viewSubContainer.Panel1.Controls.Add(this.viewPropertyGrid);
            // 
            // viewSubContainer.Panel2
            // 
            this.viewSubContainer.Panel2.Controls.Add(this.shaderPropertyGrid);
            this.viewSubContainer.Panel2.Controls.Add(this.shaderLabel);
            this.viewSubContainer.Panel2.Controls.Add(this.shaderListBox);
            this.viewSubContainer.Size = new System.Drawing.Size(235, 629);
            this.viewSubContainer.SplitterDistance = 326;
            this.viewSubContainer.TabIndex = 3;
            // 
            // viewPropertyGrid
            // 
            this.viewPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.viewPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.viewPropertyGrid.Name = "viewPropertyGrid";
            this.viewPropertyGrid.Size = new System.Drawing.Size(235, 326);
            this.viewPropertyGrid.TabIndex = 0;
            this.viewPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.viewPropertyGrid_PropertyValueChanged);
            // 
            // shaderPropertyGrid
            // 
            this.shaderPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.shaderPropertyGrid.Location = new System.Drawing.Point(3, 120);
            this.shaderPropertyGrid.Name = "shaderPropertyGrid";
            this.shaderPropertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.shaderPropertyGrid.Size = new System.Drawing.Size(229, 179);
            this.shaderPropertyGrid.TabIndex = 2;
            this.shaderPropertyGrid.ToolbarVisible = false;
            // 
            // shaderLabel
            // 
            this.shaderLabel.AutoSize = true;
            this.shaderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.shaderLabel.Location = new System.Drawing.Point(6, 10);
            this.shaderLabel.Name = "shaderLabel";
            this.shaderLabel.Size = new System.Drawing.Size(130, 13);
            this.shaderLabel.TabIndex = 0;
            this.shaderLabel.Text = "Post Screen Shaders:";
            // 
            // shaderListBox
            // 
            this.shaderListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.shaderListBox.FormattingEnabled = true;
            this.shaderListBox.Location = new System.Drawing.Point(3, 32);
            this.shaderListBox.Name = "shaderListBox";
            this.shaderListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.shaderListBox.Size = new System.Drawing.Size(229, 82);
            this.shaderListBox.TabIndex = 1;
            this.shaderListBox.SelectedIndexChanged += new System.EventHandler(this.shaderListBox_SelectedIndexChanged);
            // 
            // sceneContainer
            // 
            this.sceneContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneContainer.Location = new System.Drawing.Point(3, 0);
            this.sceneContainer.Name = "sceneContainer";
            this.sceneContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // sceneContainer.Panel1
            // 
            this.sceneContainer.Panel1.Controls.Add(this.entityContainer);
            // 
            // sceneContainer.Panel2
            // 
            this.sceneContainer.Panel2.Controls.Add(this.splitContainer1);
            this.sceneContainer.Size = new System.Drawing.Size(401, 629);
            this.sceneContainer.SplitterDistance = 326;
            this.sceneContainer.TabIndex = 4;
            // 
            // entityContainer
            // 
            this.entityContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entityContainer.Location = new System.Drawing.Point(0, 0);
            this.entityContainer.Name = "entityContainer";
            // 
            // entityContainer.Panel1
            // 
            this.entityContainer.Panel1.Controls.Add(this.renderableLabel);
            this.entityContainer.Panel1.Controls.Add(this.renderableListBox);
            this.entityContainer.Panel1.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            // 
            // entityContainer.Panel2
            // 
            this.entityContainer.Panel2.Controls.Add(this.renderablePropertyGrid);
            this.entityContainer.Panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.entityContainer.Size = new System.Drawing.Size(401, 326);
            this.entityContainer.SplitterDistance = 129;
            this.entityContainer.TabIndex = 1;
            // 
            // renderableLabel
            // 
            this.renderableLabel.AutoSize = true;
            this.renderableLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.renderableLabel.Location = new System.Drawing.Point(6, 5);
            this.renderableLabel.Name = "renderableLabel";
            this.renderableLabel.Size = new System.Drawing.Size(122, 13);
            this.renderableLabel.TabIndex = 0;
            this.renderableLabel.Text = "Scene Renderables:";
            // 
            // renderableListBox
            // 
            this.renderableListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.renderableListBox.FormattingEnabled = true;
            this.renderableListBox.Location = new System.Drawing.Point(3, 23);
            this.renderableListBox.Name = "renderableListBox";
            this.renderableListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.renderableListBox.Size = new System.Drawing.Size(124, 303);
            this.renderableListBox.TabIndex = 1;
            this.renderableListBox.SelectedIndexChanged += new System.EventHandler(this.renderableListBox_SelectedIndexChanged);
            // 
            // renderablePropertyGrid
            // 
            this.renderablePropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.renderablePropertyGrid.Location = new System.Drawing.Point(3, 0);
            this.renderablePropertyGrid.Name = "renderablePropertyGrid";
            this.renderablePropertyGrid.Size = new System.Drawing.Size(265, 326);
            this.renderablePropertyGrid.TabIndex = 0;
            this.renderablePropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.renderablePropertyGrid_PropertyValueChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.removeButton);
            this.splitContainer1.Panel1.Controls.Add(this.addButton);
            this.splitContainer1.Panel1.Controls.Add(this.lightLabel);
            this.splitContainer1.Panel1.Controls.Add(this.lightListBox);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lightPropertyGrid);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.splitContainer1.Size = new System.Drawing.Size(401, 299);
            this.splitContainer1.SplitterDistance = 129;
            this.splitContainer1.TabIndex = 2;
            // 
            // removeButton
            // 
            this.removeButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.removeButton.Location = new System.Drawing.Point(65, 274);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(62, 20);
            this.removeButton.TabIndex = 3;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // addButton
            // 
            this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addButton.Location = new System.Drawing.Point(3, 274);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(56, 20);
            this.addButton.TabIndex = 2;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // lightLabel
            // 
            this.lightLabel.AutoSize = true;
            this.lightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lightLabel.Location = new System.Drawing.Point(6, 10);
            this.lightLabel.Name = "lightLabel";
            this.lightLabel.Size = new System.Drawing.Size(85, 13);
            this.lightLabel.TabIndex = 0;
            this.lightLabel.Text = "Scene Lights:";
            // 
            // lightListBox
            // 
            this.lightListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lightListBox.FormattingEnabled = true;
            this.lightListBox.Location = new System.Drawing.Point(3, 31);
            this.lightListBox.Name = "lightListBox";
            this.lightListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lightListBox.Size = new System.Drawing.Size(124, 238);
            this.lightListBox.TabIndex = 1;
            this.lightListBox.SelectedIndexChanged += new System.EventHandler(this.lightListBox_SelectedIndexChanged);
            // 
            // lightPropertyGrid
            // 
            this.lightPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lightPropertyGrid.Location = new System.Drawing.Point(3, 0);
            this.lightPropertyGrid.Name = "lightPropertyGrid";
            this.lightPropertyGrid.Size = new System.Drawing.Size(265, 299);
            this.lightPropertyGrid.TabIndex = 0;
            this.lightPropertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.lightPropertyGrid_PropertyValueChanged);
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 2000;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // dumpViewSaveFileDialog
            // 
            this.dumpViewSaveFileDialog.Filter = "PNG image (*.png)|*.png|All files (*.*)|*.*";
            this.dumpViewSaveFileDialog.RestoreDirectory = true;
            this.dumpViewSaveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.dumpViewSaveFileDialog_FileOk);
            // 
            // logFrameSaveFileDialog
            // 
            this.logFrameSaveFileDialog.Filter = "XML file (*.xml)|*.xml|All files (*.*)|*.*";
            this.logFrameSaveFileDialog.RestoreDirectory = true;
            this.logFrameSaveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.logFrameSaveFileDialog_FileOk);
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 647);
            this.Controls.Add(this.mainContainer);
            this.Name = "DebugForm";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.Text = "OmegaEngine Debug Interface";
            this.TopMost = true;
            this.mainContainer.Panel1.ResumeLayout(false);
            this.mainContainer.Panel2.ResumeLayout(false);
            this.mainContainer.ResumeLayout(false);
            this.viewContainer.Panel1.ResumeLayout(false);
            this.viewContainer.Panel1.PerformLayout();
            this.viewContainer.Panel2.ResumeLayout(false);
            this.viewContainer.ResumeLayout(false);
            this.viewSubContainer.Panel1.ResumeLayout(false);
            this.viewSubContainer.Panel2.ResumeLayout(false);
            this.viewSubContainer.Panel2.PerformLayout();
            this.viewSubContainer.ResumeLayout(false);
            this.sceneContainer.Panel1.ResumeLayout(false);
            this.sceneContainer.Panel2.ResumeLayout(false);
            this.sceneContainer.ResumeLayout(false);
            this.entityContainer.Panel1.ResumeLayout(false);
            this.entityContainer.Panel1.PerformLayout();
            this.entityContainer.Panel2.ResumeLayout(false);
            this.entityContainer.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer mainContainer;
        private System.Windows.Forms.SplitContainer viewContainer;
        private System.Windows.Forms.SplitContainer viewSubContainer;
        private System.Windows.Forms.ListBox viewListBox;
        private System.Windows.Forms.Label viewLabel;
        private System.Windows.Forms.SplitContainer sceneContainer;
        private System.Windows.Forms.SplitContainer entityContainer;
        private System.Windows.Forms.Label renderableLabel;
        private System.Windows.Forms.ListBox renderableListBox;
        private ResettablePropertyGrid renderablePropertyGrid;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lightLabel;
        private System.Windows.Forms.ListBox lightListBox;
        private ResettablePropertyGrid lightPropertyGrid;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button addButton;
        private ResettablePropertyGrid viewPropertyGrid;
        private ResettablePropertyGrid shaderPropertyGrid;
        private System.Windows.Forms.Label shaderLabel;
        private System.Windows.Forms.ListBox shaderListBox;
        private System.Windows.Forms.Label cameraLabel;
        private ResettablePropertyGrid cameraPropertyGrid;
        private System.Windows.Forms.Button dumpViewButton;
        private System.Windows.Forms.SaveFileDialog dumpViewSaveFileDialog;
        private System.Windows.Forms.Button logFrameButton;
        private System.Windows.Forms.SaveFileDialog logFrameSaveFileDialog;


    }
}
