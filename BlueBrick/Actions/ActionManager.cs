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

namespace BlueBrick.Actions
{
	/**
	 * The action manager keep the stack of all the action,
	 * and can perform the undo/redo
	 */
	class ActionManager
	{
		// the two callstack to perform undo/redo
		private List<Action> mUndoStack = new List<Action>(BlueBrick.Properties.Settings.Default.UndoStackDepth);
		private Stack<Action> mRedoStack = new Stack<Action>(BlueBrick.Properties.Settings.Default.UndoStackDepth);

		// singleton on the map (we assume it is always valid)
		private static ActionManager sInstance = new ActionManager();

		/// <summary>
		/// The static instance of the ActionManager
		/// </summary>
		public static ActionManager Instance
		{
			get { return sInstance; }
		}

		public void doAction(Action action)
		{
			// do the action
			action.redo();
			// increase the modification counter on the map
			BlueBrick.MapData.Map.Instance.increaseModificationCounter();
			// limit the size of the undo stack
			// if the stack is full we just discard the older action (first action in the list)
			while (mUndoStack.Count >= BlueBrick.Properties.Settings.Default.UndoStackDepth)
				mUndoStack.RemoveAt(0);
			// now push the action in the stack
			mUndoStack.Add(action);
			// each time we add a action, we clear the redo stack
			mRedoStack.Clear();
			//display the action in the status bar
			MainForm.Instance.setStatusBarMessage(action.getName());
			// update the main form
			MainForm.Instance.updateView(action.UpdateMapView, action.UpdateLayerView);
		}

		public void undo(int actionNum)
		{
			Action.UpdateViewType maxUpdateMapView = Action.UpdateViewType.NONE;
			Action.UpdateViewType maxUpdateLayerView = Action.UpdateViewType.NONE;

			for (int i = 0 ; i < actionNum ; ++i)
				if (mUndoStack.Count > 0)
				{
					// undo the action
					Action action = mUndoStack[mUndoStack.Count-1];
					action.undo();
					// decrease the modification counter on the map
					BlueBrick.MapData.Map.Instance.decreaseModificationCounter();
					// add the action undone in the redo stack
					mRedoStack.Push(action);
					// store the max update type
					if (action.UpdateMapView > maxUpdateMapView)
						maxUpdateMapView = action.UpdateMapView;
					if (action.UpdateLayerView > maxUpdateLayerView)
						maxUpdateLayerView = action.UpdateLayerView;
					// remove the last action from undo stack
					mUndoStack.RemoveAt(mUndoStack.Count - 1);
				}
				else
				{
					break;
				}
			// clear the status bar
			MainForm.Instance.setStatusBarMessage("");
			// update the main form
			MainForm.Instance.updateView(maxUpdateMapView, maxUpdateLayerView);
		}

		public void redo(int actionNum)
		{
			Action.UpdateViewType maxUpdateMapView = Action.UpdateViewType.NONE;
			Action.UpdateViewType maxUpdateLayerView = Action.UpdateViewType.NONE;

			Action action = null;
			for (int i = 0; i < actionNum; ++i)
				if (mRedoStack.Count > 0)
				{
					// redo the action
					action = mRedoStack.Pop();
					action.redo();
					// increase the modification counter on the map
					BlueBrick.MapData.Map.Instance.increaseModificationCounter();
					// and add the action in the undo stack
					mUndoStack.Add(action);
					// store the max update type
					if (action.UpdateMapView > maxUpdateMapView)
						maxUpdateMapView = action.UpdateMapView;
					if (action.UpdateLayerView > maxUpdateLayerView)
						maxUpdateLayerView = action.UpdateLayerView;
				}
				else
				{
					break;
				}
			// display the action name of the last action redone in the status bar
			if (action != null)
				MainForm.Instance.setStatusBarMessage(action.getName());
			// update the main form
			MainForm.Instance.updateView(maxUpdateMapView, maxUpdateLayerView);
		}

		public void clearStacks()
		{
			mUndoStack.Clear();
			mRedoStack.Clear();
			// clear the status bar
			MainForm.Instance.setStatusBarMessage("");
			// update the main form
			MainForm.Instance.updateView(Action.UpdateViewType.FULL, Action.UpdateViewType.FULL);
		}

		public Type getUndoableActionType()
		{
			if (mUndoStack.Count > 0)
				return mUndoStack[mUndoStack.Count - 1].GetType();
			// if there's nothing in the undo stack,
			// return the type of the base class for the action as an empty type
			// because the caller doesn't want to test the null pointer
			return Type.GetType("BlueBrick.Actions.Action");
		}

		public string getUndoableActionName()
		{
			if (mUndoStack.Count > 0)
				return mUndoStack[mUndoStack.Count - 1].getName();
			return null;
		}

		public string getRedoableActionName()
		{
			if (mRedoStack.Count > 0)
				return mRedoStack.Peek().getName();
			return null;
		}

		public string[] getUndoActionNameList(int maxNum)
		{
			// compute the num of action and allocate the array
			int num = mUndoStack.Count;
			if (num > maxNum)
				num = maxNum;
			string[] result = new string[num];
			// the undo stack is reversed (old action first, and new ones at the end)
			for (int i = 0; i < num; ++i)
				result[i] = mUndoStack[mUndoStack.Count - 1 - i].getName();
			return result;
		}

		public string[] getRedoActionNameList(int maxNum)
		{
			Action[] actions = mRedoStack.ToArray();
			// compute the num of action and allocate the array
			int num = actions.Length;
			if (num > maxNum)
				num = maxNum;
			string[] result = new string[num];
			// the redo stack is in correct order
			for (int i = 0; i < num; ++i)
				result[i] = actions[i].getName();
			return result;
		}
	}
}
