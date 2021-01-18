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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlueBrick.MapData;
using BlueBrick.Properties;
using BlueBrick.Actions;

namespace BlueBrick
{
	/// <summary>
	/// A customized Panel that draw the map.
	/// </summary>
	/// <remarks>
	/// <para>The pannel draw elements of several layers.</para>
	/// </remarks>
	public class MapPanel : Panel
	{
		#region Fields

		/// <summary>
		/// This small internal enum is used to avoid duplcation of code in the mouse event handler
		/// because, you can actually perform one action with different key combination, so the event
		/// handler can first decide which action to do before writing the code that execute it
		/// </summary>
		private enum ActionToDoInMouseEvent
		{
			NONE,
			SCROLL_VIEW,
			ZOOM_VIEW
		};

		// scroll position
		private bool mIsScrolling = false;
		private Point mLastScrollMousePos = new Point();
		// initial zoom position, if not using the wheel
		private bool mIsZooming = false;
		private Point mFirstZoomMousePos = new Point();
		private Point mLastZoomMousePos = new Point();
		// last position if no button is pressed
		private Point mLastMousePos = new Point();
		// last position when a button is down
		private PointF mLastDownMouseCoordInStud = new Point();
		// the coordinate in STUD of the center of the view (that means which part of the map the center of the view is currently targeting)
		private double mViewCornerX = 0;
		private double mViewCornerY = 0;

		// selection in rectangle
		private bool mIsSelectionRectangleOn = false;
		private bool mIsMouseHandledByMap = false;
		private Point mSelectionRectangleInitialPosition = new Point();
		private Rectangle mSelectionRectangle = new Rectangle();
		private Pen mSelectionRectanglePen = new Pen(Color.Black, 2);

        // adjust the map area size if the status bar or scrollbar are displayed
		private int mCurrentStatusBarHeight = 0;

		//dragndrop of a part on the map
		private Layer.LayerItem mCurrentPartDrop = null;
		private LayerBrick mBrickLayerThatReceivePartDrop = null;
		private ContextMenuStrip contextMenuStrip;
		private System.ComponentModel.IContainer components;
		private ToolStripMenuItem bringToFrontToolStripMenuItem;
		private ToolStripMenuItem sendToBackToolStripMenuItem;
		private ToolStripSeparator selectToolStripSeparator;
		private ToolStripMenuItem deselectAllToolStripMenuItem;
		private ToolStripMenuItem selectPathToolStripMenuItem;
		private ToolStripMenuItem selectAllToolStripMenuItem;
		private ToolStripSeparator groupToolStripSeparator;
		private ToolStripMenuItem groupToolStripMenuItem;
		private ToolStripMenuItem ungroupToolStripMenuItem;
		private ToolStripSeparator attachRulerToolStripSeparator;
		private ToolStripMenuItem attachToolStripMenuItem;
		private ToolStripMenuItem detachToolStripMenuItem;
		private ToolStripMenuItem useAsModelToolStripMenuItem;
		private ToolStripSeparator propertiesToolStripSeparator;
		private ToolStripMenuItem propertiesToolStripMenuItem;
		private ToolStripSeparator appearanceToolStripSeparator;
		private ToolStripMenuItem scrollBarToolStripMenuItem;

		// scale
		private double mViewScale = 4.0;

		// a parameter to adjust the scrollbar resolution. This is the slider size but also define the resolution of the whole scrollbar
		const int mMapScrollBarSliderSize = 100;
		private HScrollBar horizontalScrollBar;
		private VScrollBar verticalScrollBar;

		// a const margin added to the total area surface, in order to compute the scrollbar maximums
		const int mScrollBarAddedMarginInStud = 32;

		// for optimization reason we want to memorize the total area, and update only when some opeartion is done on the map (item added, removed or moved)
		private RectangleF mMapTotalAreaInStud = new RectangleF(0, 0, 32, 32);
		#endregion

		#region Get / Set

		/// <summary>
		/// The current scale of the map view.
		/// </summary>
		public double ViewScale
		{
			get { return mViewScale; }
			set
			{
				double oldValue = mViewScale;
				// avoid setting a null scale or negative scale, set only a scale in the range
				if (value < 0.2)
					mViewScale = 0.2;
				else if (value > 16.0)
					mViewScale = 16.0;
				else
					mViewScale = value;
				// compute the difference of scale and call the notification on the map
				if (mViewScale != oldValue)
				{
					// call map notification
					Map.Instance.zoomScaleChangeNotification(oldValue, mViewScale);
					// update the scrollbars
					updateScrollbarSize();
					// invalidate the panel, since we must redraw it to handle the new scale
					Invalidate();
				}
			}
		}

