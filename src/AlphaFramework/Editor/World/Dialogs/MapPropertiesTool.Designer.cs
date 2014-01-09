namespace AlphaFramework.Editor.World.Dialogs
{
    partial class MapPropertiesTool
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
            this.propertyGridUniverse = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // propertyGridUniverse
            // 
            this.propertyGridUniverse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridUniverse.Location = new System.Drawing.Point(0, 0);
            this.propertyGridUniverse.Margin = new System.Windows.Forms.Padding(0);
            this.propertyGridUniverse.Name = "propertyGridUniverse";
            this.propertyGridUniverse.Size = new System.Drawing.Size(300, 270);
            this.propertyGridUniverse.TabIndex = 1;
            this.propertyGridUniverse.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridUniverse_PropertyValueChanged);
            // 
            // MapPropertiesTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 270);
            this.Controls.Add(this.propertyGridUniverse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "MapPropertiesTool";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Map properties";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGridUniverse;
    }
}