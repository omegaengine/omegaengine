namespace AlphaEditor.World
{
    partial class TerrainEditor
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
            this.groupTexture = new System.Windows.Forms.GroupBox();
            this.textPath = new Common.Controls.HintTextBox();
            this.pictureTexture = new System.Windows.Forms.PictureBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.splitVertical.Panel2.SuspendLayout();
            this.splitVertical.SuspendLayout();
            this.groupTexture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureTexture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // splitVertical
            // 
            // 
            // splitVertical.Panel2
            // 
            this.splitVertical.Panel2.Controls.Add(this.groupTexture);
            // 
            // groupTexture
            // 
            this.groupTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupTexture.Controls.Add(this.textPath);
            this.groupTexture.Controls.Add(this.pictureTexture);
            this.groupTexture.Controls.Add(this.buttonBrowse);
            this.groupTexture.Location = new System.Drawing.Point(3, 314);
            this.groupTexture.Name = "groupTexture";
            this.groupTexture.Size = new System.Drawing.Size(150, 217);
            this.groupTexture.TabIndex = 0;
            this.groupTexture.TabStop = false;
            this.groupTexture.Text = "Texture";
            // 
            // textPath
            // 
            this.textPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textPath.HintText = "Texture file name";
            this.textPath.Location = new System.Drawing.Point(6, 161);
            this.textPath.Name = "textPath";
            this.textPath.Size = new System.Drawing.Size(120, 20);
            this.textPath.TabIndex = 0;
            this.textPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textPath_KeyDown);
            this.textPath.Leave += new System.EventHandler(this.textPath_Leave);
            // 
            // pictureTexture
            // 
            this.pictureTexture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureTexture.Location = new System.Drawing.Point(6, 19);
            this.pictureTexture.Name = "pictureTexture";
            this.pictureTexture.Size = new System.Drawing.Size(136, 136);
            this.pictureTexture.TabIndex = 3;
            this.pictureTexture.TabStop = false;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonBrowse.Enabled = false;
            this.buttonBrowse.Location = new System.Drawing.Point(6, 188);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "&Browse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // TerrainEditor
            // 
            this.Name = "TerrainEditor";
            this.NameUI = "Terrain Editor";
            this.Controls.SetChildIndex(this.buttonRedo, 0);
            this.Controls.SetChildIndex(this.buttonUndo, 0);
            this.Controls.SetChildIndex(this.splitVertical, 0);
            this.splitVertical.Panel2.ResumeLayout(false);
            this.splitVertical.ResumeLayout(false);
            this.groupTexture.ResumeLayout(false);
            this.groupTexture.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureTexture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupTexture;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.PictureBox pictureTexture;
        private Common.Controls.HintTextBox textPath;
        private System.Windows.Forms.ErrorProvider errorProvider;

    }
}
