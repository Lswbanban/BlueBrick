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
	class MoveArea : Action
	{
		private LayerArea mAreaLayer = null;
		private int mMoveX = 0;
		private int mMoveY = 0;

		public MoveArea(LayerArea layer, int moveX, int moveY)
		{
			mAreaLayer = layer;
			mMoveX = moveX;
			mMoveY = moveY;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionMoveArea;
		}

		public override void redo()
		{
			// move in the normal direction
			mAreaLayer.moveCells(mMoveX, mMoveY);
		}

		public override void undo()
		{
			// move in the reverse direction
			mAreaLayer.moveCells(-mMoveX, -mMoveY);
		}
	}
}
