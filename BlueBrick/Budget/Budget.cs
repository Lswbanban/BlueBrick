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

namespace BlueBrick.Budget
{
	public class Budget
	{
		// instance on the budget (the application only handle one budget for now)
		private static Budget sInstance = new Budget();

		// the budget if the limit set by the user for each brick
		private Dictionary<string, int> mBudget = new Dictionary<string,int>();

		// the count is the actual brick total number in the map
		private Dictionary<string, int> mCount = new Dictionary<string, int>();

		// tell if the budget limitation is enabled
		private bool mIsEnabled = true;

		#region get/set
		/// <summary>
		/// The static instance of the Budget
		/// </summary>
		public static Budget Instance
		{
			get { return sInstance; }
			set { sInstance = value; }
		}
		#endregion

		#region budget management
		/// <summary>
		/// get the budget number associated with the specified part number if any.
		/// If no budget is associated with this part, 0 is returned.
		/// </summary>
		/// <param name="partID">the full part id for which you want to know the budget</param>
		/// <returns>the budget for that part or 0 if there's no budget</returns>
		public int getBudget(string partID)
		{
			// try to get the value or return 0 by default
			int result = 0;
			mBudget.TryGetValue(partID, out result);
			return result;
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
		#endregion

		#region brick count
		/// <summary>
		/// Return a formated string in the form "count/budget" that display the current number of part and it's budget
		/// </summary>
		/// <param name="partID">The full part id for which you want to know the count and budget</param>
		/// <returns>a string displaying the both number separated by a slash</returns>
		public string getCountAndBudgetAsString(string partID)
		{
			return (getCount(partID).ToString() + "/" + getBudget(partID).ToString());
		}

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
		/// and return true, otherwise if the limit is reached return false.
		/// If the budget limitation is not enable, this method always return true.
		/// </summary>
		/// <param name="partID">the full part id</param>
		/// <returns>true if you can add this part</returns>
		public bool canAddBrick(string partID)
		{
			return ((!mIsEnabled) || (getCount(partID) < getBudget(partID)));
		}

		/// <summary>
		/// Call this method when you want to notify the budget counter that a new brick has been added
		/// </summary>
		/// <param name="brick">the brick that was added</param>
		public void addBrickNotification(MapData.LayerBrick.Brick brick)
		{
			string partID = brick.PartNumber;
			if (partID != string.Empty)
			{
				// get the current count
				int currentCount = getCount(brick.PartNumber);
				// and update the value
				mCount.Remove(partID);
				mCount.Add(partID, currentCount + 1);
			}
		}

		/// <summary>
		/// Call this method when you want to notify the budget counter that a brick has been removed
		/// </summary>
		/// <param name="brick">the brick that was removed</param>
		public void removeBrickNotification(MapData.LayerBrick.Brick brick)
		{
			string partID = brick.PartNumber;
			if (partID != string.Empty)
			{
				// get the current count
				int currentCount = getCount(brick.PartNumber);
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
			//TODO
		}
		#endregion
	}
}
