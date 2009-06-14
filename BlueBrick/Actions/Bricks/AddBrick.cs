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

namespace BlueBrick.Actions.Bricks
{
	class AddBrick : Action
	{
		private LayerBrick mBrickLayer = null;
		private LayerBrick.Brick mBrick = null;
		private int mBrickIndex = -1; // this index is for the redo, to add the text at the same place

		public AddBrick(LayerBrick layer, string partNumber, PointF position)
		{
			mBrickLayer = layer;
			mBrick = new LayerBrick.Brick(partNumber);
			mBrick.Position = position;
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionAddBrick;
			actionName = actionName.Replace("&", mBrick.PartNumber);
			return actionName;
		}

		public override void redo()
		{
			// and add this text in the list of the layer
			mBrickLayer.addBrick(mBrick, mBrickIndex);
			// change the selection to the new added brick (should be done after the add)
			mBrickLayer.clearSelection();
			mBrickLayer.addObjectInSelection(mBrick);
			// update the connectivity of the bricks after having selected it
			mBrickLayer.updateBrickConnectivityOfSelection(false);
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mBrickIndex = mBrickLayer.removeBrick(mBrick);
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
