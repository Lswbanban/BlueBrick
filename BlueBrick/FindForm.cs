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
using BlueBrick.Properties;

namespace BlueBrick
{
	public partial class FindForm : Form
	{
		private string[] mLibraryPartList = BrickLibrary.Instance.getBrickNameList();
		private string[] mSelectionPartList = null;
		private string mBestPartToFindInSelection = null;
		private List<LayerBrick> mBrickOnlyLayerList = new List<LayerBrick>();

		#region init
		public FindForm()
		{
			InitializeComponent();
			// get the selected layer because we will need it
			Layer selectedLayer = Map.Instance.SelectedLayer;

			// determines which radio button should be selected
			bool isSelectionEmpty = (selectedLayer.SelectedObjects.Count == 0);
			this.inCurrentSelectionRadioButton.Checked = !isSelectionEmpty;
			this.inCurrentSelectionRadioButton.Enabled = !isSelectionEmpty;
			this.inLayerRadioButton.Checked = isSelectionEmpty;
			inLayerRadioButton_CheckedChanged(this.inLayerRadioButton, null);

			// fill the layer list (in reverse order)
			for (int i = Map.Instance.LayerList.Count-1; i >= 0; --i)
			{
				Layer layer = Map.Instance.LayerList[i];
				if (layer is LayerBrick)
				{
					// add a check box item and the corresponding layer reference in a private list
					this.LayerCheckedListBox.Items.Add(layer.Name, layer == selectedLayer);
					mBrickOnlyLayerList.Add(layer as LayerBrick);
				}
			}
			
			// construct the list of parts from the selection (if it is a valid selection)
			LayerBrick selectedBrickLayer = selectedLayer as LayerBrick;
			if ((selectedBrickLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				// collapse all the selection in a dictionnary with unique instance of each part
				// and a counter of each different part in the selection
				Dictionary<string, int> collaspedList = new Dictionary<string, int>();
				foreach (Layer.LayerItem item in selectedLayer.SelectedObjects)
				{
					// add the item
					addPartToBuildingDictionnary(ref collaspedList, item.PartNumber);
					// and also add its named parents
					List<Layer.LayerItem> namedParents = item.NamedParents;
					foreach (Layer.LayerItem parent in namedParents)
						addPartToBuildingDictionnary(ref collaspedList, parent.PartNumber);
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

		private void addPartToBuildingDictionnary(ref Dictionary<string, int> collaspedList, string partNumber)
		{
			int occurence = 0;
			if (collaspedList.TryGetValue(partNumber, out occurence))
			{
				collaspedList.Remove(partNumber);
				collaspedList.Add(partNumber, occurence + 1);
			}
			else
			{
				collaspedList.Add(partNumber, 1);
			}
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
												(!this.FindComboBox.SelectedItem.Equals(this.ReplaceComboBox.SelectedItem));
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
				this.FindPictureBox.Image = BrickLibrary.Instance.getImage(this.FindComboBox.SelectedItem as string);
			// update the search buttons
			updateButtonStatusAccordingToQueryValidity();
		}


		private void FindComboBox_DropDownClosed(object sender, EventArgs e)
		{
			// BUG in Dot Net: for some reason, when the drop down is closed
			// this change the selected item but do not call the event, so I call it.
			FindComboBox_SelectedIndexChanged(sender, e);
		}

		private void ReplaceComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.ReplaceComboBox.SelectedItem != null)
				this.ReplacePictureBox.Image = BrickLibrary.Instance.getImage(this.ReplaceComboBox.SelectedItem as string);
			// update the search buttons
			updateButtonStatusAccordingToQueryValidity();
		}

		private void ReplaceComboBox_DropDownClosed(object sender, EventArgs e)
		{
			// BUG in Dot Net: for some reason, when the drop down is closed
			// this change the selected item but do not call the event, so I call it.
			ReplaceComboBox_SelectedIndexChanged(sender, e);
		}

		private void SelectAllButton_Click(object sender, EventArgs e)
		{
			// get the search part
			string searchingPartNumber = this.FindComboBox.SelectedItem as string;
			// the new list of object to select
			List<Layer.LayerItem>	objectToSelect = new List<Layer.LayerItem>();

			// get the current selected layer because we will need it (normally it is never null)
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if (selectedLayer != null)
			{
				// check if the selection must be performed in the current selection
				// or in a new layer
				if (this.inCurrentSelectionRadioButton.Checked)
				{
					// sub select in current selection, iterate on the selection
					int nbSelectedItems = selectedLayer.SelectedObjects.Count;
					for (int i = 0; i < nbSelectedItems; ++i)
					{
						Layer.LayerItem currentItem = selectedLayer.SelectedObjects[i];
						string currentPartNumber = (currentItem as LayerBrick.Brick).PartNumber;
						if (currentPartNumber.Equals(searchingPartNumber))
							objectToSelect.Add(currentItem);
					}
				}
				else if (this.LayerCheckedListBox.CheckedItems.Count == 1)
				{
					// find in which layer the selection should be done
					LayerBrick layerToSelect = mBrickOnlyLayerList[this.LayerCheckedListBox.CheckedIndices[0]];
					// First we need to select the target layer if not already selected
					if (selectedLayer != layerToSelect)
					{
						Actions.ActionManager.Instance.doAction(new Actions.Layers.SelectLayer(layerToSelect));
						// important to update the new selected layer for the rest of the code
						selectedLayer = layerToSelect;
					}
					// then iterate on all the bricks of the selected layer to find the one we search
					foreach (LayerBrick.Brick brick in layerToSelect.BrickList)
						if (brick.PartNumber.Equals(searchingPartNumber))
							objectToSelect.Add(brick);
				}
			}

			// select the new objects
			selectedLayer.clearSelection();
			selectedLayer.addObjectInSelection(objectToSelect);

			// close the window
			this.Close();
		}

		private void ReplaceButton_Click(object sender, EventArgs e)
		{
			List<LayerBrick> layers = null;

			// check if the selection must be performed in the current selection
			// or in several layers
			if (this.inCurrentSelectionRadioButton.Checked)
			{
				// only one layer is selected
				layers = new List<LayerBrick>(1);

				// get the current selected layer because we will need it (normally it is never null)
				LayerBrick selectedLayer = (Map.Instance.SelectedLayer) as LayerBrick;
				if (selectedLayer != null)
					layers.Add(selectedLayer);
			}
			else
			{
				layers = new List<LayerBrick>(this.LayerCheckedListBox.CheckedIndices.Count);
				foreach (int index in this.LayerCheckedListBox.CheckedIndices)
					layers.Add(mBrickOnlyLayerList[index]);
			}

			// get the search and replace part number
			string searchingPartNumber = this.FindComboBox.SelectedItem as string;
			string replacementPartNumber = this.ReplaceComboBox.SelectedItem as string;

			// create the action
			Actions.Bricks.ReplaceBrick replaceAction = new Actions.Bricks.ReplaceBrick(layers, searchingPartNumber, replacementPartNumber, this.inCurrentSelectionRadioButton.Checked);
			// check if there was a budget limitation and if the user cancel the action
			bool canDoTheAction = true;
			if (Settings.Default.DisplayWarningMessageForBrickNotReplacedDueToBudgetLimitation && replaceAction.IsLimitedByBudget)
			{
				// if some brick cannot be replaced, display a warning message
				// use a local variable to get the value of the checkbox, by default we don't suggest the user to hide it
				bool dontDisplayMessageAgain = false;

				// display the warning message
				DialogResult result = ForgetableMessageBox.Show(BlueBrick.MainForm.Instance, Properties.Resources.ErrorMsgSomeBrickWereNotReplacedDueToBudgetLimitation,
								Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
								MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, ref dontDisplayMessageAgain);

				// set back the checkbox value in the settings (don't save the settings now, it will be done when exiting the application)
				Properties.Settings.Default.DisplayWarningMessageForBrickNotReplacedDueToBudgetLimitation = !dontDisplayMessageAgain;

				// change the action flag depending of the answer of the user
				canDoTheAction =  (result == DialogResult.Yes);
			}
			// do the action if we should
			if (canDoTheAction)
				Actions.ActionManager.Instance.doAction(replaceAction);

			// close the window
			this.Close();
		}
		#endregion
	}
}