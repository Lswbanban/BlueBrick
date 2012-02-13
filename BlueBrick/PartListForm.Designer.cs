namespace BlueBrick
{
	partial class PartListForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PartListForm));
			this.listView = new System.Windows.Forms.ListView();
			this.partColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.quantiyColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.colorColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.buttonClose = new System.Windows.Forms.Button();
			this.buttonExport = new System.Windows.Forms.Button();
			this.useGroupCheckBox = new System.Windows.Forms.CheckBox();
			this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanel.SuspendLayout();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.AccessibleDescription = null;
			this.listView.AccessibleName = null;
			resources.ApplyResources(this.listView, "listView");
			this.listView.AllowColumnReorder = true;
			this.listView.BackgroundImage = null;
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.partColumnHeader,
            this.quantiyColumnHeader,
            this.colorColumnHeader,
            this.descriptionColumnHeader});
			this.listView.Font = null;
			this.listView.FullRowSelect = true;
			this.listView.Name = "listView";
			this.listView.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			this.listView.VisibleChanged += new System.EventHandler(this.listView_VisibleChanged);
			// 
			// partColumnHeader
			// 
			resources.ApplyResources(this.partColumnHeader, "partColumnHeader");
			// 
			// quantiyColumnHeader
			// 
			resources.ApplyResources(this.quantiyColumnHeader, "quantiyColumnHeader");
			// 
			// colorColumnHeader
			// 
			resources.ApplyResources(this.colorColumnHeader, "colorColumnHeader");
			// 
			// descriptionColumnHeader
			// 
			resources.ApplyResources(this.descriptionColumnHeader, "descriptionColumnHeader");
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.AccessibleDescription = null;
			this.tableLayoutPanel.AccessibleName = null;
			resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
			this.tableLayoutPanel.BackgroundImage = null;
			this.tableLayoutPanel.Controls.Add(this.buttonClose, 2, 0);
			this.tableLayoutPanel.Controls.Add(this.buttonExport, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.useGroupCheckBox, 1, 0);
			this.tableLayoutPanel.Font = null;
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			// 
			// buttonClose
			// 
			this.buttonClose.AccessibleDescription = null;
			this.buttonClose.AccessibleName = null;
			resources.ApplyResources(this.buttonClose, "buttonClose");
			this.buttonClose.BackgroundImage = null;
			this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonClose.Font = null;
			this.buttonClose.Name = "buttonClose";
			this.buttonClose.UseVisualStyleBackColor = true;
			this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
			// 
			// buttonExport
			// 
			this.buttonExport.AccessibleDescription = null;
			this.buttonExport.AccessibleName = null;
			resources.ApplyResources(this.buttonExport, "buttonExport");
			this.buttonExport.BackgroundImage = null;
			this.buttonExport.Font = null;
			this.buttonExport.Name = "buttonExport";
			this.buttonExport.UseVisualStyleBackColor = true;
			this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
			// 
			// useGroupCheckBox
			// 
			this.useGroupCheckBox.AccessibleDescription = null;
			this.useGroupCheckBox.AccessibleName = null;
			resources.ApplyResources(this.useGroupCheckBox, "useGroupCheckBox");
			this.useGroupCheckBox.BackgroundImage = null;
			this.useGroupCheckBox.Checked = true;
			this.useGroupCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.useGroupCheckBox.Font = null;
			this.useGroupCheckBox.Name = "useGroupCheckBox";
			this.useGroupCheckBox.UseVisualStyleBackColor = true;
			this.useGroupCheckBox.CheckedChanged += new System.EventHandler(this.useGroupCheckBox_CheckedChanged);
			// 
			// saveFileDialog
			// 
			this.saveFileDialog.DefaultExt = "txt";
			resources.ApplyResources(this.saveFileDialog, "saveFileDialog");
			this.saveFileDialog.SupportMultiDottedExtensions = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.AccessibleDescription = null;
			this.splitContainer1.AccessibleName = null;
			resources.ApplyResources(this.splitContainer1, "splitContainer1");
			this.splitContainer1.BackgroundImage = null;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Font = null;
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.AccessibleDescription = null;
			this.splitContainer1.Panel1.AccessibleName = null;
			resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
			this.splitContainer1.Panel1.BackgroundImage = null;
			this.splitContainer1.Panel1.Controls.Add(this.listView);
			this.splitContainer1.Panel1.Font = null;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.AccessibleDescription = null;
			this.splitContainer1.Panel2.AccessibleName = null;
			resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
			this.splitContainer1.Panel2.BackgroundImage = null;
			this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel);
			this.splitContainer1.Panel2.Font = null;
			// 
			// PartListForm
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.splitContainer1);
			this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::BlueBrick.Properties.Settings.Default, "UIPartListFormLocation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
			this.Font = null;
			this.Location = global::BlueBrick.Properties.Settings.Default.UIPartListFormLocation;
			this.Name = "PartListForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PartListForm_FormClosing);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.ColumnHeader partColumnHeader;
		private System.Windows.Forms.ColumnHeader descriptionColumnHeader;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private System.Windows.Forms.Button buttonExport;
		private System.Windows.Forms.ColumnHeader quantiyColumnHeader;
		private System.Windows.Forms.ColumnHeader colorColumnHeader;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.CheckBox useGroupCheckBox;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.SplitContainer splitContainer1;
	}
}