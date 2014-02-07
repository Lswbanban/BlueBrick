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

		// the budget if the limit set by the user for each brick
		private Dictionary<string, int> mBudget = new Dictionary<string,int>();

		// the count is the actual brick total number in the map
		private Dictionary<string, int> mCount = new Dictionary<string, int>();

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

		public bool IsOpened
		{
			get { return true; /*TODO to implement*/ }
		}

		public bool WasModified
		{
			get { return true; /*TODO to implement*/ }
			set
			{
				// if the value is false (meaning we just saved the file), reset all the flags
				if (!value)
				{
					// TOOD to implement
				}
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
			// version
			reader.ReadToDescendant("Version");
			mDataVersionOfTheFileLoaded = reader.ReadElementContentAsInt();
		}

		public virtual void WriteXml(System.Xml.XmlWriter writer)
		{
			// first of all the version, we don't use the vesion read from the file,
			// for saving we always save with the last version of data
			writer.WriteElementString("Version", CURRENT_DATA_VERSION.ToString());

			// now write the budget list
			writer.WriteStartElement("Budgets");
			foreach (KeyValuePair<string, int> budget in mBudget)
			{
				writer.WriteStartElement("Budget");
				writer.WriteAttributeString("part", budget.Key);
				writer.WriteString(budget.Value.ToString());
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}
		#endregion

		#region budget management
		/// <summary>
		/// Tell if the specified part has a budget defined
		/// </summary>
		/// <param name="partID">the part id for which you want to know if it has a budget</param>
		/// <returns>true if the budget is defined (not infinite)</returns>
		public bool IsBudgeted(string partID)
		{
			return (getBudget(partID) >= 0);
		}

		/// <summary>
		/// Set a budget number for the specified part id.
		/// </summary>
		/// <param name="partID">the full part id for which you want to set the budget</param>
		/// <param name="budget">the budget number (can be negative or null)</param>
		public void setBudget(string partID, int budget)
		{
			// remove the previous value before seting the new one
			mBudget.Remove(partID);
			mBudget.Add(partID, budget);
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
		/// Return a formated string of the budget that display the current budget if set or the infinity sign if not set
		/// </summary>
		/// <param name="partID">The full part id for which you want to know the budget</param>
		/// <returns>a string displaying the number of the infinity sign</returns>
		public string getBudgetAsString(string partID)
		{
			string budgetString = "?"; // "∞"; // by default if the budget is not set, it is infinite
			int budget = getBudget(partID);
			if (budget >= 0)
				budgetString = budget.ToString();
			// return the formated string
			return budgetString;
		}		

		/// <summary>
		/// Return a formated string in the form "count/budget" that display the current number of part and it's budget
		/// </summary>
		/// <param name="partID">The full part id for which you want to know the count and budget</param>
		/// <returns>a string displaying the both number separated by a slash</returns>
		public string getCountAndBudgetAsString(string partID)
		{
			return (getCount(partID).ToString() + "/" + getBudgetAsString(partID));
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
		/// If the budget limitation is enabled, this method will check if the brick count is less than the budget
		/// and return true, otherwise if the limit is reached return false. If the budget is not set for that part,
		/// it will also return true.
		/// If the budget limitation is not enable, this method always return true.
		/// </summary>
		/// <param name="partID">the full part id</param>
		/// <returns>true if you can add this part</returns>
		public bool canAddBrick(string partID)
		{
			return canAddBrick(partID, 1);
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
			if (Properties.Settings.Default.UseBudgetLimitation)
			{
				int budget = getBudget(partID);
				return ((budget < 0) || (getCount(partID) + quantity <= budget));
			}
			return true;
		}

		/// <summary>
		/// Call this method when you want to notify the budget counter that a new brick has been added
		/// </summary>
		/// <param name="brick">the brick that was added</param>
		public void addBrickNotification(MapData.Layer.LayerItem brickOrGroup)
		{
			string partID = brickOrGroup.PartNumber;
			if (partID != string.Empty)
			{
				// get the current count
				int currentCount = getCount(partID);
				// and update the value
				mCount.Remove(partID);
				mCount.Add(partID, currentCount + 1);
			}
		}

		/// <summary>
		/// Call this method when you want to notify the budget counter that a brick has been removed
		/// </summary>
		/// <param name="brick">the brick that was removed</param>
		public void removeBrickNotification(MapData.Layer.LayerItem brickOrGroup)
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
			}
		}

		/// <summary>
		/// Recount all the bricks in the current Map.
		/// </summary>
		public void recountAllBricks()
		{
			// clear the count of all the bricks
			mCount.Clear();

			// iterate on all the brick of all the brick layers,
			foreach (Layer layer in Map.Instance.LayerList)
			{
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					foreach (Layer.LayerItem item in brickLayer.LibraryBrickList)
					{
						string partID = item.PartNumber;
						// get the current count
						int currentCount = getCount(partID);
						// update the value
						mCount.Remove(partID);
						mCount.Add(partID, currentCount + 1);
					}
			}
		}
		#endregion
	}
}
