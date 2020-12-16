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

namespace BlueBrick.Actions.Items
{
	public abstract class MoveItems : Action
	{
		protected Layer mLayer = null;
		protected List<Layer.LayerItem> mItems = null;
		protected PointF mMove;	// in Stud coord

		public MoveItems(Layer layer, List<Layer.LayerItem> items, PointF move)
		{
			mLayer = layer;
			mMove = move;
			// copy the list, because the pointer may change (specially if it is the selection)
			// To do that call the function that gives all the top items of the list (brick or group),
			// this function will return a newly created list.
			// Also moving the top group, will also move all the children items
			mItems = Layer.sGetTopItemListFromList(items);
			// however this function can return null, if the list of item is empty, so create an empty list instead
			if (mItems == null)
				mItems = new List<Layer.LayerItem>();
		}

		public override void redo()
		{
			// move all the items (if the item is a group, the whole items of the group will be recursively moved)
			foreach (Layer.LayerItem item in mItems)
				item.Position = new PointF(item.Position.X + mMove.X, item.Position.Y + mMove.Y);

			// update the bounding rectangle
			mLayer.updateBoundingSelectionRectangle();
		}

		public override void undo()
		{
			// move all the items (if the item is a group, the whole items of the group will be recursively moved)
			foreach (Layer.LayerItem item in mItems)
				item.Position = new PointF(item.Position.X - mMove.X, item.Position.Y - mMove.Y);

			// update the bounding rectangle
			mLayer.updateBoundingSelectionRectangle();
		}
	}
}
