namespace BlueBrick
{
	partial class EditRulerForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditRulerForm));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.defaultFontColorLabel = new System.Windows.Forms.Label();
			this.lineColorPictureBox = new System.Windows.Forms.PictureBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.label3 = new System.Windows.Forms.Label();
			this.guidelineColorPictureBox = new System.Windows.Forms.PictureBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.displayMesureTextCheckBox = new System.Windows.Forms.CheckBox();
			this.displayUnitCheckBox = new System.Windows.Forms.CheckBox();
			this.fontNameLabel = new System.Windows.Forms.Label();
			this.fontColorLabel = new System.Windows.Forms.Label();
			this.fontColorPictureBox = new System.Windows.Forms.PictureBox();
			this.fontButton = new System.Windows.Forms.Button();
			this.unitComboBox = new System.Windows.Forms.ComboBox();
			this.allowOffsetCheckBox = new System.Windows.Forms.CheckBox();
			this.unitLabel = new System.Windows.Forms.Label();
			this.lineThicknessNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.guidelineThicknessNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.dashPatternLineNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.dashPatternSpaceNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineColorPictureBox)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.guidelineColorPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lineThicknessNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.guidelineThicknessNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dashPatternLineNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dashPatternSpaceNumericUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cancelButton.Location = new System.Drawing.Point(12, 310);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(116, 24);
			this.cancelButton.TabIndex = 8;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.okButton.Location = new System.Drawing.Point(307, 310);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(116, 24);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "Ok";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.unitLabel);
			this.groupBox1.Controls.Add(this.unitComboBox);
			this.groupBox1.Controls.Add(this.fontNameLabel);
			this.groupBox1.Controls.Add(this.fontColorLabel);
			this.groupBox1.Controls.Add(this.fontColorPictureBox);
			this.groupBox1.Controls.Add(this.fontButton);
			this.groupBox1.Controls.Add(this.displayUnitCheckBox);
			this.groupBox1.Controls.Add(this.displayMesureTextCheckBox);
			this.groupBox1.Location = new System.Drawing.Point(13, 123);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(408, 178);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Measure Value";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lineThicknessNumericUpDown);
			this.groupBox2.Controls.Add(this.allowOffsetCheckBox);
			this.groupBox2.Controls.Add(this.defaultFontColorLabel);
			this.groupBox2.Controls.Add(this.lineColorPictureBox);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Location = new System.Drawing.Point(13, 9);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(197, 108);
			this.groupBox2.TabIndex = 11;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Line Appearance";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(18, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(97, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Thickness (in pixel)";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// defaultFontColorLabel
			// 
			this.defaultFontColorLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.defaultFontColorLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.defaultFontColorLabel.Location = new System.Drawing.Point(50, 55);
			this.defaultFontColorLabel.Name = "defaultFontColorLabel";
			this.defaultFontColorLabel.Size = new System.Drawing.Size(65, 13);
			this.defaultFontColorLabel.TabIndex = 23;
			this.defaultFontColorLabel.Text = "Color";
			this.defaultFontColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// lineColorPictureBox
			// 
			this.lineColorPictureBox.BackColor = System.Drawing.Color.White;
			this.lineColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lineColorPictureBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.lineColorPictureBox.Location = new System.Drawing.Point(121, 55);
			this.lineColorPictureBox.Name = "lineColorPictureBox";
			this.lineColorPictureBox.Size = new System.Drawing.Size(16, 16);
			this.lineColorPictureBox.TabIndex = 22;
			this.lineColorPictureBox.TabStop = false;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.dashPatternSpaceNumericUpDown);
			this.groupBox3.Controls.Add(this.dashPatternLineNumericUpDown);
			this.groupBox3.Controls.Add(this.guidelineThicknessNumericUpDown);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.guidelineColorPictureBox);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Location = new System.Drawing.Point(216, 12);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(205, 105);
			this.groupBox3.TabIndex = 12;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Guideline Appearance";
			// 
			// label3
			// 
			this.label3.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label3.Location = new System.Drawing.Point(50, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(65, 13);
			this.label3.TabIndex = 23;
			this.label3.Text = "Color";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// guidelineColorPictureBox
			// 
			this.guidelineColorPictureBox.BackColor = System.Drawing.Color.White;
			this.guidelineColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.guidelineColorPictureBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.guidelineColorPictureBox.Location = new System.Drawing.Point(121, 80);
			this.guidelineColorPictureBox.Name = "guidelineColorPictureBox";
			this.guidelineColorPictureBox.Size = new System.Drawing.Size(16, 16);
			this.guidelineColorPictureBox.TabIndex = 22;
			this.guidelineColorPictureBox.TabStop = false;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(18, 52);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(97, 13);
			this.label5.TabIndex = 0;
			this.label5.Text = "Thickness (in pixel)";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(46, 16);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(69, 26);
			this.label7.TabIndex = 24;
			this.label7.Text = "Dash Pattern\r\n(in pixel)";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// displayMesureTextCheckBox
			// 
			this.displayMesureTextCheckBox.AutoSize = true;
			this.displayMesureTextCheckBox.Location = new System.Drawing.Point(6, 19);
			this.displayMesureTextCheckBox.Name = "displayMesureTextCheckBox";
			this.displayMesureTextCheckBox.Size = new System.Drawing.Size(134, 17);
			this.displayMesureTextCheckBox.TabIndex = 0;
			this.displayMesureTextCheckBox.Text = "Display Measure Value";
			this.displayMesureTextCheckBox.UseVisualStyleBackColor = true;
			this.displayMesureTextCheckBox.CheckedChanged += new System.EventHandler(this.displayMesureTextCheckBox_CheckedChanged);
			// 
			// displayUnitCheckBox
			// 
			this.displayUnitCheckBox.AutoSize = true;
			this.displayUnitCheckBox.Location = new System.Drawing.Point(36, 45);
			this.displayUnitCheckBox.Name = "displayUnitCheckBox";
			this.displayUnitCheckBox.Size = new System.Drawing.Size(82, 17);
			this.displayUnitCheckBox.TabIndex = 1;
			this.displayUnitCheckBox.Text = "Display Unit";
			this.displayUnitCheckBox.UseVisualStyleBackColor = true;
			this.displayUnitCheckBox.CheckedChanged += new System.EventHandler(this.displayUnitCheckBox_CheckedChanged);
			// 
			// fontNameLabel
			// 
			this.fontNameLabel.BackColor = System.Drawing.SystemColors.InactiveBorder;
			this.fontNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.fontNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.fontNameLabel.Location = new System.Drawing.Point(36, 113);
			this.fontNameLabel.Name = "fontNameLabel";
			this.fontNameLabel.Size = new System.Drawing.Size(337, 53);
			this.fontNameLabel.TabIndex = 26;
			this.fontNameLabel.Text = "Font name";
			this.fontNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// fontColorLabel
			// 
			this.fontColorLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.fontColorLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.fontColorLabel.Location = new System.Drawing.Point(231, 84);
			this.fontColorLabel.Name = "fontColorLabel";
			this.fontColorLabel.Size = new System.Drawing.Size(65, 13);
			this.fontColorLabel.TabIndex = 25;
			this.fontColorLabel.Text = "Color";
			this.fontColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// fontColorPictureBox
			// 
			this.fontColorPictureBox.BackColor = System.Drawing.Color.White;
			this.fontColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.fontColorPictureBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.fontColorPictureBox.Location = new System.Drawing.Point(302, 83);
			this.fontColorPictureBox.Name = "fontColorPictureBox";
			this.fontColorPictureBox.Size = new System.Drawing.Size(16, 16);
			this.fontColorPictureBox.TabIndex = 24;
			this.fontColorPictureBox.TabStop = false;
			// 
			// fontButton
			// 
			this.fontButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.fontButton.Location = new System.Drawing.Point(36, 78);
			this.fontButton.Name = "fontButton";
			this.fontButton.Size = new System.Drawing.Size(142, 23);
			this.fontButton.TabIndex = 23;
			this.fontButton.Text = "Change Font";
			this.fontButton.UseVisualStyleBackColor = true;
			// 
			// unitComboBox
			// 
			this.unitComboBox.FormattingEnabled = true;
			this.unitComboBox.Items.AddRange(new object[] {
            "Stud",
            "LDU",
            "Module",
            "Meter",
            "Feet"});
			this.unitComboBox.Location = new System.Drawing.Point(252, 45);
			this.unitComboBox.Name = "unitComboBox";
			this.unitComboBox.Size = new System.Drawing.Size(121, 21);
			this.unitComboBox.TabIndex = 27;
			// 
			// allowOffsetCheckBox
			// 
			this.allowOffsetCheckBox.AutoSize = true;
			this.allowOffsetCheckBox.Location = new System.Drawing.Point(21, 79);
			this.allowOffsetCheckBox.Name = "allowOffsetCheckBox";
			this.allowOffsetCheckBox.Size = new System.Drawing.Size(82, 17);
			this.allowOffsetCheckBox.TabIndex = 24;
			this.allowOffsetCheckBox.Text = "Allow Offset";
			this.allowOffsetCheckBox.UseVisualStyleBackColor = true;
			// 
			// unitLabel
			// 
			this.unitLabel.AutoSize = true;
			this.unitLabel.Location = new System.Drawing.Point(220, 49);
			this.unitLabel.Name = "unitLabel";
			this.unitLabel.Size = new System.Drawing.Size(26, 13);
			this.unitLabel.TabIndex = 28;
			this.unitLabel.Text = "Unit";
			// 
			// lineThicknessNumericUpDown
			// 
			this.lineThicknessNumericUpDown.DecimalPlaces = 1;
			this.lineThicknessNumericUpDown.Location = new System.Drawing.Point(121, 24);
			this.lineThicknessNumericUpDown.Name = "lineThicknessNumericUpDown";
			this.lineThicknessNumericUpDown.Size = new System.Drawing.Size(48, 20);
			this.lineThicknessNumericUpDown.TabIndex = 25;
			// 
			// guidelineThicknessNumericUpDown
			// 
			this.guidelineThicknessNumericUpDown.DecimalPlaces = 1;
			this.guidelineThicknessNumericUpDown.Location = new System.Drawing.Point(121, 50);
			this.guidelineThicknessNumericUpDown.Name = "guidelineThicknessNumericUpDown";
			this.guidelineThicknessNumericUpDown.Size = new System.Drawing.Size(48, 20);
			this.guidelineThicknessNumericUpDown.TabIndex = 28;
			// 
			// dashPatternLineNumericUpDown
			// 
			this.dashPatternLineNumericUpDown.Location = new System.Drawing.Point(121, 21);
			this.dashPatternLineNumericUpDown.Name = "dashPatternLineNumericUpDown";
			this.dashPatternLineNumericUpDown.Size = new System.Drawing.Size(37, 20);
			this.dashPatternLineNumericUpDown.TabIndex = 29;
			// 
			// dashPatternSpaceNumericUpDown
			// 
			this.dashPatternSpaceNumericUpDown.Location = new System.Drawing.Point(164, 21);
			this.dashPatternSpaceNumericUpDown.Name = "dashPatternSpaceNumericUpDown";
			this.dashPatternSpaceNumericUpDown.Size = new System.Drawing.Size(35, 20);
			this.dashPatternSpaceNumericUpDown.TabIndex = 30;
			// 
			// EditRulerForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(435, 346);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditRulerForm";
			this.Text = "Ruler Edition";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineColorPictureBox)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.guidelineColorPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lineThicknessNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.guidelineThicknessNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dashPatternLineNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dashPatternSpaceNumericUpDown)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label defaultFontColorLabel;
		private System.Windows.Forms.PictureBox lineColorPictureBox;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox guidelineColorPictureBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox displayUnitCheckBox;
		private System.Windows.Forms.CheckBox displayMesureTextCheckBox;
		private System.Windows.Forms.Label unitLabel;
		private System.Windows.Forms.ComboBox unitComboBox;
		private System.Windows.Forms.Label fontNameLabel;
		private System.Windows.Forms.Label fontColorLabel;
		private System.Windows.Forms.PictureBox fontColorPictureBox;
		private System.Windows.Forms.Button fontButton;
		private System.Windows.Forms.CheckBox allowOffsetCheckBox;
		private System.Windows.Forms.NumericUpDown lineThicknessNumericUpDown;
		private System.Windows.Forms.NumericUpDown dashPatternSpaceNumericUpDown;
		private System.Windows.Forms.NumericUpDown dashPatternLineNumericUpDown;
		private System.Windows.Forms.NumericUpDown guidelineThicknessNumericUpDown;
	}
}