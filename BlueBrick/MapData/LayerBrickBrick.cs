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
using System.Collections;
using System.Drawing.Drawing2D;

namespace BlueBrick.MapData
{
	public partial class LayerBrick : Layer
	{
		/// <summary>
		/// A brick is just a part that can be placed on the map
		/// </summary>
		[Serializable]
		public class Brick : LayerItem
		{
			public class ConnectionPoint
			{
				public static Hashtable sHashtableForLinkRebuilding = new Hashtable(); // this hashtable is used to recreate the link when loading

				public Brick mMyBrick = null; // reference to the brick this connection refer to
				private PointF mPositionInStudWorldCoord = new PointF(0, 0); // the position of the connection point is world coord stud coord.
				private ConnectionPoint mConnectionLink = null; // link toward this conection point is connected
				private int mType = BrickLibrary.ConnectionType.DEFAULT; // 0 if the default brick type (which is a kind of Brick connection)
				private short mPolarity = 0; // 0=neutral, negative value=negative, and positive value=positive
				private short mHasElectricShortcut = 0; // 0=no shortcut, 1 = has shortcut

				/// <summary>
				/// This default constructor is for the serialization and should not be used in the program
				/// </summary>
				public ConnectionPoint()
				{
				}

				/// <summary>
				/// This constructor is used to create a dummy connection point somewhere in the world at
				/// the specified position. Be carefull, the brick is null, meaning this connection point
				/// doesn't belong to a brick and all the other parameters are also default one.
				/// This dummy connection point is used by the Flex track for attaching a orphean flex part
				/// on the world and let this part rotate.
				/// </summary>
				public ConnectionPoint(PointF positionInStudWorldCoord)
				{
					mPositionInStudWorldCoord = positionInStudWorldCoord;
				}

				public ConnectionPoint(Brick myBrick, int connexionIndex)
				{
					mMyBrick = myBrick;
					// save the brick type (for optimisation reasons)
					mType = BrickLibrary.Instance.getConnexionType(myBrick.PartNumber, connexionIndex);
				}

				public bool IsFree
				{
					get { return (mConnectionLink == null); }
				}

				public int Type
				{
					get { return mType; }
					set { mType = value; }
				}

				/// <summary>
				/// Return the index of this connecion inside the array of connection its brick 
				/// </summary>
				public int Index
				{
					get
					{
						for (int i = 0; i < mMyBrick.ConnectionPoints.Count; ++i)
							if (mMyBrick.ConnectionPoints[i] == this)
								return i;
						return 0;
					}
				}

				public Brick ConnectedBrick
				{
					get
					{
						if (mConnectionLink != null)
							return mConnectionLink.mMyBrick;
						return null;
					}
				}

				/// <summary>
				/// Property to get/set the link. When a link is created, if this is the active
				/// connection point of the brick, we try to find another one that is free. If the
				/// link is broken and the active connection point of the brick is not free, then
				/// this connection becomes the active connection point
				/// </summary>
				public ConnectionPoint ConnectionLink
				{
					get { return mConnectionLink; }
					set
					{
						// only update the active connection point of the brick if my brick is valid
						if (mMyBrick != null)
						{
							// get the active connection point (that can be null, during a loading)
							ConnectionPoint activeConnectionPoint = mMyBrick.ActiveConnectionPoint;
							// check if we brake or set the link
							if (value == null)
							{
								// the link is broken, check if the active connection point is not free
								// because then it can become me
								if (activeConnectionPoint != null && !activeConnectionPoint.IsFree)
									mMyBrick.mActiveConnectionPointIndex = mMyBrick.ConnectionPoints.IndexOf(this);
							}
							else
							{
								// the link is set, check if the active connection point is me, then force
								// the brick to choose someone else
								if (activeConnectionPoint == this)
									mMyBrick.setActiveConnectionPointWithNextOne(true);
							}
						}
						// finally set the link
						mConnectionLink = value;
					}
				}

				public PointF PositionInStudWorldCoord
				{
					get { return mPositionInStudWorldCoord; }
					set
					{
						PointF newBrickCenter = this.mMyBrick.Center;
						newBrickCenter.X += value.X - mPositionInStudWorldCoord.X;
						newBrickCenter.Y += value.Y - mPositionInStudWorldCoord.Y;
						this.mMyBrick.Center = newBrickCenter;
					}
				}

				public float Angle
				{
					get
					{
						int myIndex = mMyBrick.ConnectionPoints.IndexOf(this);
						if (myIndex >= 0)
							return BrickLibrary.Instance.getConnectionAngle(mMyBrick.PartNumber, myIndex);
						return 0.0f;
					}
				}

