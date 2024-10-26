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
using System.Windows.Forms;
using System.IO;
using BlueBrick.Actions;
using BlueBrick.Actions.Texts;
using BlueBrick.Actions.Bricks;
using BlueBrick.MapData;
using BlueBrick.Actions.Maps;
using BlueBrick.Actions.Rulers;
using System.IO.Compression;

namespace BlueBrick
{
	public partial class MainForm : Form
    {
		#region variable
		// reference on the main form (set in the constructor)
		private static MainForm sInstance = null;

		public class LanguageCodeAndName
		{
			public string mCode = null; // language string code on two characters 
			public string mName = null; // language name is its own language
			public LanguageCodeAndName(string code, string name) { mCode = code; mName = name; }
		};

		// An array that contains all the language supported in the application
		public static readonly LanguageCodeAndName[] sLanguageCodeAndName = {
			new LanguageCodeAndName("en", Properties.Resources.LanguageEnglish), // DEFAULT LANGUAGE SHOULD BE FIRST
			new LanguageCodeAndName("fr", Properties.Resources.LanguageFrench),
			new LanguageCodeAndName("de", Properties.Resources.LanguageGerman),
			new LanguageCodeAndName("nl", Properties.Resources.LanguageDutch),
			new LanguageCodeAndName("pt", Properties.Resources.LanguagePortuguese),
			new LanguageCodeAndName("es", Properties.Resources.LanguageSpanish),
			new LanguageCodeAndName("it", Properties.Resources.LanguageItalian),
			new LanguageCodeAndName("no", Properties.Resources.LanguageNorwegian),
			new LanguageCodeAndName("sv", Properties.Resources.LanguageSwedish)
			//new LanguageCodeAndName("cn", Properties.Resources.LanguageChinese) //not integrated yet
		};

		// a flag mostly never used, only when the application wants to restart, to prevent the user to
		// be able to cancel the close of the application and then finally end up with two instance of the application
		private bool mCanUserCancelTheApplicationClose = true;

		// custom cursors for the application
		private Cursor mHiddenLayerCursor = null;
		private Cursor mPanViewCursor = null;
		private Cursor mZoomCursor = null;
		private Cursor mPanOrZoomViewCursor = null;
		private Cursor mGridArrowCursor = null;
		private Cursor mBrickArrowCursor = null;
		private Cursor mFlexArrowCursor = null;		
		private Cursor mBrickDuplicateCursor = null;
		private Cursor mBrickSelectionCursor = null;
		private Cursor mBrickSelectPathCursor = null;
		private Cursor mTextArrowCursor = null;
		private Cursor mTextDuplicateCursor = null;
		private Cursor mTextCreateCursor = null;
		private Cursor mTextSelectionCursor = null;
		private Cursor mAreaPaintCursor = null;
		private Cursor mAreaEraserCursor = null;
		private Cursor mRulerArrowCursor = null;
		private Cursor mRulerEditCursor = null;
		private Cursor mRulerDuplicateCursor = null;
		private Cursor mRulerSelectionCursor = null;
		private Cursor mRulerAddPoint1Cursor = null;
		private Cursor mRulerAddPoint2Cursor = null;
		private Cursor mRulerAddCircleCursor = null;
		private Cursor mRulerMovePointCursor = null;
		private Cursor mRulerScaleVerticalCursor = null;
		private Cursor mRulerScaleHorizontalCursor = null;
		private Cursor mRulerScaleDiagonalUpCursor = null;
		private Cursor mRulerScaleDiagonalDownCursor = null;

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

		// for some strange reason, under Mono, the export form crash in the ctor when instanciated a second time.
		// so instanciate only one time and keep the instance
		private ExportImageForm mExportImageForm = new ExportImageForm();

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
			SEND_TO_BACK,
			BRING_TO_FRONT,
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
		#endregion

		#region get/set
		/// <summary>
		/// Get the reference pointer on the main form window
		/// </summary>
		public static MainForm Instance
		{
			get { return sInstance; }
		}

		/// <summary>
		/// Get the current scale of the map view.
		/// </summary>
		public double MapViewScale
		{
			get { return this.mapPanel.ViewScale; }
		}

		#region cursors
		#region cursors for all layers
		/// <summary>
		/// Get the cursor to display when the current layer is hidden
		/// </summary>
		public Cursor HiddenLayerCursor
		{
			get { return mHiddenLayerCursor; }
		}

		/// <summary>
		/// Get the cursor to display when no action is possible on the layer
		/// </summary>
		public Cursor NoCursor
		{
			get { return Cursors.No; }
		}
		
		/// <summary>
		/// Get the cursor for panning the view
		/// </summary>
		public Cursor PanViewCursor
		{
			get { return mPanViewCursor; }
		}

		/// <summary>
		/// Get the cursor for zooming the view
		/// </summary>
		public Cursor ZoomCursor
		{
			get { return mZoomCursor; }
		}

		/// <summary>
		/// Get the cursor for panning or zooming the view
		/// </summary>
		public Cursor PanOrZoomViewCursor
		{
			get { return mPanOrZoomViewCursor; }
		}
		#endregion

		#region grid cursors
		/// <summary>
		/// Get the default cursor for the grid layer
		/// </summary>
		public Cursor GridArrowCursor
		{
			get { return mGridArrowCursor; }
		}

		/// <summary>
		/// Get the cursor when moving the grid
		/// </summary>
		public Cursor GridMoveCursor
		{
			get { return Cursors.SizeAll; }
		}
		#endregion

