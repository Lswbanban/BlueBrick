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
using System.Xml.Serialization;
using System.IO;
using BlueBrick.MapData;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;

namespace BlueBrick
{
	public static class SaveLoadManager
	{
		#region Entry point

		public static bool load(string filename)
		{
			if (File.Exists(filename))
			{
				try
				{
					string filenameLower = filename.ToLower();

					if (filenameLower.EndsWith("ldr") || filenameLower.EndsWith("dat"))
						return loadLDR(filename);
					else if (filenameLower.EndsWith("mpd"))
						return loadMDP(filename);
					else if (filenameLower.EndsWith("tdl"))
						return loadTDL(filename);
					else
						return loadBBM(filename);
				}
				catch (Exception e)
				{
					string message = Properties.Resources.ErrorMsgCannotOpenMap.Replace("&", filename);
					LoadErrorForm errorMessageDialog = new LoadErrorForm(Properties.Resources.ErrorMsgTitleError, message, e.Message);
					errorMessageDialog.ShowDialog();
					return false;
				}
			}
			return false;
		}

		public static bool save(string filename)
		{
			try
			{
				string filenameLower = filename.ToLower();

				if (filenameLower.EndsWith("ldr") || filenameLower.EndsWith("dat"))
					return saveLDR(filename);
				else if (filenameLower.EndsWith("mpd"))
					return saveMDP(filename);
				else if (filenameLower.EndsWith("tdl"))
					return saveTDL(filename);
				else
					return saveBBM(filename);
			}
			catch (Exception e)
			{
				string message = Properties.Resources.ErrorMsgCannotSaveMap.Replace("&", filename);
				LoadErrorForm errorMessageDialog = new LoadErrorForm(Properties.Resources.ErrorMsgTitleError, message, e.Message);
				errorMessageDialog.ShowDialog();
				return false;
			}
		}
		#endregion
		#region BlueBrick Format

		private static bool loadBBM(string filename)
		{
			// create a serializer to load the map
			XmlSerializer mySerializer = new XmlSerializer(typeof(Map));
			FileStream myFileStream = new FileStream(filename, FileMode.Open);
			// By default init the progress bar from the size of the file
			// and divide by an approximate size value of a brick to 
			// have an estimation of the number of brick in the file.
			// This is only usefull for files which version is under 3, because
			// from version 3 we record the number of brick in the file
			MainForm.Instance.resetProgressBar((int)(myFileStream.Length / 900));
			// parse and copy the data into this
			Map.Instance = mySerializer.Deserialize(myFileStream) as Map;
            // recompute the absolute export file name, from the relative one saved in the BBM file
            // (and save temporarly in the absolute variable during the xml parsing)
            Map.Instance.computeAbsoluteExportPathAfterLoading(filename, Map.Instance.ExportAbsoluteFileName);
			// release the file stream
			myFileStream.Close();
			myFileStream.Dispose();
			// the file can be open
			return true;
		}

		private static bool saveBBM(string filename)
		{
			// the current file name must be valid to call this function
			XmlSerializer mySerializer = new XmlSerializer(typeof(Map));
			StreamWriter myWriter = new StreamWriter(filename, false);
			mySerializer.Serialize(myWriter, Map.Instance);
			myWriter.Close();
			myWriter.Dispose();
			return true;
		}

		#endregion
		#region LDRAW Format

		// for LDraw format we need to store the current group to add because to define the group a part belongs to
		// it uses two different lines in the LDraw file.
		private static Layer.Group mLDrawCurrentGroupInWhichAdd = null;
		private static readonly string LDRAW_DATE_FORMAT_STRING = "dd/MM/yyyy";
		private static readonly string LDRAW_BB_META_COMMAND = "0 !BLUEBRICK ";
		private static readonly string LDRAW_BB_CMD_RULER = "RULER 1 ";

		private static bool loadLDR(string filename)
		{
			// clear the hashmap used to load the groups
			Layer.LayerItem.sHashtableForGroupRebuilding.Clear();
			// create a new map
			Map.Instance = new Map();
			LayerBrick currentLayer = new LayerBrick();
			// open the file
			StreamReader textReader = new StreamReader(filename);
			// init the progress bar with the number of bytes of the file
			MainForm.Instance.resetProgressBar((int)(textReader.BaseStream.Length));
			// create a line spliter array
			char[] lineSpliter = { ' ', '\t' };
			while (!textReader.EndOfStream)
			{
				// read the current line
				string line = textReader.ReadLine();
				// move the progressbar according to the number of byte read
				MainForm.Instance.stepProgressBar(line.Length);
				// split the current line
				string[] token = line.Split(lineSpliter);
				// check if the first token is 0 or 1, the other are just ignored
				if ((token[0] == "0") && (token.Length > 1))
				{
					// comment or meta command
					if (token[1].Equals("STEP"))
					{
						// new step, so add a layer (if the current layer is not empty)
						if (currentLayer.BrickList.Count > 0)
						{
							currentLayer.updateFullBrickConnectivity();
							currentLayer.sortBricksByAltitude();
							Map.Instance.addLayer(currentLayer);
							currentLayer = new LayerBrick();
						}
					}
					else
					{
						parseMetaCommandLineLDRAW(line, token, currentLayer);
					}
				}
				else if (token[0] == "1")
				{
					parseBrickLineLDRAW(token, 1, currentLayer);
				}
			}
			// close the stream
			textReader.Close();

			// add the last layer if not empty
			if (currentLayer.BrickList.Count > 0)
			{
				currentLayer.updateFullBrickConnectivity();
				currentLayer.sortBricksByAltitude();
				Map.Instance.addLayer(currentLayer);
			}

			// again clear the hashmap used to load the groups
			Layer.LayerItem.sHashtableForGroupRebuilding.Clear();

			// finish the progress bar (to hide it)
			MainForm.Instance.finishProgressBar();

			// the file can be open
			return true;
		}

		private static bool loadMDP(string filename)
		{
			// clear the hashmap used to load the groups
			Layer.LayerItem.sHashtableForGroupRebuilding.Clear();
			// create a new map
			Map.Instance = new Map();
			LayerBrick currentLayer = new LayerBrick();
			List<string> hiddenLayerNames = new List<string>();
			// open the file
			StreamReader textReader = new StreamReader(filename);
			// init the progress bar with the number of bytes of the file
			MainForm.Instance.resetProgressBar((int)(textReader.BaseStream.Length));
			// create a line spliter array
			char[] lineSpliter = { ' ', '\t' };
			while (!textReader.EndOfStream)
			{
				string line = textReader.ReadLine();
				// move the progressbar according to the number of byte read
				MainForm.Instance.stepProgressBar(line.Length);
				// split the current line
				string[] token = line.Split(lineSpliter);
				// check if the first token is 0 or 1, the other are just ignored
				if ((token[0] == "0") && (token.Length > 1))
				{
					// comment or meta command
					if (token[1].Equals("FILE"))
					{
						// new file, so add a layer (if the current layer is not empty)
						if (currentLayer.BrickList.Count > 0)
						{
							currentLayer.updateFullBrickConnectivity();
							currentLayer.sortBricksByAltitude();
							Map.Instance.addLayer(currentLayer);
							currentLayer = new LayerBrick();
						}
						// and we name the layer with the name of the sub model
						currentLayer.Name = Path.GetFileNameWithoutExtension(line.Substring(7));
					}
					else if (token[1].Equals("STEP"))
					{
						// in mdp we skip the step
					}
					else if (token[1].Equals("MLCAD") && token[2].Equals("HIDE"))
					{
						// check if it is a sub model or a simple part hidden
						string partFullName = token[17];
						if (Path.GetExtension(partFullName.ToUpperInvariant()).Equals(".LDR"))
							hiddenLayerNames.Add(Path.GetFileNameWithoutExtension(partFullName));
						else
							parseMetaCommandLineLDRAW(line, token, currentLayer);
					}
					else
					{
						parseMetaCommandLineLDRAW(line, token, currentLayer);
					}
				}
				else if (token[0] == "1")
				{
					parseBrickLineLDRAW(token, 1, currentLayer);
				}
			}
			// close the stream
			textReader.Close();

			// add the last layer if not empty
			if (currentLayer.BrickList.Count > 0)
			{
				currentLayer.updateFullBrickConnectivity();
				currentLayer.sortBricksByAltitude();
				Map.Instance.addLayer(currentLayer);
			}

			// iterate on all the layers to hide the hidden ones we found
			foreach (Layer layer in Map.Instance.LayerList)
				foreach (string hiddenLayerName in hiddenLayerNames)
					if (layer.Name.Equals(hiddenLayerName))
					{
						layer.Visible = false;
						break;
					}

			// clear again the hashmap used to load the groups
			Layer.LayerItem.sHashtableForGroupRebuilding.Clear();

			// finish the progress bar (to hide it)
			MainForm.Instance.finishProgressBar();

			// the file can be open
			return true;
		}

