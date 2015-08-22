using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;
using System.Xml;
using System.IO;

namespace BlueBrick
{
	public partial class SaveGroupNameForm : Form
	{
		private enum HintIndex
		{
			EMPTY_NAME = 0,
			HAS_FORBIDDEN_CHAR,
			CYCLIC_REFERENCE,
			EXISTING_NAME,
			OVERRIDE
		}

		// use a dictionary to store all the descriptions in every languages
		private Dictionary<string, string> mDescription = new Dictionary<string, string>();
		// store the error message in an array of string
		private string[] mErrorHint = null;
		// group to save
		private Layer.Group mGroupToSave = null;
		private bool mWasGroupToSaveCreated = false;
		// xml setting for saving
		private System.Xml.XmlWriterSettings mXmlSettings = new System.Xml.XmlWriterSettings();
		// An array of forbidden chars
		private char[] mForbiddenChar = null;
		// the resulting files to load
		private List<FileInfo> mNewXmlFilesToLoad = new List<FileInfo>();
		private List<string> mNewGroupName = new List<string>();
		// this is used to store temporarily the suffix added to the group name in order to be valid
		private string mSuffixAddedToGroupName = string.Empty;
        // and this is used to store temporarily the previous group name to check what the user has changed
        private string mPreviousGroupName = string.Empty;

		#region get/set
		public List<FileInfo> NewXmlFilesToLoad
		{
			get { return mNewXmlFilesToLoad; }
		}

		public List<string> NewGroupName
		{
			get { return mNewGroupName; }
		}

		private string Author
		{
			get
			{
				// get the default author
				if (Properties.Settings.Default.DefaultAuthor.Equals("***NotInitialized***"))
					return Properties.Resources.DefaultAuthor;
				else
					return Properties.Settings.Default.DefaultAuthor;
			}
		}
		#endregion

		#region init
		public SaveGroupNameForm()
		{
			InitializeComponent();

			// create an array of forbidden char for the group name (before setting the name of the group)
			List<char> charList = new List<char>(System.IO.Path.GetInvalidFileNameChars());
			foreach (char character in System.IO.Path.GetInvalidPathChars())
				if (!charList.Contains(character))
					charList.Add(character);
			charList.Add('&'); // the ampersome is authorized in file name, but brings trouble in xml, since it is the escape char.				
			mForbiddenChar = charList.ToArray();

			// save the error list from the text field
			char[] separator = { '|' };
			mErrorHint = this.nameErrorLabel.Text.Split(separator);

			// fill the language combo
			fillAndSelectLanguageComboBox();

			// set the author (could be overriden later)
			this.authorTextBox.Text = this.Author;
			
			// get the list of the top items
			List<Layer.LayerItem> topItems = Layer.sGetTopItemListFromList(Map.Instance.SelectedLayer.SelectedObjects);
			// fill the name if there's only one group selected
			if (topItems.Count == 1)
			{
				// if this window is called with one object, it should be normally a group
				// otherwise the save group cannot be called
				mGroupToSave = (topItems[0]) as Layer.Group;
				mWasGroupToSaveCreated = false;
				if (mGroupToSave.IsANamedGroup)
				{
					string partNumber = mGroupToSave.PartNumber;
					// set the name here and init the rest in the function
					nameTextBox.Text = partNumber;
					initControlWithPartInfo(partNumber);
				}
			}
			else
			{
				// sort the top items as on the layer
				topItems.Sort(Map.Instance.SelectedLayer.compareItemOrderOnLayer);
				// create a group temporally for the export purpose
				mGroupToSave = new Layer.Group();
				mGroupToSave.addItem(topItems);
				mWasGroupToSaveCreated = true;
			}

			// call explicitly the event to set the correct color and error message
			// after setting all the data members used to check the validity of the name
			this.nameTextBox_TextChanged(nameTextBox, null);

			// configure the xmlSetting for writing
			mXmlSettings.CheckCharacters = false;
			mXmlSettings.CloseOutput = true;
			mXmlSettings.ConformanceLevel = System.Xml.ConformanceLevel.Document;
			mXmlSettings.Encoding = new UTF8Encoding(false);
			mXmlSettings.Indent = true;
			mXmlSettings.IndentChars = "\t";
			mXmlSettings.NewLineChars = "\r\n";
			mXmlSettings.NewLineOnAttributes = false;
			mXmlSettings.OmitXmlDeclaration = false;
		}

