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
		public EditRulerForm(LayerRuler.RulerItem rulerItem)
		{
			InitializeComponent();
			// set the different control with the current state of the ruler
			// line appearance
			this.lineThicknessNumericUpDown.Value = (decimal)(rulerItem.LineThickness);
			this.lineColorPictureBox.BackColor = rulerItem.Color;
			if (rulerItem is LayerRuler.LinearRuler)
				this.allowOffsetCheckBox.Checked = (rulerItem as LayerRuler.LinearRuler).AllowOffset;
			// guideline appearance
			this.dashPatternLineNumericUpDown.Value = (decimal)(rulerItem.GuidelineDashPattern[0]);
			this.dashPatternSpaceNumericUpDown.Value = (decimal)(rulerItem.GuidelineDashPattern[1]);
			this.guidelineThicknessNumericUpDown.Value = (decimal)(rulerItem.GuidelineThickness);
			this.guidelineColorPictureBox.BackColor = rulerItem.GuidelineColor;
			// measure and unit
			this.displayUnitCheckBox.Checked = rulerItem.DisplayUnit;
			this.displayMesureTextCheckBox.Checked = rulerItem.DisplayDistance;
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

		}

		private void displayMesureTextCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = displayMesureTextCheckBox.Enabled;
			// change the font properties
			this.fontButton.Enabled = enabled;
			this.fontColorLabel.Enabled = enabled;
			this.fontColorPictureBox.Enabled = enabled;
			this.fontNameLabel.Enabled = enabled;
			// change the unit properties
			this.displayUnitCheckBox.Enabled = enabled;
			if (enabled)
				enabled = this.displayUnitCheckBox.Checked;
			this.unitLabel.Enabled = enabled;
			this.unitComboBox.Enabled = enabled;
		}

		private void displayUnitCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = displayUnitCheckBox.Enabled;
			this.unitLabel.Enabled = enabled;
			this.unitComboBox.Enabled = enabled;
		}
	}
}
