namespace TerrainSample.Editor.World.Dialogs
{
    partial class TerrainSizeDialog
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
            this.numericX = new System.Windows.Forms.NumericUpDown();
            this.numericY = new System.Windows.Forms.NumericUpDown();
            this.numericStretchH = new System.Windows.Forms.NumericUpDown();
            this.numericStretchV = new System.Windows.Forms.NumericUpDown();
            this.labelX = new System.Windows.Forms.Label();
            this.labelY = new System.Windows.Forms.Label();
            this.labelStretchH = new System.Windows.Forms.Label();
            this.labelStretchV = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStretchH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStretchV)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(15, 123);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(96, 123);
            // 
            // numericX
            // 
            this.numericX.Increment = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericX.Location = new System.Drawing.Point(111, 12);
            this.numericX.Maximum = new decimal(new int[] {
            1536,
            0,
            0,
            0});
            this.numericX.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericX.Name = "numericX";
            this.numericX.Size = new System.Drawing.Size(57, 20);
            this.numericX.TabIndex = 1;
            this.numericX.Value = new decimal(new int[] {
            126,
            0,
            0,
            0});
            this.numericX.Validating += new System.ComponentModel.CancelEventHandler(this.numericSize_Validating);
            // 
            // numericY
            // 
            this.numericY.Increment = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericY.Location = new System.Drawing.Point(111, 38);
            this.numericY.Maximum = new decimal(new int[] {
            1536,
            0,
            0,
            0});
            this.numericY.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.numericY.Name = "numericY";
            this.numericY.Size = new System.Drawing.Size(57, 20);
            this.numericY.TabIndex = 3;
            this.numericY.Value = new decimal(new int[] {
            126,
            0,
            0,
            0});
            this.numericY.Validating += new System.ComponentModel.CancelEventHandler(this.numericSize_Validating);
            // 
            // numericStretchH
            // 
            this.numericStretchH.DecimalPlaces = 2;
            this.numericStretchH.Increment = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericStretchH.Location = new System.Drawing.Point(111, 64);
            this.numericStretchH.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.numericStretchH.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            131072});
            this.numericStretchH.Name = "numericStretchH";
            this.numericStretchH.Size = new System.Drawing.Size(57, 20);
            this.numericStretchH.TabIndex = 5;
            this.numericStretchH.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // numericStretchV
            // 
            this.numericStretchV.DecimalPlaces = 2;
            this.numericStretchV.Increment = new decimal(new int[] {
            10,
            0,
            0,
            131072});
            this.numericStretchV.Location = new System.Drawing.Point(111, 90);
            this.numericStretchV.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericStretchV.Name = "numericStretchV";
            this.numericStretchV.Size = new System.Drawing.Size(57, 20);
            this.numericStretchV.TabIndex = 7;
            this.numericStretchV.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // labelX
            // 
            this.labelX.AutoSize = true;
            this.labelX.Location = new System.Drawing.Point(12, 14);
            this.labelX.Name = "labelX";
            this.labelX.Size = new System.Drawing.Size(74, 13);
            this.labelX.TabIndex = 0;
            this.labelX.Text = "Terrain size &X:";
            // 
            // labelY
            // 
            this.labelY.AutoSize = true;
            this.labelY.Location = new System.Drawing.Point(12, 40);
            this.labelY.Name = "labelY";
            this.labelY.Size = new System.Drawing.Size(74, 13);
            this.labelY.TabIndex = 2;
            this.labelY.Text = "Terrain size &Y:";
            // 
            // labelStretchH
            // 
            this.labelStretchH.AutoSize = true;
            this.labelStretchH.Location = new System.Drawing.Point(12, 66);
            this.labelStretchH.Name = "labelStretchH";
            this.labelStretchH.Size = new System.Drawing.Size(92, 13);
            this.labelStretchH.TabIndex = 4;
            this.labelStretchH.Text = "&Horizontal stretch:";
            // 
            // labelStretchV
            // 
            this.labelStretchV.AutoSize = true;
            this.labelStretchV.Location = new System.Drawing.Point(12, 92);
            this.labelStretchV.Name = "labelStretchV";
            this.labelStretchV.Size = new System.Drawing.Size(80, 13);
            this.labelStretchV.TabIndex = 6;
            this.labelStretchV.Text = "&Vertical stretch:";
            // 
            // TerrainSizeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(183, 158);
            this.ControlBox = false;
            this.Controls.Add(this.labelStretchV);
            this.Controls.Add(this.labelStretchH);
            this.Controls.Add(this.labelY);
            this.Controls.Add(this.labelX);
            this.Controls.Add(this.numericStretchV);
            this.Controls.Add(this.numericStretchH);
            this.Controls.Add(this.numericY);
            this.Controls.Add(this.numericX);
            this.Name = "TerrainSizeDialog";
            this.Text = "Terrain size";
            this.Controls.SetChildIndex(this.numericX, 0);
            this.Controls.SetChildIndex(this.numericY, 0);
            this.Controls.SetChildIndex(this.numericStretchH, 0);
            this.Controls.SetChildIndex(this.numericStretchV, 0);
            this.Controls.SetChildIndex(this.labelX, 0);
            this.Controls.SetChildIndex(this.labelY, 0);
            this.Controls.SetChildIndex(this.labelStretchH, 0);
            this.Controls.SetChildIndex(this.labelStretchV, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.numericX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStretchH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericStretchV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericX;
        private System.Windows.Forms.NumericUpDown numericY;
        private System.Windows.Forms.NumericUpDown numericStretchH;
        private System.Windows.Forms.NumericUpDown numericStretchV;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.Label labelStretchH;
        private System.Windows.Forms.Label labelStretchV;
    }
}