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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using BlueBrick.Actions;
using BlueBrick.Actions.Bricks;
using System.Xml.Serialization;
using System.Collections;
using BlueBrick.MapData.Tools;

namespace BlueBrick.MapData
{
	[Serializable]
	public partial class LayerBrick : Layer
	{
				/// <summary>
		/// describe all the action that can be done with a mouse when editing a text
		/// </summary>
		private enum EditAction
		{
			NONE,
			MOVE_SELECTION,
			DUPLICATE_SELECTION,
			FLEX_MOVE,
		}

		[NonSerialized]
		private ImageAttributes mImageAttributeForSelection = new ImageAttributes();
		[NonSerialized]
		private ImageAttributes mImageAttributeForSnapping = new ImageAttributes();
		[NonSerialized]
		private ImageAttributes mImageAttributeDefault = new ImageAttributes();

		// list of bricks and connection points
		private List<Brick> mBricks = new List<Brick>(); // all the bricks in the layer
		private FreeConnectionSet mFreeConnectionPoints = new FreeConnectionSet();

		//related to selection
		private Brick mCurrentBrickUnderMouse = null; // this is the single brick under the mouse even if this brick belongs to a group
		private PointF mMouseDownInitialPosition;
		private PointF mMouseDownLastPosition;
		private PointF mMouseGrabDeltaToCenter; // The delta between the grab point of the mouse inside the grabed brick, to the center of that brick
		private PointF mMouseGrabDeltaToActiveConnectionPoint; // The delta between the grab point of the mouse inside the grabed brick, to the active connection point of that brick
		private bool mMouseIsBetweenDownAndUpEvent = false;
		private bool mMouseHasMoved = false;
		private EditAction mEditAction = EditAction.NONE;
		private FlexMove mMouseFlexMoveAction = null;
		private RotateBrickOnPivotBrick mRotationForSnappingDuringBrickMove = null; // this action is used temporally during the edition, while you are moving the selection next to a connectable brick. The Action is not recorded in the ActionManager because it is a temporary one.
		private float mSnappingOrientation = 0.0f; // this orientation is just used during the the edition of a group of part if they snap to a free connexion point

		#region get/set
		/// <summary>
		/// A readonly accessor on the brick list for saving in different fomat other than the standard BB XML serialization
		/// </summary>
		public List<Brick> BrickList
		{
			get { return mBricks; }
		}

		/// <summary>
		/// A readonly accessor on the list of brick in that layer, but the list contains only the bricks visible in the
		/// library. It may contains bricks not in library, if the user added a group from the library and ungrouped it
		/// (for deleting part of the group, for example). This brick list is computed, use with parcimony.
		/// </summary>
		public List<LayerItem> LibraryBrickList
		{
			get { return sFilterListToGetOnlyBricksInLibrary(mBricks); }
		}
		
		/// <summary>
		/// get the type name id of this type of layer used in the xml file (not localized)
		/// </summary>
		public override string XmlTypeName
		{
			get { return "brick"; }
		}

		/// <summary>
		/// get the localized name of this type of layer
		/// </summary>
		public override string LocalizedTypeName
		{
			get { return Properties.Resources.ErrorMsgLayerTypeBrick; }
		}

		public override int Transparency
		{
			set
			{
				mTransparency = value;
				ColorMatrix colorMatrix = new ColorMatrix();
				colorMatrix.Matrix33 = (float)value / 100.0f;
				mImageAttributeDefault.SetColorMatrix(colorMatrix);
				mImageAttributeForSelection.SetColorMatrix(colorMatrix);
				mImageAttributeForSnapping.SetColorMatrix(colorMatrix);
			}
		}

		/// <summary>
		/// Get the number of Bricks in this layer.
		/// </summary>
		public override int NbItems
		{
			get { return mBricks.Count; }
		}
		#endregion

		#region constructor
		public LayerBrick()
		{
			// update the gamma setting when the layer is created
			updateGammaFromSettings();
		}

		public void updateGammaFromSettings()
		{
			mImageAttributeForSelection.SetGamma(Properties.Settings.Default.GammaForSelection);
			mImageAttributeForSnapping.SetGamma(Properties.Settings.Default.GammaForSnappingPart);
		}
		#endregion

		#region XmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			// call the common reader class
			base.ReadXml(reader);

			// read all the bricks
			readItemsListFromXml<Brick>(reader, ref mBricks, "Bricks", true);

			// reconstruct the freeConnexion points list by iterating on all the connexion of all the bricks
			mFreeConnectionPoints.removeAll();
			foreach (Brick brick in mBricks)
				if (brick.ConnectionPoints != null) // do not use brick.HasConnectionPoints here
					foreach (Brick.ConnectionPoint connexion in brick.ConnectionPoints)
					{
						// 1)
						// check if we need to break a link because it is not valid 
						// this situation can happen when you load an unknow part that had
						// some connexion point before in the XML file. In that case the type
						// will be the DEFAULT one because the brick is unknown to the library
						// so the library doesn't know which type are the connection of this brick
						// 2)
						// Also happen when a part description changed in the part lib, so all
						// the connection of type DEFAULT are the trailing connection in the file
						// that does not exist anymore in the part library. So if any of the
						// two connection is of type DEFAULT, we can break the connection.
						// But you can also change a part description by changing the type of the
						// connection, so the link saved in the BBM file should be broken if one
						// connection change its type and not the other one, that's why we check
						// if the two connections of the link are still of the same type.
						// 3)
						// check if we need to break the link because two connexion point are not anymore at
						// the same place. This can happen if the file was save with a first version of the
						// library, and then we change the library and we change the connexion position.
						// So the parts are not move, but the links should be broken
						if ((connexion.Type == BrickLibrary.ConnectionType.DEFAULT) || // case 1)
								((connexion.ConnectionLink != null) &&
									((connexion.ConnectionLink.Type != connexion.Type) || // case 2)
									 !arePositionsEqual(connexion.PositionInStudWorldCoord, connexion.ConnectionLink.PositionInStudWorldCoord)))) // case 3)
						{
							// we don't use the disconnect method here, because the disconnect method
							// add the two connexion in the free connexion list, but we want to do it after.
							if (connexion.ConnectionLink != null)
								connexion.ConnectionLink.ConnectionLink = null;
							connexion.ConnectionLink = null;
						}
						// add the connexion in the free list if it is free
						if (connexion.IsFree)
							mFreeConnectionPoints.add(connexion);
					}

			// update the electric circuit on the whole layer
			ElectricCircuitChecker.check(this);
		}

		protected override T readItem<T>(System.Xml.XmlReader reader)
		{
			Brick brick = new Brick();
			brick.ReadXml(reader);
			return (brick as T);
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			// write the header
			writeHeaderAndCommonProperties(writer);
			// write all the bricks
			writeItemsListToXml(writer, mBricks, "Bricks", true);
			// write the footer
			writeFooter(writer);
		}
		#endregion

		#region action on the layer
		#region add/remove bricks
		/// <summary>
		///	Add the specified brick at the specified position in the list
		/// </summary>
		public void addBrick(Brick brickToAdd, int index)
		{
			// add its connection points to the free list
			mFreeConnectionPoints.addAllBrickConnections(brickToAdd);

			// add the brick in the list
			if (index < 0)
				mBricks.Add(brickToAdd);
			else
				mBricks.Insert(index, brickToAdd);

			// reattach its rulers (if it had some previously)
			brickToAdd.reattachAllRulersTemporarilyDetached();
		}

		/// <summary>
		///	Add the specified brick at the specified position in the list.
		/// This method is specifically design for the reordering of the list, so only
		/// the actions to bring to front or send to back should use it
		/// </summary>
		public void addBrickWithoutChangingConnectivity(Brick brickToAdd, int index)
		{
			// add the brick in the list
			if (index < 0)
				mBricks.Add(brickToAdd);
			else
				mBricks.Insert(index, brickToAdd);
		}

