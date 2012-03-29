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
                if (Engine != null) Engine.Dispose();
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
            this.panelRender = new System.Windows.Forms.Panel();
            this.timerRender = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // panelRender
            // 
            this.panelRender.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRender.Location = new System.Drawing.Point(200, 0);
            this.panelRender.Name = "panelRender";
            this.panelRender.Size = new System.Drawing.Size(318, 538);
            this.panelRender.TabIndex = 0;
            this.panelRender.Paint += new System.Windows.Forms.PaintEventHandler(this.panelRender_Paint);
            this.panelRender.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelRender_MouseMove);
            this.panelRender.Resize += new System.EventHandler(this.panelRender_Resize);
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
            this.Controls.Add(this.panelRender);
            this.NameUI = "Particle System Editor";
            this.Name = "ParticleSystemEditor";
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.panelRender, 0);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>The panel used by the engine for rendering.</summary>
        protected System.Windows.Forms.Panel panelRender;
        private System.Windows.Forms.Timer timerRender;

    }
}