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
	public partial class LayerText : Layer
	{
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

		#region set/get
		/// <summary>
		/// get the localized name of this type of layer
		/// </summary>
		public override string TypeLocalizedName
		{
			get { return Properties.Resources.ErrorMsgLayerTypeText; }
		}

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

		/// <summary>
		/// Get the number of texts in this layer.
		/// </summary>
		public override int NbItems
		{
			get { return mTexts.Count; }
		}
		#endregion

		#region constructor
		/// <summary>
		///	Constructor
		/// </summary>
		public LayerText()
		{
		}
		#endregion

		#region IXmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			// call the common reader class
			base.ReadXml(reader);

			// read all the texts
			ReadXml<TextCell>(reader, ref mTexts, true);
		}

		public override void ReadXml<T>(System.Xml.XmlReader reader, ref List<T> resultingList, bool useProgressBar)
		{
			// clear all the content of the hash table
			LayerItem.sHashtableForGroupRebuilding.Clear();

			// the text cells
			bool cellFound = reader.ReadToDescendant("TextCell");
			while (cellFound)
			{
				// instanciate a new text cell, read and add the new text cell
				TextCell cell = new TextCell();
				cell.ReadXml(reader);
				resultingList.Add(cell as T);

				// read the next text cell
				cellFound = reader.ReadToNextSibling("TextCell");

				// step the progress bar for each text cell
				if (useProgressBar)
					MainForm.Instance.stepProgressBar();
			}
			// read the TextCells tag, to finish the list of text cells
			reader.Read();

			// call the post read function to read the groups
			readGroupFromXml(reader);
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			// call the function on all the bricks
			WriteXml(writer, mTexts, true);
		}

		protected override void WriteXml<T>(System.Xml.XmlWriter writer, List<T> itemsToWrite, bool useProgressBar)
		{
			// layer of type text
			writer.WriteStartElement("Layer");
			writer.WriteAttributeString("type", "text");
			writer.WriteAttributeString("id", this.GetHashCode().ToString());

			// call base class for common attribute
			base.WriteXml(writer);
			// and the text cell list
			writer.WriteStartElement("TextCells");
			foreach (T item in itemsToWrite)
			{
				item.WriteXml(writer);
				// step the progress bar for each text cell
				if (useProgressBar)
					MainForm.Instance.stepProgressBar();
			}
			writer.WriteEndElement(); // end of TextCells

			// call the post write to write the group list
			writeGroupToXml(writer);
			writer.WriteEndElement(); // end of Layer
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
		public override void copyCurrentSelectionToClipboard()
		{
			base.copyCurrentSelectionToClipboard(mTexts);
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

					// compute the selection area in pixel if the text is selected or we need to draw the hull
					bool isSelected = mSelectedObjects.Contains(cell);
					bool displayHull = Properties.Settings.Default.DisplayHull;
					PointF[] hull = null;
					if (isSelected || displayHull)
						hull = Layer.sConvertPolygonInStudToPixel(cell.SelectionArea.Vertice, areaInStud, scalePixelPerStud);

					// draw the hull if needed
					if (displayHull)
						g.DrawPolygon(sPentoDrawHull, hull);

					// draw a frame around the selected cell while the text size is still in pixel
					if (isSelected)
						g.FillPolygon(mSelectionBrush, hull);
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
						this.copyCurrentSelectionToClipboard();
						this.pasteClipboardInLayer();
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
						mLastDuplicateAction.updatePositionShift(deltaMove.X, deltaMove.Y);
						mLastDuplicateAction = null;
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
				mLastDuplicateAction = null;
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
