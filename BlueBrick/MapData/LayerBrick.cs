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
				public BrickLibrary.Brick.ConnectionType mType = BrickLibrary.Brick.ConnectionType.BRICK;

				/// <summary>
				/// This default constructor is for the serialization and should not be used in the program
				/// </summary>
				public ConnectionPoint()
				{
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

				public BrickLibrary.Brick.ConnectionType Type
				{
					get { return mType; }
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
									mMyBrick.setActiveConnectionPointWithNextOne();
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
			private float mOrientation = 0;	// in degree
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

			#region get/set
			/// <summary>
			/// the part number of the brick
			/// The set of the part number is used by the serializer, but should not be used in the program
			/// instead consider to delete your brick and create a new one with the appropriate constructor.
			/// </summary>
			public string PartNumber
			{
				get { return mPartNumber; }
				set
				{
					mPartNumber = value;
					updateImage();
				}
			}

			/// <summary>
			/// The current rotation of the image
			/// </summary>
			public float Orientation
			{
				get { return mOrientation; }
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
			public new PointF Position
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
			public new PointF Center
			{
				set
				{
					mDisplayArea.X = value.X - (mDisplayArea.Width / 2);
					mDisplayArea.Y = value.Y - (mDisplayArea.Height / 2);
					updateConnectionPosition();				
				}
				get
				{
					return new PointF(mDisplayArea.X + (mDisplayArea.Width / 2), mDisplayArea.Y + (mDisplayArea.Height / 2));
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
							if (connexion.Type != BrickLibrary.Brick.ConnectionType.BRICK)
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
							setActiveConnectionPointWithNextOne();
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
			/// Return the list of free point for this brick
			/// </summary>
			public List<PointF> FreeConnectionPoints
			{
				get
				{
					if (mConnectionPoints != null)
					{
						List<PointF> result = new List<PointF>(mConnectionPoints.Count);
						foreach (ConnectionPoint connexion in mConnectionPoints)
							if (connexion.IsFree)
								result.Add(connexion.mPositionInStudWorldCoord);
						return result;
					}
					return null;
				}
			}

			/// <summary>
			/// Return the list of free connexion for this brick
			/// </summary>
			public List<ConnectionPoint> FreeConnections
			{
				get
				{
					if (mConnectionPoints != null)
					{
						List<ConnectionPoint> result = new List<ConnectionPoint>(mConnectionPoints.Count);
						foreach (ConnectionPoint connexion in mConnectionPoints)
							if (connexion.IsFree)
								result.Add(connexion);
						return result;
					}
					return null;
				}
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
				mPartNumber = BrickLibrary.Instance.getActualPartNumber(reader.ReadElementContentAsString().ToUpper());
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
								connexion.mType = BrickLibrary.Instance.getConnexionType(this.PartNumber, connexionIndex);
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
			private PointF getMinMaxAndSize(PointF[] points, ref PointF min, ref PointF max)
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
				PointF boundingSize = getMinMaxAndSize(boundingPoints, ref boundingMin, ref boundingMax);

				// check if this picture has a specific hull
				if (hull != boundingBox)
				{
					// there's a more precise hull, so transform it and get its size
					PointF[] hullPoints = hull.ToArray();
					rotation.TransformVectors(hullPoints);

					PointF hullMin = new PointF();
					PointF hullMax = new PointF();
					PointF hullSize = getMinMaxAndSize(hullPoints, ref hullMin, ref hullMax);

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

			public void setActiveConnectionPointUnder(PointF positionInStudCoord)
			{
				if (mConnectionPoints != null)
				{
					float bestSquareDistance = float.MaxValue;
					for (int i = 0 ; i < mConnectionPoints.Count ; ++i)
						if (mConnectionPoints[i].IsFree)
						{
							PointF point = mConnectionPoints[i].mPositionInStudWorldCoord;
							float dx = positionInStudCoord.X - point.X;
							float dy = positionInStudCoord.Y - point.Y;
							float squareDistance = (dx * dx) + (dy * dy);
							if (squareDistance < bestSquareDistance)
							{
								bestSquareDistance = squareDistance;
								mActiveConnectionPointIndex = i;
							}
						}
				}
			}

			public void setActiveConnectionPointWithNextOne()
			{
				if (mConnectionPoints != null)
				{
					// memorize the current index to know when if we are looping
					int previousActiveConnectionIndex = mActiveConnectionPointIndex;
					do
					{
						// go to the next one (and loop if reaching the end of the list)
						mActiveConnectionPointIndex++;
						if (mActiveConnectionPointIndex >= mConnectionPoints.Count)
							mActiveConnectionPointIndex = 0;
						// until we find a free point or we tested them all
					} while (!mConnectionPoints[mActiveConnectionPointIndex].IsFree && (mActiveConnectionPointIndex != previousActiveConnectionIndex));
				}
			}

			#endregion
			#region get image
			/// <summary>
			/// return the current image of the brick depending on it's mipmap level (LOD level)
			/// level 0 is the most detailed level.
			/// </summary>
			/// <param name="mipmapLevel">the level of detail</param>
			/// <returns>the image of the brick</returns>
			public Image getImage(int mipmapLevel)
			{				
				// create the image dynamically if not already created
				if (mipmapLevel < Properties.Settings.Default.StartSavedMipmapLevel)
					return createImage(mipmapLevel);
				else if (mipmapLevel >= mMipmapImages.Length)
					mipmapLevel = mMipmapImages.Length - 1;
				// else return the saved image
				if (mMipmapImages[mipmapLevel] == null)
					mMipmapImages[mipmapLevel] = createImage(mipmapLevel);
				return mMipmapImages[mipmapLevel];
			}
			#endregion
		}

		[NonSerialized]
		private static SolidBrush[] sConnexionPointBrush = { new SolidBrush(Color.Red), new SolidBrush(Color.Yellow), new SolidBrush(Color.Cyan), new SolidBrush(Color.BlueViolet), new SolidBrush(Color.LightPink), new SolidBrush(Color.GreenYellow) };
		private static float[] sConnexionPointRadius = { 2.5f, 1.0f, 1.5f, 1.0f, 1.0f, 1.0f };
		private static ImageAttributes sImageAttributeForSelection = new ImageAttributes();
		private static ImageAttributes sImageAttributeForSnapping = new ImageAttributes();

		// list of bricks and connection points
		private List<Brick> mBricks = new List<Brick>(); // all the bricks in the layer
		private List<Brick.ConnectionPoint>[] mFreeConnectionPoints = new List<Brick.ConnectionPoint>[(int)BrickLibrary.Brick.ConnectionType.COUNT];

		//related to selection
		private Brick mCurrentBrickUnderMouse = null;
		private PointF mMouseDownInitialPosition;
		private PointF mMouseDownLastPosition;
		private PointF mMouseGrabDeltaToCenter; // The delta between the grab point of the mouse inside the grabed brick, to the center of that brick
		private bool mMouseIsBetweenDownAndUpEvent = false;
		private bool mMouseHasMoved = false;
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
		#endregion

		#region constructor
		public LayerBrick()
		{
			// create all the free connection list for all the different type of connection
			for (int i = 0; i < (int)BrickLibrary.Brick.ConnectionType.COUNT; ++i)
				mFreeConnectionPoints[i] = new List<Brick.ConnectionPoint>();
		}

		public override int getNbItems()
		{
			return mBricks.Count;
		}

		public static void sUpdateGammaFromSettings()
		{
			sImageAttributeForSelection.SetGamma(Properties.Settings.Default.GammaForSelection);
			sImageAttributeForSnapping.SetGamma(Properties.Settings.Default.GammaForSnappingPart);
		}
		#endregion
		#region IXmlSerializable Members

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
			
			// clear again the hash table to free the memory after loading
			Brick.ConnectionPoint.sHashtableForLinkRebuilding.Clear();

			// reconstruct the freeConnexion points list by iterating on all the connexion of all the bricks
			foreach (List<Brick.ConnectionPoint> connexionList in mFreeConnectionPoints)
				connexionList.Clear();
			foreach (Brick brick in mBricks)
				if (brick.ConnectionPoints != null) // do not use brick.HasConnectionPoints here
					foreach (Brick.ConnectionPoint connexion in brick.ConnectionPoints)
					{
						// 1)
						// check if we need to break a link because it is not valid 
						// this situation can happen when you load an unknow part that had
						// some connexion point before in the XML file
						// 2)
						// Also happen when a part description changed in the part lib, so all
						// the connection of type BRICK are the trailing connection in the file
						// that does not exist anymore in the part library. So if any of the
						// two connection is of type BRICK, we can break the connection.
						// 3)
						// check if we need to break the link because two connexion point are not anymore at
						// the same place. This can happen if the file was save with a first version of the
						// library, and then we change the library and we change the connexion position.
						// So the parts are not move, but the links should be broken
						if ( (connexion.mType == BrickLibrary.Brick.ConnectionType.BRICK) ||
								((connexion.ConnectionLink != null) &&
									((connexion.ConnectionLink.ConnectionLink.mType == BrickLibrary.Brick.ConnectionType.BRICK) ||
									 !arePositionsEqual(connexion.mPositionInStudWorldCoord, connexion.ConnectionLink.mPositionInStudWorldCoord))) )
						{
							// we don't use the disconnect method here, because the disconnect method
							// add the two connexion in the free connexion list, but we want to do it after.
							if (connexion.ConnectionLink != null)
								connexion.ConnectionLink.ConnectionLink = null;
							connexion.ConnectionLink = null;
						}
						// add the connexion in the free list if it is free
						if (connexion.IsFree)
							mFreeConnectionPoints[(int)connexion.Type].Add(connexion);
					}
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
		}

		#endregion

		#region action on the layer
		/// <summary>
		///	Add the specified brick at the specified position in the list
		/// </summary>
		public void addBrick(Brick brickToAdd, int index)
		{
			// add its connection points to the free list
			if (brickToAdd.HasConnectionPoint)
				foreach (Brick.ConnectionPoint connexion in brickToAdd.ConnectionPoints)
					mFreeConnectionPoints[(int)connexion.Type].Add(connexion);

			// add the brick in the list
			if (index < 0)
				mBricks.Add(brickToAdd);
			else
				mBricks.Insert(index, brickToAdd);

			// notify the part list view
			MainForm.Instance.NotifyPartListForBrickAdded(this, brickToAdd);
		}

		/// <summary>
		///	Add the specified brick by connecting it to the current selected brick
		/// or don't change its position if there's no connection possible
		/// </summary>
		public void addConnectBrick(Brick brickToAdd)
		{
			// add its connection points to the free list
			if (brickToAdd.HasConnectionPoint)
				foreach (Brick.ConnectionPoint connexion in brickToAdd.ConnectionPoints)
					mFreeConnectionPoints[(int)connexion.Type].Add(connexion);
			// now make the connection
			if (mSelectedObjects.Count == 1)
			{
				Brick selectedBrick = mSelectedObjects[0] as Brick;
				if (selectedBrick.HasConnectionPoint && brickToAdd.HasConnectionPoint)
				{
					// first rotate this brick
					float newOrientation = selectedBrick.Orientation + selectedBrick.ActiveConnectionAngle + 180 - brickToAdd.ActiveConnectionAngle;
					// clamp the orientation between 0 and 360
					if (newOrientation >= 360.0f)
						newOrientation -= 360.0f;
					if (newOrientation < 0.0f)
						newOrientation += 360.0f;
					// set the new orientation
					brickToAdd.Orientation = newOrientation;
					// the place the brick to add at the correct position
					brickToAdd.ActiveConnectionPosition = selectedBrick.ActiveConnectionPosition;

					// get the prefered index now, because the connection of the brick will move automatically the the active connection
					int nextPreferedActiveConnectionIndex = BrickLibrary.Instance.getConnectionNextPreferedIndex(brickToAdd.PartNumber, brickToAdd.ActiveConnectionPointIndex);

					// set the link of the connection (and check all the other connexion  of the brick because maybe the add lock different connection at the same time)
					if (brickToAdd.ActiveConnectionPoint.Type == selectedBrick.ActiveConnectionPoint.Type)
						connectTwoConnectionPoints(brickToAdd.ActiveConnectionPoint, selectedBrick.ActiveConnectionPoint);
					foreach (Brick.ConnectionPoint brickConnexion in brickToAdd.ConnectionPoints)
						if (brickConnexion != brickToAdd.ActiveConnectionPoint)
							for (int i = 0; i < mFreeConnectionPoints[(int)brickConnexion.Type].Count; ++i)
							{
								Brick.ConnectionPoint freeConnexion = mFreeConnectionPoints[(int)brickConnexion.Type][i];
								if ((freeConnexion != brickConnexion) && (freeConnexion.Type == brickConnexion.Type) && 
									arePositionsEqual(brickConnexion.mPositionInStudWorldCoord, freeConnexion.mPositionInStudWorldCoord))
								{
									if (connectTwoConnectionPoints(brickConnexion, freeConnexion))
										--i;
								}
							}
					
					// set the current connection point to the next one
					brickToAdd.ActiveConnectionPointIndex = nextPreferedActiveConnectionIndex;
					// and add the brick in the list
					mBricks.Insert(mBricks.IndexOf(selectedBrick) + 1, brickToAdd);
				}
				else
				{
					PointF position = selectedBrick.Position;
					position.X += selectedBrick.mDisplayArea.Width;
					brickToAdd.Position = position;
					mBricks.Insert(mBricks.IndexOf(selectedBrick) + 1, brickToAdd);
				}
			}
			else
			{
				mBricks.Add(brickToAdd);
			}

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
						if (connexion.ConnectionLink != null)
						{
							mFreeConnectionPoints[(int)(connexion.ConnectionLink.Type)].Add(connexion.ConnectionLink);
							connexion.ConnectionLink.ConnectionLink = null;
						}
						mFreeConnectionPoints[(int)(connexion.Type)].Remove(connexion);
						connexion.ConnectionLink = null;
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
		/// Copy the list of the selected bricks in a separate list for later use.
		/// This method should be called on a CTRL+C
		/// </summary>
		public void copyCurrentSelection()
		{
			// reset the copy list
			sCopyItems.Clear();
			// recreate the copy list if the selection is not empty
			foreach (LayerItem item in SelectedObjects)
			{
				// add a duplicated item in the list (because the model may change between this copy and the paste)
				sCopyItems.Add((item as Brick).Clone());
			}
		}

		/// <summary>
		/// Paste (duplicate) the list of bricks that was previously copied with a call to copyCurrentSelection()
		/// This method should be called on a CTRL+V
		/// </summary>
		public void pasteCopiedList(float positionShift)
		{
			// To paste, we need to have copied something
			if (sCopyItems.Count > 0)
			{
				mLastDuplicateBrickAction = new DuplicateBrick(this, sCopyItems, positionShift, positionShift);
				ActionManager.Instance.doAction(mLastDuplicateBrickAction);
			}
		}

		/// <summary>
		/// Select all the items in this layer.
		/// </summary>
		public override void selectAll()
		{
			// convert the list of brick into list of item (do you know a better than
			// just duplicate the list ?????
			List<LayerItem> brickListAsItem = new List<LayerItem>(mBricks.Count);
			foreach (Brick brick in mBricks)
				brickListAsItem.Add(brick);
			// clear the selection and add all the item of this layer
			clearSelection();
			addObjectInSelection(brickListAsItem);
		}

		/// <summary>
		/// Connect the two connexion if possible (i.e. if both connexion are free)
		/// </summary>
		/// <param name="connexion1">the first connexion to connect with the second one</param>
		/// <param name="connexion2">the second connexion to connect with the first one</param>
		/// <returns>true if the connexion was made, else false.</returns>
		private bool connectTwoConnectionPoints(Brick.ConnectionPoint connexion1, Brick.ConnectionPoint connexion2)
		{
			// the connexion can never be stolen
			if (connexion1.IsFree && connexion2.IsFree)
			{
				connexion1.ConnectionLink = connexion2;
				connexion2.ConnectionLink = connexion1;
				mFreeConnectionPoints[(int)(connexion1.Type)].Remove(connexion1);
				mFreeConnectionPoints[(int)(connexion2.Type)].Remove(connexion2);
				return true;
			}
			return false;
		}

		private void disconnectTwoConnectionPoints(Brick.ConnectionPoint connexion1, Brick.ConnectionPoint connexion2)
		{
			if (connexion1 != null)
			{
				connexion1.ConnectionLink = null;
				mFreeConnectionPoints[(int)(connexion1.Type)].Add(connexion1);
			}
			if (connexion2 != null)
			{
				connexion2.ConnectionLink = null;
				mFreeConnectionPoints[(int)(connexion2.Type)].Add(connexion2);
			}
		}

		private bool arePositionsEqual(PointF pos1, PointF pos2)
		{
			if (Math.Abs(pos1.X - pos2.X) < 0.1)
				return (Math.Abs(pos1.Y - pos2.Y) < 0.1);
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
				List<Brick.ConnectionPoint>[] connexionPointsInSelection = new List<Brick.ConnectionPoint>[(int)(BrickLibrary.Brick.ConnectionType.COUNT)];
				List<Brick.ConnectionPoint>[] freeConnexionPoints = new List<Brick.ConnectionPoint>[(int)(BrickLibrary.Brick.ConnectionType.COUNT)];

				for (int i = 0; i < (int)(BrickLibrary.Brick.ConnectionType.COUNT); ++i)
				{
					connexionPointsInSelection[i] = new List<Brick.ConnectionPoint>();
					freeConnexionPoints[i] = new List<Brick.ConnectionPoint>();
					foreach (Brick.ConnectionPoint connexion in mFreeConnectionPoints[i])
						if (mSelectedObjects.Contains(connexion.mMyBrick))
							connexionPointsInSelection[i].Add(connexion);
						else
							freeConnexionPoints[i].Add(connexion);
				}

				// now iterate on the free connexion point in selection to search where to connect
				for (int i = 0; i < (int)(BrickLibrary.Brick.ConnectionType.COUNT); ++i)
					foreach (Brick.ConnectionPoint selConnexion in connexionPointsInSelection[i])
					{
						// try to find a new connection
						foreach (Brick.ConnectionPoint freeConnexion in freeConnexionPoints[i])
							if (arePositionsEqual(selConnexion.mPositionInStudWorldCoord, freeConnexion.mPositionInStudWorldCoord))
								connectTwoConnectionPoints(selConnexion, freeConnexion);
					}
			}
		}

		/// <summary>
		/// Update the connectivity of all the bricks base of their positions
		/// This method is slow since the whole connectivity is recompute. It should only be call after
		/// an import of a map from a file format that doesn't contain the connectivity info, such as LDraw format
		/// </summary>
		public void updateFullBrickConnectivity()
		{
			foreach (Brick brick in mBricks)
				if (brick.HasConnectionPoint)
					foreach (Brick.ConnectionPoint brickConnexion in brick.ConnectionPoints)
						for (int i = 0; i < mFreeConnectionPoints[(int)brickConnexion.Type].Count; ++i)
						{
							Brick.ConnectionPoint freeConnexion = mFreeConnectionPoints[(int)brickConnexion.Type][i];
							if ((freeConnexion.mMyBrick != brick) && arePositionsEqual(freeConnexion.mPositionInStudWorldCoord, brickConnexion.mPositionInStudWorldCoord))
							{
								if (connectTwoConnectionPoints(freeConnexion, brickConnexion))
									i--;
							}
						}
		}

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

		/// <summary>
		/// recompute all the pictures of all the brick of all the brick layers 
		/// </summary>
		public void recomputeBrickMipmapImages()
		{
			foreach (Brick brick in mBricks)
				brick.clearMipmapImages(0, 1);
		}

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
				RectangleF brickArea = brick.mDisplayArea;
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
					// the -0.5 and +1 is a hack to add 1 more pixel to have jointive baseplates
					destinationRectangle.X = (int)(((left - areaInStud.Left) * scalePixelPerStud) - 0.5f);
					destinationRectangle.Y = (int)(((top - areaInStud.Top) * scalePixelPerStud) - 0.5f);
					destinationRectangle.Width = (int)((brick.Width * scalePixelPerStud) + 1.0f);
					destinationRectangle.Height = (int)((brick.Height * scalePixelPerStud) + 1.0f);
					Image image = brick.getImage(mipmapLevel);

					// draw the current brick eventually highlighted
					if (brick == mCurrentBrickUnderMouse)
						g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, sImageAttributeForSnapping);
					else if (mSelectedObjects.Contains(brick))
						g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, sImageAttributeForSelection);
					else
						g.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
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
			else if (mSelectedObjects.Count == 1)
			{
				Brick brick = mSelectedObjects[0] as Brick;
				if (brick.HasConnectionPoint && brick.ActiveConnectionPoint.IsFree)
					brickThatHasActiveConnection = brick;
			}
			// now if the brick is valid, draw the red dot
			if (brickThatHasActiveConnection != null)
			{
				float x = (float)((brickThatHasActiveConnection.ActiveConnectionPosition.X - sConnexionPointRadius[0] - areaInStud.Left) * scalePixelPerStud);
				float y = (float)((brickThatHasActiveConnection.ActiveConnectionPosition.Y - sConnexionPointRadius[0] - areaInStud.Top) * scalePixelPerStud);
				float size = (float)(sConnexionPointRadius[0] * 2 * scalePixelPerStud);
				g.FillEllipse(sConnexionPointBrush[0], x, y, size, size);
			}

			// draw the free connexion points if needed
			if (BlueBrick.Properties.Settings.Default.DisplayFreeConnexionPoints)
				for (int i = 1; i < (int)(BrickLibrary.Brick.ConnectionType.COUNT); ++i)
					foreach (Brick.ConnectionPoint connexion in mFreeConnectionPoints[i])
					{
						float sizeInStud = sConnexionPointRadius[i];
						float x = (float)((connexion.mPositionInStudWorldCoord.X - sizeInStud - areaInStud.Left) * scalePixelPerStud);
						float y = (float)((connexion.mPositionInStudWorldCoord.Y - sizeInStud - areaInStud.Top) * scalePixelPerStud);
						float sizeInPixel = (float)(sizeInStud * 2 * scalePixelPerStud);
						g.FillEllipse(sConnexionPointBrush[i], x, y, sizeInPixel, sizeInPixel);
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

			// select the appropriate cursor:
			if (mMouseMoveIsADuplicate)
				preferedCursor = MainForm.Instance.BrickDuplicateCursor;
			else if (willMoveSelectedObject)
				preferedCursor = Cursors.SizeAll;
			else if (mCurrentBrickUnderMouse == null)
				preferedCursor = Cursors.Cross;

			// handle the mouse down if we duplicate or move the selected bricks
			bool willHandleTheMouse = (mMouseMoveIsADuplicate || willMoveSelectedObject);
			// reset the brick pointer under the mouse if finally we don't care.
			if (!willHandleTheMouse)
				mCurrentBrickUnderMouse = null;

			// compute the grab point if we grab a brick
			if (mCurrentBrickUnderMouse != null)
				mMouseGrabDeltaToCenter = new PointF(mouseCoordInStud.X - mCurrentBrickUnderMouse.Center.X, mouseCoordInStud.Y - mCurrentBrickUnderMouse.Center.Y);

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

				// update the active connexion point
				mCurrentBrickUnderMouse.setActiveConnectionPointUnder(mouseCoordInStud);
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
				PointF mouseCoordInStudSnapped = getMovedSnapPoint(mouseCoordInStud);
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
							this.pasteCopiedList(0.0f);
							// set the flag
							wereBrickJustDuplicated = true;
						}
					}
					// this is move of the selection, not a duplicate selection
					// move all the selected brick if the delta move is not null
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
						if (mCurrentBrickUnderMouse != null)
							mMouseGrabDeltaToCenter = new PointF(mouseCoordInStud.X - mCurrentBrickUnderMouse.Center.X, mouseCoordInStud.Y - mCurrentBrickUnderMouse.Center.Y);
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
						mMouseMoveIsADuplicate = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);
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
			// check if we moved the selected text
			if (mMouseHasMoved && (mSelectedObjects.Count > 0))
			{
				// reset the flag
				mMouseHasMoved = false;

				// compute the delta mouve of the mouse
				mouseCoordInStud = getMovedSnapPoint(mouseCoordInStud);
				PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownInitialPosition.X, mouseCoordInStud.Y - mMouseDownInitialPosition.Y);

				// create a new action for this move
				if ((deltaMove.X != 0) || (deltaMove.Y != 0))
				{
					// update the duplicate action or add a move action
					if (mMouseMoveIsADuplicate)
					{
						mLastDuplicateBrickAction.updatePositionShift(deltaMove.X, deltaMove.Y);
						mLastDuplicateBrickAction = null;
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
					// update the free connexion list if the use move the brick and then go back
					// to the original place (deltaMove is null), so the link was broken because
					// of the move, so we need to recreate the link
					updateBrickConnectivityOfSelection(false);
					// reset anyway the temp reference for the duplication
					mLastDuplicateBrickAction = null;
				}
			}
			else
			{
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
				if ((selectionRectangeInStud.Right > brick.mDisplayArea.Left) && (selectionRectangeInStud.Left < brick.mDisplayArea.Right) &&
					(selectionRectangeInStud.Bottom > brick.mDisplayArea.Top) && (selectionRectangeInStud.Top < brick.mDisplayArea.Bottom))
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
		/// <param name="pointInStud">the point to snap</param>
		/// <returns>a near snap point</returns>
		private PointF getMovedSnapPoint(PointF pointInStud)
		{
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
							// snap the selected brick on a free connexion points (of other bricks)
							// iterate on all the free connexion point to know if there's a nearest point						
							float nearestSquareDistance = float.MaxValue;
							Brick.ConnectionPoint bestFreeConnection = null;
							foreach (Brick.ConnectionPoint freeConnexion in mFreeConnectionPoints[(int)activeBrickConnexion.Type])
								if (freeConnexion.mMyBrick != mCurrentBrickUnderMouse)
								{
									float dx = freeConnexion.mPositionInStudWorldCoord.X - pointInStud.X;
									float dy = freeConnexion.mPositionInStudWorldCoord.Y - pointInStud.Y;
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

							// check if the nearest free connexion if close enough to snap
							if (nearestSquareDistance < 64.0f) // snapping of 8 studs
							{
								// rotate the selection
								mSnappingOrientation = bestFreeConnection.mMyBrick.Orientation - mCurrentBrickUnderMouse.Orientation;
								mSnappingOrientation += bestFreeConnection.Angle + 180 - activeBrickConnexion.Angle;
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
								PointF snapPosition = bestFreeConnection.mPositionInStudWorldCoord;
								snapPosition.X += mCurrentBrickUnderMouse.Center.X - activeBrickConnexion.mPositionInStudWorldCoord.X;
								snapPosition.Y += mCurrentBrickUnderMouse.Center.Y - activeBrickConnexion.mPositionInStudWorldCoord.Y;

								// return the position
								return snapPosition;
							}
						}
					}
					// This is the normal case for snapping the brick under the mouse.
					// Snap the position of the mouse on the grid (the snapping is a Floor style one)
					// then add the center shift of the part and the snapping offset
					pointInStud = Layer.snapToGrid(pointInStud);
					// compute the center shift
					PointF halfBrickSize = new PointF(mCurrentBrickUnderMouse.mDisplayArea.Width / 2, mCurrentBrickUnderMouse.mDisplayArea.Height / 2);
					PointF centerShift = Layer.snapToGrid(mMouseGrabDeltaToCenter);
					// there is a special case depending on the relationship between the snap size and the brick size
					if (halfBrickSize.X < CurrentSnapGridSize)
						centerShift.X = -halfBrickSize.X;
					if (halfBrickSize.Y < CurrentSnapGridSize)
						centerShift.Y = -halfBrickSize.Y;
					// shift the point according to the center and the grid offset
					pointInStud.X += -centerShift.X - mCurrentBrickUnderMouse.SnapToGridOffset.X;
					pointInStud.Y += -centerShift.Y - mCurrentBrickUnderMouse.SnapToGridOffset.Y;
					return pointInStud;
				}

				// the snapping is enable but the group of brick was grab from an empty place
				// i.e. there's no bricks under the mouse so just do a normal snapping on the grid
				return Layer.snapToGrid(pointInStud);
			}

			// by default do not change anything
			return pointInStud;
		}

		#endregion
	}
}
