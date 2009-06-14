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

namespace BlueBrick.Actions.Texts
{
	class BringTextToFront : Action
	{
		private LayerText mTextLayer = null;
		private List<Layer.LayerItem> mTexts = null;
		private List<int> mTextIndex = null; // this list of index is for the redo, to add each text at the same place

		public BringTextToFront(LayerText layer, List<Layer.LayerItem> textsToMove)
		{
			// init the layer
			mTextLayer = layer;
			// init the index array
			mTextIndex = new List<int>(textsToMove.Count);
			// copy the list, because the pointer may change (specially if it is the selection)
			// but we don't duplicate the texts themselves
			mTexts = new List<Layer.LayerItem>(textsToMove.Count);
			foreach (Layer.LayerItem item in textsToMove)
				mTexts.Add(item);
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionBringTextToFront;
		}

		public override void redo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mTextIndex.Clear();
			foreach (Layer.LayerItem obj in mTexts)
				mTextIndex.Add(mTextLayer.removeTextCell(obj as LayerText.TextCell));

			// add all the bricks at the end
			foreach (Layer.LayerItem obj in mTexts)
				mTextLayer.addTextCell(obj as LayerText.TextCell, -1);

			// reselect all the moved brick
			mTextLayer.clearSelection();
			foreach (LayerText.TextCell text in mTexts)
				mTextLayer.addObjectInSelection(text);
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer (they must be at the end
			// of the list but we don't care
			foreach (Layer.LayerItem obj in mTexts)
				mTextLayer.removeTextCell(obj as LayerText.TextCell);

			// add all the bricks at the end
			// We must add all the bricks in the reverse order to avoid crash (insert with an index greater than the size of the list)
			for (int i = mTexts.Count - 1; i >= 0; --i)
				mTextLayer.addTextCell(mTexts[i] as LayerText.TextCell, mTextIndex[i]);

			// reselect all the moved brick
			mTextLayer.clearSelection();
			foreach (LayerText.TextCell text in mTexts)
				mTextLayer.addObjectInSelection(text);
		}
	}
}
