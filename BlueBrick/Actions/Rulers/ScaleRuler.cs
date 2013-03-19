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

namespace BlueBrick.Actions.Rulers
{
	public class ScaleRuler : Action
	{
		private LayerRuler mRulerLayer = null;
		private LayerRuler.RulerItem mRulerItem = null;
		private PointF mOriginalPosition = new PointF();
		private PointF mNewPosition = new PointF();

		public ScaleRuler(LayerRuler layer, LayerRuler.RulerItem rulerItem, PointF originalPosition, PointF newPosition)
		{
			mRulerLayer = layer;
			mRulerItem = rulerItem;
			mOriginalPosition = originalPosition;
			mNewPosition = newPosition;
		}

		public override string getName()
		{
			if (mRulerItem is LayerRuler.CircularRuler)
				return BlueBrick.Properties.Resources.ActionScaleCircularRuler;
			return BlueBrick.Properties.Resources.ActionScaleLinearRuler;
		}

		public override void redo()
		{
			// scale to the new position
			mRulerItem.scaleToPoint(mNewPosition);
			// update the selection rectangle
			mRulerLayer.updateBoundingSelectionRectangle();
		}

		public override void undo()
		{
			// scale to the original position
			mRulerItem.scaleToPoint(mOriginalPosition);
			// update the selection rectangle
			mRulerLayer.updateBoundingSelectionRectangle();
		}
	}
}
