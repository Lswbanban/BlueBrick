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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlueBrick.Actions;
using BlueBrick.Actions.Area;

namespace BlueBrick.MapData
{
	[Serializable]
	public class LayerArea : Layer
	{
		private static Color sCurrentDrawColor = Color.Empty;
		private static TextureBrush sEraseBrush = null;

		private int mAreaCellSizeInStud = Properties.Settings.Default.DefaultAreaSize;
		private Dictionary<int, Dictionary<int, SolidBrush>> mColorMap = new Dictionary<int, Dictionary<int, SolidBrush>>();

		//related to selection
		private bool mIsMovingArea = false;
		private bool mIsPaintingNewArea = false;
		private Point mMouseDownInitialPosition = Point.Empty;
		private Point mMouseDownLastPosition = Point.Empty;

		#region get/set

		public static Color CurrentDrawColor
		{
			get { return sCurrentDrawColor; }
			set { sCurrentDrawColor = value; }
		}

		public static bool IsCurrentToolTheEraser
		{
			get { return (sCurrentDrawColor == Color.Empty); }
		}

		/// <summary>
		/// get the type name id of this type of layer used in the xml file (not localized)
		/// </summary>
		public override string XmlTypeName
		{
			get { return "area"; }
		}

		/// <summary>
		/// get the localized name of this type of layer
		/// </summary>
		public override string LocalizedTypeName
		{
			get { return Properties.Resources.ErrorMsgLayerTypeArea; }
		}

		public override int Transparency
		{
			set
			{
				// set the value
				mTransparency = value;
				// compute the new alpha value
				int alpha = AlphaValue;
				// iterate on all the cell to change the brush color
				foreach (KeyValuePair<int, Dictionary<int, SolidBrush>> line in mColorMap)
					foreach (KeyValuePair<int, SolidBrush> brush in line.Value)
						brush.Value.Color = Color.FromArgb(alpha, brush.Value.Color);
			}
		}

		/// <summary>
		/// Get the number of items in this layer.
		/// For the area we return the number of lines.
		/// </summary>
		public override int NbItems
		{
			get { return mColorMap.Count; }
		}

		/// <summary>
		/// Return false for now because the areas cannot be selected. May change later.
		/// </summary>
		public override bool HasSomethingToSelect
		{
			get { return false; }
		}

		public int AlphaValue
		{
			get { return (255 * mTransparency) / 100; }
		}

		public int AreaCellSizeInStud
		{
			get { return mAreaCellSizeInStud; }
			set { mAreaCellSizeInStud = value; }
		}

		public Dictionary<int, Dictionary<int, SolidBrush>> ColorMap
		{
			get { return mColorMap; }
			set { mColorMap = value; }
		}

		#endregion

		#region constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public LayerArea()
		{
			// init the erase brush if not already done
			if (sEraseBrush == null)
			{
				// create a checker texture
				Bitmap texture = new Bitmap(16, 16);
				Graphics g = Graphics.FromImage(texture);
				g.Clear(Color.FromArgb(0x70, Color.Black));
				SolidBrush whiteBrush = new SolidBrush(Color.FromArgb(0x70, Color.White));
				g.FillRectangle(whiteBrush, 0, 0, 8, 8);
				g.FillRectangle(whiteBrush, 8, 8, 8, 8);
				g.Flush();
				sEraseBrush = new TextureBrush(texture);
			}
			// set the transparency with the default one
			Transparency = Properties.Settings.Default.DefaultAreaTransparency;
		}

		/// <summary>
		/// copy only the option parameters from the specified layer
		/// </summary>
		/// <param name="layerToCopy">the model to copy from</param>
		public override void CopyOptionsFrom(Layer layerToCopy)
		{
			// call the base method
			base.CopyOptionsFrom(layerToCopy);
			// and try to cast in area layer
			LayerArea areaLayer = layerToCopy as LayerArea;
			if (areaLayer != null)
				AreaCellSizeInStud = areaLayer.AreaCellSizeInStud;
		}
		#endregion

