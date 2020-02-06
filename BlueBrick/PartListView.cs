using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;
using System.Drawing;
using System.Runtime.InteropServices;

namespace BlueBrick
{
	/// <summary>
	/// This class implement a specialized ListView for the part Library. It adds filtering methods
	/// to only display parts of the list view items.
	/// </summary>
	class PartListView : ListView
	{
		/// <summary>
		/// This class implement a IComparer to sort all the parts in the part library, based on the Name property
		/// of the ListViewItem. We don't use the ListView.Sorting property because this default sorter sort on the Text property
		/// and the Text property is displayed as a tooltips even when the ListView.ShowToolTips is false.
		/// </summary>
		private class ListViewItemComparerBasedOnName : System.Collections.IComparer
		{
			public int Compare(Object x, Object y)
			{
				return string.Compare((x as ListViewItem).Name, (y as ListViewItem).Name);
			}
		}

		// the comparer to sort the list view item, base on their name property
		private static ListViewItemComparerBasedOnName sListViewItemComparer = new ListViewItemComparerBasedOnName();

		// create a list of list view item containing all the visible not filtered and bugeted item, i.e. the currently visible item in this list view for optim reason (to use the AddRange, and not adding items one by one)
		private List<ListViewItem> mVisibleItems = new List<ListViewItem>();
		// create a temporary list view to temporary store the items that have been filtered in this list view
		private List<ListViewItem> mFilteredItems = new List<ListViewItem>();
		// create another temporary list view to temporary store the items that are not budgeted in this listview
		private List<ListViewItem> mNotBudgetedItems = new List<ListViewItem>();
		
		// the filter sentence
		private string mFilterSentence = string.Empty;
		private bool mIsFilterSentenceSynchroWithCurrentFiltering = false;

        // for label edition
		private bool mIsEditingBudget = false;
		private ListViewItem mItemForLabelEdit = null;
		private ListViewItem mItemHitOnMouseDownForAdding = null;

		#region get/set
		/// <summary>
		/// Tell if this list view is filtered, no matter if it was a local filter or a global filter.
		/// </summary>
		public bool IsFiltered
		{
			get
			{
				// We need to check the filter sentence and not the count of the filtered item list,
				// because it may happen that the filter sentence actually filter no item
				// (i.e. all the items match the filter sentence), so the filtered list may be empty even
				// if a filter sentence is entered by the user. But we need to check both the global and
				// and local filter sentence to get an accurate answer.
				if (Properties.Settings.Default.UIFilterAllLibraryTab)
					return (Properties.Settings.Default.UIFilterAllSentence != string.Empty);
				else
					return (mFilterSentence != string.Empty);
			}
		}

		public string FilterSentence
		{
			get { return mFilterSentence; }
		}

		public bool IsEditingBudget
		{
			get { return mIsEditingBudget; }
		}
		#endregion

		#region constructor
		// import this method for the optimization in the constructor
		[System.Runtime.InteropServices.DllImport("user32")]
		private static extern bool SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

