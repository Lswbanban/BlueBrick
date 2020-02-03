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
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using BlueBrick.MapData;
using BlueBrick.Properties;
using System.Drawing.Drawing2D;

namespace BlueBrick
{
	public partial class PartLibraryPanel : TabControl
	{
		private Size PART_ITEM_SMALL_SIZE = new Size(64, 64);
		private Size PART_ITEM_LARGE_SIZE = new Size(128, 128);
		private Size PART_ITEM_SMALL_SIZE_WITH_MARGIN = new Size(69, 69);
		private Size PART_ITEM_LARGE_SIZE_WITH_MARGIN = new Size(134, 134);

		#region embeded classes for items
		private enum ContextMenuIndex
		{
			LARGE_ICON = 0,
			RESPECT_PROPORTION,
			SHOW_BUBBLE_INFO
		}

		private class PartLibDisplaySetting
		{
			public bool mLargeIcons = true;
			public bool mRespectProportion = false;
            public string mFilterSentence = string.Empty;
            
            // constructors
            public PartLibDisplaySetting()
            {
            }

			public PartLibDisplaySetting(bool largeIcon, bool respectProportion, string filterSentence)
			{
				mLargeIcons = largeIcon;
				mRespectProportion = respectProportion;
                mFilterSentence = filterSentence;
			}
		};

		private class FileNameWithException
		{
			public string mFilename;
			public string mException;
			public string ShortFileName
			{
				get { return (new FileInfo(mFilename)).Name; }
			}
			public FileNameWithException(string filename, string exception)
			{
				mFilename = filename;
				mException = exception;
			}
		}

		/// <summary>
		///  This container is used during the loading of the part lib to store some info during loading
		///  and finish the building after the loading is finished
		/// </summary>
		private class CategoryBuildingInfo
		{
			public PartListView mListView = new PartListView(); // the list view for this category folder
			public List<Image> mImageList = new List<Image>(); // all the image not resized in this folder
			public bool mRespectProportion = true; // the proportion flag for this category
			public List<BrickLibrary.Brick> mGroupList = new List<BrickLibrary.Brick>(); // all the group parts in this folder

			public CategoryBuildingInfo(bool respectProportion)
			{
				mRespectProportion = respectProportion;
			}
		}
		#endregion
		
		// we store the part we drag because the drag and drop is buggy in Mono and I cannot pass the data in
		// the drag and drop. Null means no parts are droped
		private string mDraggingPartNumber = null;

		// we store also the last location from the mouse move event cause Mono call the handler when we click on the
		// left button, even if we didn't moved yet
		private Point mLastMousePosition = Point.Empty;

		#region get/set
		public string DraggingPartNumber
		{
			set { mDraggingPartNumber = value; }
			get { return mDraggingPartNumber; }
		}

		public bool IsEditingBudget
		{
			get
			{
				if ((this.SelectedTab != null) && (this.SelectedTab.Controls.Count > 0))
				{
					PartListView listView = this.SelectedTab.Controls[0] as PartListView;
					if (listView != null)
						return listView.IsEditingBudget;
				}
				return false;
			}
		}

		public static string sFolderNameForCustomParts
		{
			get { return "Custom"; }
		}

		public static string sFullPathForCustomParts
		{
			get { return (PartLibraryPanel.sFullPathForLibrary + @"/" + PartLibraryPanel.sFolderNameForCustomParts + @"/"); }
		}

		public static string sFullPathForLibrary
		{
			get { return (Application.StartupPath + @"/parts"); }
		}
		#endregion

		#region constructor and override method
		public PartLibraryPanel()
		{
			InitializeComponent();
			// the part tab control will be init by the init of the part library
		}

		/// <summary>
		/// This method is inheritated and is usefull to get the event when the arrow are pressed
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool IsInputKey(Keys keyData)
		{
			// we need the four arrow keys
			// also page up and down for rotation
			// and delete and backspace for deleting object
			if ((keyData == Keys.Left) || (keyData == Keys.Right) ||
				(keyData == Keys.Up) || (keyData == Keys.Down) ||
				(keyData == Keys.PageDown) || (keyData == Keys.PageUp) ||
				(keyData == Keys.Home) || (keyData == Keys.End) ||
				(keyData == Keys.Insert) || (keyData == Keys.Delete) ||
				(keyData == Keys.Enter) || (keyData == Keys.Return) ||
				(keyData == Keys.Escape) || (keyData == Keys.Back))
				return true;

			return base.IsInputKey(keyData);
		}
		#endregion

		#region init part library

		/// <summary>
		/// clear all the tab control
		/// </summary>
		public void clearAllData()
		{
			this.TabPages.Clear();
		}

