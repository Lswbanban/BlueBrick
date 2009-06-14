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
	class MoveLayerUp : Action
	{
		private Layer mLayerMoved;

		public MoveLayerUp(Layer layer)
		{
			mUpdateLayerView = UpdateViewType.FULL;
			mLayerMoved = layer;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionMoveLayerUp.Replace("&", mLayerMoved.Name);
		}

		public override void redo()
		{
			Map.Instance.moveLayerUp(mLayerMoved);
		}

		public override void undo()
		{
			Map.Instance.moveLayerDown(mLayerMoved);
		}
	}
}
