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
using System.Drawing;
using BlueBrick.MapData;
using System.Drawing.Drawing2D;

namespace BlueBrick.Actions.Items
{
	public abstract class RotateItems : Action
	{
		// the static center is used to handle multiple following rotation (so we keep the first computed center in that case)
		static private PointF sLastCenter = new PointF(0, 0);	// in Stud coord
		static public bool sLastCenterIsValid = false;

		// data for the action
		protected Layer mLayer = null;
		protected List<Layer.LayerItem> mItems = null;
		protected List<Layer.Group> mAllGroupsOfTheItems = null;
		protected List<Layer.Group> mTopGroupsOfTheItems = null;
		protected bool mRotateCW;
		protected float mRotationStep = 0.0f; // in degree, we need to save it because the current rotation step may change between the do and undo
		protected PointF mCenter = new PointF(0, 0);	// in Stud coord

		/// <summary>
		/// A common method that can be called by the constructor to initialize many stuff
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="originalItems"></param>
		/// <param name="angle"></param>
		/// <param name="forceKeepLastCenter"></param>
		protected virtual void commonConstructor(Layer layer, List<Layer.LayerItem> originalItems, float angle, bool forceKeepLastCenter)
		{
			mLayer = layer;
			mRotateCW = (angle < 0.0f);
			mRotationStep = Math.Abs(angle);

			// we must invalidate the last center if the last action in the undo stack is not a rotation
			if (!forceKeepLastCenter && !ActionManager.Instance.getUndoableActionType().IsInstanceOfType(this))
				sLastCenterIsValid = false;

			// fill the brick list with the one provided and set the center of rotation for this action
			if (originalItems.Count > 0)
			{
				// copy the list, because the pointer may change (specially if it is the selection)
				// also compute the center of all the bricks
				PointF minCenter = new PointF(originalItems[0].DisplayArea.Left, originalItems[0].DisplayArea.Top);
				PointF maxCenter = new PointF(originalItems[0].DisplayArea.Right, originalItems[0].DisplayArea.Bottom);
				mItems = new List<Layer.LayerItem>(originalItems.Count);
				mAllGroupsOfTheItems = new List<Layer.Group>(originalItems.Count);
				mTopGroupsOfTheItems = new List<Layer.Group>(originalItems.Count);
				foreach (Layer.LayerItem obj in originalItems)
				{
					mItems.Add(obj);
					//compute the new center if the static one is not valid
					if (!sLastCenterIsValid)
					{
						if (obj.DisplayArea.Left < minCenter.X)
							minCenter.X = obj.DisplayArea.Left;
						if (obj.DisplayArea.Top < minCenter.Y)
							minCenter.Y = obj.DisplayArea.Top;
						if (obj.DisplayArea.Right > maxCenter.X)
							maxCenter.X = obj.DisplayArea.Right;
						if (obj.DisplayArea.Bottom > maxCenter.Y)
							maxCenter.Y = obj.DisplayArea.Bottom;
					}

					// Also collect the groups amongs all the items
					Layer.Group parentGroup = obj.Group;
					if (parentGroup != null)
					{
						// we found a group, add it to the list if not already in
						if (!mAllGroupsOfTheItems.Contains(parentGroup))
							mAllGroupsOfTheItems.Add(parentGroup);

						// also if that group doesn't have a parent group, this is a top group, so add it to the top group list if not already in
						if ((parentGroup.Group == null) && !mTopGroupsOfTheItems.Contains(parentGroup))
							mTopGroupsOfTheItems.Add(parentGroup);
					}
				}
				// set the center for this rotation action (keep the previous one or compute a new one
				if (sLastCenterIsValid)
				{
					mCenter = sLastCenter;
				}
				else
				{
					// recompute a new center
					mCenter.X = (maxCenter.X + minCenter.X) * 0.5f;
					mCenter.Y = (maxCenter.Y + minCenter.Y) * 0.5f;
					// and assign it to the static one
					sLastCenter = mCenter;
					sLastCenterIsValid = true;
				}
			}
		}

		protected void rotate(Layer.LayerItem item, Matrix rotation, float rotationAngle, bool adjustPivot)
		{
			// get the pivot point of the part before the rotation
			PointF pivot = item.Pivot;

			// change the orientation of the picture
			item.Orientation = (item.Orientation + rotationAngle);

			// for some items partially attached, we may don't want to adjust the pivot
			if (adjustPivot)
			{
				// adjust the position of the pivot for a group of items
				if (mItems.Count > 1)
				{
					PointF[] points = { new PointF(pivot.X - mCenter.X, pivot.Y - mCenter.Y) };
					rotation.TransformVectors(points);
					// assign the new position
					pivot.X = mCenter.X + points[0].X;
					pivot.Y = mCenter.Y + points[0].Y;
				}

				// assign the new pivot position after rotation
				item.Pivot = pivot;
			}
		}

		protected void rotateGroups(float rotationAngle)
		{
			// rotate all the groups in order to rotate their snap margin (only usefull for named group)
			foreach (Layer.Group group in mAllGroupsOfTheItems)
				group.Orientation = group.Orientation + rotationAngle;

			// then recompute recursively their display area
			foreach (Layer.Group group in mTopGroupsOfTheItems)
				group.computeDisplayArea(true);
		}
	}
}
