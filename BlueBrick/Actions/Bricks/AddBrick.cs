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
using System.Drawing;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Bricks
{
	class AddBrick : Action
	{
		private LayerBrick mBrickLayer = null;
		private Layer.LayerItem mBrickOrGroup = null;
		private List<Layer.LayerItem> mBricks = null;
		private List<int> mBrickIndex = null; // this list of index is for the redo, to add each text at the same place

		public AddBrick(LayerBrick layer, string partNumber)
		{
			mBrickLayer = layer;
			if (BrickLibrary.Instance.isAGroup(partNumber))
				mBrickOrGroup = new Layer.Group(partNumber);
			else
				mBrickOrGroup = new LayerBrick.Brick(partNumber);
			// after setting the brick or group, call the init brick list function
			initBrickList();
		}

		public AddBrick(LayerBrick layer, Layer.LayerItem brickOrGroup)
		{
			mBrickLayer = layer;
			mBrickOrGroup = brickOrGroup;
			// after setting the brick or group, call the init brick list function
			initBrickList();
		}

		private void initBrickList()
		{
			// init the brick to add list
			if (mBrickOrGroup.IsAGroup)
			{
				mBricks = (mBrickOrGroup as Layer.Group).getAllLeafItems();
				// since the bricks are added from the end in the redo method,
				// we reverse the order of the group, such as a group from the library can
				// be inserted in the correct order.
				mBricks.Reverse();
			}
			else
			{
				mBricks = new List<Layer.LayerItem>(1);
				mBricks.Add(mBrickOrGroup);
			}

			// init the index list with the same size and fill it with -1
			mBrickIndex = new List<int>(mBricks.Count);
			for (int i = 0; i < mBricks.Count; ++i)
				mBrickIndex.Add(-1);
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionAddBrick;
			actionName = actionName.Replace("&", mBrickOrGroup.PartNumber);
			return actionName;
		}

		public override void redo()
		{
			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickAdded(mBrickLayer, mBrickOrGroup);

			// and add all the bricks in the reverse order
			for (int i = mBricks.Count - 1; i >= 0; --i)
			{
				mBrickLayer.addBrick(mBricks[i] as LayerBrick.Brick, mBrickIndex[i]);
				// update the connectivity of each brick, in order to also create connections
				// between bricks inside the same group, otherwise if we update the connectivity of
				// the selection, the connectivity inside the selection will not change
				mBrickLayer.updateFullBrickConnectivityForOneBrick(mBricks[i] as LayerBrick.Brick);
			}
			// finally reselect all the undeleted brick
			mBrickLayer.selectOnlyThisObject(mBricks);
		}

		public override void undo()
		{
			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickRemoved(mBrickLayer, mBrickOrGroup);

			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mBrickIndex.Clear();
			foreach (Layer.LayerItem obj in mBricks)
				mBrickIndex.Add(mBrickLayer.removeBrick(obj as LayerBrick.Brick));
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
