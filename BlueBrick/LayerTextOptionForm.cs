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
using System.Windows.Forms;
using BlueBrick.MapData;
using BlueBrick.Actions;
using BlueBrick.Actions.Layers;
using System.Drawing;

namespace BlueBrick
{
	public partial class LayerTextOptionForm : Form
	{
		private Layer mEditedLayer = null;
		private Color mHullColor;

		public LayerTextOptionForm(Layer layer)
		{
			InitializeComponent();
			// save the reference on the layer that we are editing
			mEditedLayer = layer;
			// change the title if it is a text or ruler layer, otherwise, leave the default brick layer title
			if (layer is LayerRuler)
				this.Text = BlueBrick.Properties.Resources.LayerRulerOptionTitle;
			// update the controls with the data of the gridLayer
			// name and visibility
			this.nameTextBox.Text = layer.Name;
			this.isVisibleCheckBox.Checked = layer.Visible;
			// transparency
			this.alphaNumericUpDown.Value = layer.Transparency;
			this.alphaTrackBar.Value = layer.Transparency;
			// the display hull settings
			this.displayHullCheckBox.Checked = layer.DisplayHulls;
			updateHullColor(layer.PenToDrawHull.Color);
			hullThicknessNumericUpDown.Value = (int)layer.PenToDrawHull.Width;
			// call the checkchange to force the enable of the hull color option
			displayHullCheckBox_CheckedChanged(this, null);
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			// create a copy of the edited layer to hold the old data
			LayerText oldLayerData = new LayerText();
			oldLayerData.CopyOptionsFrom(mEditedLayer);

			// create a new layer to store the new data
			LayerText newLayerData = new LayerText();

			// name and visibility
			newLayerData.Name = this.nameTextBox.Text;
			newLayerData.Visible = this.isVisibleCheckBox.Checked;

			//transparency
			newLayerData.Transparency = (int)(this.alphaNumericUpDown.Value);

			// hull
			newLayerData.DisplayHulls = this.displayHullCheckBox.Checked;
			newLayerData.PenToDrawHull = new Pen(mHullColor, (int)hullThicknessNumericUpDown.Value);

			// do a change option action
			ActionManager.Instance.doAction(new ChangeLayerOption(mEditedLayer, oldLayerData, newLayerData));
		}
		private void updateHullColor(Color newColor)
		{
			// memorise the new color in the internal variable
			mHullColor = newColor;
			// and update the UI depending on the checkbox for displaying hull or not
			if (displayHullCheckBox.Checked)
				hullColorPictureBox.BackColor = newColor;
			else
				hullColorPictureBox.BackColor = SystemColors.ControlLight;
		}

		private void alphaTrackBar_Scroll(object sender, EventArgs e)
		{
			alphaNumericUpDown.Value = alphaTrackBar.Value;
		}

		private void alphaNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			alphaTrackBar.Value = (int)(alphaNumericUpDown.Value);
		}

		private void alphaNumericUpDown_KeyUp(object sender, KeyEventArgs e)
		{
			alphaNumericUpDown_ValueChanged(null, null);
		}

		private void hullColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = mHullColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateHullColor(this.colorDialog.Color);
			}
		}

		private void displayHullCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			hullColorPictureBox.Enabled = displayHullCheckBox.Checked;
			updateHullColor(mHullColor);
			hullThicknessNumericUpDown.Enabled = displayHullCheckBox.Checked;
			hullThicknessUnitLabel.Enabled = displayHullCheckBox.Checked;
		}
	}
}