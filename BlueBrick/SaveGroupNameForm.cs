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

		#region get/set
		public List<FileInfo> NewXmlFilesToLoad
		{
			get { return mNewXmlFilesToLoad; }
		}

		public List<string> NewGroupName
		{
			get { return mNewGroupName; }
		}
		#endregion

		#region init
		public SaveGroupNameForm()
		{
			InitializeComponent();

			// set the author with the default one
			string author = string.Empty;
			if (Properties.Settings.Default.DefaultAuthor.Equals("***NotInitialized***"))
				author = Properties.Resources.DefaultAuthor;
			else
				author = Properties.Settings.Default.DefaultAuthor;
			
			// save the error list then clear the field
			char[] separator = { '|' };
			mErrorHint = this.nameErrorLabel.Text.Split(separator);
			// call explicitly the event to set the correct color and error message
			this.nameTextBox_TextChanged(this, null);

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
					// get the part number
					string partNumber = mGroupToSave.PartNumber;
					// set the name and sorting key
					nameTextBox.Text = partNumber;
					canUngroupCheckBox.Checked = mGroupToSave.CanUngroup;
					sortingKeyTextBox.Text = BrickLibrary.Instance.getSortingKey(partNumber);
					// for the Author, check if it is the same
					string partAuthor = BrickLibrary.Instance.getAuthor(partNumber);
					if (author != partAuthor)
						author += " & " + partAuthor;
					// get the description
					string description = BrickLibrary.Instance.getBrickInfo(partNumber)[3];
					if (description != string.Empty)
						mDescription.Add(BlueBrick.Properties.Settings.Default.Language, description);
				}
			}
			else
			{
				// create a group temporally for the export purpose
				mGroupToSave = new Layer.Group();
				mGroupToSave.addItem(topItems);
				mWasGroupToSaveCreated = true;
			}

			// set the author
			this.authorTextBox.Text = author;

			// fill the language combo
			fillAndSelectLanguageComboBox();

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

			// create an array of forbidden char for the group name
			List<char> charList = new List<char>(System.IO.Path.GetInvalidFileNameChars());
			foreach (char character in System.IO.Path.GetInvalidPathChars())
				if (!charList.Contains(character))
					charList.Add(character);
			charList.Add('&'); // the ampersome is authorized in file name, but brings trouble in xml, since it is the escape char.				
			mForbiddenChar = charList.ToArray();
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
			xmlWriter.WriteElementString("Author", this.authorTextBox.Text.Replace("&", "&amp;"));
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
		#endregion
		#region event handler
		private void SaveGroupNameForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// when the form is closing, destroyed the temp group created
			if (mWasGroupToSaveCreated)
				mGroupToSave.ungroup();
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
				MessageBox.Show(Properties.Resources.ErrorMsgCannotSaveCustomPart + "\n" + exception.Message, 
								Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void nameTextBox_TextChanged(object sender, EventArgs e)
		{
			// construct the part number from the text in the textbox
			string partNumber = this.getGroupName(nameTextBox.Text);

			// check if the name is empty or contains any forbidden char for a file name
			bool isEmptyName = (nameTextBox.Text.Trim().Length == 0);
			bool disableOkButton = (isEmptyName || (partNumber.IndexOfAny(mForbiddenChar) >= 0));

			// set the corresponding error text
			if (isEmptyName)
				this.nameErrorLabel.Text = mErrorHint[0] as string;
			else if (disableOkButton)
				this.nameErrorLabel.Text = mErrorHint[1] as string;
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
						this.nameErrorLabel.Text = mErrorHint[3] as string;
					}
					else
					{
						disableOkButton = true;
						this.nameErrorLabel.Text = mErrorHint[2] as string;
					}
				}
			}

			// put the text box in red if there's any problem in the name
			if (disableOkButton)
				nameTextBox.BackColor = Color.DarkSalmon;

			// enable or disable the Ok button
			this.okButton.Enabled = !disableOkButton;
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
