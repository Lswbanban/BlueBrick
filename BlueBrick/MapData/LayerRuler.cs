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

		/// <summary>
		/// describe all the action that can be done with a mouse when editing a ruler
		/// </summary>
		private enum EditAction
		{
			NONE,
			MOVE_SELECTION,
			DUPLICATE_SELECTION,
			MOVE_CONTROL_POINT,
			SCALE_RULER,
			CUSTOMIZE_RULER,
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
		private EditAction mEditAction = EditAction.NONE;

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
		/// The ruler list is given as readonly for saving purpose in other format than the BB XML one
		/// </summary>
		public List<RulerItem> RulerList
		{
			get { return mRulers; }
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

		/// <summary>
		/// This method is used to sort items in a list in the same order as they are in the layer list.
		/// The items can be groups, in that case, we use the max index of all the leaf children.
		/// </summary>
		/// <param name="item1">the first item to compare</param>
		/// <param name="item2">the second item t compare</param>
		/// <returns>distance between the two items in the layer list (index1 - index2)</returns>
		public override int compareItemOrderOnLayer(Layer.LayerItem item1, Layer.LayerItem item2)
		{
			return compareItemOrderOnLayer(mRulers, item1, item2);
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

		/// <summary>
		/// This function is called during the loading of the map, after all layers and all items
		/// have been loaded, in order to recreate links between items of different layers (such as
		/// for example the attachement of a ruler to a brick)
		/// </summary>
		public override void recreateLinksAfterLoading()
		{
			foreach (RulerItem rulerItem in mRulers)
				rulerItem.recreateLinksAfterLoading();
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
		#region add/remove/modify rulers
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

		public override void editSelectedItemsProperties(PointF mouseCoordInStud)
		{
			// does nothing if the selection is empty
			if (mSelectedObjects.Count > 0)
			{
				// in priority get the item under the mouse, if there's several item selected
				RulerItem rulerToEdit = getLayerItemUnderMouse(mSelectedObjects, mouseCoordInStud) as RulerItem;
				// but if user click outside of the item, get the first one of the list
				if (rulerToEdit == null)
					rulerToEdit = mSelectedObjects[0] as RulerItem;
				// and call the function to edit the properties
				editRulerItem(rulerToEdit);
			}
		}

		private void editRulerItem(RulerItem itemToEdit)
		{
			// open the edit text dialog in modal
			EditRulerForm editRulerForm = new EditRulerForm(itemToEdit);
			editRulerForm.ShowDialog(MainForm.Instance);
			if (editRulerForm.DialogResult == DialogResult.OK)
				Actions.ActionManager.Instance.doAction(new Actions.Rulers.EditRuler(this, itemToEdit, editRulerForm.EditedRulerClone));
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

			// is there selected rulers? if yes check first with it to take it in priority.
			foreach (LayerItem item in mSelectedObjects)
			{
				RulerItem rulerItem = item as RulerItem;
                float currentSquareDistance = rulerItem.findClosestControlPointAndComputeSquareDistance(pointInStud);
				if (currentSquareDistance < bestSquareDistance)
				{
                    concernedRulerItem = rulerItem;
                    mCurrentRulerWithHighlightedControlPoint = rulerItem;
					bestSquareDistance = currentSquareDistance;
				}
			}

            //if we found something in the selection we stop here, otherwise extend the search to all rulers
            if (mCurrentRulerWithHighlightedControlPoint == null)
            {
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

			// is there selected rulers? if yes check first with it to take it in priority.
            foreach (LayerItem item in mSelectedObjects)
            {
                RulerItem rulerItem = item as RulerItem;
                if (rulerItem.isInsideAScalingHandle(pointInStud, thicknessInStud))
                {
                    concernedRulerItem = rulerItem;
                    return true;
                }
            }

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
		private RulerItem evaluateIfPointIsAboveControlPointOrScaleHandle(PointF mouseCoordInStud, out EditAction action)
		{
			bool multipleSelectionPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey);
			bool duplicationPressed = (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey);
			RulerItem result = null;
			action = EditAction.NONE;

			// check if we will move a control point or grab a scaling handle of a ruler
			// this is only possible when only one ruler is selected (so empty selection, or just one)
			// but not when a group of ruler is selected, and of course not when modifier keys are pressed
			if (!multipleSelectionPressed && !duplicationPressed)
			{
				// for moving a point, we need to have the mouse above a control point
				// if not this function doesn't change the ruler in reference
				if (isPointAboveAnyRulerControlPoint(mouseCoordInStud, ref result))
					action = EditAction.MOVE_CONTROL_POINT;
				// if we are not above a control point, maybe we are above a scale handle
				else if (isPointAboveAnyRulerScalingHandle(mouseCoordInStud, ref result))
					action = EditAction.SCALE_RULER;
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
				mCurrentBrickUsedForRulerAttachement = Map.Instance.getTopMostVisibleBrickUnderMouse(currentControlPointPosition);
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
		/// <param name="areaInStud">The region in which we should draw</param>
		/// <param name="scalePixelPerStud">The scale to use to draw</param>
		/// <param name="drawSelection">If true draw the selection rectangle and also the selection overlay (this can be set to false when exporting the map to an image)</param>
		public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, bool drawSelection)
		{
			if (!Visible)
				return;

			// draw all the rulers of the layer
			foreach (RulerItem ruler in mRulers)
				ruler.draw(g, areaInStud, scalePixelPerStud, mTransparency,
							mImageAttribute, drawSelection && mSelectedObjects.Contains(ruler), mSelectionBrush,
							// special case, if we don't draw the selection (i.e. if it is the exported image), we need to rescale all the measurement text at the scale of the image
							!drawSelection, mDisplayHulls ? mPenToDrawHull : null);

			// draw the ruler that we are currently creating if any
			// (if it's the same as the ruler under the mouse
			// that means we are not creating a new one but editing an existing one)
			if ((mCurrentlyEditedRuler != null) && (mCurrentlyEditedRuler != mCurrentRulerUnderMouse))
				mCurrentlyEditedRuler.draw(g, areaInStud, scalePixelPerStud, mTransparency, mImageAttribute, false, mSelectionBrush, false, mDisplayHulls ? mPenToDrawHull : null);

			if (drawSelection && (sCurrentEditTool == EditTool.SELECT))
			{
				// draw the control points of the selected rulers
				if (Properties.Settings.Default.DisplayRulerAttachPoints)
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
            base.draw(g, areaInStud, scalePixelPerStud, drawSelection);
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
						if (mEditAction == EditAction.DUPLICATE_SELECTION ||
							(mEditAction == EditAction.MOVE_SELECTION && !mMouseHasMoved && (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)))
							return MainForm.Instance.RulerDuplicateCursor;
						else if (mEditAction == EditAction.MOVE_CONTROL_POINT)
							return MainForm.Instance.RulerMovePointCursor;
						else if ((mEditAction == EditAction.SCALE_RULER) && (mCurrentlyEditedRuler != null))
							return getScalingCursorFromOrientation(mCurrentlyEditedRuler.getScalingOrientation(mouseCoordInStud));
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
							else if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseZoomPanKey)
							{
								return MainForm.Instance.PanOrZoomViewCursor;
							}
							else
							{
								// we need to check if we will modify one ruler by moving its control point or scalling (no matter if there is a selection)
								EditAction action = EditAction.NONE;
								RulerItem editableRuler = evaluateIfPointIsAboveControlPointOrScaleHandle(mouseCoordInStud, out action);

								// check the resulting action
								mCurrentRulerUnderMouse = editableRuler;
								if (action == EditAction.MOVE_CONTROL_POINT)
									return MainForm.Instance.RulerMovePointCursor;
								else if ((action == EditAction.SCALE_RULER) && (mCurrentRulerUnderMouse != null))
									return getScalingCursorFromOrientation(mCurrentRulerUnderMouse.getScalingOrientation(mouseCoordInStud));

                                // Now if we are inside with the selection, and not above control point, we will move the selection
                                if (isPointInsideSelectionRectangle(mouseCoordInStud))
                                    return MainForm.Instance.RulerMoveCursor;
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
					// early exit, if it's not the left button
					if (e.Button == MouseButtons.Left)
					{
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

						// start by clearing the edition action
						mEditAction = EditAction.NONE;
						preferedCursor = MainForm.Instance.RulerArrowCursor;

						// First priority: check if the user wants to duplicate of the selection
						// Be carreful for a duplication we take only the selected objects, not the ruler
						// under the mouse that may not be selected
						if (duplicationPressed)
						{
							if (isMouseInsideSelectedObjects)
							{
								mEditAction = EditAction.DUPLICATE_SELECTION;
								preferedCursor = MainForm.Instance.RulerDuplicateCursor;
							}
							// else if not inside we keep doing nothing because the duplicate key is pressed
						}
						// now check if we will move a control point or scale handle (which is not the case for a double click)
						else if (e.Clicks == 1)
						{
							// this method will also give the edit action for the editable ruler in out param
							RulerItem editableRuler = evaluateIfPointIsAboveControlPointOrScaleHandle(mouseCoordInStud, out mEditAction);
							// assign the edited ruler if we are editing its point or scale it (after evaluation)
							if (mEditAction != EditAction.NONE)
							{
								mCurrentRulerUnderMouse = editableRuler;
								mCurrentlyEditedRuler = editableRuler;
								// set the cursor
								if (mEditAction == EditAction.MOVE_CONTROL_POINT)
									preferedCursor = MainForm.Instance.RulerMovePointCursor;
								else if (mEditAction == EditAction.SCALE_RULER)
									preferedCursor = getScalingCursorFromOrientation(editableRuler.getScalingOrientation(mouseCoordInStud));
							}
							else
							{
								// cancel the highlighted ruler in that case
								mCurrentRulerWithHighlightedControlPoint = null;
								mEditAction = EditAction.NONE;
							}
						}

						// if still not find an action, continue to search by order of priority
						if (mEditAction == EditAction.NONE)
						{
							// we will add or edit a text if we double click
							if ((e.Clicks == 2) && (mCurrentRulerUnderMouse != null))
							{
								mEditAction = EditAction.CUSTOMIZE_RULER;
								preferedCursor = MainForm.Instance.RulerEditCursor;
							}
							// Now check if the user plan to move the selected items
							// for that of course we must not have a modifier key pressed
							// and none of the selected objects must be attached
							else if (!multipleSelectionPressed && !duplicationPressed &&
								((isMouseInsideSelectedObjects && !areSelectedItemsFullyAttached()) ||
								((mCurrentRulerUnderMouse != null) && (!mCurrentRulerUnderMouse.IsFullyAttached))))
							{
								mEditAction = EditAction.MOVE_SELECTION;
								preferedCursor = MainForm.Instance.RulerMoveCursor;
							}
						}
					}

					// handle the mouse down if we do an edition action with a left or right click (right click for cancel)
					willHandleMouse = (mEditAction != EditAction.NONE) && ((e.Button == MouseButtons.Left) || (e.Button == MouseButtons.Right));
					break;

				case EditTool.LINE:
					// check if we are finishing the edition of the ruler by moving the offset,
					// in that case it is the click to fix the offset
					if ((e.Button == MouseButtons.Left) && (mEditAction != EditAction.SCALE_RULER))
						preferedCursor = MainForm.Instance.RulerAddPoint2Cursor;
					// we handle all the click if it's a left click or if it's right click and we are editing a ruler
					willHandleMouse = ((e.Button == MouseButtons.Left) || ((e.Button == MouseButtons.Right) && (mCurrentlyEditedRuler != null)));
					break;

				case EditTool.CIRCLE:
					// we handle all the click if it's a left click or if it's right click and we are editing a ruler
					willHandleMouse = ((e.Button == MouseButtons.Left) || ((e.Button == MouseButtons.Right) && (mCurrentlyEditedRuler != null)));
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
			if ((mEditAction == EditAction.SCALE_RULER) && (mCurrentlyEditedRuler != null))
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
					if (e.Button == MouseButtons.Left)
					{
						// we select the ruler under the mouse if the selection list is empty
						if ((mCurrentRulerUnderMouse != null) && (mEditAction != EditAction.DUPLICATE_SELECTION))
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
							if (mEditAction == EditAction.MOVE_CONTROL_POINT)
							{
								// if we are moving a control point, use the position of the control point instead of the mouse position
								mMouseDownInitialPosition = mCurrentlyEditedRuler.CurrentControlPoint;
								mMouseDownLastPosition = mMouseDownInitialPosition;
							}
							else if (mEditAction == EditAction.SCALE_RULER)
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
					}
					else if (e.Button == MouseButtons.Right)
					{
						// if it's the right button, cancel the edition
						if (mEditAction == EditAction.MOVE_CONTROL_POINT)
						{
							// put back the control point to the original position and update the rectangle
							mCurrentlyEditedRuler.CurrentControlPoint = mMouseDownInitialPosition;
							this.updateBoundingSelectionRectangle();
						}
						else if (mEditAction == EditAction.SCALE_RULER)
						{
							// rescale at the original position and update the rectangle
							mCurrentlyEditedRuler.scaleToPoint(mMouseDownInitialPosition);
							this.updateBoundingSelectionRectangle();
						}
						else if (mEditAction == EditAction.DUPLICATE_SELECTION)
						{
							// undo the duplicate action and clear it
							if (mLastDuplicateAction != null)
								mLastDuplicateAction.undo();
							mLastDuplicateAction = null;
						}
						else if (mEditAction == EditAction.MOVE_SELECTION)
						{
							// reset the initial position to each ruler
							PointF deltaMove = new PointF(mouseCoordInStudSnapped.X - mMouseDownInitialPosition.X, mouseCoordInStudSnapped.Y - mMouseDownInitialPosition.Y);
							if ((deltaMove.X != 0) || (deltaMove.Y != 0))
							{
								foreach (LayerItem item in mSelectedObjects)
									item.Position = new PointF(item.Position.X - deltaMove.X, item.Position.Y - deltaMove.Y);
								// reset the bounding rectangle
								this.updateBoundingSelectionRectangle();
							}
						}
						mEditAction = EditAction.NONE;
						mCurrentRulerUnderMouse = null;
						mCurrentlyEditedRuler = null;
						mustRefresh = true;
					}
					break;

				case EditTool.LINE:
					if (e.Button == MouseButtons.Left)
					{
						mMouseDownInitialPosition = mouseCoordInStudSnapped;
						mMouseDownLastPosition = mouseCoordInStudSnapped;
						if (mEditAction != EditAction.SCALE_RULER)
							mCurrentlyEditedRuler = new LinearRuler(mouseCoordInStudSnapped, mouseCoordInStudSnapped);
					}
					else if (e.Button == MouseButtons.Right)
					{
						// if it's the right button, cancel the edition
						mCurrentlyEditedRuler = null;
						mEditAction = EditAction.NONE;
						mustRefresh = true;
					}
					else
					{
						mustRefresh = true;
					}
					break;

				case EditTool.CIRCLE:
					if (e.Button == MouseButtons.Left)
					{
						mMouseDownInitialPosition = mouseCoordInStudSnapped;
						mMouseDownLastPosition = mouseCoordInStudSnapped;
						// for the creation of a circle the center start on mouse click and we
						// immediatly go to the scaling of the circle
						mCurrentlyEditedRuler = new CircularRuler(mouseCoordInStudSnapped, 0.0f);
						mEditAction = EditAction.SCALE_RULER;
					}
					else if (e.Button == MouseButtons.Right)
					{
						// if it's the right button, cancel the edition
						mCurrentlyEditedRuler = null;
						mEditAction = EditAction.NONE;
						mustRefresh = true;
					}
					else
					{
						mustRefresh = true;
					}
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
							if (mEditAction == EditAction.MOVE_CONTROL_POINT)
							{
								// check if we can move the control point
                                bool isCurrentControlPointFree = !mCurrentlyEditedRuler.IsCurrentControlPointAttached;
                                if (isCurrentControlPointFree || mCurrentlyEditedRuler.BrickAttachedToCurrentControlPoint.SelectionArea.isPointInside(mouseCoordInStudSnapped))
                                {
                                    // move the control point
                                    mCurrentlyEditedRuler.CurrentControlPoint = mouseCoordInStudSnapped;
                                    // update the bounding rectangle in any case (even when moving a circle, cause the circle can be part of a selection)
                                    this.updateBoundingSelectionRectangle();
                                    mustRefresh = true;
                                }
							}
							else if (mEditAction == EditAction.SCALE_RULER) // or scale it
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
						else if (mEditAction == EditAction.DUPLICATE_SELECTION || mEditAction == EditAction.MOVE_SELECTION)
						{
							bool wereRulersJustDuplicated = false;
							// check if it is a move or a duplicate
							if (mEditAction == EditAction.DUPLICATE_SELECTION)
							{
								// this is a duplicate, if we didn't duplicated yet, this is the moment to copy  and paste the selection
								// and this will change the current selection, that will be move normally after
								if (mLastDuplicateAction == null)
								{
									this.copyCurrentSelectionToClipboard();
									AddActionInHistory addInHistory = AddActionInHistory.DO_NOT_ADD_TO_HISTORY_EXCEPT_IF_POPUP_OCCURED;
									this.pasteClipboardInLayer(AddOffsetAfterPaste.NO, ref addInHistory);
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
					else if (mEditAction == EditAction.MOVE_SELECTION && !mMouseHasMoved)
					{
						// give a second chance to duplicate if the user press the duplicate key
						// after pressing down the mouse key, but not if the user already moved
						if (Control.ModifierKeys == BlueBrick.Properties.Settings.Default.MouseDuplicateSelectionKey)
							mEditAction = EditAction.DUPLICATE_SELECTION;
					}

					break;

				case EditTool.LINE:
					LinearRuler linearRuler = mCurrentlyEditedRuler as LinearRuler;
					if ((linearRuler != null) && mMouseHasMoved)
					{
						// adjust the offset or the second point
						if (mEditAction == EditAction.SCALE_RULER)
							linearRuler.scaleToPoint(mouseCoordInStudSnapped);
						else
							linearRuler.Point2 = mouseCoordInStudSnapped;
						mustRefresh = true;
					}
					break;

				case EditTool.CIRCLE:
					if ((mCurrentlyEditedRuler != null) && mMouseHasMoved)
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
			bool wasARulerItemCreated = false;
			// snap the mouse coord to the grid
			PointF mouseCoordInStudSnapped = getSnapPoint(mouseCoordInStud);
			// compute the delta move of the mouse
			PointF deltaMove = new PointF(mouseCoordInStudSnapped.X - mMouseDownInitialPosition.X, mouseCoordInStudSnapped.Y - mMouseDownInitialPosition.Y);

			switch (sCurrentEditTool)
			{
				case EditTool.SELECT:

					// if it's a double click, we should prompt a box for text editing
					// WARNING: prompt the box in the mouse up event,
					// otherwise, if you do it in the mouse down, the mouse up is not triggered (both under dot net and mono)
					// and this can mess up the click count in mono
					if (mEditAction == EditAction.CUSTOMIZE_RULER)
					{
						editRulerItem(mCurrentRulerUnderMouse);
					}
					else if (mMouseHasMoved && (mSelectedObjects.Count > 0)) // check if we moved the selected bricks
					{
						// reset the flag
						mMouseHasMoved = false;

						// create a new action for this move
						if ((deltaMove.X != 0) || (deltaMove.Y != 0))
						{
							if (mEditAction == EditAction.MOVE_CONTROL_POINT)
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
							else if (mEditAction == EditAction.SCALE_RULER)
							{
								// no need to rescale back the ruler
								// and create an action
								Actions.ActionManager.Instance.doAction(new Actions.Rulers.ScaleRuler(this, mCurrentlyEditedRuler, mMouseDownInitialPosition, mouseCoordInStudSnapped));
							}
							else if (mEditAction == EditAction.DUPLICATE_SELECTION)
							{
								// update the duplicate action
								if (mLastDuplicateAction != null)
									mLastDuplicateAction.updatePositionShift(deltaMove.X, deltaMove.Y);
							}
							else if (mEditAction == EditAction.MOVE_SELECTION)
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

						// after the last move of the duplicate, add the action in the manager
						if ((mEditAction == EditAction.DUPLICATE_SELECTION) && (mLastDuplicateAction != null))
						{
							// undo it and do it in the action manager to add it in the history
							mLastDuplicateAction.undo();
							Actions.ActionManager.Instance.doAction(mLastDuplicateAction);
							// then clear it
							mLastDuplicateAction = null;
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

					mEditAction = EditAction.NONE;
					mCurrentRulerUnderMouse = null;
					mCurrentlyEditedRuler = null;
					break;

				case EditTool.LINE:
					LinearRuler linearRuler = mCurrentlyEditedRuler as LinearRuler;
					if (linearRuler != null)
					{
						if (linearRuler.AllowOffset)
						{
							if (mEditAction == EditAction.SCALE_RULER)
							{
								linearRuler.scaleToPoint(mouseCoordInStudSnapped);
								Actions.ActionManager.Instance.doAction(new Actions.Rulers.AddRuler(this, linearRuler));
								mCurrentlyEditedRuler = null;
								mEditAction = EditAction.NONE;
								wasARulerItemCreated = true;
							}
							else
							{
								linearRuler.Point2 = mouseCoordInStudSnapped;
								mEditAction = EditAction.SCALE_RULER;
							}
						}
						else
						{
							Actions.ActionManager.Instance.doAction(new Actions.Rulers.AddRuler(this, linearRuler));
							mCurrentlyEditedRuler = null;
							wasARulerItemCreated = true;
						}
						mustRefresh = true;
					}
					break;

				case EditTool.CIRCLE:
					if (mCurrentlyEditedRuler != null)
					{
						mCurrentlyEditedRuler.scaleToPoint(mouseCoordInStudSnapped);
						Actions.ActionManager.Instance.doAction(new Actions.Rulers.AddRuler(this, mCurrentlyEditedRuler));
						mCurrentlyEditedRuler = null;
						mEditAction = EditAction.NONE;
						wasARulerItemCreated = true;
						mustRefresh = true;
					}
					break;
			}

			mMouseIsBetweenDownAndUpEvent = false;
			mMouseHasMoved = false;

			// if we have finished to edit the current ruler, change the tool back to edition
			if (wasARulerItemCreated && (Properties.Settings.Default.SwitchToEditionAfterRulerCreation))
				MainForm.Instance.rulerSelectAndEditToolStripMenuItem_Click(null, null);

			return mustRefresh;
		}

		/// <summary>
		/// This method is called when the zoom scale changed
		/// </summary>
		/// <param name="oldScaleInPixelPerStud">The previous scale</param>
		/// <param name="newScaleInPixelPerStud">The new scale</param>
		public override void zoomScaleChangeNotification(double oldScaleInPixelPerStud, double newScaleInPixelPerStud)
		{
			foreach (RulerItem item in mRulers)
				item.zoomScaleChangeNotification(oldScaleInPixelPerStud, newScaleInPixelPerStud);
			// then update the selection rectangle because some ruler has been resized
			updateBoundingSelectionRectangle();
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
