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
using System.Drawing.Drawing2D;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Bricks
{
	class RotateBrick : Action
	{
		// the static center is used to handle multiple following rotation (so we keep the first computed center in that case)
		static private PointF sLastCenter = new PointF(0, 0);	// in Stud coord
		static public bool sLastCenterIsValid = false;

		// data for the action
		protected LayerBrick mBrickLayer = null;
		protected List<Layer.LayerItem> mBricks = null;
		private bool mRotateCW;
		private float mRotationStep = 0.0f; // in degree, we need to save it because the current rotation step may change between the do and undo
		private PointF mCenter = new PointF(0,0);	// in Stud coord

		// special case for only one brick connected that we must rotate
		private PointF mConnexionPosition = new PointF(0, 0);	// in Stud coord
		private int mNewConnexionPointIndex = -1; // -1 means no connexion
		private int mOldConnexionPointIndex = -1; // -1 means no connexion
		
		// Another configuration data, for special case when this action is used during edition
		private bool mMustUpdateBrickConnectivity = true;

		public bool MustUpdateBrickConnectivity
		{
			set { mMustUpdateBrickConnectivity = value; }
		}

		public RotateBrick(LayerBrick layer, List<Layer.LayerItem> bricks, int rotateSteps)
			: this(layer, bricks, rotateSteps, false)
		{
		}

		public RotateBrick(LayerBrick layer, List<Layer.LayerItem> bricks, int rotateSteps, bool forceKeepLastCenter)
		{
			bool rotateCW = (rotateSteps < 0);
			int nbSteps = Math.Abs(rotateSteps);
			float angle = MapData.Layer.CurrentRotationStep * nbSteps;

			// special case for 1 brick that has connexion, in that case we don't care about the rotation step
			// we rotate the brick such at it can connect with its next connexion point
			// we do the special case first because maybe it will invalidate the flag sLastCenterIsValid
			if (bricks.Count == 1)
			{
				LayerBrick.Brick brick = bricks[0] as LayerBrick.Brick;
				if (brick.HasConnectionPoint)
				{
					string partNumber = brick.PartNumber;
					for (int i = 0; i < brick.ConnectionPoints.Count; ++i)
					{
						LayerBrick.Brick.ConnectionPoint connexion = brick.ConnectionPoints[i];
						if (connexion.ConnectionLink != null)
						{
							// in that case we don't use the static center
							sLastCenterIsValid = false;
							// store the connection position
							mConnexionPosition = connexion.mPositionInStudWorldCoord;
							mOldConnexionPointIndex = i;

							// search the previous connexion point of the same type than the current connexion point
							// in the worst case, we just find the same connexion point
							int nextConnexionType = BrickLibrary.ConnectionType.DEFAULT;
							angle = 0.0f;
							mNewConnexionPointIndex = i;

							// Store the next connection point index which will become the new connection point after the rotation
							// compute the rotation step by subtracted the connected point angle with the next connexion angle
							if (rotateCW)
							{
								for (int s = 0; s < nbSteps; s++)
								{
									do
									{
										int currentConnexionPointIndex = mNewConnexionPointIndex;
										mNewConnexionPointIndex--;
										if (mNewConnexionPointIndex < 0)
											mNewConnexionPointIndex = brick.ConnectionPoints.Count - 1;
										angle += BrickLibrary.Instance.getConnectionAngleToPrev(partNumber, currentConnexionPointIndex);
										nextConnexionType = BrickLibrary.Instance.getConnexionType(partNumber, mNewConnexionPointIndex);
									} while (nextConnexionType != connexion.mType);
								}
							}
							else
							{
								for (int s = 0; s < nbSteps; s++)
								{
									do
									{
										int currentConnexionPointIndex = mNewConnexionPointIndex;
										mNewConnexionPointIndex++;
										if (mNewConnexionPointIndex >= brick.ConnectionPoints.Count)
											mNewConnexionPointIndex = 0;
										angle += BrickLibrary.Instance.getConnectionAngleToNext(partNumber, currentConnexionPointIndex);
										nextConnexionType = BrickLibrary.Instance.getConnexionType(partNumber, mNewConnexionPointIndex);
									} while (nextConnexionType != connexion.mType);
								}
							}
							break;
						}
					}
				}
			}

			// then call the normal constructor
			commonConstructor(layer, bricks, angle * Math.Sign(rotateSteps), forceKeepLastCenter);
		}

		public RotateBrick(LayerBrick layer, List<Layer.LayerItem> bricks, float angle)
			: this(layer, bricks, angle, false)
		{
		}

		public RotateBrick(LayerBrick layer, List<Layer.LayerItem> bricks, float angle, bool forceKeepLastCenter)
		{
			commonConstructor(layer, bricks, angle, forceKeepLastCenter);
		}

		private void commonConstructor(LayerBrick layer, List<Layer.LayerItem> bricks, float angle, bool forceKeepLastCenter)
		{
			mBrickLayer = layer;
            mRotateCW = (angle < 0.0f);
            mRotationStep = Math.Abs(angle);

			// we must invalidate the last center if the last action in the undo stack is not a rotation
			if (!forceKeepLastCenter && !ActionManager.Instance.getUndoableActionType().IsInstanceOfType(this))
				sLastCenterIsValid = false;

			// fill the brick list with the one provided and set the center of rotation for this action
			if (bricks.Count > 0)
			{
				// copy the list, because the pointer may change (specially if it is the selection)
				// also compute the center of all the bricks
				PointF minCenter = new PointF(bricks[0].mDisplayArea.Left, bricks[0].mDisplayArea.Top);
				PointF maxCenter = new PointF(bricks[0].mDisplayArea.Right, bricks[0].mDisplayArea.Bottom);
				mBricks = new List<Layer.LayerItem>(bricks.Count);
				foreach (Layer.LayerItem obj in bricks)
				{
					mBricks.Add(obj);
					//compute the new center if the static one is not valid
					if (!sLastCenterIsValid)
					{
						if (obj.mDisplayArea.Left < minCenter.X)
							minCenter.X = obj.mDisplayArea.Left;
						if (obj.mDisplayArea.Top < minCenter.Y)
							minCenter.Y = obj.mDisplayArea.Top;
						if (obj.mDisplayArea.Right > maxCenter.X)
							maxCenter.X = obj.mDisplayArea.Right;
						if (obj.mDisplayArea.Bottom > maxCenter.Y)
							maxCenter.Y = obj.mDisplayArea.Bottom;
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
					mCenter.X = (maxCenter.X + minCenter.X) / 2;
					mCenter.Y = (maxCenter.Y + minCenter.Y) / 2;
					// and assign it to the static one
					sLastCenter = mCenter;
					sLastCenterIsValid = true;
				}
			}
		}

		public override string getName()
		{
			if (mBricks.Count == 1)
			{
				string actionName = BlueBrick.Properties.Resources.ActionRotateBrick;
				actionName = actionName.Replace("&", (mBricks[0] as LayerBrick.Brick).PartNumber);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionRotateSeveralBricks;
			}
		}

		public override void redo()
		{
			if (mRotateCW)
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(-mRotationStep);
				foreach (Layer.LayerItem obj in mBricks)
					rotate(obj as LayerBrick.Brick, rotation, -mRotationStep);
			}
			else
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(mRotationStep);
				foreach (Layer.LayerItem obj in mBricks)
					rotate(obj as LayerBrick.Brick, rotation, mRotationStep);
			}

			// special case, if the brick we have to rotate is connected, we need also to move it after the rotation
			// to keep the connexion (since the rotation only rotate in the center of the part)
			if (mNewConnexionPointIndex > -1)
			{
				LayerBrick.Brick brick = mBricks[0] as LayerBrick.Brick;
				// set the current active connexion point before seting the position via this point
				brick.ActiveConnectionPointIndex = mNewConnexionPointIndex;
				brick.ActiveConnectionPosition = mConnexionPosition;
				// and set again the current active connexion point for easy building
				brick.ActiveConnectionPointIndex = mOldConnexionPointIndex;
			}

			// update the bounding rectangle in any case because the brick is not necessary squared
			mBrickLayer.updateBoundingSelectionRectangle();
			if (mMustUpdateBrickConnectivity)
				mBrickLayer.updateBrickConnectivityOfSelection(false);
		}

		public override void undo()
		{
			// special case, if the brick we have to rotate is connected, we need to cancel the move
			// we add after the rotation, before undoing the rotation itself
			if (mNewConnexionPointIndex > -1)
			{
				LayerBrick.Brick brick = mBricks[0] as LayerBrick.Brick;
				brick.Center = mCenter;
				// and set back the current active connexion point for easy building
				brick.ActiveConnectionPointIndex = mNewConnexionPointIndex;
			}

			if (mRotateCW)
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(mRotationStep);
				foreach (Layer.LayerItem obj in mBricks)
					rotate(obj as LayerBrick.Brick, rotation, mRotationStep);
			}
			else
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(-mRotationStep);
				foreach (Layer.LayerItem obj in mBricks)
					rotate(obj as LayerBrick.Brick, rotation, -mRotationStep);
			}
			// update the bounding rectangle in any case because the brick is not necessary squared
			mBrickLayer.updateBoundingSelectionRectangle();
			if (mMustUpdateBrickConnectivity)
				mBrickLayer.updateBrickConnectivityOfSelection(false);
		}

		private void rotate(LayerBrick.Brick brick, Matrix rotation, float rotationAngle)
		{
			// compute the pivot point of the part before the rotation
			PointF brickCenter = brick.Center; // use this variable for optimization reason (the center is computed)
			PointF centerOffset = brick.OffsetFromOriginalImage;
			PointF brickPivot = new PointF(brickCenter.X + centerOffset.X, brickCenter.Y + centerOffset.Y);

			// change the orientation of the picture
			brick.Orientation = (brick.Orientation + rotationAngle);

			// change the position for a group of parts
			if (mBricks.Count > 1)
			{
				PointF[] points = { new PointF(brickPivot.X - mCenter.X, brickPivot.Y - mCenter.Y) };
				rotation.TransformVectors(points);
				// recompute the pivot
				brickPivot.X = mCenter.X + points[0].X;
				brickPivot.Y = mCenter.Y + points[0].Y;
			}

			// compute the new center of the part based on the pivot of the part and the new offset
			centerOffset = brick.OffsetFromOriginalImage;
			brickCenter.X = brickPivot.X - centerOffset.X;
			brickCenter.Y = brickPivot.Y - centerOffset.Y;
			// assign the new center position
			brick.Center = brickCenter;
		}
	}
}
