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

namespace BlueBrick.Actions.Layers
{
	class ChangeLayerOption : Action
	{
		private Layer mLayer = null;
		private Layer mOldLayerData = null;
		private Layer mNewLayerData = null;
		private bool mLayerNameChanged = false;
		private Dictionary<int, Dictionary<int, SolidBrush>> mOldColorMap = null;

		public ChangeLayerOption(Layer layer, Layer oldLayerTemplate, Layer newLayerTemplate)
		{
			mUpdateLayerView = UpdateViewType.LIGHT;
			mUpdateMapView = UpdateViewType.FULL;
			// save the reference of the layer
			mLayer = layer;
			// and create two new layers to save the data in it
			mOldLayerData = oldLayerTemplate;
			mNewLayerData = newLayerTemplate;
			// check if the name changed
			mLayerNameChanged = !oldLayerTemplate.Name.Equals(mNewLayerData.Name);
			// if the layer is an area layer, save the current color map
			LayerArea layerArea = layer as LayerArea;
			if (layerArea != null)
				mOldColorMap = layerArea.ColorMap;
		}

		public override string getName()
		{
			string actionName = BlueBrick.Properties.Resources.ChangeLayerOption;
			actionName = actionName.Replace("&", mLayer.Name);
			return actionName;
		}

		public override void redo()
		{
			// if the layer is an area layer, rescale the colormap
			LayerArea layerArea = mLayer as LayerArea;
			if (layerArea != null)
			{
				LayerArea newlayerArea = mNewLayerData as LayerArea;
				if (newlayerArea != null)
					layerArea.rescaleColorMap(newlayerArea.AreaCellSizeInStud);
			}
			
			// copy the options
			mLayer.CopyOptionsFrom(mNewLayerData);

			// notify the part list if the name changed
			if (mLayerNameChanged)
				MainForm.Instance.NotifyPartListForLayerRenamed(mLayer);
		}

		public override void undo()
		{
			// if the layer is an area layer, restore the colormap
			LayerArea layerArea = mLayer as LayerArea;
			if ((layerArea != null) && (mOldColorMap != null))
				layerArea.ColorMap = mOldColorMap;

			// copy the options
			mLayer.CopyOptionsFrom(mOldLayerData);

			// notify the part list if the name changed
			if (mLayerNameChanged)
				MainForm.Instance.NotifyPartListForLayerRenamed(mLayer);
		}
	}
}
