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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using BlueBrick.Actions;
using BlueBrick.Actions.Texts;
using BlueBrick.Actions.Bricks;
using BlueBrick.MapData;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using BlueBrick.Actions.Maps;

namespace BlueBrick
{
	public partial class MainForm : Form
    {
		#region variable
		// this static flag is used by the splash screen thread to know when to stop
		private static bool sIsMainFormReady = false;

		// reference on the main form (set in the constructor)
		private static MainForm sInstance = null;
		
		// a flag mostly never used, only when the application wants to restart, to prevent the user to
		// be able to cancel the close of the application and then finally end up with two instance of the application
		private bool mCanUserCancelTheApplicationClose = true;

		// custom cursors for the application
		private Cursor mBrickArrowCursor = null;
		private Cursor mBrickDuplicateCursor = null;
		private Cursor mBrickSelectionCursor = null;
		private Cursor mTextArrowCursor = null;
		private Cursor mTextDuplicateCursor = null;
		private Cursor mTextSelectionCursor = null;
		private Cursor mAreaPaintCursor = null;
		private Cursor mAreaEraserCursor = null;
		
		// for shortcut key
		// var for updating the move
		private PointF mObjectTotalMove = new PointF(0, 0);
		private bool mIsLeftArrowDown = false;
		private bool mIsRightArrowDown = false;
		private bool mIsUpArrowDown = false;
		private bool mIsDownArrowDown = false;
		// var for updating the rotation
		private int mObjectTotalStepToRotate = 0;
		private bool mIsRotateLeftDown = false;
		private bool mIsRotateRightDown = false;
		// flags for the key events
		private Keys mLastModifierKeyDown = Keys.None;
		private bool mModifierWasPressedIgnoreCustomShortcut = false;

		// painting tool
		private Bitmap mPaintIcon = null; // the paint icon contains the color of the paint in the background
		private Color mCurrentPaintIconColor = Color.Empty;

		// for the current map
		private string mCurrentMapFileName = Properties.Resources.DefaultSaveFileName;
		private bool mIsCurrentMapNameValid = false;

		// the part list view
		private PartListForm mPartListForm = null;

		// for the selection Path
		AStar mAStar = new AStar();

		// a mapping key table to store the shortcut for each action
		enum shortcutableAction
		{
			ADD_PART = 0,
			DELETE_PART,
			ROTATE_LEFT,
			ROTATE_RIGHT,
			MOVE_LEFT,
			MOVE_RIGHT,
			MOVE_UP,
			MOVE_DOWN,
			CHANGE_CURRENT_CONNEXION,
			NB_ACTIONS
		};
		private class KeyAndPart
		{
			public KeyAndPart(Keys keyCode, string partName, int connexion)
			{
				mKeyCode = keyCode;
				mPartName = partName;
				mConnexion = connexion;
			}
			public Keys mKeyCode = Keys.None;
			public string mPartName = String.Empty;
			public int mConnexion = 0;
		}
		private List<KeyAndPart>[] mShortcutKeys = null;

		/// <summary>
		/// this static flag is used by the splash screen thread to know when to stop
		/// </summary>
		public static bool IsMainFormReady
		{
			get { return sIsMainFormReady; }
		}

		/// <summary>
		/// Get the reference pointer on the main form window
		/// </summary>
		public static MainForm Instance
		{
			get { return sInstance; }
		}

		/// <summary>
		/// Get the cursor for duplication of layer bricks
		/// </summary>
		public Cursor BrickArrowCursor
		{
			get { return mBrickArrowCursor; }
		}

		/// <summary>
		/// Get the cursor for duplication of layer bricks
		/// </summary>
		public Cursor BrickDuplicateCursor
		{
			get { return mBrickDuplicateCursor; }
		}

		/// <summary>
		/// Get the cursor for selection of layer bricks
		/// </summary>
		public Cursor BrickSelectionCursor
		{
			get { return mBrickSelectionCursor; }
		}

		/// <summary>
		/// Get the cursor for duplication of layer texts
		/// </summary>
		public Cursor TextArrowCursor
		{
			get { return mTextArrowCursor; }
		}

		/// <summary>
		/// Get the cursor for duplication of layer texts
		/// </summary>
		public Cursor TextDuplicateCursor
		{
			get { return mTextDuplicateCursor; }
		}

		/// <summary>
		/// Get the cursor for selection of layer texts
		/// </summary>
		public Cursor TextSelectionCursor
		{
			get { return mTextSelectionCursor; }
		}

		/// <summary>
		/// Get the cursor for painting the area layer
		/// </summary>
		public Cursor AreaPaintCursor
		{
			get { return mAreaPaintCursor; }
		}

		/// <summary>
		/// Get the cursor for erasing the area layer
		/// </summary>
		public Cursor AreaEraserCursor
		{
			get { return mAreaEraserCursor; }
		}
		#endregion

        #region Initialisation of the application

		public MainForm(string fileToOpen)
		{
			InitializeComponent();
			sInstance = this;
			// load the custom cursors
			LoadEmbededCustomCursors();
			// regenerate the paint icon with the right color in the background
			mCurrentPaintIconColor = this.colorDialog.Color;
			generatePaintIcon();
			// init the drawing data for the brick layer
			LayerBrick.sUpdateGammaFromSettings();
			// reset the shortcut keys
			initShortcutKeyArrayFromSettings();
			// PATCH FIX BECAUSE DOT NET FRAMEWORK IS BUGGED
			SaveDefaultKeyInDefaultSettings();
			// hide the part list form
			mPartListForm = new PartListForm(this);
			mPartListForm.Visible = false;
			// load the part info
			loadPartLibraryFromDisk();
			// check if we need to open a file or create a new map
			if ((fileToOpen != null) && canOpenThisFile(fileToOpen))
			{
				openMap(fileToOpen);
			}
			else
			{
				createNewMap();
				// we update the list in the else because it is already updated in the openMap()
				UpdateRecentFileMenuFromConfigFile();
			}
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			// set the static flag to terminate the thread of the splash screen
			sIsMainFormReady = true;
		}

