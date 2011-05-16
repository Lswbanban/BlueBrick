namespace BlueBrick
{
	partial class FindForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FindPictureBox = new System.Windows.Forms.PictureBox();
            this.ReplacePictureBox = new System.Windows.Forms.PictureBox();
            this.ReplaceComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.FindComboBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.allLayerCheckBox = new System.Windows.Forms.CheckBox();
            this.LayerCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.inLayerRadioButton = new System.Windows.Forms.RadioButton();
            this.inCurrentSelectionRadioButton = new System.Windows.Forms.RadioButton();
            this.cancelButton = new System.Windows.Forms.Button();
            this.ReplaceButton = new System.Windows.Forms.Button();
            this.SelectAllButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FindPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ReplacePictureBox)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.FindPictureBox);
            this.groupBox1.Controls.Add(this.ReplacePictureBox);
            this.groupBox1.Controls.Add(this.ReplaceComboBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.FindComboBox);
            this.groupBox1.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // FindPictureBox
            // 
            resources.ApplyResources(this.FindPictureBox, "FindPictureBox");
            this.FindPictureBox.Name = "FindPictureBox";
            this.FindPictureBox.TabStop = false;
            // 
            // ReplacePictureBox
            // 
            resources.ApplyResources(this.ReplacePictureBox, "ReplacePictureBox");
            this.ReplacePictureBox.Name = "ReplacePictureBox";
            this.ReplacePictureBox.TabStop = false;
            // 
            // ReplaceComboBox
            // 
            this.ReplaceComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.ReplaceComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.ReplaceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ReplaceComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.ReplaceComboBox, "ReplaceComboBox");
            this.ReplaceComboBox.Name = "ReplaceComboBox";
            this.ReplaceComboBox.Sorted = true;
            this.ReplaceComboBox.SelectedIndexChanged += new System.EventHandler(this.ReplaceComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // FindComboBox
            // 
            this.FindComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.FindComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.FindComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FindComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.FindComboBox, "FindComboBox");
            this.FindComboBox.Name = "FindComboBox";
            this.FindComboBox.Sorted = true;
            this.FindComboBox.SelectedIndexChanged += new System.EventHandler(this.FindComboBox_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.allLayerCheckBox);
            this.groupBox2.Controls.Add(this.LayerCheckedListBox);
            this.groupBox2.Controls.Add(this.inLayerRadioButton);
            this.groupBox2.Controls.Add(this.inCurrentSelectionRadioButton);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // allLayerCheckBox
            // 
            resources.ApplyResources(this.allLayerCheckBox, "allLayerCheckBox");
            this.allLayerCheckBox.Name = "allLayerCheckBox";
            this.allLayerCheckBox.UseVisualStyleBackColor = true;
            this.allLayerCheckBox.CheckedChanged += new System.EventHandler(this.allLayerCheckBox_CheckedChanged);
            // 
            // LayerCheckedListBox
            // 
            this.LayerCheckedListBox.CheckOnClick = true;
            this.LayerCheckedListBox.FormattingEnabled = true;
            resources.ApplyResources(this.LayerCheckedListBox, "LayerCheckedListBox");
            this.LayerCheckedListBox.Name = "LayerCheckedListBox";
            this.LayerCheckedListBox.SelectedIndexChanged += new System.EventHandler(this.LayerCheckedListBox_SelectedIndexChanged);
            // 
            // inLayerRadioButton
            // 
            resources.ApplyResources(this.inLayerRadioButton, "inLayerRadioButton");
            this.inLayerRadioButton.Name = "inLayerRadioButton";
            this.inLayerRadioButton.TabStop = true;
            this.inLayerRadioButton.UseVisualStyleBackColor = true;
            this.inLayerRadioButton.CheckedChanged += new System.EventHandler(this.inLayerRadioButton_CheckedChanged);
            // 
            // inCurrentSelectionRadioButton
            // 
            resources.ApplyResources(this.inCurrentSelectionRadioButton, "inCurrentSelectionRadioButton");
            this.inCurrentSelectionRadioButton.Name = "inCurrentSelectionRadioButton";
            this.inCurrentSelectionRadioButton.TabStop = true;
            this.inCurrentSelectionRadioButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ReplaceButton
            // 
            resources.ApplyResources(this.ReplaceButton, "ReplaceButton");
            this.ReplaceButton.Name = "ReplaceButton";
            this.ReplaceButton.UseVisualStyleBackColor = true;
            this.ReplaceButton.Click += new System.EventHandler(this.ReplaceButton_Click);
            // 
            // SelectAllButton
            // 
            resources.ApplyResources(this.SelectAllButton, "SelectAllButton");
            this.SelectAllButton.Name = "SelectAllButton";
            this.SelectAllButton.UseVisualStyleBackColor = true;
            this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
            // 
            // FindForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SelectAllButton);
            this.Controls.Add(this.ReplaceButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "FindForm";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FindPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ReplacePictureBox)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton inLayerRadioButton;
		private System.Windows.Forms.RadioButton inCurrentSelectionRadioButton;
		private System.Windows.Forms.CheckedListBox LayerCheckedListBox;
		private System.Windows.Forms.CheckBox allLayerCheckBox;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button ReplaceButton;
		private System.Windows.Forms.Button SelectAllButton;
		private System.Windows.Forms.ComboBox FindComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox ReplacePictureBox;
		private System.Windows.Forms.ComboBox ReplaceComboBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.PictureBox FindPictureBox;
	}
}