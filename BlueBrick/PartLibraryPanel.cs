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

namespace BlueBrick
{
	public partial class PartLibraryPanel : TabControl
	{
		private Size PART_ITEM_SMALL_SIZE = new Size(64, 64);
		private Size PART_ITEM_LARGE_SIZE = new Size(128, 128);
		private Size PART_ITEM_SMALL_SIZE_WITH_MARGIN = new Size(69, 69);
		private Size PART_ITEM_LARGE_SIZE_WITH_MARGIN = new Size(134, 134);

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
			public PartLibDisplaySetting(bool largeIcon, bool respectProportion)
			{
				mLargeIcons = largeIcon;
				mRespectProportion = respectProportion;
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
			public ListView mListView = new ListView(); // the list view for this category folder
			public List<Image> mImageList = new List<Image>(); // all the image not resized in this folder
			public bool mRespectProportion = true; // the proportion flag for this category
			public List<BrickLibrary.Brick> mGroupList = new List<BrickLibrary.Brick>(); // all the group parts in this folder
		}

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
			// return the well form context menu
			return contextMenu;
		}

		/// <summary>
		/// parse the part folder to find all the part in the library
		/// </summary>
		public void initPartsTabControl()
		{
			// init the part tab control based on the folders found on the drive
			// first clear the tab control
			this.TabPages.Clear();
			// then search the "parts" folder, if not here maybe we should display
			// an error message (something wrong with the installation of the application?)
			DirectoryInfo partsFolder = new DirectoryInfo(Application.StartupPath + @"/parts");
			if (partsFolder.Exists)
			{
				// create from the Settings a dictionary to store the display status of each tab
				Dictionary<string, PartLibDisplaySetting> tabDisplayStatus = new Dictionary<string,PartLibDisplaySetting>();
				foreach (string tabConfig in Settings.Default.UIPartLibDisplayConfig)
					tabDisplayStatus.Add(tabConfig.Remove(tabConfig.Length - 2), new PartLibDisplaySetting(tabConfig[tabConfig.Length - 2] == '1', tabConfig[tabConfig.Length - 1] == '1'));

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
						displaySetting = new PartLibDisplaySetting(true, false);

					// create a building info and add it to the list
					CategoryBuildingInfo buildingInfo = new CategoryBuildingInfo();
					categoryList.Add(buildingInfo);

					// save the proportion flag in the building info
					buildingInfo.mRespectProportion = displaySetting.mRespectProportion;

					// add the tab in the tab control, based on the name of the folder
					TabPage newTabPage = new TabPage(category.Name);
					newTabPage.Name = category.Name;
					newTabPage.ContextMenuStrip = createContextMenuItemForATabPage(displaySetting.mLargeIcons, displaySetting.mRespectProportion);
					this.TabPages.Add(newTabPage);

					// then for the new tab added, we add a list control to 
					// fill it with the pictures found in that folder
					ListView newListView = buildingInfo.mListView; // get a shortcut on the list view
					// set the property of the list view
					newListView.Anchor = AnchorStyles.Top | AnchorStyles.Left;
					newListView.Dock = DockStyle.Fill;
					newListView.Alignment = ListViewAlignment.SnapToGrid;
					newListView.BackColor = Settings.Default.PartLibBackColor;
					newListView.ShowItemToolTips = Settings.Default.PartLibDisplayBubbleInfo;
					newListView.MultiSelect = false;
					newListView.View = View.Tile; // we always use this view, because of the layout of the item
					if (displaySetting.mLargeIcons)
						newListView.TileSize = PART_ITEM_LARGE_SIZE_WITH_MARGIN;
					else
						newListView.TileSize = PART_ITEM_SMALL_SIZE_WITH_MARGIN;
					newListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseClick);
					newListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseClick);
					newListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView_MouseMove);
					// add the list view to the tab page
					newTabPage.Controls.Add(newListView);

					// fill the list view with the file name
					fillListViewWithParts(buildingInfo, category);
				}

				// reiterate on a second pass on all the list view, because we need to add the group parts
				foreach (CategoryBuildingInfo buildingInfo in categoryList)
				{
					// add the group parts in the list view. We need to do it after parsing the whole library
					// because a group part can reference any part in any folder
					fillListViewWithGroups(buildingInfo);
					// then fill the list view (we cannot pass the building info as parameter because this function is called elsewhere)
					fillListViewFromImageList(buildingInfo.mListView, buildingInfo.mImageList, buildingInfo.mRespectProportion);
				}

				// after creating all the tabs, sort them according to the settings
				updateAppearanceAccordingToSettings(true, false, false);
			}
		}

		private void addOnePartInListView(CategoryBuildingInfo buildingInfo, string partName, Bitmap image)
        {
            // create a new item for the list view item
			ListViewItem newItem = new ListViewItem(null as string, buildingInfo.mImageList.Count);
            newItem.ToolTipText = BrickLibrary.Instance.getFormatedBrickInfo(partName,
                                                        Settings.Default.PartLibBubbleInfoPartID,
                                                        Settings.Default.PartLibBubbleInfoPartColor,
                                                        Settings.Default.PartLibBubbleInfoPartDescription);
            newItem.Tag = partName;
			buildingInfo.mListView.Items.Add(newItem);

			// add the image in the image list (after using the imageList.Count)
			buildingInfo.mImageList.Add(image);
        }

		private void fillListViewWithParts(CategoryBuildingInfo buildingInfo, DirectoryInfo folder)
		{
			// create a list of image to load all the images in a list
			List<FileNameWithException> imageFileUnloadable = new List<FileNameWithException>();
			List<FileNameWithException> xmlFileUnloadable = new List<FileNameWithException>();
			List<string> xmlFileLoaded = new List<string>();

			// get the list of image in the folder
			FileInfo[] imageFiles = folder.GetFiles("*.gif");
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
						string name = file.Name.Substring(0, file.Name.Length - 4).ToUpper();

						// push the xml file name in the list first because we don't want to try to load
						// it a second time if an exception is raised.
						xmlFileLoaded.Add(xmlFileName);

						// put the image in the database
						BrickLibrary.Instance.AddBrick(name, image, xmlFileName);

                        // add this part into the listview
						addOnePartInListView(buildingInfo, name, image);
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

			// now check if there's xml files without GIF. In that case we still load them but these
			// parts will be ignored by BlueBrick
			FileInfo[] xmlFiles = folder.GetFiles("*.xml");
			foreach (FileInfo file in xmlFiles)
			{
				try
				{
					if (!xmlFileLoaded.Contains(file.FullName))
					{
						// get the name without extension and use upper case
						string name = file.Name.Substring(0, file.Name.Length - 4).ToUpper();

                        // add the brick in the library
                        BrickLibrary.Brick brickAdded = BrickLibrary.Instance.AddBrick(name, null, file.FullName);

                        // if the image is a group, add it to the group list
                        if (brickAdded.IsAGroup)
							buildingInfo.mGroupList.Add(brickAdded);
					}
				}
				catch (Exception e)
				{
					// add the file that can't be loaded in the list of problems
					xmlFileUnloadable.Add(new FileNameWithException(file.FullName, e.Message));
				}
			}

			// check if there was some error with some files
			string message = null;
			string details = "";
			if (imageFileUnloadable.Count > 0)
			{
				// display a warning message
				message = Properties.Resources.ErrorMsgCanNotLoadImage;
				foreach (FileNameWithException error in imageFileUnloadable)
				{
					message += "\n" + error.mFilename;
					details += error.ShortFileName + ":\r\n" + error.mException + "\r\n\r\n";
				}
			}
			if (xmlFileUnloadable.Count > 0)
			{
				if (message == null)
					message = "";
				else
					message += "\n\n";
				// display a warning message
				message += Properties.Resources.ErrorMsgCanNotLoadPartXML;
				foreach (FileNameWithException error in xmlFileUnloadable)
				{
					message += "\n" + error.mFilename;
					details += error.ShortFileName + ":\r\n" + error.mException + "\r\n\r\n";
				}
			}

			// display the error message if there was some errors
			if (message != null)
			{
				LoadErrorForm messageBox = new LoadErrorForm(Properties.Resources.ErrorMsgTitleWarning, message, details);
				messageBox.Show();
			}
		}

		private void fillListViewWithGroups(CategoryBuildingInfo buildingInfo)
        {
			// do a third pass to generate and add the group parts in the library
			// we need to do it in a third pass in order to have read all the group xml files
			foreach (BrickLibrary.Brick group in buildingInfo.mGroupList)
			{
				// generate the image for the group
				if (BrickLibrary.Instance.createGroupImage(group))
				{
					// add this brick in the list view
					addOnePartInListView(buildingInfo, group.mPartNumber, group.Image as Bitmap);
				}
			}

        }

		private void fillListViewFromImageList(ListView listViewToFill, List<Image> imageList, bool respectProportion)
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

			// create two image list that will receive a snapshot of each image
			// found in the folder
			ImageList largeImageList = new ImageList();
			largeImageList.ImageSize = PART_ITEM_LARGE_SIZE;
			ImageList smallImageList = new ImageList();
			smallImageList.ImageSize = PART_ITEM_SMALL_SIZE;

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
				graphics.Clear(Color.Transparent);
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

		public void updateAppearanceAccordingToSettings(bool updateTabOrder, bool updateAppearance, bool updateBubbleInfoFormat)
		{
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
						ListView listView = tabPage.Controls[0] as ListView;
						listView.BackColor = Settings.Default.PartLibBackColor;
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

			this.ResumeLayout();
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
								+ (doesCurrentTabRespectProportion ? "1" : "0");
				// add the new config in the list
				Settings.Default.UIPartLibDisplayConfig.Add(tabConfig);
			}
		}
		#endregion
		#region event handler for parts library

		private void menuItem_LargeIconClick(object sender, EventArgs e)
		{
			ListView listView = this.SelectedTab.Controls[0] as ListView;
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
			ListView listView = this.SelectedTab.Controls[0] as ListView;
			if (listView != null && menuItem != null)
			{
				// create a list of image with all the original part image from the part lib
				List<Image> imageList = new List<Image>(listView.Items.Count);
				foreach (ListViewItem item in listView.Items)
					imageList.Add(BrickLibrary.Instance.getImage(item.Tag as string));

				// regenerate the two image list for the current list view
				fillListViewFromImageList(listView, imageList, menuItem.Checked);
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
					(tabPage.Controls[0] as ListView).ShowItemToolTips = displayBubbleInfo;
					(tabPage.ContextMenuStrip.Items[(int)ContextMenuIndex.SHOW_BUBBLE_INFO] as ToolStripMenuItem).Checked = displayBubbleInfo;
				}
				catch
				{
				}
			}
		}

		private void listView_MouseClick(object sender, MouseEventArgs e)
		{
			// try to get the list view (it can also be the tab page that contains the list view)
			ListView listView = sender as ListView;
			if (listView != null)
			{
				// for a right click
				switch (e.Button)
				{
					case MouseButtons.Left:
						{
							string partNumber = MainForm.Instance.getSelectedPartNumberInPartLib();
							if (partNumber != null)
								Map.Instance.addConnectBrick(partNumber);
							break;
						}
				}
			}
		}

		private void listView_MouseMove(object sender, MouseEventArgs e)
		{
			// display the part description but only if there's no button press
			// because else it means we are doing a drag of a part on the map
			if (e.Button == MouseButtons.None)
			{
				ListView listView = sender as ListView;
				if (listView == null)
					listView = this.SelectedTab.Controls[0] as ListView;

				if (listView != null)
				{
					ListViewItem item = listView.GetItemAt(e.X, e.Y);
					// construct the message to display
					string message = "";
					if (item != null)
						message = BrickLibrary.Instance.getFormatedBrickInfo(item.Tag as string, true, true, true);

					//display the message in the status bar
					MainForm.Instance.setStatusBarMessage(message);
				}
			}
		}

		private void PartLibraryPanel_MouseLeave(object sender, EventArgs e)
		{
			// clear the selected item when the mouse leave the control unless
			// the left button is still press (that means the user wants to do
			// a drag'n'drop of a part.
			if ((Control.MouseButtons != MouseButtons.Left) && (this.SelectedTab != null))
			{
				if (this.SelectedTab.Controls.Count > 0)
				{
					ListView selectedListView = this.SelectedTab.Controls[0] as ListView;
					selectedListView.SelectedItems.Clear();
				}
			}
		}
		#endregion
	}
}
