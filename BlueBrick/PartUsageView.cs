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
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;
using System.IO;

namespace BlueBrick
{
	public partial class PartUsageView : ListView
	{
		enum ColumnId
		{
			PART_ID = 0,
			PART_COUNT,
			COLOR,
			DESCRIPTION,
			BUDGET_COUNT,
			MISSING_COUNT,
			PART_USAGE,
		}

		private class IconEntry
		{
			public Bitmap mImage = null;
			public int mImageIndex = 0;

			/// <summary>
			/// create an icon and store it in the specified image list and keep the index
			/// </summary>
			public IconEntry(string partNumber, ImageList imageList)
			{
				// get the image of the part from the library
				Image originalPartImage = BrickLibrary.Instance.getImage(partNumber);
				// create a snapshot of the current image and replace it in the two image lists
				// but to avoid a stretching effect, we redraw the picture in a square
				// first compute the position and size of the bitmap to draw
				int maxSize = Math.Max(originalPartImage.Width, originalPartImage.Height);
				Rectangle drawRectangle = new Rectangle();
				drawRectangle.Width = (16 * originalPartImage.Width) / maxSize;
				drawRectangle.Height = (16 * originalPartImage.Height) / maxSize;
				drawRectangle.X = (16 - drawRectangle.Width) / 2;
				drawRectangle.Y = (16 - drawRectangle.Height) / 2;
				// create a new bitmap to draw the scaled part
				mImage = new Bitmap(16, 16);
				Graphics graphics = Graphics.FromImage(mImage);
				graphics.Clear(Color.Transparent);
				graphics.DrawImage(originalPartImage, drawRectangle);
				graphics.Flush();
				// add the thumbnailImage to the list
				mImageIndex = imageList.Images.Count;
				imageList.Images.Add(mImage);
			}
		}

		private class BrickEntry
		{
			private const int MAX_RED_VALUE = 227;
			private string mPartNumber = string.Empty;
			private int mQuantity = 0;
			private int mImageIndex = 0;
			private ListViewItem mItem = null;

			public bool IsQuantityNull
			{
				get { return (mQuantity == 0); }
			}

			public ListViewItem Item
			{
				get { return mItem; }
			}

			/// <summary>
			/// Create a brick entry, and also create a thumbnail image of this part
			/// </summary>
			/// <param name="partNumber">The id of the part for this brick entry</param>
			/// <param name="imageIndex">The index of the image to use</param>
			/// <param name="shouldIncludeHiddenParts">Tell if we should count the hidden parts when computing the part usage</param>
			public BrickEntry(string partNumber, int imageIndex, bool shouldIncludeHiddenParts)
			{
				mPartNumber = partNumber;
				mImageIndex = imageIndex;

				// get the description and color from the database
				string[] brickInfo = BrickLibrary.Instance.getBrickInfo(mPartNumber);
				// the first text of the array must be the part number because it is treated as the item text itself
				// and the columns are defined in this order: part, quantity, color and description
				// even if the display order is different (quantity, part, color and description)
				string[] itemTexts = { brickInfo[0], mQuantity.ToString(), brickInfo[2], brickInfo[3], Properties.Resources.TextNA, Properties.Resources.TextNA, Properties.Resources.TextNA };
				mItem = new ListViewItem(itemTexts, mImageIndex);
				mItem.SubItems[(int)ColumnId.COLOR].Tag = brickInfo[1]; // store the color index in the tag of the color subitem, used in the html export
				// activate the style for subitems because we have a budget in different colors
				mItem.UseItemStyleForSubItems = false;
				// set my tag to zero, in order for the sum to work in any way
				mItem.SubItems[(int)ColumnId.BUDGET_COUNT].Tag = 0;
				mItem.SubItems[(int)ColumnId.MISSING_COUNT].Tag = 0;
				// update the part usage percentage if there's some budget
				if (Budget.Budget.Instance.IsExisting)
					updateUsagePercentageForPart(shouldIncludeHiddenParts);
			}

			/// <summary>
			/// Create a brick entry that is a sum for a layer
			/// </summary>
			/// <param name="brickLayer">The layer associated with this sum</param>
			/// <param name="shouldIncludeHiddenParts">Tell if we should count the hidden parts or not when initializing the sum</param>
			public BrickEntry(LayerBrick brickLayer, bool shouldIncludeHiddenParts)
			{
				// create a list view item with the total count and the total part usage
				string[] itemTexts = { Properties.Resources.TextTotal, mQuantity.ToString(), string.Empty, string.Empty, Properties.Resources.TextNA, Properties.Resources.TextNA, Properties.Resources.TextNA };
				mItem = new ListViewItem(itemTexts);
				// set a color
				mItem.UseItemStyleForSubItems = true; // use the same color for the whole line, even the budget
				mItem.ForeColor = Color.MediumBlue;
				mItem.BackColor = Color.MintCream;
				// add a tag to the item, so that it can be sorted, always at the bottom
				if (brickLayer != null)
					mItem.Tag = brickLayer;
				else
					mItem.Tag = mItem;
				// set my tag to zero, in order for the sum to work in any way
				mItem.SubItems[(int)ColumnId.BUDGET_COUNT].Tag = 0;
				mItem.SubItems[(int)ColumnId.MISSING_COUNT].Tag = 0;
				// update the part usage percentage if there's some budget
				if (Budget.Budget.Instance.IsExisting)
					updateUsagePercentageForLayerSum(shouldIncludeHiddenParts);
			}

			public void incrementQuantity()
			{
				mQuantity++;
				mItem.SubItems[(int)ColumnId.PART_COUNT].Text = mQuantity.ToString();
			}

			public void decrementQuantity()
			{
				mQuantity--;
				mItem.SubItems[(int)ColumnId.PART_COUNT].Text = mQuantity.ToString();
			}

