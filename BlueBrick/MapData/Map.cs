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
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using BlueBrick.Actions;
using BlueBrick.Actions.Bricks;
using BlueBrick.Properties;

namespace BlueBrick.MapData
{
	/// <summary>
	/// This class represent a map created/edited by this application.
	/// </summary>
	/// <remarks>
	/// <para>A Map contains several <see cref="Layer"/> stacked in a specific order.
	/// The layers are displayed from the bottom to the top.</para>
	/// <para>This class correspond also to the file that is save/load by the
	/// application.</para>
	/// </remarks>
	[Serializable]
	public class Map : IXmlSerializable
	{
		public enum BrickAddability
		{
			YES,
			NO_WRONG_LAYER_TYPE,
			NO_BUDGET_EXCEEDED,
			YES_AND_NO_TRIMMED_BY_BUDGET,
			YES_AND_NO_REPLACEMENT_LIMITED_BY_BUDGET
		}

		// the current version of the data this version of BlueBrick can read/write
		private const int CURRENT_DATA_VERSION = 8;

		// the current version of the data
		private static int mDataVersionOfTheFileLoaded = CURRENT_DATA_VERSION;

        // for the current map
        private string mMapFileName = Properties.Resources.DefaultSaveFileName;
        private bool mIsMapNameValid = false;

		// data global to the map that can change the user
		private string mAuthor = Properties.Resources.DefaultAuthor;
		private string mLUG = Properties.Resources.DefaultLUG;
		private string mShow = Properties.Resources.DefaultShow;
		private DateTime mDate = DateTime.Today;
		private string mComment = "";
		private Color mBackgroundColor = BlueBrick.Properties.Settings.Default.DefaultBackgroundColor;
		private string mGeneralInfoWatermark = "";

		// data for the image export (this contains the last export settings for this map)
		private string mExportAbsoluteFileName = string.Empty; // file name including full path from root
        private int mExportFileTypeIndex = 1; // index in the combobox for the different type of export
		private RectangleF mExportArea = new RectangleF();
		private double mExportScale = 0.0;
        private bool mExportWatermark = BlueBrick.Properties.Settings.Default.UIExportWatermark;
        private bool mExportBrickHull = BlueBrick.Properties.Settings.Default.UIExportHulls;
        private bool mExportElectricCircuit = BlueBrick.Properties.Settings.Default.UIExportElectricCircuit;
        private bool mExportConnectionPoints = BlueBrick.Properties.Settings.Default.UIExportConnection;
		private bool mHasExportSettingsChanged = false; // a boolean flag indicating that the settings has changed and that the file need to be saved

		// some data for compatibility with Track designer
		private bool mAllowElectricShortCuts = false;
		private bool mAllowUnderground = false;
		private bool mAllowSteps = false;
		private bool mAllowSlopeMismatch = false;

		// a counter of modification
		private int mNumberOfModificationSinceLastSave = 0;

		// layer
		private List<Layer> mLayers = new List<Layer>();

		private Layer mSelectedLayer = null;
		private Layer mLayerThatHandleMouse = null;

		// singleton on the map (we assume it is always valid)
		private static Map sInstance = new Map();

        // drawing data to draw the general info watermark
        private static SolidBrush mWatermarkBrush = new SolidBrush(Settings.Default.WatermarkColor);
        private static SolidBrush mWatermarkBackgroundBrush = new SolidBrush(Settings.Default.WatermarkBackgroundColor);

		#region get/set

		/// <summary>
		/// The static instance of the map (this application is a single
		/// doc application, so we can only have one map at the same time)
		/// </summary>
		public static Map Instance
		{
			get { return sInstance; }
			set	{ sInstance = value; }
		}

        public string MapFileName
        {
            get { return mMapFileName; }
            set { mMapFileName = value; }
        }

        public bool IsMapNameValid
        {
            get { return mIsMapNameValid; }
            set { mIsMapNameValid = value; }
        }

		public string Author
		{
			get { return mAuthor; }
			set { mAuthor = value; computeGeneralInfoWatermark();  }
		}

		public string LUG
		{
			get { return mLUG; }
			set { mLUG = value; computeGeneralInfoWatermark(); }
		}

		public string Show
		{
			get { return mShow; }
			set { mShow = value; computeGeneralInfoWatermark(); }
		}

		public DateTime Date
		{
			get { return mDate; }
			set { mDate = value; computeGeneralInfoWatermark(); }
		}

		public string Comment
		{
			get { return mComment; }
			set { mComment = value; }
		}

		public string GeneralInfoWatermark
		{
			get { return mGeneralInfoWatermark; }
		}

		public Color BackgroundColor
		{
			get { return mBackgroundColor; }
			set { mBackgroundColor = value; }
		}

		public bool AllowElectricShortCuts
		{
			get { return mAllowElectricShortCuts; }
			set { mAllowElectricShortCuts = value; }
		}

		public bool AllowUnderground
		{
			get { return mAllowUnderground; }
			set { mAllowUnderground = value; }
		}

		public bool AllowSteps
		{
			get { return mAllowSteps; }
			set { mAllowSteps = value; }
		}

		public bool AllowSlopeMismatch
		{
			get { return mAllowSlopeMismatch; }
			set { mAllowSlopeMismatch = value; }
		}

