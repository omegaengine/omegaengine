namespace AlphaEditor
{
    partial class ModSelectorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModSelectorForm));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupNew = new System.Windows.Forms.GroupBox();
            this.buttonNew = new System.Windows.Forms.Button();
            this.groupExisting = new System.Windows.Forms.GroupBox();
            this.buttonMainGame = new System.Windows.Forms.Button();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.recentGroup = new System.Windows.Forms.GroupBox();
            this.buttonClear = new System.Windows.Forms.Button();
            this.listBoxRecent = new System.Windows.Forms.ListBox();
            this.pictureLogo = new System.Windows.Forms.PictureBox();
            this.groupNew.SuspendLayout();
            this.groupExisting.SuspendLayout();
            this.recentGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialog
            // 
            resources.ApplyResources(this.openFileDialog, "openFileDialog");
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // groupNew
            // 
            this.groupNew.AccessibleDescription = null;
            this.groupNew.AccessibleName = null;
            resources.ApplyResources(this.groupNew, "groupNew");
            this.groupNew.BackgroundImage = null;
            this.groupNew.Controls.Add(this.buttonNew);
            this.groupNew.Font = null;
            this.groupNew.Name = "groupNew";
            this.groupNew.TabStop = false;
            // 
            // buttonNew
            // 
            this.buttonNew.AccessibleDescription = null;
            this.buttonNew.AccessibleName = null;
            resources.ApplyResources(this.buttonNew, "buttonNew");
            this.buttonNew.BackgroundImage = null;
            this.buttonNew.Font = null;
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // groupExisting
            // 
            this.groupExisting.AccessibleDescription = null;
            this.groupExisting.AccessibleName = null;
            resources.ApplyResources(this.groupExisting, "groupExisting");
            this.groupExisting.BackgroundImage = null;
            this.groupExisting.Controls.Add(this.buttonMainGame);
            this.groupExisting.Controls.Add(this.buttonOpen);
            this.groupExisting.Font = null;
            this.groupExisting.Name = "groupExisting";
            this.groupExisting.TabStop = false;
            // 
            // buttonMainGame
            // 
            this.buttonMainGame.AccessibleDescription = null;
            this.buttonMainGame.AccessibleName = null;
            resources.ApplyResources(this.buttonMainGame, "buttonMainGame");
            this.buttonMainGame.BackgroundImage = null;
            this.buttonMainGame.Font = null;
            this.buttonMainGame.Name = "buttonMainGame";
            this.buttonMainGame.UseVisualStyleBackColor = true;
            this.buttonMainGame.Click += new System.EventHandler(this.buttonMainGame_Click);
            // 
            // buttonOpen
            // 
            this.buttonOpen.AccessibleDescription = null;
            this.buttonOpen.AccessibleName = null;
            resources.ApplyResources(this.buttonOpen, "buttonOpen");
            this.buttonOpen.BackgroundImage = null;
            this.buttonOpen.Font = null;
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // recentGroup
            // 
            this.recentGroup.AccessibleDescription = null;
            this.recentGroup.AccessibleName = null;
            resources.ApplyResources(this.recentGroup, "recentGroup");
            this.recentGroup.BackgroundImage = null;
            this.recentGroup.Controls.Add(this.buttonClear);
            this.recentGroup.Controls.Add(this.listBoxRecent);
            this.recentGroup.Font = null;
            this.recentGroup.Name = "recentGroup";
            this.recentGroup.TabStop = false;
            // 
            // buttonClear
            // 
            this.buttonClear.AccessibleDescription = null;
            this.buttonClear.AccessibleName = null;
            resources.ApplyResources(this.buttonClear, "buttonClear");
            this.buttonClear.BackgroundImage = null;
            this.buttonClear.Font = null;
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.TabStop = false;
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // listBoxRecent
            // 
            this.listBoxRecent.AccessibleDescription = null;
            this.listBoxRecent.AccessibleName = null;
            resources.ApplyResources(this.listBoxRecent, "listBoxRecent");
            this.listBoxRecent.BackgroundImage = null;
            this.listBoxRecent.Font = null;
            this.listBoxRecent.FormattingEnabled = true;
            this.listBoxRecent.Name = "listBoxRecent";
            this.listBoxRecent.DoubleClick += new System.EventHandler(this.listBoxRecent_DoubleClick);
            // 
            // pictureLogo
            // 
            this.pictureLogo.AccessibleDescription = null;
            this.pictureLogo.AccessibleName = null;
            resources.ApplyResources(this.pictureLogo, "pictureLogo");
            this.pictureLogo.BackgroundImage = null;
            this.pictureLogo.Font = null;
            this.pictureLogo.Image = global::AlphaEditor.Properties.Resources.Editor;
            this.pictureLogo.ImageLocation = null;
            this.pictureLogo.Name = "pictureLogo";
            this.pictureLogo.TabStop = false;
            // 
            // ModSelectorForm
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.pictureLogo);
            this.Controls.Add(this.recentGroup);
            this.Controls.Add(this.groupExisting);
            this.Controls.Add(this.groupNew);
            this.Font = null;
            this.MaximizeBox = false;
            this.Name = "ModSelectorForm";
            this.Load += new System.EventHandler(this.ModSelector_Load);
            this.groupNew.ResumeLayout(false);
            this.groupExisting.ResumeLayout(false);
            this.recentGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.GroupBox groupNew;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.GroupBox groupExisting;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.GroupBox recentGroup;
        private System.Windows.Forms.ListBox listBoxRecent;
        private System.Windows.Forms.Button buttonMainGame;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.PictureBox pictureLogo;
    }
}