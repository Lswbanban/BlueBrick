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

namespace BlueBrick.MapData.FlexTrack
{
	/// <summary>
	/// This class convert a set of track parts hopefully connected and containing flex tracks,
	/// into a list of bones ready for the IKSolver.
	/// </summary>
	class FlexChain
	{
		// For optimization reason, we create a small class to store the brick connection and the angle 
		// between the two bricks linked 
		private class ChainLink
		{
			public LayerBrick.Brick.ConnectionPoint mFirstConnection = null;
			public LayerBrick.Brick.ConnectionPoint mSecondConnection = null;
			public float mAngleBetween = 0.0f;

			public ChainLink(LayerBrick.Brick.ConnectionPoint firstConnection, LayerBrick.Brick.ConnectionPoint secondConnection)
			{
				mFirstConnection = firstConnection;
				mSecondConnection = secondConnection;
				if (firstConnection != null)
				{
					int firstIndex = firstConnection.mMyBrick.ConnectionPoints.IndexOf(firstConnection);
					int secondIndex = secondConnection.mMyBrick.ConnectionPoints.IndexOf(secondConnection);
					mAngleBetween = BrickLibrary.Instance.getConnectionAngleDifference(secondConnection.mMyBrick.PartNumber, secondIndex,
																					firstConnection.mMyBrick.PartNumber, firstIndex);
				}
				else
				{
					// if the first connection point is null, create a dummy connection point in the world,
					// at the position of the second connection and not attached to a brick.
					// The angle between is in that case of course null
					mFirstConnection = new LayerBrick.Brick.ConnectionPoint(secondConnection.PositionInStudWorldCoord);
				}
			}
		}

		// data members
		private List<IKSolver.Bone_2D_CCD> mBoneList = null;
		private List<ChainLink> mFlexChainList = null;
		private int mRootHingedLinkIndex = 0; // the first hinged connection in the chain that can move

		/// <summary>
		/// Private constructor because there are condition to create it. Use static method createFlexChain instead.
		/// </summary>
		private FlexChain(int boneCount)
		{
			mBoneList = new List<IKSolver.Bone_2D_CCD>(boneCount);
			mFlexChainList = new List<ChainLink>(boneCount * 2);
		}

		private static float simplifyAngle(float angle)
		{
			if (angle <= -360.0f)
				angle += 360.0f;
			if (angle >= 360.0f)
				angle -= 360.0f;
			return angle;
		}

