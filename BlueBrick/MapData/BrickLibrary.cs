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

namespace BlueBrick.MapData
{
	public class BrickLibrary
	{
		public class Brick
		{
			public enum ConnectionType
			{
				BRICK = 0,
				RAIL,
				ROAD,
				MONORAIL,
				MONORAIL_SHORT_CURVE,
				DUPLO_RAIL,
				// the last one to count
				COUNT,
			};

			public class Margin
			{
				public float mLeft = 0.0f;
				public float mRight = 0.0f;
				public float mTop = 0.0f;
				public float mBottom = 0.0f;
			}

			public string			mPartNumber = null; // the part number (same as the key in the dictionnary), usefull in case of part renaming
			public Image			mImage = null;	// the image of the brick just as it is loaded from the hardrive
			public List<ConnectionType>	mConnectionType = null;
			public List<PointF>		mConnectionPoint = null; // list of all the connection point in pixel, relative to the center of the brick
			public List<float>		mConnectionAngle = null; // list of all the angle that we should to add to connect to each connection point
			public List<float>		mConnectionAngleToPrev = null;
			public List<float>		mConnectionAngleToNext = null;
			public List<int>		mConnectionNextPreferedIndex = null;
			public Margin			mSnapMargin = new Margin(); // the the inside margin that should be use for snapping the part on the grid
			public List<PointF>		mBoundingBox = new List<PointF>(5); // list of the 4 corner in pixel, plus the origin in stud and from the center
			public List<PointF>		mHull = new List<PointF>(4); // list of all the points in pixel that describe the hull of the part

			public Brick(string partNumber, Image image, string xmlFileName)
			{
				// assign the part num and the image
				mPartNumber = partNumber;
				mImage = image;
				bool hullFound = false;
				// parse the xml data
				if (System.IO.File.Exists(xmlFileName))
				{
					System.Xml.XmlReaderSettings xmlSettings = new System.Xml.XmlReaderSettings();
					xmlSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
					xmlSettings.IgnoreWhitespace = true;
					xmlSettings.IgnoreComments = true;
					System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(xmlFileName, xmlSettings);

					// origin of the brick
					if (xmlReader.ReadToFollowing("SnapMargin"))
					{
						xmlReader.ReadToDescendant("left");
						mSnapMargin.mLeft = xmlReader.ReadElementContentAsFloat();
						mSnapMargin.mRight = xmlReader.ReadElementContentAsFloat();
						mSnapMargin.mTop = xmlReader.ReadElementContentAsFloat();
						mSnapMargin.mBottom = xmlReader.ReadElementContentAsFloat();
					}
					// the connexion
					bool connexionFound = xmlReader.ReadToFollowing("connexion");
					if (connexionFound)
					{
						mConnectionType = new List<ConnectionType>();
						mConnectionPoint = new List<PointF>();
						mConnectionAngle = new List<float>();
						mConnectionAngleToPrev = new List<float>();
						mConnectionAngleToNext = new List<float>();
						mConnectionNextPreferedIndex = new List<int>();
					}
					while (connexionFound)
					{
						// read the type of the connexion
						ConnectionType connexionType = ConnectionType.BRICK;
						if (xmlReader.ReadToDescendant("type"))
						{
							connexionType = (ConnectionType)(xmlReader.ReadElementContentAsInt());
						}
						mConnectionType.Add(connexionType);
						// read the position of the connexion
						PointF position = new PointF();
						if (xmlReader.LocalName.Equals("position"))
						{
							xmlReader.ReadToDescendant("x");
							position.X = xmlReader.ReadElementContentAsFloat();
							position.Y = xmlReader.ReadElementContentAsFloat();
						}
						mConnectionPoint.Add(position);
						// read the angle of the connexion
						float angle = 0.0f;
						if (xmlReader.ReadToFollowing("angle"))
						{
							angle = xmlReader.ReadElementContentAsFloat();
						}
						mConnectionAngle.Add(angle);
						// read the angle to prev of the connexion
						angle = 180.0f;
						if (xmlReader.LocalName.Equals("angleToPrev"))
						{
							angle = xmlReader.ReadElementContentAsFloat();
						}
						mConnectionAngleToPrev.Add(angle);
						// read the angle to prev of the connexion
						angle = 180.0f;
						if (xmlReader.LocalName.Equals("angleToNext"))
						{
							angle = xmlReader.ReadElementContentAsFloat();
						}
						mConnectionAngleToNext.Add(angle);
						// read the preference for the next connexion
						int preferedIndex = 0;
						if (xmlReader.LocalName.Equals("nextConnexionPreference"))
						{
							preferedIndex = xmlReader.ReadElementContentAsInt();
						}
						mConnectionNextPreferedIndex.Add(preferedIndex);
						// go to next connexion
						connexionFound = xmlReader.ReadToNextSibling("connexion");
					}
					// the hull
					hullFound = xmlReader.ReadToFollowing("hull");
					if (hullFound)
					{
						bool pointFound = xmlReader.ReadToDescendant("point");
						while (pointFound)
						{
							xmlReader.ReadToDescendant("x");
							PointF point = new PointF(xmlReader.ReadElementContentAsFloat(), xmlReader.ReadElementContentAsFloat());
							mHull.Add(point);
							pointFound = xmlReader.ReadToNextSibling("point");
						}
					}
					// close the xml file
					xmlReader.Close();
				}

				// create the bounding box list (usefull for the creation of the rotated parts
				mBoundingBox.Add(new PointF(0, 0));
				mBoundingBox.Add(new PointF((float)(image.Width), 0));
				mBoundingBox.Add(new PointF(0, (float)(image.Height)));
				mBoundingBox.Add(new PointF((float)(image.Width), (float)(image.Height)));
									
				// If there's no hull, we use the bounding box as a default hull
				if (!hullFound)
					mHull = mBoundingBox;
			}
		}

