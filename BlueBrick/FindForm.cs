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

namespace BlueBrick
{
	public partial class FindForm : Form
	{
		private string[] mLibraryPartList = MapData.BrickLibrary.Instance.getBrickNameList();
		private string[] mSelectionPartList = null;
		private string mBestPartToFindInSelection = null;

		#region init
		public FindForm()
		{
			InitializeComponent();
			// get the selected layer because we will need it
			BlueBrick.MapData.Layer selectedLayer = BlueBrick.MapData.Map.Instance.SelectedLayer;

			// determines which radio button should be selected
			bool isSelectionEmpty = (selectedLayer.SelectedObjects.Count == 0);
			this.inCurrentSelectionRadioButton.Checked = !isSelectionEmpty;
			this.inCurrentSelectionRadioButton.Enabled = !isSelectionEmpty;
			this.inLayerRadioButton.Checked = isSelectionEmpty;
			inLayerRadioButton_CheckedChanged(this.inLayerRadioButton, null);

			// fill the layer list (in reverse order)
			for (int i = BlueBrick.MapData.Map.Instance.LayerList.Count-1; i >= 0; --i)
			{
				BlueBrick.MapData.Layer layer = BlueBrick.MapData.Map.Instance.LayerList[i];
				if (layer.GetType().Name.Equals("LayerBrick"))
					this.LayerCheckedListBox.Items.Add(layer.Name, layer == selectedLayer);
			}
			
			// construct the list of parts from the selection (if it is a valid selection)
			BlueBrick.MapData.LayerBrick selectedBrickLayer = selectedLayer as BlueBrick.MapData.LayerBrick;
			if ((selectedBrickLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				// collapse all the selection in a dictionnary with unique instance of each part
				// and a counter of each different part in the selection
				Dictionary<string, int> collaspedList = new Dictionary<string, int>();
				int nbSelectedItems = selectedLayer.SelectedObjects.Count;
				for (int i = 0; i < nbSelectedItems; ++i)
				{
					string currentPartNumber = (selectedLayer.SelectedObjects[i] as MapData.LayerBrick.Brick).PartNumber;
					int occurence = 0;
					if (collaspedList.TryGetValue(currentPartNumber, out occurence))
					{
						collaspedList.Remove(currentPartNumber);
						collaspedList.Add(currentPartNumber, occurence + 1);
					}
					else
					{
						collaspedList.Add(currentPartNumber, 1);
					}
				}
				// construct the list by expanding the dictionnary
				// and at the same time find the best part (the one which higher occurence)
				int j = 0;
				int bestScore = 0;
				mSelectionPartList = new string[collaspedList.Count];
				foreach (KeyValuePair<string, int> keyValue in collaspedList)
				{
					mSelectionPartList[j++] = keyValue.Key;
					if (keyValue.Value > bestScore)
					{
						bestScore = keyValue.Value;
						mBestPartToFindInSelection = keyValue.Key;
					}
				}
			}

			// fill the find and replace combo box
			if (mSelectionPartList != null)
				this.FindComboBox.Items.AddRange(mSelectionPartList);
			else
				this.FindComboBox.Items.AddRange(mLibraryPartList);
			this.ReplaceComboBox.Items.AddRange(mLibraryPartList);
			setBestSelectedItemForFindComboBox();

			// update the check all button according to the number of layer checked
			// the function to update the status of the button will be called by the event handler
			LayerCheckedListBox_SelectedIndexChanged(null, null);
		}

		private void setBestSelectedItemForFindComboBox()
		{
			if (this.FindComboBox.Items.Count > 0)
			{
				if (mBestPartToFindInSelection != null)
					this.FindComboBox.SelectedItem = mBestPartToFindInSelection;
				else
					this.FindComboBox.SelectedIndex = 0;
			}
		}
		#endregion

		#region event handler
		private void inLayerRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			// Enabled or disable the controls depending if the radio button is checked
			bool enabled = this.inLayerRadioButton.Checked;
			this.LayerCheckedListBox.Enabled = enabled;
			this.allLayerCheckBox.Enabled = enabled;

			// change the list of findable items (if there's 2 lists)
			if (mSelectionPartList != null)
			{
				string previousSelection = this.FindComboBox.SelectedItem as string;
				this.FindComboBox.Items.Clear();
				if (enabled)
					this.FindComboBox.Items.AddRange(mLibraryPartList);
				else
					this.FindComboBox.Items.AddRange(mSelectionPartList);
				// reset the selection if it exist
				if (previousSelection != null)
					this.FindComboBox.SelectedItem = previousSelection;
				// if the reset of the previous selection failed, set the best selection
				if (this.FindComboBox.SelectedItem == null)
					setBestSelectedItemForFindComboBox();
			}

			// update the search buttons
			updateButtonStatusAccordingToQueryValidity();
		}

		private void updateButtonStatusAccordingToQueryValidity()
		{
			// determines the condition for what and where
			bool whatToSearchIsValidForFind = (this.FindComboBox.SelectedItem != null);
			bool whatToSearchIsValidForReplace = whatToSearchIsValidForFind &&
												(this.ReplaceComboBox.SelectedItem != null) &&
												(this.FindComboBox.SelectedIndex != this.ReplaceComboBox.SelectedIndex);
			bool whereToSearchIsValidForFind = this.inCurrentSelectionRadioButton.Checked ||
												(this.LayerCheckedListBox.CheckedItems.Count == 1);
			bool whereToSearchIsValidForReplace = this.inCurrentSelectionRadioButton.Checked ||
												(this.LayerCheckedListBox.CheckedItems.Count != 0);
			// set the enability of the button
			this.SelectAllButton.Enabled = whatToSearchIsValidForFind && whereToSearchIsValidForFind;
			this.ReplaceButton.Enabled = whatToSearchIsValidForReplace && whereToSearchIsValidForReplace;
		}

		private void allLayerCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (this.allLayerCheckBox.CheckState != CheckState.Indeterminate)
			{
				for (int i = 0; i < this.LayerCheckedListBox.Items.Count; ++i)
					this.LayerCheckedListBox.SetItemChecked(i, this.allLayerCheckBox.Checked);
				// update the search buttons
				updateButtonStatusAccordingToQueryValidity();
			}
		}

		private void LayerCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.LayerCheckedListBox.CheckedItems.Count == 0)
				this.allLayerCheckBox.CheckState = CheckState.Unchecked;
			else if (this.LayerCheckedListBox.CheckedItems.Count == this.LayerCheckedListBox.Items.Count)
				this.allLayerCheckBox.CheckState = CheckState.Checked;
			else
				this.allLayerCheckBox.CheckState = CheckState.Indeterminate;
			// update the search buttons
			updateButtonStatusAccordingToQueryValidity();
		}

		private void FindComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.FindComboBox.SelectedItem != null)
				this.FindPictureBox.Image = MapData.BrickLibrary.Instance.getImage(this.FindComboBox.SelectedItem as string);
			// update the search buttons
			updateButtonStatusAccordingToQueryValidity();
		}

		private void ReplaceComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.ReplaceComboBox.SelectedItem != null)
				this.ReplacePictureBox.Image = MapData.BrickLibrary.Instance.getImage(this.ReplaceComboBox.SelectedItem as string);
			// update the search buttons
			updateButtonStatusAccordingToQueryValidity();
		}
		#endregion
	}
}