		public int NumLayers
		{
			get { return mLayers.Count; }
		}

		public List<Layer> LayerList
		{
			get { return mLayers; }
		}

		public bool WasModified
		{
			get { return ((mNumberOfModificationSinceLastSave != 0) || mHasExportSettingsChanged); }
			set
			{
				// if the value is false (meaning we just saved the file), reset all the flags
				if (!value)
				{
					mNumberOfModificationSinceLastSave = 0;
					mHasExportSettingsChanged = false;
				}
			}
		}

		public static int DataVersionOfTheFileLoaded
		{
			get { return mDataVersionOfTheFileLoaded; }
		}

		/// <summary>
		/// The current selected layer
		/// </summary>
		public Layer SelectedLayer
		{
			get { return mSelectedLayer; }
			set
			{
				// clear the selection of the previous selected layer
				if (mSelectedLayer != null)
					mSelectedLayer.clearSelection();

				// select the new layer
				mSelectedLayer = value;

				// according to the type of the layer selected, enable or disable some tool on the main form
				if (mSelectedLayer != null)
				{
					if (mSelectedLayer is LayerGrid)
						MainForm.Instance.enableToolbarButtonOnLayerSelection(false, false, false);
					else if (mSelectedLayer is LayerArea)
						MainForm.Instance.enableToolbarButtonOnLayerSelection(false, true, false);
					else if (mSelectedLayer is LayerRuler)
						MainForm.Instance.enableToolbarButtonOnLayerSelection(true, false, true);
					else
						MainForm.Instance.enableToolbarButtonOnLayerSelection(true, false, false);
				}
				else
				{
					MainForm.Instance.enableToolbarButtonOnLayerSelection(false, false, false);
				}
			}
		}

        public string ExportAbsoluteFileName
		{
			get { return mExportAbsoluteFileName; }
		}

        public int ExportFileTypeIndex
		{
			get { return mExportFileTypeIndex; }
		}

		public RectangleF ExportArea
		{
			get { return mExportArea; }
		}

		public double ExportScale
		{
			get { return mExportScale; }
		}

        public bool ExportWatermark
        {
            get { return mExportWatermark; }
        }

        public bool ExportBrickHull
        {
            get { return mExportBrickHull; }
        }

        public bool ExportElectricCircuit
        {
            get { return mExportElectricCircuit; }
        }

        public bool ExportConnectionPoints
        {
            get { return mExportConnectionPoints; }
        }
		#endregion

		#region constructor
		/// <summary>
		/// Constructor.
		/// </summary>
		public Map()
		{
			// and construct the watermark
			computeGeneralInfoWatermark();
		}

		/// <summary>
		/// Update the gamma setting for the selections of bricks in brick layers
		/// </summary>
		public void updateGammaFromSettings()
		{
			foreach (Layer layer in mLayers)
			{
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					brickLayer.updateGammaFromSettings();
			}
		}

		/// <summary>
		/// Compute the string displayed on top of the map from some general infos
		/// </summary>
		private void computeGeneralInfoWatermark()
		{
			mGeneralInfoWatermark = this.Author + ", " + this.LUG + ", " + this.Show + " (" + this.Date.ToShortDateString() + ")";
		}
		#endregion

		#region IXmlSerializable Members

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			// reset the counter of modifications because we just load the map (no modification done)
			mNumberOfModificationSinceLastSave = 0;
			// version
			readVersionNumber(reader);
			// check if the BlueBrick program is not too old, that
			// means the user try to load a file generated with
			/// a earlier version of BlueBrick
			if (mDataVersionOfTheFileLoaded > CURRENT_DATA_VERSION)
			{
				MessageBox.Show(null, Properties.Resources.ErrorMsgProgramObsolete,
					Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);				
				return;
			}
			// get the number of layer for the progressbar
			if (mDataVersionOfTheFileLoaded >= 3)
			{
				int nbItems = reader.ReadElementContentAsInt();
				// init the progress bar with the real number of layer items (+1 for the header +1 for the link rebuilding)
				MainForm.Instance.resetProgressBar(nbItems + 2);
			}
			// check is there is a background color
			if (reader.Name.Equals("BackgroundColor"))
				mBackgroundColor = XmlReadWrite.readColor(reader);
			// data of the map
			mAuthor = reader.ReadElementContentAsString();
			mLUG = reader.ReadElementContentAsString();
			mShow = reader.ReadElementContentAsString();
			reader.ReadToDescendant("Day");
			int day = reader.ReadElementContentAsInt();
			int month = reader.ReadElementContentAsInt();
			int year = reader.ReadElementContentAsInt();
			mDate = new DateTime(year, month, day);
			// read the comment if the version is greater than 0
			if (mDataVersionOfTheFileLoaded > 0)
			{
				reader.ReadToFollowing("Comment");
                mComment = reader.ReadElementContentAsString().Replace("\n", Environment.NewLine);
			}
			else
			{
				reader.ReadToFollowing("CurrentSnapGridSize");
			}
			if (mDataVersionOfTheFileLoaded < 2)
			{
				// skip the static data of layers that before we were saving
				// but now I think it is stupid, since we don't have action to change that
				// and we don't have way to update the enabled of the buttons
				reader.ReadElementContentAsFloat(); // CurrentSnapGridSize
				reader.ReadElementContentAsBoolean(); // SnapGridEnabled
				reader.ReadElementContentAsFloat(); // CurrentRotationStep
			}

