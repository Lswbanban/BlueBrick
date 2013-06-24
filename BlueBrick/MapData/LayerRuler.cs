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

		// variable for selection drawing
		private const int BASE_SELECTION_TRANSPARENCY = 112;
		private SolidBrush mSelectionBrush = new SolidBrush(Color.FromArgb(BASE_SELECTION_TRANSPARENCY, 255, 255, 255));
		// variable used during the edition
		private RulerItem mCurrentRulerUnderMouse = null;
		private RulerItem mCurrentRulerWithHighlightedControlPoint = null;
		private RulerItem mCurrentlyEditedRuler = null;
		private LayerBrick.Brick mCurrentBrickUsedForRulerAttachement = null;
		// variable for mouse state
		private PointF mMouseDownInitialPosition;
		private PointF mMouseDownLastPosition;
		private bool mMouseIsBetweenDownAndUpEvent = false;
		private bool mMouseHasMoved = false;
		private bool mMouseMoveIsADuplicate = false;
		private bool mMouseMoveWillCustomizeRuler = false; // true for a double click when we will call the option window to change properties of a ruler (color, mesurement unit, etc...)
		private bool mMouseIsMovingControlPoint = false; // true when moving one of the two points of a linear ruler or the center of a circular ruler
		private bool mMouseIsScalingRuler = false; // true when moving the offset of a linear ruler, or changing the radius of a circular ruler

		#region set/get
		public static EditTool CurrentEditTool
		{
			get { return sCurrentEditTool; }
			set
			{
				sCurrentEditTool = value;
				// update the view because we shoudl show/hide the control point of the selection depending on the type of tool
				MainForm.Instance.updateView(Actions.Action.UpdateViewType.LIGHT, Actions.Action.UpdateViewType.NONE);
			}
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

		/// <summary>
		/// Return the current ruler item on this layer which has one of his control point highlighted
		/// </summary>
		public RulerItem CurrentRulerWithHighlightedControlPoint
		{
			get { return mCurrentRulerWithHighlightedControlPoint; }
		}

		/// <summary>
		/// Return the current ruler item on this layer which has one of his control point highlighted
		/// </summary>
		public LayerBrick.Brick CurrentBrickUsedForRulerAttachement
		{
			get { return mCurrentBrickUsedForRulerAttachement; }
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
		/// This function tells if the specified point is just above at least one control point of any
		/// ruler in this layer. A control point for a linear ruler is one of its two extremity and for 
		/// a circular ruler it is its center. A certain distance is used to check if the point is above.
		/// If more than one control point is a possible candidate, the closer one is chosen but priority
		/// is given to the selected item is if two candidate are at the exact same position (which
		/// can happen when two rulers are attached to the same brick center for example).
		/// If no candidate are found, the specified concernedRulerItem is not changed.
		/// </summary>
		/// <param name="pointInStud">the position to check in stud coord</param>
		/// <param name="concernedRulerItem">the ruler items that owns the found control point</param>
		/// <returns>true if the specified position is near a control point</returns>
		private bool isPointAboveAnyRulerControlPoint(PointF pointInStud, ref RulerItem concernedRulerItem)
		{
			// We want the distance fixed in pixel (so the snapping is always the same no matter the scale)
			// so divide the pixel snapping distance by the scale to get a variable distance in stud
			float bestSquareDistance = (float)BlueBrick.Properties.Settings.Default.RulerControlPointRadiusInPixel / (float)MainForm.Instance.MapViewScale;
			bestSquareDistance *= bestSquareDistance; //square it

			// check if the highlighted ruler will change
			RulerItem previousHighlightedRuler = mCurrentRulerWithHighlightedControlPoint;
			mCurrentRulerWithHighlightedControlPoint = null;

			// is there a selected ruler? if yes check first with it to take it in priority.
			if (mSelectedObjects.Count == 1)
			{
				RulerItem item = (mSelectedObjects[0] as RulerItem);
				float currentSquareDistance = item.findClosestControlPointAndComputeSquareDistance(pointInStud);
				if (currentSquareDistance < bestSquareDistance)
				{
					concernedRulerItem = item;
					mCurrentRulerWithHighlightedControlPoint = item;
					bestSquareDistance = currentSquareDistance;
				}
			}

			// iterate on all the rulers to find the nearest control point
			foreach (RulerItem item in mRulers)
			{
				float currentSquareDistance = item.findClosestControlPointAndComputeSquareDistance(pointInStud);
				if (currentSquareDistance < bestSquareDistance)
				{
					concernedRulerItem = item;
					mCurrentRulerWithHighlightedControlPoint = item;
					bestSquareDistance = currentSquareDistance;
				}
			}

			// check if we need to update the view panel
			if (mCurrentRulerWithHighlightedControlPoint != previousHighlightedRuler)
				MainForm.Instance.updateView(Actions.Action.UpdateViewType.LIGHT, Actions.Action.UpdateViewType.NONE);

			// return true if we found a good candidate
			return (mCurrentRulerWithHighlightedControlPoint != null);
		}

		/// <summary>
		/// This function tells if the specified position is above at least one scaling handle of any
		/// ruler in this layer. The scaling handle for a linear ruler is the line (possibly offseted) 
		/// and for a circular ruler it the circle (both within a certain thickness).
		/// </summary>
		/// <param name="pointInStud">the position to check in stud coord</param>
		/// <returns>true if the specified position is above any scaling handle</returns>
		private bool isPointAboveAnyRulerScalingHandle(PointF pointInStud, ref RulerItem concernedRulerItem)
		{
			// We want the distance fixed in pixel (so the snapping is always the same no matter the scale)
			// so divide the pixel snapping distance by the scale to get a variable distance in stud
			float thicknessInStud = (float)BlueBrick.Properties.Settings.Default.RulerControlPointRadiusInPixel / (float)MainForm.Instance.MapViewScale;

			// iterate on the ruler in reverse order to get the one on top first
			for (int i = mRulers.Count - 1; i >= 0; i--)
			{
				RulerItem item = mRulers[i];
				if (item.isInsideAScalingHandle(pointInStud, thicknessInStud))
				{
					concernedRulerItem = item;
					return true;
				}
			}
			// we didn't find any handle
			return false;
		}

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
		/// <returns>the ruler which is under the specified point or null if there's not</returns>
		private RulerItem evaluateIfPointIsAboveControlPointOrScaleHandle(PointF mouseCoordInStud)
		{
			bool multipleSelectionPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey);
			bool duplicationPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);
			RulerItem result = null;

			// check if we will move a control point or grab a scaling handle of a ruler
			// this is only possible when only one ruler is selected (so empty selection, or just one)
			// but not when a group of ruler is selected, and of course not when modifier keys are pressed
			if (!multipleSelectionPressed && !duplicationPressed && (this.SelectedObjects.Count <= 1))
			{
				// for moving a point, we need to have the mouse above a control point
				// if not this function doesn't change the ruler in reference
				mMouseIsMovingControlPoint = isPointAboveAnyRulerControlPoint(mouseCoordInStud, ref result);
				// if we are not above a control point, maybe we are above a scale handle
				if (!mMouseIsMovingControlPoint)
					mMouseIsScalingRuler = isPointAboveAnyRulerScalingHandle(mouseCoordInStud, ref result);
			}
			// return the found ruler if any
			return result;
		}
		#endregion

		#region ruler attachement
		/// <summary>
		/// This function iterate through the selection and check if any ruler is attached to a part.
		/// </summary>
		/// <returns>true if at least one selected ruler is attached</returns>
		private bool areSelectedItemsFullyAttached()
		{
			// if any one is not fully attached, stop searching cause we will be able to move something
			foreach (LayerItem item in this.SelectedObjects)
				if (!(item as RulerItem).IsFullyAttached)
					return false;
			return true;
		}

		/// <summary>
		/// Tell if the mouse is currently in position to attach a ruler
		/// </summary>
		public bool canAttachRuler()
		{
			if ((mCurrentRulerWithHighlightedControlPoint != null) && !mCurrentRulerWithHighlightedControlPoint.IsCurrentControlPointAttached)
			{
				PointF currentControlPointPosition = mCurrentRulerWithHighlightedControlPoint.CurrentControlPoint;
				mCurrentBrickUsedForRulerAttachement = Map.Instance.getTopMostBrickUnderMouse(currentControlPointPosition);
				return (mCurrentBrickUsedForRulerAttachement != null);
			}
			return false;
		}

		/// <summary>
		/// Tell if the mouse is currently in position to detach a ruler
		/// </summary>
		public bool canDetachRuler()
		{
			if ((mCurrentRulerWithHighlightedControlPoint != null) && mCurrentRulerWithHighlightedControlPoint.IsCurrentControlPointAttached)
			{
				mCurrentBrickUsedForRulerAttachement = mCurrentRulerWithHighlightedControlPoint.BrickAttachedToCurrentControlPoint;		
				return (mCurrentBrickUsedForRulerAttachement != null);
			}
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

			// draw the ruler that we are currently creating if any
			// (if it's the same as the ruler under the mouse
			// that means we are not creating a new one but editing an existing one)
			if ((mCurrentlyEditedRuler != null) && (mCurrentlyEditedRuler != mCurrentRulerUnderMouse))
				mCurrentlyEditedRuler.draw(g, areaInStud, scalePixelPerStud, mTransparency, mImageAttribute, false, mSelectionBrush);

			if (sCurrentEditTool == EditTool.SELECT)
			{
				// draw the control points of the selected rulers
				if (BlueBrick.Properties.Settings.Default.DisplayRulerAttachPoints)
				{
					Color redColor = Color.FromArgb((int)(mTransparency * 2.55f), Color.Red);
					foreach (LayerItem item in this.SelectedObjects)
						if (item != mCurrentRulerWithHighlightedControlPoint)
							(item as RulerItem).drawControlPoints(g, areaInStud, scalePixelPerStud, redColor);
				}

				// draw the control point near the mouse
				Color orangeColor = Color.FromArgb((int)(mTransparency * 2.55f), Color.Orange);
				if (mCurrentRulerWithHighlightedControlPoint != null)
					mCurrentRulerWithHighlightedControlPoint.drawControlPoints(g, areaInStud, scalePixelPerStud, orangeColor);
			}

			// call the base class to draw the surrounding selection rectangle
			base.draw(g, areaInStud, scalePixelPerStud);
		}
		#endregion

		#region mouse event
		/// <summary>
		/// Return the correct Cursor for scaling ruler given the specified orientation of the scaling handle
		/// </summary>
		/// <param name="orientation">orientation of the handle in degrees</param>
		/// <returns>the best looking cursor</returns>
		private Cursor getScalingCursorFromOrientation(float orientation)
		{
			// careful the orientation is not in trigo direction but inversed
			if (orientation > 157.5f)
				return MainForm.Instance.RulerScaleHorizontalCursor;
			else if (orientation > 112.5f)
				return MainForm.Instance.RulerScaleDiagonalUpCursor;
			else if (orientation > 67.5f)
				return MainForm.Instance.RulerScaleVerticalCursor;
			else if (orientation > 22.5f)
				return MainForm.Instance.RulerScaleDiagonalDownCursor;
			else if (orientation > -22.5f)
				return MainForm.Instance.RulerScaleHorizontalCursor;
			else if (orientation > -67.5f)
				return MainForm.Instance.RulerScaleDiagonalUpCursor;
			else if (orientation > -112.5f)
				return MainForm.Instance.RulerScaleVerticalCursor;
			else if (orientation > -157.5f)
				return MainForm.Instance.RulerScaleDiagonalDownCursor;
			else
				return MainForm.Instance.RulerScaleHorizontalCursor;
		}

		/// <summary>
		/// Return the cursor that should be display when the mouse is above the map without mouse click
		/// </summary>
		/// <param name="mouseCoordInStud">the mouse coordinate in stud</param>
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
						else if (!mMouseMoveWillCustomizeRuler)
						{
							if (mMouseIsMovingControlPoint)
								return MainForm.Instance.RulerMovePointCursor;
							else if (mMouseIsScalingRuler && (mCurrentlyEditedRuler != null))
								return getScalingCursorFromOrientation(mCurrentlyEditedRuler.getScalingOrientation(mouseCoordInStud));
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
								// first check if we are inside the selection rectangle
								bool isInside = isPointInsideSelectionRectangle(mouseCoordInStud);

								// if we are inside with a group of ruler, it will move them
								if (isInside && (mSelectedObjects.Count > 1))
									return MainForm.Instance.RulerMoveCursor;

								// if we have 0 or one ruler selected, we need to check if we will modify one ruler
								RulerItem editableRuler = evaluateIfPointIsAboveControlPointOrScaleHandle(mouseCoordInStud);

								// if we are above one ruler inside the selection but which is not the selection, we won't edit it
								if (isInside && (mSelectedObjects.Count == 1) && (mSelectedObjects[0] != editableRuler))
								{
									// cancel the highlighted ruler
									mCurrentRulerWithHighlightedControlPoint = null;
									return MainForm.Instance.RulerMoveCursor;
								}
								
								// this is all the other cases:
								// 1) either we are outside the selection
								// 2) or the selection is empty
								// 3) or there's just one item selected, the mouse is inside, but the mouse is above the selected ruler
								mCurrentRulerUnderMouse = editableRuler;
								if (mMouseIsMovingControlPoint)
									return MainForm.Instance.RulerMovePointCursor;
								else if (mMouseIsScalingRuler && (mCurrentRulerUnderMouse != null))
									return getScalingCursorFromOrientation(mCurrentRulerUnderMouse.getScalingOrientation(mouseCoordInStud));
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
					bool isMouseOutsideSelectedObjectsWithoutModifier = !isMouseInsideSelectedObjects && !multipleSelectionPressed && !duplicationPressed;

					// clear the selection if we click outside the selection without any key pressed
					if (isMouseOutsideSelectedObjectsWithoutModifier)
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

					// now check if we will move a control point or scale handle if not inside the selection rectangle
					if (isMouseOutsideSelectedObjectsWithoutModifier || (mSelectedObjects.Count == 1))
					{
						// this method update mMouseIsMovingControlPoint and mMouseIsScalingRuler
						RulerItem editableRuler = evaluateIfPointIsAboveControlPointOrScaleHandle(mouseCoordInStud);
						// assign the edited ruler if we are editing its point or scale it (after evaluation)
						if ((mMouseIsMovingControlPoint || mMouseIsScalingRuler) &&
							(isMouseOutsideSelectedObjectsWithoutModifier || (mSelectedObjects[0] == editableRuler)))
						{
							mCurrentRulerUnderMouse = editableRuler;
							mCurrentlyEditedRuler = editableRuler;
						}
						else
						{
							// cancel the highlighted ruler in that case
							mCurrentRulerWithHighlightedControlPoint = null;
							mMouseIsMovingControlPoint = false;
							mMouseIsScalingRuler = false;
						}
					}

					// check if the user plan to move the selected items
					// for that of course we must not have a modifier key pressed
					// we should also not do a control point move neither handle scaling
					// and none of the selected objects must be attached
					bool willMoveSelectedObject = !multipleSelectionPressed && !duplicationPressed
												&& !mMouseIsMovingControlPoint && !mMouseIsScalingRuler
												&& ((isMouseInsideSelectedObjects && !areSelectedItemsFullyAttached()) ||
													((mCurrentRulerUnderMouse != null) && (!mCurrentRulerUnderMouse.IsFullyAttached)));

					// we will add or edit a text if we double click
					mMouseMoveWillCustomizeRuler = (e.Clicks == 2);

					// select the appropriate cursor:
					if (mMouseMoveIsADuplicate)
						preferedCursor = MainForm.Instance.RulerDuplicateCursor;
					else if (willMoveSelectedObject)
						preferedCursor = MainForm.Instance.RulerMoveCursor;
					else if (mMouseMoveWillCustomizeRuler)
						preferedCursor = MainForm.Instance.RulerArrowCursor; //TODO I think we should use another cursor here
					else if (mMouseIsMovingControlPoint)
						preferedCursor = MainForm.Instance.RulerMovePointCursor;
					else if (mMouseIsScalingRuler)
						preferedCursor = getScalingCursorFromOrientation(mCurrentRulerUnderMouse.getScalingOrientation(mouseCoordInStud));
					else
						preferedCursor = MainForm.Instance.RulerArrowCursor;

					// handle the mouse down if we duplicate or move the selected texts, or edit a text
					willHandleMouse = (mMouseMoveIsADuplicate || willMoveSelectedObject || mMouseMoveWillCustomizeRuler || mMouseIsMovingControlPoint || mMouseIsScalingRuler);
					break;

				case EditTool.LINE:
					// check if we are finishing the edition of the ruler by moving the offset,
					// in that case it is the click to fix the offset
					if (!mMouseIsScalingRuler)
						preferedCursor = MainForm.Instance.RulerAddPoint2Cursor;
					// we handle all the click when editing a ruler
					willHandleMouse = true;
					break;

				case EditTool.CIRCLE:
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
			if (mMouseIsScalingRuler && (mCurrentlyEditedRuler != null))
			{
				preferedCursor = getScalingCursorFromOrientation(mCurrentlyEditedRuler.getScalingOrientation(mouseCoordInStud));
				// for now only handle it if we are editing a linear ruler
				return true;
			}
			return false;
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
			mMouseHasMoved = false;
			bool mustRefresh = false;
			// snap the mouse coord to the grid
			PointF mouseCoordInStudSnapped = getSnapPoint(mouseCoordInStud);

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
					if (mCurrentlyEditedRuler != null)
					{
						if (mMouseIsMovingControlPoint)
						{
							// if we are moving a control point, use the position of the control point instead of the mouse position
							mMouseDownInitialPosition = mCurrentlyEditedRuler.CurrentControlPoint;
							mMouseDownLastPosition = mMouseDownInitialPosition;
						}
						else if (mMouseIsScalingRuler)
						{
							mMouseDownInitialPosition = mCurrentlyEditedRuler.getReferencePointForScale();
							mMouseDownLastPosition = mouseCoordInStudSnapped;
						}
					}
					else
					{
						// for moving several rulers
						mMouseDownInitialPosition = mouseCoordInStudSnapped;
						mMouseDownLastPosition = mouseCoordInStudSnapped;
					}
					break;

				case EditTool.LINE:
					mMouseDownInitialPosition = mouseCoordInStudSnapped;
					mMouseDownLastPosition = mouseCoordInStudSnapped;
					if (!mMouseIsScalingRuler)
						mCurrentlyEditedRuler = new LinearRuler(mouseCoordInStudSnapped, mouseCoordInStudSnapped);
					break;

				case EditTool.CIRCLE:
					mMouseDownInitialPosition = mouseCoordInStudSnapped;
					mMouseDownLastPosition = mouseCoordInStudSnapped;
					// for the creation of a circle the center start on mouse click and we
					// immediatly go to the scaling of the circle
					mCurrentlyEditedRuler = new CircularRuler(mouseCoordInStudSnapped, 0.0f);					
					mMouseIsScalingRuler = true;
					break;
			}

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public override bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			bool mustRefresh = false;
			// snap the mouse coord to the grid
			PointF mouseCoordInStudSnapped = getSnapPoint(mouseCoordInStud);
			// compute the delta move of the mouse
			PointF deltaMove = new PointF(mouseCoordInStudSnapped.X - mMouseDownLastPosition.X, mouseCoordInStudSnapped.Y - mMouseDownLastPosition.Y);
			// set the flag that indicate that we moved the mouse
			if (deltaMove.X != 0.0f || deltaMove.Y != 0.0)
				mMouseHasMoved = true;			
			// memorize the last position of the mouse
			mMouseDownLastPosition = mouseCoordInStudSnapped;

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:
					// check if the delta move is not null
					if (mMouseHasMoved)
					{
						// check if we are actually editing a ruler
						if (mCurrentlyEditedRuler != null)
						{
							// update the control point if it's what we are doing
							if (mMouseIsMovingControlPoint)
							{
								// move the control point
								if (!mCurrentlyEditedRuler.IsCurrentControlPointAttached ||
									mCurrentlyEditedRuler.BrickAttachedToCurrentControlPoint.SelectionArea.isPointInside(mouseCoordInStudSnapped))
									mCurrentlyEditedRuler.CurrentControlPoint = mouseCoordInStudSnapped;
								// move or update the bounding rectangle
								if (mCurrentlyEditedRuler is CircularRuler)
								{
									// when moving a control point of a Circular ruler, we just shift the circle
									this.moveBoundingSelectionRectangle(deltaMove);
								}
								else
								{
									// when moving a control point of a linear ruler, the ruler is deformed, so we need to recompute it
									this.updateBoundingSelectionRectangle();
								}
								mustRefresh = true;
							}
							else if (mMouseIsScalingRuler) // or scale it
							{
								mCurrentlyEditedRuler.scaleToPoint(mouseCoordInStudSnapped);
								// update the cursor if it is a circular ruler
								if (mCurrentlyEditedRuler is CircularRuler)
									preferedCursor = getScalingCursorFromOrientation(mCurrentlyEditedRuler.getScalingOrientation(mouseCoordInStudSnapped));
								// update the bounding selection rectangle
								this.updateBoundingSelectionRectangle();
								mustRefresh = true;
							}
						}
						else if (mSelectedObjects.Count > 0)
						{
							bool wereRulersJustDuplicated = false;
							// check if it is a move or a duplicate
							if (mMouseMoveIsADuplicate)
							{
								// this is a duplicate, if we didn't move yet, this is the moment to copy  and paste the selection
								// and this will change the current selection, that will be move normally after
								if (!mMouseHasMoved)
								{
									this.copyCurrentSelectionToClipboard();
									this.pasteClipboardInLayer(AddOffsetAfterPaste.NO);
									// set the flag
									wereRulersJustDuplicated = true;
								}
							}
							// the duplication above will change the current selection
							// The code below is to move the selection, either the original one or the duplicated one
							bool isAnyRulerAttached = false;
							foreach (LayerItem item in mSelectedObjects)
							{
								item.Center = new PointF(item.Center.X + deltaMove.X, item.Center.Y + deltaMove.Y);
								isAnyRulerAttached = isAnyRulerAttached || !((item as RulerItem).IsNotAttached);
							}
							// move also the bounding rectangle
							if (isAnyRulerAttached)
								this.updateBoundingSelectionRectangle();
							else
								moveBoundingSelectionRectangle(deltaMove);
							// after we moved the selection check if we need to refresh the current highlighted brick
							if (wereRulersJustDuplicated)
								mCurrentRulerUnderMouse = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as RulerItem;
							// refresh the view
							mustRefresh = true;
						}
					}
					else if ((mSelectedObjects.Count > 0) && !mMouseHasMoved && !mMouseMoveIsADuplicate)
					{
						// give a second chance to duplicate if the user press the duplicate key
						// after pressing down the mouse key, but not if the user already moved
						mMouseMoveIsADuplicate = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);
					}

					break;

				case EditTool.LINE:
					LinearRuler linearRuler = mCurrentlyEditedRuler as LinearRuler;
					if ((linearRuler != null) && mMouseHasMoved)
					{
						// adjust the offset or the second point
						if (mMouseIsScalingRuler)
							linearRuler.scaleToPoint(mouseCoordInStudSnapped);
						else
							linearRuler.Point2 = mouseCoordInStudSnapped;
						mustRefresh = true;
					}
					break;

				case EditTool.CIRCLE:
					if (mMouseHasMoved)
					{
						// scale the ruler
						mCurrentlyEditedRuler.scaleToPoint(mouseCoordInStudSnapped);
						// update also the prefered cursor because we may move the mouse while scaling
						preferedCursor = getScalingCursorFromOrientation(mCurrentlyEditedRuler.getScalingOrientation(mouseCoordInStudSnapped));
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
			// snap the mouse coord to the grid
			PointF mouseCoordInStudSnapped = getSnapPoint(mouseCoordInStud);
			// compute the delta move of the mouse
			PointF deltaMove = new PointF(mouseCoordInStudSnapped.X - mMouseDownInitialPosition.X, mouseCoordInStudSnapped.Y - mMouseDownInitialPosition.Y);

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:

					// check if we moved the selected bricks
					if (mMouseHasMoved && (mSelectedObjects.Count > 0))
					{
						// reset the flag
						mMouseHasMoved = false;

						// create a new action for this move
						if ((deltaMove.X != 0) || (deltaMove.Y != 0))
						{
							if (mMouseIsMovingControlPoint)
							{
								// compute the final position before reseting the current position to the initial one
								PointF finalPosition = mCurrentlyEditedRuler.CurrentControlPoint;
								if (!mCurrentlyEditedRuler.IsCurrentControlPointAttached ||
									mCurrentlyEditedRuler.BrickAttachedToCurrentControlPoint.SelectionArea.isPointInside(mouseCoordInStudSnapped))
									finalPosition = mouseCoordInStudSnapped;
								// move back the control point because for a linear ruler, because a swap of
								// current control point can happen if you move the two control point at the same place
								mCurrentlyEditedRuler.CurrentControlPoint = mMouseDownInitialPosition;
								// and create an action
								Actions.ActionManager.Instance.doAction(new Actions.Rulers.MoveRulerControlPoint(this, mCurrentlyEditedRuler, mMouseDownInitialPosition, finalPosition));
							}
							else if (mMouseIsScalingRuler)
							{
								// no need to rescale back the ruler
								// and create an action
								Actions.ActionManager.Instance.doAction(new Actions.Rulers.ScaleRuler(this, mCurrentlyEditedRuler, mMouseDownInitialPosition, mouseCoordInStudSnapped));
							}
							else if (mMouseMoveIsADuplicate)
							{
								// update the duplicate action or add a move action
								mLastDuplicateAction.updatePositionShift(deltaMove.X, deltaMove.Y);
								mLastDuplicateAction = null;
							}
							else
							{
								// reset the initial position to each ruler
								foreach (LayerItem item in mSelectedObjects)
									item.Position = new PointF(item.Position.X - deltaMove.X, item.Position.Y - deltaMove.Y);
								// this is a simple move of rulers
								Actions.ActionManager.Instance.doAction(new Actions.Rulers.MoveRulers(this, mSelectedObjects, deltaMove));
							}

							// if we moved we did something
							mustRefresh = true;
						}
					}
					else
					{
						// if we didn't move the item and use the control key, we need to add or remove object from the selection
						// we must do it in the up event because if we do it in the down, we may remove an object before moving
						// we do this only if the mMouseHasMoved flag is not set to avoid this change if we move
						if ((mCurrentRulerUnderMouse != null) && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey))
						{
							if (mSelectedObjects.Contains(mCurrentRulerUnderMouse))
								removeObjectFromSelection(mCurrentRulerUnderMouse);
							else
								addObjectInSelection(mCurrentRulerUnderMouse);
						}
					}

					mMouseIsMovingControlPoint = false;
					mMouseIsScalingRuler = false;
					mMouseMoveWillCustomizeRuler = false;
					mCurrentRulerUnderMouse = null;
					mCurrentlyEditedRuler = null;
					break;

				case EditTool.LINE:
					LinearRuler linearRuler = mCurrentlyEditedRuler as LinearRuler;
					if (linearRuler != null)
					{
						if (mMouseIsScalingRuler)
						{
							linearRuler.scaleToPoint(mouseCoordInStudSnapped);
							Actions.ActionManager.Instance.doAction(new Actions.Rulers.AddRuler(this, linearRuler));
							mCurrentlyEditedRuler = null;
							mMouseIsScalingRuler = false;
						}
						else
						{
							linearRuler.Point2 = mouseCoordInStudSnapped;
							mMouseIsScalingRuler = true;
						}
						mustRefresh = true;
					}
					break;

				case EditTool.CIRCLE:
					mCurrentlyEditedRuler.scaleToPoint(mouseCoordInStudSnapped);
					Actions.ActionManager.Instance.doAction(new Actions.Rulers.AddRuler(this, mCurrentlyEditedRuler));
					mCurrentlyEditedRuler = null;
					mustRefresh = true;
					mMouseIsScalingRuler = false;
					break;
			}

			mMouseIsBetweenDownAndUpEvent = false;
			mMouseHasMoved = false;

			// if we have finished to edit the current ruler, change the tool back to edition
			if ((mCurrentlyEditedRuler == null) && (Properties.Settings.Default.SwitchToEditionAfterRulerCreation))
				MainForm.Instance.rulerSelectAndEditToolStripMenuItem_Click(null, null);

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

		/// <summary>
		/// This method return a snap point near the specified point according to different
		/// snapping rules that are specific of this brick layer:
		/// For now, if the snapping is enable return the closest point of the grid
		/// </summary>
		/// <param name="pointInStud">the rough point to snap</param>
		/// <returns>a near snap point</returns>
		public PointF getSnapPoint(PointF pointInStud)
		{
			// don't do anything is the snapping is not enabled
			if (SnapGridEnabled)
				return Layer.snapToGrid(pointInStud, true);

			// by default do not change anything
			return pointInStud;
		}
		#endregion
	}
}
