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
	/// This a node in the graph of all the possible paths.
	/// </summary>
	public class AStarNode
	{
		/// <summary>
		/// The brick to which this node refer.
		/// </summary>
		public LayerBrick.Brick mBrick = null;

		/// <summary>
		/// The parent node in the path. Used to come back to the start when we reached the goal.
		/// </summary>
		public AStarNode mParentNode = null;

		/// <summary>
		/// The cost of the current path from the start node to this one.
		/// Sorry the name of this variable is not explicit but refer to its classical name
		/// in the A* documentations. Please see an A* doc for more details.
		/// </summary>
		public float g = 0.0f;

		/// <summary>
		/// A heuristic cost of the path from this node to the goal node.
		/// Sorry the name of this variable is not explicit but refer to its classical name
		/// in the A* documentations. Please see an A* doc for more details.
		/// </summary>
		public float h = 0.0f;

		/// <summary>
		/// The global cost of the path if the path use this node.
		/// Sorry the name of this variable is not explicit but refer to its classical name
		/// in the A* documentations. Please see an A* doc for more details.
		/// </summary>
		public float f = 0.0f;

		/// <summary>
		/// construct a new node
		/// </summary>
		/// <param name="brick">the brick corresponding to this node</param>
		public AStarNode(LayerBrick.Brick brick)
		{
			mBrick = brick;
		}

		/// <summary>
		/// Compute the parameter g, h, f of this node.
		/// </summary>
		public void ComputeParameters(LayerBrick.Brick goalBrick)
		{
			PointF distance = new PointF();

			// we will compute g first
			if (mParentNode != null)
			{
				// the cost between this node and the previous one is always equal to the distance between
				// this brick and the parent brick position
				PointF parentCenter = mParentNode.mBrick.Center;
				distance.X = parentCenter.X - mBrick.Center.X;
				distance.Y = parentCenter.Y - mBrick.Center.Y;

				// g is the cost of the path from start node to this node. This is a real cost, defined by the algorithm, can't be changed.
				// so g equals to the cost from the start node to the previous one PLUS the cost between the previous one and this one.
				g = mParentNode.g + (float)Math.Sqrt((distance.X*distance.X) + (distance.Y*distance.Y));
			}
			else
			{
				g = 0;	// no parent that means this node is the start node, so g equals to 0
			}

			// h is the cost of the path from this node to the goal node. This is not a real cost,
			// just an heuristic, that is to say the remaining distance.
			distance.X = goalBrick.Center.X - mBrick.Center.X;
			distance.Y = goalBrick.Center.Y - mBrick.Center.Y;
			h = (float)Math.Sqrt((distance.X * distance.X) + (distance.Y * distance.Y));

			// f is the global cost. Defined by the algorithm, can't be changed.
			f = g + h;
		}
	}

	public class AStarNodeList : List<AStarNode>
	{
		public AStarNode Find(LayerBrick.Brick brickToFind)
		{
			foreach (AStarNode node in this)
				if (node.mBrick == brickToFind)
					return node;
			return null;
		}
	}

	public class AStar
	{
		private AStarNodeList mOpenList = new AStarNodeList();
		private AStarNodeList mCloseList = new AStarNodeList();

		public List<Layer.LayerItem> findPath(LayerBrick.Brick startBrick, LayerBrick.Brick goalBrick)
		{
			List<Layer.LayerItem> result = new List<Layer.LayerItem>();

			// init some variables
			mOpenList.Clear();
			mCloseList.Clear();

			// create the first node with the starting brick
			AStarNode currentNode = new AStarNode(startBrick);
			currentNode.ComputeParameters(goalBrick);

			// start of the loop
			while (currentNode != null)
			{
				// get the refernce of the current brick
				LayerBrick.Brick currentBrick = currentNode.mBrick;

				// check if we reached the goal
				if (currentBrick == goalBrick)
				{
					// the goal is reached, put all the bricks in the result list
					for (AStarNode node = currentNode; node != null; node = node.mParentNode)
						result.Add(node.mBrick);
					return result;
				}

				// now iterate on all the connexion point of the current brick
				List<LayerBrick.Brick.ConnectionPoint> connexionList = currentBrick.ConnectionPoints;
				if (connexionList != null)
					foreach (LayerBrick.Brick.ConnectionPoint connexion in connexionList)
					{
						// check if the connexion is free, or if there is a brick connected to it
						LayerBrick.Brick neighborBrick = connexion.ConnectedBrick;
						if (neighborBrick == null)
							continue;

						// we found a valid connexion, create a new node for this new potential brick to explore
						AStarNode potentialNewNeighborNode = new AStarNode(neighborBrick);
						potentialNewNeighborNode.mParentNode = currentNode;
						potentialNewNeighborNode.ComputeParameters(goalBrick);

						// try to search the neighbor brick in the close list
						AStarNode neighborNode = mCloseList.Find(neighborBrick);
						if (neighborNode != null)
						{
							// we found this brick in the close list, that means this brick was already explored,
							// but we need to check if we found a shorter way, by checking the f values
							// so we check if the node stay where it is
							if (neighborNode.f <= potentialNewNeighborNode.f)
								continue; // that's fine the previous exploration was the shorter way
							// else we found a shorter way
							// so we remove the node from the close list (cause we will add it in open)
							mCloseList.Remove(neighborNode);
						}
						else
						{
							// the neighbor brick is not in the close list so now we check if it is in open list
							neighborNode = mOpenList.Find(neighborBrick);
							if (neighborNode != null)
							{
								// the brick is already in the open list but we check if we have found a shorter way
								// by checking the f values.
								if (neighborNode.f <= potentialNewNeighborNode.f)
									continue; // that's fine the new way we found is not shorter
								// else that mean the new way is shorter
								// so we remove the node from the open list (cause we will add the better one)
								mOpenList.Remove(neighborNode);
							}
						}

						// If we reach this point, that means the potential new node is valid and
						// must be added in the open list
						mOpenList.Add(potentialNewNeighborNode);
					}

				// the current node is finished to be expored, so add it in the close list
				mCloseList.Add(currentNode);

				// get the next node to explore
				if (mOpenList.Count > 0)
				{
					currentNode = mOpenList[0];
					mOpenList.RemoveAt(0);
				}
				else
				{
					currentNode = null;
				}
			}

			// the open list is empty and we didn't find the goal brick, so the search failed.
			return result;
		}
	}
}
