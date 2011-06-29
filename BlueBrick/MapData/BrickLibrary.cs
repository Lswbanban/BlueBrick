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
using System.Windows.Forms;
using Microsoft.Win32;
using System.Drawing.Drawing2D;

namespace BlueBrick.MapData
{
	public class BrickLibrary
	{
		public class ConnectionType
		{
			public const int DEFAULT = 0;

			// there's a specific connection type for rendering the selected connections
			public static ConnectionType sSelectedConnection = new ConnectionType(string.Empty, Color.Red, 2.5f);

			// the data used to render the connection
			private string mName = string.Empty;
			private Color mColor = Color.Empty;
			private float mSize = 1.0f;

			public Color Color
			{
				get { return mColor; }
			}

			public float Size
			{
				get { return mSize; }
			}

			public ConnectionType(string name, Color color, float size)
			{
				mName = name;
				mColor = color;
				mSize = size;
			}
		};

		public class Brick
		{
			public class Margin
			{
				public float mLeft = 0.0f;
				public float mRight = 0.0f;
				public float mTop = 0.0f;
				public float mBottom = 0.0f;
			}

			public class ConnectionPoint
			{
				public int mType = 0;
				public PointF mPosition = new PointF(0.0f, 0.0f); // the connection point in pixel, relative to the center of the brick
				public float mAngle = 0.0f; // the angle that we should add to connect to this connection point
				public float mAngleToPrev = 180.0f;
				public float mAngleToNext = 180.0f;
				public int mNextPreferedIndex = 0;
				public int mElectricPlug = 0;
			}

			public class ElectricCircuit
			{
				public int mIndex1 = 0; // index of the first connection that makes the circuit
				public int mIndex2 = 1; // index of the second connection that makes the circuit
				public float mDistance = 16.0f; // distance in stud between the two connection
				public ElectricCircuit(int index1, int index2, float distance)
				{
					mIndex1 = index1;
					mIndex2 = index2;
					mDistance = distance;
				}
			}

			public class TDRemapData
			{
				public class ConnexionData
				{
					public int mBBConnexionPointIndex = 0;
					public int mType = 20; // 20 = Custom
					public float mDiffAngleBtwTDandBB = 0.0f;
				}

				public class TDId
				{
					public static string sCurrentRegistryUsed = "default";
					private int mDefaultId = 0;
					private Dictionary<string, int> mOtherIds = null;

					public int ID
					{
						get
						{
							// first try to get the id for the registry set on this machine
							int id = this.IDForCurrentRegistry;
							if (id != 0)
								return id;
							// else return the default ID
							return mDefaultId;
						}
					}

					public int IDForCurrentRegistry
					{
						get
						{
							// first try to get the id for the registry set on this machine
							if (mOtherIds != null)
							{
								int id = 0;								
								if (mOtherIds.TryGetValue(sCurrentRegistryUsed, out id))
									return id;
							}
							// else return 0
							return 0;
						}
					}

					public int DefaultID
					{
						get	{ return mDefaultId; }
					}

					public void addId(string registry, int id)
					{
						// check if it is the default id
						if (registry.Equals("default"))
						{
							mDefaultId = id;
						}
						else
						{
							// check if we need to create the dictionnary
							if (mOtherIds == null)
								mOtherIds = new Dictionary<string, int>();
							// add the new id with its key
							mOtherIds.Add(registry, id);
							// also if the default id is not set yet, use this one as a default id
							if (mDefaultId == 0)
								mDefaultId = id;
						}
					}

					private void addIdToAssociationDictionnary(Dictionary<int, List<Brick>> TDPartNumberAssociation, int id, Brick brick)
					{
						// try to get the list that maybe already exist
						List<Brick> brickList = null;
						TDPartNumberAssociation.TryGetValue(id, out brickList);
						// if the list doesn't exist create it and add it to the dictionary
						if (brickList == null)
						{
							brickList = new List<Brick>(1); // most of the time there's only one brick associated with a TD id
							TDPartNumberAssociation.Add(id, brickList);
						}
						// add the brick to the list
						if (!brickList.Contains(brick))
							brickList.Add(brick);
					}

					public void addToAssociationDictionnary(Dictionary<int, List<Brick>> TDPartNumberAssociation, Brick brick)
					{
						// add the default id
						addIdToAssociationDictionnary(TDPartNumberAssociation, mDefaultId, brick);
						// and add the other id if needed
						if (mOtherIds != null)
							foreach(int id in mOtherIds.Values)
								addIdToAssociationDictionnary(TDPartNumberAssociation, id, brick);
					}
				}

				public TDId mTDId = new TDId();
				public int mFlags = 0;
				public bool mHasSeveralPort = false;
				public List<ConnexionData> mConnexionData = null;
			}

			public class LDrawRemapData
			{
				public float mAngle = 0.0f;
				public PointF mTranslation = new PointF();
				public float mPreferredHeight = 0.0f;
				public string mSleeperBrickNumber = null;
				public string mSleeperBrickColor = null;
				public string mAliasPartNumber = null;
				public string mAliasPartColor = null;
			}

            public class SubPart
            {
                // we store the number and the brick because not all the brick may have been parsed when the group is parsed
                public string mSubPartNumber;
                public Brick mSubPartBrick = null;
                public PointF mPosition = new PointF();
                public float mAngle = 0.0f;
                public Matrix mTransform = null;
            }

            public enum BrickType
            {
                BRICK,
                SEALED_GROUP,
                OPEN_GROUP,
                IGNORABLE,
            }

            public BrickType        mBrickType = BrickType.BRICK;
			public string			mImageURL = null; // the URL on internet of the image for part list export in HTML
			public string			mDescription = BlueBrick.Properties.Resources.TextUnknown; // the description of the part in the current language of the application
			public string			mPartNumber = null; // the part number (same as the key in the dictionnary), usefull in case of part renaming
			public Image			mImage = null;	// the image of the brick just as it is loaded from the hardrive. If null, the brick is ignored by BlueBrick.
			public Margin			mSnapMargin = new Margin(); // the the inside margin that should be use for snapping the part on the grid
			public List<ConnectionPoint> mConnectionPoints = null; // all the information for each connection
			public List<ElectricCircuit> mElectricCircuitList = null; // the list of all the electric circuit for this brick (if any)
			public List<PointF>		mConnectionPositionList = null; // for optimization reason, the positions of the connections are also saved into a list
			public List<PointF>		mBoundingBox = new List<PointF>(5); // list of the 4 corner in pixel
			public List<PointF>		mHull = new List<PointF>(4); // list of all the points in pixel that describe the hull of the part
            public List<SubPart>    mGroupSubPartList = null; // if this brick is a group, list of all the parts belonging to this group
			public TDRemapData		mTDRemapData = null;
			public LDrawRemapData	mLDrawRemapData = null;