		private static void checkIfBrickMustBeAddedToGroup(Layer.LayerItem item)
		{
			// if the mLDrawCurrentGroupInWhichAdd is not null that means a "MLCAD BTG" was found in the previous line of the file
			// so we need to add the current brick (or group) to the group specified in the "MLCAD BTG" and then
			// we clear the reference
			if (mLDrawCurrentGroupInWhichAdd != null)
			{
				// add the item to the group
				mLDrawCurrentGroupInWhichAdd.addItem(item);
				// clear the reference, a new "MLCAD BTG" command must be find again
				mLDrawCurrentGroupInWhichAdd = null;
			}
		}

		private static string getGroupNameForSaving(Layer.Group group)
		{
			// return the part number followed by the hash code
			// the part number is needed at loading time to ask the part lib if this group can be split
			// the hash code is needed to make unique group name
			return group.PartNumber + "#" + group.GetHashCode().ToString();
		}

		private static Layer.Group createOrGetGroup(string groupName)
		{
			// look in the hastable if this group alread exists, else create it
			Layer.Group group = Layer.LayerItem.sHashtableForGroupRebuilding[groupName] as Layer.Group;
			if (group == null)
			{
				// remove the unique hash code at the end of the the group name to get only the part number
				// and ask to the part lib if it is an non ungroupable group
				bool canUngroup = true;
				int hashCodeIndex = groupName.LastIndexOf('#');
				if (hashCodeIndex > 0)
				{
					string partNumber = groupName.Substring(0, hashCodeIndex);
					if (partNumber != string.Empty)
						canUngroup = BrickLibrary.Instance.canUngroup(partNumber);
				}
				// instanciate a new group, and add it in the hash table
				group = new Layer.Group(canUngroup);
				// then add the group in the hash table
				Layer.LayerItem.sHashtableForGroupRebuilding.Add(groupName, group);
			}
			// return the group
			return group;
		}

		private static string getRemainingOfLineAfterTokenInLDRAW(string line, string token)
		{
			return line.Substring(line.IndexOf(token) + token.Length).Trim();
		}

		private static void parseDateInLDRAW(string dateToParse)
		{
			try
			{
				DateTime date = DateTime.ParseExact(dateToParse, LDRAW_DATE_FORMAT_STRING, System.Globalization.CultureInfo.InvariantCulture);
				Map.Instance.Date = date;
			}
			catch (Exception)
			{
			}
		}

		private static void parseMetaCommandLineLDRAW(string line, string[] token, LayerBrick currentLayer)
		{
			if (token[1].StartsWith("Author"))
			{
				Map.Instance.Author = getRemainingOfLineAfterTokenInLDRAW(line, token[1]);
			}
			else if (token[1].StartsWith("Lug"))
			{
				Map.Instance.LUG = getRemainingOfLineAfterTokenInLDRAW(line, token[1]);
			}
			else if (token[1].StartsWith("Event"))
			{
				Map.Instance.Show = getRemainingOfLineAfterTokenInLDRAW(line, token[1]);
			}
			else if (token[1].StartsWith("Date"))
			{
				parseDateInLDRAW(getRemainingOfLineAfterTokenInLDRAW(line, token[1]));
			}
			else if (token[1].StartsWith("//"))
			{
				if (token[2].StartsWith("LUG"))
					Map.Instance.LUG = getRemainingOfLineAfterTokenInLDRAW(line, token[2]);
				else if (token[2].StartsWith("Event"))
					Map.Instance.Show = getRemainingOfLineAfterTokenInLDRAW(line, token[2]);
				else if (token[2].StartsWith("Date"))
					parseDateInLDRAW(getRemainingOfLineAfterTokenInLDRAW(line, token[2]));
				else
				{
					// add the comment in the comment line
					string comment = Map.Instance.Comment;
					comment += line.Substring(5) + "\n";
					Map.Instance.Comment = comment;
				}
			}
			else if (token[1].Equals("MLCAD"))
			{
				if (token[2].Equals("HIDE"))
				{
					parseBrickLineLDRAW(token, 4, currentLayer);
					currentLayer.Visible = false;
				}
				else if (token[2].Equals("BTG"))
				{
					// the meta command is: 0 MLCAD BTG <group name>
					// get the group (or create it) and store it in the current group variable
					string groupName = line.Substring(line.IndexOf(token[3])).Trim();
					mLDrawCurrentGroupInWhichAdd = createOrGetGroup(groupName);
				}
			}
			else if (token[1].Equals("GROUP"))
			{
				// the meta command from MLCAD is: 0 GROUP <item count> <group name>
				string groupName = line.Substring(line.IndexOf(token[3])).Trim();
				Layer.Group group = createOrGetGroup(groupName);
				checkIfBrickMustBeAddedToGroup(group);
			}

			// skip all the rest of unknown meta commands
		}

		private static void parseBrickLineLDRAW(string[] token, int startIndex, LayerBrick currentLayer)
		{
			// only parse the DAT part, if the part is a ldr (i.e submodel, we just skip it)
			string partFullName = token[startIndex + 13];
			if (!Path.GetExtension(partFullName.ToUpperInvariant()).Equals(".DAT"))
				return;

			try
			{
				string color = token[startIndex++];
				float x = float.Parse(token[startIndex++], System.Globalization.CultureInfo.InvariantCulture);
				float y = float.Parse(token[startIndex++], System.Globalization.CultureInfo.InvariantCulture);
				float z = -float.Parse(token[startIndex++], System.Globalization.CultureInfo.InvariantCulture);
				float a = float.Parse(token[startIndex++], System.Globalization.CultureInfo.InvariantCulture);
				startIndex++; // skip b
				float c = float.Parse(token[startIndex++], System.Globalization.CultureInfo.InvariantCulture);
				startIndex += 6; // skip d f g h i j
				string partNumberWithoutColor = Path.GetFileNameWithoutExtension(partFullName).ToUpperInvariant();
				string partNumber = partNumberWithoutColor + "." + color;

				// check if it is a sleeper that we should ignore
				if (BrickLibrary.Instance.shouldBeIgnoredAtLoading(partNumber))
					return;

				// compute the orientation angle
				float angle = (float)Math.Atan2(c, a);
				// compute the angle in degree
				angle *= (float)(180.0 / Math.PI);

				// check if we have some origin conversion to do
				BrickLibrary.Brick.LDrawRemapData remapData = BrickLibrary.Instance.getLDrawRemapData(partNumber);
				if (remapData != null)
				{
					// cheat the angle
					angle -= remapData.mAngle;
					// add a shift in the good direction
					if ((remapData.mTranslation.X != 0.0f) || (remapData.mTranslation.Y != 0.0f))
					{
						Matrix rotation = new Matrix();
						rotation.Rotate(angle);
						PointF[] offset = { new PointF(-remapData.mTranslation.X, remapData.mTranslation.Y) };
						rotation.TransformVectors(offset);
						x += offset[0].X;
						z += offset[0].Y;
					}
				}

				// create a new brick
				LayerBrick.Brick brick = new LayerBrick.Brick(BrickLibrary.Instance.getActualPartNumber(partNumber));

				// rotate the brick (will recompute the correct OffsetFromOriginalImage)
				brick.Orientation = angle;

				// rescale the position because 1 stud = 20 LDU and set the center position
				x = (x / 20.0f) - brick.OffsetFromOriginalImage.X;
				z = (z / 20.0f) - brick.OffsetFromOriginalImage.Y;
				brick.Center = new PointF(x, z);
				brick.Altitude = y;

				// add the brick to the layer
				currentLayer.addBrick(brick, -1);

				// check if we need to add this brick to a group
				checkIfBrickMustBeAddedToGroup(brick);
			}
			catch (Exception)
			{
			}
		}