				public short Polarity
				{
					get { return mPolarity; }
					set
					{
						mPolarity = value;
						mHasElectricShortcut = 0;
					}
				}

				public bool HasElectricShortcut
				{
					get { return (mHasElectricShortcut != 0); }
					set { mHasElectricShortcut = (short)(value ? 1 : 0); }
				}

				#region IXmlSerializable Members

				public System.Xml.Schema.XmlSchema GetSchema()
				{
					return null;
				}

				public void ReadXml(System.Xml.XmlReader reader)
				{
					// read the position of the connexion (for file version under 4)
					if (Map.DataVersionOfTheFileLoaded <= 3)
						mPositionInStudWorldCoord = XmlReadWrite.readPointF(reader);
					else
						reader.ReadToDescendant("LinkedTo");
					// check if we have a link
					if (reader.IsEmptyElement)
					{
						mConnectionLink = null;
						reader.Read();
					}
					else
					{
						int hashCodeOfTheLink = reader.ReadElementContentAsInt();
						// look in the hastable if this connexion alread exists, else create it
						mConnectionLink = ConnectionPoint.sHashtableForLinkRebuilding[hashCodeOfTheLink] as ConnectionPoint;
						if (mConnectionLink == null)
						{
							// instanciate a ConnectionPoint, and add it in the hash table
							mConnectionLink = new ConnectionPoint();
							ConnectionPoint.sHashtableForLinkRebuilding.Add(hashCodeOfTheLink, mConnectionLink);
						}
					}
				}

				public void WriteXml(System.Xml.XmlWriter writer)
				{
					writer.WriteAttributeString("id", GetHashCode().ToString());
					writer.WriteStartElement("LinkedTo");
					if (mConnectionLink != null)
						writer.WriteString(mConnectionLink.GetHashCode().ToString());
					writer.WriteEndElement();
				}

				#endregion

				/// <summary>
				/// This accessor is reserved to the Brick class, to set the position of each of its connection
				/// one by one. Use the normal accessor for setting the brick position via this connection position
				/// </summary>
				/// <param name="positionInStudWorldCoord">the position of this connection in stud world coordinate</param>
				public void setPositionReservedForBrick(float xInStudWorldCoord, float yInStudWorldCoord)
				{
					mPositionInStudWorldCoord.X = xInStudWorldCoord;
					mPositionInStudWorldCoord.Y = yInStudWorldCoord;
				}
			}

			private string mPartNumber = null;	// id of the part
			private int mActiveConnectionPointIndex = 0; // the current active connection point in the connexion point list
			private List<ConnectionPoint> mConnectionPoints = null; // list of all the connection point (if this brick can connect)
			private float mAltitude = 0.0f; //for improving compatibility with LDRAW we save a up coordinate for each brick

			// the list of attached rulers are not serialized but reconstructed at loading
			[NonSerialized]
			private RulerAttachementSet mAttachedRulers = null;

			// the image and the connection point are not serialized, they are built in the constructor
			// or when the part number property is set by the serializer
			[NonSerialized]
			private Image[] mMipmapImages = new Image[5];	// all the images in different LOD level
			[NonSerialized]
			private Image mOriginalImageReference = null;	// reference on the original image in the database
			[NonSerialized]
			private PointF mTopLeftCornerInPixel = PointF.Empty;
			[NonSerialized]
			private PointF mOffsetFromOriginalImage = new PointF(0, 0); // when the image is rotated, the size is not the same as the orginal one, so this offset handle the difference
			[NonSerialized]
			private List<BrickLibrary.Brick.ElectricCircuit> mElectricCircuitIndexList = null; // reference on the array describing the electric circuit for this part

			[NonSerialized]
			private static Bitmap sInvalidDummyImageToSkip = new Bitmap(1, 1); // a dummy image to indicate the the image is not valid

			#region get/set
			/// <summary>
			/// the part number of the brick
			/// The set of the part number is used by the serializer, but should not be used in the program
			/// instead consider to delete your brick and create a new one with the appropriate constructor.
			/// </summary>
			public override string PartNumber
			{
				get { return mPartNumber; }
				set
				{
					// set the value
					mPartNumber = value;
					// update the associated electric current when the part number change
					mElectricCircuitIndexList = BrickLibrary.Instance.getElectricCircuitList(value);
					// update the image
					updateImage();
					updateSnapMargin();
				}
			}

			/// <summary>
			/// The current rotation of the image
			/// </summary>
			public override float Orientation
			{
				set
				{
					mOrientation = value;
					updateImage();
					updateSnapMargin();
					updateConnectionPosition();
					if (mAttachedRulers != null)
						mAttachedRulers.brickRotateNotification();
				}
			}