		private void fillAndSelectLanguageComboBox()
		{
			int selectedIndex = 0;

			// add all the language names in the combobox
			languageCodeComboBox.Items.Clear();
			for (int i = 0; i < MainForm.sLanguageCodeAndName.Length; ++i)
			{
				languageCodeComboBox.Items.Add(MainForm.sLanguageCodeAndName[i].mCode);
				if (MainForm.sLanguageCodeAndName[i].mCode.Equals(BlueBrick.Properties.Settings.Default.Language))
					selectedIndex = i;
			}

			// select english by default (which will also set the language name label)
			languageCodeComboBox.SelectedIndex = selectedIndex;
		}

		private void initControlWithPartInfo(string partNumber)
		{
			// set the ungroup and sorting key
			canUngroupCheckBox.Checked = BrickLibrary.Instance.canUngroup(partNumber);
			sortingKeyTextBox.Text = BrickLibrary.Instance.getSortingKey(partNumber);
			// for the Author, check if it is the same
			string author = this.Author;
			string partAuthor = BrickLibrary.Instance.getAuthor(partNumber);
			if (!partAuthor.Contains(author))
				partAuthor = author + " & " + partAuthor;
			this.authorTextBox.Text = partAuthor;
			// get the description
			string description = BrickLibrary.Instance.getBrickInfo(partNumber)[3];
			if (description != string.Empty)
			{
				mDescription.Clear();
				mDescription.Add(BlueBrick.Properties.Settings.Default.Language, description);
			}
			// the image URL
			this.imageURLTextBox.Text = BrickLibrary.Instance.getImageURL(partNumber);
			// force the event to set the description because we are constructing the window and the focus event is skip
			descriptionTextBox_Enter(this, null);
		}
		#endregion

		#region saving
		/// <summary>
		/// All the group files are saved in the Custom folder of the part library.
		/// Given a specific name, this function return the full path.
		/// </summary>
		/// <param name="groupName">The name of the group you want to save</param>
		/// <returns>The full path of the file that will be saved, including extension</returns>
		private string getFullFileNameFromGroupName(string groupName)
		{
			string filename = PartLibraryPanel.sFullPathForCustomParts + groupName.Trim().ToUpper();
			if (!filename.EndsWith(".XML"))
				filename += ".XML";
			return filename;
		}

		private string getGroupName(string userInput)
		{
			string groupName = userInput.Trim().ToUpper();
			if (groupName.LastIndexOf('.') < 0)
				return groupName + ".SET";
			return groupName;
		}

		private string getSubGroupName(string groupName, int id)
		{
			int index = groupName.LastIndexOf('.');
			if (index >= 0)
				groupName = groupName.Substring(0, index);
			return (groupName + ".SUB" + id.ToString()); 
		}

