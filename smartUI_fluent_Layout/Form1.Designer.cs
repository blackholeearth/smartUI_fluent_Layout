namespace smartUI_fluent_Layout
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			btnSave = new Button();
			btnCancel = new Button();
			dataGridView1 = new DataGridView();
			label1 = new Label();
			textBox1 = new TextBox();
			label2 = new Label();
			btnZoomIn = new Button();
			btnZoomOut = new Button();
			btnReset = new Button();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			SuspendLayout();
			// 
			// btnSave
			// 
			btnSave.Location = new Point(301, 337);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(94, 29);
			btnSave.TabIndex = 0;
			btnSave.Text = "save";
			btnSave.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			btnCancel.Location = new Point(429, 337);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(94, 29);
			btnCancel.TabIndex = 1;
			btnCancel.Text = "cancel";
			btnCancel.UseVisualStyleBackColor = true;
			// 
			// dataGridView1
			// 
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Location = new Point(271, 131);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.RowHeadersWidth = 51;
			dataGridView1.Size = new Size(300, 188);
			dataGridView1.TabIndex = 3;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(65, 16);
			label1.Name = "label1";
			label1.Size = new Size(196, 20);
			label1.TabIndex = 4;
			label1.Text = "Smart UI for Winforms - lbl1";
			// 
			// textBox1
			// 
			textBox1.Location = new Point(366, 75);
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(125, 27);
			textBox1.TabIndex = 5;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(259, 75);
			label2.Name = "label2";
			label2.Size = new Size(50, 20);
			label2.TabIndex = 6;
			label2.Text = "label2";
			// 
			// btnZoomIn
			// 
			btnZoomIn.Location = new Point(477, 12);
			btnZoomIn.Name = "btnZoomIn";
			btnZoomIn.Size = new Size(63, 29);
			btnZoomIn.TabIndex = 7;
			btnZoomIn.Text = "+";
			btnZoomIn.UseVisualStyleBackColor = true;
			// 
			// btnZoomOut
			// 
			btnZoomOut.Location = new Point(577, 12);
			btnZoomOut.Name = "btnZoomOut";
			btnZoomOut.Size = new Size(63, 29);
			btnZoomOut.TabIndex = 8;
			btnZoomOut.Text = "-";
			btnZoomOut.UseVisualStyleBackColor = true;
			// 
			// btnReset
			// 
			btnReset.Location = new Point(677, 12);
			btnReset.Name = "btnReset";
			btnReset.Size = new Size(63, 29);
			btnReset.TabIndex = 9;
			btnReset.Text = "Reset";
			btnReset.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(btnReset);
			Controls.Add(btnZoomOut);
			Controls.Add(btnZoomIn);
			Controls.Add(label2);
			Controls.Add(textBox1);
			Controls.Add(label1);
			Controls.Add(dataGridView1);
			Controls.Add(btnCancel);
			Controls.Add(btnSave);
			Name = "Form1";
			Text = "Form1";
			Load += Form1_Load;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button btnSave;
		private Button btnCancel;
		private DataGridView dataGridView1;
		private Label label1;
		private TextBox textBox1;
		private Label label2;
		private Button btnZoomIn;
		private Button btnZoomOut;
		private Button btnReset;
	}
}
