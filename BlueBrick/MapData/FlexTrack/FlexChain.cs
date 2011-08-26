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
		private float mInitialStaticCumulativeOrientation = 0.0f;
		private List<Layer.LayerItem> mBricksInTheFlexChain = null;

		#region get/set
		public List<Layer.LayerItem> BricksInTheFlexChain
		{
			get { return mBricksInTheFlexChain; }
		}
		#endregion

		/// <summary>
		/// Private constructor because there are condition to create it. Use static method createFlexChain instead.
		/// </summary>
		private FlexChain(int boneCount)
		{
			mBoneList = new List<IKSolver.Bone_2D_CCD>(boneCount);
			mFlexChainList = new List<ChainLink>(boneCount * 2);
			mBricksInTheFlexChain = new List<Layer.LayerItem>(boneCount);
		}

		private static float sSimplifyAngle(float angle)
		{
			if (angle <= -360.0f)
				angle += 360.0f;
			if (angle >= 360.0f)
				angle -= 360.0f;
			return angle;
		}

		private static void sAddNewBone(FlexChain flexChain, LayerBrick.Brick.ConnectionPoint connection, double maxAngleInDeg)
		{
			IKSolver.Bone_2D_CCD newBone = new IKSolver.Bone_2D_CCD();
			newBone.maxAbsoluteAngleInRad = maxAngleInDeg * (Math.PI / 180);
			newBone.worldX = connection.PositionInStudWorldCoord.X;
			newBone.worldY = -connection.PositionInStudWorldCoord.Y; // BlueBrick use an indirect coord sys, and the IKSolver a direct one
			newBone.connectionPoint = connection;
			flexChain.mBoneList.Insert(0, newBone);
		}

		/// <summary>
		/// read and set the two parameter to follow a chain of brick until we found a flexible part. This function only
		/// advance the search for one step (one link).
		/// </summary>
		/// <param name="connectionLead">the current connection point, from which we need to find the next one</param>
		/// <param name="isConnectionLeadingToFlexiblePart">a flag to tell if a flexible brick is found</param>
		private static void sFollowConnectionToFindFlexiblePart(ref LayerBrick.Brick.ConnectionPoint connectionLead, ref bool isConnectionLeadingToFlexiblePart)
		{
			if (connectionLead != null)
			{
				// set the flag if not already true
				isConnectionLeadingToFlexiblePart |= (BrickLibrary.Instance.getConnexionHingeAngle(connectionLead.Type) != 0.0f);
				// if we found a flexible part, stop following the path
				if (isConnectionLeadingToFlexiblePart)
				{
					connectionLead = null; // stop following this path
				}
				else
				{
					// jump to the attach brick accross the link (which may be null, in such case, the search will stop)
					connectionLead = connectionLead.ConnectionLink;
					// if there's a brick connected, check if it is a 2 connections brick and take the other
					if (connectionLead != null)
					{
						// get the list of connection point for the first leading brick
						List<LayerBrick.Brick.ConnectionPoint> connectionList = connectionLead.mMyBrick.ConnectionPoints;
						if (connectionList.Count == 2)
						{
							// check the next connections and set it
							int otherIndex = (connectionList[0] == connectionLead) ? 1 : 0;
							connectionLead = connectionList[otherIndex];
						}
						else
						{
							// if it is a 0, 1 or 3 or more connections brick type, stop the search
							connectionLead = null;
						}
					}
				}
			}
		}

		/// <summary>
		/// This function try different strategy to find the best connection point to use on the specified grabbed part
		/// to start the flex chain. It is assumed that the grab part only have two connection, then:
		/// 1) if one connection is flexible and not the other, return this one,
		/// 2) else if one connection leads to a flexible connection and not the other, return this one
		/// 3) else if one connection is connected to a brick part of the track list and not the other, return this one,
		/// 4) else if both connections are or lead to a flexible one, return the connection closest to the mouse click
		/// 5) else if no connection are or lead to a flexible one, return null (no flexible chain can be created)
		/// </summary>
		/// <param name="trackList">A set of track hopefully connected together, and hopefully containing flex track</param>
		/// <param name="grabbedTrack">The part which will try to reach the target. It should be part of the list.</param>
		/// <param name="mouseCoordInStud">the coordinate of the mouse in stud coord</param>
		/// <returns>The best connection point of the grabbed part or null</returns>
		private static LayerBrick.Brick.ConnectionPoint sFindStartingConnectionPoint(List<Layer.LayerItem> trackList, LayerBrick.Brick grabbedTrack, PointF mouseCoordInStud)
		{
			// try to find the best first connection point which will be the end target of the flex.
			LayerBrick.Brick.ConnectionPoint grabbedFirstConnection = grabbedTrack.ConnectionPoints[0];
			LayerBrick.Brick.ConnectionPoint grabbedSecondConnection = grabbedTrack.ConnectionPoints[1];
			// by preference choose the not flexible connection on the grabbed part
			float firstHingeAngle = BrickLibrary.Instance.getConnexionHingeAngle(grabbedFirstConnection.Type);
			float secondHingeAngle = BrickLibrary.Instance.getConnexionHingeAngle(grabbedSecondConnection.Type);
			if (firstHingeAngle != 0.0f && secondHingeAngle == 0.0f)
				return grabbedSecondConnection;
			else if (firstHingeAngle == 0.0f && secondHingeAngle != 0.0f)
				return grabbedFirstConnection;
			else if (firstHingeAngle == 0.0f && secondHingeAngle == 0.0f)
			{
				// neither connection are flexibles on the grab part, so we choose the connection that 
				// do not lead to a flexible part (and the other connection does)
				bool firstConnectionLeadToFlexiblePart = false;
				bool secondConnectionLeadToFlexiblePart = false;
				LayerBrick.Brick.ConnectionPoint firstConnectionLead = grabbedFirstConnection;
				LayerBrick.Brick.ConnectionPoint secondConnectionLead = grabbedSecondConnection;
				// use a clone of the track list because we will remove the tested track to avoid infinite loop
				// in case of a close track chain
				List<Layer.LayerItem> remaingTrackToTestList = new List<Layer.LayerItem>(trackList);
				while ((!firstConnectionLeadToFlexiblePart || !secondConnectionLeadToFlexiblePart) &&
						(firstConnectionLead != null || secondConnectionLead != null))
				{
					// check if the leading bricks are in the trask list otherwise stop to follow the lead
					LayerBrick.Brick firstBrickLead = (firstConnectionLead != null) ? firstConnectionLead.mMyBrick : null;
					LayerBrick.Brick secondBrickLead = (secondConnectionLead != null) ? secondConnectionLead.mMyBrick : null;
					if (!remaingTrackToTestList.Contains(firstBrickLead))
						firstConnectionLead = null;
					if (!remaingTrackToTestList.Contains(secondBrickLead))
						secondConnectionLead = null;
					// remove those check brick from the list. Do it after and not in the if because first and second brick
					// may be the same and flexible, so it could be unfair to the second test
					remaingTrackToTestList.Remove(firstBrickLead);
					remaingTrackToTestList.Remove(secondBrickLead);
					// set the flag and follow the lead for the two leads
					sFollowConnectionToFindFlexiblePart(ref firstConnectionLead, ref firstConnectionLeadToFlexiblePart);
					sFollowConnectionToFindFlexiblePart(ref secondConnectionLead, ref secondConnectionLeadToFlexiblePart);
				}
				// either the loop is finished because we found flexible part on both side, or because we
				// finished to explore the two paths. Check if we have only one side leading to a flexible part
				// and choose the other one, otherwise go to the default method
				if (firstConnectionLeadToFlexiblePart && !secondConnectionLeadToFlexiblePart)
					return grabbedSecondConnection;
				else if (!firstConnectionLeadToFlexiblePart && secondConnectionLeadToFlexiblePart)
					return grabbedFirstConnection;
				else if (!firstConnectionLeadToFlexiblePart && !secondConnectionLeadToFlexiblePart)
					return null; // neither connection lead to a flexible part
			}

			// both connection of the grab part are flexibles, or both side of the grab part
			// lead to a flexible part, so we choose the connection which neighboor is not in the list
			bool isGrabbedFirstNeighborInList = trackList.Contains(grabbedFirstConnection.ConnectedBrick);
			bool isGrabbedSecondNeighborInList = trackList.Contains(grabbedSecondConnection.ConnectedBrick);
			if (isGrabbedFirstNeighborInList && !isGrabbedSecondNeighborInList)
				return grabbedSecondConnection;
			else if (!isGrabbedFirstNeighborInList && isGrabbedSecondNeighborInList)
				return grabbedFirstConnection;
			else
			{
				// both or neither neighboor are in the list, so we choose the closest connection to the mouse
				PointF distToFirstConnection = new PointF(grabbedFirstConnection.PositionInStudWorldCoord.X - mouseCoordInStud.X, grabbedFirstConnection.PositionInStudWorldCoord.Y - mouseCoordInStud.Y);
				PointF distToSecondConnection = new PointF(grabbedSecondConnection.PositionInStudWorldCoord.X - mouseCoordInStud.X, grabbedSecondConnection.PositionInStudWorldCoord.Y - mouseCoordInStud.Y);
				float squareDistToFirstConnection = (distToFirstConnection.X * distToFirstConnection.X) + (distToFirstConnection.Y * distToFirstConnection.Y);
				float squareDistToSecondConnection = (distToSecondConnection.X * distToSecondConnection.X) + (distToSecondConnection.Y * distToSecondConnection.Y);
				if (squareDistToFirstConnection < squareDistToSecondConnection)
					return grabbedFirstConnection;
				else
					return grabbedSecondConnection;
			}
		}

		/// <summary>
		/// Create a flex chain from a set of brick given a starting brick. From the free connection point
		/// of this starting brick go through all the linked connection points while the brick only have
		/// two connections. If the brick has only one or more than 2 connection (like a joint or crossing)
		/// we stop the chain.
		/// </summary>
		/// <param name="trackList">A set of track hopefully connected together, and hopefully containing flex track</param>
		/// <param name="grabbedTrack">The part which will try to reach the target. It should be part of the list.</param>
		/// <param name="mouseCoordInStud">the coordinate of the mouse in stud coord</param>
		public static FlexChain sCeateFlexChain(List<Layer.LayerItem> trackList, LayerBrick.Brick grabbedTrack, PointF mouseCoordInStud)
		{
			// check that the grabbed part is a part with 2 connection points
			if (!grabbedTrack.HasConnectionPoint || grabbedTrack.ConnectionPoints.Count != 2)
				return null;

			// if the grabbed part is not in the list, it failed
			if (!trackList.Contains(grabbedTrack))
				return null;

			// try to find the starting connection point, if the result is null, the flex chain cannot be created
			LayerBrick.Brick.ConnectionPoint currentFirstConnection = sFindStartingConnectionPoint(trackList, grabbedTrack, mouseCoordInStud);
			if (currentFirstConnection == null)
				return null;

			// the chain to return
			FlexChain flexChain = new FlexChain(trackList.Count);

			// start to iterate from the grabbed track
			LayerBrick.Brick currentBrick = grabbedTrack;
			ChainLink hingedLink = null;
			sAddNewBone(flexChain, currentFirstConnection, 0.0);
			// the chain is made with track that exclusively has 2 connections to make a chain.
			while ((currentBrick != null) && (currentBrick.ConnectionPoints.Count == 2) && trackList.Contains(currentBrick))
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
					sAddNewBone(flexChain, currentSecondConnection, hingeAngle);
					// compute the current angle between the hinge connection and set it to the current bone
					// to do that we use the current angle between the connected brick and remove the static angle between them
					// to only get the flexible angle.
					float angleInDegree = 0.0f;
					if (nextBrick != null)
						angleInDegree = sSimplifyAngle(nextBrick.Orientation - currentBrick.Orientation - link.mAngleBetween);
					flexChain.mBoneList[0].localAngleInRad = angleInDegree * (Math.PI / 180);

					// save the initial static cumulative orientation: start with the orientation of the root brick
					// if the hinge is connected to a brick, otherwise, if the hinge is free (connected to the world)
					// use the orientation of the hinge brick.
					// we set the value several time in the loop, in order to set it with the brick directly
					// connected to the last hinge in the chain
					if (nextBrick != null)
						flexChain.mInitialStaticCumulativeOrientation = -nextBrick.Orientation;
					else
						flexChain.mInitialStaticCumulativeOrientation = -currentBrick.Orientation;
				}

				// advance to the next link
				currentBrick = nextBrick;
				currentFirstConnection = nextFirstConnection;

				// check if the track is not a loop, otherwise we will have an infinite loop.
				// but don't add the test in the while because the first iteration must pass
				if (currentBrick == grabbedTrack)
					break;
			}

			// store the root hinge connection index (the last hinge found is the flexible root)
			if (hingedLink != null)
				flexChain.mRootHingedLinkIndex = flexChain.mFlexChainList.IndexOf(hingedLink);

			// add the current brick in the list of bricks, by not going further than the root brick
			LayerBrick.Brick.ConnectionPoint rootConnection = flexChain.mFlexChainList[flexChain.mRootHingedLinkIndex].mFirstConnection;
			if (rootConnection.mMyBrick != null)
				flexChain.mBricksInTheFlexChain.Add(rootConnection.mMyBrick);
			for (int i = flexChain.mRootHingedLinkIndex; i < flexChain.mFlexChainList.Count; ++i)
				flexChain.mBricksInTheFlexChain.Add(flexChain.mFlexChainList[i].mSecondConnection.mMyBrick);

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
			// start with a null total flexible orientation that we will increase with the angle of every bone
			float flexibleCumulativeOrientation = 0.0f;
			// init the static cumulative orientation with the one saved in this class.
			// we cannot use the orientation of the root brick of the chain because this brick orientation
			// is also changed in the loop, leading to some divergence
			float staticCumulativeOrientation = mInitialStaticCumulativeOrientation;

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
