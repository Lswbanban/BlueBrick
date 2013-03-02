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
	public abstract class ChangeItemOrder : Action
	{
		public enum FrontOrBack
		{
			FRONT = -1,
			BACK = 0
		}

		protected Layer mLayer = null;
		protected List<Layer.LayerItem> mItems = null;
		protected List<int> mItemIndex = null; // this list of index is for the redo, to add each text at the same place
		private int mInsertPosition = 0;

		public ChangeItemOrder(Layer layer, List<Layer.LayerItem> itemsToMove, FrontOrBack whereToSend)
		{
			// init the layer
			mLayer = layer;
			mInsertPosition = (int)whereToSend;
			// init the index array
			mItemIndex = new List<int>(itemsToMove.Count);
			// copy the list, because the pointer may change (specially if it is the selection)
			// but we don't duplicate the texts themselves
			mItems = new List<Layer.LayerItem>(itemsToMove.Count);
			foreach (Layer.LayerItem item in itemsToMove)
				mItems.Add(item);
		}

		protected abstract int removeItem(Layer.LayerItem item);

		protected abstract void addItem(Layer.LayerItem item, int position);

		public override void redo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mItemIndex.Clear();
			foreach (Layer.LayerItem item in mItems)
				mItemIndex.Add(this.removeItem(item));

			// add all the bricks at the begining of the list (so the back = 0) or at the end of the list (so at the front -1)
			foreach (Layer.LayerItem item in mItems)
				this.addItem(item, mInsertPosition);

			// reselect all the moved brick
			mLayer.clearSelection();
			mLayer.addObjectInSelection(mItems);
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer (they must be at the end
			// of the list but we don't care
			foreach (Layer.LayerItem item in mItems)
				this.removeItem(item);

			// add all the bricks at their previous positions
			// We must add all the bricks in the reverse order to avoid crash (insert with an index greater than the size of the list)
			for (int i = mItems.Count - 1; i >= 0; --i)
				this.addItem(mItems[i], mItemIndex[i]);

			// reselect all the moved brick
			mLayer.clearSelection();
			mLayer.addObjectInSelection(mItems);
		}
	}
}
