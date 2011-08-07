namespace BlueBrick
{
	partial class LayerAreaOptionForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerAreaOptionForm));
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.isVisibleCheckBox = new System.Windows.Forms.CheckBox();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.alphaNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.alphaProgressBar = new System.Windows.Forms.ProgressBar();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cellSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.alphaNumericUpDown)).BeginInit();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cellSizeNumericUpDown)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonCancel
			// 
			this.buttonCancel.AccessibleDescription = null;
			this.buttonCancel.AccessibleName = null;
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.BackgroundImage = null;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Font = null;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			// 
			// buttonOk
			// 
			this.buttonOk.AccessibleDescription = null;
			this.buttonOk.AccessibleName = null;
			resources.ApplyResources(this.buttonOk, "buttonOk");
			this.buttonOk.BackgroundImage = null;
			this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOk.Font = null;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseVisualStyleBackColor = true;
			this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.AccessibleDescription = null;
			this.groupBox3.AccessibleName = null;
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.BackgroundImage = null;
			this.groupBox3.Controls.Add(this.isVisibleCheckBox);
			this.groupBox3.Controls.Add(this.nameTextBox);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.label2);
			this.groupBox3.Controls.Add(this.alphaNumericUpDown);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.alphaProgressBar);
			this.groupBox3.Font = null;
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.TabStop = false;
			// 
			// isVisibleCheckBox
			// 
			this.isVisibleCheckBox.AccessibleDescription = null;
			this.isVisibleCheckBox.AccessibleName = null;
			resources.ApplyResources(this.isVisibleCheckBox, "isVisibleCheckBox");
			this.isVisibleCheckBox.BackgroundImage = null;
			this.isVisibleCheckBox.Font = null;
			this.isVisibleCheckBox.Name = "isVisibleCheckBox";
			this.isVisibleCheckBox.UseVisualStyleBackColor = true;
			// 
			// nameTextBox
			// 
			this.nameTextBox.AccessibleDescription = null;
			this.nameTextBox.AccessibleName = null;
			resources.ApplyResources(this.nameTextBox, "nameTextBox");
			this.nameTextBox.BackgroundImage = null;
			this.nameTextBox.Font = null;
			this.nameTextBox.Name = "nameTextBox";
			// 
			// label3
			// 
			this.label3.AccessibleDescription = null;
			this.label3.AccessibleName = null;
			resources.ApplyResources(this.label3, "label3");
			this.label3.Font = null;
			this.label3.Name = "label3";
			// 
			// label2
			// 
			this.label2.AccessibleDescription = null;
			this.label2.AccessibleName = null;
			resources.ApplyResources(this.label2, "label2");
			this.label2.Font = null;
			this.label2.Name = "label2";
			// 
			// alphaNumericUpDown
			// 
			this.alphaNumericUpDown.AccessibleDescription = null;
			this.alphaNumericUpDown.AccessibleName = null;
			resources.ApplyResources(this.alphaNumericUpDown, "alphaNumericUpDown");
			this.alphaNumericUpDown.Font = null;
			this.alphaNumericUpDown.Name = "alphaNumericUpDown";
			this.alphaNumericUpDown.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.alphaNumericUpDown.ValueChanged += new System.EventHandler(this.alphaNumericUpDown_ValueChanged);
			this.alphaNumericUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.alphaNumericUpDown_KeyUp);
			// 
			// label1
			// 
			this.label1.AccessibleDescription = null;
			this.label1.AccessibleName = null;
			resources.ApplyResources(this.label1, "label1");
			this.label1.Font = null;
			this.label1.Name = "label1";
			// 
			// alphaProgressBar
			// 
			this.alphaProgressBar.AccessibleDescription = null;
			this.alphaProgressBar.AccessibleName = null;
			resources.ApplyResources(this.alphaProgressBar, "alphaProgressBar");
			this.alphaProgressBar.BackgroundImage = null;
			this.alphaProgressBar.Font = null;
			this.alphaProgressBar.Name = "alphaProgressBar";
			this.alphaProgressBar.Step = 100;
			this.alphaProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.alphaProgressBar.Value = 50;
			this.alphaProgressBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.alphaProgressBar_MouseMove);
			this.alphaProgressBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.alphaProgressBar_MouseDown);
			this.alphaProgressBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.alphaProgressBar_MouseUp);
			// 
			// groupBox1
			// 
			this.groupBox1.AccessibleDescription = null;
			this.groupBox1.AccessibleName = null;
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.BackgroundImage = null;
			this.groupBox1.Controls.Add(this.cellSizeNumericUpDown);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Font = null;
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// cellSizeNumericUpDown
			// 
			this.cellSizeNumericUpDown.AccessibleDescription = null;
			this.cellSizeNumericUpDown.AccessibleName = null;
			resources.ApplyResources(this.cellSizeNumericUpDown, "cellSizeNumericUpDown");
			this.cellSizeNumericUpDown.Font = null;
			this.cellSizeNumericUpDown.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.cellSizeNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.cellSizeNumericUpDown.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
			this.cellSizeNumericUpDown.Name = "cellSizeNumericUpDown";
			this.cellSizeNumericUpDown.Value = new decimal(new int[] {
            32,
            0,
            0,
            0});
			// 
			// label4
			// 
			this.label4.AccessibleDescription = null;
			this.label4.AccessibleName = null;
			resources.ApplyResources(this.label4, "label4");
			this.label4.Font = null;
			this.label4.Name = "label4";
			// 
			// LayerAreaOptionForm
			// 
			this.AcceptButton = this.buttonOk;
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.CancelButton = this.buttonCancel;
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.groupBox3);
			this.Font = null;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LayerAreaOptionForm";
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.alphaNumericUpDown)).EndInit();
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.cellSizeNumericUpDown)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOk;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox isVisibleCheckBox;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ProgressBar alphaProgressBar;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown alphaNumericUpDown;
		private System.Windows.Forms.NumericUpDown cellSizeNumericUpDown;
		private System.Windows.Forms.Label label4;
	}
}