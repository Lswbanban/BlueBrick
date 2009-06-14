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
using BlueBrick.MapData;

namespace BlueBrick.Actions.Texts
{
	class RotateText : Action
	{
		// the static center is used to handle multiple following rotation (so we keep the first computed center in that case)
		static private PointF sLastCenter = new PointF(0, 0);	// in Stud coord
		static public bool sLastCenterIsValid = false;

		// data for the action
		private LayerText mTextLayer = null;
		private List<Layer.LayerItem> mTexts = null;
		private bool mRotateCW;
		private float mRotationStep = 0.0f; // in degree, we need to save it because the current rotation step may change between the do and undo
		private PointF mCenter = new PointF(0, 0);	// in Stud coord

		public RotateText(LayerText layer, List<Layer.LayerItem> texts, bool rotateCW)
		{
			mTextLayer = layer;
			mRotateCW = rotateCW;
			mRotationStep = MapData.Layer.CurrentRotationStep;
			// we must invalidate the last center is the last action in the undo stack is not a rotation
			if (!ActionManager.Instance.getUndoableActionType().IsInstanceOfType(this))
				sLastCenterIsValid = false;
			// fill the text list with the one provided and set the center of rotation for this action
			if (texts.Count > 0)
			{
				// copy the list, because the pointer may change (specially if it is the selection)
				// also compute the center of all the texts
				PointF minCenter = new PointF(texts[0].mDisplayArea.Left, texts[0].mDisplayArea.Top);
				PointF maxCenter = new PointF(texts[0].mDisplayArea.Right, texts[0].mDisplayArea.Bottom);
				mTexts = new List<Layer.LayerItem>(texts.Count);
				foreach (Layer.LayerItem obj in texts)
				{
					mTexts.Add(obj);
					//compute the new center if the static one is not valid
					if (!sLastCenterIsValid)
					{
						if (obj.mDisplayArea.Left < minCenter.X)
							minCenter.X = obj.mDisplayArea.Left;
						if (obj.mDisplayArea.Top < minCenter.Y)
							minCenter.Y = obj.mDisplayArea.Top;
						if (obj.mDisplayArea.Right > maxCenter.X)
							maxCenter.X = obj.mDisplayArea.Right;
						if (obj.mDisplayArea.Bottom > maxCenter.Y)
							maxCenter.Y = obj.mDisplayArea.Bottom;
					}
				}
				// set the center for this rotation action (keep the previous one or compute a new one
				if (sLastCenterIsValid)
				{
					mCenter = sLastCenter;
				}
				else
				{
					// recompute a new center
					mCenter.X = (maxCenter.X + minCenter.X) / 2;
					mCenter.Y = (maxCenter.Y + minCenter.Y) / 2;
					// and assign it to the static one
					sLastCenter = mCenter;
					sLastCenterIsValid = true;
				}
			}
		}

		public override string getName()
		{
			if (mTexts.Count == 1)
			{
				string actionName = BlueBrick.Properties.Resources.ActionRotateText;
				string text = (mTexts[0] as LayerText.TextCell).Text.Replace("\r\n", " ");
				if (text.Length > 10)
					text = text.Substring(0, 10) + "...";
				actionName = actionName.Replace("&", text);
				return actionName;
			}
			else
			{
				return BlueBrick.Properties.Resources.ActionRotateSeveralTexts;
			}
		}

		public override void redo()
		{
			if (mRotateCW)
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(-mRotationStep);
				foreach (Layer.LayerItem obj in mTexts)
					rotateCW(obj as LayerText.TextCell, rotation);
			}
			else
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(mRotationStep);
				foreach (Layer.LayerItem obj in mTexts)
					rotateCCW(obj as LayerText.TextCell, rotation);
			}
			// update the bounding rectangle (because the text is not square)
			mTextLayer.updateBoundingSelectionRectangle();
		}

		public override void undo()
		{
			if (mRotateCW)
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(mRotationStep);
				foreach (Layer.LayerItem obj in mTexts)
					rotateCCW(obj as LayerText.TextCell, rotation);
			}
			else
			{
				Matrix rotation = new Matrix();
				rotation.Rotate(-mRotationStep);
				foreach (Layer.LayerItem obj in mTexts)
					rotateCW(obj as LayerText.TextCell, rotation);
			}
			// update the bounding rectangle (because the text is not square)
			mTextLayer.updateBoundingSelectionRectangle();
		}

		private void rotateCCW(LayerText.TextCell text, Matrix rotation)
		{
			PointF textCenterBeforeRotation = new PointF(text.Center.X, text.Center.Y);

			// change the orientation of the picture
			text.Orientation = (text.Orientation + mRotationStep);

			// change the position for a group of parts
			if (mTexts.Count > 1)
			{
				PointF[] points = { new PointF(textCenterBeforeRotation.X - mCenter.X, textCenterBeforeRotation.Y - mCenter.Y) };
				rotation.TransformVectors(points);
				// assign the new position
				PointF newPosition = new PointF();
				newPosition.X = mCenter.X + points[0].X;
				newPosition.Y = mCenter.Y + points[0].Y;
				// assign the new position
				text.Center = newPosition;
			}
			else
			{
				//if only one part, reset the center, to rotate on the center
				text.Center = textCenterBeforeRotation;
			}
		}

		private void rotateCW(LayerText.TextCell text, Matrix rotation)
		{
			PointF textCenterBeforeRotation = new PointF(text.Center.X, text.Center.Y);

			// change the orientation of the picture
			text.Orientation = (text.Orientation - mRotationStep);

			// change the position for a group of parts
			if (mTexts.Count > 1)
			{
				PointF[] points = { new PointF(textCenterBeforeRotation.X - mCenter.X, textCenterBeforeRotation.Y - mCenter.Y) };
				rotation.TransformVectors(points);
				// assign the new position
				PointF newPosition = new PointF();
				newPosition.X = mCenter.X + points[0].X;
				newPosition.Y = mCenter.Y + points[0].Y;
				// assign the new position
				text.Center = newPosition;
			}
			else
			{
				//if only one part, reset the center, to rotate on the center
				text.Center = textCenterBeforeRotation;
			}
		}
	}
}
