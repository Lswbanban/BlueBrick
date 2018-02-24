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

namespace BlueBrick.MapData
{
	/// <summary>
	/// This class store all the Free connections available in one BrickLayer. Each Brick Layer owns one
	/// instance of this class. The Free Connections are split by type. Basically this class is a list of list
	/// of brick connection. The first level of list is the connection type, and the second level holds all
	/// the free connections.
	/// </summary>
	class FreeConnectionSet
	{
		// the list of list of connection points
		private List<List<LayerBrick.Brick.ConnectionPoint>> mFreeConnections = null;

		public int ConnectionTypeCount
		{
			get { return mFreeConnections.Count; }
		}

		#region initialization
		public FreeConnectionSet()
		{
			// the first level of list is the connection type
			int numberOfTypes = BrickLibrary.Instance.ConnectionTypes.Count;
			mFreeConnections = new List<List<LayerBrick.Brick.ConnectionPoint>>(numberOfTypes);
			// then create all the free connection list for all the different type of connection
			for (int i = 0; i < numberOfTypes; ++i)
				mFreeConnections.Add(new List<LayerBrick.Brick.ConnectionPoint>());
		}
		#endregion

		#region add/remove
		public void add(LayerBrick.Brick.ConnectionPoint connection)
		{
			mFreeConnections[connection.Type].Add(connection);
		}

		public void addAllBrickConnections(LayerBrick.Brick brick)
		{
			// add its connection points to the free list
			if (brick.HasConnectionPoint)
				foreach (LayerBrick.Brick.ConnectionPoint connection in brick.ConnectionPoints)
					if (connection.IsFree)
						mFreeConnections[connection.Type].Add(connection);
		}

		public void remove(LayerBrick.Brick.ConnectionPoint connection)
		{
			mFreeConnections[connection.Type].Remove(connection);
		}

		public void removeAll()
		{
			foreach (List<LayerBrick.Brick.ConnectionPoint> connexionList in mFreeConnections)
				connexionList.Clear();
		}
		#endregion

		#region full list access
		public List<LayerBrick.Brick.ConnectionPoint> getListForType(int connectionType)
		{
			return mFreeConnections[connectionType];
		}

		/// <summary>
		/// Get the list that contains the most connection among all the different list for all
		/// the different type of connection. This function also return in the out parameter what is
		/// the winner type. If several lists have the same numbers of the connection, the one
		/// with the lowest type number is returned
		/// </summary>
		/// <param name="connectionType">the type of the wining list</param>
		/// <returns>the list of connection which has the most items</returns>
		public List<LayerBrick.Brick.ConnectionPoint> getBiggestList(out int connectionType)
		{
			connectionType = 0;
			int bestCount = mFreeConnections[0].Count;
			for (int i = 1; i < mFreeConnections.Count; ++i)
				if (mFreeConnections[i].Count > bestCount)
				{
					connectionType = i;
					bestCount = mFreeConnections[i].Count;
				}
			// return the winning list
			return mFreeConnections[connectionType];
		}
		#endregion
	}
}
