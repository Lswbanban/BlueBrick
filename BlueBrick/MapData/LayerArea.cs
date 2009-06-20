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

		private int mTransparency = Properties.Settings.Default.DefaultAreaTransparency;
		private int mAreaCellSizeInStud = Properties.Settings.Default.DefaultAreaSize;
		private Dictionary<int, Dictionary<int, SolidBrush>> mColorMap = new Dictionary<int, Dictionary<int, SolidBrush>>();

		//related to selection
		private bool mIsPaintingNewArea = false;
		private Point mMouseDownInitialPosition = Point.Empty;
		private Point mMouseDownLastPosition = Point.Empty;

		#region get/set

		public static Color CurrentDrawColor
		{
			get { return sCurrentDrawColor; }
			set { sCurrentDrawColor = value; }
		}

		public int Transparency
		{
			get { return mTransparency; }
			set
			{
				// set the value
				mTransparency = value;
				// compute the new alpha value
				int alpha = AlphaValue;
				// iterate on all the cell to change the brush color
				foreach (KeyValuePair<int, Dictionary<int, SolidBrush>> line in mColorMap)
					foreach (KeyValuePair<int, SolidBrush> brush in line.Value)
						brush.Value.Color = changeAlpha(brush.Value.Color, alpha);
			}
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

		public LayerArea()
		{
			// init the erase brush if not already done
			if (sEraseBrush == null)
			{
				// create a checker texture
				Bitmap texture = new Bitmap(16, 16);
				Graphics g = Graphics.FromImage(texture);
				g.Clear(changeAlpha(Color.Black, 0x70));
				SolidBrush whiteBrush = new SolidBrush(changeAlpha(Color.White, 0x70));
				g.FillRectangle(whiteBrush, 0, 0, 8, 8);
				g.FillRectangle(whiteBrush, 8, 8, 8, 8);
				g.Flush();
				sEraseBrush = new TextureBrush(texture);
			}
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
			{
				Transparency = areaLayer.Transparency;
				AreaCellSizeInStud = areaLayer.AreaCellSizeInStud;
			}
		}

		public override int getNbItems()
		{
			// for the area we return the number of lines
			return mColorMap.Count;
		}

		#endregion
		#region IXmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			base.ReadXml(reader);
			// read transparency and cell size
			mTransparency = reader.ReadElementContentAsInt();
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
			writer.WriteAttributeString("type", "area");
			base.WriteXml(writer);
			// write transparency and area cell size
			writer.WriteElementString("Transparency", mTransparency.ToString());
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
			writer.WriteEndElement();
		}

		#endregion

		#region add/remove color cell

		private Color changeAlpha(Color color, int alpha)
		{
			int intColor = (alpha << 24) | (color.R << 16) | (color.G << 8) | color.B;
			return Color.FromArgb(intColor);
		}

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
				newColor = changeAlpha(color, AlphaValue);
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
		public RectangleF getTotalAreaInStud()
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
			//if (topLeft.X < 0)
			//    topLeft.X = topLeft.X - 1;
			//if (topLeft.Y < 0)
			//    topLeft.Y = topLeft.Y - 1;
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
		/// <param name="area">the area in layer pixel</param>
		public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
		{
			if (!Visible)
				return;

			float areaCellSizeInPixel = (float)(mAreaCellSizeInStud * scalePixelPerStud);
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
					brush = new SolidBrush(changeAlpha(sCurrentDrawColor, 0x70));
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
			if (sCurrentDrawColor == Color.Empty)
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
			// if the layer is not visible it is not sensible to mouve click
			if (!Visible)
				return false;

			// we can paint every part, so we are always interested in the left click mouse event
			return (e.Button == MouseButtons.Left);
		}

		/// <summary>
		/// This method is called if the map decided that this layer should handle
		/// this mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseDown(MouseEventArgs e, PointF mouseCoordInStud)
		{
			mMouseDownInitialPosition = computeCellCoordFromStudCoord(mouseCoordInStud);
			mMouseDownLastPosition = mMouseDownInitialPosition;
			mIsPaintingNewArea = true;
			// update on the down event because we want to draw the fist square
			return true;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud)
		{
			// compute the position snapped on the grid
			Point newPosition = computeCellCoordFromStudCoord(mouseCoordInStud);
			// update is the snapped position changed
			bool mustUpdate = (mMouseDownLastPosition.X != newPosition.X) || (mMouseDownLastPosition.Y != newPosition.Y);
			mMouseDownLastPosition = newPosition;
			// update if the user move the mouse
			return mustUpdate;
		}

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseUp(MouseEventArgs e, PointF mouseCoordInStud)
		{
			// reset the painting flag
			mIsPaintingNewArea = false;
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