			#region get/set
			/// <summary>
			/// An ignorable brick is a brick not visible in the part library for the user, and also
			/// not displayed on the layout as an unknown part. This is basically a part that is invisible to
			/// the user and that don't create error on the layout.
			/// </summary>
			public bool ShouldBeIgnored
			{
                get { return (mBrickType == BrickType.IGNORABLE); }
			}
			#endregion

			public Brick(string partNumber, Image image, string xmlFileName, Dictionary<string, int> connectionRemapingDictionary)
			{
                // a flag to tell if this part is a group (we'll know it by reading the XML file)
                bool isGroupPart = false;

				// assign the part num
				mPartNumber = partNumber;

				// parse the xml data if the xml file exists
				if (System.IO.File.Exists(xmlFileName))
				{
					// create an XML reader to parse the data
					System.Xml.XmlParserContext xmlContext = new System.Xml.XmlParserContext(null, null, "", System.Xml.XmlSpace.Default, System.Text.Encoding.UTF7);
					System.Xml.XmlReaderSettings xmlSettings = new System.Xml.XmlReaderSettings();
					xmlSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
					xmlSettings.IgnoreWhitespace = true;
					xmlSettings.IgnoreComments = true;
					xmlSettings.CheckCharacters = false;
					xmlSettings.CloseInput = true;
					System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(xmlFileName, xmlSettings, xmlContext);

					// first find and enter the unique root tag
                    bool rootNodeFound = false;
                    do
                    {
                        xmlReader.Read();
                        isGroupPart = xmlReader.Name.Equals("group");
                        rootNodeFound = isGroupPart || xmlReader.Name.Equals("part");
                    } while (!rootNodeFound && !xmlReader.EOF);

                    // set the brick type if it is a group
                    if (isGroupPart)
                        mBrickType = Brick.BrickType.OPEN_GROUP;

                    // if we found the root node, start to parse it
					if (rootNodeFound)
					{
						// read the first child node
						xmlReader.Read();
						bool continueToRead = !xmlReader.EOF;
						while (continueToRead)
						{
							if (xmlReader.Name.Equals("Author"))
								readAuthorTag(ref xmlReader);
							else if (xmlReader.Name.Equals("Description"))
								readDescriptionTag(ref xmlReader);
							else if (xmlReader.Name.Equals("ImageURL"))
								readImageURLTag(ref xmlReader);
							else if (xmlReader.Name.Equals("SnapMargin"))
								readSnapMarginTag(ref xmlReader);
							else if (xmlReader.Name.Equals("ConnexionList"))
								readConnexionListTag(ref xmlReader, connectionRemapingDictionary);
							else if (xmlReader.Name.Equals("hull"))
								readHullTag(ref xmlReader);
							else if (xmlReader.Name.Equals("TrackDesigner"))
								readTrackDesignerTag(ref xmlReader);
							else if (xmlReader.Name.Equals("LDraw"))
								readLDRAWTag(ref xmlReader);
							else if (xmlReader.Name.Equals("OldNameList"))
								readImageOldNameTag(ref xmlReader);
                            else if (xmlReader.Name.Equals("CanUngroup"))
                                readCanUngroupTag(ref xmlReader);
                            else if (xmlReader.Name.Equals("SubPartList"))
                                readSubPartListTag(ref xmlReader);
                            else
								xmlReader.Read();
							// check if we need to continue
							continueToRead = !(xmlReader.Name.Equals("part") || xmlReader.Name.Equals("group")) && !xmlReader.EOF;
						}
					}

					// close the xml file
					xmlReader.Close();
				}

                // check if the image given in parameter is not null. This can happen if we only find an xml file
                // without a GIF file, and this happen in two situation: either it's a part that should be ignored
                // or it is a group part. In either we try to create a valid image
                if (image == null)
                {
                    if (isGroupPart)
                    {
                        image = BrickLibrary.Instance.createGroupImage(this);
                        // the brick type was set during the loading
                    }
                    else
                    {
                        // if the image is null for a normal brick, this brick should be ignored by BlueBrick
                        // The ignore bricks have different meaning than the unknown bricks
                        // if the brick should be ignored, set the brick type and create a dummy small image
                        image = new Bitmap(1, 1);
                        mBrickType = Brick.BrickType.IGNORABLE;
                    }
                }

                // assign the image that can still be null for a group whose compounds parts are not loaded yet 
                mImage = image;

				// create the bounding box list (usefull for the creation of the rotated parts)
                if (image != null)
                {
                    mBoundingBox.Add(new PointF(0, 0));
                    mBoundingBox.Add(new PointF((float)(image.Width), 0));
                    mBoundingBox.Add(new PointF(0, (float)(image.Height)));
                    mBoundingBox.Add(new PointF((float)(image.Width), (float)(image.Height)));
                }
									
				// If there's no hull, we use the bounding box as a default hull
				if (mHull.Count == 0)
					mHull = mBoundingBox;
			}

