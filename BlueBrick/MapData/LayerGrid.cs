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

namespace BlueBrick.MapData
{
	[Serializable]
	public class LayerGrid : Layer
	{
		public enum CellIndexType
		{
			LETTERS = 0,
			NUMBERS
		};

		// grid and sub grid
		private Pen mGridLinePen = new Pen(Properties.Settings.Default.DefaultGridColor, 2);
		private Pen mSubGridLinePen = new Pen(Properties.Settings.Default.DefaultSubGridColor, 1);
		private int mGridSizeInStud = Properties.Settings.Default.DefaultGridSize;
		private int mSubDivisionNumber = Properties.Settings.Default.DefaultSubDivisionNumber;
		private bool mDisplayGrid = true;
		private bool mDisplaySubGrid = true;

		// cell index
		private bool mDisplayCellIndex = true;
		private Font mCellIndexFont = Properties.Settings.Default.DefaultTextFont;
		private SolidBrush mCellIndexBrush = new SolidBrush(Properties.Settings.Default.DefaultTextColor);
		private CellIndexType mCellIndexColumnType = CellIndexType.LETTERS;
		private CellIndexType mCellIndexRowType = CellIndexType.NUMBERS;
		private Point mCellIndexCorner = new Point(0, 0); // the position in stud coordinate

		// global param for drawing the cell index
		static private StringFormat sCellIndexStringFormat = new StringFormat();

		#region get/set
		public int GridSizeInStud
		{
			get { return mGridSizeInStud; }
			set { mGridSizeInStud = value; }
		}

		public int SubDivisionNumber
		{
			get { return mSubDivisionNumber; }
			set { mSubDivisionNumber = value; }
		}

		public bool DisplaySubGrid
		{
			get { return mDisplaySubGrid; }
			set { mDisplaySubGrid = value; }
		}

		public bool DisplayGrid
		{
			get { return mDisplayGrid; }
			set { mDisplayGrid = value; }
		}

		public Color GridColor
		{
			get { return mGridLinePen.Color; }
			set	{ mGridLinePen.Color = value; }
		}

		public Color SubGridColor
		{
			get { return mSubGridLinePen.Color; }
			set	{ mSubGridLinePen.Color = value; }
		}

		public float GridThickness
		{
			get { return mGridLinePen.Width; }
			set { mGridLinePen.Width = value; }
		}

		public float SubGridThickness
		{
			get { return mSubGridLinePen.Width; }
			set { mSubGridLinePen.Width = value; }
		}

		public bool DisplayCellIndex
		{
			get { return mDisplayCellIndex; }
			set { mDisplayCellIndex = value; }
		}

		public Font CellIndexFont
		{
			get { return mCellIndexFont; }
			set { mCellIndexFont = value; }
		}

		public Color CellIndexColor
		{
			get { return mCellIndexBrush.Color; }
			set { mCellIndexBrush.Color = value; }
		}

		public CellIndexType CellIndexColumnType
		{
			get { return mCellIndexColumnType; }
			set { mCellIndexColumnType = value; }
		}

		public CellIndexType CellIndexRowType
		{
			get { return mCellIndexRowType; }
			set { mCellIndexRowType = value; }
		}

		public int CellIndexCornerX
		{
			get { return mCellIndexCorner.X; }
			set { mCellIndexCorner.X = value; }
		}

		public int CellIndexCornerY
		{
			get { return mCellIndexCorner.Y; }
			set { mCellIndexCorner.Y = value; }
		}
		#endregion

		#region constructor/copy
		/// <summary>
		/// constructor
		/// </summary>
		public LayerGrid()
		{
			sCellIndexStringFormat.Alignment = StringAlignment.Center;
			sCellIndexStringFormat.LineAlignment = StringAlignment.Center;
		}

		/// <summary>
		/// copy only the option parameters from the specified layer
		/// </summary>
		/// <param name="layerToCopy">the model to copy from</param>
		public override void CopyOptionsFrom(Layer layerToCopy)
		{
			// call the base method
			base.CopyOptionsFrom(layerToCopy);
			// and try to cast in grid layer
			LayerGrid gridLayer = layerToCopy as LayerGrid;
			if (gridLayer != null)
			{
				mGridLinePen = gridLayer.mGridLinePen.Clone() as Pen;
				mSubGridLinePen = gridLayer.mSubGridLinePen.Clone() as Pen;
				mCellIndexFont = gridLayer.mCellIndexFont.Clone() as Font;
				mCellIndexBrush = gridLayer.mCellIndexBrush;
				mGridSizeInStud = gridLayer.mGridSizeInStud;
				mSubDivisionNumber = gridLayer.mSubDivisionNumber;
				mDisplayGrid = gridLayer.mDisplayGrid;
				mDisplaySubGrid = gridLayer.mDisplaySubGrid;
				mDisplayCellIndex = gridLayer.mDisplayCellIndex;
				mCellIndexColumnType = gridLayer.mCellIndexColumnType;
				mCellIndexRowType = gridLayer.mCellIndexRowType;
				mCellIndexCorner = gridLayer.mCellIndexCorner;
			}
		}