            // read the export data if the version is 5 or higher
            if (mDataVersionOfTheFileLoaded > 4)
            {
                reader.ReadToDescendant("ExportPath");
                // read the relative export path and store it temporarly in the absolute path variable
                // the absolute path will be computed after the xml serialization is finished
                mExportAbsoluteFileName = reader.ReadElementContentAsString();
                // read the other export info
                mExportFileTypeIndex = reader.ReadElementContentAsInt();
                mExportArea = XmlReadWrite.readRectangleF(reader);
                mExportScale = reader.ReadElementContentAsFloat();
                // read even more info from version 8
                if (mDataVersionOfTheFileLoaded > 7)
                {
                    mExportWatermark = XmlReadWrite.readBoolean(reader);
                    mExportBrickHull = XmlReadWrite.readBoolean(reader);
                    mExportElectricCircuit = XmlReadWrite.readBoolean(reader);
                    mExportConnectionPoints = XmlReadWrite.readBoolean(reader);
                }
                reader.ReadEndElement();
            }

			// selected layer
			int selectedLayerIndex = reader.ReadElementContentAsInt();
			// step the progress bar after the read of the header
			MainForm.Instance.stepProgressBar();

			// layers
			// first clear the hashtable that contains all the bricks
            SaveLoadManager.UniqueId.ClearHashtableForLinkRebuilding();
			// then load all the layers
			bool layerFound = reader.ReadToDescendant("Layer");
			while (layerFound)
			{
				// get the 'type' attribute of the layer
				reader.ReadAttributeValue();
				string layerType = reader.GetAttribute(0);

				// instantiate the right layer according to the type
				Layer layer = null;
				if (layerType.Equals("grid"))
					layer = new LayerGrid();
				else if (layerType.Equals("brick"))
					layer = new LayerBrick();
				else if (layerType.Equals("text"))
					layer = new LayerText();
				else if (layerType.Equals("area"))
					layer = new LayerArea();
				else if (layerType.Equals("ruler"))
					layer = new LayerRuler();

				// read and add the new layer
				if (layer != null)
				{
					layer.ReadXml(reader);
					mLayers.Add(layer);
				}

				// read the next layer
				layerFound = reader.ReadToNextSibling("Layer");
			}
			reader.ReadEndElement(); // end of Layers

			// once we have finished to read all the layers thus all the items, we need to recreate all the links they have between them
			foreach (Layer layer in mLayers)
				layer.recreateLinksAfterLoading();
			// then clear again the hash table to free the memory
            SaveLoadManager.UniqueId.ClearHashtableForLinkRebuilding();

			// step the progress bar after the rebuilding of links
			MainForm.Instance.stepProgressBar();

			// if the selected index is valid, reset the selected layer
			// use the setter in order to enable the toolbar buttons
			if ((selectedLayerIndex >= 0) && (selectedLayerIndex < mLayers.Count))
				SelectedLayer = mLayers[selectedLayerIndex];
			else
				SelectedLayer = null;

			// DO NOT READ YET THE BRICK URL LIST, BECAUSE THE BRICK DOWNLOAD FEATURE IS NOT READY
			if (false)
			{
				// read the url of all the parts for version 5 or later
				if ((mDataVersionOfTheFileLoaded > 5) && !reader.IsEmptyElement)
				{
					bool urlFound = reader.ReadToDescendant("BrickUrl");
					while (urlFound)
					{
						// read the next url
						urlFound = reader.ReadToNextSibling("BrickUrl");
					}
					reader.ReadEndElement();
				}
			}

			// construct the watermark
			computeGeneralInfoWatermark();
			// for old version, make disapear the progress bar, since it was just an estimation
			MainForm.Instance.finishProgressBar();
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			// reset the counter of modifications because we just save the map
			mNumberOfModificationSinceLastSave = 0;

			// first of all the version, we don't use the vesion read from the file,
			saveVersionNumber(writer);

			// write the number of items
			int nbItems = 0;
			foreach (Layer layer in mLayers)
				nbItems += layer.NbItems;
			writer.WriteElementString("nbItems", nbItems.ToString());
			// init the progress bar
			MainForm.Instance.resetProgressBar(nbItems + 1);

			// the background color
			XmlReadWrite.writeColor(writer, "BackgroundColor", mBackgroundColor);

			// data of the map
			writer.WriteElementString("Author", mAuthor);
			writer.WriteElementString("LUG", mLUG);
			writer.WriteElementString("Show", mShow);
			writer.WriteStartElement("Date");
				writer.WriteElementString("Day", mDate.Day.ToString());
				writer.WriteElementString("Month", mDate.Month.ToString());
				writer.WriteElementString("Year", mDate.Year.ToString());
			writer.WriteEndElement();
			writer.WriteElementString("Comment", mComment);

