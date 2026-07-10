namespace Template.AlphaFramework.Editor
{
    partial class MainForm
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
            this.toolMap = new System.Windows.Forms.ToolStripButton();
            this.SuspendLayout();
            //
            // toolStrip (inherited from MainFormBase)
            //
            this.toolStrip.Items.Insert(0, this.toolMap);
            //
            // toolMap
            //
            this.toolMap.Name = "toolMap";
            this.toolMap.Text = "Map Editor";
            this.toolMap.Click += new System.EventHandler(this.toolMap_Click);
            //
            // MainForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "MainForm";
            this.Text = "Editor";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripButton toolMap;
    }
}