			private void readAuthorTag(ref System.Xml.XmlReader xmlReader)
			{
				// nothing for now, we just ignore the author tag
				// maybe later we will display the author somewhere
				if (!xmlReader.IsEmptyElement)
				{
					/*string author = */ xmlReader.ReadElementContentAsString();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readDescriptionTag(ref System.Xml.XmlReader xmlReader)
			{
				// check if the description is not empty
				bool continueToRead = !xmlReader.IsEmptyElement;
				if (continueToRead)
				{
					// declare a variable to store the default description when we will find it
					// the default description is in english
					string defaultDescription = BlueBrick.Properties.Resources.TextUnknown;
					bool isDescriptionFound = false;

					// read the first child node
					xmlReader.Read();
					while (continueToRead)
					{
						// get the current language tag
						string language = xmlReader.Name.ToLower();

						// check if we found the language of the application,
						// else check if it is the default english language,
						// else read the next entry
						if (language.Equals(BlueBrick.Properties.Settings.Default.Language))
						{
							mDescription = xmlReader.ReadElementContentAsString();
							isDescriptionFound = true;
						}
						else if (language.Equals("en"))
							defaultDescription = xmlReader.ReadElementContentAsString();
						else
							xmlReader.Read();

						// check if we reach the end of the Description
						continueToRead = !xmlReader.Name.Equals("Description") && !xmlReader.EOF;
					}

					// check if we have found a description for the current language
					if (!isDescriptionFound)
						mDescription = defaultDescription;

					// finish the Description tag
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readImageURLTag(ref System.Xml.XmlReader xmlReader)
			{
				if (!xmlReader.IsEmptyElement)
				{
					mImageURL = xmlReader.ReadElementContentAsString();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readImageOldNameTag(ref System.Xml.XmlReader xmlReader)
			{
				// check if the old name list is not empty
				bool continueToRead = !xmlReader.IsEmptyElement;
				if (continueToRead)
				{
					// read the first child node
					xmlReader.Read();
					while (continueToRead)
					{
						if (xmlReader.Name.Equals("OldName"))
						{
							string oldName = xmlReader.ReadElementContentAsString().ToUpper();
							BrickLibrary.Instance.AddToTempRenamedPartList(oldName, this);
						}
						else
							xmlReader.Read();

						// check if we reach the end of the Description
						continueToRead = !xmlReader.Name.Equals("OldNameList") && !xmlReader.EOF;
					}

					// finish the old name list tag
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readSnapMarginTag(ref System.Xml.XmlReader xmlReader)
			{
				// check if the margin is not empty
				bool continueToRead = !xmlReader.IsEmptyElement;
				if (continueToRead)
				{
					// read the first child node
					xmlReader.Read();
					while (continueToRead)
					{
						if (xmlReader.Name.Equals("left"))
							mSnapMargin.mLeft = xmlReader.ReadElementContentAsFloat();
						else if (xmlReader.Name.Equals("right"))
							mSnapMargin.mRight = xmlReader.ReadElementContentAsFloat();
						else if (xmlReader.Name.Equals("top"))
							mSnapMargin.mTop = xmlReader.ReadElementContentAsFloat();
						else if (xmlReader.Name.Equals("bottom"))
							mSnapMargin.mBottom = xmlReader.ReadElementContentAsFloat();
						else
							xmlReader.Read();
						// check if we reach the end of the snap margin
						continueToRead = !xmlReader.Name.Equals("SnapMargin") && !xmlReader.EOF;
					}
					// finish the snap margin
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readConnexionListTag(ref System.Xml.XmlReader xmlReader, Dictionary<string, int> connectionRemapingDictionary)
			{
				if (!xmlReader.IsEmptyElement)
				{
					// the connexion
					bool connexionFound = xmlReader.ReadToDescendant("connexion");
					if (connexionFound)
					{
						mConnectionPoints = new List<ConnectionPoint>();
						mConnectionPositionList = new List<PointF>();
					}

					while (connexionFound)
					{
						// instanciate a connection point for the current connexion
						ConnectionPoint connectionPoint = new ConnectionPoint();

						// read the first child node of the connexion
						xmlReader.Read();
						bool continueToReadConnexion = true;
						while (continueToReadConnexion)
						{
							if (xmlReader.Name.Equals("type"))
								connectionPoint.mType = readConnectionType(ref xmlReader, connectionRemapingDictionary);
							else if (xmlReader.Name.Equals("position"))
								connectionPoint.mPosition = readPointTag(ref xmlReader, "position");
							else if (xmlReader.Name.Equals("angle"))
								connectionPoint.mAngle = xmlReader.ReadElementContentAsFloat();
							else if (xmlReader.Name.Equals("angleToPrev"))
								connectionPoint.mAngleToPrev = xmlReader.ReadElementContentAsFloat();
							else if (xmlReader.Name.Equals("angleToNext"))
								connectionPoint.mAngleToNext = xmlReader.ReadElementContentAsFloat();
							else if (xmlReader.Name.Equals("nextConnexionPreference"))
								connectionPoint.mNextPreferedIndex = xmlReader.ReadElementContentAsInt();
							else if (xmlReader.Name.Equals("electricPlug"))
								connectionPoint.mElectricPlug = xmlReader.ReadElementContentAsInt();
							else
								xmlReader.Read();
							// check if we reach the end of the connexion
							continueToReadConnexion = !xmlReader.Name.Equals("connexion") && !xmlReader.EOF;
						}

						// add the current connexion in the list
						mConnectionPoints.Add(connectionPoint);
						mConnectionPositionList.Add(connectionPoint.mPosition);

						// go to next connexion
						connexionFound = !xmlReader.EOF && xmlReader.ReadToNextSibling("connexion");
					}
					// finish the connexion
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();

					// build the electric circuit if these connections are electrical
					// to know if there's a circuit between two connections, we must find pairs like
					// 1/-1 or 2/-2 or 3/-3, etc...
					if (mConnectionPoints != null)
					{
						int nbPossibleCircuits = mConnectionPoints.Count - 1;
						for (int i = 0; i < nbPossibleCircuits; ++i)
							for (int j = i + 1; j < mConnectionPoints.Count; ++j)
								if ((mConnectionPoints[i].mElectricPlug != 0) &&
									(mConnectionPoints[i].mElectricPlug == -mConnectionPoints[j].mElectricPlug))
								{
									// we found a circuit, so create the list if not already done
									if (this.mElectricCircuitList == null)
										this.mElectricCircuitList = new List<ElectricCircuit>();
									// compute the distance between the two connection (length of the circuit)
									PointF distance = new PointF(	mConnectionPoints[i].mPosition.X - mConnectionPoints[j].mPosition.X,
																	mConnectionPoints[i].mPosition.Y - mConnectionPoints[j].mPosition.Y);
									float length = (float)Math.Sqrt((distance.X * distance.X) + (distance.Y * distance.Y));
									// add the new circuit in the list
									this.mElectricCircuitList.Add(new ElectricCircuit(i, j, length));
								}
					}
				}
				else
				{
					xmlReader.Read();
				}
			}

			private int readConnectionType(ref System.Xml.XmlReader xmlReader, Dictionary<string, int> remapingDictionary)
			{
				string connectionName = xmlReader.ReadElementContentAsString();
				int index = BrickLibrary.ConnectionType.DEFAULT;
				remapingDictionary.TryGetValue(connectionName, out index);
				return index;
			}

			private void readHullTag(ref System.Xml.XmlReader xmlReader)
			{
				if (!xmlReader.IsEmptyElement)
				{
					bool pointFound = xmlReader.ReadToDescendant("point");
					while (pointFound)
					{
						if (xmlReader.Name.Equals("point"))
							mHull.Add(readPointTag(ref xmlReader, "point"));
						else
							xmlReader.Read();
						pointFound = !xmlReader.Name.Equals("hull") && !xmlReader.EOF;
					}
					// finish the hull
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readTrackDesignerTag(ref System.Xml.XmlReader xmlReader)
			{
				// check if the track designer is not empty
				if (!xmlReader.IsEmptyElement)
				{
					// the Track designer tag is not empty, instanciate the class that will hold the data
					mTDRemapData = new TDRemapData();

					// read the first child node
					xmlReader.Read();
					bool continueToRead = !xmlReader.EOF;
					while (continueToRead)
					{
						if (xmlReader.Name.Equals("ID"))
							readTrackDesignerIDTag(ref xmlReader);
						else if (xmlReader.Name.Equals("IDList"))
							readTrackDesignerIDListTag(ref xmlReader);
						else if (xmlReader.Name.Equals("Flag"))
							mTDRemapData.mFlags = xmlReader.ReadElementContentAsInt();
						else if (xmlReader.Name.Equals("HasSeveralGeometries"))
							mTDRemapData.mHasSeveralPort = xmlReader.ReadElementContentAsBoolean();
						else if (xmlReader.Name.Equals("TDBitmapList"))
							readTrackDesignerBitmapTag(ref xmlReader);
						else
							xmlReader.Read();

						// check if we need to continue
						continueToRead = !xmlReader.Name.Equals("TrackDesigner") && !xmlReader.EOF;
					}

					// finish the track designer tag
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readTrackDesignerIDTag(ref System.Xml.XmlReader xmlReader)
			{
				// check if the id list is not empty
				if (!xmlReader.IsEmptyElement)
				{
					// get the registry attribute if any
					string registry = "default";
					if (xmlReader.HasAttributes)
						registry = xmlReader.GetAttribute("registry");
					// read the int id
					int id = xmlReader.ReadElementContentAsInt();
					// add it to the id class (that can maintain several ids)
					mTDRemapData.mTDId.addId(registry, id);
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readTrackDesignerIDListTag(ref System.Xml.XmlReader xmlReader)
			{
				// check if the id list is not empty
				if (!xmlReader.IsEmptyElement)
				{
					// read the first child node
					xmlReader.Read();
					bool continueToRead = !xmlReader.EOF;
					while (continueToRead)
					{
						if (xmlReader.Name.Equals("ID"))
							readTrackDesignerIDTag(ref xmlReader);
						else
							xmlReader.Read();

						// check if we need to continue
						continueToRead = !xmlReader.Name.Equals("IDList") && !xmlReader.EOF;
					}

					// finish the ID list
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
			}

			private void readTrackDesignerBitmapTag(ref System.Xml.XmlReader xmlReader)
			{
				bool bitmapFound = xmlReader.ReadToDescendant("TDBitmap");
				// instanciate the list of bitmap id
				if (bitmapFound)
					mTDRemapData.mConnexionData = new List<TDRemapData.ConnexionData>();

				while (bitmapFound)
				{
					// instanciate a connection point for the current connexion
					TDRemapData.ConnexionData connexionData = new TDRemapData.ConnexionData();

					// read the first child node
					xmlReader.Read();
					bool continueToRead = !xmlReader.EOF;
					while (continueToRead)
					{
						if (xmlReader.Name.Equals("BBConnexionPointIndex"))
							connexionData.mBBConnexionPointIndex = xmlReader.ReadElementContentAsInt();
						else if (xmlReader.Name.Equals("Type"))
							connexionData.mType = xmlReader.ReadElementContentAsInt();
						else if (xmlReader.Name.Equals("AngleBetweenTDandBB"))
							connexionData.mDiffAngleBtwTDandBB = xmlReader.ReadElementContentAsFloat();
						else
							xmlReader.Read();

						// check if we need to continue
						continueToRead = !xmlReader.Name.Equals("TDBitmap") && !xmlReader.EOF;
					}

					// add the current connexion in the list
					mTDRemapData.mConnexionData.Add(connexionData);

					// go to next bitmap
					bitmapFound = !xmlReader.EOF && xmlReader.ReadToNextSibling("TDBitmap");
				}

				// finish the bitmap list
				if (!xmlReader.EOF)
					xmlReader.ReadEndElement();
			}

			private void readLDRAWTag(ref System.Xml.XmlReader xmlReader)
			{
				// check if the description is not empty
				bool continueToRead = !xmlReader.IsEmptyElement;
				if (continueToRead)
				{
					// the LDRAW tag is not empty, instanciate the class that will hold the data
					mLDrawRemapData = new LDrawRemapData();

					// read the first child node
					xmlReader.Read();
					continueToRead = !xmlReader.EOF;
					while (continueToRead)
					{
						if (xmlReader.Name.Equals("Angle"))
							mLDrawRemapData.mAngle = xmlReader.ReadElementContentAsFloat();
						else if (xmlReader.Name.Equals("Translation"))
							mLDrawRemapData.mTranslation = readPointTag(ref xmlReader, "Translation");
						else if (xmlReader.Name.Equals("PreferredHeight"))
							mLDrawRemapData.mPreferredHeight = xmlReader.ReadElementContentAsFloat();
						else if (xmlReader.Name.Equals("SleeperID"))
						{
							readBlueBrickId(ref xmlReader, ref mLDrawRemapData.mSleeperBrickNumber, ref mLDrawRemapData.mSleeperBrickColor);
							if (mLDrawRemapData.mSleeperBrickColor == null)
								mLDrawRemapData.mSleeperBrickColor = "0"; // black as default color
						}
						else if (xmlReader.Name.Equals("Alias"))
						{
							// before reading the alias content check if we need to create a remaping by reading the attribute
							bool needToAddRemap = !(xmlReader.HasAttributes && (xmlReader.GetAttribute("noremap") == "true"));
							// read the alias
							string fullPartId = readBlueBrickId(ref xmlReader, ref mLDrawRemapData.mAliasPartNumber, ref mLDrawRemapData.mAliasPartColor);
							// add the part to the remap list if need
							if (needToAddRemap)
								BrickLibrary.Instance.AddToTempRenamedPartList(fullPartId, this);
						}
						else
							xmlReader.Read();
						// check if we need to continue
						continueToRead = !xmlReader.Name.Equals("LDraw") && !xmlReader.EOF;
					}

					// finish the LDraw tag
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
			}

            private void readCanUngroupTag(ref System.Xml.XmlReader xmlReader)
            {
                if (!xmlReader.IsEmptyElement)
                {
                    bool canUngroup = xmlReader.ReadElementContentAsBoolean();
                    if (!canUngroup && (this.mBrickType == BrickType.OPEN_GROUP))
                        this.mBrickType = BrickType.SEALED_GROUP;
                }
                else
                {
                    xmlReader.Read();
                }
            }

            private void readSubPartListTag(ref System.Xml.XmlReader xmlReader)
            {
                if (!xmlReader.IsEmptyElement)
                {
                    // TODO
                    xmlReader.Read();
                }
                else
                {
                    xmlReader.Read();
                }
            }

			private string readBlueBrickId(ref System.Xml.XmlReader xmlReader, ref string partNumber, ref string partColor)
			{
				char[] partNumberSpliter = { '.' };
				string fullPartId = xmlReader.ReadElementContentAsString().ToUpper();
				string[] partNumberAndColor = fullPartId.Split(partNumberSpliter);
				if (partNumberAndColor.Length > 0)
				{
					partNumber = partNumberAndColor[0];
					if (partNumberAndColor.Length > 1)
						partColor = partNumberAndColor[1];
				}
				return fullPartId;
			}

			private PointF readPointTag(ref System.Xml.XmlReader xmlReader, string pointTagName)
			{
				float x = 0;
				float y = 0;
				if (!xmlReader.IsEmptyElement)
				{
					// read the first child node (x or y)
					xmlReader.Read();
					bool continueToReadPoint = true;
					while (continueToReadPoint)
					{
						if (xmlReader.Name.Equals("x") && !xmlReader.IsEmptyElement)
							x = xmlReader.ReadElementContentAsFloat();
						else if (xmlReader.Name.Equals("y") && !xmlReader.IsEmptyElement)
							y = xmlReader.ReadElementContentAsFloat();
						else
							xmlReader.Read();
						// check if we reach the end of the point
						continueToReadPoint = !xmlReader.Name.Equals(pointTagName) && !xmlReader.EOF;
					}
					// finish the point
					if (!xmlReader.EOF)
						xmlReader.ReadEndElement();
				}
				else
				{
					xmlReader.Read();
				}
				// return the point (or default 0,0 point)
				return new PointF(x, y);
			}
		}

		// a dictionary that contains all the images of the parts
		private Dictionary<string, Brick> mBrickDictionary = new Dictionary<string, Brick>();

		// a dictionary that contains all color names in the current langage of the application
		private Dictionary<int, string> mColorNames = new Dictionary<int, string>();

		// a list of all the different type of connections
		private List<ConnectionType> mConnectionTypes = new List<ConnectionType>();
		private Dictionary<string, int> mConnectionTypeRemapingDictionnary = new Dictionary<string, int>();

		// a dictionary to find the corresponding BlueBrick part number from the TD part id
		private Dictionary<int, List<Brick>> mTrackDesignerPartNumberAssociation = new Dictionary<int, List<Brick>>();

		// a dictionary that match the registry file names with a registry keyword used in the part XML files
		private Dictionary<string, string> mTrackDesignerRegistryFiles = new Dictionary<string, string>();

		// This temporary list is used during the loading of the Brick library to record the parts that have an alias
		private Dictionary<string, Brick> mTempRenamedPartList = new Dictionary<string, Brick>();

		// singleton on the map (we assume it is always valid)
		private static BrickLibrary sInstance = new BrickLibrary();

		// a special part to return as default for the unknown parts
		private bool mWereUnknownBricksAdded = false;

		#region get/set

		/// <summary>
		/// The static instance of the library of part that is only created
		/// when the application is started
		/// </summary>
		public static BrickLibrary Instance
		{
			get { return sInstance; }
		}

		/// <summary>
		/// Tell if some unknown brick were added since the creation of the library,
		/// or since the last time you reset the flag.
		/// </summary>
		public bool WereUnknownBricksAdded
		{
			get { return mWereUnknownBricksAdded; }
			set { mWereUnknownBricksAdded = value; }
		}

		public List<ConnectionType> ConnectionTypes
		{
			get { return mConnectionTypes; }
		}
		#endregion

		#region initialisation

		/// <summary>
		/// clear all the data contained in the part library
		/// </summary>
		public void clearAllData()
		{
			mBrickDictionary.Clear();
			mColorNames.Clear();
			mConnectionTypes.Clear();
			mConnectionTypeRemapingDictionnary.Clear();
			mTrackDesignerPartNumberAssociation.Clear();
			mTrackDesignerRegistryFiles.Clear();
			mTempRenamedPartList.Clear();
			mWereUnknownBricksAdded = false;
		}

		/// <summary>
		/// Add the specified brick with the specified old name to the temporary list for the rename parts
		/// </summary>
		/// <param name="partNumber">the old part number</param>
		/// <param name="brick">the actual brick that is linked with the old name</param>
		public void AddToTempRenamedPartList(string partNumber, Brick brick)
		{
			try
			{
				mTempRenamedPartList.Add(partNumber, brick);
			}
			catch
			{
				// if the brick is already added, we just don't care
			}
		}

		/// <summary>
		/// This method should be called when the application start to load the information from
		/// the config xml file.
		/// </summary>
		public void loadColorInfo()
		{
			// clear the color name table and description tables
			mColorNames.Clear();
			// try to load the xml file
			string xmlFileName = Application.StartupPath + @"/config/ColorTable.xml";
			if (System.IO.File.Exists(xmlFileName))
			{
				System.Xml.XmlReaderSettings xmlSettings = new System.Xml.XmlReaderSettings();
				xmlSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
				xmlSettings.IgnoreWhitespace = true;
				xmlSettings.IgnoreComments = true;
				xmlSettings.CheckCharacters = false;
				xmlSettings.CloseInput = true;
				System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(xmlFileName, xmlSettings);
 
				// use a variable to know if we need to read another language
				bool isDefaultLanguageEnglish = BlueBrick.Properties.Settings.Default.Language.Equals("en");

				// the colors
				bool colorFound = xmlReader.ReadToFollowing("color");
				while (colorFound)
				{
					// read the color id
					xmlReader.ReadAttributeValue();
					int colorId = int.Parse(xmlReader.GetAttribute(0));
					
					// read the default langage "en"
					xmlReader.ReadToDescendant("en");
					string defaultColorName = xmlReader.ReadElementContentAsString();

					// now search the name in the language of the application
					string colorName = string.Empty;
					if (isDefaultLanguageEnglish)
						colorName = defaultColorName;
					else if (xmlReader.Name.Equals(BlueBrick.Properties.Settings.Default.Language))
						colorName = xmlReader.ReadElementContentAsString();
					else if (xmlReader.ReadToNextSibling(BlueBrick.Properties.Settings.Default.Language))
						colorName = xmlReader.ReadElementContentAsString();
					else
						colorName = defaultColorName;

					// save the name in the dictionary
					mColorNames.Add(colorId, colorName);

					// read the next color
					while (!xmlReader.Name.Equals("color"))
						xmlReader.ReadToNextSibling("color");
					colorFound = xmlReader.ReadToNextSibling("color");
				}

				//close the stream
				xmlReader.Close();
			}
			else
			{
				string message = Properties.Resources.ErrorMsgMissingColorInfoFile.Replace("&", xmlFileName);
				MessageBox.Show(null, message,
					Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}

		/// <summary>
		/// This method should be called when the application start in order to load the information from
		/// the config xml file. This config file contains the list of connection types.
		/// </summary>
		public void loadConnectionTypeInfo()
		{
			// clear the color name table and description tables
			mConnectionTypes.Clear();
			mConnectionTypeRemapingDictionnary.Clear();

			// add the default connection at first in the list
			mConnectionTypes.Add(new ConnectionType(string.Empty, Color.Black, 1.0f));

			// try to load the xml file
			string xmlFileName = Application.StartupPath + @"/config/ConnectionTypeList.xml";
			if (System.IO.File.Exists(xmlFileName))
			{
				System.Xml.XmlReaderSettings xmlSettings = new System.Xml.XmlReaderSettings();
				xmlSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
				xmlSettings.IgnoreWhitespace = true;
				xmlSettings.IgnoreComments = true;
				xmlSettings.CheckCharacters = false;
				xmlSettings.CloseInput = true;
				System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(xmlFileName, xmlSettings);

				// read the first node
				if (xmlReader.ReadToFollowing("info"))
				{
					// read the first descendant
					xmlReader.Read();
					bool continueToRead = !xmlReader.EOF;
					// check if we have an override color fo the selected connection
					if (continueToRead && xmlReader.Name.Equals("SelectedConnection"))
					{
						Color color = Color.Red;
						if (xmlReader.ReadToDescendant("ColorARGB"))
						{
							string hexaNumber = xmlReader.ReadElementContentAsString();
							color = Color.FromArgb(int.Parse(hexaNumber, System.Globalization.NumberStyles.HexNumber));
						}
						float size = 2.5f;
						if (xmlReader.Name.Equals("Size"))
							size = xmlReader.ReadElementContentAsFloat();
						// update the selected connection rendering
						ConnectionType.sSelectedConnection = new ConnectionType(string.Empty, color, size);
					}
					// now parse the list of connection
					while (continueToRead && !xmlReader.Name.Equals("ConnectionType"))
					{
						xmlReader.Read();
						continueToRead = !xmlReader.EOF;
					}
					while (continueToRead)
					{
						// read the connection id
						xmlReader.ReadAttributeValue();
						string name = xmlReader.GetAttribute(0);

						// read the color
						Color color = Color.Black;
						if (xmlReader.ReadToDescendant("ColorARGB"))
						{
							string hexaNumber = xmlReader.ReadElementContentAsString();
							color = Color.FromArgb(int.Parse(hexaNumber, System.Globalization.NumberStyles.HexNumber));
						}

						// read the size
						float size = 1.0f;
						if (xmlReader.Name.Equals("Size"))
							size = xmlReader.ReadElementContentAsFloat();

						// add the new connection to the list
						mConnectionTypeRemapingDictionnary.Add(name, mConnectionTypes.Count);
						mConnectionTypes.Add(new ConnectionType(name, color, size));

						// read the next connection
						while (continueToRead && !xmlReader.Name.Equals("ConnectionType"))
						{
							xmlReader.ReadToNextSibling("ConnectionType");
							continueToRead = !xmlReader.EOF;
						}
						continueToRead = xmlReader.ReadToNextSibling("ConnectionType");
						continueToRead &= !xmlReader.EOF;
					}
				}

				//close the stream
				xmlReader.Close();
			}
			else
			{
				string message = Properties.Resources.ErrorMsgMissingConnectionTypeInfoFile.Replace("&", xmlFileName);
				MessageBox.Show(null, message,
					Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}

		/// <summary>
		/// This method should be called when the application start but after having
		/// initialized the library to duplicate entry in the dictionnary on the renamed parts
		/// </summary>
		public void createEntriesForRenamedParts()
		{
			// Add all the parts that have an alias (previous name or alias in LDRaw tag)
			foreach (KeyValuePair<string, Brick> renamedPart in mTempRenamedPartList)
			{
				// check if the alias brick does not already exist in the library, else we wont erase it
				Brick brickRef = null;
				mBrickDictionary.TryGetValue(renamedPart.Key, out brickRef);
				if (brickRef == null)
					mBrickDictionary.Add(renamedPart.Key, renamedPart.Value);
			}

			// we can clear the temporary list after 
			mTempRenamedPartList.Clear();
		}

		/// <summary>
		/// This method should be called when the application start to associate keyword and filename
		/// for the TD registry files.
		/// </summary>
		public void loadTrackDesignerRegistryFileList()
		{
			try
			{
				// clear the dictionnary before starting
				mTrackDesignerRegistryFiles.Clear();
				// try to load the rempa file
				string registryFileName = Application.StartupPath + @"/config/TDRegistryList.txt";
				System.IO.StreamReader textReader = new System.IO.StreamReader(registryFileName);
				char[] lineSpliter = { '=' };
				while (!textReader.EndOfStream)
				{
					// read the line, trim it and avoid empty line and comments
					string line = textReader.ReadLine();
					line = line.Trim();
					if ((line.Length > 0) && (line[0] != ';'))
					{
						try
						{
							// split the line with the "=" char, keep the line case sensitive
							string[] token = line.Split(lineSpliter);
							if (token.Length == 2)
								mTrackDesignerRegistryFiles.Add(token[1], token[0]);
						}
						catch (Exception)
						{
							// we just skip the line, if the format is not correct
						}
					}
				}
			}
			catch (Exception)
			{
				// ignore the remaping if the file is not present
			}
		}
		
		#endregion

		/// <summary>
		/// This method will check the windows registry info to know if there is any key
		/// set by the TrackDesigner software to know which TDR registry is used by TD.
		/// Track Designer store its preference in this key:
		/// HKEY_CURRENT_USER\Software\Train Depot\Track Designer\Registry
		/// </summary>
		public void updateCurrentTrackDesignerRegistryUsed()
		{
			// set the default keyword in case we don't find it
			Brick.TDRemapData.TDId.sCurrentRegistryUsed = "default";

			// try to read the value from the windows registry
			Object TDRegistryFilenameObject = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Train Depot\\Track Designer\\Registry", "Filename", null);
			if (TDRegistryFilenameObject != null)
			{
				// try to convert the key into a string and then find the file in the registry list
				string TDRegistryFilename = TDRegistryFilenameObject as string;
				if (TDRegistryFilename != null)
				{
					System.IO.FileInfo fileInfo = new System.IO.FileInfo(TDRegistryFilename);
					string keyword = null;
					if (mTrackDesignerRegistryFiles.TryGetValue(fileInfo.Name, out keyword))
						Brick.TDRemapData.TDId.sCurrentRegistryUsed = keyword;
				}
			}
		}

		/// <summary>
		/// This method create a default bitmap with a red cross and the part number,
		/// </summary>
		/// <param name="partNumber">can be null</param>
		/// <param name="widthInStud">width of the image of the brick in stud</param>
		/// <param name="heightInStud">height of the image of the brick in stud</param>
		/// <returns></returns>
		private Bitmap createUnknownImage(string partNumber, int widthInStud, int heightInStud)
		{
			int widthInPixel = widthInStud * Layer.NUM_PIXEL_PER_STUD_FOR_BRICKS;
			int heightInPixel = heightInStud * Layer.NUM_PIXEL_PER_STUD_FOR_BRICKS;
			int penWidth = Math.Max(widthInPixel, heightInPixel) / 16; // for the pen, take a percentage of the max value
			if (penWidth == 0)
				penWidth = 1;
			Pen pen = new Pen(Color.Red, penWidth);
			Bitmap unknownImage = new Bitmap(widthInPixel, heightInPixel);
			Graphics graphics = Graphics.FromImage(unknownImage);
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			graphics.DrawLine(pen, new Point(0, 0), new Point(widthInPixel, heightInPixel));
			graphics.DrawLine(pen, new Point(0, heightInPixel), new Point(widthInPixel, 0));
			if ((partNumber != null) && (partNumber.Length > 0))
			{
				SolidBrush brush = new SolidBrush(Color.Black);
				StringFormat format = new StringFormat();
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;
				float fontSize = Math.Min(widthInPixel, heightInPixel) / partNumber.Length;
				if (fontSize <= 4.0f)
					fontSize = 4.0f;
				Font font = new Font(FontFamily.GenericSansSerif, fontSize);
				graphics.DrawString(partNumber, font, brush, (float)widthInPixel * 0.5f, (float)heightInPixel * 0.5f, format);
			}
			// return the image created
			return unknownImage;
		}

        /// <summary>
        /// create a image from the parts of the group
        /// </summary>
        /// <param name="group">the brick representing the group in the library</param>
        /// <returns>the new image or null if the image cannot be created</returns>
        public Bitmap createGroupImage(Brick group)
        {
            // do nothing if the specified brick is not a group
            if (group.mBrickType != Brick.BrickType.OPEN_GROUP && group.mBrickType != Brick.BrickType.SEALED_GROUP)
                return null;

            // declare 4 variable to get the bounding box of the group part (in stud)
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            // iterate on the bricks to compute the size of the image
            foreach (Brick.SubPart subPart in group.mGroupSubPartList)
            {
                // try to get the part from the library, otherwise it's impossible to build group image
                if (mBrickDictionary.TryGetValue(subPart.mSubPartNumber, out subPart.mSubPartBrick))
                {
			        // create the transform
			        subPart.mTransform = new Matrix();
			        subPart.mTransform.Rotate(subPart.mAngle);
			        subPart.mTransform.Translate(subPart.mPosition.X, subPart.mPosition.Y, MatrixOrder.Append);
                    // transform the bounding box
                    PointF[] boundingPoints = subPart.mSubPartBrick.mBoundingBox.ToArray();
				    subPart.mTransform.TransformVectors(boundingPoints);
                    foreach (PointF point in boundingPoints)
                    {
                        if (point.X < minX)
                            minX = point.X;
                        if (point.X > maxX)
                            maxX = point.X;
                        if (point.Y < minY)
                            minY = point.Y;
                        if (point.Y > maxY)
                            maxY = point.Y;
                    }
                }
                else
                {
                    return null;
                }
            }
			// create a new image with the correct size
			int width = (int)((maxX - minX) * Layer.NUM_PIXEL_PER_STUD_FOR_BRICKS);
            int height = (int)((maxY - minY) * Layer.NUM_PIXEL_PER_STUD_FOR_BRICKS);
            if ((width > 0) && (height > 0))
            {
                Bitmap image = new Bitmap(width, height);
                // get the graphic context and draw the referenc image in it with the correct transform and scale
                Graphics graphics = Graphics.FromImage(image);
                graphics.Clear(Color.Transparent);
                graphics.CompositingMode = CompositingMode.SourceCopy; // this should be enough since we draw the image on an empty transparent area
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear; // we need it for the high scale down version
                foreach (Brick.SubPart subPart in group.mGroupSubPartList)
                {
                    graphics.Transform = subPart.mTransform;
                    graphics.DrawImage(subPart.mSubPartBrick.mImage, 0, 0, subPart.mSubPartBrick.mImage.Width, subPart.mSubPartBrick.mImage.Height);
                }
                graphics.Flush();
                return image;
            }
            return null;
        }

		/// <summary>
		/// Return the complete list of all the names of the bricks currently in the library
		/// </summary>
		/// <returns>the complete list of all the names of the bricks currently in the library</returns>
		public string[] getBrickNameList()
		{
			string[] result = new string[mBrickDictionary.Keys.Count];
			mBrickDictionary.Keys.CopyTo(result, 0);
			return result;
		}

		/// <summary>
		/// If the part was rename, this function give the new name from the specified old name
		/// </summary>
		/// <param name="partNumber">a potential old name</param>
		/// <returns>the new name of the part or the specified param if the brick was not founded</returns>
		public string getActualPartNumber(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mPartNumber;
			return partNumber;
		}

		public void AddUnknownBrick(string partNumber, int widthInStud, int heightInStud)
		{
			if (widthInStud <= 0)
				widthInStud = 32;
			if (heightInStud <= 0)
				heightInStud = 32;
			Bitmap unknownImage = createUnknownImage(partNumber, widthInStud, heightInStud);
			Brick brick = new Brick(partNumber, unknownImage, null, mConnectionTypeRemapingDictionnary);
			mBrickDictionary.Add(partNumber, brick);
			mWereUnknownBricksAdded = true;
		}

		public void AddBrick(string partNumber, Image image, string xmlFileName)
		{
			// instanciate the brick and add it into the dictionnary
            Brick brick = new Brick(partNumber, image, xmlFileName, mConnectionTypeRemapingDictionnary);
			// if the creation of the brick launch an exception it will be catched by the calling function.
			try
			{
				mBrickDictionary.Add(partNumber, brick);
				// add also the link between the TD number and the BB number if there is an available TD remap data
				if (brick.mTDRemapData != null)
					brick.mTDRemapData.mTDId.addToAssociationDictionnary(mTrackDesignerPartNumberAssociation, brick);
			}
			catch
			{
				// we don't care if there is already a part mapped
			}
		}

		public Image getImage(string partNumber, ref List<PointF> boundingBox, ref List<PointF> hull)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
			{
				boundingBox = brickRef.mBoundingBox;
				hull = brickRef.mHull;
				return brickRef.mImage;
			}
			boundingBox = null;
			hull = null;
			return null;
		}

		public Image getImage(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mImage;
			return null;
		}

		public int getConnexionType(string partNumber, int connexionIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionPoints != null))
				return brickRef.mConnectionPoints[connexionIndex].mType;
			return ConnectionType.DEFAULT;
		}

		public Brick.Margin getSnapMargin(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mSnapMargin;
			return new Brick.Margin();
		}

		public List<Brick.ConnectionPoint> getConnectionList(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mConnectionPoints;
			return null;
		}

		public List<PointF> getConnectionPositionList(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mConnectionPositionList;
			return null;
		}

		/// <summary>
		/// Get the number of connection for the specified part. If the part doesn't have any connection
		/// list, then the function return 0. Also if the part doesn't exist in the library, the function
		/// return 0.
		/// </summary>
		/// <param name="partNumber">the full part number</param>
		/// <returns>The number of connection for the specified part, or 0 if the part doesn't exist.</returns>
		public int getConnectionCount(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionPoints != null))
				return brickRef.mConnectionPoints.Count;
			return 0;
		}

		public float getConnectionAngle(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionPoints != null))
				return brickRef.mConnectionPoints[pointIndex].mAngle;
			return 0.0f;
		}

		public float getConnectionAngleToPrev(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionPoints != null))
				return brickRef.mConnectionPoints[pointIndex].mAngleToPrev;
			return 0.0f;
		}

		public float getConnectionAngleToNext(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionPoints != null))
				return brickRef.mConnectionPoints[pointIndex].mAngleToNext;
			return 0.0f;
		}

		public int getConnectionNextPreferedIndex(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionPoints != null))
				return brickRef.mConnectionPoints[pointIndex].mNextPreferedIndex;
			return 0;
		}

		public List<Brick.ElectricCircuit> getElectricCircuitList(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mElectricCircuitList;
			return null;
		}

		public string getImageURL(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mImageURL;
			return null;
		}

		/// <summary>
		/// return info based on the part number. 4 texts are returned, in an array in this order:
		/// the LDRAW part number, the color number, the color name, and the description
		/// </summary>
		/// <param name="partNumber">the bluebrick part number</param>
		/// <returns></returns>
		public string[] getBrickInfo(string partNumber)
		{
			string[] result = new string[4];

			// first split the bluebrick number
			string[] numberAndColor = partNumber.Split(new char[] { '.' });
			// the LDRAW part number is always the first part of the BlueBrick part number
			result[0] = numberAndColor[0];
			// now check if we have a valid color
			if (numberAndColor.Length < 2)
			{
				// this case if for the logo for example
				result[1] = string.Empty;
				result[2] = BlueBrick.Properties.Resources.TextNA;
			}
			else
			{
				// We copy the color id no matter if it is a valid id number or "set" for example
				// we copy it anyway in the result
				result[1] = numberAndColor[1];

				// try to parse the color id that may failed if the color is "set"
				int colorId = 0;
				if (int.TryParse(numberAndColor[1], out colorId))
				{
					// try to get the name of this color (this can fail because we may have a valid color id
					// but no name for this color in the list of color name)
					string colorName = string.Empty;
					if (mColorNames.TryGetValue(colorId, out colorName))
						result[2] = colorName;
					else
						result[2] = BlueBrick.Properties.Resources.TextUnknown;
				}
				else
				{
					// no valid color id, this case is for the "set" for example
					result[2] = BlueBrick.Properties.Resources.TextNA;
				}
			}

			// try to get the description
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				result[3] = brickRef.mDescription;
			else
				result[3] = BlueBrick.Properties.Resources.TextUnknown;

			return result;
		}

		/// <summary>
		/// return a formated string containing various info on the specified brick
		/// and that can be used to display on the Status bar of the application
		/// </summary>
		/// <param name="partNumber">the bluebrick part number</param>
		/// <returns>A formated description of the part</returns>
		public string getFormatedBrickInfo(string partNumber, bool withId, bool withColor, bool withDescription)
		{
			string[] partInfo = getBrickInfo(partNumber);
			// construct the message with part number, color and description and return it
			string formatedInfo = "";
			if (withId)
				formatedInfo += partInfo[0];
			if (withColor)
			{
				if (withId)
					formatedInfo += ", ";
				formatedInfo += partInfo[2];
			}
			if (withDescription)
			{
				if (withId || withColor)
					formatedInfo += ", ";
				formatedInfo += partInfo[3];
			}
			return formatedInfo;
		}

		/// <summary>
		/// Get the Track Designer remap data class for the specified BlueBrick part number.
		/// </summary>
		/// <param name="partNumber">the bluebrick part number</param>
		/// <returns>An instance of remap data, that contains all the information to translate the specified part into Track Designer</returns>
		public Brick.TDRemapData getTDRemapData(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mTDRemapData;
			return null;
		}

		/// <summary>
		/// Get the Track Designer remap data class for the specified Track Designer ID.
		/// </summary>
		/// <param name="partNumber">the Track Designer ID</param>
		/// <param name="BBPartNumber">The corresponding BlueBrick part number return</param>
		/// <returns>An instance of remap data, that contains all the information to translate the specified part into Track Designer</returns>
		public Brick.TDRemapData getTDRemapData(int tdID, out string BBPartNumber)
		{
			// the brick that will best fit the TD id
			Brick brickFound = null;
			// first get the corresponding Brick list from the TD id
			List<Brick> brickList = null;
			mTrackDesignerPartNumberAssociation.TryGetValue(tdID, out brickList);
			// if not null try to get the remap data for this or these Brick parts
			if (brickList != null)
			{
				if (brickList.Count == 1)
				{
					// if only one brick is mapped with this TD id, just use this one
					brickFound = brickList[0];
				}
				else
				{
					// The brick library normally can not generate an empty list, but even if the
					// list is empty, the following code will not fail.
					// else we need to search the best brick according to the current TD registry set
					// on this machine and the TD id
					Brick bestBrick = null; // the brick for the registry set on this machine
					Brick defaultBrick = null; // the default brick
					Brick otherRegistryBrick = null; // the brick comes from another registry
					foreach (Brick brick in brickList)
						if (brick.mTDRemapData.mTDId.IDForCurrentRegistry == tdID)
							bestBrick = brick;
						else if (brick.mTDRemapData.mTDId.DefaultID == tdID)
							defaultBrick = brick;
						else
							otherRegistryBrick = brick;

					// return the best brick among all the brick found
					if (bestBrick != null)
						brickFound = bestBrick;
					else if (defaultBrick != null)
						brickFound = defaultBrick;
					else if (otherRegistryBrick != null)
						brickFound = otherRegistryBrick;
				}
			}

			// if we found a brick, set the part number and return its remapdata (that can be null)
			if (brickFound != null)
			{
				BBPartNumber = brickFound.mPartNumber;
				return brickFound.mTDRemapData;
			}
			else
			{
				BBPartNumber = null;
				return null;
			}
		}

		/// <summary>
		/// Get the LDRAW remap data class for the specified BlueBrick part number.
		/// </summary>
		/// <param name="partNumber">the bluebrick part number</param>
		/// <returns>An instance of remap data, that contains all the information to translate the specified part into LDRAW</returns>
		public Brick.LDrawRemapData getLDrawRemapData(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mLDrawRemapData;
			return null;
		}

		/// <summary>
		/// Tell if the specified brick id (with color) should be ignore during the loading of a LDraw File.
		/// </summary>
		/// <param name="partNumber">the bluebrick part number</param>
		/// <returns>true if the brick should be skiped</returns>
		public bool shouldBeIgnored(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.ShouldBeIgnored;
			return false;
		}
	}
}
