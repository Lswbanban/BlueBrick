namespace BlueBrick
{
	partial class DownloadCenterForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DownloadCenterForm));
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.DownloadListView = new System.Windows.Forms.ListView();
			this.FileColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.DestinationColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.SourceColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.PercentColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.StartButton = new System.Windows.Forms.Button();
			this.TotalProgressBar = new System.Windows.Forms.ProgressBar();
			this.panel1 = new System.Windows.Forms.Panel();
			this.CloseButton = new System.Windows.Forms.Button();
			this.CancelButton = new System.Windows.Forms.Button();
			this.downloadBackgroundWorker = new System.ComponentModel.BackgroundWorker();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AccessibleDescription = null;
			this.tableLayoutPanel1.AccessibleName = null;
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.BackgroundImage = null;
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.DownloadListView, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.StartButton, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.TotalProgressBar, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
			this.tableLayoutPanel1.Font = null;
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// label1
			// 
			this.label1.AccessibleDescription = null;
			this.label1.AccessibleName = null;
			resources.ApplyResources(this.label1, "label1");
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 3);
			this.label1.Font = null;
			this.label1.Name = "label1";
			// 
			// DownloadListView
			// 
			this.DownloadListView.AccessibleDescription = null;
			this.DownloadListView.AccessibleName = null;
			resources.ApplyResources(this.DownloadListView, "DownloadListView");
			this.DownloadListView.AllowColumnReorder = true;
			this.DownloadListView.AutoArrange = false;
			this.DownloadListView.BackgroundImage = null;
			this.DownloadListView.CheckBoxes = true;
			this.DownloadListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.FileColumnHeader,
            this.DestinationColumnHeader,
            this.SourceColumnHeader,
            this.PercentColumnHeader});
			this.tableLayoutPanel1.SetColumnSpan(this.DownloadListView, 3);
			this.DownloadListView.FullRowSelect = true;
			this.DownloadListView.LabelEdit = true;
			this.DownloadListView.Name = "DownloadListView";
			this.DownloadListView.UseCompatibleStateImageBehavior = false;
			this.DownloadListView.View = System.Windows.Forms.View.Details;
			this.DownloadListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.DownloadListView_AfterLabelEdit);
			// 
			// FileColumnHeader
			// 
			resources.ApplyResources(this.FileColumnHeader, "FileColumnHeader");
			// 
			// DestinationColumnHeader
			// 
			resources.ApplyResources(this.DestinationColumnHeader, "DestinationColumnHeader");
			// 
			// SourceColumnHeader
			// 
			resources.ApplyResources(this.SourceColumnHeader, "SourceColumnHeader");
			// 
			// PercentColumnHeader
			// 
			resources.ApplyResources(this.PercentColumnHeader, "PercentColumnHeader");
			// 
			// StartButton
			// 
			this.StartButton.AccessibleDescription = null;
			this.StartButton.AccessibleName = null;
			resources.ApplyResources(this.StartButton, "StartButton");
			this.StartButton.BackgroundImage = null;
			this.StartButton.Font = null;
			this.StartButton.Name = "StartButton";
			this.StartButton.UseVisualStyleBackColor = true;
			this.StartButton.Click += new System.EventHandler(this.StartStopButton_Click);
			// 
			// TotalProgressBar
			// 
			this.TotalProgressBar.AccessibleDescription = null;
			this.TotalProgressBar.AccessibleName = null;
			resources.ApplyResources(this.TotalProgressBar, "TotalProgressBar");
			this.TotalProgressBar.BackgroundImage = null;
			this.TotalProgressBar.Font = null;
			this.TotalProgressBar.Name = "TotalProgressBar";
			this.TotalProgressBar.Step = 1;
			// 
			// panel1
			// 
			this.panel1.AccessibleDescription = null;
			this.panel1.AccessibleName = null;
			resources.ApplyResources(this.panel1, "panel1");
			this.panel1.BackgroundImage = null;
			this.panel1.Controls.Add(this.CloseButton);
			this.panel1.Controls.Add(this.CancelButton);
			this.panel1.Font = null;
			this.panel1.Name = "panel1";
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
			// 
			// CancelButton
			// 
			this.CancelButton.AccessibleDescription = null;
			this.CancelButton.AccessibleName = null;
			resources.ApplyResources(this.CancelButton, "CancelButton");
			this.CancelButton.BackgroundImage = null;
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Font = null;
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.UseVisualStyleBackColor = true;
			// 
			// downloadBackgroundWorker
			// 
			this.downloadBackgroundWorker.WorkerReportsProgress = true;
			this.downloadBackgroundWorker.WorkerSupportsCancellation = true;
			this.downloadBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.downloadBackgroundWorker_DoWork);
			this.downloadBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.downloadBackgroundWorker_RunWorkerCompleted);
			this.downloadBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.downloadBackgroundWorker_ProgressChanged);
			// 
			// DownloadCenterForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AccessibleDescription = null;
			this.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = null;
			this.Name = "DownloadCenterForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DownloadCenterForm_FormClosing);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView DownloadListView;
		private System.Windows.Forms.Button StartButton;
		private System.Windows.Forms.ProgressBar TotalProgressBar;
		private System.Windows.Forms.ColumnHeader SourceColumnHeader;
		private System.Windows.Forms.ColumnHeader DestinationColumnHeader;
		private System.Windows.Forms.ColumnHeader PercentColumnHeader;
		private System.ComponentModel.BackgroundWorker downloadBackgroundWorker;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button CloseButton;
		private System.Windows.Forms.Button CancelButton;
		private System.Windows.Forms.ColumnHeader FileColumnHeader;
	}
}