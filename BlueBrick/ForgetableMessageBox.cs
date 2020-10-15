// BlueBrick, a LEGO(c) layout editor.
// Copyright (C) 2008 Alban NANTY
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// see http://www.fsf.org/licensing/licenses/gpl.html
// and http://www.gnu.org/licenses/
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using System.Drawing;
using System.Windows.Forms;

namespace BlueBrick
{
	/// <summary>
	/// A simple implementation of a MessageBox with a checkbox to not display it again.
	/// </summary>
	public partial class ForgetableMessageBox : Form
	{
		#region the Show methods
		/// <summary>
		/// Use the same interface than the MessageBox. See the doc of the Message box.
		/// </summary>
		/// <returns>the dialog result as for the Message box</returns>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons,
			MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, ref bool checkboxValue)
		{
			// create a message box
			ForgetableMessageBox messageBox = new ForgetableMessageBox();

			// set the parameters
			messageBox.Text = caption;
			messageBox.messageLabel.Text = text;
			messageBox.setButtons(buttons);
			messageBox.setDefaultButtons(defaultButton);
			messageBox.setIcon(icon);
			messageBox.dontShowCheckBox.Checked = checkboxValue;

			// show the message box in modal state and get its result
			DialogResult result = messageBox.ShowDialog(owner);

			// put back the check state flag in the ref variable
			checkboxValue = messageBox.dontShowCheckBox.Checked;

			// and return the result
			return result;
		}
		#endregion

		public ForgetableMessageBox()
		{
			InitializeComponent();
		}

		#region set parameters
		private void setButtons(MessageBoxButtons buttons)
		{
			switch (buttons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
					this.button1.Text = BlueBrick.Properties.Resources.ErrorMsgAbortButton;
					this.button1.DialogResult = DialogResult.Abort;
					this.button2.Text = BlueBrick.Properties.Resources.ErrorMsgRetryButton;
					this.button2.DialogResult = DialogResult.Retry;
					this.button3.Text = BlueBrick.Properties.Resources.ErrorMsgIgnoreButton;
					this.button3.DialogResult = DialogResult.Ignore;
					this.AcceptButton = button3;
					this.CancelButton = button1;
					break;
				case MessageBoxButtons.OK:
					this.button1.Text = BlueBrick.Properties.Resources.ErrorMsgOkButton;
					this.button1.DialogResult = DialogResult.OK;
					this.button2.Hide();
					this.button3.Hide();
					this.AcceptButton = button1;
					this.CancelButton = button1;
					break;
				case MessageBoxButtons.OKCancel:
					this.button1.Text = BlueBrick.Properties.Resources.ErrorMsgOkButton;
					this.button1.DialogResult = DialogResult.OK;
					this.button2.Text = BlueBrick.Properties.Resources.ErrorMsgCancelButton;
					this.button2.DialogResult = DialogResult.Cancel;
					this.button3.Hide();
					this.AcceptButton = button1;
					this.CancelButton = button2;
					break;
				case MessageBoxButtons.RetryCancel:
					this.button1.Text = BlueBrick.Properties.Resources.ErrorMsgRetryButton;
					this.button1.DialogResult = DialogResult.Retry;
					this.button2.Text = BlueBrick.Properties.Resources.ErrorMsgCancelButton;
					this.button2.DialogResult = DialogResult.Cancel;
					this.button3.Hide();
					this.AcceptButton = button1;
					this.CancelButton = button2;
					break;
				case MessageBoxButtons.YesNo:
					this.button1.Text = BlueBrick.Properties.Resources.ErrorMsgYesButton;
					this.button1.DialogResult = DialogResult.Yes;
					this.button2.Text = BlueBrick.Properties.Resources.ErrorMsgNoButton;
					this.button2.DialogResult = DialogResult.No;
					this.button3.Hide();
					this.AcceptButton = button1;
					this.CancelButton = button2;
					break;
				case MessageBoxButtons.YesNoCancel:
					this.button1.Text = BlueBrick.Properties.Resources.ErrorMsgYesButton;
					this.button1.DialogResult = DialogResult.Yes;
					this.button2.Text = BlueBrick.Properties.Resources.ErrorMsgNoButton;
					this.button2.DialogResult = DialogResult.No;
					this.button3.Text = BlueBrick.Properties.Resources.ErrorMsgCancelButton;
					this.button3.DialogResult = DialogResult.Cancel;
					this.AcceptButton = button1;
					this.CancelButton = button3;
					break;
			}
		}

		private void setDefaultButtons(MessageBoxDefaultButton defaultButton)
		{
			switch (defaultButton)
			{
				case MessageBoxDefaultButton.Button1:
					this.button1.Focus();
					break;
				case MessageBoxDefaultButton.Button2:
					this.button2.Focus();
					break;
				case MessageBoxDefaultButton.Button3:
					this.button3.Focus();
					break;
			}
		}

		private void setIcon(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.Information:
//				case MessageBoxIcon.Asterisk: // same as information
					this.iconPictureBox.Image = SystemIcons.Information.ToBitmap();
					break;
				case MessageBoxIcon.Error:
//				case MessageBoxIcon.Hand: // same as error
//				case MessageBoxIcon.Stop: // same as error
					this.iconPictureBox.Image = SystemIcons.Error.ToBitmap();
					break;
				case MessageBoxIcon.Exclamation:
//				case MessageBoxIcon.Warning: //same as exclamation
					this.iconPictureBox.Image = SystemIcons.Exclamation.ToBitmap();
					break;
				case MessageBoxIcon.Question:
					this.iconPictureBox.Image = SystemIcons.Question.ToBitmap();
					break;
				case MessageBoxIcon.None:
					this.iconAndMessageTableLayoutPanel.ColumnStyles[0].Width = 0;
					break;
			}
		}
		#endregion
	}
}