			/// <summary>
			///	Set the position in stud coord. The position of a brick is its top left corner.
			/// </summary>
			public override PointF Position
			{
				set
				{
					// call the base class, such as the selection area is also updated
					base.Position = value;
					updateConnectionPosition();
					if (mAttachedRulers != null)
						mAttachedRulers.brickMoveNotification();
				}
				get { return new PointF(mDisplayArea.X, mDisplayArea.Y); }
			}

			/// <summary>
			/// Set the position via the center of the object in stud coord.
			/// </summary>
			public override PointF Center
			{
				set
				{
					// call the base class, such as the selection area is also updated
					base.Center = value;
					updateConnectionPosition();
					if (mAttachedRulers != null)
						mAttachedRulers.brickMoveNotification();
				}
			}

			/// <summary>
			/// Get the Pivot of the object in stud coord. Or set the position of the object through its Pivot.
			/// The pivot of an item is its rotation center.
			/// </summary>
			public override PointF Pivot
			{
				get
				{
					// compute the pivot point of the brick
					PointF brickCenter = this.Center; // use this variable for optimization reason (the center is computed)
					PointF centerOffset = this.OffsetFromOriginalImage;
					return new PointF(brickCenter.X + centerOffset.X, brickCenter.Y + centerOffset.Y);
				}
				set
				{
					// compute the new center of the part based on the pivot of the part and the new offset
					PointF centerOffset = this.OffsetFromOriginalImage;
					// assign the new center position
					this.Center = new PointF(value.X - centerOffset.X, value.Y - centerOffset.Y);
				}
			}

			/// <summary>
			/// an offset to adjust the centers from between original image and reframed rotated image.
			/// </summary>
			public PointF OffsetFromOriginalImage
			{
				get { return mOffsetFromOriginalImage; }
			}

			/// <summary>
			/// Tell if this brick has any connection point
			/// </summary>
			public bool HasConnectionPoint
			{
				get
				{
					if (mConnectionPoints != null)
					{
						foreach (ConnectionPoint connexion in mConnectionPoints)
							if (connexion.Type != BrickLibrary.ConnectionType.DEFAULT)
								return true;
					}
					return false;
				}
			}

			/// <summary>
			/// Accessor on the currently active connexion point if any
			/// </summary>
			public ConnectionPoint ActiveConnectionPoint
			{
				get
				{
					if (mConnectionPoints != null)
						return mConnectionPoints[mActiveConnectionPointIndex];
					return null;
				}
			}

			/// <summary>
			/// Accessor on the currently active connexion position if any
			/// </summary>
			public PointF ActiveConnectionPosition
			{
				get
				{
					if (mConnectionPoints != null)
						return mConnectionPoints[mActiveConnectionPointIndex].PositionInStudWorldCoord;
					return new PointF();
				}
				set
				{
					if (mConnectionPoints != null)
						mConnectionPoints[mActiveConnectionPointIndex].PositionInStudWorldCoord = value;
				}
			}

			/// <summary>
			/// Accessor to get the current active connexion angle if any
			/// </summary>
			public float ActiveConnectionAngle
			{
				get
				{
					if (mConnectionPoints != null)
						return BrickLibrary.Instance.getConnectionAngle(mPartNumber, mActiveConnectionPointIndex);
					return 0.0f;
				}
			}

			/// <summary>
			/// Set the index of the active connexion point
			/// </summary>
			public int ActiveConnectionPointIndex
			{
				get { return mActiveConnectionPointIndex; }
				set
				{
					if (mConnectionPoints != null)
					{
						// check if the value is in the range
						if (value < 0)
							mActiveConnectionPointIndex = 0;
						else if (value >= mConnectionPoints.Count)
							mActiveConnectionPointIndex = mConnectionPoints.Count - 1;
						else
							mActiveConnectionPointIndex = value;
						// check if the active connection point is Free, else select the next one
						if (!ActiveConnectionPoint.IsFree)
							setActiveConnectionPointWithNextOne(false);
					}
				}
			}

			/// <summary>
			/// get an accessor on the list of connection point
			/// </summary>
			public List<ConnectionPoint> ConnectionPoints
			{
				get { return mConnectionPoints; }
			}

			/// <summary>
			/// Return the a list of couple of connection index that describe each an electric circuit on the part
			/// </summary>
			public List<BrickLibrary.Brick.ElectricCircuit> ElectricCircuitIndexList
			{
				get { return mElectricCircuitIndexList; }
			}

			/// <summary>
			/// for improving compatibility with LDRAW we save an altitude (up coord) for each brick
			/// but it is not used in the program for now
			/// </summary>
			public float Altitude
			{
				get { return mAltitude; }
				set { mAltitude = value; }
			}
			#endregion

