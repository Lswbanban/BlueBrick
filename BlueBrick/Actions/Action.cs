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

namespace BlueBrick.Actions
{
	/**
	 * This is the base class for all the actions
	 */
	public abstract class Action
	{
		/// <summary>
		/// This enum describe how this action will impact the views, and
		/// how the view should be updated
		/// </summary>
		public enum UpdateViewType
		{
			NONE = 0,
			LIGHT,
			FULL,
		};

		/// <summary>
		/// By default each action will modify the map, so each action
		/// need a redraw of the map
		/// </summary>
		protected UpdateViewType mUpdateMapView = UpdateViewType.FULL;

		/// <summary>
		/// By default the action do not change the layers.
		/// Only the layer actions will set this member with an update
		/// </summary>
		protected UpdateViewType mUpdateLayerView = UpdateViewType.NONE;

		#region set/get
		public UpdateViewType UpdateMapView
		{
			get { return mUpdateMapView; }
		}

		public UpdateViewType UpdateLayerView
		{
			get { return mUpdateLayerView; }
		}
		#endregion

		public abstract string getName();
		public abstract void redo();
		public abstract void undo();
	}
}
