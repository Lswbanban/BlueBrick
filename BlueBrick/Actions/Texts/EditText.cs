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
	class EditText : Action
	{
		private LayerText mTextLayer = null;
		private LayerText.TextCell mTextCell = null;
		private string mOldText = null;
		private string mNewText = null;
		private Font mOldFont = null;
		private Font mNewFont = null;
		private Color mOldColor;
		private Color mNewColor;
		private StringAlignment mOldAlignment;
		private StringAlignment mNewAlignment;

		public EditText(LayerText layer, LayerText.TextCell cellToEdit, string newText, Font newFont, Color newColor, StringAlignment newAlignment)
		{
			mTextLayer = layer;
			mTextCell = cellToEdit;
			mOldText = cellToEdit.Text;
			mNewText = newText;
			mOldFont = cellToEdit.Font;
			mNewFont = newFont;
			mOldColor = cellToEdit.FontColor;
			mNewColor = newColor;
			mOldAlignment = cellToEdit.TextAlignment;
			mNewAlignment = newAlignment;
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionEditText;
			string text = mTextCell.Text.Replace("\r\n", " ");
			if (text.Length > 10)
				text = text.Substring(0, 10) + "...";
			actionName = actionName.Replace("&", text);
			return actionName;
		}

		public override void redo()
		{
			mTextCell.Text = mNewText;
			mTextCell.Font = mNewFont;
			mTextCell.FontColor = mNewColor;
			mTextCell.TextAlignment = mNewAlignment;
			mTextLayer.updateBoundingSelectionRectangle();
		}

		public override void undo()
		{
			mTextCell.Text = mOldText;
			mTextCell.Font = mOldFont;
			mTextCell.FontColor = mOldColor;
			mTextCell.TextAlignment = mOldAlignment;
			mTextLayer.updateBoundingSelectionRectangle();
		}
	}
}