		public int CurrentStatusBarHeight
		{
			set { mCurrentStatusBarHeight = value; }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Construct a new MapPanel.
		/// </summary>
		public MapPanel()
		{
			// Note that we add our own scroll bar, otherwise they appear below the staus bar, and also the panel try to scroll them automatically based on the component on the panel (which are none)
			InitializeComponent();
			// Default values
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
			mSelectionRectanglePen.DashPattern = new float[] { 3.0f, 2.0f };
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapPanel));
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.bringToFrontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.sendToBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deselectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.groupToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.groupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ungroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.attachRulerToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.attachToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.detachToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.useAsModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.propertiesToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.appearanceToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.scrollBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.horizontalScrollBar = new System.Windows.Forms.HScrollBar();
			this.verticalScrollBar = new System.Windows.Forms.VScrollBar();
			this.contextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStrip
			// 
			resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bringToFrontToolStripMenuItem,
            this.sendToBackToolStripMenuItem,
            this.selectToolStripSeparator,
            this.selectAllToolStripMenuItem,
            this.deselectAllToolStripMenuItem,
            this.selectPathToolStripMenuItem,
            this.groupToolStripSeparator,
            this.groupToolStripMenuItem,
            this.ungroupToolStripMenuItem,
            this.attachRulerToolStripSeparator,
            this.attachToolStripMenuItem,
            this.detachToolStripMenuItem,
            this.useAsModelToolStripMenuItem,
            this.propertiesToolStripSeparator,
            this.propertiesToolStripMenuItem,
            this.appearanceToolStripSeparator,
            this.scrollBarToolStripMenuItem});
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// bringToFrontToolStripMenuItem
			// 
			resources.ApplyResources(this.bringToFrontToolStripMenuItem, "bringToFrontToolStripMenuItem");
			this.bringToFrontToolStripMenuItem.Name = "bringToFrontToolStripMenuItem";
			this.bringToFrontToolStripMenuItem.Click += new System.EventHandler(this.bringToFrontToolStripMenuItem_Click);
			// 
			// sendToBackToolStripMenuItem
			// 
			resources.ApplyResources(this.sendToBackToolStripMenuItem, "sendToBackToolStripMenuItem");
			this.sendToBackToolStripMenuItem.Name = "sendToBackToolStripMenuItem";
			this.sendToBackToolStripMenuItem.Click += new System.EventHandler(this.sendToBackToolStripMenuItem_Click);
			// 
			// selectToolStripSeparator
			// 
			resources.ApplyResources(this.selectToolStripSeparator, "selectToolStripSeparator");
			this.selectToolStripSeparator.Name = "selectToolStripSeparator";
			// 
			// selectAllToolStripMenuItem
			// 
			resources.ApplyResources(this.selectAllToolStripMenuItem, "selectAllToolStripMenuItem");
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// deselectAllToolStripMenuItem
			// 
			resources.ApplyResources(this.deselectAllToolStripMenuItem, "deselectAllToolStripMenuItem");
			this.deselectAllToolStripMenuItem.Name = "deselectAllToolStripMenuItem";
			this.deselectAllToolStripMenuItem.Click += new System.EventHandler(this.deselectAllToolStripMenuItem_Click);
			// 
			// selectPathToolStripMenuItem
			// 
			resources.ApplyResources(this.selectPathToolStripMenuItem, "selectPathToolStripMenuItem");
			this.selectPathToolStripMenuItem.Name = "selectPathToolStripMenuItem";
			this.selectPathToolStripMenuItem.Click += new System.EventHandler(this.selectPathToolStripMenuItem_Click);
			// 
			// groupToolStripSeparator
			// 
			resources.ApplyResources(this.groupToolStripSeparator, "groupToolStripSeparator");
			this.groupToolStripSeparator.Name = "groupToolStripSeparator";
			// 
			// groupToolStripMenuItem
			// 
			resources.ApplyResources(this.groupToolStripMenuItem, "groupToolStripMenuItem");
			this.groupToolStripMenuItem.Name = "groupToolStripMenuItem";
			this.groupToolStripMenuItem.Click += new System.EventHandler(this.groupToolStripMenuItem_Click);
			// 
			// ungroupToolStripMenuItem
			// 
			resources.ApplyResources(this.ungroupToolStripMenuItem, "ungroupToolStripMenuItem");
			this.ungroupToolStripMenuItem.Name = "ungroupToolStripMenuItem";
			this.ungroupToolStripMenuItem.Click += new System.EventHandler(this.ungroupToolStripMenuItem_Click);
			// 
			// attachRulerToolStripSeparator
			// 
			resources.ApplyResources(this.attachRulerToolStripSeparator, "attachRulerToolStripSeparator");
			this.attachRulerToolStripSeparator.Name = "attachRulerToolStripSeparator";
			// 
			// attachToolStripMenuItem
			// 
			resources.ApplyResources(this.attachToolStripMenuItem, "attachToolStripMenuItem");
			this.attachToolStripMenuItem.Name = "attachToolStripMenuItem";
			this.attachToolStripMenuItem.Click += new System.EventHandler(this.attachToolStripMenuItem_Click);
			// 
			// detachToolStripMenuItem
			// 
			resources.ApplyResources(this.detachToolStripMenuItem, "detachToolStripMenuItem");
			this.detachToolStripMenuItem.Name = "detachToolStripMenuItem";
			this.detachToolStripMenuItem.Click += new System.EventHandler(this.detachToolStripMenuItem_Click);
			// 
			// useAsModelToolStripMenuItem
			// 
			resources.ApplyResources(this.useAsModelToolStripMenuItem, "useAsModelToolStripMenuItem");
			this.useAsModelToolStripMenuItem.Name = "useAsModelToolStripMenuItem";
			this.useAsModelToolStripMenuItem.Click += new System.EventHandler(this.useAsModelToolStripMenuItem_Click);
			// 
			// propertiesToolStripSeparator
			// 
			resources.ApplyResources(this.propertiesToolStripSeparator, "propertiesToolStripSeparator");
			this.propertiesToolStripSeparator.Name = "propertiesToolStripSeparator";
			// 
			// propertiesToolStripMenuItem
			// 
			resources.ApplyResources(this.propertiesToolStripMenuItem, "propertiesToolStripMenuItem");
			this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
			this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
			// 
			// appearanceToolStripSeparator
			// 
			resources.ApplyResources(this.appearanceToolStripSeparator, "appearanceToolStripSeparator");
			this.appearanceToolStripSeparator.Name = "appearanceToolStripSeparator";
			// 
			// scrollBarToolStripMenuItem
			// 
			resources.ApplyResources(this.scrollBarToolStripMenuItem, "scrollBarToolStripMenuItem");
			this.scrollBarToolStripMenuItem.CheckOnClick = true;
			this.scrollBarToolStripMenuItem.Name = "scrollBarToolStripMenuItem";
			this.scrollBarToolStripMenuItem.Click += new System.EventHandler(this.scrollBarToolStripMenuItem_Click);
			// 
			// horizontalScrollBar
			// 
			resources.ApplyResources(this.horizontalScrollBar, "horizontalScrollBar");
			this.horizontalScrollBar.Name = "horizontalScrollBar";
			this.horizontalScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.horizontalScrollBar_Scroll);
			this.horizontalScrollBar.MouseEnter += new System.EventHandler(this.horizontalScrollBar_MouseEnter);
			// 
			// verticalScrollBar
			// 
			resources.ApplyResources(this.verticalScrollBar, "verticalScrollBar");
			this.verticalScrollBar.Name = "verticalScrollBar";
			this.verticalScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.verticalScrollBar_Scroll);
			this.verticalScrollBar.MouseEnter += new System.EventHandler(this.verticalScrollBar_MouseEnter);
			// 
			// MapPanel
			// 
			resources.ApplyResources(this, "$this");
			this.AllowDrop = true;
			this.ContextMenuStrip = this.contextMenuStrip;
			this.Controls.Add(this.horizontalScrollBar);
			this.Controls.Add(this.verticalScrollBar);
			this.SizeChanged += new System.EventHandler(this.MapPanel_SizeChanged);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MapPanel_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MapPanel_DragEnter);
			this.DragOver += new System.Windows.Forms.DragEventHandler(this.MapPanel_DragOver);
			this.DragLeave += new System.EventHandler(this.MapPanel_DragLeave);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapPanel_MouseDown);
			this.MouseEnter += new System.EventHandler(this.MapPanel_MouseEnter);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapPanel_MouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MapPanel_MouseUp);
			this.Resize += new System.EventHandler(this.MapPanel_Resize);
			this.contextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		public void updateView()
		{
			// nothing special except invalidate the view
			Invalidate();
		}

		/// <summary>
		/// This method is inheritated and is usefull to get the event when the arrow are pressed
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool IsInputKey(Keys keyData)
		{
			// we need the four arrow keys
			// also page up and down for rotation
			// and delete and backspace for deleting object
			if ((keyData == Keys.Left) || (keyData == Keys.Right) ||
				(keyData == Keys.Up) || (keyData == Keys.Down) ||
				(keyData == Keys.PageDown) || (keyData == Keys.PageUp) ||
				(keyData == Keys.Home) || (keyData == Keys.End) ||
				(keyData == Keys.Insert) || (keyData == Keys.Delete) ||
				(keyData == Keys.Enter) || (keyData == Keys.Return) ||
				(keyData == Keys.Escape) || (keyData == Keys.Back))
				return true;

			return base.IsInputKey(keyData);
		}

		#endregion

		#region Display methods

		/// <summary>
		/// Overridden Paint method to perform a custom draw of the map view.
		/// </summary>
		/// <param name="e">Event argument.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;

