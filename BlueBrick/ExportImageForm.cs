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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using BlueBrick.MapData;

namespace BlueBrick
{
	public partial class ExportImageForm : Form
	{
		private RectangleF mTotalAreaInStud;
		private RectangleF mSelectedAreaInStud;
		private double mTotalScalePixelPerStud = 0.5;
		private Bitmap mMapImage = null;
		private Pen mSelectionPen = new Pen(Color.Red, 2);
		private NumericUpDown mFirstSender = null;
		private bool mIsDragingSelectionRectangle = false;
		private PointF mStartDragPoint = new PointF();
		private bool mUpdateImage = true;

		#region get/set
		public RectangleF AreaInStud
		{
			get { return mSelectedAreaInStud; }
		}

		public int ImageWidth
		{
			get { return (int)(this.imageWidthNumericUpDown.Value); }
		}

		public int ImageHeight
		{
			get { return (int)(this.imageHeightNumericUpDown.Value); }
		}

		public double ScalePixelPerStud
		{
			get { return (double)(this.scaleNumericUpDown.Value); }
		}
		#endregion

		public ExportImageForm()
		{
			InitializeComponent();

			// compute the total area of the map, and the total scale in order to display the full map
			// in the preview window and assign the selected area with the same value
			mTotalAreaInStud = Map.Instance.getTotalAreaInStud(false);
			mSelectedAreaInStud = mTotalAreaInStud;

			// draw the map and preview images
			computePreviewPictureSizeAndPos();
			drawMapImage();
			drawPreviewImage();

			//init the numericupdown controls for image size
			if (mTotalScalePixelPerStud > (float)(this.scaleNumericUpDown.Maximum))
				this.scaleNumericUpDown.Value = this.scaleNumericUpDown.Maximum;
			else
				this.scaleNumericUpDown.Value = (Decimal)mTotalScalePixelPerStud;
			this.scaleNumericUpDown.Minimum = (Decimal)0.01;
			this.scaleNumericUpDown.Increment = (Decimal)0.01;

			//init the numericupdown controls for area
			this.areaLeftNumericUpDown.Value = (Decimal)(mSelectedAreaInStud.Left);
			this.areaRightNumericUpDown.Value = (Decimal)(mSelectedAreaInStud.Right);
			this.areaTopNumericUpDown.Value = (Decimal)(mSelectedAreaInStud.Top);
			this.areaBottomNumericUpDown.Value = (Decimal)(mSelectedAreaInStud.Bottom);

			this.areaLeftNumericUpDown.Minimum = this.areaLeftNumericUpDown.Value;
			this.areaRightNumericUpDown.Maximum = this.areaRightNumericUpDown.Value;
			this.areaTopNumericUpDown.Minimum = this.areaTopNumericUpDown.Value;
			this.areaBottomNumericUpDown.Maximum = this.areaBottomNumericUpDown.Value;

			this.areaLeftNumericUpDown.Maximum = this.areaRightNumericUpDown.Maximum;
			this.areaRightNumericUpDown.Minimum = this.areaLeftNumericUpDown.Minimum;
			this.areaTopNumericUpDown.Maximum = this.areaBottomNumericUpDown.Maximum;
			this.areaBottomNumericUpDown.Minimum = this.areaTopNumericUpDown.Minimum;
		}

		#region update of the preview image
		private void computePreviewPictureSizeAndPos()
		{
			// get the new available width and height
			int parentWidth = this.tableLayoutPanel.ClientSize.Width - 4;
			int parentHeight = this.tableLayoutPanel.ClientSize.Height - 154;
			// compute the scale of the preview and rescale and center the preview image
			double xScale = (double)(parentWidth - 2) / mTotalAreaInStud.Width;
			double yScale = (double)(parentHeight - 2) / mTotalAreaInStud.Height;
			// take the min scale and resize the picture box
			if (xScale < yScale)
			{
				mTotalScalePixelPerStud = xScale;
				int newHeight = (int)(xScale * mTotalAreaInStud.Height) + 2;
				this.previewPictureBox.Top += (parentHeight - newHeight) / 2;
				this.previewPictureBox.Height = newHeight - 1;
				this.previewPictureBox.Left = 0;
				this.previewPictureBox.Width = parentWidth;
			}
			else
			{
				mTotalScalePixelPerStud = yScale;
				int newWidth = (int)(yScale * mTotalAreaInStud.Width) + 2;
				this.previewPictureBox.Left += (parentWidth - newWidth) / 2;
				this.previewPictureBox.Width = newWidth - 1;
				this.previewPictureBox.Top = 0;
				this.previewPictureBox.Height = parentHeight;
			}
		}

		private void drawMapImage()
		{
			// clear all the selection before the export
			Map.Instance.clearAllSelection();
			// create the map image with the correct size
			mMapImage = new Bitmap(this.previewPictureBox.Width, this.previewPictureBox.Height);
			// get the graphic context from the preview picture box
			Graphics graphics = Graphics.FromImage(mMapImage);
			graphics.Clear(Map.Instance.BackgroundColor);
			graphics.CompositingMode = CompositingMode.SourceOver;
			graphics.SmoothingMode = SmoothingMode.None;
			graphics.CompositingQuality = CompositingQuality.HighSpeed;
			graphics.InterpolationMode = InterpolationMode.Low;
			// draw the bitmap
			Map.Instance.draw(graphics, mTotalAreaInStud, mTotalScalePixelPerStud);
		}

