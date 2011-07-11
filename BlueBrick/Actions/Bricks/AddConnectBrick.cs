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

namespace BlueBrick.Actions.Bricks
{
	class AddConnectBrick : Action
	{
		// data for this action
		private LayerBrick mBrickLayer = null;
		private LayerBrick.Brick mBrick = null;
		private int mBrickIndex = -1; // this index is for the redo, to add the brick at the same place
		private int mNextPreferedActiveConnectionIndex = 0; // the prefered active connection index according to the brick library

		/// <summary>
		/// Compute and return the angle that should take the brickToAttach if it is connected to the
		/// fixedBrick with both their respective active connection point.
		/// </summary>
		/// <param name="fixedBrick">The brick on the map that doesn't move</param>
		/// <param name="brickToAttach">The brick to add for which we should compute the angle</param>
		/// <returns></returns>
		public static float sGetOrientationOfConnectedBrick(LayerBrick.Brick fixedBrick, LayerBrick.Brick brickToAttach)
		{
			// compute the rotation
			float newOrientation = fixedBrick.Orientation + fixedBrick.ActiveConnectionAngle + 180 - brickToAttach.ActiveConnectionAngle;
			// clamp the orientation between 0 and 360
			if (newOrientation >= 360.0f)
				newOrientation -= 360.0f;
			if (newOrientation < 0.0f)
				newOrientation += 360.0f;
			// return the value
			return newOrientation;
		}

		public AddConnectBrick(LayerBrick layer, string partNumber, int wantedConnexion)
		{
			mBrickLayer = layer;
			mBrick = new LayerBrick.Brick(partNumber);
			LayerBrick.Brick selectedBrick = layer.getConnectableBrick();

			if (selectedBrick != null)
			{
				// check if the selected brick has connection point
				if (selectedBrick.HasConnectionPoint && mBrick.HasConnectionPoint)
				{
					// choose the best active connection point for the brick
					setBestConnectionPointIndex(selectedBrick, mBrick, wantedConnexion);

					// after setting the active connection point index from which this brick will be attached,
					// get the prefered index from the library
					mNextPreferedActiveConnectionIndex = BrickLibrary.Instance.getConnectionNextPreferedIndex(partNumber, mBrick.ActiveConnectionPointIndex);

					// then rotate the brick to connect
					mBrick.Orientation = sGetOrientationOfConnectedBrick(selectedBrick, mBrick);
					// the place the brick to add at the correct position
					mBrick.ActiveConnectionPosition = selectedBrick.ActiveConnectionPosition;
				}
				else
				{
					PointF position = selectedBrick.Position;
					position.X += selectedBrick.DisplayArea.Width;
					mBrick.Position = position;
				}

				// set the index of the brick in the list just after the selected brick
				mBrickIndex = layer.BrickList.IndexOf(selectedBrick) + 1;
			}
		}

		/// <summary>
		/// Try to find the best connection index that should be used for the brickToAttach when we attach it
		/// to the current active connection point of the fixedBrick, and set it.
		/// </summary>
		/// <param name="fixedBrick">The brick that doesn't move neither change its connection point</param>
		/// <param name="brickToAttach">The brick for which computing the best connection point</param>
		/// <param name="wantedConnexion">The prefered connection index if possible</param>
		private void setBestConnectionPointIndex(LayerBrick.Brick fixedBrick, LayerBrick.Brick brickToAttach, int wantedConnexion)
		{
			// get the type of the active connexion of the selected brick
			int fixedConnexionType = fixedBrick.ActiveConnectionPoint.Type;

			// try to give the correct connexion point, either the specified wanted one, or if
			// we add the same brick do a special case
			bool isActiveConnectionPointChosen = false;
			if (wantedConnexion >= 0)
			{
				// set the active connexion point with the wanted one
				brickToAttach.ActiveConnectionPointIndex = wantedConnexion;
				// check that the wanted connection type is the same as the selected brick
				isActiveConnectionPointChosen = (brickToAttach.ActiveConnectionPoint.Type == fixedConnexionType) &&
												brickToAttach.ActiveConnectionPoint.IsFree;
			}
			else if (fixedBrick.PartNumber == brickToAttach.PartNumber)
			{
				// check if the new added brick is the same kind of the selected one, if so,
				// then we choose the previous connection point, but check if it is the same type
				brickToAttach.ActiveConnectionPointIndex = BrickLibrary.Instance.getConnectionNextPreferedIndex(fixedBrick.PartNumber, fixedBrick.ActiveConnectionPointIndex);
				// check that the connection type is the same
				isActiveConnectionPointChosen = (brickToAttach.ActiveConnectionPoint.Type == fixedConnexionType) &&
												brickToAttach.ActiveConnectionPoint.IsFree;
			}

			// if we didn't find any valid active connexion point, set the active connection
			// with the first connexion of the same type that we can find (if the brick as any connection point)
			if (!isActiveConnectionPointChosen)
				for (int i = 0; i < brickToAttach.ConnectionPoints.Count; ++i)
					if ((brickToAttach.ConnectionPoints[i].Type == fixedConnexionType) &&
						brickToAttach.ConnectionPoints[i].IsFree)
					{
						brickToAttach.ActiveConnectionPointIndex = i;
						break;
					}
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionAddBrick;
			actionName = actionName.Replace("&", mBrick.PartNumber);
			return actionName;
		}

		public override void redo()
		{
			// and add this brick in the list of the layer
			mBrickLayer.addBrick(mBrick, mBrickIndex);

			// change the selection to the new added brick (should be done after the add)
			mBrickLayer.clearSelection();
			mBrickLayer.addObjectInSelection(mBrick);
			// update the connectivity of the bricks after selecting it
			mBrickLayer.updateBrickConnectivityOfSelection(false);

			// set the prefered index after the adding,
			// because the connection of the brick will move automatically the the active connection
			mBrick.ActiveConnectionPointIndex = mNextPreferedActiveConnectionIndex;
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mBrickIndex = mBrickLayer.removeBrick(mBrick);
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