//for debug FPS			DateTime time = DateTime.Now;

			// set the setting of the graphic
			g.CompositingMode = CompositingMode.SourceOver;
			g.SmoothingMode = SmoothingMode.None;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			g.InterpolationMode = InterpolationMode.Low;
            g.PixelOffsetMode = PixelOffsetMode.None;

			// NOTE: the background color is set directly in this.BackColor !!!
			this.BackColor = Map.Instance.BackgroundColor;

			// compute the eventual height of the scrollbar if visible
			int currentScrollbarHeight = this.horizontalScrollBar.Visible ? this.horizontalScrollBar.Height : 0;

			// call the draw of the map
			float widthInStud = (float)(this.Size.Width / mViewScale);
            float heightInStud = (float)((this.Size.Height - mCurrentStatusBarHeight - currentScrollbarHeight) / mViewScale);			
			float startXInStud = (float)mViewCornerX;
			float startYInStud = (float)mViewCornerY;
			RectangleF rectangle = new RectangleF(startXInStud, startYInStud, widthInStud, heightInStud);
			Map.Instance.draw(g, rectangle, mViewScale, true);
			Map.Instance.drawWatermark(g, rectangle, mViewScale);

			// on top of all the layer draw the selection rectangle
			if (mIsSelectionRectangleOn)
			{
				// avoid drawing an empty rectangle
				int width = mSelectionRectangle.Width;
				if (width == 0)
					width = 1;
				int height = mSelectionRectangle.Height;
				if (height == 0)
					height = 1;
				// draw the rectangle
				g.DrawRectangle(mSelectionRectanglePen, mSelectionRectangle.X, mSelectionRectangle.Y, width, height);
			}

