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
	public partial class EditRulerForm : Form
	{
		// a clone of the ruler given in the constructor, that hold all the modification on the form is closed
		private LayerRuler.RulerItem mEditedRulerClone = null;

		#region get/set
		public LayerRuler.RulerItem EditedRulerClone
		{
			get { return mEditedRulerClone; }
		}
		#endregion

		public EditRulerForm(LayerRuler.RulerItem rulerItem)
		{
			InitializeComponent();

			// clone the specified ruler in a new instance that will receive the edited properties
			mEditedRulerClone = rulerItem.Clone() as LayerRuler.RulerItem;

			// set the different control with the current state of the ruler
			// line appearance
			this.lineThicknessNumericUpDown.Value = (decimal)(rulerItem.LineThickness);
			this.lineColorPictureBox.BackColor = rulerItem.Color;
			if (rulerItem is LayerRuler.LinearRuler)
				this.allowOffsetCheckBox.Checked = (rulerItem as LayerRuler.LinearRuler).AllowOffset;
			else
				this.allowOffsetCheckBox.Enabled = false;
			// guideline appearance
			this.dashPatternLineNumericUpDown.Value = (decimal)(rulerItem.GuidelineDashPattern[0]);
			this.dashPatternSpaceNumericUpDown.Value = (decimal)(rulerItem.GuidelineDashPattern[1]);
			this.guidelineThicknessNumericUpDown.Value = (decimal)(rulerItem.GuidelineThickness);
			this.guidelineColorPictureBox.BackColor = rulerItem.GuidelineColor;
			// measure and unit
			this.displayUnitCheckBox.Checked = rulerItem.DisplayUnit;
			this.displayMeasureTextCheckBox.Checked = rulerItem.DisplayDistance;
			this.unitComboBox.SelectedIndex = (int)(rulerItem.CurrentUnit);
			this.fontColorPictureBox.BackColor = rulerItem.MeasureColor;
			updateChosenFont(rulerItem.MeasureFont);
		}

		private void updateChosenFont(Font newFont)
		{
			this.fontNameLabel.ForeColor = this.fontColorPictureBox.BackColor;
			this.fontNameLabel.Text = newFont.Name + " " + newFont.SizeInPoints.ToString();
			this.fontNameLabel.Font = newFont;
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			// copy all the properties in the cloned rulers
			mEditedRulerClone.LineThickness = (float)(this.lineThicknessNumericUpDown.Value);
			mEditedRulerClone.Color = this.lineColorPictureBox.BackColor;
			if (mEditedRulerClone is LayerRuler.LinearRuler)
				(mEditedRulerClone as LayerRuler.LinearRuler).AllowOffset = this.allowOffsetCheckBox.Checked;
			// guideline appearance
			mEditedRulerClone.GuidelineDashPattern = new float[]{(float)(this.dashPatternLineNumericUpDown.Value), (float)(this.dashPatternSpaceNumericUpDown.Value)};
			mEditedRulerClone.GuidelineThickness = (float)(this.guidelineThicknessNumericUpDown.Value);
			mEditedRulerClone.GuidelineColor = this.guidelineColorPictureBox.BackColor;
			// measure and unit
			mEditedRulerClone.DisplayUnit = this.displayUnitCheckBox.Checked;
			mEditedRulerClone.DisplayDistance = this.displayMeasureTextCheckBox.Checked;
			mEditedRulerClone.CurrentUnit = (MapData.Tools.Distance.Unit)(this.unitComboBox.SelectedIndex);
			mEditedRulerClone.MeasureColor = this.fontColorPictureBox.BackColor;
			mEditedRulerClone.MeasureFont = this.fontNameLabel.Font;
		}

		private void displayMeasureTextCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = displayMeasureTextCheckBox.Checked;
			// change the unit properties
			this.displayUnitCheckBox.Enabled = enabled;
			this.unitLabel.Enabled = enabled;
			this.unitComboBox.Enabled = enabled;
			// change the font properties
			this.fontButton.Enabled = enabled;
			this.fontColorLabel.Enabled = enabled;
			this.fontColorPictureBox.Enabled = enabled;
			this.fontNameLabel.Enabled = enabled;
		}

		private void openColorDialogAndUpdatePictureBox(PictureBox pictureBox)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = pictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				pictureBox.BackColor = this.colorDialog.Color;
			}
		}

		private void fontColorPictureBox_Click(object sender, EventArgs e)
		{
			openColorDialogAndUpdatePictureBox(this.fontColorPictureBox);
			// update alls the font color
			this.fontNameLabel.ForeColor = this.fontColorPictureBox.BackColor;
		}

		private void lineColorPictureBox_Click(object sender, EventArgs e)
		{
			openColorDialogAndUpdatePictureBox(this.lineColorPictureBox);
		}

		private void guidelineColorPictureBox_Click(object sender, EventArgs e)
		{
			openColorDialogAndUpdatePictureBox(this.guidelineColorPictureBox);
		}

		private void fontButton_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.fontDialog.Font = this.fontNameLabel.Font;
			// open the color box in modal
			DialogResult result = this.fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateChosenFont(this.fontDialog.Font);
			}
		}
	}
}
