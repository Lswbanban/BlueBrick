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
			public LayerBrick.Brick mOldBrick = null;
			public LayerBrick.Brick mNewBrick = null;

			public BrickPair(LayerBrick.Brick oldBrick, LayerBrick.Brick newBrick)
			{
				mOldBrick = oldBrick;
				mNewBrick = newBrick;
			}
		}

		private List<LayerBrick> mBrickLayerList = null;
		private List<List<BrickPair>> mBrickPairList = null;
		private string mPartNumberToReplace = string.Empty;

		public ReplaceBrick(List<LayerBrick> layerList, string partNumberToReplace, string newPartNumber, bool replaceInSelectionOnly)
		{
			// store the list of layer for which this action apply and create a list of the same size for the bricks
			mBrickLayerList = layerList;
			mPartNumberToReplace = partNumberToReplace;
			mBrickPairList = new List<List<BrickPair>>(layerList.Count);
			// iterate on all the layers
			foreach (LayerBrick layer in layerList)
			{
				// add the list of brick pair (that can stay empty if no brick is found in that layer)
				List<BrickPair> currentPairList = new List<BrickPair>();
				mBrickPairList.Add(currentPairList);
				// iterate on all the bricks of the layer or on the selection to find the brick to replace
				if (replaceInSelectionOnly)
				{
					foreach (Layer.LayerItem item in layer.SelectedObjects)
						createBrickPairIfNeeded(currentPairList, (item as LayerBrick.Brick), newPartNumber);
				}
				else
				{
					foreach (LayerBrick.Brick brick in layer.BrickList)
						createBrickPairIfNeeded(currentPairList, brick, newPartNumber);
				}
			}
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionReplaceBrick;
			actionName = actionName.Replace("&", mPartNumberToReplace);
			return actionName;
		}

		private void createBrickPairIfNeeded(List<BrickPair> currentPairList, LayerBrick.Brick brick, string newPartNumber)
		{
			if (brick.PartNumber.Equals(mPartNumberToReplace))
			{
				// create a new brick and copy all the paramters of the old one
				LayerBrick.Brick newBrick = new LayerBrick.Brick(newPartNumber);
				newBrick.Orientation = brick.Orientation;
				newBrick.Center = brick.Center;
				newBrick.Altitude = brick.Altitude;
				// create the pair and add it to the list
				BrickPair brickPair = new BrickPair(brick, newBrick);
				currentPairList.Add(brickPair);
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
						LayerBrick.Brick oldOne = brickPair.mOldBrick;
						LayerBrick.Brick newOne = brickPair.mNewBrick;
						// reverse them if needed
						if (newByOld)
						{
							oldOne = brickPair.mNewBrick;
							newOne = brickPair.mOldBrick;
						}
						// remove the specified brick from the list of the layer, and memorise its last position
						int brickIndex = currentLayer.removeBrick(oldOne);
						// then add the new brick at the same position
						currentLayer.addBrick(newOne, brickIndex);
						// add the brick in the selection for updating connectivity later
						currentLayer.addObjectInSelection(newOne);
					}

					// update the connectivity of the whole layer after replacement
					currentLayer.updateFullBrickConnectivityForSelectedBricksOnly();
					// clear the selection again (it was only use for fast connectivity update)
					currentLayer.clearSelection();
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
