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
using BlueBrick.MapData.FlexTrack;

namespace BlueBrick.MapData
{
	[Serializable]
	public class LayerBrick : Layer
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
				public PointF mPositionInStudWorldCoord = new PointF(0, 0); // the position of the connection point is world coord stud coord.
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
					get	{ return mConnectionLink; }
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
			}

			private string mPartNumber = null;	// id of the part
			private int mActiveConnectionPointIndex = 0; // the current active connection point in the connexion point list
			private List<ConnectionPoint> mConnectionPoints = null; // list of all the connection point (if this brick can connect)
			private float mAltitude = 0.0f; //for improving compatibility with LDRAW we save a up coordinate for each brick

			// the image and the connection point are not serialized, they are built in the constructor
			// or when the part number property is set by the serializer
			[NonSerialized]
			private Image[] mMipmapImages = new Image[5];	// all the images in different LOD level
			[NonSerialized]
			private Image mOriginalImageReference = null;	// reference on the original image in the database
			[NonSerialized]
			private PointF mTopLeftCornerInPixel = PointF.Empty;
			[NonSerialized]
			private PointF mSnapToGridOffset = new PointF(0, 0); // an offset from the center of the part to the point that should snap to the grid border (in stud)
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
					updateConnectionPosition();
				}
			}

			/// <summary>
			///	Set the position in stud coord. The position of a brick is its top left corner.
			/// </summary>
			public override PointF Position
			{
				set
				{
					mDisplayArea.X = value.X;
					mDisplayArea.Y = value.Y;
					updateConnectionPosition();
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
					mDisplayArea.X = value.X - (mDisplayArea.Width * 0.5f);
					mDisplayArea.Y = value.Y - (mDisplayArea.Height * 0.5f);
					updateConnectionPosition();				
				}
			}

			/// <summary>
			/// an offset from the center of the part to the point that should snap a corner of the grid
			/// </summary>
			public PointF SnapToGridOffset
			{
				get { return mSnapToGridOffset; }
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
						return mConnectionPoints[mActiveConnectionPointIndex].mPositionInStudWorldCoord;
					return new PointF();
				}
				set
				{
					if (mConnectionPoints != null)
					{
						PointF newCenter = this.Center;
						newCenter.X += value.X - mConnectionPoints[mActiveConnectionPointIndex].mPositionInStudWorldCoord.X;
						newCenter.Y += value.Y - mConnectionPoints[mActiveConnectionPointIndex].mPositionInStudWorldCoord.Y;
						this.Center = newCenter;
					}
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
			public Brick Clone()
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
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
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
				writer.WriteEndElement();
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
				return new PointF(Math.Abs(max.X - min.X), Math.Abs(max.Y - min.Y));
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

				// check if this picture has a specific hull
				if (hull != boundingBox)
				{
					// there's a more precise hull, so transform it and get its size
					PointF[] hullPoints = hull.ToArray();
					rotation.TransformVectors(hullPoints);

					PointF hullMin = new PointF();
					PointF hullMax = new PointF();
					PointF hullSize = sGetMinMaxAndSize(hullPoints, ref hullMin, ref hullMax);

					// compute the offset between the hull and the normal bounding box
					PointF deltaMin = new PointF(boundingMin.X - hullMin.X, boundingMin.Y - hullMin.Y);
					PointF deltaMax = new PointF(boundingMax.X - hullMax.X, boundingMax.Y - hullMax.Y);
					mOffsetFromOriginalImage = new PointF((deltaMax.X + deltaMin.X) / (NUM_PIXEL_PER_STUD_FOR_BRICKS * 2),
														  (deltaMax.Y + deltaMin.Y) / (NUM_PIXEL_PER_STUD_FOR_BRICKS * 2));

					// overwrite the bounding size and min with the hull ones which are more precise
					boundingSize = hullSize;
					boundingMin = hullMin;
				}
				else
				{
					mOffsetFromOriginalImage = new PointF(0, 0);
				}

				// compute the snapToGridOffset
				BrickLibrary.Brick.Margin snapMargin = BrickLibrary.Instance.getSnapMargin(mPartNumber);
				if ((snapMargin.mLeft != 0.0f) || (snapMargin.mRight != 0.0f) || (snapMargin.mTop != 0.0f) || (snapMargin.mBottom != 0.0f))
				{
					double angleInRadian = mOrientation * Math.PI / 180.0;
					double cosAngle = Math.Cos(angleInRadian);
					double sinAngle = Math.Sin(angleInRadian);
					double xSnapOffset = 0;
					double ySnapOffset = 0;
					if (cosAngle > 0)
					{
						xSnapOffset = snapMargin.mLeft * cosAngle;
						ySnapOffset = snapMargin.mTop * cosAngle;
					}
					else
					{
						cosAngle = -cosAngle;
						xSnapOffset = snapMargin.mRight * cosAngle;
						ySnapOffset = snapMargin.mBottom * cosAngle;
					}
					if (sinAngle > 0)
					{
						xSnapOffset += snapMargin.mBottom * sinAngle;
						ySnapOffset += snapMargin.mLeft * sinAngle;
					}
					else
					{
						sinAngle = -sinAngle;
						xSnapOffset += snapMargin.mTop * sinAngle;
						ySnapOffset += snapMargin.mRight * sinAngle;
					}
					mSnapToGridOffset = new PointF((float)xSnapOffset, (float)ySnapOffset);
				}
				else
				{
					mSnapToGridOffset = new PointF(0, 0);
				}

				// set the size of the display area with the new computed bounding size, and recompute the snap to grid offset
				mDisplayArea.Width = boundingSize.X / NUM_PIXEL_PER_STUD_FOR_BRICKS;
				mDisplayArea.Height = boundingSize.Y / NUM_PIXEL_PER_STUD_FOR_BRICKS;
				mTopLeftCornerInPixel = new PointF(-boundingMin.X, -boundingMin.Y);

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
							mConnectionPoints[i].mPositionInStudWorldCoord.X = center.X + pointArray[i].X;
							mConnectionPoints[i].mPositionInStudWorldCoord.Y = center.Y + pointArray[i].Y;
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
						PointF point = connectionList[i].mPositionInStudWorldCoord;
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
				// get all the connection points
				Group myTopGroup = null;
				List<Brick.ConnectionPoint> connectionList = getConnectionsListForAllMyGroup(out myTopGroup);

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

		[NonSerialized]
		private ImageAttributes mImageAttributeForSelection = new ImageAttributes();
		private ImageAttributes mImageAttributeForSnapping = new ImageAttributes();
		private ImageAttributes mImageAttributeDefault = new ImageAttributes();

		// list of bricks and connection points
		private List<Brick> mBricks = new List<Brick>(); // all the bricks in the layer
		private FreeConnectionSet mFreeConnectionPoints = new FreeConnectionSet();

		//related to selection
		private Brick mCurrentBrickUnderMouse = null;
		private PointF mMouseDownInitialPosition;
		private PointF mMouseDownLastPosition;
		private PointF mMouseGrabDeltaToCenter; // The delta between the grab point of the mouse inside the grabed brick, to the center of that brick
		private PointF mMouseGrabDeltaToActiveConnectionPoint; // The delta between the grab point of the mouse inside the grabed brick, to the active connection point of that brick
		private bool mMouseIsBetweenDownAndUpEvent = false;
		private bool mMouseHasMoved = false;
		private bool mMouseMoveIsAFlexMove = false;
		private FlexMove mMouseFlexMoveAction = null;
		private bool mMouseMoveIsADuplicate = false;
		private DuplicateBrick mLastDuplicateBrickAction = null; // temp reference use during a ALT+mouse move action (that duplicate and move the bricks at the same time)
		private RotateBrickOnPivotBrick mRotationForSnappingDuringBrickMove = null; // this action is used temporally during the edition, while you are moving the selection next to a connectable brick. The Action is not recorded in the ActionManager because it is a temporary one.
		private float mSnappingOrientation = 0.0f; // this orientation is just used during the the edition of a group of part if they snap to a free connexion point

		#region get/set
		/// <summary>
		/// An accessor on the brick list for saving in different fomat
		/// </summary>
		public List<Brick> BrickList
		{
			get { return mBricks; }
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
		#endregion

		#region constructor
		public LayerBrick()
		{
			// update the gamma setting when the layer is created
			updateGammaFromSettings();
		}

		public override int getNbItems()
		{
			return mBricks.Count;
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
			base.ReadXml(reader);
			// clear all the content of the hash table
			Brick.ConnectionPoint.sHashtableForLinkRebuilding.Clear();
			// the brick
			bool brickFound = reader.ReadToDescendant("Brick");
			while (brickFound)
			{
				// instanciate a new brick, read and add the new brick
				Brick brick = new Brick();
				brick.ReadXml(reader);
				mBricks.Add(brick);

				// read the next brick
				brickFound = reader.ReadToNextSibling("Brick");

				// step the progress bar for each brick
				MainForm.Instance.stepProgressBar();
			}
			// read the Bricks tag, to finish the list of brick
			reader.Read();

			// call the post read function to read the groups
			postReadXml(reader);
			
			// clear again the hash table to free the memory after loading
			Brick.ConnectionPoint.sHashtableForLinkRebuilding.Clear();

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
									 !arePositionsEqual(connexion.mPositionInStudWorldCoord, connexion.ConnectionLink.mPositionInStudWorldCoord)))) // case 3)
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

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteAttributeString("type", "brick");
			base.WriteXml(writer);
			// and serialize the brick list
			writer.WriteStartElement("Bricks");
			foreach (Brick brick in mBricks)
			{
				writer.WriteStartElement("Brick");
				brick.WriteXml(writer);
				writer.WriteEndElement();
				// step the progress bar for each brick
				MainForm.Instance.stepProgressBar();
			}
			writer.WriteEndElement();

			// call the post write to write the group list
			postWriteXml(writer);
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

			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickAdded(this, brickToAdd);
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

			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickRemoved(this, brickToRemove);

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

		/// <summary>
		/// This static tool method, clone all the item of the specified list into a new list.
		/// This method also clone the groups that may belong to this list of bricks.
		/// The cloned bricks are in the same order as the original list
		/// </summary>
		/// <param name="listToCopy">The original list of brick to copy</param>
		/// <returns>A clone list of cloned brick with there cloned groups</returns>
		public static List<LayerItem> sCloneBrickList(List<LayerItem> listToClone)
		{
			// the resulting list
			List<LayerItem> result = new List<LayerItem>(listToClone.Count);

			// use a dictionnary to recreate the groups that may be inside the list of brick to duplicate
			// this dictionnary makes an association between the group to duplicate and the new duplicated one
			Dictionary<Group, Group> groupsToCreate = new Dictionary<Group, Group>();
			// also use a list of item that we will make grow to create all the groups
			List<LayerItem> fullOriginalItemList = new List<LayerItem>(listToClone);

			// use a for instead of a foreach because the list will grow
			for (int i = 0; i < fullOriginalItemList.Count; ++i)
			{
				// get the current item
				LayerItem originalItem = fullOriginalItemList[i];
				LayerItem duplicatedItem = null;

				// check if the item is a group or a brick
				if (originalItem.IsAGroup)
				{
					// if the item is a group that means the list already grown, and that means we also have it in the dictionnary
					Group associatedGroup = null;
					groupsToCreate.TryGetValue(originalItem as Group, out associatedGroup);
					duplicatedItem = associatedGroup;
				}
				else
				{
					// if the item is a brick, just clone it and add it to the result
					// clone the item (because the same list of text to add can be paste several times)
					duplicatedItem = (originalItem as Brick).Clone();
					// add the duplicated item in the list
					result.Add(duplicatedItem);
				}

				// check if the item to clone belongs to a group then also duplicate the group
				if (originalItem.Group != null)
				{
					// get the duplicated group if already created otherwise create it and add it in the dictionary
					Group duplicatedGroup = null;
					groupsToCreate.TryGetValue(originalItem.Group, out duplicatedGroup);
					if (duplicatedGroup == null)
					{
						duplicatedGroup = new Group(originalItem.Group);
						groupsToCreate.Add(originalItem.Group, duplicatedGroup);
						fullOriginalItemList.Add(originalItem.Group);
					}
					// assign the group to the brick
					duplicatedGroup.addItem(duplicatedItem);
					// check if we need to also assign the brick that hold the connection point
					if (originalItem.Group.BrickThatHoldsActiveConnection == originalItem)
						duplicatedGroup.BrickThatHoldsActiveConnection = (duplicatedItem as Brick);
				}
			}

			// delete the dictionary
			groupsToCreate.Clear();
			fullOriginalItemList.Clear();
			// return the cloned list
			return result;
		}

		/// <summary>
		/// Copy the list of the selected bricks in a separate list for later use.
		/// This method should be called on a CTRL+C
		/// </summary>
		public void copyCurrentSelection()
		{
			// reset the copy list
			sCopyItems.Clear();
			// Sort the seltected list as it is sorted on the layer such as the clone list
			// will be also sorted as on the layer
			LayerItemComparer<LayerBrick.Brick> comparer = new LayerItemComparer<LayerBrick.Brick>(mBricks);
			SelectedObjects.Sort(comparer);
			// and copy the list
			sCopyItems = sCloneBrickList(SelectedObjects);
		}

		/// <summary>
		/// Paste (duplicate) the list of bricks that was previously copied with a call to copyCurrentSelection()
		/// This method should be called on a CTRL+V
		/// </summary>
		public void pasteCopiedList()
		{
			// To paste, we need to have copied something
			if (sCopyItems.Count > 0)
			{
				mLastDuplicateBrickAction = new DuplicateBrick(this, sCopyItems);
				ActionManager.Instance.doAction(mLastDuplicateBrickAction);
			}
		}
		#endregion

		#region selection
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
		/// This method return the unique brick in the layer to which you can connect another brick
		/// or null if there's no suitable brick candidate for connection.
		/// If there's only one brick selected, this brick is return, otherwise check
		/// if all the bricks selected belongs to the same hierarchical tree of group,
		/// meaning the top group parent is the same for all the bricks.
		/// If yes, then it return the brick that hold the active connection in the group (which can be null)
		/// </summary>
		/// <returns>The brick that is connectable and should display its active connection point, or null</returns>
		public Brick getConnectableBrick()
		{
			LayerItem topItem = sGetTopItemFromList(mSelectedObjects);
			if (topItem != null)
			{
				if (topItem.IsAGroup)
					return (topItem as Group).BrickThatHoldsActiveConnection;
				else
					return (topItem as Brick);
			}
			return null;
		}

		/// <summary>
		/// This static tool method return the top item of a hierachical group of bricks, or null if all the
		/// bricks of the list doesn't belong to the same unique hierarchical group.
		/// </summary>
		/// <param name="brickList">a list of bricks among which we should search a top item</param>
		/// <returns>a Brick or a Group which is at the top of the hierarchical group</returns>
		public static LayerItem sGetTopItemFromList(List<LayerItem> brickList) 
		{
			if (brickList.Count == 1)
			{
				return brickList[0];
			}
			else if (brickList.Count > 1)
			{
				Layer.Group topGroup = null;
				foreach (Layer.LayerItem item in brickList)
				{
					// get the group of the item
					Layer.Group fatherGroup = item.Group;
					// if any item doesn't have any group, since there's several items selected,
					// we know that they cannot be in the same group
					if (fatherGroup == null)
						return null;
					// find the top father
					while (fatherGroup.Group != null)
						fatherGroup = fatherGroup.Group;
					// if the top group is not initialized yet, do it now
					if (topGroup == null)
						topGroup = fatherGroup;
					// if we found two different top father, stop the search
					if (fatherGroup != topGroup)
						return null;
				}
				// iteration finished without finding different top group, so it's ok
				return topGroup;
			}
			// no object selected (selection is empty)
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
							if (!arePositionsEqual(connexion.mPositionInStudWorldCoord, connexion.ConnectionLink.mPositionInStudWorldCoord))
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
							if (arePositionsEqual(selConnexion.mPositionInStudWorldCoord, freeConnexion.mPositionInStudWorldCoord))
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
								arePositionsEqual(brickConnexion.mPositionInStudWorldCoord, freeConnexion.mPositionInStudWorldCoord))
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
		/// <param name="brick1"></param>
		/// <param name="brick2"></param>
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
		public RectangleF getTotalAreaInStud()
		{
			PointF topLeft = new PointF(float.MaxValue, float.MaxValue);
			PointF bottomRight = new PointF(float.MinValue, float.MinValue);
			foreach (Brick brick in mBricks)
			{
				RectangleF brickArea = brick.DisplayArea;
				if (brickArea.X < topLeft.X)
					topLeft.X = brickArea.X;
				if (brickArea.Y < topLeft.Y)
					topLeft.Y = brickArea.Y;
				if (brickArea.Right > bottomRight.X)
					bottomRight.X = brickArea.Right;
				if (brickArea.Bottom > bottomRight.Y)
					bottomRight.Y = brickArea.Bottom;
			}
			return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
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
						if (brick == mCurrentBrickUnderMouse)
							g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, mImageAttributeForSnapping);
						else if (mSelectedObjects.Contains(brick))
							g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, mImageAttributeForSelection);
						else
							g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, mImageAttributeDefault);

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
						float x = (float)((connexion.mPositionInStudWorldCoord.X - sizeInStud - areaInStud.Left) * scalePixelPerStud);
						float y = (float)((connexion.mPositionInStudWorldCoord.Y - sizeInStud - areaInStud.Top) * scalePixelPerStud);
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
			if (mMouseIsBetweenDownAndUpEvent)
			{
				// the second test after the or, is because we give a second chance to the user to duplicate
				// the selection if he press the duplicate key after the mouse down, but before he start to move
				if (mMouseMoveIsADuplicate ||
					(!mMouseHasMoved && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)))
					return MainForm.Instance.BrickDuplicateCursor;
				else
					return Cursors.SizeAll;
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
			// set the new value
			mCurrentBrickUnderMouse = brick;

			// update the 2 grab distance if you change the brick under the mouse
			if (brick != null)
			{
				// grab distance to center
				mMouseGrabDeltaToCenter = new PointF(mouseCoordInStud.X - brick.Center.X, mouseCoordInStud.Y - brick.Center.Y);
				// grab distance to the active connection point
				Brick.ConnectionPoint activeConnectionPoint = brick.ActiveConnectionPoint;
				if (activeConnectionPoint != null)
					mMouseGrabDeltaToActiveConnectionPoint = new PointF(mouseCoordInStud.X - activeConnectionPoint.mPositionInStudWorldCoord.X, mouseCoordInStud.Y - activeConnectionPoint.mPositionInStudWorldCoord.Y);
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

			// check if the mouse is inside the bounding rectangle of the selected objects
			bool isMouseInsideSelectedObjects = isPointInsideSelectionRectangle(mouseCoordInStud);
			if (!isMouseInsideSelectedObjects && (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
				&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey))
				clearSelection();

			// clear the current brick under the mouse and compute it again
			mCurrentBrickUnderMouse = null;

			// We search if there is a cell under the mouse but in priority we choose from the current selected bricks
			mCurrentBrickUnderMouse = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as Brick;

			// if the current selected brick is not under the mouse we search among the other bricks
			// but in reverse order to choose first the brick on top
			if (mCurrentBrickUnderMouse == null)
				mCurrentBrickUnderMouse = getBrickUnderMouse(mouseCoordInStud);

			// save a flag that tell if it is a simple move or a duplicate of the selection
			// Be carreful for a duplication we take only the selected objects, not the cell
			// under the mouse that may not be selected
			mMouseMoveIsADuplicate = isMouseInsideSelectedObjects &&
									(Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);

			// if we move the brick, use 4 directionnal arrows cursor
			// if there's a brick under the mouse, use the hand
			bool willMoveSelectedObject = (isMouseInsideSelectedObjects || (mCurrentBrickUnderMouse != null))
										&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
										&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);

			// check if it is a double click, to see if we need to do a flex move
			if (!mMouseMoveIsADuplicate && (mCurrentBrickUnderMouse != null) && (e.Clicks == 2))
			{
				mMouseFlexMoveAction = new FlexMove(this, this.SelectedObjects, mCurrentBrickUnderMouse, mouseCoordInStud);
				mMouseMoveIsAFlexMove = mMouseFlexMoveAction.IsValid;
				// update the selection with only the brick in the flex chain
				if (mMouseMoveIsAFlexMove)
					mSelectedObjects = new List<LayerItem>(mMouseFlexMoveAction.BricksInTheFlexChain);
				else
					mMouseFlexMoveAction = null;
			}

			// select the appropriate cursor:
			if (mMouseMoveIsADuplicate)
				preferedCursor = MainForm.Instance.BrickDuplicateCursor;
			else if (mMouseMoveIsAFlexMove)
				preferedCursor = MainForm.Instance.FlexArrowCursor;
			else if (willMoveSelectedObject)
				preferedCursor = Cursors.SizeAll;
			else if (mCurrentBrickUnderMouse == null)
				preferedCursor = Cursors.Cross;

			// handle the mouse down if we duplicate or move the selected bricks
			bool willHandleTheMouse = (mMouseMoveIsADuplicate || mMouseMoveIsAFlexMove || willMoveSelectedObject);
			// reset the brick pointer under the mouse if finally we don't care.
			if (!willHandleTheMouse)
				mCurrentBrickUnderMouse = null;

			// compute the grab point if we grab a brick
			setBrickUnderMouse(mCurrentBrickUnderMouse, mouseCoordInStud);

			// return the result
			return willHandleTheMouse;
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

			// if finally we are called to handle this mouse down,
			// we add the cell under the mouse if the selection list is empty
			if ((mCurrentBrickUnderMouse != null) && !mMouseMoveIsADuplicate
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
				if (!mMouseMoveIsADuplicate)
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

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud)
		{
			if (mSelectedObjects.Count > 0)
			{
				// snap the mouse coord to the grid
				Brick.ConnectionPoint snappedConnection = null;
				PointF mouseCoordInStudSnapped = getMovedSnapPoint(mouseCoordInStud, mCurrentBrickUnderMouse, out snappedConnection);

				// check if it is a flex move or normal move
				if (mMouseMoveIsAFlexMove)
				{
					mMouseFlexMoveAction.reachTarget(mouseCoordInStudSnapped, snappedConnection);
					return true;
				}
				else
				{
					// compute the delta move of the mouse
					PointF deltaMove = new PointF(mouseCoordInStudSnapped.X - mMouseDownLastPosition.X, mouseCoordInStudSnapped.Y - mMouseDownLastPosition.Y);
					// check if the delta move is not null
					if (deltaMove.X != 0.0f || deltaMove.Y != 0.0)
					{
						bool wereBrickJustDuplicated = false;

						// check if it is a move or a duplicate
						if (mMouseMoveIsADuplicate)
						{
							// this is a duplicate, if we didn't move yet, this is the moment to copy  and paste the selection
							// and this will change the current selection, that will be move normally after
							if (!mMouseHasMoved)
							{
								this.copyCurrentSelection();
								this.pasteCopiedList();
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
							mCurrentBrickUnderMouse = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as Brick;
							setBrickUnderMouse(mCurrentBrickUnderMouse, mouseCoordInStud);
						}
						// memorize the last position of the mouse
						mMouseDownLastPosition = mouseCoordInStudSnapped;
						// set the flag that indicate that we moved the mouse
						mMouseHasMoved = true;
						return true;
					}
					else
					{
						// give a second chance to duplicate if the user press the duplicate key
						// after pressing down the mouse key, but not if the user already moved
						if (!mMouseHasMoved && !mMouseMoveIsADuplicate)
						{
							// check if the duplicate key is pressed
							mMouseMoveIsADuplicate = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);
							// if finally the user press the duplicate key, reconnect the selection because we broke
							// the connnection between selected brick and non-selected bricks in the mouse down event.
							if (mMouseMoveIsADuplicate)
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
			if (mMouseMoveIsAFlexMove)
			{
				// finish the action for this move and add it to the manager
				mMouseFlexMoveAction.finishActionConstruction();
				ActionManager.Instance.doAction(mMouseFlexMoveAction);

				// reset the flag and forget the action
				mMouseMoveIsAFlexMove = false;
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
						if (mMouseMoveIsADuplicate)
						{
							mLastDuplicateBrickAction.updatePositionShift(deltaMove.X, deltaMove.Y);
							mLastDuplicateBrickAction = null;
							// clear also the rotation snapping, in case of a series of duplication, but do not
							// undo it, since we want to keep the rotation applied on the duplicated bricks.
							mRotationForSnappingDuringBrickMove = null;
						}
						else
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
						// update the free connexion list if the user move the brick and then go back
						// to the original place (deltaMove is null), so the link was broken because
						// of the move, so we need to recreate the link
						updateBrickConnectivityOfSelection(false);
						// reset anyway the temp reference for the duplication
						mLastDuplicateBrickAction = null;
					}
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

			mMouseIsBetweenDownAndUpEvent = false;
			mCurrentBrickUnderMouse = null;
			return true;
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public override void selectInRectangle(RectangleF selectionRectangeInStud)
		{
			// fill it with all the cells in the rectangle
			List<LayerItem> objListInRectangle = new List<LayerItem>(mBricks.Count);
			foreach (Brick brick in mBricks)
			{
				if ((selectionRectangeInStud.Right > brick.DisplayArea.Left) && (selectionRectangeInStud.Left < brick.DisplayArea.Right) &&
					(selectionRectangeInStud.Bottom > brick.DisplayArea.Top) && (selectionRectangeInStud.Top < brick.DisplayArea.Bottom))
				{
					objListInRectangle.Add(brick);
				}
			}
			// check if it is a brand new selection or a add/remove selection
			if (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
			{
				// the control key is not pressed, it is a brand new selection
				// clear the selection list and add all the object in the rectangle
				mSelectedObjects.Clear();
				addObjectInSelection(objListInRectangle);
			}
			else
			{
				// check if we found new object to add in the selection, then add them
				// else remove all the objects in the rectangle
				bool objectToAddFound = false;
				foreach (LayerItem item in objListInRectangle)
					if (!mSelectedObjects.Contains(item))
					{
						addObjectInSelection(item);
						objectToAddFound = true;
					}
				// check if it is a remove type
				if (!objectToAddFound)
					removeObjectFromSelection(objListInRectangle);
			}
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
					result = Layer.snapToGrid(pointInStud);
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
		/// <param name="itemWhichCenterIsSnapped">if one brick is grabbed, compute a snapping position for the center of this specified item. Can be null if no brick is grabbed.</param>
		/// <returns>a near snap point</returns>
		public PointF getMovedSnapPoint(PointF pointInStud, Layer.LayerItem itemWhichCenterIsSnapped)
		{
			Brick.ConnectionPoint ignoredSnappedConnection = null;
			return getMovedSnapPoint(pointInStud, itemWhichCenterIsSnapped, out ignoredSnappedConnection);
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
		/// <param name="itemWhichCenterIsSnapped">if one brick is grabbed, compute a snapping position for the center of this specified item. Can be null if no brick is grabbed.</param>
		/// <param name="snappedConnection">If the brick should snap to a connection, this is the one, otherwise null</param>
		/// <returns>a near snap point</returns>
		public PointF getMovedSnapPoint(PointF pointInStud, Layer.LayerItem itemWhichCenterIsSnapped, out Brick.ConnectionPoint snappedConnection)
		{
			// init the output value
			snappedConnection = null;
			// don't do anything is the snapping is not enabled
			if (SnapGridEnabled)
			{
				// check if there is a master brick
				if (mCurrentBrickUnderMouse != null)
				{
					// now check if the master brick has some connections
					if (mCurrentBrickUnderMouse.HasConnectionPoint)
					{
						// but we also need to check if the brick has a FREE connexion
						// but more than that we need to check if the Active Connection is a free connexion.
						// Because for connection snapping, we always snap the active connection with
						// the other bricks. That give the feedback to the user of which free connection
						// of is moving brick will try to connect
						Brick.ConnectionPoint activeBrickConnexion = mCurrentBrickUnderMouse.ActiveConnectionPoint;
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
									float dx = freeConnexion.mPositionInStudWorldCoord.X - virtualActiveConnectionPosition.X;
									float dy = freeConnexion.mPositionInStudWorldCoord.Y - virtualActiveConnectionPosition.Y;
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
							float threshold = Math.Max(CurrentSnapGridSize, mMouseMoveIsAFlexMove ? 4.0f : 8.0f);
							// check if the nearest free connexion if close enough to snap
							if (nearestSquareDistance < threshold * threshold)
							{
								// the distance to the closest connection is under the max threshold distance, so set the output value
								snappedConnection = bestFreeConnection;

								// we found a snapping connection, start to compute the snap position for the best connection
								PointF snapPosition = snappedConnection.mPositionInStudWorldCoord;

								// if it is not a flex move, rotate the selection
								if (!mMouseMoveIsAFlexMove)
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
									snapPosition.X += itemWhichCenterIsSnapped.Center.X - activeBrickConnexion.mPositionInStudWorldCoord.X;
									snapPosition.Y += itemWhichCenterIsSnapped.Center.Y - activeBrickConnexion.mPositionInStudWorldCoord.Y;
								}
								// otherwise, for a flex move, just keep the best connection as the snapping value

								// return the position
								return snapPosition;
							}
						}
					}

					// if we didn't find any connection to snap to, and if it is a flex move, just
					// return the value without snaping, otherwise, we will snap the part on the grid
					if (mMouseMoveIsAFlexMove)
						return pointInStud;

					// This is the normal case for snapping the brick under the mouse.
					// Snap the position of the mouse on the grid (the snapping is a Floor style one)
					// then add the center shift of the part and the snapping offset
					pointInStud = Layer.snapToGrid(pointInStud);
					
					// compute the center shift (including the snap grid margin
					PointF halfBrickShift = new PointF((mCurrentBrickUnderMouse.DisplayArea.Width / 2) - mCurrentBrickUnderMouse.SnapToGridOffset.X,
													(mCurrentBrickUnderMouse.DisplayArea.Height / 2) - mCurrentBrickUnderMouse.SnapToGridOffset.Y);

					// compute a snapped grab delta
					PointF snappedGrabDelta = mMouseGrabDeltaToCenter;
					snappedGrabDelta.X += halfBrickShift.X;
					snappedGrabDelta.Y += halfBrickShift.Y;
					snappedGrabDelta = Layer.snapToGrid(snappedGrabDelta);

					// shift the point according to the center and the snap grabbed delta
					pointInStud.X += halfBrickShift.X - snappedGrabDelta.X;
					pointInStud.Y += halfBrickShift.Y - snappedGrabDelta.Y;
					return pointInStud;
				}

				// the snapping is enable but the group of brick was grab from an empty place
				// i.e. there's no bricks under the mouse so just do a normal snapping on the grid
				return Layer.snapToGrid(pointInStud);
			}

			// by default do not change anything
			return pointInStud;
		}

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

			// grab the brick or group in its center
			mMouseGrabDeltaToCenter = new PointF(0.0f, 0.0f);

			// check if it is a single Brick or a group
			if (itemDrop.IsAGroup)
			{
				// the part to drop is a group
				Layer.Group groupDrop = itemDrop as Layer.Group;
				// the part to drop is a group
				List<Layer.LayerItem> partsInTheGroup = groupDrop.getAllChildrenItems();
				foreach (Layer.LayerItem item in partsInTheGroup)
				{
					Brick brick = item as Brick;
					mBricks.Add(brick);
					mSelectedObjects.Add(brick);
				}
				
				// by default the active connection index of a group is 0
				// and get the corresponding brick that hold the active connection index
				mMouseGrabDeltaToActiveConnectionPoint = new PointF(0.0f, 0.0f);
				mCurrentBrickUnderMouse = groupDrop.BrickThatHoldsActiveConnection;
				if (mCurrentBrickUnderMouse != null)
				{
					Brick.ConnectionPoint activeConnectionPoint = mCurrentBrickUnderMouse.ActiveConnectionPoint;
					if (activeConnectionPoint != null)
						mMouseGrabDeltaToActiveConnectionPoint = new PointF(groupDrop.Center.X - activeConnectionPoint.mPositionInStudWorldCoord.X,
																			groupDrop.Center.Y - activeConnectionPoint.mPositionInStudWorldCoord.Y);
				}

				// update the brick connectivity for the group after having selected them
				updateFullBrickConnectivityForSelectedBricksOnly();
			}
			else
			{
				// the part to drop is a brick
				Brick brickDrop = itemDrop as Brick;
				mBricks.Add(brickDrop);
				mSelectedObjects.Add(brickDrop);
				mCurrentBrickUnderMouse = brickDrop;
				// check if the brick as connection point to set the garb distance to connection point
				Brick.ConnectionPoint activeConnectionPoint = brickDrop.ActiveConnectionPoint;
				if (activeConnectionPoint != null)
					mMouseGrabDeltaToActiveConnectionPoint = new PointF(brickDrop.Center.X - activeConnectionPoint.mPositionInStudWorldCoord.X,
																		brickDrop.Center.Y - activeConnectionPoint.mPositionInStudWorldCoord.Y);
				else
					mMouseGrabDeltaToActiveConnectionPoint = new PointF(0.0f, 0.0f);
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
			mCurrentBrickUnderMouse = null;
			mMouseGrabDeltaToCenter = new PointF(0.0f, 0.0f);
			mMouseGrabDeltaToActiveConnectionPoint = new PointF(0.0f, 0.0f);
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
				List<Layer.LayerItem> partsInTheGroup = (itemDrop as Layer.Group).getAllChildrenItems();
				foreach (Layer.LayerItem item in partsInTheGroup)
					mBricks.Remove(item as Brick);
			}
		}
		#endregion
	}
}
