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
			this.MessageLabel = new System.Windows.Forms.Label();
			this.CloseButton = new System.Windows.Forms.Button();
			this.DetailButton = new System.Windows.Forms.Button();
			this.DetailsTextBox = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessageLabel
			// 
			this.MessageLabel.AccessibleDescription = null;
			this.MessageLabel.AccessibleName = null;
			resources.ApplyResources(this.MessageLabel, "MessageLabel");
			this.tableLayoutPanel1.SetColumnSpan(this.MessageLabel, 2);
			this.MessageLabel.Font = null;
			this.MessageLabel.Name = "MessageLabel";
			// 
			// CloseButton
			// 
			this.CloseButton.AccessibleDescription = null;
			this.CloseButton.AccessibleName = null;
			resources.ApplyResources(this.CloseButton, "CloseButton");
			this.CloseButton.BackgroundImage = null;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CloseButton.Font = null;
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// DetailButton
			// 
			this.DetailButton.AccessibleDescription = null;
			this.DetailButton.AccessibleName = null;
			resources.ApplyResources(this.DetailButton, "DetailButton");
			this.DetailButton.BackgroundImage = null;
			this.DetailButton.Font = null;
			this.DetailButton.Name = "DetailButton";
			this.DetailButton.UseVisualStyleBackColor = true;
			this.DetailButton.Click += new System.EventHandler(this.DetailButton_Click);
			// 
			// DetailsTextBox
			// 
			this.DetailsTextBox.AcceptsReturn = true;
			this.DetailsTextBox.AcceptsTab = true;
			this.DetailsTextBox.AccessibleDescription = null;
			this.DetailsTextBox.AccessibleName = null;
			resources.ApplyResources(this.DetailsTextBox, "DetailsTextBox");
			this.DetailsTextBox.BackgroundImage = null;
			this.tableLayoutPanel1.SetColumnSpan(this.DetailsTextBox, 2);
			this.DetailsTextBox.Font = null;
			this.DetailsTextBox.HideSelection = false;
			this.DetailsTextBox.Name = "DetailsTextBox";
			this.DetailsTextBox.ReadOnly = true;
			this.DetailsTextBox.TabStop = false;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AccessibleDescription = null;
			this.tableLayoutPanel1.AccessibleName = null;
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.BackgroundImage = null;
			this.tableLayoutPanel1.Controls.Add(this.CloseButton, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.DetailsTextBox, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.MessageLabel, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.DetailButton, 0, 2);
			this.tableLayoutPanel1.Font = null;
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// LoadErrorForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.CancelButton = this.CloseButton;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = null;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "LoadErrorForm";
			this.TopMost = true;
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Button DetailButton;
		private System.Windows.Forms.Label MessageLabel;
		private System.Windows.Forms.TextBox DetailsTextBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}