		private static bool saveLDR(string filename)
		{
			// init the progress bar with the number of items (+1 for init remap +1 for header)
			int nbItems = 0;
			foreach (Layer layer in Map.Instance.LayerList)
			{
				// check the type because we only save brick layers
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					nbItems += layer.NbItems;
			}
			MainForm.Instance.resetProgressBar(nbItems + 2);

			// step the progressbar after the init of part remap
			MainForm.Instance.stepProgressBar();

			StreamWriter textWriter = new StreamWriter(filename, false, new UTF8Encoding(false));
			// write the header
			saveHeaderInLDRAW(textWriter);
			// add a line break
			textWriter.WriteLine("0");
			// step the progressbar after the write of the header
			MainForm.Instance.stepProgressBar();

			// iterate on all the layers of the Map
			foreach (Layer layer in Map.Instance.LayerList)
			{
				// check the type because we only save brick layers
				if (layer is LayerBrick)
					saveBrickLayerInLDRAW(textWriter, layer as LayerBrick, true);
				else if (layer is LayerRuler)
					saveRulerLayerInLDRAW(textWriter, layer as LayerRuler, true);
				else if (layer is LayerArea)
					saveAreaLayerInLDRAW(textWriter, layer as LayerArea, true);
				else if (layer is LayerGrid)
					saveGridLayerInLDRAW(textWriter, layer as LayerGrid, true);
				else if (layer is LayerText)
					saveTextLayerInLDRAW(textWriter, layer as LayerText, true);

				// add a step to separate the layers
				textWriter.WriteLine("0 STEP");
			}
			// close the file
			textWriter.Close();
			return true;
		}

		private static bool saveMDP(string filename)
		{
			// init the progress bar with the number of items (+1 for init remap +2 for header)
			int nbItems = 0;
			foreach (Layer layer in Map.Instance.LayerList)
			{
				// check the type because we only save brick layers
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					nbItems += layer.NbItems;
			}
			MainForm.Instance.resetProgressBar(nbItems + 3);

			// step the progressbar after the init of part remap
			MainForm.Instance.stepProgressBar();

			StreamWriter textWriter = new StreamWriter(filename, false, new UTF8Encoding(false));
			// in mpd, we always start with the 0 FILE command,
			// and we start with the main model that contains all the layers
			textWriter.WriteLine("0 FILE " + Path.GetFileNameWithoutExtension(filename) + ".ldr");
			// write the header
			saveHeaderInLDRAW(textWriter);
			// step the progressbar after the write of the header
			MainForm.Instance.stepProgressBar();

			// write the list of all the layer, at the 0 position with identity matrix in black
			// the format line is like for a single part:
			// 1 <colour> x y z a b c d e f g h i <file> 
			// the black color has the code 1, the <file> is the name of the layer, replacing the space by _
			List<string> layerStandardizedNames = new List<string>(Map.Instance.LayerList.Count);
			foreach (Layer layer in Map.Instance.LayerList)
			{
				// check the type because we only save brick layers
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
				{
					// compute the clean layer name, removing white character
					string layerName = layer.Name.Trim().Replace(' ', '_');
					layerName = layerName.Replace('\t', '_');
					layerName += ".ldr";
					// write the line of the part
					string line = "";
					if (!brickLayer.Visible)
						line += "0 MLCAD HIDE ";
					line += "1 1 0 0 0 1 0 0 0 1 0 0 0 1 " + layerName;
					textWriter.WriteLine(line);
					// add the name in the list for later use
					layerStandardizedNames.Add(layerName);
				}
			}
			// step the progressbar after the write of the layer list
			MainForm.Instance.stepProgressBar();

			// add a line break
			textWriter.WriteLine("0");
			// iterate on all the layers of the Map
			int layerIndex = 0;
			foreach (Layer layer in Map.Instance.LayerList)
			{
				// check the type because we only save brick layers
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
				{
					// write the file meta command with the same name computed before for the layer
					textWriter.WriteLine("0 FILE " + layerStandardizedNames[layerIndex++]);
					// write a small header, but not the full info, just the type and author
					textWriter.WriteLine("0 Author: " + Map.Instance.Author);
					textWriter.WriteLine("0 Unofficial Model");
					textWriter.WriteLine("0");
					// write the content of the layer
					saveBrickLayerInLDRAW(textWriter, brickLayer, false);
					// add a line break
					textWriter.WriteLine("0");
				}
			}
			// close the file
			textWriter.Close();
			return true;
		}

		private static void saveHeaderInLDRAW(StreamWriter textWriter)
		{
			// the first 0 line should be the file description (so we use the file name without extension)
			textWriter.WriteLine("0 " + Path.GetFileNameWithoutExtension(Map.Instance.MapFileName));
			textWriter.WriteLine("0 Name: " + Path.GetFileName(Map.Instance.MapFileName));
			textWriter.WriteLine("0 Author: " + Map.Instance.Author);
			textWriter.WriteLine("0 Unofficial Model");
			// write other global infos of this file
			textWriter.WriteLine("0");
			textWriter.WriteLine("0 // LUG: " + Map.Instance.LUG);
			textWriter.WriteLine("0 // Event: " + Map.Instance.Show);
			textWriter.WriteLine("0 // Date: " + Map.Instance.Date.ToString(LDRAW_DATE_FORMAT_STRING, System.Globalization.CultureInfo.InvariantCulture));
			// write the comments of this map
			char[] commentSpliter = { '\r', '\n' };
			String[] commentLines = Map.Instance.Comment.Split(commentSpliter);
			foreach (string commentLine in commentLines)
				if (commentLine != string.Empty)
					textWriter.WriteLine("0 // " + commentLine);
			// add one spaced line
			textWriter.WriteLine("0");
		}

