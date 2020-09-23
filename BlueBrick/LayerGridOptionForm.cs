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
	public partial class LayerGridOptionForm : Form
	{
		private LayerGrid mEditedGridLayer = null;
		private Font mCurrentChosenFont = null;

		public LayerGridOptionForm(LayerGrid gridLayer)
		{
			InitializeComponent();
			// save the reference on the layer that we are editing
			mEditedGridLayer = gridLayer;
			// update the controls with the data of the gridLayer
			// name and visibility
			this.nameTextBox.Text = gridLayer.Name;
			this.isVisibleCheckBox.Checked = gridLayer.Visible;
			// transparency
			this.alphaNumericUpDown.Value = gridLayer.Transparency;
			this.alphaTrackBar.Value = gridLayer.Transparency;
			// grid
			this.gridCheckBox.Checked = gridLayer.DisplayGrid;
			this.gridSizeNumericUpDown.Value = gridLayer.GridSizeInStud;
			this.gridPixelNumericUpDown.Value = (int)gridLayer.GridThickness;
			this.gridColorPictureBox.BackColor = gridLayer.GridColor;
			// subgrid
			this.subGridCheckBox.Checked = gridLayer.DisplaySubGrid;
			this.subGridSizeNumericUpDown.Value = gridLayer.SubDivisionNumber;
			this.subGridPixelNumericUpDown.Value = (int)gridLayer.SubGridThickness;
			this.subGridColorPictureBox.BackColor = gridLayer.SubGridColor;
			// cell index
			this.cellIndexCheckBox.Checked = gridLayer.DisplayCellIndex;
			this.cellIndexColumnComboBox.SelectedIndex = (int)gridLayer.CellIndexColumnType;
			this.cellIndexRowComboBox.SelectedIndex = (int)gridLayer.CellIndexRowType;
			updateChosenFont(gridLayer.CellIndexFont);
			this.cellIndexColorPictureBox.BackColor = gridLayer.CellIndexColor;
			this.cellIndexOriginXNumericUpDown.Value = gridLayer.CellIndexCornerX;
			this.cellIndexOriginYNumericUpDown.Value = gridLayer.CellIndexCornerY;
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			// create a copy of the edited layer to hold the old data
			LayerGrid oldLayerData = new LayerGrid();
			oldLayerData.CopyOptionsFrom(mEditedGridLayer);

			// create a new layer to store the new data
			LayerGrid newLayerData = new LayerGrid();

			// name and visibility
			newLayerData.Name = this.nameTextBox.Text;
			newLayerData.Visible = this.isVisibleCheckBox.Checked;
			//transparency
			newLayerData.Transparency = (int)(this.alphaNumericUpDown.Value);
			// grid
			newLayerData.DisplayGrid = this.gridCheckBox.Checked;
			newLayerData.GridSizeInStud = (int)this.gridSizeNumericUpDown.Value;
			newLayerData.GridThickness = (float)this.gridPixelNumericUpDown.Value;
			newLayerData.GridColor = this.gridColorPictureBox.BackColor;
			// subgrid
			newLayerData.DisplaySubGrid = this.subGridCheckBox.Checked;
			newLayerData.SubDivisionNumber = (int)this.subGridSizeNumericUpDown.Value;
			newLayerData.SubGridThickness = (float)this.subGridPixelNumericUpDown.Value;
			newLayerData.SubGridColor = this.subGridColorPictureBox.BackColor;
			// cell index
			newLayerData.DisplayCellIndex = this.cellIndexCheckBox.Checked;
			newLayerData.CellIndexColumnType = (LayerGrid.CellIndexType)this.cellIndexColumnComboBox.SelectedIndex;
			newLayerData.CellIndexRowType = (LayerGrid.CellIndexType)this.cellIndexRowComboBox.SelectedIndex;
			newLayerData.CellIndexFont = mCurrentChosenFont;
			newLayerData.CellIndexColor = this.cellIndexColorPictureBox.BackColor;
			newLayerData.CellIndexCornerX = (int)this.cellIndexOriginXNumericUpDown.Value;
			newLayerData.CellIndexCornerY = (int)this.cellIndexOriginYNumericUpDown.Value;

			// do a change option action
			ActionManager.Instance.doAction(new ChangeLayerOption(mEditedGridLayer, oldLayerData, newLayerData));
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

		private void updateChosenFont(Font newFont)
		{
			mCurrentChosenFont = newFont;
			this.cellIndexFontNameLabel.Text = mCurrentChosenFont.Name + " " + mCurrentChosenFont.SizeInPoints.ToString();
			this.cellIndexFontNameLabel.Font = mCurrentChosenFont;
		}

		private void gridColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = gridColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				gridColorPictureBox.BackColor = this.colorDialog.Color;
			}
		}

		private void subGridColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = subGridColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				subGridColorPictureBox.BackColor = this.colorDialog.Color;
			}
		}

		private void cellIndexColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = cellIndexColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				cellIndexColorPictureBox.BackColor = this.colorDialog.Color;
			}
		}

		private void buttonFont_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.fontDialog.Font = mCurrentChosenFont;
			// open the color box in modal
			DialogResult result = this.fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateChosenFont(this.fontDialog.Font);
			}
		}

		private void gridCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = this.gridCheckBox.Checked;
			// enable or disable all the grid line according to the state
			this.gridColorlabel.Enabled = enabled;
			this.gridColorPictureBox.Enabled = enabled;
			this.gridPixelNumericUpDown.Enabled = enabled;
			this.gridSizeNumericUpDown.Enabled = enabled;
			this.gridThicknessLabel.Enabled = enabled;
		}

		private void subGridCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = this.subGridCheckBox.Checked;
			// enable or disable all the sub grid line according to the state
			this.subGridColorlabel.Enabled = enabled;
			this.subGridColorPictureBox.Enabled = enabled;
			this.subGridPixelNumericUpDown.Enabled = enabled;
			this.subGridSizeNumericUpDown.Enabled = enabled;
			this.subGridThicknessLabel.Enabled = enabled;
		}

		private void cellIndexCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool enabled = this.cellIndexCheckBox.Checked;
			// enable or disable all the sub grid line according to the state
			this.cellIndexColorLabel.Enabled = enabled;
			this.cellIndexColorPictureBox.Enabled = enabled;
			this.cellIndexColumnComboBox.Enabled = enabled;
			this.cellIndexColumnLabel.Enabled = enabled;
			this.cellIndexFontButton.Enabled = enabled;
			this.cellIndexFontNameLabel.Enabled = enabled;
			this.cellIndexRowComboBox.Enabled = enabled;
			this.cellIndexRowLabel.Enabled = enabled;
			this.cellIndexOriginLabel.Enabled = enabled;
			this.cellIndexOriginXNumericUpDown.Enabled = enabled;
			this.cellIndexOriginYNumericUpDown.Enabled = enabled;
			this.cellIndexCommaLabel.Enabled = enabled;
			this.cellIndexOriginButton.Enabled = enabled;
		}

		private void cellIndexOriginButton_Click(object sender, EventArgs e)
		{
			PointF position = Map.Instance.getMostTopLeftBrickPosition();
			int x = (int)(position.X / (int)this.gridSizeNumericUpDown.Value) - 1;
			int y = (int)(position.Y / (int)this.gridSizeNumericUpDown.Value) - 1;
			if (position.X < 0)
				x--;
			if (position.Y < 0)
				y--;
			// set the new values in the controls
			this.cellIndexOriginXNumericUpDown.Value = x;
			this.cellIndexOriginYNumericUpDown.Value = y;
		}
	}
}