		#region brick cursor
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
		public Cursor FlexArrowCursor
		{
			get { return mFlexArrowCursor; }
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
		/// Get the cursor for selection of a path of connected bricks
		/// </summary>
		public Cursor BrickSelectPathCursor
		{
			get { return mBrickSelectPathCursor; }
		}

		/// <summary>
		/// Get the cursor when moving bricks
		/// </summary>
		public Cursor BrickMoveCursor
		{
			get { return Cursors.SizeAll; }
		}
		#endregion

		#region text cursor
		/// <summary>
		/// Get the default cursor for the text layer
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
		/// Get the cursor for creation of a new text cell
		/// </summary>
		public Cursor TextCreateCursor
		{
			get { return mTextCreateCursor; }
		}

		/// <summary>
		/// Get the cursor for selection of layer texts
		/// </summary>
		public Cursor TextSelectionCursor
		{
			get { return mTextSelectionCursor; }
		}

		/// <summary>
		/// Get the cursor when moving texts
		/// </summary>
		public Cursor TextMoveCursor
		{
			get { return Cursors.SizeAll; }
		}
		#endregion

		#region area cursors
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

		/// <summary>
		/// Get the cursor when moving areas
		/// </summary>
		public Cursor AreaMoveCursor
		{
			get { return Cursors.SizeAll; }
		}
		#endregion

		#region ruler cursors
		/// <summary>
		/// Get the cursor for selecting a ruler (the default cursor for a ruler layer)
		/// </summary>
		public Cursor RulerArrowCursor
		{
			get { return mRulerArrowCursor; }
		}

		/// <summary>
		/// Get the cursor for editing the properties of a ruler
		/// </summary>
		public Cursor RulerEditCursor
		{
			get { return mRulerEditCursor; }
		}

		/// <summary>
		/// Get the cursor for duplication of rulers
		/// </summary>
		public Cursor RulerDuplicateCursor
		{
			get { return mRulerDuplicateCursor; }
		}

		/// <summary>
		/// Get the cursor for multiple selection of rulers
		/// </summary>
		public Cursor RulerSelectionCursor
		{
			get { return mRulerSelectionCursor; }
		}

		/// <summary>
		/// Get the cursor for adding the first ruler point
		/// </summary>
		public Cursor RulerAddPoint1Cursor
		{
			get { return mRulerAddPoint1Cursor; }
		}

		/// <summary>
		/// Get the cursor for adding the second ruler point
		/// </summary>
		public Cursor RulerAddPoint2Cursor
		{
			get { return mRulerAddPoint2Cursor; }
		}

		/// <summary>
		/// Get the cursor for adding a circle ruler
		/// </summary>
		public Cursor RulerAddCircleCursor
		{
			get { return mRulerAddCircleCursor; }
		}

		/// <summary>
		/// Get the cursor for moving a linear ruler point
		/// </summary>
		public Cursor RulerMovePointCursor
		{
			get { return mRulerMovePointCursor; }
		}

		/// <summary>
		/// Get the cursor for scaling a ruler verticaly
		/// </summary>
		public Cursor RulerScaleVerticalCursor
		{
			get { return mRulerScaleVerticalCursor; }
		}

		/// <summary>
		/// Get the cursor for scaling a ruler horizontaly
		/// </summary>
		public Cursor RulerScaleHorizontalCursor
		{
			get { return mRulerScaleHorizontalCursor; }
		}

		/// <summary>
		/// Get the cursor for scaling toward north-east or south-west
		/// </summary>
		public Cursor RulerScaleDiagonalUpCursor
		{
			get { return mRulerScaleDiagonalUpCursor; }
		}

		/// <summary>
		/// Get the cursor for scaling north-west or south-east
		/// </summary>
		public Cursor RulerScaleDiagonalDownCursor
		{
			get { return mRulerScaleDiagonalDownCursor; }
		}

		/// <summary>
		/// Get the cursor when moving rulers
		/// </summary>
		public Cursor RulerMoveCursor
		{
			get { return Cursors.SizeAll; }
		}
		#endregion
		#endregion

		#region localized labels
		public string LabelAuthorLocalized
		{
			get { return labelAuthor.Text; }
		}
		public string LabelLUGLocalized
		{
			get { return labelLUG.Text; }
		}
		public string LabelEventLocalized
		{
			get { return labelEvent.Text; }
		}
		public string LabelDateLocalized
		{
			get { return labelDate.Text; }
		}
		public string LabelCommentLocalized
		{
			get { return labelComment.Text; }
		}
		#endregion

		#endregion

		#region Initialisation of the application

		public MainForm(string fileToOpen)
		{
			InitializeComponent();
			sInstance = this;
			// load the custom cursors and icons
			LoadEmbededCustomCursors();
			// reset the shortcut keys
			initShortcutKeyArrayFromSettings();
			// PATCH FIX BECAUSE DOT NET FRAMEWORK IS BUGGED
			PreferencesForm.sSaveDefaultKeyInSettings();
			// load the part info
			loadPartLibraryFromDisk();
			// PATCH FIX BECAUSE DOT NET FRAMEWORK IS BUGGED for mapping UI properties in settings
			// and do it after loading the library cause some UI settings concern the library
			loadUISettingFromDefaultSettings();
			// disbale all the buttons of the toolbar and menu items by default
			// the open of the file or the creation of new map will enable the correct buttons
			enableGroupingButton(false, false);
			enablePasteButton(false);
			enableToolbarButtonOnItemSelection(false);
			enableToolbarButtonOnLayerSelection(false, false, false);
			// check if we need to open a budget at startup
			if (Properties.Settings.Default.BudgetFilenameToLoadAtStartup != string.Empty)
			{
				if (!openBudget(Properties.Settings.Default.BudgetFilenameToLoadAtStartup, null))
				{
					// clear the settings if the budget is not valid, to avoid throwing the error message every time
					Properties.Settings.Default.BudgetFilenameToLoadAtStartup = string.Empty;
				}
			}
			else
			{
				updateEnableStatusForBudgetMenuItem();
				this.PartUsageListView.updateBudgetNotification();
			}
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
			// set the split container distance in the shown event because else the distance is not
			// correct if the window was maximise
			this.mainSplitContainer.SplitterDistance = Properties.Settings.Default.UIMainSplitContainerDistance;
			this.toolSplitContainer.SplitterDistance = Properties.Settings.Default.UIToolSplitContainerDistance;
		}

		/// <summary>
		/// A util function to fill a combobox with text that is read from a text file.
		/// The format of the text file is simple: every line in the text file will create an entry in the combo box
		/// This is used to fill the LUG and Event combo box
		/// </summary>
		/// <param name="comboBoxToFill">The combobox you want to fill</param>
		/// <param name="sourceDataFileName">The text file you want to read the data from</param>
		private void fillComboBoxFromTextFile(ComboBox comboBoxToFill, string sourceDataFileName)
		{
			try
			{
				string sourceDataFullFileName = Application.StartupPath + sourceDataFileName;
				System.IO.StreamReader textReader = new System.IO.StreamReader(sourceDataFullFileName);
				comboBoxToFill.Items.Clear();
				comboBoxToFill.Sorted = true;
				while (!textReader.EndOfStream)
					comboBoxToFill.Items.Add(textReader.ReadLine());
				textReader.Close();
			}
			catch
			{
			}
		}

		private void loadUISettingFromDefaultSettings()
		{
			// DOT NET BUG: the data binding of the Form size and window state interfere with the
			// the normal behavior of saving, so we remove the data binding and do it manually
			this.Location = Properties.Settings.Default.UIMainFormLocation;
			this.Size = Properties.Settings.Default.UIMainFormSize;
			this.WindowState = Properties.Settings.Default.UIMainFormWindowState;
			// part lib
			if (Properties.Settings.Default.UIFilterAllSentence != string.Empty)
			{
				this.removeInputFilterIndication();
				this.textBoxPartFilter.Text = Properties.Settings.Default.UIFilterAllSentence;
			}
			else if (this.textBoxPartFilter.Text == string.Empty)
			{
				// add the filter incitation message (which is saved in the ressource)
				// only if not another filter sentence was set during the loading
				addInputFilterIndication();
			}
			// set the flag after the sentence, cause the checked event handler will check the text
			this.filterAllTabCheckBox.Checked = Properties.Settings.Default.UIFilterAllLibraryTab;
			// budget menu
			this.showOnlyBudgetedPartsToolStripMenuItem.Checked = Properties.Settings.Default.ShowOnlyBudgetedParts;
			this.showBudgetNumbersToolStripMenuItem.Checked = Properties.Settings.Default.ShowBudgetNumbers;
			this.useBudgetLimitationToolStripMenuItem.Checked = Properties.Settings.Default.UseBudgetLimitation;
			// snap grid button enable and size
			enableSnapGridButton(Properties.Settings.Default.UISnapGridEnabled, Properties.Settings.Default.UISnapGridSize);
			// rotation step
			updateRotationStepButton(Properties.Settings.Default.UIRotationStep);
			// the zooming value
			this.mapPanel.ViewScale = Properties.Settings.Default.UIViewScale;
			// setting the correct Ruler tool
			switch (Properties.Settings.Default.UIRulerToolSelected)
			{
				case 0: rulerSelectAndEditToolStripMenuItem_Click(this, null); break;
				case 1: rulerAddRulerToolStripMenuItem_Click(this, null); break;
				case 2: rulerAddCircleToolStripMenuItem_Click(this, null); break;
			}
			// regenerate the paint icon with the right color in the background
			generatePaintIcon(Properties.Settings.Default.UIPaintColor);
			if (Properties.Settings.Default.UIIsEraserToolSelected)
				paintToolEraseToolStripMenuItem_Click(this, null);
			else
				paintToolPaintToolStripMenuItem_Click(this, null);
			// flag to split the part usage list (and force the check change in case it was not changed)
			this.SplitPartUsagePerLayerCheckBox.Checked = Properties.Settings.Default.UISplitPartUsagePerLayer;
			SplitPartUsagePerLayerCheckBox_CheckedChanged(this, null);
			this.IncludeHiddenLayerInPartListCheckBox.Checked = Properties.Settings.Default.UIIncludeHiddenPartsInPartUsage;
			IncludeHiddenLayerInPartListCheckBox_CheckedChanged(this, null);
			// toolbar and status bar visibility
			this.toolBar.Visible = this.toolbarMenuItem.Checked = Properties.Settings.Default.UIToolbarIsVisible;
			this.statusBar.Visible = this.statusBarMenuItem.Checked = Properties.Settings.Default.UIStatusbarIsVisible;
			this.mapPanel.CurrentStatusBarHeight = Properties.Settings.Default.UIStatusbarIsVisible ? this.statusBar.Height : 0;
			this.mapScrollBarsToolStripMenuItem.Checked = Properties.Settings.Default.UIMapScrollBarsAreVisible;
			this.mapPanel.ShowHideScrollBars(Properties.Settings.Default.UIMapScrollBarsAreVisible);
			this.watermarkToolStripMenuItem.Checked = Properties.Settings.Default.DisplayGeneralInfoWatermark;
			this.electricCircuitsMenuItem.Checked = Properties.Settings.Default.DisplayElectricCircuit;
			this.connectionPointsToolStripMenuItem.Checked = Properties.Settings.Default.DisplayFreeConnexionPoints;
            this.rulerAttachPointsToolStripMenuItem.Checked = Properties.Settings.Default.DisplayRulerAttachPoints;
            // the export window
            this.mExportImageForm.loadUISettingFromDefaultSettings();
			// fill the combo box in the Properties panel
			fillComboBoxFromTextFile(this.lugComboBox, @"/config/LugList.txt");
			fillComboBoxFromTextFile(this.eventComboBox, @"/config/EventList.txt");
		}

		private void saveUISettingInDefaultSettings()
		{
			// DOT NET BUG: the data binding of the Form size and window state interfere with the
			// the normal behavior of saving, so we remove the data binding and do it manually

			// don't save the window state in minimized, else when you reopen the application
			// it only appears in the task bar
			if (this.WindowState == FormWindowState.Minimized)
				Properties.Settings.Default.UIMainFormWindowState = FormWindowState.Normal;
			else
				Properties.Settings.Default.UIMainFormWindowState = this.WindowState;
			// save the normal size or the restore one
			if (this.WindowState == FormWindowState.Normal)
			{
				// normal window size
				Properties.Settings.Default.UIMainFormLocation = this.Location;
				Properties.Settings.Default.UIMainFormSize = this.Size;
			}
			else
			{
				// save the restore window size
				Properties.Settings.Default.UIMainFormLocation = this.RestoreBounds.Location;
				Properties.Settings.Default.UIMainFormSize = this.RestoreBounds.Size;
			}

			// split container
			Properties.Settings.Default.UIMainSplitContainerDistance = this.mainSplitContainer.SplitterDistance;
			Properties.Settings.Default.UIToolSplitContainerDistance = this.toolSplitContainer.SplitterDistance;

			// snap grid size and rotation and the current zooming value (view scale)
			Properties.Settings.Default.UISnapGridEnabled = Layer.SnapGridEnabled;
			Properties.Settings.Default.UISnapGridSize = Layer.CurrentSnapGridSize;
			Properties.Settings.Default.UIRotationStep = Layer.CurrentRotationStep;
			Properties.Settings.Default.UIViewScale = this.mapPanel.ViewScale;

			// paint color
			Properties.Settings.Default.UIPaintColor = mCurrentPaintIconColor;
			Properties.Settings.Default.UIIsEraserToolSelected = LayerArea.IsCurrentToolTheEraser;

			// ruler tool selected
			Properties.Settings.Default.UIRulerToolSelected = (int)(LayerRuler.CurrentEditTool);

			// flags for the part usage list
			Properties.Settings.Default.UISplitPartUsagePerLayer = this.SplitPartUsagePerLayerCheckBox.Checked;
			Properties.Settings.Default.UIIncludeHiddenPartsInPartUsage = this.IncludeHiddenLayerInPartListCheckBox.Checked;

			// toolbar and status bar visibility
			Properties.Settings.Default.UIToolbarIsVisible = this.toolBar.Visible;
			Properties.Settings.Default.UIStatusbarIsVisible = this.statusBar.Visible;
			Properties.Settings.Default.UIMapScrollBarsAreVisible = this.mapScrollBarsToolStripMenuItem.Checked;

			// the part lib display config
			savePartLibUISettingInDefaultSettings();

            // the export window
            this.mExportImageForm.saveUISettingInDefaultSettings();

			// try to save (never mind if we can not (for example BlueBrick is launched
			// from a write protected drive)
			try
			{
				BlueBrick.Properties.Settings.Default.Save();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Save all the UI setting related to the part library panel inside the default settings.
		/// This function is separated from the general save UI setting function, because it should
		/// be called also before reloading the library without closing the application
		/// </summary>
		private void savePartLibUISettingInDefaultSettings()
		{
			this.PartsTabControl.savePartListDisplayStatusInSettings();
			Properties.Settings.Default.UIFilterAllLibraryTab = this.filterAllTabCheckBox.Checked;
		}

		/// <summary>
		/// Load the specified cursor from the specified assembly and return it
		/// </summary>
		/// <param name="assembly">the assembly from which loading the cursor</param>
		/// <param name="cursorResourceName">the resource name of the embeded cursor</param>
		/// <returns>a new created cursor</returns>
		private Cursor LoadEmbededCustomCursors(System.Reflection.Assembly assembly, string cursorResourceName)
		{
			// get the stream from the assembly and create the cursor giving the stream
			System.IO.Stream stream = assembly.GetManifestResourceStream(cursorResourceName);
			Cursor cursor = new Cursor(stream);
			stream.Close();
			// return the created cursor
			return cursor;
		}

		/// <summary>
		/// Load and create all the embeded cursors creates specially for this application
		/// </summary>
		private void LoadEmbededCustomCursors()
		{
			// get the assembly
			System.Reflection.Assembly assembly = this.GetType().Assembly;
			// the load all the cursors
			mHiddenLayerCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.HiddenLayerCursor.cur");
			mPanViewCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.PanViewCursor.cur");
			mZoomCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.ZoomCursor.cur");
			mPanOrZoomViewCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.PanOrZoomViewCursor.cur");
			mGridArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.GridArrowCursor.cur");
			mBrickArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickArrowCursor.cur");
			mFlexArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.FlexArrowCursor.cur");
			mBrickDuplicateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickDuplicateCursor.cur");
			mBrickSelectionCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickSelectionCursor.cur");
			mBrickSelectPathCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.BrickSelectPathCursor.cur");
			mTextArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextArrowCursor.cur");
			mTextDuplicateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextDuplicateCursor.cur");
			mTextCreateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextCreateCursor.cur");
			mTextSelectionCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.TextSelectionCursor.cur");
			mAreaPaintCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.AreaPaintCursor.cur");
			mAreaEraserCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.AreaEraserCursor.cur");
			mRulerArrowCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerArrowCursor.cur");
			mRulerEditCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerEditCursor.cur");
			mRulerDuplicateCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerDuplicateCursor.cur");
			mRulerSelectionCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerSelectionCursor.cur");
			mRulerAddPoint1Cursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerAddPoint1Cursor.cur");
			mRulerAddPoint2Cursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerAddPoint2Cursor.cur");
			mRulerAddCircleCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerAddCircleCursor.cur");
			mRulerMovePointCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerMovePointCursor.cur");
			mRulerScaleVerticalCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleVerticalCursor.cur");
			mRulerScaleHorizontalCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleHorizontalCursor.cur");
			mRulerScaleDiagonalUpCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleDiagonalUpCursor.cur");
			mRulerScaleDiagonalDownCursor = LoadEmbededCustomCursors(assembly, "BlueBrick.Cursor.RulerScaleDiagonalDownCursor.cur");
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
			// reload first the connection type info because we will need it for loading the part library
			BrickLibrary.Instance.loadConnectionTypeInfo();
			// reload the color info, because the bricks also need the color name (for correctly setting up the color name in the bubble info)
			BrickLibrary.Instance.loadColorInfo();
			// reinit the parts tab control (that will fill the brick library again)
			this.PartsTabControl.initPartsTabControl();
			// and relod the other data for the brick library (after the brick library is loaded)
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
			return (fileExtension.Equals("bbm") || fileExtension.Equals("ldr") || fileExtension.Equals("ncp") ||
					fileExtension.Equals("mpd") || fileExtension.Equals("tdl") || fileExtension.Equals("dat"));
		}

		#endregion

		#region update function

		/// <summary>
		/// update all the view of the application
		/// </summary>
        public void updateView(Actions.Action.UpdateViewType mapUpdateType, Actions.Action.UpdateViewType layerUpdateType)
		{
			// update the map
            if (mapUpdateType > Actions.Action.UpdateViewType.NONE)
				this.mapPanel.updateView();

			// update the layer
            if (layerUpdateType > Actions.Action.UpdateViewType.NONE)
				this.layerStackPanel.updateView(layerUpdateType);

			// check if we need to change the "*" on the title bar
			updateTitleBar();

			// update the undo/redo stack
			updateUndoRedoMenuItems();
		}

		/// <summary>
		/// Update the title bar by displaying the application Name (BlueBrick) followed by the map file name with or without
		/// an asterix, and the budget file name with or without an asterix, like this:
		/// BlueBrick - Untitled.bbm (*) - MyBudget.bbb (*)
		/// </summary>
		public void updateTitleBar()
		{
			// use the file name of the map (not the full path)
			FileInfo fileInfo = new FileInfo(Map.Instance.MapFileName);
			string title = "BlueBrick - " + fileInfo.Name;
			if (Map.Instance.WasModified)
				title += " *";
			// Add the budget name if any is loaded
			if (Budget.Budget.Instance.IsExisting)
			{
				FileInfo budgetFileInfo = new FileInfo(Budget.Budget.Instance.BudgetFileName);
				title += " - " + budgetFileInfo.Name;
				if (Budget.Budget.Instance.WasModified)
					title += " *";
			}
			// set the title bar text
			this.Text = title;
		}

		/// <summary>
		/// This function can be called when the current tab is changed, and we want to update the filter box
		/// with the current one saved in the tab
		/// </summary>
		/// <param name="filterSentence">the new filter sentence to set in the filter box</param>
		public void updateFilterComboBox(string filterSentence)
		{
			if (filterSentence == string.Empty)
			{
				addInputFilterIndication();
			}
			else
			{
				removeInputFilterIndication();
				this.textBoxPartFilter.Text = filterSentence;
			}
		}

        /// <summary>
        /// Enable or disable the group/ungroup menu item in the edit menu and context menu
        /// </summary>
        /// <param name="canGroup">if true, the grouping buttons are enabled</param>
        /// <param name="canUngroup">if true the ungrouping buttons are enabled</param>
        public void enableGroupingButton(bool canGroup, bool canUngroup)
        {
            this.groupToolStripMenuItem.Enabled = canGroup;
            this.ungroupToolStripMenuItem.Enabled = canUngroup;
            this.groupMenuToolStripMenuItem.Enabled = canGroup || canUngroup;
        }

		/// <summary>
		/// Enable or disable the paste item button depending on the parameter and also if there's something
		/// to paste. So if the parameter is true but nothing was copied, the paste button will stay disabled
		/// <param name="canPaste">if true that means you can enable the button is needed</param>
		/// </summary>
		public void enablePasteButton(bool canPaste)
		{
			if (canPaste)
			{
				bool isThereAnyItemCopied = false;
				try
				{
					// if there's crazy stuff copied in the clipboard, this function may throw an exception
					// and this can happen at startup
					isThereAnyItemCopied = Clipboard.ContainsText();
				}
				catch
				{
					// do nothing, we probably cannot copy that shit
				}
				this.pasteToolStripMenuItem.Enabled = isThereAnyItemCopied;
				this.toolBarPasteButton.Enabled = isThereAnyItemCopied;
			}
			else
			{
				this.pasteToolStripMenuItem.Enabled = false;
				this.toolBarPasteButton.Enabled = false;
			}
		}

		/// <summary>
		/// Enable or disable the buttons in the tool bar that allow manipulation of the layer item
		/// such as cut/copy/delete and rotate and sned to back/bring to front button WHEN the item selection
		/// changed on the current layer.
		/// <param name="isThereAnyItemSelected">if true that means no item is selected on the current layer</param>
		/// </summary>
		public void enableToolbarButtonOnItemSelection(bool isThereAnyItemSelected)
		{
			// enable/disable the copy button (toolbar and menu)
			this.toolBarCopyButton.Enabled = isThereAnyItemSelected;
			this.copyToolStripMenuItem.Enabled = isThereAnyItemSelected;

			// enable/disable the cut button (toolbar and menu)
			this.toolBarCutButton.Enabled = isThereAnyItemSelected;
			this.cutToolStripMenuItem.Enabled = isThereAnyItemSelected;

			// enable/disable the delete button (toolbar and menu)
			this.toolBarDeleteButton.Enabled = isThereAnyItemSelected;
			this.deleteToolStripMenuItem.Enabled = isThereAnyItemSelected;

			// enable/disable the rotate buttons (toolbar and menu)
			this.toolBarRotateCCWButton.Enabled = isThereAnyItemSelected;
			this.toolBarRotateCWButton.Enabled = isThereAnyItemSelected;
			this.rotateCCWToolStripMenuItem.Enabled = isThereAnyItemSelected;
			this.rotateCWToolStripMenuItem.Enabled = isThereAnyItemSelected;

			// enable/disable the send to back/bring to front buttons (toolbar and menu)
			this.toolBarBringToFrontButton.Enabled = isThereAnyItemSelected;
			this.toolBarSendToBackButton.Enabled = isThereAnyItemSelected;
			this.bringToFrontToolStripMenuItem.Enabled = isThereAnyItemSelected;
			this.sendToBackToolStripMenuItem.Enabled = isThereAnyItemSelected;

			// enable/disable the deselect all button (menu only)
			this.deselectAllToolStripMenuItem.Enabled = isThereAnyItemSelected;

			// enable/disable the select path button (menu only)
            bool selectedLayerIsBrick = (Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer is LayerBrick);
			this.selectPathToolStripMenuItem.Enabled = selectedLayerIsBrick && (Map.Instance.SelectedLayer.SelectedObjects.Count >= 2);

            // enable/disable the save of the selection to the library
            this.saveSelectionInLibraryToolStripMenuItem.Enabled = selectedLayerIsBrick && (Map.Instance.SelectedLayer.SelectedObjects.Count > 1);
		}

		/// <summary>
		/// Enable or disable the buttons in the tool bar that allow manipulation of the layer item
		/// such as snap grid, snap rotation, rotate button and paint/erase WHEN the selected layer is changed.
		/// This method do not affect the cut/copy/delete and rotate and send to back/bring to front button
		/// because these buttons depends on the items selected on the layer, not on the layer type.
		/// </summary>
		/// <param name="enableMoveRotateButton">if true, enable the button related to move and rotate</param>
		/// <param name="enablePaintButton">if true, show and enable the tools button related to paint</param>
		/// <param name="enableRulerButton">if true, show and enable the tools buttons related to ruler</param>
		public void enableToolbarButtonOnLayerSelection(bool enableMoveRotateButton, bool enablePaintButton, bool enableRulerButton)
		{
			// enable the paste button if a layer with item has been selected (brick ot text layer)
			enablePasteButton(enableMoveRotateButton);

			// enable/disable the sub menu item Transform (only menu)
			this.transformToolStripMenuItem.Enabled = enableMoveRotateButton;

			// enable/disable the snapping grid (toolbar and menu)
			this.toolBarSnapGridButton.Enabled = enableMoveRotateButton;
			this.moveStepToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStepDisabledToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStep32ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStep16ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStep8ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStep4ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStep2ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStep1ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.moveStep05ToolStripMenuItem.Enabled = enableMoveRotateButton;

			// enable/disable the rotation step (toolbar and menu)
			this.toolBarRotationAngleButton.Enabled = enableMoveRotateButton;
			this.rotationStepToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotationStep1ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotationStep5ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotationStep11ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotationStep22ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotationStep45ToolStripMenuItem.Enabled = enableMoveRotateButton;
			this.rotationStep90ToolStripMenuItem.Enabled = enableMoveRotateButton;

			// the toolbar is enabled either if there's a paint or ruler to enable
			this.toolBarToolButton.Enabled = enablePaintButton || enableRulerButton;

			// enable/disable the paint button in the menu
			this.paintToolToolStripMenuItem.Enabled = enablePaintButton;
			this.paintToolEraseToolStripMenuItem.Enabled = enablePaintButton;
			this.paintToolPaintToolStripMenuItem.Enabled = enablePaintButton;
			this.paintToolChooseColorToolStripMenuItem.Enabled = enablePaintButton;			
			// show/hide the paint button in the toolbar
			this.paintToolStripMenuItem.Visible = enablePaintButton;
			this.eraseToolStripMenuItem.Visible = enablePaintButton;
			// adjust the image of the main button
			if (enablePaintButton)
			{
				if (LayerArea.IsCurrentToolTheEraser)
					this.toolBarToolButton.Image = this.eraseToolStripMenuItem.Image;
				else
					this.toolBarToolButton.Image = mPaintIcon;
			}

			// enable/disable the ruler buttons in the menu
			this.rulerToolToolStripMenuItem.Enabled = enableRulerButton;
			this.selectAndEditToolStripMenuItem.Enabled = enableRulerButton;
			this.addRulerToolStripMenuItem.Enabled = enableRulerButton;
			this.addCircleToolStripMenuItem.Enabled = enableRulerButton;
			// show/hide the ruler button in the toolbar
			this.rulerSelectAndEditToolStripMenuItem.Visible = enableRulerButton;
			this.rulerAddRulerToolStripMenuItem.Visible = enableRulerButton;
			this.rulerAddCircleToolStripMenuItem.Visible = enableRulerButton;
			// adjust the image of the main tool button
			if (enableRulerButton)
			{
				switch (LayerRuler.CurrentEditTool)
				{
					case LayerRuler.EditTool.SELECT:
						this.toolBarToolButton.Image = this.rulerSelectAndEditToolStripMenuItem.Image;
						break;
					case LayerRuler.EditTool.LINE:
						this.toolBarToolButton.Image = this.rulerAddRulerToolStripMenuItem.Image;
						break;
					case LayerRuler.EditTool.CIRCLE:
						this.toolBarToolButton.Image = this.rulerAddCircleToolStripMenuItem.Image;
						break;
				}
			}
		}

		/// <summary>
		/// This private method is only to mutualize code between the menu and the toolbar,
		/// to enable/disable the snap grid button
		/// </summary>
		/// <param name="enable">true is the snap grid is enabled</param>
		/// <param name="size">the new size</param>
		private void enableSnapGridButton(bool enable, float size)
		{
			// uncheck all the menu item
			this.moveStep32ToolStripMenuItem.Checked = false;
			this.moveStep16ToolStripMenuItem.Checked = false;
			this.moveStep8ToolStripMenuItem.Checked = false;
			this.moveStep4ToolStripMenuItem.Checked = false;
			this.moveStep2ToolStripMenuItem.Checked = false;
			this.moveStep1ToolStripMenuItem.Checked = false;
			this.moveStep05ToolStripMenuItem.Checked = false;
			// uncheck all the toolbar item
			this.toolBarGrid32Button.Checked = false;
			this.toolBarGrid16Button.Checked = false;
			this.toolBarGrid8Button.Checked = false;
			this.toolBarGrid4Button.Checked = false;
			this.toolBarGrid2Button.Checked = false;
			this.toolBarGrid1Button.Checked = false;
			this.toolBarGrid05Button.Checked = false;
			// enable or disable the correct items
			if (enable)
			{
				// menu
				this.moveStepDisabledToolStripMenuItem.Checked = false;
				if (size == 32.0f)
				{
					this.moveStep32ToolStripMenuItem.Checked = true;
					this.toolBarGrid32Button.Checked = true;
				}
				else if (size == 16.0f)
				{
					this.moveStep16ToolStripMenuItem.Checked = true;
					this.toolBarGrid16Button.Checked = true;
				}
				else if (size == 8.0f)
				{
					this.moveStep8ToolStripMenuItem.Checked = true;
					this.toolBarGrid8Button.Checked = true;
				}
				else if (size == 4.0f)
				{
					this.moveStep4ToolStripMenuItem.Checked = true;
					this.toolBarGrid4Button.Checked = true;
				}
				else if (size == 2.0f)
				{
					this.moveStep2ToolStripMenuItem.Checked = true;
					this.toolBarGrid2Button.Checked = true;
				}
				else if (size == 1.0f)
				{
					this.moveStep1ToolStripMenuItem.Checked = true;
					this.toolBarGrid1Button.Checked = true;
				}
				else
				{
					this.moveStep05ToolStripMenuItem.Checked = true;
					this.toolBarGrid05Button.Checked = true;
				}
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
			// set the size
			Layer.CurrentSnapGridSize = size;
		}

		/// <summary>
		/// Update the General info UI controls in the property tab from the currently loaded map.
		/// This function is called programatically to update the UI when the program need to, so the event handler
		/// of the UI element are disabled during this function to avoid having another change map info action created.
		/// <param name="doesNeedToUpdateDimension"/>If true, the dimension of the full map will also be updated, otherwise they are left as they are</param>
		/// </summary>
		public void updateMapGeneralInfo(bool doesNeedToUpdateDimension)
		{
			// disable the UI event handlers
			this.AuthorTextBox.Leave -= AuthorTextBox_Leave;
			this.lugComboBox.Leave -= lugComboBox_Leave;
			this.eventComboBox.Leave -= eventComboBox_Leave;
			this.dateTimePicker.ValueChanged -= dateTimePicker_ValueChanged;
			this.commentTextBox.Leave -= commentTextBox_Leave;

			// update the back color of the background color button
			this.DocumentDataPropertiesMapBackgroundColorButton.BackColor = Map.Instance.BackgroundColor;
			// fill the text controls
			this.AuthorTextBox.Text = Map.Instance.Author;
			this.lugComboBox.Text = Map.Instance.LUG;
			this.eventComboBox.Text = Map.Instance.Event;
			this.dateTimePicker.Value = Map.Instance.Date;
			char[] splitter = { '\n' };
			this.commentTextBox.Lines = Map.Instance.Comment.Split(splitter);
			// update also the map dimensions if needed
			if (doesNeedToUpdateDimension)
				updateMapDimensionInfo();

			// re-enable the UI event handlers
			this.AuthorTextBox.Leave += AuthorTextBox_Leave;
			this.lugComboBox.Leave += lugComboBox_Leave;
			this.eventComboBox.Leave += eventComboBox_Leave;
			this.dateTimePicker.ValueChanged += dateTimePicker_ValueChanged;
			this.commentTextBox.Leave += commentTextBox_Leave;
		}

		/// <summary>
		/// This function recompute the overall size of the map, and display it on the properties tab
		/// </summary>
		private void updateMapDimensionInfo()
		{
			// warn the map Panel that the map dimension may have changed
			this.mapPanel.MapAreaChangedNotification();

			// ignore if the properties tab is not visible
			if (this.DocumentDataTabControl.SelectedTab != this.DocumentDataPropertiesTabPage)
				return;

			// compute the size
			RectangleF totalArea = Map.Instance.getTotalAreaInStud(true);
			MapData.Tools.Distance width = new MapData.Tools.Distance(totalArea.Width, MapData.Tools.Distance.Unit.STUD);
			MapData.Tools.Distance height = new MapData.Tools.Distance(totalArea.Height, MapData.Tools.Distance.Unit.STUD);

			this.labelWidthModule.Text = Math.Ceiling(width.DistanceInModule).ToString();
			this.labelHeightModule.Text = Math.Ceiling(height.DistanceInModule).ToString();

			this.labelWidthStud.Text = Math.Round(width.DistanceInStud).ToString();
			this.labelHeightStud.Text = Math.Round(height.DistanceInStud).ToString();

			this.labelWidthMeter.Text = width.DistanceInMeter.ToString("N2");
			this.labelHeightMeter.Text = height.DistanceInMeter.ToString("N2");

			this.labelWidthFeet.Text = width.DistanceInFeet.ToString("N2");
			this.labelHeightFeet.Text = height.DistanceInFeet.ToString("N2");
		}

		public void NotifyPartListForLayerAdded(Layer layer)
		{
			this.PartUsageListView.addLayerNotification(layer as LayerBrick);
		}

		public void NotifyPartListForLayerRemoved(Layer layer)
		{
			this.PartUsageListView.removeLayerNotification(layer as LayerBrick);
		}

		public void NotifyPartListForLayerRenamed(Layer layer)
		{
			this.PartUsageListView.renameLayerNotification(layer as LayerBrick);
		}

		public void NotifyPartListForBrickAdded(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isCausedByRegroup)
		{
			// inform the budget first, because the part usage also display the budget
			Budget.Budget.Instance.addBrickNotification(layer, brickOrGroup, isCausedByRegroup);
			this.PartUsageListView.addBrickNotification(layer, brickOrGroup, isCausedByRegroup);
			this.PartsTabControl.updatePartCountAndBudget(brickOrGroup);
			// update the map dimensions
			updateMapDimensionInfo();
		}

		public void NotifyPartListForBrickRemoved(LayerBrick layer, Layer.LayerItem brickOrGroup, bool isCausedByUngroup)
		{
			// inform the budget first, because the part usage also display the budget
			Budget.Budget.Instance.removeBrickNotification(layer, brickOrGroup, isCausedByUngroup);
			this.PartUsageListView.removeBrickNotification(layer, brickOrGroup, isCausedByUngroup);
			this.PartsTabControl.updatePartCountAndBudget(brickOrGroup);
			// update the map dimensions
			updateMapDimensionInfo();
		}

		public void NotifyForPartMoved()
		{
			// update the map dimensions
			updateMapDimensionInfo();
		}

		public void NotifyForLayerVisibilityChangedOrLayerDeletion()
		{
			// update the map dimensions
			updateMapDimensionInfo();
		}

		public void NotifyForMapBackgroundColorChanged()
		{
			// update the back color of the background color button
			this.DocumentDataPropertiesMapBackgroundColorButton.BackColor = Map.Instance.BackgroundColor;
		}
		public void NotifyForBudgetChanged(string partId)
		{
			this.PartUsageListView.updateBudgetNotification(partId);
		}
		#endregion

		#region status bar

		public void setStatusBarMessage(string message)
		{
			// escape the ampersome character
			this.statusBarLabel.Text = message.Replace("&", "&&");
		}

		public void resetProgressBar(int maxValue)
		{
			try
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
			catch
			{
				// ignore if if the progressbar is already dead
			}
		}

		public void stepProgressBar()
		{
			try
			{
				// perform the step
				this.statusBarProgressBar.PerformStep();
				// hide automatically the progress bar when the end is reached
				if (this.statusBarProgressBar.Value >= this.statusBarProgressBar.Maximum)
					this.statusBarProgressBar.Visible = false;
			}
			catch
			{
				// ignore if if the progressbar is already dead
			}
		}

		public void stepProgressBar(int stepValue)
		{
			try
			{
				if (stepValue > 0)
				{
					this.statusBarProgressBar.Step = stepValue;
					stepProgressBar();
				}
			}
			catch
			{
				// ignore if if the progressbar is already dead
			}
		}

		public void finishProgressBar()
		{
			try
			{
				if (this.statusBarProgressBar.Value < this.statusBarProgressBar.Maximum)
				{
					this.statusBarProgressBar.Step = this.statusBarProgressBar.Maximum - this.statusBarProgressBar.Value;
					stepProgressBar();
				}
			}
			catch
			{
				// ignore if if the progressbar is already dead
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
					// if the save failed for whatever reason, return true to cancel the continuation
					// and give a second chance to the user to save its file or just to quit without saving
					if (!saveMap())
						return false;
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

		private void reinitializeCurrentMap()
		{
			// create a new map to trash the previous one
			Map.Instance = new Map();
			Layer.resetNameInstanceCounter();
			ActionManager.Instance.clearStacks();
			// reset the modified flag
			Map.Instance.WasModified = false;
			// reset the current file name
			changeCurrentMapFileName(Properties.Resources.DefaultSaveFileName, false);
			// update the view any way
            this.updateView(Actions.Action.UpdateViewType.FULL, Actions.Action.UpdateViewType.FULL);
			Budget.Budget.Instance.recountAllBricks();
			this.PartUsageListView.rebuildList();
			this.PartsTabControl.updateAllPartCountAndBudget();
			// update the properties
			updateMapGeneralInfo(true);
			// force a garbage collect because we just trashed the previous map
			GC.Collect();
		}

		private void createNewMap()
		{
			// trash the previous map
			reinitializeCurrentMap();

			// check the name of the template file to load when creating a new map, and load it if it is valid
			string templateFileToOpen = Properties.Settings.Default.TemplateFilenameWhenCreatingANewMap;
			if ((templateFileToOpen != null) && File.Exists(templateFileToOpen) && canOpenThisFile(templateFileToOpen))
			{
				// the template file seems valid, so open it
				openMap(templateFileToOpen, true);
			}
			else
			{
				// no valid template file, create a default map with default settings
				ActionManager.Instance.doAction(new AddLayer("LayerGrid", false));
				ActionManager.Instance.doAction(new AddLayer("LayerBrick", false));
			}

			// after adding the two default layer, we reset the WasModified flag of the map
			// (and before the update of the title bar)
			Map.Instance.WasModified = false;
			// update the view any way
            this.updateView(Actions.Action.UpdateViewType.FULL, Actions.Action.UpdateViewType.FULL);
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
            Map.Instance.MapFileName = filename;
			Map.Instance.IsMapNameValid = isAValidName;
			// update the name of the title bar with the name of the file
			updateTitleBar();
		}

		private void openMap(string filename, bool isTemplateFile = false)
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
				this.updateView(Actions.Action.UpdateViewType.FULL, Actions.Action.UpdateViewType.FULL);
				Budget.Budget.Instance.recountAllBricks();
				this.PartUsageListView.rebuildList();
				this.PartsTabControl.updateAllPartCountAndBudget();
				// update the properties
				updateMapGeneralInfo(true);
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
			else
			{
				// call the finish progress bar to hide it, in case we had a error while loading the file
				finishProgressBar();
			}
			// restore the cursor after loading
			this.Cursor = Cursors.Default;
			// save the current file name of the loaded map
			if (isFileValid && !isTemplateFile)
				changeCurrentMapFileName(filename, true);
			else
				changeCurrentMapFileName(Properties.Resources.DefaultSaveFileName, false);
			// update the recent file list
			if (!isTemplateFile)
				UpdateRecentFileMenuFromConfigFile(filename, isFileValid);
			// force a garbage collect because we just trashed the previous map
			GC.Collect();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// check if the current map is not save and display a warning message
			if (checkForUnsavedMap())
			{
				DialogResult result = this.openFileDialog.ShowDialog(this);
				if (result == DialogResult.OK)
					openMap(this.openFileDialog.FileName);
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
			// get a reference on the list
			System.Collections.Specialized.StringCollection recentFiles = BlueBrick.Properties.Settings.Default.RecentFiles;
			// first update the settings
			// remove the file from this list since it will be re-added (or not) on top of the list
			recentFiles.Remove(fileName);
			// add the filename on top of the list (according to the parameter)
			if (isAdded)
				recentFiles.Insert(0, fileName);
			// if the maximum files is reached, we delete the last one (the old one)
			if (recentFiles.Count > 20)
				recentFiles.RemoveAt(20);
			// In order to save the change we need to recreate the list due to a Mono BUG:
			// if you just modify the list in the default settings, Mono didn't detect it and think no modification
			// was made, therefore the Save() method does nothing (don't save the modified list)
			// Moreover, if you just try to assign the recentFile list to Default.RecentFiles an exception is raised
			// because I guess it references the same list.
			// Moreover the Specialized.StringCollection class do not have a constructor to clone the list.
			// Therefore the only way is to recreate an empty list and fill it again. While doing it I clean it
			// a bit by removing the empty string (because the list is initiallized with empty string)
			BlueBrick.Properties.Settings.Default.RecentFiles = new System.Collections.Specialized.StringCollection();
			foreach (string name in recentFiles)
				if (name.Length > 0)
					BlueBrick.Properties.Settings.Default.RecentFiles.Add(name);
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

		/// <summary>
		/// Save the map that we know as a specified name. If you're not sure if the map as a name, call the saveMap() method instead.
		/// </summary>
		/// <returns>true if the map was correctly saved</returns>
		private bool saveNamedMap()
		{
			// set the wait cursor
			this.Cursor = Cursors.WaitCursor;
			// save the file
            bool saveDone = SaveLoadManager.save(Map.Instance.MapFileName);
			if (saveDone)
			{
				// after saving the map in any kind of format, we reset the WasModified flag of the map
				// (and before the update of the title bar)
				Map.Instance.WasModified = false;
				// update the title bar to remove the asterix
				this.updateTitleBar();
			}
			// restore the cursor
			this.Cursor = Cursors.Default;
			// return the save status
			return saveDone;
		}

		/// <summary>
		/// Save the current map with its current name. If the map is untitle (unnamed) then the saveMapAs() method will be called.
		/// </summary>
		/// <returns>true if the map was correctly saved</returns>
		private bool saveMap()
		{
			// a flag to know if the save was correctly done or not
			bool saveDone = false;

			// if the current file name is not defined we do a "save as..."
			if (Map.Instance.IsMapNameValid)
				saveDone = saveNamedMap();
			else
				saveDone = saveMapAs();

			// return the flag that tells if the map was saved
			return saveDone;
		}

		/// <summary>
		/// Save the current map under a different name (no matter if the current map has a name or not).
		/// This will pop out the save as dialog, and may also pop out a warning message if the user choose a not
		/// complete file format for the save.
		/// </summary>
		/// <returns>true if the map was correctly saved</returns>
		private bool saveMapAs()
		{
			// put the current file name in the dialog (which can be the default one)
			// but remove the extension, such as the user can easily change the extension in
			// the save dialog drop list, and the save dialog will add it automatically
            this.saveFileDialog.FileName = Path.GetFileNameWithoutExtension(Map.Instance.MapFileName);
            // if there's no initial directory, choose the My Documents directory
            this.saveFileDialog.InitialDirectory = Path.GetDirectoryName(Map.Instance.MapFileName);
            if (this.saveFileDialog.InitialDirectory.Length == 0)
                this.saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			// instantiate a flag to know if the save was actually done
			bool saveDone = false;
			// open the save as dialog
			DialogResult result = this.saveFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// for a "Save As..." only (not for a save), we check if the user choose a LDRAW or TDL format
				// to display a warning message, that he will lost data
				string filenameLower = this.saveFileDialog.FileName.ToLower();
				if (Properties.Settings.Default.DisplayWarningMessageForNotSavingInBBM && !filenameLower.EndsWith("bbm"))
				{
					// use a local variable to get the value of the checkbox, by default we don't suggest the user to hide it
					bool dontDisplayMessageAgain = false;

					// display the warning message
					result = ForgetableMessageBox.Show(this, Properties.Resources.ErrorMsgNotSavingInBBM,
									Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
									MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, ref dontDisplayMessageAgain);

					// set back the checkbox value in the settings (don't save the settings now, it will be done when exiting the application)
					Properties.Settings.Default.DisplayWarningMessageForNotSavingInBBM = !dontDisplayMessageAgain;

					// if the user doesn't want to continue, do not save and
					// do not add the name in the recent list file
					if (result == DialogResult.No)
						return false;
				}

				// change the current file name before calling the save
				changeCurrentMapFileName(this.saveFileDialog.FileName, true);
				// save the map
				saveDone = saveNamedMap();
				// update the recent file list with the new file saved (if correctly saved)
				if (saveDone)
					UpdateRecentFileMenuFromConfigFile(this.saveFileDialog.FileName, true);
			}
			// return the flag to know if the save was done
			return saveDone;
		}
		
		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveMap();
		}

		private void saveasToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveMapAs();
		}

		private void exportAsPictureToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// open the export form
			mExportImageForm.init();
			DialogResult result = mExportImageForm.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// if some export options (at least the first one) were validated, we need to update the view
				// to set the little "*" after the name of the file in the title bar, because the export options
				// have been saved in the map, therefore the map was modified.
				this.updateTitleBar();
			}
		}

		private void exportPartListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// open the save file dialog
			DialogResult result = this.exportPartListFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
				this.PartUsageListView.export(this.exportPartListFileDialog.FileName);
		}

		private void reloadPartLibraryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// try to unload the part library, if sucessful continue
			string mapFileNameToReload = null;
			if (unloadPartLibrary(out mapFileNameToReload))
			{
				// then display a waiting message box, giving the user the oppotunity to change the data before reloading
				MessageBox.Show(this, BlueBrick.Properties.Resources.ErrorMsgReadyToReloadPartLib,
					BlueBrick.Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

				// reload the part library when the user close the message box
				reloadPartLibrary(mapFileNameToReload);
			}
		}

		private bool unloadPartLibrary(out string mapFileNameToReload)
		{
			// by default no open file
			mapFileNameToReload = null;

			// we have first to undload the current file
			// check if the current map is not save and display a warning message
			// we also need to check for unsave budget because we will destroy the library
			if (checkForUnsavedMap() && checkForUnsavedBudget())
			{
				// save the name of the current map open to reload it (if it is valid)
				if (Map.Instance.IsMapNameValid)
					mapFileNameToReload = Map.Instance.MapFileName;

				// save the UI settings of the part lib before reloading it (and before clearing it), because the user may
				// have change some UI setting after the startup of the application, and now he wants to reload the part lib
				// and normally the settings are saved when you exit the application.
				savePartLibUISettingInDefaultSettings();

				// then clear the part lib panel and the brick library (before creating the new map)
				this.PartsTabControl.clearAllData();
				BrickLibrary.Instance.clearAllData();

				// destroy the current map
				reinitializeCurrentMap();

				// call the GC to be sure that all the image are correctly released, and no files stay locked
				// the garbage collector was called at then end of the reinitializeCurrentMap function called just above
				// GC.Collect();

				return true;
			}

			// part lib was not unloaded
			return false;
		}

		private void reloadPartLibrary(string mapFileNameToReload)
		{
			// then reload the library
			this.Cursor = Cursors.WaitCursor;
			loadPartLibraryFromDisk();
			this.Cursor = Cursors.Default;

			// Update the budget: most of the time the budget text for the item are correct (cause correctly set during creation)
			// however, the user may have rename a part just before reloading the part lib, 
			// so we need to update the budget and the view again if the budget was modified
			if (Budget.Budget.Instance.updatePartId())
			{
				// update the count and budget
				this.PartsTabControl.updateAllPartCountAndBudget();
				// update the part lib view filtering on budget (because the renamed items may appear/disappear)
				this.PartsTabControl.updateFilterOnBudgetedParts();
			}

			// finally reload the previous map or create a new one
			if (mapFileNameToReload != null)
			{
				openMap(mapFileNameToReload);
				// since the connexion position may have changed after the reload of the library,
				// maybe so 2 free connexions will become aligned, and can be connected, that's why
				// we perform a slow update connectivity in that case.
				foreach (Layer layer in Map.Instance.LayerList)
					if (layer is LayerBrick)
						(layer as LayerBrick).updateFullBrickConnectivity();
			}
			else
			{
				createNewMap();
			}
		}

		private void downloadAdditionnalPartsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first spawn the window to let the user choose the online source of the part library
			LibraryPackageSourceForm packageSourceForm = new LibraryPackageSourceForm();
			packageSourceForm.ShowDialog(this);

			// get the list of files that has been collected by the previous form dialog
			List<DownloadCenterForm.DownloadableFileInfo> filesToDownload = packageSourceForm.FilesToDownload;

			// check if we have something to download (if not just ignore, because the error message as already been displayed in the previous form)
			if (filesToDownload.Count > 0)
			{
				// open the download center form in dialog mode
				DownloadCenterForm downloadCenterForm = new DownloadCenterForm(filesToDownload, true);
				downloadCenterForm.ShowDialog(this);

				// get the list of files that has been succesfully downloaded
				List<DownloadCenterForm.DownloadableFileInfo> successfullyDownloadedFiles = downloadCenterForm.SuccessfullyDownloadedFiles;
				// when the user closed the dialog, check if any package was successfully download, and if yes, we need to reload the library
				if (successfullyDownloadedFiles.Count > 0)
				{
					// display a warning message and reload the library
					MessageBox.Show(this, BlueBrick.Properties.Resources.ErrorMsgNeedToReloadPartLib,
									BlueBrick.Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
									MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

					// after the user close the message box, unload, install and reload the library
					string mapFileNameToReload = null;
					if (unloadPartLibrary(out mapFileNameToReload))
					{
						// a string to get the part folder
						string partsFolder = Application.StartupPath + @"/parts";
						foreach (DownloadCenterForm.DownloadableFileInfo filePackageToInstall in successfullyDownloadedFiles)
						{
							try
							{
								// get the current folder of the library
								string currentPackageFolderName = partsFolder + @"/" + filePackageToInstall.FileName;
								currentPackageFolderName = currentPackageFolderName.Remove(currentPackageFolderName.Length - 4);
								// check if the package already exists, and delete it in that case.
								// give several chance to user in case this directory is locked
								DialogResult result = DialogResult.Retry;
								while ((result == DialogResult.Retry) && Directory.Exists(currentPackageFolderName))
								{
									try
									{
										Directory.Delete(currentPackageFolderName, true);
									}
									catch (IOException ioe)
									{
										// display a warning message and reload the library
										result = MessageBox.Show(this, BlueBrick.Properties.Resources.ErrorMsgExceptionWhenDeletingPartLib.Replace("&&", ioe.Message).Replace("&", filePackageToInstall.FileName),
														BlueBrick.Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.AbortRetryIgnore,
														MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
									}
								}
								// check if user aborted the library installation, in that case break the loop to stop the install
								// else if he ignore, then skip that package and continue with the other packages
								if (result == DialogResult.Abort)
									break;
								else if (result == DialogResult.Ignore)
									continue;
								// unzip the new archive
								string zipFileName = Application.StartupPath + filePackageToInstall.DestinationFolder;
								ZipFile.ExtractToDirectory(zipFileName, partsFolder);
								// then delete the archive
								File.Delete(zipFileName);
							}
							catch
							{
							}
						}

						// reload the part library when the user close the message box
						reloadPartLibrary(mapFileNameToReload);
					}
				}
			}
		}

		private void saveSelectionInLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
			// create a window for editing the option of the group and show it
			SaveGroupNameForm form = new SaveGroupNameForm();
			DialogResult result = form.ShowDialog(this);
			// check if we need to update the part lib
			if ((result == DialogResult.OK) && (form.NewXmlFilesToLoad.Count > 0))
				this.PartsTabControl.loadAdditionnalGroups(form.NewXmlFilesToLoad, form.NewGroupName);
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
			// check if the current map or budget is not save and display a warning message
			if (!checkForUnsavedMap() || !checkForUnsavedBudget())
			{
				// if the player cancel the closing then cancel the event
				e.Cancel = true;
			}

			// if the user didn't cancel the close, save the setting of the user,
			// in order to save the main form position, size and state
			if (!e.Cancel)
				saveUISettingInDefaultSettings();
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
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				// a cut is a copy followed by a delete
				selectedLayer.copyCurrentSelectionToClipboard();
				deleteToolStripMenuItem_Click(sender, e);
			}
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			// then call the funcion to copy the selection on the selected layer
			if (selectedLayer != null)
				selectedLayer.copyCurrentSelectionToClipboard();
		}

