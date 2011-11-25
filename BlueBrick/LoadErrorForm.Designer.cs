namespace BlueBrick
{
	partial class LoadErrorForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadErrorForm));
			this.CloseButton = new System.Windows.Forms.Button();
			this.DetailButton = new System.Windows.Forms.Button();
			this.DetailsTextBox = new System.Windows.Forms.TextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.MessageTextBox = new System.Windows.Forms.TextBox();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// CloseButton
			// 
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(this.CloseButton, "CloseButton");
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// DetailButton
			// 
			resources.ApplyResources(this.DetailButton, "DetailButton");
			this.DetailButton.Name = "DetailButton";
			this.DetailButton.UseVisualStyleBackColor = true;
			this.DetailButton.Click += new System.EventHandler(this.DetailButton_Click);
			// 
			// DetailsTextBox
			// 
			this.DetailsTextBox.AcceptsReturn = true;
			this.DetailsTextBox.AcceptsTab = true;
			this.DetailsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.DetailsTextBox, "DetailsTextBox");
			this.DetailsTextBox.HideSelection = false;
			this.DetailsTextBox.Name = "DetailsTextBox";
			this.DetailsTextBox.ReadOnly = true;
			this.DetailsTextBox.TabStop = false;
			// 
			// splitContainer1
			// 
			resources.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.CloseButton);
			this.splitContainer1.Panel2.Controls.Add(this.DetailButton);
			resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
			// 
			// splitContainer2
			// 
			resources.ApplyResources(this.splitContainer2, "splitContainer2");
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.MessageTextBox);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.DetailsTextBox);
			// 
			// MessageTextBox
			// 
			this.MessageTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.MessageTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(this.MessageTextBox, "MessageTextBox");
			this.MessageTextBox.Name = "MessageTextBox";
			this.MessageTextBox.ReadOnly = true;
			// 
			// LoadErrorForm
			// 
			this.AcceptButton = this.CloseButton;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CloseButton;
			this.Controls.Add(this.splitContainer1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoadErrorForm";
			this.TopMost = true;
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel1.PerformLayout();
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.Panel2.PerformLayout();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Button DetailButton;
		private System.Windows.Forms.TextBox DetailsTextBox;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.TextBox MessageTextBox;
	}
}