            // the export data
            string exportRelativeFileName = computeRelativePath(mMapFileName, mExportAbsoluteFileName);
            writer.WriteStartElement("ExportInfo");
                writer.WriteElementString("ExportPath", exportRelativeFileName);
                writer.WriteElementString("ExportFileType", mExportFileTypeIndex.ToString());
				XmlReadWrite.writeRectangleF(writer, "ExportArea", mExportArea);
                writer.WriteElementString("ExportScale", mExportScale.ToString(System.Globalization.CultureInfo.InvariantCulture));
                XmlReadWrite.writeBoolean(writer, "ExportWatermark", mExportWatermark);
                XmlReadWrite.writeBoolean(writer, "ExportHull", mExportBrickHull);
                XmlReadWrite.writeBoolean(writer, "ExportElectricCircuit", mExportElectricCircuit);
                XmlReadWrite.writeBoolean(writer, "ExportConnectionPoints", mExportConnectionPoints);
            writer.WriteEndElement();

			// selected layer index
			int selectedLayerIndex = -1;
			if (mSelectedLayer != null)
				selectedLayerIndex = mLayers.IndexOf(mSelectedLayer);
			writer.WriteElementString("SelectedLayerIndex", selectedLayerIndex.ToString());

			// step the progressbar after the header
			MainForm.Instance.stepProgressBar();

			// and the layer list
			writer.WriteStartElement("Layers");
			foreach (Layer layer in mLayers)
				layer.WriteXml(writer);
			writer.WriteEndElement(); // end of Layers

			// DO NOT WRITE YET THE BRICK URL LIST, BECAUSE THE BRICK DOWNLOAD FEATURE IS NOT READY
			if (false)
			{
				// write the brick url for all the bricks in the map
				writer.WriteStartElement("BrickURLList");
				// we use a hastable for fast hash search
				System.Collections.Hashtable partList = new System.Collections.Hashtable();
				foreach (Layer layer in mLayers)
					if (layer is LayerBrick)
						foreach (LayerBrick.Brick brick in (layer as LayerBrick).BrickList)
							if (!partList.ContainsKey(brick.PartNumber))
							{
								// add the part in the hash map
								partList.Add(brick.PartNumber, null);
								// get the part from the Brick Library
								string url = BrickLibrary.Instance.getImageURL(brick.PartNumber);
								// serialize its url
								if (url != null)
								{
									writer.WriteStartElement("BrickURL");
									writer.WriteAttributeString("id", brick.PartNumber);
									writer.WriteAttributeString("official", "yes");
									writer.WriteString(url);
									writer.WriteEndElement();
								}
							}
				writer.WriteEndElement();
			}
		}

		public void saveVersionNumber(System.Xml.XmlWriter writer)
		{
			// for saving we always save with the last version of data
			writer.WriteElementString("Version", CURRENT_DATA_VERSION.ToString());
		}

		public void readVersionNumber(System.Xml.XmlReader reader)
		{
			reader.ReadToDescendant("Version");
			if (reader.Name.Equals("Version"))
				mDataVersionOfTheFileLoaded = reader.ReadElementContentAsInt();
		}
		#endregion

		#region update of map data

		public void increaseModificationCounter()
		{
			++mNumberOfModificationSinceLastSave;
		}

		public void decreaseModificationCounter()
		{
			--mNumberOfModificationSinceLastSave;
		}

        private string computeRelativePath(string fromFullPath, string toFullPath)
        {
            // both path should not be empty
            if (fromFullPath.Length == 0 || toFullPath.Length == 0)
                return Path.GetFileName(toFullPath);

            // both path must be rooted, otherwise, we cannot compute the relative path
            if (!Path.IsPathRooted(fromFullPath) || !Path.IsPathRooted(toFullPath))
                return Path.GetFileName(toFullPath);

            // if the two paths are not on the same drive this function cannot compute a relative path
            // for that we check the first letter. On Windows it will check the drive letter
            // on linux both path start with the directory separtor
            if (fromFullPath[0] != toFullPath[0])
                return Path.GetFileName(toFullPath);

            // declare an array to store all the directory separator for all platform because one file
            // save on Windows could be open on Linux and vice and versa
            char[] separatorList = new char[] { Path.DirectorySeparatorChar, '/', '\\' };

            // cut the drive letter for windows, assuming on linux the function to get the root return an empty string
            fromFullPath = fromFullPath.Remove(0, Path.GetPathRoot(fromFullPath).Length);
            toFullPath = toFullPath.Remove(0, Path.GetPathRoot(toFullPath).Length);
            // remove also the first directory separator if any
            for (int i = 0; i < separatorList.Length; ++i)
            {
                if (fromFullPath[0] == separatorList[i])
                    fromFullPath = fromFullPath.Remove(0, 1);
                if (toFullPath[0] == separatorList[i])
                    toFullPath = toFullPath.Remove(0, 1);
            }

            // finally it seems the conditions are ok to compute a relative path
            // split the two paths in a serie of folders
            string[] fromFolders = fromFullPath.Split(separatorList);
            string[] toFolders = toFullPath.Split(separatorList);

            // iterate while we find the same folders
            int divergenceStartingIndex = 0;
            int maxIndex = Math.Min(fromFolders.Length, toFolders.Length);
            for (int i = 0; i < maxIndex; ++i)
            {
                divergenceStartingIndex = i;
                if (fromFolders[i] != toFolders[i])
                    break;
            }

            // count how many times we will have to go back
            int goBackCount = fromFolders.Length - divergenceStartingIndex - 1; // -1 because the last one is the file name, not a folder

            // construct the relative path
            string relativePath = string.Empty;
            for (int i = 0; i < goBackCount; ++i)
                relativePath += @"../";
            for (int i = divergenceStartingIndex; i < toFolders.Length - 1; ++i)
                relativePath += toFolders[i] + @"/";
            relativePath += toFolders[toFolders.Length - 1];

            // return it
            return relativePath;
        }