		public override int getNbItems()
		{
			return 1;
		}
		#endregion

		#region IXmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			base.ReadXml(reader);
			GridColor = XmlReadWrite.readColor(reader);
			GridThickness = reader.ReadElementContentAsFloat();
			SubGridColor = XmlReadWrite.readColor(reader);
			SubGridThickness = reader.ReadElementContentAsFloat();
			mGridSizeInStud = reader.ReadElementContentAsInt();
			mSubDivisionNumber = reader.ReadElementContentAsInt();
			if (reader.Name.Equals("DisplayGrid"))
				mDisplayGrid = reader.ReadElementContentAsBoolean();
			mDisplaySubGrid = reader.ReadElementContentAsBoolean();
			mDisplayCellIndex = reader.ReadElementContentAsBoolean();
			mCellIndexFont = XmlReadWrite.readFont(reader);
			CellIndexColor = XmlReadWrite.readColor(reader);
			mCellIndexColumnType = (CellIndexType)reader.ReadElementContentAsInt();
			mCellIndexRowType = (CellIndexType)reader.ReadElementContentAsInt();
			mCellIndexCorner = XmlReadWrite.readPoint(reader);
			// step the progress bar for the grid
			MainForm.Instance.stepProgressBar();
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteAttributeString("type", "grid");
			base.WriteXml(writer);
			XmlReadWrite.writeColor(writer, "GridColor", GridColor);
			writer.WriteElementString("GridThickness", GridThickness.ToString(System.Globalization.CultureInfo.InvariantCulture));
			XmlReadWrite.writeColor(writer, "SubGridColor", SubGridColor);
			writer.WriteElementString("SubGridThickness", SubGridThickness.ToString(System.Globalization.CultureInfo.InvariantCulture));
			writer.WriteElementString("GridSizeInStud", mGridSizeInStud.ToString());
			writer.WriteElementString("SubDivisionNumber", mSubDivisionNumber.ToString());
			writer.WriteElementString("DisplayGrid", mDisplayGrid.ToString().ToLower());
			writer.WriteElementString("DisplaySubGrid", mDisplaySubGrid.ToString().ToLower());
			writer.WriteElementString("DisplayCellIndex", mDisplayCellIndex.ToString().ToLower());
			XmlReadWrite.writeFont(writer, "CellIndexFont", mCellIndexFont);
			XmlReadWrite.writeColor(writer, "CellIndexColor", CellIndexColor);
			writer.WriteElementString("CellIndexColumnType", ((int)mCellIndexColumnType).ToString());
			writer.WriteElementString("CellIndexRowType", ((int)mCellIndexRowType).ToString());
			XmlReadWrite.writePoint(writer, "CellIndexCorner", mCellIndexCorner);
			// step the progress bar for the grid
			MainForm.Instance.stepProgressBar();
		}

		#endregion

