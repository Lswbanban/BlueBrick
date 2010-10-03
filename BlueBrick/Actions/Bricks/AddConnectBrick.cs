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

namespace BlueBrick.Actions.Bricks
{
	class AddConnectBrick : Action
	{
		// data for this action
		private LayerBrick mBrickLayer = null;
		private LayerBrick.Brick mBrick = null;
		private int mBrickIndex = -1; // this index is for the redo, to add the text at the same place

		public AddConnectBrick(LayerBrick layer, string partNumber, int wantedConnexion)
		{
			mBrickLayer = layer;
			mBrick = new LayerBrick.Brick(partNumber);

			if (layer.SelectedObjects.Count == 1)
			{
				// check if the selected brick has connection point
				LayerBrick.Brick selectedBrick = layer.SelectedObjects[0] as LayerBrick.Brick;
				if (selectedBrick.HasConnectionPoint && mBrick.HasConnectionPoint)
				{
					// get the type of the active connexion of the selected brick
					int selectedConnexionType = selectedBrick.ActiveConnectionPoint.mType;

					// try to give the correct connexion point, either the specified wanted one, or if
					// we add the same brick do a special case
					bool isActiveConnectionPointChosen = false;
					if (wantedConnexion >= 0)
					{
						// set the active connexion point with the wanted one
						mBrick.ActiveConnectionPointIndex = wantedConnexion;
						// check that the wanted connection type is the same as the selected brick
						isActiveConnectionPointChosen = (mBrick.ActiveConnectionPoint.mType == selectedConnexionType);
					}
					else if (selectedBrick.PartNumber == partNumber)
					{
						// check if the new added brick is the same kind of the selected one, if so,
						// then we choose the previous connection point, but check if it is the same type
						mBrick.ActiveConnectionPointIndex = BrickLibrary.Instance.getConnectionNextPreferedIndex(partNumber, selectedBrick.ActiveConnectionPointIndex);
						// check that the connection type is the same
						isActiveConnectionPointChosen = (mBrick.ActiveConnectionPoint.mType == selectedConnexionType);
					}

					// if we didn't find any valid active connexion point, set the active connection
					// with the first connexion of the same type that we can find (if the brick as any connection point)
					if (!isActiveConnectionPointChosen)
						for (int i = 0; i < mBrick.ConnectionPoints.Count; ++i)
							if (mBrick.ConnectionPoints[i].mType == selectedConnexionType)
							{
								mBrick.ActiveConnectionPointIndex = i;
								break;
							}
				}
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
			if (mBrickIndex == -1)
				mBrickLayer.addConnectBrick(mBrick);
			else
				mBrickLayer.addBrick(mBrick, mBrickIndex);
			// change the selection to the new added brick (should be done after the add)
			mBrickLayer.clearSelection();
			mBrickLayer.addObjectInSelection(mBrick);
			// update the connectivity of the bricks after selecting it
			mBrickLayer.updateBrickConnectivityOfSelection(false);
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
