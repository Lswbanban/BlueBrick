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
	class MoveBrick : Action
	{
		private LayerBrick mBrickLayer = null;
		private List<Layer.LayerItem> mBricks = null;
		private PointF mMove;	// in Stud coord
		private string mPartNumber = string.Empty; //if the list contains only one brick or one group, this is the name of this specific brick or group

		public MoveBrick(LayerBrick layer, List<Layer.LayerItem> bricks, PointF move)
		{
			mBrickLayer = layer;
			mMove = move;
			// copy the list, because the pointer may change (specially if it is the selection)
			mBricks = new List<Layer.LayerItem>(bricks.Count);
			foreach (Layer.LayerItem obj in bricks)
				mBricks.Add(obj);

			// try to get a part number (which can be the name of a group)
			Layer.LayerItem topItem = Layer.sGetTopItemFromList(mBricks);
			if (topItem != null)
			{
				if (topItem.IsAGroup)
					mPartNumber = (topItem as Layer.Group).PartNumber;
				else
					mPartNumber = (topItem as LayerBrick.Brick).PartNumber;
			}
		}

		public override string getName()
		{
			// if the part number is valid, use the specific message
			if (mPartNumber != string.Empty)
			{
				string actionName = BlueBrick.Properties.Resources.ActionMoveBrick;
				actionName = actionName.Replace("&", mPartNumber);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionMoveSeveralBricks;
			}
		}

		public override void redo()
		{
			foreach (Layer.LayerItem obj in mBricks)
			{
				LayerBrick.Brick brick = obj as LayerBrick.Brick;
				brick.Position = new PointF(brick.Position.X + mMove.X, brick.Position.Y + mMove.Y);
			}
			// update the bounding rectangle
			mBrickLayer.updateBoundingSelectionRectangle();
			mBrickLayer.updateBrickConnectivityOfSelection(false);
		}

		public override void undo()
		{
			foreach (Layer.LayerItem obj in mBricks)
			{
				LayerBrick.Brick brick = obj as LayerBrick.Brick;
				brick.Position = new PointF(brick.Position.X - mMove.X, brick.Position.Y - mMove.Y);
			}
			// update the bounding rectangle
			mBrickLayer.updateBoundingSelectionRectangle();
			mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
