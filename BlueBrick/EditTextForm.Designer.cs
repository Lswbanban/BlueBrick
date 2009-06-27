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
			this.textBox = new System.Windows.Forms.TextBox();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.fontColorLabel = new System.Windows.Forms.Label();
			this.fontColorPictureBox = new System.Windows.Forms.PictureBox();
			this.fontButton = new System.Windows.Forms.Button();
			this.alignLeftButton = new System.Windows.Forms.Button();
			this.alignCenterButton = new System.Windows.Forms.Button();
			this.alignRightButton = new System.Windows.Forms.Button();
			this.fontDialog = new System.Windows.Forms.FontDialog();
			this.colorDialog = new System.Windows.Forms.ColorDialog();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.labelSize = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).BeginInit();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox
			// 
			this.textBox.AcceptsReturn = true;
			this.textBox.AccessibleDescription = null;
			this.textBox.AccessibleName = null;
			resources.ApplyResources(this.textBox, "textBox");
			this.textBox.BackgroundImage = null;
			this.tableLayoutPanel1.SetColumnSpan(this.textBox, 3);
			this.textBox.Font = null;
			this.textBox.Name = "textBox";
			this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
			// 
			// cancelButton
			// 
			this.cancelButton.AccessibleDescription = null;
			this.cancelButton.AccessibleName = null;
			resources.ApplyResources(this.cancelButton, "cancelButton");
			this.cancelButton.BackgroundImage = null;
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Font = null;
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.AccessibleDescription = null;
			this.okButton.AccessibleName = null;
			resources.ApplyResources(this.okButton, "okButton");
			this.okButton.BackgroundImage = null;
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Font = null;
			this.okButton.Name = "okButton";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// fontColorLabel
			// 
			this.fontColorLabel.AccessibleDescription = null;
			this.fontColorLabel.AccessibleName = null;
			resources.ApplyResources(this.fontColorLabel, "fontColorLabel");
			this.fontColorLabel.Font = null;
			this.fontColorLabel.Name = "fontColorLabel";
			// 
			// fontColorPictureBox
			// 
			this.fontColorPictureBox.AccessibleDescription = null;
			this.fontColorPictureBox.AccessibleName = null;
			resources.ApplyResources(this.fontColorPictureBox, "fontColorPictureBox");
			this.fontColorPictureBox.BackColor = System.Drawing.Color.White;
			this.fontColorPictureBox.BackgroundImage = null;
			this.fontColorPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.fontColorPictureBox.Font = null;
			this.fontColorPictureBox.ImageLocation = null;
			this.fontColorPictureBox.Name = "fontColorPictureBox";
			this.fontColorPictureBox.TabStop = false;
			this.fontColorPictureBox.Click += new System.EventHandler(this.fontColorPictureBox_Click);
			// 
			// fontButton
			// 
			this.fontButton.AccessibleDescription = null;
			this.fontButton.AccessibleName = null;
			resources.ApplyResources(this.fontButton, "fontButton");
			this.fontButton.BackgroundImage = null;
			this.fontButton.Font = null;
			this.fontButton.Name = "fontButton";
			this.fontButton.UseVisualStyleBackColor = true;
			this.fontButton.Click += new System.EventHandler(this.fontButton_Click);
			// 
			// alignLeftButton
			// 
			this.alignLeftButton.AccessibleDescription = null;
			this.alignLeftButton.AccessibleName = null;
			resources.ApplyResources(this.alignLeftButton, "alignLeftButton");
			this.alignLeftButton.BackgroundImage = null;
			this.alignLeftButton.Font = null;
			this.alignLeftButton.Image = global::BlueBrick.Properties.Resources.alignleft;
			this.alignLeftButton.Name = "alignLeftButton";
			this.alignLeftButton.UseVisualStyleBackColor = false;
			this.alignLeftButton.Click += new System.EventHandler(this.alignLeftButton_Click);
			// 
			// alignCenterButton
			// 
			this.alignCenterButton.AccessibleDescription = null;
			this.alignCenterButton.AccessibleName = null;
			resources.ApplyResources(this.alignCenterButton, "alignCenterButton");
			this.alignCenterButton.BackColor = System.Drawing.SystemColors.Control;
			this.alignCenterButton.BackgroundImage = null;
			this.alignCenterButton.Font = null;
			this.alignCenterButton.Image = global::BlueBrick.Properties.Resources.aligncentered;
			this.alignCenterButton.Name = "alignCenterButton";
			this.alignCenterButton.UseVisualStyleBackColor = false;
			this.alignCenterButton.Click += new System.EventHandler(this.alignCenterButton_Click);
			// 
			// alignRightButton
			// 
			this.alignRightButton.AccessibleDescription = null;
			this.alignRightButton.AccessibleName = null;
			resources.ApplyResources(this.alignRightButton, "alignRightButton");
			this.alignRightButton.BackgroundImage = null;
			this.alignRightButton.Font = null;
			this.alignRightButton.Image = global::BlueBrick.Properties.Resources.alignright;
			this.alignRightButton.Name = "alignRightButton";
			this.alignRightButton.UseVisualStyleBackColor = false;
			this.alignRightButton.Click += new System.EventHandler(this.alignRightButton_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AccessibleDescription = null;
			this.tableLayoutPanel1.AccessibleName = null;
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.BackgroundImage = null;
			this.tableLayoutPanel1.Controls.Add(this.okButton, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.cancelButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.textBox, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.fontButton, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
			this.tableLayoutPanel1.Font = null;
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// panel1
			// 
			this.panel1.AccessibleDescription = null;
			this.panel1.AccessibleName = null;
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.BackgroundImage = null;
			this.tableLayoutPanel1.SetColumnSpan(this.panel1, 2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.labelSize);
			this.panel1.Controls.Add(this.alignLeftButton);
			this.panel1.Controls.Add(this.fontColorPictureBox);
			this.panel1.Controls.Add(this.alignRightButton);
			this.panel1.Controls.Add(this.alignCenterButton);
			this.panel1.Controls.Add(this.fontColorLabel);
			this.panel1.Font = null;
			this.panel1.Name = "panel1";
			// 
			// label1
			// 
			this.label1.AccessibleDescription = null;
			this.label1.AccessibleName = null;
			resources.ApplyResources(this.label1, "label1");
			this.label1.Font = null;
			this.label1.Name = "label1";
			// 
			// labelSize
			// 
			this.labelSize.AccessibleDescription = null;
			this.labelSize.AccessibleName = null;
			resources.ApplyResources(this.labelSize, "labelSize");
			this.labelSize.Font = null;
			this.labelSize.Name = "labelSize";
			// 
			// EditTextForm
			// 
			this.AcceptButton = this.okButton;
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.CancelButton = this.cancelButton;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = null;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditTextForm";
			((System.ComponentModel.ISupportInitialize)(this.fontColorPictureBox)).EndInit();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TextBox textBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label fontColorLabel;
		private System.Windows.Forms.PictureBox fontColorPictureBox;
		private System.Windows.Forms.Button fontButton;
		private System.Windows.Forms.Button alignLeftButton;
		private System.Windows.Forms.Button alignCenterButton;
		private System.Windows.Forms.Button alignRightButton;
		private System.Windows.Forms.FontDialog fontDialog;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label labelSize;
		private System.Windows.Forms.Label label1;
	}
}