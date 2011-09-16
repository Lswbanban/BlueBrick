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

namespace BlueBrick.Actions.Items
{
	class GroupItems : Action
	{
		List<Layer.LayerItem> mItemsToGroup = null;
		Layer.Group mGroup = new Layer.Group();
        Layer mLayer = null;

        /// <summary>
        /// From a list of items to group, find and construct the top items of the forest, such as
        /// you can only group the top items and get a hierarchical tree.
        /// </summary>
        /// <param name="itemsToGroup">A list of items (usually not group) that you want to group</param>
        /// <returns>A list of top tree items (that can be group) that can be put inside a group</returns>
        static public List<Layer.LayerItem> findItemsToGroup(List<Layer.LayerItem> itemsToGroup)
        {
			// create a search list that we will expend and to keep the original selection intact
			List<Layer.LayerItem> searchList = new List<Layer.LayerItem>(itemsToGroup);
            List<Layer.LayerItem> result = new List<Layer.LayerItem>(itemsToGroup.Count);

			// we cannot use a foreach keyword here because it through an exception when
			// the list is modified during the iteration, which is exactly what I want to do
			for (int i = 0; i < searchList.Count; ++i)
			{
				Layer.LayerItem item = searchList[i];
                if (!result.Contains(item))
				{
					if (item.Group == null)
                        result.Add(item);
					else if (!searchList.Contains(item.Group))
						searchList.Add(item.Group);
				}
			}

            return result;
        }

        public GroupItems(List<Layer.LayerItem> itemsToGroup, Layer layer)
		{
            // save the item list but don't add them in the group in the constructor.
            // we do that in the redo. And we only group the top items of the tree.
            mItemsToGroup = findItemsToGroup(itemsToGroup);
            mLayer = layer;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionGroupItems;
		}

		public override void redo()
		{
			// add all the items in the group
			mGroup.addItem(mItemsToGroup);

            // reselect the current selection, because the regroup may affect a currently selected item
            // and if we don't reselect, we may end up with some items of the same group selected and some
            // not selected. So by reselecting the current selection, the grouping links will ensure to have
            // a correct status.
            List<Layer.LayerItem> currentSelection = new List<Layer.LayerItem>(mLayer.SelectedObjects);
            mLayer.clearSelection();
            mLayer.addObjectInSelection(currentSelection);
		}

		public override void undo()
		{
			// remove all the items from the group
			mGroup.removeItem(mItemsToGroup);
		}
	}
}
