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
using BlueBrick.Actions.Texts;
using System.Drawing.Imaging;

namespace BlueBrick.MapData
{
	[Serializable]
	public class LayerText : Layer
	{
		[Serializable]
		public class TextCell : LayerItem
		{
			private StringFormat mTextStringFormat = new StringFormat();
			private Font mTextFont = Properties.Settings.Default.DefaultTextFont;
			private SolidBrush mTextBrush = new SolidBrush(Properties.Settings.Default.DefaultTextColor);
			private string mText = "";
			private Bitmap mImage = new Bitmap(1, 1);	// image representing the text to draw in the correct orientation

			#region get/set
			public string Text
			{
				get { return mText; }
				set { mText = value; updateBitmap(); }
			}

			public override float Orientation
			{
				set	{ mOrientation = value;	updateBitmap();	}
			}

			public Color FontColor
			{
				get { return mTextBrush.Color; }
				set { mTextBrush.Color = value; updateBitmap(); }
			}

			public StringAlignment TextAlignment
			{
				get { return mTextStringFormat.Alignment; }
				set { mTextStringFormat.Alignment = value; updateBitmap(); }
			}

			public Font Font
			{
				get { return mTextFont; }
				set { mTextFont = value; updateBitmap(); }
			}

			public Bitmap Image
			{
				get { return mImage; }
			}
			#endregion

			#region constructor
			/// <summary>
			/// The paramererless constructor is used for serialization, it should not be used by the program
			/// </summary>
			public TextCell()
			{
				mTextStringFormat.LineAlignment = StringAlignment.Center;
			}

			public TextCell(string text, Font font, Color color, StringAlignment alignment)
			{
				init(text, font, color, alignment);
			}

			/// <summary>
			/// Clone this TextCell
			/// </summary>
			/// <returns>a new TextCell which is a conform copy of this</returns>
			public TextCell Clone()
			{
				TextCell result = new TextCell();
				result.mDisplayArea = this.mDisplayArea;
				result.mOrientation = this.mOrientation;
				// call the init after setting the orientation to compute the image in the right orientation
				// the init method will initialize mImage, mConnectionPoints and mSnapToGridOffsetFromTopLeftCorner
				result.init(this.Text, this.Font, this.FontColor, this.TextAlignment);
				// return the cloned value
				return result;
			}

			private void init(string text, Font font, Color color, StringAlignment alignment)
			{
				mTextStringFormat.Alignment = alignment;
				mTextStringFormat.LineAlignment = StringAlignment.Center;
				// set parameter directly to avoid calling several time the rebuild of picture
				mText = text;
				mTextBrush.Color = color;
				// then finally use an accessor in order to create the picture
				this.Font = font;
			}

			#endregion

			#region IXmlSerializable Members

			public override void ReadXml(System.Xml.XmlReader reader)
			{
				base.ReadXml(reader);
				// avoid using the accessor to reduce the number of call of updateBitmap
				mText = reader.ReadElementContentAsString();
				mOrientation = reader.ReadElementContentAsFloat();
				mTextBrush.Color = XmlReadWrite.readColor(reader);
				mTextFont = XmlReadWrite.readFont(reader);
				// for the last use the accessor to recreate the bitmap
				string alignment = reader.ReadElementContentAsString();
				if (alignment.Equals("Near"))
					TextAlignment = StringAlignment.Near;
				else if (alignment.Equals("Far"))
					TextAlignment = StringAlignment.Far;
				else
					TextAlignment = StringAlignment.Center;
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				base.WriteXml(writer);
				writer.WriteElementString("Text", mText);
				writer.WriteElementString("Orientation", mOrientation.ToString(System.Globalization.CultureInfo.InvariantCulture));
				XmlReadWrite.writeColor(writer, "FontColor", FontColor);
				XmlReadWrite.writeFont(writer, "Font", mTextFont);
				writer.WriteElementString("TextAlignment", TextAlignment.ToString());
			}

			#endregion

