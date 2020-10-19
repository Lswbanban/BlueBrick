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

namespace BlueBrick
{
	public partial class LayerBrickOptionForm : Form
	{
		private Layer mEditedLayer = null;

		public LayerBrickOptionForm(Layer layer)
		{
			InitializeComponent();
			// save the reference on the layer that we are editing
			mEditedLayer = layer;
			// update the controls with the data of the gridLayer
			// name and visibility
			this.nameTextBox.Text = layer.Name;
			this.isVisibleCheckBox.Checked = layer.Visible;
			// transparency
			this.alphaNumericUpDown.Value = layer.Transparency;
			this.alphaTrackBar.Value = layer.Transparency;
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			// create a copy of the edited layer to hold the old data
			LayerBrick oldLayerData = new LayerBrick();
			oldLayerData.CopyOptionsFrom(mEditedLayer);

			// create a new layer to store the new data
			LayerBrick newLayerData = new LayerBrick();

			// name and visibility
			newLayerData.Name = this.nameTextBox.Text;
			newLayerData.Visible = this.isVisibleCheckBox.Checked;

			//transparency
			newLayerData.Transparency = (int)(this.alphaNumericUpDown.Value);

			// do a change option action
			ActionManager.Instance.doAction(new ChangeLayerOption(mEditedLayer, oldLayerData, newLayerData));
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
	}
}