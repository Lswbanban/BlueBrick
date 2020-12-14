namespace BlueBrick
{
	partial class EditTextForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditTextForm));
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.panel1 = new System.Windows.Forms.Panel();
			this.fontButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.labelSize = new System.Windows.Forms.Label();
			this.alignLeftButton = new System.Windows.Forms.Button();
			this.fontColorPictureBox = new System.Windows.Forms.PictureBox();
			this.alignRightButton = new System.Windows.Forms.Button();
			this.alignCenterButton = new System.Windows.Forms.Button();
			this.fontColorLabel = new System.Windows.Forms.Label();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.textBox = new System.Windows.Forms.TextBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.fontDialog = new System.Windows.Forms.FontDialog();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			resources.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
			this.splitContainer1.Panel1.Controls.Add(this.panel1);
			// 
			// splitContainer1.Panel2
			// 
			resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.TabStop = false;
			// 
			// panel1
			// 
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.Controls.Add(this.fontButton);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.labelSize);
			this.panel1.Controls.Add(this.alignLeftButton);
			this.panel1.Controls.Add(this.fontColorPictureBox);
			this.panel1.Controls.Add(this.alignRightButton);
			this.panel1.Controls.Add(this.alignCenterButton);
			this.panel1.Controls.Add(this.fontColorLabel);
			this.panel1.Name = "panel1";
			// 
			// fontButton
			// 
			resources.ApplyResources(this.fontButton, "fontButton");
			this.fontButton.Name = "fontButton";
			this.fontButton.UseVisualStyleBackColor = true;
			this.fontButton.Click += new System.EventHandler(this.fontButton_Click);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// labelSize
			// 
			resources.ApplyResources(this.labelSize, "labelSize");
			this.labelSize.Name = "labelSize";
			// 
			// alignLeftButton
			// 
			resources.ApplyResources(this.alignLeftButton, "alignLeftButton");
			this.alignLeftButton.Image = global::BlueBrick.Properties.Resources.alignleft;
			this.alignLeftButton.Name = "alignLeftButton";
			this.alignLeftButton.UseVisualStyleBackColor = false;
			this.alignLeftButton.Click += new System.EventHandler(this.alignLeftButton_Click);
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
			// alignRightButton
			// 
			resources.ApplyResources(this.alignRightButton, "alignRightButton");
			this.alignRightButton.Image = global::BlueBrick.Properties.Resources.alignright;
			this.alignRightButton.Name = "alignRightButton";
			this.alignRightButton.UseVisualStyleBackColor = false;
			this.alignRightButton.Click += new System.EventHandler(this.alignRightButton_Click);
			// 
			// alignCenterButton
			// 
			resources.ApplyResources(this.alignCenterButton, "alignCenterButton");
			this.alignCenterButton.BackColor = System.Drawing.SystemColors.Control;
			this.alignCenterButton.Image = global::BlueBrick.Properties.Resources.aligncentered;
			this.alignCenterButton.Name = "alignCenterButton";
			this.alignCenterButton.UseVisualStyleBackColor = false;
			this.alignCenterButton.Click += new System.EventHandler(this.alignCenterButton_Click);
			// 
			// fontColorLabel
			// 
			resources.ApplyResources(this.fontColorLabel, "fontColorLabel");
			this.fontColorLabel.Name = "fontColorLabel";
			// 
			// splitContainer2
			// 
			resources.ApplyResources(this.splitContainer2, "splitContainer2");
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			resources.ApplyResources(this.splitContainer2.Panel1, "splitContainer2.Panel1");
			this.splitContainer2.Panel1.Controls.Add(this.textBox);
			// 
			// splitContainer2.Panel2
			// 
			resources.ApplyResources(this.splitContainer2.Panel2, "splitContainer2.Panel2");
			this.splitContainer2.Panel2.Controls.Add(this.panel2);
			this.splitContainer2.TabStop = false;
			// 
			// textBox
			// 
			this.textBox.AcceptsReturn = true;
			resources.ApplyResources(this.textBox, "textBox");
			this.textBox.Name = "textBox";
			this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
			// 
			// panel2
			// 
			resources.ApplyResources(this.panel2, "panel2");
			this.panel2.Controls.Add(this.cancelButton);
			this.panel2.Controls.Add(this.okButton);
			this.panel2.Name = "panel2";
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
			// 
			// EditTextForm
			// 
			this.AcceptButton = this.okButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.Controls.Add(this.splitContainer1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditTextForm";
			this.Shown += new System.EventHandler(this.EditTextForm_Shown);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).EndInit();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox textBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label fontColorLabel;
		private System.Windows.Forms.PictureBox fontColorPictureBox;
		private System.Windows.Forms.Button alignLeftButton;
		private System.Windows.Forms.Button alignCenterButton;
		private System.Windows.Forms.Button alignRightButton;
		private System.Windows.Forms.FontDialog fontDialog;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelSize;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button fontButton;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
	}
}