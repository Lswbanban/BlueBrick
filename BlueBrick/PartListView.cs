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

		// create a temporary list view to temporary store the items that have been filtered in this list view
		private List<ListViewItem> mFilteredItems = new List<ListViewItem>();
		// create another temporary list view to temporary store the items that are not budgeted in this listview
		private List<ListViewItem> mNotBudgetedItems = new List<ListViewItem>();
		
		// the filter sentence
		private string mFilterSentence = string.Empty;
		private bool mIsFilterSentenceSynchroWithCurrentFiltering = false;

        // for label edition
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
			updateViewStyle(); // set the view style depending if budget need to be visible or not
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
            this.MultiSelect = false;
            this.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.PartListView_AfterLabelEdit);
            this.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.PartListView_BeforeLabelEdit);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PartListView_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PartListView_MouseUp);
			this.MouseLeave += new EventHandler(this.PartListView_MouseLeave);
			this.ResumeLayout(false);
        }

		/// <summary>
		/// This method is only to encapsulate a Mono bug that throw an exception for the 16th item added.
		/// It will add the item to the list
		/// </summary>
		/// <param name="itemToAdd">The item to add to the list</param>
		private void addItemToItemsList(ListViewItem itemToAdd)
		{
			try
			{
				// for a strange reason, on mono, this method throw an exception for all the
				// item added after the 16th one added (but the item is still added).
				// Probably a bug from Mono.
				this.Items.Add(itemToAdd);
			}
			catch
			{
				// so ignore this exception for mono, otherwise it is displayed in the error message box
			}
		}

		/// <summary>
		/// This method is only to encapsulate a Mono bug that throw an exception for the 16th item added.
		/// It will add the list of item to the list
		/// </summary>
		/// <param name="itemToAdd">The item to add to the list</param>
		private void addItemToItemsList(ListViewItem[] itemsToAdd)
		{
			try
			{
				// for a strange reason, on mono, this method throw an exception for all the
				// item added after the 16th one added (but the item is still added).
				// Probably a bug from Mono.
				this.Items.AddRange(itemsToAdd);
			}
			catch
			{
				// so ignore this exception for mono, otherwise it is displayed in the error message box
			}
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
				addItemToItemsList(itemToAdd);
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
			addItemToItemsList(mFilteredItems.ToArray());
			// On Mono, the AddRange call the EndUpdate, so we put back the begin update, because we want to continue to update
			// On Dot Net, the BeginUpdate are counted, but not on Mono, so we will have to put another EndUpdate
			this.BeginUpdate();

			// clear the list
			mFilteredItems.Clear();
			// resort the list view
			this.Sort();
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
					for (int i = 0; i < this.Items.Count; ++i)
					{
						ListViewItem item = this.Items[i];
						string itemId = item.Tag as string;
						string brickInfo = BrickLibrary.Instance.getFormatedBrickInfo(itemId, true, true, true).ToLower();
						// a flag to stop search if it is already removed
						bool continueSearch = true;
						// iterate on the 3 filter lists
						// first filter on the id only
						foreach (string filter in includeIdFilter)
							if (!itemId.Contains(filter))
							{
								item.Remove();
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
									item.Remove();
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
									item.Remove();
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
			Image[] imageArray = new Image[this.Items.Count + mFilteredItems.Count + mNotBudgetedItems.Count];
			foreach (ListViewItem item in this.Items)
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
		/// <summary>
		/// begin the edition of the current selected item if any. Otherwise do nothing.
		/// </summary>
		public void editCurrentSelectedItemLabel()
		{
			if (this.SelectedIndices.Count > 0)
			{
				mItemForLabelEdit = this.SelectedItems[0];
				mItemForLabelEdit.Text = Budget.Budget.Instance.getBudgetAsString(mItemForLabelEdit.Tag as string);
				mItemForLabelEdit.BeginEdit();
			}
		}

        private void PartListView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            // put node label to initial state
            // to ensure that in case of label editing cancelled
            // the initial state of label is preserved
			this.Items[e.Item].Text = Budget.Budget.Instance.getCountAndBudgetAsString(this.Items[e.Item].Tag as string);
        }

        private void PartListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
			// cancel the edition in any case, cause if the edition is correct, we will use a formated text
			e.CancelEdit = true;

			// if the user escaped the edition, its null, so donot change the label
			if (e.Label == null)
				return;

			// by default it's an inifinte budget. Anything not parsable will result in an infinite budget (meaning the user erase the budget)
			int newBudget = -1;
            // check if it is an int
            try
            {
                // try to parse as int (positive number)
				newBudget = int.Parse(e.Label);
				if (newBudget < -1)
					newBudget = -1;
            }
            catch
            {
            }

			// get the partID
			string partID = this.Items[e.Item].Tag as string;
			// add the current count and change the text myself
			// set the budget first
			Budget.Budget.Instance.setBudget(partID, newBudget);
			// before asking its formating
			this.Items[e.Item].Text = Budget.Budget.Instance.getCountAndBudgetAsString(partID);
			this.Items[e.Item].BackColor = Budget.Budget.Instance.getBudgetBackgroundColor(partID);
			// check if we have unbudgeted the part
			if (newBudget == -1)
				updateFilterForUnbudgetingPart(this.Items[e.Item]);
		}

        private void PartListView_MouseDown(object sender, MouseEventArgs e)
        {
			// reset the item index
			mItemForLabelEdit = null;

			// check if we click with the left button
			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				// unselect the previous item if we click left
				foreach (int index in this.SelectedIndices)
					this.Items[index].Selected = false;

				// get the info to know if we click an item and where
				ListViewHitTestInfo hitTest = this.HitTest(e.Location);
				if (hitTest.Item != null)
				{
					// if we click on one item, select it
					hitTest.Item.Selected = true;

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
						{
							mItemForLabelEdit.Text = Budget.Budget.Instance.getBudgetAsString(mItemForLabelEdit.Tag as string);
							mItemForLabelEdit.BeginEdit();
						}
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
			mItemForLabelEdit = null;
			mItemHitOnMouseDownForAdding = null;	
		}

		#endregion

		#region related to budget
		private void updatePartTextAndBackColor(ListViewItem item, string onlyForThisPartId)
		{
			string partID = item.Tag as string;
			if ((onlyForThisPartId == null) || (partID.Equals(onlyForThisPartId)))
			{
				item.Text = Budget.Budget.Instance.getCountAndBudgetAsString(partID);
				item.BackColor = Budget.Budget.Instance.getBudgetBackgroundColor(partID);
			}
		}

		/// <summary>
		/// Iterate on all the items of this list view (no matter if they are filtered or not)
		/// and update the text and background color of the text for each of these items, based
		/// on the budget and count value in the current Budget class.
		/// </summary>
		/// <param name="onlyForThisPartId">can be null for updating all items, or a non null string to update only the specified item</param>
		private void updateAllPartTextAndBackColor(string onlyForThisPartId)
		{
			// iterate on the 3 lists
			// the item list
			foreach (ListViewItem item in this.Items)
				updatePartTextAndBackColor(item, onlyForThisPartId);
			// the filtered items
			foreach (ListViewItem item in mFilteredItems)
				updatePartTextAndBackColor(item, onlyForThisPartId);
			// the non budgeted items
			foreach (ListViewItem item in mNotBudgetedItems)
				updatePartTextAndBackColor(item, onlyForThisPartId);
		}

		public void updatePartCountAndBudget(string partID)
		{
			// since the partID is saved in the tag, no other choice than iterating on the list (cannot use Find() for example)
			// no need to suspend the layout since we will update only one item
			updateAllPartTextAndBackColor(partID);
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
			updateAllPartTextAndBackColor(null);
			// resume the layout
			this.EndUpdate();
			this.ResumeLayout();
		}

		/// <summary>
		/// Change the view style of this list view depending if the budget is visible or not.
		/// The budget visibility is checked from the settings, so update the settings before calling this method.
		/// If the budget is visible, the view uses LargeIcon, otherwise, it uses Tile.
		/// </summary>
		public void updateViewStyle()
		{
			if (Budget.Budget.Instance.ShouldShowBudgetNumbers)
				this.View = View.LargeIcon;
			else
				this.View = View.Tile;
		}
		
		/// <summary>
		/// Filter or not the part lib with the budgeted parts, depending on the current value of the setting
		/// </summary>
		public void updateFilterOnBudgetedParts()
		{
			// use a bool flag to store the current filter state, because we will unfilter all, which will clear the flag
			bool wasFiltered = this.IsFiltered;

			// if the list view is currently filtered, unfilter it temporarly, then we will refilter it after having moved
			// the some items to/from the non budgeted item list
			if (wasFiltered)
				this.filter(string.Empty, false);

			// suspend the layout since we will remove some items from the list
			this.SuspendLayout();
			this.BeginUpdate();

			// put back all the previous filtered item in the list, and then we will iterate on all the items in the
			// list to remove them again. We do that, because it may happen that some items were filtered because they
			// didn't have a budget, then we load a Budget file, and these items now have a budget
			addItemToItemsList(mNotBudgetedItems.ToArray());
			// On Mono, the AddRange call the EndUpdate, so we put back the begin update, because we want to continue to update
			// On Dot Net, the BeginUpdate are counted, but not on Mono, so we will have to put another EndUpdate
			this.BeginUpdate();
			// clear the list
			mNotBudgetedItems.Clear();

			// if we need to filter, put the non budgeted parts in the temporary list, otherwise put them back in the listview
			if (Budget.Budget.Instance.ShouldShowOnlyBudgetedParts)
			{
				try
				{
					// do not use a foreach here because Mono doesn't support to remove items while iterating on the list
					// so use an index instead and decrease the index when we remove the item
					for (int i = 0; i < this.Items.Count; ++i)
					{
						ListViewItem item = this.Items[i];
						string itemId = item.Tag as string;
						// first filter on the budget: if not budgeted, remove it
						if (!Budget.Budget.Instance.IsBudgeted(itemId))
						{
							item.Remove();
							i--;
							mNotBudgetedItems.Add(item);
						}
					}
				}
				catch
				{
				}
			}
			else
			{
				// if we don't filter on budget, we have already put all the budget in, so we just need
				// to resort the list, but we only sort if we don't refilter (cause refilter will resort)
				// resort the list view if we added some item (unless we will refilter it, which will cause a sort)
				if (!wasFiltered)
					this.Sort();
			}

			// resume the layout
			this.EndUpdate();
			// call it a second time because on Dot Net, the number of BeginUpdate are counted
			this.EndUpdate();
			this.ResumeLayout();

			// refilter after move the (non) budgeted parts if needed
			if (wasFiltered)
				this.refilter();

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
				// search in the item list and in the filtered list (but this last case should never happe
				// as the item must be in the list to be un-bugeted, otherwise the user cannot edit its buget)
				if (this.Items.Contains(unbudgetedItem))
				{
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