		// a dictionary that contains all the images of the parts
		private Dictionary<string, Brick> mBrickDictionary = new Dictionary<string, Brick>();

		// a dictionary that contains all color names in the current langage of the application
		private Dictionary<int, string> mColorNames = new Dictionary<int, string>();

		// a dictionary that contains all parts description in the current langage of the application
		private Dictionary<string, string> mPartDescriptions = new Dictionary<string, string>();

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
		#endregion

		#region initialisation

		/// <summary>
		/// clear all the data contained in the part library
		/// </summary>
		public void clearAllData()
		{
			mBrickDictionary.Clear();
			mColorNames.Clear();
			mPartDescriptions.Clear();
			mWereUnknownBricksAdded = false;
		}

		/// <summary>
		/// This method should be called when the application start to load the information from
		/// the config xml file.
		/// </summary>
		public void loadColorAndDescriptionInfo()
		{
			// clear the color name table and description tables
			mColorNames.Clear();
			mPartDescriptions.Clear();
			// try to load the xml file
			string xmlFileName = Application.StartupPath + @"/config/PartInfo.xml";
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

				// the descriptions
				bool partFound = xmlReader.ReadToFollowing("part");
				while (partFound)
				{
					// read the color id
					xmlReader.ReadAttributeValue();
					string partId = xmlReader.GetAttribute(0).ToUpper();

					// read the default langage "en"
					xmlReader.ReadToDescendant("en");
					string defaultDescriptionName = xmlReader.ReadElementContentAsString();

					// now search the name in the language of the application
					string descriptionName = string.Empty;
					if (isDefaultLanguageEnglish)
						descriptionName = defaultDescriptionName;
					else if (xmlReader.Name.Equals(BlueBrick.Properties.Settings.Default.Language))
						descriptionName = xmlReader.ReadElementContentAsString();
					else if (xmlReader.ReadToNextSibling(BlueBrick.Properties.Settings.Default.Language))
						descriptionName = xmlReader.ReadElementContentAsString();
					else
						descriptionName = defaultDescriptionName;

					// save the name in the dictionary
					mPartDescriptions.Add(partId, descriptionName);

					// read the next part
					while (!xmlReader.Name.Equals("part"))
						xmlReader.ReadToNextSibling("part");
					partFound = xmlReader.ReadToNextSibling("part");
				}

				//close the stream
				xmlReader.Close();
			}
			else
			{
				string message = Properties.Resources.ErrorMissingPartInfoFile.Replace("&", xmlFileName);
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
			try
			{
				// try to load the rempa file
				string remapFileName = Application.StartupPath + @"/parts/PartRemap.txt";
				System.IO.StreamReader textReader = new System.IO.StreamReader(remapFileName);
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
							string[] token = line.ToUpper().Split(lineSpliter);
							// try to get the two bricks from the two refs
							Brick oldBrickRef = null;
							mBrickDictionary.TryGetValue(token[0], out oldBrickRef);
							Brick newBrickRef = null;
							mBrickDictionary.TryGetValue(token[1], out newBrickRef);
							// check if the old name doesn't already have a brick on it,
							// and that we found the brick for the new name
							if ((oldBrickRef == null) && (newBrickRef != null))
							{
								// if we found the new brick, we can add a second entry for it (with the old name)
								mBrickDictionary.Add(token[0], newBrickRef);
							}
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
			if (partNumber != null)
			{
				SolidBrush brush = new SolidBrush(Color.Black);
				StringFormat format = new StringFormat();
				format.Alignment = StringAlignment.Center;
				format.LineAlignment = StringAlignment.Center;
				float fontSize = Math.Min(widthInPixel, heightInPixel) / partNumber.Length;
				Font font = new Font(FontFamily.GenericSansSerif, fontSize);
				graphics.DrawString(partNumber, font, brush, widthInPixel / 2, heightInPixel / 2, format);
			}
			// return the image created
			return unknownImage;
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
			Brick brick = new Brick(partNumber, unknownImage, null);
			mBrickDictionary.Add(partNumber, brick);
			mWereUnknownBricksAdded = true;
		}

		public void AddBrick(string partNumber, Image image, string xmlFileName)
		{
			Brick brick = new Brick(partNumber, image, xmlFileName);
			mBrickDictionary.Add(partNumber, brick);
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

		public Brick.ConnectionType getConnexionType(string partNumber, int connexionIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionType != null))
				return brickRef.mConnectionType[connexionIndex];
			return Brick.ConnectionType.BRICK;
		}

