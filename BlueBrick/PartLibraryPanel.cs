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
		private ContextMenuStrip createContextMenuItemForATabPage(bool respectProportionIsChecked)
		{
			// create the context menu
			ContextMenuStrip contextMenu = new ContextMenuStrip();
			// menu item to display the icons in large
			ToolStripMenuItem largeIconsMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemLargeIcons, null, menuItem_LargeIconClick);
			largeIconsMenuItem.CheckOnClick = true;
			largeIconsMenuItem.Checked = true;
			contextMenu.Items.Add(largeIconsMenuItem);
			// menu item to repect the proportions
			ToolStripMenuItem proportionMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemRespectProportion, null, menuItem_RespectProportionClick);
			proportionMenuItem.CheckOnClick = true;
			proportionMenuItem.Checked = respectProportionIsChecked;
			contextMenu.Items.Add(proportionMenuItem);
			// menu item to display tooltips
			ToolStripMenuItem bubbleInfoMenuItem = new ToolStripMenuItem(Resources.PartLibMenuItemDisplayTooltips, null, menuItem_DisplayTooltipsClick);
			bubbleInfoMenuItem.CheckOnClick = true;
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
				// get all the folders in the parts folder to create a tab for each folder found
				DirectoryInfo[] categoryFolder = partsFolder.GetDirectories();
				foreach (DirectoryInfo category in categoryFolder)
				{
					// add the tab in the tab control, based on the name of the folder
					TabPage newTabPage = new TabPage(category.Name);
					newTabPage.Name = category.Name;
					newTabPage.ContextMenuStrip = createContextMenuItemForATabPage(false);
					this.TabPages.Add(newTabPage);

					// then for the new tab added, we add a list control to 
					// fill it with the pictures found in that folder
					ListView newListView = new ListView();
					// set the property of the list view
					newListView.Anchor = AnchorStyles.Top | AnchorStyles.Left;
					newListView.Dock = DockStyle.Fill;
					newListView.Alignment = ListViewAlignment.SnapToGrid;
					newListView.BackColor = Settings.Default.PartLibBackColor;
					newListView.ShowItemToolTips = false; //item tooltip are the name of the image file
					newListView.MultiSelect = false;
					newListView.View = View.Tile; // we always use this view, because of the layout of the item
					newListView.TileSize = PART_ITEM_LARGE_SIZE_WITH_MARGIN;
					newListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseClick);
					newListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listView_MouseClick);
					newListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listView_MouseMove);
					// add the list view to the tab page
					newTabPage.Controls.Add(newListView);

					// fill the list view with the file name
					fillListViewWithParts(newListView, category);
				}

				// after creating all the tabs, sort them according to the settings
				updateAppearanceAccordingToSettings();
			}
		}

		private void fillListViewWithParts(ListView listViewToFill, DirectoryInfo folder)
		{
			// create a list of image to load all the images in a list
			List<Image> imageList = new List<Image>();
			List<string> imageFileUnloadable = new List<string>();
			List<string> xmlFileUnloadable = new List<string>();
			List<string> xmlFileLoaded = new List<string>();

			// get the list of image in the folder
			FileInfo[] imageFiles = folder.GetFiles("*.gif");
			int imageIndex = 0;
			foreach (FileInfo file in imageFiles)
			{
				try
				{
					// read the image from the file
					Bitmap image = new Bitmap(file.FullName);

					try
					{
						// get the name without extension and use upper case
						string name = file.Name.Substring(0, file.Name.Length - 4).ToUpper();

						// put the image in the database
						string xmlFileName = file.FullName.Substring(0, file.FullName.Length - 3) + "xml";
						BrickLibrary.Instance.AddBrick(name, image, xmlFileName);
						xmlFileLoaded.Add(xmlFileName);

						// add the image in the image list
						imageList.Add(image);

						// create a new item for the list view item
						ListViewItem newItem = new ListViewItem(null as string, imageIndex);
						newItem.ToolTipText = name;
						newItem.Tag = name;
						listViewToFill.Items.Add(newItem);
						imageIndex++;
					}
					catch
					{
						// add the file that can't be loaded in the list of problems
						xmlFileUnloadable.Add(file.FullName);
					}
				}
				catch
				{
					// add the file that can't be loaded in the list of problems
					imageFileUnloadable.Add(file.FullName);
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
						// and add the brick with a null image
						BrickLibrary.Instance.AddBrick(name, null, file.FullName);
					}
				}
				catch
				{
					// add the file that can't be loaded in the list of problems
					xmlFileUnloadable.Add(file.FullName);
				}
			}

			// check if there was some error with some files
			string message = null;
			if (imageFileUnloadable.Count > 0)
			{
				// display a warning message
				message = Properties.Resources.ErrorMsgCanNotLoadImage;
				foreach (string filename in imageFileUnloadable)
					message += "\n" + filename;
			}
			if (xmlFileUnloadable.Count > 0)
			{
				if (message == null)
					message = "";
				else
					message += "\n\n";
				// display a warning message
				message += Properties.Resources.ErrorMsgCanNotLoadPartXML;
				foreach (string filename in xmlFileUnloadable)
					message += "\n" + filename;
			}

			// display the error message if there was some errors
			if (message != null)
			{
				MessageBox.Show(null, message,
					Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
					MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
			}

			// then fill the list view
			fillListViewFromImageList(listViewToFill, imageList, false);
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

		public void updateAppearanceAccordingToSettings()
		{
			this.SuspendLayout();

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

			// then update the background color and the bubble info status
			bool displayBubbleInfo = BlueBrick.Properties.Settings.Default.PartLibDisplayBubbleInfo;
			foreach (TabPage tabPage in this.TabPages)
			{
				try
				{
					ListView listView = tabPage.Controls[0] as ListView;
					listView.BackColor = BlueBrick.Properties.Settings.Default.PartLibBackColor;
					listView.ShowItemToolTips = displayBubbleInfo;
					(tabPage.ContextMenuStrip.Items[2] as ToolStripMenuItem).Checked = displayBubbleInfo;
				}
				catch
				{
				}
			}

			this.ResumeLayout();
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
					(tabPage.ContextMenuStrip.Items[2] as ToolStripMenuItem).Checked = displayBubbleInfo;
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
						message = BrickLibrary.Instance.getFormatedBrickInfo(item.Tag as string);

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