		private static void saveBrickLayerInLDRAW(StreamWriter textWriter, LayerBrick brickLayer, bool useMLCADHide)
		{
			// an array containing the spliter for part number and color
			char[] partNumberSpliter = { '.' };

			// clear the group list for saving
			Layer.Group.sListForGroupSaving.Clear();

			// check if the layer is hidden (and so we should hide all the bricks)
			bool hideBricks = useMLCADHide && !brickLayer.Visible;

			// declare a list to store all the connection point for which the sleepers where already added
			List<LayerBrick.Brick.ConnectionPoint> addedSleepers = new List<LayerBrick.Brick.ConnectionPoint>();

			// iterate on all the bricks
			foreach (LayerBrick.Brick brick in brickLayer.BrickList)
			{
				// step the progressbar for each brick (we do it at the begining because there is a continue in this loop)
				MainForm.Instance.stepProgressBar();

				// split the part name and color
				string[] partNumberAndColor = brick.PartNumber.Split(partNumberSpliter);
				string originalBrickNumber = partNumberAndColor[0];
				// skip the brick if it is a set, a logo, or a special custom part
				// so we skip all the parts that don't have a valid color number
				int intColor = 0;
				if ((partNumberAndColor.Length < 2) || !int.TryParse(partNumberAndColor[1], out intColor))
					continue;

				// compute x and y because the pair bricks doesn't have image in the library
				float x = brick.Center.X + brick.OffsetFromOriginalImage.X;
				float z = -brick.Center.Y - brick.OffsetFromOriginalImage.Y;

				// get the remap data
				BrickLibrary.Brick.LDrawRemapData remapData = BrickLibrary.Instance.getLDrawRemapData(brick.PartNumber);

				// check if we need to save another brick number instead
				if (remapData != null)
				{
					if (remapData.mAliasPartNumber != null)
						partNumberAndColor[0] = remapData.mAliasPartNumber;
					if (remapData.mAliasPartColor != null)
						partNumberAndColor[1] = remapData.mAliasPartColor;
				}

				// save the brick
				saveOneBrickInLDRAW(textWriter, brick, partNumberAndColor, x, z, remapData, hideBricks);

				// if there's no remap for this brick, just continue to next brick
				if (remapData == null)
					continue;

				// check if we need to add sleepers
				if ((remapData.mSleeperBrickNumber != null) && (brick.HasConnectionPoint))
				{
					// this is a rail brick, we need to add 2 sleepers
					// save the sleeper brick name into the part number array
					partNumberAndColor[0] = remapData.mSleeperBrickNumber;
					partNumberAndColor[1] = remapData.mSleeperBrickColor;
					// and create a temp brick for saving
					LayerBrick.Brick sleeperBrick = new LayerBrick.Brick(partNumberAndColor[0] + "." + partNumberAndColor[1]);
					sleeperBrick.Altitude = brick.Altitude;

					// get the remap data of the sleeper
					BrickLibrary.Brick.LDrawRemapData sleeperRemapData = BrickLibrary.Instance.getLDrawRemapData(sleeperBrick.PartNumber);

					// if we found the sleeper remap data, add the difference of height between the rail brick and the sleeper
					if ((sleeperRemapData != null) && (sleeperBrick.Altitude != 0.0f))
						sleeperBrick.Altitude += sleeperRemapData.mPreferredHeight - remapData.mPreferredHeight;

					// first check the connections points (the two extremity of the rail)
					int nbConnexions = brick.ConnectionPoints.Count;
					for (int i = 0; i < nbConnexions; ++i)
					{
						// get the current connexion
						LayerBrick.Brick.ConnectionPoint connexion = brick.ConnectionPoints[i];
						// if the connexion is free we need to add the sleeper
						// else if it is not free we check if we didn't already add it or if we should add it's neighboor
						// by default we will add the sleeper, and find the false cases
						bool needToAddSleeper = true;
						// first check if the connexion is not free
						if (!connexion.IsFree)
						{
							// if the track is connected we check if we didn't already added the sleeper (if yes we don't need to add it)
							if (addedSleepers.Contains(connexion.ConnectionLink))
							{
								// no need to add the sleeper, it's already done
								needToAddSleeper = false;
							}
							else
							{
								// if we didn't already add the sleeper then we need to check if the current sleeper
								// is of type gray 12V (those with clip) because if the other sleeper is a normal
								// 2x8 plate, then we don't add it, we will add the plate instead
								if (remapData.mSleeperBrickNumber.Equals("767"))
								{
									BrickLibrary.Brick.LDrawRemapData connectedBrickRemapData = BrickLibrary.Instance.getLDrawRemapData(connexion.ConnectedBrick.PartNumber);
									if ((connectedBrickRemapData != null) && (connectedBrickRemapData.mSleeperBrickNumber != null))
										needToAddSleeper = (connectedBrickRemapData.mSleeperBrickNumber.Equals("767"));
								}
							}
						}
						// now add the sleeper if we need to
						if (needToAddSleeper)
						{
							// set the correct position and orientation of the sleeper
							sleeperBrick.Orientation = brick.Orientation + BrickLibrary.Instance.getConnectionAngle(brick.PartNumber, i);
							x = connexion.PositionInStudWorldCoord.X;
							z = -connexion.PositionInStudWorldCoord.Y;
							saveOneBrickInLDRAW(textWriter, sleeperBrick, partNumberAndColor, x, z, sleeperRemapData, hideBricks);
							// add the brick to the list (to avoid adding another sleeper at the same place)
							addedSleepers.Add(connexion);
						}
					}
				}
			}

			// now save the groups if we found some
			foreach (Layer.Group group in Layer.Group.sListForGroupSaving)
				saveOneGroupInLDRAW(textWriter, group, hideBricks);
			// and clear the group list for saving
			Layer.Group.sListForGroupSaving.Clear();
		}

		private static void saveOneBrickInLDRAW(StreamWriter textWriter, LayerBrick.Brick brick, string[] partNumberAndColor, float x, float z, BrickLibrary.Brick.LDrawRemapData remapData, bool hideBricks)
		{
			// the LDRAW format for a brick is:
			// 1 <colour> x y z a b c d e f g h i <file> 
			// where x y z is the position and a b c d e f g h i is the matrix
			// in BlueBrick the color is contained in the name of the part number
			// the position must be translated from stud to LDU (1 stud = 20 LDU),
			// and the Y coord is from the altitude of the brick (0 by default)
			// The matrix is the identity, except if you rotate the part, but
			// the second line of the matrix is always the identity
			// GROUPED BRICK:
			// If the brick is part of a group, MLCAD introduced two commands.
			// The line "0 MLCAD BTG <my group name>" should be placed before the brick
			// to indicate that the brick Belongs To the Group (BTG)
			// the line "0 GROUP <Num items> <group name>" should be placed where the group
			// should appear

			// first check if this brick belongs to a group
			if (brick.Group != null)
			{
				textWriter.WriteLine("0 MLCAD BTG " + getGroupNameForSaving(brick.Group));
				// add this group to the temporary list for saving if not already done
				if (!Layer.Group.sListForGroupSaving.Contains(brick.Group))
					Layer.Group.sListForGroupSaving.Add(brick.Group);
			}

			// the position of the brick
			x *= 20;
			float y = brick.Altitude;
			z *= 20;
			// angle
			float angle = brick.Orientation;

			// check if we have some origin conversion to do
			if (remapData != null)
			{
				// add a shift in the good direction
				if ((remapData.mTranslation.X != 0.0f) || (remapData.mTranslation.Y != 0.0f))
				{
					Matrix rotation = new Matrix();
					rotation.Rotate(-angle);
					PointF[] offset = { new PointF(remapData.mTranslation.X, remapData.mTranslation.Y) };
					rotation.TransformVectors(offset);
					x += offset[0].X;
					z += offset[0].Y;
				}
				// set the height if there's not already a non null height set
				if (y == 0.0f)
					y = remapData.mPreferredHeight;
				// cheat the angle
				angle += remapData.mAngle;
			}

			// construct the line
			string line = "";
			// first the visible if we should use it
			if (hideBricks)
				line += "0 MLCAD HIDE ";
			// then the type and color
			line += "1 " + partNumberAndColor[1];
			// position
			line += " " + x.ToString(System.Globalization.CultureInfo.InvariantCulture);
			line += " " + y.ToString(System.Globalization.CultureInfo.InvariantCulture);
			line += " " + z.ToString(System.Globalization.CultureInfo.InvariantCulture);
			// matrix
			// convert the angle in radian
			angle *= (float)Math.PI / 180.0f;
			float cosAngle = (float)Math.Cos(angle);
			float sinAngle = (float)Math.Sin(angle);
			string cosAngleStr = cosAngle.ToString(System.Globalization.CultureInfo.InvariantCulture);
			string sinAngleStr = sinAngle.ToString(System.Globalization.CultureInfo.InvariantCulture);
			string minusSinAngleStr = (-sinAngle).ToString(System.Globalization.CultureInfo.InvariantCulture);
			line += " " + cosAngleStr + " 0 " + sinAngleStr;
			line += " 0 1 0";
			line += " " + minusSinAngleStr + " 0 " + cosAngleStr;
			// file
			line += " " + partNumberAndColor[0] + ".DAT";
			//write the line
			textWriter.WriteLine(line);
		}

		private static void saveOneGroupInLDRAW(StreamWriter textWriter, LayerBrick.Group group, bool hideGroup)
		{
			// the MLCAD format for a group is:
			// "0 GROUP <Num items> <group name>"
			// it should be placed where the group should appear
			// start to construct the line
			// If the brick is part of a group, MLCAD introduced two commands.
			// The line "0 MLCAD BTG <my group name>" should be placed before the brick
			// to indicate that the brick Belongs To the Group (BTG)

			// first check if this group belongs to a group
			if (group.Group != null)
				textWriter.WriteLine("0 MLCAD BTG " + getGroupNameForSaving(group.Group));

			string line = "";
			// first the visible if we should use it
			if (hideGroup)
				line += "0 MLCAD HIDE ";

			// group meta command and number of items and group unique name
			line += "0 GROUP " + group.ItemsCount.ToString() + " " + getGroupNameForSaving(group);
			//write the line
			textWriter.WriteLine(line);
		}

		private static void saveRulerLayerInLDRAW(StreamWriter textWriter, LayerRuler rulerLayer, bool useMLCADHide)
		{
			// clear the group list for saving
			Layer.Group.sListForGroupSaving.Clear();

			// check if the layer is hidden (and so we should hide all the bricks)
			bool hideRulers = useMLCADHide && !rulerLayer.Visible;

			// iterate on all the bricks
			foreach (LayerRuler.RulerItem ruler in rulerLayer.RulerList)
			{
				// step the progressbar for each brick (we do it at the begining because there is a continue in this loop)
				MainForm.Instance.stepProgressBar(); //TODO increment the bar counts
				// save the ruler
				saveOneRulerItemInLDRAW(textWriter, ruler, hideRulers);
			}

			// now save the groups if we found some
			foreach (Layer.Group group in Layer.Group.sListForGroupSaving)
				saveOneGroupInLDRAW(textWriter, group, hideRulers);
			// and clear the group list for saving
			Layer.Group.sListForGroupSaving.Clear();
		}

