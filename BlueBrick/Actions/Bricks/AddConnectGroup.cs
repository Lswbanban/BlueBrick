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
	class AddConnectGroup : Action
	{
		// data for this action
		private LayerBrick mBrickLayer = null;
		private Layer.Group mGroup = null;
		private List<Layer.LayerItem> mBricksInTheGroup = null;
		private int mInsertIndex = -1; // this index is for the redo, to add the bricks at the same place
		private int mNextPreferedActiveConnectionIndex = 0; // the prefered active connection index according to the brick library

		/// <summary>
		/// Add a new named group which has the specified partNumber on the specifier layer, and connected it to
		/// the specified selectedItem (part or group) using the specified wanted connection for that new part.
		/// </summary>
		/// <param name="layer">The layer on which to add the group, and in which the selectedItem is selected</param>
		/// <param name="selectedItem">The single selected item (this can be a single part or a single group). This parameter cannot be null.</param>
		/// <param name="partNumber">The number of the named group to add</param>
		/// <param name="wantedConnexion">The connection index of the group to add that should be used to connect to the selected item, or -1 if you don't care.</param>
		public AddConnectGroup(LayerBrick layer, Layer.LayerItem selectedItem, string partNumber, int wantedConnexion)
		{
			// the selected item should not be null
			System.Diagnostics.Debug.Assert(selectedItem != null);

			// save the layer and construct the group
			mBrickLayer = layer;
			mGroup = new Layer.Group(partNumber);
			// get the flat list of bricks from the hierarchical group
			mBricksInTheGroup = mGroup.getAllLeafItems();

			// get the connectable brick among the selection, and also the selected item (in case the selected item is a single group without connections points)
			LayerBrick.Brick selectedBrick = layer.getConnectableBrick();
			LayerBrick.Brick brickToConnectInAddedGroup = null;
			
			// check if we can attach the group to the unique selected object
			if ((selectedBrick != null) && selectedBrick.HasConnectionPoint)
			{
				// find the brick of the group that will be connected to the selected brick
				brickToConnectInAddedGroup = findBrickToConnectAndSetBestConnectionPointIndex(selectedItem, ref wantedConnexion);
			}

			// check if the brick to connect is valid and has connection point
			if ((brickToConnectInAddedGroup != null) && brickToConnectInAddedGroup.HasConnectionPoint)
			{
				// after setting the active connection point index from which this brick will be attached,
				// get the prefered index from the library
				mNextPreferedActiveConnectionIndex = BrickLibrary.Instance.getConnectionNextPreferedIndex(partNumber, wantedConnexion);

				// Compute the orientation of the bricks
				float newOrientation = AddConnectBrick.sGetOrientationOfConnectedBrick(selectedBrick, brickToConnectInAddedGroup);
				newOrientation -= brickToConnectInAddedGroup.Orientation;
				// Rotate all the bricks of the group first before translating
				RotateBrickOnPivotBrick rotateBricksAction = new RotateBrickOnPivotBrick(layer, mBricksInTheGroup, newOrientation, brickToConnectInAddedGroup);
				rotateBricksAction.MustUpdateBrickConnectivity = false;
				rotateBricksAction.redo();

				// compute the translation to add to all the bricks
				PointF translation = new PointF(selectedBrick.ActiveConnectionPosition.X - brickToConnectInAddedGroup.ActiveConnectionPosition.X,
												selectedBrick.ActiveConnectionPosition.Y - brickToConnectInAddedGroup.ActiveConnectionPosition.Y);
				mGroup.translate(translation);
			}
			else
			{
				// and just compute the position to the right of the selected item
				PointF position = selectedItem.Position;
				position.X += selectedItem.DisplayArea.Width;
				mGroup.Position = position;

				// the reassing the selected brick with the first brick of the group if the selected item is a group
				// so that the brick index can correctly be set
				if (selectedItem.IsAGroup)
					selectedBrick = (selectedItem as Layer.Group).getAllLeafItems()[0] as LayerBrick.Brick;
				else
					selectedBrick = selectedItem as LayerBrick.Brick;
			}

			// set the index of the group in the list just after the selected brick
			if (selectedBrick != null)
				mInsertIndex = layer.BrickList.IndexOf(selectedBrick) + 1;
		}

		private LayerBrick.Brick setActiveConnectionIndex(int connexionIndex)
		{
			mGroup.ActiveConnectionIndex = connexionIndex;
			return mGroup.BrickThatHoldsActiveConnection;
		}

		private LayerBrick.Brick findBrickToConnectAndSetBestConnectionPointIndex(Layer.LayerItem fixedItem, ref int wantedConnexion)
		{
			// the brick that will be used to attach the group
			LayerBrick.Brick brickToAttach = null;
			int originalWantedConnexion = wantedConnexion; // memorize the original wanted connection in case we need to reset it

			// get the type of the active connexion of the selected brick
			Layer.Group fixedGroup = fixedItem as Layer.Group;
			int fixedConnexionType = (fixedGroup != null) ? fixedGroup.ActiveConnectionPoint.Type : (fixedItem as LayerBrick.Brick).ActiveConnectionPoint.Type;

			// try to give the correct connexion point, either the specified wanted one, or if
			// we add the same brick do a special case
			bool isActiveConnectionPointChosen = false;
			if (wantedConnexion >= 0)
			{
				// set the active connexion point with the wanted one
				brickToAttach = setActiveConnectionIndex(wantedConnexion);
				// check that the wanted connection type is the same as the selected brick
				isActiveConnectionPointChosen = (brickToAttach != null) &&
												(brickToAttach.ActiveConnectionPoint.Type == fixedConnexionType) &&
												brickToAttach.ActiveConnectionPoint.IsFree;
			}
			else if ((fixedGroup != null) && (fixedGroup.PartNumber == mGroup.PartNumber))
			{
				// check if the new added brick is the same kind of the selected one, if so,
				// then we choose the previous connection point, but check if it is the same type
				wantedConnexion = BrickLibrary.Instance.getConnectionNextPreferedIndex(fixedGroup.PartNumber, fixedGroup.ActiveConnectionIndex);
				brickToAttach = setActiveConnectionIndex(wantedConnexion);
				// check that the connection type is the same
				isActiveConnectionPointChosen = (brickToAttach != null) &&
												(brickToAttach.ActiveConnectionPoint.Type == fixedConnexionType) &&
												brickToAttach.ActiveConnectionPoint.IsFree;
			}

			// if we didn't find any valid active connexion point, set the active connection
			// with the first connexion of the same type that we can find (if the brick as any connection point)
			if (!isActiveConnectionPointChosen)
			{
				wantedConnexion = 0;
				foreach (Layer.LayerItem item in mBricksInTheGroup)
				{
					LayerBrick.Brick brick = item as LayerBrick.Brick;
					if (brick.HasConnectionPoint)
						for (int i = 0; i < brick.ConnectionPoints.Count; ++i)
						{
							if ((brick.ConnectionPoints[i].Type == fixedConnexionType) &&
								brick.ConnectionPoints[i].IsFree)
							{
								brick.ActiveConnectionPointIndex = i;
								brickToAttach = brick;
								isActiveConnectionPointChosen = true;
								break;
							}
							// increase the connection count
							++wantedConnexion;
						}
					// break again if we found it
					if (isActiveConnectionPointChosen)
						break;
				}
			}

			// if we still didn't find a compatible brick, reset the wanted connection that we have modified
			if (!isActiveConnectionPointChosen)
				wantedConnexion = originalWantedConnexion;

			// finally return the brick selected for the attachement
			return brickToAttach;
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionAddBrick;
			actionName = actionName.Replace("&", mGroup.PartNumber);
			return actionName;
		}

		public override void redo()
		{
			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickAdded(mBrickLayer, mGroup, false);

			// clear the selection to reselect the parts of the group
			mBrickLayer.clearSelection();

			// add all the part of the group in the same order as in the group
			int insertIndex = mInsertIndex;
			foreach (Layer.LayerItem item in mBricksInTheGroup)
			{
				mBrickLayer.addBrick(item as LayerBrick.Brick, insertIndex);
				// update the connectivity of each brick, in order to also create connections
				// between bricks inside the same group, otherwise if we update the connectivity of
				// the selection, the connectivity inside the selection will not change
				mBrickLayer.updateFullBrickConnectivityForOneBrick(item as LayerBrick.Brick);
				// select the brick (in order to select the whole group at the end
				mBrickLayer.addObjectInSelection(item);
				// increase the index if it is valid
				if (insertIndex != -1)
					insertIndex++;
			}

			// set the prefered index after the adding,
			// because the connection of the brick will move automatically the the active connection
			setActiveConnectionIndex(mNextPreferedActiveConnectionIndex);
		}

		public override void undo()
		{
			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickRemoved(mBrickLayer, mGroup, false);

			// remove all the bricks of the group but do not delete them
			foreach (Layer.LayerItem item in mBricksInTheGroup)
				mBrickLayer.removeBrick(item as LayerBrick.Brick);
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