		#region IXmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			base.ReadXml(reader);
			// before version 5, the transparency was only in the area layer, after it is read in the base class
			if (Map.DataVersionOfTheFileLoaded < 5)
				mTransparency = reader.ReadElementContentAsInt();
			// read the cell size
			mAreaCellSizeInStud = reader.ReadElementContentAsInt();
			// the areas list
			int currentLineIndex = int.MaxValue;
			bool areaFound = reader.ReadToDescendant("Area");
			while (areaFound)
			{
				// read all the data of the area
				reader.ReadToDescendant("x");
				int x = reader.ReadElementContentAsInt();
				int y = reader.ReadElementContentAsInt();
				string stringColor = reader.ReadElementContentAsString();
				Color color = Color.FromArgb(int.Parse(stringColor, System.Globalization.NumberStyles.HexNumber));
				// and paint the appropriate cell
				paintCell(x, y, color);

				// read the next area
				areaFound = reader.ReadToNextSibling("Area");

				// step the progress bar for each line
				if (x != currentLineIndex)
				{
					MainForm.Instance.stepProgressBar();
					currentLineIndex = x;
				}
			}
			// read the Areas tag, to finish the list of area
			reader.Read();
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			// write the header
			writeHeaderAndCommonProperties(writer);
			// write area cell size
			writer.WriteElementString("AreaCellSize", mAreaCellSizeInStud.ToString());
			// and serialize the area list
			writer.WriteStartElement("Areas");
			// iterate on all the cell to change the brush color
			foreach (KeyValuePair<int, Dictionary<int, SolidBrush>> line in mColorMap)
			{
				foreach (KeyValuePair<int, SolidBrush> brush in line.Value)
				{
					writer.WriteStartElement("Area");
					writer.WriteElementString("x", line.Key.ToString());
					writer.WriteElementString("y", brush.Key.ToString());
					Color color = brush.Value.Color;
					int intColor = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
					writer.WriteElementString("color", intColor.ToString("X"));
					writer.WriteEndElement();
				}
				// step the progress bar for each line
				MainForm.Instance.stepProgressBar();
			}
			writer.WriteEndElement(); // end of Areas
			// write the footer
			writeFooter(writer); // end of layer
		}
		#endregion

		#region add/remove color cell

		/// <summary>
		/// get the brush at the specified cell position
		/// </summary>
		/// <param name="x">x index in cell coord</param>
		/// <param name="y">y index in cell coord</param>
		/// <returns>the solid brush at this position or null if there is nothing here.</returns>
		private SolidBrush getBrush(int x, int y)
		{
			Dictionary<int, SolidBrush> line = null;
			mColorMap.TryGetValue(x, out line);
			if (line == null)
				return null;
			SolidBrush brush = null;
			line.TryGetValue(y, out brush);
			return brush;
		}

		/// <summary>
		/// Add the specified color cell at the specified position (in stud).
		/// The color cell have the size of a sub grid element. If you specify an empty color, it clears the cell.
		/// </summary>
		/// <param name="xInCellIndex">x coord in cell index that will be colored</param>
		/// <param name="yInCellIndex">y coord in cell index that will be colored</param>
		/// <param name="color">the new color (the alpha will be replaced). Can be empty.</param>
		/// <returns>the previous color in the specified cell</returns>
		public Color paintCell(int xInCellIndex, int yInCellIndex, Color color)
		{
			// compute the new color by adding the alpha (if not empty)
			Color newColor = color;
			if (color != Color.Empty)
				newColor = Color.FromArgb(AlphaValue, color);
			Color oldColor = Color.Empty;
			// check if the brush already exist and replace it, or create it
			Dictionary<int, SolidBrush> line = null;
			mColorMap.TryGetValue(xInCellIndex, out line);
			if (line == null)
			{
				line = new Dictionary<int, SolidBrush>();
				mColorMap.Add(xInCellIndex, line);
			}
			SolidBrush brush = null;
			line.TryGetValue(yInCellIndex, out brush);
			if (brush == null)
			{
				if (newColor != Color.Empty)
				{
					brush = new SolidBrush(newColor);
					line.Add(yInCellIndex, brush);
				}
			}
			else
			{
				oldColor = brush.Color;
				if (newColor != Color.Empty)
					brush.Color = newColor;
				else
					line.Remove(yInCellIndex);
			}
			// return the previous color
			return oldColor;
		}

