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
	class DeleteBrick : Action
	{
		private LayerBrick mBrickLayer = null;
		private List<Layer.LayerItem> mBricks = null;
		private List<int> mBrickIndex = null; // this list of index is for the redo, to add each text at the same place
		private string mPartNumber = string.Empty; //if the list contains only one brick or one group, this is the name of this specific brick or group

		public DeleteBrick(LayerBrick layer, List<Layer.LayerItem> bricksToDelete)
		{
			mBrickLayer = layer;
			mBrickIndex = new List<int>(bricksToDelete.Count);
			// copy the list, because the pointer may change (specially if it is the selection)
			mBricks = new List<Layer.LayerItem>(bricksToDelete.Count);
			foreach (Layer.LayerItem obj in bricksToDelete)
				mBricks.Add(obj);

			// try to get a part number (which can be the name of a group)
			Layer.LayerItem topItem = Layer.sGetTopItemFromList(mBricks);
			if (topItem != null)
			{
				if (topItem.IsAGroup)
					mPartNumber = (topItem as Layer.Group).PartNumber;
				else
					mPartNumber = (topItem as LayerBrick.Brick).PartNumber;
			}
		}

		public override string getName()
		{
			// if the part number is valid, use the specific message
			if (mPartNumber != string.Empty)
			{
				string actionName = BlueBrick.Properties.Resources.ActionDeleteBrick;
				actionName = actionName.Replace("&", mPartNumber);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionDeleteSeveralBricks;
			}
		}

		public override void redo()
		{
			// special case for easy editing: if the group of brick has connection points and is connected
			// to bricks not deleted we select the connected brick, 
			// such as the user can press several times on the del button to delete a full line of track.
			LayerBrick.Brick nextBrickToSelect = null;
			foreach (Layer.LayerItem item in mBricks)
			{
				LayerBrick.Brick brick = item as LayerBrick.Brick;
				if (brick.HasConnectionPoint)
					foreach (LayerBrick.Brick.ConnectionPoint connexion in brick.ConnectionPoints)
						if (!connexion.IsFree && !mBricks.Contains(connexion.ConnectionLink.mMyBrick))
						{
							nextBrickToSelect = connexion.ConnectionLink.mMyBrick;
							break;
						}
				if (nextBrickToSelect != null)
					break;
			}

			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mBrickIndex.Clear();
			foreach (Layer.LayerItem obj in mBricks)
				mBrickIndex.Add(mBrickLayer.removeBrick(obj as LayerBrick.Brick));
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);

			// check if we have to select a brick
			if (nextBrickToSelect != null)
			{
				mBrickLayer.clearSelection();
				mBrickLayer.addObjectInSelection(nextBrickToSelect);
			}
		}

		public override void undo()
		{
			// and add all the texts in the reverse order
			for (int i = mBricks.Count - 1; i >= 0; --i)
			{
				mBrickLayer.addBrick(mBricks[i] as LayerBrick.Brick, mBrickIndex[i]);
				// recompute the connexions of each brick, we must do it one by one, else
				// the bricks inside the group deleted will not be connected.
				// the other solution is to perform a full connectivity rebuild outside of this loop
				// but it is not guaranted to be faster.
				mBrickLayer.updateFullBrickConnectivityForOneBrick(mBricks[i] as LayerBrick.Brick);
			}
			// finally reselect all the undeleted brick
			mBrickLayer.clearSelection();
			mBrickLayer.addObjectInSelection(mBricks);
		}
	}
}