		private static void saveOneRulerItemInLDRAW(StreamWriter textWriter, LayerRuler.RulerItem ruler, bool hideRulers)
		{
			// the LDRAW format for a ruler is:
			// 0 !BLUEBRICK RULER <version> <type> <DisplayDistance> <DisplayUnit> <color> <GuidelineColor> <MeasureFontColor> <LineThickness> <GuidelineThickness> <GuidelineDashPattern> <Unit> <geometry> <MeasureFont>
			// where <version> is an int describing the current version of the command
			// <type> is LINEAR or CIRCULAR
			// <DisplayDistance> <DisplayUnit> are bool in form of an int 0 or 1
			// <color> <GuidelineColor> <MeasureFontColor> are colors in hex format AARRGGBB
			// <LineThickness> <GuidelineThickness> are float in pixel
			// <GuidelineDashPattern> is four float representing "space-dash" in percentage of GuidelineThickness
			// <Unit> is an int for that order
			// <geometry> depends on the type
			//		for linear:
			//		for circular:
			// <MeasureFont> is the rest of the line as as string representing the font name
			// GROUPED BRICK:
			// If the ruler is part of a group, MLCAD introduced two commands.
			// The line "0 MLCAD BTG <my group name>" should be placed before the ruler
			// to indicate that the brick Belongs To the Group (BTG)
			// the line "0 GROUP <Num items> <group name>" should be placed where the group
			// should appear

			// first check if this ruler belongs to a group
			if (ruler.Group != null)
			{
				textWriter.WriteLine("0 MLCAD BTG " + getGroupNameForSaving(ruler.Group));
				// add this group to the temporary list for saving if not already done
				if (!Layer.Group.sListForGroupSaving.Contains(ruler.Group))
					Layer.Group.sListForGroupSaving.Add(ruler.Group);
			}

			string line = "";
			// first the visible if we should use it
			if (hideRulers)
				line += "0 MLCAD HIDE ";

			// get the type
			bool isLinear = (ruler is LayerRuler.LinearRuler);

			// meta command for BB Ruler
			line += LDRAW_BB_META_COMMAND + LDRAW_BB_CMD_RULER + (isLinear ? "LINEAR " : "CIRCULAR ");
			// display options
			line += (ruler.DisplayDistance ? "1 " : "0 ");
			line += (ruler.DisplayUnit ? "1 " : "0 ");
			// color
			line += ruler.Color.ToArgb().ToString() + " ";
			line += ruler.GuidelineColor.ToArgb().ToString() + " ";
			line += ruler.MeasureColor.ToArgb().ToString() + " ";
			// line thinckness
			line += ruler.LineThickness.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
			line += ruler.GuidelineThickness.ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
			line += ruler.GuidelineDashPattern[0].ToString(System.Globalization.CultureInfo.InvariantCulture) + " " +
					ruler.GuidelineDashPattern[1].ToString(System.Globalization.CultureInfo.InvariantCulture) + " ";
			// unit
			line += ((int)(ruler.CurrentUnit)).ToString() + " ";


			// finish by the measure font
			line += ruler.MeasureFont.Name;

			//write the line
			textWriter.WriteLine(line);
		}

		private static void saveAreaLayerInLDRAW(StreamWriter textWriter, LayerArea areaLayer, bool useMLCADHide)
		{
			textWriter.WriteLine("0 // Not implemented yet, see you maybe in BB 1.9");
		}

		private static void saveGridLayerInLDRAW(StreamWriter textWriter, LayerGrid gridLayer, bool useMLCADHide)
		{
			textWriter.WriteLine("0 // Not implemented yet, see you maybe in BB 1.9");
		}

		private static void saveTextLayerInLDRAW(StreamWriter textWriter, LayerText textLayer, bool useMLCADHide)
		{
			textWriter.WriteLine("0 // Not implemented yet, see you maybe in BB 1.9");
		}
		#endregion
		#region TrackDesigner Format