			public void updateUsagePercentageForPart(bool shouldIncludeHiddenParts)
			{
				// get the total count of the part all layer included
				int partCount = 0;
				int partBudget = 0;
				int missingCount = 0;

				// get the current budget for this part
				string budgetCountAsString;
				string usageAsString;

				// we should not use the mQuantity to compute the budget percentage, because this quantity is only for this
				// group, but the part can appear in multiple group (on multiple layer), and the budget is an overall budget
				// all part included, so let the Budget class to use its own count of part
				float usagePercentage = Budget.Budget.Instance.getUsagePercentage(mPartNumber, shouldIncludeHiddenParts, out partCount, out partBudget);
				if (usagePercentage < 0)
				{
					// illimited budget
					budgetCountAsString = Properties.Resources.TextUnbudgeted;
					mItem.SubItems[(int)ColumnId.BUDGET_COUNT].ForeColor = Color.BlueViolet;
					missingCount = partCount; // if not budgeted, then we need all of them
					mItem.SubItems[(int)ColumnId.MISSING_COUNT].ForeColor = Color.BlueViolet;
					usageAsString = Properties.Resources.TextUnbudgeted;
					mItem.SubItems[(int)ColumnId.PART_USAGE].ForeColor = Color.BlueViolet;
				}
				else
				{
					budgetCountAsString = partBudget.ToString();
					mItem.SubItems[(int)ColumnId.BUDGET_COUNT].ForeColor = Color.Black;
					missingCount = (partCount <= partBudget) ? 0 : (partCount - partBudget);
					mItem.SubItems[(int)ColumnId.MISSING_COUNT].ForeColor = (missingCount == 0) ? Color.Black : Color.Red;
					usageAsString = DownloadCenterForm.ComputePercentageBarAsString(usagePercentage);
					mItem.SubItems[(int)ColumnId.PART_USAGE].ForeColor = DownloadCenterForm.ComputeColorFromPercentage((int)usagePercentage, MAX_RED_VALUE, true);
				}

				mItem.SubItems[(int)ColumnId.BUDGET_COUNT].Text = budgetCountAsString;
				mItem.SubItems[(int)ColumnId.BUDGET_COUNT].Tag = partBudget;
				mItem.SubItems[(int)ColumnId.MISSING_COUNT].Text = missingCount.ToString();
				mItem.SubItems[(int)ColumnId.MISSING_COUNT].Tag = missingCount;
				mItem.SubItems[(int)ColumnId.PART_USAGE].Text = usageAsString;
			}

			public void updateUsagePercentageForLayerSum(bool shouldIncludeHiddenParts)
			{
				// get the total count of the part all layer included
				int totalPartCount = 0;
				int partBudget = 0;
				float usagePercentage = 0f;

				// check if this layer sum is a sum for a layer or for the whole map
				if (mItem.Tag is LayerBrick)
				{
					LayerBrick layerBrick = this.mItem.Tag as LayerBrick;
					totalPartCount = Budget.Budget.Instance.getTotalCountForLayer(layerBrick, false);
					usagePercentage = Budget.Budget.Instance.getUsagePercentageForLayer(layerBrick, out partBudget);
				}
				else
				{
					totalPartCount = Budget.Budget.Instance.getTotalCount(false, shouldIncludeHiddenParts);
					usagePercentage = Budget.Budget.Instance.getTotalUsagePercentage(shouldIncludeHiddenParts, out partBudget);
				}

				mItem.SubItems[(int)ColumnId.BUDGET_COUNT].Text = partBudget.ToString();
				mItem.SubItems[(int)ColumnId.MISSING_COUNT].Text = sumUpTagOfMyGroup((int)ColumnId.MISSING_COUNT).ToString();
				mItem.SubItems[(int)ColumnId.PART_USAGE].Text = DownloadCenterForm.ComputePercentageBarAsString(usagePercentage);
				mItem.SubItems[(int)ColumnId.PART_USAGE].ForeColor = DownloadCenterForm.ComputeColorFromPercentage((int)usagePercentage, MAX_RED_VALUE, true);
			}

			private int sumUpTagOfMyGroup(int columnId)
			{
				int total = 0;
				if (this.mItem.Group != null)
					foreach (ListViewItem item in this.mItem.Group.Items)
						total += (int)item.SubItems[columnId].Tag;
				else if (this.mItem.ListView != null)
					foreach (ListViewItem item in this.mItem.ListView.Items)
						total += (int)item.SubItems[columnId].Tag;
				return total;
			}
		}

		private class GroupEntry
		{
			public Dictionary<string, BrickEntry> mBrickEntryList = new Dictionary<string, BrickEntry>();
			public LayerBrick mLayer = null;
			public BrickEntry mBrickEntrySumLine = null;
			private ListViewGroup mGroup = null;

			public ListViewGroup Group
			{
				get { return mGroup; }
			}

			public GroupEntry(LayerBrick layer, PartUsageView listView)
			{
				// save the layer
				mLayer = layer;

				// create the specific brick entry for the sum line
				mBrickEntrySumLine = new BrickEntry(layer, listView.IncludeHiddenLayers);

				// if the layer is not null, that means we use group
				if (layer != null)
				{
					// so create the group
					mGroup = new ListViewGroup(layer.Name);
					// and assign the correct group to the sum line
					mBrickEntrySumLine.Item.Group = mGroup;
				}

				// and add the line to the list view
				listView.Items.Add(mBrickEntrySumLine.Item);
			}
		}

