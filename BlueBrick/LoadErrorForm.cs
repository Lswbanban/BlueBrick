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
		public LoadErrorForm(string message, string details)
		{
			InitializeComponent();
			// set the message and detail texts
			this.MessageLabel.Text = message;
			this.DetailsTextBox.Text = details;
			// set the title
			this.Text = Properties.Resources.ErrorMsgTitleWarning;
			this.DetailButton.Text = Properties.Resources.ShowDetails;
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