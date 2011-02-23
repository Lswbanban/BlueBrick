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
			this.CancelButton = new System.Windows.Forms.Button();
			this.StartStopButton = new System.Windows.Forms.Button();
			this.TotalProgressBar = new System.Windows.Forms.ProgressBar();
			this.SourceColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.DestinationColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.PercentColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.downloadBackgroundWorker = new System.ComponentModel.BackgroundWorker();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.DownloadListView, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.CancelButton, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.StartStopButton, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.TotalProgressBar, 2, 2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.tableLayoutPanel1.SetColumnSpan(this.label1, 3);
			this.label1.Name = "label1";
			// 
			// DownloadListView
			// 
			this.DownloadListView.AutoArrange = false;
			this.DownloadListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.SourceColumnHeader,
            this.DestinationColumnHeader,
            this.PercentColumnHeader});
			this.tableLayoutPanel1.SetColumnSpan(this.DownloadListView, 3);
			resources.ApplyResources(this.DownloadListView, "DownloadListView");
			this.DownloadListView.Name = "DownloadListView";
			this.DownloadListView.UseCompatibleStateImageBehavior = false;
			this.DownloadListView.View = System.Windows.Forms.View.Details;
			// 
			// CancelButton
			// 
			resources.ApplyResources(this.CancelButton, "CancelButton");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.UseVisualStyleBackColor = true;
			// 
			// StartStopButton
			// 
			resources.ApplyResources(this.StartStopButton, "StartStopButton");
			this.StartStopButton.Name = "StartStopButton";
			this.StartStopButton.UseVisualStyleBackColor = true;
			this.StartStopButton.Click += new System.EventHandler(this.StartStopButton_Click);
			// 
			// TotalProgressBar
			// 
			resources.ApplyResources(this.TotalProgressBar, "TotalProgressBar");
			this.TotalProgressBar.Name = "TotalProgressBar";
			// 
			// SourceColumnHeader
			// 
			resources.ApplyResources(this.SourceColumnHeader, "SourceColumnHeader");
			// 
			// DestinationColumnHeader
			// 
			resources.ApplyResources(this.DestinationColumnHeader, "DestinationColumnHeader");
			// 
			// PercentColumnHeader
			// 
			resources.ApplyResources(this.PercentColumnHeader, "PercentColumnHeader");
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
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.CancelButton;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "DownloadCenterForm";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView DownloadListView;
		private System.Windows.Forms.Button CancelButton;
		private System.Windows.Forms.Button StartStopButton;
		private System.Windows.Forms.ProgressBar TotalProgressBar;
		private System.Windows.Forms.ColumnHeader SourceColumnHeader;
		private System.Windows.Forms.ColumnHeader DestinationColumnHeader;
		private System.Windows.Forms.ColumnHeader PercentColumnHeader;
		private System.ComponentModel.BackgroundWorker downloadBackgroundWorker;
	}
}