		/// <summary>
		/// create and return a context menu that can be assigned to a tab page
		/// </summary>
		/// <returns>a new instance of context menu for a part lib tab</returns>
		private ContextMenuStrip createContextMenuItemForATabPage(bool useLargeIcon, bool respectProportionIsChecked)
		{
			// create the context menu
			ContextMenuStrip contextMenu = new ContextMenuStrip();
			// add the openning event to eventually change some states
			contextMenu.Opening += new System.ComponentModel.CancelEventHandler(contextMenu_Opening);
			// menu item to display the icons in large
			ToolStripMenuItem largeIconsMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemLargeIcons, null, menuItem_LargeIconClick);
			largeIconsMenuItem.CheckOnClick = true;
			largeIconsMenuItem.Checked = useLargeIcon;
			contextMenu.Items.Add(largeIconsMenuItem);
			// menu item to repect the proportions
			ToolStripMenuItem proportionMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemRespectProportion, null, menuItem_RespectProportionClick);
			proportionMenuItem.CheckOnClick = true;
			proportionMenuItem.Checked = respectProportionIsChecked;
			contextMenu.Items.Add(proportionMenuItem);
			// menu item to display tooltips
			ToolStripMenuItem bubbleInfoMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemDisplayTooltips, null, menuItem_DisplayTooltipsClick);
			bubbleInfoMenuItem.CheckOnClick = true;
			bubbleInfoMenuItem.Checked = Settings.Default.PartLibDisplayBubbleInfo;
			contextMenu.Items.Add(bubbleInfoMenuItem);
			// add a line
			ToolStripSeparator separator = new ToolStripSeparator();
			separator.Name = "budgetSeparator";
			contextMenu.Items.Add(separator);
			// menu item to show only the budgeted parts
			ToolStripMenuItem showOnlyBudgetedPartsMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemShowOnlyBudgetedParts, null, menuItem_ShowOnlyBudgetedPartsClick);
			showOnlyBudgetedPartsMenuItem.Name = "showOnlyBudgetedPartsMenuItem";
			showOnlyBudgetedPartsMenuItem.CheckOnClick = false;
			showOnlyBudgetedPartsMenuItem.Checked = Properties.Settings.Default.ShowOnlyBudgetedParts;
			contextMenu.Items.Add(showOnlyBudgetedPartsMenuItem);
			// menu item to display budget numbers
			ToolStripMenuItem showBudgetNumbersMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemShowBudgetNumbers, null, menuItem_ShowBudgetNumberClick);
			showBudgetNumbersMenuItem.Name = "showBudgetNumbersMenuItem";
			showBudgetNumbersMenuItem.CheckOnClick = false;
			showBudgetNumbersMenuItem.Checked = Properties.Settings.Default.ShowBudgetNumbers;
			contextMenu.Items.Add(showBudgetNumbersMenuItem);
			// menu item to use the budget limitation
			ToolStripMenuItem useBudgetLimitationMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemUseBudgetLimitation, null, menuItem_UseBudgetLimitationClick);
			useBudgetLimitationMenuItem.Name = "useBudgetLimitationMenuItem";
			useBudgetLimitationMenuItem.CheckOnClick = false;
			useBudgetLimitationMenuItem.Checked = Properties.Settings.Default.UseBudgetLimitation;
			contextMenu.Items.Add(useBudgetLimitationMenuItem);
			// menu item to display tooltips
			ToolStripMenuItem editBudgetMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemEditBudget, null, menuItem_EditBudgetClick);
			editBudgetMenuItem.Name = "editBudgetMenuItem";
			editBudgetMenuItem.CheckOnClick = false;
			editBudgetMenuItem.Checked = false;
			contextMenu.Items.Add(editBudgetMenuItem);
			// return the well form context menu
			return contextMenu;
		}

		/// <summary>
		/// parse the part folder to find all the part in the library
		/// </summary>
		public void initPartsTabControl()
		{
			// suspend layout when rebuilding the library
			this.SuspendLayout();

			// init the part tab control based on the folders found on the drive
			// first clear the tab control
			this.TabPages.Clear();
			// then search the "parts" folder, if not here maybe we should display
			// an error message (something wrong with the installation of the application?)
			DirectoryInfo partsFolder = new DirectoryInfo(PartLibraryPanel.sFullPathForLibrary);
			if (partsFolder.Exists)
			{
				// create two list to record the exception thrown by some files
				List<FileNameWithException> imageFileUnloadable = new List<FileNameWithException>();
				List<FileNameWithException> xmlFileUnloadable = new List<FileNameWithException>();

				// create from the Settings a dictionary to store the display status of each tab
				Dictionary<string, PartLibDisplaySetting> tabDisplayStatus = new Dictionary<string,PartLibDisplaySetting>();
                foreach (string tabConfig in Settings.Default.UIPartLibDisplayConfig)
                {
                    // the format of the tabConfig string is: tabName00?filterSentence
                    // we will split it in the middle at the '?' char
                    string filterSentence = string.Empty;
                    // get the index of the ? character which is the separator between the tab name and the filter keywords 
                    // because the ? char cannot be used in filename (and tab name comes from filename)
                    int separatorIndex = tabConfig.IndexOf('?');
                    if (separatorIndex < 0)
                        separatorIndex = tabConfig.Length; // check the case of old config file without "?" char separator
                    // get the first part before the keyword
                    string tabNameAndDisplayConfig = tabConfig.Substring(0, separatorIndex);
                    // maybe we have some keywords, so try to get them
                    if (tabConfig.Length > separatorIndex + 1)
                        filterSentence = tabConfig.Substring(separatorIndex + 1);
                    tabDisplayStatus.Add(tabNameAndDisplayConfig.Remove(separatorIndex - 2),
                                         new PartLibDisplaySetting(tabNameAndDisplayConfig[separatorIndex - 2] == '1', 
                                                                    tabNameAndDisplayConfig[separatorIndex - 1] == '1',
                                                                    filterSentence));
                }

				// get all the folders in the parts folder to create a tab for each folder found
				DirectoryInfo[] categoryFolder = partsFolder.GetDirectories();

				// create a list to store all the info necessary for the library building for each category
				List<CategoryBuildingInfo> categoryList = new List<CategoryBuildingInfo>(categoryFolder.Length);

				// iterate on each folder
				foreach (DirectoryInfo category in categoryFolder)
				{
					// try to get the display setting or construct a default one
					PartLibDisplaySetting displaySetting = null;
					if (!tabDisplayStatus.TryGetValue(category.Name, out displaySetting))
						displaySetting = new PartLibDisplaySetting();

					// create a building info and add it to the list
					CategoryBuildingInfo buildingInfo = new CategoryBuildingInfo(displaySetting.mRespectProportion);
					categoryList.Add(buildingInfo);

					// create the tab page corresponding to the folder
					addOneTabPageWithItsListView(buildingInfo, category.Name, displaySetting);

					// check if there's a local config folder in that directory, and load the connection types before loading the parts
					// because the parts will probably need those type of connections
					string connectionTypeFileName = category.FullName + @"/config/ConnectionTypeList.xml";
					if (File.Exists(connectionTypeFileName))
						BrickLibrary.Instance.loadConnectionTypeInfo(connectionTypeFileName, false);

					// fill the list view with the parts loaded from the files
					fillListViewWithParts(buildingInfo, category, imageFileUnloadable, xmlFileUnloadable);
				}

				// reiterate on a second pass on all the list view, because we need to add the group parts
				foreach (CategoryBuildingInfo buildingInfo in categoryList)
				{
					fillListViewWithGroupAndImageToFinalize(buildingInfo, imageFileUnloadable, xmlFileUnloadable);
				}

				// after the loading is finished eventually display the error messages
				displayErrorMessage(imageFileUnloadable, xmlFileUnloadable);

				// after creating all the tabs, sort them according to the settings
				updateAppearanceAccordingToSettings(true, false, false, false, true, true);

				// call the index event handler manually cause, it is not called on Mono
				PartLibraryPanel_SelectedIndexChanged(null, null);
			}

			// suspend layout when rebuilding the library
			this.ResumeLayout();
		}

		/// <summary>
		/// This function should be called when you want to load more files after the loading of the library is finished.
		/// This function is mainly called when the user save a new group in the library. This function will not delete any
		/// part, but will update the existing part if you reload the same part.
		/// </summary>
		/// <param name="xmlFiles">The list of group xml to load</param>
		/// <param name="groupNames">The list of group names corresponding to the list of xml files</param>
		public void loadAdditionnalGroups(List<FileInfo> xmlFiles, List<string> groupNames)
		{
			// use a default display setting that won't be used to change the setting of the Custom tab page
			// unless this tab page doesn't exist, in which case the default setting is suitable
			PartLibDisplaySetting displaySetting = new PartLibDisplaySetting();
			CategoryBuildingInfo buildingInfo = new CategoryBuildingInfo(displaySetting.mRespectProportion);

			// first check if the Custom tab exits. If not we need to create it.
			if (this.TabPages.ContainsKey(PartLibraryPanel.sFolderNameForCustomParts))
			{
				// the tabe page exist, so get it
				TabPage tabPage = this.TabPages[this.TabPages.IndexOfKey(PartLibraryPanel.sFolderNameForCustomParts)];
				// patch the building info with the correct listview
				buildingInfo.mListView = tabPage.Controls[0] as PartListView;
				// patch also the image list
				buildingInfo.mImageList = buildingInfo.mListView.reconstructImageListFromBrickLibrary();
				// patch the respect proportion
				buildingInfo.mRespectProportion = (tabPage.ContextMenuStrip.Items[(int)ContextMenuIndex.RESPECT_PROPORTION] as ToolStripMenuItem).Checked;

				// now check if the part is already in, and remove it in order to replace it
				foreach (string name in groupNames)
					foreach (ListViewItem item in buildingInfo.mListView.Items)
						if (item.Tag.Equals(name))
						{
							int removedImageIndex = item.ImageIndex;
							buildingInfo.mImageList.RemoveAt(removedImageIndex);
							buildingInfo.mListView.Items.Remove(item);
							// then iterate again on all the item to shift all the image index that are after the item removed of -1
							foreach (ListViewItem itemToShift in buildingInfo.mListView.Items)
								if (itemToShift.ImageIndex > removedImageIndex)
									itemToShift.ImageIndex = itemToShift.ImageIndex - 1;
							// break the list view search since we found the item to remove
							break;
						}
			}
			else
			{
				// The custom page doesn't exist we need to create a new one
				addOneTabPageWithItsListView(buildingInfo, PartLibraryPanel.sFolderNameForCustomParts, displaySetting);
			}

			// now load the xml files
			List<FileNameWithException> imageFileUnloadable = new List<FileNameWithException>();
			List<FileNameWithException> xmlFileUnloadable = new List<FileNameWithException>();
			fillListViewWithPartsWithoutImage(buildingInfo, xmlFiles, xmlFileUnloadable, true);

			// the fill the list view with the new groups
			fillListViewWithGroupAndImageToFinalize(buildingInfo, imageFileUnloadable, xmlFileUnloadable);

			// after the loading is finished eventually display the error messages
			displayErrorMessage(imageFileUnloadable, xmlFileUnloadable);

			// Select the Custom Tab page
			int cutsomTabIndex = this.TabPages.IndexOfKey(PartLibraryPanel.sFolderNameForCustomParts);
			if (cutsomTabIndex >= 0)
			{
				// select the Custom tab
				this.SelectTab(cutsomTabIndex);
				// find the first item created and scroll it in view
				foreach (ListViewItem item in buildingInfo.mListView.Items)
					if (item.Tag.Equals(groupNames[0]))
					{
						buildingInfo.mListView.EnsureVisible(item.Index);
						break;
					}
			}
		}

		private void displayErrorMessage(List<FileNameWithException> imageFileUnloadable, List<FileNameWithException> xmlFileUnloadable)
		{
			// check if there was some error with some files
			string message = null;
			string details = "";
			if (imageFileUnloadable.Count > 0)
			{
				// display a warning message
				message = Properties.Resources.ErrorMsgCanNotLoadImage;
				foreach (FileNameWithException error in imageFileUnloadable)
				{
                    message += Environment.NewLine + error.mFilename;
                    details += error.ShortFileName + ":" + Environment.NewLine + error.mException + Environment.NewLine + Environment.NewLine;
				}
			}
			if (xmlFileUnloadable.Count > 0)
			{
				if (message == null)
					message = "";
				else
                    message += Environment.NewLine + Environment.NewLine;
				// display a warning message
				message += Properties.Resources.ErrorMsgCanNotLoadPartXML;
				foreach (FileNameWithException error in xmlFileUnloadable)
				{
                    message += Environment.NewLine + error.mFilename;
                    details += error.ShortFileName + ":" + Environment.NewLine + error.mException + Environment.NewLine + Environment.NewLine;
				}
			}

			// display the error message if there was some errors
			if (message != null)
			{
				LoadErrorForm messageBox = new LoadErrorForm(Properties.Resources.ErrorMsgTitleWarning, message, details);
				messageBox.Show();
			}
		}

		/// <summary>
		/// This method create a new TabPage into the tab control of the library panel, and also create the ListView
		/// that holds the brick items, which is the only child of the TabPage
		/// </summary>
		/// <param name="buildingInfo">The building info that can be used to create the tab</param>
		/// <param name="tabPageName">The name of the tab page (which should be the name of the folder)</param>
		/// <param name="displaySetting">a display setting to correctly init the tab properties</param>
		private void addOneTabPageWithItsListView(CategoryBuildingInfo buildingInfo, string tabPageName, PartLibDisplaySetting displaySetting)
		{
			// add the tab in the tab control, based on the name of the folder
			TabPage newTabPage = new TabPage(tabPageName);
			newTabPage.Name = tabPageName;
			newTabPage.ContextMenuStrip = createContextMenuItemForATabPage(displaySetting.mLargeIcons, displaySetting.mRespectProportion);
			this.TabPages.Add(newTabPage);

			// then for the new tab added, we add a list control to 
			// fill it with the pictures found in that folder
			// but we don't need to create it, we use the one created in the building info
			PartListView newListView = buildingInfo.mListView; // get a shortcut on the list view
            // filter the view (at this point it will be useless since there's no item in the view, but the goal is to save the filter sentence, it will be refilter later)
            newListView.filter(displaySetting.mFilterSentence, true);
            // set the tile size
			if (displaySetting.mLargeIcons)
				newListView.TileSize = PART_ITEM_LARGE_SIZE_WITH_MARGIN;
			else
				newListView.TileSize = PART_ITEM_SMALL_SIZE_WITH_MARGIN;
            // set the event handler
			newListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView_MouseMove);

			// add the list view to the tab page
			newTabPage.Controls.Add(newListView);
		}

		private void addOnePartInListView(CategoryBuildingInfo buildingInfo, BrickLibrary.Brick brick)
        {
			// add the brick in library if we should do it
			if (!brick.NotListedInLibrary)
			{
				int imageIndex = buildingInfo.mImageList.Count;

				// add the image in the image list (after using the imageList.Count, but before creating the item,
				// otherwise mono is not happy cause it tries to access the image while creating the item)
				buildingInfo.mImageList.Add(brick.Image);

				// create a new item for the list view item
				ListViewItem newItem = new ListViewItem(null as string, imageIndex);
				newItem.ToolTipText = BrickLibrary.Instance.getFormatedBrickInfo(brick.mPartNumber,
															Settings.Default.PartLibBubbleInfoPartID,
															Settings.Default.PartLibBubbleInfoPartColor,
															Settings.Default.PartLibBubbleInfoPartDescription);

				// set the tag to the item 
				newItem.Tag = brick.mPartNumber;
				// also set the name to allow the sorting of the part
				// we could have use the Text property and the Sorting property of the listview, but the stupid
				// listview display the Text in the bubble info even when ShowToolTips is false.
				// also concatenate the sorting key with the part number such as we always have a fix order,
				// even after several filtering and even if the sorting key is not set. But if it is set,
				// the sorting key has the priority since it is place in front
				newItem.Name = BrickLibrary.Instance.getSortingKey(brick.mPartNumber) + brick.mPartNumber;
				// the text is used to display the count and budget
				newItem.Text = Budget.Budget.Instance.getCountAndBudgetAsString(brick.mPartNumber);

				// and insert the item
				buildingInfo.mListView.addNewItem(newItem);
			}
        }

		private void fillListViewWithParts(CategoryBuildingInfo buildingInfo, DirectoryInfo folder, List<FileNameWithException> imageFileUnloadable, List<FileNameWithException> xmlFileUnloadable)
		{
			// start the begin update outside of the loop because on Mono it's very slow to add items in the list view
			buildingInfo.mListView.BeginUpdate();

			// get the list of xml and image in the folder
			List<FileInfo> xmlFiles = new List<FileInfo>(folder.GetFiles("*.xml"));
			FileInfo[] imageFiles = folder.GetFiles("*.gif");
			// then iterate on all the images
			foreach (FileInfo file in imageFiles)
			{
				try
				{
					// read the image from the file
					Bitmap image = new Bitmap(file.FullName);

					// construct the XML file
					string xmlFileName = file.FullName.Substring(0, file.FullName.Length - 3) + "xml";

					try
					{
						// get the name without extension and use upper case
						string name = file.Name.Substring(0, file.Name.Length - 4).ToUpperInvariant();

						// remove the xml file name from the list first because we don't want to try to load
						// it a second time if an exception is raised.
						foreach (FileInfo xmlFileInfo in xmlFiles)
							if (xmlFileInfo.FullName.Equals(xmlFileName))
							{
								xmlFiles.Remove(xmlFileInfo);
								break;
							}

						// put the image in the database
						BrickLibrary.Brick brick = BrickLibrary.Instance.AddBrick(name, image, xmlFileName, false);

                        // add this part into the listview
						addOnePartInListView(buildingInfo, brick);
					}
					catch (Exception e)
					{
						// add the file that can't be loaded in the list of problems
						xmlFileUnloadable.Add(new FileNameWithException(xmlFileName, e.Message));
					}
				}
				catch (Exception e)
				{
					// add the file that can't be loaded in the list of problems
					imageFileUnloadable.Add(new FileNameWithException(file.FullName, e.Message));
				}
			}

			// resume the update
			buildingInfo.mListView.EndUpdate();

			// now check if there's xml files without GIF. In that case we still load them but these
			// parts will be ignored by BlueBrick
			fillListViewWithPartsWithoutImage(buildingInfo, xmlFiles, xmlFileUnloadable, false);
		}

		/// <summary>
		/// This method should be call when you want to load the xml files which don't have a corresponding gif file
		/// of the same name. This method is actually called by the method wich load all the gif of a folder, as a second pass.
		/// </summary>
		/// <param name="buildingInfo">The building info, mainly used to store the group encountered</param>
		/// <param name="xmlFiles">The list of xml file to load</param>
		/// <param name="xmlFileUnloadable">a list to store the errors when some file cannot be loaded</param>
		private void fillListViewWithPartsWithoutImage(CategoryBuildingInfo buildingInfo, List<FileInfo> xmlFiles, List<FileNameWithException> xmlFileUnloadable, bool allowReplacement)
		{
			// iterate on the array of xmlFiles
			foreach (FileInfo file in xmlFiles)
			{
				try
				{
					// get the name without extension and use upper case
					string name = file.Name.Substring(0, file.Name.Length - 4).ToUpperInvariant();

                    // add the brick in the library
					BrickLibrary.Brick brickAdded = BrickLibrary.Instance.AddBrick(name, null, file.FullName, allowReplacement);

                    // if the image is a group, add it to the group list
                    if (brickAdded.IsAGroup)
						buildingInfo.mGroupList.Add(brickAdded);
				}
				catch (Exception e)
				{
					// add the file that can't be loaded in the list of problems
					xmlFileUnloadable.Add(new FileNameWithException(file.FullName, e.Message));
				}
			}
		}

		private void fillListViewWithGroups(CategoryBuildingInfo buildingInfo, List<FileNameWithException> imageFileUnloadable, List<FileNameWithException> xmlFileUnloadable)
        {
			// start the begin update outside of the loop because on Mono it's very slow to add items in the list view
			buildingInfo.mListView.BeginUpdate();

			// do a third pass to generate and add the group parts in the library
			// we need to do it in a third pass in order to have read all the group xml files
			foreach (BrickLibrary.Brick group in buildingInfo.mGroupList)
			{
				try
				{
					// generate the image for the group
					BrickLibrary.Instance.createGroupImage(group);
					// add this brick in the list view
					addOnePartInListView(buildingInfo, group);
				}
				catch (Exception e)
				{
					// add the group that can't be loaded in the list of problems (cyclic group for example)
					xmlFileUnloadable.Add(new FileNameWithException(group.mPartNumber + ".xml", e.Message));
				}
			}

			// resume the update
			buildingInfo.mListView.EndUpdate();
        }

		private void fillListViewWithGroupAndImageToFinalize(CategoryBuildingInfo buildingInfo, List<FileNameWithException> imageFileUnloadable, List<FileNameWithException> xmlFileUnloadable)
		{
			// then add the group parts in the list view. We need to do it after parsing the whole library
			// because a group part can reference any part in any folder
			fillListViewWithGroups(buildingInfo, imageFileUnloadable, xmlFileUnloadable);
			// then fill the list view (we cannot pass the building info as parameter because this function is called elsewhere)
			fillListViewWithImageList(buildingInfo.mListView, buildingInfo.mImageList, buildingInfo.mRespectProportion);
			// refilter the list view after we added all the parts (that will also sort the list)
            buildingInfo.mListView.refilter(true);
		}


		private void fillListViewWithImageList(PartListView listViewToFill, List<Image> imageList, bool respectProportion)
		{
			// compute the global rescale factor if we need to respect the proportions
			float globalImageRescaleFactor = 1.0f;
			if (respectProportion)
			{
				// declare a variable to find the biggest image size
				int biggestSize = 1; // 1 to avoid a division by 0
				foreach (Image image in imageList)
				{
					if (biggestSize < image.Width)
						biggestSize = image.Width;
					if (biggestSize < image.Height)
						biggestSize = image.Height;
				}

				// compute the rescale factor:
				globalImageRescaleFactor = (float)PART_ITEM_LARGE_SIZE.Width / (float)biggestSize;
			}

			// create two image list that will receive a snapshot of each image found in the folder
			// WARNING: by default the colorDepth of the image list is 8bit, so the palette is recomputed
			// everytime new images are added. For performance issue, we need a higher palette depth to
			// avoid quantization. If we use 32bit depth, the selection is not drawn for small image list
			// 24 bit avoid paletization, without the selection bug ondot net 
			ImageList largeImageList = new ImageList();
			largeImageList.ImageSize = PART_ITEM_LARGE_SIZE;
			largeImageList.ColorDepth = ColorDepth.Depth24Bit;
			ImageList smallImageList = new ImageList();
			smallImageList.ImageSize = PART_ITEM_SMALL_SIZE;
			smallImageList.ColorDepth = ColorDepth.Depth24Bit;

			// now we rescale all the images according to the biggest one in the folder
			foreach (Image image in imageList)
			{
				// choose the the current scale that should be used
				float imageRescaleFactor = 1.0f;
				if (respectProportion)
				{
					imageRescaleFactor = globalImageRescaleFactor;
				}
				else
				{
					if (image.Width > image.Height)
						imageRescaleFactor = (float)PART_ITEM_LARGE_SIZE.Width / (float)image.Width;
					else
						imageRescaleFactor = (float)PART_ITEM_LARGE_SIZE.Width / (float)image.Height;
				}

				// create a snapshot of the current image and replace it in the two image lists
				// but to avoid a stretching effect, we redraw the picture in a square
				// first compute the position and size of the bitmap to draw
				Rectangle drawRectangle = new Rectangle();
				drawRectangle.Width = (int)(image.Width * imageRescaleFactor);
				drawRectangle.Height = (int)(image.Height * imageRescaleFactor);
				drawRectangle.X = (PART_ITEM_LARGE_SIZE.Width - drawRectangle.Width) / 2;
				drawRectangle.Y = (PART_ITEM_LARGE_SIZE.Height - drawRectangle.Height) / 2;
				// create a new bitmap to draw the scaled part
				Bitmap snapshotImage = new Bitmap(PART_ITEM_LARGE_SIZE.Width, PART_ITEM_LARGE_SIZE.Height);
				Graphics graphics = Graphics.FromImage(snapshotImage);
				graphics.CompositingMode = CompositingMode.SourceCopy; // copy is enough
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.CompositingQuality = CompositingQuality.HighSpeed;
				graphics.InterpolationMode = InterpolationMode.Default;
				graphics.DrawImage(image, drawRectangle);
				graphics.Flush();

				// reassign the scalled image
				largeImageList.Images.Add(snapshotImage);
				smallImageList.Images.Add(snapshotImage);
			}

			// assign the two image list created
			if (listViewToFill.TileSize == PART_ITEM_LARGE_SIZE_WITH_MARGIN)
			{
				listViewToFill.LargeImageList = largeImageList;
				listViewToFill.SmallImageList = smallImageList;
			}
			else
			{
				listViewToFill.LargeImageList = smallImageList;
				listViewToFill.SmallImageList = largeImageList;
			}
		}
		#endregion

		#region configuration of the part lib
		public List<string> getTabNames()
		{
			List<string> resultList = new List<string>(this.TabCount);
			foreach (TabPage tabPage in this.TabPages)
				resultList.Add(tabPage.Name);
			return resultList;
		}

		/// <summary>
		/// Filter the current tab with the specified string. The string should be a list of keywords that can be
		/// prefixed by a '-' to exclude, a '+' to include or a '#' to filter on part id. Each keyword should be separated
		/// by blank character such as space or tab.
		/// </summary>
		/// <param name="filterSentence">several keywords separated by blank characters with or without prefixes</param>
		public void filterCurrentTab(string filterSentence)
		{
			if (this.SelectedTab != null)
			{
				PartListView listView = this.SelectedTab.Controls[0] as PartListView;
				if (listView != null)
					listView.filter(filterSentence, true);
			}
		}

		/// <summary>
		/// Filter the all the tabs with the specified string. The string should be a list of keywords that can be
		/// prefixed by a '-' to exclude, a '+' to include or a '#' to filter on part id. Each keyword should be separated
		/// by blank character such as space or tab.
		/// </summary>
		/// <param name="filterSentence">several keywords separated by blank characters with or without prefixes</param>
		public void filterAllTabs(string filterSentence)
		{
			// save the global filter sentence
			Settings.Default.UIFilterAllSentence = filterSentence;
			Settings.Default.UIFilterAllLibraryTab = true;
			// and iterate on all the tabs
			foreach (TabPage tabPage in this.TabPages)
			{
				PartListView listView = tabPage.Controls[0] as PartListView;
				if (listView != null)
					listView.filter(filterSentence, (tabPage == this.SelectedTab)); // save the filter only for the selected tab
			}
		}

		/// <summary>
		/// remove the global filtering currently set for all tabs
		/// <param name="filterSentence">the current filter sentence in the filter combo box, when the unfilter all button is pressed</param>
		/// </summary>
		public void unfilterAllTabs(string filterSentence)
		{
			// clear the global filter sentence
			Settings.Default.UIFilterAllSentence = string.Empty;
			Settings.Default.UIFilterAllLibraryTab = false;
			// and iterate on all the tabs
			foreach (TabPage tabPage in this.TabPages)
			{
				PartListView listView = tabPage.Controls[0] as PartListView;
				if (listView != null)
				{
					// if the user hit the "unfilter all button", we need to save the current filter sentence which is displayed 
				    // in the combo box, in the list view, other wise the refiltering will not be in synch with what is displayed
					// in the combo box
					if (tabPage == this.SelectedTab)
						listView.filter(filterSentence, true);
					else
						listView.refilter();
				}
			}
		}

		public void updatePartCountAndBudget(Layer.LayerItem brickOrGroup)
		{
			// and iterate on all the tabs
			foreach (TabPage tabPage in this.TabPages)
			{
				PartListView listView = tabPage.Controls[0] as PartListView;
				if (listView != null)
					listView.updatePartCountAndBudget(brickOrGroup.PartNumber);
			}
			// if the part is a group, also iterate on all it's items
			if (brickOrGroup.IsAGroup)
				foreach (Layer.LayerItem item in (brickOrGroup as Layer.Group).Items)
					this.updatePartCountAndBudget(item);
		}

		/// <summary>
		/// Update all the budget of all the parts in all tabs of the part library
		/// </summary>
		public void updateAllPartCountAndBudget()
		{
			// and iterate on all the tabs
			foreach (TabPage tabPage in this.TabPages)
			{
				PartListView listView = tabPage.Controls[0] as PartListView;
				if (listView != null)
					listView.updateAllPartCountAndBudget();
			}
		}

		public void updateAppearanceAccordingToSettings(bool updateTabOrder, bool updateAppearance, bool updateBudgetCount, bool updateBubbleInfoFormat, bool updateSelectedTab, bool updateCommonFilter)
		{
			// save the selected tab to reselect it after reorder
			TabPage selectedTab = this.SelectedTab;

			// suspend the layout since we will rearrange everything
			this.SuspendLayout();

			if (updateTabOrder)
			{
				// first sort the tabs
				// get the sorted name list from the settings
				System.Collections.Specialized.StringCollection sortedNameList = BlueBrick.Properties.Settings.Default.PartLibTabOrder;

				int insertIndex = 0;
				foreach (string tabName in sortedNameList)
				{
					int currentIndex = this.TabPages.IndexOfKey(tabName);
					if (currentIndex != -1)
					{
						// get the tab page, remove it and reinsert it at the correct position
						TabPage tabPage = this.TabPages[currentIndex];
						this.TabPages.Remove(tabPage); // do not use RemoveAt() that throw an exception even if the index is correct
						this.TabPages.Insert(insertIndex, tabPage);

						// increment the insert point
						if (insertIndex < this.TabPages.Count)
							insertIndex++;
					}
				}
			}

			// update the budget number if needed
			if (updateBudgetCount)
				updateAllPartCountAndBudget();

			if (updateAppearance || updateBubbleInfoFormat)
			{
				// then update the background color and the bubble info status
				bool displayBubbleInfo = Settings.Default.PartLibDisplayBubbleInfo;
				bool displayPartId = Settings.Default.PartLibBubbleInfoPartID;
				bool displayColor = Settings.Default.PartLibBubbleInfoPartColor;
				bool displayDescription = Settings.Default.PartLibBubbleInfoPartDescription;
				foreach (TabPage tabPage in this.TabPages)
				{
					try
					{
						PartListView listView = tabPage.Controls[0] as PartListView;
						listView.updateBackgroundColor();
						listView.ShowItemToolTips = displayBubbleInfo;
						(tabPage.ContextMenuStrip.Items[(int)ContextMenuIndex.SHOW_BUBBLE_INFO] as ToolStripMenuItem).Checked = displayBubbleInfo;
						// update the tooltip text of all the items
						if (updateBubbleInfoFormat)
							foreach (ListViewItem item in listView.Items)
								item.ToolTipText = BrickLibrary.Instance.getFormatedBrickInfo(item.Tag as string,
													displayPartId, displayColor, displayDescription);
					}
					catch
					{
					}
				}
			}

			// global filter if we need to do it
			if (updateCommonFilter && Settings.Default.UIFilterAllLibraryTab && (Settings.Default.UIFilterAllSentence != string.Empty))
				this.filterAllTabs(Settings.Default.UIFilterAllSentence);

			// now resume the layout
			this.ResumeLayout();

			// select the correct tab according to the parameter flag
			// we do it after the resume of the layout otherwise if done during the suspends, the tab may not be correctly visible
			if (updateSelectedTab)
			{
				// the selected tab from the setting data
				int selectedTabIndex = BlueBrick.Properties.Settings.Default.UIPartLibSelectedTabIndex;
				if ((selectedTabIndex >= 0) && (selectedTabIndex < this.TabPages.Count))
					this.SelectTab(BlueBrick.Properties.Settings.Default.UIPartLibSelectedTabIndex);
			}
			else
			{
				// reselect the previous selected tab
				if (selectedTab != null)
					this.SelectTab(selectedTab);
			}

			// redraw the selected tab to have the correct background color for part exceeding budget
            if (this.SelectedTab != null)
			    this.SelectedTab.Invalidate();
		}

		/// <summary>
		/// Update the view style of the list view of each tab, depending if the budgets are displayed or not
		/// </summary>
		public void updateViewStyle()
		{
			// change the view of the list view
			foreach (TabPage tabPage in this.TabPages)
			{
				PartListView listView = tabPage.Controls[0] as PartListView;
				if (listView != null)
					listView.updateViewStyle();
			}
		}

		/// <summary>
		/// Filter or not the part lib with the budgeted parts, depending on the current value of the setting
		/// </summary>
		public void updateFilterOnBudgetedParts()
		{
			// change the view of the list view
			foreach (TabPage tabPage in this.TabPages)
			{
				PartListView listView = tabPage.Controls[0] as PartListView;
				if (listView != null)
					listView.updateFilterOnBudgetedParts();
			}
		}

		public void savePartListDisplayStatusInSettings()
		{
			// reset the setting list
			Settings.Default.UIPartLibDisplayConfig = new System.Collections.Specialized.StringCollection();
			// collect the info for all the pages
			foreach (TabPage tabPage in this.TabPages)
			{
				// get the status
				bool hasCurrentTabLargeIcon = (tabPage.ContextMenuStrip.Items[(int)ContextMenuIndex.LARGE_ICON] as ToolStripMenuItem).Checked;
				bool doesCurrentTabRespectProportion = (tabPage.ContextMenuStrip.Items[(int)ContextMenuIndex.RESPECT_PROPORTION] as ToolStripMenuItem).Checked;
				// construct the string
				string tabConfig = tabPage.Name + (hasCurrentTabLargeIcon ? "1" : "0")
								+ (doesCurrentTabRespectProportion ? "1" : "0")
                                + "?" + (tabPage.Controls[0] as PartListView).FilterSentence;
				// add the new config in the list
				Settings.Default.UIPartLibDisplayConfig.Add(tabConfig);
			}
			// also save the current tab displayed
			Properties.Settings.Default.UIPartLibSelectedTabIndex = this.SelectedIndex;
		}
		#endregion

		#region event handler for parts library

		private void contextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// update the checkstate of the budget stuff, cause they are globals at all the tab
			ContextMenuStrip contextMenu = sender as ContextMenuStrip;
			bool isBudgetVisible = Budget.Budget.Instance.IsExisting;
			// separator
			(contextMenu.Items["budgetSeparator"] as ToolStripSeparator).Visible = isBudgetVisible;
			// show only budget
			ToolStripMenuItem menuItem = (contextMenu.Items["showOnlyBudgetedPartsMenuItem"] as ToolStripMenuItem);
			menuItem.Visible = isBudgetVisible;
			menuItem.Checked = Properties.Settings.Default.ShowOnlyBudgetedParts;
			// show budget number
			menuItem = (contextMenu.Items["showBudgetNumbersMenuItem"] as ToolStripMenuItem);
			menuItem.Visible = isBudgetVisible;
			menuItem.Checked = Properties.Settings.Default.ShowBudgetNumbers;
			// use budget limitation
			menuItem = (contextMenu.Items["useBudgetLimitationMenuItem"] as ToolStripMenuItem);
			menuItem.Visible = isBudgetVisible;
			menuItem.Checked = Properties.Settings.Default.UseBudgetLimitation;
			// budget edition
			menuItem = (contextMenu.Items["editBudgetMenuItem"] as ToolStripMenuItem);
			menuItem.Visible = isBudgetVisible;
			// enable the edition only if a part is selected
			menuItem.Enabled = ((this.SelectedTab.Controls[0] as PartListView).SelectedItems.Count > 0);
		}

		private void menuItem_LargeIconClick(object sender, EventArgs e)
		{
			PartListView listView = this.SelectedTab.Controls[0] as PartListView;
			if (listView != null)
			{
				// also adjust the size of the tile according to the
				// size of the next current image (which is the small one for the moment)
				// we change the size before changing the image list, because the change of
				// the image list will call a refresh anyway
				if (listView.SmallImageList.ImageSize == PART_ITEM_SMALL_SIZE)
					listView.TileSize = PART_ITEM_SMALL_SIZE_WITH_MARGIN;
				else
					listView.TileSize = PART_ITEM_LARGE_SIZE_WITH_MARGIN;

				// switch between large view and small view
				ImageList swap = listView.LargeImageList;
				listView.LargeImageList = listView.SmallImageList;
				listView.SmallImageList = swap;

				// resize the tabcontrol to handle a layout bug when
				// the scroll bar disapear
				this.Size += new Size(1, 1);
			}
		}

		private void menuItem_RespectProportionClick(object sender, EventArgs e)
		{
			ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
			PartListView listView = this.SelectedTab.Controls[0] as PartListView;
			if (listView != null && menuItem != null)
			{
				// we need to reconstruct the image list
				List<Image> imageList = listView.reconstructImageListFromBrickLibrary();
				// regenerate the two image list for the current list view (converting the array into a list)
				fillListViewWithImageList(listView, imageList, menuItem.Checked);
			}
		}

		private void menuItem_DisplayTooltipsClick(object sender, EventArgs e)
		{
			// get the checked status
			bool displayBubbleInfo = (sender as ToolStripMenuItem).Checked;
			// update the setting
			BlueBrick.Properties.Settings.Default.PartLibDisplayBubbleInfo = displayBubbleInfo;
			// the display tooltip is global to all the tabpage, so update all the pages
			foreach (TabPage tabPage in this.TabPages)
			{
				try
				{
					(tabPage.Controls[0] as PartListView).ShowItemToolTips = displayBubbleInfo;
					(tabPage.ContextMenuStrip.Items[(int)ContextMenuIndex.SHOW_BUBBLE_INFO] as ToolStripMenuItem).Checked = displayBubbleInfo;
				}
				catch
				{
				}
			}
		}

		private void menuItem_ShowOnlyBudgetedPartsClick(object sender, EventArgs e)
		{
			// call the event handler of the main form
			MainForm.Instance.showOnlyBudgetedPartsToolStripMenuItem_Click(null, null);
		}

		private void menuItem_ShowBudgetNumberClick(object sender, EventArgs e)
		{
			// call the event handler of the main form
			MainForm.Instance.showBudgetNumbersToolStripMenuItem_Click(null, null);
		}

		private void menuItem_UseBudgetLimitationClick(object sender, EventArgs e)
		{
			// call the event handler of the main form
			MainForm.Instance.useBudgetLimitationToolStripMenuItem_Click(null, null);
		}

		private void menuItem_EditBudgetClick(object sender, EventArgs e)
		{
			ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
			PartListView listView = this.SelectedTab.Controls[0] as PartListView;
			if (listView != null && menuItem != null)
				listView.editCurrentSelectedItemLabel();
		}

		private string getSelectedPartNumberInListView(ListView listView)
		{
			if (listView.SelectedItems.Count > 0)
				return (listView.SelectedItems[0].Tag as string);
			return null;
		}

		private void listView_MouseMove(object sender, MouseEventArgs e)
		{
			// check if we really moved (cause Mono call this even just when we click, before moving)
			if ((e.X != mLastMousePosition.X) || (e.Y != mLastMousePosition.Y))
				mLastMousePosition = e.Location;
			else
				return; // not move, so early exit

			// get the sender listview
			ListView listView = sender as ListView;
			if ((listView == null) && (this.SelectedTab != null) && (this.SelectedTab.Controls.Count > 0))
				listView = this.SelectedTab.Controls[0] as ListView;

			// we need a valid list view
			if (listView != null)
			{
				// move the mouse on the part lib with or without a button pressed.
				// Without: display the info
				// With: start a drag and drop
				switch (e.Button)
				{
					case MouseButtons.None:
						{
							// display the part description but only if there's no button press
							// because else it means we are doing a drag of a part on the map
							ListViewItem item = listView.GetItemAt(e.X, e.Y);
							// construct the message to display
							string message = "";
							if (item != null)
								message = BrickLibrary.Instance.getFormatedBrickInfo(item.Tag as string, true, true, true);

							//display the message in the status bar
							MainForm.Instance.setStatusBarMessage(message);
							break;
						}
					case MouseButtons.Left:
						{
							string partNumber = getSelectedPartNumberInListView(listView);
							if (partNumber != null)
							{
								// set the internal part number because the data cannot be passed in mono
								DraggingPartNumber = partNumber;
								// the normal code to do a drag and drop
								DataObject data = new DataObject();
								data.SetData(DataFormats.StringFormat, partNumber);
								this.DoDragDrop(data, DragDropEffects.Copy);
							}
							break;
						}
				}
			}
		}

		private void PartLibraryPanel_GiveFeedback(object sender, GiveFeedbackEventArgs e)
		{
			// we use our own cursors
			e.UseDefaultCursors = false;
			// check if the selected layer is a brick layer, otherwise, we can't drop a part
			if ((e.Effect == DragDropEffects.Copy) && (Map.Instance.canAddBrick(DraggingPartNumber) == Map.BrickAddability.YES))
			{
				Cursor.Current = MainForm.Instance.BrickDuplicateCursor;
			}
			else
			{
				// by default use the NO cursor
				// but first set it to Arrow cursor because there's a bug in Mono, if the Cursor.Current is already
				// set to No, but actually the cursor was changed elsewhere, it doesn't change it again. So change it
				// first to anything else to be sure that it will be changed really to NO cursor
				Cursor.Current = Cursors.Arrow;
				Cursor.Current = Cursors.No;
			}
		}

		private void PartLibraryPanel_SelectedIndexChanged(object sender, EventArgs e)
		{
			// if we don't have a global filtering, update the filter combo box, with the 
			// filter of the new current tab
			if (!Settings.Default.UIFilterAllLibraryTab && (this.SelectedTab != null) && (this.SelectedTab.Controls.Count > 0))
			{
				PartListView listView = this.SelectedTab.Controls[0] as PartListView;
				if (listView != null)
					MainForm.Instance.updateFilterComboBox(listView.FilterSentence);
			}
		}
		#endregion
	}
}
