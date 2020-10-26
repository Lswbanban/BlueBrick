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

using System.Collections.Generic;
using BlueBrick.MapData;

namespace BlueBrick.Actions.Bricks
{
	class ChangeBrickElevation : Action
	{
		private List<LayerBrick.Brick> mBricksToEdit = null;
		private List<float> mOldElevationForEachBrick = null;
		private float mNewElevation = 0;

		public ChangeBrickElevation(List<LayerBrick.Brick> bricksToEdit, float newElevation)
		{
			// save the item list to edit and the other parameters
			mBricksToEdit = bricksToEdit;
			mNewElevation = newElevation;
			// and memorise the elevation of all the bricks
			mOldElevationForEachBrick = new List<float>(bricksToEdit.Count);
			foreach (LayerBrick.Brick brick in bricksToEdit)
				mOldElevationForEachBrick.Add(brick.Altitude);
		}

		public override string getName()
		{
			return Properties.Resources.ActionChangeBrickElevation.Replace("&", mNewElevation.ToString("F0"));
		}

		public override void redo()
		{
			// assign the same new elevation to all the bricks
			foreach (LayerBrick.Brick brick in mBricksToEdit)
				brick.Altitude = mNewElevation;
		}

		public override void undo()
		{
			// restore the old elevation to all the bricks to edit
			for (int i = 0; i < mBricksToEdit.Count; ++i)
				mBricksToEdit[i].Altitude = mOldElevationForEachBrick[i];
		}
	}
}
