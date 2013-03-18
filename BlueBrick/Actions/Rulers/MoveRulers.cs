﻿// BlueBrick, a LEGO(c) layout editor.
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
using System.Drawing;
using BlueBrick.MapData;
using BlueBrick.Actions.Items;

namespace BlueBrick.Actions.Rulers
{
	class MoveRulers : MoveItems
	{
		public MoveRulers(LayerRuler layer, List<Layer.LayerItem> rulers, PointF move)
			: base(layer, rulers, move)
		{
		}

		public override string getName()
		{
			if (mItems.Count == 1)
				return BlueBrick.Properties.Resources.ActionMoveRuler;
			else
				return BlueBrick.Properties.Resources.ActionMoveSeveralRulers;
		}
	}
}