		#region draw
		/// <summary>
		/// Draw the layer.
		/// </summary>
		/// <param name="g">the graphic context in which draw the layer</param>
		/// <param name="area">the area in layer pixel</param>
		public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
		{
			if (!Visible)
				return;

			float endX = (float)((areaInStud.Width + 1) * scalePixelPerStud);
			float endY = (float)((areaInStud.Height + 1) * scalePixelPerStud);

			// draw the grid and sub grid
			if (mDisplayGrid || mDisplaySubGrid)
			{
				int subGridSizeInStud = mGridSizeInStud / mSubDivisionNumber;
				float subGridSizeInPixel = (float)(subGridSizeInStud * scalePixelPerStud);
				float startX = (float)((-(areaInStud.Left % subGridSizeInStud)) * scalePixelPerStud);
				float startY = (float)((-(areaInStud.Top % subGridSizeInStud)) * scalePixelPerStud);

				int numLineDrawn = (int)areaInStud.Left / subGridSizeInStud;
				for (float x = startX; x < endX; x += subGridSizeInPixel)
				{
					if (mDisplayGrid && ((numLineDrawn % mSubDivisionNumber) == 0))
						g.DrawLine(mGridLinePen, new PointF(x, 0), new PointF(x, endY));
					else if (mDisplaySubGrid)
						g.DrawLine(mSubGridLinePen, new PointF(x, 0), new PointF(x, endY));
					numLineDrawn++;
				}

				numLineDrawn = (int)areaInStud.Top / subGridSizeInStud;
				for (float y = startY; y < endY; y += subGridSizeInPixel)
				{
					if (mDisplayGrid && ((numLineDrawn % mSubDivisionNumber) == 0))
						g.DrawLine(mGridLinePen, new PointF(0, y), new PointF(endX, y));
					else if (mDisplaySubGrid)
						g.DrawLine(mSubGridLinePen, new PointF(0, y), new PointF(endX, y));
					numLineDrawn++;
				}
			}

			// draw the cell index
			if (mDisplayCellIndex)
			{
				// --- COMMON VARIABLES
				// scale the font according to the zoom
				Font scaledFont = new Font(mCellIndexFont.FontFamily, (float)(mCellIndexFont.Size * scalePixelPerStud));
				// compute the half of the grid size
				int halfGridSizeInStud = mGridSizeInStud / 2;
				// compute the start position
				float gridSizeInPixel = (float)(mGridSizeInStud * scalePixelPerStud);

				// ---- COLUMN
				// compute all the start x
				int leftInGrid = (int)(areaInStud.Left / mGridSizeInStud) - mGridSizeInStud;
				int startXInGrid = mCellIndexCorner.X;
				if (startXInGrid < leftInGrid)
					startXInGrid = leftInGrid; // we clamp the part outside the screen
				float startX = (float)(((startXInGrid * mGridSizeInStud) + halfGridSizeInStud - areaInStud.Left) * scalePixelPerStud);
				// iterate for the columns
				int cellCornerYInStud = mCellIndexCorner.Y * mGridSizeInStud;
				if ((cellCornerYInStud + mGridSizeInStud > areaInStud.Top) && (cellCornerYInStud < areaInStud.Bottom))
				{
					int currentIndex = leftInGrid - mCellIndexCorner.X;
					if (currentIndex < 0)
						currentIndex = 0;
					float y = (float)((cellCornerYInStud + halfGridSizeInStud - areaInStud.Top) * scalePixelPerStud);
					for (float x = startX; x < endX; x += gridSizeInPixel)
					{
						string indexInText = getIndexInString(currentIndex, mCellIndexColumnType);
						g.DrawString(indexInText, scaledFont, mCellIndexBrush, x, y, sCellIndexStringFormat);
						++currentIndex;
					}
				}

				// --- ROW ---
				// compute all the start y
				int topInGrid = (int)(areaInStud.Top / mGridSizeInStud) - mGridSizeInStud;
				int startYInGrid = mCellIndexCorner.Y;
				if (startYInGrid < topInGrid)
					startYInGrid = topInGrid; // we clamp the part outside the screen
				float startY = (float)(((startYInGrid * mGridSizeInStud) + halfGridSizeInStud - areaInStud.Top) * scalePixelPerStud);
				// iterate for the rows
				int cellCornerXInStud = mCellIndexCorner.X * mGridSizeInStud;
				if ((cellCornerXInStud + mGridSizeInStud > areaInStud.Left) && (cellCornerXInStud < areaInStud.Right))
				{
					int currentIndex = topInGrid - mCellIndexCorner.Y;
					if (currentIndex < 0)
						currentIndex = 0;
					float x = (float)((cellCornerXInStud + halfGridSizeInStud - areaInStud.Left) * scalePixelPerStud);
					for (float y = startY; y < endY; y += gridSizeInPixel)
					{
						string indexInText = getIndexInString(currentIndex, mCellIndexRowType);
						g.DrawString(indexInText, scaledFont, mCellIndexBrush, x, y, sCellIndexStringFormat);
						++currentIndex;
					}
				}
			}
		}

		private string getIndexInString(int index, CellIndexType indexType)
		{
			// skip the index 0
			if (index == 0)
				return "";
			// return incremented letters or numbers
			if (indexType == CellIndexType.LETTERS)
			{
				string result = "";
				// just compute a number in base 26 (26 letters in the alphabet)
				const string numbase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
				int rest = index;
				int modulo;
				do
				{
					modulo = (rest - 1) % 26;
					result = numbase[modulo] + result;
					if ((rest % 26) == 0)
					{
						rest /= 26;
						rest--;
					}
					else
					{
						rest /= 26;
					}
				} while (rest > 0);
				return result;
			}
			else
			{
				return index.ToString();
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
			// We can do nothing on the grid
			return Cursors.No;
		}

		/// <summary>
		/// This function is called to know if this layer is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if this layer wants to handle it</returns>
		public override bool handleMouseDown(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			// since we dont want to have the selection in rectangle to be displayed
			// we reply that we are always interested in the left click mouse event
			// even if we do nothing with it after
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
			// the grid handle none click
			return false;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud)
		{
			// the grid handle none click
			return false;
		}

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseUp(MouseEventArgs e, PointF mouseCoordInStud)
		{
			// the grid handle none click
			return false;
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public override void selectInRectangle(RectangleF selectionRectangeInStud)
		{
			// nothing to select on a grid
		}
		#endregion
	}
}
