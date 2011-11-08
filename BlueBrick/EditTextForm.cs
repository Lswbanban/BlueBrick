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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;

namespace BlueBrick
{
	public partial class EditTextForm : Form
	{
		// the font edited for the text
		private Font mEditedFont = Properties.Settings.Default.DefaultTextFont;

		// we use a constant size of the font for the edition,
		// else the text is unreadable if you use a small font
		private const float FONT_SIZE_FOR_EDITION = 14;

		public Font EditedFont
		{
			get { return mEditedFont; }
		}

		public Color EditedColor
		{
			get { return this.fontColorPictureBox.BackColor; }
		}

		public StringAlignment EditedAlignment
		{
			get
			{
				if (this.textBox.TextAlign == HorizontalAlignment.Left)
					return StringAlignment.Near;
				else if (this.textBox.TextAlign == HorizontalAlignment.Center)
					return StringAlignment.Center;
				else
					return StringAlignment.Far;

			}
		}

		public string EditedText
		{
			get { return this.textBox.Text; }
		}

		public EditTextForm(LayerText.TextCell textCell)
		{
			InitializeComponent();

			if (textCell != null)
			{
				// text font
				mEditedFont = textCell.Font;
				// color
				changeColor(textCell.FontColor);
				// text alignement
				if (textCell.TextAlignment == StringAlignment.Near)
					alignLeftButton_Click(null, null);
				else if (textCell.TextAlignment == StringAlignment.Center)
					alignCenterButton_Click(null, null);
				else
					alignRightButton_Click(null, null);
				// the text itself (replace the single "\n" by a "\r\n" to be
				// correctly handled by the text box. The "\r" is lost during a save of the file.
				this.textBox.Text = textCell.Text.Replace("\n", "\r\n").Replace("\r\r", "\r");
			}
			else
			{
				// text font
				mEditedFont = Properties.Settings.Default.DefaultTextFont;
				// color
				changeColor(Properties.Settings.Default.DefaultTextColor);
				// text alignement
				alignCenterButton_Click(null, null);
				// the text itself
				this.textBox.Text = BlueBrick.Properties.Resources.TextEnterText;
				this.textBox.SelectAll();
			}

			// text box font
			this.labelSize.Text = mEditedFont.Size.ToString();
			this.textBox.Font = new Font(mEditedFont.FontFamily, FONT_SIZE_FOR_EDITION, mEditedFont.Style);
		}

		private void EditTextForm_Shown(object sender, EventArgs e)
		{
			// focus the text box such as the user can type the text immediately
			this.textBox.Focus();
		}

		private void alignLeftButton_Click(object sender, EventArgs e)
		{
			this.textBox.TextAlign = HorizontalAlignment.Left;
			this.alignLeftButton.FlatStyle = FlatStyle.Popup;
			this.alignCenterButton.FlatStyle = FlatStyle.Standard;
			this.alignRightButton.FlatStyle = FlatStyle.Standard;
		}

		private void alignCenterButton_Click(object sender, EventArgs e)
		{
			this.textBox.TextAlign = HorizontalAlignment.Center;
			this.alignLeftButton.FlatStyle = FlatStyle.Standard;
			this.alignCenterButton.FlatStyle = FlatStyle.Popup;
			this.alignRightButton.FlatStyle = FlatStyle.Standard;
		}

		private void alignRightButton_Click(object sender, EventArgs e)
		{
			this.textBox.TextAlign = HorizontalAlignment.Right;
			this.alignLeftButton.FlatStyle = FlatStyle.Standard;
			this.alignCenterButton.FlatStyle = FlatStyle.Standard;
			this.alignRightButton.FlatStyle = FlatStyle.Popup;
		}

		private void fontButton_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.fontDialog.Font = mEditedFont;
			// open the color box in modal
			DialogResult result = this.fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// save the edited font
				mEditedFont = this.fontDialog.Font;
				// and use the same in the edit box, except that we override the font size
				this.labelSize.Text = mEditedFont.Size.ToString();
				this.textBox.Font = new Font(mEditedFont.FontFamily, FONT_SIZE_FOR_EDITION, mEditedFont.Style);
			}
		}

		private void fontColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = fontColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				changeColor(this.colorDialog.Color);
			}
		}

		private void changeColor(Color newColor)
		{
			// set the specified color in the back color of the picture box
			fontColorPictureBox.BackColor = newColor;
		}

		private void textBox_TextChanged(object sender, EventArgs e)
		{
			this.okButton.Enabled = (this.textBox.Text.Length > 0);
		}
	}
}