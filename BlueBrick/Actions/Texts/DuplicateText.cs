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

namespace BlueBrick.Actions.Texts
{
	class DuplicateText : Items.DuplicateItems
	{
		private LayerText mTextLayer = null;

		public DuplicateText(LayerText layer, List<Layer.LayerItem> itemsToDuplicate, bool needToAddOffset)
			: base(itemsToDuplicate, needToAddOffset, false)
		{
			// init the layer
			mTextLayer = layer;
		}

		public override string getName()
		{
			if (mItems.Count == 1)
			{
				string text = (mItems[0] as LayerText.TextCell).Text.Replace("\r\n", " ");
				if (text.Length > 10)
					text = text.Substring(0, 10) + "...";
				return BlueBrick.Properties.Resources.ActionDuplicateText.Replace("&", text);
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionDuplicateSeveralTexts;
			}
		}

		public override void redo()
		{
			// clear the selection first, because we want to select only all the added texts
			mTextLayer.clearSelection();
			// add all the texts (by default all the text index are initialized with -1
			// so the first time they are added, we just add them at the end,
			// after the index is record in the array during the undo)
			// We must add all the texts in the reverse order to avoid crash (insert with an index greater than the size of the list)
			for (int i = mItems.Count - 1; i >= 0; --i)
			{
				mTextLayer.addTextCell(mItems[i] as LayerText.TextCell, mItemIndex[i]);
				mTextLayer.addObjectInSelection(mItems[i]);
			}
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mItemIndex.Clear();
			foreach (Layer.LayerItem obj in mItems)
				mItemIndex.Add(mTextLayer.removeTextCell(obj as LayerText.TextCell));
		}
	}
}
