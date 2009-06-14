namespace BlueBrick
{
	partial class LayerBrickOptionForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerBrickOptionForm));
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.isVisibleCheckBox = new System.Windows.Forms.CheckBox();
			this.nameTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOk = new System.Windows.Forms.Button();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox3
			// 
			this.groupBox3.AccessibleDescription = null;
			this.groupBox3.AccessibleName = null;
			resources.ApplyResources(this.groupBox3, "groupBox3");
			this.groupBox3.BackgroundImage = null;
			this.groupBox3.Controls.Add(this.isVisibleCheckBox);
			this.groupBox3.Controls.Add(this.nameTextBox);
			this.groupBox3.Controls.Add(this.label2);
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
			// label2
			// 
			this.label2.AccessibleDescription = null;
			this.label2.AccessibleName = null;
			resources.ApplyResources(this.label2, "label2");
			this.label2.Font = null;
			this.label2.Name = "label2";
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
			// LayerBrickOptionForm
			// 
			this.AcceptButton = this.buttonOk;
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.CancelButton = this.buttonCancel;
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOk);
			this.Controls.Add(this.groupBox3);
			this.Font = null;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LayerBrickOptionForm";
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.CheckBox isVisibleCheckBox;
		private System.Windows.Forms.TextBox nameTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOk;
	}
}