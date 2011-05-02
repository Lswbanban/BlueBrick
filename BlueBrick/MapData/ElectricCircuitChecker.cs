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
using System.Drawing;

namespace BlueBrick.MapData
{
	/// <summary>
	/// This class implement basically a graph exploration starting on the specified part and
	/// exploring all the parts of the graph. It use a counter as a kind of timestamp to know
	/// what is the currently exploration iteration
	/// </summary>
	class ElectricCircuitChecker
	{
		// the current iteration number, it will increment each time the check() method is called
		private static int sTimeStamp = 1;

		// the list of all the node still to explore
		private static List<LayerBrick.Brick> mBricksToExplore = new List<LayerBrick.Brick>();

		// the list of all the shortcut found
		private static List<LayerBrick.Brick.ConnectionPoint> mShortcuts = new List<LayerBrick.Brick.ConnectionPoint>();

		public static List<LayerBrick.Brick.ConnectionPoint> ShortcutList
		{
			get { return mShortcuts; }
		}

		private static void incrementTimeStamp()
		{
			// increment the time stamp (check for reboot of the number)
			sTimeStamp++;
			if (sTimeStamp == int.MaxValue)
				sTimeStamp = 1;
		}

		public static void check(LayerBrick.Brick startingBrick)
		{
			// check that the specified brick is an electric brick, else early exit
			if (startingBrick.ElectricCircuitIndexList == null)
				return;

			// increment the time stamp
			incrementTimeStamp();

			// clear the list and add the first node
			mShortcuts.Clear();
			mBricksToExplore.Clear();
			mBricksToExplore.Add(startingBrick);
			// init the first connection of the starting brick with the new timestamp
			LayerBrick.Brick.ConnectionPoint firstConnection = startingBrick.ConnectionPoints[startingBrick.ElectricCircuitIndexList[0].mIndex1];
			firstConnection.mPolarity = sTimeStamp;
			// if the first connection is connected to another brick, also add this brick in the list
			if (firstConnection.ConnectionLink != null)
			{
				firstConnection.ConnectionLink.mPolarity = -sTimeStamp;
				mBricksToExplore.Add(firstConnection.ConnectedBrick);
			}

			//explore while the list is not empty
			while (mBricksToExplore.Count > 0)
			{
				// pop the first node of the list
				LayerBrick.Brick brick = mBricksToExplore[0];
				mBricksToExplore.RemoveAt(0);

				// declare a boolean variable to check if during the exploration of all the circuits of the
				// brick, one was ignore. If yes and later we transfert electricity on the brick, we will
				// have to re-explore the brick for checking again the ignored circuits
				bool needToReexploreTheBrick = false;

				// iterate on all the brick connection
				if (brick.ElectricCircuitIndexList != null)
					foreach (BrickLibrary.Brick.ElectricCircuit circuit in brick.ElectricCircuitIndexList)
					{
						// get the connection point of the current circuit inside the brick
						LayerBrick.Brick.ConnectionPoint start = brick.ConnectionPoints[circuit.mIndex1];
						LayerBrick.Brick.ConnectionPoint end = brick.ConnectionPoints[circuit.mIndex2];

						// check wich one has the incoming electricity, if it's not the start,
						// swap the two connections point in order to have only one algorithm in the following
						// after this swap, normally we should have the start +/-timestamp
						if (Math.Abs(end.mPolarity) == sTimeStamp)
						{
							LayerBrick.Brick.ConnectionPoint swap = start;
							start = end;
							end = swap;
						}

						// transfert the time stamp from the start to the end
						if (Math.Abs(start.mPolarity) == sTimeStamp)
						{
							// check if we have a shortcut in the current circuit.
							// If the end is already set with the same polarity than the start, we have a shortcut
							if (end.mPolarity == start.mPolarity)
							{
								// shortcut!!
								addShortcut(start);
							}
							// else if no shorcut check if we didn't already transfer the electricity to the end
							else if (end.mPolarity != -start.mPolarity)
							{
								// the end connection was not explored yet, so we will set its timestamp and push
								// his neighbor for future exploration.

								// transfert the electricity to the end
								end.mPolarity = -start.mPolarity;

								// for complex part that has several circuit in it, if we transfert electricity
								// in a circuit, this circuit may also be linked to a previous one already iterated.
								// This is the case of the rail jonction point. The circuit 0 is for example the
								// straight line, whereas circuit 1, is the curved derivation. If the current comes
								// from circuit 1, then the circuit 0 was already ignored and must be re-tested.
								// So we reinsert the part if some circuit werz ignored on this part
								if (needToReexploreTheBrick)
								{
									mBricksToExplore.Insert(0, brick);
									needToReexploreTheBrick = false;
								}

								// add the neighbor if any
								LayerBrick.Brick.ConnectionPoint connectionLink = end.ConnectionLink;
								if (connectionLink != null)
								{
									// check if we have a shortcut
									if (connectionLink.mPolarity == end.mPolarity)
									{
										// shortcut!!
										addShortcut(end);
									}
									// if no shortcut, check if we have to to explore the connection
									else if (connectionLink.mPolarity != -end.mPolarity)
									{
										// transfert the polarity to the linked connection
										connectionLink.mPolarity = -end.mPolarity;
										// and add the new brick in the list for furture exploration
										mBricksToExplore.Add(end.ConnectedBrick);
									}
								}
							}
						}
						else
						{
							// no electric current in this circuit. This may be the case with a part
							// with different independant circuit.
							// Maybe the circuits are linked, maybe they are independant, anyway, we
							// check the flag to tell that the brick need to be re-explored
							needToReexploreTheBrick = true;
						}
					}
			}
		}

		private static void addShortcut(LayerBrick.Brick.ConnectionPoint connection)
		{
			// add the connection in the list
			mShortcuts.Add(connection);
		}

		public static void check(LayerBrick brickLayer)
		{
			// increment the time stamp
			incrementTimeStamp();

			// check for every brick if not already at the time stamp
			foreach (LayerBrick.Brick brick in brickLayer.BrickList)
				if (brick.ElectricCircuitIndexList != null)
					if (Math.Abs(brick.ConnectionPoints[0].mPolarity) != sTimeStamp)
						check(brick);
		}
	}
}
