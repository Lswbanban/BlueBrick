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
		private string mPartNumber = string.Empty; //if the list contains only one brick or one group, this is the name of this specific brick or group

		// special case for only one brick connected that we must rotate
		private PointF mConnexionPosition = new PointF(0, 0);	// in Stud coord
		private LayerBrick.Brick.ConnectionPoint mNewConnectionPoint = null;
		private LayerBrick.Brick.ConnectionPoint mOldConnectionPoint = null;
		
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
			// compute the default rotation angle, used if no bricks in the list is connected to something else
			float angle = MapData.Layer.CurrentRotationStep * rotateSteps;

			// now check if we can find any brick in the list which is connected to another
			// brick not in the list: in that case we don't care about the rotation step,
			// we rotate the group of brick such as it can connect with its next connexion point.
			// we do this first because maybe it will invalidate the flag sLastCenterIsValid

			// gather the free connection or those linked with bricks outside of the list of brick to rotate
			FreeConnectionSet externalConnectionSet = new FreeConnectionSet();
			FreeConnectionSet availableConnectionSet = new FreeConnectionSet();
			foreach (Layer.LayerItem item in bricks)
			{
				LayerBrick.Brick brick = item as LayerBrick.Brick;
				if (brick.HasConnectionPoint)
					foreach (LayerBrick.Brick.ConnectionPoint connection in brick.ConnectionPoints)
						if (connection.IsFree)
						{
							availableConnectionSet.add(connection);
						}
						else if (!bricks.Contains(connection.ConnectionLink.mMyBrick))
						{
							availableConnectionSet.add(connection);
							externalConnectionSet.add(connection);
						}
			}

			// get the biggest group of external connection among all the available types
			int connexionType = BrickLibrary.ConnectionType.DEFAULT;
			List<LayerBrick.Brick.ConnectionPoint> externalConnectionList = externalConnectionSet.getBiggestList(out connexionType);

			// check if there is any external connection on which we should rotate
			if (externalConnectionList.Count > 0)
			{
				// for now, without a lot of imagination, take the first connection of the list
				LayerBrick.Brick.ConnectionPoint connection = externalConnectionList[0];
				// in that case we don't use the static center
				sLastCenterIsValid = false;
				// store the connection position
				mConnexionPosition = connection.PositionInStudWorldCoord;
				mOldConnectionPoint = connection;
				LayerBrick.Brick fixedBrick = connection.ConnectedBrick;
				int fixedBrickConnectionIndex = connection.ConnectionLink.Index;

				// get the same list but for available connections
				List<LayerBrick.Brick.ConnectionPoint> availableConnectionList = availableConnectionSet.getListForType(connexionType);

				// get the index connection depending on the number of steps and the rotation direction
				int index = availableConnectionList.IndexOf(connection);
				index += (nbSteps % availableConnectionList.Count);
				mNewConnectionPoint = availableConnectionList[index % availableConnectionList.Count];

				// compute the angle to rotate
				LayerBrick.Brick newConnectedBrick = mNewConnectionPoint.mMyBrick;
				angle = AddConnectBrick.sGetOrientationOfConnectedBrick(fixedBrick, fixedBrickConnectionIndex, newConnectedBrick, mNewConnectionPoint.Index) - newConnectedBrick.Orientation;
			}

			// then call the normal constructor
			commonConstructor(layer, bricks, angle, forceKeepLastCenter);
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
				PointF minCenter = new PointF(bricks[0].DisplayArea.Left, bricks[0].DisplayArea.Top);
				PointF maxCenter = new PointF(bricks[0].DisplayArea.Right, bricks[0].DisplayArea.Bottom);
				mBricks = new List<Layer.LayerItem>(bricks.Count);
				foreach (Layer.LayerItem obj in bricks)
				{
					mBricks.Add(obj);
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

			// try to get a part number (which can be the name of a group)
			Layer.LayerItem topItem = LayerBrick.sGetTopItemFromList(mBricks);
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
				string actionName = BlueBrick.Properties.Resources.ActionRotateBrick;
				actionName = actionName.Replace("&", mPartNumber);
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

			// special case, if the bricks we have to rotate are connected, we need also to move them after the rotation
			// to keep the connexion (since the rotation only rotate in the center of the part)
			if (mNewConnectionPoint != null)
				moveToConnect(mNewConnectionPoint);

			// update the bounding rectangle in any case because the brick is not necessary squared
			mBrickLayer.updateBoundingSelectionRectangle();
			if (mMustUpdateBrickConnectivity)
				mBrickLayer.updateBrickConnectivityOfSelection(false);
		}

		public override void undo()
		{
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

			// special case, if the bricks we have to rotate are connected, we need to 
			// reattach them to the old brick after canceling the rotation
			if (mOldConnectionPoint != null)
				moveToConnect(mOldConnectionPoint);

			// update the bounding rectangle in any case because the brick is not necessary squared
			mBrickLayer.updateBoundingSelectionRectangle();
			if (mMustUpdateBrickConnectivity)
				mBrickLayer.updateBrickConnectivityOfSelection(false);
		}

		private void moveToConnect(LayerBrick.Brick.ConnectionPoint connectionToUse)
		{
			// get the owner of the connection
			LayerBrick.Brick brickToConnect = connectionToUse.mMyBrick;
			// memorise the previous position to compute the shift
			PointF oldPosition = brickToConnect.Position;
			// set the position of the brick via its connection point
			connectionToUse.PositionInStudWorldCoord = mConnexionPosition;
			// compute the shift
			PointF deltaMove = brickToConnect.Position;
			deltaMove.X -= oldPosition.X;
			deltaMove.Y -= oldPosition.Y;
			// move all the other bricks
			foreach (Layer.LayerItem item in mBricks)
				if (item != brickToConnect)
					item.Position = new PointF(item.Position.X + deltaMove.X, item.Position.Y + deltaMove.Y);
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