		private void SaveDefaultKeyInDefaultSettings()
		{
			// PATCH FIX BECAUSE DOT NET FRAMEWORK IS BUGGED
			// Indeed the "Ctrl" string doesn't not exist on german OS (the key name is "Strg"),
			// so the default setting fail to load if you put a CTRL as default key in the default
			// Setting. So instead we save the default key here if the default key is not saved
			// and then after the application will be able to reload correctly the settings
			bool needToSave = false;

			if (BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey == Keys.None)
			{
				BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey = Keys.Control;
				needToSave = true;
			}
			if (BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey == Keys.None)
			{
				BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey = Keys.Alt;
				needToSave = true;
			}
			if (BlueBrick.Properties.Settings.Default.MouseZoomKey == Keys.None)
			{
				BlueBrick.Properties.Settings.Default.MouseZoomKey = Keys.Control | Keys.Shift;
				needToSave = true;
			}
			if (BlueBrick.Properties.Settings.Default.MousePanKey == Keys.None)
			{
				BlueBrick.Properties.Settings.Default.MousePanKey = Keys.Alt | Keys.Shift;
				needToSave = true;
			}

			// try to save (never mind if we can not (for example BlueBrick is launched
			// from a write protected drive)
			try
			{
				if (needToSave)
					BlueBrick.Properties.Settings.Default.Save();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Load and create all the embeded cursors creates specially for this application
		/// </summary>
		private void LoadEmbededCustomCursors()
		{
			// get the assembly
			System.Reflection.Assembly assembly = this.GetType().Assembly;
			// brick arrow cursor
			System.IO.Stream stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.BrickArrowCursor.cur");
			mBrickArrowCursor = new Cursor(stream);
			stream.Close();
			// brick duplicate cursor
			stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.BrickDuplicateCursor.cur");
			mBrickDuplicateCursor = new Cursor(stream);
			stream.Close();
			// brick duplicate cursor
			stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.BrickSelectionCursor.cur");
			mBrickSelectionCursor = new Cursor(stream);
			stream.Close();
			// text arrow cursor
			stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.TextArrowCursor.cur");
			mTextArrowCursor = new Cursor(stream);
			stream.Close();
			// text duplicate cursor
			stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.TextDuplicateCursor.cur");
			mTextDuplicateCursor = new Cursor(stream);
			stream.Close();
			// text selection cursor
			stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.TextSelectionCursor.cur");
			mTextSelectionCursor = new Cursor(stream);
			stream.Close();
			// area paint cursor
			stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.AreaPaintCursor.cur");
			mAreaPaintCursor = new Cursor(stream);
			stream.Close();
			// area erase cursor
			stream = assembly.GetManifestResourceStream("BlueBrick.Cursor.AreaEraserCursor.cur");
			mAreaEraserCursor = new Cursor(stream);
			stream.Close();
		}

		/// <summary>
		/// Init and load the part library for both
		/// the part library database and the part library panel
		/// The part lib is supposed to have been cleared before, but if not we clear it anyway
		/// </summary>
		private void loadPartLibraryFromDisk()
		{
			// first clear the database for precaution
			BrickLibrary.Instance.clearAllData();
			// reinit the parts tab control (that will fill the brick library again)
			this.partsTabControl.initPartsTabControl();
			// and relod the other data for the brick libray (after the brick library is loaded)
			BrickLibrary.Instance.loadColorInfo();
			BrickLibrary.Instance.createEntriesForRenamedParts();
			BrickLibrary.Instance.loadTrackDesignerRegistryFileList();
		}

		/// <summary>
		/// Tell if the specified file name has a valid extension that BlueBrick can open.
		/// </summary>
		/// <param name="filename">the filename to check</param>
		/// <returns>true if the extension of the file is well known</returns>
		private bool canOpenThisFile(string filename)
		{
			// get the last dot
			int lastDotIndex = filename.LastIndexOf('.');
			string fileExtension = filename.Substring(lastDotIndex + 1);
			fileExtension = fileExtension.ToLower();

			// authorize the drop if it's a file with the good extension
			return (fileExtension.Equals("bbm") || fileExtension.Equals("ldr") ||
					fileExtension.Equals("mpd") || fileExtension.Equals("tdl") || fileExtension.Equals("dat"));
		}

		#endregion

		#region update function

		/// <summary>
		/// update all the view of the application
		/// </summary>
		public void updateView(Action.UpdateViewType mapUpdateType, Action.UpdateViewType layerUpdateType)
		{
			// update the map
			if (mapUpdateType > Action.UpdateViewType.NONE)
				this.mapPanel.updateView();

			// update the layer
			if (layerUpdateType > Action.UpdateViewType.NONE)
				this.layerStackPanel.updateView(layerUpdateType);

			// check if we need to change the "*" on the title bar
			if (this.Text.EndsWith(" *"))
			{
				if (!Map.Instance.WasModified)
					this.Text = this.Text.Remove(this.Text.Length - 2);
			}
			else
			{
				if (Map.Instance.WasModified)
					this.Text += " *"; 
			}

			// update the undo/redo stack
			updateUndoRedoMenuItems();
		}

		/// <summary>
		/// Enable or disable the copy/cut/delete menu item and copy/cut/delete toolbar button
		/// </summary>
		/// <param name="enable"></param>
		public void enableCopyButton(bool enable)
		{
			this.copyToolStripMenuItem.Enabled = enable;
			this.cutToolStripMenuItem.Enabled = enable;
			this.deleteToolStripMenuItem.Enabled = enable;
			this.toolBarCopyButton.Enabled = enable;
			this.toolBarCutButton.Enabled = enable;
			this.toolBarDeleteButton.Enabled = enable;
			this.deselectAllToolStripMenuItem.Enabled = enable;
			this.selectPathToolStripMenuItem.Enabled = ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.GetType().Name.Equals("LayerBrick")) && (Map.Instance.SelectedLayer.SelectedObjects.Count == 2));
		}

		/// <summary>
		/// Enable or disable the buttons in the tool bar that allow manipulation of the layer item
		/// such as snap grid, snap rotation, rotate button and paint/erase
		/// </summary>
		/// <param name="enableMoveRotateButton">enable the button related to move and rotate</param>
		/// <param name="enablePaintButton">enable the button related to paint</param>
		public void enableToolbarButtonOnLayerSelection(bool enableMoveRotateButton, bool enablePaintButton)
		{
			// enable the copy/paste button
			if (enableMoveRotateButton)
			{
				this.pasteToolStripMenuItem.Enabled = true;
				this.toolBarPasteButton.Enabled = true;
				// do not enable copy button now, it will be enable when the selection is not null
			}
			else
			{
				// this is a layer without selectionnable items, disable all copy/paste button
				this.pasteToolStripMenuItem.Enabled = false;
				this.toolBarPasteButton.Enabled = false;
				enableCopyButton(false);
			}

			// enable/disable the move rotate buttons
			// toolbar
			this.toolBarSnapGridButton.Enabled = enableMoveRotateButton;
			this.toolBarRotationAngleButton.Enabled = enableMoveRotateButton;
			this.toolBarRotateCCWButton.Enabled = enableMoveRotateButton;
			this.toolBarRotateCWButton.Enabled = enableMoveRotateButton;
			this.toolBarBringToFrontButton.Enabled = enableMoveRotateButton;
			this.toolBarSendToBackButton.Enabled = enableMoveRotateButton;
			//menu
			this.moveStepToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotationStepToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotateCCWToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotateCWToolStripMenuItem.Enabled = enableMoveRotateButton;

			// enable/disable the paint button (toolbar and menu)
			this.toolBarPaintButton.Enabled = enablePaintButton;
			this.paintToolToolStripMenuItem.Enabled = enablePaintButton;
		}

		/// <summary>
		/// This private method is only to mutualize code between the menu and the toolbar,
		/// to enable/disable the snap grid button
		/// </summary>
		/// <param name="enable">true is the snap grid is enabled</param>
		private void enableSnapGridButton(bool enable)
		{
			// uncheck all the menu item anyway
			this.moveStep32ToolStripMenuItem.Checked = false;
			this.moveStep16ToolStripMenuItem.Checked = false;
			this.moveStep8ToolStripMenuItem.Checked = false;
			this.moveStep4ToolStripMenuItem.Checked = false;
			this.moveStep1ToolStripMenuItem.Checked = false;
			this.moveStep05ToolStripMenuItem.Checked = false;
			// enable or disable the correct items
			if (enable)
			{
				// menu
				this.moveStepDisabledToolStripMenuItem.Checked = false;
				if (this.toolBarGrid32Button.Checked)
					this.moveStep32ToolStripMenuItem.Checked = true;
				else if (this.toolBarGrid16Button.Checked)
					this.moveStep16ToolStripMenuItem.Checked = true;
				else if (this.toolBarGrid8Button.Checked)
					this.moveStep8ToolStripMenuItem.Checked = true;
				else if (this.toolBarGrid4Button.Checked)
					this.moveStep4ToolStripMenuItem.Checked = true;
				else if (this.toolBarGrid1Button.Checked)
					this.moveStep1ToolStripMenuItem.Checked = true;
				else if (this.toolBarGrid05Button.Checked)
					this.moveStep05ToolStripMenuItem.Checked = true;
				// toolbar
				this.toolBarSnapGridButton.DropDown.Enabled = true;
				this.toolBarSnapGridButton.Image = BlueBrick.Properties.Resources.SnapGridOn;
				Layer.SnapGridEnabled = true;
			}
			else
			{
				// menu
				this.moveStepDisabledToolStripMenuItem.Checked = true;
				// toolbar
				this.toolBarSnapGridButton.DropDown.Enabled = false;
				this.toolBarSnapGridButton.Image = BlueBrick.Properties.Resources.SnapGridOff;
				Layer.SnapGridEnabled = false;
			}
		}

		public void NotifyPartListForLayerAdded(Layer layer)
		{
			mPartListForm.addLayerNotification(layer as LayerBrick);
		}

		public void NotifyPartListForLayerRemoved(Layer layer)
		{
			mPartListForm.removeLayerNotification(layer as LayerBrick);
		}

		public void NotifyPartListForLayerRenamed(Layer layer)
		{
			mPartListForm.renameLayerNotification(layer as LayerBrick);
		}

		public void NotifyPartListForBrickAdded(LayerBrick layer, LayerBrick.Brick brick)
		{
			mPartListForm.addBrickNotification(layer, brick);
		}

		public void NotifyPartListForBrickRemoved(LayerBrick layer, LayerBrick.Brick brick)
		{
			mPartListForm.removeBrickNotification(layer, brick);
		}
		#endregion

		#region status bar

		public void setStatusBarMessage(string message)
		{
			this.statusBarLabel.Text = message;
		}

		public void resetProgressBar(int maxValue)
		{
			if (maxValue > 0)
			{
				this.statusBarProgressBar.Step = 1;
				this.statusBarProgressBar.Minimum = 0;
				this.statusBarProgressBar.Maximum = maxValue;
				this.statusBarProgressBar.Value = 0;
				this.statusBarProgressBar.Visible = true;
			}
		}

		public void stepProgressBar()
		{
			// perform the step
			this.statusBarProgressBar.PerformStep();
			// hide automatically the progress bar when the end is reached
			if (this.statusBarProgressBar.Value >= this.statusBarProgressBar.Maximum)
				this.statusBarProgressBar.Visible = false;
		}

		public void stepProgressBar(int stepValue)
		{
			if (stepValue > 0)
			{
				this.statusBarProgressBar.Step = stepValue;
				stepProgressBar();
			}
		}

