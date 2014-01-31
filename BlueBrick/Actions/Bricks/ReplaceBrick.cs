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
using System.Text;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Bricks
{
	class ReplaceBrick : Action
	{
		/// <summary>
		/// A small class to handle a pair of the original brick and the brick to replace
		/// </summary>
		private class BrickPair
		{
			public Layer.LayerItem mOldBrick = null;
            public Layer.LayerItem mNewBrick = null;

            public BrickPair(Layer.LayerItem oldBrick, Layer.LayerItem newBrick)
			{
				mOldBrick = oldBrick;
				mNewBrick = newBrick;
			}
		}

		private List<LayerBrick> mBrickLayerList = null;
		private List<List<BrickPair>> mBrickPairList = null;
		private string mPartNumberToReplace = string.Empty;

		// for selection of the replaced bricks
		private LayerBrick mLayerWhereToSelectTheReplacedBricks = null;
		private List<BrickPair> mReplacedBricksToSelect = new List<BrickPair>();

		public ReplaceBrick(List<LayerBrick> layerList, string partNumberToReplace, string newPartNumber, bool replaceInSelectionOnly)
		{
			// store the list of layer for which this action apply and create a list of the same size for the bricks
			mBrickLayerList = layerList;
			mPartNumberToReplace = partNumberToReplace;
			mBrickPairList = new List<List<BrickPair>>(layerList.Count);
			// iterate on all the layers
			foreach (LayerBrick layer in layerList)
			{
				// memorize the layer that has the selected bricks
				if (layer.SelectedObjects.Count > 0)
					mLayerWhereToSelectTheReplacedBricks = layer;
				// add the list of brick pair (that can stay empty if no brick is found in that layer)
				List<BrickPair> currentPairList = new List<BrickPair>();
				mBrickPairList.Add(currentPairList);
                // compute the list of bricks on which we should iterate
                List<Layer.LayerItem> itemsToReplace = null;
				if (replaceInSelectionOnly)
					itemsToReplace = computeListOfItemToReplace(layer.SelectedObjects);
				else
					itemsToReplace = computeListOfItemToReplace(layer.BrickList);
                // iterate on all the bricks of the layer or on the selection to find the brick to replace
				foreach (Layer.LayerItem item in itemsToReplace)
                {
                    // create the new item
                    Layer.LayerItem newItem = createReplacementBrick(item, newPartNumber);
                    // create the pair and add it to the list
                    BrickPair brickPair = new BrickPair(item, newItem);
                    currentPairList.Add(brickPair);
                    // check if we also need to add this pair to the list of brick to reselect
                    if (layer.SelectedObjects.Contains(item))
                        mReplacedBricksToSelect.Add(brickPair);
                }
            }
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionReplaceBrick;
			actionName = actionName.Replace("&", mPartNumberToReplace);
			return actionName;
		}

		/// <summary>
		/// Compute the list of the items that should be replaced, i.e. the items that have the same part number
		/// as the one specified for this replacement action. The list is constructed from the specified list of
		/// brick. The computed list also included the parents of the specified list if their name match the
		/// name to replace.
		/// </summary>
		/// <typeparam name="T">a base item type</typeparam>
		/// <param name="brickList">the list from which constructing the replacement list</param>
		/// <returns>the list of all the item that should be replaced</returns>
		private List<Layer.LayerItem> computeListOfItemToReplace<T>(List<T> brickList) where T : Layer.LayerItem
		{
			List<Layer.LayerItem> result = new List<Layer.LayerItem>(brickList.Count);
			foreach (Layer.LayerItem item in brickList)
			{
				// add the parent of the item if they have the correct name (and not already added)
				if (item.Group != null)
				{
					List<Layer.LayerItem> namedParent = item.NamedParent;
					foreach (Layer.LayerItem group in namedParent)
						if (group.PartNumber.Equals(mPartNumberToReplace) && !result.Contains(group))
							result.Add(group);						
				}
				// add the item itself if it has the correct name
				if (item.PartNumber.Equals(mPartNumberToReplace))
					result.Add(item);
			}
			// return the resulting list
			return result;
		}

        private Layer.LayerItem createReplacementBrick(Layer.LayerItem brick, string newPartNumber)
		{
			// compute the altitude of the brick we want to replace
			float altitude = 0.0f;
			if (brick.IsAGroup)
			{
				// get the average altitude of all the children
				List<Layer.LayerItem> children = (brick as Layer.Group).getAllLeafItems();
				if (children.Count > 0)
				{
					foreach (Layer.LayerItem child in children)
						altitude += (child as LayerBrick.Brick).Altitude;
					altitude /= children.Count;
				}
			}
			else
			{
				altitude = (brick as LayerBrick.Brick).Altitude;
			}

			// create a new brick and copy all the paramters of the old one
            Layer.LayerItem newBrick = null;
            if (BrickLibrary.Instance.isAGroup(newPartNumber))
            {
                newBrick = new Layer.Group(newPartNumber);
				// set the altitude to all children
				List<Layer.LayerItem> children = (newBrick as Layer.Group).getAllLeafItems();
				foreach (Layer.LayerItem child in children)
					(child as LayerBrick.Brick).Altitude = altitude;
            }
            else
            {
                newBrick = new LayerBrick.Brick(newPartNumber);
				(newBrick as LayerBrick.Brick).Altitude = altitude;
            }
			newBrick.Orientation = brick.Orientation;
			newBrick.Center = brick.Center;
            // return the new brick
            return newBrick;
		}

        private int removeOneItem(LayerBrick layer, Layer.LayerItem itemToRemove)
        {
			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickRemoved(layer, itemToRemove);

            int brickIndex = -1;
            // check if it's a group or a simple brick
            if (itemToRemove.IsAGroup)
            {
                List<Layer.LayerItem> bricksToRemove = (itemToRemove as Layer.Group).getAllLeafItems();
                foreach (Layer.LayerItem item in bricksToRemove)
                    brickIndex = layer.removeBrick(item as LayerBrick.Brick);
				// we alsways take the last index to be sure we don't keep an index bigger than the brick list
				// after removing all the parts of the group
            }
            else
            {
				brickIndex = layer.removeBrick(itemToRemove as LayerBrick.Brick);
            }
			// return the index of the removed brick
			return brickIndex;
        }

		private void addOneItem(LayerBrick layer, Layer.LayerItem itemToAdd, int itemIndex)
		{
			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickAdded(layer, itemToAdd);

			// check if it's a group or a simple brick
			if (itemToAdd.IsAGroup)
			{
				List<Layer.LayerItem> bricksToAdd = (itemToAdd as Layer.Group).getAllLeafItems();
				// since we will add all the brick at the same index, iterating in the normal order
				// the insertion order will be reversed. So we reverse the list to make the insertion in correct order
				bricksToAdd.Reverse();
				foreach (Layer.LayerItem item in bricksToAdd)
					layer.addBrick(item as LayerBrick.Brick, itemIndex);
			}
			else
			{
				layer.addBrick(itemToAdd as LayerBrick.Brick, itemIndex);
			}
		}

		private void replace(bool newByOld)
		{
			// iterate on all the layers
			for (int i = 0 ; i < mBrickLayerList.Count; ++i)
			{
				// get the layer and the list of brick pair to replace
				LayerBrick currentLayer = mBrickLayerList[i];
				List<BrickPair> currentPairList = mBrickPairList[i];

				// clear the selection (we will select the replaced brick for updating the connectivity)
				currentLayer.clearSelection();

				if (currentPairList.Count > 0)
				{
					// iterate on all the brick pair
					foreach (BrickPair brickPair in currentPairList)
					{
						// get the old and new brick
						Layer.LayerItem oldOne = brickPair.mOldBrick;
                        Layer.LayerItem newOne = brickPair.mNewBrick;
						// reverse them if needed
						if (newByOld)
						{
							oldOne = brickPair.mNewBrick;
							newOne = brickPair.mOldBrick;
						}
						// remove the specified brick from the list of the layer, and memorise its last position
						int brickIndex = removeOneItem(currentLayer, oldOne);
						// then add the new brick at the same position
						addOneItem(currentLayer, newOne, brickIndex);
						// add the brick in the selection for updating connectivity later
						currentLayer.addObjectInSelection(newOne);
					}

					// update the connectivity of the whole layer after replacement
					currentLayer.updateFullBrickConnectivityForSelectedBricksOnly();
					// clear the selection again (it was only use for fast connectivity update)
					currentLayer.clearSelection();
				}

				// If the current layer is the one which has the selection
				// reselect all the new brick
				if (currentLayer == mLayerWhereToSelectTheReplacedBricks)
					foreach (BrickPair brickPair in mReplacedBricksToSelect)
					{
						if (newByOld)
							currentLayer.addObjectInSelection(brickPair.mOldBrick);
						else
							currentLayer.addObjectInSelection(brickPair.mNewBrick);
					}
			}
		}

		public override void redo()
		{
			replace(false);
		}

		public override void undo()
		{
			replace(true);
		}
	}
}
