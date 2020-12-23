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
using System.Drawing;
using BlueBrick.MapData;
using BlueBrick.Actions.Items;

namespace BlueBrick.Actions.Texts
{
	public class MoveText : MoveItems
	{
		public MoveText(LayerText layer, List<Layer.LayerItem> cells, PointF move)
			: base(layer, cells, move)
		{
		}

		public override string getName()
		{
			if (mItems.Count == 1)
			{
				string actionName = BlueBrick.Properties.Resources.ActionMoveText;
				// if the first item is a group, search recursively the first non group item
				Layer.LayerItem firstItem = mItems[0];
				while (firstItem.IsAGroup)
					firstItem = (firstItem as Layer.Group).Items[0];
				// get the text and cut it if it is too long
				string text = (firstItem as LayerText.TextCell).Text.Replace("\r\n", " ");
				if (text.Length > 10)
					text = text.Substring(0, 10) + "...";
				// construct the action name
				actionName = actionName.Replace("&", text);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionMoveSeveralTexts;
			}
		}
	}
}
