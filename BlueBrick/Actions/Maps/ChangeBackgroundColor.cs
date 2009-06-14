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

namespace BlueBrick.Actions.Maps
{
	class ChangeBackgroundColor : Action
	{
		private Color mOldColor = Color.Empty;
		private Color mNewColor = Color.Empty;

		public ChangeBackgroundColor(Color newColor)
		{
			mOldColor = Map.Instance.BackgroundColor;
			mNewColor = newColor;
		}

		public override string getName()
		{
			return BlueBrick.Properties.Resources.ActionChangeBackgroundColor;
		}

		public override void redo()
		{
			Map.Instance.BackgroundColor = mNewColor;
		}

		public override void undo()
		{
			Map.Instance.BackgroundColor = mOldColor;
		}
	}
}
