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
using System.Drawing;

namespace BlueBrick.Actions.Bricks
{
	class DuplicateBrick : Action
	{
		private LayerBrick mBrickLayer = null;
		private List<Layer.LayerItem> mBricks = null;
		private List<int> mBrickIndex = null; // this list of index is for the redo, to add each text at the same place

		public DuplicateBrick(LayerBrick layer, List<Layer.LayerItem> bricksToDuplicate)
		{
			// init the layer
			mBrickLayer = layer;
			// init the index array with -1
			mBrickIndex = new List<int>(bricksToDuplicate.Count);
			for (int i = 0; i < bricksToDuplicate.Count; ++i)
				mBrickIndex.Add(-1);
			// copy the list, because the pointer may change (specially if it is the selection)
			// and we also need to duplicate the bricks themselves
			mBricks = new List<Layer.LayerItem>(bricksToDuplicate.Count);
			foreach (Layer.LayerItem item in bricksToDuplicate)
			{
				// clone the item (because the same list of text to add can be paste several times)
				LayerBrick.Brick duplicatedBrick = (item as LayerBrick.Brick).Clone();
				// add the duplicated item in the list
				mBricks.Add(duplicatedBrick);
			}
		}

		public override string getName()
		{
			if (mBricks.Count == 1)
			{
				string actionName = BlueBrick.Properties.Resources.ActionDuplicateBrick;
				actionName = actionName.Replace("&", (mBricks[0] as LayerBrick.Brick).PartNumber);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionDuplicateSeveralBricks;
			}
		}

		public override void redo()
		{
			// add all the bricks (by default all the brick index are initialized with -1
			// so the first time they are added, we just add them at the end,
			// after the index is record in the array during the undo)
			// We must add all the bricks in the reverse order to avoid crash (insert with an index greater than the size of the list)
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
			// finally reselect all the duplicated brick
			mBrickLayer.clearSelection();
			mBrickLayer.addObjectInSelection(mBricks);
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mBrickIndex.Clear();
			foreach (Layer.LayerItem obj in mBricks)
				mBrickIndex.Add(mBrickLayer.removeBrick(obj as LayerBrick.Brick));
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);
		}

		/// <summary>
		/// The duplacate brick action is a bit specific because the position shift of the duplicated
		/// bricks can be updated after the execution of the action. This is due to a combo from the UI.
		/// In the UI of the application by pressing a modifier key + moving the mouse you can duplicate
		/// the selection but also move it at the same moment, but since it is the same action for the user
		/// we don't want to record 2 actions in the undo stack (one for duplicate, another for move)
		/// </summary>
		/// <param name="positionShiftX">the new shift for x coordinate from the position when this action was created</param>
		/// <param name="positionShiftY">the new shift for y coordinate from the position when this action was created</param>
		public void updatePositionShift(float positionShiftX, float positionShiftY)
		{
			foreach (Layer.LayerItem obj in mBricks)
			{
				LayerBrick.Brick brick = (obj as LayerBrick.Brick);
				PointF newPosition = brick.Position;
				newPosition.X += positionShiftX;
				newPosition.Y += positionShiftY;
			}
			// update the connectivity of the bricks (call only one time outside of the loop)
			mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