		public Brick.Margin getSnapMargin(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mSnapMargin;
			return new Brick.Margin();
		}

		public List<PointF> getConnectionList(string partNumber)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if (brickRef != null)
				return brickRef.mConnectionPoint;
			return null;
		}

		public float getConnectionAngle(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionAngle != null))
				return brickRef.mConnectionAngle[pointIndex];
			return 0.0f;
		}

		public float getConnectionAngleToPrev(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionAngleToPrev != null))
				return brickRef.mConnectionAngleToPrev[pointIndex];
			return 0.0f;
		}

		public float getConnectionAngleToNext(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionAngleToNext != null))
				return brickRef.mConnectionAngleToNext[pointIndex];
			return 0.0f;
		}

		public int getConnectionNextPreferedIndex(string partNumber, int pointIndex)
		{
			Brick brickRef = null;
			mBrickDictionary.TryGetValue(partNumber, out brickRef);
			if ((brickRef != null) && (brickRef.mConnectionNextPreferedIndex != null))
				return brickRef.mConnectionNextPreferedIndex[pointIndex];
			return 0;
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

			// compute the part id by removing the color, but if there's no
			// valid color, or for example "set" instead of the color number
			// use the full partNumber.
			string partId = string.Empty;

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
				partId = partNumber;
			}
			else
			{
				// try to parse the color id that may failed if the color is "set"
				int colorId = 0;
				if (int.TryParse(numberAndColor[1], out colorId))
				{
					// we have a valid color, so the partId is only the first part
					partId = numberAndColor[0];
					// try to get the name of this color
					string colorName = string.Empty;
					if (mColorNames.TryGetValue(colorId, out colorName))
					{
						result[1] = numberAndColor[1];
						result[2] = colorName;
					}
					else
					{
						result[1] = string.Empty;
						result[2] = BlueBrick.Properties.Resources.TextUnknown;
					}
				}
				else
				{
					// no valid color id, this case is for the "set" for example
					result[1] = string.Empty;
					result[2] = BlueBrick.Properties.Resources.TextNA;
					partId = partNumber;
				}
			}

			// try to get the description
			string description = string.Empty;
			if (!mPartDescriptions.TryGetValue(partId, out description))
				description = BlueBrick.Properties.Resources.TextUnknown;
			result[3] = description;

			return result;
		}

		/// <summary>
		/// return a formated string containing various info on the specified brick
		/// and that can be used to display on the Status bar of the application
		/// </summary>
		/// <param name="partNumber">the bluebrick part number</param>
		/// <returns>A formated description of the part</returns>
		public string getFormatedBrickInfo(string partNumber)
		{
			string[] partInfo = getBrickInfo(partNumber);
			// construct the message with part number, color and description and return it
			return (partInfo[0] + ", " + partInfo[2] + ", " + partInfo[3]);
		}
	}
}
