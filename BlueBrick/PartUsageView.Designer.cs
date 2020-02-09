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
			this.partColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.quantiyColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colorColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.descriptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.budgetColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// partColumnHeader
			// 
			this.partColumnHeader.Name = "partColumnHeader";
			this.partColumnHeader.Text = "Part";
			this.partColumnHeader.Width = 80;
			// 
			// quantiyColumnHeader
			// 
			this.quantiyColumnHeader.Name = "quantiyColumnHeader";
			this.quantiyColumnHeader.Text = "Quantity";
			this.quantiyColumnHeader.Width = 55;
			// 
			// colorColumnHeader
			// 
			this.colorColumnHeader.Name = "colorColumnHeader";
			this.colorColumnHeader.Text = "Color";
			// 
			// descriptionColumnHeader
			// 
			this.descriptionColumnHeader.Name = "descriptionColumnHeader";
			this.descriptionColumnHeader.Text = "Description";
			// 
			// budgetColumnHeader
			// 
			this.budgetColumnHeader.Name = "budgetColumnHeader";
			this.budgetColumnHeader.Text = "Part Usage %";
			this.budgetColumnHeader.Width = 150;
			// 
			// PartUsageView
			// 
			this.AllowColumnReorder = true;
			this.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.partColumnHeader,
            this.quantiyColumnHeader,
            this.budgetColumnHeader,
            this.colorColumnHeader,
            this.descriptionColumnHeader});
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
		private System.Windows.Forms.ColumnHeader budgetColumnHeader;
	}
}