		/// <summary>
		/// Recursive function that save one xml file for the specified group in the custom library
		/// </summary>
		/// <param name="group">The group to save</param>
		/// <param name="groupName">the name of the group that should be used to save the file</param>
		/// <param name="groupNumber">The sequential number of the group, starting with 0 for the top group</param>
		private void saveGroup(Layer.Group group, string groupName, int groupNumber)
		{
			// use a counter for the sub-groups of this group, starting from this group number + 1
			int subGroupNumber = groupNumber + 1;

			// get the position of the first item to make it the origin
			PointF origin = new PointF();
			if (group.ItemsCount > 0)
				origin = group.Items[0].Center;

			// trim and uppercase the group name and save it in the array
			groupName = groupName.Trim().ToUpper();
			mNewGroupName.Add(groupName);
			
			// get the full filename and save it in the array
			string filename = getFullFileNameFromGroupName(groupName);
			mNewXmlFilesToLoad.Add(new FileInfo(filename));

			// open the stream
			XmlWriter xmlWriter = System.Xml.XmlWriter.Create(filename, mXmlSettings);
			// start to write the header and the top node
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteStartElement("group");
			// author
			xmlWriter.WriteElementString("Author", this.authorTextBox.Text);
			// description
			xmlWriter.WriteStartElement("Description");
			foreach (KeyValuePair<string, string> keyValue in mDescription)
				xmlWriter.WriteElementString(keyValue.Key, keyValue.Value);
			xmlWriter.WriteEndElement(); // Description
			// sorting key
			xmlWriter.WriteElementString("SortingKey", this.sortingKeyTextBox.Text.Trim());
			// in library? Only the top group is in library, the other one are hidden (normal behavior)
			if (groupNumber != 0)
				xmlWriter.WriteElementString("NotListedInLibrary", "true");
			// image URL if it exists
			if (this.imageURLTextBox.Text != string.Empty)
				xmlWriter.WriteElementString("ImageURL", this.imageURLTextBox.Text);
			// can ungroup?
			if (this.canUngroupCheckBox.Checked)
				xmlWriter.WriteElementString("CanUngroup", "true");
			else
				xmlWriter.WriteElementString("CanUngroup", "false");
			// sub part list
			xmlWriter.WriteStartElement("SubPartList");
			foreach (Layer.LayerItem item in group.Items)
			{
				xmlWriter.WriteStartElement("SubPart");
				if (item.PartNumber != string.Empty)
					xmlWriter.WriteAttributeString("id", item.PartNumber);
				else
					xmlWriter.WriteAttributeString("id", getSubGroupName(groupName, subGroupNumber++));
				// position and angle
				PointF center = item.Center;
				center.X -= origin.X;
				center.Y -= origin.Y;
				XmlReadWrite.writePointFLowerCase(xmlWriter, "position", center);
				XmlReadWrite.writeFloat(xmlWriter, "angle", item.Orientation);
				// end of subpart
				xmlWriter.WriteEndElement(); // SubPart
			}
			xmlWriter.WriteEndElement(); // SubPartList
			// write the end element and close the stream
			xmlWriter.WriteEndElement(); // group
			xmlWriter.Close();

			// now iterate on all the unnamed group recursively
			// we do two iteration on the group list because we don't like to open several files at the same time
			subGroupNumber = groupNumber + 1; // reinit the counter
			foreach (Layer.LayerItem item in group.Items)
				if (item.PartNumber == string.Empty)
				{
					saveGroup(item as Layer.Group, getSubGroupName(groupName, subGroupNumber), subGroupNumber);
					subGroupNumber++;
				}
		}

		/// <summary>
		/// This method check that the specified partNumber is not used inside the hierrachy of the group
		/// in order to avoid saving a group with cyclic reference (that will generate an error at loading).
		/// This is a recursive function on the specified group
		/// </summary>
		/// <param name="partNumber">the name of the group the user wants to use to save his group</param>
		/// <returns>true if some cyclic reference is detected</returns>
		private bool isCyclicReferenceDetected(Layer.Group group, string partNumber)
		{
			foreach (Layer.LayerItem item in group.Items)
			{
				// if we find a match, return true immediatly
				if (item.PartNumber.Equals(partNumber))
					return true;
				// if it's a group, call recursively
				if (item.IsAGroup)
				{
					bool isCyclic = isCyclicReferenceDetected(item as Layer.Group, partNumber);
					// only return true if we found a match, if false continue to iterate on next item
					if (isCyclic)
						return true;
				}
			}
			// nothing detected, return false
			return false;
		}
		#endregion
		#region event handler
		private void SaveGroupNameForm_Shown(object sender, EventArgs e)
		{
			// for some reason, I cannot focus the name text box in the constructor, so focus it here.
			this.nameTextBox.Focus();
			this.nameTextBox.Select(0, 0);
		}