		private void drawPreviewImage()
		{
			// recreate the preview image
			this.previewPictureBox.Image = new Bitmap(mMapImage);
			// get the graphic context from the preview picture box
			Graphics graphics = Graphics.FromImage(this.previewPictureBox.Image);
			graphics.CompositingMode = CompositingMode.SourceOver;
			graphics.SmoothingMode = SmoothingMode.None;
			graphics.CompositingQuality = CompositingQuality.HighSpeed;
			graphics.InterpolationMode = InterpolationMode.Low;
			// and draw the selected area
			graphics.DrawRectangle(mSelectionPen,
				(float)((mSelectedAreaInStud.X - mTotalAreaInStud.X) * mTotalScalePixelPerStud),
				(float)((mSelectedAreaInStud.Y - mTotalAreaInStud.Y) * mTotalScalePixelPerStud),
				(float)(mSelectedAreaInStud.Width * mTotalScalePixelPerStud),
				(float)(mSelectedAreaInStud.Height * mTotalScalePixelPerStud));
		}

		private void updateImage(NumericUpDown firstSender)
		{
			if (mUpdateImage)
			{
				drawPreviewImage();
				mFirstSender = firstSender;
				computeImageSizeFromAreaAndScale();
				mFirstSender = null;
			}
		}

		private void suspendUpdateImage()
		{
			mUpdateImage = false;
		}

		private void resumeUpdateImage()
		{
			mUpdateImage = true;
			updateImage(this.areaLeftNumericUpDown);
		}
		#endregion

		private void computeImageSizeFromAreaAndScale()
		{
			float newScaleValue = (float)(this.scaleNumericUpDown.Value);
			if (mFirstSender != this.imageWidthNumericUpDown)
			{
				int newValue = (int)(mSelectedAreaInStud.Width * newScaleValue);
				if (newValue < (int)(this.imageWidthNumericUpDown.Minimum))
					newValue = (int)(this.imageWidthNumericUpDown.Minimum);
				if (newValue > (int)(this.imageWidthNumericUpDown.Maximum))
					newValue = (int)(this.imageWidthNumericUpDown.Maximum);
				this.imageWidthNumericUpDown.Value = (Decimal)newValue;
			}
			if (mFirstSender != this.imageHeightNumericUpDown)
			{
				int newValue = (int)(mSelectedAreaInStud.Height * newScaleValue);
				if (newValue < (int)(this.imageHeightNumericUpDown.Minimum))
					newValue = (int)(this.imageHeightNumericUpDown.Minimum);
				if (newValue > (int)(this.imageHeightNumericUpDown.Maximum))
					newValue = (int)(this.imageHeightNumericUpDown.Maximum);
				this.imageHeightNumericUpDown.Value = (Decimal)newValue;
			}
		}

		private PointF getAreaPointFromMousePoint(Point mouseCoord)
		{
			PointF result = new PointF();
			// compute x
			result.X = (float)mouseCoord.X / (float)mTotalScalePixelPerStud;
			result.X += mTotalAreaInStud.X;
			if (result.X < (float)(this.areaLeftNumericUpDown.Minimum))
				result.X = (float)(this.areaLeftNumericUpDown.Minimum);
			if (result.X > (float)(this.areaLeftNumericUpDown.Maximum))
				result.X = (float)(this.areaLeftNumericUpDown.Maximum);
			// compute y
			result.Y = (float)mouseCoord.Y / (float)mTotalScalePixelPerStud;
			result.Y += mTotalAreaInStud.Y;
			if (result.Y < (float)(this.areaTopNumericUpDown.Minimum))
				result.Y = (float)(this.areaTopNumericUpDown.Minimum);
			if (result.Y > (float)(this.areaTopNumericUpDown.Maximum))
				result.Y = (float)(this.areaTopNumericUpDown.Maximum);
			// return the result
			return result;
		}

		#region event handler