		public PartListView()
		{
            InitializeComponent();
			// set the property of the list view
			this.ShowItemToolTips = Properties.Settings.Default.PartLibDisplayBubbleInfo;
			this.ListViewItemSorter = sListViewItemComparer; // we want to sort the items based on their Name (which contains a sorting key)
			updateViewStyle(false); // set the view style depending if budget need to be visible or not
			updateBackgroundColor();
			// remove the transparent background color for the text (which is a performance issue on dot net)
			uint LVM_SETTEXTBKCOLOR = 0x1026;
			SendMessage(this.Handle, LVM_SETTEXTBKCOLOR, IntPtr.Zero, unchecked((IntPtr)(int)0xFFFFFF));
		}

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // PartListView
            // 
            this.Alignment = System.Windows.Forms.ListViewAlignment.SnapToGrid;
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelEdit = true;
			this.LabelWrap = false;
            this.MultiSelect = false;
			this.LabelWrap = true; // we need label wrap for the "\n" to work, between the part name and the budget display
			this.DoubleBuffered = true; // the double buffered also prevent a crash bug in Mono, otherwise it tries to paint while editing the list by a filtering
			this.BeforeLabelEdit += new LabelEditEventHandler(this.PartListView_BeforeLabelEdit);
            this.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.PartListView_AfterLabelEdit);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PartListView_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PartListView_MouseUp);
			this.MouseLeave += new EventHandler(this.PartListView_MouseLeave);
			this.ResumeLayout(false);
        }

		/// <summary>
		/// Reset the this.Items list with all the listViewItems present in the mVisibleItems list
		/// </summary>
		public void resetThisListViewWithVisibleList()
		{
			this.BeginUpdate();
			// reset the list of item of this list view, with an AddRange for optim reason
			this.Items.Clear();
			try
			{
				// for a strange reason, on mono, this method throw an exception for all the
				// item added after the 16th one added (but the item is still added).
				// Probably a bug from Mono.
				this.Items.AddRange(mVisibleItems.ToArray());
			}
			catch
			{
				// so ignore this exception for mono, otherwise it is displayed in the error message box
			}
			// On Mono the EndUpate is called, but on Dot Net the begin and End update are counted
			this.EndUpdate();
		}

		/// <summary>
		/// Move all the items of the specified list into the mVisibleList of this class.
		/// The specified list will be emptied after the call of this function
		/// </summary>
		/// <param name="itemsToMove">The item to add to the list</param>
		private void moveItemsToVisibleList(List<ListViewItem> itemsToMove)
		{
			mVisibleItems.AddRange(itemsToMove);
			itemsToMove.Clear();
		}

		/// <summary>
		/// This method will add the specified item to the list view, either in the visible item list
		/// or in the non budgeted item list if the setting to hide non budgeted item is set.
		/// </summary>
		/// <param name="itemToAdd">the new item to add to the list view</param>
		public void addNewItem(ListViewItem itemToAdd)
		{
			if (Budget.Budget.Instance.ShouldShowOnlyBudgetedParts && !Budget.Budget.Instance.IsBudgeted(itemToAdd.Tag as string))
				mNotBudgetedItems.Add(itemToAdd);
			else
				mVisibleItems.Add(itemToAdd);
		}

		/// <summary>
		/// Remove from internal lists the item which has the specified matching tag
		/// </summary>
		/// <param name="">the tag of the item that you want to remove</param>
		public void removeItemByTag(string tag)
		{
			// create a list with the 3 internal lists
			List<List<ListViewItem>> allLists = new List<List<ListViewItem>>() { mVisibleItems, mFilteredItems, mNotBudgetedItems };

			// iterate on all the item of all the lists
			foreach (List<ListViewItem> list in allLists)
				foreach (ListViewItem item in list)
					if (item.Tag.Equals(tag))
					{
						// remove the item from the list
						int removedImageIndex = item.ImageIndex;
						list.Remove(item);
						// then iterate again on all the item of all list to shift all the image index that are after the item removed of -1
						foreach (List<ListViewItem> listToShift in allLists)
							foreach (ListViewItem itemToShift in listToShift)
							if (itemToShift.ImageIndex > removedImageIndex)
								itemToShift.ImageIndex = itemToShift.ImageIndex - 1;
						// stop the list search since we found the item to remove
						return;
					}
		}
		#endregion

		#region util function
		/// <summary>
		///  This function make the first items found that match the specified tag, visible on screen by scrolling the list view.
		/// </summary>
		/// <param name="tagToShow">The tag of the item that you want to show</param>
		public void ensureVisibleByTag(string tagToShow)
		{
			// find the first item that match the tag and make it visible
			foreach (ListViewItem item in this.Items)
				if (item.Tag.Equals(tagToShow))
				{
					this.EnsureVisible(item.Index);
					break;
				}
		}

		/// <summary>
		/// Call this funtion when you want to change the format of the tooltip texts
		/// </summary>
		public void updateToolTipTextOfAllItems()
		{
			bool displayPartId = Properties.Settings.Default.PartLibBubbleInfoPartID;
			bool displayColor = Properties.Settings.Default.PartLibBubbleInfoPartColor;
			bool displayDescription = Properties.Settings.Default.PartLibBubbleInfoPartDescription;

			// update the items of the three lists
			foreach (ListViewItem item in mVisibleItems)
				item.ToolTipText = BrickLibrary.Instance.getFormatedBrickInfo(item.Tag as string, displayPartId, displayColor, displayDescription);
			foreach (ListViewItem item in mFilteredItems)
				item.ToolTipText = BrickLibrary.Instance.getFormatedBrickInfo(item.Tag as string, displayPartId, displayColor, displayDescription);
			foreach (ListViewItem item in mNotBudgetedItems)
				item.ToolTipText = BrickLibrary.Instance.getFormatedBrickInfo(item.Tag as string, displayPartId, displayColor, displayDescription);
		}

		/// <summary>
		/// Search in all internal list and return the first ListViewItem whose tag is matching the specified part id
		/// </summary>
		/// <param name="partId">The tag you are looking for</param>
		/// <returns>The list view item that match the specified tag, or null if no matching item was found</returns>
		private ListViewItem getListViewItemFromTag(string partId)
		{
			// iterate on the 3 lists
			// the item list
			foreach (ListViewItem item in mVisibleItems)
				if (partId.Equals(item.Tag as string))
					return item;
			// the filtered items
			foreach (ListViewItem item in mFilteredItems)
				if (partId.Equals(item.Tag as string))
					return item;
			// the non budgeted items
			foreach (ListViewItem item in mNotBudgetedItems)
				if (partId.Equals(item.Tag as string))
					return item;
			// no matching item found.
			return null;
		}
		#endregion

		#region filtering
		/// <summary>
		/// Filter the current tab with the specified string. The string should be a list of keywords that can be
		/// prefixed by a '-' to exclude, a '+' to include or a '#' to filter on part id. Each keyword should be separated
		/// by blank character such as space or tab.
		/// </summary>
		/// <param name="filterSentence">several keywords separated by blank characters with or without prefixes</param>
		/// <param name="saveFilterSentence">if true the specified filter sentence is saved</param>
		public void filter(string filterSentence, bool saveFilterSentence)
		{
			// if we are asked to filter with the same sentence, just exit (nothing more to do, it's already correct)
			if (mIsFilterSentenceSynchroWithCurrentFiltering && mFilterSentence.Equals(filterSentence))
				return;

			// save the sentence if needed
			mIsFilterSentenceSynchroWithCurrentFiltering = saveFilterSentence;
			if (saveFilterSentence)
				mFilterSentence = filterSentence;

			// split the searching filter in token
			// first we split the sentence by sub-sentence inside double quote for example: a "b c" d
			// will be split in 3: { 'a', 'b c', 'd' }
			// it's important to keep the empty entries in the split list because we want to catch cases like
			// the first character of the sentence is a double quote, or no space between two sub sentence:
			// "a b" c cc "d e""f g" => { '', 'a b', 'c cc', 'd e', '', 'f g' }
			//                            0     1       2      3    4     5
			// then we will split again even index, but not odd index, so split 0, 2, 4 ; keep 1, 3, 5
			char[] doubleQuoteSeparator = { '"' };
			string[] subSentenceList = filterSentence.ToLower().Split(doubleQuoteSeparator, StringSplitOptions.None);
			// now re-split only the even index with the empty char
			char[] separatorList = { ' ', '\t' };
			List<string> tokenList = new List<string>();
			for (int i = 0; i < subSentenceList.Length; ++i)
			{
				string subSentence = subSentenceList[i];
				bool isEvenIndex = ((i % 2) == 0);
				// special case for a special character (-+#) in front of a double quoted sentence
				// if we can find a special character at the end of the current sub sentence, we need to transfert it
				// at the begining of the next subsentence, but only for even index
				if (isEvenIndex && subSentence.Length > 0)
				{
					char lastChar = subSentence[subSentence.Length - 1];
					if ("-+#".LastIndexOf(lastChar) >= 0)
					{
						// remove the last char
						subSentence = subSentence.Remove(subSentence.Length - 1);
						// and add it to the next subsence if any
						if (i + 1 < subSentenceList.Length)
							subSentenceList[i + 1] = lastChar.ToString() + subSentenceList[i + 1];
					}
				}
				// now if the current sub sentence is not empty
				if (subSentence != string.Empty)
				{
					// split even index and directly add odd index
					if (isEvenIndex)
						tokenList.AddRange(subSentence.Split(separatorList, StringSplitOptions.RemoveEmptyEntries));
					else
						tokenList.Add(subSentence);
				}
			}
			// now create 3 lists for including and exclusion
			List<string> includeIdFilter = new List<string>();
			List<string> includeFilter = new List<string>();
			List<string> excludeFilter = new List<string>();
			// recreate two lists for include/exclude
			foreach (string token in tokenList)
				if (token[0] == '-')
				{
					if (token.Length > 1)
						excludeFilter.Add(token.Substring(1));
				}
				else if (token[0] == '#')
				{
					if (token.Length > 1)
						includeIdFilter.Add(token.Substring(1).ToUpper());
				}
				else if (token[0] == '+')
				{
					if (token.Length > 1)
						includeFilter.Add(token.Substring(1));
				}
				else
					includeFilter.Add(token);
			// call the function to filter the list
			filterDisplayedParts(includeIdFilter, includeFilter, excludeFilter);
		}

		/// <summary>
		/// Call the filter function with the currently saved filter sentence or with the global filter sentence
		/// depending on what is currently selected
		/// </summary>
		public void refilter()
		{
			// check with what we need to refilter
			if (Properties.Settings.Default.UIFilterAllLibraryTab)
				filter(Properties.Settings.Default.UIFilterAllSentence, false);
			else
				filter(mFilterSentence, false);
		}

        /// <summary>
		/// Call the filter function with the currently saved filter sentence or with the global filter sentence
		/// depending on what is currently selected.
        /// <param name="forceEvenIfNotNeeded">if true the filtering will be performed, even if it is not needed</param>
        /// </summary>
        public void refilter(bool forceEvenIfNotNeeded)
        {
			// to force the refiltering, we need to change the synch flag to false
            if (forceEvenIfNotNeeded)
                mIsFilterSentenceSynchroWithCurrentFiltering = false;
			// if we perform a global filtering do not save the gloabl filter sentence in the local one
			if (Properties.Settings.Default.UIFilterAllLibraryTab)
				filter(Properties.Settings.Default.UIFilterAllSentence, false);
			else
				// use true as param to save the filter in order to resynch the flag
				filter(mFilterSentence, true);
        }

		/// <summary>
		/// Modify the current list of parts listed in the current tab to keep only (or exclude) the part whose ID,
		/// color or description match any of the keywords given in parameter. The keywords must be provided
		/// in lower case to make a case insensitive search.
		/// </summary>
		/// <param name="includeIdFilter">All the parts whose ID only contains any of this filter will be displayed</param>
		/// <param name="includeFilter">All the parts whose ID, color or description contains any of this filter will be displayed</param>
		/// <param name="excludeFilter">All the parts whose ID, color or description contains any of this filter will be hidden</param>
		private void filterDisplayedParts(List<string> includeIdFilter, List<string> includeFilter, List<string> excludeFilter)
		{
			// stop the redraw
			this.SuspendLayout();
			this.BeginUpdate();

			//put back all the previous filtered item in the list
			moveItemsToVisibleList(mFilteredItems);

			// clear the background color with the default one
			updateBackgroundColor();

			// and now filter the current this list view if any
			if ((includeIdFilter.Count != 0) || (includeFilter.Count != 0) || (excludeFilter.Count != 0))
			{
				try
				{
					this.BackColor = Properties.Settings.Default.PartLibFilteredBackColor;
					// do not use a foreach here because Mono doesn't support to remove items while iterating on the list
					// so use an index instead and decrease the index when we remove the item
					for (int i = 0; i < mVisibleItems.Count; ++i)
					{
						ListViewItem item = mVisibleItems[i];
						string itemId = item.Tag as string;
						string brickInfo = BrickLibrary.Instance.getFormatedBrickInfo(itemId, true, true, true).ToLower();
						// a flag to stop search if it is already removed
						bool continueSearch = true;
						// iterate on the 3 filter lists
						// first filter on the id only
						foreach (string filter in includeIdFilter)
							if (!itemId.Contains(filter))
							{
								mVisibleItems.RemoveAt(i);
								i--;
								mFilteredItems.Add(item);
								continueSearch = false;
								break;
							}
						// then the include filter on everything
						if (continueSearch)
							foreach (string filter in includeFilter)
								if (!brickInfo.Contains(filter))
								{
									mVisibleItems.RemoveAt(i);
									i--;
									mFilteredItems.Add(item);
									continueSearch = false;
									break;
								}
						// then the exclude filter on everything
						if (continueSearch)
							foreach (string filter in excludeFilter)
								if (brickInfo.Contains(filter))
								{
									mVisibleItems.RemoveAt(i);
									i--;
									mFilteredItems.Add(item);
									continueSearch = false;
									break;
								}
					}
				}
				catch
				{
				}
			}

			// after having updated the visible list, copy it to the actual list of items of this listView
			resetThisListViewWithVisibleList();

			// On Mono, the AddRange call the EndUpdate, so we put back the begin update, because we want to continue to update
			// On Dot Net, the BeginUpdate are counted, but not on Mono, so we will have to put another EndUpdate
			this.BeginUpdate();

			// resort the list view
			this.Sort();

			// resume the redraw
			this.EndUpdate();
			// call it a second time because on Dot Net, the number of BeginUpdate are counted
			this.EndUpdate();
			this.ResumeLayout();
		}

		/// <summary>
		/// rebuild an image list for this list view, by getting the image from the Brick Library,
		/// but by keeping the same order as the orginial order in this ListView
		/// </summary>
		/// <returns>a list of image for this ListView</returns>
		public List<Image> reconstructImageListFromBrickLibrary()
		{
			// create a list of image with all the original part image from the part lib
			// but do it in the correct order, so we use an array for that to put the original image
			// directly inside the good index (so at the right place)
			// also since the user can change the proportion flag while the list is filtered,
			// also take into account the filtered list view items
			Image[] imageArray = new Image[mVisibleItems.Count + mFilteredItems.Count + mNotBudgetedItems.Count];
			foreach (ListViewItem item in mVisibleItems)
				imageArray[item.ImageIndex] = BrickLibrary.Instance.getImage(item.Tag as string);
			foreach (ListViewItem item in mFilteredItems)
				imageArray[item.ImageIndex] = BrickLibrary.Instance.getImage(item.Tag as string);
			foreach (ListViewItem item in mNotBudgetedItems)
				imageArray[item.ImageIndex] = BrickLibrary.Instance.getImage(item.Tag as string);
			// return the result as a list of image
			return (new List<Image>(imageArray));
		}

		/// <summary>
		/// Update the background color according to the current filtering state. If there's some filtering keywords
		/// it has the priority, otherwise check if we display only the budgeted part.
		/// </summary>
		public void updateBackgroundColor()
		{
			if (IsFiltered)
				this.BackColor = Properties.Settings.Default.PartLibFilteredBackColor;
			else if (Budget.Budget.Instance.ShouldShowOnlyBudgetedParts)
				this.BackColor = Properties.Settings.Default.PartLibShowOnlyBudgetedPartsColor;
			else
				this.BackColor = Properties.Settings.Default.PartLibBackColor;
		}
		#endregion

		#region event handler
		private void beginEditLabel()
		{
			mIsEditingBudget = true;
			mItemForLabelEdit.Text = Budget.Budget.Instance.getBudgetAsString(mItemForLabelEdit.Tag as string, true);
			mItemForLabelEdit.BeginEdit();
		}

		/// <summary>
		/// begin the edition of the current selected item if any. Otherwise do nothing.
		/// </summary>
		public void editCurrentSelectedItemLabel()
		{
			if (this.SelectedIndices.Count > 0)
			{
				mItemForLabelEdit = this.SelectedItems[0];
				beginEditLabel();
			}
		}

		private void PartListView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
		{
			// check if we need to cancel the edition, cause Mono just start the edition on a simple click
			e.CancelEdit = (mItemForLabelEdit == null) ||
				!mItemForLabelEdit.Text.Equals(Budget.Budget.Instance.getBudgetAsString(mItemForLabelEdit.Tag as string, true));
		}

        private void PartListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
			// cancel the edition in any case, cause if the edition is correct, we will use a formated text
			e.CancelEdit = true;

			// get the partID
			string partID = this.Items[e.Item].Tag as string;

			// if the user escaped the edition, its null, so do not change the label in that case, otherwise try to change it
			if (e.Label != null)
			{
				// by default it's an inifinte budget. Anything not parsable will result in an infinite budget (meaning the user erase the budget)
				int newBudget = -1;
				// try to parse as int (positive number)
				if (int.TryParse(e.Label, out newBudget))
				{
					// every negative number will result as - 1 for infinite budget
 					if (newBudget < -1)
						newBudget = -1;
				}
				else
				{
					newBudget = -1;
				}

				// add the current count and change the text myself
				// set the budget first
				Budget.Budget.Instance.setBudget(partID, newBudget);
				// before asking its formating
				updatePartTextAndBackColor(this.Items[e.Item]);
				// check if we have unbudgeted the part
				if (newBudget == -1)
					updateFilterForUnbudgetingPart(this.Items[e.Item]);
				// Bug Mono: if you change the text for a longer text, it keeps the previous displayed size
				// even if there's enough space to display the new long text. A resize will recompute the display length of the texts
				this.Size += new Size(1, 1);
			}
			else
			{
				// just set back the proper text after the edition
				updatePartTextAndBackColor(this.Items[e.Item]);
			}

			// reset the label for edition if we are really editing the budget
			if (mIsEditingBudget)
			{
				mIsEditingBudget = false;
				mItemForLabelEdit = null;
			}
		}

		/// <summary>
		/// Override the HitTest method, cause on Mono, this method throw an exception if the location
		/// is outside of the client area
		/// </summary>
		/// <param name="location">where you want to test</param>
		/// <returns>the hit test result</returns>
		public new ListViewHitTestInfo HitTest(Point location)
		{
			ListViewHitTestInfo result = null;
			try
			{
				result = base.HitTest(location);
			}
			catch
			{
				result = new ListViewHitTestInfo(null, null, ListViewHitTestLocations.None);
			}
			return result;
		}

        private void PartListView_MouseDown(object sender, MouseEventArgs e)
        {
			// MONO crash bug: do not try to change the selected item in this event handler,
			// or it mess up the selection and leads to crash on Mono

			// reset the item pointer
			mItemForLabelEdit = null;

			// check if we click with the left button
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				// get the info to know if we click an item and where
				ListViewHitTestInfo hitTest = this.HitTest(e.Location);
				if (hitTest.Item != null)
				{
					// check where the user clicked, if it's on the label, we have a chance that he want to edit the label
					if (hitTest.Location == ListViewHitTestLocations.Label)
					{
						// and select the clicked one
						mItemForLabelEdit = hitTest.Item;
					}
					else
					{
						// enable the add on click (will be cleared if we leave the view)
						mItemHitOnMouseDownForAdding = hitTest.Item;
					}
				}
			}
		}

		private void PartListView_MouseUp(object sender, MouseEventArgs e)
		{
			// check if we click with the left button
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				// check where the user clicked, if it's on the label, we have a chance that he want to edit the label
				ListViewHitTestInfo hitTest = this.HitTest(e.Location);
				if (hitTest.Item != null)
				{
					// check if we click on the label to edit it or on the item to add it in the map
					if (hitTest.Location == ListViewHitTestLocations.Label)
					{
						// check if the clicked one is still the same
						if (hitTest.Item == mItemForLabelEdit)
							beginEditLabel();
					}
					else if (hitTest.Item == mItemHitOnMouseDownForAdding)
					{
						// reset the item flag
						mItemHitOnMouseDownForAdding = null;
						// and add an item if the part number is valid
						string partNumber = hitTest.Item.Tag as string;
						if (partNumber != null)
							Map.Instance.addConnectBrick(partNumber);
					}
				}
			}
		}

		void PartListView_MouseLeave(object sender, EventArgs e)
		{
			// clear the item hit if we moved out of the view
			mIsEditingBudget = false;
			mItemForLabelEdit = null;
			mItemHitOnMouseDownForAdding = null;	
		}

		#endregion

		#region related to budget
		public void updatePartTextAndBackColor(ListViewItem item)
		{
			// get the part id
			string partID = item.Tag as string;
			
			// first get the part info if it is displayed
			string itemText = string.Empty;
			if (Properties.Settings.Default.PartLibDisplayPartInfo)
			{
				itemText = BrickLibrary.Instance.getFormatedBrickInfo(partID,
								Properties.Settings.Default.PartLibBubbleInfoPartID,
								Properties.Settings.Default.PartLibBubbleInfoPartColor,
								Properties.Settings.Default.PartLibBubbleInfoPartDescription) + "\n";
			}

			// then concatene the part info with the budget if we have some
			if (Budget.Budget.Instance.ShouldShowBudgetNumbers)
			{
				itemText += Budget.Budget.Instance.getCountAndBudgetAsString(partID);
				item.BackColor = Budget.Budget.Instance.getBudgetBackgroundColor(partID);
			}
			else
			{
				item.BackColor = System.Drawing.Color.Empty;
			}

			// set the resulting text
			item.Text = itemText;
		}

		/// <summary>
		/// Iterate on all the items of this list view (no matter if they are filtered or not)
		/// and update the text and background color of the text for each of these items, based
		/// on the budget and count value in the current Budget class.
		/// </summary>
		private void updateAllPartTextAndBackColor()
		{
			// iterate on the 3 lists
			// the item list
			foreach (ListViewItem item in mVisibleItems)
				updatePartTextAndBackColor(item);
			// the filtered items
			foreach (ListViewItem item in mFilteredItems)
				updatePartTextAndBackColor(item);
			// the non budgeted items
			foreach (ListViewItem item in mNotBudgetedItems)
				updatePartTextAndBackColor(item);
		}

		/// <summary>
		/// Update the part count and budget text written under the part in the part library for the specified part id.
		/// </summary>
		/// <param name="partID">The part id of the part that you want to update</param>
		public void updatePartCountAndBudget(string partID)
		{
			// since the partID is saved in the tag, no other choice than iterating on the list (cannot use Find() for example)
			// no need to suspend the layout since we will update only one item
			ListViewItem item = getListViewItemFromTag(partID);
			if (item != null)
				updatePartTextAndBackColor(item);
		}

		/// <summary>
		/// Update the budget of all the parts in the list view, by asking the current Budget class.
		/// This method should be called after loading a new budget file for example, or after reseting
		/// the budget.
		/// </summary>
		public void updateAllPartCountAndBudget()
		{
			// suspend the layout, since we will update all the items
			this.SuspendLayout();
			this.BeginUpdate();
			// iterate on all items
			updateAllPartTextAndBackColor();
			// resume the layout
			this.EndUpdate();
			this.ResumeLayout();
		}

		/// <summary>
		/// Change the view style of this list view depending if the budget and/or the part info are visible or not.
		/// The budget visibility and part info visibility are checked from the settings, so update the settings before calling this method.
		/// If the budget or part info is visible, the view uses LargeIcon, otherwise, it uses Tile.
		/// <param name="shouldUpdateItemText"/>Tells if we should also update the text of each list view items</param>
		/// </summary>
		public void updateViewStyle(bool shouldUpdateItemText)
		{
			// update all the item text if we need to
			if (shouldUpdateItemText)
				updateAllPartCountAndBudget();
			// change the view style
			if (Budget.Budget.Instance.ShouldShowBudgetNumbers || Properties.Settings.Default.PartLibDisplayPartInfo)
				this.View = View.LargeIcon;
			else
				this.View = View.Tile;
		}
		
		/// <summary>
		/// Filter or not the part lib with the budgeted parts, depending on the current value of the setting
		/// </summary>
		public void updateFilterOnBudgetedParts()
		{
			// suspend the layout since we will remove some items from the list
			this.SuspendLayout();
			this.BeginUpdate();

			// put back all the previous not budgeted item in the list, and then we will iterate on all the items in the
			// list to remove them again. We do that, because it may happen that some items were not visible because they
			// didn't have a budget, then we load a Budget file, and these items now have a budget
			moveItemsToVisibleList(mNotBudgetedItems);
			// put back all the filtered items also, we will refilter the list after
			moveItemsToVisibleList(mFilteredItems);

			// if we need to remove the non budgeted parts, put the non budgeted parts in the temporary list, otherwise put them back in the visible list
			if (Budget.Budget.Instance.ShouldShowOnlyBudgetedParts)
			{
				try
				{
					// do not use a foreach here because Mono doesn't support to remove items while iterating on the list
					// so use an index instead and decrease the index when we remove the item
					for (int i = 0; i < mVisibleItems.Count; ++i)
					{
						ListViewItem item = mVisibleItems[i];
						string itemId = item.Tag as string;
						// filter on the budget: if not budgeted, remove it
						if (!Budget.Budget.Instance.IsBudgeted(itemId))
						{
							mVisibleItems.RemoveAt(i);
							i--;
							mNotBudgetedItems.Add(item);
						}
					}
				}
				catch
				{
				}
			}

			// after having updated the visible list, copy it to the actual list of items of this listView
			resetThisListViewWithVisibleList();

			// resume the layout
			this.EndUpdate();
			this.ResumeLayout();

			// refilter after move the (non) budgeted parts if needed
			// if we don't filter on budget, we have already put all the budget in, so we just need to resort the list,
			// and the refilter will also sort, so we don't need to sort if we refilter
			if (this.IsFiltered)
				this.refilter();
			else
				this.Sort();

			// update the background color with the default one
			updateBackgroundColor();
		}

		/// <summary>
		/// If the list view is filtered to display only budgeted part, in case the user un-budget a part
		/// (i.e. delete its budget), we need to make it disapear from the list of item and place it in the
		/// not budgeted part temporary list
		/// </summary>
		/// <param name="unbudgetedItem">The item whose budget was removed</param>
		private void updateFilterForUnbudgetingPart(ListViewItem unbudgetedItem)
		{
			if (Budget.Budget.Instance.ShouldShowOnlyBudgetedParts)
			{
				// search in the visible list and in the filtered list (but this last case should never happen
				// as the item must be in the list to be un-bugeted, otherwise the user cannot edit its buget)
				if (mVisibleItems.Contains(unbudgetedItem))
				{
					mVisibleItems.Remove(unbudgetedItem);
					// also remove it from the actual list of items
					unbudgetedItem.Remove();
					// then add it to the list of unbugeted items
					mNotBudgetedItems.Add(unbudgetedItem);
				}
				else if (this.mFilteredItems.Contains(unbudgetedItem))
				{
					this.mFilteredItems.Remove(unbudgetedItem);
					// then add it to the list of unbugeted items
					mNotBudgetedItems.Add(unbudgetedItem);
				}
			}
		}
        #endregion
    }
}
