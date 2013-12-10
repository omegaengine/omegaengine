namespace TerrainSample.Editor
{
    partial class WelcomeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeForm));
            this.textWelcome = new System.Windows.Forms.TextBox();
            this.checkBoxDontShowAgain = new System.Windows.Forms.CheckBox();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.pictureLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // textWelcome
            // 
            resources.ApplyResources(this.textWelcome, "textWelcome");
            this.textWelcome.Name = "textWelcome";
            // 
            // checkBoxDontShowAgain
            // 
            resources.ApplyResources(this.checkBoxDontShowAgain, "checkBoxDontShowAgain");
            this.checkBoxDontShowAgain.Name = "checkBoxDontShowAgain";
            this.checkBoxDontShowAgain.UseVisualStyleBackColor = true;
            // 
            // buttonContinue
            // 
            resources.ApplyResources(this.buttonContinue, "buttonContinue");
            this.buttonContinue.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // pictureLogo
            // 
            resources.ApplyResources(this.pictureLogo, "pictureLogo");
            this.pictureLogo.Image = global::AlphaEditor.Properties.Resources.Editor;
            this.pictureLogo.Name = "pictureLogo";
            this.pictureLogo.TabStop = false;
            // 
            // WelcomeForm
            // 
            this.AcceptButton = this.buttonContinue;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureLogo);
            this.Controls.Add(this.textWelcome);
            this.Controls.Add(this.checkBoxDontShowAgain);
            this.Controls.Add(this.buttonContinue);
            this.MaximizeBox = false;
            this.Name = "WelcomeForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureLogo;
        private System.Windows.Forms.TextBox textWelcome;
        private System.Windows.Forms.CheckBox checkBoxDontShowAgain;
        private System.Windows.Forms.Button buttonContinue;
    }
}