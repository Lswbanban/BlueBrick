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

namespace BlueBrick.Actions.Layers
{
	class SelectLayer : Action
	{
		private Layer mLayerToSelect = null;
		private Layer mPreviousSelectedLayer = null;

		public SelectLayer(Layer layerToSelect)
		{
			// update flags
			mUpdateLayerView = UpdateViewType.LIGHT; // because the color of the layer changed
			mUpdateMapView = UpdateViewType.FULL; // because the change of the selection, unselect all the item in the layer
			// save the data
			mLayerToSelect = layerToSelect;
			mPreviousSelectedLayer = Map.Instance.SelectedLayer;
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ActionSelectLayer;
			actionName = actionName.Replace("&", mLayerToSelect.Name);
			return actionName;
		}

		public override void redo()
		{
			if (mLayerToSelect != null)
				Map.Instance.SelectedLayer = mLayerToSelect;
		}

		public override void undo()
		{
			if (mPreviousSelectedLayer != null)
				Map.Instance.SelectedLayer = mPreviousSelectedLayer;
		}
	}
}
