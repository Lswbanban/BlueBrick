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

namespace BlueBrick.Actions.Grid
{
	class MoveGridOrigin : Action
	{
		private LayerGrid mGridLayer = null;
		private int mMoveX = 0;
		private int mMoveY = 0;

		public MoveGridOrigin(LayerGrid layer, int moveX, int moveY)
		{
			mGridLayer = layer;
			mMoveX = moveX;
			mMoveY = moveY;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionMoveGridOrigin;
		}

		public override void redo()
		{
			// move in the normal direction
			mGridLayer.CellIndexCornerX += mMoveX;
			mGridLayer.CellIndexCornerY += mMoveY;
		}

		public override void undo()
		{
			// move in the reverse direction
			mGridLayer.CellIndexCornerX -= mMoveX;
			mGridLayer.CellIndexCornerY -= mMoveY;
		}
	}
}
