namespace AlphaEditor
{
    partial class FileSelectorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileSelectorDialog));
            this.buttonNew = new System.Windows.Forms.Button();
            this.selectLabel = new System.Windows.Forms.Label();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelLegendChanged = new System.Windows.Forms.Label();
            this.labelLegendAdded = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonNew
            // 
            resources.ApplyResources(this.buttonNew, "buttonNew");
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // selectLabel
            // 
            resources.ApplyResources(this.selectLabel, "selectLabel");
            this.selectLabel.Name = "selectLabel";
            // 
            // buttonOpen
            // 
            resources.ApplyResources(this.buttonOpen, "buttonOpen");
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.RestoreDirectory = true;
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.RestoreDirectory = true;
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // labelLegendChanged
            // 
            resources.ApplyResources(this.labelLegendChanged, "labelLegendChanged");
            this.labelLegendChanged.ForeColor = System.Drawing.Color.Blue;
            this.labelLegendChanged.Name = "labelLegendChanged";
            // 
            // labelLegendAdded
            // 
            resources.ApplyResources(this.labelLegendAdded, "labelLegendAdded");
            this.labelLegendAdded.ForeColor = System.Drawing.Color.Green;
            this.labelLegendAdded.Name = "labelLegendAdded";
            // 
            // FileSelectorDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.labelLegendAdded);
            this.Controls.Add(this.labelLegendChanged);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.buttonNew);
            this.Controls.Add(this.selectLabel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileSelectorDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.Label selectLabel;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelLegendChanged;
        private System.Windows.Forms.Label labelLegendAdded;
    }
}