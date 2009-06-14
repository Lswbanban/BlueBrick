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

namespace BlueBrick.Actions
{
	class AddLayer : Action
	{
		private string	mLayerType = "";
		private Layer mLayerAdded = null;

		public AddLayer(string layerType)
		{
			mUpdateLayerView = UpdateViewType.FULL;
			mLayerType = layerType;
			// create the layer according to the type
			// if the layer does not exists
			switch (layerType)
			{
				case "LayerGrid":
					mLayerAdded = new LayerGrid();
					break;
				case "LayerBrick":
					mLayerAdded = new LayerBrick();
					break;
				case "LayerText":
					mLayerAdded = new LayerText();
					break;
				case "LayerArea":
					mLayerAdded = new LayerArea();
					break;
			}
		}

		public override string getName()
		{
			switch (mLayerType)
			{
				case "LayerGrid":
					return BlueBrick.Properties.Resources.ActionAddLayerGrid;
				case "LayerText":
					return BlueBrick.Properties.Resources.ActionAddLayerText;
				case "LayerArea":
					return BlueBrick.Properties.Resources.ActionAddLayerArea;
			}
			return BlueBrick.Properties.Resources.ActionAddLayerBrick;
		}

		public override void redo()
		{
			Map.Instance.addLayer(mLayerAdded);
		}

		public override void undo()
		{
			Map.Instance.removeLayer(mLayerAdded);
		}
	}
}
