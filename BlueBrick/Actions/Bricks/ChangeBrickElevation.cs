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

using System.Collections.Generic;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Bricks
{
	class ChangeBrickElevation : Action
	{
		private LayerBrick mLayer = null;
		private List<LayerBrick.Brick> mBricksToEdit = null;
		private List<float> mOldElevationForEachBrick = null;
		private float mNewElevation = 0;
		// for restoring the order in the list we need a list of brick and index sorted by index in increasing order
		struct BriCkAndIndex
		{
			public LayerBrick.Brick mBrick;
			public int mIndex;
		}
		private List<BriCkAndIndex> mBrickIndex = null; // this list of index is for the redo, to add each text at the same place

		public ChangeBrickElevation(LayerBrick layer, List<LayerBrick.Brick> bricksToEdit, float newElevation)
		{
			// save the item list to edit and the other parameters
			mLayer = layer;
			mBricksToEdit = bricksToEdit;
			mNewElevation = newElevation;
			// and memorise the elevation of all the bricks and also its original index (for the undo)
			mOldElevationForEachBrick = new List<float>(bricksToEdit.Count);
			mBrickIndex = new List<BriCkAndIndex>(bricksToEdit.Count);
			foreach (LayerBrick.Brick brick in bricksToEdit)
			{
				mOldElevationForEachBrick.Add(brick.Altitude);
				mBrickIndex.Add(new BriCkAndIndex() { mBrick = brick, mIndex = mLayer.BrickList.IndexOf(brick) });
			}
			// sort the brick index list
			mBrickIndex.Sort((x, y) => x.mIndex - y.mIndex);
		}

		public override string getName()
		{
			return Properties.Resources.ActionChangeBrickElevation.Replace("&", mNewElevation.ToString("F0"));
		}

		public override void redo()
		{
			// assign the same new elevation to all the bricks
			foreach (LayerBrick.Brick brick in mBricksToEdit)
				brick.Altitude = mNewElevation;
			// sort the bricks on the layer
			mLayer.sortBricksByElevation();
		}

		public override void undo()
		{
			// restore the old elevation to all the bricks to edit
			for (int i = 0; i < mBricksToEdit.Count; ++i)
			{
				mBricksToEdit[i].Altitude = mOldElevationForEachBrick[i];
				// remove the brick from the list, we will resinsert it at the previous index
				mLayer.removeBrickWithoutChangingConnectivity(mBricksToEdit[i]);
			}

			// and restore the brick at the right index in the list of bricks
			foreach (BriCkAndIndex briCkAndIndex in mBrickIndex)
				mLayer.addBrickWithoutChangingConnectivity(briCkAndIndex.mBrick, briCkAndIndex.mIndex);
		}
	}
}
