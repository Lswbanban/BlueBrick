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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using BlueBrick.Properties;
using BlueBrick.Actions;
using BlueBrick.Actions.Maps;
using BlueBrick.MapData;

namespace BlueBrick
{
	public partial class GlobalOptionsForm : Form
	{
		private enum ColorScheme
		{
			CUSTOM = 0,
			BLUEPRINT,
			PARCHMENT,
			CLASSIC,
		};

		private class LanguageCodeAndName
		{
			public string mCode = null; // language string code on two characters 
			public string mName = null; // language name is its own language
			public LanguageCodeAndName(string code, string name) { mCode = code; mName = name; }
		};

		// An array that contains all the language supported in the application
		private LanguageCodeAndName[] mLanguageCodeAndName = {
			new LanguageCodeAndName("en", Resources.LanguageEnglish), // DEFAULT LANGUAGE SHOULD BE FIRST
			new LanguageCodeAndName("fr", Resources.LanguageFrench),
			new LanguageCodeAndName("de", Resources.LanguageGerman),
			new LanguageCodeAndName("nl", Resources.LanguageDutch),
			new LanguageCodeAndName("pt", Resources.LanguagePortuguese),
			new LanguageCodeAndName("es", Resources.LanguageSpanish),
			new LanguageCodeAndName("it", Resources.LanguageItalian)
			//new LanguageCodeAndName("cn", Resources.LanguageChinese) //not integrated yet
		};

		// save the old value of the setting to restore the old value after a click on "Reset Default Settings" + "Cancel"
		private Settings mOldSettings = new Settings();

		// save the default string in the old language
		private string mLastDefaultAuthor = Resources.DefaultAuthor;
		private string mLastDefaultLUG = Resources.DefaultLUG;
		private string mLastDefaultShow = Resources.DefaultShow;

		// a flag to check if the user changed the part lib order
		private bool mHasPartLibOrderChanged = false;

		//save the last sorted column for the shortcut list
		private int mGlobalStatsLastColumnSorted = -1;

		// flag for the main application
		private bool mDoesNeedToRestart = false;

		#region properties
		public bool DoesNeedToRestart
		{
			get { return mDoesNeedToRestart; }
		}
		#endregion
		#region init / close
		public GlobalOptionsForm()
		{
			InitializeComponent();
			initControlValues(false);
			// save the old settings
			copySettings(mOldSettings, Settings.Default);
		}

