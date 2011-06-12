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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlueBrick.MapData;
using BlueBrick.Properties;
using BlueBrick.Actions;
using BlueBrick.Actions.Texts;
using BlueBrick.Actions.Bricks;

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

		// scroll position
		private bool mIsScrolling = false;
		private Point mLastScrollMousePos = new Point();
		// initial zoom position, if not using the wheel
		private bool mIsZooming = false;
		private Point mFirstZoomMousePos = new Point();
		private Point mLastZoomMousePos = new Point();
		// last position if no button is pressed
		private Point mLastMousePos = new Point();
		// the coordinate in STUD of the center of the view (that means which part of the map the center of the view is currently targeting)
		private double mViewCornerX = 0;
		private double mViewCornerY = 0;

		// selection in rectangle
		private bool mIsSelectionRectangleOn = false;
		private bool mIsMouseHandledByMap = false;
		private Point mSelectionRectangleInitialPosition = new Point();
		private Rectangle mSelectionRectangle = new Rectangle();
		private Pen mSelectionRectanglePen = new Pen(Color.Black, 2);

		// drawing data to draw the general info watermark
		private Font mGeneralInfoFont = new Font(FontFamily.GenericSansSerif, 10);
		private SolidBrush mGeneralInfoBackgroundBrush = new SolidBrush(Color.FromArgb(0x77FFFFFF));
		private int mCurrentStatusBarHeight = 0;

		//dragndrop of a part on the map
		private LayerBrick.Brick mCurrentPartDrop = null;
		private LayerBrick mBrickLayerThatReceivePartDrop = null;
		private ContextMenuStrip contextMenuStrip;
		private System.ComponentModel.IContainer components;
		private ToolStripMenuItem bringToFrontToolStripMenuItem;
		private ToolStripMenuItem sendToBackToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripMenuItem deselectAllToolStripMenuItem;
		private ToolStripMenuItem selectPathToolStripMenuItem;
		private ToolStripMenuItem selectAllToolStripMenuItem;
		private ToolStripSeparator toolStripSeparator2;
		private ToolStripMenuItem groupToolStripMenuItem;
		private ToolStripMenuItem ungroupToolStripMenuItem;

		// scale
		private double mViewScale = 4.0;

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
				// avoid setting a null scale or negative scale, set only a scale in the range
				if (value < 0.2)
					mViewScale = 0.2;
				else if (value > 16.0)
					mViewScale = 16.0;
				else
					mViewScale = value;
				// invalidate the panel, since we must redraw it to handle the new scale
				Invalidate();
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
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deselectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.selectPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.groupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ungroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bringToFrontToolStripMenuItem,
            this.sendToBackToolStripMenuItem,
            this.toolStripSeparator1,
            this.selectAllToolStripMenuItem,
            this.deselectAllToolStripMenuItem,
            this.selectPathToolStripMenuItem,
            this.toolStripSeparator2,
            this.groupToolStripMenuItem,
            this.ungroupToolStripMenuItem});
			this.contextMenuStrip.Name = "contextMenuStrip";
			resources.ApplyResources(this.contextMenuStrip, "contextMenuStrip");
			this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
			// 
			// bringToFrontToolStripMenuItem
			// 
			this.bringToFrontToolStripMenuItem.Name = "bringToFrontToolStripMenuItem";
			resources.ApplyResources(this.bringToFrontToolStripMenuItem, "bringToFrontToolStripMenuItem");
			this.bringToFrontToolStripMenuItem.Click += new System.EventHandler(this.bringToFrontToolStripMenuItem_Click);
			// 
			// sendToBackToolStripMenuItem
			// 
			this.sendToBackToolStripMenuItem.Name = "sendToBackToolStripMenuItem";
			resources.ApplyResources(this.sendToBackToolStripMenuItem, "sendToBackToolStripMenuItem");
			this.sendToBackToolStripMenuItem.Click += new System.EventHandler(this.sendToBackToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			resources.ApplyResources(this.selectAllToolStripMenuItem, "selectAllToolStripMenuItem");
			this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
			// 
			// deselectAllToolStripMenuItem
			// 
			this.deselectAllToolStripMenuItem.Name = "deselectAllToolStripMenuItem";
			resources.ApplyResources(this.deselectAllToolStripMenuItem, "deselectAllToolStripMenuItem");
			this.deselectAllToolStripMenuItem.Click += new System.EventHandler(this.deselectAllToolStripMenuItem_Click);
			// 
			// selectPathToolStripMenuItem
			// 
			this.selectPathToolStripMenuItem.Name = "selectPathToolStripMenuItem";
			resources.ApplyResources(this.selectPathToolStripMenuItem, "selectPathToolStripMenuItem");
			this.selectPathToolStripMenuItem.Click += new System.EventHandler(this.selectPathToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
			// 
			// groupToolStripMenuItem
			// 
			this.groupToolStripMenuItem.Name = "groupToolStripMenuItem";
			resources.ApplyResources(this.groupToolStripMenuItem, "groupToolStripMenuItem");
			this.groupToolStripMenuItem.Click += new System.EventHandler(this.groupToolStripMenuItem_Click);
			// 
			// ungroupToolStripMenuItem
			// 
			this.ungroupToolStripMenuItem.Name = "ungroupToolStripMenuItem";
			resources.ApplyResources(this.ungroupToolStripMenuItem, "ungroupToolStripMenuItem");
			this.ungroupToolStripMenuItem.Click += new System.EventHandler(this.ungroupToolStripMenuItem_Click);
			// 
			// MapPanel
			// 
			this.ContextMenuStrip = this.contextMenuStrip;
			this.MouseLeave += new System.EventHandler(this.MapPanel_MouseLeave);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapPanel_MouseMove);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapPanel_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MapPanel_MouseUp);
			this.MouseEnter += new System.EventHandler(this.MapPanel_MouseEnter);
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

			// NOTE: the background color is set directly in this.BackColor !!!
			this.BackColor = Map.Instance.BackgroundColor;

			// call the draw of the map
			float widthInStud = (float)(this.Size.Width / mViewScale);
			float heightInStud = (float)(this.Size.Height / mViewScale);
			float startXInStud = (float)mViewCornerX;
			float startYInStud = (float)mViewCornerY;
			RectangleF rectangle = new RectangleF(startXInStud, startYInStud, widthInStud, heightInStud);
			Map.Instance.draw(g, rectangle, mViewScale);

			// draw the global info if it is enabled
			if (Properties.Settings.Default.DisplayGeneralInfoWatermark)
			{
				SizeF generalInfoSize = g.MeasureString(Map.Instance.GeneralInfoWatermark, mGeneralInfoFont);
				float x = (float)(this.Size.Width) - generalInfoSize.Width;
				float y = (float)(this.Size.Height - mCurrentStatusBarHeight) - generalInfoSize.Height;
				g.FillRectangle(mGeneralInfoBackgroundBrush, x, y, generalInfoSize.Width, generalInfoSize.Height);
				g.DrawString(Map.Instance.GeneralInfoWatermark, mGeneralInfoFont, Brushes.Black, x, y);
			}

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
			RectangleF totalArea = Map.Instance.getTotalAreaInStud(false);
			mViewCornerX = ((totalArea.Left + totalArea.Right) / 2) - halfViewWidthInStud;
			mViewCornerY = ((totalArea.Top + totalArea.Bottom) / 2) - halfViewHeightInStud;
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

		private void addTextCellAtPosition(PointF position)
		{
			// check if a text layer is selected
			if ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.GetType().Name == "LayerText"))
			{
				// open the edit text dialog in modal
				EditTextForm editTextForm = new EditTextForm(null);
				editTextForm.ShowDialog(this.TopLevelControl);
				if (editTextForm.DialogResult == DialogResult.OK)
				{
					LayerText layerInWhichAdd = (Map.Instance.SelectedLayer as LayerText);
					// add the text cell
					ActionManager.Instance.doAction(new AddText(layerInWhichAdd, editTextForm.EditedText, editTextForm.EditedFont, editTextForm.EditedColor, editTextForm.EditedAlignment, position));
				}
			}
		}

		private void MapPanel_MouseDown(object sender, MouseEventArgs e)
		{
			bool mustRefreshView = false;

			// take the focus anyway if we clik in the map view
			this.Focus();

			// the cursor to set according to the action
			PointF mouseCoordInStud = getMouseCoordInStud(e);
			Cursor preferedCursor = getDefaultCursor(mouseCoordInStud);

			// then dispatch the event
			switch (e.Button)
			{
				case MouseButtons.Left:
					// left button is handle by layers (so give it to the map)
					if (Control.ModifierKeys == Settings.Default.MouseZoomKey)
					{
						// this is the zoom with the keys, not the wheel, save the initial position of the mouse
						mIsZooming = true;
						mFirstZoomMousePos = e.Location;
						mLastZoomMousePos = e.Location;
					}
					else if (Control.ModifierKeys == Settings.Default.MousePanKey)
					{
						// this is the pan with the keys, not the wheel
						mIsScrolling = true;
						mLastScrollMousePos = e.Location;
						preferedCursor = Cursors.NoMove2D;
					}
					else if (Map.Instance.handleMouseDown(e, mouseCoordInStud, ref preferedCursor))
					{
						mustRefreshView = Map.Instance.mouseDown(e, mouseCoordInStud);
						mIsMouseHandledByMap = true;
					}
					else if (e.Clicks == 1)
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
						addTextCellAtPosition(mouseCoordInStud);
						preferedCursor = Cursors.Arrow;
					}

					break;
				case MouseButtons.Middle:
					// middle button is used to scroll
					mIsScrolling = true;
					mLastScrollMousePos = e.Location;
					preferedCursor = Cursors.NoMove2D;
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
			bool mustRefreshView = false;

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
						mustRefreshView = Map.Instance.mouseMove(e, getMouseCoordInStud(e));
					}
					else if (mIsZooming)
					{
						int yDiff = e.Y - mLastZoomMousePos.Y;
						mLastZoomMousePos = e.Location;
						zoom((float)(1.0f + (yDiff * 4.0f * Settings.Default.WheelMouseZoomSpeed)),
								Settings.Default.WheelMouseIsZoomOnCursor, mFirstZoomMousePos);
						// the zoom function already update the view, don't need to set the bool flag here
					}
					else if (mIsScrolling)
					{
						mViewCornerX += (mLastScrollMousePos.X - e.X) / mViewScale;
						mViewCornerY += (mLastScrollMousePos.Y - e.Y) / mViewScale;
						mLastScrollMousePos.X = e.X;
						mLastScrollMousePos.Y = e.Y;
						// update the view continuously
						mustRefreshView = true;
					}
					else if (e.Clicks == 0)
					{
						// if the number of click is null, that means it can be a dragndrop from another view, such as the part lib
						// check if we need to search the image dropped, or if we already have it
						if (mCurrentPartDrop == null)
						{
							if (Map.Instance.canAddBrick())
							{
								// ask the main window if one part was selected in the part lib
								string partDropNumber = (this.TopLevelControl as MainForm).getSelectedPartNumberInPartLib();
								mBrickLayerThatReceivePartDrop = Map.Instance.SelectedLayer as LayerBrick;
								if (partDropNumber != null && mBrickLayerThatReceivePartDrop != null)
								{
									mCurrentPartDrop = new LayerBrick.Brick(partDropNumber);
									mBrickLayerThatReceivePartDrop.addTemporaryPartDrop(mCurrentPartDrop);
									// set the hand cursor
									this.Cursor = MainForm.Instance.BrickDuplicateCursor;
								}
							}
							else
							{
								// set the no cursor
								this.Cursor = Cursors.No;
							}
						}

						// check if we are currently dragging a part
						if (mCurrentPartDrop != null)
						{
							// memorise the position of the mouse snapped to the grid
							PointF partCenter = getScreenPointInStud(e.Location);
							if (mBrickLayerThatReceivePartDrop != null)
							{
								mCurrentPartDrop.Center = mBrickLayerThatReceivePartDrop.getMovedSnapPoint(partCenter);
								mBrickLayerThatReceivePartDrop.updateBoundingSelectionRectangle();
								// refresh the view
								mustRefreshView = true;
							}
						}
					}
					break;
				case MouseButtons.Middle:
					// middle button is used to scroll
					if (mIsScrolling)
					{
						mViewCornerX += (mLastScrollMousePos.X - e.X) / mViewScale;
						mViewCornerY += (mLastScrollMousePos.Y - e.Y) / mViewScale;
						mLastScrollMousePos.X = e.X;
						mLastScrollMousePos.Y = e.Y;
						// update the view continuously
						mustRefreshView = true;
					}
					break;
				case MouseButtons.None:
					// nothing to do if we didn't move
					if ((mLastMousePos.X != e.X) || (mLastMousePos.Y != e.Y))
					{
						// set the cursor with the preference
						this.Cursor = getDefaultCursor(getMouseCoordInStud(e));

						// if there's a brick under the mouse, and the player don't use a button,
						// display the description of the brick in the status bar
						string message = "";
						LayerBrick brickLayer = Map.Instance.SelectedLayer as LayerBrick;
						if (brickLayer != null)
						{
							LayerBrick.Brick brickUnderMouse = brickLayer.getBrickUnderMouse(getMouseCoordInStud(e));
							if (brickUnderMouse != null)
								message = BrickLibrary.Instance.getFormatedBrickInfo(brickUnderMouse.PartNumber, true, true, true);
						}
						else
						{
							LayerText textLayer = Map.Instance.SelectedLayer as LayerText;
							if (textLayer != null)
							{
								LayerText.TextCell textUnderMouse = textLayer.getTextCellUnderMouse(getMouseCoordInStud(e));
								if (textUnderMouse != null)
								{
									message = textUnderMouse.Text.Replace("\r", "");
									message = message.Replace('\n', ' ');
									if (message.Length > 20)
										message = message.Substring(0, 20) + "...";
								}
							}
						}
						//display the message in the status bar
						MainForm.Instance.setStatusBarMessage(message);
					}
					break;
			}

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

			switch (e.Button)
			{
				case MouseButtons.Left:
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
					else if (mCurrentPartDrop != null)
					{
						// we have finished a dragndrop, remove the temporary part
						if (mBrickLayerThatReceivePartDrop != null)
						{
							mBrickLayerThatReceivePartDrop.removeTemporaryPartDrop(mCurrentPartDrop);
							mBrickLayerThatReceivePartDrop = null;
						}
						// and add the real new part
						Map.Instance.addBrick(mCurrentPartDrop.PartNumber, mCurrentPartDrop.Position, mCurrentPartDrop.Orientation);
						(this.TopLevelControl as MainForm).resetSelectedPartInPartLib();
						mCurrentPartDrop = null;
						mustRefreshView = true;
					}
					break;
				case MouseButtons.Middle:
					// middle button is used to scroll
					mIsScrolling = false;
					// update the view at the end of the scroll
					mustRefreshView = true;
					break;
			}

			// restore the default cursor
			this.Cursor = getDefaultCursor(mouseCoordInStud);

			// check if we need to update the view
			if (mustRefreshView)
				updateView();
		}

		private void MapPanel_MouseEnter(object sender, EventArgs e)
		{
			// set the default cursor
			this.Cursor = getDefaultCursor(PointF.Empty);
		}

		private void MapPanel_MouseLeave(object sender, EventArgs e)
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
				// restore the default cursor
				this.Cursor = Cursors.Arrow;
				// update the view
				updateView();
			}
		}

		public void MapPanel_MouseWheel(object sender, MouseEventArgs e)
		{
			zoom((float)(1.0f + (e.Delta * Settings.Default.WheelMouseZoomSpeed)),
				Settings.Default.WheelMouseIsZoomOnCursor, e.Location);
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
			mViewCornerX += previousZoomPointInStud.X - newZoomPointInStud.X;
			mViewCornerY += previousZoomPointInStud.Y - newZoomPointInStud.Y;
			updateView();
		}

		private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			bool enableItemRelatedToSelection = ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count > 0));
			this.bringToFrontToolStripMenuItem.Enabled = enableItemRelatedToSelection;
			this.sendToBackToolStripMenuItem.Enabled = enableItemRelatedToSelection;
			this.deselectAllToolStripMenuItem.Enabled = enableItemRelatedToSelection;
			this.selectPathToolStripMenuItem.Enabled = ((Map.Instance.SelectedLayer != null) && (Map.Instance.SelectedLayer.SelectedObjects.Count == 2));
		}
		#endregion

		#region right click context menu
		private void bringToFrontToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.toolBarBringToFrontButton_Click(sender, e);
		}

		private void sendToBackToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MainForm.Instance.toolBarSendToBackButton_Click(sender, e);
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
			Actions.Items.GroupItems groupAction = new BlueBrick.Actions.Items.GroupItems(null);
			Actions.ActionManager.Instance.doAction(groupAction);
		}

		private void ungroupToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}
		#endregion
	}
}