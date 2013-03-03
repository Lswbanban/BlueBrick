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
using System.Drawing;
using System.Text;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Texts
{
	class AddText : Action
	{
		private LayerText mTextLayer = null;
		private LayerText.TextCell mTextCell = null;
		private int mTextCellIndex = -1; // this index is for the redo, to add the text at the same place, start with -1 to add it at the end of the list (so on top of the other texts)

		public AddText(LayerText layer, string textToAdd, Font font, Color color, StringAlignment alignment, PointF position)
		{
			mTextLayer = layer;
			mTextCell = new LayerText.TextCell(textToAdd, font, color, alignment);
			mTextCell.Position = position;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionAddText;
		}

		public override void redo()
		{
			// and add this text in the list of the layer
			mTextLayer.addTextCell(mTextCell, mTextCellIndex);
			// change the selection to the new added text (should be done after the add)
			mTextLayer.clearSelection();
			mTextLayer.addObjectInSelection(mTextCell);
		}

		public override void undo()
		{
			// remove the specified textCell from the list of the layer,
			// but do not delete it, also memorise its last position
			mTextCellIndex = mTextLayer.removeTextCell(mTextCell);
		}
	}
}
