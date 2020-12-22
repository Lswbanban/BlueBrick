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

using System.Collections.Generic;
using System.Drawing;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Bricks
{
	class RotateAndMoveBrick : RotateBrickOnPivotBrick
	{
		private MoveBrick mMoveAction = null;

		public RotateAndMoveBrick(LayerBrick layer, List<Layer.LayerItem> bricks, float angle, LayerBrick.Brick pivotBrick, PointF move)
			: base(layer, bricks, angle, pivotBrick)
		{
			this.MustUpdateBrickConnectivity = false; // the connectivity will be updated by the move action
			mMoveAction = new MoveBrick(layer, bricks, move);
		}

		public override string getName()
		{
			// use the same action name than the move action
			return mMoveAction.getName();
		}

		public override void redo()
		{
			// do the move action after to update the connectivity
			base.redo();
			mMoveAction.redo();
		}

		public override void undo()
		{
			// do the move action after to update the connectivity
			base.undo();
			mMoveAction.undo();
		}
	}
}
