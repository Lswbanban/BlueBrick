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

namespace BlueBrick.Actions.Texts
{
	class MoveText : Action
	{
		private LayerText mTextLayer = null;
		private List<Layer.LayerItem> mTextCells = null;
		private PointF mMove;	// in Stud coord

		public MoveText(LayerText layer, List<Layer.LayerItem> cells, PointF move)
		{
			mTextLayer = layer;
			mMove = move;
			// copy the list, because the pointer may change (specially if it is the selection)
			mTextCells = new List<Layer.LayerItem>(cells.Count);
			foreach (Layer.LayerItem obj in cells)
				mTextCells.Add(obj);
		}

		public override string getName()
		{
			if (mTextCells.Count == 1)
			{
				string actionName = BlueBrick.Properties.Resources.ActionMoveText;
				string text = (mTextCells[0] as LayerText.TextCell).Text.Replace("\r\n", " ");
				if (text.Length > 10)
					text = text.Substring(0, 10) + "...";
				actionName = actionName.Replace("&", text);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionMoveSeveralTexts;
			}
		}

		public override void redo()
		{
			foreach (Layer.LayerItem obj in mTextCells)
			{
				LayerText.TextCell cell = obj as LayerText.TextCell;
				cell.Position = new PointF(cell.Position.X + mMove.X, cell.Position.Y + mMove.Y);
			}
			// update the bounding rectangle
			mTextLayer.updateBoundingSelectionRectangle();
		}

		public override void undo()
		{
			foreach (Layer.LayerItem obj in mTextCells)
			{
				LayerText.TextCell cell = obj as LayerText.TextCell;
				cell.Position = new PointF(cell.Position.X - mMove.X, cell.Position.Y - mMove.Y);
			}
			// update the bounding rectangle
			mTextLayer.updateBoundingSelectionRectangle();
		}
	}
}
