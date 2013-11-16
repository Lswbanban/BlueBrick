using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;
using System.Drawing;

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

		// create a temporary list view to temporary store the items that have been filtered on the current tab
		private List<ListViewItem> mFilteredItems = new List<ListViewItem>();
		
		// the filter sentence
		private string mFilterSentence = string.Empty;
		private bool mIsFilterSentenceSynchroWithCurrentFiltering = false;

		#region get/set
		public bool IsFiltered
		{
			get { return (mFilterSentence != string.Empty); }
		}

		public string FilterSentence
		{
			get { return mFilterSentence; }
		}
		#endregion

		#region constructor
		public PartListView()
		{
			// set the property of the list view
			this.Anchor = AnchorStyles.Top | AnchorStyles.Left;
			this.Dock = DockStyle.Fill;
			this.Alignment = ListViewAlignment.SnapToGrid;
			this.BackColor = Properties.Settings.Default.PartLibBackColor;
			this.ShowItemToolTips = Properties.Settings.Default.PartLibDisplayBubbleInfo;
			this.MultiSelect = false;
			this.ListViewItemSorter = sListViewItemComparer; // we want to sort the items based on their Name (which contains a sorting key)
			this.View = View.Tile; // we always use this view, because of the layout of the item
		}
		#endregion
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
		/// Call the filter function with the currently saved filter sentence
		/// </summary>
		public void refilter()
		{
			filter(mFilterSentence, false);
		}

        /// <summary>
        /// Call the filter function with the currently saved filter sentence
        /// <param name="forceEvenIfNotNeeded">if true the filtering will be performed, even if it is not needed</param>
        /// </summary>
        public void refilter(bool forceEvenIfNotNeeded)
        {
            if (forceEvenIfNotNeeded)
                mIsFilterSentenceSynchroWithCurrentFiltering = false;
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
			this.SuspendLayout();

			//put back all the previous filtered item in the list
			foreach (ListViewItem item in mFilteredItems)
				this.Items.Add(item);
			// clear the list
			mFilteredItems.Clear();
			// resort the list view
			this.Sort();
			// clear the background color with the default one
			this.BackColor = Properties.Settings.Default.PartLibBackColor;

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
			Image[] imageArray = new Image[this.Items.Count + mFilteredItems.Count];
			foreach (ListViewItem item in this.Items)
				imageArray[item.ImageIndex] = BrickLibrary.Instance.getImage(item.Tag as string);
			foreach (ListViewItem item in mFilteredItems)
				imageArray[item.ImageIndex] = BrickLibrary.Instance.getImage(item.Tag as string);
			// return the result as a list of image
			return (new List<Image>(imageArray));
		}
	}
}
