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

namespace BlueBrick.Actions.Bricks
{
	class BringBrickToFront : Action
	{
		private LayerBrick mBrickLayer = null;
		private List<Layer.LayerItem> mBricks = null;
		private List<int> mBrickIndex = null; // this list of index is for the redo, to add each text at the same place

		public BringBrickToFront(LayerBrick layer, List<Layer.LayerItem> bricksToMove)
		{
			// init the layer
			mBrickLayer = layer;
			// init the index array
			mBrickIndex = new List<int>(bricksToMove.Count);
			// copy the list, because the pointer may change (specially if it is the selection)
			// but we don't duplicate the bricks themselves
			mBricks = new List<Layer.LayerItem>(bricksToMove.Count);
			foreach (Layer.LayerItem item in bricksToMove)
				mBricks.Add(item);
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionBringBrickToFront;
		}

		public override void redo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mBrickIndex.Clear();
			foreach (Layer.LayerItem obj in mBricks)
				mBrickIndex.Add(mBrickLayer.removeBrickWithoutChangingConnectivity(obj as LayerBrick.Brick));

			// add all the bricks at the end
			foreach (Layer.LayerItem obj in mBricks)
				mBrickLayer.addBrickWithoutChangingConnectivity(obj as LayerBrick.Brick, -1);

			// reselect all the moved brick
			mBrickLayer.clearSelection();
			foreach (LayerBrick.Brick brick in mBricks)
				mBrickLayer.addObjectInSelection(brick);
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer (they must be at the end
			// of the list but we don't care
			foreach (Layer.LayerItem obj in mBricks)
				mBrickLayer.removeBrickWithoutChangingConnectivity(obj as LayerBrick.Brick);

			// add all the bricks at the end
			// We must add all the bricks in the reverse order to avoid crash (insert with an index greater than the size of the list)
			for (int i = mBricks.Count - 1; i >= 0; --i)
				mBrickLayer.addBrickWithoutChangingConnectivity(mBricks[i] as LayerBrick.Brick, mBrickIndex[i]);

			// reselect all the moved brick
			mBrickLayer.clearSelection();
			foreach (LayerBrick.Brick brick in mBricks)
				mBrickLayer.addObjectInSelection(brick);
		}
	}
}