		public void finishProgressBar()
		{
			if (this.statusBarProgressBar.Value < this.statusBarProgressBar.Maximum)
			{
				this.statusBarProgressBar.Step = this.statusBarProgressBar.Maximum - this.statusBarProgressBar.Value;
				stepProgressBar();
			}
		}
		#endregion

        #region event handler for menu bar

		#region File menu
		/// <summary>
		/// This method check if the current map is not save and prompt a message box asking
		/// what to do (save, do not save or cancel). This method sould be called before
		/// exiting the application, and before creating a new map
		/// <returns>true if the action can continue, false if user choose to cancel</returns>
		/// </summary>
		private bool checkForUnsavedMap()
		{
			if (Map.Instance.WasModified)
			{
				// if the user can cancel the application close, give him 3 buttons yes/no/cancel,
				// else give him only 2 buttons yes/no:
				DialogResult result = MessageBox.Show(this,
					BlueBrick.Properties.Resources.ErrorMsgMapWasModified,
					BlueBrick.Properties.Resources.ErrorMsgTitleWarning,
					mCanUserCancelTheApplicationClose ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

				if (result == DialogResult.Yes)
				{
					// call the save method (that maybe will perform a save as)
					saveToolStripMenuItem_Click(null, null);
				}
				else if (result == DialogResult.Cancel)
				{
					// user cancel so return false
					return false;
				}
			}
			// the map was not modified (or user choose "yes" or "no"), the action can continue
			return true;
		}

		private void createNewMap()
		{
			// create a new map
			Map.Instance = new Map();
			Layer.resetNameInstanceCounter();
			ActionManager.Instance.clearStacks();
			// reset the current file name
			changeCurrentMapFileName(Properties.Resources.DefaultSaveFileName, false);
			// check if we need to add layer
			if (Properties.Settings.Default.AddGridLayerOnNewMap)
				ActionManager.Instance.doAction(new AddLayer("LayerGrid"));
			if (Properties.Settings.Default.AddBrickLayerOnNewMap)
				ActionManager.Instance.doAction(new AddLayer("LayerBrick"));
			// after adding the two default layer, we reset the WasModified flag of the map
			// (and before the update of the title bar)
			Map.Instance.WasModified = false;
			// update the view any way
			this.updateView(Action.UpdateViewType.FULL, Action.UpdateViewType.FULL);
			mPartListForm.rebuildList();
			// force a garbage collect because we just trashed the previous map
			GC.Collect();
		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// check if the current map is not save and display a warning message
			if (checkForUnsavedMap())
				createNewMap();
		}

		private void changeCurrentMapFileName(string filename, bool isAValidName)
		{
			// save the filename
			mCurrentMapFileName = filename;
			mIsCurrentMapNameValid = isAValidName;
			// update the name (not the full path) of the title bar with the name of the file
			FileInfo fileInfo = new FileInfo(filename);
			this.Text = "BlueBrick - " + fileInfo.Name;
		}

		private void openMap(string filename)
		{
			// set the wait cursor
			this.Cursor = Cursors.WaitCursor;
			// reset the action stacks and layer counter
			Layer.resetNameInstanceCounter();
			ActionManager.Instance.clearStacks();
			BrickLibrary.Instance.WereUnknownBricksAdded = false;
			// load the file
			bool isFileValid = SaveLoadManager.load(filename);
			if (isFileValid)
			{
				// move the view to center of the map
				this.mapPanel.moveViewToMapCenter();
				// update the view
				this.updateView(Action.UpdateViewType.FULL, Action.UpdateViewType.FULL);
				mPartListForm.rebuildList();
				//check if some parts were missing in the library for displaying a warning message
				if (BrickLibrary.Instance.WereUnknownBricksAdded)
				{
					// restore the cursor before displaying the error message box
					this.Cursor = Cursors.Default;
					string message = BlueBrick.Properties.Resources.ErrorMsgMissingParts.Replace("&", Map.Instance.Author);
					MessageBox.Show(this, message,
						BlueBrick.Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
						MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
				}
			}
			// restore the cursor after loading
			this.Cursor = Cursors.Default;
			// save the current file name of the loaded map
			if (isFileValid)
				changeCurrentMapFileName(filename, true);
			else
				changeCurrentMapFileName(Properties.Resources.DefaultSaveFileName, false);
			// update the recent file list
			UpdateRecentFileMenuFromConfigFile(filename, isFileValid);
			// force a garbage collect because we just trashed the previous map
			GC.Collect();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// check if the current map is not save and display a warning message
			if (checkForUnsavedMap())
			{
				DialogResult result = this.openFileDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					openMap(this.openFileDialog.FileName);
				}
			}
		}

		private void OpenRecentFileMenuItem_Click(object sender, EventArgs e)
		{
			// check if the current map is not save and display a warning message
			if (checkForUnsavedMap())
			{
				ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
				string fileName = menuItem.Tag as string;
				openMap(fileName);
			}
		}

		private void UpdateRecentFileMenuFromConfigFile(string fileName, bool isAdded)
		{
			// first update the settings
			// remove the file from this list since it will be re-added (or not) on top of the list
			BlueBrick.Properties.Settings.Default.RecentFiles.Remove(fileName);
			// add the filename on top of the list (according to the parameter)
			if (isAdded)
				BlueBrick.Properties.Settings.Default.RecentFiles.Insert(0, fileName);
			// if the maximum files is reached, we delete the last one (the old one)
			if (BlueBrick.Properties.Settings.Default.RecentFiles.Count > 20)
				BlueBrick.Properties.Settings.Default.RecentFiles.RemoveAt(20);
			// save the change
			BlueBrick.Properties.Settings.Default.Save();
			// then update the menu item list
			UpdateRecentFileMenuFromConfigFile();
		}

		private void UpdateRecentFileMenuFromConfigFile()
		{
			// clear the list of recent files
			openRecentToolStripMenuItem.DropDownItems.Clear();
			// if the list is not empty, populate the menu
			foreach (string fileName in BlueBrick.Properties.Settings.Default.RecentFiles)
			{
				// check if the filename is valid
				if (fileName.Length > 0)
				{
					ToolStripMenuItem item = new ToolStripMenuItem();
					item.Tag = fileName;
					item.Text = System.IO.Path.GetFileName(fileName);
					item.ToolTipText = fileName; // to display the full path as tooltip
					item.Click += new EventHandler(OpenRecentFileMenuItem_Click);
					openRecentToolStripMenuItem.DropDownItems.Add(item);
				}
				// stop adding files if we reach the maximum number to display
				if (openRecentToolStripMenuItem.DropDownItems.Count >= BlueBrick.Properties.Settings.Default.MaxRecentFilesNum)
					break;
			}
			// enable the Open Recent menu item if we have some recent files
			openRecentToolStripMenuItem.Enabled = (openRecentToolStripMenuItem.DropDownItems.Count > 0);
		}

		private void saveMap()
		{
			// set the wait cursor
			this.Cursor = Cursors.WaitCursor;
			// save the file
			SaveLoadManager.save(mCurrentMapFileName);
			// after saving the map in any kind of format, we reset the WasModified flag of the map
			// (and before the update of the title bar)
			Map.Instance.WasModified = false;
			// call the update view with NONE, because we just need to update the title bar
			this.updateView(Action.UpdateViewType.NONE, Action.UpdateViewType.NONE);
			// restore the cursor
			this.Cursor = Cursors.Default;
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// if the current file name is not defined we do a "save as..."
			if (mIsCurrentMapNameValid)
				saveMap();
			else
				saveasToolStripMenuItem_Click(sender, e);
		}

		private void saveasToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// put the current file name in the dialog (which can be the default one)
			// but remove the extension, such as the user can easily change the extension in
			// the save dialog drop list, and the save dialog will add it automatically
			this.saveFileDialog.FileName = Path.GetFileNameWithoutExtension(mCurrentMapFileName);
			// open the save as dialog
			DialogResult result = this.saveFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				// for a "Save As..." only (not for a save), we check if the user choose a LDRAW or TDL format
				// to display a warning message, that he will lost data
				string filenameLower = this.saveFileDialog.FileName.ToLower();
				if (!filenameLower.EndsWith("bbm"))
				{
					// display the warning message
					result = MessageBox.Show(this, Properties.Resources.ErrorMsgNotSavingInBBM,
									Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
									MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

					// if the user doesn't want to continue, do not save and
					// do not add the name in the recent list file
					if (result == DialogResult.No)
						return;
				}

				// change the current file name before calling the save
				changeCurrentMapFileName(this.saveFileDialog.FileName, true);
				// save the map
				saveMap();
				// update the recent file list with the new file saved
				UpdateRecentFileMenuFromConfigFile(this.saveFileDialog.FileName, true);
			}
		}