        public void computeAbsoluteExportPathAfterLoading(string absoluteStartPath, string relativeCompletivePath)
        {
			// get the BBM file name (first before truncating it)
			string BBMFilename = Path.GetFileNameWithoutExtension(absoluteStartPath);
			// get only the directory of the BBM, remove the file name
			absoluteStartPath = Path.GetDirectoryName(absoluteStartPath);

			// check if the relative path is empty of not
			if (relativeCompletivePath == string.Empty)
			{
				// if the relative path is empty (nothing was saved in the BBM, i.e. the file was never exported)
				// just return the absolute path, changing the extension to png by default
				mExportAbsoluteFileName = Path.Combine(absoluteStartPath, BBMFilename + ".png");
				mExportFileTypeIndex = 4;
			}
			else
			{
				char[] separatorList = new char[] { Path.DirectorySeparatorChar, '/', '\\' };
				string resultPath = absoluteStartPath;

				//remove the back track path
				while (relativeCompletivePath.StartsWith(".."))
				{
					// find the last folder position
					int separatorIndex = resultPath.LastIndexOfAny(separatorList);
					// If the absolute path is too short for supporting all the backtracking of the relative path
					// just return the same path as the absolute one, with the file name of the relative one.
					if (separatorIndex == -1)
					{
						mExportAbsoluteFileName = Path.Combine(absoluteStartPath, Path.GetFileName(relativeCompletivePath));
						return;
					}
					// remove the last folder from the resulting path
					resultPath = resultPath.Remove(separatorIndex);
					// remove the two dots and the path separator
					relativeCompletivePath = relativeCompletivePath.Remove(0, 3);
				}

				// compute the path as a concatanation
				mExportAbsoluteFileName = Path.Combine(resultPath, relativeCompletivePath);
			}
        }

		public void saveExportFileSettings(string exportFileName, int exportFileTypeIndex)
		{
            // set the flag to true if there's any change. If the flag was changed previously, keep the info.
            mHasExportSettingsChanged = mHasExportSettingsChanged ||
                                        (mExportAbsoluteFileName != exportFileName) ||
                                        (mExportFileTypeIndex != exportFileTypeIndex);
            // then remember the settings
			mExportAbsoluteFileName = exportFileName;
			mExportFileTypeIndex = exportFileTypeIndex;
		}

		public void saveExportAreaAndDisplaySettings(RectangleF exportArea, double exportScale, bool exportWatermark, bool exportBrickHull, bool exportElectricCircuit, bool exportConnectionPoints)
		{
			// epsilon value to compare double values.
			const double epsilon = 0.000000001;
            // set the flag to true if there's any change. If the flag was changed previously, keep the info.
            mHasExportSettingsChanged = mHasExportSettingsChanged || 
                                        (mExportArea != exportArea) ||
										(Math.Abs(mExportScale - exportScale) > epsilon) ||
                                        (mExportWatermark != exportWatermark) ||
                                        (mExportBrickHull != exportBrickHull) ||
                                        (mExportElectricCircuit != exportElectricCircuit) ||
                                        (mExportConnectionPoints != exportConnectionPoints);
            // then remember the settings
            mExportArea = exportArea;
			mExportScale = exportScale;
		    mExportWatermark = exportWatermark;
            mExportBrickHull = exportBrickHull;
            mExportElectricCircuit = exportElectricCircuit;
            mExportConnectionPoints = exportConnectionPoints;
		}

		#endregion

		#region layer management

		/// <summary>
		/// Return the index of the specified layer in the list
		/// </summary>
		/// <param name="layer">the layer to search</param>
		/// <returns>the index</returns>
		public int getIndexOf(Layer layer)
		{
			return mLayers.IndexOf(layer);
		}

		/// <summary>
		/// Return the index just above of the selected layer.
		/// If no layer is selected, return the index after the last layer (which may be 0 if the list is empty)
		/// </summary>
		/// <returns>The index of the selected layer or the index of the last layer if no layer is selected</return>
		public int getIndexAboveTheSelectedLayer()
		{
			if (SelectedLayer != null)
				return mLayers.IndexOf(SelectedLayer) + 1;
			return mLayers.Count;
		}

		/// <summary>
		/// Add the specified layer to the map at the top of the list
		/// </summary>
		/// <param name="layerToAdd">the layer to add</param>
		public void addLayer(Layer layerToAdd)
		{
			mLayers.Add(layerToAdd);
			// select the new created layer
			SelectedLayer = layerToAdd;
			// notify the part list
			MainForm.Instance.NotifyPartListForLayerAdded(layerToAdd);
		}