		/// <summary>
		/// Move all the cells according to the specified move direction
		/// </summary>
		/// <param name="moveX">the move in x axis, in cell numbers</param>
		/// <param name="moveY">the move in y axis, in cell numbers</param>
		public void moveCells(int moveX, int moveY)
		{
			// move the cells if necessary
			if ((moveX != 0) || (moveY != 0))
			{
				// copy the key of the lines
				int[] lineKeys = new int[mColorMap.Keys.Count];
				mColorMap.Keys.CopyTo(lineKeys, 0);
				// according to the doc, the order of the keys in the Dictionary.KeyCollection is unspecified
				// so we need to sort the keys, to avoid override when moving
				Array.Sort(lineKeys);

				// depending on the direction of the move, we copy from the begining or the end
				int startX = 0;
				int endX = lineKeys.Length;
				int dirX = 1;
				if (moveX > 0)
				{
					startX = lineKeys.Length - 1;
					endX = -1;
					dirX = -1;
				}

				// iterate to move the lines
				for (int x = startX; x != endX; x += dirX)
				{
					int currentLineKey = lineKeys[x];
					Dictionary<int, SolidBrush> currentLine = null;
					mColorMap.TryGetValue(currentLineKey, out currentLine);
					if (currentLine != null)
					{
						// move the line if necessary
						if (moveX != 0)
						{
							mColorMap.Remove(currentLineKey);
							mColorMap.Add(currentLineKey + moveX, currentLine);
						}

						// move the row if necessary
						if (moveY != 0)
						{
							// copy the key inside the current line
							int[] rowKeys = new int[currentLine.Keys.Count];
							currentLine.Keys.CopyTo(rowKeys, 0);
							// according to the doc, the order of the keys in the Dictionary.KeyCollection is unspecified
							// so we need to sort the keys, to avoid override when moving
							Array.Sort(rowKeys);

							// depending on the direction of the move, we copy from the begining or the end
							int startY = 0;
							int endY = rowKeys.Length;
							int dirY = 1;
							if (moveY > 0)
							{
								startY = rowKeys.Length - 1;
								endY = -1;
								dirY = -1;
							}

							// iterate to move the rows
							for (int y = startY; y != endY; y += dirY)
							{
								int currentRowKey = rowKeys[y];
								SolidBrush currentRow = null;
								currentLine.TryGetValue(currentRowKey, out currentRow);
								if (currentRow != null)
								{
									// move the row
									currentLine.Remove(currentRowKey);
									currentLine.Add(currentRowKey + moveY, currentRow);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Rescale the colormap by trying to keep the cell at the same place, by duplicating
		/// some cell, or skiping some cells depending on the direction of the rescale (smaller
		/// or bigger cell size)
		/// </summary>
		/// <param name="newCellSize"></param>
		public void rescaleColorMap(int newCellSize)
		{
			// check if the new scale is bigger or smaller than the current one, 
			// to add or remove cells
			if (AreaCellSizeInStud > newCellSize)
			{
				int cellSizeFactor = (int)Math.Round((float)AreaCellSizeInStud / (float)newCellSize);
				Dictionary<int, Dictionary<int, SolidBrush>> newColorMap = new Dictionary<int, Dictionary<int, SolidBrush>>();
				// iterate on the current color map and fill the new one
				foreach (KeyValuePair<int, Dictionary<int, SolidBrush>> line in mColorMap)
				{
					for (int i = 0; i < cellSizeFactor; ++i)
					{
						Dictionary<int, SolidBrush> newLine = new Dictionary<int, SolidBrush>();
						newColorMap.Add(line.Key * cellSizeFactor + i, newLine);

						foreach (KeyValuePair<int, SolidBrush> brush in line.Value)
						{
							for (int j = 0; j < cellSizeFactor; ++j)
								newLine.Add(brush.Key * cellSizeFactor + j, brush.Value.Clone() as SolidBrush);
						}
					}
				}
				// set the new color map
				mColorMap = newColorMap;
			}
			else if (AreaCellSizeInStud < newCellSize)
			{
				int cellSizeFactor = (int)Math.Round((float)newCellSize / (float)AreaCellSizeInStud);
				Dictionary<int, Dictionary<int, SolidBrush>> newColorMap = new Dictionary<int, Dictionary<int, SolidBrush>>();
				// iterate on the current color map and fill the new one
				foreach (KeyValuePair<int, Dictionary<int, SolidBrush>> line in mColorMap)
				{
					if (line.Key % cellSizeFactor == 0)
					{
						Dictionary<int, SolidBrush> newLine = new Dictionary<int, SolidBrush>();
						newColorMap.Add(line.Key / cellSizeFactor, newLine);

						foreach (KeyValuePair<int, SolidBrush> brush in line.Value)
						{
							if (brush.Key % cellSizeFactor == 0)
								newLine.Add(brush.Key / cellSizeFactor, brush.Value.Clone() as SolidBrush);
						}
					}
				}
				// set the new color map
				mColorMap = newColorMap;
			}
		}

		#endregion
		#region draw

		/// <summary>
		/// get the total area in stud covered by all the area cells in this layer
		/// </summary>
		/// <returns></returns>
		public override RectangleF getTotalAreaInStud()
		{
			PointF topLeft = new PointF(float.MaxValue, float.MaxValue);
			PointF bottomRight = new PointF(float.MinValue, float.MinValue);
			foreach (KeyValuePair<int, Dictionary<int, SolidBrush>> line in mColorMap)
				foreach (KeyValuePair<int, SolidBrush> brush in line.Value)
				{
					// store x and y coord in temp var for more readibility
					int x = line.Key;
					int y = brush.Key;
					// check the coord of the cell with the two corner
					if (x < topLeft.X)
						topLeft.X = x;
					if (y < topLeft.Y)
						topLeft.Y = y;
					if (x > bottomRight.X)
						bottomRight.X = x;
					if (y > bottomRight.Y)
						bottomRight.Y = y;
				}
			// compute the result by transforming the cell coordinates into studs coord
			RectangleF result = new RectangleF();
			result.X = topLeft.X * mAreaCellSizeInStud;
			result.Y = topLeft.Y * mAreaCellSizeInStud;
			result.Width = (bottomRight.X - topLeft.X + 1) * mAreaCellSizeInStud;
			result.Height = (bottomRight.Y - topLeft.Y + 1) * mAreaCellSizeInStud;
			return result;
		}

		/// <summary>
		/// Draw the layer.
		/// </summary>
		/// <param name="g">the graphic context in which draw the layer</param>
        /// <param name="areaInStud">The region in which we should draw</param>
        /// <param name="scalePixelPerStud">The scale to use to draw</param>
        /// <param name="drawSelectionRectangle">If true draw the selection rectangle (this can be set to false when exporting the map to an image)</param>
        public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, bool drawSelectionRectangle)
		{
			if (!Visible)
				return;

			// if the user is moving the area, we just shift the rendering
			if (mIsMovingArea)
			{
				int diffXInCell = mMouseDownLastPosition.X - mMouseDownInitialPosition.X;
				int diffYInCell = mMouseDownLastPosition.Y - mMouseDownInitialPosition.Y;
				areaInStud.X -= diffXInCell * mAreaCellSizeInStud;
				areaInStud.Y -= diffYInCell * mAreaCellSizeInStud;
			}

			// compute one area size in pixel unit accordind to the current zoom
			float areaCellSizeInPixel = (float)(mAreaCellSizeInStud * scalePixelPerStud);

			// start to draw the visible part of the map
			int startX = (int)(areaInStud.Left / mAreaCellSizeInStud) - 1;
			int endX = (int)(areaInStud.Right / mAreaCellSizeInStud) + 1;
			int startY = (int)(areaInStud.Top / mAreaCellSizeInStud) - 1;
			int endY = (int)(areaInStud.Bottom / mAreaCellSizeInStud) + 1;

			float startPixelX = (float)(-areaInStud.Left * scalePixelPerStud);
			float startPixelY = (float)(-areaInStud.Top * scalePixelPerStud);
			for (int x = startX; x <= endX; ++x)
			{
				Dictionary<int, SolidBrush> line = null;
				mColorMap.TryGetValue(x, out line);
				if (line != null)
					for (int y = startY; y <= endY; ++y)
					{
						SolidBrush brush = null;
						line.TryGetValue(y, out brush);
						if (brush != null)
							g.FillRectangle(brush, startPixelX + (x * areaCellSizeInPixel), startPixelY + (y * areaCellSizeInPixel), areaCellSizeInPixel, areaCellSizeInPixel);
					}
			}

			// check if the user is painting some new area and draw a new rectangle
			if (mIsPaintingNewArea)
			{
				// choose the right brush according to the current draw color
				Brush brush = null;
				if (sCurrentDrawColor != Color.Empty)
					brush = new SolidBrush(Color.FromArgb(0x70, sCurrentDrawColor));
				else
					brush = sEraseBrush;
				// draw the selection area
				float x = startPixelX + (Math.Min(mMouseDownInitialPosition.X, mMouseDownLastPosition.X) * areaCellSizeInPixel);
				float y = startPixelY + (Math.Min(mMouseDownInitialPosition.Y, mMouseDownLastPosition.Y) * areaCellSizeInPixel);
				float width = (Math.Abs(mMouseDownLastPosition.X - mMouseDownInitialPosition.X) + 1) * areaCellSizeInPixel;
				float height = (Math.Abs(mMouseDownLastPosition.Y - mMouseDownInitialPosition.Y) + 1) * areaCellSizeInPixel;
				g.FillRectangle(brush, x, y, width, height);
			}
		}

		#endregion
		#region mouse event

		/// <summary>
		/// Return the cursor that should be display when the mouse is above the map without mouse click
		/// </summary>
		/// <param name="mouseCoordInStud"></param>
		public override Cursor getDefaultCursorWithoutMouseClick(PointF mouseCoordInStud)
		{
			// if the layer is not visible you can basically do nothing on it
			if (!Visible)
				return MainForm.Instance.HiddenLayerCursor;
			else if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
				return MainForm.Instance.AreaMoveCursor;
			else if (LayerArea.IsCurrentToolTheEraser)
				return MainForm.Instance.AreaEraserCursor;
			return MainForm.Instance.AreaPaintCursor;
		}

		/// <summary>
		/// Convert a mouse coord in stud into a cell coord
		/// </summary>
		/// <param name="mouseCoordInStud">the point to convert</param>
		/// <returns>the converted coordinates</returns>
		private Point computeCellCoordFromStudCoord(PointF mouseCoordInStud)
		{
			Point result = new Point((int)(mouseCoordInStud.X / mAreaCellSizeInStud), (int)(mouseCoordInStud.Y / mAreaCellSizeInStud));
			if (mouseCoordInStud.X < 0)
				result.X = result.X - 1;
			if (mouseCoordInStud.Y < 0)
				result.Y = result.Y - 1;
			return result;
		}

		/// <summary>
		/// This function is called to know if this layer is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if this layer wants to handle it</returns>
		public override bool handleMouseDown(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			// if the layer is not visible it is not sensible to mouse click
			if (!Visible)
				return false;

			// we can paint every part, so we are always interested in the left click mouse event
			// and the right click for canceling
			return ((e.Button == MouseButtons.Left) || ((mIsMovingArea || mIsPaintingNewArea) && (e.Button == MouseButtons.Right)));
		}

		/// <summary>
		/// This method is called if the map decided that this layer should handle
		/// this mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseDown(MouseEventArgs e, PointF mouseCoordInStud)
		{
			if (e.Button == MouseButtons.Left)
			{
				// check if we paint or move the area
				mIsMovingArea = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey);
				mIsPaintingNewArea = !mIsMovingArea;
				// record the initial position
				mMouseDownInitialPosition = computeCellCoordFromStudCoord(mouseCoordInStud);
				mMouseDownLastPosition = mMouseDownInitialPosition;
			}
			else if (e.Button == MouseButtons.Right)
			{
				// right click = cancel the edition so reset all the flag
				mIsMovingArea = false;
				mIsPaintingNewArea = false;
			}
			// update on the down event because we want to draw the fist square
			return true;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			if (mIsMovingArea || mIsPaintingNewArea)
			{
				// compute the position snapped on the grid
				Point newPosition = computeCellCoordFromStudCoord(mouseCoordInStud);
				// update is the snapped position changed
				bool mustUpdate = (mMouseDownLastPosition.X != newPosition.X) || (mMouseDownLastPosition.Y != newPosition.Y);
				mMouseDownLastPosition = newPosition;
				// update if the user move the mouse
				return mustUpdate;
			}
			return false;
		}

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseUp(MouseEventArgs e, PointF mouseCoordInStud)
		{
			if (mIsMovingArea)
			{
				// compute the moving direction
				int x = mMouseDownLastPosition.X - mMouseDownInitialPosition.X;
				int y = mMouseDownLastPosition.Y - mMouseDownInitialPosition.Y;
				ActionManager.Instance.doAction(new MoveArea(this, x, y));
			}
			else if (mIsPaintingNewArea)
			{
				// compute the rectangle of the modified area
				int x = Math.Min(mMouseDownInitialPosition.X, mMouseDownLastPosition.X);
				int y = Math.Min(mMouseDownInitialPosition.Y, mMouseDownLastPosition.Y);
				int width = Math.Abs(mMouseDownLastPosition.X - mMouseDownInitialPosition.X) + 1;
				int height = Math.Abs(mMouseDownLastPosition.Y - mMouseDownInitialPosition.Y) + 1;
				Rectangle rectangle = new Rectangle(x, y, width, height);			
				// depending on the current draw color, we add or remove area
				if (sCurrentDrawColor != Color.Empty)
					ActionManager.Instance.doAction(new AddArea(this, sCurrentDrawColor, rectangle));
				else
					ActionManager.Instance.doAction(new DeleteArea(this, rectangle));
			}
			// reset the moving and painting flag
			mIsMovingArea = false;
			mIsPaintingNewArea = false;
			// and finally update the view
			return true;
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public override void selectInRectangle(RectangleF selectionRectangeInStud)
		{
			// nothing to select on an area,
			// since we always take the mouse down event, this event is never called by the map
		}

		#endregion
	}
}
