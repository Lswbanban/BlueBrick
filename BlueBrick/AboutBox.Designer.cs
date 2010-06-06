namespace BlueBrick
{
	partial class AboutBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.logoPictureBox = new System.Windows.Forms.PictureBox();
			this.labelProductName = new System.Windows.Forms.Label();
			this.labelVersion = new System.Windows.Forms.Label();
			this.labelCopyright = new System.Windows.Forms.Label();
			this.labelWebSiteName = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.okButton = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.translatorListView = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.label3 = new System.Windows.Forms.Label();
			this.tableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.AccessibleDescription = null;
			this.tableLayoutPanel.AccessibleName = null;
			resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
			this.tableLayoutPanel.BackgroundImage = null;
			this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.labelProductName, 1, 0);
			this.tableLayoutPanel.Controls.Add(this.labelVersion, 1, 1);
			this.tableLayoutPanel.Controls.Add(this.labelCopyright, 1, 2);
			this.tableLayoutPanel.Controls.Add(this.labelWebSiteName, 1, 3);
			this.tableLayoutPanel.Controls.Add(this.label1, 1, 4);
			this.tableLayoutPanel.Controls.Add(this.okButton, 0, 8);
			this.tableLayoutPanel.Controls.Add(this.label2, 0, 6);
			this.tableLayoutPanel.Controls.Add(this.translatorListView, 0, 7);
			this.tableLayoutPanel.Controls.Add(this.label3, 1, 5);
			this.tableLayoutPanel.Font = null;
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			// 
			// logoPictureBox
			// 
			this.logoPictureBox.AccessibleDescription = null;
			this.logoPictureBox.AccessibleName = null;
			resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
			this.logoPictureBox.BackgroundImage = null;
			this.logoPictureBox.Font = null;
			this.logoPictureBox.ImageLocation = null;
			this.logoPictureBox.Name = "logoPictureBox";
			this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 6);
			this.logoPictureBox.TabStop = false;
			// 
			// labelProductName
			// 
			this.labelProductName.AccessibleDescription = null;
			this.labelProductName.AccessibleName = null;
			resources.ApplyResources(this.labelProductName, "labelProductName");
			this.labelProductName.MaximumSize = new System.Drawing.Size(0, 30);
			this.labelProductName.Name = "labelProductName";
			// 
			// labelVersion
			// 
			this.labelVersion.AccessibleDescription = null;
			this.labelVersion.AccessibleName = null;
			resources.ApplyResources(this.labelVersion, "labelVersion");
			this.labelVersion.Font = null;
			this.labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelVersion.Name = "labelVersion";
			// 
			// labelCopyright
			// 
			this.labelCopyright.AccessibleDescription = null;
			this.labelCopyright.AccessibleName = null;
			resources.ApplyResources(this.labelCopyright, "labelCopyright");
			this.labelCopyright.Font = null;
			this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelCopyright.Name = "labelCopyright";
			// 
			// labelWebSiteName
			// 
			this.labelWebSiteName.AccessibleDescription = null;
			this.labelWebSiteName.AccessibleName = null;
			resources.ApplyResources(this.labelWebSiteName, "labelWebSiteName");
			this.labelWebSiteName.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelWebSiteName.Name = "labelWebSiteName";
			// 
			// label1
			// 
			this.label1.AccessibleDescription = null;
			this.label1.AccessibleName = null;
			resources.ApplyResources(this.label1, "label1");
			this.label1.Font = null;
			this.label1.Name = "label1";
			// 
			// okButton
			// 
			this.okButton.AccessibleDescription = null;
			this.okButton.AccessibleName = null;
			resources.ApplyResources(this.okButton, "okButton");
			this.okButton.BackgroundImage = null;
			this.tableLayoutPanel.SetColumnSpan(this.okButton, 2);
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.okButton.Font = null;
			this.okButton.Name = "okButton";
			// 
			// label2
			// 
			this.label2.AccessibleDescription = null;
			this.label2.AccessibleName = null;
			resources.ApplyResources(this.label2, "label2");
			this.tableLayoutPanel.SetColumnSpan(this.label2, 2);
			this.label2.Font = null;
			this.label2.Name = "label2";
			// 
			// translatorListView
			// 
			this.translatorListView.AccessibleDescription = null;
			this.translatorListView.AccessibleName = null;
			resources.ApplyResources(this.translatorListView, "translatorListView");
			this.translatorListView.AutoArrange = false;
			this.translatorListView.BackColor = System.Drawing.SystemColors.Control;
			this.translatorListView.BackgroundImage = null;
			this.translatorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
			this.tableLayoutPanel.SetColumnSpan(this.translatorListView, 2);
			this.translatorListView.Font = null;
			this.translatorListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.translatorListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("translatorListView.Items"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("translatorListView.Items1"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("translatorListView.Items2"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("translatorListView.Items3"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("translatorListView.Items4"))),
            ((System.Windows.Forms.ListViewItem)(resources.GetObject("translatorListView.Items5")))});
			this.translatorListView.MultiSelect = false;
			this.translatorListView.Name = "translatorListView";
			this.translatorListView.Scrollable = false;
			this.translatorListView.ShowGroups = false;
			this.translatorListView.UseCompatibleStateImageBehavior = false;
			this.translatorListView.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			resources.ApplyResources(this.columnHeader1, "columnHeader1");
			// 
			// columnHeader2
			// 
			resources.ApplyResources(this.columnHeader2, "columnHeader2");
			// 
			// label3
			// 
			this.label3.AccessibleDescription = null;
			this.label3.AccessibleName = null;
			resources.ApplyResources(this.label3, "label3");
			this.label3.Name = "label3";
			// 
			// AboutBox
			// 
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.tableLayoutPanel);
			this.Font = null;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = null;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private System.Windows.Forms.PictureBox logoPictureBox;
		private System.Windows.Forms.Label labelProductName;
		private System.Windows.Forms.Label labelVersion;
		private System.Windows.Forms.Label labelCopyright;
		private System.Windows.Forms.Label labelWebSiteName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ListView translatorListView;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Label label3;
	}
}