		/// <summary>
		/// Remove the specified Brick
		/// </summary>
		/// <param name="brickToRemove">the brick you want to remove</param>
		/// <returns>the previous index of the cell deleted</returns>
		public int removeBrick(Brick brickToRemove)
		{
			int index = mBricks.IndexOf(brickToRemove);
			if (index >= 0)
			{
				// remove its connextion points
				if (brickToRemove.HasConnectionPoint)
					foreach (Brick.ConnectionPoint connexion in brickToRemove.ConnectionPoints)
					{
						// get the eventually connected brick for the current connection before breaking the link
						Brick connectedBrick = connexion.ConnectedBrick;
						// break the links if there's a link
						if (connexion.ConnectionLink != null)
						{
							mFreeConnectionPoints.add(connexion.ConnectionLink);
							connexion.ConnectionLink.ConnectionLink = null;
						}
						// remove the connection from the free connection list because we will delete the part
						mFreeConnectionPoints.remove(connexion);
						connexion.ConnectionLink = null;
						// after the link is fully break, update the electric circuit on the connected brick
						// not on the brick we are removing, since we are removing it
						if (connectedBrick != null)
							ElectricCircuitChecker.check(connectedBrick);
					}
				// detach its rulers
				brickToRemove.detachAllRulersTemporarily();

				// remove the brick
				mBricks.Remove(brickToRemove);
				// remove also the item from the selection list if in it
				if (mSelectedObjects.Contains(brickToRemove))
					removeObjectFromSelection(brickToRemove);
			}
			else
			{
				index = 0;
			}

			return index;
		}

		/// <summary>
		/// Remove the specified Brick.
		/// This method is specifically design for the reordering of the list, so only
		/// the actions to bring to front or send to back should use it
		/// </summary>
		/// <param name="brickToRemove">the brick you want to remove</param>
		/// <returns>the previous index of the cell deleted</returns>
		public int removeBrickWithoutChangingConnectivity(Brick brickToRemove)
		{
			int index = mBricks.IndexOf(brickToRemove);
			if (index >= 0)
			{
				// remove the brick
				mBricks.Remove(brickToRemove);
				// remove also the item from the selection list if in it
				if (mSelectedObjects.Contains(brickToRemove))
					removeObjectFromSelection(brickToRemove);
			}
			else
			{
				index = 0;
			}

			return index;
		}
		#endregion

		#region selection
		/// <summary>
		/// Copy the list of the selected bricks in a separate list for later use.
		/// This method should be called on a CTRL+C
		/// </summary>
		public override void copyCurrentSelectionToClipboard()
		{
			base.copyCurrentSelectionToClipboard(mBricks);
		}

		/// <summary>
		/// Select all the items in this layer.
		/// </summary>
		public override void selectAll()
		{
			// clear the selection and add all the item of this layer
			clearSelection();
			addObjectInSelection(mBricks);
		}

		/// <summary>
		/// This method return the unique brick in the current selection to which you can connect another brick
		/// or null if there's no suitable brick candidate for connection.
		/// If there's only one brick selected, this brick is return, otherwise check
		/// if all the bricks selected belongs to the same hierarchical tree of group,
		/// meaning the top group parent is the same for all the bricks.
		/// If yes, then it return the brick that hold the active connection in the group (which can be null)
		/// </summary>
		/// <returns>The brick that is connectable and should display its active connection point, or null</returns>
		public Brick getConnectableBrick()
		{
			LayerItem topItem = Layer.sGetTopItemFromList(mSelectedObjects);
			if (topItem != null)
			{
				if (topItem.IsAGroup)
					return (topItem as Group).BrickThatHoldsActiveConnection;
				else
					return (topItem as Brick);
			}
			return null;
		}
		#endregion

		#region connectivity
		/// <summary>
		/// Connect the two connexion if possible (i.e. if both connexion are free)
		/// </summary>
		/// <param name="connexion1">the first connexion to connect with the second one</param>
		/// <param name="connexion2">the second connexion to connect with the first one</param>
		/// <param name="checkElectricShortcut">boolean to tell if we need to check the electric circuits</param>
		/// <returns>true if the connexion was made, else false.</returns>
		private bool connectTwoConnectionPoints(Brick.ConnectionPoint connexion1, Brick.ConnectionPoint connexion2, bool checkElectricShortcut)
		{
			// the connexion can never be stolen
			if (connexion1.IsFree && connexion2.IsFree)
			{
				connexion1.ConnectionLink = connexion2;
				connexion2.ConnectionLink = connexion1;
				mFreeConnectionPoints.remove(connexion1);
				mFreeConnectionPoints.remove(connexion2);
				// check the current for the new connection (only one call with one brick is enough, since the two bricks are connected)
				if (checkElectricShortcut)
					ElectricCircuitChecker.check(connexion1.mMyBrick);
				return true;
			}
			return false;
		}

		private void disconnectTwoConnectionPoints(Brick.ConnectionPoint connexion1, Brick.ConnectionPoint connexion2)
		{
			// first break the link on both connections
			if (connexion1 != null)
			{
				connexion1.ConnectionLink = null;
				mFreeConnectionPoints.add(connexion1);
			}
			if (connexion2 != null)
			{
				connexion2.ConnectionLink = null;
				mFreeConnectionPoints.add(connexion2);
			}

			// check the electric circuit on both brick after the link is broken totally
			if (connexion1 != null)
				ElectricCircuitChecker.check(connexion1.mMyBrick);
			if (connexion2 != null)
				ElectricCircuitChecker.check(connexion2.mMyBrick);
		}

		private bool arePositionsEqual(PointF pos1, PointF pos2)
		{
			if (Math.Abs(pos1.X - pos2.X) < 0.5)
				return (Math.Abs(pos1.Y - pos2.Y) < 0.5);
			return false;
		}

		/// <summary>
		/// Update the connectivity of all the selected bricks base of their positions
		/// </summary>
		/// <param name="breakLinkOnly">if true only update the disconnection, that means only break some links, do not create new links</param>
		public void updateBrickConnectivityOfSelection(bool breakLinkOnly)
		{
			//--- DISCONNEXION FIRST
			// search amond the selected bricks all the connexions that does not connect to another brick in the selection
			// then check the position of the two connected point to know if we must break the link
			foreach (Brick brick in mSelectedObjects)
				if (brick.HasConnectionPoint)
					foreach (Brick.ConnectionPoint connexion in brick.ConnectionPoints)
						if ((connexion.ConnectionLink != null) && !mSelectedObjects.Contains(connexion.ConnectionLink.mMyBrick))
						{
							// check if we need to brake the link
							if (!arePositionsEqual(connexion.PositionInStudWorldCoord, connexion.ConnectionLink.PositionInStudWorldCoord))
								disconnectTwoConnectionPoints(connexion, connexion.ConnectionLink);
						}

			//--- NEW CONNEXION
			if (!breakLinkOnly)
			{
				// build two lists from the free connection points, one in the selection, and one for the others
				FreeConnectionSet connexionPointsInSelection = new FreeConnectionSet();
				FreeConnectionSet freeConnexionPoints = new FreeConnectionSet();

				int connectionTypeCount = mFreeConnectionPoints.ConnectionTypeCount;
				for (int i = 0; i < connectionTypeCount; ++i)
				{
					foreach (Brick.ConnectionPoint connexion in mFreeConnectionPoints.getListForType(i))
						if (mSelectedObjects.Contains(connexion.mMyBrick))
							connexionPointsInSelection.add(connexion);
						else
							freeConnexionPoints.add(connexion);
				}

				// now iterate on the free connexion point in selection to search where to connect
				for (int i = 0; i < connectionTypeCount; ++i)
					foreach (Brick.ConnectionPoint selConnexion in connexionPointsInSelection.getListForType(i))
					{
						// try to find a new connection
						foreach (Brick.ConnectionPoint freeConnexion in freeConnexionPoints.getListForType(i))
							if (arePositionsEqual(selConnexion.PositionInStudWorldCoord, freeConnexion.PositionInStudWorldCoord))
								connectTwoConnectionPoints(selConnexion, freeConnexion, true);
					}
			}
		}

