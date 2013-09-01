using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;
using System.Xml;

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
			string filename = Application.StartupPath + @"/parts/Custom/" + groupName.Trim().ToUpper();
			if (!filename.EndsWith(".XML"))
				filename += ".XML";
			return filename;
		}

		private void saveGroup()
		{
			// call the recursive function by starting to save the top group
			saveGroup(mGroupToSave, nameTextBox.Text);
		}

		/// <summary>
		/// Recursive function that save one xml file for the specified group in the custom library
		/// </summary>
		/// <param name="group">The group to save</param>
		/// <param name="groupName">the name of the group that should be used to save the file</param>
		private void saveGroup(Layer.Group group, string groupName)
		{
			string filename = getFullFileNameFromGroupName(groupName);
			XmlTextWriter xmlWriter = new XmlTextWriter(filename, new UTF8Encoding(false));

		}
		#endregion
		#region event handler
		private void SaveGroupNameForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// when the form is closing, destroyed the temp group created
			if (mWasGroupToSaveCreated)
				mGroupToSave.ungroup();
		}

		private void nameTextBox_TextChanged(object sender, EventArgs e)
		{
			// construct the part number from the text in the textbox
			string partNumber = nameTextBox.Text.Trim().ToUpper();

			// check if the name is empty or contains any forbidden char for a file name
			bool disableOkButton = ((partNumber.Length == 0) || (partNumber.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
									|| (partNumber.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0));

			// set the corresponding error text
			if (partNumber.Length == 0)
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
