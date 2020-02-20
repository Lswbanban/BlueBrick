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
using System.Text;
using BlueBrick.MapData;
using System.Xml.Serialization;

namespace BlueBrick.Budget
{
	/// <summary>
	/// The budget class hold a limit number possibly for every part of the library.
	/// The budget will only maintain the budget numbers that were defined by the user.
	/// A non defined budget will return either 0 or -1 (infinity) depending on the preference settings.
	/// The budget class also keep the count of every part on the map, but do not save it in its 
	/// XML serialization.
	/// </summary>
	[Serializable]
	public class Budget : IXmlSerializable
	{
		// instance on the budget (the application only handle one budget for now)
		private static Budget sInstance = new Budget();

		// the current version of the data this version of BlueBrick can read/write
		private const int CURRENT_DATA_VERSION = 1;

		// the current version of the data
		private static int mDataVersionOfTheFileLoaded = CURRENT_DATA_VERSION;

		// for the current budget
		private string mBudgetFileName = Properties.Resources.DefaultSaveFileNameForBudget;
		private bool mIsFileNameValid = false;
		private bool mIsExisting = false; // tell if a budget currently exists (was created with new, or opened)
		private bool mWasModified = false;

		// the budget if the limit set by the user for each brick
		private Dictionary<string, int> mBudget = new Dictionary<string,int>();

		// the count is the actual brick total number in the map
		private Dictionary<string, int> mCount = new Dictionary<string, int>();

		// We also store the count of brick on the map but per layer, in order to facilitate the statistics with the part usage view
		private Dictionary<LayerBrick, Dictionary<string, int>> mCountPerLayer = new Dictionary<LayerBrick, Dictionary<string, int>>();

		#region get/set
		/// <summary>
		/// The static instance of the Budget
		/// </summary>
		public static Budget Instance
		{
			get { return sInstance; }
			set { sInstance = value; }
		}

		public string BudgetFileName
		{
			get { return mBudgetFileName; }
			set { mBudgetFileName = value; }
		}

		public bool IsFileNameValid
		{
			get { return mIsFileNameValid; }
			set { mIsFileNameValid = value; }
		}

		public bool IsExisting
		{
			get { return mIsExisting; }
		}

		public bool ShouldShowOnlyBudgetedParts
		{
			get { return (mIsExisting && Properties.Settings.Default.ShowOnlyBudgetedParts); }
		}

		public bool ShouldShowBudgetNumbers
		{
			get { return (mIsExisting && Properties.Settings.Default.ShowBudgetNumbers); }
		}

		public bool ShouldUseBudgetLimitation
		{
			get { return (mIsExisting && Properties.Settings.Default.UseBudgetLimitation); }
		}

		public bool WasModified
		{
			get { return mWasModified; }
			set
			{
 				// check if the state will change
				bool stateChanged = (mWasModified != value);
				// change the flag
				mWasModified = value;
				//if the state changed, call the update of the title bar
				if (stateChanged)
					MainForm.Instance.updateTitleBar();
			}
		}
		#endregion

		#region IXmlSerializable Members
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public virtual void ReadXml(System.Xml.XmlReader reader)
		{
			// The budget is already cleared cause a new instance of Budget is created for serialization

			// set the existing flag to false before reading. If the reading goes well (no exception thrown) we will set it to true
			mIsExisting = false;

			// version
			reader.ReadToDescendant("Version");
			mDataVersionOfTheFileLoaded = reader.ReadElementContentAsInt();

			// read the parts
			bool partFound = reader.ReadToDescendant("Part");
			while (partFound)
			{
				// read the part name and value (get the new value if it was renamed)
				string partId = BrickLibrary.Instance.getActualPartNumber(reader.GetAttribute("id"));
				int budget = reader.ReadElementContentAsInt();
				// and set the budget
				mBudget.Add(partId, budget);
				// read the next part
				partFound = reader.Name.Equals("Part");
			}
			// read the PartList tag, to finish the list of parts
			reader.Read();

			// after that the reading went well (no exception thrown)
			// set the flag to tell that the budget now exists
			mIsExisting = true;
			// reset the was modified flag cause we just load a new budget
			this.WasModified = false;
		}