		/// <summary>
		/// This class is used to sort the column of the list view
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
				// check if the tag is not null, that means it is the sum line and should appear at the end
				ListViewItem xItem = x as ListViewItem;
				if (xItem.Tag != null)
					return int.MaxValue;
				ListViewItem yItem = y as ListViewItem;
				if (yItem.Tag != null)
					return int.MinValue;
				// use a string compare with the correct column
				int result = string.Compare(xItem.SubItems[mColumn].Text, yItem.SubItems[mColumn].Text);
				// check the order
				if (mOrder == SortOrder.Descending)
					result = -result;
				return result;
			}
		}


		private bool mSplitPartPerLayer = true;
		public bool SplitPartPerLayer
		{
			get { return mSplitPartPerLayer; }
			set
			{
				// check if the value has actually changed (to avoid rebuilding list for nothing)
				if (mSplitPartPerLayer != value)
				{
					mSplitPartPerLayer = value;
					rebuildList();
				}
			}
		}

		private bool mIncludeHiddenLayers = false;
		public bool IncludeHiddenLayers
		{
			get { return mIncludeHiddenLayers; }
			set
			{
				// check if the value has actually changed (to avoid rebuilding list for nothing)
				if (mIncludeHiddenLayers != value)
				{
					mIncludeHiddenLayers = value;
					rebuildList();
				}
			}
		}

		private List<GroupEntry> mGroupEntryList = new List<GroupEntry>();
		private Dictionary<string, IconEntry> mThumbnailImage = new Dictionary<string, IconEntry>();

		//save the last sorted column
		private int mLastColumnSortedIndex = -1;

		#region constructor
		public PartUsageView()
		{
			InitializeComponent();

			// set the size of the image (do not change)
			this.SmallImageList = new ImageList();
			this.SmallImageList.ImageSize = new Size(16, 16);

			// sort the first column
			OnColumnClick(new ColumnClickEventArgs(0));
		}
		#endregion
		#region update

		/// <summary>
		/// this method should be called when a new layer is added
		/// </summary>
		public void addLayerNotification(LayerBrick newLayer)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (newLayer == null || !this.Visible)
				return;

			addLayer(newLayer);
		}

		/// <summary>
		/// this method should be called when a new layer is removed
		/// </summary>
		public void removeLayerNotification(LayerBrick deletedLayer)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (deletedLayer == null || !this.Visible)
				return;

			removeLayer(deletedLayer);
		}

		/// <summary>
		/// this method should be called when a new layer is renamed
		/// </summary>
		public void renameLayerNotification(LayerBrick layer)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (layer == null || !this.Visible || !this.SplitPartPerLayer)
				return;

			// search the layer
			foreach (GroupEntry groupEntry in mGroupEntryList)
				if (groupEntry.mLayer == layer)
				{
					groupEntry.Group.Header = layer.Name;
					break;
				}
		}

		/// <summary>
		/// Find and return the group entry in the list that correspond to the specified layer.
		/// </summary>
		/// <param name="layer">The layer you want to search</param>
		/// <param name="createOneIfMissing">if true a group entry will be created if we can't find a match, otherwise null will be returned</param>
		/// <returns>the group entry corresponding to the specified layer or null if it can't be find and it was not specified to create one</returns>
		private GroupEntry getGroupEntryFromLayer(LayerBrick layer, bool createOneIfMissing)
		{
			// search the group entry associated with this layer
			GroupEntry currentGroupEntry = null;
			if (this.SplitPartPerLayer)
			{
				foreach (GroupEntry groupEntry in mGroupEntryList)
					if (groupEntry.mLayer == layer)
					{
						currentGroupEntry = groupEntry;
						break;
					}
				// if the group entry is not found we create one
				if ((currentGroupEntry == null) && createOneIfMissing)
				{
					currentGroupEntry = new GroupEntry(layer, this);
					mGroupEntryList.Add(currentGroupEntry);
					this.Groups.Add(currentGroupEntry.Group);
				}
			}
			else
			{
				currentGroupEntry = mGroupEntryList[0];
			}

			return currentGroupEntry;
		}

		/// <summary>
		/// Update the part usage percentage for the specified part number, everywhere it appears in the list.
		/// The specified part can appear multiple time if the part list is split by layer, and if the specified part
		/// appears in multiple layers
		/// </summary>
		/// <param name="partNumber">The part id for which the budget has been updated.</param>
		public void updateBudgetNotification(string partNumber)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			// or if there's no budget open
			if (!this.Visible || !Budget.Budget.Instance.IsExisting)
				return;

			// iterate on all the group entry, because the specified part can be in multiple group (if the part list is split by layers)
			foreach (GroupEntry groupEntry in mGroupEntryList)
			{
				// try to get an entry in the dictionnary for the specified brick
				// if we find it, update its usage percentage, otherwise just ignore it
				BrickEntry brickEntry = null;
				if (groupEntry.mBrickEntryList.TryGetValue(partNumber, out brickEntry))
				{
					// update the percentage of the found brick
					brickEntry.updateUsagePercentageForPart(this.IncludeHiddenLayers);
					// since we found the brick in this group entry, update also the percentage of the sum line
					groupEntry.mBrickEntrySumLine.updateUsagePercentageForLayerSum(this.IncludeHiddenLayers);
				}
			}

			// if it is currently sorted by budget, we need to resort
			if (mLastColumnSortedIndex == 2)
				this.Sort();
		}

		/// <summary>
		/// Update all the part usage percentage of all the brick in the list view
		/// </summary>
		public void updateBudgetNotification()
		{
			// when the budget status is changed (open or close a budget), we need first to add or remove the column, even if the list view is not visible
			showHideBudgetColumns(Budget.Budget.Instance.IsExisting);

			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			// or if there's no budget open
			if (!this.Visible || !Budget.Budget.Instance.IsExisting)
				return;

			// iterate on all brick entrey on all the group entry to update the usage percentage
			foreach (GroupEntry groupEntry in mGroupEntryList)
			{
				// update all the other brick entries
				foreach (BrickEntry brickEntry in groupEntry.mBrickEntryList.Values)
					brickEntry.updateUsagePercentageForPart(this.IncludeHiddenLayers);
				// and also update the percentage of the sum brick entry after, so that the missing count is correctly computed
				groupEntry.mBrickEntrySumLine.updateUsagePercentageForLayerSum(this.IncludeHiddenLayers);
			}

			// if it is currently sorted by budget, we need to resort
			if (mLastColumnSortedIndex == 2)
				this.Sort();
		}

		/// <summary>
		/// This method should be called when a budget is active on the map
		/// </summary>
		private void showHideBudgetColumns(bool shouldShow)
		{
			if (shouldShow)
			{
				// add the budget column if they are not already there and set there correct dispay index
				if (!this.Columns.Contains(budgetCountColumnHeader))
					this.Columns.Insert((int)ColumnId.BUDGET_COUNT, budgetCountColumnHeader);
				budgetCountColumnHeader.DisplayIndex = 2;

				if (!this.Columns.Contains(missingCountColumnHeader))
					this.Columns.Insert((int)ColumnId.MISSING_COUNT, missingCountColumnHeader);
				missingCountColumnHeader.DisplayIndex = 3;

				if (!this.Columns.Contains(usagePercentageColumnHeader))
					this.Columns.Insert((int)ColumnId.PART_USAGE, usagePercentageColumnHeader);
				usagePercentageColumnHeader.DisplayIndex = 4;
			}
			else
			{
				// remove the budget columns
				this.Columns.Remove(budgetCountColumnHeader);
				this.Columns.Remove(missingCountColumnHeader);
				this.Columns.Remove(usagePercentageColumnHeader);
			}
		}

		/// <summary>
		/// this method should be called when a brick is added on the specified layer. This will add the specified brick to the list.
		/// Moreover, if the specified brick is an unnamed group (i.e. a simple group, but not a composed set), it will also add all
		/// the bricks belonging to the group to the list. But this never happen as other code filter the list to remove unnamed group.
		/// On the contrary, if the specified brick is actually a named group and this notifaction is due to an regroup action (specified flag set to true)
		/// then it will remove all the sub brick of this named group to the list, because those brick has been added during the ungroup action.
		/// <param name="layer">The layer on which the brick or group has been added</param>
		/// <param name="brickOrGroup"/>The brick or group that has been added</param>
		/// <param name="isCausedByRegroup">Tell if this add notification was caused by an regroup action</param>
		/// </summary>
		public void addBrickNotification(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isCausedByRegroup)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (!this.Visible)
				return;

			// do nothing if the layer is hidden, unless we should also count hidden bricks
			if (!layer.Visible && !IncludeHiddenLayers)
				return;

			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(layer, true);

			// add the specified brick (will do nothing if the brick is unnamed, such as anonym group, or not listed in library)
			addBrick(brickOrGroup, currentGroupEntry);

			// if the specified brick is group, add also the sub items in the list
			if (brickOrGroup.IsAGroup)
				if (brickOrGroup.PartNumber == string.Empty) // if the group is unnamed, add also all the brick in the group
				{
					// however, this should never happen, as usually when adding/duplicating a group, the unnamed group has been filtered
					// We need to do a recursive call on all the brick, even the unnamed group, because those unnamed group may contains named brick inside
					// but no worries, the addBrick() function will check and avoid adding unnamed group
					foreach (Layer.LayerItem item in (brickOrGroup as Layer.Group).Items)
						this.addBrickNotification(layer, item, false); // allways isCausedByRegroup is false in that case, the regroup is only on the first level of the recursion
				}
				else if (isCausedByRegroup)
				{
					// otherwise if the group is named, stop here, does nothing, unless we are regrouping an old named group,
					// and in that case we need to remove all the sub element of that group
					// because of the false flag, it will not readd anything
					foreach (Layer.LayerItem item in (brickOrGroup as Layer.Group).Items)
						this.removeBrickNotification(layer, item, false); // don't use true otherwise it's an infinite loop
				}

			// if it is currently sorted by quantity, we need to resort.
			// if it is sorted by budget, the addBrick function will anyway call the update budget modification
			if (mLastColumnSortedIndex == 1)
				this.Sort();
		}

		/// <summary>
		/// this method should be called when a brick is deleted on the specified layer. This will remove the specified brick from the list.
		/// Moreover, if the specified brick is an unnamed group (i.e. a simple group, but not a composed set), it will also remove all
		/// the bricks belonging to the group from the list. But this never happen as other code filter the list to remove unnamed group.
		/// On the contrary, if the specified brick is actually a named group and this notifaction is due to an ungroup action (specified flag set to true)
		/// then it will add all the sub brick of this named group to the list, because now each sub element of the specified group
		/// will appeared as single part on the map.
		/// <param name="layer">The layer on which the brick or group has been removed</param>
		/// <param name="brickOrGroup"/>The brick or group that has been removed</param>
		/// <param name="isCausedByUngroup">Tell if this remove notification was caused by an ungroup action</param>
		/// </summary>
		public void removeBrickNotification(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isCausedByUngroup)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (!this.Visible)
				return;

			// do nothing if the layer is hidden, unless we should also count hidden bricks
			if (!layer.Visible && !IncludeHiddenLayers)
				return;

			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(layer, false);
			// if the group entry is not found we can exit
			if (currentGroupEntry == null)
				return;

			// remove the specified brick (will do nothing if the brick is unnamed, such as anonym group, or not listed in library)
			removeBrick(brickOrGroup, currentGroupEntry);

			// if the specified brick is group, remove also the sub items from the list
			if (brickOrGroup.IsAGroup)
				if (brickOrGroup.PartNumber == string.Empty) // if the group is unnamed, remove also all the brick in the group
				{
					// however, this should never happen, as usually when deleting a group, the unnamed group has been filtered
					// We need to do a recursive call on all the brick, even the unnamed group, because those unnamed group may contains named brick inside
					// but no worries, the removeBrick() function will check and avoid removing unnamed group
					foreach (Layer.LayerItem item in (brickOrGroup as Layer.Group).Items)
						this.removeBrickNotification(layer, item, false); // alway isCausedByUngroup is false otherwise it's an infinite loop
				}
				else if (isCausedByUngroup)
				{
					// else if it is a named group and we just ungroup it, then we should add all its named items
					// because of the false flag set in the recursive call, we will only add the named group and not under,
					// and will add hierrachy of unnamed group
					foreach (Layer.LayerItem item in (brickOrGroup as Layer.Group).Items)
						this.addBrickNotification(layer, item, false); // don't use true otherwise it's an infinite loop
				}

			// remove the group from the list view and the mGroupEntryList if it is empty
			if (this.SplitPartPerLayer && (currentGroupEntry.Group.Items.Count == 0))
			{
				this.Groups.Remove(currentGroupEntry.Group);
				mGroupEntryList.Remove(currentGroupEntry);
			}

			// if it is currently sorted by quantity, we need to resort
			// if it is sorted by budget, the removeBrick function will anyway call the update budget modification
			if (mLastColumnSortedIndex == 1)
				this.Sort();
		}

		private void addLayer(LayerBrick brickLayer)
		{
			// skip the invisible layers unless we should include them
			if (!brickLayer.Visible && !IncludeHiddenLayers)
				return;

			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(brickLayer, true);

			// iterate on all the bricks of the list
			foreach (Layer.LayerItem item in brickLayer.LibraryBrickList)
				addBrick(item, currentGroupEntry);
		}

		/// <summary>
		/// this method update the view by adding the specified brick or incrementing its count.
		/// The brick will be ignored if it is not named, as we don't list unnamed brick or group,
		/// so in that case, this method does nothing.
		/// </summary>
		/// <param name="brickOrGroup">the brick to add in the view</param>
		/// <param name="groupEntry">the concerned group in which adding the brick</param>
		private void addBrick(Layer.LayerItem brickOrGroup, GroupEntry groupEntry)
		{
			// early exit if the brick is unnamed or unlisted
			if (brickOrGroup.PartNumber == string.Empty)
				return;

			// get a pointer on the current brick entry list
			Dictionary<string, BrickEntry> brickEntryList = groupEntry.mBrickEntryList;

			// get the part number
			string partNumber = brickOrGroup.PartNumber;

			// try to get an entry in the image dictionary, else add it
			IconEntry iconEntry = null;
			if (!mThumbnailImage.TryGetValue(partNumber, out iconEntry))
			{
				iconEntry = new IconEntry(partNumber, this.SmallImageList);
				mThumbnailImage.Add(partNumber, iconEntry);
			}

			// try to get an entry in the dictionnary for the current brick
			// to get the previous count, then increase the count and store the new value
			BrickEntry brickEntry = null;
			if (!brickEntryList.TryGetValue(partNumber, out brickEntry))
			{
				// create a new entry and add it
				brickEntry = new BrickEntry(partNumber, iconEntry.mImageIndex, this.IncludeHiddenLayers);
				brickEntryList.Add(partNumber, brickEntry);
			}
			// assign the correct group to the item
			brickEntry.Item.Group = groupEntry.Group;

			// add the item in the list view if not already in
			if (brickEntry.IsQuantityNull)
				this.Items.Add(brickEntry.Item);
			// and increment its count
			brickEntry.incrementQuantity();
			// also increment the count for the whole group
			groupEntry.mBrickEntrySumLine.incrementQuantity();
			// update the part usage for all the part that bear the same part number in all the groups
			updateBudgetNotification(partNumber);
		}

		/// <summary>
		/// This method update the view when a layer is removed from the view
		/// </summary>
		private void removeLayer(LayerBrick brickLayer)
		{
			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(brickLayer, false);
			// if the group entry is not found we can exit
			if (currentGroupEntry == null)
				return;

			// iterate on the bricks of the layer to decrease all the part count in the current dictionary
			foreach (Layer.LayerItem item in brickLayer.LibraryBrickList)
				removeBrick(item, currentGroupEntry);

			// remove the group from the list view and the mGroupEntryList
			if (this.SplitPartPerLayer)
			{
				this.Groups.Remove(currentGroupEntry.Group);
				mGroupEntryList.Remove(currentGroupEntry);
			}
		}

		/// <summary>
		/// Update the view by decrementing the brick count or removing it.
		/// This function does nothing if the brick is unnamed.
		/// </summary>
		/// <param name="brickOrGroup">the brick to remove from the view</param>
		/// <param name="brickEntryList">the concerned group from which removing the brick</param>
		private void removeBrick(Layer.LayerItem brickOrGroup, GroupEntry groupEntry)
		{
			// early exit if the brick is unnamed or unlisted
			if (brickOrGroup.PartNumber == string.Empty)
				return;

			BrickEntry brickEntry = null;
			if (groupEntry.mBrickEntryList.TryGetValue(brickOrGroup.PartNumber, out brickEntry))
			{
				// decrement the count for the brick and the whole group
				brickEntry.decrementQuantity();
				groupEntry.mBrickEntrySumLine.decrementQuantity();
				// check if the brick becomes null
				if (brickEntry.IsQuantityNull)
				{
					this.Items.Remove(brickEntry.Item);
				}
				// update the part usage for all the part that bear the same part number in all the groups
				updateBudgetNotification(brickOrGroup.PartNumber);
			}
		}

		/// <summary>
		/// rebuild the full list from scratch
		/// </summary>
		public void rebuildList()
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (!this.Visible)
				return;

			// and rebuild the list
			rebuildListInternal();
		}

		/// <summary>
		/// rebuild the full list from scratch
		/// </summary>
		private void rebuildListInternal()
		{ 
			// clear everyting that we will rebuild
			mThumbnailImage.Clear();
			mGroupEntryList.Clear();
			this.Groups.Clear();
			this.Items.Clear();
			this.SmallImageList.Images.Clear();

			// create a default group if we don't use the layers
			if (!this.SplitPartPerLayer)
			{
				GroupEntry groupEntry = new GroupEntry(null, this);
				mGroupEntryList.Add(groupEntry);
			}

			// iterate on all the brick of all the brick layers,
			// create on group per layer if needed, and create one entry
			// in the dictionnary
			foreach (Layer layer in Map.Instance.LayerList)
			{
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					addLayer(brickLayer);
			}
		}

		#endregion
		#region export list

		public void export(string filename)
		{
			// if the part list is not visible, we need to rebuild it before exporting
			// (otherwise, if it is visible, the list is already up to date)
			if (!this.Visible)
				rebuildListInternal();

			// compute an array to store the order of the columns
			int[] columnOrder = new int[this.Columns.Count];
			int columnIndex = 0;
			foreach (ColumnHeader columnHeader in this.Columns)
			{
				columnOrder[columnHeader.DisplayIndex] = columnIndex;
				columnIndex++;
			}
			// call the correct exporter
			if (filename.ToLower().EndsWith(".txt"))
				exportListInTxt(filename, columnOrder);
			else if (filename.ToLower().EndsWith(".csv"))
				exportListInCSV(filename, columnOrder);
			else
				exportListInHtml(filename, columnOrder);
		}

		#region export in text
		private void exportColumnHeaderInTxt(StreamWriter writer, int[] columnOrder, int[] maxLength)
		{
			writer.Write("| ");
			for (int i = 0; i < maxLength.Length; ++i)
			{
				// prepare the text to write with padding
				string text = this.Columns[columnOrder[i]].Text;
				// remove the column sorter char
				if (columnOrder[i] == this.mLastColumnSortedIndex)
					text = text.Substring(2);
				// compute the padding
				int padding = (maxLength[i] - text.Length) + 1;
				for (int j = 0; j < padding; ++j)
					text += " ";
				// write the text and a pipe
				if (i == (maxLength.Length - 1))
					writer.Write(text + "|\n");
				else
					writer.Write(text + "| ");
			}
		}
		private void exportOneItemInTxt(StreamWriter writer, int[] columnOrder, int[] maxLength, ListViewItem item)
		{
			writer.Write("| ");
			for (int i = 0; i < maxLength.Length; ++i)
			{
				// prepare the text to write with padding
				string text = item.SubItems[columnOrder[i]].Text;
				int padding = (maxLength[i] - text.Length) + 1;
				for (int j = 0; j < padding; ++j)
					text += " ";
				// write the text and a pipe
				if (i == (maxLength.Length - 1))
					writer.Write(text + "|\n");
				else
					writer.Write(text + "| ");
			}
		}

		private void exportItemsInTxt(StreamWriter writer, int[] columnOrder, int[] maxLength, ListView.ListViewItemCollection itemList, string headerLine)
		{
			ListViewItem sumLineItem = null;

			foreach (ListViewItem item in itemList)
			{
				// skip the sum line in order to add it at the end only
				if (item.Tag != null)
				{
					sumLineItem = item;
					continue;
				}
				// export the item
				exportOneItemInTxt(writer, columnOrder, maxLength, item);
			}

			// finally export the sum line if not null
			if (sumLineItem != null)
			{
				writer.WriteLine(headerLine);
				exportOneItemInTxt(writer, columnOrder, maxLength, sumLineItem);
			}
		}

		private void exportListInTxt(string fileName, int[] columnOrder)
		{
			//compute the max lenght of texts of each column and the column header
			int[] maxLength = new int[this.Columns.Count];
			for (int i = 0; i < maxLength.Length; ++i)
				maxLength[i] = this.Columns[i].Text.Length;
			foreach (ListViewItem item in this.Items)
			{
				for (int i = 0; i < maxLength.Length; ++i)
				{
					int currentTextLength = item.SubItems[columnOrder[i]].Text.Length;
					if (currentTextLength > maxLength[i])
						maxLength[i] = currentTextLength;
				}
			}

			//compute the header line
			int headerLineLength = 0;
			for (int i = 0; i < maxLength.Length; ++i)
				headerLineLength += maxLength[i] + 3;
			headerLineLength--;
			string headerLine = "+";
			for (int i = 0; i < headerLineLength; ++i)
				headerLine += "-";
			headerLine += "+";

			try
			{
				// open a stream
				StreamWriter writer = new StreamWriter(fileName);

				// get the title
				string title = Properties.Resources.TextExportPartListTitle.Replace("&", Map.Instance.MapFileName);
				string frameLine = "+=";
				for (int i = 0; i < title.Length; ++i)
					frameLine += "=";
				frameLine += "=+";
				// header
				writer.WriteLine("                    {0}", frameLine);
				writer.WriteLine("                    | {0} |", title);
				writer.WriteLine("                    {0}", frameLine);
				writer.WriteLine();
				writer.WriteLine("{0} {1}", MainForm.Instance.LabelAuthorLocalized, Map.Instance.Author);
				writer.WriteLine("{0} {1}", MainForm.Instance.LabelLUGLocalized, Map.Instance.LUG);
				writer.WriteLine("{0} {1}", MainForm.Instance.LabelEventLocalized, Map.Instance.Event);
				writer.WriteLine("{0} {1}", MainForm.Instance.LabelDateLocalized, Map.Instance.Date.ToLongDateString());
				writer.WriteLine("{0}\n{1}", MainForm.Instance.LabelCommentLocalized, Map.Instance.Comment);
				writer.WriteLine();
				writer.WriteLine();
				// the parts
				if (this.SplitPartPerLayer)
				{
					foreach (ListViewGroup group in this.Groups)
					{
						writer.WriteLine("| " + group.Header);
						writer.WriteLine(headerLine);
						exportColumnHeaderInTxt(writer, columnOrder, maxLength);
						writer.WriteLine(headerLine);
						exportItemsInTxt(writer, columnOrder, maxLength, group.Items, headerLine);
						writer.WriteLine(headerLine);
						writer.WriteLine();
					}
				}
				else
				{
					writer.WriteLine(headerLine);
					exportColumnHeaderInTxt(writer, columnOrder, maxLength);
					writer.WriteLine(headerLine);
					exportItemsInTxt(writer, columnOrder, maxLength, this.Items, headerLine);
					writer.WriteLine(headerLine);
				}

				// close the stream
				writer.Close();
			}
			catch
			{
			}
		}
		#endregion

		#region export in CSV
		private void exportColumnHeaderInCSV(StreamWriter writer, int[] columnOrder)
		{
			// construct the line with all the info
			string line = string.Empty;
			for (int i = 0; i < columnOrder.Length; ++i)
			{
				string text = this.Columns[columnOrder[i]].Text;
				// remove the column sorter char
				if (columnOrder[i] == this.mLastColumnSortedIndex)
					text = text.Substring(2);
				// add the text
				line += text;
				// add the comma except for the last item
				if (i < columnOrder.Length - 1)
					line += ",";
			}
			// write the line
			writer.WriteLine(line);
		}

		private void exportOneItemInCSV(StreamWriter writer, int[] columnOrder, ListViewItem item)
		{
			// construct the line with all the info
			string line = string.Empty;
			for (int i = 0; i < columnOrder.Length; ++i)
			{
				// get the text
				string text = item.SubItems[columnOrder[i]].Text;
				// remove the comma from the description (in case some part description has a comma
				if (columnOrder[i] == (int)ColumnId.DESCRIPTION)
					text = text.Replace(",", " ");
				else if ((columnOrder[i] == (int)ColumnId.PART_USAGE) && !text.Equals(Properties.Resources.TextUnbudgeted) && !text.Equals(Properties.Resources.TextNA))
					// for the part usage, remove the progress bar, as in CSV export user is more interested by the value
					// the first 10 characters is the progress bar, then a space character, then the number and the % char at the end
					text = text.Substring(11, text.Length - 12);
				// add the text to the line
				line += text;
				// add the comma except for the last item
				if (i < columnOrder.Length - 1)
					line += ",";
			}
			// write the line
			writer.WriteLine(line);
		}

		private void exportItemsInCSV(StreamWriter writer, int[] columnOrder, ListView.ListViewItemCollection itemList)
		{
			ListViewItem sumLineItem = null;

			foreach (ListViewItem item in itemList)
			{
				// skip the sum line in order to add it at the end only
				if (item.Tag != null)
				{
					sumLineItem = item;
					continue;
				}
				// export the current item
				exportOneItemInCSV(writer, columnOrder, item);
			}

			// finally export the sum line if not null
			if (sumLineItem != null)
				exportOneItemInCSV(writer, columnOrder, sumLineItem);
		}

		private void exportListInCSV(string fileName, int[] columnOrder)
		{
			try
			{
				// open a stream
				StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);

				// the parts
				if (this.SplitPartPerLayer)
				{
					foreach (ListViewGroup group in this.Groups)
					{
						// write the name of the layer
						writer.WriteLine(group.Header);
						// write the column header in front of each group
						exportColumnHeaderInCSV(writer, columnOrder);
						exportItemsInCSV(writer, columnOrder, group.Items);
						// write an empty line
						writer.WriteLine();
					}
				}
				else
				{
					// write the column header
					exportColumnHeaderInCSV(writer, columnOrder);
					// and the item list
					exportItemsInCSV(writer, columnOrder, this.Items);
				}

				// close the stream
				writer.Close();
			}
			catch
			{
			}
		}
		#endregion

		#region export in HTML
		private void exportColumnHeaderInHtml(StreamWriter writer, int[] columnOrder)
		{
			writer.WriteLine("<tr class=\"header\">");
			for (int i = 0; i < columnOrder.Length; ++i)
			{
				string styleClass = string.Empty;
				// get the column header text
				string text = this.Columns[columnOrder[i]].Text;
				// remove the column sorter char
				if (columnOrder[i] == this.mLastColumnSortedIndex)
					text = text.Substring(2);
				// write the header
				switch (columnOrder[i])
				{
					case (int)ColumnId.PART_ID:
						styleClass = "partIdHeader";						
						break;
					case (int)ColumnId.PART_COUNT:
						styleClass = "partCountHeader";
						break;
					case (int)ColumnId.BUDGET_COUNT:
						styleClass = "budgetHeader";
						break;
					case (int)ColumnId.MISSING_COUNT:
						styleClass = "missingHeader";
						break;
					case (int)ColumnId.PART_USAGE:
						styleClass = "partUsageHeader";
						break;
					case (int)ColumnId.COLOR:
						styleClass = "colorHeader";
						break;
					case (int)ColumnId.DESCRIPTION:
						styleClass = "descriptionHeader";
						break;
				}
				// write the text with its style class
				writer.WriteLine("\t<td class=\"{0}\">{1}</td>", styleClass, text);
			}
			writer.WriteLine("</tr>");
		}

		private void exportOneItemInHtml(StreamWriter writer, int[] columnOrder, ListViewItem item, bool isTotalLine)
		{
			if (isTotalLine)
				writer.WriteLine("<tr class=\"total\">");
			else
				writer.WriteLine("<tr>");

			for (int i = 0; i < columnOrder.Length; ++i)
			{
				string styleClass = string.Empty;
				string text = item.SubItems[columnOrder[i]].Text;
				switch (columnOrder[i])
				{
					case (int)ColumnId.PART_ID:
						//special case for the part column, we may also add the picture according to the specified flag
						if (!isTotalLine)
						{
							// check if we have an imageURL or if we need to construct the default image path
							string colorNum = item.SubItems[(int)ColumnId.COLOR].Tag as string;
							string partNumber = text;
							if (colorNum != string.Empty)
								partNumber += "." + colorNum;
							string imageURL = BrickLibrary.Instance.getImageURL(partNumber);
							if (imageURL == null)
								imageURL = colorNum + "/" + text.ToLower() + ".png";
							// construct the text for the IMG tag
							text = "<img width=\"100%\" src=\"" + imageURL + "\"><br/>" + text;
						}
						// the class
						styleClass = "partId";
						break;
					case (int)ColumnId.PART_COUNT:
						styleClass = "partCount";
						break;
					case (int)ColumnId.BUDGET_COUNT:
						styleClass = "budget";
						break;
					case (int)ColumnId.MISSING_COUNT:
						styleClass = "missing";
						break;
					case (int)ColumnId.PART_USAGE:
						styleClass = "partUsage";
						break;
					case (int)ColumnId.COLOR:
						styleClass = "color";
						break;
					case (int)ColumnId.DESCRIPTION:
						styleClass = "description";
						break;
				}
				// write the text with its style class
				writer.WriteLine("\t<td class=\"{0}\">{1}</td>", styleClass, text);
			}
			writer.WriteLine("</tr>");
		}

		private void exportItemsInHtml(StreamWriter writer, int[] columnOrder, ListView.ListViewItemCollection itemList)
		{
			ListViewItem sumLineItem = null;

			foreach (ListViewItem item in itemList)
			{
				// skip the sum line in order to add it at the end only
				if (item.Tag != null)
				{
					sumLineItem = item;
					continue;
				}
				// export the item
				exportOneItemInHtml(writer, columnOrder, item, false);
			}

			// finally export the sum line if not null
			if (sumLineItem != null)
				exportOneItemInHtml(writer, columnOrder, sumLineItem, true);
		}

		private void writeHtmlStyles(StreamWriter writer)
		{
			writer.WriteLine("\t<style type=\"text/css\" >");
			writer.WriteLine("\th2.title { text-align: center; font-weight: bold; font-variant: small-caps; margin: 2.5%; background-color: #bdffc0; border: 2px solid black; padding: 10px;}");
			writer.WriteLine("\ttd.info { text-align: right; vertical-align: top; font-weight: bold; }");
			writer.WriteLine("\ttr.groupName { background-color: #90d7ff; font-weight: bold; }");
			writer.WriteLine("\ttr.header { background-color: #bfe8ff; font-style: italic; }");
			writer.WriteLine("\ttd.partIdHeader { text-align: center; width: 20% }");
			writer.WriteLine("\ttd.partCountHeader { text-align: center; width: 5% }");
			writer.WriteLine("\ttd.budgetHeader { text-align: center; width: 5% }");
			writer.WriteLine("\ttd.missingHeader { text-align: center; width: 5% }");
			writer.WriteLine("\ttd.partUsageHeader { text-align: center; width: 10% }");
			writer.WriteLine("\ttd.colorHeader { text-align: center; width: 10% }");
			writer.WriteLine("\ttd.descriptionHeader { text-align: center; width: 46% }");
			writer.WriteLine("\ttd.partId { text-align: center; }");
			writer.WriteLine("\ttd.partCount { text-align: center; }");
			writer.WriteLine("\ttd.budget { text-align: center; }");
			writer.WriteLine("\ttd.missing { text-align: center; }");
			writer.WriteLine("\ttd.partUsage { }");
			writer.WriteLine("\ttd.color { text-align: center; }");
			writer.WriteLine("\ttd.description { }");
			writer.WriteLine("\ttr.total { background-color: #feffea; }");
			writer.WriteLine("\t</style>");
		}

		private void exportListInHtml(string fileName, int[] columnOrder)
		{
			try
			{
				// open a stream
				StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);

				string title = Properties.Resources.TextExportPartListTitle.Replace("&", Map.Instance.MapFileName);
				// header
				writer.WriteLine("<html>\n<head>\n\t<title>{0}</title>", title);
				writer.WriteLine("\t<base href=\"http://media.peeron.com/ldraw/images/\">");
				writeHtmlStyles(writer);
				writer.WriteLine("</head>\n<body>");
				writer.WriteLine("<h2 class=\"title\">{0}</h2>", title);
				writer.WriteLine("<table border=\"0\" style=\"margin-left: 3%\">");
				writer.WriteLine("\t<tr><td class=\"info\">{0}</td><td>{1}</td></tr>", MainForm.Instance.LabelAuthorLocalized, Map.Instance.Author);
				writer.WriteLine("\t<tr><td class=\"info\">{0}</td><td>{1}</td></tr>", MainForm.Instance.LabelLUGLocalized, Map.Instance.LUG);
				writer.WriteLine("\t<tr><td class=\"info\">{0}</td><td>{1}</td></tr>", MainForm.Instance.LabelEventLocalized, Map.Instance.Event);
				writer.WriteLine("\t<tr><td class=\"info\">{0}</td><td>{1}</td></tr>", MainForm.Instance.LabelDateLocalized, Map.Instance.Date.ToLongDateString());
				writer.WriteLine("\t<tr><td class=\"info\">{0}</td><td>{1}</td></tr>", MainForm.Instance.LabelCommentLocalized, Map.Instance.Comment.Replace(Environment.NewLine, "<br/>"));
				writer.WriteLine("</table>\n<br/>\n<br/>\n");
				// the parts
				if (this.SplitPartPerLayer)
				{
					foreach (ListViewGroup group in this.Groups)
					{
						writer.WriteLine("<table border=\"1px\" width=\"95%\" cellpadding=\"10\" style=\"margin: auto\">");
						writer.WriteLine("<tr class=\"groupName\"><td colspan=\"{0}\"><b>{1}</b></td></tr>", columnOrder.Length, group.Header);
						exportColumnHeaderInHtml(writer, columnOrder);
						exportItemsInHtml(writer, columnOrder, group.Items);
						writer.WriteLine("</table>");
						writer.WriteLine("<br/>");
					}
				}
				else
				{
					writer.WriteLine("<table border=\"1\" width=\"95%\" cellpadding=\"10\" style=\"margin: auto\">");
					exportColumnHeaderInHtml(writer, columnOrder);
					exportItemsInHtml(writer, columnOrder, this.Items);
					writer.WriteLine("</table>");
				}
				writer.WriteLine("</body>\n</html>");

				// close the stream
				writer.Close();
			}
			catch
			{
			}
		}
		#endregion

		#endregion
		#region list view events
		protected override void OnVisibleChanged(EventArgs e)
		{
			// rebuild the list if the form becomes visible
			if (this.Visible)
				rebuildList();
		}

		protected override void OnColumnClick(ColumnClickEventArgs e)
		{
			// start of the update of the control
			this.BeginUpdate();

			// change the sort order if we click again on the same column
			// but if we change the column, don't change the sort order
			if (mLastColumnSortedIndex == e.Column)
			{
				if (this.Sorting == SortOrder.Ascending)
					this.Sorting = SortOrder.Descending;
				else
					this.Sorting = SortOrder.Ascending;
			}
			else
			{
				// We change the twice the listViewShortcutKeys.Sorting value
				// and reset it with the same value to FORCE the Sort to be done,
				// once if the listViewShortcutKeys.Sorting didn't changed the Sort
				// method does nothing.
				SortOrder oldOrder = this.Sorting;
				this.Sorting = SortOrder.None;
				this.Sorting = oldOrder;
			}

			// remove the previous sorting icon, and add the icon to the new column
			setSortIcon(e.Column);

			// create a new comparer with the right column then call the sort method
			this.ListViewItemSorter = new MyListViewItemComparer(e.Column, this.Sorting);
			this.Sort();

			// end of the update of the control
			this.EndUpdate();
		}

		private void setSortIcon(int columnIndex)
		{
			// remove the order sign on the previous sorted column
			if (mLastColumnSortedIndex != -1)
			{
				string header = this.Columns[mLastColumnSortedIndex].Text;
				this.Columns[mLastColumnSortedIndex].Text = header.Substring(2);
			}

			// save the new current column index
			mLastColumnSortedIndex = columnIndex;

			// add a descending or ascending sign to the header of the column
			if (this.Sorting == SortOrder.Ascending)
				this.Columns[columnIndex].Text = char.ConvertFromUtf32(0x25B2) + " " + this.Columns[columnIndex].Text;
			else
				this.Columns[columnIndex].Text = char.ConvertFromUtf32(0x25BC) + " " + this.Columns[columnIndex].Text;
		}
		#endregion
	}
}