			#region method
			private void updateBitmap()
			{
				// create a bitmap if the text is not empty
				if (mText != "")
				{
					// create a font to mesure the text
					Font textFont = new Font(mTextFont.FontFamily, mTextFont.Size, mTextFont.Style);

					Graphics graphics = Graphics.FromImage(mImage);
					SizeF textFontSize = graphics.MeasureString(mText, textFont);
					float halfWidth = textFontSize.Width * 0.5f;
					float halfHeight = textFontSize.Height * 0.5f;

					Matrix rotation = new Matrix();
					rotation.Rotate(mOrientation);
					// compute the rotated corners
					PointF[] corners = new PointF[] { new PointF(-halfWidth, -halfHeight), new PointF(-halfWidth, halfHeight), new PointF(halfWidth, halfHeight), new PointF(halfWidth, -halfHeight) };
					rotation.TransformVectors(corners);

					PointF min = corners[0];
					PointF max = corners[0];
					for (int i = 1; i < 4; ++i)
					{
						if (corners[i].X < min.X)
							min.X = corners[i].X;
						if (corners[i].Y < min.Y)
							min.Y = corners[i].Y;
						if (corners[i].X > max.X)
							max.X = corners[i].X;
						if (corners[i].Y > max.Y)
							max.Y = corners[i].Y;
					}
					// adjust the display area and selection area
					mDisplayArea.Width = Math.Abs(max.X - min.X);
					mDisplayArea.Height = Math.Abs(max.Y - min.Y);

					// adjust the selection area (after adjusting the display area sucha as the center properties is correct)
					Matrix translation = new Matrix();
					translation.Translate(Center.X, Center.Y);
					translation.TransformPoints(corners);

					// then create the new selection area
					mSelectionArea = new Tools.Polygon(corners);

					// now create a scaled font from the current one, to avoid aliasing
					const float FONT_SCALE = 4.0f;
					Font scaledTextFont = new Font(mTextFont.FontFamily, mTextFont.Size * FONT_SCALE, mTextFont.Style);
					mImage = new Bitmap(mImage, new Size((int)(mDisplayArea.Width * FONT_SCALE), (int)(mDisplayArea.Height * FONT_SCALE)));

					// compute the position where to draw according to the alignment (if centered == 0)
					float posx = 0;
					if (this.TextAlignment == StringAlignment.Far)
						posx = halfWidth;
					else if (this.TextAlignment == StringAlignment.Near)
						posx = -halfWidth;

					graphics = Graphics.FromImage(mImage);
					rotation.Translate(mImage.Width / 2, mImage.Height / 2, MatrixOrder.Append);
					graphics.Transform = rotation;
					graphics.Clear(Color.Transparent);
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.DrawString(mText, scaledTextFont, mTextBrush, posx * FONT_SCALE, 0, mTextStringFormat);
					graphics.Flush();
				}
			}
			#endregion
		}

		private List<TextCell> mTexts = new List<TextCell>();

		// the image attribute to draw the text including the layer transparency
		private ImageAttributes mImageAttribute = new ImageAttributes();

		// related to selection
		private const int BASE_SELECTION_TRANSPARENCY = 112;
		private TextCell mCurrentTextCellUnderMouse = null;
		private SolidBrush mSelectionBrush = new SolidBrush(Color.FromArgb(BASE_SELECTION_TRANSPARENCY, 255, 255, 255));
		private PointF mMouseDownInitialPosition;
		private PointF mMouseDownLastPosition;
		private bool mMouseIsBetweenDownAndUpEvent = false;
		private bool mMouseHasMoved = false;
		private bool mMouseMoveIsADuplicate = false;
		private bool mMouseMoveWillAddOrEditText = false;
		private DuplicateText mLastDuplicateTextAction = null; // temp reference use during a ALT+mouse move action (that duplicate and move the bricks at the same time)

		#region set/get
		public override int Transparency
		{
			set
			{
				mTransparency = value;
				ColorMatrix colorMatrix = new ColorMatrix();
				colorMatrix.Matrix33 = (float)value / 100.0f;
				mImageAttribute.SetColorMatrix(colorMatrix);
				mSelectionBrush = new SolidBrush(Color.FromArgb((BASE_SELECTION_TRANSPARENCY * value) / 100, 255, 255, 255));
			}
		}
		#endregion

		/// <summary>
		///	Constructor
		/// </summary>
		public LayerText()
		{
		}

		public override int getNbItems()
		{
			return mTexts.Count;
		}

		#region IXmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			base.ReadXml(reader);
			// the text cells
			bool cellFound = reader.ReadToDescendant("TextCell");
			while (cellFound)
			{
				// instanciate a new text cell, read and add the new text cell
				TextCell cell = new TextCell();
				cell.ReadXml(reader);
				mTexts.Add(cell);

				// read the next text cell
				cellFound = reader.ReadToNextSibling("TextCell");

				// step the progress bar for each text cell
				MainForm.Instance.stepProgressBar();
			}
			// read the TextCells tag, to finish the list of text cells
			reader.Read();

