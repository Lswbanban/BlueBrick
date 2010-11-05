using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BlueBrick
{
	public partial class LoadErrorForm : Form
	{
		public LoadErrorForm()
		{
			InitializeComponent();
			// set the title
			this.Text = Properties.Resources.ErrorMsgTitleWarning;
			this.DetailButton.Text = Properties.Resources.ShowDetails;
		}

		public void setMessageText(string message)
		{
			this.MessageLabel.Text = message;
		}

		public void setDetailsText(string details)
		{
			this.DetailsTextBox.Text = details;
		}

		private void DetailButton_Click(object sender, EventArgs e)
		{
			this.DetailsTextBox.Visible = !(this.DetailsTextBox.Visible);
			if (this.DetailsTextBox.Visible)
				this.DetailButton.Text = Properties.Resources.HideDetails;
			else
				this.DetailButton.Text = Properties.Resources.ShowDetails;
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}