		private void initControlValues(bool isForResetingDefaultSetting)
		{
			// init the controls

			// -- tab general
			fillAndSelectLanguageComboBox();
			this.mouseZoomCenteredCheckBox.Checked = Settings.Default.WheelMouseIsZoomOnCursor;
			this.mouseZoomSpeedNumericUpDown.Value = (Decimal)Settings.Default.WheelMouseZoomSpeed;
			fillAndSelectMultipleAndDuplicateSelectionKeyComboBox();
			this.optimComboBox.SelectedIndex = Settings.Default.StartSavedMipmapLevel;
			// new map
			GeneralInfoForm.sFillLUGComboBox(this.lugComboBox, @"/config/LugList.txt");
			GeneralInfoForm.sFillLUGComboBox(this.showComboBox, @"/config/EventList.txt");
			this.addGridLayerCheckBox.Checked = Settings.Default.AddGridLayerOnNewMap;
			this.addBrickLayerCheckBox.Checked = Settings.Default.AddBrickLayerOnNewMap;
			if (Settings.Default.DefaultAuthor.Equals("***NotInitialized***"))
				this.authorTextBox.Text = Resources.DefaultAuthor;
			else
				this.authorTextBox.Text = Settings.Default.DefaultAuthor;
			if (Settings.Default.DefaultLUG.Equals("***NotInitialized***"))
				this.lugComboBox.Text = Resources.DefaultLUG;
			else
				this.lugComboBox.Text = Settings.Default.DefaultLUG;
			if (Settings.Default.DefaultShow.Equals("***NotInitialized***"))
				this.showComboBox.Text = Resources.DefaultShow;
			else
				this.showComboBox.Text = Settings.Default.DefaultShow;
			// recent files
			this.RecentFilesNumericUpDown.Value = Settings.Default.MaxRecentFilesNum;
			this.clearRecentFilesButton.Enabled = (Settings.Default.RecentFiles.Count > 0);
			// undo
			this.undoRecordedNumericUpDown.Value = Settings.Default.UndoStackDepth;
			this.undoDisplayedNumericUpDown.Value = Settings.Default.UndoStackDisplayedDepth;

			// -- tab appearance
			this.backgroundColorPictureBox.BackColor = Settings.Default.DefaultBackgroundColor;
			this.gridColorPictureBox.BackColor = Settings.Default.DefaultGridColor;
			this.subGridColorPictureBox.BackColor = Settings.Default.DefaultSubGridColor;
			this.displayFreeConnexionPointCheckBox.Checked = Settings.Default.DisplayFreeConnexionPoints;
			this.displayGeneralInfoWatermarkCheckBox.Checked = Settings.Default.DisplayGeneralInfoWatermark;
			setGammaToNumericUpDown(this.GammaForSelectionNumericUpDown, Settings.Default.GammaForSelection);
			setGammaToNumericUpDown(this.GammaForSnappingNumericUpDown, Settings.Default.GammaForSnappingPart);
			// grid size
			this.gridSizeNumericUpDown.Value = (Decimal)Math.Min(this.gridSizeNumericUpDown.Maximum,
														Math.Max(this.gridSizeNumericUpDown.Minimum, Settings.Default.DefaultGridSize));
			this.gridSubdivisionNumericUpDown.Value = (Decimal)Math.Min(this.gridSubdivisionNumericUpDown.Maximum,
														Math.Max(this.gridSubdivisionNumericUpDown.Minimum, Settings.Default.DefaultSubDivisionNumber));
			this.gridEnabledCheckBox.Checked = Settings.Default.DefaultGridEnabled;
			this.subGridEnabledCheckBox.Checked = Settings.Default.DefaultSubGridEnabled;
			// redraw the sample box only after having set the colors
			redrawSamplePictureBox();
			// only set the color scheme after having set the colors
			fillColorSchemeComboBox();
			findCorectColorSchemeAccordingToColors();
			// font
			this.defaultFontColorPictureBox.BackColor = Settings.Default.DefaultTextColor;
			this.defaultFontNameLabel.ForeColor = Settings.Default.DefaultTextColor;
			updateChosenFont(Settings.Default.DefaultTextFont);
			// area
			this.areaTransparencyNumericUpDown.Value = (Decimal)Settings.Default.DefaultAreaTransparency;
			this.areaCellSizeNumericUpDown.Value = (Decimal)Settings.Default.DefaultAreaSize;

			// -- tab part lib
			fillPartLibraryListBox(isForResetingDefaultSetting);
			this.PartLibBackColorPictureBox.BackColor = Settings.Default.PartLibBackColor;
			this.displayPartIDCheckBox.Checked = Settings.Default.PartLibBubbleInfoPartID;
			this.displayPartColorCheckBox.Checked = Settings.Default.PartLibBubbleInfoPartColor;
			this.displayPartDescriptionCheckBox.Checked = Settings.Default.PartLibBubbleInfoPartDescription;
			this.displayBubbleInfoCheckBox.Checked = Settings.Default.PartLibDisplayBubbleInfo;

			// -- tab shortcut key
			// init the list view
			this.listViewShortcutKeys.Items.Clear();
			char[] separator = { '|' };
			foreach (string text in Settings.Default.ShortcutKey)
			{
				string[] itemNames = text.Split(separator);
				addShortcutKey(itemNames);
			}
			// fill the part combobox
			this.comboBoxPartNum.Items.AddRange(BrickLibrary.Instance.getBrickNameList());
			// init the combobox selections (the selection for connexion is set in the event handler of the selection of part num)
			this.comboBoxKey.SelectedIndex = 0;
			this.comboBoxAction.SelectedIndex = 0;
			// test if there's at list one part in the library, else an exception is raised when seting the selected index
			if (this.comboBoxPartNum.Items.Count > 0)
				this.comboBoxPartNum.SelectedIndex = 0;
			// click on the first column to add the little sign of the sort order
			listViewShortcutKeys_ColumnClick(this, new ColumnClickEventArgs(0));
		}

		private void copySettings(Settings destination, Settings source)
		{
			// general
			destination.AddBrickLayerOnNewMap = source.AddBrickLayerOnNewMap;
			destination.AddGridLayerOnNewMap = source.AddGridLayerOnNewMap;
			destination.DefaultBackgroundColor = source.DefaultBackgroundColor;
			destination.DefaultAuthor = source.DefaultAuthor.Clone() as string;
			destination.DefaultLUG = source.DefaultLUG.Clone() as string;
			destination.DefaultShow = source.DefaultShow.Clone() as string;
			destination.Language = source.Language.Clone() as string;
			destination.MouseMultipleSelectionKey = source.MouseMultipleSelectionKey;
			destination.UndoStackDepth = source.UndoStackDepth;
			destination.UndoStackDisplayedDepth = source.UndoStackDisplayedDepth;
			destination.WheelMouseIsZoomOnCursor = source.WheelMouseIsZoomOnCursor;
			destination.WheelMouseZoomSpeed = source.WheelMouseZoomSpeed;
			destination.StartSavedMipmapLevel = source.StartSavedMipmapLevel;
			destination.MaxRecentFilesNum = source.MaxRecentFilesNum;
			// appearance
			destination.DefaultAreaTransparency = source.DefaultAreaTransparency;
			destination.DefaultAreaSize = source.DefaultAreaSize;
			destination.DisplayFreeConnexionPoints = source.DisplayFreeConnexionPoints;
			destination.DisplayGeneralInfoWatermark = source.DisplayGeneralInfoWatermark;
			destination.GammaForSelection = source.GammaForSelection;
			destination.GammaForSnappingPart = source.GammaForSnappingPart;
			destination.DefaultGridColor = source.DefaultGridColor;
			destination.DefaultGridSize = source.DefaultGridSize;
			destination.DefaultGridEnabled = source.DefaultGridEnabled;
			destination.DefaultSubDivisionNumber = source.DefaultSubDivisionNumber;
			destination.DefaultSubGridEnabled = source.DefaultSubGridEnabled;
			destination.DefaultSubGridColor = source.DefaultSubGridColor;
			destination.DefaultTextColor = source.DefaultTextColor;
			destination.DefaultTextFont = source.DefaultTextFont.Clone() as Font;
			// part lib
			destination.PartLibTabOrder = new System.Collections.Specialized.StringCollection();
			foreach (string text in source.PartLibTabOrder)
				destination.PartLibTabOrder.Add(text.Clone() as string);
			destination.PartLibBackColor = source.PartLibBackColor;
			destination.PartLibBubbleInfoPartID = source.PartLibBubbleInfoPartID;
			destination.PartLibBubbleInfoPartColor = source.PartLibBubbleInfoPartColor;
			destination.PartLibBubbleInfoPartDescription = source.PartLibBubbleInfoPartDescription;
			destination.PartLibDisplayBubbleInfo = source.PartLibDisplayBubbleInfo;
			// shortcut
			destination.ShortcutKey = new System.Collections.Specialized.StringCollection();
			foreach (string text in source.ShortcutKey)
				destination.ShortcutKey.Add(text.Clone() as string);
		}