		private void areaLeftNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaLeftNumericUpDown.Value);
			mSelectedAreaInStud.Width += mSelectedAreaInStud.X - newValue;
			mSelectedAreaInStud.X = newValue;
			updateImage(this.areaLeftNumericUpDown);
		}

		private void areaTopNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaTopNumericUpDown.Value);
			mSelectedAreaInStud.Height += mSelectedAreaInStud.Y - newValue;
			mSelectedAreaInStud.Y = newValue;
			updateImage(this.areaTopNumericUpDown);
		}

		private void areaRightNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaRightNumericUpDown.Value);
			mSelectedAreaInStud.Width = newValue - mSelectedAreaInStud.X;
			updateImage(this.areaRightNumericUpDown);
		}

		private void areaBottomNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaBottomNumericUpDown.Value);
			mSelectedAreaInStud.Height = newValue - mSelectedAreaInStud.Y;
			updateImage(this.areaBottomNumericUpDown);
		}

		private void imageWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (mFirstSender == null)
			{
				mFirstSender = this.imageWidthNumericUpDown;
				float newScaleValue = (float)(this.imageWidthNumericUpDown.Value) / mSelectedAreaInStud.Width;
				if (newScaleValue < (int)(this.scaleNumericUpDown.Minimum))
					newScaleValue = (int)(this.scaleNumericUpDown.Minimum);
				if (newScaleValue > (int)(this.scaleNumericUpDown.Maximum))
					newScaleValue = (int)(this.scaleNumericUpDown.Maximum);
				this.scaleNumericUpDown.Value = (Decimal)newScaleValue;
				mFirstSender = null;
			}
		}

		private void imageHeightNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (mFirstSender == null)
			{
				mFirstSender = this.imageHeightNumericUpDown;
				float newScaleValue = (float)(this.imageHeightNumericUpDown.Value) / mSelectedAreaInStud.Height;
				if (newScaleValue < (int)(this.scaleNumericUpDown.Minimum))
					newScaleValue = (int)(this.scaleNumericUpDown.Minimum);
				if (newScaleValue > (int)(this.scaleNumericUpDown.Maximum))
					newScaleValue = (int)(this.scaleNumericUpDown.Maximum);
				this.scaleNumericUpDown.Value = (Decimal)newScaleValue;
				mFirstSender = null;
			}
		}

		private void scaleNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (mFirstSender == null)
			{
				mFirstSender = this.scaleNumericUpDown;
				computeImageSizeFromAreaAndScale();
				mFirstSender = null;
			}
			else
			{
				computeImageSizeFromAreaAndScale();
			}
		}

		private void previewPictureBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Clicks == 1)
			{
				//suspend the update of image
				suspendUpdateImage();
				// get the mouse coord in area coord
				mStartDragPoint = getAreaPointFromMousePoint(e.Location);
				// assign the numeric up down
				areaLeftNumericUpDown.Value = (Decimal)mStartDragPoint.X;
				areaRightNumericUpDown.Value = areaLeftNumericUpDown.Value;
				areaTopNumericUpDown.Value = (Decimal)mStartDragPoint.Y;
				areaBottomNumericUpDown.Value = areaTopNumericUpDown.Value;
				//set the flag of the drag
				mIsDragingSelectionRectangle = true;
				//resume the update of image
				resumeUpdateImage();
			}
		}

		private void previewPictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (mIsDragingSelectionRectangle)
			{
				//suspend the update of image
				suspendUpdateImage();
				// get the mouse coord in area coord
				PointF areaPoint = getAreaPointFromMousePoint(e.Location);
				// assign the numeric up down
				if (areaPoint.X <= mStartDragPoint.X)
				{
					areaLeftNumericUpDown.Value = (Decimal)areaPoint.X;
					areaRightNumericUpDown.Value = (Decimal)mStartDragPoint.X;
				}
				else
				{
					areaLeftNumericUpDown.Value = (Decimal)mStartDragPoint.X;
					areaRightNumericUpDown.Value = (Decimal)areaPoint.X;
				}
				if (areaPoint.Y <= mStartDragPoint.Y)
				{
					areaTopNumericUpDown.Value = (Decimal)areaPoint.Y;
					areaBottomNumericUpDown.Value = (Decimal)mStartDragPoint.Y;
				}
				else
				{
					areaTopNumericUpDown.Value = (Decimal)mStartDragPoint.Y;
					areaBottomNumericUpDown.Value = (Decimal)areaPoint.Y;
				}
				//resume the update of image
				resumeUpdateImage();
			}
		}

		private void previewPictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (mIsDragingSelectionRectangle)
			{
				// get the mouse coord in area coord
				PointF areaPoint = getAreaPointFromMousePoint(e.Location);
				// update the rectangle, if the width and height are not null
				if ((areaPoint.X != mStartDragPoint.X) && (areaPoint.Y != mStartDragPoint.Y))
				{
					// call the same update as move
					previewPictureBox_MouseMove(sender, e);
				}
				else
				{
					// a simple click do the same thing as a double click
					previewPictureBox_DoubleClick(sender, e);
				}
				//reset the flag of the drag
				mIsDragingSelectionRectangle = false;
			}
		}

		private void previewPictureBox_DoubleClick(object sender, EventArgs e)
		{
			//suspend the update of image
			suspendUpdateImage();
			// reset the selection to the total area
			areaLeftNumericUpDown.Value = (Decimal)(mTotalAreaInStud.Left);
			areaRightNumericUpDown.Value = (Decimal)(mTotalAreaInStud.Right);
			areaTopNumericUpDown.Value = (Decimal)(mTotalAreaInStud.Top);
			areaBottomNumericUpDown.Value = (Decimal)(mTotalAreaInStud.Bottom);
			//resume the update of image
			resumeUpdateImage();
		}

		private void ExportImageForm_SizeChanged(object sender, EventArgs e)
		{
			computePreviewPictureSizeAndPos();
			drawMapImage();
			drawPreviewImage();
		}

		#endregion
	}
}