		private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			// call the paste method on the selected layer, and display an error if the paste was not possible
			string itemTypeName = "";
			Layer.AddActionInHistory addInHistory = Layer.AddActionInHistory.ADD_TO_HISTORY;
			if (!selectedLayer.pasteClipboardInLayer(Layer.AddOffsetAfterPaste.USE_SETTINGS_RULE, out itemTypeName, ref addInHistory))
			{
				// we have a type mismatch
				if (Properties.Settings.Default.DisplayWarningMessageForPastingOnWrongLayer)
				{
					// first replace the layer type name
					string message = Properties.Resources.ErrorMsgCanNotPaste.Replace("&&", selectedLayer.LocalizedTypeName);
					// then replace the item type name
					message = message.Replace("&", itemTypeName);
					// and display the message box
					bool dontDisplayMessageAgain = false;
					ForgetableMessageBox.Show(this, message, Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
									MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, ref dontDisplayMessageAgain);
					Properties.Settings.Default.DisplayWarningMessageForPastingOnWrongLayer = !dontDisplayMessageAgain;
				}
				else
				{
					System.Media.SystemSounds.Beep.Play();
				}
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				if (selectedLayer is LayerText)
					ActionManager.Instance.doAction(new DeleteText(selectedLayer as LayerText, selectedLayer.SelectedObjects));
				else if (selectedLayer is LayerBrick)
					ActionManager.Instance.doAction(new DeleteBrick(selectedLayer as LayerBrick, selectedLayer.SelectedObjects));
				else if (selectedLayer is LayerRuler)
					ActionManager.Instance.doAction(new DeleteRuler(selectedLayer as LayerRuler, selectedLayer.SelectedObjects));
			}
		}