		private void buttonOk_Click(object sender, EventArgs e)
		{
			// set the values of the controls in the settings
			// -- tab general
			// language
			bool hasLanguageChanged = setLanguageSettingAccordingToComboBox();
			// if the language change, we need to restart the application
			mDoesNeedToRestart = hasLanguageChanged;
			// mouse
			Settings.Default.WheelMouseIsZoomOnCursor = this.mouseZoomCenteredCheckBox.Checked;
			Settings.Default.WheelMouseZoomSpeed = (double)this.mouseZoomSpeedNumericUpDown.Value;
			setMultipleAndDuplicateSelectionKeySettingAccordingToComboBox();
			// new map
			Settings.Default.AddGridLayerOnNewMap = this.addGridLayerCheckBox.Checked;
			Settings.Default.AddBrickLayerOnNewMap = this.addBrickLayerCheckBox.Checked;
			Settings.Default.DefaultAuthor = this.authorTextBox.Text;
			Settings.Default.DefaultLUG = this.lugComboBox.Text;
			Settings.Default.DefaultShow = this.showComboBox.Text;
			// recent files
			Settings.Default.MaxRecentFilesNum = (int)this.RecentFilesNumericUpDown.Value;
			// undo
			Settings.Default.UndoStackDepth = (int)this.undoRecordedNumericUpDown.Value;
			Settings.Default.UndoStackDisplayedDepth = (int)this.undoDisplayedNumericUpDown.Value;
			// performances
			if (setOptimSettingAccordingToComboBox())
			{
				// set the wait cursor because the recreation of image can take a long time
				this.Cursor = Cursors.WaitCursor;
				Map.Instance.recomputeBrickMipmapImages();
				this.Cursor = Cursors.Default;
			}

			// -- tab appearance
			// check if the user changed the grid color
			bool doesGridColorChanged = (mOldSettings.DefaultBackgroundColor != backgroundColorPictureBox.BackColor) ||
										(mOldSettings.DefaultGridColor != gridColorPictureBox.BackColor) ||
										(mOldSettings.DefaultSubGridColor != subGridColorPictureBox.BackColor);
			Settings.Default.DefaultBackgroundColor = backgroundColorPictureBox.BackColor;
			Settings.Default.DefaultGridColor = gridColorPictureBox.BackColor;
			Settings.Default.DefaultSubGridColor = subGridColorPictureBox.BackColor;
			Settings.Default.DisplayFreeConnexionPoints = this.displayFreeConnexionPointCheckBox.Checked;
			Settings.Default.DisplayGeneralInfoWatermark = this.displayGeneralInfoWatermarkCheckBox.Checked;
			Settings.Default.GammaForSelection = getGammaFromNumericUpDown(this.GammaForSelectionNumericUpDown);
			Settings.Default.GammaForSnappingPart = getGammaFromNumericUpDown(this.GammaForSnappingNumericUpDown);
			// font
			bool doesFontChanged = (!mOldSettings.DefaultTextFont.Equals(defaultFontNameLabel.Font)) ||
									(mOldSettings.DefaultTextColor != defaultFontColorPictureBox.BackColor);
			Settings.Default.DefaultTextFont = this.defaultFontNameLabel.Font;
			Settings.Default.DefaultTextColor = this.defaultFontColorPictureBox.BackColor;
			// grid size
			bool isSizeModified = (mOldSettings.DefaultGridSize != (int)this.gridSizeNumericUpDown.Value) ||
									(mOldSettings.DefaultSubDivisionNumber != (int)this.gridSubdivisionNumericUpDown.Value) ||
									(mOldSettings.DefaultGridEnabled != this.gridEnabledCheckBox.Checked) ||
									(mOldSettings.DefaultSubGridEnabled != this.subGridEnabledCheckBox.Checked);
			Settings.Default.DefaultGridSize = (int)this.gridSizeNumericUpDown.Value;
			Settings.Default.DefaultSubDivisionNumber = (int)this.gridSubdivisionNumericUpDown.Value;
			Settings.Default.DefaultGridEnabled = this.gridEnabledCheckBox.Checked;
			Settings.Default.DefaultSubGridEnabled = this.subGridEnabledCheckBox.Checked;

			//area
			bool doesAreaChanged = (mOldSettings.DefaultAreaTransparency != (int)this.areaTransparencyNumericUpDown.Value) ||
									(mOldSettings.DefaultAreaSize != (int)this.areaCellSizeNumericUpDown.Value);
			Settings.Default.DefaultAreaTransparency = (int)this.areaTransparencyNumericUpDown.Value;
			Settings.Default.DefaultAreaSize = (int)this.areaCellSizeNumericUpDown.Value;

			// if the grid color changed, we prompt a dialog box to ask if we need to apply the change on all the grid layer
			if (doesGridColorChanged || doesFontChanged || isSizeModified || doesAreaChanged)
			{
				DialogResult result = MessageBox.Show(this,
					Resources.ErrorMsgApplyNewSettingsToAllLayers,
					Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
				if (result == DialogResult.Yes)
				{
					ActionManager.Instance.doAction(new ChangeMapAppearance(doesGridColorChanged, doesFontChanged, isSizeModified, doesAreaChanged));
				}				
			}

			// -- tab PartLib
			savePartLibraryTabOrder();
			bool doesAppearanceChanged = (Settings.Default.PartLibBackColor != this.PartLibBackColorPictureBox.BackColor) ||
										(Settings.Default.PartLibDisplayBubbleInfo != this.displayBubbleInfoCheckBox.Checked);
			bool doesBubbleInfoChanged = (Settings.Default.PartLibBubbleInfoPartID != this.displayPartIDCheckBox.Checked) ||
										(Settings.Default.PartLibBubbleInfoPartColor != this.displayPartColorCheckBox.Checked) ||
										(Settings.Default.PartLibBubbleInfoPartDescription != this.displayPartDescriptionCheckBox.Checked);
			Settings.Default.PartLibBackColor = this.PartLibBackColorPictureBox.BackColor;
			Settings.Default.PartLibBubbleInfoPartID = this.displayPartIDCheckBox.Checked;
			Settings.Default.PartLibBubbleInfoPartColor = this.displayPartColorCheckBox.Checked;
			Settings.Default.PartLibBubbleInfoPartDescription = this.displayPartDescriptionCheckBox.Checked;
			Settings.Default.PartLibDisplayBubbleInfo = this.displayBubbleInfoCheckBox.Checked;
			// call the function on the part lib to reflect the change
			BlueBrick.MainForm.Instance.PartsTabControl.updateAppearanceAccordingToSettings(mHasPartLibOrderChanged, doesAppearanceChanged, doesBubbleInfoChanged);

			// -- tab shortcut key
			// save the list view
			Settings.Default.ShortcutKey = new System.Collections.Specialized.StringCollection();
			foreach (ListViewItem item in this.listViewShortcutKeys.Items)
			{
				string text = item.SubItems[0].Tag + "|" + item.SubItems[1].Tag + "|" + item.SubItems[2].Text + "|" + item.SubItems[3].Text;
				Settings.Default.ShortcutKey.Add(text);
			}

			//save the settings and close the window
			Settings.Default.Save();
		}

		private void restoreDefaultButton_Click(object sender, EventArgs e)
		{
			DialogResult result = MessageBox.Show(this,
				Resources.ErrorMsgConfirmRestoreDefault,
				Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
				MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (result == DialogResult.Yes)
			{
				// reset the settings (except the language)
				string currentLanguage = Settings.Default.Language;
				Settings.Default.Upgrade();
				Settings.Default.Reset();
				// restore the language
				Settings.Default.Language = currentLanguage;
				// init the controls
				initControlValues(true);
			}
		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			// copy back the original setting that may have changed if the user clicked on the restore button
			copySettings(Settings.Default, mOldSettings);
		}
		#endregion

		#region tab general

		private string getLanguageStringAccordingToComboBox()
		{
			// return the language string according to the selected index
			if (languageComboBox.SelectedIndex < mLanguageCodeAndName.Length)
				return mLanguageCodeAndName[languageComboBox.SelectedIndex].mCode;
			// return the default language
			return mLanguageCodeAndName[0].mCode;
		}

		private bool setLanguageSettingAccordingToComboBox()
		{
			// get the new language
			string newLanguage = getLanguageStringAccordingToComboBox();
			// set the new language
			Settings.Default.Language = newLanguage;
			// and return true if the language changed
			return (!mOldSettings.Language.Equals(newLanguage));
		}

		private void fillAndSelectLanguageComboBox()
		{
			// by default select the first index
			int selectedIndex = 0;

			// add all the language names in the combobox
			languageComboBox.Items.Clear();
			for (int i = 0; i < mLanguageCodeAndName.Length; ++i)
			{
				// add the language name in the combobox
				languageComboBox.Items.Add(mLanguageCodeAndName[i].mName);
				// check if this is the current selected one
				if (Settings.Default.Language.Equals(mLanguageCodeAndName[i].mCode))
					selectedIndex = i;
			}

			// select the correct index
			languageComboBox.SelectedIndex = selectedIndex;
		}

		private void setMultipleAndDuplicateSelectionKeySettingAccordingToComboBox()
		{
			// Multiple selection
			switch (this.mouseMultipleSelKeyComboBox.SelectedIndex)
			{
				case 0: Settings.Default.MouseMultipleSelectionKey = Keys.Control; break;
				case 1: Settings.Default.MouseMultipleSelectionKey = Keys.Alt; break;
				case 2: Settings.Default.MouseMultipleSelectionKey = Keys.Shift; break;
			}

			// Duplicate
			switch (this.mouseDuplicateSelKeyComboBox.SelectedIndex)
			{
				case 0: Settings.Default.MouseDuplicateSelectionKey = Keys.Control; break;
				case 1: Settings.Default.MouseDuplicateSelectionKey = Keys.Alt; break;
				case 2: Settings.Default.MouseDuplicateSelectionKey = Keys.Shift; break;
			}
		}

		private void languageComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// get the new language
			string newLanguage = getLanguageStringAccordingToComboBox();
			// change the culture of the resource to get the string in the correct language
			// create a new culture info based on the property
			System.Globalization.CultureInfo previousCultureInfo = Resources.Culture;
			Resources.Culture = new System.Globalization.CultureInfo(newLanguage);
			// check if we need to replace the default string of the old language with the
			// default string of the new language
			if (this.authorTextBox.Text.Equals(mLastDefaultAuthor))
			{
				this.authorTextBox.Text = Resources.DefaultAuthor;
				mLastDefaultAuthor = Resources.DefaultAuthor;
			}
			if (this.lugComboBox.Text.Equals(mLastDefaultLUG))
			{
				this.lugComboBox.Text = Resources.DefaultLUG;
				mLastDefaultLUG = Resources.DefaultLUG;
			}
			if (this.showComboBox.Text.Equals(mLastDefaultShow))
			{
				this.showComboBox.Text = Resources.DefaultShow;
				mLastDefaultShow = Resources.DefaultShow;
			}
			// and restore the previous culture (to avoid partially translated software before the restart)
			Resources.Culture = previousCultureInfo;
		}

		private void mouseDuplicateSelKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// avoid to have the multiple selection and the duplicate on the same key
			if (this.mouseDuplicateSelKeyComboBox.SelectedIndex == this.mouseMultipleSelKeyComboBox.SelectedIndex)
			{
				if (this.mouseMultipleSelKeyComboBox.SelectedIndex == 2)
					this.mouseMultipleSelKeyComboBox.SelectedIndex = 0;
				else
					this.mouseMultipleSelKeyComboBox.SelectedIndex = this.mouseMultipleSelKeyComboBox.SelectedIndex + 1;
			}
		}

		private void mouseMultipleSelKeyComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			// avoid to have the multiple selection and the duplicate on the same key
			if (this.mouseMultipleSelKeyComboBox.SelectedIndex == this.mouseDuplicateSelKeyComboBox.SelectedIndex)
			{
				if (this.mouseDuplicateSelKeyComboBox.SelectedIndex == 2)
					this.mouseDuplicateSelKeyComboBox.SelectedIndex = 0;
				else
					this.mouseDuplicateSelKeyComboBox.SelectedIndex = this.mouseDuplicateSelKeyComboBox.SelectedIndex + 1;
			}
		}

		private void fillAndSelectMultipleAndDuplicateSelectionKeyComboBox()
		{
			// select the correct index for multiple selection
			switch (Settings.Default.MouseMultipleSelectionKey)
			{
				case Keys.Control: this.mouseMultipleSelKeyComboBox.SelectedIndex = 0; break;
				case Keys.Alt: this.mouseMultipleSelKeyComboBox.SelectedIndex = 1; break;
				case Keys.Shift: this.mouseMultipleSelKeyComboBox.SelectedIndex = 2; break;
			}

			// select the correct index for duplicate
			switch (Settings.Default.MouseDuplicateSelectionKey)
			{
				case Keys.Control: this.mouseDuplicateSelKeyComboBox.SelectedIndex = 0; break;
				case Keys.Alt: this.mouseDuplicateSelKeyComboBox.SelectedIndex = 1; break;
				case Keys.Shift: this.mouseDuplicateSelKeyComboBox.SelectedIndex = 2; break;
			}
		}