		private static bool loadTDL(string filename)
		{
			// create a new map and different layer for different type of parts
			Map.Instance = new Map();
			LayerBrick baseplateLayer = new LayerBrick();
			LayerBrick rail9VLayer = new LayerBrick();
			LayerBrick monorailLayer = new LayerBrick();
			LayerBrick currentLayer = baseplateLayer;

			// declare a bool to check if we found some part not remaped in the remap file
			List<int> noRemapablePartFound = new List<int>();

			// update the registry to use
			BrickLibrary.Instance.updateCurrentTrackDesignerRegistryUsed();

			// open the file
			FileStream myFileStream = new FileStream(filename, FileMode.Open);
			BinaryReader binaryReader = new BinaryReader(myFileStream);
			// init the progress bar with the number of bytes of the file
			MainForm.Instance.resetProgressBar((int)(myFileStream.Length));

			// read the header which is 124 bytes normally without comments
			int headerSize = loadTDLHeader(binaryReader);
			// check if there was an error reading the TD header
			if (headerSize <= 0)
			{
				// finish the progressbar to hide it
				MainForm.Instance.finishProgressBar();
				binaryReader.Close();
				myFileStream.Close();
				return false;
			}

			// read the number of CTrackPieces in the list of parts
			int nbTrackPieces = binaryReader.ReadInt16();
			headerSize += 2;
			// read the header of the CTrackPieces list (containing the string "CTrackPiece")
			binaryReader.ReadChars(17);
			headerSize += 17;

			// move the progressbar according to the number of byte read
			MainForm.Instance.stepProgressBar(headerSize);

			// read until we reach the end of file
			bool endOfFile = (binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length);
			while (!endOfFile)
			{
				// The part number (that contains the class of the part) and separate the number and the class
				// The class are like that:
				//             0 = baseplate
				//       100 000 = 9Vtrain,  except 9V crossover is 1 000 100 000 and train station set is 110 000
				//       200 000 = monorail
				//       300 000 = road
				//       4x0 000 = 12V gray rail where x is 1 or 3
				//       5x0 000 = 4.5V gray rail where x is 0 or 3
				//       6x0 000 = 12V blue rail where x is 1 or 3
				//       7x0 000 = 4.5V blue rail where x is 0 or 3
				//       800 000 = road
				//       900 000 = road (green) except one part is 10 900 000
				//     1 000 000 = road (green)
				//    11 000 000 = road (green)
				// 1 0x0 000 000 = support where x is from 1 to 7
				int TDPartNumber = binaryReader.ReadInt32();

				// skip the pointer in memory (also used as an instance ID)
				binaryReader.ReadInt32();

				// read angle in degree
				double angle = binaryReader.ReadDouble();
				// read x in stud
				double x = binaryReader.ReadDouble();
				// read y in stud
				double y = binaryReader.ReadDouble();
				// skip z (also a double), the value is multiplied by 3 compared to the value displayed in TrackDesigner
				double z = binaryReader.ReadDouble();

				// le type de la pièce (an int) (0 = Straight, 1 = Left Curve, 2 = Right Curve, 3 = Left Split, 4 = Right Split, 5 = Left Merge, 6 = Right Merge, 7 = Left Join, 8 = Right Join, 9 = Crossover, 10 = T Junction, 11 = Up Ramp, 12 = Down Ramp, 13 = Short Straight, 14 = Short Left Curve In, 15 = Short Right Curve In, 16 = Short Left Curve Out, 17 = Short Right Curve Out, 18 = Left Reverse Switch, 19 = Right Reverse Switch, 20 = Custom)
				int type = binaryReader.ReadInt32();

				// the id of bitmap (an int) when a part (like a curve) has two bitmap in the TrackDesigner part library, or for two baseplase with different color
				int portIdOfOrigin = binaryReader.ReadInt32();

				// skip 4 structures for the connexion, each structure is made of:
				// - a pointer (instance ID)
				// - a int that is the Port number connected to on other piece
				// - a int that is the Port polarity: 0 unasasigned, 2 -ve, 3 +ve
				// = so it is 4*3*4 = 48 bytes
				// then skip also:
				// - a int that is flags
				// - a int that is slope 
				// + one word (2 bytes that I don't know the meaning maybe comming from the list) = 10 bytes
				// Note: the 2 bytes are not present for the last part in the stream, so for the last part we should skip less bytes
				long remainingBytesCount = binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;
				if (remainingBytesCount >= 58)
					binaryReader.ReadBytes(58);
				else
					endOfFile = true;

				// --------------

				// special case for a down ramp (TD give to the up and down ramp the same id with different bitmap id)
				// whereas in fact it is two different parts.
				if ((TDPartNumber == 232677) && (portIdOfOrigin == 1))
					TDPartNumber = 232678;

				// try to get the remap data for the BlueBrick part number
				string BBPartNumber = null;
				BrickLibrary.Brick.TDRemapData remapData = BrickLibrary.Instance.getTDRemapData(TDPartNumber, out BBPartNumber);

				// if it is a valid part, get the class of the brick to know in which layer add
				// and then create the brick and add it to the layer
				if (remapData != null)
				{
					// create a new brick
					LayerBrick.Brick brick = new LayerBrick.Brick(BBPartNumber);

					// choose the corect layer according to the type of connexion
					if (brick.HasConnectionPoint)
					{
						// this switch is hard-coded, it should be refactored
						switch (brick.ConnectionPoints[0].Type)
						{
							case 1:
								currentLayer = rail9VLayer;
								break;
							case 3:
							case 4:
								currentLayer = monorailLayer;
								break;
							default:
								currentLayer = baseplateLayer;
								break;
						}
					}
					else
					{
						currentLayer = baseplateLayer;
					}

					// check if we have to remap the connexion
					float diffAngleBtwTDandBB = 0;
					// try to get the conexion remap data
					if (portIdOfOrigin < remapData.mConnexionData.Count)
					{
						BrickLibrary.Brick.TDRemapData.ConnexionData connexion = remapData.mConnexionData[portIdOfOrigin];
						brick.ActiveConnectionPointIndex = connexion.mBBConnexionPointIndex;
						diffAngleBtwTDandBB = connexion.mDiffAngleBtwTDandBB;
					}

					// the brick with connections, use there connexion point as origin of their position
					if (brick.ConnectionPoints != null)
					{
						// set the angle of the brick first
						brick.Orientation = (float)angle + diffAngleBtwTDandBB;
						// then set the position
						brick.ActiveConnectionPosition = new PointF((float)x, (float)y);
					}
					else
					{
						// first rotate the brick to have it like in TD
						// such has the brick.Image.Width is correct
						if (diffAngleBtwTDandBB != 0)
							brick.Orientation = diffAngleBtwTDandBB;
						// if the brick don't have connexion point, the position of the TD brick be the middle
						// of the left border, but of course we need to rotate this virtual connexion point
						Matrix rotation = new Matrix();
						rotation.Rotate((float)angle);
						PointF[] originToCenter = { new PointF(brick.Width / 2, 0) };
						rotation.TransformVectors(originToCenter);
						x += originToCenter[0].X;
						y += originToCenter[0].Y;
						// set the correct angle of the brick
						brick.Orientation = (float)angle + diffAngleBtwTDandBB;
						// set the position from the center that we have computed
						brick.Center = new PointF((float)x, (float)y);
					}


					// add the brick to the layer
					currentLayer.addBrick(brick, -1);

					// special case for the monorail ramp in TD, it's only one part but in fact it is two part in LDRAW
					if ((TDPartNumber == 232677) || (TDPartNumber == 232678))
					{
						// create the second part of the ramp
						LayerBrick.Brick rampBrick = null;
						if (TDPartNumber == 232677)
						{
							rampBrick = new LayerBrick.Brick("2678.7");
							brick.ActiveConnectionPointIndex = 1;
							rampBrick.ActiveConnectionPointIndex = 0;
						}
						else
						{
							rampBrick = new LayerBrick.Brick("2677.7");
							brick.ActiveConnectionPointIndex = 0;
							rampBrick.ActiveConnectionPointIndex = 1;
						}
						// normally it should be: rampBrick.Orientation = (float)angle + diffAngleBtwTDandBB;
						rampBrick.Orientation = Actions.Bricks.AddConnectBrick.sGetOrientationOfConnectedBrick(brick, rampBrick);						
						rampBrick.ActiveConnectionPosition = brick.ActiveConnectionPosition;
						currentLayer.addBrick(rampBrick, -1);
					}
				}
				else
				{
					if (!noRemapablePartFound.Contains(TDPartNumber))
						noRemapablePartFound.Add(TDPartNumber);
				}

				// move the progressbar according to the number of byte read (each part takes 106 bytes)
				MainForm.Instance.stepProgressBar(106);
			}

			// close the stream
			binaryReader.Close();
			myFileStream.Close();

			// add the layers that are not empty
			if (baseplateLayer.BrickList.Count > 0)
			{
				baseplateLayer.Name = "Baseplate";
				baseplateLayer.updateFullBrickConnectivity();
				Map.Instance.addLayer(baseplateLayer);
			}
			if (rail9VLayer.BrickList.Count > 0)
			{
				rail9VLayer.Name = "Rail";
				rail9VLayer.updateFullBrickConnectivity();
				Map.Instance.addLayer(rail9VLayer);
			}
			if (monorailLayer.BrickList.Count > 0)
			{
				monorailLayer.Name = "Monorail";
				monorailLayer.updateFullBrickConnectivity();
				Map.Instance.addLayer(monorailLayer);
			}

			// finish the progressbar to hide it
			MainForm.Instance.finishProgressBar();

			// check if we found some part that can be remaped
			if (noRemapablePartFound.Count > 0)
			{
				string message = Properties.Resources.ErrorMsgMissingTDRemap;
				foreach (int id in noRemapablePartFound)
					message += id.ToString() + ", ";
				message = message.Remove(message.Length - 2) + ".";
				MessageBox.Show(null, message,
					Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
					MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
			}

			// the file can be open
			return true;
		}

		private static int loadTDLHeader(BinaryReader binaryReader)
		{
			// read the origin point
			int originX = binaryReader.ReadInt32();
			int originY = binaryReader.ReadInt32();
			// read the number of pieces
			int nbPieces = binaryReader.ReadInt32();
			// read the number of the file version that must be 20
			int fileVersionNumber = binaryReader.ReadInt32();
			if (fileVersionNumber != 20)
			{
				MessageBox.Show(null, Properties.Resources.ErrorMsgOldTDFile,
					Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
					MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
				return -1;
			}
			// read the boundaries of the map (in stud coord)
			int boundXMin = binaryReader.ReadInt32();
			int boundYMin = binaryReader.ReadInt32();
			int boundXMax = binaryReader.ReadInt32();
			int boundYMax = binaryReader.ReadInt32();
			// position and angle of the cursor
			double x = binaryReader.ReadDouble();
			double y = binaryReader.ReadDouble();
			double z = binaryReader.ReadDouble();
			double angle = binaryReader.ReadDouble();
			// number of the selected port (connection point) of the selected part
			int selectedPortNumber = binaryReader.ReadInt32();
			// id of the selected part
			int partId = binaryReader.ReadInt32();
			// read the size of boundaries of the map (in stud coord), normally it should be (boundXMax - boundXMin, boundYMax - boundYMin)
			int boundWidth = binaryReader.ReadInt32();
			int boundHeight = binaryReader.ReadInt32();
			// old dummy data (an empty CString)
			char dummyStringSize = binaryReader.ReadChar();
			// options flags and slopes??
			Map.Instance.AllowElectricShortCuts = (binaryReader.ReadInt32() != 0);
			int slope = binaryReader.ReadInt32();
			Map.Instance.AllowUnderground = (binaryReader.ReadInt32() != 0);
			Map.Instance.AllowSteps = (binaryReader.ReadInt32() != 0);
			Map.Instance.AllowSlopeMismatch = (binaryReader.ReadInt32() != 0);
			// description string
			char descriptionStringSize = binaryReader.ReadChar();
			Map.Instance.Show = new string(binaryReader.ReadChars(descriptionStringSize));
			// comment string
			char commentStringSize = binaryReader.ReadChar();
			Map.Instance.Comment = new string(binaryReader.ReadChars(commentStringSize));
			// now skip the piece list (normally it should be 0)
			int nbPiecesInPieceList = binaryReader.ReadInt16();
			int pieceListSize = 0;
			if (nbPiecesInPieceList > 0)
			{
				// ??? We don't support piece list, just skip them for now
				// read the header of the piece list (12 char) + all the pieces
				pieceListSize = 12 + (nbPiecesInPieceList * 40);
				binaryReader.ReadChars(pieceListSize);
			}
			// return the total of byte read
			return (105 + descriptionStringSize + commentStringSize + pieceListSize);
		}

		private static bool saveTDL(string filename)
		{
			// compute the number of brick to save
			int nbItems = 0;
			foreach (Layer layer in Map.Instance.LayerList)
			{
				// check the type because we only save brick layers
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					nbItems += layer.NbItems;
			}
			// init the progress bar with the number of parts to write
			MainForm.Instance.resetProgressBar(nbItems + 2);

			// step the progressbar after the init of part remap
			MainForm.Instance.stepProgressBar();

			// update the registry to use
			BrickLibrary.Instance.updateCurrentTrackDesignerRegistryUsed();

			// open the file
			FileStream myFileStream = new FileStream(filename, FileMode.Create);
			BinaryWriter binaryWriter = new BinaryWriter(myFileStream);

			// write the header of the file
			saveTDLHeader(binaryWriter, nbItems);

			// step the progressbar after the write of the header
			MainForm.Instance.stepProgressBar();

			// now the piece list (record the stream position because the number of item may changed if some brick were skipped)
			long streamPositionOfNbItem = binaryWriter.BaseStream.Position;
			binaryWriter.Write((short)nbItems); // the number of piece in the TrackPiece list

			if (nbItems > 0)
			{
				int nbItemsWritten = 0;

				// write the header of the CTrackPieces list (containing the string "CTrackPiece")
				binaryWriter.Write((int)0x14FFFF);
				binaryWriter.Write((short)0x0B);
				binaryWriter.Write((char[])"CTrackPiece".ToCharArray());

				// save all the bricks
				foreach (Layer layer in Map.Instance.LayerList)
				{
					// check the type because we only save brick layers
					LayerBrick brickLayer = layer as LayerBrick;
					if (brickLayer != null)
						foreach (LayerBrick.Brick brick in brickLayer.BrickList)
						{
							// save the brick
							if (saveOneBrickInTDL(binaryWriter, brick, (nbItemsWritten > 0)))
								nbItemsWritten++;
							// step the progress bar
							MainForm.Instance.stepProgressBar();
						}
				}

				// check if some items were skip, then we have to rewrite the number of items
				if (nbItemsWritten < nbItems)
				{
					binaryWriter.BaseStream.Position = 8;
					binaryWriter.Write((short)nbItemsWritten);
					binaryWriter.BaseStream.Position = streamPositionOfNbItem;
					binaryWriter.Write((short)nbItemsWritten);
				}
			}

			// close the binary writer to close the file
			binaryWriter.Close();

			// finish the progressbar to hide it
			MainForm.Instance.finishProgressBar();

			return true;
		}

		private static int getConnectedBrickOtherBBConnexionIndex(LayerBrick.Brick brick, int connexionIndexOnBrick, LayerBrick.Brick connectedBrick)
		{
			int connectedBrickOtherBBConnexionIndex = 0;
			// check if the connected brick is not null which may happen
			if (connectedBrick != null)
			{
				// get the connected connexion
				LayerBrick.Brick.ConnectionPoint connectedConnexion = brick.ConnectionPoints[connexionIndexOnBrick].ConnectionLink;
				// search the BB connexion index of the connected brick
				foreach (LayerBrick.Brick.ConnectionPoint connexion in connectedBrick.ConnectionPoints)
					if (connexion != connectedConnexion)
						connectedBrickOtherBBConnexionIndex++;
					else
						break;
			}
			return connectedBrickOtherBBConnexionIndex;
		}

		private static bool saveOneBrickInTDL(BinaryWriter binaryWriter, LayerBrick.Brick brick, bool writeSeparatorWord)
		{
			// try to get the remap data structure
			BrickLibrary.Brick.TDRemapData remapData = BrickLibrary.Instance.getTDRemapData(brick.PartNumber);

			// check if we found the correct remap data
			if (remapData != null)
			{
				// special case for a down ramp (TD give to the up and down ramp the same id with different bitmap id)
				// this part doesn't exist in TD, so we skip it.
				if (remapData.mTDId.ID == 232678)
					return false;

				// write one word (2 bytes that I don't know the meaning maybe comming from the list)
				// except for the first brick (specified with the parameter)
				if (writeSeparatorWord)
					binaryWriter.Write((ushort)0x8001);

				// write the TD part number
				binaryWriter.Write((int)remapData.mTDId.ID);

				// write an instance ID (use the hash code for that)
				binaryWriter.Write((int)brick.GetHashCode());

				// by default we use the active connection point to find the corresponding
				// port id of origin. But some parts like the straight or the cross over, can
				// have different connection point in BB but only one port id in TD because it
				// is a symetric part. So for those parts we only use the first BB connection point
				int connectionPointIndex = brick.ActiveConnectionPointIndex;
				if (!remapData.mHasSeveralPort)
					connectionPointIndex = 0;

				// check if we have to remap the connexion
				int portIdOfOrigin = -1;
				int partType = 0;
				float diffAngleBtwTDandBB = 0;
				// search the connexion point 0
				foreach (BrickLibrary.Brick.TDRemapData.ConnexionData connexion in remapData.mConnexionData)
				{
					// increment the port id
					portIdOfOrigin++;
					// check if we found the port for which the BB connexion id is equal to the current active connexion
					if (connexion.mBBConnexionPointIndex == connectionPointIndex)
					{
						partType = connexion.mType;
						diffAngleBtwTDandBB = connexion.mDiffAngleBtwTDandBB;
						break;
					}
				}

				// the brick with connections, use there connexion point as origin of their position
				double orientation = 0.0;
				PointF position;
				if ((brick.ConnectionPoints != null) && (connectionPointIndex < brick.ConnectionPoints.Count))
				{
					// set the angle of the brick first
					orientation = (double)(brick.Orientation - diffAngleBtwTDandBB);
					// then set the position by getting it from the corresponding connection point
					position = brick.ConnectionPoints[connectionPointIndex].PositionInStudWorldCoord;
				}
				else
				{
					// set the correct angle of the brick
					orientation = (double)(brick.Orientation - diffAngleBtwTDandBB);
					// set the position from the center that we have computed
					position = brick.Center;
					// first rotate the brick to have it like in TD
					// such has the brick.Image.Width is correct
					LayerBrick.Brick dummyBrickToGetWidth = new LayerBrick.Brick(brick.PartNumber);
					if (diffAngleBtwTDandBB != 0)
						dummyBrickToGetWidth.Orientation = diffAngleBtwTDandBB;
					// if the brick don't have connexion point, the position of the TD brick be the middle
					// of the left border, but of course we need to rotate this virtual connexion point
					Matrix rotation = new Matrix();
					rotation.Rotate((float)orientation);
					PointF[] originToCenter = { new PointF(dummyBrickToGetWidth.Width / 2, 0) };
					rotation.TransformVectors(originToCenter);
					position.X -= originToCenter[0].X;
					position.Y -= originToCenter[0].Y;
				}

				// normalize the rotation between 0 and 360
				while (orientation < 0.0f)
					orientation += 360.0f;
				while (orientation >= 360.0f)
					orientation -= 360.0f;
				// write the angle in degree
				binaryWriter.Write((double)orientation);

				// write the position in stud
				binaryWriter.Write((double)position.X);
				binaryWriter.Write((double)position.Y);
				binaryWriter.Write((double)brick.Altitude);

				// the type of the part (an int) (0 = Straight, 1 = Left Curve, 2 = Right Curve, 3 = Left Split, 4 = Right Split, 5 = Left Merge, 6 = Right Merge, 7 = Left Join, 8 = Right Join, 9 = Crossover, 10 = T Junction, 11 = Up Ramp, 12 = Down Ramp, 13 = Short Straight, 14 = Short Left Curve In, 15 = Short Right Curve In, 16 = Short Left Curve Out, 17 = Short Right Curve Out, 18 = Left Reverse Switch, 19 = Right Reverse Switch, 20 = Custom)
				binaryWriter.Write((int)partType);

				// the id of bitmap (an int) when a part (like a curve) has two bitmap in the TrackDesigner part library, or for two baseplase with different color
				// but in TD code, this is since as the port id (i.e. connexion id) that is used as the origin point of the part
				binaryWriter.Write((int)portIdOfOrigin);

				// 4 structures for the 4 possibles connexions
				for (int i = 0; i < 4; ++i)
				{
					int connectedBrickInstanceId = 0;
					int connectedBrickOtherPortId = 0;
					int connectedBrickPolarity = 0;

					// check if the brick has some connection point
					if ((brick.ConnectionPoints != null) && (i < remapData.mConnexionData.Count))
					{
						// get the corresponding connexion index of the BB part for the ith TD port id
						int BBConnexionIndexForI = remapData.mConnexionData[i].mBBConnexionPointIndex;
						// check if the connexion index is valid
						if (BBConnexionIndexForI < brick.ConnectionPoints.Count)
						{
							// get the polarity of the connection
							if (brick.ConnectionPoints[BBConnexionIndexForI].Polarity < 0)
								connectedBrickPolarity = 2; // in TD 2 == -ve
							else if (brick.ConnectionPoints[BBConnexionIndexForI].Polarity > 0)
								connectedBrickPolarity = 3; // in TD 3 == +ve

							// get the connected brick at the BB connexion point
							LayerBrick.Brick connectedBrick = brick.ConnectionPoints[BBConnexionIndexForI].ConnectedBrick;
							// search the BB connexion index of the connected brick
							int connectedBrickOtherBBConnexionIndex = getConnectedBrickOtherBBConnexionIndex(brick, BBConnexionIndexForI, connectedBrick);
							while (connectedBrick != null)
							{
								// special case for the down ramp that doesn't exist in TD, in fact the down ramp is merged
								// inside the up ramp. So we need to skip the down ramp and continue with the next linked part
								if (connectedBrick.PartNumber.StartsWith("2678."))
								{
									// take the other connexion point of the 2678 part (that only have 2 connexion points)
									int nextConnexionIndex = 0;
									if (connectedBrickOtherBBConnexionIndex == 0)
										nextConnexionIndex = 1;
									else
										nextConnexionIndex = 0;
									// get the next connected brick (which can be null)
									LayerBrick.Brick nextConnectedBrick = connectedBrick.ConnectionPoints[nextConnexionIndex].ConnectedBrick;
									connectedBrickOtherBBConnexionIndex = getConnectedBrickOtherBBConnexionIndex(connectedBrick, nextConnexionIndex, nextConnectedBrick);
									// continue to the next brick
									connectedBrick = nextConnectedBrick;
									continue;
								}

								// compute the instance id
								connectedBrickInstanceId = connectedBrick.GetHashCode();

								// so now connectedBrickOtherBBConnexionIndex contain the BB connexion index, and we
								// have to remap this index to the TD port id so get the remap data of the connect brick
								BrickLibrary.Brick.TDRemapData connectedRemapData = BrickLibrary.Instance.getTDRemapData(connectedBrick.PartNumber);
								// if we found the remap data of the connected brick
								if (connectedRemapData != null)
								{
									// try to find the same BB connexion index, and the TD port id is the index j in the array
									for (int j = 0; j < connectedRemapData.mConnexionData.Count; ++j)
										if (connectedBrickOtherBBConnexionIndex == connectedRemapData.mConnexionData[j].mBBConnexionPointIndex)
										{
											connectedBrickOtherPortId = j;
											break;
										}
								}

								// stop the loop
								break;
							}
						}
					}

					// write the instance ID of the first connected brick
					binaryWriter.Write((int)connectedBrickInstanceId);
					// write the Port number connected to on other piece
					binaryWriter.Write((int)connectedBrickOtherPortId);
					// write the Port polarity: 0 unasasigned, 2 -ve, 3 +ve
					binaryWriter.Write((int)connectedBrickPolarity);
				}

				// write the flags
				// TPF_ATTACHMENT     1     // piece is an attachment      
				// TPF_SUPPORT        2     // piece is used to support elevation
				// TPF_MODIFIED       4     // piece has been modified
				// TPF_CONNECTION_MADE 8    // piece has been modified
				binaryWriter.Write((int)remapData.mFlags);

				// write the slope
				binaryWriter.Write((int)0);

				// the part was correctly written
				return true;
			}

			// the remap was not found, the part was not written
			return false;
		}

		private static void saveTDLHeader(BinaryWriter binaryWriter, int nbTotalBricks)
		{
			// get the boundaries of the map
			RectangleF boundaries = Map.Instance.getTotalAreaInStud(true);
			const int margin = 5;
			boundaries.X = (int)Math.Round(boundaries.X) - margin;
			boundaries.Width = (int)Math.Round(boundaries.Width) + (margin * 2);
			boundaries.Y = (int)Math.Round(boundaries.Y) - margin;
			boundaries.Height = (int)Math.Round(boundaries.Height) + (margin * 2);

			// write the origin point (x,y)
			binaryWriter.Write((int)-boundaries.Left);
			binaryWriter.Write((int)-boundaries.Top);

			// write the number of pieces
			binaryWriter.Write((int)nbTotalBricks);

			// write the number of the file version that must be 20
			binaryWriter.Write((int)20);

			// save the boundaries
			binaryWriter.Write((int)boundaries.Left); // x min
			binaryWriter.Write((int)boundaries.Top); // y min
			binaryWriter.Write((int)boundaries.Right); // x max
			binaryWriter.Write((int)boundaries.Bottom); // y max

			// get the current selected brick if any
			LayerBrick.Brick selectedBrick = null;
			if (Map.Instance.SelectedLayer != null)
			{
				LayerBrick brickLayer = Map.Instance.SelectedLayer as LayerBrick;
				if ((brickLayer != null) && (brickLayer.SelectedObjects.Count > 0))
					selectedBrick = brickLayer.SelectedObjects[0] as LayerBrick.Brick;
			}

			// get different data to save depending if we have a selected brick or not
			PointF cursorPosition;
			double cursorPositionAltitude = 0.0;
			double cursorAngle = 0.0;
			int selectedPort = 0;
			int selectedBrickId = 0;
			if (selectedBrick != null)
			{
				cursorPositionAltitude = (double)selectedBrick.Altitude;
				selectedBrickId = selectedBrick.GetHashCode();
				if (selectedBrick.HasConnectionPoint)
				{
					// get the cursor position from the selected connection of the selected part
					cursorPosition = selectedBrick.ActiveConnectionPosition;
					cursorAngle = (double)selectedBrick.ActiveConnectionAngle;
					selectedPort = selectedBrick.ActiveConnectionPointIndex;
				}
				else
				{
					// get the cursor position from the selected brick
					cursorPosition = selectedBrick.Center;
				}
			}
			else
			{
				// get the cursor position from the left top most position (this function return (0,0) if there's no bricks)
				cursorPosition = Map.Instance.getMostTopLeftBrickPosition();
			}

			// save the cursor position
			binaryWriter.Write((double)cursorPosition.X);
			binaryWriter.Write((double)cursorPosition.Y);
			binaryWriter.Write((double)cursorPositionAltitude);
			// save the angle of the cursor
			binaryWriter.Write((double)cursorAngle);
			// selected port of the selected part
			binaryWriter.Write((int)selectedPort);
			// selected part hashcode (as an id)
			binaryWriter.Write((int)selectedBrickId);

			// size of the document
			binaryWriter.Write((int)boundaries.Width);
			binaryWriter.Write((int)boundaries.Height);
			// old dummy data (an empty CString)
			binaryWriter.Write((char)0);

			// options flags and slopes??
			binaryWriter.Write((int)(Map.Instance.AllowElectricShortCuts ? 1 : 0));
			binaryWriter.Write((int)0); // slopes, what is it ???
			binaryWriter.Write((int)(Map.Instance.AllowUnderground ? 1 : 0));
			binaryWriter.Write((int)(Map.Instance.AllowSteps ? 1 : 0));
			binaryWriter.Write((int)(Map.Instance.AllowSlopeMismatch ? 1 : 0));

			// description string (the TDL file only support a maximum number of 255 char
			// because the first char is the length of the string)
			string description = Map.Instance.Show.Clone() as string;
			if (description.Length > 255)
				description = description.Substring(0, 255);
			binaryWriter.Write((char)description.Length);
			binaryWriter.Write((char[])description.ToCharArray());
			// comment string (the TDL file only support a maximum number of 255 char
			// because the first char is the length of the string)
			string comment = Map.Instance.Comment.Clone() as string;
			if (comment.Length > 255)
				comment = description.Substring(0, 255);
			binaryWriter.Write((char)comment.Length);
			binaryWriter.Write((char[])comment.ToCharArray());

			// now the piece list (normally it's an empty list)
			binaryWriter.Write((short)0); // the number of piece in the piece list
		}

		#endregion
	}
}
