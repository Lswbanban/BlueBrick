namespace BlueBrick
{
	partial class SaveGroupNameForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveGroupNameForm));
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.nameErrorLabel = new System.Windows.Forms.Label();
			this.sortingKeyTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.authorTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.canUngroupCheckBox = new System.Windows.Forms.CheckBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.languageNameLabel = new System.Windows.Forms.Label();
			this.descriptionTextBox = new System.Windows.Forms.TextBox();
			this.languageCodeComboBox = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
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
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// nameTextBox
			// 
			this.nameTextBox.BackColor = System.Drawing.Color.LightSalmon;
			resources.ApplyResources(this.nameTextBox, "nameTextBox");
			this.nameTextBox.Name = "nameTextBox";
			this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.nameErrorLabel);
			this.groupBox1.Controls.Add(this.sortingKeyTextBox);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.authorTextBox);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.canUngroupCheckBox);
			this.groupBox1.Controls.Add(this.nameTextBox);
			this.groupBox1.Controls.Add(this.label1);
			resources.ApplyResources(this.groupBox1, "groupBox1");
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabStop = false;
			// 
			// nameErrorLabel
			// 
			this.nameErrorLabel.ForeColor = System.Drawing.Color.DarkRed;
			resources.ApplyResources(this.nameErrorLabel, "nameErrorLabel");
			this.nameErrorLabel.Name = "nameErrorLabel";
			// 
			// sortingKeyTextBox
			// 
			resources.ApplyResources(this.sortingKeyTextBox, "sortingKeyTextBox");
			this.sortingKeyTextBox.Name = "sortingKeyTextBox";
			// 
			// label4
			// 
			resources.ApplyResources(this.label4, "label4");
			this.label4.Name = "label4";
			// 
			// authorTextBox
			// 
			resources.ApplyResources(this.authorTextBox, "authorTextBox");
			this.authorTextBox.Name = "authorTextBox";
			// 
			// label2
			// 
			resources.ApplyResources(this.label2, "label2");
			this.label2.Name = "label2";
			// 
			// canUngroupCheckBox
			// 
			resources.ApplyResources(this.canUngroupCheckBox, "canUngroupCheckBox");
			this.canUngroupCheckBox.Checked = true;
			this.canUngroupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.canUngroupCheckBox.Name = "canUngroupCheckBox";
			this.canUngroupCheckBox.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.languageNameLabel);
			this.groupBox2.Controls.Add(this.descriptionTextBox);
			this.groupBox2.Controls.Add(this.languageCodeComboBox);
			this.groupBox2.Controls.Add(this.label3);
			resources.ApplyResources(this.groupBox2, "groupBox2");
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.TabStop = false;
			// 
			// languageNameLabel
			// 
			resources.ApplyResources(this.languageNameLabel, "languageNameLabel");
			this.languageNameLabel.Name = "languageNameLabel";
			// 
			// descriptionTextBox
			// 
			resources.ApplyResources(this.descriptionTextBox, "descriptionTextBox");
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Enter += new System.EventHandler(this.descriptionTextBox_Enter);
			this.descriptionTextBox.Leave += new System.EventHandler(this.descriptionTextBox_Leave);
			// 
			// languageCodeComboBox
			// 
			this.languageCodeComboBox.FormattingEnabled = true;
			resources.ApplyResources(this.languageCodeComboBox, "languageCodeComboBox");
			this.languageCodeComboBox.Name = "languageCodeComboBox";
			this.languageCodeComboBox.SelectedIndexChanged += new System.EventHandler(this.languageCodeComboBox_SelectedIndexChanged);
			this.languageCodeComboBox.TextChanged += new System.EventHandler(this.languageCodeComboBox_TextChanged);
			this.languageCodeComboBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.languageCodeComboBox_PreviewKeyDown);
			// 
			// label3
			// 
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// SaveGroupNameForm
			// 
			this.AcceptButton = this.okButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
			this.CancelButton = this.cancelButton;
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SaveGroupNameForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SaveGroupNameForm_FormClosing);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox authorTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.CheckBox canUngroupCheckBox;
		private System.Windows.Forms.TextBox sortingKeyTextBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox descriptionTextBox;
		private System.Windows.Forms.ComboBox languageCodeComboBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label languageNameLabel;
		private System.Windows.Forms.Label nameErrorLabel;
	}
}