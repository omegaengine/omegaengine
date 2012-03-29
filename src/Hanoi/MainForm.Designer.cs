namespace Hanoi
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.pegsBox = new System.Windows.Forms.TextBox();
            this.setupButton = new System.Windows.Forms.Button();
            this.solveButton = new System.Windows.Forms.Button();
            this.speedSlider = new System.Windows.Forms.TrackBar();
            this.sourceBox = new System.Windows.Forms.ComboBox();
            this.targetBox = new System.Windows.Forms.ComboBox();
            this.moveButton = new System.Windows.Forms.Button();
            this.discsBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.speedSlider)).BeginInit();
            this.SuspendLayout();
            // 
            // pegsBox
            // 
            this.pegsBox.Location = new System.Drawing.Point(12, 13);
            this.pegsBox.Name = "pegsBox";
            this.pegsBox.Size = new System.Drawing.Size(44, 20);
            this.pegsBox.TabIndex = 0;
            this.pegsBox.Text = "3";
            // 
            // setupButton
            // 
            this.setupButton.Location = new System.Drawing.Point(139, 9);
            this.setupButton.Name = "setupButton";
            this.setupButton.Size = new System.Drawing.Size(75, 23);
            this.setupButton.TabIndex = 2;
            this.setupButton.Text = "&Setup";
            this.setupButton.UseVisualStyleBackColor = true;
            this.setupButton.Click += new System.EventHandler(this.setupButton_Click);
            // 
            // solveButton
            // 
            this.solveButton.Location = new System.Drawing.Point(139, 48);
            this.solveButton.Name = "solveButton";
            this.solveButton.Size = new System.Drawing.Size(75, 23);
            this.solveButton.TabIndex = 4;
            this.solveButton.Text = "So&lve";
            this.solveButton.UseVisualStyleBackColor = true;
            this.solveButton.Click += new System.EventHandler(this.solveButton_Click);
            // 
            // speedSlider
            // 
            this.speedSlider.Location = new System.Drawing.Point(12, 39);
            this.speedSlider.Maximum = 24;
            this.speedSlider.Minimum = 1;
            this.speedSlider.Name = "speedSlider";
            this.speedSlider.Size = new System.Drawing.Size(121, 45);
            this.speedSlider.TabIndex = 3;
            this.speedSlider.Value = 2;
            this.speedSlider.Scroll += new System.EventHandler(this.speedSlider_Scroll);
            // 
            // sourceBox
            // 
            this.sourceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sourceBox.FormattingEnabled = true;
            this.sourceBox.Items.AddRange(new object[] {
            "1",
            "2",
            "3"});
            this.sourceBox.Location = new System.Drawing.Point(12, 94);
            this.sourceBox.Name = "sourceBox";
            this.sourceBox.Size = new System.Drawing.Size(53, 21);
            this.sourceBox.TabIndex = 5;
            // 
            // targetBox
            // 
            this.targetBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.targetBox.FormattingEnabled = true;
            this.targetBox.Items.AddRange(new object[] {
            "1",
            "2",
            "3"});
            this.targetBox.Location = new System.Drawing.Point(101, 94);
            this.targetBox.Name = "targetBox";
            this.targetBox.Size = new System.Drawing.Size(53, 21);
            this.targetBox.TabIndex = 7;
            // 
            // moveButton
            // 
            this.moveButton.Location = new System.Drawing.Point(71, 92);
            this.moveButton.Name = "moveButton";
            this.moveButton.Size = new System.Drawing.Size(24, 23);
            this.moveButton.TabIndex = 6;
            this.moveButton.Text = "->";
            this.moveButton.UseVisualStyleBackColor = true;
            this.moveButton.Click += new System.EventHandler(this.moveButton_Click);
            // 
            // discsBox
            // 
            this.discsBox.Location = new System.Drawing.Point(62, 13);
            this.discsBox.Name = "discsBox";
            this.discsBox.Size = new System.Drawing.Size(44, 20);
            this.discsBox.TabIndex = 1;
            this.discsBox.Text = "4";
            // 
            // ControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(226, 130);
            this.Controls.Add(this.discsBox);
            this.Controls.Add(this.moveButton);
            this.Controls.Add(this.targetBox);
            this.Controls.Add(this.sourceBox);
            this.Controls.Add(this.speedSlider);
            this.Controls.Add(this.solveButton);
            this.Controls.Add(this.setupButton);
            this.Controls.Add(this.pegsBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ControlForm";
            this.Text = "Towers of Hanoi - Control";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.speedSlider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox pegsBox;
        private System.Windows.Forms.Button setupButton;
        private System.Windows.Forms.Button solveButton;
        private System.Windows.Forms.TrackBar speedSlider;
        private System.Windows.Forms.ComboBox sourceBox;
        private System.Windows.Forms.ComboBox targetBox;
        private System.Windows.Forms.Button moveButton;
        private System.Windows.Forms.TextBox discsBox;
    }
}