		public virtual void WriteXml(System.Xml.XmlWriter writer)
		{
			// reset the was modified flag each time we save
			this.WasModified = false;

			// first of all the version, we don't use the vesion read from the file,
			// for saving we always save with the last version of data
			writer.WriteElementString("Version", CURRENT_DATA_VERSION.ToString());

			// now write the budget list
			writer.WriteStartElement("PartList");
			foreach (KeyValuePair<string, int> budget in mBudget)
			{
				writer.WriteStartElement("Part");
				writer.WriteAttributeString("id", budget.Key);
				writer.WriteString(budget.Value.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// reinit the whole budget.
		/// </summary>
		private void init()
		{
			// clear the budget but do not clear the count (as you may change your budget, but the number of part didn't changed if you didn't changed your map)
			mBudget.Clear();
			// reset the flags
			mBudgetFileName = Properties.Resources.DefaultSaveFileNameForBudget;
			mIsFileNameValid = false;
			// reset the was modified flag
			this.WasModified = false;
		}

		/// <summary>
		/// create a new budget by reiniting the current one
		/// </summary>
		public void create()
		{
			// reinit the budget
			init();
			// set the flag to tell that the budget now exist
			mIsExisting = true;
		}

		/// <summary>
		/// call this function if you want to destroy this budget. Be careful, no warning will be raised,
		/// warning message has to be handled by the main form.
		/// </summary>
		public void destroy()
		{
			// reinit the budget
			init();
			// and set the flag to tell that no budget is created
			mIsExisting = false;
		}

		public void mergeWith(Budget budgetToMerge)
		{
			int budgetValue = 0;
			foreach (KeyValuePair<string, int> budget in budgetToMerge.mBudget)
				if (mBudget.TryGetValue(budget.Key, out budgetValue))
				{
					mBudget.Remove(budget.Key);
					mBudget.Add(budget.Key, budget.Value + budgetValue);
				}
				else
				{
					mBudget.Add(budget.Key, budget.Value);
				}
			// set the was modified flag, if we actually merge something (otherwise don't touch the flag)
			if (budgetToMerge.mBudget.Count > 0)
				this.WasModified = true;
		}
		#endregion

		#region update
		/// <summary>
		/// This method iterate on all the budgeted part, and update the partid, for parts that have been renamed
		/// <returns>true if some parts have been updated</returns>
		/// </summary>
		public bool updatePartId()
		{
			// create a list of budget that we need to rename, cause we will iterate on the dictionnary
			List<KeyValuePair<string, int>> budgetsToRename = new List<KeyValuePair<string, int>>();

			// iterate on the dictionary
			foreach (KeyValuePair<string, int> budget in mBudget)
			{
				string newPartId = BrickLibrary.Instance.getActualPartNumber(budget.Key);
				if (!newPartId.Equals(budget.Key))
					budgetsToRename.Add(budget);
			}

			// now remove all the old keys, and add the new ones in both budget and count dictionary
			foreach (KeyValuePair<string, int> budget in budgetsToRename)
			{
				// rename in the budget
				string newPartId = BrickLibrary.Instance.getActualPartNumber(budget.Key);
				mBudget.Remove(budget.Key);
				mBudget.Add(newPartId, budget.Value);
				// no need to rename the count, because the map as been unloaded when this function is called
			}

			// change the flag if we modified the budget (but don't change it, if we didn't modified it)
			if (budgetsToRename.Count > 0)
			{
				this.WasModified = true;
				return true;
			}
			// by default return false
			return false;
		}
		#endregion

		#region budget management
		/// <summary>
		/// Tell if the specified part has a budget defined which is not null
		/// </summary>
		/// <param name="partID">the part id for which you want to know if it has a budget</param>
		/// <returns>true if the budget is defined (not infinite and not null)</returns>
		public bool IsBudgeted(string partID)
		{
			return (getBudget(partID) > 0);
		}

		/// <summary>
		/// Set a budget number for the specified part id.
		/// </summary>
		/// <param name="partID">the full part id for which you want to set the budget</param>
		/// <param name="budget">the budget number (can be negative or null)</param>
		public void setBudget(string partID, int budget)
		{
			// get the current value if any, and remove the it before seting the new one
			int currentBudget = 0;
			if (mBudget.TryGetValue(partID, out currentBudget))
				mBudget.Remove(partID); // if we found the value, remove it to avoid exception when adding it, or maybe we won't add it anymore if it's infinite budget
			else
				currentBudget = -1; // this is necessary, cause the TryGetValue set the value to zero if not found
			// set the new budget but only if it is defined (otherwise a -1 budget, means, it should not be included in the list)
			if (budget >= 0)
				mBudget.Add(partID, budget);
			else
				budget = -1; // for any negative value, transform it into -1 (for the comparison below)
			// every time we set a new budget (with a different value), change the modified flag. But do not change the flag if it is already true.
			if (budget != currentBudget)
				this.WasModified = true;
			// notify the MainForm for budget change
			MainForm.Instance.NotifyForBudgetChanged(partID);
		}

		/// <summary>
		/// get the budget number associated with the specified part number if any.
		/// If no budget is associated with this part, 0 is returned.
		/// </summary>
		/// <param name="partID">the full part id for which you want to know the budget</param>
		/// <returns>the budget for that part or 0 if there's no budget</returns>
		public int getBudget(string partID)
		{
			// try to get the value or return 0 by default
			int result = 0; //-1 means the budget is not set, i.e. you have an infinite budgets
			if (!mBudget.TryGetValue(partID, out result))
				result = Properties.Settings.Default.IsDefaultBudgetInfinite ? -1 : 0; 
			return result;
		}

		/// <summary>
		/// Get the total budget count of brick present in this budget
		/// </summary>
		/// <returns>The number of bricks that can be budgeted</returns>
		public int getTotalBudget()
		{
			int total = 0;
			foreach (int count in mBudget.Values)
				total += count;
			return total;
		}

		/// <summary>
		/// get the percentage of usage of the specified part, according to its current budget.
		/// If no budget is associated with this part (illimited budget), a negative value is returned.
		/// </summary>
		/// <param name="partID">the full part id for which you want to know the usage percentage</param>
		/// <returns>the usage percentage for that part or -1 if there's no budget (illimited budget)</returns>
		public float getUsagePercentage(string partID)
		{
			// try to get the value or return 0 by default
			float result = -1f; //-1 means the budget is not set, i.e. you have an infinite budgets
			int budget = getBudget(partID);
			int count = getCount(partID);
			if (budget < 0)
			{
				// if budget is negative, that means infinite budget, so the percentage will also be negative
				result = -1f;
			}
			else if (budget == 0)
			{
				// if the budget is null, you use all of it if you have no part,
				// otherwise you exceed the budget, and each part count as a 100% exceed
				result = (count + 1) * 100f;
			}
			else
			{
				result = (float)(count * 100) / (float)budget;
			}
			// return the result
			return result;
		}

		/// <summary>
		/// Get the total usage percentage of the whole map, i.e. the total count of brick divided
		/// by the total count of budget.
		/// </summary>
		/// <param name="shouldIncludeHiddenParts">tell if we should count the hidden parts</param>
		/// <returns>The percentage of the budget already used on the current map</returns>
		public float getTotalUsagePercentage(bool shouldIncludeHiddenParts)
		{
			return (float)getTotalCount(shouldIncludeHiddenParts) / (float)getTotalBudget();
		}

		/// <summary>
		/// Get the percentage of usage for the specified layer, according to the current budget.
		/// This method will iterate on all the bricks present on the specified layer and will
		/// sum all the budget of those bricks. Then it will compute a percentage between the
		/// total count of the budgeted bricks on this layer, and the total budget count computed.
		/// </summary>
		/// <param name="layer">The layer in which you want to know the brick usage</param>
		/// <returns>the brick usage percentage on the specified layer</returns>
		public float getUsagePercentageForLayer(LayerBrick layer)
		{
			float result = 0;
			// try to get the specified layer and iterate on all its values
			Dictionary<string, int> layeredCount = null;
			if (mCountPerLayer.TryGetValue(layer, out layeredCount))
			{
				int totalCount = 0;
				int totalBudget = 0;
				// itearate on all the pair on the specified layer
				foreach (KeyValuePair<string, int> brickCount in layeredCount)
				{
					// check if the current brick has a budget, otherwise ignore it
					int budget = 0;
					if (mBudget.TryGetValue(brickCount.Key, out budget))
					{
						// increase the count and budget
						totalCount += brickCount.Value;
						totalBudget += budget;
					}
				}
				// compute the result (if budget is not null to avoid division by zero)
				if (totalBudget > 0)
					result = (float)(totalCount * 100) / (float)totalBudget;
			}
			return result;
		}

		/// <summary>
		/// Return a formated string of the budget that display the current budget if set or the infinity sign if not set
		/// </summary>
		/// <param name="partID">The full part id for which you want to know the budget</param>
		/// <returns>a string displaying the number of the infinity sign</returns>
		public string getBudgetAsString(string partID, bool returnEmptyStringIfNoBudget)
		{
			string budgetString = "?"; // "∞"; // by default if the budget is not set, it is infinite
			int budget = getBudget(partID);
			if (budget >= 0)
				budgetString = budget.ToString();
			else if (returnEmptyStringIfNoBudget)
				budgetString = string.Empty;
			// return the formated string
			return budgetString;
		}

		/// <summary>
		/// Return a formated string in the form "count/budget" that display the current number of part and it's budget.
		/// Depending on the Settings.Default.DisplayRemainingPartCountInBudgetInsteadOfUsedCount the count will be either
		/// the currently used parts, or the remaining part against its bugdet (which could be negative if you exceed the
		/// budget)
		/// </summary>
		/// <param name="partID">The full part id for which you want to know the count and budget</param>
		/// <returns>a string displaying the both number separated by a slash</returns>
		public string getCountAndBudgetAsString(string partID)
		{
			// get the count or remaining count depending on the settings
			int count = Properties.Settings.Default.DisplayRemainingPartCountInBudgetInsteadOfUsedCount ? getRemainingCount(partID) : getCount(partID);
			// retur the formated string
			return (count.ToString() + "/" + getBudgetAsString(partID, false));
		}

		/// <summary>
		/// Get the color that should be used to display the backgroun of count and budget in the list view. If the budget is infinite
		/// or if count is less or equal than budget return transparent, otherwise return red.
		/// </summary>
		/// <param name="partID">The full part id for which you want to know the color</param>
		/// <returns>the appropriate color to display the count and budget</returns>
		public System.Drawing.Color getBudgetBackgroundColor(string partID)
		{
			int budget = getBudget(partID);
			if (budget >= 0)
			{
				int count = getCount(partID);
				if (count > budget)
					return System.Drawing.Color.Red;
			}
			return System.Drawing.Color.Empty;
		}
		#endregion

		#region brick count
		/// <summary>
		/// Get the total number of the specified brick in the current map
		/// </summary>
		/// <param name="partID">the full part id for which you want to know the count</param>
		/// <returns>the number of that brick in the map which could be 0</returns>
		public int getCount(string partID)
		{
			// try to get the value or return 0 by default
			int result = 0;
			mCount.TryGetValue(partID, out result);
			return result;
		}

		/// <summary>
		/// Return the remaining parts available according to the budget set for the specified part.
		/// If the part doesn't have a budget set, then the negative count (number of used parts
		/// with a minus sign in front of it) is returned.
		/// </summary>
		/// <param name="partID">The part for which you want to know the remaining parts</param>
		/// <returns>the number of part you can use before exhausting the budget for this part.</returns>
		public int getRemainingCount(string partID)
		{
			int count = getCount(partID);
			int budget = getBudget(partID);
			if (budget >= 0)
				return (budget - count);
			return -count;
		}

		/// <summary>
		/// Get the total count of brick used in the map. You can specify if you want to also count the parts
		/// that are currently on hidden layers.
		/// </summary>
		/// <param name="shouldIncludeHiddenParts">tell if we should count the hidden parts</param>
		/// <returns>The number of bricks used on the current map, with ot without the hidden parts</returns>
		public int getTotalCount(bool shouldIncludeHiddenParts)
		{
			int total = 0;
			if (shouldIncludeHiddenParts)
			{
				// if we include hidden parts, count on all parts
				foreach (int count in mCount.Values)
					total += count;
			}
			else
			{
				// otherwise iterate on the count per layer, and only count the parts if the layer is visible
				foreach (KeyValuePair<LayerBrick, Dictionary<string, int>> layerPair in mCountPerLayer)
					if (layerPair.Key.Visible)
						foreach (int count in layerPair.Value.Values)
							total += count;
			}
			return total;
		}

		/// <summary>
		/// Get the total number of bricks present in the specified layer of the current map
		/// </summary>
		/// <param name="layer">The layer in which you want to know the count</param>
		/// <returns>the number of bricks in the specified layer of the map which could be 0</returns>
		public int getTotalCountForLayer(LayerBrick layer)
		{
			int result = 0;
			// try to get the specified layer and iterate on all its values
			Dictionary<string, int> layeredCount = null;
			if (mCountPerLayer.TryGetValue(layer, out layeredCount))
				foreach (int count in layeredCount.Values)
					result += count;
			return result;
		}

		/// <summary>
		/// If the budget limitation is enabled, this method will check if the brick count is less than the budget
		/// and return true, otherwise if the limit is reached return false. If the budget is not set for that part,
		/// it will also return true.
		/// If the budget limitation is not enable, this method always return true.
		/// </summary>
		/// <param name="partID">the full part id</param>
		/// <returns>true if you can add this part</returns>
		public bool canAddBrick(string partID)
		{
			// by default we can
			bool canAdd = true;
			// check if we need to check the budget limitation
			if (this.ShouldUseBudgetLimitation)
			{
				// first check with the main brick
				canAdd = canAddBrick(partID, 1);
				// and if it is a group, check that all subparts don't exceed the budget
				if (canAdd)
				{
					// so get the subpart count and if any part fail the whole group will fail
					Dictionary<string, int> subPartCount = BrickLibrary.Instance.getSubPartCount(partID);
					foreach (KeyValuePair<string, int> pair in subPartCount)
						if (!canAddBrick(pair.Key, pair.Value))
							return false;
				}
			}
			return canAdd;
		}

		/// <summary>
		/// If the budget limitation is enabled, this method will check if the brick count is less than the budget
		/// and return true, otherwise if the limit is reached return false. If the budget is not set for that part,
		/// it will also return true.
		/// If the budget limitation is not enable, this method always return true.
		/// </summary>
		/// <param name="partID">the full part id</param>
		/// <param name="quantity">the quantity you want to add</param>
		/// <returns>true if you can add this part</returns>
		public bool canAddBrick(string partID, int quantity)
		{
			if (this.ShouldUseBudgetLimitation)
			{
				int budget = getBudget(partID);
				return ((budget < 0) || (getCount(partID) + quantity <= budget));
			}
			return true;
		}

		/// <summary>
		/// Call this method when you want to notify the budget counter that a new brick has been added
		/// </summary>
		/// <param name="layer">The layer on which the brick is added</param>
		/// <param name="brick">the brick that was added</param>
		/// <param name="isDueToRegroup">tells if the brick is a group that was regrouped by an undo</param>
		public void addBrickNotification(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isDueToRegroup)
		{
			string partID = brickOrGroup.PartNumber;
			if (partID != string.Empty)
			{
				// get the current count
				int currentCount = getCount(partID);
				// and update the value in the global dictionnary
				mCount.Remove(partID);
				mCount.Add(partID, currentCount + 1);

				// also update in the layered dictionary
				Dictionary<string, int> layeredCount = null;
				if (mCountPerLayer.TryGetValue(layer, out layeredCount))
				{
					int currentCountInLayer = 0;
					layeredCount.TryGetValue(partID, out currentCountInLayer);
					layeredCount.Remove(partID);
					layeredCount.Add(partID, currentCountInLayer + 1);
				}
				else
				{
					// this is a new layer (this layer didn't exist before), so add the layer and also the count
					layeredCount = new Dictionary<string, int>();
					layeredCount.Add(partID, 1);
					mCountPerLayer.Add(layer, layeredCount);
				}
			}
			// add also all the named children if the brick is a group
			// (unless it is a regroup in that case the children are already counted)
			if (!isDueToRegroup)
			{
				Layer.Group group = brickOrGroup as Layer.Group;
				if (group != null)
					foreach (Layer.LayerItem item in group.Items)
						addBrickNotification(layer, item, isDueToRegroup);
			}
		}

		/// <summary>
		/// Call this method when you want to notify the budget counter that a brick has been removed
		/// </summary>
		/// <param name="layer">The layer on which the brick is removed</param>
		/// <param name="brick">the brick that was removed</param>
		/// <param name="isDueToUngroup">tell if the brick is a group that is ungrouped</param>
		public void removeBrickNotification(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isDueToUngroup)
		{
			string partID = brickOrGroup.PartNumber;
			if (partID != string.Empty)
			{
				// get the current count
				int currentCount = getCount(partID);
				if (currentCount > 0)
				{
					// update the value
					mCount.Remove(partID);
					mCount.Add(partID, currentCount - 1);
				}

				// also update in the layered dictionary
				Dictionary<string, int> layeredCount = null;
				if (mCountPerLayer.TryGetValue(layer, out layeredCount))
				{
					int currentCountInLayer = 0;
					layeredCount.TryGetValue(partID, out currentCountInLayer);
					if (currentCountInLayer > 0)
					{
						layeredCount.Remove(partID);
						layeredCount.Add(partID, currentCountInLayer - 1);
					}
				}
				// if the layer doesn't exist, we have nothing to remove
			}
			// remove also all the named children if the brick is a group
			// (unless it is a ungroup in that case we leave the children and just remove the one we ungrouped)
			if (!isDueToUngroup)
			{
				Layer.Group group = brickOrGroup as Layer.Group;
				if (group != null)
					foreach (Layer.LayerItem item in group.Items)
						removeBrickNotification(layer, item, isDueToUngroup);
			}
		}

		/// <summary>
		/// Recount all the bricks in the current Map.
		/// </summary>
		public void recountAllBricks()
		{
			// clear the count of all the bricks
			mCount.Clear();
			mCountPerLayer.Clear();

			// iterate on all the brick of all the brick layers,
			foreach (Layer layer in Map.Instance.LayerList)
			{
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
				{
					// add a layered count for this current layer
					Dictionary<string, int> layeredCount = new Dictionary<string, int>();
					mCountPerLayer.Add(brickLayer, layeredCount);

					// now iterate on all the brick of the current layer
					foreach (Layer.LayerItem item in brickLayer.LibraryBrickList)
					{
						// get the part id
						string partID = item.PartNumber;

						// get the current global count
						int currentCount = getCount(partID);
						// update the value
						mCount.Remove(partID);
						mCount.Add(partID, currentCount + 1);

						// also count the brick on the layered count
						int currentCountInLayer = 0;
						layeredCount.TryGetValue(partID, out currentCountInLayer);
						// update the value
						layeredCount.Remove(partID);
						layeredCount.Add(partID, currentCountInLayer + 1);
					}
				}
			}
		}
		#endregion
	}
}
