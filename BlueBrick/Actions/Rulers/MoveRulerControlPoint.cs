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

namespace BlueBrick.Actions.Rulers
{
	public class MoveRulerControlPoint : Action
	{
		private LayerRuler mRulerLayer = null;
		private LayerRuler.RulerItem mRulerItem = null;
		private int mControlPointIndex = 0;
		private PointF mOriginalPosition = new PointF();
		private PointF mNewPosition = new PointF();
		private RulerAttachementSet.Anchor mAnchor = null;
		private PointF mOriginalLocalAttachOffset = new PointF();
		private PointF mNewLocalAttachOffset = new PointF();
		private float mAttachedBrickOrientation = 0.0f;

		public MoveRulerControlPoint(LayerRuler layer, LayerRuler.RulerItem rulerItem, PointF originalPosition, PointF newPosition)
		{
			mRulerLayer = layer;
			mRulerItem = rulerItem;
			mControlPointIndex = rulerItem.CurrentControlPointIndex;
			mOriginalPosition = originalPosition;
			mNewPosition = newPosition;
			// compute the new attach offset if the control point is attached
			if (rulerItem.IsCurrentControlPointAttached)
			{
				LayerBrick.Brick attachedBrick = rulerItem.BrickAttachedToCurrentControlPoint;
				mAnchor = attachedBrick.getRulerAttachmentAnchor(rulerItem);
				mOriginalLocalAttachOffset = mAnchor.LocalAttachOffsetFromCenter;
				mNewLocalAttachOffset = RulerAttachementSet.Anchor.sComputeLocalOffsetFromLayerItem(attachedBrick, newPosition);
				mAttachedBrickOrientation = attachedBrick.Orientation;
			}
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionMoveRulerControlPoint;
		}

		public override void redo()
		{
			// if the anchor is not null, update it with the new offset
			if (mAnchor != null)
				mAnchor.updateAttachOffsetFromCenter(mNewLocalAttachOffset, mAttachedBrickOrientation);
			// set the new position
			mRulerItem.setControlPointPosition(mControlPointIndex, mNewPosition);
			// update the selection rectangle
			mRulerLayer.updateBoundingSelectionRectangle();
		}

		public override void undo()
		{
			// if the anchor is not null, update it with the old offset
			if (mAnchor != null)
				mAnchor.updateAttachOffsetFromCenter(mOriginalLocalAttachOffset, mAttachedBrickOrientation);
			// set back the original position
			mRulerItem.setControlPointPosition(mControlPointIndex, mOriginalPosition);
			// update the selection rectangle
			mRulerLayer.updateBoundingSelectionRectangle();
		}
	}
}