		/// <summary>
		/// Add the specified layer to the map at the specified index
		/// </summary>
		/// <param name="layerToAdd">the layer to add</param>
		public void addLayer(Layer layerToAdd, int index)
		{
			// clamp the index
			if (index > mLayers.Count)
				index = mLayers.Count;
			if (index < 0)
				index = 0;
			// insert at the right place
			mLayers.Insert(index, layerToAdd);
			// select the new created layer
			SelectedLayer = layerToAdd;
			// notify the part list
			MainForm.Instance.NotifyPartListForLayerAdded(layerToAdd);
		}

		/// <summary>
		/// Remove the specified layer from the map
		/// </summary>
		/// <param name="layerToAdd">the layer to add</param>
		public void removeLayer(Layer layerToRemove)
		{
			// get the current index of the layer to remove
			int currentIndex = mLayers.IndexOf(layerToRemove);
			// remove the layer
			mLayers.Remove(layerToRemove);
			// check if it was the last layer that we removed
			if (currentIndex == mLayers.Count)
				currentIndex--;
			// select the layer with the same index to always have a layer selected, unless there's no layer anymore
			if (mLayers.Count > 0)
				SelectedLayer = mLayers[currentIndex];
			else
				SelectedLayer = null;
			// notify the part list
			MainForm.Instance.NotifyPartListForLayerRemoved(layerToRemove);
		}

		/// <summary>
		/// Move the specified layer up, in the list.
		/// </summary>
		/// <param name="layerToMove">the layer to move up</param>
		public void moveLayerUp(Layer layerToMove)
		{
			int index = mLayers.IndexOf(layerToMove);
			mLayers.RemoveAt(index);
			mLayers.Insert(++index, layerToMove);
		}

		/// <summary>
		/// Move the specified layer down, in the list.
		/// </summary>
		/// <param name="layerToMove">the layer to move up</param>
		public void moveLayerDown(Layer layerToMove)
		{
			int index = mLayers.IndexOf(layerToMove);
			mLayers.RemoveAt(index);
			mLayers.Insert(--index, layerToMove);
		}

		/// <summary>
		/// Show the specified layer. Do nothing if already shwown.
		/// </summary>
		/// <param name="layer">the layer to show</param>
		public void showLayer(Layer layer)
		{
			layer.Visible = true;
			MainForm.Instance.NotifyPartListForLayerAdded(layer);
		}

		/// <summary>
		/// Hide the specified layer. Do nothing if already hidden.
		/// </summary>
		/// <param name="layer">the layer to hide</param>
		public void hideLayer(Layer layer)
		{
			layer.Visible = false;
			MainForm.Instance.NotifyPartListForLayerRemoved(layer);
		}
		#endregion

		#region parts management
		public void giveFeedbackForNotAddingBrick(BrickAddability reason)
		{
			if ((reason == BrickAddability.YES_AND_NO_TRIMMED_BY_BUDGET) || (reason == BrickAddability.NO_BUDGET_EXCEEDED) ||
				(reason == BrickAddability.YES_AND_NO_REPLACEMENT_LIMITED_BY_BUDGET))
			{
				// beep + status message
				System.Media.SystemSounds.Beep.Play();
				BlueBrick.MainForm.Instance.setStatusBarMessage(Properties.Resources.StatusMsgBudgetReached);
				// and check if we need to display a forgetable message box (for trimmed copy it is displayed later)
				if (Properties.Settings.Default.DisplayWarningMessageForBrickNotAddedDueToBudgetLimitation && (reason == BrickAddability.NO_BUDGET_EXCEEDED))
				{
					// the flag to forget the messagebox
					bool dontDisplayMessageAgain = false;
					// display the warning message
					ForgetableMessageBox.Show(BlueBrick.MainForm.Instance, Properties.Resources.ErrorMsgCannotAddDueToBudgetLimitation,
								Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
								MessageBoxIcon.Stop, MessageBoxDefaultButton.Button1, ref dontDisplayMessageAgain);
					// set back the checkbox value in the settings (don't save the settings now, it will be done when exiting the application)
					Properties.Settings.Default.DisplayWarningMessageForBrickNotAddedDueToBudgetLimitation = !dontDisplayMessageAgain;
				}
			}
			else if (reason == BrickAddability.NO_WRONG_LAYER_TYPE)
			{
				// display a status message
				BlueBrick.MainForm.Instance.setStatusBarMessage(Properties.Resources.StatusMsgWrongLayerType);
			}
		}

		public BrickAddability canAddBrick(string partNumber)
		{
			if (Map.sInstance.SelectedLayer is LayerBrick)
			{
				if (Budget.Budget.Instance.canAddBrick(partNumber, true))
					return BrickAddability.YES;
				else
					return BrickAddability.NO_BUDGET_EXCEEDED;
			}
			return BrickAddability.NO_WRONG_LAYER_TYPE;
		}

		public void addBrick(string partNumber)
		{
			BrickAddability canAdd = canAddBrick(partNumber);
			if (canAdd == BrickAddability.YES)
				ActionManager.Instance.doAction(new AddBrick(Map.sInstance.SelectedLayer as LayerBrick, partNumber));
			else
				giveFeedbackForNotAddingBrick(canAdd);
		}

