namespace BlueBrick
{
	partial class LibraryPackageSourceForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LibraryPackageSourceForm));
			this.checkBoxSearchNonLego = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxUnofficialPartLibraryURL = new System.Windows.Forms.TextBox();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonSearch = new System.Windows.Forms.Button();
			this.checkBoxSearchOfficial = new System.Windows.Forms.CheckBox();
			this.checkBoxSearchUnofficial = new System.Windows.Forms.CheckBox();
			this.backgroundWorkerSearchOnline = new System.ComponentModel.BackgroundWorker();
			this.labelURL = new System.Windows.Forms.Label();
			this.labelSearchStatus = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// checkBoxSearchNonLego
			// 
			resources.ApplyResources(this.checkBoxSearchNonLego, "checkBoxSearchNonLego");
			this.checkBoxSearchNonLego.Name = "checkBoxSearchNonLego";
			this.checkBoxSearchNonLego.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.Name = "label1";
			// 
			// textBoxUnofficialPartLibraryURL
			// 
			resources.ApplyResources(this.textBoxUnofficialPartLibraryURL, "textBoxUnofficialPartLibraryURL");
			this.textBoxUnofficialPartLibraryURL.Name = "textBoxUnofficialPartLibraryURL";
			// 
			// buttonCancel
			// 
			resources.ApplyResources(this.buttonCancel, "buttonCancel");
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonSearch
			// 
			resources.ApplyResources(this.buttonSearch, "buttonSearch");
			this.buttonSearch.Name = "buttonSearch";
			this.buttonSearch.UseVisualStyleBackColor = true;
			this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
			// 
			// checkBoxSearchOfficial
			// 
			resources.ApplyResources(this.checkBoxSearchOfficial, "checkBoxSearchOfficial");
			this.checkBoxSearchOfficial.Checked = true;
			this.checkBoxSearchOfficial.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSearchOfficial.Name = "checkBoxSearchOfficial";
			this.checkBoxSearchOfficial.UseVisualStyleBackColor = true;
			// 
			// checkBoxSearchUnofficial
			// 
			resources.ApplyResources(this.checkBoxSearchUnofficial, "checkBoxSearchUnofficial");
			this.checkBoxSearchUnofficial.Name = "checkBoxSearchUnofficial";
			this.checkBoxSearchUnofficial.UseVisualStyleBackColor = true;
			this.checkBoxSearchUnofficial.CheckedChanged += new System.EventHandler(this.checkBoxSearchUnofficial_CheckedChanged);
			// 
			// backgroundWorkerSearchOnline
			// 
			this.backgroundWorkerSearchOnline.WorkerReportsProgress = true;
			this.backgroundWorkerSearchOnline.WorkerSupportsCancellation = true;
			this.backgroundWorkerSearchOnline.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSearchOnline_DoWork);
			this.backgroundWorkerSearchOnline.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSearchOnline_ProgressChanged);
			this.backgroundWorkerSearchOnline.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerSearchOnline_RunWorkerCompleted);
			// 
			// labelURL
			// 
			resources.ApplyResources(this.labelURL, "labelURL");
			this.labelURL.Name = "labelURL";
			// 
			// labelSearchStatus
			// 
			resources.ApplyResources(this.labelSearchStatus, "labelSearchStatus");
			this.labelSearchStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelSearchStatus.Name = "labelSearchStatus";
			// 
			// LibraryPackageSourceForm
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.labelSearchStatus);
			this.Controls.Add(this.labelURL);
			this.Controls.Add(this.checkBoxSearchUnofficial);
			this.Controls.Add(this.checkBoxSearchOfficial);
			this.Controls.Add(this.buttonSearch);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.textBoxUnofficialPartLibraryURL);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.checkBoxSearchNonLego);
			this.Name = "LibraryPackageSourceForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBoxSearchNonLego;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBoxUnofficialPartLibraryURL;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonSearch;
		private System.Windows.Forms.CheckBox checkBoxSearchOfficial;
		private System.Windows.Forms.CheckBox checkBoxSearchUnofficial;
		private System.ComponentModel.BackgroundWorker backgroundWorkerSearchOnline;
		private System.Windows.Forms.Label labelURL;
		private System.Windows.Forms.Label labelSearchStatus;
	}
}