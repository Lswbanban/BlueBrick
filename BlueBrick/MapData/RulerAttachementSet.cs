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

namespace BlueBrick.MapData
{
	public class RulerAttachementSet
	{
		public class Anchor
		{
			private LayerRuler.RulerItem mAttachedRuler = null;
			private int mAttachedPointIndex = 0;
			private PointF mLocalAttachOffsetFromCenter = new PointF();
			private PointF mWorldAttachOffsetFromCenter = new PointF();

			#region get/set
			public LayerRuler.RulerItem AttachedRuler
			{
				get { return mAttachedRuler; }
			}

			public int AttachedPointIndex
			{
				get { return mAttachedPointIndex; }
			}

			public PointF LocalAttachOffsetFromCenter
			{
				get { return mLocalAttachOffsetFromCenter; }
			}

			public PointF WorldAttachOffsetFromCenter
			{
				get { return mWorldAttachOffsetFromCenter; }
			}
			#endregion

			public static PointF sComputeLocalOffsetFromLayerItem(LayerBrick.Brick brick, PointF worldPositionInStud)
			{
				// get the brick pivot in world coordinate
				PointF brickPivot = brick.Pivot;
				// compute the offset from the brick center in world coordinate
				PointF offset = new PointF(worldPositionInStud.X - brickPivot.X, worldPositionInStud.Y - brickPivot.Y);
				// compute the rotation matrix of the brick in order to find the local offset
				Matrix matrix = new Matrix();
				matrix.Rotate(-brick.Orientation);
				PointF[] vector = { offset };
				matrix.TransformVectors(vector);
				// return the local offset
				return vector[0];
			}

			public Anchor(LayerRuler.RulerItem ruler, int index, PointF localAttachOffset)
			{
				mAttachedRuler = ruler;
				mAttachedPointIndex = index;
				mLocalAttachOffsetFromCenter = localAttachOffset;
				mWorldAttachOffsetFromCenter = localAttachOffset; // initialize with a zero angle
			}

			public void rotate(Matrix matrix)
			{
				PointF[] vector = { mLocalAttachOffsetFromCenter };
				matrix.TransformVectors(vector);
				mWorldAttachOffsetFromCenter = vector[0];
			}

			public void rotate(float angleInDegree)
			{
				Matrix matrix = new Matrix();
				matrix.Rotate(angleInDegree);
				rotate(matrix);
			}

			public void updateAttachOffsetFromCenter(PointF newLocalOffset, float attachedBrickOrientation)
			{
				mLocalAttachOffsetFromCenter = newLocalOffset;
				rotate(attachedBrickOrientation);
			}
		}

		private LayerBrick.Brick mOwnerBrick = null;
		private List<Anchor> mAnchors = new List<Anchor>();

		public RulerAttachementSet(LayerBrick.Brick owner)
		{
			mOwnerBrick = owner;
		}

		/// <summary>
		/// This method is called by the owner brick when the brick has moved to allow
		/// the attached rulers to follow the brick.
		/// </summary>
		public void brickMoveNotification()
		{
			PointF brickPivot = mOwnerBrick.Pivot;
			foreach (Anchor anchor in mAnchors)
			{
				PointF attachPosition = new PointF(brickPivot.X + anchor.WorldAttachOffsetFromCenter.X,
													brickPivot.Y + anchor.WorldAttachOffsetFromCenter.Y);
				anchor.AttachedRuler.setControlPointPosition(anchor.AttachedPointIndex, attachPosition);
			}
		}

		/// <summary>
		/// This method is called by the owner brick when the brick has rotated to allow
		/// the attached rulers to follow the brick.
		/// </summary>
		public void brickRotateNotification()
		{
			// compute the rotation matrix
			Matrix matrix = new Matrix();
			matrix.Rotate(mOwnerBrick.Orientation);
			// rotate all the anchor offset
			foreach (Anchor anchor in mAnchors)
				anchor.rotate(matrix);
			// then call the move notification
			brickMoveNotification();
		}

		public void attachRuler(Anchor anchor)
		{
			// when we attach the specified anchor, rotate it according to the orientation of the owner brick
			anchor.rotate(mOwnerBrick.Orientation);
			// then notify the ruler of the attachment
			anchor.AttachedRuler.attachControlPointToBrick(anchor.AttachedPointIndex, mOwnerBrick);
			// and add the anchor in the attachment list
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
