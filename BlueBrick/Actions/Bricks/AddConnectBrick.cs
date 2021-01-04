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

		/// <summary>
		/// Compute and return the angle that should take the brickToAttach if it is connected to the
		/// fixedBrick with both their respective active connection point.
		/// </summary>
		/// <param name="fixedBrick">The brick on the map that doesn't move</param>
		/// <param name="brickToAttach">The brick to add for which we should compute the angle</param>
		/// <returns>The absolute orientation that should have the brick to attach to be correctly connected to the fixed brick (in degree)</returns>
		public static float sGetOrientationOfConnectedBrick(LayerBrick.Brick fixedBrick, LayerBrick.Brick brickToAttach)
		{
			return sGetOrientationOfConnectedBrick(fixedBrick, fixedBrick.ActiveConnectionPointIndex, brickToAttach, brickToAttach.ActiveConnectionPointIndex);
		}

		/// <summary>
		/// Compute and return the angle that should take the brickToAttach if it is connected to the
		/// fixedBrick with both the specified connection point.
		/// </summary>
		/// <param name="fixedBrick">The brick on the map that doesn't move</param>
		/// <param name="connectionIndexForFixedBrick">the fixed brick's connection index that you want to use to compute the orientaion</param>
		/// <param name="brickToAttach">The brick to add for which we should compute the angle</param>
		/// <param name="connectionIndexForBrickToAttach">the attached brick's connection index that you want to use to compute the orientaion</param>
		/// <returns>The absolute orientation that should have the brick to attach to be correctly connected to the fixed brick (in degree)</returns>
		public static float sGetOrientationOfConnectedBrick(LayerBrick.Brick fixedBrick, int connectionIndexForFixedBrick, LayerBrick.Brick brickToAttach, int connectionIndexForBrickToAttach)
		{
			// compute the rotation
			float newOrientation = fixedBrick.Orientation +
									BrickLibrary.Instance.getConnectionAngleDifference(fixedBrick.PartNumber,
												connectionIndexForFixedBrick,
												brickToAttach.PartNumber,
												connectionIndexForBrickToAttach);
			// clamp the orientation between 0 and 360
			if (newOrientation >= 360.0f)
				newOrientation -= 360.0f;
			if (newOrientation < 0.0f)
				newOrientation += 360.0f;
			// return the value
			return newOrientation;
		}

		/// <summary>
		/// Add a new single part which has the specified partNumber on the specifier layer, and connected it to 
		/// the specified selectedItem (part or group) using the specified wanted connection for that new part.
		/// </summary>
		/// <param name="layer">The layer on which to add the part, and in which the selectedItem is selected</param>
		/// <param name="selectedItem">The single selected item (this can be a single part or a single group). This parameter cannot be null.</param>
		/// <param name="partNumber">The number of the part to add</param>
		/// <param name="wantedConnexion">The connection index of the part to add that should be used to connect to the selected item, or -1 if you don't care.</param>
		public AddConnectBrick(LayerBrick layer, Layer.LayerItem selectedItem, string partNumber, int wantedConnexion)
		{
			// the selected item should not be null
			System.Diagnostics.Debug.Assert(selectedItem != null);

			mBrickLayer = layer;
			mBrick = new LayerBrick.Brick(partNumber);
			LayerBrick.Brick selectedBrick = layer.getConnectableBrick();
			bool hasAGoodConnectionBeenFound = false;

			// check if the selected brick and the brick to add have connection points, and if we can find a good connection match
			// The selected brick can be null if there's no connection in the single group connected in the layer
			if ((selectedBrick != null) && selectedBrick.HasConnectionPoint && mBrick.HasConnectionPoint)
			{
				// choose the best active connection point for the brick
				hasAGoodConnectionBeenFound = setBestConnectionPointIndex(selectedBrick, mBrick, wantedConnexion);
			}

			// check if we found a good connection, continue to set the position of the brick to add relative to the connection
			if (hasAGoodConnectionBeenFound)
			{
				// then rotate the brick to connect
				mBrick.Orientation = sGetOrientationOfConnectedBrick(selectedBrick, mBrick);
				// the place the brick to add at the correct position
				mBrick.ActiveConnectionPosition = selectedBrick.ActiveConnectionPosition;
			}
			else
			{
				// and just compute the position to the right of the selected item
				PointF position = selectedItem.Position;
				position.X += selectedItem.DisplayArea.Width;
				mBrick.Position = position;

				// the reassing the selected brick with the first brick of the group if the selected item is a group
				// so that the brick index can correctly be set
				if (selectedItem.IsAGroup)
					selectedBrick = (selectedItem as Layer.Group).getAllLeafItems()[0] as LayerBrick.Brick;
				else
					selectedBrick = selectedItem as LayerBrick.Brick;
			}

			// set the index of the brick in the list just after the selected brick
			if (selectedBrick != null)
				mBrickIndex = layer.BrickList.IndexOf(selectedBrick) + 1;
		}

		/// <summary>
		/// Try to find the best connection index that should be used for the brickToAttach when we attach it
		/// to the current active connection point of the fixedBrick, and set it.
		/// </summary>
		/// <param name="fixedBrick">The brick that doesn't move neither change its connection point</param>
		/// <param name="brickToAttach">The brick for which computing the best connection point</param>
		/// <param name="wantedConnexion">The prefered connection index if possible</param>
		private bool setBestConnectionPointIndex(LayerBrick.Brick fixedBrick, LayerBrick.Brick brickToAttach, int wantedConnexion)
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
					if ((brickToAttach.ConnectionPoints[i].Type == fixedConnexionType)
						/* && brickToAttach.ConnectionPoints[i].IsFree*/) // actually it is useless to check if it is free, because all connection of a new part added are all free
					{
						brickToAttach.ActiveConnectionPointIndex = i;
						isActiveConnectionPointChosen = true;
						break;
					}

			// return if we found a good connection for the brick to add
			return isActiveConnectionPointChosen;
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
			mBrickLayer.selectOnlyThisObject(mBrick);
			// update the connectivity of the bricks after selecting it
			mBrickLayer.updateBrickConnectivityOfSelection(false);

			// notify the part list view (after actually adding the brick because the total map size need to be recomputed)
			MainForm.Instance.NotifyPartListForBrickAdded(mBrickLayer, mBrick, false);
		}

		public override void undo()
		{
			// remove the specified brick from the list of the layer,
			// but do not delete it, also memorise its last position
			mBrickIndex = mBrickLayer.removeBrick(mBrick);
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);

			// notify the part list view (after actually deleting the brick because the total map size need to be recomputed)
			MainForm.Instance.NotifyPartListForBrickRemoved(mBrickLayer, mBrick, false);
		}
	}
}