			// call the post read function to read the groups
			postReadXml(reader);
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteAttributeString("type", "text");
			base.WriteXml(writer);
			// and the text cell list
			writer.WriteStartElement("TextCells");
			foreach (TextCell cell in mTexts)
			{
				writer.WriteStartElement("TextCell");
				cell.WriteXml(writer);
				writer.WriteEndElement();
				// step the progress bar for each text cell
				MainForm.Instance.stepProgressBar();
			}
			writer.WriteEndElement();

			// call the post write to write the group list
			postWriteXml(writer);
		}

		#endregion

		#region action on the layer

		/// <summary>
		///	Add the specified text cell at the specified position
		/// </summary>
		public void addTextCell(TextCell cellToAdd, int index)
		{
			if (index < 0)
				mTexts.Add(cellToAdd);
			else
				mTexts.Insert(index, cellToAdd);
		}

		/// <summary>
		/// Remove the specified text cell
		/// </summary>
		/// <param name="cellToRemove"></param>
		/// <returns>the previous index of the cell deleted</returns>
		public int removeTextCell(TextCell cellToRemove)
		{
			int index = mTexts.IndexOf(cellToRemove);
			if (index >= 0)
			{
				mTexts.Remove(cellToRemove);
				// remove also the item from the selection list if in it
				if (mSelectedObjects.Contains(cellToRemove))
					removeObjectFromSelection(cellToRemove);
			}
			else
				index = 0;
			return index;
		}

		/// <summary>
		/// Copy the list of the selected texts in a separate list for later use.
		/// This method should be called on a CTRL+C
		/// </summary>
		public void copyCurrentSelection()
		{
			// reset the copy list
			sCopyItems.Clear();
			// Sort the seltected list as it is sorted on the layer such as the clone list
			// will be also sorted as on the layer
			LayerItemComparer<TextCell> comparer = new LayerItemComparer<TextCell>(mTexts);
			SelectedObjects.Sort(comparer);
			// recreate the copy list if the selection is not empty
			foreach (LayerItem item in SelectedObjects)
			{
				// add a duplicated item in the list (because the model may change between this copy and the paste)
				sCopyItems.Add((item as TextCell).Clone());
			}
			// enable the paste buttons
			MainForm.Instance.enablePasteButton(true);
		}

		/// <summary>
		/// Paste (duplicate) the list of bricks that was previously copied with a call to copyCurrentSelection()
		/// This method should be called on a CTRL+V
		/// </summary>
		public void pasteCopiedList()
		{
			// To paste, we need to have copied something
			if (sCopyItems.Count > 0)
			{
				mLastDuplicateTextAction = new DuplicateText(this, sCopyItems);
				ActionManager.Instance.doAction(mLastDuplicateTextAction);
			}
		}

		/// <summary>
		/// Select all the items in this layer.
		/// </summary>
		public override void selectAll()
		{
			// clear the selection and add all the item of this layer
			clearSelection();
			addObjectInSelection(mTexts);
		}
		#endregion

		#region draw

