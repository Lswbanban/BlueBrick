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
using BlueBrick.Properties;

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

        // save settings that can be changed with export option
		private bool mOriginalDisplayWatermarkInSetting = Settings.Default.DisplayGeneralInfoWatermark;
		private bool mOriginalDisplayBrickHullInSetting = Settings.Default.DisplayBrickHull;
		private bool mOriginalDisplayElectricCircuitInSetting = Settings.Default.DisplayElectricCircuit;
        private bool mOriginalDisplayConnectionPointInSetting = Settings.Default.DisplayFreeConnexionPoints;

		private const int MAX_IMAGE_SIZE_IN_PIXEL = 4096;
        private const int TOTAL_HEIGHT_OF_FIXED_PANEL = 204;

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
		}

        public void loadUISettingFromDefaultSettings()
        {
            this.exportWatermarkCheckBox.Checked = Settings.Default.UIExportWatermark;
            this.exportHullCheckBox.Checked = Settings.Default.UIExportHulls;
            this.exportElectricCircuitCheckBox.Checked = Settings.Default.UIExportElectricCircuit;
            this.exportConnectionPointCheckBox.Checked = Settings.Default.UIExportConnection;
        }

        public void saveUISettingInDefaultSettings()
        {
            Settings.Default.UIExportWatermark = this.exportWatermarkCheckBox.Checked;
            Settings.Default.UIExportHulls = this.exportHullCheckBox.Checked;
            Settings.Default.UIExportElectricCircuit = this.exportElectricCircuitCheckBox.Checked;
            Settings.Default.UIExportConnection = this.exportConnectionPointCheckBox.Checked;
        }

		public void init()
		{
            // change the setting temporarly for exporting the images
            saveAndChangeDisplaySettings();

            // compute the total area of the map, and the total scale in order to display the full map
			// in the preview window and assign the selected area with the same value
			mTotalAreaInStud = Map.Instance.getTotalAreaInStud(false);

			// select the total area. It depends if we already have exported this file
			if (Map.Instance.ExportArea.Width != 0.0f && Map.Instance.ExportArea.Height != 0.0f)
			{
				// if we already exported this file we take the previous exportation anyway,
				// because the use rmay have tune it the way he wants
				mSelectedAreaInStud = Map.Instance.ExportArea;
				// but we need to check if the total area is smaller than the previous exported area
				// then we should take the bigger size otherwise the selected area cannot fit into
				// the total area.
				if (mSelectedAreaInStud.Left < mTotalAreaInStud.Left)
				{
					// increase the width before moving the left because we only want to move the left and not the right
					mTotalAreaInStud.Width = mTotalAreaInStud.Right - mSelectedAreaInStud.Left;
					mTotalAreaInStud.X = mSelectedAreaInStud.Left;
				}
				if (mSelectedAreaInStud.Right > mTotalAreaInStud.Right)
					mTotalAreaInStud.Width = mSelectedAreaInStud.Right - mTotalAreaInStud.Left;
				// vertical
				if (mSelectedAreaInStud.Top < mTotalAreaInStud.Top)
				{
					// increase the height before moving the top because we only want to move the top and not the bottom
					mTotalAreaInStud.Height = mTotalAreaInStud.Bottom - mSelectedAreaInStud.Top;
					mTotalAreaInStud.Y = mSelectedAreaInStud.Top;
				}
				if (mSelectedAreaInStud.Bottom > mTotalAreaInStud.Bottom)
					mTotalAreaInStud.Height = mSelectedAreaInStud.Bottom - mTotalAreaInStud.Top;
			}
			else
			{
				mSelectedAreaInStud = mTotalAreaInStud;
			}

			// save the selected area in a local variable to set the correct values of the numeric up down
			// because, setting the min and max for the numeric up down can trigger the value changed event
			// (the min/max value can clamp the current value not set yet), and in these value changed event
			// we change the mSelectedAreaInStud. So after setting the min/max, we set the correct values
			// with the local saved area
			RectangleF initialSelectedAreaInStud = mSelectedAreaInStud;

			// add a margin to the total area
			mTotalAreaInStud.X -= 32.0f;
			mTotalAreaInStud.Y -= 32.0f;
			mTotalAreaInStud.Width += 64.0f;
			mTotalAreaInStud.Height += 64.0f;

			// draw the map and preview images
			computePreviewPictureSizeAndPos();
            drawAll();

			//init the numericupdown controls for image size
			this.imageHeightNumericUpDown.Minimum = (Decimal)1;
			this.imageHeightNumericUpDown.Maximum = (Decimal)MAX_IMAGE_SIZE_IN_PIXEL;
			this.imageWidthNumericUpDown.Minimum = (Decimal)1;
			this.imageWidthNumericUpDown.Maximum = (Decimal)MAX_IMAGE_SIZE_IN_PIXEL;

			// set the min, max, inc and value for the scale
			updateMinAndMaxScaleAccordingToSelectedArea();
			// check if we need to use the previous scaled saved in the map
			double scaleValue = mTotalScalePixelPerStud;
			if (Map.Instance.ExportScale != 0.0)
				scaleValue = Map.Instance.ExportScale;
			// ensure that the scale value is inside the min and max and set it the numeric updown control
			scaleValue = Math.Max(Math.Min(scaleValue, (double)this.scaleNumericUpDown.Maximum),
									(double)this.scaleNumericUpDown.Minimum);
			this.scaleNumericUpDown.Value = (Decimal)scaleValue;

			//init the numericupdown controls for area
			// start by setting the min/max to avoid an out of bound exception when setting the value
			this.areaLeftNumericUpDown.Minimum = (Decimal)(mTotalAreaInStud.Left);
			this.areaRightNumericUpDown.Maximum = (Decimal)(mTotalAreaInStud.Right);
			this.areaTopNumericUpDown.Minimum = (Decimal)(mTotalAreaInStud.Top);
			this.areaBottomNumericUpDown.Maximum = (Decimal)(mTotalAreaInStud.Bottom);

			this.areaLeftNumericUpDown.Maximum = this.areaRightNumericUpDown.Maximum;
			this.areaRightNumericUpDown.Minimum = this.areaLeftNumericUpDown.Minimum;
			this.areaTopNumericUpDown.Maximum = this.areaBottomNumericUpDown.Maximum;
			this.areaBottomNumericUpDown.Minimum = this.areaTopNumericUpDown.Minimum;

			// set the value after setting the minimum and maximum otherwise we can raise an exeption
			// and used the local saved variable because the min/max setting may have already call
			// the value changed event to clamp the value, and in the event the mSelectedAreaInStud is changed
			this.areaLeftNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Left);
			this.areaRightNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Right);
			this.areaTopNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Top);
			this.areaBottomNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Bottom);
		}

		private void updateMinAndMaxScaleAccordingToSelectedArea()
		{
			// get the biggest dimension among width and height of the selected area, and ensure it is not null
			double bigestDimensionOfSelectedArea = Math.Max(1.0, Math.Max(mSelectedAreaInStud.Width, mSelectedAreaInStud.Height));
			// compute the max scale according to the max size of the export image,
			// and the size of the total area
			double maxScale = MAX_IMAGE_SIZE_IN_PIXEL / bigestDimensionOfSelectedArea;
			double minScale = Math.Min(0.01, maxScale / 2); // 0.01 by default, the second value is to handle extrem case where the max is under 0.01
			double incScale = Math.Min(0.01, maxScale / 4); // 0.01 by default, the second value is to handle extrem case where the distance between min and max is less than 0.01
			this.scaleNumericUpDown.Maximum = (Decimal)maxScale;
			this.scaleNumericUpDown.Minimum = (Decimal)minScale;
			this.scaleNumericUpDown.Increment = (Decimal)incScale;
		}

        private void saveAndChangeDisplaySettings()
        {
            // save the settings
            mOriginalDisplayWatermarkInSetting = Settings.Default.DisplayGeneralInfoWatermark;
            mOriginalDisplayBrickHullInSetting = Settings.Default.DisplayBrickHull;
            mOriginalDisplayElectricCircuitInSetting = Settings.Default.DisplayElectricCircuit;
            mOriginalDisplayConnectionPointInSetting = Settings.Default.DisplayFreeConnexionPoints;
            // now change them: for each we change the check state and simulate a click
            this.exportWatermarkCheckBox.Checked = Map.Instance.ExportWatermark;
            this.exportWatermarkCheckBox_Click(null, null);

            this.exportHullCheckBox.Checked = Map.Instance.ExportBrickHull;
            this.exportHullCheckBox_Click(null, null);

            this.exportElectricCircuitCheckBox.Checked = Map.Instance.ExportElectricCircuit;
            this.exportElectricCircuitCheckBox_Click(null, null);

            this.exportConnectionPointCheckBox.Checked = Map.Instance.ExportConnectionPoints;
            this.exportConnectionPointCheckBox_Click(null, null);
        }

        /// <summary>
        /// This method is called from main form.
        /// </summary>
        public void restoreDisplaySettings()
        {
            Settings.Default.DisplayGeneralInfoWatermark = mOriginalDisplayWatermarkInSetting;
            Settings.Default.DisplayBrickHull = mOriginalDisplayBrickHullInSetting;
            Settings.Default.DisplayElectricCircuit = mOriginalDisplayElectricCircuitInSetting;
            Settings.Default.DisplayFreeConnexionPoints = mOriginalDisplayConnectionPointInSetting;
        }
		#region update of the preview image
		private void computePreviewPictureSizeAndPos()
		{
			// get the new available width and height
			int parentWidth = this.topTableLayoutPanel.ClientSize.Width - 4;
			int parentHeight = this.topTableLayoutPanel.ClientSize.Height - TOTAL_HEIGHT_OF_FIXED_PANEL;
			// check that the width and height are not negative, this can happen
			// when the sizeChanged event is called before the tableLayout has its correct size
			if (parentWidth < 4)
				parentWidth = 4;
			if (parentHeight < 4)
				parentHeight = 4;
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
			// check that the size is not null, which can happen if the window is minimized
			if ((this.previewPictureBox.Width > 0) && (this.previewPictureBox.Height > 0))
			{
				// the constructor of the bitmap throw an exception if the size of the image is negative or null
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
			else
			{
				// otherwise delete the map image to avoid setting the picture box image
				if (mMapImage != null)
					mMapImage.Dispose();
				mMapImage = null;
			}
		}

		private void drawPreviewImage()
		{
			// the preview image is created from the map image so check that the map image is not null
			if (mMapImage != null)
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
                // draw the watermark
                RectangleF watermarkRectangleInStud = new RectangleF(mTotalAreaInStud.X, mTotalAreaInStud.Y,
                                                                    mSelectedAreaInStud.X - mTotalAreaInStud.X + mSelectedAreaInStud.Width,
                                                                    mSelectedAreaInStud.Y - mTotalAreaInStud.Y + mSelectedAreaInStud.Height);
                Map.Instance.drawWatermark(graphics, watermarkRectangleInStud, mTotalScalePixelPerStud);
            }
		}

        private void drawAll()
        {
            drawMapImage();
            drawPreviewImage();
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
			updateMinAndMaxScaleAccordingToSelectedArea();
			updateImage(this.areaLeftNumericUpDown);
		}

		private void areaTopNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaTopNumericUpDown.Value);
			mSelectedAreaInStud.Height += mSelectedAreaInStud.Y - newValue;
			mSelectedAreaInStud.Y = newValue;
			updateMinAndMaxScaleAccordingToSelectedArea();
			updateImage(this.areaTopNumericUpDown);
		}

		private void areaRightNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaRightNumericUpDown.Value);
			mSelectedAreaInStud.Width = newValue - mSelectedAreaInStud.X;
			updateMinAndMaxScaleAccordingToSelectedArea();
			updateImage(this.areaRightNumericUpDown);
		}

		private void areaBottomNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaBottomNumericUpDown.Value);
			mSelectedAreaInStud.Height = newValue - mSelectedAreaInStud.Y;
			updateMinAndMaxScaleAccordingToSelectedArea();
			updateImage(this.areaBottomNumericUpDown);
		}

		private void imageWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (mFirstSender == null)
			{
				mFirstSender = this.imageWidthNumericUpDown;
				double newScaleValue = (double)(this.imageWidthNumericUpDown.Value) / mSelectedAreaInStud.Width;
				if (newScaleValue < (double)(this.scaleNumericUpDown.Minimum))
					newScaleValue = (double)(this.scaleNumericUpDown.Minimum);
				if (newScaleValue > (double)(this.scaleNumericUpDown.Maximum))
					newScaleValue = (double)(this.scaleNumericUpDown.Maximum);
				this.scaleNumericUpDown.Value = (Decimal)newScaleValue;
				mFirstSender = null;
			}
		}

		private void imageHeightNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (mFirstSender == null)
			{
				mFirstSender = this.imageHeightNumericUpDown;
				double newScaleValue = (double)(this.imageHeightNumericUpDown.Value) / mSelectedAreaInStud.Height;
				if (newScaleValue < (double)(this.scaleNumericUpDown.Minimum))
					newScaleValue = (double)(this.scaleNumericUpDown.Minimum);
				if (newScaleValue > (double)(this.scaleNumericUpDown.Maximum))
					newScaleValue = (double)(this.scaleNumericUpDown.Maximum);
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
			drawAll();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			// save the setting chosen in the map
			Map.Instance.saveExportAreaAndDisplaySettings(mSelectedAreaInStud, (double)(this.scaleNumericUpDown.Value),
                this.exportWatermarkCheckBox.Checked, this.exportHullCheckBox.Checked,
                this.exportElectricCircuitCheckBox.Checked, this.exportConnectionPointCheckBox.Checked);

            // save also the UI settings: if the user has exported, that means he like the option like that, and if he make
            // a new map and export, he wants to find back his last settings
            saveUISettingInDefaultSettings();
		}

        private void exportWatermarkCheckBox_Click(object sender, EventArgs e)
        {
            // we use the click event and not the CheckedChanged, because we don't want this to be called at startup, when the form is initialized
            Settings.Default.DisplayGeneralInfoWatermark = this.exportWatermarkCheckBox.Checked;
			// if the sender is null, this method is just called from the init function, so no need to draw several times
			if (sender != null)
				drawAll();
        }

        private void exportHullCheckBox_Click(object sender, EventArgs e)
        {
            // we use the click event and not the CheckedChanged, because we don't want this to be called at startup, when the form is initialized
            Settings.Default.DisplayBrickHull = this.exportHullCheckBox.Checked;
			// if the sender is null, this method is just called from the init function, so no need to draw several times
			if (sender != null)
				drawAll();
		}

        private void exportElectricCircuitCheckBox_Click(object sender, EventArgs e)
        {
            // we use the click event and not the CheckedChanged, because we don't want this to be called at startup, when the form is initialized
            Settings.Default.DisplayElectricCircuit = this.exportElectricCircuitCheckBox.Checked;
			// if the sender is null, this method is just called from the init function, so no need to draw several times
			if (sender != null)
				drawAll();
		}

        private void exportConnectionPointCheckBox_Click(object sender, EventArgs e)
        {
            // we use the click event and not the CheckedChanged, because we don't want this to be called at startup, when the form is initialized
            Settings.Default.DisplayFreeConnexionPoints = this.exportConnectionPointCheckBox.Checked;
			// if the sender is null, this method is just called from the init function, so no need to draw several times
			if (sender != null)
				drawAll();
		}
        #endregion

		private void ExportImageForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// set the visible false for catching this strange case:
			// Minimize the window, then right click on the task bar and choose close
			// in such case, next time you try do do a Show Dialog, and exception is raised
			this.Visible = false;
		}
	}
}