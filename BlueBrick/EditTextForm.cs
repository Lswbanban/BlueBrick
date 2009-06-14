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
		public Font EditedFont
		{
			get { return this.textBox.Font; }
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
				this.textBox.Font = textCell.Font;
				// color
				changeColor(textCell.FontColor);
				// text alignement
				if (textCell.TextAlignment == StringAlignment.Near)
					alignLeftButton_Click(null, null);
				else if (textCell.TextAlignment == StringAlignment.Center)
					alignCenterButton_Click(null, null);
				else
					alignRightButton_Click(null, null);
				// the text itself
				this.textBox.Text = textCell.Text;
			}
			else
			{
				// text font
				this.textBox.Font = Properties.Settings.Default.DefaultTextFont;
				// color
				changeColor(Properties.Settings.Default.DefaultTextColor);
				// text alignement
				alignCenterButton_Click(null, null);
				// the text itself
				this.textBox.Text = BlueBrick.Properties.Resources.TextEnterText;
				this.textBox.SelectAll();
			}
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
			this.fontDialog.Font = this.textBox.Font;
			// open the color box in modal
			DialogResult result = this.fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				this.textBox.Font = this.fontDialog.Font;
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
			this.textBox.ForeColor = newColor;
			// check if we need to change the background color for a better contrast
			if ((newColor.R > 127) && (newColor.G > 127) && (newColor.B > 127))
				this.textBox.BackColor = Color.Black;
			else
				this.textBox.BackColor = Color.White;
		}

		private void textBox_TextChanged(object sender, EventArgs e)
		{
			this.okButton.Enabled = (this.textBox.Text.Length > 0);
		}
	}
}