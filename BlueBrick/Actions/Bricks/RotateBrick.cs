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
using System.Drawing.Drawing2D;
using BlueBrick.MapData;
using BlueBrick.Actions.Items;

namespace BlueBrick.Actions.Bricks
{
	class RotateBrick : RotateItems
	{
		// data for the action
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
			// compute the default rotation angle, used if no bricks in the list is connected to something else
			float angle = MapData.Layer.CurrentRotationStep * rotateSteps;

			// now check if we can find any brick in the list which is connected to another
			// brick not in the list: in that case we don't care about the rotation step,
			// we rotate the group of brick such as it can connect with its next connexion point.
			// we do this first because maybe it will invalidate the flag sLastCenterIsValid

			// first we gather all the free connections or those linked with bricks outside of the list of brick
			FreeConnectionSet externalConnectionSet = new FreeConnectionSet(); // all the connection connected to external bricks
			FreeConnectionSet availableConnectionSet = new FreeConnectionSet(); // all the possible connections that can be used to link to one external brick
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

			// get the biggest group of external connection among all the available types, and also get its type
			int chosenConnexionType = BrickLibrary.ConnectionType.DEFAULT;
			List<LayerBrick.Brick.ConnectionPoint> externalConnectionList = externalConnectionSet.getBiggestList(out chosenConnexionType);

			// check if there is any external connection on which we should rotate
			if (externalConnectionList.Count > 0)
			{
				// in that case we don't use the static center
				sLastCenterIsValid = false;

				// for now, without a lot of imagination, take the first connection of the list
				mOldConnectionPoint = externalConnectionList[0];

				// store the connection position
				mConnexionPosition = mOldConnectionPoint.PositionInStudWorldCoord;

				// get the fixed brick, the external brick on the other side of the chosen connection
				LayerBrick.Brick fixedBrick = mOldConnectionPoint.ConnectedBrick;
				int fixedBrickConnectionIndex = mOldConnectionPoint.ConnectionLink.Index;

				// get the same list but for available connections
				List<LayerBrick.Brick.ConnectionPoint> availableConnectionList = availableConnectionSet.getListForType(chosenConnexionType);

				// check in which direction and how many connection we should jump
				bool rotateCW = (rotateSteps < 0);
				int nbSteps = Math.Abs(rotateSteps);

				// get the index of the chosen connection in the available connection list				
				int index = availableConnectionList.IndexOf(mOldConnectionPoint);
				// start from it then count forward or backward a certain number of connections
				// depending on the number of steps and the rotation direction
				if (rotateCW)
				{
					index -= (nbSteps % availableConnectionList.Count);
					if (index < 0)
						index += availableConnectionList.Count;
				}
				else
				{
					index += (nbSteps % availableConnectionList.Count);
					if (index >= availableConnectionList.Count)
						index -= availableConnectionList.Count;
				}
				// finally get the new connection from the chosen index
				mNewConnectionPoint = availableConnectionList[index];

				// compute the angle to rotate
				LayerBrick.Brick newConnectedBrick = mNewConnectionPoint.mMyBrick;
				angle = AddConnectBrick.sGetOrientationOfConnectedBrick(fixedBrick, fixedBrickConnectionIndex,
					newConnectedBrick, mNewConnectionPoint.Index) - newConnectedBrick.Orientation;
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

		protected override void commonConstructor(Layer layer, List<Layer.LayerItem> bricks, float angle, bool forceKeepLastCenter)
		{
			// call the base method
			base.commonConstructor(layer, bricks, angle, forceKeepLastCenter);

			// try to get a part number (which can be the name of a group)
			Layer.LayerItem topItem = Layer.sGetTopItemFromList(mItems);
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
			// get the rotation angle according to the rotation direction
			float rotationAngle = mRotateCW ? -mRotationStep : mRotationStep;

			// rotate all the objects
			Matrix rotation = new Matrix();
			rotation.Rotate(rotationAngle);
			foreach (Layer.LayerItem item in mItems)
				rotate(item, rotation, rotationAngle, true);

			// special case, if the bricks we have to rotate are connected, we need also to move them after the rotation
			// to keep the connexion (since the rotation only rotate in the center of the part)
			if (mNewConnectionPoint != null)
				moveToConnect(mNewConnectionPoint);

			// rotate also the groups in order to rotate their snap margin and to adjust their display area
			rotateGroups(rotationAngle);

			// reselect the items of the action, cause after we will update the connectivity of the selection
			// the selection may have changed ater a succession of undo/redo
			mLayer.selectOnlyThisObject(mItems);

			// update the bounding rectangle in any case because the brick is not necessary squared
			if (mMustUpdateBrickConnectivity)
				(mLayer as LayerBrick).updateBrickConnectivityOfSelection(false);

			// notify the main form for the brick move
			MainForm.Instance.NotifyForPartMoved();
		}

		public override void undo()
		{
			// get the rotation angle according to the rotation direction
			float rotationAngle = mRotateCW ? mRotationStep : -mRotationStep;

			// rotate all the objects
			Matrix rotation = new Matrix();
			rotation.Rotate(rotationAngle);
			foreach (Layer.LayerItem item in mItems)
				rotate(item, rotation, rotationAngle, true);

			// special case, if the bricks we have to rotate are connected, we need to 
			// reattach them to the old brick after canceling the rotation
			if (mOldConnectionPoint != null)
				moveToConnect(mOldConnectionPoint);

			// rotate also the groups in order to rotate their snap margin and to adjust their display area
			rotateGroups(rotationAngle);

			// reselect the items of the action, cause after we will update the connectivity of the selection
			// the selection may have changed ater a succession of undo/redo
			mLayer.selectOnlyThisObject(mItems);

			// update the bounding rectangle in any case because the brick is not necessary squared
			if (mMustUpdateBrickConnectivity)
				(mLayer as LayerBrick).updateBrickConnectivityOfSelection(false);

			// notify the main form for the brick move
			MainForm.Instance.NotifyForPartMoved();
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
			foreach (Layer.LayerItem item in mItems)
				if (item != brickToConnect)
					item.Position = new PointF(item.Position.X + deltaMove.X, item.Position.Y + deltaMove.Y);
		}
	}
}
