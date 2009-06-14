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
			this.tableLayoutPanel.SuspendLayout();
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
			this.tableLayoutPanel.SetColumnSpan(this.listView, 3);
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
			this.tableLayoutPanel.Controls.Add(this.buttonClose, 2, 1);
			this.tableLayoutPanel.Controls.Add(this.listView, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.buttonExport, 0, 1);
			this.tableLayoutPanel.Controls.Add(this.useGroupCheckBox, 1, 1);
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
			// PartListForm
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.tableLayoutPanel);
			this.Font = null;
			this.Name = "PartListForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PartListForm_FormClosing);
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
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
	}
}