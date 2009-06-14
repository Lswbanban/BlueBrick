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

namespace BlueBrick.Actions.Area
{
	class AddArea : Action
	{
		private LayerArea mAreaLayer = null;
		private Rectangle mAreaInCellIndex = Rectangle.Empty;
		private Color mNewColor = Color.Empty;
		private Color[,] mOldColor = null;

		public AddArea(LayerArea layer, Color color, Rectangle area)
		{
			mAreaLayer = layer;
			mNewColor = color;
			mAreaInCellIndex = area;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionAddArea;
		}

		public override void redo()
		{
			// the first time we do the redo, we should init the old color array
			if (mOldColor == null)
			{
				mOldColor = new Color[mAreaInCellIndex.Width, mAreaInCellIndex.Height];
				for (int x = 0; x < mAreaInCellIndex.Width; ++x)
					for (int y = 0; y < mAreaInCellIndex.Height; ++y)
						mOldColor[x, y] = mAreaLayer.paintCell(mAreaInCellIndex.Left + x, mAreaInCellIndex.Top + y, mNewColor);
			}
			else
			{
				// just set the color for the whole area
				for (int x = 0; x < mAreaInCellIndex.Width; ++x)
					for (int y = 0; y < mAreaInCellIndex.Height; ++y)
						mAreaLayer.paintCell(mAreaInCellIndex.Left + x, mAreaInCellIndex.Top + y, mNewColor);
			}
		}

		public override void undo()
		{
			// reset the old color
			for (int x = 0; x < mAreaInCellIndex.Width; ++x)
				for (int y = 0; y < mAreaInCellIndex.Height; ++y)
					mAreaLayer.paintCell(mAreaInCellIndex.Left + x, mAreaInCellIndex.Top + y, mOldColor[x, y]);
		}
	}
}