		/// <summary>
		/// update the connectivity of the specified brick with all possible bricks on the map.
		/// This method doesn't break existing connection for the brick, only create new links.
		/// </summary>
		/// <param name="brick">the brick for which we need to check the connectivity</param>
		public void updateFullBrickConnectivityForOneBrick(Brick brick)
		{
			updateFullBrickConnectivityForOneBrick(brick, true);
		}

		/// <summary>
		/// update the connectivity of the specified brick with all possible bricks on the map.
		/// This method doesn't break existing connection for the brick, only create new links.
		/// </summary>
		/// <param name="brick">the brick for which we need to check the connectivity</param>
		/// <param name="checkElectricShortcut">boolean to tell if we need to check the electric circuits</param>
		private void updateFullBrickConnectivityForOneBrick(Brick brick, bool checkElectricShortcut)
		{
			if (brick.HasConnectionPoint)
				foreach (Brick.ConnectionPoint brickConnexion in brick.ConnectionPoints)
					if (brickConnexion.IsFree)
					{
						// get the list of freeConnection for the specified type
						List<Brick.ConnectionPoint> freeConnectionList = mFreeConnectionPoints.getListForType(brickConnexion.Type);
						// ask the Count of the list in the for loop because the list can decrease.
						for (int i = 0; i < freeConnectionList.Count; ++i)
						{
							// get the current free connection
							Brick.ConnectionPoint freeConnexion = freeConnectionList[i];
							// check that we are not linking a free connection of the brick with another free connection of
							// the same brick (avoiding linking the brick to itself and at the same time avoiding linking
							// the freeconnection with itself which is at the same place of course)
							// We don't need to check is the type of the connection are the same because we asked the list
							// of the free connection for the specific type of the current connection.
							// and of course the most important is to check that the two connection are at the same place
							if ((freeConnexion.mMyBrick != brick) &&
								arePositionsEqual(brickConnexion.PositionInStudWorldCoord, freeConnexion.PositionInStudWorldCoord))
							{
								if (connectTwoConnectionPoints(brickConnexion, freeConnexion, checkElectricShortcut))
									--i;
							}
						}
					}
		}

		/// <summary>
		/// Update the connectivity of all the selected bricks based on their positions.
		/// This method is quite slow especially if the selection list is big
		/// </summary>
		public void updateFullBrickConnectivityForSelectedBricksOnly()
		{
			// for optimization reason do not update the electric circuit for every brick
			foreach (Layer.LayerItem item in mSelectedObjects)
				updateFullBrickConnectivityForOneBrick(item as Brick, false);

			// update the electric circuit for the whole layer
			ElectricCircuitChecker.check(this);
		}

		/// <summary>
		/// Update the connectivity of all the bricks based on their positions
		/// This method is slow since the whole connectivity is recompute. It should only be call after
		/// an import of a map from a file format that doesn't contain the connectivity info, such as LDraw format
		/// </summary>
		public void updateFullBrickConnectivity()
		{
			// for optimization reason do not update the electric circuit for every brick
			foreach (Brick brick in mBricks)
				updateFullBrickConnectivityForOneBrick(brick, false);

			// update the electric circuit for the whole layer
			ElectricCircuitChecker.check(this);
		}
		#endregion 

		#region altitude
		/// <summary>
		/// A delegate to compare two bricks by altitude
		/// </summary>
		/// <param name="brick1">the first brick to compare</param>
		/// <param name="brick2">the second brick to compare</param>
		/// <returns></returns>
		private static int CompareBricksByAltitudeDelegate(Brick brick1, Brick brick2)
		{
			if (brick1.Altitude > brick2.Altitude)
				return -1;
			if (brick1.Altitude < brick2.Altitude)
				return 1;
			return 0;
		}

		/// <summary>
		/// This method sort the array of bricks according to the altitude of each bricks,
		/// such as the higher bricks are displayed last, and appear on top.
		/// This function is usefull when we load a LDRAW or TD file that contains altitude
		/// </summary>
		public void sortBricksByAltitude()
		{
			mBricks.Sort(CompareBricksByAltitudeDelegate);
		}
		#endregion

		#region recompute image
		/// <summary>
		/// recompute all the pictures of all the brick of all the brick layers 
		/// </summary>
		public void recomputeBrickMipmapImages()
		{
			foreach (Brick brick in mBricks)
				brick.clearMipmapImages(0, 1);
		}
		#endregion
		#endregion

		#region draw

		/// <summary>
		/// Get the brick in this layer that is placed on the most top left place
		/// </summary>
		/// <returns></returns>
		public PointF getMostTopLeftBrickPosition()
		{
			PointF result = new PointF(float.MaxValue, float.MaxValue);
			// iterate on all the bricks
			foreach (Brick brick in mBricks)
			{
				if (brick.Position.X < result.X)
					result.X = brick.Position.X;
				if (brick.Position.Y < result.Y)
					result.Y = brick.Position.Y;
			}
			return result;
		}

		/// <summary>
		/// get the total area in stud covered by all the bricks in this layer
		/// </summary>
		/// <returns></returns>
		public override RectangleF getTotalAreaInStud()
		{
			return getTotalAreaInStud(mBricks);
		}

