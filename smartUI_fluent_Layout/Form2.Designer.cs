namespace smartUI_fluent_Layout
{
	partial class Form2
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
			btnReset = new Button();
			btnZoomOut = new Button();
			btnZoomIn = new Button();
			label2 = new Label();
			textBox1 = new TextBox();
			label1 = new Label();
			dataGridView1 = new DataGridView();
			btnCancel = new Button();
			btnSave = new Button();
			lblStatus = new Label();
			btnMenu1 = new Button();
			btnMenu2 = new Button();
			btnMenu3 = new Button();
			lblVersion = new Label();
			btnHelp = new Button();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			SuspendLayout();
			// 
			// btnReset
			// 
			btnReset.Location = new Point(675, 48);
			btnReset.Name = "btnReset";
			btnReset.Size = new Size(63, 29);
			btnReset.TabIndex = 18;
			btnReset.Text = "Reset";
			btnReset.UseVisualStyleBackColor = true;
			// 
			// btnZoomOut
			// 
			btnZoomOut.Location = new Point(575, 48);
			btnZoomOut.Name = "btnZoomOut";
			btnZoomOut.Size = new Size(63, 29);
			btnZoomOut.TabIndex = 17;
			btnZoomOut.Text = "-";
			btnZoomOut.UseVisualStyleBackColor = true;
			// 
			// btnZoomIn
			// 
			btnZoomIn.Location = new Point(475, 48);
			btnZoomIn.Name = "btnZoomIn";
			btnZoomIn.Size = new Size(63, 29);
			btnZoomIn.TabIndex = 16;
			btnZoomIn.Text = "+";
			btnZoomIn.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(257, 111);
			label2.Name = "label2";
			label2.Size = new Size(50, 20);
			label2.TabIndex = 15;
			label2.Text = "label2";
			// 
			// textBox1
			// 
			textBox1.Location = new Point(364, 111);
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(125, 27);
			textBox1.TabIndex = 14;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(63, 52);
			label1.Name = "label1";
			label1.Size = new Size(196, 20);
			label1.TabIndex = 13;
			label1.Text = "Smart UI for Winforms - lbl1";
			// 
			// dataGridView1
			// 
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Location = new Point(269, 167);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.RowHeadersWidth = 51;
			dataGridView1.Size = new Size(300, 188);
			dataGridView1.TabIndex = 12;
			// 
			// btnCancel
			// 
			btnCancel.Location = new Point(427, 373);
			btnCancel.Name = "btnCancel";
			btnCancel.Size = new Size(94, 29);
			btnCancel.TabIndex = 11;
			btnCancel.Text = "cancel";
			btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnSave
			// 
			btnSave.Location = new Point(299, 373);
			btnSave.Name = "btnSave";
			btnSave.Size = new Size(94, 29);
			btnSave.TabIndex = 10;
			btnSave.Text = "save";
			btnSave.UseVisualStyleBackColor = true;
			// 
			// lblStatus
			// 
			lblStatus.AutoSize = true;
			lblStatus.Location = new Point(63, 215);
			lblStatus.Name = "lblStatus";
			lblStatus.Size = new Size(66, 20);
			lblStatus.TabIndex = 19;
			lblStatus.Text = "lblStatus";
			// 
			// btnMenu1
			// 
			btnMenu1.Location = new Point(35, 12);
			btnMenu1.Name = "btnMenu1";
			btnMenu1.Size = new Size(94, 29);
			btnMenu1.TabIndex = 20;
			btnMenu1.Text = "btnMenu1";
			btnMenu1.UseVisualStyleBackColor = true;
			// 
			// btnMenu2
			// 
			btnMenu2.Location = new Point(165, 12);
			btnMenu2.Name = "btnMenu2";
			btnMenu2.Size = new Size(94, 29);
			btnMenu2.TabIndex = 21;
			btnMenu2.Text = "btnMenu2";
			btnMenu2.UseVisualStyleBackColor = true;
			// 
			// btnMenu3
			// 
			btnMenu3.Location = new Point(280, 12);
			btnMenu3.Name = "btnMenu3";
			btnMenu3.Size = new Size(94, 29);
			btnMenu3.TabIndex = 21;
			btnMenu3.Text = "btnMenu3";
			btnMenu3.UseVisualStyleBackColor = true;
			// 
			// lblVersion
			// 
			lblVersion.AutoSize = true;
			lblVersion.Location = new Point(63, 262);
			lblVersion.Name = "lblVersion";
			lblVersion.Size = new Size(74, 20);
			lblVersion.TabIndex = 22;
			lblVersion.Text = "lblVersion";
			// 
			// btnHelp
			// 
			btnHelp.Location = new Point(63, 326);
			btnHelp.Name = "btnHelp";
			btnHelp.Size = new Size(94, 29);
			btnHelp.TabIndex = 10;
			btnHelp.Text = "Help";
			btnHelp.UseVisualStyleBackColor = true;
			// 
			// Form2
			// 
			AutoScaleDimensions = new SizeF(8F, 20F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(lblVersion);
			Controls.Add(btnMenu3);
			Controls.Add(btnMenu2);
			Controls.Add(btnMenu1);
			Controls.Add(lblStatus);
			Controls.Add(btnReset);
			Controls.Add(btnZoomOut);
			Controls.Add(btnZoomIn);
			Controls.Add(label2);
			Controls.Add(textBox1);
			Controls.Add(label1);
			Controls.Add(dataGridView1);
			Controls.Add(btnCancel);
			Controls.Add(btnHelp);
			Controls.Add(btnSave);
			Name = "Form2";
			Text = "Form2";
			Load += Form2_Load;
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Button btnReset;
		private Button btnZoomOut;
		private Button btnZoomIn;
		private Label label2;
		private TextBox textBox1;
		private Label label1;
		private DataGridView dataGridView1;
		private Button btnCancel;
		private Button btnSave;
		private Label lblStatus;
		private Button btnMenu1;
		private Button btnMenu2;
		private Button btnMenu3;
		private Label lblVersion;
		private Button btnHelp;
	}
}