		private bool setOptimSettingAccordingToComboBox()
		{
			Settings.Default.StartSavedMipmapLevel = optimComboBox.SelectedIndex;
			return (mOldSettings.StartSavedMipmapLevel != Settings.Default.StartSavedMipmapLevel);
		}

		private void clearRecentFilesButton_Click(object sender, EventArgs e)
		{
			Settings.Default.RecentFiles.Clear();
			this.clearRecentFilesButton.Enabled = false;
		}
		#endregion

		#region tab appearance
		private float getGammaFromNumericUpDown(NumericUpDown control)
		{
			float gamma = (float)(100 - (int)(control.Value)) / 100.0f;
			if (gamma == 0.0f)
				gamma = 0.01f;
			return gamma;
		}

		private void setGammaToNumericUpDown(NumericUpDown control, float gamma)
		{
			if (gamma == 0.01f)
				gamma = 0.0f;
			gamma = 100 - (int)(gamma * 100.0f);
			control.Value = (Decimal)gamma;
		}

		private void redrawSamplePictureBox()
		{
			// create the image if not already existing
			if (this.samplePictureBox.Image == null)
				this.samplePictureBox.Image = new Bitmap(this.samplePictureBox.Width, this.samplePictureBox.Height);
			// get the gc of the image
			Graphics graphics = Graphics.FromImage(this.samplePictureBox.Image);
			// set the background color
			graphics.Clear(backgroundColorPictureBox.BackColor);
			// start corner for the grid
			int startGridCoord = 10;
			// draw some sub-grid lines
			Pen linePen = new Pen(subGridColorPictureBox.BackColor, 1);
			for (int i = startGridCoord - 8; i < this.samplePictureBox.Height; i += 8)
			{
				graphics.DrawLine(linePen, 0, i, this.samplePictureBox.Width, i);
				graphics.DrawLine(linePen, i, 0, i, this.samplePictureBox.Height);
			}
			// draw some grid lines
			linePen = new Pen(gridColorPictureBox.BackColor, 2);
			for (int i = startGridCoord; i < this.samplePictureBox.Height; i += 32)
			{
				graphics.DrawLine(linePen, 0, i, this.samplePictureBox.Width, i);
				graphics.DrawLine(linePen, i, 0, i, this.samplePictureBox.Height);
			}

			// create the image attributes
			ImageAttributes imageAttributeForSelection = new ImageAttributes();
			imageAttributeForSelection.SetGamma(getGammaFromNumericUpDown(this.GammaForSelectionNumericUpDown));
			ImageAttributes imageAttributeForSnapping = new ImageAttributes();
			imageAttributeForSnapping.SetGamma(getGammaFromNumericUpDown(this.GammaForSnappingNumericUpDown));

			// get the part example image from the resource
			Image image = Properties.Resources.PartForOptionPreview;

			// draw 3 part images as an example
			Rectangle destinationRectangle = new Rectangle(42, 42, image.Width, image.Height);
			graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
			destinationRectangle.Y += 32;
			graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributeForSelection);
			destinationRectangle.Y += 32;
			graphics.DrawImage(image, destinationRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributeForSnapping);