		private void exportAsPictureToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// change the flag that display the connexion points
			bool saveDrawFreeConnexionPointFlag = BlueBrick.Properties.Settings.Default.DisplayFreeConnexionPoints;
			BlueBrick.Properties.Settings.Default.DisplayFreeConnexionPoints = false;
			// open the export form
			ExportImageForm exportImageForm = new ExportImageForm();
			DialogResult result = exportImageForm.ShowDialog();
			if (result == DialogResult.OK)
			{
				// option were set, now open the save dialog
				result = this.saveExportImageDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					// create the Bitmap and get the graphic context from it
					Bitmap image = new Bitmap(exportImageForm.ImageWidth, exportImageForm.ImageHeight, PixelFormat.Format24bppRgb);
					Graphics graphics = Graphics.FromImage(image);
					graphics.Clear(Map.Instance.BackgroundColor);
					graphics.SmoothingMode = SmoothingMode.Default; // the HighQuality let appears some grid line above the area cells
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.CompositingMode = CompositingMode.SourceOver;
					graphics.InterpolationMode = InterpolationMode.High; // this one need to be high else there's some rendering bug appearing with a lower mode, the scale of the stud looks not correct when zooming out.
					// draw the bitmap
					Map.Instance.draw(graphics, exportImageForm.AreaInStud, exportImageForm.ScalePixelPerStud);
					// find the correct format according to the last extension.
					// Normally the FileName MUST have a valid extension because the SaveFileDialog
					// automatically add an extension if the user forgot to precise one, or if
					// the use put an extension that is not in the list of the filters.
					string fileName = saveExportImageDialog.FileName;
					int lastExtensionIndex = fileName.LastIndexOf('.');
					// declare a variable that receive the choosen format
					ImageFormat choosenFormat = ImageFormat.Bmp;
					if (lastExtensionIndex != -1)
					{
						string extension = fileName.Substring(lastExtensionIndex + 1).ToLower();
						if (extension.Equals("bmp"))
							choosenFormat = ImageFormat.Bmp;
						else if (extension.Equals("gif"))
							choosenFormat = ImageFormat.Gif;
						else if (extension.Equals("jpg"))
							choosenFormat = ImageFormat.Jpeg;
						else if (extension.Equals("png"))
							choosenFormat = ImageFormat.Png;
						else
						{
							// the extension is not a valid extension (like "txt" for example)
							// so we choose the format according to the filter index
							// and add the correct extension
							switch (saveExportImageDialog.FilterIndex)
							{
								case 1: choosenFormat = ImageFormat.Bmp; fileName += ".bmp"; break;
								case 2: choosenFormat = ImageFormat.Gif; fileName += ".gif"; break;
								case 3: choosenFormat = ImageFormat.Jpeg; fileName += ".jpg"; break;
								case 4: choosenFormat = ImageFormat.Png; fileName += ".png"; break;
								default: choosenFormat = ImageFormat.Bmp; fileName += ".bmp"; break;
							}
						}
					}
					// save the bitmap in a file
					image.Save(fileName, choosenFormat);
				}
			}
			// reset the flag with the previous value
			BlueBrick.Properties.Settings.Default.DisplayFreeConnexionPoints = saveDrawFreeConnexionPointFlag;
		}

		private void reloadPartLibraryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// we have first to undload the current file
			// check if the current map is not save and display a warning message
			if (checkForUnsavedMap())
			{
				// save the name of the current map open to reload it (if it is valid)
				string previousOpenMapFileName = null;
				if (mIsCurrentMapNameValid)
					previousOpenMapFileName = mCurrentMapFileName;

				// then clear the part lib panel and the brick library (before creating the new map)
				this.partsTabControl.clearAllData();
				BrickLibrary.Instance.clearAllData();

				// create a new map (in order to unload the current one)
				createNewMap();

				// call the GC to be sure that all the image are correctly released, and no files stay locked
				// even if the GC was normally already called in the create new map function
				GC.Collect();

				// then display a waiting message box, giving the user the oppotunity to change the
				// data before reloading
				DialogResult result = MessageBox.Show(this,
					BlueBrick.Properties.Resources.ErrorMsgReadyToReloadPartLib,
					BlueBrick.Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				// then reload the library
				this.Cursor = Cursors.WaitCursor;
				loadPartLibraryFromDisk();
				this.Cursor = Cursors.Default;

				// finally reload the previous map
				if (previousOpenMapFileName != null)
				{
					openMap(previousOpenMapFileName);
					// since the connexion position may have changed after the reload of the library,
					// maybe so 2 free connexions will become aligned, and can be connected, that's why
					// we perform a slow update connectivity in that case.
					foreach (Layer layer in Map.Instance.LayerList)
						if (layer.GetType().Name == "LayerBrick")
							(layer as LayerBrick).updateFullBrickConnectivity();
				}
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// the check if the current map is not save is done in the Closing Event
			// in order to catch all the path to close the application
			// (such as Alt+F4, the cross button in the title bar, the Close item
			// in the menu of the title bar, etc...)
			this.Close();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// check if the current map is not save and display a warning message
			if (!checkForUnsavedMap())
			{
				// if the player cancel the closing then cancel the event
				e.Cancel = true;
			}
		}

		#endregion

		#region Edit Menu

		private void updateUndoRedoMenuItems()
		{
			// reinit undo/redo menu with the correct language
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			resources.ApplyResources(this.undoToolStripMenuItem, "undoToolStripMenuItem");
			resources.ApplyResources(this.redoToolStripMenuItem, "redoToolStripMenuItem");

			// check if the undo stack is empty or not
			string actionName = ActionManager.Instance.getUndoableActionName();
			this.undoToolStripMenuItem.Enabled = (actionName != null);
			if (actionName != null)
				this.undoToolStripMenuItem.Text += " \"" + actionName + "\"";
			
			// check if the redo stack is empty or not
			actionName = ActionManager.Instance.getRedoableActionName();
			this.redoToolStripMenuItem.Enabled = (actionName != null);
			if (actionName != null)
				this.redoToolStripMenuItem.Text += " \"" + actionName + "\"";

			// the undo toolbar button
			this.toolBarUndoButton.Enabled = this.undoToolStripMenuItem.Enabled;
			this.toolBarUndoButton.DropDownItems.Clear();
			string[] actionNameList = ActionManager.Instance.getUndoActionNameList(BlueBrick.Properties.Settings.Default.UndoStackDisplayedDepth);
			foreach (string action in actionNameList)
				this.toolBarUndoButton.DropDownItems.Add(action);

			// the redo toolbar button
			this.toolBarRedoButton.Enabled = this.redoToolStripMenuItem.Enabled;
			this.toolBarRedoButton.DropDownItems.Clear();
			actionNameList = ActionManager.Instance.getRedoActionNameList(BlueBrick.Properties.Settings.Default.UndoStackDisplayedDepth);
			foreach (string action in actionNameList)
				this.toolBarRedoButton.DropDownItems.Add(action);
		}

		private void undoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.undo(1);
		}

		private void redoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.redo(1);
		}

		private void cutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// a cut is a copy followed by a delete
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				switch (selectedLayer.GetType().Name)
				{
					case "LayerText":
						(selectedLayer as LayerText).copyCurrentSelection();
						ActionManager.Instance.doAction(new DeleteText(selectedLayer as LayerText, selectedLayer.SelectedObjects));
						break;
					case "LayerBrick":
						(selectedLayer as LayerBrick).copyCurrentSelection();
						ActionManager.Instance.doAction(new DeleteBrick(selectedLayer as LayerBrick, selectedLayer.SelectedObjects));
						break;
				}
			}
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				switch (selectedLayer.GetType().Name)
				{
					case "LayerText":
						(selectedLayer as LayerText).copyCurrentSelection();
						break;
					case "LayerBrick":
						(selectedLayer as LayerBrick).copyCurrentSelection();
						break;
				}
			}
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (Layer.CopyItems.Count > 0))
			{
				bool typeMismatch = true;
				string layerTypeLocalizedName = "";
				string itemTypeName = Layer.CopyItems[0].GetType().Name;
				switch (selectedLayer.GetType().Name)
				{
					case "LayerArea":
						layerTypeLocalizedName = Properties.Resources.ErrorMsgLayerTypeArea;
						break;
					case "LayerGrid":
						layerTypeLocalizedName = Properties.Resources.ErrorMsgLayerTypeGrid;
						break;
					case "LayerText":
						layerTypeLocalizedName = Properties.Resources.ErrorMsgLayerTypeText;
						if (itemTypeName.Equals("TextCell"))
						{
							(selectedLayer as LayerText).pasteCopiedList(10.0f);
							typeMismatch = false;
						}
						break;
					case "LayerBrick":
						layerTypeLocalizedName = Properties.Resources.ErrorMsgLayerTypeBrick;
						if (itemTypeName.Equals("Brick"))
						{
							(selectedLayer as LayerBrick).pasteCopiedList(10.0f);
							typeMismatch = false;
						}
						break;
				}
				if (typeMismatch)
				{
					string message = Properties.Resources.ErrorMsgCanNotPaste.Replace("&&", layerTypeLocalizedName);
					if (itemTypeName.Equals("Brick"))
						message = message.Replace("&", Properties.Resources.ErrorMsgLayerTypeBrick);
					else
						message = message.Replace("&", Properties.Resources.ErrorMsgLayerTypeText);
					MessageBox.Show(this, message,
									Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
									MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
				}
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				switch (selectedLayer.GetType().Name)
				{
					case "LayerText":
						ActionManager.Instance.doAction(new DeleteText(selectedLayer as LayerText, selectedLayer.SelectedObjects));
						break;
					case "LayerBrick":
						ActionManager.Instance.doAction(new DeleteBrick(selectedLayer as LayerBrick, selectedLayer.SelectedObjects));
						break;
				}
			}
		}

		public void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// select all in the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if (selectedLayer != null)
			{
				selectedLayer.selectAll();
				this.mapPanel.Invalidate();
			}
		}

		public void deselectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				selectedLayer.clearSelection();
				this.mapPanel.Invalidate();
			}
		}

		public void selectPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.GetType().Name.Equals("LayerBrick")) && (selectedLayer.SelectedObjects.Count == 2))
			{
				List<Layer.LayerItem> brickToSelect = mAStar.findPath(selectedLayer.SelectedObjects[0] as LayerBrick.Brick, selectedLayer.SelectedObjects[1] as LayerBrick.Brick);
				// if AStar found a path, select the path
				if (brickToSelect.Count > 0)
				{
					selectedLayer.clearSelection();
					selectedLayer.addObjectInSelection(brickToSelect);
					this.mapPanel.Invalidate();
				}
			}
		}

		private void moveStepDisabledToolStripMenuItem_Click(object sender, EventArgs e)
		{
			enableSnapGridButton(false);
		}

		private void moveStep32ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarGrid32Button.Checked = true;
			this.toolBarGrid16Button.Checked = false;
			this.toolBarGrid8Button.Checked = false;
			this.toolBarGrid4Button.Checked = false;
			this.toolBarGrid1Button.Checked = false;
			this.toolBarGrid05Button.Checked = false;
			// enable the toolbar, and the menu item will be checked according to the previously set toolbar
			enableSnapGridButton(true);
			//layer
			Layer.CurrentSnapGridSize = 32;
		}

		private void moveStep16ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarGrid32Button.Checked = false;
			this.toolBarGrid16Button.Checked = true;
			this.toolBarGrid8Button.Checked = false;
			this.toolBarGrid4Button.Checked = false;
			this.toolBarGrid1Button.Checked = false;
			this.toolBarGrid05Button.Checked = false;
			// enable the toolbar, and the menu item will be checked according to the previously set toolbar
			enableSnapGridButton(true);
			//layer
			Layer.CurrentSnapGridSize = 16;
		}

		private void moveStep8ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarGrid32Button.Checked = false;
			this.toolBarGrid16Button.Checked = false;
			this.toolBarGrid8Button.Checked = true;
			this.toolBarGrid4Button.Checked = false;
			this.toolBarGrid1Button.Checked = false;
			this.toolBarGrid05Button.Checked = false;
			// enable the toolbar, and the menu item will be checked according to the previously set toolbar
			enableSnapGridButton(true);
			//layer
			Layer.CurrentSnapGridSize = 8;
		}

		private void moveStep4ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarGrid32Button.Checked = false;
			this.toolBarGrid16Button.Checked = false;
			this.toolBarGrid8Button.Checked = false;
			this.toolBarGrid4Button.Checked = true;
			this.toolBarGrid1Button.Checked = false;
			this.toolBarGrid05Button.Checked = false;
			// enable the toolbar, and the menu item will be checked according to the previously set toolbar
			enableSnapGridButton(true);
			//layer
			Layer.CurrentSnapGridSize = 4;
		}

		private void moveStep1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarGrid32Button.Checked = false;
			this.toolBarGrid16Button.Checked = false;
			this.toolBarGrid8Button.Checked = false;
			this.toolBarGrid4Button.Checked = false;
			this.toolBarGrid1Button.Checked = true;
			this.toolBarGrid05Button.Checked = false;
			// enable the toolbar, and the menu item will be checked according to the previously set toolbar
			enableSnapGridButton(true);
			//layer
			Layer.CurrentSnapGridSize = 1;
		}

		private void moveStep05ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarGrid32Button.Checked = false;
			this.toolBarGrid16Button.Checked = false;
			this.toolBarGrid8Button.Checked = false;
			this.toolBarGrid4Button.Checked = false;
			this.toolBarGrid1Button.Checked = false;
			this.toolBarGrid05Button.Checked = true;
			// enable the toolbar, and the menu item will be checked according to the previously set toolbar
			enableSnapGridButton(true);
			//layer
			Layer.CurrentSnapGridSize = 0.5f;
		}

		private void rotationStep90ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarAngle90Button.Checked = true;
			this.toolBarAngle45Button.Checked = false;
			this.toolBarAngle22Button.Checked = false;
			this.toolBarAngle1Button.Checked = false;
			//menu
			this.rotationStep90ToolStripMenuItem.Checked = true;
			this.rotationStep45ToolStripMenuItem.Checked = false;
			this.rotationStep22ToolStripMenuItem.Checked = false;
			this.rotationStep1ToolStripMenuItem.Checked = false;
			//layer
			Layer.CurrentRotationStep = 90;
		}

		private void rotationStep45ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarAngle90Button.Checked = false;
			this.toolBarAngle45Button.Checked = true;
			this.toolBarAngle22Button.Checked = false;
			this.toolBarAngle1Button.Checked = false;
			//menu
			this.rotationStep90ToolStripMenuItem.Checked = false;
			this.rotationStep45ToolStripMenuItem.Checked = true;
			this.rotationStep22ToolStripMenuItem.Checked = false;
			this.rotationStep1ToolStripMenuItem.Checked = false;
			//layer
			Layer.CurrentRotationStep = 45;
		}

		private void rotationStep22ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarAngle90Button.Checked = false;
			this.toolBarAngle45Button.Checked = false;
			this.toolBarAngle22Button.Checked = true;
			this.toolBarAngle1Button.Checked = false;
			//menu
			this.rotationStep90ToolStripMenuItem.Checked = false;
			this.rotationStep45ToolStripMenuItem.Checked = false;
			this.rotationStep22ToolStripMenuItem.Checked = true;
			this.rotationStep1ToolStripMenuItem.Checked = false;
			//layer
			Layer.CurrentRotationStep = 22.5f;
		}

		private void rotationStep1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarAngle90Button.Checked = false;
			this.toolBarAngle45Button.Checked = false;
			this.toolBarAngle22Button.Checked = false;
			this.toolBarAngle1Button.Checked = true;
			//menu
			this.rotationStep90ToolStripMenuItem.Checked = false;
			this.rotationStep45ToolStripMenuItem.Checked = false;
			this.rotationStep22ToolStripMenuItem.Checked = false;
			this.rotationStep1ToolStripMenuItem.Checked = true;
			//layer
			Layer.CurrentRotationStep = 1;
		}

		private void rotateCWToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count > 0))
			{
				switch (Map.Instance.SelectedLayer.GetType().Name)
				{
					case "LayerBrick":
						ActionManager.Instance.doAction(new RotateBrick(Map.Instance.SelectedLayer as LayerBrick, Map.Instance.SelectedLayer.SelectedObjects, 1));
						break;
					case "LayerText":
						ActionManager.Instance.doAction(new RotateText(Map.Instance.SelectedLayer as LayerText, Map.Instance.SelectedLayer.SelectedObjects, 1));
						break;
				}
			}
		}

		private void rotateCCWToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count > 0))
			{
				switch (Map.Instance.SelectedLayer.GetType().Name)
				{
					case "LayerBrick":
						ActionManager.Instance.doAction(new RotateBrick(Map.Instance.SelectedLayer as LayerBrick, Map.Instance.SelectedLayer.SelectedObjects, -1));
						break;
					case "LayerText":
						ActionManager.Instance.doAction(new RotateText(Map.Instance.SelectedLayer as LayerText, Map.Instance.SelectedLayer.SelectedObjects, -1));
						break;
				}
			}
		}

		private void paintToolPaintToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarPaintButton.Image = mPaintIcon;
			// menu
			this.paintToolPaintToolStripMenuItem.Checked = true;
			this.paintToolEraseToolStripMenuItem.Checked = false;
			// set the current static paint color in the area layer
			LayerArea.CurrentDrawColor = mCurrentPaintIconColor;
		}

		private void paintToolEraseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarPaintButton.Image = this.eraseToolStripMenuItem.Image;
			// menu
			this.paintToolPaintToolStripMenuItem.Checked = false;
			this.paintToolEraseToolStripMenuItem.Checked = true;
			// set the current static paint color in the area layer
			LayerArea.CurrentDrawColor = Color.Empty;
		}

		private void generatePaintIcon()
		{
			// get the background color from the color dialog
			// but cheat a little if the user choose the magenta color, because it is the transparent
			// color of the original bitmap
			Color backColor = mCurrentPaintIconColor;
			if ((backColor == Color.Magenta) || (backColor == Color.Fuchsia))
				backColor = Color.FromArgb(unchecked((int)0xFFFF00FE));
			// recreate the icon and use the color of the color dialog for the background
			mPaintIcon = new Bitmap(16, 16);
			Graphics g = Graphics.FromImage(mPaintIcon);
			g.Clear(backColor);
			g.DrawImage(this.paintToolStripMenuItem.Image, 0, 0);
			g.Flush();
			// refresh the icon
			this.toolBarPaintButton.Image = mPaintIcon;
			// set the current static paint color in the area layer
			LayerArea.CurrentDrawColor = mCurrentPaintIconColor;
		}

		private void paintToolChooseColorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.colorDialog.Color = mCurrentPaintIconColor;
			DialogResult result = this.colorDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				mCurrentPaintIconColor = this.colorDialog.Color;
				generatePaintIcon();
			}
		}

		private void mapBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DialogResult result = this.colorDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				ActionManager.Instance.doAction(new ChangeBackgroundColor(this.colorDialog.Color));
			}
		}

		private void currentLayerOptionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			if (Map.Instance.SelectedLayer != null)
			{
				switch (Map.Instance.SelectedLayer.GetType().Name)
				{
					case "LayerGrid":
						{
							LayerGridOptionForm optionForm = new LayerGridOptionForm(Map.Instance.SelectedLayer as LayerGrid);
							optionForm.ShowDialog();
							break;
						}
					case "LayerBrick":
						{
							LayerBrickOptionForm optionForm = new LayerBrickOptionForm(Map.Instance.SelectedLayer as LayerBrick);
							optionForm.ShowDialog();
							break;
						}
					case "LayerText":
						{
							LayerBrickOptionForm optionForm = new LayerBrickOptionForm(Map.Instance.SelectedLayer as LayerText);
							optionForm.ShowDialog();
							break;
						}
					case "LayerArea":
						{
							LayerAreaOptionForm optionForm = new LayerAreaOptionForm(Map.Instance.SelectedLayer as LayerArea);
							optionForm.ShowDialog();
							break;
						}
				}
			}
		}

		private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GlobalOptionsForm optionForm = new GlobalOptionsForm();
			DialogResult result = optionForm.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// reinit the array of shortcut
				initShortcutKeyArrayFromSettings();
				// update the gamma for the layer bricks because they may have changed (before redrawing the map)
				LayerBrick.sUpdateGammaFromSettings();
				// redraw the map because the color scheme may have changed
				this.mapPanel.Invalidate();
				// also redraw the undo stack
				updateUndoRedoMenuItems();
			}
			// update the the recent file list anyway because the user may have click the
			// clear recent file list button before clicking cancel
			UpdateRecentFileMenuFromConfigFile();

			// check if we need to restart, if yes, ask the user what he wants to do
			if (optionForm.DoesNeedToRestart)
			{
				DialogResult doesUserWantRestart = MessageBox.Show(this, Properties.Resources.ErrorMsgLanguageHasChanged,
					Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

				if (doesUserWantRestart == DialogResult.Yes)
				{
					// the user can not cancel the close of the application, because once the restart is called
					// it will launch a new instance of the application, so if the user can cancel the close
					// of the current instance he may end up with two instances.
					mCanUserCancelTheApplicationClose = false;
					// close the Part List else it prevent the application to close
					mPartListForm.Close();
					mPartListForm.Dispose();
					// and restart the application
					Application.Restart();
				}
			}
		}

		#endregion

		#region View Menu
		private void toolbarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.toolBar.Visible = this.toolbarMenuItem.Checked;
		}

		private void statusBarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.statusBar.Visible = this.statusBarMenuItem.Checked;
		}

		private void generalInformationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			GeneralInfoForm infoForm = new GeneralInfoForm();
			infoForm.ShowDialog();
		}

		public void partListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// reverse the state of the visibility of the part list form
			mPartListForm.Visible = !mPartListForm.Visible;
			// and set the checkbox as the visibility
			this.partListToolStripMenuItem.Checked = mPartListForm.Visible;
		}

		#endregion

		#region Help Menu
		private void helpContentsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// construct the path of the help file from the current language
			FileInfo	helpFileInfo = new FileInfo(Properties.Settings.Default.Language + @"/BlueBrick.chm");

			// If the help file related with the current language of the application does not exist,
			// load the default one which is in the same folder as the application
			if (!helpFileInfo.Exists)
				helpFileInfo = new FileInfo(@"BlueBrick.chm");

			// now check if we can open the help file, else display a warning message
			if (helpFileInfo.Exists)
			{
				Help.ShowHelp(this, helpFileInfo.FullName);
			}
			else
			{
				MessageBox.Show(this, Properties.Resources.ErrorMsgNoHelpFile,
								Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
								MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}

		private void aboutBlueBrickToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox aboutBox = new AboutBox();
			aboutBox.ShowDialog();
		}
		#endregion

		#endregion

		#region event handler for toolbar
		private void toolBarNewButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			newToolStripMenuItem_Click(sender, e);
		}

		private void toolBarOpenButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			openToolStripMenuItem_Click(sender, e);
		}

		private void toolBarSaveButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			saveToolStripMenuItem_Click(sender, e);
		}

		private void toolBarUndoButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			int index = e.ClickedItem.Owner.Items.IndexOf(e.ClickedItem);
			ActionManager.Instance.undo(index+1);
		}

		private void toolBarRedoButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			int index = e.ClickedItem.Owner.Items.IndexOf(e.ClickedItem);
			ActionManager.Instance.redo(index + 1);
		}

		private void toolBarDeleteButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			deleteToolStripMenuItem_Click(sender, e);
		}

		private void toolBarCutButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			cutToolStripMenuItem_Click(sender, e);
		}

		private void toolBarCopyButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			copyToolStripMenuItem_Click(sender, e);
		}

		private void toolBarPasteButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			pasteToolStripMenuItem_Click(sender, e);
		}

		private void toolBarSnapGridButton_Click(object sender, EventArgs e)
		{
			if (this.toolBarSnapGridButton.ButtonPressed)
			{
				enableSnapGridButton(!this.toolBarSnapGridButton.DropDown.Enabled);
			}
		}

		private void toolBarGrid32Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			moveStep32ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarGrid16Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			moveStep16ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarGrid8Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			moveStep8ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarGrid4Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			moveStep4ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarGrid1Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			moveStep1ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarGrid05Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			moveStep05ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarAngle90Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			rotationStep90ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarAngle45Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			rotationStep45ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarAngle22Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			rotationStep22ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarAngle1Button_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			rotationStep1ToolStripMenuItem_Click(sender, e);
		}

		private void toolBarRotateCCWButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			rotateCCWToolStripMenuItem_Click(sender, e);
		}

		private void toolBarRotateCWButton_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			rotateCWToolStripMenuItem_Click(sender, e);
		}

		public void toolBarSendToBackButton_Click(object sender, EventArgs e)
		{
			if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count > 0))
			{
				if (Map.Instance.SelectedLayer.GetType().Name == "LayerBrick")
				{
					ActionManager.Instance.doAction(new SendBrickToBack(Map.Instance.SelectedLayer as LayerBrick, Map.Instance.SelectedLayer.SelectedObjects));
				}
				else if (Map.Instance.SelectedLayer.GetType().Name == "LayerText")
				{
					ActionManager.Instance.doAction(new SendTextToBack(Map.Instance.SelectedLayer as LayerText, Map.Instance.SelectedLayer.SelectedObjects));
				}
			}
		}

		public void toolBarBringToFrontButton_Click(object sender, EventArgs e)
		{
			if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count > 0))
			{
				if (Map.Instance.SelectedLayer.GetType().Name == "LayerBrick")
				{
					ActionManager.Instance.doAction(new BringBrickToFront(Map.Instance.SelectedLayer as LayerBrick, Map.Instance.SelectedLayer.SelectedObjects));
				}
				else if (Map.Instance.SelectedLayer.GetType().Name == "LayerText")
				{
					ActionManager.Instance.doAction(new BringTextToFront(Map.Instance.SelectedLayer as LayerText, Map.Instance.SelectedLayer.SelectedObjects));
				}
			}
		}

		private void toolBarPaintButton_ButtonClick(object sender, EventArgs e)
		{
			// nothing happen if the eraser is choosen
			if (this.toolBarPaintButton.Image == mPaintIcon)
			{
				// else choose the color
				paintToolChooseColorToolStripMenuItem_Click(sender, e);
			}
		}

		private void paintToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			paintToolPaintToolStripMenuItem_Click(sender, e);
		}

		private void eraseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// call the event handler of the menu
			paintToolEraseToolStripMenuItem_Click(sender, e);
		}

		#endregion

		#region event handler for layers


		private void layerUpButton_Click(object sender, EventArgs e)
		{
			if (Map.Instance.SelectedLayer != null)
			{
				int index = Map.Instance.getIndexOf(Map.Instance.SelectedLayer);
				if (index < Map.Instance.NumLayers - 1)
					ActionManager.Instance.doAction(new MoveLayerUp(Map.Instance.SelectedLayer));
			}
		}

		private void layerDownButton_Click(object sender, EventArgs e)
		{
			if (Map.Instance.SelectedLayer != null)
			{
				int index = Map.Instance.getIndexOf(Map.Instance.SelectedLayer);
				if (index > 0)
					ActionManager.Instance.doAction(new MoveLayerDown(Map.Instance.SelectedLayer));
			}
		}
		private void newLayerTextButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerText"));
		}

		private void newLayerGridButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerGrid"));
		}
		private void newLayerAreaButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerArea"));
		}

		private void newLayerBrickButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerBrick"));
		}

		private void trashLayerButton_Click(object sender, EventArgs e)
		{
			if (Map.Instance.SelectedLayer != null)
				ActionManager.Instance.doAction(new RemoveLayer(Map.Instance.SelectedLayer));
		}

		#endregion

		#region event handler for map

		private void MainForm_MouseWheel(object sender, MouseEventArgs e)
		{
			if (this.mapPanel.Focused)
			{
				// convert the coord of the mouse position into client area
				Point clientCoord = this.TopLevelControl.PointToScreen(e.Location);
				clientCoord = this.mapPanel.PointToClient(clientCoord);
				// recreate a new event for the client and call the client
				MouseEventArgs clientEvent = new MouseEventArgs(e.Button, e.Clicks, clientCoord.X, clientCoord.Y, e.Delta);
				mapPanel.MapPanel_MouseWheel(sender, clientEvent);
			}
		}

		/// <summary>
		/// Move the currently selected object from the specified value
		/// </summary>
		/// <param name="isRealMove">tell if it is a real move that must be recorded in the undo stack, or just an update</param>
		/// <param name="move">an incremental move if this is not a real move, else the total move</param>
		private void moveSelectedObjects(bool isRealMove, PointF move)
		{
			// check if there's nothing to move
			if ((move.X == 0.0f) && (move.Y == 0.0f))
				return;

			// first get the current selected layer
			if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count > 0))
			{
				List<Layer.LayerItem> itemList = Map.Instance.SelectedLayer.SelectedObjects;

				switch (Map.Instance.SelectedLayer.GetType().Name)
				{
					case "LayerBrick":
						if (isRealMove)
						{
							// create the action and first undo it, to cancel all the incremental moves
							MoveBrick moveAction = new MoveBrick(Map.Instance.SelectedLayer as LayerBrick, itemList, move);
							moveAction.undo();
							// then add it to the undo stack (that will perform the redo)
							ActionManager.Instance.doAction(moveAction);
						}
						else
						{
							// do a move action without puting it in the undo stack
							MoveBrick moveAction = new MoveBrick(Map.Instance.SelectedLayer as LayerBrick, itemList, move);
							moveAction.redo();
						}
						break;
					case "LayerText":
						if (isRealMove)
						{
							// create the action and first undo it, to cancel all the incremental moves
							MoveText moveAction = new MoveText(Map.Instance.SelectedLayer as LayerText, itemList, move);
							moveAction.undo();
							// then add it to the undo stack (that will perform the redo)
							ActionManager.Instance.doAction(moveAction);
						}
						else
						{
							// do a move action without puting it in the undo stack
							MoveText moveAction = new MoveText(Map.Instance.SelectedLayer as LayerText, itemList, move);
							moveAction.redo();
						}
						break;
				}
			}
		}

		/// <summary>
		/// Rotate the currently selected object from the specified value
		/// </summary>
		/// <param name="isRealMove">tell if it is a real rotation that must be recorded in the undo stack, or just an update</param>
		/// <param name="move">an incremental angle to rotate if this is not a real move, else the total angle</param>
		private void rotateSelectedObjects(bool isRealMove, int angleStep)
		{
			// check if there's nothing to rotate
			if (angleStep == 0)
				return;

			// first get the current selected layer
			if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count > 0))
			{
				List<Layer.LayerItem> itemList = Map.Instance.SelectedLayer.SelectedObjects;

				switch (Map.Instance.SelectedLayer.GetType().Name)
				{
					case "LayerBrick":
						if (isRealMove)
						{
							// create the opposite action and do it, to cancel all the incremental moves
							// we can not create the normal action and undo it because the rotation of connected
							// brick is not symetrical (because the rotation step is not constant)
							RotateBrick unrotateAction = new RotateBrick(Map.Instance.SelectedLayer as LayerBrick, itemList, -angleStep, true);
							unrotateAction.redo();
							// So create a new move action to add in the undo stack
							ActionManager.Instance.doAction(new RotateBrick(Map.Instance.SelectedLayer as LayerBrick, itemList, angleStep, true));
						}
						else
						{
							// do a move action without puting it in the undo stack
							RotateBrick rotateAction = new RotateBrick(Map.Instance.SelectedLayer as LayerBrick, itemList, angleStep, ((angleStep != -1) && (angleStep != 1)));
							rotateAction.redo();
						}
						break;
					case "LayerText":
						if (isRealMove)
						{
							//// undo the total move of all the objects
							RotateText rotateAction = new RotateText(Map.Instance.SelectedLayer as LayerText, itemList, angleStep, true);
							rotateAction.undo();
							// then add it to the undo stack (that will perform the redo)
							ActionManager.Instance.doAction(rotateAction);
						}
						else
						{
							// do a move action without puting it in the undo stack
							RotateText rotateAction = new RotateText(Map.Instance.SelectedLayer as LayerText, itemList, angleStep, ((angleStep != -1) && (angleStep != 1)));
							rotateAction.redo();
						}
						break;
				}
			}
		}
		#endregion

		#region event handler for drag and drop file
		private void MainForm_DragEnter(object sender, DragEventArgs e)
		{
			// we accept the drop if the data is a file name
			if (e.Data.GetDataPresent("FileNameW"))
			{
				// check if the first file has a valid suported extension
				string filename = ((string[])(e.Data.GetData("FileNameW")))[0];

				// authorize the drop if it's a file with the good extension
				if (canOpenThisFile(filename))
					e.Effect = DragDropEffects.Copy;
				else
					e.Effect = DragDropEffects.None;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void MainForm_DragDrop(object sender, DragEventArgs e)
		{
			// check if the current map is not save and display a warning message
			if (checkForUnsavedMap())
			{
				string[] filenames = (string[])(e.Data.GetData("FileNameW"));
				openMap(filenames[0]);
			}
		}
		#endregion

		#region Keyboard shortcut
		private void initShortcutKeyArrayFromSettings()
		{
			// we use an array to remap the keys from the index in the dropdown list of the option page to the real key code
			Keys[] keyRemap = { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z,
								Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.Space, Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9,
								Keys.Divide, Keys.Multiply, Keys.Subtract, Keys.Add, Keys.Decimal, Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12,
								Keys.Escape, Keys.Back, Keys.Return, Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Insert, Keys.Delete, Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown };

			// recreate a new array
			mShortcutKeys = new List<KeyAndPart>[(int)shortcutableAction.NB_ACTIONS];
			for (shortcutableAction actionIndex = 0; actionIndex < shortcutableAction.NB_ACTIONS; ++actionIndex)
				mShortcutKeys[(int)actionIndex] = new List<KeyAndPart>();

			// iterate on the settings list
			char[] separator = { '|' };
			foreach (string text in BlueBrick.Properties.Settings.Default.ShortcutKey)
			{
				try
				{
					string[] itemNames = text.Split(separator);
					int keyIndex = int.Parse(itemNames[0]);
					int actionIndex = int.Parse(itemNames[1]);
					int connexion = 0;
					if (actionIndex == 0)
						connexion = int.Parse(itemNames[3]);
					// add the key to the corresponding array
					mShortcutKeys[actionIndex].Add(new KeyAndPart(keyRemap[keyIndex], itemNames[2], connexion));
				}
				catch { }
			}
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			// by default we don't handle the keys
			e.Handled = false;

			// if any modifier is pressed, we don't handle the key, for example the CTRL+S will be handle
			// by the shortcut of the "Save" menu item
			if (e.Alt || e.Control || e.Shift)
			{
				// a modifier is pressed, we start to ignore all the shortcut until all the key of the
				// keyboard are released.
				mModifierWasPressedIgnoreCustomShortcut = true;

				// if a modifier is just pressed we will warn the mapPanel in case it wants to change the cursor
				// we need to check if the modifier changed because of the auto repeat key down event
				// if you keep pressing a keep
				if (mLastModifierKeyDown != e.Modifiers)
				{
					mLastModifierKeyDown = e.Modifiers;
					this.mapPanel.setDefaultCursor();
				}
				return;
			}

			// check if we need to ignore the custom shortcut
			if (mModifierWasPressedIgnoreCustomShortcut)
				return;

			// get the current value of the grid step in case of we need to move the selected objects
			float moveSize = Layer.CurrentSnapGridSize;
			if (!Layer.SnapGridEnabled)
				moveSize = 0.1f;

			// iterate on all the possible actions
			for (shortcutableAction actionIndex = 0; actionIndex < shortcutableAction.NB_ACTIONS; ++actionIndex)
			{
				// iterate on all the shortcut key for the current action
				foreach (KeyAndPart shortcut in mShortcutKeys[(int)actionIndex])
				{
					// check the key to see if we handle it
					if (e.KeyCode == shortcut.mKeyCode)
					{
						// we handle this key
						e.Handled = true;

						// more stuff to init for some specific actions
						switch (actionIndex)
						{
							case shortcutableAction.ADD_PART:
								// add a connected brick, or if the selection count is different from 1, add a brick in the origin
								if (Map.Instance.SelectedLayer.SelectedObjects.Count == 1)
									Map.Instance.addConnectBrick(shortcut.mPartName, shortcut.mConnexion);
								else
									Map.Instance.addBrick(shortcut.mPartName, new PointF(0.0f, 0.0f));
								break;
							case shortcutableAction.DELETE_PART:
								// shortcut to the event handler of the menu
								deleteToolStripMenuItem_Click(sender, e);
								break;
							case shortcutableAction.ROTATE_LEFT:
								// set the flag
								mIsRotateLeftDown = true;
								// modify the move vector
								mObjectTotalStepToRotate--;
								// add a fake rotation for updating the view
								rotateSelectedObjects(false, -1);
								break;
							case shortcutableAction.ROTATE_RIGHT:
								// set the flag
								mIsRotateRightDown = true;
								// modify the move vector
								mObjectTotalStepToRotate++;
								// add a fake rotation for updating the view
								rotateSelectedObjects(false, 1);
								break;
							case shortcutableAction.MOVE_LEFT:
								// set the flag
								mIsLeftArrowDown = true;
								// modify the move vector
								mObjectTotalMove.X -= moveSize;
								// add a fake move for updating the view
								moveSelectedObjects(false, new PointF(-moveSize, 0));
								break;
							case shortcutableAction.MOVE_RIGHT:
								// set the flag
								mIsRightArrowDown = true;
								// modify the move vector
								mObjectTotalMove.X += moveSize;
								// add a fake move for updating the view
								moveSelectedObjects(false, new PointF(moveSize, 0));
								break;
							case shortcutableAction.MOVE_UP:
								// set the flag
								mIsUpArrowDown = true;
								// modify the move vector
								mObjectTotalMove.Y -= moveSize;
								// add a fake move for updating the view
								moveSelectedObjects(false, new PointF(0, -moveSize));
								break;
							case shortcutableAction.MOVE_DOWN:
								// set the flag
								mIsDownArrowDown = true;
								// modify the move vector
								mObjectTotalMove.Y += moveSize;
								// add a fake move for updating the view
								moveSelectedObjects(false, new PointF(0, moveSize));
								break;
							case shortcutableAction.CHANGE_CURRENT_CONNEXION:
								Layer selectedLayer = Map.Instance.SelectedLayer;
								if ((selectedLayer != null) && (selectedLayer.GetType().Name.Equals("LayerBrick")) && (selectedLayer.SelectedObjects.Count == 1))
								{
									LayerBrick.Brick selectedBrick = (selectedLayer as LayerBrick).SelectedObjects[0] as LayerBrick.Brick;
									selectedBrick.setActiveConnectionPointWithNextOne();
									this.mapPanel.updateView();
								}
								break;
						}

						// we need to force the refresh of the map immediatly because the invalidate
						// is not fast enough compare to the repeat key event.
						this.mapPanel.Refresh();

						// and just return, because don't need to search more keys
						return;
					}
				}
			}
		}

		private void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			// we will try to handle the key, but there are several case for which 
			// we don't handle them
			e.Handled = true;

			// We will warn the mapPanel if a modifier is released in case it wants to change the cursor
			bool wasModifierReleased = (mLastModifierKeyDown != e.Modifiers);
			if (wasModifierReleased)
			{
				// save the new modifier state
				mLastModifierKeyDown = e.Modifiers;
				// and change the cursor of the panel
				this.mapPanel.setDefaultCursor();
				// a modifier was released anyway, we don't handle the key
				e.Handled = false;
			}

			// if any modifier is pressed, we don't handle the key released,
			// for example the CTRL+S will be handle by the shortcut of the "Save" menu item
			// when the S is released. Than means you can releas any keys you want if one
			// modifier is pressed, we ignore all of them
			if (e.Alt || e.Control || e.Shift)
				e.Handled = false;

			// if we still need to ignore the shortcut, because one modifier was pressed before,
			// the user must release all the keys before we start to handle the normal shortcut again
			if (mModifierWasPressedIgnoreCustomShortcut)
			{
				// if the modifier was pressed, but now it is another key which is released, we can reset
				// the flag, because we assume the other key was pressed during the modifier down
				// unfortunatly this is only working with one normal key pressed with a modifier,
				// if you press two normal key with a modifier, and release the modifier first, the
				// first released normal key will be skiped but not the second one.
				if (!wasModifierReleased)
					mModifierWasPressedIgnoreCustomShortcut = false;
				e.Handled = false;
			}

			// if we don't handle the key just return
			if (!e.Handled)
				return;

			// now reset the handle flag to false and we try to find if we handle that key
			e.Handled = false;

			// iterate on all the possible actions
			for (shortcutableAction actionIndex = 0; actionIndex < shortcutableAction.NB_ACTIONS; ++actionIndex)
			{
				// iterate on all the shortcut key for the current action
				foreach (KeyAndPart shortcut in mShortcutKeys[(int)actionIndex])
				{
					// check the key to see if we handle it
					if (e.KeyCode == shortcut.mKeyCode)
					{
						// we handle this key
						e.Handled = true;

						// a boolean to check if we must move the objects
						bool mustMoveObject = false;
						bool mustRotateObject = false;

						// more stuff to init for some specific actions
						switch (actionIndex)
						{
							case shortcutableAction.ROTATE_LEFT:
								mIsRotateLeftDown = false;
								mustRotateObject = !mIsRotateRightDown;
								break;
							case shortcutableAction.ROTATE_RIGHT:
								mIsRotateRightDown = false;
								mustRotateObject = !mIsRotateLeftDown;
								break;
							case shortcutableAction.MOVE_LEFT:
								mIsLeftArrowDown = false;
								mustMoveObject = !(mIsRightArrowDown || mIsDownArrowDown || mIsUpArrowDown);
								break;
							case shortcutableAction.MOVE_RIGHT:
								mIsRightArrowDown = false;
								mustMoveObject = !(mIsLeftArrowDown || mIsDownArrowDown || mIsUpArrowDown);
								break;
							case shortcutableAction.MOVE_UP:
								mIsUpArrowDown = false;
								mustMoveObject = !(mIsLeftArrowDown || mIsRightArrowDown || mIsDownArrowDown);
								break;
							case shortcutableAction.MOVE_DOWN:
								mIsDownArrowDown = false;
								mustMoveObject = !(mIsLeftArrowDown || mIsRightArrowDown || mIsUpArrowDown);
								break;
						}

						// check if one of these four keys are still pressed
						if (mustMoveObject)
						{
							// move the object with the specified vector
							moveSelectedObjects(true, mObjectTotalMove);
							// reset the move object
							mObjectTotalMove.X = 0;
							mObjectTotalMove.Y = 0;
						}

						// check if one of the two rotate key are still pressed, else do the real rotation
						if (mustRotateObject)
						{
							// rotate the selected parts
							rotateSelectedObjects(true, mObjectTotalStepToRotate);
							// reset the total rotate angle
							mObjectTotalStepToRotate = 0;
						}

						// and just return, because don't need to search more keys
						return;
					}
				}
			}
		}
		#endregion

		#region function related to parts library
		public string getSelectedPartNumberInPartLib()
		{
			ListView selectedListView = this.partsTabControl.SelectedTab.Controls[0] as ListView;
			if (selectedListView.SelectedItems.Count > 0)
				return (selectedListView.SelectedItems[0].Tag as string);
			return null;
		}

		public void resetSelectedPartInPartLib()
		{
			ListView selectedListView = this.partsTabControl.SelectedTab.Controls[0] as ListView;
			selectedListView.SelectedItems.Clear();
		}
		#endregion
	}
}