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

namespace BlueBrick.MapData
{
	public class RulerAttachementSet
	{
		private LayerBrick.Brick mOwnerBrick = null;
		private List<LayerRuler.RulerItem> mRulersAttached = new List<LayerRuler.RulerItem>();

		public RulerAttachementSet(LayerBrick.Brick owner)
		{
			mOwnerBrick = owner;
		}

		public void updatePosition()
		{
			foreach (LayerRuler.RulerItem ruler in mRulersAttached)
				ruler.setControlPointPositionForBrick(mOwnerBrick, mOwnerBrick.Center); //TODO instead of center, we need to compute a position depending of the attached offset
		}

		public void attachRuler(LayerRuler.RulerItem ruler, PointF attachPositionInStud)
		{
			ruler.attachCurrentControlPointToBrick(mOwnerBrick);
			mRulersAttached.Add(ruler);
		}

		public void detachRuler(LayerRuler.RulerItem ruler)
		{
			ruler.detachCurrentControlPoint();
			mRulersAttached.Remove(ruler);
		}
	}
}
