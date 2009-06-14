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
	public partial class LayerAreaOptionForm : Form
	{
		private LayerArea mEditedLayer = null;
		private bool mIsMouseDown = false;

		public LayerAreaOptionForm(Layer layer)
		{
			InitializeComponent();
			// save the reference on the layer that we are editing
			mEditedLayer = layer as LayerArea;
			// update the controls with the data of the areaLayer
			// name and visibility
			this.nameTextBox.Text = layer.Name;
			this.isVisibleCheckBox.Checked = layer.Visible;
			// transparency and cell size
			this.alphaNumericUpDown.Value = mEditedLayer.Transparency;
			this.alphaProgressBar.Value = mEditedLayer.Transparency;
			this.cellSizeNumericUpDown.Value = mEditedLayer.AreaCellSizeInStud;
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			// create a copy of the edited layer to hold the old data
			LayerArea oldLayerData = new LayerArea();
			oldLayerData.CopyOptionsFrom(mEditedLayer);

			// create a new layer to store the new data
			LayerArea newLayerData = new LayerArea();

			// name and visibility
			newLayerData.Name = this.nameTextBox.Text;
			newLayerData.Visible = this.isVisibleCheckBox.Checked;
			//transparency and cell size
			newLayerData.Transparency = (int)(this.alphaNumericUpDown.Value);
			newLayerData.AreaCellSizeInStud = (int)(this.cellSizeNumericUpDown.Value);

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