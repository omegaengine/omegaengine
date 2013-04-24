namespace AlphaEditor.Graphics
{
    partial class ParticleSystemEditor
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
            this.renderPanel = new OmegaEngine.RenderPanel();
            this.timerRender = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // panelRender
            // 
            this.renderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.renderPanel.Location = new System.Drawing.Point(200, 0);
            this.renderPanel.Name = "panelRender";
            this.renderPanel.Size = new System.Drawing.Size(318, 538);
            this.renderPanel.TabIndex = 0;
            this.renderPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseMove);
            // 
            // timerRender
            // 
            this.timerRender.Enabled = true;
            this.timerRender.Interval = 33;
            this.timerRender.Tick += new System.EventHandler(this.timerRender_Tick);
            // 
            // ParticleSystemEditor
            // 
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.renderPanel);
            this.NameUI = "Particle System Editor";
            this.Name = "ParticleSystemEditor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.renderPanel, 0);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>The panel used by the engine for rendering.</summary>
        protected OmegaEngine.RenderPanel renderPanel;
        private System.Windows.Forms.Timer timerRender;

    }
}