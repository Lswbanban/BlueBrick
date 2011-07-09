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
//		private int mNextPreferedActiveConnectionIndex = 0; // the prefered active connection index according to the brick library

		public AddConnectGroup(LayerBrick layer, string partNumber, int wantedConnexion)
		{
			// save the layer and construct the group
			mBrickLayer = layer;
			mGroup = new Layer.Group(partNumber);

			// get the flat list of bricks from the hierarchical group
			mBricksInTheGroup = mGroup.getAllChildrenItems();

			// check if we can attach the group to the unique selected object
			if (layer.SelectedObjects.Count == 1)
			{
				// find the brick of the group that will be connected to the selected brick
				LayerBrick.Brick brickToConnect = mBricksInTheGroup[0] as LayerBrick.Brick;

				// check if the selected brick has connection point
				LayerBrick.Brick selectedBrick = layer.SelectedObjects[0] as LayerBrick.Brick;
				if (selectedBrick.HasConnectionPoint && brickToConnect.HasConnectionPoint)
				{
					// choose the best active connection point for the brick
					AddConnectBrick.sSetBestConnectionPointIndex(selectedBrick, brickToConnect, wantedConnexion);

					// after setting the active connection point index from which this brick will be attached,
					// get the prefered index from the library
//					mNextPreferedActiveConnectionIndex = BrickLibrary.Instance.getConnectionNextPreferedIndex(partNumber, mBrick.ActiveConnectionPointIndex);

					// Compute the orientation of the bricks
					float newOrientation = AddConnectBrick.sGetOrientationOfConnectedBrick(selectedBrick, brickToConnect);
					newOrientation -= brickToConnect.Orientation;
					// Rotate all the bricks of the group first before translating
					RotateBrickOnPivotBrick rotateBricksAction = new RotateBrickOnPivotBrick(layer, mBricksInTheGroup, newOrientation, brickToConnect);
					rotateBricksAction.MustUpdateBrickConnectivity = false;
					rotateBricksAction.redo();

					// compute the translation to add to all the bricks
					PointF translation = new PointF(selectedBrick.ActiveConnectionPosition.X - brickToConnect.ActiveConnectionPosition.X,
													selectedBrick.ActiveConnectionPosition.Y - brickToConnect.ActiveConnectionPosition.Y);
					mGroup.translate(translation);
				}
				else
				{
					PointF position = selectedBrick.Position;
					position.X += selectedBrick.DisplayArea.Width;
					mGroup.Position = position;
				}

				// set the index of the brick in the list just after the selected brick
				mInsertIndex = layer.BrickList.IndexOf(selectedBrick) + 1;
			}
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionAddBrick;
			actionName = actionName.Replace("&", mGroup.PartNumber);
			return actionName;
		}

		public override void redo()
		{
			// clear the selection to reselect the parts of the group
			mBrickLayer.clearSelection();

			// add all the part of the group in the same order as in the group
			int insertIndex = mInsertIndex;
			foreach (Layer.LayerItem item in mBricksInTheGroup)
			{
				mBrickLayer.addBrick(item as LayerBrick.Brick, insertIndex);
				mBrickLayer.addObjectInSelection(item);
				// increase the index if it is valid
				if (insertIndex != -1)
					insertIndex++;
			}

			// update the connectivity of the bricks after selecting it
			// we don't need to update the connectivity of the bricks inside the group
			// because it is already correct and was done in the constructor of the group
			// so we can just update the connectivity with the parts outside of the group
			mBrickLayer.updateBrickConnectivityOfSelection(false);

			// set the prefered index after the adding,
			// because the connection of the brick will move automatically the the active connection
//			mBrick.ActiveConnectionPointIndex = mNextPreferedActiveConnectionIndex;
		}

		public override void undo()
		{
			// remove all the bricks of the group but do not delete them
			foreach (Layer.LayerItem item in mBricksInTheGroup)
				mBrickLayer.removeBrick(item as LayerBrick.Brick);
			// don't need to update the connectivity of the bricks because we do it specifically for the brick removed
			// mBrickLayer.updateBrickConnectivityOfSelection(false);
		}
	}
}