//for debug FPS			TimeSpan delta = DateTime.Now - time;
//for debug FPS			g.DrawString(delta.Ticks.ToString(), new Font(FontFamily.GenericMonospace,12), Brushes.Black, 0, 0);
		}

		public void moveViewToMapCenter()
		{
			float halfViewWidthInStud = (float)(this.Size.Width / (mViewScale * 2));
			float halfViewHeightInStud = (float)(this.Size.Height / (mViewScale * 2));
			// get the total area of the map
			mMapTotalAreaInStud = Map.Instance.getTotalAreaInStud(false);
			mViewCornerX = ((mMapTotalAreaInStud.Left + mMapTotalAreaInStud.Right) / 2) - halfViewWidthInStud;
			mViewCornerY = ((mMapTotalAreaInStud.Top + mMapTotalAreaInStud.Bottom) / 2) - halfViewHeightInStud;
			// and update the scroll bar as we changed the view corner, and also the whole map may have changed
			updateScrollbarSize();
		}
		#endregion
	
		#region Mouse event

		/// <summary>
		/// Set the default Cursor on the map according to the current selected layer
		/// </summary>
		private Cursor getDefaultCursor(PointF mouseCoordInStud)
		{
			if (Map.Instance.SelectedLayer != null)
				return Map.Instance.SelectedLayer.getDefaultCursorWithoutMouseClick(mouseCoordInStud);
			return Cursors.Default;
		}

		public void setDefaultCursor()
		{
			this.Cursor = getDefaultCursor(getPointCoordInStud(this.PointToClient(Cursor.Position)));
		}

		private PointF getPointCoordInStud(Point pointCoordInPixel)
		{
			PointF pointCoordInStud = new PointF();
			pointCoordInStud.X = (float)(mViewCornerX + (pointCoordInPixel.X / mViewScale));
			pointCoordInStud.Y = (float)(mViewCornerY + (pointCoordInPixel.Y / mViewScale));
			return pointCoordInStud;
		}

		private PointF getMouseCoordInStud(MouseEventArgs e)
		{
			return getPointCoordInStud(e.Location);
		}

		private PointF getScreenPointInStud(Point pointInScreenCoord)
		{
			return getPointCoordInStud(pointInScreenCoord);
		}

		private void MapPanel_MouseDown(object sender, MouseEventArgs e)
		{
			ActionToDoInMouseEvent actionToDo = ActionToDoInMouseEvent.NONE;
			bool mustRefreshView = false;

			// take the focus anyway if we clik in the map view
			this.Focus();

			// the cursor to set according to the action
			mLastDownMouseCoordInStud = getMouseCoordInStud(e);
			Cursor preferedCursor = getDefaultCursor(mLastDownMouseCoordInStud);

			// then dispatch the event
			switch (e.Button)
			{
				case MouseButtons.Left:
					if (Control.ModifierKeys == Settings.Default.MouseZoomPanKey)
					{
						// this is the pan with the keys, not the wheel
						actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					}
					else if (Map.Instance.handleMouseDown(e, mLastDownMouseCoordInStud, ref preferedCursor))
					{
						// left button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.mouseDown(e, mLastDownMouseCoordInStud);
						mIsMouseHandledByMap = true;
					}
					else if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.Visible))
					{
						// if the selected layer is not visible don't even start a selection or double click
						if (e.Clicks == 1)
						{
							//simple click not handle by the layer, so we start a selection rectangle
							mSelectionRectangleInitialPosition = new Point(e.Location.X, e.Location.Y);
							mSelectionRectangle = new Rectangle(e.Location.X, e.Location.Y, 0, 0);
							mIsSelectionRectangleOn = true;
							mustRefreshView = true;
						}
						else
						{
							// this is a double click
							mIsSelectionRectangleOn = false;
						}
					}
					break;

				case MouseButtons.Middle:
					// middle button is used to scroll
					actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					break;

				case MouseButtons.Right:
					if (Control.ModifierKeys == Settings.Default.MouseZoomPanKey)
					{
						actionToDo = ActionToDoInMouseEvent.ZOOM_VIEW;
					}
					else if (Map.Instance.handleMouseDown(e, mLastDownMouseCoordInStud, ref preferedCursor))
					{
						// right button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.mouseDown(e, mLastDownMouseCoordInStud);
						mIsMouseHandledByMap = true;
					}

					break;
			}

			switch (actionToDo)
			{
				case ActionToDoInMouseEvent.SCROLL_VIEW:
					mIsScrolling = true;
					mLastScrollMousePos = e.Location;
					preferedCursor = BlueBrick.MainForm.Instance.PanViewCursor;
					break;

				case ActionToDoInMouseEvent.ZOOM_VIEW:
					mIsZooming = true;
					mFirstZoomMousePos = e.Location;
					mLastZoomMousePos = e.Location;
					preferedCursor = BlueBrick.MainForm.Instance.ZoomCursor;
					break;
			}

			// set the cursor with the preference
			this.Cursor = preferedCursor;

			// check if we need to update the view
			if (mustRefreshView)
				updateView();
		}

		private void MapPanel_MouseMove(object sender, MouseEventArgs e)
		{
			ActionToDoInMouseEvent actionToDo = ActionToDoInMouseEvent.NONE;
			bool mustRefreshView = false;
			PointF mouseCoordInStud = getMouseCoordInStud(e);
			string statusBarMessage = "(" + mouseCoordInStud.X.ToString("F1") + " / " + mouseCoordInStud.Y.ToString("F1") + ") ";
			Cursor preferedCursor = this.Cursor;

			switch (e.Button)
			{
				case MouseButtons.Left:
					// take the focus anyway that way, we can receive an event mouseup
					// and this simulate a dragndrop in the map
					this.Focus();

					// check if we are using a selecion rectangle
					if (mIsSelectionRectangleOn)
					{
						mSelectionRectangle.X = Math.Min(e.X, mSelectionRectangleInitialPosition.X);
						mSelectionRectangle.Y = Math.Min(e.Y, mSelectionRectangleInitialPosition.Y);
						mSelectionRectangle.Width = Math.Abs(e.X - mSelectionRectangleInitialPosition.X);
						mSelectionRectangle.Height = Math.Abs(e.Y - mSelectionRectangleInitialPosition.Y);
						this.Cursor = Cursors.Cross;
						mustRefreshView = true;
					}
					else if (mIsMouseHandledByMap)
					{
						// left button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.mouseMove(e, mouseCoordInStud, ref preferedCursor);
						this.Cursor = preferedCursor;
					}
					else if (mIsZooming)
					{
						actionToDo = ActionToDoInMouseEvent.ZOOM_VIEW;
					}
					else if (mIsScrolling)
					{
						actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					}
					break;

				case MouseButtons.Middle:
					// middle button is used to scroll
					actionToDo = ActionToDoInMouseEvent.SCROLL_VIEW;
					break;

				case MouseButtons.Right:
					if (mIsMouseHandledByMap)
					{
						// left button is handle by layers (so give it to the map)
						mustRefreshView = Map.Instance.mouseMove(e, mouseCoordInStud, ref preferedCursor);
						this.Cursor = preferedCursor;
					}
					else if (mIsZooming)
					{
						actionToDo = ActionToDoInMouseEvent.ZOOM_VIEW;
					}
					break;

				case MouseButtons.None:
					// if the map also want to handle this free move, call it
					preferedCursor = getDefaultCursor(mouseCoordInStud);
					mIsMouseHandledByMap = Map.Instance.handleMouseMoveWithoutClick(e, mouseCoordInStud, ref preferedCursor);
					if (mIsMouseHandledByMap)
						mustRefreshView = Map.Instance.mouseMove(e, mouseCoordInStud, ref preferedCursor);
					// set the cursor with the preference
					this.Cursor = preferedCursor;

					// nothing to do if we didn't move
					if ((mLastMousePos.X != e.X) || (mLastMousePos.Y != e.Y))
					{
						// if there's a brick under the mouse, and the player don't use a button,
						// display the description of the brick in the status bar
						LayerBrick brickLayer = Map.Instance.SelectedLayer as LayerBrick;
						if (brickLayer != null)
						{
							LayerBrick.Brick brickUnderMouse = brickLayer.getBrickUnderMouse(mouseCoordInStud);
							if (brickUnderMouse != null)
								statusBarMessage += BrickLibrary.Instance.getFormatedBrickInfo(brickUnderMouse.PartNumber, true, true, true);
						}
						else
						{
							LayerText textLayer = Map.Instance.SelectedLayer as LayerText;
							if (textLayer != null)
							{
								LayerText.TextCell textUnderMouse = textLayer.getTextCellUnderMouse(mouseCoordInStud);
								if (textUnderMouse != null)
								{
									statusBarMessage += textUnderMouse.Text.Replace("\r", "");
									statusBarMessage = statusBarMessage.Replace('\n', ' ');
									if (statusBarMessage.Length > 80)
										statusBarMessage = statusBarMessage.Substring(0, 80) + "...";
								}
							}
						}
					}
					break;
			}

			switch (actionToDo)
			{
				case ActionToDoInMouseEvent.SCROLL_VIEW:
					pan((mLastScrollMousePos.X - e.X) / mViewScale, (mLastScrollMousePos.Y - e.Y) / mViewScale);
					mLastScrollMousePos.X = e.X;
					mLastScrollMousePos.Y = e.Y;
					// update the view continuously
					mustRefreshView = true;
					break;

				case ActionToDoInMouseEvent.ZOOM_VIEW:
					int yDiff = e.Y - mLastZoomMousePos.Y;
					mLastZoomMousePos = e.Location;
					zoom((float)(1.0f + (yDiff * 4.0f * Settings.Default.WheelMouseZoomSpeed)),
							Settings.Default.WheelMouseIsZoomOnCursor, mFirstZoomMousePos);
					// the zoom function already update the view, don't need to set the bool flag here
					break;
			}

			//display the message in the status bar
			if ((mLastMousePos.X != e.X) || (mLastMousePos.Y != e.Y))
				MainForm.Instance.setStatusBarMessage(statusBarMessage);

			// save the last mouse position anyway
			mLastMousePos = e.Location;

			// check if we need to update the view
			if (mustRefreshView)
				updateView();
		}

		private void MapPanel_MouseUp(object sender, MouseEventArgs e)
		{
			bool mustRefreshView = false;
			PointF mouseCoordInStud = getMouseCoordInStud(e);

			// check if we are using a selecion rectangle
			if (mIsSelectionRectangleOn)
			{
				mSelectionRectangle.X = Math.Min(e.X, mSelectionRectangleInitialPosition.X);
				mSelectionRectangle.Y = Math.Min(e.Y, mSelectionRectangleInitialPosition.Y);
				mSelectionRectangle.Width = Math.Abs(e.X - mSelectionRectangleInitialPosition.X);
				mSelectionRectangle.Height = Math.Abs(e.Y - mSelectionRectangleInitialPosition.Y);

				// compute the new selection rectangle in stud and call the selection on the map
				PointF topLeftCorner = getScreenPointInStud(mSelectionRectangle.Location);
				PointF BottomRightCorner = getScreenPointInStud(new Point(mSelectionRectangle.Right, mSelectionRectangle.Bottom));
				RectangleF selectionRectangeInStud = new RectangleF(topLeftCorner.X, topLeftCorner.Y, BottomRightCorner.X - topLeftCorner.X, BottomRightCorner.Y - topLeftCorner.Y);
				Map.Instance.selectInRectangle(selectionRectangeInStud);

				// delete the selection rectangle
				mIsSelectionRectangleOn = false;
				// refresh the view
				mustRefreshView = true;
			}
			else if (mIsMouseHandledByMap)
			{
				// left button is handle by layers (so give it to the map)
				mustRefreshView = Map.Instance.mouseUp(e, mouseCoordInStud);
				mIsMouseHandledByMap = false;
			}
			else if (mIsZooming)
			{
				// the zoom is finished
				mIsZooming = false;
				// no need to update the view
			}
			else if (mIsScrolling)
			{
				// the scroll is finished
				mIsScrolling = false;
				// update the view at the end of the scroll
				mustRefreshView = true;
			}

			// restore the default cursor
			this.Cursor = getDefaultCursor(mouseCoordInStud);

			// check if we need to update the view
			if (mustRefreshView)
				updateView();
		}

		private void MapPanel_DragEnter(object sender, DragEventArgs e)
		{
			// if the number of click is null, that means it can be a dragndrop from another view, such as the part lib
			// check if we need to search the image dropped, or if we already have it
			if (mCurrentPartDrop == null)
			{
				// by default do not accept the drop
				e.Effect = DragDropEffects.None;

				// ask the main window if one part was selected in the part lib
				// because the getData from the event doesn't work well under mono, normally it should be: e.Data.GetData(DataFormats.SystemString) as string
				string partDropNumber = (this.TopLevelControl as MainForm).getDraggingPartNumberInPartLib();

				// check if we can add it
				Map.BrickAddability canAdd = Map.Instance.canAddBrick(partDropNumber);
				if (canAdd == Map.BrickAddability.YES)
				{
					mBrickLayerThatReceivePartDrop = Map.Instance.SelectedLayer as LayerBrick;
					if (partDropNumber != null && mBrickLayerThatReceivePartDrop != null)
					{
						if (BrickLibrary.Instance.isAGroup(partDropNumber))
							mCurrentPartDrop = new Layer.Group(partDropNumber);
						else
							mCurrentPartDrop = new LayerBrick.Brick(partDropNumber);
						mBrickLayerThatReceivePartDrop.addTemporaryPartDrop(mCurrentPartDrop);
						// set the effect in order to get the drop event
						e.Effect = DragDropEffects.Copy;
					}
				}
				else
					Map.Instance.giveFeedbackForNotAddingBrick(canAdd);
			}

			// check again if we are not dragging a part, maybe we drag a file
			if (mCurrentPartDrop == null)
				(this.TopLevelControl as MainForm).MainForm_DragEnter(sender, e);
		}

		private void MapPanel_DragOver(object sender, DragEventArgs e)
		{
			// check if we are currently dragging a part
			if ((mCurrentPartDrop != null) && (mBrickLayerThatReceivePartDrop != null))
			{
				// memorise the position of the mouse snapped to the grid
				PointF mouseCoordInStud = getScreenPointInStud(this.PointToClient(new Point(e.X, e.Y)));
				mCurrentPartDrop.Center = mBrickLayerThatReceivePartDrop.getMovedSnapPoint(mouseCoordInStud, mCurrentPartDrop);
				mBrickLayerThatReceivePartDrop.updateBoundingSelectionRectangle();
				// refresh the view
				updateView();
			}
		}

		private void MapPanel_DragDrop(object sender, DragEventArgs e)
		{
			if (mCurrentPartDrop != null)
			{
				// we have finished a dragndrop, remove the temporary part
				if (mBrickLayerThatReceivePartDrop != null)
				{
					mBrickLayerThatReceivePartDrop.removeTemporaryPartDrop(mCurrentPartDrop);
					mBrickLayerThatReceivePartDrop = null;
				}
				// and add the real new part
				Map.Instance.addBrick(mCurrentPartDrop);
				// reset the dropping part number here and there
				mCurrentPartDrop = null;
				(this.TopLevelControl as MainForm).resetDraggingPartNumberInPartLib();
				// refresh the view
				updateView();
				// and give the focus to the map panel such as if the user use the wheel to zoom
				// just after after the drop, the zoom is performed instead of the part lib scrolling
				this.Focus();
			}
			else
			{
				// if it is not a string, call the drag enter of the main app
				(this.TopLevelControl as MainForm).MainForm_DragDrop(sender, e);
			}
		}

		private void MapPanel_DragLeave(object sender, EventArgs e)
		{
			// if the user leave the panel while is was dropping a part,
			// just cancel the drop
			if (mCurrentPartDrop != null)
			{
				// remove and destroy the part
				if (mBrickLayerThatReceivePartDrop != null)
				{
					mBrickLayerThatReceivePartDrop.removeTemporaryPartDrop(mCurrentPartDrop);
					mBrickLayerThatReceivePartDrop = null;
				}
				mCurrentPartDrop = null;
				// update the view
				updateView();
			}
		}

		private void MapPanel_MouseEnter(object sender, EventArgs e)
		{
			// set the default cursor
			this.Cursor = getDefaultCursor(PointF.Empty);
			// focus on the panel for handling the mouse scroll
			this.Focus();
		}

		public void MapPanel_MouseWheel(object sender, MouseEventArgs e)
		{
			// use the zoom cursor
			this.Cursor = MainForm.Instance.ZoomCursor;
			// and call the zoom function
			zoom((float)(1.0f + (e.Delta * Settings.Default.WheelMouseZoomSpeed)),
				Settings.Default.WheelMouseIsZoomOnCursor, e.Location);
		}

		private void pan(double deltaX, double deltaY)
		{
			// pan the view according to the delta
			mViewCornerX += deltaX;
			mViewCornerY += deltaY;
			// and update the scrollbar position
			UpdateScrollBarThumbFromViewCorner(true, true);
		}

		private void zoom(float zoomFactor, bool zoomOnMousePosition, Point mousePosition)
		{
			// compute the center of the view in case of we need to zoom in the center
			Point screenCenterInPixel = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);

			// if the zoom must be performed on the mouse (or on the center of the screen),
			// we need to compute the point under the mouse (or under the center of the
			// screen) in stud coord system
			PointF previousZoomPointInStud;
			if (zoomOnMousePosition)
				previousZoomPointInStud = getPointCoordInStud(mousePosition);
			else
				previousZoomPointInStud = getPointCoordInStud(screenCenterInPixel);

			// compute the delta of the zoom according to the setting and
			// set the new scale by using the accessor to clamp and refresh the view
			ViewScale = (mViewScale * zoomFactor);

			// recompute the new zoom point in the same way because the scaled changed
			// but the zoom point (mouse coord or center) in pixel didn't changed
			PointF newZoomPointInStud;
			if (zoomOnMousePosition)
				newZoomPointInStud = getPointCoordInStud(mousePosition);
			else
				newZoomPointInStud = getPointCoordInStud(screenCenterInPixel);

			// compute how much we should scroll the view to keep the same
			// point in stud under the mouse coord on screen
			pan(previousZoomPointInStud.X - newZoomPointInStud.X, previousZoomPointInStud.Y - newZoomPointInStud.Y);
			updateView();
		}
		#endregion

		#region right click context menu
		private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// if the zoom modifier key is pressed, cancel the opening of the context menu
			if (mIsMouseHandledByMap || mIsZooming || (Control.ModifierKeys == Settings.Default.MouseZoomPanKey))
			{
				e.Cancel = true;
			}
			else
			{
				Layer selectedLayer = Map.Instance.SelectedLayer;
				bool isThereAVisibleSelectedLayer = ((selectedLayer != null) && (selectedLayer.Visible));
				bool enableItemRelatedToSelection = (isThereAVisibleSelectedLayer && (selectedLayer.SelectedObjects.Count > 0));
				this.bringToFrontToolStripMenuItem.Enabled = enableItemRelatedToSelection;
				this.sendToBackToolStripMenuItem.Enabled = enableItemRelatedToSelection;
				this.selectAllToolStripMenuItem.Enabled = (isThereAVisibleSelectedLayer && (selectedLayer.HasSomethingToSelect));
				this.deselectAllToolStripMenuItem.Enabled = enableItemRelatedToSelection;
				this.selectPathToolStripMenuItem.Visible = (selectedLayer is LayerBrick);
				this.selectPathToolStripMenuItem.Enabled = (isThereAVisibleSelectedLayer && (selectedLayer.SelectedObjects.Count >= 2));
				if (isThereAVisibleSelectedLayer)
				{
					this.groupToolStripMenuItem.Enabled = Actions.Items.GroupItems.findItemsToGroup(selectedLayer.SelectedObjects).Count > 1;
					this.ungroupToolStripMenuItem.Enabled = Actions.Items.UngroupItems.findItemsToUngroup(selectedLayer.SelectedObjects).Count > 0;
				}
				else
				{
					this.groupToolStripMenuItem.Enabled = false;
					this.ungroupToolStripMenuItem.Enabled = false;
				}
				// check if the current layer is of type ruler
				bool isSelectedLayerRuler = isThereAVisibleSelectedLayer && (selectedLayer is LayerRuler);
				bool isSelectedLayerText = isThereAVisibleSelectedLayer && (selectedLayer is LayerText);
				this.attachRulerToolStripSeparator.Visible = isSelectedLayerRuler;
				this.attachToolStripMenuItem.Visible = isSelectedLayerRuler;
				this.detachToolStripMenuItem.Visible = isSelectedLayerRuler;
				this.useAsModelToolStripMenuItem.Visible = isSelectedLayerRuler || isSelectedLayerText;
				this.useAsModelToolStripMenuItem.Enabled = (isSelectedLayerRuler || isSelectedLayerText) && (selectedLayer.SelectedObjects.Count == 1);
				if (isSelectedLayerRuler)
				{
					LayerRuler rulerLayer = selectedLayer as LayerRuler;
					this.attachToolStripMenuItem.Enabled = rulerLayer.canAttachRuler();
					this.detachToolStripMenuItem.Enabled = rulerLayer.canDetachRuler();
				}
				else
				{
					this.attachToolStripMenuItem.Enabled = false;
					this.detachToolStripMenuItem.Enabled = false;
				}

				// check is we need to enable the properties
				this.propertiesToolStripMenuItem.Enabled = enableItemRelatedToSelection && ((selectedLayer is LayerRuler) || (selectedLayer is LayerText) || (selectedLayer is LayerBrick));

				// update the check mark of the scrollbar depending on the current state of the scrollbar
				this.scrollBarToolStripMenuItem.Checked = this.horizontalScrollBar.Visible || this.verticalScrollBar.Visible;

				// finally after enabling the context menu items
				// check if at leat one toolstrip menu item is enabled otherwise, cancel the opening
				bool isEnabled = false;
				foreach (ToolStripItem stripItem in this.ContextMenuStrip.Items)
					if (stripItem is ToolStripMenuItem)
						isEnabled = isEnabled || stripItem.Enabled;
				// do we cancel it? yes if none is enabled
				e.Cancel = !isEnabled;
			}
		}

		private void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.bringToFrontToolStripMenuItem_Click(sender, e);
		}

		private void sendToBackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.sendToBackToolStripMenuItem_Click(sender, e);
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.selectAllToolStripMenuItem_Click(sender, e);
		}

		private void deselectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.deselectAllToolStripMenuItem_Click(sender, e);
		}

		private void selectPathToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.selectPathToolStripMenuItem_Click(sender, e);
		}

		private void groupToolStripMenuItem_Click(object sender, EventArgs e)
		{
            MainForm.Instance.groupToolStripMenuItem_Click(sender, e);
		}

		private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
		{
            MainForm.Instance.ungroupToolStripMenuItem_Click(sender, e);
		}

		private void attachToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LayerRuler rulerLayer = Map.Instance.SelectedLayer as LayerRuler;
			if (rulerLayer != null)
				ActionManager.Instance.doAction(new Actions.Rulers.AttachRulerToBrick(rulerLayer.CurrentRulerWithHighlightedControlPoint, rulerLayer.CurrentBrickUsedForRulerAttachement));
		}

		private void detachToolStripMenuItem_Click(object sender, EventArgs e)
		{
			LayerRuler rulerLayer = Map.Instance.SelectedLayer as LayerRuler;
			if (rulerLayer!= null)
				ActionManager.Instance.doAction(new Actions.Rulers.DetachRuler(rulerLayer.CurrentRulerWithHighlightedControlPoint, rulerLayer.CurrentBrickUsedForRulerAttachement));
		}

		private void useAsModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// this action (i.e. changing the settings) is not undoable
			if (Map.Instance.SelectedLayer is LayerRuler)
			{
				LayerRuler rulerLayer = Map.Instance.SelectedLayer as LayerRuler;
				if (rulerLayer.SelectedObjects.Count == 1)
				{
					LayerRuler.RulerItem item = rulerLayer.SelectedObjects[0] as LayerRuler.RulerItem;
					if (item != null)
						PreferencesForm.sChangeRulerSettingsFromRuler(item);
				}
			}
			else if (Map.Instance.SelectedLayer is LayerText)
			{
				LayerText textLayer = Map.Instance.SelectedLayer as LayerText;
				if (textLayer.SelectedObjects.Count == 1)
				{
					LayerText.TextCell item = textLayer.SelectedObjects[0] as LayerText.TextCell;
					if (item != null)
						PreferencesForm.sChangeTextSettingsFromText(item);
				}
			}
		}

		private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Map.Instance.editSelectedItemsProperties(mLastDownMouseCoordInStud);
		}

		private void scrollBarToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Warn Main form to update its menu item for the scroll bars
			MainForm.Instance.mapScrollBarsVisibilityChangeNotification(scrollBarToolStripMenuItem.Checked);
			// show or hide the scrollbar
			ShowHideScrollBars(scrollBarToolStripMenuItem.Checked);
		}
		#endregion

		#region scrollbars

		/// <summary>
		/// This notification function should be called when the total area of the map (including text and rulers) changed.
		/// This include when an item is added, removed, or moved, when a layer is deleted or readded, etc...
		/// </summary>
		public void MapAreaChangedNotification()
		{
			// recompute the total area, but only if the scrollbars are visible, otherwise it's useless
			if (this.horizontalScrollBar.Visible || this.verticalScrollBar.Visible)
			{
				// recompute the map area
				mMapTotalAreaInStud = Map.Instance.getTotalAreaInStud(false);
				// update the scrollbars size (if they are not visible, nothing happen)
				updateScrollbarSize();
			}
		}

		public void ShowHideScrollBars(bool isVisible)
		{
			// show or hide the two scrollbars
			this.horizontalScrollBar.Visible = isVisible;
			this.verticalScrollBar.Visible = isVisible;

			// call the map area change notification in order to recompute the area size and update the scrollbar size
			MapAreaChangedNotification();
		}

		private void UpdateScrollBarThumbFromViewCorner(bool updateX, bool updateY)
		{
			// update the scrollbar values
			if (updateX)
			{
				int newValue = (int)((((mViewCornerX - mMapTotalAreaInStud.Left + mScrollBarAddedMarginInStud) * mViewScale) / this.Size.Width) * mMapScrollBarSliderSize);
				this.horizontalScrollBar.Value = Math.Max(Math.Min(newValue, this.horizontalScrollBar.Maximum), this.horizontalScrollBar.Minimum);
			}
			if (updateY)
			{
				int newValue = (int)((((mViewCornerY - mMapTotalAreaInStud.Top + mScrollBarAddedMarginInStud) * mViewScale) / this.Size.Height) * mMapScrollBarSliderSize);
				this.verticalScrollBar.Value = Math.Max(Math.Min(newValue, this.verticalScrollBar.Maximum), this.verticalScrollBar.Minimum);
			}
		}

	private void UpdateViewCornerFromScrollBarThumb(bool updateX, bool updateY)
		{
			// update the view corner
			if (updateX)
				mViewCornerX = ((((double)this.horizontalScrollBar.Value / mMapScrollBarSliderSize) * this.Size.Width) / mViewScale) + mMapTotalAreaInStud.Left - mScrollBarAddedMarginInStud;
			if (updateY)
				mViewCornerY = ((((double)this.verticalScrollBar.Value / mMapScrollBarSliderSize) * this.Size.Height) / mViewScale) + mMapTotalAreaInStud.Top - mScrollBarAddedMarginInStud;
		}

		private void updateScrollbarSize()
		{
			// does nothing if the scrollbars are not visible
			if (this.horizontalScrollBar.Visible || this.verticalScrollBar.Visible)
			{
				// compute the total area in screen pixel (from studs)
				double totalWidthInPixel = (mMapTotalAreaInStud.Right - mMapTotalAreaInStud.Left + (mScrollBarAddedMarginInStud * 2)) * mViewScale;
				double totalHeightInPixel = (mMapTotalAreaInStud.Bottom - mMapTotalAreaInStud.Top + (mScrollBarAddedMarginInStud * 2)) * mViewScale;

				// compute how many screens are needed to display the total map area, that will define how long should be the scroll bar
				double screenCountToDisplayTotalWidth = totalWidthInPixel / this.Size.Width;
				double screenCountToDisplayTotalHeight = totalHeightInPixel / this.Size.Height;

				// set the maximum of the scroll bars
				this.horizontalScrollBar.Maximum = (int)(mMapScrollBarSliderSize * screenCountToDisplayTotalWidth);
				this.horizontalScrollBar.LargeChange = mMapScrollBarSliderSize;
				this.verticalScrollBar.Maximum = (int)(mMapScrollBarSliderSize * screenCountToDisplayTotalHeight);
				this.verticalScrollBar.LargeChange = mMapScrollBarSliderSize;

				// set the value of the scrollbar depending on the current view
				UpdateScrollBarThumbFromViewCorner(true, true);
			}
		}

		private void horizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			UpdateViewCornerFromScrollBarThumb(true, false);
			this.Invalidate();
		}

		private void verticalScrollBar_Scroll(object sender, ScrollEventArgs e)
		{
			UpdateViewCornerFromScrollBarThumb(false, true);
			this.Invalidate();
		}
		private void verticalScrollBar_MouseEnter(object sender, EventArgs e)
		{
			this.Cursor = this.DefaultCursor;
		}

		private void horizontalScrollBar_MouseEnter(object sender, EventArgs e)
		{
			this.Cursor = this.DefaultCursor;
		}

		private void MapPanel_Resize(object sender, EventArgs e)
		{
			updateScrollbarSize();
		}

		private void MapPanel_SizeChanged(object sender, EventArgs e)
		{
			updateScrollbarSize();
		}
		#endregion
	}
}