		public void addBrick(Layer.LayerItem brickOrGroup)
		{
			BrickAddability canAdd = canAddBrick(brickOrGroup.PartNumber);
			if (canAdd == BrickAddability.YES)
				ActionManager.Instance.doAction(new AddBrick(Map.sInstance.SelectedLayer as LayerBrick, brickOrGroup));
			else
				giveFeedbackForNotAddingBrick(canAdd);
		}

		public void addConnectBrick(string partNumber)
		{
			// we check if we can add brick in the other signature of this method
			addConnectBrick(partNumber, -1);
		}

		public void addConnectBrick(string partNumber, int connexion)
		{
			BrickAddability canAdd = canAddBrick(partNumber);
			if (canAdd == BrickAddability.YES)
			{
				LayerBrick brickLayer = Map.sInstance.SelectedLayer as LayerBrick;
				if (brickLayer != null)
				{
					Layer.LayerItem selectedItem = brickLayer.getSingleBrickOrGroupSelected();
					if (selectedItem != null)
					{
						// create the correct action depending if the part is a group or not
						Actions.Action action = null;
						if (BrickLibrary.Instance.isAGroup(partNumber))
							action = new AddConnectGroup(brickLayer, selectedItem, partNumber, connexion);
						else
							action = new AddConnectBrick(brickLayer, selectedItem, partNumber, connexion);
						// and add the action in the manager
						ActionManager.Instance.doAction(action);
					}
				}
			}
			else
				giveFeedbackForNotAddingBrick(canAdd);
		}

		/// <summary>
		/// Get the top left corner in stud that include the brick that is the most on the left and the
		/// brick that is the most on the top.
		/// This method only search among the brick layers, so text cells are not included.
		/// </summary>
		/// <returns>the position of the extrem top left corner</returns>
		public PointF getMostTopLeftBrickPosition()
		{
			PointF result = new PointF(float.MaxValue, float.MaxValue);
			// iterate on all the bricks of all the brick layers.
			foreach (Layer layer in mLayers)
			{
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
				{
					PointF mostTopLeft = brickLayer.getMostTopLeftBrickPosition();
					if (mostTopLeft.X < result.X)
						result.X = mostTopLeft.X;
					if (mostTopLeft.Y < result.Y)
						result.Y = mostTopLeft.Y;
				}
			}
			// check if we found any brick, else return (0,0)
			if (result.X == float.MaxValue)
				result.X = 0.0f;
			if (result.Y == float.MaxValue)
				result.Y = 0.0f;
			return result;
		}


		/// <summary>
		/// Get the top most VISIBLE brick under the mouse position.
        /// All invisible brick layers are ignored during the search, and the search is done from the top layer to the bottom.
		/// </summary>
		/// <param name="mouseCoordInStud">the coordinate of the mouse cursor, where to look for</param>
		/// <returns>the brick that is under the mouse coordinate or null if there is none.</returns>
		public LayerBrick.Brick getTopMostVisibleBrickUnderMouse(PointF mouseCoordInStud)
		{
			// iterate on all the bricks of all the brick layers.
            // but iterate on reverse order to get the top most brick layer
			for (int i = mLayers.Count - 1; i >= 0; --i)
			{
				LayerBrick brickLayer = mLayers[i] as LayerBrick;
				if ((brickLayer != null) && brickLayer.Visible)
				{
					LayerBrick.Brick topBrick = brickLayer.getBrickUnderMouse(mouseCoordInStud);
					if (topBrick != null)
						return topBrick;
				}
			}
			// nothing found under the mouse
			return null;
		}

		/// <summary>
		/// Get the total area of the map in stud that include all the bricks, and depending
		/// of the specified parameter all the text cells and all the area cells.
		/// </summary>
		/// <returns>the total area that covers all the bricks and texts</returns>
		public RectangleF getTotalAreaInStud(bool onlyBrickLayers)
		{
			PointF topLeft = new PointF(float.MaxValue, float.MaxValue);
			PointF bottomRight = new PointF(float.MinValue, float.MinValue);
			// iterate on all the bricks of all the brick layers.
			foreach (Layer layer in mLayers)
			{
				if (layer.Visible)
				{
					// layer try to convert to brick layer to filter in case we only want brick layer
					LayerBrick brickLayer = layer as LayerBrick;
					if ((!onlyBrickLayers) || (brickLayer != null))
					{
						RectangleF layerSurface = layer.getTotalAreaInStud();
						if (!layerSurface.IsEmpty)
						{
							if (layerSurface.X < topLeft.X)
								topLeft.X = layerSurface.X;
							if (layerSurface.Y < topLeft.Y)
								topLeft.Y = layerSurface.Y;
							if (layerSurface.Right > bottomRight.X)
								bottomRight.X = layerSurface.Right;
							if (layerSurface.Bottom > bottomRight.Y)
								bottomRight.Y = layerSurface.Bottom;
						}
					}
				}
			}
			// check the validity of the corner (maybe the map is empty)
			if (topLeft.X == float.MaxValue)
				topLeft.X = 0;
			if (topLeft.Y == float.MaxValue)
				topLeft.Y = 0;
			if (bottomRight.X == float.MinValue)
			{
				if (onlyBrickLayers)
					bottomRight.X = topLeft.X;
				else
					bottomRight.X = topLeft.X + 32;
			}
			if (bottomRight.Y == float.MinValue)
			{
				if (onlyBrickLayers)
					bottomRight.Y = topLeft.Y;
				else
					bottomRight.Y = topLeft.Y + 32;
			}
			return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
		}

