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
using BlueBrick.Actions;
using BlueBrick.Actions.Maps;

namespace BlueBrick
{
	public partial class GeneralInfoForm : Form
	{
		public GeneralInfoForm()
		{
			InitializeComponent();

			// fill lug list box from text file
			sFillLUGComboBox(this.lugComboBox);

			// fill the text controls
			this.AuthorTextBox.Text = Map.Instance.Author;
			this.lugComboBox.Text = Map.Instance.LUG;
			this.showTextBox.Text = Map.Instance.Show;
			this.dateTimePicker.Value = Map.Instance.Date;
			char[] splitter = { '\n' };
			this.commentTextBox.Lines = Map.Instance.Comment.Split(splitter);

			// compute the size
			RectangleF totalArea = Map.Instance.getTotalAreaInStud(true);

			this.labelWidthModule.Text = Math.Ceiling(totalArea.Width / 96.0f).ToString();
			this.labelHeightModule.Text = Math.Ceiling(totalArea.Height / 96.0f).ToString();

			this.labelWidthStud.Text = Math.Round(totalArea.Width).ToString();
			this.labelHeightStud.Text = Math.Round(totalArea.Height).ToString();

			this.labelWidthMeter.Text = (totalArea.Width * 0.008f).ToString("N2");
			this.labelHeightMeter.Text = (totalArea.Height * 0.008f).ToString("N2");

			this.labelWidthFeet.Text = (totalArea.Width * 0.026248f).ToString("N2");
			this.labelHeightFeet.Text = (totalArea.Height * 0.026248f).ToString("N2");
		}

		private void ButtonOk_Click(object sender, EventArgs e)
		{
			// save the data to the map (must do an action for that)
			ActionManager.Instance.doAction(new ChangeGeneralInfo(this.AuthorTextBox.Text,
				this.lugComboBox.Text, this.showTextBox.Text, this.dateTimePicker.Value,
				this.commentTextBox.Text));
		}

		public static void sFillLUGComboBox(ComboBox comboBoxToFill)
		{
			try
			{
				string lugListFileName = Application.StartupPath + "/config/LugList.txt";
				System.IO.StreamReader textReader = new System.IO.StreamReader(lugListFileName);
				comboBoxToFill.Items.Clear();
				comboBoxToFill.Sorted = true;
				while (!textReader.EndOfStream)
					comboBoxToFill.Items.Add(textReader.ReadLine());
				textReader.Close();
			}
			catch
			{
			}
		}
	}
}