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

		public DeleteBrick(LayerBrick layer, List<Layer.LayerItem> bricksToDelete)
		{
			mBrickLayer = layer;
			mBrickIndex = new List<int>(bricksToDelete.Count);
			// copy the list, because the pointer may change (specially if it is the selection)
			mBricks = new List<Layer.LayerItem>(bricksToDelete.Count);
			foreach (Layer.LayerItem obj in bricksToDelete)
				mBricks.Add(obj);
		}

		public override string getName()
		{
			if (mBricks.Count == 1)
			{
				string actionName = BlueBrick.Properties.Resources.ActionDeleteBrick;
				actionName = actionName.Replace("&", (mBricks[0] as LayerBrick.Brick).PartNumber);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionDeleteSeveralBricks;
			}
		}

		public override void redo()
		{
			// special case for easy editing: if the delete brick is alone, and if this brick has connection points
			// we select the connected brick, such as the user can press several times on the del button
			// to delete a full line of track
			LayerBrick.Brick nextBrickToSelect = null;
			if (mBricks.Count == 1)
			{
				LayerBrick.Brick deletedBrick = (mBricks[0] as LayerBrick.Brick);
				if (deletedBrick.HasConnectionPoint)
					foreach (LayerBrick.Brick.ConnectionPoint connexion in deletedBrick.ConnectionPoints)
						if (!connexion.IsFree)
						{
							nextBrickToSelect = connexion.ConnectionLink.mMyBrick;
							break;
						}
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
				// clear the selection to select only current the brick undeleted,
				// such as we can recompute its connexions, we must do it one by one, else
				// the bricks inside the group deleted will not be connected.
				// the other solution is to perform a full connectivity rebuild outside of this loop
				// but it is not guaranted to be faster.
				mBrickLayer.clearSelection();
				mBrickLayer.addObjectInSelection(mBricks[i]);
				// update the connectivity of the bricks (call only one time outside of the loop)
				mBrickLayer.updateBrickConnectivityOfSelection(false);
			}
			// finally reselect all the undeleted brick
			mBrickLayer.clearSelection();
			mBrickLayer.addObjectInSelection(mBricks);
		}
	}
}
