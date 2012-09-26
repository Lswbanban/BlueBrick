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
	class AddRuler : Action
	{
		private LayerRuler mRulerLayer = null;
		private LayerRuler.RulerItem mRulerItem = null;
		private int mRulerItemIndex = 0; // this index is for the redo, to add the ruler at the same place

		public AddRuler(LayerRuler layer, LayerRuler.RulerItem rulerItem)
		{
			mRulerLayer = layer;
			mRulerItem = rulerItem;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionAddRuler;
		}

		public override void redo()
		{
			// and add this ruler in the list of the layer
			mRulerLayer.addRulerItem(mRulerItem, mRulerItemIndex);
			// change the selection to the new added text (should be done after the add)
			mRulerLayer.clearSelection();
			mRulerLayer.addObjectInSelection(mRulerItem);
		}

		public override void undo()
		{
			// remove the specified RulerItem from the list of the layer,
			// but do not delete it, also memorise its last position
			mRulerItemIndex = mRulerLayer.removeRulerItem(mRulerItem);
		}
	}
}
