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
		private List<Layer.LayerItem> mTexts = null;
		private List<int> mTextIndex = null; // this list of index is for the redo, to add each text at the same place

		public DuplicateText(LayerText layer, List<Layer.LayerItem> textsToAdd, bool needToAddOffset)
		{
			// init the layer
			mTextLayer = layer;
			// init the index array with -1
			mTextIndex = new List<int>(textsToAdd.Count);
			for (int i = 0; i < textsToAdd.Count; ++i)
				mTextIndex.Add(-1);
			// copy the list, because the pointer may change (specially if it is the selection)
			mTexts = new List<Layer.LayerItem>(textsToAdd.Count);
			foreach (Layer.LayerItem item in textsToAdd)
			{
				// clone the item (because the same list of text to add can be paste several times)
				LayerText.TextCell duplicatedText = (item as LayerText.TextCell).Clone();
				// add an offset if needed
				if (needToAddOffset)
				{
					PointF newPosition = duplicatedText.Position;
					newPosition.X += Properties.Settings.Default.OffsetAfterCopyValue;
					newPosition.Y += Properties.Settings.Default.OffsetAfterCopyValue;
					duplicatedText.Position = newPosition;
				}
				// add the duplicated item in the list
				mTexts.Add(duplicatedText);
			}
		}

		public override string getName()
		{
			if (mTexts.Count == 1)
			{
				string text = (mTexts[0] as LayerText.TextCell).Text.Replace("\r\n", " ");
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
			// clear the selection first, because we want to select only all the added bricks
			mTextLayer.clearSelection();
			// add all the bricks (by default all the brick index are initialized with -1
			// so the first time they are added, we just add them at the end,
			// after the index is record in the array during the undo)
			// We must add all the bricks in the reverse order to avoid crash (insert with an index greater than the size of the list)
			for (int i = mTexts.Count - 1; i >= 0; --i)
			{
				mTextLayer.addTextCell(mTexts[i] as LayerText.TextCell, mTextIndex[i]);
				mTextLayer.addObjectInSelection(mTexts[i]);
			}
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mTextIndex.Clear();
			foreach (Layer.LayerItem obj in mTexts)
				mTextIndex.Add(mTextLayer.removeTextCell(obj as LayerText.TextCell));
		}

		/// <summary>
		/// The duplacate brick action is a bit specific because the position shift of the duplicated
		/// bricks can be updated after the execution of the action. This is due to a combo from the UI.
		/// In the UI of the application by pressing a modifier key + moving the mouse you can duplicate
		/// the selection but also move it at the same moment, but since it is the same action for the user
		/// we don't want to record 2 actions in the undo stack (one for duplicate, another for move)
		/// </summary>
		/// <param name="positionShiftX">the new shift for x coordinate from the position when this action was created</param>
		/// <param name="positionShiftY">the new shift for y coordinate from the position when this action was created</param>
		public override void updatePositionShift(float positionShiftX, float positionShiftY)
		{
			foreach (Layer.LayerItem obj in mTexts)
			{
				LayerText.TextCell text = (obj as LayerText.TextCell);
				PointF newPosition = text.Position;
				newPosition.X += positionShiftX;
				newPosition.Y += positionShiftY;
			}
		}
	}
}
