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

namespace BlueBrick.Actions.Rulers
{
	class DuplicateRuler : Items.DuplicateItems
	{
		private LayerRuler mRulerLayer = null;

		public DuplicateRuler(LayerRuler layer, List<Layer.LayerItem> itemsToDuplicate, bool needToAddOffset)
			: base(itemsToDuplicate, needToAddOffset, false)
		{
			// init the layer
			mRulerLayer = layer;
		}

		public override string getName()
		{
			if (mItems.Count == 1)
				return BlueBrick.Properties.Resources.ActionDuplicateRuler;
			else
				return BlueBrick.Properties.Resources.ActionDuplicateSeveralRulers;
		}

		public override void redo()
		{
			// clear the selection first, because we want to select only all the added rulers
			mRulerLayer.clearSelection();
			// add all the rulers (by default all the rulers index are initialized with -1
			// so the first time they are added, we just add them at the end,
			// after the index is record in the array during the undo)
			// We must add all the rulers in the reverse order to avoid crash (insert with an index greater than the size of the list)
			for (int i = mItems.Count - 1; i >= 0; --i)
			{
				mRulerLayer.addRulerItem(mItems[i] as LayerRuler.RulerItem, mItemIndex[i]);
				mRulerLayer.addObjectInSelection(mItems[i]);
			}
		}

		public override void undo()
		{
			// remove the specified ruler from the list of the layer,
			// but do not delete it, also memorise its last position
			mItemIndex.Clear();
			foreach (Layer.LayerItem obj in mItems)
				mItemIndex.Add(mRulerLayer.removeRulerItem(obj as LayerRuler.RulerItem));
		}
	}
}
