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
using System.Xml;
using System.Drawing;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Items
{
	public abstract class DuplicateItems : Action
	{
		protected List<Layer.LayerItem> mItems = null;
		protected List<int> mItemIndex = null; // this list of index is for the redo, to add each text at the same place

		public DuplicateItems(List<Layer.LayerItem> itemsToDuplicate, bool needToAddOffset, bool addGroupsInItemList)
		{
			// init the index array with -1
			mItemIndex = new List<int>(itemsToDuplicate.Count);
			for (int i = 0; i < itemsToDuplicate.Count; ++i)
				mItemIndex.Add(-1);

			// copy the list, because the pointer may change (specially if it is the selection)
			mItems = cloneItemList(itemsToDuplicate, addGroupsInItemList);

			// add an offset if needed
			if (needToAddOffset)
				foreach (Layer.LayerItem duplicatedItem in mItems)
				{
					PointF newPosition = duplicatedItem.Position;
					newPosition.X += Properties.Settings.Default.OffsetAfterCopyValue;
					newPosition.Y += Properties.Settings.Default.OffsetAfterCopyValue;
					duplicatedItem.Position = newPosition;
				}
		}

		/// <summary>
		/// This tool method clones all the item of the specified list into a new list.
		/// This method also clone the groups that may belong to this list of bricks.
		/// The cloned items are in the same order as the original list
		/// </summary>
		/// <param name="listToClone">The original list of brick to copy</param>
		/// <param name="addGroupsInItemList">if this parameter is true, the groups are also added in the Items list</param>
		/// <returns>A clone list of cloned brick with there cloned groups</returns>
		protected List<Layer.LayerItem> cloneItemList(List<Layer.LayerItem> listToClone, bool addGroupsInItemList)
		{
			// the resulting list
			List<Layer.LayerItem> result = new List<Layer.LayerItem>(listToClone.Count);

			// use a dictionnary to recreate the groups that may be inside the list of brick to duplicate
			// this dictionnary makes an association between the group to duplicate and the new duplicated one
			Dictionary<Layer.Group, Layer.Group> groupsToCreate = new Dictionary<Layer.Group, Layer.Group>();
			// also use a list of item that we will make grow to create all the groups
			List<Layer.LayerItem> fullOriginalItemList = new List<Layer.LayerItem>(listToClone);

			// use a for instead of a foreach because the list will grow
			for (int i = 0; i < fullOriginalItemList.Count; ++i)
			{
				// get the current item
				Layer.LayerItem originalItem = fullOriginalItemList[i];
				Layer.LayerItem duplicatedItem = null;

				// check if the item is a group or a brick
				if (originalItem.IsAGroup)
				{
					// if the item is a group that means the list already grown, and that means we also have it in the dictionnary
					Layer.Group associatedGroup = null;
					groupsToCreate.TryGetValue(originalItem as Layer.Group, out associatedGroup);
					duplicatedItem = associatedGroup;
					// check if we also need to add the group
					if (addGroupsInItemList)
						result.Add(duplicatedItem);
				}
				else
				{
					// if the item is a brick, just clone it and add it to the result
					// clone the item (because the same list of text to add can be paste several times)
					duplicatedItem = originalItem.Clone();
					// add the duplicated item in the list
					result.Add(duplicatedItem);
				}

				// check if the item to clone belongs to a group then also duplicate the group
				if (originalItem.Group != null)
				{
					// get the duplicated group if already created otherwise create it and add it in the dictionary
					Layer.Group duplicatedGroup = null;
					groupsToCreate.TryGetValue(originalItem.Group, out duplicatedGroup);
					if (duplicatedGroup == null)
					{
						duplicatedGroup = new Layer.Group(originalItem.Group);
						groupsToCreate.Add(originalItem.Group, duplicatedGroup);
						fullOriginalItemList.Add(originalItem.Group);
					}
					// assign the group to the brick
					duplicatedGroup.addItem(duplicatedItem);
					// check if we need to also assign the brick that hold the connection point
					if (originalItem.Group.BrickThatHoldsActiveConnection == originalItem)
						duplicatedGroup.BrickThatHoldsActiveConnection = (duplicatedItem as LayerBrick.Brick);
				}
			}

			// delete the dictionary
			groupsToCreate.Clear();
			fullOriginalItemList.Clear();
			// return the cloned list
			return result;
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
		public virtual void updatePositionShift(float positionShiftX, float positionShiftY)
		{
			foreach (Layer.LayerItem item in mItems)
			{
				PointF newPosition = item.Position;
				newPosition.X += positionShiftX;
				newPosition.Y += positionShiftY;
			}
		}
	}
}