			#region constructor/copy
			/// <summary>
			/// this parameter less constructor is requested for the serialization, but should not
			/// be used by the program
			/// </summary>
			public Brick()
			{
			}

			/// <summary>
			/// This is the normal constructor that you should use in your program
			/// </summary>
			/// <param name="partNumber">the part number used to create this brick</param>
			public Brick(string partNumber)
			{
				init(partNumber);
			}

			/// <summary>
			/// This constructor is used by the group constructor to place the part at a specific position and orientation
			/// </summary>
			/// <param name="partNumber">the part number used to create this brick</param>
			/// <param name="centerPosition">the position of the center of the brick</param>
			/// <param name="orientation">the orientation of the brick</param>
			public Brick(string partNumber, PointF centerPosition, float orientation)
			{
				// set the orientation before calling the init method to compute
				// the image directly with the correct orientation during init.
				// We do not use the accessor intentionnaly to not trigger an image building
				this.mOrientation = orientation;
				// the init parameter will generate the image
				init(partNumber);
				// finally adjust the center position
				this.Center = centerPosition;
			}

			/// <summary>
			/// Clone this Brick
			/// </summary>
			/// <returns>a new Brick which is a conform copy of this</returns>
			public override LayerItem Clone()
			{
				Brick result = new Brick();
				result.mDisplayArea = this.mDisplayArea;
				result.mOrientation = this.mOrientation;
				result.mActiveConnectionPointIndex = this.mActiveConnectionPointIndex;
				// call the init after setting the orientation to compute the image in the right orientation
				// the init method will initialize mImage, mConnectionPoints and mSnapToGridOffsetFromTopLeftCorner
				result.init(this.mPartNumber);
				// return the cloned value
				return result;
			}

			private void init(string partNumber)
			{
				// We must set the part number first because the connection list need it
				// call the accessor to recreate the picture
				PartNumber = partNumber;
				// create the connection list if any
				List<BrickLibrary.Brick.ConnectionPoint> connectionList = BrickLibrary.Instance.getConnectionList(partNumber);
				if (connectionList != null)
				{
					mConnectionPoints = new List<ConnectionPoint>(connectionList.Count);
					for (int i = 0; i < connectionList.Count; i++)
						mConnectionPoints.Add(new ConnectionPoint(this, i));
					updateConnectionPosition();
				}
			}
			#endregion

			#region IXmlSerializable Members

