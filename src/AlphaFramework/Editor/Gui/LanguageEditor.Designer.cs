namespace AlphaFramework.Editor.Gui
{
    partial class LanguageEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.dataGridKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonUndo
            // 
            this.buttonUndo.Location = new System.Drawing.Point(697, 517);
            // 
            // buttonRedo
            // 
            this.buttonRedo.Location = new System.Drawing.Point(697, 496);
            // 
            // dataGrid
            // 
            this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridKey,
            this.dataGridValue});
            this.dataGrid.Location = new System.Drawing.Point(5, 5);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.Size = new System.Drawing.Size(687, 525);
            this.dataGrid.TabIndex = 0;
            this.dataGrid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.OnDataGridDataError);
            // 
            // dataGridKey
            // 
            this.dataGridKey.DataPropertyName = "Key";
            this.dataGridKey.HeaderText = "Key";
            this.dataGridKey.MinimumWidth = 75;
            this.dataGridKey.Name = "dataGridKey";
            this.dataGridKey.Width = 125;
            // 
            // dataGridValue
            // 
            this.dataGridValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridValue.DataPropertyName = "Value";
            this.dataGridValue.HeaderText = "Value";
            this.dataGridValue.Name = "dataGridValue";
            // 
            // LanguageEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.dataGrid);
            this.Name = "LanguageEditor";
            this.NameUI = "Language Editor";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(718, 538);
            this.Controls.SetChildIndex(this.buttonRedo, 0);
            this.Controls.SetChildIndex(this.buttonUndo, 0);
            this.Controls.SetChildIndex(this.dataGrid, 0);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridKey;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridValue;

    }
}
