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
using System.Drawing.Drawing2D;

namespace BlueBrick.Actions.Rulers
{
	class AttachRulerToBrick : Action
	{
		private RulerAttachementSet.Anchor mAnchor = null;
		private LayerBrick.Brick mBrick = null;

		public AttachRulerToBrick(LayerRuler.RulerItem rulerItem, LayerBrick.Brick brick)
		{
			PointF brickCenter = brick.Center;
			PointF attachOffset = rulerItem.CurrentControlPoint;
			// compute the offset from the brick center in world coordinate
			attachOffset.X -= brickCenter.X;
			attachOffset.Y -= brickCenter.Y;
			// compute the rotation matrix of the brick in order to find the local offset
			Matrix matrix = new Matrix();
			matrix.Rotate(-brick.Orientation);
			PointF[] vector = { attachOffset };
			matrix.TransformVectors(vector);
			attachOffset = vector[0];
			// create a new Anchor
			mAnchor = new RulerAttachementSet.Anchor(rulerItem, rulerItem.CurrentControlPointIndex, attachOffset);
			mBrick = brick;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionAttachRulerToBrick;
		}

		public override void redo()
		{
			mBrick.attachRuler(mAnchor);
		}

		public override void undo()
		{
			mBrick.detachRuler(mAnchor);
		}
	}
}
