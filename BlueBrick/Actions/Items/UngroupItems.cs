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
	class UngroupItems : Action
	{
		List<Layer.Group> mGroupToUngroup = null;
		Layer mLayer = null;

        public static List<Layer.Group> findItemsToUngroup(List<Layer.LayerItem> itemsToUngroup)
        {
            // create a search list that we will expend and to keep the original selection intact
            List<Layer.LayerItem> searchList = new List<Layer.LayerItem>(itemsToUngroup);
            List<Layer.Group> result = new List<Layer.Group>(itemsToUngroup.Count);

            // Search the top group of the tree, because this action only ungroup the top of the tree
            // The list of items to ungroup can also be a forest, so keep all the top of the trees
            for (int i = 0; i < searchList.Count; ++i)
            {
                Layer.LayerItem item = searchList[i];
                if (item.Group == null)
                {
                    // check if it is a group or a simple item
                    Layer.Group group = item as Layer.Group;
                    if ((group != null) && !result.Contains(group))
                        result.Add(group);
                }
                else if (!searchList.Contains(item.Group))
                {
                    searchList.Add(item.Group);
                }
            }

            return result;
        }

		public UngroupItems(List<Layer.LayerItem> itemsToUngroup, Layer layer)
		{
            mGroupToUngroup = findItemsToUngroup(itemsToUngroup);
			mLayer = layer;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionUngroupItems;
		}

		public override void redo()
		{
			// disband the groups
			foreach (Layer.Group group in mGroupToUngroup)
				group.ungroup();
		}

		public override void undo()
		{
			// reform the groups
			foreach (Layer.Group group in mGroupToUngroup)
				group.regroup();

			// reselect the current selection, because the regroup may affect a currently selected item
			// and if we don't reselect, we may end up with some items of the same group selected and some
			// not selected. So by reselecting the current selection, the grouping links will ensure to have
			// a correct status.
			List<Layer.LayerItem> currentSelection = new List<Layer.LayerItem>(mLayer.SelectedObjects);
			mLayer.clearSelection();
			mLayer.addObjectInSelection(currentSelection);
		}
	}
}