		private void findAndReplaceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FindForm findForm = new FindForm();
			findForm.ShowDialog(this);
			this.mapPanel.Invalidate();
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
			if ((selectedLayer != null) && (selectedLayer is LayerBrick))
			{
				int selectedBrickCount = selectedLayer.SelectedObjects.Count;
				if (selectedBrickCount >= 2)
				{
					List<Layer.LayerItem> brickToSelect = MapData.Tools.AStar.findPath(selectedLayer.SelectedObjects[selectedBrickCount - 2] as LayerBrick.Brick, selectedLayer.SelectedObjects[selectedBrickCount - 1] as LayerBrick.Brick);
					// if AStar found a path, select the path
					if (brickToSelect.Count > 0)
					{
						selectedLayer.addObjectInSelection(brickToSelect);
						this.mapPanel.Invalidate();
					}
				}
			}
		}

        public void groupToolStripMenuItem_Click(object sender, EventArgs e)
        {
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0)
				&& ((selectedLayer is LayerBrick) || (selectedLayer is LayerText) || (selectedLayer is LayerRuler)))
            {
				Actions.Items.GroupItems groupAction = new BlueBrick.Actions.Items.GroupItems(selectedLayer.SelectedObjects, selectedLayer);
                Actions.ActionManager.Instance.doAction(groupAction);
            }
		}

        public void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0)
				&& ((selectedLayer is LayerBrick) || (selectedLayer is LayerText) || (selectedLayer is LayerRuler)))
			{
				Actions.Items.UngroupItems ungroupAction = new BlueBrick.Actions.Items.UngroupItems(selectedLayer.SelectedObjects, selectedLayer);
                Actions.ActionManager.Instance.doAction(ungroupAction);
            }
        }

		private void rotateCWToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				if (selectedLayer is LayerBrick)
					ActionManager.Instance.doAction(new RotateBrick(selectedLayer as LayerBrick, selectedLayer.SelectedObjects, 1));
				else if (selectedLayer is LayerText)
					ActionManager.Instance.doAction(new RotateText(selectedLayer as LayerText, selectedLayer.SelectedObjects, 1));
				else if (selectedLayer is LayerRuler)
					ActionManager.Instance.doAction(new RotateRulers(selectedLayer as LayerRuler, selectedLayer.SelectedObjects, 1));
			}
		}

		private void rotateCCWToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				if (selectedLayer is LayerBrick)
					ActionManager.Instance.doAction(new RotateBrick(selectedLayer as LayerBrick, selectedLayer.SelectedObjects, -1));
				else if (selectedLayer is LayerText)
					ActionManager.Instance.doAction(new RotateText(selectedLayer as LayerText, selectedLayer.SelectedObjects, -1));
				else if (selectedLayer is LayerRuler)
					ActionManager.Instance.doAction(new RotateRulers(selectedLayer as LayerRuler, selectedLayer.SelectedObjects, -1));
			}
		}

		public void sendToBackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				if (selectedLayer is LayerBrick)
					ActionManager.Instance.doAction(new SendBrickToBack(selectedLayer, selectedLayer.SelectedObjects));
				else if (selectedLayer is LayerText)
					ActionManager.Instance.doAction(new SendTextToBack(selectedLayer, selectedLayer.SelectedObjects));
				else if (selectedLayer is LayerRuler)
					ActionManager.Instance.doAction(new SendRulerToBack(selectedLayer, selectedLayer.SelectedObjects));
			}
		}

		public void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				if (selectedLayer is LayerBrick)
					ActionManager.Instance.doAction(new BringBrickToFront(selectedLayer, selectedLayer.SelectedObjects));
				else if (selectedLayer is LayerText)
					ActionManager.Instance.doAction(new BringTextToFront(selectedLayer, selectedLayer.SelectedObjects));
				else if (selectedLayer is LayerRuler)
					ActionManager.Instance.doAction(new BringRulerToFront(selectedLayer, selectedLayer.SelectedObjects));
			}
		}

		private void moveStepDisabledToolStripMenuItem_Click(object sender, EventArgs e)
		{
			enableSnapGridButton(false, Layer.CurrentSnapGridSize);
		}

		private void moveStep32ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// enable the toolbar and the menu item
			enableSnapGridButton(true, 32.0f);
		}

		private void moveStep16ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// enable the toolbar and the menu item
			enableSnapGridButton(true, 16.0f);
		}

		private void moveStep8ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// enable the toolbar and the menu item
			enableSnapGridButton(true, 8.0f);
		}

		private void moveStep4ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// enable the toolbar and the menu item
			enableSnapGridButton(true, 4.0f);
		}

		private void moveStep2ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// enable the toolbar and the menu item
			enableSnapGridButton(true, 2.0f);
		}

		private void moveStep1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// enable the toolbar and the menu item
			enableSnapGridButton(true, 1.0f);
		}

		private void moveStep05ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// enable the toolbar and the menu item
			enableSnapGridButton(true, 0.5f);
		}

		/// <summary>
		/// Update the checkmark in front of the correct rotation step in the menu and the toolbard
		/// according to the specified angle value
		/// </summary>
		/// <param name="angle">the new angle chosen</param>
		private void updateRotationStepButton(float angle)
		{
			// toolbar and menu
			this.toolBarAngle90Button.Checked = this.rotationStep90ToolStripMenuItem.Checked = (angle == 90.0f);
			this.toolBarAngle45Button.Checked = this.rotationStep45ToolStripMenuItem.Checked = (angle == 45.0f);
			this.toolBarAngle22Button.Checked = this.rotationStep22ToolStripMenuItem.Checked = (angle == 22.5f);
			this.toolBarAngle11Button.Checked = this.rotationStep11ToolStripMenuItem.Checked = (angle == 11.25f);
			this.toolBarAngle5Button.Checked = this.rotationStep5ToolStripMenuItem.Checked = (angle == 5.625f);
			this.toolBarAngle1Button.Checked = this.rotationStep1ToolStripMenuItem.Checked = (angle == 1.0f);
			//layer
			Layer.CurrentRotationStep = angle;
		}

		private void rotationStep90ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateRotationStepButton(90.0f);
		}

		private void rotationStep45ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateRotationStepButton(45.0f);
		}

		private void rotationStep22ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateRotationStepButton(22.5f);
		}

		private void rotationStep11ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateRotationStepButton(11.25f);
		}

		private void rotationStep5ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateRotationStepButton(5.625f);
		}

		private void rotationStep1ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			updateRotationStepButton(1.0f);
		}

		private void paintToolPaintToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarToolButton.Image = mPaintIcon;
			// menu
			this.paintToolPaintToolStripMenuItem.Checked = true;
			this.paintToolEraseToolStripMenuItem.Checked = false;
			// set the current static paint color in the area layer
			LayerArea.CurrentDrawColor = mCurrentPaintIconColor;
		}

		private void paintToolEraseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarToolButton.Image = this.eraseToolStripMenuItem.Image;
			// menu
			this.paintToolPaintToolStripMenuItem.Checked = false;
			this.paintToolEraseToolStripMenuItem.Checked = true;
			// set the current static paint color in the area layer
			LayerArea.CurrentDrawColor = Color.Empty;
		}

		public void rulerSelectAndEditToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarToolButton.Image = this.rulerSelectAndEditToolStripMenuItem.Image;
			// menu
			this.selectAndEditToolStripMenuItem.Checked = true;
			this.addRulerToolStripMenuItem.Checked = false;
			this.addCircleToolStripMenuItem.Checked = false;
			// set the selected tool in the static vaqr of the ruler layer
			LayerRuler.CurrentEditTool = LayerRuler.EditTool.SELECT;
		}

		private void rulerAddRulerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarToolButton.Image = this.rulerAddRulerToolStripMenuItem.Image;
			// menu
			this.selectAndEditToolStripMenuItem.Checked = false;
			this.addRulerToolStripMenuItem.Checked = true;
			this.addCircleToolStripMenuItem.Checked = false;
			// set the selected tool in the static vaqr of the ruler layer
			LayerRuler.CurrentEditTool = LayerRuler.EditTool.LINE;
		}

		private void rulerAddCircleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// toolbar
			this.toolBarToolButton.Image = this.rulerAddCircleToolStripMenuItem.Image;
			// menu
			this.selectAndEditToolStripMenuItem.Checked = false;
			this.addRulerToolStripMenuItem.Checked = false;
			this.addCircleToolStripMenuItem.Checked = true;
			// set the selected tool in the static vaqr of the ruler layer
			LayerRuler.CurrentEditTool = LayerRuler.EditTool.CIRCLE;
		}

		private void generatePaintIcon(Color color)
		{
			// assign the current paint color with the specified parameter
			mCurrentPaintIconColor = color;
			// get the background color from the specified color
			// but cheat a little if the user choose the magenta color, because it is the transparent
			// color of the original bitmap
			Color backColor = color;
			if ((backColor == Color.Magenta) || (backColor == Color.Fuchsia))
				backColor = Color.FromArgb(unchecked((int)0xFFFF00FE));
			// recreate the icon and use the color of the color dialog for the background
			mPaintIcon = new Bitmap(16, 16);
			Graphics g = Graphics.FromImage(mPaintIcon);
			g.Clear(backColor);
			g.DrawImage(this.paintToolStripMenuItem.Image, 0, 0);
			g.Flush();
			// refresh the icon
			this.toolBarToolButton.Image = mPaintIcon;
			// set the current static paint color in the area layer
			LayerArea.CurrentDrawColor = color;
		}

		private void paintToolChooseColorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.colorDialog.Color = mCurrentPaintIconColor;
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// regenerate the icon
				generatePaintIcon(this.colorDialog.Color);
				// and reselect the paint tool
				paintToolPaintToolStripMenuItem_Click(sender, e);
			}
		}

		/// <summary>
		/// This function can be called via two different places, either via the menu "Edit > Map Background Color"
		/// of via the button in the Properties tab. Both way do the same thing.
		/// </summary>
		private void openColorPickerToChangeMapBackgroundColor()
		{
			DialogResult result = this.colorDialog.ShowDialog(this);
			if (result == DialogResult.OK)
				ActionManager.Instance.doAction(new ChangeBackgroundColor(this.colorDialog.Color));
		}

		private void mapBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			openColorPickerToChangeMapBackgroundColor();
		}

		private void currentLayerOptionsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// first get the current selected layer
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if (selectedLayer != null)
			{
				if (selectedLayer is LayerGrid)
				{
					LayerGridOptionForm optionForm = new LayerGridOptionForm(selectedLayer as LayerGrid);
					optionForm.ShowDialog(this);
				}
				else if (selectedLayer is LayerBrick)
				{
					LayerBrickOptionForm optionForm = new LayerBrickOptionForm(selectedLayer as LayerBrick);
					optionForm.ShowDialog(this);
				}
				else if (selectedLayer is LayerText)
				{
					LayerTextOptionForm optionForm = new LayerTextOptionForm(selectedLayer);
					optionForm.ShowDialog(this);
				}
				else if (selectedLayer is LayerArea)
				{
					LayerAreaOptionForm optionForm = new LayerAreaOptionForm(selectedLayer);
					optionForm.ShowDialog(this);
				}
				else if (selectedLayer is LayerRuler)
				{
					LayerTextOptionForm optionForm = new LayerTextOptionForm(selectedLayer);
					optionForm.ShowDialog(this);
				}
			}
		}

		private void preferencesMenuItem_Click(object sender, EventArgs e)
		{
			PreferencesForm preferenceForm = new PreferencesForm();
			DialogResult result = preferenceForm.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// reinit the array of shortcut
				initShortcutKeyArrayFromSettings();
				// update the gamma for the layer bricks because they may have changed (before redrawing the map)
				Map.Instance.updateGammaFromSettings();
				// redraw the map because the color scheme may have changed
				this.mapPanel.Invalidate();
				// also redraw the undo stack
				updateUndoRedoMenuItems();
			}
			// update the the recent file list anyway because the user may have click the
			// clear recent file list button before clicking cancel
			UpdateRecentFileMenuFromConfigFile();

			// before checking if we need to restart, check if the language package is correctly installed
			LanguageManager.checkLanguage(BlueBrick.Properties.Settings.Default.Language);

			// check if we need to restart, if yes, ask the user what he wants to do
			if (preferenceForm.DoesNeedToRestart)
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
			this.mapPanel.CurrentStatusBarHeight = this.statusBar.Visible ? this.statusBar.Height : 0;
			this.mapPanel.Invalidate();
		}

		public void mapScrollBarsVisibilityChangeNotification(bool newVisibility)
		{
			this.mapScrollBarsToolStripMenuItem.Checked = newVisibility;
		}

		private void mapScrollBarsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.mapPanel.ShowHideScrollBars(mapScrollBarsToolStripMenuItem.Checked);
			this.mapPanel.Invalidate();
		}

		private void watermarkToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.DisplayGeneralInfoWatermark = this.watermarkToolStripMenuItem.Checked;
			this.mapPanel.Invalidate();
		}

		private void electricCircuitsMenuItem_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.DisplayElectricCircuit = this.electricCircuitsMenuItem.Checked;
			this.mapPanel.Invalidate();
		}

		private void connectionPointsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.DisplayFreeConnexionPoints = this.connectionPointsToolStripMenuItem.Checked;
			this.mapPanel.Invalidate();
		}

		private void rulerAttachPointsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.DisplayRulerAttachPoints = this.rulerAttachPointsToolStripMenuItem.Checked;
			this.mapPanel.Invalidate();
		}
		#endregion

		#region Budget Menu
		/// <summary>
		/// Enable or disable the budget menu item, depending if there's a current budget existing
		/// </summary>
		/// <param name="isEnabled"></param>
		private void updateEnableStatusForBudgetMenuItem()
		{
			bool isEnabled = Budget.Budget.Instance.IsExisting;
			this.budgetImportAndMergeToolStripMenuItem.Enabled = isEnabled;
			this.budgetCloseToolStripMenuItem.Enabled = isEnabled;
			this.budgetSaveToolStripMenuItem.Enabled = isEnabled;
			this.budgetSaveAsToolStripMenuItem.Enabled = isEnabled;
			this.showOnlyBudgetedPartsToolStripMenuItem.Enabled = isEnabled;
			this.showBudgetNumbersToolStripMenuItem.Enabled = isEnabled;
			this.useBudgetLimitationToolStripMenuItem.Enabled = isEnabled;
		}

		/// <summary>
		/// This method check if the current budget is not saved and prompt a message box asking
		/// what to do (save, do not save or cancel). This method sould be called before
		/// exiting the application, and before creating or loading a new budget
		/// <returns>true if the action can continue, false if user choose to cancel</returns>
		/// </summary>
		private bool checkForUnsavedBudget()
		{
			if (Budget.Budget.Instance.WasModified)
			{
				// if the user can cancel the application close, give him 3 buttons yes/no/cancel,
				// else give him only 2 buttons yes/no:
				DialogResult result = MessageBox.Show(this,
					BlueBrick.Properties.Resources.ErrorMsgBudgetWasModified,
					BlueBrick.Properties.Resources.ErrorMsgTitleWarning,
					mCanUserCancelTheApplicationClose ? MessageBoxButtons.YesNoCancel : MessageBoxButtons.YesNo,
					MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

				if (result == DialogResult.Yes)
				{
					// call the save method (that maybe will perform a save as)
					budgetSaveToolStripMenuItem_Click(null, null);
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

		/// <summary>
		/// Open the specified bugdet and eventually merge it with the other specified budget
		/// </summary>
		/// <param name="filename">the budget file to open</param>
		/// <param name="budgetToMerge">the optionnal budget to merge with or null</param>
		/// <returns>true if the budget was correctly open</returns>
		private bool openBudget(string filename, Budget.Budget budgetToMerge)
		{
			// set the wait cursor
			this.Cursor = Cursors.WaitCursor;
			// load the file
			bool isFileValid = SaveLoadManager.load(filename);
			if (isFileValid)
			{
				// check if we need to merge a budget in the new opened budget or not
				if (budgetToMerge != null)
				{
					Budget.Budget.Instance.mergeWith(budgetToMerge);
					// give back the title of the original budget to the loaded budget
					changeCurrentBudgetFileName(budgetToMerge.BudgetFileName, true);
				}
				else
				{
					// change the filename in the title bar
					changeCurrentBudgetFileName(filename, true);
				}
				// recount the parts because, opening a new budget actually create a new instance of Budget, so the count is destroyed
				Budget.Budget.Instance.recountAllBricks();
				this.PartUsageListView.rebuildList();
				// update the filtering on of the part lib after recounting all the bricks
				this.PartsTabControl.updateFilterOnBudgetedParts();
			}
			// update the menu items
			updateEnableStatusForBudgetMenuItem();
			// then update the view of the part lib
			this.PartsTabControl.updateViewStyle();
			// update the budgets in the part usage view
			this.PartUsageListView.updateBudgetNotification();
			// restore the cursor after loading
			this.Cursor = Cursors.Default;
			// return if the file was correctly loaded
			return isFileValid;
		}

		private void changeCurrentBudgetFileName(string filename, bool isAValidName)
		{
			// save the filename
			Budget.Budget.Instance.BudgetFileName = filename;
			Budget.Budget.Instance.IsFileNameValid = isAValidName;
			// update the name of the title bar with the name of the file
			updateTitleBar();
		}

		private void budgetNewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (checkForUnsavedBudget())
			{
				// create a new budget
				Budget.Budget.Instance.create();
				// update the part lib view
				this.PartsTabControl.updateFilterOnBudgetedParts();
				// update the title bar
				this.updateTitleBar();
				// update the menu items
				updateEnableStatusForBudgetMenuItem();
				// then update the view of the part lib
				this.PartsTabControl.updateViewStyle();
				// update the budgets in the part usage view
				this.PartUsageListView.updateBudgetNotification();
			}
		}

		private void budgetOpenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// check if the current budget is not save and display a warning message
			if (checkForUnsavedBudget())
			{
				DialogResult result = this.openBudgetFileDialog.ShowDialog(this);
				if (result == DialogResult.OK)
					openBudget(this.openBudgetFileDialog.FileName, null);
			}
		}

		private void budgetImportAndMergeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// we don't check for unsaved budget, cause we will merge the current budget with the one we open
			// save the current budget instance, cause the loading of new budget will erase it
			Budget.Budget currentBudget = Budget.Budget.Instance;
			// open the new one
			DialogResult result = this.openBudgetFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
				openBudget(this.openBudgetFileDialog.FileName, currentBudget);
		}

		private void budgetCloseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (checkForUnsavedBudget())
			{
				// destroy the budget
				Budget.Budget.Instance.destroy();
				// update the part lib view
				this.PartsTabControl.updateFilterOnBudgetedParts();
				// update the title bar (to remove the budget name from the title bar)
				this.updateTitleBar();
				// update the menu items
				updateEnableStatusForBudgetMenuItem();
				// then update the view of the part lib
				this.PartsTabControl.updateViewStyle();
				// update the budgets in the part usage view
				this.PartUsageListView.updateBudgetNotification();
			}
		}

		private void saveBudget()
		{
			// set the wait cursor
			this.Cursor = Cursors.WaitCursor;
			// save the file
			bool saveDone = SaveLoadManager.save(Budget.Budget.Instance.BudgetFileName);
			if (saveDone)
			{
				// after saving the budget, we reset the WasModified flag of the budget
				// (and before the update of the title bar)
				Budget.Budget.Instance.WasModified = false;
				// update the title bar
				this.updateTitleBar();
			}
			// restore the cursor
			this.Cursor = Cursors.Default;
		}

		private void budgetSaveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// if the current file name is not defined we do a "save as..."
			if (Budget.Budget.Instance.IsFileNameValid)
				saveBudget();
			else
				budgetSaveAsToolStripMenuItem_Click(sender, e);
		}

		private void budgetSaveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// put the current file name in the dialog (which can be the default one)
			this.saveBudgetFileDialog.FileName = Budget.Budget.Instance.BudgetFileName;
			// if there's no initial directory, choose the My Documents directory
			this.saveBudgetFileDialog.InitialDirectory = Path.GetDirectoryName(Budget.Budget.Instance.BudgetFileName);
			if (this.saveBudgetFileDialog.InitialDirectory.Length == 0)
				this.saveBudgetFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			// open the save as dialog
			DialogResult result = this.saveBudgetFileDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// change the current file name before calling the save
				changeCurrentBudgetFileName(this.saveBudgetFileDialog.FileName, true);
				// save the map
				saveBudget();
			}
		}

		public void showOnlyBudgetedPartsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// update the setting
			Properties.Settings.Default.ShowOnlyBudgetedParts = !Properties.Settings.Default.ShowOnlyBudgetedParts;
			// then udpate the check state according to the new setting
			this.showOnlyBudgetedPartsToolStripMenuItem.Checked = Properties.Settings.Default.ShowOnlyBudgetedParts;
			// update the filtering on budget of the part lib
			this.PartsTabControl.updateFilterOnBudgetedParts();
		}

		public void showBudgetNumbersToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// update the setting
			Properties.Settings.Default.ShowBudgetNumbers = !Properties.Settings.Default.ShowBudgetNumbers;
			// then udpate the check state according to the new setting
			this.showBudgetNumbersToolStripMenuItem.Checked = Properties.Settings.Default.ShowBudgetNumbers;
			// then update the view of the part lib
			this.PartsTabControl.updateViewStyle();
		}

		public void useBudgetLimitationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// update the setting
			Properties.Settings.Default.UseBudgetLimitation = !Properties.Settings.Default.UseBudgetLimitation;
			// then udpate the check state according to the new setting
			this.useBudgetLimitationToolStripMenuItem.Checked = Properties.Settings.Default.UseBudgetLimitation;
			// now check if we need to also show the budget numbers
			if (Properties.Settings.Default.DisplayWarningMessageForShowingBudgetNumbers &&
				Properties.Settings.Default.UseBudgetLimitation && !this.showBudgetNumbersToolStripMenuItem.Checked)
			{
				bool dontDisplayMessageAgain = false;
				DialogResult result = ForgetableMessageBox.Show(this, Properties.Resources.ErrorMsgDoYouWantToDisplayBudgetNumber,
									Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNo,
									MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, ref dontDisplayMessageAgain);
				// set back the checkbox value in the settings (don't save the settings now, it will be done when exiting the application)
				Properties.Settings.Default.DisplayWarningMessageForShowingBudgetNumbers = !dontDisplayMessageAgain;
				// check the result, if yes, click also on display the budget numbers
				if (result == DialogResult.Yes)
					showBudgetNumbersToolStripMenuItem_Click(null, null);
			}
		}
		#endregion

		#region Help Menu
		private void helpContentsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// construct the path of the help file from the current language
			FileInfo helpFileInfo = new FileInfo(Application.StartupPath + @"/" + Properties.Settings.Default.Language + @"/BlueBrick.chm");

			// If the help file related with the current language of the application does not exist,
			// load the default one which is in the same folder as the application
			if (!helpFileInfo.Exists)
				helpFileInfo = new FileInfo(Application.StartupPath + @"/BlueBrick.chm");

			// now check if we can open the help file, else display a warning message
			if (helpFileInfo.Exists)
			{
                // MONOBUG: The Help.ShowHelp is not implemented yet on Mono, do our own implementation for Linux or Mac
                if ((Environment.OSVersion.Platform == PlatformID.Unix) ||
                    (Environment.OSVersion.Platform == PlatformID.MacOSX))
                {
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = helpFileInfo.FullName;
                    System.Diagnostics.Process.Start(startInfo);
                }
                else
                {
                    Help.ShowHelp(this, helpFileInfo.FullName);
                }
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
			aboutBox.ShowDialog(this);
		}
		#endregion

		#endregion

		#region event handler for toolbar

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

		private void toolBarSnapGridButton_Click(object sender, EventArgs e)
		{
			if (this.toolBarSnapGridButton.ButtonPressed)
			{
				enableSnapGridButton(!this.toolBarSnapGridButton.DropDown.Enabled, Layer.CurrentSnapGridSize);
			}
		}

		private void toolBarPaintButton_ButtonClick(object sender, EventArgs e)
		{
			// nothing happen by default, but if the paint icon is selected this call the color picker
			if (this.toolBarToolButton.Image == mPaintIcon)
				paintToolChooseColorToolStripMenuItem_Click(sender, e);
		}
		#endregion

		#region event handler for part lib
		private void addInputFilterIndication()
		{
			// set the color first cause we will check it in the TextChange event
			this.textBoxPartFilter.ForeColor = Color.Gray;
			this.textBoxPartFilter.Text = Properties.Resources.InputFilterIndication;
		}

		private void removeInputFilterIndication()
		{
			// set the color first cause we will check it in the TextChange event
			this.textBoxPartFilter.ForeColor = Color.Black;
			this.textBoxPartFilter.Text = string.Empty;
		}

		private bool isThereAnyUserFilterSentence()
		{
			// we use the color of the text box filter, because the user may type a sentence
			// which could be exactly as the input filter indication (i.e. the hint)
			return (this.textBoxPartFilter.ForeColor == Color.Black);
		}

		private string getUserFilterSentence()
		{
			return (isThereAnyUserFilterSentence() ? this.textBoxPartFilter.Text : string.Empty);
		}

		private void textBoxPartFilter_TextChanged(object sender, EventArgs e)
		{
			// do not call the filtering if the box is disabled
			if (isThereAnyUserFilterSentence())
			{
				// checked which filter method to call
				if (this.filterAllTabCheckBox.Checked)
					this.PartsTabControl.filterAllTabs(this.textBoxPartFilter.Text);
				else
					this.PartsTabControl.filterCurrentTab(this.textBoxPartFilter.Text);
			}
		}

		private void textBoxPartFilter_Enter(object sender, EventArgs e)
		{
			// if the user enter the text box without any keyword set, delete the hint
			if (!isThereAnyUserFilterSentence())
				removeInputFilterIndication();
		}

		private void textBoxPartFilter_Leave(object sender, EventArgs e)
		{
			// if the user deleted the whole text when leaving the box, add the incitation text
			if (this.textBoxPartFilter.Text == string.Empty)
				addInputFilterIndication();
		}

		private void textBoxPartFilter_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			// capture all the keys in the text box, such as CTRL+C or CTRL+A to avoid these
			// key to be executed at the map level (the MainForm level) and avoid modification
			// of the map (copy/paste, etc..) while the filter box is focused
			e.IsInputKey = true;
		}

		private void textBoxPartFilter_KeyDown(object sender, KeyEventArgs e)
		{
			// it seems the CTRL+A is not handled by default by the text box control??
			if ((e.Control) && (e.KeyCode == Keys.A))
			{
				textBoxPartFilter.SelectAll();
				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void filterAllTabCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// get the filter sentence that can be empty if the user didn't set one
			string filterSentence = getUserFilterSentence();

			// change the icon of the button according to the button state
			if (filterAllTabCheckBox.Checked)
			{
				filterAllTabCheckBox.ImageIndex = 0;
				this.PartsTabControl.filterAllTabs(filterSentence);
			}
			else
			{
				filterAllTabCheckBox.ImageIndex = 1;
				// after refiltering all tabs with their own filter, filter the current tab
				// with the current filter text of the combo box. because we want this behavior:
				// 1) select tab A, filter with "A"
				// 2) select tab B, filter with "B"
				// 3) select tab C, filter with "C"
				// 3) hit the filter all checkbox: now all tabs are filtered with "C"
				// 4) select tab A (which is filetered with "C" as expected, with the filter text set to "C")
				// 5) uncheck the filter all
				// 6) we want to keep the filtering of A with "C" (and tab B is with "B" and tab C with "C")
				// another possible behavior would be to update the filter text with the filter sentence of the
				// current tab: in that case, the user would see the text change when he uncheck the filter all checkbox
				// so actually we give in parameter the filter sentence to use for the current tab
				this.PartsTabControl.unfilterAllTabs(filterSentence);
			}
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

		private void newLayerGridButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerGrid", true));
		}

		private void newLayerBrickButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerBrick", true));
		}

		private void newLayerAreaButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerArea", true));
		}

		private void newLayerTextButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerText", true));
		}

		private void newLayerRulerButton_Click(object sender, EventArgs e)
		{
			ActionManager.Instance.doAction(new AddLayer("LayerRuler", true));
		}

		private void trashLayerButton_Click(object sender, EventArgs e)
		{
			if (Map.Instance.SelectedLayer != null)
				ActionManager.Instance.doAction(new RemoveLayer(Map.Instance.SelectedLayer));
		}

		#endregion

		#region event handler for part usage list tab
		private void SplitPartUsagePerLayerCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// change the status of the split status
			this.PartUsageListView.SplitPartPerLayer = SplitPartUsagePerLayerCheckBox.Checked;
		}

		private void IncludeHiddenLayerInPartListCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// change the status of the hidden status
			this.PartUsageListView.IncludeHiddenLayers = IncludeHiddenLayerInPartListCheckBox.Checked;
		}
		#endregion

		#region event handler for properties tab
		private ChangeGeneralInfo mGeneralInfoStateWhenEnteringFocus = null;

		private void DocumentDataTabControl_Selected(object sender, TabControlEventArgs e)
		{
			// update the map dimension if we just selected the properties tab
			if (e.TabPage == this.DocumentDataPropertiesTabPage)
				updateMapDimensionInfo();
		}

		private void DocumentDataPropertiesMapBackgroundColorButton_Click(object sender, EventArgs e)
		{
			openColorPickerToChangeMapBackgroundColor();
		}

		private ChangeGeneralInfo getGeneralInfoActionFromUI()
		{
			return new ChangeGeneralInfo(this.AuthorTextBox.Text, this.lugComboBox.Text,
					this.eventComboBox.Text, this.dateTimePicker.Value, this.commentTextBox.Text);
		}

		private void memorizeGeneralInfoActionWhenEnteringFocus()
		{
			mGeneralInfoStateWhenEnteringFocus = getGeneralInfoActionFromUI();
		}

		private void doChangeGeneralInfoActionIfSomethingChangedInUI()
		{
			// create a new action from UI
			ChangeGeneralInfo newAction = getGeneralInfoActionFromUI();
			// check if the new action is diffent than we enter the focus, add it to the action manager
			if (!newAction.Equals(mGeneralInfoStateWhenEnteringFocus))
				ActionManager.Instance.doAction(newAction);
			}

		private void AuthorTextBox_Enter(object sender, EventArgs e)
		{
			memorizeGeneralInfoActionWhenEnteringFocus();
		}

		private void AuthorTextBox_Leave(object sender, EventArgs e)
		{
			doChangeGeneralInfoActionIfSomethingChangedInUI();
		}

		private void AuthorTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				SendKeys.Send("{TAB}");
		}

		private void lugComboBox_Enter(object sender, EventArgs e)
		{
			memorizeGeneralInfoActionWhenEnteringFocus();
		}

		private void lugComboBox_Leave(object sender, EventArgs e)
		{
			doChangeGeneralInfoActionIfSomethingChangedInUI();
		}

		private void lugComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				SendKeys.Send("{TAB}");
		}

		private void eventComboBox_Enter(object sender, EventArgs e)
		{
			memorizeGeneralInfoActionWhenEnteringFocus();
		}

		private void eventComboBox_Leave(object sender, EventArgs e)
		{
			doChangeGeneralInfoActionIfSomethingChangedInUI();
		}

		private void eventComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				SendKeys.Send("{TAB}");
		}

		private void dateTimePicker_ValueChanged(object sender, EventArgs e)
		{
			doChangeGeneralInfoActionIfSomethingChangedInUI();
		}

		private void commentTextBox_Enter(object sender, EventArgs e)
		{
			memorizeGeneralInfoActionWhenEnteringFocus();
		}

		private void commentTextBox_Leave(object sender, EventArgs e)
		{
			doChangeGeneralInfoActionIfSomethingChangedInUI();
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
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				List<Layer.LayerItem> itemList = selectedLayer.SelectedObjects;

				// create the action according to the layer type
                Actions.Action moveAction = null;
				if (selectedLayer is LayerBrick)
					moveAction = new MoveBrick(selectedLayer as LayerBrick, itemList, move);
				else if (selectedLayer is LayerText)
					moveAction = new MoveText(selectedLayer as LayerText, itemList, move);
				else if (selectedLayer is LayerRuler)
					moveAction = new MoveRulers(selectedLayer as LayerRuler, itemList, move);

				// if we found a compatible layer
				if (moveAction != null)
				{
					if (isRealMove)
					{
						// undo the action
						moveAction.undo();
						// then add it to the undo stack (that will perform the redo)
						ActionManager.Instance.doAction(moveAction);
					}
					else
					{
						// do a move action without puting it in the undo stack
						moveAction.redo();
					}
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
			Layer selectedLayer = Map.Instance.SelectedLayer;
			if ((selectedLayer != null) && (selectedLayer.SelectedObjects.Count > 0))
			{
				List<Layer.LayerItem> itemList = Map.Instance.SelectedLayer.SelectedObjects;

				if (selectedLayer is LayerBrick)
				{
					if (isRealMove)
					{
						// create the opposite action and do it, to cancel all the incremental moves
						// we can not create the normal action and undo it because the rotation of connected
						// brick is not symetrical (because the rotation step is not constant)
						RotateBrick unrotateAction = new RotateBrick(selectedLayer as LayerBrick, itemList, -angleStep, true);
						unrotateAction.redo();
						// So create a new move action to add in the undo stack
						ActionManager.Instance.doAction(new RotateBrick(selectedLayer as LayerBrick, itemList, angleStep, true));
					}
					else
					{
						// do a move action without puting it in the undo stack
						RotateBrick rotateAction = new RotateBrick(selectedLayer as LayerBrick, itemList, angleStep, ((angleStep != -1) && (angleStep != 1)));
						rotateAction.redo();
					}
				}
				else if (selectedLayer is LayerText)
				{
					if (isRealMove)
					{
						// create the rotation action
						RotateText rotateAction = new RotateText(selectedLayer as LayerText, itemList, angleStep, true);
						// undo the total rotate of all the objects
						rotateAction.undo();
						// then add it to the undo stack (that will perform the redo)
						ActionManager.Instance.doAction(rotateAction);
					}
					else
					{
						// do a move action without puting it in the undo stack
						RotateText rotateAction = new RotateText(selectedLayer as LayerText, itemList, angleStep, ((angleStep != -1) && (angleStep != 1)));
						rotateAction.redo();
					}
				}
				else if (selectedLayer is LayerRuler)
				{
					if (isRealMove)
					{
						// create the rotation action
						RotateRulers rotateAction = new RotateRulers(selectedLayer as LayerRuler, itemList, angleStep, true);
						// undo the total rotate of all the objects
						rotateAction.undo();
						// then add it to the undo stack (that will perform the redo)
						ActionManager.Instance.doAction(rotateAction);
					}
					else
					{
						// do a move action without puting it in the undo stack
						RotateRulers rotateAction = new RotateRulers(selectedLayer as LayerRuler, itemList, angleStep, ((angleStep != -1) && (angleStep != 1)));
						rotateAction.redo();
					}
				}
			}
		}
		#endregion

		#region event handler for drag and drop file
		public void MainForm_DragEnter(object sender, DragEventArgs e)
		{
			// by default do not accept the drop
			e.Effect = DragDropEffects.None;

			// we accept the drop if the data is a file name
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				// check if the first file has a valid suported extension
				string[] filenames = (string[])(e.Data.GetData(DataFormats.FileDrop));
				if (filenames.Length > 0)
				{
					// authorize the drop if it's a file with the good extension
					if (canOpenThisFile(filenames[0]))
						e.Effect = DragDropEffects.Copy;
				}
			}
		}

		public void MainForm_DragDrop(object sender, DragEventArgs e)
		{
			string[] filenames = (string[])(e.Data.GetData(DataFormats.FileDrop));
			if (filenames.Length > 0)
			{
				// check if the current map is not save and display a warning message
				if (checkForUnsavedMap())
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

		private bool isUserTypingTextInATextBox()
		{
			return this.textBoxPartFilter.Focused || this.PartsTabControl.IsEditingBudget ||
					this.AuthorTextBox.Focused || this.lugComboBox.Focused || this.eventComboBox.Focused || this.commentTextBox.Focused;
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			// by default we don't handle the keys
			e.Handled = false;

			// if we are inputing text in the filter box do not handle the key
			if (isUserTypingTextInATextBox())
				return;

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
								{
									LayerBrick brickLayer = Map.Instance.SelectedLayer as LayerBrick;
									if (brickLayer != null)
									{
										// add a connected brick, or if there's no connectable brick, add a brick in the origin
										if (brickLayer.getConnectableBrick() != null)
											Map.Instance.addConnectBrick(shortcut.mPartName, shortcut.mConnexion);
										else
											Map.Instance.addBrick(shortcut.mPartName);
									}
									break;
								}
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
								{
									LayerBrick brickLayer = Map.Instance.SelectedLayer as LayerBrick;
									if (brickLayer != null)
									{
										LayerBrick.Brick selectedBrick = brickLayer.getConnectableBrick();
										if (selectedBrick != null)
										{
											selectedBrick.setActiveConnectionPointWithNextOne(true);
											this.mapPanel.updateView();
										}
									}
									break;
								}
							case shortcutableAction.SEND_TO_BACK:
								// shortcut to the event handler of the menu
								sendToBackToolStripMenuItem_Click(sender, e);
								break;
							case shortcutableAction.BRING_TO_FRONT:
								// shortcut to the event handler of the menu
								bringToFrontToolStripMenuItem_Click(sender, e);
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

			// if we are inputing text in the filter box do not handle the key
			if (isUserTypingTextInATextBox())
				return;

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
		public string getDraggingPartNumberInPartLib()
		{
			return this.PartsTabControl.DraggingPartNumber;
		}

		public void resetDraggingPartNumberInPartLib()
		{
			this.PartsTabControl.DraggingPartNumber = null;
		}
		#endregion
	}
}