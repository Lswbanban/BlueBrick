using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BlueBrick
{
	public partial class SaveGroupNameForm : Form
	{
		// use a dictionary to store all the descriptions in every languages
		private Dictionary<string, string> mDescription = new Dictionary<string, string>();

		#region init
		public SaveGroupNameForm()
		{
			InitializeComponent();
			// fill the language combo
			fillAndSelectLanguageComboBox();
			// set the author with the default one
			if (Properties.Settings.Default.DefaultAuthor.Equals("***NotInitialized***"))
				this.authorTextBox.Text = Properties.Resources.DefaultAuthor;
			else
				this.authorTextBox.Text = Properties.Settings.Default.DefaultAuthor;
		}

		private void fillAndSelectLanguageComboBox()
		{
			// add all the language names in the combobox
			languageCodeComboBox.Items.Clear();
			for (int i = 0; i < MainForm.sLanguageCodeAndName.Length; ++i)
				languageCodeComboBox.Items.Add(MainForm.sLanguageCodeAndName[i].mCode);

			// select english by default (which will also set the language name label)
			languageCodeComboBox.SelectedIndex = 0;
		}
		#endregion

		#region event handler
		private void nameTextBox_TextChanged(object sender, EventArgs e)
		{
			// check if the name is not empty
			bool disableOkButton = (nameTextBox.Text.Length == 0);
			// TODO: check if the name already exist in the library
 			// TODO: if yes, check if the part is inside the custom lib for overiding purpose
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
