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

using BlueBrick.MapData;

namespace BlueBrick.Actions
{
	class AddLayer : Action
	{
		private string	mLayerType = "";
		private Layer mLayerAdded = null;
		private int mInsertionIndex = 0;

		public AddLayer(string layerType, bool updateLayerView)
		{
			// update the view unless specified by the parameter.
			// This param can be null in case we want to create several layer and updating the view at the end (which is the case at startup)
			// but we won't keep that settings after the first redo
			mUpdateLayerView = updateLayerView ? UpdateViewType.FULL : UpdateViewType.NONE;
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
				case "LayerRuler":
					mLayerAdded = new LayerRuler();
					break;
			}
			// get the current position of the selected layer
			mInsertionIndex = Map.Instance.getIndexAboveTheSelectedLayer();
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
				case "LayerRuler":
					return BlueBrick.Properties.Resources.ActionAddLayerRuler;
			}
			return BlueBrick.Properties.Resources.ActionAddLayerBrick;
		}

		public override void redo()
		{
			Map.Instance.addLayer(mLayerAdded, mInsertionIndex);
		}

		public override void undo()
		{
			Map.Instance.removeLayer(mLayerAdded);
			// force back the update (if the update was not asked at the creation)
			mUpdateLayerView = UpdateViewType.FULL;
		}
	}
}