			// invalidate the picture box
			this.samplePictureBox.Invalidate();
		}

		private void backgroundColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = backgroundColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				backgroundColorPictureBox.BackColor = this.colorDialog.Color;
				redrawSamplePictureBox();
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
				findCorectColorSchemeAccordingToColors();
			}
		}

		private void gridColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = gridColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				gridColorPictureBox.BackColor = this.colorDialog.Color;
				redrawSamplePictureBox();
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
				findCorectColorSchemeAccordingToColors();
			}
		}

		private void subGridColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = subGridColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				subGridColorPictureBox.BackColor = this.colorDialog.Color;
				redrawSamplePictureBox();
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
				findCorectColorSchemeAccordingToColors();
			}
		}

		private void GammaForSelectionNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			redrawSamplePictureBox();
		}

		private void GammaForSnappingNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			redrawSamplePictureBox();
		}

		private void colorSchemeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch ((ColorScheme)colorSchemeComboBox.SelectedIndex)
			{
				case ColorScheme.BLUEPRINT:
					{
						backgroundColorPictureBox.BackColor = Color.CornflowerBlue;
						gridColorPictureBox.BackColor = Color.White;
						subGridColorPictureBox.BackColor = Color.LightGray;
						break;
					}
				case ColorScheme.PARCHMENT:
					{
						backgroundColorPictureBox.BackColor = Color.LemonChiffon;
						gridColorPictureBox.BackColor = Color.Olive;
						subGridColorPictureBox.BackColor = Color.DarkKhaki;
						break;
					}
				case ColorScheme.CLASSIC:
					{
						backgroundColorPictureBox.BackColor = Color.White;
						gridColorPictureBox.BackColor = Color.Black;
						subGridColorPictureBox.BackColor = Color.Gray;
						break;
					}
			}
			// redraw the picture boxes
			if ((ColorScheme)colorSchemeComboBox.SelectedIndex != ColorScheme.CUSTOM)
			{
				backgroundColorPictureBox.Invalidate();
				gridColorPictureBox.Invalidate();
				subGridColorPictureBox.Invalidate();
				redrawSamplePictureBox();
			}
		}

		private void findCorectColorSchemeAccordingToColors()
		{
			if (backgroundColorPictureBox.BackColor.Equals(Color.CornflowerBlue) &&
				gridColorPictureBox.BackColor.Equals(Color.White) &&
				subGridColorPictureBox.BackColor.Equals(Color.LightGray))
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.BLUEPRINT;
			else if (backgroundColorPictureBox.BackColor.Equals(Color.LemonChiffon) &&
				gridColorPictureBox.BackColor.Equals(Color.Olive) &&
				subGridColorPictureBox.BackColor.Equals(Color.DarkKhaki))
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.PARCHMENT;
			else if (backgroundColorPictureBox.BackColor.Equals(Color.White) &&
				gridColorPictureBox.BackColor.Equals(Color.Black) &&
				subGridColorPictureBox.BackColor.Equals(Color.Gray))
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CLASSIC;
			else
				colorSchemeComboBox.SelectedIndex = (int)ColorScheme.CUSTOM;
		}

		private void fillColorSchemeComboBox()
		{
			// respect the order of the enum ColorScheme
			colorSchemeComboBox.Items.Clear();
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeCustom);
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeBluePrint);
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeParchment);
			colorSchemeComboBox.Items.Add(Resources.ColorSchemeClassic);
		}

		private void updateChosenFont(Font newFont)
		{
			this.defaultFontNameLabel.Text = newFont.Name + " " + newFont.SizeInPoints.ToString();
			this.defaultFontNameLabel.Font = newFont;
		}

		private void defaultFontButton_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.fontDialog.Font = this.defaultFontNameLabel.Font;
			// open the color box in modal
			DialogResult result = this.fontDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				updateChosenFont(this.fontDialog.Font);
			}
		}

		private void defaultFontColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = defaultFontColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				this.defaultFontColorPictureBox.BackColor = this.colorDialog.Color;
				this.defaultFontNameLabel.ForeColor = this.colorDialog.Color;
			}
		}


		private void gridEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.gridSizeNumericUpDown.Enabled = this.gridEnabledCheckBox.Checked;
		}

		private void subGridEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			this.gridSubdivisionNumericUpDown.Enabled = this.subGridEnabledCheckBox.Checked;
		}
		#endregion

		#region tab PartLib
		private void fillPartLibraryListBox(bool isForResetingDefaultSetting)
		{
			// get the list of tabs
			List<string> tabNames = BlueBrick.MainForm.Instance.PartsTabControl.getTabNames();

			// if we need to reset the order from the setting, change the tab name list
			// according to the default setting
			if (isForResetingDefaultSetting)
			{
				System.Collections.Specialized.StringCollection settingNameList = Settings.Default.PartLibTabOrder;
				int insertIndex = 0;
				foreach (string settingName in settingNameList)
				{
					int currentIndex = tabNames.IndexOf(settingName);
					if (currentIndex != -1)
					{
						// get the tab page, remove it and reinsert it at the correct position
						string name = tabNames[currentIndex];
						tabNames.Remove(name); // do not use RemoveAt() that throw an exception even if the index is correct
						tabNames.Insert(insertIndex, name);

						// increment the insert point
						if (insertIndex < tabNames.Count)
							insertIndex++;
					}
				}
			}

			// fill the list with it
			this.PartLibTabListBox.Items.Clear();
			foreach (string name in tabNames)
				this.PartLibTabListBox.Items.Add(name);
		}

		private void savePartLibraryTabOrder()
		{
			if (mHasPartLibOrderChanged)
			{
				// recreate the setting array
				Settings.Default.PartLibTabOrder = new System.Collections.Specialized.StringCollection();
				// iterate on the list in the control
				foreach (object item in this.PartLibTabListBox.Items)
					Settings.Default.PartLibTabOrder.Add(item as string);
			}
		}

		private void MoveUpButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.PartLibTabListBox.SelectedIndex;
			if (selectedIndex > 0)
			{
				string name = this.PartLibTabListBox.Items[selectedIndex] as string;
				this.PartLibTabListBox.Items.RemoveAt(selectedIndex);
				int newIndex = selectedIndex - 1;
				this.PartLibTabListBox.Items.Insert(newIndex, name);
				this.PartLibTabListBox.SelectedIndex = newIndex;
				mHasPartLibOrderChanged = true;
			}
		}

		private void MoveDownButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = this.PartLibTabListBox.SelectedIndex;
			if ((selectedIndex >= 0) && (selectedIndex < this.PartLibTabListBox.Items.Count - 1))
			{
				string name = this.PartLibTabListBox.Items[selectedIndex] as string;
				this.PartLibTabListBox.Items.RemoveAt(selectedIndex);
				int newIndex = selectedIndex + 1;
				this.PartLibTabListBox.Items.Insert(newIndex, name);
				this.PartLibTabListBox.SelectedIndex = newIndex;
				mHasPartLibOrderChanged = true;
			}
		}

		private void PartLibTabListBox_SelectedValueChanged(object sender, EventArgs e)
		{
			int selectedIndex = this.PartLibTabListBox.SelectedIndex;
			MoveUpButton.Enabled = (selectedIndex > 0);
			MoveDownButton.Enabled = (selectedIndex >= 0) && (selectedIndex < this.PartLibTabListBox.Items.Count - 1);
		}

		private void alphabeticOrderButton_Click(object sender, EventArgs e)
		{
			this.PartLibTabListBox.Sorted = true;
			this.PartLibTabListBox.Sorted = false;
			mHasPartLibOrderChanged = true;
		}

		private void displayBubbleInfoCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			bool isEnabled = this.displayBubbleInfoCheckBox.Checked;
			this.displayPartIDCheckBox.Enabled = isEnabled;
			this.displayPartColorCheckBox.Enabled = isEnabled;
			this.displayPartDescriptionCheckBox.Enabled = isEnabled;
		}

		private void PartLibBackColorPictureBox_Click(object sender, EventArgs e)
		{
			// set the color with the current back color of the picture box
			this.colorDialog.Color = PartLibBackColorPictureBox.BackColor;
			// open the color box in modal
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if the user choose a color, set it back in the back color of the picture box
				PartLibBackColorPictureBox.BackColor = this.colorDialog.Color;
			}
		}
		#endregion

		#region tab Shortcut key

		/// <summary>
		/// This class is used to sort the column of the list view that list the shortcut keys
		/// </summary>
		private class MyListViewItemComparer : System.Collections.IComparer
		{
			private int mColumn;
			private SortOrder mOrder;

			public MyListViewItemComparer(int column, SortOrder order)
			{
				mColumn = column;
				mOrder = order;
			}

			public int Compare(Object x, Object y)
			{
				// use a string compare with the correct column
				int result = string.Compare((x as ListViewItem).SubItems[mColumn].Text, (y as ListViewItem).SubItems[mColumn].Text);
				// check the order
				if (mOrder == SortOrder.Descending)
					result = -result;
				return result;
			}
		};

		/// <summary>
		/// Add a new item (with its subitems) to the list view of shortcut key, from an array of string.
		/// The first cell of the array is the index in the shortcut key dropdown list.
		/// The first cell of the array is the index in the action dropdown list.
		/// The two last cells are the part number (in clear) and the connection number.
		/// </summary>
		/// <param name="itemNames"></param>
		private void addShortcutKey(string[] itemNames)
		{
			ListViewItem.ListViewSubItem[] subItems = { new ListViewItem.ListViewSubItem(), new ListViewItem.ListViewSubItem(), new ListViewItem.ListViewSubItem(), new ListViewItem.ListViewSubItem() };

			// try to replace the first two indices by the corresponding text
			try
			{
				// key
				int index = int.Parse(itemNames[0]);
				subItems[0].Text = this.comboBoxKey.Items[index].ToString();
				subItems[0].Tag = itemNames[0];
				// action
				index = int.Parse(itemNames[1]);
				subItems[1].Text = this.comboBoxAction.Items[index].ToString();
				subItems[1].Tag = itemNames[1];
			}
			catch { }
			subItems[2].Text = itemNames[2];
			subItems[3].Text = itemNames[3];

			// add the new item
			ListViewItem newItem = new ListViewItem(subItems,0);
			this.listViewShortcutKeys.Items.Add(newItem);
		}

		private void comboBoxAction_SelectedIndexChanged(object sender, EventArgs e)
		{
			// disable the 2 last combobox if we don't choose the add action
			bool isEnabled = (this.comboBoxAction.SelectedIndex == 0);
			this.comboBoxPartNum.Enabled = isEnabled;
			this.comboBoxConnexion.Enabled = isEnabled;
		}

		private void comboBoxPartNum_SelectedIndexChanged(object sender, EventArgs e)
		{
			// refill the connexion combobox according to the current selected part
			this.comboBoxConnexion.Items.Clear();
			// ask the list of connexion to the brick library
			List<BrickLibrary.Brick.ConnectionPoint> connexionList = BrickLibrary.Instance.getConnectionList(this.comboBoxPartNum.SelectedItem.ToString());
			int nbConnexion = 1;
			if (connexionList != null)
				nbConnexion = connexionList.Count;
			// fill the combobox
			for (int i = 0; i < nbConnexion; ++i)
				this.comboBoxConnexion.Items.Add(i.ToString());
			// select the first item
			this.comboBoxConnexion.SelectedIndex = 0;
		}

		private void buttonAdd_Click(object sender, EventArgs e)
		{
			string[] itemNames = { this.comboBoxKey.SelectedIndex.ToString(), this.comboBoxAction.SelectedIndex.ToString(), this.comboBoxPartNum.SelectedItem.ToString(), this.comboBoxConnexion.SelectedItem.ToString() };
			if (this.comboBoxAction.SelectedIndex != 0)
			{
				itemNames[2] = String.Empty;
				itemNames[3] = String.Empty;
			}
			addShortcutKey(itemNames);
		}

		private void buttonDelete_Click(object sender, EventArgs e)
		{
			List<ListViewItem>	itemsToDelete = new List<ListViewItem>();
			foreach (ListViewItem item in this.listViewShortcutKeys.SelectedItems)
				itemsToDelete.Add(item);
			foreach (ListViewItem item in itemsToDelete)
				this.listViewShortcutKeys.Items.Remove(item);
		}

		private void listViewShortcutKeys_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.buttonDelete.Enabled = (this.listViewShortcutKeys.SelectedItems.Count > 0);
		}

		private void listViewShortcutKeys_ColumnClick(object sender, ColumnClickEventArgs e)
		{
			// start of the update of the control
			this.listViewShortcutKeys.BeginUpdate();

			// change the sort order if we click again on the same column
			// but if we change the column, don't change the sort order
			if (mGlobalStatsLastColumnSorted == e.Column || mGlobalStatsLastColumnSorted == -1)
			{
				if (this.listViewShortcutKeys.Sorting == SortOrder.Ascending)
					this.listViewShortcutKeys.Sorting = SortOrder.Descending;
				else
					this.listViewShortcutKeys.Sorting = SortOrder.Ascending;
			}
			else
			{
				// We change the twice the listViewShortcutKeys.Sorting value
				// and reset it with the same value to FORCE the Sort to be done,
				// once if the listViewShortcutKeys.Sorting didn't changed the Sort
				// method does nothing.
				SortOrder oldOrder = this.listViewShortcutKeys.Sorting;
				this.listViewShortcutKeys.Sorting = SortOrder.None;
				this.listViewShortcutKeys.Sorting = oldOrder;
			}

			// remove the order sign on the previous sorted column
			if (mGlobalStatsLastColumnSorted != -1)
			{
				string header = this.listViewShortcutKeys.Columns[mGlobalStatsLastColumnSorted].Text;
				this.listViewShortcutKeys.Columns[mGlobalStatsLastColumnSorted].Text = header.Substring(0, header.Length - 2);
			}

			// keep in mind the last sorted column
			mGlobalStatsLastColumnSorted = e.Column;

			// add a descending or ascending sign to the header of the column
			if (this.listViewShortcutKeys.Sorting == SortOrder.Ascending)
				this.listViewShortcutKeys.Columns[e.Column].Text += " " + char.ConvertFromUtf32(0x25B2);
			else
				this.listViewShortcutKeys.Columns[e.Column].Text += " " + char.ConvertFromUtf32(0x25BC);

			// create a new comparer with the right column then call the sort method
			this.listViewShortcutKeys.ListViewItemSorter = new MyListViewItemComparer(e.Column, this.listViewShortcutKeys.Sorting);
			this.listViewShortcutKeys.Sort();

			// end of the update of the control
			this.listViewShortcutKeys.EndUpdate();
		}
		#endregion
	}
}