			public override void ReadXml(System.Xml.XmlReader reader)
			{
				base.ReadXml(reader);
				// avoid using the accessor to reduce the number of call of updateBitmap
				mPartNumber = BrickLibrary.Instance.getActualPartNumber(reader.ReadElementContentAsString().ToUpperInvariant());
				// but then update its electric list
				mElectricCircuitIndexList = BrickLibrary.Instance.getElectricCircuitList(mPartNumber);
				mOrientation = reader.ReadElementContentAsFloat();
				mActiveConnectionPointIndex = reader.ReadElementContentAsInt();
				// the altitude
				if (Map.DataVersionOfTheFileLoaded >= 3)
					mAltitude = reader.ReadElementContentAsFloat();
				// update the bitmap
				updateImage();
				updateSnapMargin();
				// read the connexion points if any
				reader.ReadAttributeValue();
				int count = int.Parse(reader.GetAttribute(0));

				// check the number of connection is the same in the Brick library and in the loading file.
				// They can be different if the file was saved with an old part library and then one part
				// was updated to add or remove connection. So there is 3 different cases:
				// - if they are the same: no problems.
				// - if there is more connections in the part lib than in the file: we reserve enough space in the list,
				// based on the library value, and then we will add empty connections instances to fullfill the list
				// after finishing reading the connection tag.
				// - if there is more parts in the file than in the list, we need to discard some connection,
				// so we add a check in the parsing to create the last connections as default one (of type brick).
				// This will ensure that the link will be broken (because all connections or type BRICK are
				// broken after the loading of the file is finished) and then the GC will destroy these connections.
				// And of course the list is reserved based on the library value.

				// So first, we ask the number of connection to the part lib then allocate
				// the list of connection based on the number set in the library and not the number
				// read in the file, because finally the number of connection must be like the part lib says
				int connectionCountInBrickLibrary = BrickLibrary.Instance.getConnectionCount(mPartNumber);
				if (connectionCountInBrickLibrary > 0)
					mConnectionPoints = new List<ConnectionPoint>(connectionCountInBrickLibrary);

				// now check if we need to parse some connection in the file
				if (count > 0)
				{
					// declare a counter for the connections
					int connexionIndex = 0;
					bool connexionFound = reader.ReadToDescendant("Connexion");
					while (connexionFound)
					{
						// a boolean saying if the current connection is valid or will be destroyed later
						// because it is over the number indicated by the part library
						// be careful mConnectionPoints can be null, so use the int var instead
						bool isConnectionValid = (connexionIndex < connectionCountInBrickLibrary);

						// read the id (hashcode key) of the connexion
						reader.ReadAttributeValue();
						int hashCode = int.Parse(reader.GetAttribute(0));

						// look in the hastable if this connexion alread exists, else create it
						ConnectionPoint connexion = ConnectionPoint.sHashtableForLinkRebuilding[hashCode] as ConnectionPoint;
						if (connexion == null)
						{
							// instanciate a ConnectionPoint, and add it in the hash table
							if (isConnectionValid)
								connexion = new ConnectionPoint(this, connexionIndex);
							else
								connexion = new ConnectionPoint();
							ConnectionPoint.sHashtableForLinkRebuilding.Add(hashCode, connexion);
						}
						else
						{
							// set the connexion type, if not set during the above creation
							if (isConnectionValid)
								connexion.Type = BrickLibrary.Instance.getConnexionType(this.PartNumber, connexionIndex);
						}

						//read the connexion data and add it in the Connection list
						connexion.mMyBrick = this;
						connexion.ReadXml(reader);

						// during the reading of the connection list in the file, we check if
						// we didn't reached the limit of the part library. If there is more connection
						// in the file than in the part lib, we continue to read the connections,
						// but we don't add them in the connection list.
						if (isConnectionValid)
							mConnectionPoints.Add(connexion);

						// increment the connexion index
						connexionIndex++;

						// read the next brick
						connexionFound = reader.ReadToNextSibling("Connexion");
					}
					reader.ReadEndElement();

					// check if we read all the connections in the file, if not we have to instanciate
					// empty connection to fullfill the list
					if (mConnectionPoints != null)
						for (int i = mConnectionPoints.Count; i < mConnectionPoints.Capacity; ++i)
						{
							ConnectionPoint connexion = new ConnectionPoint(this, i);
							mConnectionPoints.Add(connexion);
							// we don't need to add this connection in the hastable since we know this
							// connection doesn't exist in the file, so there is no link attached to it
						}

					// update the connexion position which is not stored in the bbm file
					// in file version before 3 it was stored, but I removed it because the connexion
					// point can move in different part libraries
					updateConnectionPosition();
				}
				else
				{
					reader.Read();
				}
				// read the end element of the brick
				reader.ReadEndElement();
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("Brick");
				base.WriteXml(writer);
				writer.WriteElementString("PartNumber", mPartNumber);
				writer.WriteElementString("Orientation", mOrientation.ToString(System.Globalization.CultureInfo.InvariantCulture));
				writer.WriteElementString("ActiveConnectionPointIndex", mActiveConnectionPointIndex.ToString());
				writer.WriteElementString("Altitude", mAltitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
				// save the connexion points if any
				writer.WriteStartElement("Connexions");
				if (mConnectionPoints != null)
				{
					writer.WriteAttributeString("count", mConnectionPoints.Count.ToString());
					foreach (ConnectionPoint connexion in mConnectionPoints)
					{
						writer.WriteStartElement("Connexion");
						connexion.WriteXml(writer);
						writer.WriteEndElement();
					}
				}
				else
				{
					writer.WriteAttributeString("count", "0");
				}
				writer.WriteEndElement(); // end of Connexions
				writer.WriteEndElement(); // end of Brick
			}

			#endregion

			#region update method
			static public PointF sGetMinMaxAndSize(PointF[] points, ref PointF min, ref PointF max)
			{
				min = points[0];
				max = points[0];
				for (int i = 1; i < points.Length; ++i)
				{
					if (points[i].X < min.X)
						min.X = points[i].X;
					if (points[i].Y < min.Y)
						min.Y = points[i].Y;
					if (points[i].X > max.X)
						max.X = points[i].X;
					if (points[i].Y > max.Y)
						max.Y = points[i].Y;
				}
				// The +1 is added because this function is currently used for computing a size from
				// the hull or bounding box which is expressed in pixel, and both min and max pixel
				// coord should be included in the size
				return new PointF(Math.Abs(max.X - min.X) + 1.0f, Math.Abs(max.Y - min.Y) + 1.0f);
			}

			private void updateImage()
			{
				List<PointF> boundingBox = null;
				List<PointF> hull = null;
				mOriginalImageReference = BrickLibrary.Instance.getImage(mPartNumber, ref boundingBox, ref hull);
				// check if the image is not in the library, create one
				if (mOriginalImageReference == null)
				{
					// add a default image in the library and ask it again
					BrickLibrary.Instance.AddUnknownBrick(mPartNumber, (int)(mDisplayArea.Width), (int)(mDisplayArea.Height));
					mOriginalImageReference = BrickLibrary.Instance.getImage(mPartNumber, ref boundingBox, ref hull);
				}
				// normally now, we should have an image
				// transform the bounding box of the part
				PointF[] boundingPoints = boundingBox.ToArray();
				Matrix rotation = new Matrix();
				rotation.Rotate(mOrientation);
				rotation.TransformVectors(boundingPoints);

				// get the min, max and the size of the bounding box
				PointF boundingMin = new PointF();
				PointF boundingMax = new PointF();
				PointF boundingSize = sGetMinMaxAndSize(boundingPoints, ref boundingMin, ref boundingMax);

				// store computationnal variable for optimization
				const float PIXEL_TO_STUD_RATIO = 1.0f / NUM_PIXEL_PER_STUD_FOR_BRICKS;

				// transform the hull to get the selection area
				PointF[] hullArray = hull.ToArray();
				rotation.TransformVectors(hullArray);

				// check if this picture has a specific hull
				if (hull != boundingBox)
				{
					// get the bounding size from the hull
					PointF hullMin = new PointF();
					PointF hullMax = new PointF();
					PointF hullSize = sGetMinMaxAndSize(hullArray, ref hullMin, ref hullMax);

					// compute the offset between the hull and the normal bounding box
					PointF deltaMin = new PointF(boundingMin.X - hullMin.X, boundingMin.Y - hullMin.Y);
					PointF deltaMax = new PointF(boundingMax.X - hullMax.X, boundingMax.Y - hullMax.Y);
					mOffsetFromOriginalImage = new PointF((deltaMax.X + deltaMin.X) * PIXEL_TO_STUD_RATIO * 0.5f,
														  (deltaMax.Y + deltaMin.Y) * PIXEL_TO_STUD_RATIO * 0.5f);

					// overwrite the bounding size and min with the hull ones which are more precise
					boundingSize = hullSize;
					boundingMin = hullMin;
				}
				else
				{
					mOffsetFromOriginalImage = new PointF(0, 0);
				}

				// set the size of the display area with the new computed bounding size, and recompute the snap to grid offset
				mDisplayArea.Width = boundingSize.X * PIXEL_TO_STUD_RATIO;
				mDisplayArea.Height = boundingSize.Y * PIXEL_TO_STUD_RATIO;
				mTopLeftCornerInPixel = new PointF(-boundingMin.X, -boundingMin.Y);

				// adjust the selection area after computing the new display area size to have a correct center
				// first we add the translation of the top left corner in pixel to the hull point already in pixel
				// then convert the pixel to studs, and finally add the top left corner in stud
				Matrix translation = new Matrix();
				translation.Translate(mTopLeftCornerInPixel.X, mTopLeftCornerInPixel.Y);
				translation.Scale(PIXEL_TO_STUD_RATIO, PIXEL_TO_STUD_RATIO, MatrixOrder.Append);
				translation.Translate(Center.X - (mDisplayArea.Width * 0.5f), Center.Y - (mDisplayArea.Height * 0.5f), MatrixOrder.Append);
				translation.TransformPoints(hullArray);

				// create the new selection area from the rotated hull
				mSelectionArea = new Tools.Polygon(hullArray);

				// clear the new images array for all the levels
				clearMipmapImages(0, mMipmapImages.Length - 1);
			}

			public void clearMipmapImages(int from, int to)
			{
				// create the new images for all the levels and draw in them
				for (int i = from; i <= to; ++i)
				{
					if (mMipmapImages[i] != null)
						mMipmapImages[i].Dispose();
					mMipmapImages[i] = null;
				}
			}

			/// <summary>
			/// construct an image from the referenced image in the database and which size depend on the 
			/// the mipmap level. Level 0 correspond to the full size (same size as in the part database).
			/// The image is of course computed base on the current orientation of this brick
			/// </summary>
			/// <param name="mipmapLevel">a value from 0 to 5</param>
			/// <returns>an image scaled depending on the mipmap level</returns>
			private Image createImage(int mipmapLevel)
			{
				// create the transform
				int powerOfTwo = (1 << mipmapLevel);
				Matrix transform = new Matrix();
				transform.Rotate(mOrientation);
				transform.Translate(mTopLeftCornerInPixel.X / powerOfTwo, mTopLeftCornerInPixel.Y / powerOfTwo, MatrixOrder.Append);
				// create a new image with the correct size
				int newWidth = (int)((mDisplayArea.Width * NUM_PIXEL_PER_STUD_FOR_BRICKS) / powerOfTwo);
				int newHeight = (int)((mDisplayArea.Height * NUM_PIXEL_PER_STUD_FOR_BRICKS) / powerOfTwo);
				if ((newWidth > 0) && (newHeight > 0))
				{
					Bitmap image = new Bitmap(newWidth, newHeight);
					// get the graphic context and draw the referenc image in it with the correct transform and scale
					Graphics graphics = Graphics.FromImage(image);
					graphics.Transform = transform;
					graphics.Clear(Color.Transparent);
					graphics.CompositingMode = CompositingMode.SourceCopy; // this should be enough since we draw the image on an empty transparent area
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.CompositingQuality = CompositingQuality.HighSpeed;
					graphics.InterpolationMode = InterpolationMode.HighQualityBilinear; // we need it for the high scale down version
					graphics.DrawImage(mOriginalImageReference, 0, 0, (float)(mOriginalImageReference.Width) / powerOfTwo, (float)(mOriginalImageReference.Height) / powerOfTwo);
					graphics.Flush();
					// return the created image
					return image;
				}
				else
				{
					// if the resulting image is too small return a specific image pointer for skipping reason
					return sInvalidDummyImageToSkip;
				}
			}

			private void updateConnectionPosition()
			{
				if (mConnectionPoints != null)
				{
					List<PointF> pointList = BrickLibrary.Instance.getConnectionPositionList(mPartNumber);
					if (pointList != null)
					{
						Matrix rotation = new Matrix();
						rotation.Rotate(mOrientation);
						PointF[] pointArray = pointList.ToArray();
						rotation.TransformVectors(pointArray);

						PointF center = this.Center;
						center.X += mOffsetFromOriginalImage.X;
						center.Y += mOffsetFromOriginalImage.Y;

						// in this function we assume the two arrays have the same size,
						// i.e. mConnectionPoints.Count == pointArray.Length
						// during the loading code we have created the mConnectionPoints with the
						// same size as the part library.
						for (int i = 0; i < pointList.Count; ++i)
						{
							mConnectionPoints[i].setPositionReservedForBrick(center.X + pointArray[i].X,
																			center.Y + pointArray[i].Y);
						}
					}
				}
			}

			/// <summary>
			/// Return a list of connections that belongs to this brick or to other bricks that belong to the same
			/// hierarchical group than this brick.
			/// This function can return an empty list but not null.
			/// </summary>
			/// <param name="myTopGroup">If this brick belongs to a group, set the top group in this param, or null otherwise</param>
			/// <returns>A list of connection in this brick and other brick of the same group (can be empty)</returns>
			private List<Brick.ConnectionPoint> getConnectionsListForAllMyGroup(out Group myTopGroup)
			{
				// get all the bricks in the group of this brick
				myTopGroup = this.TopGroup;
				if (myTopGroup != null)
				{
					List<LayerItem> brickList = myTopGroup.getAllChildrenItems();

					// create the result list with an estimation of the number of connections
					List<ConnectionPoint> result = new List<ConnectionPoint>(brickList.Count * 2);

					// now iterate on all the bricks to get all the free connection points
					foreach (LayerItem item in brickList)
					{
						Brick brick = item as Brick;
						// iterate on the connections points to find the free ones
						if (brick.mConnectionPoints != null)
							result.AddRange(brick.mConnectionPoints);
					}

					// return the result that may be null
					return result;
				}
				else
				{
					if (mConnectionPoints != null)
						return mConnectionPoints;
					else
						return (new List<ConnectionPoint>(0));
				}
			}

			public void setActiveConnectionPointUnder(PointF positionInStudCoord)
			{
				// get all the connection points
				Group myTopGroup = null;
				List<Brick.ConnectionPoint> connectionList = getConnectionsListForAllMyGroup(out myTopGroup);

				// find the closest free connection point from the mouse position
				float bestSquareDistance = float.MaxValue;
				int bestConnectionIndex = -1;
				for (int i = 0; i < connectionList.Count; ++i)
					if (connectionList[i].IsFree)
					{
						PointF point = connectionList[i].PositionInStudWorldCoord;
						float dx = positionInStudCoord.X - point.X;
						float dy = positionInStudCoord.Y - point.Y;
						float squareDistance = (dx * dx) + (dy * dy);
						if (squareDistance < bestSquareDistance)
						{
							bestSquareDistance = squareDistance;
							bestConnectionIndex = i;
						}
					}

				// check if we found a connection index to set
				if (bestConnectionIndex != -1)
				{
					// check if this brick belongs to a group
					if (myTopGroup != null)
						myTopGroup.ActiveConnectionIndex = bestConnectionIndex;
					else
						mActiveConnectionPointIndex = bestConnectionIndex;
				}
			}

			/// <summary>
			/// Set the active connection index with the next one in the list.
			/// </summary>
			/// <param name="ignoreIfNotMainBrickOfAGroup">If true, do nothing if this brick belongs to a group and this brick is not the brick that hold the active connection index</param>
			public void setActiveConnectionPointWithNextOne(bool ignoreIfNotMainBrickOfAGroup)
			{
				// get all the connection points for this brick or among all the connection of my group (if I belong to a group)
				Group myTopGroup = null;
				List<Brick.ConnectionPoint> connectionList = getConnectionsListForAllMyGroup(out myTopGroup);

				// if there is no connection on this part neither inside my group, just exit the function
				if (connectionList.Count == 0)
					return;

				// check if we need to ignore this change if this brick belongs to a group
				// and that you want to change the active connection of the whole group
				if (ignoreIfNotMainBrickOfAGroup && (myTopGroup != null) &&
					(myTopGroup.BrickThatHoldsActiveConnection != this))
					return;

				// memorize the current index to know when if we are looping
				int currentActiveConnectionIndex = mActiveConnectionPointIndex;
				if (myTopGroup != null)
					currentActiveConnectionIndex = myTopGroup.ActiveConnectionIndex;
				// initialize the new connection index with the current one
				int nextActiveConnectionIndex = currentActiveConnectionIndex;

				// iterate until we find the next free connection point or that we made a full loop
				do
				{
					// go to the next one (and loop if reaching the end of the list)
					nextActiveConnectionIndex++;
					if (nextActiveConnectionIndex >= connectionList.Count)
						nextActiveConnectionIndex = 0;
					// until we find a free point or we tested them all
				} while (!connectionList[nextActiveConnectionIndex].IsFree && (nextActiveConnectionIndex != currentActiveConnectionIndex));

				// check if we found a connection index to set
				if (nextActiveConnectionIndex != currentActiveConnectionIndex)
				{
					// check if this brick belongs to a group
					if (myTopGroup != null)
						myTopGroup.ActiveConnectionIndex = nextActiveConnectionIndex;
					else
						mActiveConnectionPointIndex = nextActiveConnectionIndex;
				}
			}

			/// <summary>
			/// Attach the ruler specified in the anchor parameter with the other anchor properties to this brick
			/// </summary>
			/// <param name="anchor">attachement properties</param>
			public void attachRuler(RulerAttachementSet.Anchor anchor)
			{
				// create the set if it doesn't exist yet
				if (mAttachedRulers == null)
					mAttachedRulers = new RulerAttachementSet(this);
				mAttachedRulers.attachRuler(anchor);
			}

			/// <summary>
			/// Detach the ruler specified in the anchor parameter from this brick
			/// </summary>
			/// <param name="anchor">the anchor to detach</param>
			public void detachRuler(RulerAttachementSet.Anchor anchor)
			{
				if (mAttachedRulers != null)
					mAttachedRulers.detachRuler(anchor);
			}

			/// <summary>
			/// get the anchor for the specified ruler for its current control point
			/// </summary>
			/// <param name="rulerItem">the anchor or null if the specified control point is not attached</param>
			/// <returns></returns>
			public RulerAttachementSet.Anchor getRulerAttachmentAnchor(LayerRuler.RulerItem rulerItem)
			{
				if (mAttachedRulers != null)
					return mAttachedRulers.getRulerAttachmentAnchor(rulerItem);
				return null;
			}
			#endregion

			#region get image
			/// <summary>
			/// return the current image of the brick depending on it's mipmap level (LOD level)
			/// level 0 is the most detailed level. Can return null if the image too small to be
			/// visible for the LOD specified.
			/// </summary>
			/// <param name="mipmapLevel">the level of detail</param>
			/// <returns>the image of the brick or null if the image is too small to be seen</returns>
			public Image getImage(int mipmapLevel)
			{
				// the result image
				Image image = null;
				// check if the mipmap level is under the level that should be saved in memory
				// if yes that means the image is not saved and must be recreated every time
				if (mipmapLevel < Properties.Settings.Default.StartSavedMipmapLevel)
				{
					image = createImage(mipmapLevel);
				}
				else
				{
					// avoid having the mipmap level above the save array
					if (mipmapLevel >= mMipmapImages.Length)
						mipmapLevel = mMipmapImages.Length - 1;
					// create the image dynamically if not already created
					if (mMipmapImages[mipmapLevel] == null)
						mMipmapImages[mipmapLevel] = createImage(mipmapLevel);
					// return the saved image
					image = mMipmapImages[mipmapLevel];
				}
				// check if the saved or generated image is a dummy one
				if (image == sInvalidDummyImageToSkip)
					return null;
				return image;
			}
			#endregion
		}
	}
}