		private void SaveGroupNameForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// when the form is closing, destroyed the temp group created
			if (mWasGroupToSaveCreated)
				mGroupToSave.ungroup(null);
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			try
			{
				// first check if the custom folder exists and if not try to create it
				DirectoryInfo customFolder = new DirectoryInfo(PartLibraryPanel.sFullPathForCustomParts);
				if (!customFolder.Exists)
				    customFolder.Create();
				// then call the recursive function by starting to save the top group
				saveGroup(mGroupToSave, getGroupName(nameTextBox.Text), 0);
			}
			catch (Exception exception)
			{
                MessageBox.Show(Properties.Resources.ErrorMsgCannotSaveCustomPart + Environment.NewLine + exception.Message, 
								Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void nameTextBox_TextChanged(object sender, EventArgs e)
		{
			// unsubscribe the event handler to avoid being called back (make Mono crash probably on stack over flow)
			this.nameTextBox.TextChanged -= this.nameTextBox_TextChanged;

			// add a default extension if not added by user
			const string DEFAULT_SUFFIX = ".SET";
			string textOfTheUser = string.Empty;
			int indexOfDot = nameTextBox.Text.IndexOf('.');
			if (indexOfDot < 0)
			{
				// if there's no dot at all, check if the user just deleted the dot on the special case
                // of the default suffix (this can happen if he delete just the dot, or backspace all including the dot)
                if (nameTextBox.Text.EndsWith(DEFAULT_SUFFIX.Substring(1)) && mPreviousGroupName.EndsWith(DEFAULT_SUFFIX))
                    textOfTheUser = mPreviousGroupName.Remove(mPreviousGroupName.Length - DEFAULT_SUFFIX.Length);
                else
                    textOfTheUser = nameTextBox.Text;
                // anyway we add the defaut .set at the end
                mSuffixAddedToGroupName = DEFAULT_SUFFIX;
            }
            else if ((mSuffixAddedToGroupName != string.Empty) && nameTextBox.Text.EndsWith(mSuffixAddedToGroupName))
			{
				// remove the suffix (if not empty)
				textOfTheUser = nameTextBox.Text.Remove(nameTextBox.Text.Length - mSuffixAddedToGroupName.Length);
				// copy the suffix only if there's nothing or just the dot
				int suffixStartIndex = textOfTheUser.Length - indexOfDot;
				if (suffixStartIndex < 2)
					mSuffixAddedToGroupName = DEFAULT_SUFFIX.Substring(suffixStartIndex);
				else
					mSuffixAddedToGroupName = string.Empty;
			}
			else if (nameTextBox.Text.Length == indexOfDot + 1)
			{
				// if the user deleted anything after the dot, put back the suffix
				textOfTheUser = nameTextBox.Text;
				mSuffixAddedToGroupName = DEFAULT_SUFFIX.Substring(1);
			}
			else
			{
				textOfTheUser = nameTextBox.Text;
				mSuffixAddedToGroupName = string.Empty;
			}

			// reset the text with the correct colors anyway (cause the user may have modify the suffix)
			int cursorPosition = nameTextBox.SelectionStart;
			nameTextBox.Clear();
			nameTextBox.SelectionColor = Color.Black;
			nameTextBox.AppendText(textOfTheUser);
            // add the suffix in grey if it is not null
			if (mSuffixAddedToGroupName != string.Empty)
			{
				nameTextBox.SelectionColor = Color.Gray;
				nameTextBox.AppendText(mSuffixAddedToGroupName);
			}
			nameTextBox.Select(cursorPosition, 0);

            // now store the new group name, after all the modifications
            mPreviousGroupName = nameTextBox.Text;

            // construct the part number from the text in the textbox (after it has been modified)
            string partNumber = this.getGroupName(mPreviousGroupName);

			// check if the name is empty or contains any forbidden char for a file name
			bool isEmptyName = (textOfTheUser.Trim().Length == 0);
			bool hasForbiddenChar = (partNumber.IndexOfAny(mForbiddenChar) >= 0);
			bool hasCyclicReference = isCyclicReferenceDetected(mGroupToSave, partNumber);
			bool disableOkButton = (isEmptyName || hasForbiddenChar || hasCyclicReference);

			// set the corresponding error text
			if (isEmptyName)
				this.nameErrorLabel.Text = mErrorHint[(int)HintIndex.EMPTY_NAME] as string;
			else if (hasForbiddenChar)
				this.nameErrorLabel.Text = mErrorHint[(int)HintIndex.HAS_FORBIDDEN_CHAR] as string;
			else if (hasCyclicReference)
				this.nameErrorLabel.Text = mErrorHint[(int)HintIndex.CYCLIC_REFERENCE] as string;
			else
				this.nameErrorLabel.Text = string.Empty;

			// If the name is ok so far, check if the part already exists in the library
			if (!disableOkButton)
			{
				// No problem with the name, so by default set the text color in white
				nameTextBox.BackColor = Color.White;
				// Now check if the name already exist in the library
				if (BrickLibrary.Instance.isInLibrary(partNumber))
				{
					// if yes, check on the disk if the part is inside the custom lib for overriding purpose
					// which is ok, otherwise if the part is in another folder, it's forbidden
					System.IO.FileInfo fileInfo = new System.IO.FileInfo(getFullFileNameFromGroupName(partNumber));
					if (fileInfo.Exists)
					{
						nameTextBox.BackColor = Color.Gold;
						this.nameErrorLabel.Text = mErrorHint[(int)HintIndex.OVERRIDE] as string;
						initControlWithPartInfo(partNumber);
					}
					else
					{
						disableOkButton = true;
						this.nameErrorLabel.Text = mErrorHint[(int)HintIndex.EXISTING_NAME] as string;
					}
				}
			}

			// put the text box in red if there's any problem in the name
			if (disableOkButton)
				nameTextBox.BackColor = Color.DarkSalmon;

			// enable or disable the Ok button
			this.okButton.Enabled = !disableOkButton;

			// resubscribe the event handler after changing the text
			this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
		}

		private void languageCodeComboBox_TextChanged(object sender, EventArgs e)
		{
			// by default set the unknow text
			this.languageNameLabel.Text = Properties.Resources.TextUnknown;
			// search in the list of code if we can find a known one
			// if yes set the text in the language name label
			for (int i = 0; i < MainForm.sLanguageCodeAndName.Length; ++i)
				if (MainForm.sLanguageCodeAndName[i].mCode.Equals(this.languageCodeComboBox.Text))
					this.languageNameLabel.Text = MainForm.sLanguageCodeAndName[i].mName;
		}

		private void languageCodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.descriptionTextBox.Focus();
		}

		private void languageCodeComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			// change the focus on an enter or tab key
			if ((e.KeyCode == Keys.Return) || (e.KeyCode == Keys.Tab))
				this.descriptionTextBox.Focus();
			// capture all the keys in the text box, to avoid closing the window
			e.IsInputKey = true;
		}

		private void descriptionTextBox_Enter(object sender, EventArgs e)
		{
			// when the description get the focus, update the contain with the the description of the current language
			// first check in the dictionnary if we already have a text for the language code
			string description = null;
			if (mDescription.TryGetValue(this.languageCodeComboBox.Text, out description))
				descriptionTextBox.Text = description;
			else
				descriptionTextBox.Text = string.Empty;
			// place the cursor at the end of the text and deselct all
			descriptionTextBox.Select(descriptionTextBox.Text.Length, 0);
		}

		private void descriptionTextBox_Leave(object sender, EventArgs e)
		{
			string key = this.languageCodeComboBox.Text;
			// chek if we need to remove the previous value
			if (mDescription.ContainsKey(key))
				mDescription.Remove(key);
			// when leaving the description box, save the description in the dictionary
			// if it is not null, expect for the default english language
			if ((descriptionTextBox.Text.Length > 0) || key.Equals("en"))
				mDescription.Add(key, descriptionTextBox.Text);
		}
		#endregion
	}
}
