﻿// BlueBrick, a LEGO(c) layout editor.
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
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace BlueBrick.MapData
{
	[Serializable]
	public partial class LayerRuler : Layer
	{
		public enum EditTool
		{
			SELECT,
			LINE,
			CIRCLE
		}
		// the current edition tool
		private static EditTool sCurrentEditTool = EditTool.SELECT;

		// all the rulers in the layer
		private List<RulerItem> mRulers = new List<RulerItem>();

		// the image attribute to draw the text including the layer transparency
		private ImageAttributes mImageAttribute = new ImageAttributes();

		// variable used during the edition
		private RulerItem mCurrentRulerUnderMouse = null;
		private LinearRuler mCurrentlyEditedRuler = null;
		private CircularRuler mCurrentlyEditedCircle = null;
		private bool mIsEditingOffsetOfRuler = false;
		private const int BASE_SELECTION_TRANSPARENCY = 112;
		private SolidBrush mSelectionBrush = new SolidBrush(Color.FromArgb(BASE_SELECTION_TRANSPARENCY, 255, 255, 255));
		private PointF mMouseDownInitialPosition;
		private PointF mMouseDownLastPosition;
		private bool mMouseIsBetweenDownAndUpEvent = false;
		private bool mMouseHasMoved = false;
		private bool mMouseMoveIsADuplicate = false;
		private bool mMouseMoveWillAddOrEditRuler = false;
		private bool mMouseIsMovingControlPoint = false;
		private bool mMouseIsScalingRuler = false;

		#region set/get
		public static EditTool CurrentEditTool
		{
			get { return sCurrentEditTool; }
			set { sCurrentEditTool = value; }
		}

		/// <summary>
		/// get the type name id of this type of layer used in the xml file (not localized)
		/// </summary>
		public override string XmlTypeName
		{
			get { return "ruler"; }
		}

		/// <summary>
		/// get the localized name of this type of layer
		/// </summary>
		public override string LocalizedTypeName
		{
			get { return Properties.Resources.ErrorMsgLayerTypeRuler; }
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
		/// Get the number of Rulers in this layer.
		/// </summary>
		public override int NbItems
		{
			get { return mRulers.Count; }
		}
		#endregion

		#region constructor
		public LayerRuler()
		{
		}
		#endregion

		#region IXmlSerializable Members

		public override void ReadXml(System.Xml.XmlReader reader)
		{
			// call the common reader class
			base.ReadXml(reader);
			// read all the rulers
			readItemsListFromXml<RulerItem>(reader, ref mRulers, "RulerItems", true);
		}

		protected override T readItem<T>(System.Xml.XmlReader reader)
		{
			// instanciate the correct ruler
			RulerItem ruler = null;
			if (reader.Name.Equals("LinearRuler"))
				ruler = new LinearRuler();
			else
				ruler = new CircularRuler();
			// then call the read function
			ruler.ReadXml(reader);
			return (ruler as T);
		}

		public override void WriteXml(System.Xml.XmlWriter writer)
		{
			// write the header
			writeHeaderAndCommonProperties(writer);
			// write all the bricks
			writeItemsListToXml(writer, mRulers, "RulerItems", true);
			// write the footer
			writeFooter(writer);
		}
		#endregion

		#region action on the layer
		#region add/remove rulers
		/// <summary>
		///	Add the specified ruler at the specified position.
		///	If the position is negative, add the item at the end
		/// </summary>
		public void addRulerItem(RulerItem rulerToAdd, int index)
		{
			if (index < 0)
				mRulers.Add(rulerToAdd);
			else
				mRulers.Insert(index, rulerToAdd);
		}

		/// <summary>
		/// Remove the specified ruler item
		/// </summary>
		/// <param name="rulerToRemove">the ruler item to remove from the layer</param>
		/// <returns>the previous index of the ruler item deleted</returns>
		public int removeRulerItem(RulerItem rulerToRemove)
		{
			int index = mRulers.IndexOf(rulerToRemove);
			if (index >= 0)
			{
				mRulers.Remove(rulerToRemove);
				// remove also the item from the selection list if in it
				if (mSelectedObjects.Contains(rulerToRemove))
					removeObjectFromSelection(rulerToRemove);
			}
			else
				index = 0;
			return index;
		}
		#endregion

		#region selection
		/// <summary>
		/// Copy the list of the selected texts in a separate list for later use.
		/// This method should be called on a CTRL+C
		/// </summary>
		public override void copyCurrentSelectionToClipboard()
		{
			base.copyCurrentSelectionToClipboard(mRulers);
		}

		/// <summary>
		/// Select all the items in this layer.
		/// </summary>
		public override void selectAll()
		{
			// clear the selection and add all the item of this layer
			clearSelection();
			addObjectInSelection(mRulers);
		}
		#endregion
		#endregion

		#region util functions
		/// <summary>
		/// Compute the distance in stud between the given point and the currently edited ruler.
		/// </summary>
		/// <param name="pointInStud">the point in stud coord for which you want to know the distance</param>
		/// <returns>the distance in stud</returns>
		private float computePointDistanceFromCurrentRuler(PointF pointInStud)
		{
			float distance = 0.0f;
			if (mCurrentlyEditedRuler != null)
			{
				// get the two vector to make a vectorial product
				PointF unitVector = mCurrentlyEditedRuler.UnitVector;
				PointF point1ToMouse = new PointF(pointInStud.X - mCurrentlyEditedRuler.Point1.X, pointInStud.Y - mCurrentlyEditedRuler.Point1.Y);
				// compute the vectorial product (x and y are null cause z is null):
				distance = (point1ToMouse.X * unitVector.Y) - (point1ToMouse.Y * unitVector.X);
			}
			return distance;
		}

		/// <summary>
		/// This function tells if the specified point is just above at least one control point of any
		/// ruler in this layer. A control point for a linear ruler is one of its two extremity and for 
		/// a circular ruler it is its center. A certain distance is used to check if the point is above.
		/// If more than one control point is a possible candidate, the closer one is chosen.
		/// If no candidate are found, the specified concernedRulerItem is not changed.
		/// </summary>
		/// <param name="pointInStud">the position to check in stud coord</param>
		/// <param name="concernedRulerItem">the ruler items that owns the found control point</param>
		/// <returns>true if the specified position is near a control point</returns>
		private bool isPointAboveAnyRulerControlPoint(PointF pointInStud, ref RulerItem concernedRulerItem)
		{
			bool candidateFound = false; // use a flag to avoid touching the ref concernedRulerItem in case of not found
			float bestSquareDistance = 5.0f; // TODO for now use a fixed 5 studs, but the distance should be computed in pixels so this should be multiply by some scale 
			foreach (RulerItem item in mRulers)
			{
				float currentSquareDistance = item.findClosestControlPointAndComputeSquareDistance(pointInStud);
				if (currentSquareDistance < bestSquareDistance)
				{
					concernedRulerItem = item;
					bestSquareDistance = currentSquareDistance;
					candidateFound = true;
				}
			}
			// return true if we found a candidate
			return candidateFound;
		}

		/// <summary>
		/// This function tells if the specified position is above at least one scaling handle of any
		/// ruler in this layer. The scaling handle for a linear ruler is the line (possibly offseted) 
		/// and for a circular ruler it the circle (both within a certain thickness).
		/// </summary>
		/// <param name="pointInStud">the position to check in stud coord</param>
		/// <returns>true if the specified position is above any scaling handle</returns>
		private bool isPointAboveAnyRulerScalingHandle(PointF pointInStud)
		{
			return false; // TODO
		}		
		#endregion

		#region ruler attachement
		/// <summary>
		/// This function iterate through the selection and check if any ruler is attached to a part.
		/// </summary>
		/// <returns>true if at least one selected ruler is attached</returns>
		private bool areSelectedItemsAttached()
		{
			// if any one is attached, stop searchingn the whole group is attached
			foreach (LayerItem item in this.SelectedObjects)
				if ((item as RulerItem).IsAttached)
					return true;
			return false;
		}
		#endregion

		#region draw
		/// <summary>
		/// get the total area in stud covered by all the ruler items in this layer
		/// </summary>
		/// <returns></returns>
		public override RectangleF getTotalAreaInStud()
		{
			return getTotalAreaInStud(mRulers);
		}

		/// <summary>
		/// Draw the layer.
		/// </summary>
		/// <param name="g">the graphic context in which draw the layer</param>
		public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
		{
			if (!Visible)
				return;

			// draw all the rulers of the layer
			foreach (RulerItem ruler in mRulers)
				ruler.draw(g, areaInStud, scalePixelPerStud, mTransparency,
							mImageAttribute, mSelectedObjects.Contains(ruler), mSelectionBrush);

			// draw the ruler we are currently creating if any
			if (mCurrentlyEditedRuler != null)
				mCurrentlyEditedRuler.draw(g, areaInStud, scalePixelPerStud, mTransparency, mImageAttribute, false, mSelectionBrush);
			if (mCurrentlyEditedCircle != null)
				mCurrentlyEditedCircle.draw(g, areaInStud, scalePixelPerStud, mTransparency, mImageAttribute, false, mSelectionBrush);

			// call the base class to draw the surrounding selection rectangle
			base.draw(g, areaInStud, scalePixelPerStud);
		}
		#endregion

		#region mouse event
		/// <summary>
		/// This method use some criteria to determines if the mouse should be considered like above
		/// a ruler control point or a ruler scaling handle. The criteria are:
		/// First there should be no ruler selected or only one selected,
		/// Then the multiple selection modifier key and duplicate modifier key should not be pressed,
		/// And finally the mouse coord specified in parameter should be near a control point or
		/// scale handle. this method update the two private flags mMouseIsMovingControlPoint
		/// and mMouseIsScalingRuler. This method was create to factorised code (need to be checked
		/// in different mouse event)
		/// </summary>
		/// <param name="mouseCoordInStud">the mouse coordinate in stud</param>
		private void evaluateIfPointIsAboveControlPointOrScaleHandle(PointF mouseCoordInStud)
		{
			bool multipleSelectionPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey);
			bool duplicationPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);

			// check if we will move a control point or grab a scaling handle of a ruler
			// this is only possible when only one ruler is selected (so empty selection, or just one)
			// but not when a group of ruler is selected, and of course not when modifier keys are pressed
			if (!multipleSelectionPressed && !duplicationPressed && (this.SelectedObjects.Count <= 1))
			{
				// for moving a point, we need to have the mouse above a control point
				// if not this function doesn't change the ruler in reference
				mMouseIsMovingControlPoint = isPointAboveAnyRulerControlPoint(mouseCoordInStud, ref mCurrentRulerUnderMouse);
				// if we are not above a control point, maybe we are above a scale handle
				if (!mMouseIsMovingControlPoint)
					mMouseIsScalingRuler = isPointAboveAnyRulerScalingHandle(mouseCoordInStud);
			}
		}

		/// <summary>
		/// Return the cursor that should be display when the mouse is above the map without mouse click
		/// </summary>
		/// <param name="mouseCoordInStud"></param>
		public override Cursor getDefaultCursorWithoutMouseClick(PointF mouseCoordInStud)
		{
			// if the layer is not visible you can basically do nothing on it
			if (!Visible)
				return MainForm.Instance.HiddenLayerCursor;

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:
					if (mMouseIsBetweenDownAndUpEvent)
					{
						// the second test after the or, is because we give a second chance to the user to duplicate
						// the selection if he press the duplicate key after the mouse down, but before he start to move
						if (mMouseMoveIsADuplicate ||
							(!mMouseHasMoved && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)))
						{
							return MainForm.Instance.RulerDuplicateCursor;
						}
						else if (!mMouseMoveWillAddOrEditRuler)
						{
							if (mMouseIsMovingControlPoint)
								return MainForm.Instance.RulerMovePointCursor;
							else if (mMouseIsScalingRuler)
								return MainForm.Instance.RulerMoveCursor; // TODO: call a function
						}
					}
					else
					{
						if (mouseCoordInStud != PointF.Empty)
						{
							if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)
							{
								if (isPointInsideSelectionRectangle(mouseCoordInStud))
									return MainForm.Instance.RulerDuplicateCursor;
							}
							else if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
							{
								return MainForm.Instance.RulerSelectionCursor;
							}
							else
							{
								evaluateIfPointIsAboveControlPointOrScaleHandle(mouseCoordInStud);
								if (mMouseIsMovingControlPoint)
									return MainForm.Instance.RulerMovePointCursor;
								else if (mMouseIsScalingRuler)
									return MainForm.Instance.RulerMoveCursor; // TODO: call a function							}
							}
						}
					}
					return MainForm.Instance.RulerArrowCursor;
				case EditTool.LINE:
					return MainForm.Instance.RulerAddPoint1Cursor;
				case EditTool.CIRCLE:
					return MainForm.Instance.RulerAddCircleCursor;
			}

			// return the default cursor
			return MainForm.Instance.RulerArrowCursor;
		}

		/// <summary>
		/// Get the ruler item under the specified mouse coordinate or null if there's no ruler item under.
		/// The search is done in reverse order of the list to get the topmost item.
		/// </summary>
		/// <param name="mouseCoordInStud">the coordinate of the mouse cursor, where to look for</param>
		/// <returns>the ruler item that is under the mouse coordinate or null if there is none.</returns>
		public RulerItem getRulerItemUnderMouse(PointF mouseCoordInStud)
		{
			return getLayerItemUnderMouse(mRulers, mouseCoordInStud) as RulerItem;
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

			// flag to check if we will handle the mouse down
			bool willHandleMouse = false;

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:
					// boolean flags for the keyboard control keys
					bool multipleSelectionPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey);
					bool duplicationPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);

					// check if the mouse is inside the bounding rectangle of the selected objects
					bool isMouseInsideSelectedObjects = isPointInsideSelectionRectangle(mouseCoordInStud);

					// clear the selection if we click outside the selection without any key pressed
					if (!isMouseInsideSelectedObjects && !multipleSelectionPressed && !duplicationPressed)
						clearSelection();

					// compute the current ruler under the mouse
					mCurrentRulerUnderMouse = null;

					// We search if there is a cell under the mouse but in priority we choose from the current selected cells
					mCurrentRulerUnderMouse = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as RulerItem;

					// if the current selected ruler is not under the mouse we search among the other rulers
					// but in reverse order to choose first the brick on top
					if (mCurrentRulerUnderMouse == null)
						mCurrentRulerUnderMouse = getRulerItemUnderMouse(mouseCoordInStud);

					// save a flag that tell if it is a simple move or a duplicate of the selection
					// Be carreful for a duplication we take only the selected objects, not the cell
					// under the mouse that may not be selected
					mMouseMoveIsADuplicate = isMouseInsideSelectedObjects && duplicationPressed;

					// now check if we will move a control point or scale handle.
					// this method update mMouseIsMovingControlPoint and mMouseIsScalingRuler
					evaluateIfPointIsAboveControlPointOrScaleHandle(mouseCoordInStud);

					// check if the user plan to move the selected items
					// for that of course we must not have a modifier key pressed
					// we should also not do a control point move neither handle scaling
					// and none of the selected objects must be attached
					bool willMoveSelectedObject = !multipleSelectionPressed && !duplicationPressed
												&& !mMouseIsMovingControlPoint && !mMouseIsScalingRuler
												&& ((isMouseInsideSelectedObjects && !areSelectedItemsAttached()) ||
													((mCurrentRulerUnderMouse != null) && (!mCurrentRulerUnderMouse.IsAttached)));

					// we will add or edit a text if we double click
					mMouseMoveWillAddOrEditRuler = (e.Clicks == 2);

					// select the appropriate cursor:
					if (mMouseMoveIsADuplicate)
						preferedCursor = MainForm.Instance.RulerDuplicateCursor;
					else if (willMoveSelectedObject)
						preferedCursor = MainForm.Instance.RulerMoveCursor;
					else if (mMouseMoveWillAddOrEditRuler)
						preferedCursor = MainForm.Instance.RulerArrowCursor; //TODO I think we should use another cursor here
					else if (mMouseIsMovingControlPoint)
						preferedCursor = MainForm.Instance.RulerMovePointCursor;
					else if (mMouseIsScalingRuler)
						preferedCursor = MainForm.Instance.RulerMoveCursor; // TODO: call a function
					else
						preferedCursor = MainForm.Instance.RulerArrowCursor;

					// handle the mouse down if we duplicate or move the selected texts, or edit a text
					willHandleMouse = (mMouseMoveIsADuplicate || willMoveSelectedObject || mMouseMoveWillAddOrEditRuler || mMouseIsMovingControlPoint || mMouseIsScalingRuler);
					break;

				case EditTool.LINE:
					// check if we are finishing the edition of the ruler by moving the offset,
					// in that case it is the click to fix the offset
					if (!mIsEditingOffsetOfRuler)
						preferedCursor = MainForm.Instance.RulerAddPoint2Cursor;
					// we handle all the click when editing a ruler
					willHandleMouse = true;
					break;

				case EditTool.CIRCLE:
					//TODO
					willHandleMouse = true;
					break;
			}

			// return the result flag
			return willHandleMouse;
		}

		/// <summary>
		/// This function is called to know if this layer is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if this layer wants to handle it</returns>
		public override bool handleMouseMoveWithoutClick(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			if (mIsEditingOffsetOfRuler)
			{
				float orientation = mCurrentlyEditedRuler.Orientation;
				if (orientation > 157.5f)
					preferedCursor = MainForm.Instance.RulerOffsetHorizontalCursor;
				else if (orientation > 112.5f)
					preferedCursor = MainForm.Instance.RulerOffsetDiagonalDownCursor;
				else if (orientation > 67.5f)
					preferedCursor = MainForm.Instance.RulerOffsetVerticalCursor;
				else if (orientation > 22.5f)
					preferedCursor = MainForm.Instance.RulerOffsetDiagonalUpCursor;
				else if (orientation > -22.5f)
					preferedCursor = MainForm.Instance.RulerOffsetHorizontalCursor;
				else if (orientation > -67.5f)
					preferedCursor = MainForm.Instance.RulerOffsetDiagonalDownCursor;
				else if (orientation > -112.5f)
					preferedCursor = MainForm.Instance.RulerOffsetVerticalCursor;
				else if (orientation > -157.5f)
					preferedCursor = MainForm.Instance.RulerOffsetDiagonalUpCursor;
				else
					preferedCursor = MainForm.Instance.RulerOffsetHorizontalCursor;
			}
			return mIsEditingOffsetOfRuler;
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

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:
					// we select the ruler under the mouse if the selection list is empty
					if ((mCurrentRulerUnderMouse != null) && !mMouseMoveIsADuplicate)
					{
						// if the selection is empty add the ruler item, else check the control key state
						if ((mSelectedObjects.Count == 0) && (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey))
						{
							addObjectInSelection(mCurrentRulerUnderMouse);
						}
						mustRefresh = true;
					}

					// record the initial position of the mouse
					mMouseDownInitialPosition = mouseCoordInStud;
					mMouseDownLastPosition = mouseCoordInStud;
					mMouseHasMoved = false;
					break;

				case EditTool.LINE:
					if (!mIsEditingOffsetOfRuler)
						mCurrentlyEditedRuler = new LinearRuler(mouseCoordInStud, mouseCoordInStud);
					mustRefresh = true;
					break;

				case EditTool.CIRCLE:
					mCurrentlyEditedCircle = new CircularRuler(mouseCoordInStud, 0.0f);
					break;
			}

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud)
		{
			bool mustRefresh = false;

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:
					// update the control point if it's what we are doing
					if (mMouseIsMovingControlPoint && (mCurrentRulerUnderMouse != null))
					{
						mCurrentRulerUnderMouse.CurrentControlPoint = mouseCoordInStud;
						mustRefresh = true;
					}
					break;

				case EditTool.LINE:
					if (mCurrentlyEditedRuler != null)
					{
						// adjust the offset or the second point
						if (mIsEditingOffsetOfRuler)
							mCurrentlyEditedRuler.OffsetDistance = computePointDistanceFromCurrentRuler(mouseCoordInStud);
						else
							mCurrentlyEditedRuler.Point2 = mouseCoordInStud;
						mustRefresh = true;
					}
					break;

				case EditTool.CIRCLE:
					if (mCurrentlyEditedCircle != null)
					{
						mCurrentlyEditedCircle.OnePointOnCircle = mouseCoordInStud;
						mustRefresh = true;
					}
					break;
			}

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseUp(MouseEventArgs e, PointF mouseCoordInStud)
		{
			bool mustRefresh = false;

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:
					if (mMouseIsMovingControlPoint)
					{
						// TODO need to create an action to modify the ruler
						mustRefresh = true;
					}
					mMouseIsMovingControlPoint = false;
					mMouseIsScalingRuler = false;
					mMouseMoveWillAddOrEditRuler = false;
					mCurrentRulerUnderMouse = null;
					break;

				case EditTool.LINE:
					if (mIsEditingOffsetOfRuler)
					{
						mCurrentlyEditedRuler.OffsetDistance = computePointDistanceFromCurrentRuler(mouseCoordInStud);
						Actions.ActionManager.Instance.doAction(new Actions.Rulers.AddRuler(this, mCurrentlyEditedRuler));
						mCurrentlyEditedRuler = null;
						mIsEditingOffsetOfRuler = false;
					}
					else
					{
						mCurrentlyEditedRuler.Point2 = mouseCoordInStud;
						mIsEditingOffsetOfRuler = true;
					}
					mustRefresh = true;
					break;

				case EditTool.CIRCLE:
					if (mCurrentlyEditedCircle != null)
					{
						mCurrentlyEditedCircle.OnePointOnCircle = mouseCoordInStud;
						Actions.ActionManager.Instance.doAction(new Actions.Rulers.AddRuler(this, mCurrentlyEditedCircle));
						mCurrentlyEditedCircle = null;
						mustRefresh = true;
					}
					break;
			}

			mMouseIsBetweenDownAndUpEvent = false;

			return mustRefresh;
		}

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public override void selectInRectangle(RectangleF selectionRectangeInStud)
		{
			selectInRectangle(selectionRectangeInStud, mRulers);
		}
		#endregion
	}
}
