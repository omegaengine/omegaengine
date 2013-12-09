namespace AlphaEditor
{
    partial class ModPropertyDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModPropertyDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.labelWebsite = new System.Windows.Forms.Label();
            this.textWebsite = new System.Windows.Forms.TextBox();
            this.labelAuthor = new System.Windows.Forms.Label();
            this.textAuthor = new System.Windows.Forms.TextBox();
            this.textLocation = new System.Windows.Forms.TextBox();
            this.labelLocation = new System.Windows.Forms.Label();
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.textName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.textVersion = new System.Windows.Forms.TextBox();
            this.labelVersion = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelDescription
            // 
            resources.ApplyResources(this.labelDescription, "labelDescription");
            this.labelDescription.Name = "labelDescription";
            // 
            // textDescription
            // 
            resources.ApplyResources(this.textDescription, "textDescription");
            this.textDescription.Name = "textDescription";
            // 
            // buttonBrowse
            // 
            resources.ApplyResources(this.buttonBrowse, "buttonBrowse");
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // labelWebsite
            // 
            resources.ApplyResources(this.labelWebsite, "labelWebsite");
            this.labelWebsite.Name = "labelWebsite";
            // 
            // textWebsite
            // 
            resources.ApplyResources(this.textWebsite, "textWebsite");
            this.textWebsite.Name = "textWebsite";
            // 
            // labelAuthor
            // 
            resources.ApplyResources(this.labelAuthor, "labelAuthor");
            this.labelAuthor.Name = "labelAuthor";
            // 
            // textAuthor
            // 
            resources.ApplyResources(this.textAuthor, "textAuthor");
            this.textAuthor.Name = "textAuthor";
            // 
            // textLocation
            // 
            resources.ApplyResources(this.textLocation, "textLocation");
            this.textLocation.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textLocation.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.textLocation.Name = "textLocation";
            this.textLocation.TextChanged += new System.EventHandler(this.textLocation_TextChanged);
            // 
            // labelLocation
            // 
            resources.ApplyResources(this.labelLocation, "labelLocation");
            this.labelLocation.Name = "labelLocation";
            // 
            // textName
            // 
            resources.ApplyResources(this.textName, "textName");
            this.textName.Name = "textName";
            this.textName.TextChanged += new System.EventHandler(this.textName_TextChanged);
            // 
            // labelName
            // 
            resources.ApplyResources(this.labelName, "labelName");
            this.labelName.Name = "labelName";
            // 
            // textVersion
            // 
            this.textVersion.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            resources.ApplyResources(this.textVersion, "textVersion");
            this.textVersion.Name = "textVersion";
            // 
            // labelVersion
            // 
            resources.ApplyResources(this.labelVersion, "labelVersion");
            this.labelVersion.Name = "labelVersion";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // ModPropertyDialog
            // 
            this.AcceptButton = this.buttonOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.textVersion);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.labelDescription);
            this.Controls.Add(this.textDescription);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.labelWebsite);
            this.Controls.Add(this.textWebsite);
            this.Controls.Add(this.labelAuthor);
            this.Controls.Add(this.textAuthor);
            this.Controls.Add(this.textLocation);
            this.Controls.Add(this.labelLocation);
            this.Controls.Add(this.textName);
            this.Controls.Add(this.labelName);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ModPropertyDialog";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.ModPropertyDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label labelWebsite;
        private System.Windows.Forms.TextBox textWebsite;
        private System.Windows.Forms.Label labelAuthor;
        private System.Windows.Forms.TextBox textAuthor;
        private System.Windows.Forms.TextBox textLocation;
        private System.Windows.Forms.Label labelLocation;
        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textVersion;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}