		private static void addNewBone(FlexChain flexChain, LayerBrick.Brick.ConnectionPoint connection)
		{
			IKSolver.Bone_2D_CCD newBone = new IKSolver.Bone_2D_CCD();
			newBone.worldX = connection.PositionInStudWorldCoord.X;
			newBone.worldY = -connection.PositionInStudWorldCoord.Y; // BlueBrick use an indirect coord sys, and the IKSolver a direct one
			newBone.connectionPoint = connection;
			flexChain.mBoneList.Insert(0, newBone);
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
			LayerBrick.Brick currentBrick = grabbedTrack;
			LayerBrick.Brick.ConnectionPoint currentFirstConnection = grabbedTrack.ActiveConnectionPoint;
			ChainLink hingedLink = null;
			addNewBone(flexChain, currentFirstConnection);
			// the chain is made with track that exclusively has 2 connections to make a chain.
			while ((currentBrick != null) && (currentBrick.ConnectionPoints.Count == 2))
			{
				// get the other connection on the current brick
				int secondIndex = (currentBrick.ConnectionPoints[0] == currentFirstConnection) ? 1 : 0;
				LayerBrick.Brick.ConnectionPoint currentSecondConnection = currentBrick.ConnectionPoints[secondIndex];
				LayerBrick.Brick nextBrick = currentSecondConnection.ConnectedBrick;
				LayerBrick.Brick.ConnectionPoint nextFirstConnection = currentSecondConnection.ConnectionLink;

				// add the two connections of the brick
				ChainLink link = new ChainLink(nextFirstConnection, currentSecondConnection);
				flexChain.mFlexChainList.Insert(0, link);

				// check if the connection can rotate (if it is an hinge)
				float hingeAngle = BrickLibrary.Instance.getConnexionHingeAngle(currentSecondConnection.Type);
				if (hingeAngle != 0.0f)
				{
					// advance the hinge conncetion
					hingedLink = link;
					// add the link in the list
					addNewBone(flexChain, currentSecondConnection);
					// compute the current angle between the hinge connection and set it to the current bone
					// to do that we use the current angle between the connected brick and remove the static angle between them
					// to only get the flexible angle.
					float angleInDegree = 0.0f;
					if (nextBrick != null)
						angleInDegree = simplifyAngle(nextBrick.Orientation - currentBrick.Orientation - link.mAngleBetween);
					flexChain.mBoneList[0].localAngleInRad = angleInDegree * (Math.PI / 180);
				}

				// advance to the next link
				currentBrick = nextBrick;
				currentFirstConnection = nextFirstConnection;
			}

			// store the root hinge connection index (the last hinge found is the flexible root)
			if (hingedLink != null)
			{
				flexChain.mRootHingedLinkIndex = flexChain.mFlexChainList.IndexOf(hingedLink);
				if (hingedLink.mSecondConnection == null)
					flexChain.mRootHingedLinkIndex++;
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
			// reverse the Y because BlueBrick use an indirect coord sys, and the IKSolver a direct one
			IKSolver.CCDResult result = IKSolver.CalcIK_2D_CCD(ref mBoneList, xWorldStudCoord, -yWorldStudCoord, 0.5);
			computeBrickPositionAndOrientation();
			return (result == IKSolver.CCDResult.Processing);
		}

		/// <summary>
		/// compute the position and orientation of all the bricks along the Flex Chain
		/// based on the angle of each rotationable (flexible) connection point
		/// </summary>
		private void computeBrickPositionAndOrientation()
		{
			// start with the first bone of the list
			int boneIndex = 0;
			IKSolver.Bone_2D_CCD currentBone = mBoneList[boneIndex];
			// start with a null total orientation that we will increase with the angle of every bone
			float flexibleCumulativeOrientation = 0.0f;
			float staticCumulativeOrientation = 0.0f;

			// iterate on the link list and change the world angle everytime we meet an hinge
			// start with the first hinged connection of the chain (because everything before doesn't move)
			for (int linkIndex = mRootHingedLinkIndex; linkIndex < mFlexChainList.Count; ++linkIndex)
			{
				// get the previous and current brick
				ChainLink currentLink = mFlexChainList[linkIndex];
				LayerBrick.Brick.ConnectionPoint previousConnection = currentLink.mFirstConnection;
				PointF previousPosition = previousConnection.PositionInStudWorldCoord;
				LayerBrick.Brick.ConnectionPoint currentConnection = currentLink.mSecondConnection;
				LayerBrick.Brick currentBrick = currentConnection.mMyBrick;

				// check if we reach an hinge connection
				if (currentConnection == currentBone.connectionPoint)
				{
					// set the new world position to the current bone with the previous connection position
					// because the previous brick was already placed at the correct position
					currentBone.worldX = previousPosition.X;
					currentBone.worldY = -previousPosition.Y; // BlueBrick use an indirect coord sys, and the IKSolver a direct one
					// increase the flexible angle
					flexibleCumulativeOrientation += (float)(currentBone.localAngleInRad * (180.0 / Math.PI));
					// take the next bone
					boneIndex++;
					if (boneIndex < mBoneList.Count)
						currentBone = mBoneList[boneIndex];
				}

				// add the difference of orientation between the previous brick and current brick through their linked connections
				staticCumulativeOrientation += currentLink.mAngleBetween;

				// set the orientation of the current brick
				currentBrick.Orientation = -flexibleCumulativeOrientation - staticCumulativeOrientation;
				
				// compute the new position of the current brick by putting the current connection at the same
				// place than the previous connection
				PointF newBrickPosition = currentBrick.Position;
				newBrickPosition.X += previousPosition.X - currentConnection.PositionInStudWorldCoord.X;
				newBrickPosition.Y += previousPosition.Y - currentConnection.PositionInStudWorldCoord.Y;
				currentBrick.Position = newBrickPosition;
			}

			// update the last bone position
			if (mBoneList.Count > 0)
			{
				int lastIndex = mBoneList.Count - 1;
				// get the last position
				PointF lastPosition = mBoneList[lastIndex].connectionPoint.PositionInStudWorldCoord;
				// and set it in the last bone
				mBoneList[lastIndex].worldX = lastPosition.X;
				mBoneList[lastIndex].worldY = -lastPosition.Y; // BlueBrick use an indirect coord sys, and the IKSolver a direct one
			}
		}
	}
}
