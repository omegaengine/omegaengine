namespace AlphaEditor.Graphics
{
    partial class CpuParticleSystemEditor
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
            this.propertyGridUpper2 = new System.Windows.Forms.PropertyGrid();
            this.propertyGridLower2 = new System.Windows.Forms.PropertyGrid();
            this.labelGridUpper2 = new System.Windows.Forms.Label();
            this.labelLower2 = new System.Windows.Forms.Label();
            this.propertyGridUpper1 = new System.Windows.Forms.PropertyGrid();
            this.propertyGridLower1 = new System.Windows.Forms.PropertyGrid();
            this.propertyGridSystem = new System.Windows.Forms.PropertyGrid();
            this.labelGridUpper1 = new System.Windows.Forms.Label();
            this.labelLower1 = new System.Windows.Forms.Label();
            this.labelSystem = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelRender
            // 
            this.panelRender.TabIndex = 10;
            // 
            // propertyGridUpper2
            // 
            this.propertyGridUpper2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridUpper2.Location = new System.Drawing.Point(527, 237);
            this.propertyGridUpper2.Name = "propertyGridUpper2";
            this.propertyGridUpper2.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridUpper2.Size = new System.Drawing.Size(188, 170);
            this.propertyGridUpper2.TabIndex = 9;
            this.propertyGridUpper2.ToolbarVisible = false;
            this.propertyGridUpper2.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // propertyGridLower2
            // 
            this.propertyGridLower2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridLower2.Location = new System.Drawing.Point(527, 48);
            this.propertyGridLower2.Name = "propertyGridLower2";
            this.propertyGridLower2.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridLower2.Size = new System.Drawing.Size(188, 170);
            this.propertyGridLower2.TabIndex = 7;
            this.propertyGridLower2.ToolbarVisible = false;
            this.propertyGridLower2.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // labelGridUpper2
            // 
            this.labelGridUpper2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelGridUpper2.AutoSize = true;
            this.labelGridUpper2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGridUpper2.Location = new System.Drawing.Point(524, 221);
            this.labelGridUpper2.Name = "labelGridUpper2";
            this.labelGridUpper2.Size = new System.Drawing.Size(125, 13);
            this.labelGridUpper2.TabIndex = 8;
            this.labelGridUpper2.Text = "2nd life upper range:";
            // 
            // labelLower2
            // 
            this.labelLower2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLower2.AutoSize = true;
            this.labelLower2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLower2.Location = new System.Drawing.Point(524, 32);
            this.labelLower2.Name = "labelLower2";
            this.labelLower2.Size = new System.Drawing.Size(123, 13);
            this.labelLower2.TabIndex = 6;
            this.labelLower2.Text = "2nd life lower range:";
            // 
            // propertyGridUpper1
            // 
            this.propertyGridUpper1.Location = new System.Drawing.Point(3, 205);
            this.propertyGridUpper1.Name = "propertyGridUpper1";
            this.propertyGridUpper1.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridUpper1.Size = new System.Drawing.Size(188, 170);
            this.propertyGridUpper1.TabIndex = 3;
            this.propertyGridUpper1.ToolbarVisible = false;
            this.propertyGridUpper1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // propertyGridLower1
            // 
            this.propertyGridLower1.Location = new System.Drawing.Point(3, 16);
            this.propertyGridLower1.Name = "propertyGridLower1";
            this.propertyGridLower1.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propertyGridLower1.Size = new System.Drawing.Size(188, 170);
            this.propertyGridLower1.TabIndex = 1;
            this.propertyGridLower1.ToolbarVisible = false;
            this.propertyGridLower1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // propertyGridSystem
            // 
            this.propertyGridSystem.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.propertyGridSystem.Location = new System.Drawing.Point(3, 395);
            this.propertyGridSystem.Name = "propertyGridSystem";
            this.propertyGridSystem.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGridSystem.Size = new System.Drawing.Size(188, 140);
            this.propertyGridSystem.TabIndex = 5;
            this.propertyGridSystem.ToolbarVisible = false;
            this.propertyGridSystem.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // labelGridUpper1
            // 
            this.labelGridUpper1.AutoSize = true;
            this.labelGridUpper1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelGridUpper1.Location = new System.Drawing.Point(0, 189);
            this.labelGridUpper1.Name = "labelGridUpper1";
            this.labelGridUpper1.Size = new System.Drawing.Size(132, 13);
            this.labelGridUpper1.TabIndex = 2;
            this.labelGridUpper1.Text = "Particles upper range:";
            // 
            // labelLower1
            // 
            this.labelLower1.AutoSize = true;
            this.labelLower1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelLower1.Location = new System.Drawing.Point(0, 0);
            this.labelLower1.Name = "labelLower1";
            this.labelLower1.Size = new System.Drawing.Size(130, 13);
            this.labelLower1.TabIndex = 0;
            this.labelLower1.Text = "Particles lower range:";
            // 
            // labelSystem
            // 
            this.labelSystem.AutoSize = true;
            this.labelSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSystem.Location = new System.Drawing.Point(0, 379);
            this.labelSystem.Name = "labelSystem";
            this.labelSystem.Size = new System.Drawing.Size(96, 13);
            this.labelSystem.TabIndex = 4;
            this.labelSystem.Text = "Particle system:";
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Location = new System.Drawing.Point(527, 512);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 10;
            this.buttonReset.Text = "&Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // CpuParticleSystemEditor
            // 
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.propertyGridUpper1);
            this.Controls.Add(this.propertyGridLower1);
            this.Controls.Add(this.propertyGridSystem);
            this.Controls.Add(this.labelGridUpper1);
            this.Controls.Add(this.labelLower1);
            this.Controls.Add(this.labelSystem);
            this.Controls.Add(this.propertyGridUpper2);
            this.Controls.Add(this.propertyGridLower2);
            this.Controls.Add(this.labelGridUpper2);
            this.Controls.Add(this.labelLower2);
            this.Name = "CpuParticleSystemEditor";
            this.NameUI = "CPU Particle System Editor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.panelRender, 0);
            this.Controls.SetChildIndex(this.labelLower2, 0);
            this.Controls.SetChildIndex(this.labelGridUpper2, 0);
            this.Controls.SetChildIndex(this.propertyGridLower2, 0);
            this.Controls.SetChildIndex(this.propertyGridUpper2, 0);
            this.Controls.SetChildIndex(this.labelSystem, 0);
            this.Controls.SetChildIndex(this.labelLower1, 0);
            this.Controls.SetChildIndex(this.labelGridUpper1, 0);
            this.Controls.SetChildIndex(this.propertyGridSystem, 0);
            this.Controls.SetChildIndex(this.propertyGridLower1, 0);
            this.Controls.SetChildIndex(this.propertyGridUpper1, 0);
            this.Controls.SetChildIndex(this.buttonReset, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGridUpper2;
        private System.Windows.Forms.PropertyGrid propertyGridLower2;
        private System.Windows.Forms.Label labelGridUpper2;
        private System.Windows.Forms.Label labelLower2;
        private System.Windows.Forms.PropertyGrid propertyGridUpper1;
        private System.Windows.Forms.PropertyGrid propertyGridLower1;
        private System.Windows.Forms.PropertyGrid propertyGridSystem;
        private System.Windows.Forms.Label labelGridUpper1;
        private System.Windows.Forms.Label labelLower1;
        private System.Windows.Forms.Label labelSystem;
        private System.Windows.Forms.Button buttonReset;

    }
}