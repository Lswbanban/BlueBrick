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

namespace BlueBrick.Actions.Rulers
{
	class EditRuler : Action
	{
		private LayerRuler.RulerItem mRulerItem = null;
		private LayerRuler.RulerItem mOldRulerItemTemplate = null;
		private LayerRuler.RulerItem mNewRulerItemTemplate = null;

		public EditRuler(LayerRuler.RulerItem rulerItem, LayerRuler.RulerItem rulerItemTemplateForNewProperties)
		{
			// memorise the item
			mRulerItem = rulerItem;
			// and clone the two templates, for doing undo/redo and keeping the properties when the ruler change
			mOldRulerItemTemplate = rulerItem.Clone() as LayerRuler.RulerItem;
			mNewRulerItemTemplate = rulerItemTemplateForNewProperties.Clone() as LayerRuler.RulerItem;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionEditRuler;
		}

		public override void redo()
		{
			copyRulerProperties(mNewRulerItemTemplate);
		}

		public override void undo()
		{
			copyRulerProperties(mOldRulerItemTemplate);
		}

		private void copyRulerProperties(LayerRuler.RulerItem rulerTemplate)
		{
			// line appearance
			mRulerItem.LineThickness = rulerTemplate.LineThickness;
			mRulerItem.Color = rulerTemplate.Color;
			if (mRulerItem is LayerRuler.LinearRuler)
				(mRulerItem as LayerRuler.LinearRuler).AllowOffset = (rulerTemplate as LayerRuler.LinearRuler).AllowOffset;
			// guideline appearance
			mRulerItem.GuidelineDashPattern = rulerTemplate.GuidelineDashPattern;
			mRulerItem.GuidelineThickness = rulerTemplate.GuidelineThickness;
			mRulerItem.GuidelineColor = rulerTemplate.GuidelineColor;
			// measure and unit
			mRulerItem.DisplayUnit = rulerTemplate.DisplayUnit;
			mRulerItem.DisplayDistance = rulerTemplate.DisplayDistance;
			mRulerItem.CurrentUnit = rulerTemplate.CurrentUnit;
			mRulerItem.MeasureColor = rulerTemplate.MeasureColor;
			mRulerItem.MeasureFont = rulerTemplate.MeasureFont;
		}
	}
}
