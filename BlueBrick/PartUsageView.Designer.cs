namespace BlueBrick
{
	partial class PartUsageView
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PartUsageView));
			this.partColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.quantiyColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colorColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.descriptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.usagePercentageColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.budgetCountColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.missingCountColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// partColumnHeader
			// 
			this.partColumnHeader.Name = "partColumnHeader";
			resources.ApplyResources(this.partColumnHeader, "partColumnHeader");
			// 
			// quantiyColumnHeader
			// 
			this.quantiyColumnHeader.Name = "quantiyColumnHeader";
			resources.ApplyResources(this.quantiyColumnHeader, "quantiyColumnHeader");
			// 
			// colorColumnHeader
			// 
			resources.ApplyResources(this.colorColumnHeader, "colorColumnHeader");
			this.colorColumnHeader.Name = "colorColumnHeader";
			// 
			// descriptionColumnHeader
			// 
			resources.ApplyResources(this.descriptionColumnHeader, "descriptionColumnHeader");
			this.descriptionColumnHeader.Name = "descriptionColumnHeader";
			// 
			// usagePercentageColumnHeader
			// 
			resources.ApplyResources(this.usagePercentageColumnHeader, "usagePercentageColumnHeader");
			this.usagePercentageColumnHeader.Name = "usagePercentageColumnHeader";
			// 
			// budgetCountColumnHeader
			// 
			resources.ApplyResources(this.budgetCountColumnHeader, "budgetCountColumnHeader");
			this.budgetCountColumnHeader.Name = "budgetCountColumnHeader";
			// 
			// missingCountColumnHeader
			// 
			resources.ApplyResources(this.missingCountColumnHeader, "missingCountColumnHeader");
			this.missingCountColumnHeader.Name = "missingCountColumnHeader";
			// 
			// PartUsageView
			// 
			this.AllowColumnReorder = true;
			this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.partColumnHeader,
            this.quantiyColumnHeader,
            this.colorColumnHeader,
            this.descriptionColumnHeader,
            this.budgetCountColumnHeader,
            this.missingCountColumnHeader,
            this.usagePercentageColumnHeader});
			this.FullRowSelect = true;
			this.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.UseCompatibleStateImageBehavior = false;
			this.View = System.Windows.Forms.View.Details;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ColumnHeader partColumnHeader;
		private System.Windows.Forms.ColumnHeader descriptionColumnHeader;
		private System.Windows.Forms.ColumnHeader quantiyColumnHeader;
		private System.Windows.Forms.ColumnHeader colorColumnHeader;
		private System.Windows.Forms.ColumnHeader usagePercentageColumnHeader;
		private System.Windows.Forms.ColumnHeader budgetCountColumnHeader;
		private System.Windows.Forms.ColumnHeader missingCountColumnHeader;
	}
}
