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
			this.unitLabel = new System.Windows.Forms.Label();
			this.unitComboBox = new System.Windows.Forms.ComboBox();
			this.fontNameLabel = new System.Windows.Forms.Label();
			this.fontColorLabel = new System.Windows.Forms.Label();
			this.fontColorPictureBox = new System.Windows.Forms.PictureBox();
			this.fontButton = new System.Windows.Forms.Button();
			this.displayUnitCheckBox = new System.Windows.Forms.CheckBox();
			this.displayMeasureTextCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lineThicknessNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.allowOffsetCheckBox = new System.Windows.Forms.CheckBox();
			this.lineColorLabel = new System.Windows.Forms.Label();
			this.lineColorPictureBox = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.dashPatternSpaceNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.dashPatternLineNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.guidelineThicknessNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.guidelineColorPictureBox = new System.Windows.Forms.PictureBox();
			this.label5 = new System.Windows.Forms.Label();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.fontDialog = new System.Windows.Forms.FontDialog();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).BeginInit();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineThicknessNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.lineColorPictureBox)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dashPatternSpaceNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dashPatternLineNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.guidelineThicknessNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.guidelineColorPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			resources.ApplyResources(this.okButton, "okButton");
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Name = "okButton";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// groupBox1
			// 
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Controls.Add(this.unitLabel);
			this.groupBox1.Controls.Add(this.unitComboBox);
			this.groupBox1.Controls.Add(this.fontNameLabel);
			this.groupBox1.Controls.Add(this.fontColorLabel);
			this.groupBox1.Controls.Add(this.fontColorPictureBox);
			this.groupBox1.Controls.Add(this.fontButton);
			this.groupBox1.Controls.Add(this.displayUnitCheckBox);
			this.groupBox1.Controls.Add(this.displayMeasureTextCheckBox);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// unitLabel
			// 
			resources.ApplyResources(this.unitLabel, "unitLabel");
			this.unitLabel.Name = "unitLabel";
			// 
			// unitComboBox
			// 
			resources.ApplyResources(this.unitComboBox, "unitComboBox");
			this.unitComboBox.FormattingEnabled = true;
			this.unitComboBox.Items.AddRange(new object[] {
            resources.GetString("unitComboBox.Items"),
            resources.GetString("unitComboBox.Items1"),
            resources.GetString("unitComboBox.Items2"),
            resources.GetString("unitComboBox.Items3"),
            resources.GetString("unitComboBox.Items4")});
			this.unitComboBox.Name = "unitComboBox";
			// 
			// fontNameLabel
			// 
			resources.ApplyResources(this.fontNameLabel, "fontNameLabel");
			this.fontNameLabel.BackColor = System.Drawing.SystemColors.InactiveBorder;
			this.fontNameLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.fontNameLabel.Name = "fontNameLabel";
			// 
			// fontColorLabel
			// 
			resources.ApplyResources(this.fontColorLabel, "fontColorLabel");
			this.fontColorLabel.Name = "fontColorLabel";
			// 
			// fontColorPictureBox
			// 
			resources.ApplyResources(this.fontColorPictureBox, "fontColorPictureBox");
			this.fontColorPictureBox.BackColor = System.Drawing.Color.White;
			this.fontColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.fontColorPictureBox.Name = "fontColorPictureBox";
			this.fontColorPictureBox.TabStop = false;
			this.fontColorPictureBox.Click += new System.EventHandler(this.fontColorPictureBox_Click);
			// 
			// fontButton
			// 
			resources.ApplyResources(this.fontButton, "fontButton");
			this.fontButton.Name = "fontButton";
			this.fontButton.UseVisualStyleBackColor = true;
			this.fontButton.Click += new System.EventHandler(this.fontButton_Click);
			// 
			// displayUnitCheckBox
			// 
			resources.ApplyResources(this.displayUnitCheckBox, "displayUnitCheckBox");
			this.displayUnitCheckBox.Name = "displayUnitCheckBox";
			this.displayUnitCheckBox.UseVisualStyleBackColor = true;
			// 
			// displayMeasureTextCheckBox
			// 
			resources.ApplyResources(this.displayMeasureTextCheckBox, "displayMeasureTextCheckBox");
			this.displayMeasureTextCheckBox.Name = "displayMeasureTextCheckBox";
			this.displayMeasureTextCheckBox.UseVisualStyleBackColor = true;
			this.displayMeasureTextCheckBox.CheckedChanged += new System.EventHandler(this.displayMeasureTextCheckBox_CheckedChanged);
			// 
			// groupBox2
			// 
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this.lineThicknessNumericUpDown);
			this.groupBox2.Controls.Add(this.allowOffsetCheckBox);
			this.groupBox2.Controls.Add(this.lineColorLabel);
			this.groupBox2.Controls.Add(this.lineColorPictureBox);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// lineThicknessNumericUpDown
			// 
			resources.ApplyResources(this.lineThicknessNumericUpDown, "lineThicknessNumericUpDown");
			this.lineThicknessNumericUpDown.DecimalPlaces = 1;
			this.lineThicknessNumericUpDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.lineThicknessNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.lineThicknessNumericUpDown.Name = "lineThicknessNumericUpDown";
			this.lineThicknessNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// allowOffsetCheckBox
			// 
			resources.ApplyResources(this.allowOffsetCheckBox, "allowOffsetCheckBox");
			this.allowOffsetCheckBox.Name = "allowOffsetCheckBox";
			this.allowOffsetCheckBox.UseVisualStyleBackColor = true;
			// 
			// lineColorLabel
			// 
			resources.ApplyResources(this.lineColorLabel, "lineColorLabel");
			this.lineColorLabel.Name = "lineColorLabel";
			// 
			// lineColorPictureBox
			// 
			resources.ApplyResources(this.lineColorPictureBox, "lineColorPictureBox");
			this.lineColorPictureBox.BackColor = System.Drawing.Color.White;
			this.lineColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.lineColorPictureBox.Name = "lineColorPictureBox";
			this.lineColorPictureBox.TabStop = false;
			this.lineColorPictureBox.Click += new System.EventHandler(this.lineColorPictureBox_Click);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// groupBox3
			// 
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Controls.Add(this.dashPatternSpaceNumericUpDown);
			this.groupBox3.Controls.Add(this.dashPatternLineNumericUpDown);
			this.groupBox3.Controls.Add(this.guidelineThicknessNumericUpDown);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.guidelineColorPictureBox);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			// 
			// dashPatternSpaceNumericUpDown
			// 
			resources.ApplyResources(this.dashPatternSpaceNumericUpDown, "dashPatternSpaceNumericUpDown");
			this.dashPatternSpaceNumericUpDown.DecimalPlaces = 1;
			this.dashPatternSpaceNumericUpDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.dashPatternSpaceNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.dashPatternSpaceNumericUpDown.Name = "dashPatternSpaceNumericUpDown";
			this.dashPatternSpaceNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// dashPatternLineNumericUpDown
			// 
			resources.ApplyResources(this.dashPatternLineNumericUpDown, "dashPatternLineNumericUpDown");
			this.dashPatternLineNumericUpDown.DecimalPlaces = 1;
			this.dashPatternLineNumericUpDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.dashPatternLineNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.dashPatternLineNumericUpDown.Name = "dashPatternLineNumericUpDown";
			this.dashPatternLineNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// guidelineThicknessNumericUpDown
			// 
			resources.ApplyResources(this.guidelineThicknessNumericUpDown, "guidelineThicknessNumericUpDown");
			this.guidelineThicknessNumericUpDown.DecimalPlaces = 1;
			this.guidelineThicknessNumericUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.guidelineThicknessNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.guidelineThicknessNumericUpDown.Name = "guidelineThicknessNumericUpDown";
			this.guidelineThicknessNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label7
			// 
			resources.ApplyResources(this.label7, "label7");
			this.label7.Name = "label7";
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// guidelineColorPictureBox
			// 
			resources.ApplyResources(this.guidelineColorPictureBox, "guidelineColorPictureBox");
			this.guidelineColorPictureBox.BackColor = System.Drawing.Color.White;
			this.guidelineColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.guidelineColorPictureBox.Name = "guidelineColorPictureBox";
			this.guidelineColorPictureBox.TabStop = false;
			this.guidelineColorPictureBox.Click += new System.EventHandler(this.guidelineColorPictureBox_Click);
			// 
			// label5
			// 
			resources.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			// 
			// EditRulerForm
			// 
			this.AcceptButton = this.okButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditRulerForm";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.lineThicknessNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.lineColorPictureBox)).EndInit();
			this.groupBox3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dashPatternSpaceNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dashPatternLineNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.guidelineThicknessNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.guidelineColorPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lineColorLabel;
		private System.Windows.Forms.PictureBox lineColorPictureBox;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox guidelineColorPictureBox;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox displayUnitCheckBox;
		private System.Windows.Forms.CheckBox displayMeasureTextCheckBox;
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
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.FontDialog fontDialog;
	}
}