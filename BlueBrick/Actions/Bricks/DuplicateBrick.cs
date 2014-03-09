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
	class DuplicateBrick : Items.DuplicateItems
	{
		private LayerBrick mBrickLayer = null;
		private string mPartNumber = string.Empty; //if the list contains only one brick or one group, this is the name of this specific brick or group
		private List<Layer.LayerItem> mBricksForNotification = null;
		private bool mWereItemsTrimmed = false;

		/// <summary>
		/// Tell if some items have been trimmed from the specified list of item to duplicate
		/// </summary>
		public bool WereItemsTrimmed
		{
			get { return mWereItemsTrimmed; }
		}

		public DuplicateBrick(LayerBrick layer, List<Layer.LayerItem> bricksToDuplicate, bool needToAddOffset)
			: base(bricksToDuplicate, needToAddOffset, Budget.Budget.Instance.ShouldUseBudgetLimitation)
		{
			// init the layer
			mBrickLayer = layer;

			// elagate the list according to the budget limit
			mWereItemsTrimmed = trimItemListWithBudgetLimitation();

			// get bricks for the notification from the trimmed list
			mBricksForNotification = Layer.sFilterListToGetOnlyBricksInLibrary(mItems);

			// remove the group that were added to the item list for brick notification purpose
			// they are added at the end, so find the first one and erase the end
			if (Budget.Budget.Instance.ShouldUseBudgetLimitation)
				for (int i = 0; i < mItems.Count; ++i)
					if (mItems[i].IsAGroup)
					{
						mItems.RemoveRange(i, mItems.Count - i);
						break;
					}

			// try to get a part number (which can be the name of a group)
			Layer.LayerItem topItem = Layer.sGetTopItemFromList(mItems);
			if (topItem != null)
				mPartNumber = topItem.PartNumber;
		}

		public override string getName()
		{
			// if the part number is valid, use the specific message
			if (mPartNumber != string.Empty)
			{
				string actionName = BlueBrick.Properties.Resources.ActionDuplicateBrick;
				actionName = actionName.Replace("&", mPartNumber);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionDuplicateSeveralBricks;
			}
		}

		/// <summary>
		/// The item list to duplicate has been created in the constructor of the base class wich is common
		/// to all item list. Now for the bricks only, we need to check if some bricks have reach the limit
		/// from the Budget, and remove them from the list to duplicate.
		/// <returns>true if some items have been trimmed</returns>
		/// </summary>
		private bool trimItemListWithBudgetLimitation()
		{
			// first check if the budget limitation is enabled
			if (!Budget.Budget.Instance.ShouldUseBudgetLimitation)
				return false;

			// use a temporary dictionnary to count the number of similar items the user wants to add
			Dictionary<string, int> itemCount = new Dictionary<string, int>();

			// use a temporary list of items to remove
			List<Layer.LayerItem> itemToRemove = new List<Layer.LayerItem>(mItems.Count);

			// iterate on all the items of the list to find the one to remove
			foreach (Layer.LayerItem item in mItems)
				if (item.PartNumber != string.Empty)
				{
					// check if we already met this part
					int count = 0;
					if (itemCount.TryGetValue(item.PartNumber, out count))
						itemCount.Remove(item.PartNumber);
					// increase and add the count
					itemCount.Add(item.PartNumber, ++count);
					// check if we can add it
					if (!Budget.Budget.Instance.canAddBrick(item.PartNumber, count))
					{
						// checked if this item is a group, in that case, we need to remove all the hierachy
						if (item.IsAGroup)
							itemToRemove.AddRange((item as Layer.Group).getAllItemsInTheTree());
						else
							itemToRemove.Add(item);
					}
				}

			// then remove all the items
			foreach (Layer.LayerItem item in itemToRemove)
				mItems.Remove(item);

			// beep if we removed some items and return true
			if (itemToRemove.Count > 0)
			{
				Map.Instance.giveFeedbackForNotAddingBrick();
				return true;
			}
			return false;
		}

		public override void redo()
		{
			// notify the part list view
			foreach (Layer.LayerItem item in mBricksForNotification)
				MainForm.Instance.NotifyPartListForBrickAdded(mBrickLayer, item);

			// add all the bricks (by default all the brick index are initialized with -1
			// so the first time they are added, we just add them at the end,
			// after the index is record in the array during the undo)
			// We must add all the bricks in the reverse order to avoid crash (insert with an index greater than the size of the list)
			for (int i = mItems.Count - 1; i >= 0; --i)
			{
				mBrickLayer.addBrick(mItems[i] as LayerBrick.Brick, mItemIndex[i]);
				// recompute the connexions of each brick, we must do it one by one, else
				// the bricks inside the group deleted will not be connected.
				// the other solution is to perform a full connectivity rebuild outside of this loop
				// but it is not guaranted to be faster.
				mBrickLayer.updateFullBrickConnectivityForOneBrick(mItems[i] as LayerBrick.Brick);
			}
			// finally reselect all the duplicated brick
			mBrickLayer.clearSelection();
			mBrickLayer.addObjectInSelection(mItems);
		}

		public override void undo()
		{
			// notify the part list view
			foreach (Layer.LayerItem item in mBricksForNotification)
				MainForm.Instance.NotifyPartListForBrickRemoved(mBrickLayer, item);

			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mItemIndex.Clear();
			foreach (Layer.LayerItem obj in mItems)
				mItemIndex.Add(mBrickLayer.removeBrick(obj as LayerBrick.Brick));
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
		public override void updatePositionShift(float positionShiftX, float positionShiftY)
		{
			// call the base class for the shift
			base.updatePositionShift(positionShiftX, positionShiftY);

			// update the connectivity of the bricks (call only one time outside of the loop)
			mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