		// clear the selection of all the layers
		public void clearAllSelection()
		{
			foreach (Layer layer in mLayers)
				layer.clearSelection();
		}

		// recompute all the pictures of all the brick of all the brick layers
		public void recomputeBrickMipmapImages()
		{
			foreach (Layer layer in mLayers)
			{
				// layer brick
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					brickLayer.recomputeBrickMipmapImages();
			}
		}

		public void editSelectedItemsProperties(PointF mouseCoordInStud)
		{
			if (Map.sInstance.SelectedLayer != null)
				Map.sInstance.SelectedLayer.editSelectedItemsProperties(mouseCoordInStud);
		}
		#endregion

		#region draw / mouse event
		/// <summary>
		/// Draw the whole map in the specified graphic context.
		/// </summary>
		/// <remarks>The map is drawn by respecting the order of the layer.</remarks>
		/// <param name="g">the graphic context in which draw the layer</param>
        /// <param name="areaInStud">The region in which we should draw</param>
        /// <param name="scalePixelPerStud">The scale to use to draw</param>
        /// <param name="drawSelectionRectangle">If true draw the selection rectangle (this can be set to false when exporting the map to an image)</param>
		public void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, bool drawSelectionRectangle)
		{
			foreach (Layer layer in mLayers)
                layer.draw(g, areaInStud, scalePixelPerStud, drawSelectionRectangle);
        }

        /// <summary>
        /// Draw the watermark info in one corner (depending on settings)
        /// </summary>
        /// <param name="g">the graphic context in which drawing the watermark</param>
        /// <param name="areaInStud">The region in which we should draw</param>
        /// <param name="scale">The scale to use to draw</param>
        public void drawWatermark(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
        {
            // draw the global info if it is enabled
            if (Properties.Settings.Default.DisplayGeneralInfoWatermark)
            {
                SizeF generalInfoSize = g.MeasureString(this.GeneralInfoWatermark, Settings.Default.WatermarkFont);
                float x = (float)(areaInStud.Width * scalePixelPerStud) - generalInfoSize.Width;
                float y = (float)(areaInStud.Height * scalePixelPerStud) - generalInfoSize.Height;
                g.FillRectangle(mWatermarkBackgroundBrush, x, y, generalInfoSize.Width, generalInfoSize.Height);
                g.DrawString(this.GeneralInfoWatermark, Settings.Default.WatermarkFont, mWatermarkBrush, x, y);
            }
		}

		/// <summary>
		/// This function is called to know if the map is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if the map wants to handle it</returns>
		public bool handleMouseDown(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			// we ask if the selected layer want to handle this mouse down event
			if (mSelectedLayer != null)
			{
				if (mSelectedLayer.handleMouseDown(e, mouseCoordInStud, ref preferedCursor))
				{
					mLayerThatHandleMouse = mSelectedLayer;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// This function is called to know if the map is interested by a mouse move without click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the map wants to handle it</returns>
		public bool handleMouseMoveWithoutClick(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			// we ask if the selected layer want to handle this mouse down event
			if (mSelectedLayer != null)
			{
				if (mSelectedLayer.handleMouseMoveWithoutClick(e, mouseCoordInStud, ref preferedCursor))
				{
					mLayerThatHandleMouse = mSelectedLayer;
					return true;
				}
				else
				{
					mLayerThatHandleMouse = null;
				}
			}
			return false;
		}

		/// <summary>
		/// This method is called if the map decided that this layer should handle
		/// this mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public bool mouseDown(MouseEventArgs e, PointF mouseCoordInStud)
		{
			// we ask if the selected layer want to handle this mouse down event
			if (mLayerThatHandleMouse != null)
				return mLayerThatHandleMouse.mouseDown(e, mouseCoordInStud);
			return false;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			if (mLayerThatHandleMouse != null)
				return mLayerThatHandleMouse.mouseMove(e, mouseCoordInStud, ref preferedCursor);
			return false;
		}

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public bool mouseUp(MouseEventArgs e, PointF mouseCoordInStud)
		{
			bool mustRefreshView = false;
			if (mLayerThatHandleMouse != null)
			{
				mustRefreshView = mLayerThatHandleMouse.mouseUp(e, mouseCoordInStud);
				mLayerThatHandleMouse = null;
			}
			return mustRefreshView;
		}

		/// <summary>
		/// This method is called when the zoom scale changed
		/// </summary>
		/// <param name="oldScaleInPixelPerStud">The previous scale</param>
		/// <param name="newScaleInPixelPerStud">The new scale</param>
		public void zoomScaleChangeNotification(double oldScaleInPixelPerStud, double newScaleInPixelPerStud)
		{
			foreach (Layer layer in mLayers)
				layer.zoomScaleChangeNotification(oldScaleInPixelPerStud, newScaleInPixelPerStud);
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public void selectInRectangle(RectangleF selectionRectangeInStud)
		{
			if (Map.sInstance.SelectedLayer != null)
				Map.sInstance.SelectedLayer.selectInRectangle(selectionRectangeInStud);
		}

		#endregion
	}
}