		/// <summary>
		/// get the total area in stud covered by all the text cells in this layer
		/// </summary>
		/// <returns></returns>
		public override RectangleF getTotalAreaInStud()
		{
			return getTotalAreaInStud(mTexts);
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

			Rectangle destinationRectangle = new Rectangle();
			foreach (TextCell cell in mTexts)
			{
				float left = cell.Position.X;
				float right = left + cell.Width;
				float top = cell.Position.Y;
				float bottom = top + cell.Height;
				if ((right >= areaInStud.Left) && (left <= areaInStud.Right) && (bottom >= areaInStud.Top) && (top <= areaInStud.Bottom))
				{
					// compute the position of the text in pixel coord
					destinationRectangle.X = (int)((left - areaInStud.Left) * scalePixelPerStud);
					destinationRectangle.Y = (int)((top - areaInStud.Top) * scalePixelPerStud);
					destinationRectangle.Width = (int)(cell.Width * scalePixelPerStud);
					destinationRectangle.Height = (int)(cell.Height * scalePixelPerStud);

					// draw the image containing the text
					g.DrawImage(cell.Image, destinationRectangle, 0, 0, cell.Image.Width, cell.Image.Height, GraphicsUnit.Pixel, mImageAttribute);

					// draw a frame around the selected cell while the text size is still in pixel
					if (mSelectedObjects.Contains(cell))
						g.FillPolygon(mSelectionBrush, Layer.sConvertPolygonInStudToPixel(cell.SelectionArea.Vertice, areaInStud, scalePixelPerStud));
				}
			}

			// call the base class to draw the surrounding selection rectangle
			base.draw(g, areaInStud, scalePixelPerStud);
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
			{
				return MainForm.Instance.HiddenLayerCursor;
			}
			else if (mMouseIsBetweenDownAndUpEvent)
			{
				// the second test after the or, is because we give a second chance to the user to duplicate
				// the selection if he press the duplicate key after the mouse down, but before he start to move
				if (mMouseMoveIsADuplicate ||
					(!mMouseHasMoved && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)))
					return MainForm.Instance.TextDuplicateCursor;
				else if (!mMouseMoveWillAddOrEditText)
					return Cursors.SizeAll;
			}
			else
			{
				if (mouseCoordInStud != PointF.Empty)
				{
					if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)
					{
						if (isPointInsideSelectionRectangle(mouseCoordInStud))
							return MainForm.Instance.TextDuplicateCursor;
					}
					else if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
					{
						return MainForm.Instance.TextSelectionCursor;
					}
				}
			}
			// return the default arrow cursor
			return MainForm.Instance.TextArrowCursor;
		}

		/// <summary>
		/// Get the text cell under the specified mouse coordinate or null if there's no text cell under.
		/// The search is done in reverse order of the list to get the topmost item.
		/// </summary>
		/// <param name="mouseCoordInStud">the coordinate of the mouse cursor, where to look for</param>
		/// <returns>the text cell that is under the mouse coordinate or null if there is none.</returns>
		public TextCell getTextCellUnderMouse(PointF mouseCoordInStud)
		{
			return getLayerItemUnderMouse(mTexts, mouseCoordInStud) as TextCell;
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

			// check if the mouse is inside the bounding rectangle of the selected objects
			bool isMouseInsideSelectedObjects = isPointInsideSelectionRectangle(mouseCoordInStud);
			if (!isMouseInsideSelectedObjects && (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
				&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey))
				clearSelection();
			
			// compute the current text cell under the mouse
			mCurrentTextCellUnderMouse = null;

			// We search if there is a cell under the mouse but in priority we choose from the current selected cells
			mCurrentTextCellUnderMouse = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as TextCell;

			// if the current selected text is not under the mouse we search among the other texts
			// but in reverse order to choose first the brick on top
			if (mCurrentTextCellUnderMouse == null)
				mCurrentTextCellUnderMouse = getTextCellUnderMouse(mouseCoordInStud);

			// save a flag that tell if it is a simple move or a duplicate of the selection
			// Be carreful for a duplication we take only the selected objects, not the cell
			// under the mouse that may not be selected
			mMouseMoveIsADuplicate = isMouseInsideSelectedObjects &&
									(Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);

			// check if the user plan to move the selected items
			bool willMoveSelectedObject = (isMouseInsideSelectedObjects || (mCurrentTextCellUnderMouse != null))
											&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
											&& (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);
			
			// we will add or edit a text if we double click
			mMouseMoveWillAddOrEditText = (e.Clicks == 2);

			// select the appropriate cursor:
			if (mMouseMoveIsADuplicate)
				preferedCursor = MainForm.Instance.TextDuplicateCursor;
			else if (willMoveSelectedObject)
				preferedCursor = Cursors.SizeAll;
			else if (mMouseMoveWillAddOrEditText)
				preferedCursor = MainForm.Instance.TextArrowCursor;
			else if (mCurrentTextCellUnderMouse == null)
				preferedCursor = Cursors.Cross;

			// handle the mouse down if we duplicate or move the selected texts, or edit a text
			return (mMouseMoveIsADuplicate || willMoveSelectedObject || mMouseMoveWillAddOrEditText);
		}

		/// <summary>
		/// This method is called if the map decided that this layer should handle
		/// this mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseDown(MouseEventArgs e, PointF mouseCoordInStud)
		{
			mMouseIsBetweenDownAndUpEvent = true;

			bool mustRefresh = false;

			// if finally we are called to handle this mouse down,
			// we add the cell under the mouse if the selection list is empty
			if ((mCurrentTextCellUnderMouse != null) && !mMouseMoveIsADuplicate)
			{
				// if the selection is empty add the text cell, else check the control key state
				if ((mSelectedObjects.Count == 0) && (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey))
				{
					addObjectInSelection(mCurrentTextCellUnderMouse);
				}
				mustRefresh = true;
			}

			// record the initial position of the mouse
			mMouseDownInitialPosition = mouseCoordInStud;
			mMouseDownLastPosition = mouseCoordInStud;
			mMouseHasMoved = false;

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud)
		{
			mMouseMoveWillAddOrEditText = false;

			if (mSelectedObjects.Count > 0)
			{
				// give a second chance to duplicate if the user press the duplicate key
				// after pressing down the mouse key, but not if the user already moved
				if (!mMouseHasMoved && !mMouseMoveIsADuplicate)
					mMouseMoveIsADuplicate = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);

				// check if it is a move or a duplicate
				if (mMouseMoveIsADuplicate)
				{
					// this is a duplicate, if we didn't move yet, this is the moment to copy  and paste the selection
					// and this will change the current selection, that will be move normally after
					if (!mMouseHasMoved)
					{
						this.copyCurrentSelection();
						this.pasteCopiedList();
					}
				}
				// compute the delta move of the mouse
				PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownLastPosition.X, mouseCoordInStud.Y - mMouseDownLastPosition.Y);
				// this is move of the selection, not a duplicate selection
				foreach (LayerText.TextCell cell in mSelectedObjects)
					cell.Position = new PointF(cell.Position.X + deltaMove.X, cell.Position.Y + deltaMove.Y);
				// move also the bounding rectangle
				moveBoundingSelectionRectangle(deltaMove);
				// memorize the last position of the mouse
				mMouseDownLastPosition = mouseCoordInStud;
				// reset the current brick under the mouse such as we will not remove or add it in the selection
				// in the up event
				mCurrentTextCellUnderMouse = null;
				// set the flag that indicate that we moved the mouse
				mMouseHasMoved = true;
				return true;
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
			// if it's a double click, we should prompt a box for text editing
			// WARNING: prompt the box in the mouse up event,
			// otherwise, if you do it in the mouse down, the mouse up is not triggered (both under dot net and mono)
			// and this can mess up the click count in mono
			if (mMouseMoveWillAddOrEditText)
			{
				// open the edit text dialog in modal
				EditTextForm editTextForm = new EditTextForm(mCurrentTextCellUnderMouse);
				editTextForm.ShowDialog();
				if (editTextForm.DialogResult == DialogResult.OK)
				{
					// check if it is an edition of an existing text or a new text
					if (mCurrentTextCellUnderMouse != null)
						ActionManager.Instance.doAction(new EditText(this, mCurrentTextCellUnderMouse, editTextForm.EditedText, editTextForm.EditedFont, editTextForm.EditedColor, editTextForm.EditedAlignment));
					else
						ActionManager.Instance.doAction(new AddText(this, editTextForm.EditedText, editTextForm.EditedFont, editTextForm.EditedColor, editTextForm.EditedAlignment, mouseCoordInStud));
				}
			}
			else if (mMouseHasMoved && (mSelectedObjects.Count > 0)) // check if we moved the selected text
			{
				// reset the flag
				mMouseHasMoved = false;

				// compute the delta mouve of the mouse
				PointF deltaMove = new PointF(mouseCoordInStud.X - mMouseDownInitialPosition.X, mouseCoordInStud.Y - mMouseDownInitialPosition.Y);

				// create a new action for this move
				if ((deltaMove.X != 0) || (deltaMove.Y != 0))
				{
					// update the duplicate action or add a move action
					if (mMouseMoveIsADuplicate)
					{
						mLastDuplicateTextAction.updatePositionShift(deltaMove.X, deltaMove.Y);
						mLastDuplicateTextAction = null;
					}
					else
					{
						// reset the initial position to each text
						foreach (LayerText.TextCell cell in mSelectedObjects)
							cell.Position = new PointF(cell.Position.X - deltaMove.X, cell.Position.Y - deltaMove.Y);
						// and add an action
						ActionManager.Instance.doAction(new MoveText(this, mSelectedObjects, deltaMove));
					}
				}
				// reset anyway the temp reference for the duplication
				mLastDuplicateTextAction = null;
			}
			else
			{
				// if we didn't move the item and use the control key, we need to add or remove object from the selection
				// we must do it in the up event because if we do it in the down, we may remove an object before moving
				// in the move event we reset the mCurrentBrickUnderMouse to avoid this change if we move
				if ((mCurrentTextCellUnderMouse != null) && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey))
				{
					if (mSelectedObjects.Contains(mCurrentTextCellUnderMouse))
						removeObjectFromSelection(mCurrentTextCellUnderMouse);
					else
						addObjectInSelection(mCurrentTextCellUnderMouse);
				}
			}

			mMouseIsBetweenDownAndUpEvent = false;
			mMouseMoveWillAddOrEditText = false;
			mCurrentTextCellUnderMouse = null;

			// refresh in any case
			return true;
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public override void selectInRectangle(RectangleF selectionRectangeInStud)
		{
			selectInRectangle(selectionRectangeInStud, mTexts);
		}

		#endregion
	}
}
