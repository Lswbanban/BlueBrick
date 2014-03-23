namespace BlueBrick
{
	partial class ExportImageForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportImageForm));
			this.label8 = new System.Windows.Forms.Label();
			this.previewPictureBox = new System.Windows.Forms.PictureBox();
			this.settingAndButtonTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.cancelButton = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.linkPictureBox = new System.Windows.Forms.PictureBox();
			this.imageWidthNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.scaleNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.imageHeightNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.okButton = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.areaTopNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.areaLeftNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.areaBottomNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.areaRightNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.topTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.exportConnectionPointCheckBox = new System.Windows.Forms.CheckBox();
			this.exportElectricCircuitCheckBox = new System.Windows.Forms.CheckBox();
			this.exportWatermarkCheckBox = new System.Windows.Forms.CheckBox();
			this.exportHullCheckBox = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
			this.settingAndButtonTableLayoutPanel.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.linkPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imageWidthNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.scaleNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.imageHeightNumericUpDown)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.areaTopNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.areaLeftNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.areaBottomNumericUpDown)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.areaRightNumericUpDown)).BeginInit();
			this.topTableLayoutPanel.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// label8
			// 
			resources.ApplyResources(this.label8, "label8");
			this.label8.Name = "label8";
			// 
			// previewPictureBox
			// 
			resources.ApplyResources(this.previewPictureBox, "previewPictureBox");
			this.previewPictureBox.Name = "previewPictureBox";
			this.previewPictureBox.TabStop = false;
			this.previewPictureBox.DoubleClick += new System.EventHandler(this.previewPictureBox_DoubleClick);
			this.previewPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.previewPictureBox_MouseDown);
			this.previewPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.previewPictureBox_MouseMove);
			this.previewPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.previewPictureBox_MouseUp);
			// 
			// settingAndButtonTableLayoutPanel
			// 
			resources.ApplyResources(this.settingAndButtonTableLayoutPanel, "settingAndButtonTableLayoutPanel");
			this.settingAndButtonTableLayoutPanel.Controls.Add(this.cancelButton, 0, 1);
			this.settingAndButtonTableLayoutPanel.Controls.Add(this.groupBox2, 1, 0);
			this.settingAndButtonTableLayoutPanel.Controls.Add(this.okButton, 1, 1);
			this.settingAndButtonTableLayoutPanel.Controls.Add(this.groupBox1, 0, 0);
			this.settingAndButtonTableLayoutPanel.Name = "settingAndButtonTableLayoutPanel";
			// 
			// cancelButton
			// 
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.linkPictureBox);
			this.groupBox2.Controls.Add(this.imageWidthNumericUpDown);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.scaleNumericUpDown);
			this.groupBox2.Controls.Add(this.imageHeightNumericUpDown);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// label4
			// 
			resources.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			// 
			// linkPictureBox
			// 
			resources.ApplyResources(this.linkPictureBox, "linkPictureBox");
			this.linkPictureBox.Name = "linkPictureBox";
			this.linkPictureBox.TabStop = false;
			// 
			// imageWidthNumericUpDown
			// 
			resources.ApplyResources(this.imageWidthNumericUpDown, "imageWidthNumericUpDown");
			this.imageWidthNumericUpDown.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.imageWidthNumericUpDown.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.imageWidthNumericUpDown.Name = "imageWidthNumericUpDown";
			this.imageWidthNumericUpDown.Value = new decimal(new int[] {
            800,
            0,
            0,
            0});
			this.imageWidthNumericUpDown.ValueChanged += new System.EventHandler(this.imageWidthNumericUpDown_ValueChanged);
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// label7
			// 
			resources.ApplyResources(this.label7, "label7");
			this.label7.Name = "label7";
			// 
			// scaleNumericUpDown
			// 
			resources.ApplyResources(this.scaleNumericUpDown, "scaleNumericUpDown");
			this.scaleNumericUpDown.DecimalPlaces = 2;
			this.scaleNumericUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.scaleNumericUpDown.Name = "scaleNumericUpDown";
			this.scaleNumericUpDown.ValueChanged += new System.EventHandler(this.scaleNumericUpDown_ValueChanged);
			// 
			// imageHeightNumericUpDown
			// 
			resources.ApplyResources(this.imageHeightNumericUpDown, "imageHeightNumericUpDown");
			this.imageHeightNumericUpDown.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
			this.imageHeightNumericUpDown.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
			this.imageHeightNumericUpDown.Name = "imageHeightNumericUpDown";
			this.imageHeightNumericUpDown.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
			this.imageHeightNumericUpDown.ValueChanged += new System.EventHandler(this.imageHeightNumericUpDown_ValueChanged);
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
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.areaTopNumericUpDown);
			this.groupBox1.Controls.Add(this.areaLeftNumericUpDown);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.areaBottomNumericUpDown);
			this.groupBox1.Controls.Add(this.areaRightNumericUpDown);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// label5
			// 
			resources.ApplyResources(this.label5, "label5");
			this.label5.Name = "label5";
			// 
			// label6
			// 
			resources.ApplyResources(this.label6, "label6");
			this.label6.Name = "label6";
			// 
			// areaTopNumericUpDown
			// 
			resources.ApplyResources(this.areaTopNumericUpDown, "areaTopNumericUpDown");
			this.areaTopNumericUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.areaTopNumericUpDown.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
			this.areaTopNumericUpDown.Name = "areaTopNumericUpDown";
			this.areaTopNumericUpDown.ValueChanged += new System.EventHandler(this.areaTopNumericUpDown_ValueChanged);
			// 
			// areaLeftNumericUpDown
			// 
			resources.ApplyResources(this.areaLeftNumericUpDown, "areaLeftNumericUpDown");
			this.areaLeftNumericUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
			this.areaLeftNumericUpDown.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
			this.areaLeftNumericUpDown.Name = "areaLeftNumericUpDown";
			this.areaLeftNumericUpDown.ValueChanged += new System.EventHandler(this.areaLeftNumericUpDown_ValueChanged);
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// areaBottomNumericUpDown
			// 
			resources.ApplyResources(this.areaBottomNumericUpDown, "areaBottomNumericUpDown");
			this.areaBottomNumericUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.areaBottomNumericUpDown.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
			this.areaBottomNumericUpDown.Name = "areaBottomNumericUpDown";
			this.areaBottomNumericUpDown.Value = new decimal(new int[] {
            96,
            0,
            0,
            0});
			this.areaBottomNumericUpDown.ValueChanged += new System.EventHandler(this.areaBottomNumericUpDown_ValueChanged);
			// 
			// areaRightNumericUpDown
			// 
			resources.ApplyResources(this.areaRightNumericUpDown, "areaRightNumericUpDown");
			this.areaRightNumericUpDown.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
			this.areaRightNumericUpDown.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
			this.areaRightNumericUpDown.Name = "areaRightNumericUpDown";
			this.areaRightNumericUpDown.Value = new decimal(new int[] {
            96,
            0,
            0,
            0});
			this.areaRightNumericUpDown.ValueChanged += new System.EventHandler(this.areaRightNumericUpDown_ValueChanged);
			// 
			// topTableLayoutPanel
			// 
			resources.ApplyResources(this.topTableLayoutPanel, "topTableLayoutPanel");
			this.topTableLayoutPanel.Controls.Add(this.settingAndButtonTableLayoutPanel, 0, 3);
			this.topTableLayoutPanel.Controls.Add(this.previewPictureBox, 0, 1);
			this.topTableLayoutPanel.Controls.Add(this.label8, 0, 0);
			this.topTableLayoutPanel.Controls.Add(this.groupBox3, 0, 2);
			this.topTableLayoutPanel.Name = "topTableLayoutPanel";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.exportConnectionPointCheckBox);
			this.groupBox3.Controls.Add(this.exportElectricCircuitCheckBox);
			this.groupBox3.Controls.Add(this.exportWatermarkCheckBox);
			this.groupBox3.Controls.Add(this.exportHullCheckBox);
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			// 
			// exportConnectionPointCheckBox
			// 
			resources.ApplyResources(this.exportConnectionPointCheckBox, "exportConnectionPointCheckBox");
			this.exportConnectionPointCheckBox.Name = "exportConnectionPointCheckBox";
			this.exportConnectionPointCheckBox.UseVisualStyleBackColor = true;
			this.exportConnectionPointCheckBox.Click += new System.EventHandler(this.exportConnectionPointCheckBox_Click);
			// 
			// exportElectricCircuitCheckBox
			// 
			resources.ApplyResources(this.exportElectricCircuitCheckBox, "exportElectricCircuitCheckBox");
			this.exportElectricCircuitCheckBox.Name = "exportElectricCircuitCheckBox";
			this.exportElectricCircuitCheckBox.UseVisualStyleBackColor = true;
			this.exportElectricCircuitCheckBox.Click += new System.EventHandler(this.exportElectricCircuitCheckBox_Click);
			// 
			// exportWatermarkCheckBox
			// 
			resources.ApplyResources(this.exportWatermarkCheckBox, "exportWatermarkCheckBox");
			this.exportWatermarkCheckBox.Name = "exportWatermarkCheckBox";
			this.exportWatermarkCheckBox.UseVisualStyleBackColor = true;
			this.exportWatermarkCheckBox.Click += new System.EventHandler(this.exportWatermarkCheckBox_Click);
			// 
			// exportHullCheckBox
			// 
			resources.ApplyResources(this.exportHullCheckBox, "exportHullCheckBox");
			this.exportHullCheckBox.Name = "exportHullCheckBox";
			this.exportHullCheckBox.UseVisualStyleBackColor = true;
			this.exportHullCheckBox.Click += new System.EventHandler(this.exportHullCheckBox_Click);
			// 
			// ExportImageForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.topTableLayoutPanel);
			this.DoubleBuffered = true;
			this.MinimizeBox = false;
			this.Name = "ExportImageForm";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExportImageForm_FormClosed);
			this.SizeChanged += new System.EventHandler(this.ExportImageForm_SizeChanged);
			((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
			this.settingAndButtonTableLayoutPanel.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.linkPictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imageWidthNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.scaleNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.imageHeightNumericUpDown)).EndInit();
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.areaTopNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.areaLeftNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.areaBottomNumericUpDown)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.areaRightNumericUpDown)).EndInit();
			this.topTableLayoutPanel.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown areaBottomNumericUpDown;
		private System.Windows.Forms.NumericUpDown areaRightNumericUpDown;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown imageHeightNumericUpDown;
		private System.Windows.Forms.NumericUpDown imageWidthNumericUpDown;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown areaTopNumericUpDown;
		private System.Windows.Forms.NumericUpDown areaLeftNumericUpDown;
		private System.Windows.Forms.PictureBox previewPictureBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown scaleNumericUpDown;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TableLayoutPanel settingAndButtonTableLayoutPanel;
		private System.Windows.Forms.PictureBox linkPictureBox;
		private System.Windows.Forms.TableLayoutPanel topTableLayoutPanel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox exportHullCheckBox;
        private System.Windows.Forms.CheckBox exportConnectionPointCheckBox;
        private System.Windows.Forms.CheckBox exportElectricCircuitCheckBox;
        private System.Windows.Forms.CheckBox exportWatermarkCheckBox;
	}
}