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

namespace BlueBrick.MapData.FlexTrack
{
	/// <summary>
	/// This class convert a set of track parts hopefully connected and containing flex tracks,
	/// into a list of bones ready for the IKSolver.
	/// </summary>
	class FlexChain
	{
		// data members
		List<IKSolver.Bone_2D_CCD> mBoneList = null;

		/// <summary>
		/// Private constructor because there are condition to create it. Use static method createFlexChain instead.
		/// </summary>
		private FlexChain(int boneCount)
		{
			mBoneList = new List<IKSolver.Bone_2D_CCD>(boneCount);
		}

		/// <summary>
		/// Create a flex chain from a set of brick given a starting brick. From the free connection point
		/// of this starting brick go through all the linked connection points while the brick only have
		/// two connections. If the brick has only one or more than 2 connection (like a joint or crossing)
		/// we stop the chain.
		/// </summary>
		/// <param name="trackList">A set of track hopefully connected together, and hopefully containing flex track</param>
		/// <param name="grabbedTrack">The part which will try to reach the target. It should be part of the list.</param>
		public static FlexChain createFlexChain(List<Layer.LayerItem> trackList, LayerBrick.Brick grabbedTrack)
		{
			// if the grabbed part is not in the list, it failed
			if (!trackList.Contains(grabbedTrack))
				return null;

			// the target point should be the free connection point that is grabbed on the grabbed part
			if (!grabbedTrack.HasConnectionPoint || !grabbedTrack.ActiveConnectionPoint.IsFree)
				return null;

			// the chain to return
			FlexChain flexChain = new FlexChain(trackList.Count);

			// start to iterate from the grabbed track
			LayerBrick.Brick currentTrack = grabbedTrack;
			LayerBrick.Brick.ConnectionPoint currentConnection = grabbedTrack.ActiveConnectionPoint;
			LayerBrick.Brick.ConnectionPoint hingedConnection = currentConnection;
			// the chain is made with track that exclusively has 2 connections to make a chain.
			while ((currentTrack != null) && (currentTrack.ConnectionPoints.Count == 2))
			{
				// get the other connection on the current brick
				int nextIndex = (currentTrack.ConnectionPoints[0] == currentConnection) ? 1 : 0;
				LayerBrick.Brick.ConnectionPoint nextConnection = currentTrack.ConnectionPoints[nextIndex];
				LayerBrick.Brick nextTrack = nextConnection.ConnectedBrick;

				// check if the connection can rotate (if it is an hinge)
				float hingeAngle = BrickLibrary.Instance.getConnexionHingeAngle(nextConnection.Type);
				if ((hingeAngle != 0.0f) || (nextTrack == null) || (nextTrack.ConnectionPoints.Count == 2))
				{
					// add the link in the list
					IKSolver.Bone_2D_CCD newBone = new IKSolver.Bone_2D_CCD();
					newBone.x = (hingedConnection.PositionInStudWorldCoord.X - nextConnection.PositionInStudWorldCoord.X);
					newBone.y = (hingedConnection.PositionInStudWorldCoord.Y - nextConnection.PositionInStudWorldCoord.Y);
					newBone.angle = 0;
					flexChain.mBoneList.Add(newBone);

					// advance the current conncetion
					hingedConnection = nextConnection.ConnectionLink;
				}

				// advance to the next link
				currentTrack = nextTrack;
				currentConnection = nextConnection.ConnectionLink;
			}

			// return the chain
			return flexChain;
		}

		/// <summary>
		/// Try to reach the specify target position with this chain
		/// </summary>
		/// <param name="xWorldStudCoord">the x target position in world stud coord</param>
		/// <param name="yWorldStudCoord">the y target position in world stud coord</param>
		/// <returns>if true, this method should be called again</returns>
		public bool reachTarget(double xWorldStudCoord, double yWorldStudCoord)
		{
			IKSolver.CCDResult result = IKSolver.CalcIK_2D_CCD(ref mBoneList, xWorldStudCoord, yWorldStudCoord, 0.5);
			return (result == IKSolver.CCDResult.Processing);
		}
	}
}
