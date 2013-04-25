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
using BlueBrick.Actions.Layers;

namespace BlueBrick
{
	public partial class LayerBrickOptionForm : Form
	{
		private Layer mEditedLayer = null;
		private bool mIsMouseDown = false;

		public LayerBrickOptionForm(Layer layer)
		{
			InitializeComponent();
			// save the reference on the layer that we are editing
			mEditedLayer = layer;
			// change the title if it is a text or ruler layer, otherwise, leave the default brick layer title
			if (layer is LayerText)
				this.Text = BlueBrick.Properties.Resources.LayerTextOptionTitle;
			else if (layer is LayerRuler)
				this.Text = BlueBrick.Properties.Resources.LayerRulerOptionTitle;
			// update the controls with the data of the gridLayer
			// name and visibility
			this.nameTextBox.Text = layer.Name;
			this.isVisibleCheckBox.Checked = layer.Visible;
			// transparency
			this.alphaNumericUpDown.Value = layer.Transparency;
			this.alphaProgressBar.Value = layer.Transparency;
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

		private int getPercentageValueFromMouseCoord(int x)
		{
			int value = (x * 100) / alphaProgressBar.Width;
			if (value < 0)
				value = 0;
			if (value > 100)
				value = 100;
			return value;
		}

		private void alphaProgressBar_MouseDown(object sender, MouseEventArgs e)
		{
			mIsMouseDown = true;
			int value = getPercentageValueFromMouseCoord(e.X);
			alphaProgressBar.Value = value;
			alphaNumericUpDown.Value = value;
		}

		private void alphaProgressBar_MouseMove(object sender, MouseEventArgs e)
		{
			if (mIsMouseDown)
			{
				int value = getPercentageValueFromMouseCoord(e.X);
				alphaProgressBar.Value = value;
				alphaNumericUpDown.Value = value;
			}
		}

		private void alphaProgressBar_MouseUp(object sender, MouseEventArgs e)
		{
			mIsMouseDown = false;
		}

		private void alphaNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (!mIsMouseDown)
			{
				alphaProgressBar.Value = (int)(alphaNumericUpDown.Value);
				alphaProgressBar.Invalidate();
			}
		}

		private void alphaNumericUpDown_KeyUp(object sender, KeyEventArgs e)
		{
			alphaNumericUpDown_ValueChanged(null, null);
		}
	}
}