		/// <summary>
		/// Draw the layer.
		/// </summary>
		/// <param name="g">the graphic context in which draw the layer</param>
		/// <param name="area">the area in layer pixel</param>
		public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
		{
			if (!Visible)
				return;

			// compute the mipmap level according to the current scale
			int mipmapLevel = 0;
			if (scalePixelPerStud < 0.75f)
				mipmapLevel = 4;
			else if (scalePixelPerStud < 1.5f)
				mipmapLevel = 3;
			else if (scalePixelPerStud < 3.0f)
				mipmapLevel = 2;
			else if (scalePixelPerStud < 6.0f)
				mipmapLevel = 1;
			else
				mipmapLevel = 0;

			// compute the transparency on one byte
			int alphaValue = (255 * mTransparency) / 100;

			// create a list of visible electric brick
			List<Brick> visibleElectricBricks = new List<Brick>();

			// iterate on all the bricks
			Rectangle destinationRectangle = new Rectangle();
			foreach (Brick brick in mBricks)
			{
				float left = brick.Position.X;
				float right = left + brick.Width;
				float top = brick.Position.Y;
				float bottom = top + brick.Height;
				if ((right >= areaInStud.Left) && (left <= areaInStud.Right) && (bottom >= areaInStud.Top) && (top <= areaInStud.Bottom))
				{
					Image image = brick.getImage(mipmapLevel);
					// the return image can be null if too small to be visible
					if (image != null)
					{
						// the -0.5 and +1 is a hack to add 1 more pixel to have jointive baseplates
						destinationRectangle.X = (int)(((left - areaInStud.Left) * scalePixelPerStud) - 0.5f);
						destinationRectangle.Y = (int)(((top - areaInStud.Top) * scalePixelPerStud) - 0.5f);
						destinationRectangle.Width = (int)((brick.Width * scalePixelPerStud) + 1.0f);
						destinationRectangle.Height = (int)((brick.Height * scalePixelPerStud) + 1.0f);

						// draw the current brick eventually highlighted
						if (mSelectedObjects.Contains(brick))
						{
							if (brick == mCurrentBrickUnderMouse)
								g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, mImageAttributeForSnapping);
							else
								g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, mImageAttributeForSelection);
						}
						else
							g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, mImageAttributeDefault);

						if (Properties.Settings.Default.DisplayBrickHull)
                            g.DrawPolygon(sPenToDrawBrickHull, Layer.sConvertPolygonInStudToPixel(brick.SelectionArea.Vertice, areaInStud, scalePixelPerStud));

						// if the brick is electric, add it to the list
						if (brick.ElectricCircuitIndexList != null)
							visibleElectricBricks.Add(brick);
					}
				}
			}

			// draw eventually the electric circuit
			if (BlueBrick.Properties.Settings.Default.DisplayElectricCircuit)
			{
				// compute some constant value for the drawing of the electric circuit
				float ELECTRIC_WIDTH = (float)(2.5 * scalePixelPerStud);
				Pen ELECTRIC_RED_PEN = new Pen(Color.FromArgb(alphaValue, Color.OrangeRed), (float)(0.5 * scalePixelPerStud));
				Pen ELECTRIC_BLUE_PEN = new Pen(Color.FromArgb(alphaValue, Color.Cyan), (float)(0.5 * scalePixelPerStud));

				foreach (Brick brick in visibleElectricBricks)
					foreach (BrickLibrary.Brick.ElectricCircuit circuit in brick.ElectricCircuitIndexList)
					{
						// draw the line between the two connections
						PointF start = brick.ConnectionPoints[circuit.mIndex1].PositionInStudWorldCoord;
						PointF end = brick.ConnectionPoints[circuit.mIndex2].PositionInStudWorldCoord;
						start.X = (float)((start.X - areaInStud.Left) * scalePixelPerStud);
						start.Y = (float)((start.Y - areaInStud.Top) * scalePixelPerStud);
						end.X = (float)((end.X - areaInStud.Left) * scalePixelPerStud);
						end.Y = (float)((end.Y - areaInStud.Top) * scalePixelPerStud);

						// computre the direction vector of the circuit
						float length = (float)(circuit.mDistance * scalePixelPerStud);
						PointF direction = new PointF((end.X - start.X) / length, (end.Y - start.Y) / length);
						// compute the normal of the circuit
						PointF normal = new PointF(-direction.Y * ELECTRIC_WIDTH, direction.X * ELECTRIC_WIDTH);

						// compute the two lines of the circuit
						PointF start1 = new PointF(start.X + normal.X, start.Y + normal.Y);
						PointF end1 = new PointF(end.X + normal.X, end.Y + normal.Y);
						PointF start2 = new PointF(start.X - normal.X, start.Y - normal.Y);
						PointF end2 = new PointF(end.X - normal.X, end.Y - normal.Y);

						// draw the two lines according to the polarity of connection one
						if (brick.ConnectionPoints[circuit.mIndex1].Polarity < 0)
						{
							g.DrawLine(ELECTRIC_BLUE_PEN, start1, end1);
							g.DrawLine(ELECTRIC_RED_PEN, start2, end2);
						}
						else
						{
							g.DrawLine(ELECTRIC_RED_PEN, start1, end1);
							g.DrawLine(ELECTRIC_BLUE_PEN, start2, end2);
						}						
					}

				// pen for the electric shortcut sign
				float SHORTCUT_WIDTH = (float)(3.0 * scalePixelPerStud);
				Pen SHORTCUT_PEN = new Pen(Color.FromArgb(alphaValue, Color.Orange), (float)(1.5 * scalePixelPerStud));

				foreach (Brick brick in visibleElectricBricks)
					foreach (BrickLibrary.Brick.ElectricCircuit circuit in brick.ElectricCircuitIndexList)
					{
						// check if there's a shortcut among the two connections
						int index = -1;
						if (brick.ConnectionPoints[circuit.mIndex1].HasElectricShortcut)
							index = circuit.mIndex1;
						else if (brick.ConnectionPoints[circuit.mIndex2].HasElectricShortcut)
							index = circuit.mIndex2;

						// draw the electric shortcut sign if any
						if (index > -1)
						{
							PointF center = brick.ConnectionPoints[index].PositionInStudWorldCoord;
							center.X = (float)((center.X - areaInStud.Left) * scalePixelPerStud);
							center.Y = (float)((center.Y - areaInStud.Top) * scalePixelPerStud);

							PointF[] vertices = new PointF[]{ new PointF(center.X - SHORTCUT_WIDTH, center.Y),
										new PointF(center.X, center.Y - SHORTCUT_WIDTH),
										new PointF(center.X, center.Y + SHORTCUT_WIDTH),
										new PointF(center.X + SHORTCUT_WIDTH, center.Y) };
							g.DrawLines(SHORTCUT_PEN, vertices);
						}
					}
			}

			// call the base class to draw the surrounding selection rectangle
			base.draw(g, areaInStud, scalePixelPerStud);

			// check if there's a brick for which we need to draw the current connection point (red dot)
			// two conditions: one brick under the mouse, or only one brick selected.
			Brick brickThatHasActiveConnection = null;
			if (mCurrentBrickUnderMouse != null && mCurrentBrickUnderMouse.HasConnectionPoint &&
				mCurrentBrickUnderMouse.ActiveConnectionPoint.IsFree)
			{
				brickThatHasActiveConnection = mCurrentBrickUnderMouse;
			}
			else
			{
				Brick brick = getConnectableBrick();
				if (brick != null && brick.HasConnectionPoint && brick.ActiveConnectionPoint.IsFree)
					brickThatHasActiveConnection = brick;
			}
			// now if the brick is valid, draw the dot of the selected connection
			if (brickThatHasActiveConnection != null)
			{
				float sizeInStud = BrickLibrary.ConnectionType.sSelectedConnection.Size;
				float x = (float)((brickThatHasActiveConnection.ActiveConnectionPosition.X - sizeInStud - areaInStud.Left) * scalePixelPerStud);
				float y = (float)((brickThatHasActiveConnection.ActiveConnectionPosition.Y - sizeInStud - areaInStud.Top) * scalePixelPerStud);
				float size = (float)(sizeInStud * 2 * scalePixelPerStud);
				Brush brush = new SolidBrush(Color.FromArgb((mTransparency * BrickLibrary.ConnectionType.sSelectedConnection.Color.A) / 100, BrickLibrary.ConnectionType.sSelectedConnection.Color));
				g.FillEllipse(brush, x, y, size, size);
			}

			// draw the free connexion points if needed
			if (BlueBrick.Properties.Settings.Default.DisplayFreeConnexionPoints)
				for (int i = 1; i < mFreeConnectionPoints.ConnectionTypeCount; ++i)
				{
					BrickLibrary.ConnectionType connectionType = BrickLibrary.Instance.ConnectionTypes[i];
					Brush brush = new SolidBrush(Color.FromArgb((mTransparency * connectionType.Color.A) / 100, connectionType.Color));
					foreach (Brick.ConnectionPoint connexion in mFreeConnectionPoints.getListForType(i))
					{
						float sizeInStud = connectionType.Size;
						float x = (float)((connexion.PositionInStudWorldCoord.X - sizeInStud - areaInStud.Left) * scalePixelPerStud);
						float y = (float)((connexion.PositionInStudWorldCoord.Y - sizeInStud - areaInStud.Top) * scalePixelPerStud);
						float sizeInPixel = (float)(sizeInStud * 2 * scalePixelPerStud);
						g.FillEllipse(brush, x, y, sizeInPixel, sizeInPixel);
					}
				}

			// check if we need to continue to update the flex move after the drawing
			// the update of the flex move cannot be called during a draw
			if (mMouseFlexMoveAction != null)
			{
				bool needRedraw = mMouseFlexMoveAction.update();
				if (needRedraw)
					MainForm.Instance.updateView(Action.UpdateViewType.FULL, Action.UpdateViewType.NONE);
				// debug draw for the flex
				// mMouseFlexMoveAction.draw(g, areaInStud, scalePixelPerStud);
			}
		}
		#endregion

		#region mouse event
		/// <summary>
		/// Return the cursor that should be display when the mouse is above the map without mouse click
		/// </summary>
		/// <param name="mouseCoordInStud"></param>
		public override Cursor getDefaultCursorWithoutMouseClick(PointF mouseCoordInStud)
		{
			// if the layer is not visible you can basically do nothing on it
			if (!Visible)
			{
				return MainForm.Instance.HiddenLayerCursor;
			}
			else if (mMouseIsBetweenDownAndUpEvent)
			{
				// the second test after the or, is because we give a second chance to the user to duplicate
				// the selection if he press the duplicate key after the mouse down, but before he start to move
				if ((mEditAction == EditAction.DUPLICATE_SELECTION) ||
					((mEditAction == EditAction.MOVE_SELECTION) && !mMouseHasMoved && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)))
					return MainForm.Instance.BrickDuplicateCursor;
				else if (mEditAction == EditAction.MOVE_SELECTION)
					return MainForm.Instance.BrickMoveCursor;
				else if (mEditAction == EditAction.FLEX_MOVE)
					return MainForm.Instance.FlexArrowCursor;
			}
			else
			{
				if (mouseCoordInStud != PointF.Empty)
				{
					if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)
					{
						if (isPointInsideSelectionRectangle(mouseCoordInStud))
							return MainForm.Instance.BrickDuplicateCursor;
					}
					else if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
					{
						return MainForm.Instance.BrickSelectionCursor;
					}
					else if (isPointInsideSelectionRectangle(mouseCoordInStud))
					{
						return MainForm.Instance.BrickMoveCursor;
					}
				}
			}
			// return the default arrow cursor
			return MainForm.Instance.BrickArrowCursor;
		}

		/// <summary>
		/// Get the brick under the specified mouse coordinate or null if there's no brick under.
		/// The search is done in revers order of the list to get the topmost item.
		/// </summary>
		/// <param name="mouseCoordInStud">the coordinate of the mouse cursor, where to look for</param>
		/// <returns>the brick that is under the mouse coordinate or null if there is none.</returns>
		public Brick getBrickUnderMouse(PointF mouseCoordInStud)
		{
			return getLayerItemUnderMouse(mBricks, mouseCoordInStud) as Brick;
		}

		private void setBrickUnderMouse(Brick brick, PointF mouseCoordInStud)
		{
			setBrickUnderMouse(brick, brick, mouseCoordInStud);
		}

		private void setBrickUnderMouse(Brick brickUnderMouse, LayerItem referenceItemForMove, PointF mouseCoordInStud)
		{
			// set the new value
			mCurrentBrickUnderMouse = brickUnderMouse;

			// update the 2 grab distance if you change the brick under the mouse
			if (brickUnderMouse != null)
			{
				// Try to find if this brick belongs to a group in order to use the snap margin of the group
				LayerItem topGroup = brickUnderMouse.TopGroup;
				if ((topGroup == null) || (topGroup.PartNumber == string.Empty))
					topGroup = brickUnderMouse;
				else if (topGroup.IsAGroup)
					(topGroup as Group).computeDisplayArea(true);

				// ------
				// compute the position of the top left corner of the brick or group of brick including the snap margin
				PointF topGroupPosition = topGroup.Position;
				PointF brickCenter = referenceItemForMove.Center;
				PointF currentAnchorCornerPosition = new PointF(topGroupPosition.X + topGroup.SnapToGridOffset.X,
																topGroupPosition.Y + topGroup.SnapToGridOffset.Y);

				// Compute the grab distance to center for snapping on grid (without connection)
				// first compute the center shift (including the snap grid margin)
				PointF anchorCornerToBrickCenter = new PointF(brickCenter.X - currentAnchorCornerPosition.X,
															brickCenter.Y - currentAnchorCornerPosition.Y);

				// we need to compute the vector from the corner to the mouse and not the vector from
				// the mouse to the corner bacause, the snapToGrid is a Floor type of snapping so the
				// resulting snapped vector won't be the same
				PointF anchorCornerToMouseSnapped = Layer.snapToGrid(new PointF(mouseCoordInStud.X - currentAnchorCornerPosition.X,
																				mouseCoordInStud.Y - currentAnchorCornerPosition.Y), false);

				// compute the grab delta between the mouse and the center of the brick 
				// by summing the vector from mouse to the anchor (snapped) with the vector from the anchor to the center
				mMouseGrabDeltaToCenter.X = anchorCornerToBrickCenter.X - anchorCornerToMouseSnapped.X;
				mMouseGrabDeltaToCenter.Y = anchorCornerToBrickCenter.Y - anchorCornerToMouseSnapped.Y;

				// ------
				// Compute the grab distance to the active connection point, usefull for snapping with connection
				Brick.ConnectionPoint activeConnectionPoint = brickUnderMouse.ActiveConnectionPoint;
				if (activeConnectionPoint != null)
					mMouseGrabDeltaToActiveConnectionPoint = new PointF(mouseCoordInStud.X - activeConnectionPoint.PositionInStudWorldCoord.X,
																		mouseCoordInStud.Y - activeConnectionPoint.PositionInStudWorldCoord.Y);
				else
					mMouseGrabDeltaToActiveConnectionPoint = new PointF(0.0f, 0.0f);
			}
			else
			{
				mMouseGrabDeltaToCenter = new PointF(0.0f, 0.0f);
				mMouseGrabDeltaToActiveConnectionPoint = new PointF(0.0f, 0.0f);
			}
		}

		/// <summary>
		/// This function is called to know if this layer is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if this layer wants to handle it</returns>
		public override bool handleMouseDown(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			// if the layer is not visible it is not sensible to mouve click
			if (!Visible)
				return false;

			// do stuff only for the left button
			if (e.Button == MouseButtons.Left)
			{
				// check if the mouse is inside the bounding rectangle of the selected objects
				bool isMouseInsideSelectedObjects = isPointInsideSelectionRectangle(mouseCoordInStud);
				if (!isMouseInsideSelectedObjects && (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
					&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey))
					clearSelection();

				// find the current brick under the mouse
				Brick currentBrickUnderMouse = null;

				// We search if there is a cell under the mouse but in priority we choose from the current selected bricks
				currentBrickUnderMouse = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as Brick;

				// if the current selected brick is not under the mouse we search among the other bricks
				// but in reverse order to choose first the brick on top
				if (currentBrickUnderMouse == null)
					currentBrickUnderMouse = getBrickUnderMouse(mouseCoordInStud);

				// reset the action and the cursor
				mEditAction = EditAction.NONE;
				preferedCursor = MainForm.Instance.BrickArrowCursor;

				// check if it is a duplicate of the selection
				// Be carreful for a duplication we take only the selected objects, not the cell
				// under the mouse that may not be selected
				if (isMouseInsideSelectedObjects && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey))
				{
					mEditAction = EditAction.DUPLICATE_SELECTION;
					preferedCursor = MainForm.Instance.BrickDuplicateCursor;
				}
				// check if it is a double click, to see if we need to do a flex move
				else if ((currentBrickUnderMouse != null) && (e.Clicks == 2))
				{
					mMouseFlexMoveAction = new FlexMove(this, this.SelectedObjects, currentBrickUnderMouse, mouseCoordInStud);
					if (mMouseFlexMoveAction.IsValid)
					{
						mEditAction = EditAction.FLEX_MOVE;
						preferedCursor = MainForm.Instance.FlexArrowCursor;
					}
					else
					{
						// destroy the action if it is not valid
						mMouseFlexMoveAction = null;
					}
				}
				
				// if we move the brick, use 4 directionnal arrows cursor
				// if there's a brick under the mouse, use the hand
				if ((mEditAction == EditAction.NONE) && (isMouseInsideSelectedObjects || (currentBrickUnderMouse != null))
					&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
					&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey))
				{
					mEditAction = EditAction.MOVE_SELECTION;
					preferedCursor = MainForm.Instance.BrickMoveCursor;
				}

				// reset the brick pointer under the mouse if finally we don't care.
				if (mEditAction == EditAction.NONE)
					currentBrickUnderMouse = null;

				// compute the grab point if we grab a brick
				setBrickUnderMouse(currentBrickUnderMouse, mouseCoordInStud);
			}
			
			// return the result
			return (mEditAction != EditAction.NONE) && ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right));
		}

		/// <summary>
		/// This method is called if the map decided that this layer should handle
		/// this mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseDown(MouseEventArgs e, PointF mouseCoordInStud)
		{
			mMouseIsBetweenDownAndUpEvent = true;

			// if there's a brick under the mouse, we have to refresh the view to display the highlight
			bool mustRefresh = (mCurrentBrickUnderMouse != null);

			if (e.Button == MouseButtons.Left)
			{
				// if finally we are called to handle this mouse down,
				// we add the cell under the mouse if the selection list is empty
				if ((mCurrentBrickUnderMouse != null) && (mEditAction != EditAction.DUPLICATE_SELECTION)
					&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey))
				{
					// if the selection is empty add the brick, else check the control key state
					if (mSelectedObjects.Count == 0)
						addObjectInSelection(mCurrentBrickUnderMouse);

					// Break all the connections between the selected bricks and the non selected bricks
					// meaning iterate on all the brick of the selection and when we find a link to a brick which
					// is not in the selection, cut it. This way all the bridges between the selected group and the
					// non selected group are cut. What will happen is at the moment the mouse is click the user see
					// the selected group separated from the other bricks.
					// This is important to fix a shaking bug in the snapping algo: in the snapping algo we search
					// for free connection points from the active connection point of the grabbed brick. If the grabbed
					// brick is not free, the snapping algo will snap on the grid for the first little move of the mouse
					// then the brick becomes free, and the snapping algo choose again the previous linked brick for the
					// second little move. By breaking the link now, the snapping algo will always choose the previous
					// linked brick unless you start to do a big move.
					if (mEditAction != EditAction.DUPLICATE_SELECTION)
						foreach (Brick brick in mSelectedObjects)
							if (brick.ConnectionPoints != null)
								foreach (Brick.ConnectionPoint connection in brick.ConnectionPoints)
									if (connection.ConnectedBrick != null && !mSelectedObjects.Contains(connection.ConnectedBrick))
										disconnectTwoConnectionPoints(connection, connection.ConnectionLink);

					// update the active connexion point (after cutting the bridge with non selected parts)
					mCurrentBrickUnderMouse.setActiveConnectionPointUnder(mouseCoordInStud);
					// and call again the function to recompute the grab distance from the modified active connection point
					setBrickUnderMouse(mCurrentBrickUnderMouse, mouseCoordInStud);
				}

				// record the initial position of the mouse
				mMouseDownInitialPosition = getStartSnapPoint(mouseCoordInStud);
				mMouseDownLastPosition = mMouseDownInitialPosition;
				mMouseHasMoved = false;
			}
			else if (e.Button == MouseButtons.Right)
			{
				if (mEditAction == EditAction.FLEX_MOVE)
				{
					mMouseFlexMoveAction.finishActionConstruction();
					mMouseFlexMoveAction = null;
				}
				else if (mEditAction == EditAction.DUPLICATE_SELECTION)
				{
					// undo the duplicate action and clear it
					mLastDuplicateAction.undo();
					mLastDuplicateAction = null;
					mRotationForSnappingDuringBrickMove = null;
					mSnappingOrientation = 0.0f;
				}
				else if (mEditAction == EditAction.MOVE_SELECTION)
				{
					// get the delta before changing any rotation action
					mouseCoordInStud = getMovedSnapPoint(mouseCoordInStud, mCurrentBrickUnderMouse);
					PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownInitialPosition.X, mouseCoordInStud.Y - mMouseDownInitialPosition.Y);

					// undo the rotation action if needed
					if (mRotationForSnappingDuringBrickMove != null)
					{
						mRotationForSnappingDuringBrickMove.undo();
						mRotationForSnappingDuringBrickMove = null;
						mSnappingOrientation = 0.0f;
					}

					// reset the initial position to each brick
					if ((deltaMove.X != 0) || (deltaMove.Y != 0))
						foreach (LayerBrick.Brick brick in mSelectedObjects)
							brick.Center = new PointF(brick.Center.X - deltaMove.X, brick.Center.Y - deltaMove.Y);

					// reconnect the bricks and update the bounding rectangle
					updateBrickConnectivityOfSelection(false);
					this.updateBoundingSelectionRectangle();
				}

				mEditAction = EditAction.NONE;
				setBrickUnderMouse(null, PointF.Empty);
				mustRefresh = true;
			}

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			if ((mEditAction != EditAction.NONE) && (mSelectedObjects.Count > 0))
			{
				// snap the mouse coord to the grid
				Brick.ConnectionPoint snappedConnection = null;
				PointF mouseCoordInStudSnapped = getMovedSnapPoint(mouseCoordInStud, mCurrentBrickUnderMouse, out snappedConnection);

				// check if it is a flex move or normal move
				if (mEditAction == EditAction.FLEX_MOVE)
				{
					mMouseFlexMoveAction.reachTarget(mouseCoordInStudSnapped, snappedConnection);
					return true;
				}
				else if ((mEditAction == EditAction.MOVE_SELECTION) || (mEditAction == EditAction.DUPLICATE_SELECTION))
				{
					// compute the delta move of the mouse
					PointF deltaMove = new PointF(mouseCoordInStudSnapped.X - mMouseDownLastPosition.X, mouseCoordInStudSnapped.Y - mMouseDownLastPosition.Y);
					// check if the delta move is not null
					if (deltaMove.X != 0.0f || deltaMove.Y != 0.0)
					{
						bool wereBrickJustDuplicated = false;

						// check if it is a move or a duplicate
						if (mEditAction == EditAction.DUPLICATE_SELECTION)
						{
							// this is a duplicate, if we didn't created the duplicate action yet, this is the moment to copy and paste the selection
							// and this will change the current selection, that will be move normally after
							if (mLastDuplicateAction == null)
							{
								this.copyCurrentSelectionToClipboard();
								this.pasteClipboardInLayer(AddOffsetAfterPaste.NO, false);
								// set the flag
								wereBrickJustDuplicated = true;
							}
						}
						// the duplication above will change the current selection
						// The code below is to move the selection, either the original one or the duplicated one
						foreach (LayerBrick.Brick brick in mSelectedObjects)
							brick.Center = new PointF(brick.Center.X + deltaMove.X, brick.Center.Y + deltaMove.Y);
						// update the free connexion list
						updateBrickConnectivityOfSelection(true);
						// move also the bounding rectangle
						moveBoundingSelectionRectangle(deltaMove);
						// after we moved the selection check if we need to refresh the current highlighted brick
						if (wereBrickJustDuplicated)
						{
							Brick currentBrickUnderMouse = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as Brick;
							setBrickUnderMouse(currentBrickUnderMouse, mouseCoordInStud);
						}
						// memorize the last position of the mouse
						mMouseDownLastPosition = mouseCoordInStudSnapped;
						// set the flag that indicate that we moved the mouse
						mMouseHasMoved = true;
						return true;
					}
					else if (mEditAction == EditAction.MOVE_SELECTION && !mMouseHasMoved)
					{
						// give a second chance to duplicate if the user press the duplicate key
						// after pressing down the mouse key, but not if the user already moved
						if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)
						{
							mEditAction = EditAction.DUPLICATE_SELECTION;
							updateBrickConnectivityOfSelection(false);
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseUp(MouseEventArgs e, PointF mouseCoordInStud)
		{
			if (mEditAction == EditAction.FLEX_MOVE)
			{
				// finish the action for this move and add it to the manager
				mMouseFlexMoveAction.finishActionConstruction();
				ActionManager.Instance.doAction(mMouseFlexMoveAction);

				// forget the action
				mMouseFlexMoveAction = null;
			}
			else
			{
				// check if we moved the selected bricks
				if (mMouseHasMoved && (mSelectedObjects.Count > 0))
				{
					// reset the flag
					mMouseHasMoved = false;

					// compute the delta mouve of the mouse
					mouseCoordInStud = getMovedSnapPoint(mouseCoordInStud, mCurrentBrickUnderMouse);
					PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownInitialPosition.X, mouseCoordInStud.Y - mMouseDownInitialPosition.Y);

					// create a new action for this move
					if ((deltaMove.X != 0) || (deltaMove.Y != 0))
					{
						// update the duplicate action or add a move action
						if (mEditAction == EditAction.DUPLICATE_SELECTION)
						{
							// update the position, undo the action and add it in the manager
							mLastDuplicateAction.updatePositionShift(deltaMove.X, deltaMove.Y);
							// clear also the rotation snapping, in case of a series of duplication, but do not
							// undo it, since we want to keep the rotation applied on the duplicated bricks.
							mRotationForSnappingDuringBrickMove = null;
						}
						else if (mEditAction == EditAction.MOVE_SELECTION)
						{
							// undo the rotation action if needed
							bool isComplexActionNeeded = false;
							if (mRotationForSnappingDuringBrickMove != null)
							{
								mRotationForSnappingDuringBrickMove.undo();
								mRotationForSnappingDuringBrickMove = null;
								isComplexActionNeeded = true;
							}
							// reset the initial position to each brick
							foreach (LayerBrick.Brick brick in mSelectedObjects)
								brick.Center = new PointF(brick.Center.X - deltaMove.X, brick.Center.Y - deltaMove.Y);

							// create a move or complex move action depending if some roatation are needed
							if (isComplexActionNeeded)
							{
								ActionManager.Instance.doAction(new RotateAndMoveBrick(this, mSelectedObjects, mSnappingOrientation, mCurrentBrickUnderMouse, deltaMove));
								mSnappingOrientation = 0.0f;
							}
							else
							{
								ActionManager.Instance.doAction(new MoveBrick(this, mSelectedObjects, deltaMove));
							}
						}
					}
					else
					{
						// if the user start from a connected group of bricks, move them and snap them
						// to another connection, then come back and snap them to the original connection
						// the the delta move is null, but the snapping rotation is not, so we need to
						// cancel also this rotation, and more important we need to set the rotation to null
						if (mRotationForSnappingDuringBrickMove != null)
						{
							mRotationForSnappingDuringBrickMove.undo();
							mRotationForSnappingDuringBrickMove = null;
						}
						// update the free connexion list if the user move the brick and then go back
						// to the original place (deltaMove is null), so the link was broken because
						// of the move, so we need to recreate the link
						updateBrickConnectivityOfSelection(false);
					}

					// after the last move of the duplicated bricks, add the action in the manager
					if (mEditAction == EditAction.DUPLICATE_SELECTION)
					{
						// undo the action and add it in the manager
						mLastDuplicateAction.undo();
						ActionManager.Instance.doAction(mLastDuplicateAction);
					}
					// reset anyway the temp reference for the duplication
					mLastDuplicateAction = null;
				}
				else
				{
					// update the connection for the selection, because we broke the connnection between selected 
					// brick and non-selected bricks in the mouse down event.
					updateBrickConnectivityOfSelection(false);

					// if we didn't move the item and use the control key, we need to add or remove object from the selection
					// we must do it in the up event because if we do it in the down, we may remove an object before moving
					// we do this only if the mMouseHasMoved flag is not set to avoid this change if we move
					if ((mCurrentBrickUnderMouse != null) && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey))
					{
						if (mSelectedObjects.Contains(mCurrentBrickUnderMouse))
							removeObjectFromSelection(mCurrentBrickUnderMouse);
						else
							addObjectInSelection(mCurrentBrickUnderMouse);
					}
				}
			}

			mEditAction = EditAction.NONE;
			mMouseIsBetweenDownAndUpEvent = false;
			setBrickUnderMouse(null, PointF.Empty);
			return true;
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public override void selectInRectangle(RectangleF selectionRectangeInStud)
		{
			selectInRectangle(selectionRectangeInStud, mBricks);
		}

		/// <summary>
		/// This method return a snap point near the specified point according to different
		/// snapping rules that are specific of this brick layer:
		/// If the mouse is not under a selected brick, that means the player is moving a group
		/// of brick, handling the group from an empty part, then the snapping is a relative snaping,
		/// i.e. we want to move the whole group by step of the grid size from their original position.
		/// But if the user handle the group of object by one brick, we want to snap this brick on the
		/// world grid; that mean the first snap value can be small to reach the world grid, then the
		/// snap will use the step of the grid size.
		/// Now if the user is moving a group of brick with connexion point, we want to snap the group
		/// on the connexion point. Here again we look at the brick under the mouse which is the master
		/// brick to move
		/// </summary>
		/// <param name="pointInStud">the point to snap</param>
		/// <returns>a near snap point</returns>
		private PointF getStartSnapPoint(PointF pointInStud)
		{
			PointF result;

			if (SnapGridEnabled)
			{
				// check if there is a master brick
				if (mCurrentBrickUnderMouse != null)
				{
					result = mCurrentBrickUnderMouse.Center;
				}
				else
				{
					// there's no master brick, just do a relative snapping
					result = Layer.snapToGrid(pointInStud, false);
				}
			}
			else
			{
				result = pointInStud;
			}

			return result;
		}

		/// <summary>
		/// See the doc of the other signature method
		/// </summary>
		/// <param name="pointInStud">the rough point to snap</param>
		/// <param name="referenceItem">the reference item from which we compute the snap point in case of connection</param>
		/// <returns>a near snap point</returns>
		public PointF getMovedSnapPoint(PointF pointInStud, LayerItem referenceItem)
		{
			Brick.ConnectionPoint ignoredSnappedConnection = null;
			return getMovedSnapPoint(pointInStud, referenceItem, out ignoredSnappedConnection);
		}

		/// <summary>
		/// This method return a snap point near the specified point according to different
		/// snapping rules that are specific of this brick layer:
		/// If the mouse is not under a selected brick, that means the player is moving a group
		/// of brick, handling the group from an empty area, then the snapping is a relative snaping,
		/// i.e. we want to move the whole group by step of the grid size from their original position.
		/// But if the user handle the group of object by one brick, we want to snap this brick on the
		/// world grid; that mean the first snap value can be small to reach the world grid, then the
		/// snap will use the step of the grid size.
		/// Now if the user is moving a group of brick by holding a brick with connexion point, 
		/// we want to snap the group by snapping the hold brick on the connexion point.
		/// Here again we look at the brick under the mouse which is the master brick to move and snap.
		/// </summary>
		/// <param name="pointInStud">the rough point to snap</param>
		/// <param name="referenceItem">the reference item from which we compute the snap point in case of connection</param>
		/// <param name="snappedConnection">If the brick should snap to a connection, this is the one, otherwise null</param>
		/// <returns>a near snap point</returns>
		public PointF getMovedSnapPoint(PointF pointInStud, LayerItem referenceItem, out Brick.ConnectionPoint snappedConnection)
		{
			// init the output value
			snappedConnection = null;
			// don't do anything is the snapping is not enabled
			if (SnapGridEnabled)
			{
				// check if there is a master brick
				if (mCurrentBrickUnderMouse != null)
				{
					// get the active brick connection either from the master brick (a group or a single brick)
					// but it can stay null if the brick or group doesn't have any connection
					// In case of a group drop from the part lib, the brick under the mouse has been 
					// set with the brick that hold the active connection in the group
					Brick.ConnectionPoint activeBrickConnexion = mCurrentBrickUnderMouse.ActiveConnectionPoint;
					// now check if the master brick has some connections
					if (activeBrickConnexion != null)
					{
						// but we also need to check if the brick has a FREE connexion
						// but more than that we need to check if the Active Connection is a free connexion.
						// Because for connection snapping, we always snap the active connection with
						// the other bricks. That give the feedback to the user of which free connection
						// of is moving brick will try to connect
						if (activeBrickConnexion.IsFree)
						{
							// compute the virtual position of the active connection point, from the
							// real position of the mouse.
							PointF virtualActiveConnectionPosition = pointInStud;
							virtualActiveConnectionPosition.X -= mMouseGrabDeltaToActiveConnectionPoint.X;
							virtualActiveConnectionPosition.Y -= mMouseGrabDeltaToActiveConnectionPoint.Y;

							// snap the selected brick on a free connexion points (of other bricks)
							// iterate on all the free connexion point to know if there's a nearest point						
							float nearestSquareDistance = float.MaxValue;
							Brick.ConnectionPoint bestFreeConnection = null;
							foreach (Brick.ConnectionPoint freeConnexion in mFreeConnectionPoints.getListForType(activeBrickConnexion.Type))
								if (!mSelectedObjects.Contains(freeConnexion.mMyBrick))
								{
									float dx = freeConnexion.PositionInStudWorldCoord.X - virtualActiveConnectionPosition.X;
									float dy = freeConnexion.PositionInStudWorldCoord.Y - virtualActiveConnectionPosition.Y;
									float squareDistance = (dx * dx) + (dy * dy);
									if (squareDistance < nearestSquareDistance)
									{
										nearestSquareDistance = squareDistance;
										bestFreeConnection = freeConnexion;
									}
								}

							// update the temporary rotation of the selection
							// undo the previous rotation
							if (mRotationForSnappingDuringBrickMove != null)
							{
								mRotationForSnappingDuringBrickMove.undo();
								mRotationForSnappingDuringBrickMove = null;
								mSnappingOrientation = 0.0f;
							}

							// compute the snapping value from the grid snapping
							// but with of 4 or 8 studs minimum (depending if it is a flex move)
							float threshold = Math.Max(CurrentSnapGridSize, (mEditAction == EditAction.FLEX_MOVE) ? 4.0f : 8.0f);
							// check if the nearest free connexion if close enough to snap
							if (nearestSquareDistance < threshold * threshold)
							{
								// the distance to the closest connection is under the max threshold distance, so set the output value
								snappedConnection = bestFreeConnection;

								// we found a snapping connection, start to compute the snap position for the best connection
								PointF snapPosition = snappedConnection.PositionInStudWorldCoord;

								// if it is not a flex move, rotate the selection
								if (mEditAction != EditAction.FLEX_MOVE)
								{
									// rotate the selection
									mSnappingOrientation = snappedConnection.mMyBrick.Orientation - mCurrentBrickUnderMouse.Orientation;
									mSnappingOrientation += snappedConnection.Angle + 180 - activeBrickConnexion.Angle;
									// clamp the orientation between 0 and 360
									if (mSnappingOrientation >= 360.0f)
										mSnappingOrientation -= 360.0f;
									if (mSnappingOrientation < 0.0f)
										mSnappingOrientation += 360.0f;

									// and create a new action for the new angle
									mRotationForSnappingDuringBrickMove = new RotateBrickOnPivotBrick(this, SelectedObjects, mSnappingOrientation, mCurrentBrickUnderMouse);
									mRotationForSnappingDuringBrickMove.MustUpdateBrickConnectivity = false;
									mRotationForSnappingDuringBrickMove.redo();

									// compute the position from the connection points
									snapPosition.X += referenceItem.Center.X - activeBrickConnexion.PositionInStudWorldCoord.X;
									snapPosition.Y += referenceItem.Center.Y - activeBrickConnexion.PositionInStudWorldCoord.Y;
								}
								// otherwise, for a flex move, just keep the best connection as the snapping value

								// return the position
								return snapPosition;
							}
						}
					}

					// if we didn't find any connection to snap to, and if it is a flex move, just
					// return the value without snaping, otherwise, we will snap the part on the grid
					if (mEditAction == EditAction.FLEX_MOVE)
						return pointInStud;

					// This is the normal case for snapping the brick under the mouse.
					// Snap the position of the mouse on the grid (the snapping is a Floor style one)
					// then add the center shift of the part and the snapping offset
					pointInStud = Layer.snapToGrid(pointInStud, false);
					
					// shift the point according to the center and the snap grabbed delta
					pointInStud.X += mMouseGrabDeltaToCenter.X;
					pointInStud.Y += mMouseGrabDeltaToCenter.Y;
					return pointInStud;
				}

				// the snapping is enable but the group of brick was grab from an empty place
				// i.e. there's no bricks under the mouse so just do a normal snapping on the grid
				return Layer.snapToGrid(pointInStud, false);
			}

			// by default do not change anything
			return pointInStud;
		}
		#endregion

		#region drag'n'drop
		/// <summary>
		/// This method is called by the Map Panel when the user want to drag and drop a part from
		/// the part library on this layer. The selection and the current part under the mouse
		/// is then patch with this temporary part.
		/// </summary>
		/// <param name="itemDrop">The temporary part to add which can be a Brick or a Group</param>
		public void addTemporaryPartDrop(Layer.LayerItem itemDrop)
		{
			// clear the selection to only select the part(s) drop
			mSelectedObjects.Clear();

			// check if it is a single Brick or a group
			if (itemDrop.IsAGroup)
			{
				// the part to drop is a group
				Layer.Group groupDrop = itemDrop as Layer.Group;
				// the part to drop is a group
				List<Layer.LayerItem> partsInTheGroup = groupDrop.getAllLeafItems();
				foreach (Layer.LayerItem item in partsInTheGroup)
				{
					Brick brick = item as Brick;
					mBricks.Add(brick);
					mSelectedObjects.Add(brick);
				}
				
				// by default the active connection index of a group is 0
				// and get the corresponding brick that hold the active connection index
				// but if the group doesn't have any connection at all, get the first brick
				// of the group as the brick under the mouse
				Brick brickUnderMouse = groupDrop.BrickThatHoldsActiveConnection;
				if (brickUnderMouse == null)
				{
					List<LayerItem> children = groupDrop.getAllLeafItems();
					if (children.Count > 0)
						brickUnderMouse = (children[0]) as Brick;
				}
				setBrickUnderMouse(brickUnderMouse, groupDrop, groupDrop.Center);

				// update the brick connectivity for the group after having selected them
				updateFullBrickConnectivityForSelectedBricksOnly();
			}
			else
			{
				// the part to drop is a brick
				Brick brickDrop = itemDrop as Brick;
				mBricks.Add(brickDrop);
				mSelectedObjects.Add(brickDrop);
				setBrickUnderMouse(brickDrop, brickDrop.Center);
			}
		}

		/// <summary>
		/// This method is called by the Map Panel when the user finished to drag and drop a part from
		/// the part library on this layer. The temporary part is removed from the layer.
		/// </summary>
		/// <param name="itemDrop">The temporary part to remove which can be a Brick or a Group</param>
		public void removeTemporaryPartDrop(Layer.LayerItem itemDrop)
		{
			// clear the data
			mSelectedObjects.Clear();
			setBrickUnderMouse(null, PointF.Empty);
			// clear also the rotation action (but do not undo the action since we want to keep the orientation of the part added)
			mRotationForSnappingDuringBrickMove = null;
			mSnappingOrientation = 0.0f;

			// check if it is a single Brick or a group to remove one or several bricks
			Brick brickDrop = itemDrop as Brick;
			if (brickDrop != null)
			{
				// the cast succeed, this is a brick
				mBricks.Remove(brickDrop);
			}
			else
			{
				// the cast failed, the part drop is a group
				List<Layer.LayerItem> partsInTheGroup = (itemDrop as Layer.Group).getAllLeafItems();
				foreach (Layer.LayerItem item in partsInTheGroup)
					mBricks.Remove(item as Brick);
			}
		}
		#endregion
	}
}
