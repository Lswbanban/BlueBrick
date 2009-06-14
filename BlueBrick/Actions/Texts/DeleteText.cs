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
	class DeleteText : Action
	{
		private LayerText mTextLayer = null;
		private List<Layer.LayerItem> mTextCells = null;
		private List<int> mTextCellIndex = null; // this list of index is for the redo, to add each text at the same place

		public DeleteText(LayerText layer, List<Layer.LayerItem> cells)
		{
			mTextLayer = layer;
			mTextCellIndex = new List<int>(cells.Count);
			// copy the list, because the pointer may change (specially if it is the selection)
			mTextCells = new List<Layer.LayerItem>(cells.Count);
			foreach (Layer.LayerItem obj in cells)
				mTextCells.Add(obj);
		}

		public override string getName()
		{
			if (mTextCells.Count == 1)
			{
				string text = (mTextCells[0] as LayerText.TextCell).Text.Replace("\r\n", " ");
				if (text.Length > 10)
					text = text.Substring(0, 10) + "...";
				return BlueBrick.Properties.Resources.ActionDeleteText.Replace("&", text);
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionDeleteSeveralTexts;
			}
		}

		public override void redo()
		{
			// remove the specified textCell from the list of the layer,
			// but do not delete it, also memorise its last position
			mTextCellIndex.Clear();
			foreach (Layer.LayerItem obj in mTextCells)
				mTextCellIndex.Add(mTextLayer.removeTextCell(obj as LayerText.TextCell));
		}

		public override void undo()
		{
			// and add all the texts in the reverse order
			for (int i = mTextCells.Count - 1 ; i >= 0 ; --i)
				mTextLayer.addTextCell(mTextCells[i] as LayerText.TextCell, mTextCellIndex[i]);
		}
	}
}
