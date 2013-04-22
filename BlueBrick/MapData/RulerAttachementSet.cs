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

namespace BlueBrick.MapData
{
	public class RulerAttachementSet
	{
		public class Anchor
		{
			private LayerRuler.RulerItem mAttachedRuler = null;
			private int mAttachedPointIndex = 0;
			private PointF mAttachOffsetFromCenter = new PointF();

			public LayerRuler.RulerItem AttachedRuler
			{
				get { return mAttachedRuler; }
			}

			public int AttachedPointIndex
			{
				get { return mAttachedPointIndex; }
			}

			public PointF AttachOffsetFromCenter
			{
				get { return mAttachOffsetFromCenter; }
			}

			public Anchor(LayerRuler.RulerItem ruler, int index, PointF attachOffset)
			{
				mAttachedRuler = ruler;
				mAttachedPointIndex = index;
				mAttachOffsetFromCenter = attachOffset;
			}
		}

		private LayerBrick.Brick mOwnerBrick = null;
		private List<Anchor> mAnchors = new List<Anchor>();

		public RulerAttachementSet(LayerBrick.Brick owner)
		{
			mOwnerBrick = owner;
		}

		public void updatePosition()
		{
			PointF brickCenter = mOwnerBrick.Center;
			foreach (Anchor anchor in mAnchors)
			{
				PointF attachPosition = new PointF(brickCenter.X + anchor.AttachOffsetFromCenter.X,
													brickCenter.Y + anchor.AttachOffsetFromCenter.Y);
				anchor.AttachedRuler.setControlPointPosition(anchor.AttachedPointIndex, attachPosition);
			}
		}

		public void attachRuler(Anchor anchor)
		{
			anchor.AttachedRuler.attachControlPointToBrick(anchor.AttachedPointIndex, mOwnerBrick);
			mAnchors.Add(anchor);
		}

		public void detachRuler(Anchor anchor)
		{
			anchor.AttachedRuler.detachControlPoint(anchor.AttachedPointIndex);
			mAnchors.Remove(anchor);
		}

		/// <summary>
		/// get the anchor for the specified ruler for its current control point
		/// </summary>
		/// <param name="rulerItem">the anchor or null if the specified control point is not attached</param>
		/// <returns>the anchor that match both the ruler item and the connection point index</returns>
		public RulerAttachementSet.Anchor getRulerAttachmentAnchor(LayerRuler.RulerItem rulerItem)
		{
			foreach (Anchor anchor in mAnchors)
				if ((anchor.AttachedRuler == rulerItem) && (anchor.AttachedPointIndex == rulerItem.CurrentControlPointIndex))
					return anchor;
			return null;
		}
	}
}
