namespace AlphaEditor.Graphics
{
    partial class GpuParticleSystemEditor
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
                if (components != null) components.Dispose();
                if (_particleSystem != null) _particleSystem.Dispose();
                if (_scene != null) _scene.Dispose();
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
            this.panelLeft = new System.Windows.Forms.Panel();
            this.propertyGridSystem = new System.Windows.Forms.PropertyGrid();
            this.labelSystem = new System.Windows.Forms.Label();
            this.panelLeft.SuspendLayout();
            this.SuspendLayout();
            // 
            // renderPanel
            // 
            this.renderPanel.Size = new System.Drawing.Size(491, 538);
            this.renderPanel.TabIndex = 3;
            // 
            // panelLeft
            // 
            this.panelLeft.Controls.Add(this.propertyGridSystem);
            this.panelLeft.Controls.Add(this.labelSystem);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(200, 538);
            this.panelLeft.TabIndex = 1;
            // 
            // propertyGridSystem
            // 
            this.propertyGridSystem.Location = new System.Drawing.Point(6, 22);
            this.propertyGridSystem.Name = "propertyGridSystem";
            this.propertyGridSystem.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridSystem.Size = new System.Drawing.Size(188, 240);
            this.propertyGridSystem.TabIndex = 1;
            this.propertyGridSystem.ToolbarVisible = false;
            this.propertyGridSystem.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // labelSystem
            // 
            this.labelSystem.AutoSize = true;
            this.labelSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSystem.Location = new System.Drawing.Point(3, 6);
            this.labelSystem.Name = "labelSystem";
            this.labelSystem.Size = new System.Drawing.Size(96, 13);
            this.labelSystem.TabIndex = 0;
            this.labelSystem.Text = "Particle system:";
            // 
            // GpuParticleSystemEditor
            // 
            this.Controls.Add(this.panelLeft);
            this.NameUI = "GPU Particle System Editor";
            this.Name = "GpuParticleSystemEditor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.renderPanel, 0);
            this.Controls.SetChildIndex(this.panelLeft, 0);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.PropertyGrid propertyGridSystem;
        private System.Windows.Forms.Label labelSystem;

    }
}