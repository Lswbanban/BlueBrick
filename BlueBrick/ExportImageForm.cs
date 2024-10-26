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
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using BlueBrick.MapData;
using BlueBrick.Properties;
using System.IO;
using System.Drawing.Imaging;

namespace BlueBrick
{
	public partial class ExportImageForm : Form
	{
		private RectangleF mTotalAreaInStud;
		private RectangleF mSelectedAreaInStud;
		private double mTotalScalePixelPerStud = 0.5;
		private Bitmap mMapImage = null;
		private Pen mSelectionPen = new Pen(Color.Red, 2);
		SolidBrush mMarginBlackOverlayBrush = new SolidBrush(Color.FromArgb(127, Color.Black));
		private NumericUpDown mFirstSender = null;
		private bool mIsDragingSelectionRectangle = false;
		private PointF mStartDragPoint = new PointF();
		private bool mCanUpdateImage = true;

        // save settings that can be changed with export option
		private bool mOriginalDisplayWatermarkInSetting = Settings.Default.DisplayGeneralInfoWatermark;
		private bool mOriginalDisplayElectricCircuitInSetting = Settings.Default.DisplayElectricCircuit;
        private bool mOriginalDisplayConnectionPointInSetting = Settings.Default.DisplayFreeConnexionPoints;

		private const int MAX_IMAGE_SIZE_IN_PIXEL = 4096;
        private const int TOTAL_HEIGHT_OF_FIXED_PANEL = 230;

		#region init
		public ExportImageForm()
		{
			InitializeComponent();
		}

        public void loadUISettingFromDefaultSettings()
        {
            this.exportWatermarkCheckBox.Checked = Settings.Default.UIExportWatermark;
            this.exportElectricCircuitCheckBox.Checked = Settings.Default.UIExportElectricCircuit;
            this.exportConnectionPointCheckBox.Checked = Settings.Default.UIExportConnection;
        }

        public void saveUISettingInDefaultSettings()
        {
            Settings.Default.UIExportWatermark = this.exportWatermarkCheckBox.Checked;
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

			// init the numericupdown controls for area
			// start by setting the min/max to avoid an out of bound exception when setting the value
            // Use a margin of 1, because if you set the area by typing the values in the text box and
            // setting a right value smaller than a left value, then the left value is set one pixel aside the right value
			this.areaLeftNumericUpDown.Minimum = (Decimal)(mTotalAreaInStud.Left - 1.0f);
            this.areaLeftNumericUpDown.Maximum = (Decimal)(mTotalAreaInStud.Right);

            this.areaRightNumericUpDown.Minimum = (Decimal)(mTotalAreaInStud.Left);
            this.areaRightNumericUpDown.Maximum = (Decimal)(mTotalAreaInStud.Right + 1.0f);

			this.areaTopNumericUpDown.Minimum = (Decimal)(mTotalAreaInStud.Top - 1.0f);
            this.areaTopNumericUpDown.Maximum = (Decimal)(mTotalAreaInStud.Bottom);

            this.areaBottomNumericUpDown.Minimum = (Decimal)(mTotalAreaInStud.Top);
            this.areaBottomNumericUpDown.Maximum = (Decimal)(mTotalAreaInStud.Bottom + 1.0f);

			// set the value after setting the minimum and maximum otherwise we can raise an exeption
			// and used the local saved variable because the min/max setting may have already call
			// the value changed event to clamp the value, and in the event the mSelectedAreaInStud is changed
			this.areaLeftNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Left);
			this.areaRightNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Right);
			this.areaTopNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Top);
			this.areaBottomNumericUpDown.Value = (Decimal)(initialSelectedAreaInStud.Bottom);
		}

        private void saveAndChangeDisplaySettings()
        {
            // save the settings
            mOriginalDisplayWatermarkInSetting = Settings.Default.DisplayGeneralInfoWatermark;
            mOriginalDisplayElectricCircuitInSetting = Settings.Default.DisplayElectricCircuit;
            mOriginalDisplayConnectionPointInSetting = Settings.Default.DisplayFreeConnexionPoints;
            // now change them: for each we change the check state and simulate a click
            this.exportWatermarkCheckBox.Checked = Map.Instance.ExportWatermark;
            this.exportWatermarkCheckBox_Click(null, null);

            this.exportElectricCircuitCheckBox.Checked = Map.Instance.ExportElectricCircuit;
            this.exportElectricCircuitCheckBox_Click(null, null);

            this.exportConnectionPointCheckBox.Checked = Map.Instance.ExportConnectionPoints;
            this.exportConnectionPointCheckBox_Click(null, null);
        }

        /// <summary>
        /// Restore the display settings that have been altered by the user on this Export Form, as export options
        /// </summary>
        private void restoreDisplaySettings()
        {
            Settings.Default.DisplayGeneralInfoWatermark = mOriginalDisplayWatermarkInSetting;
            Settings.Default.DisplayElectricCircuit = mOriginalDisplayElectricCircuitInSetting;
            Settings.Default.DisplayFreeConnexionPoints = mOriginalDisplayConnectionPointInSetting;
        }
		#endregion

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
				Map.Instance.draw(graphics, mTotalAreaInStud, mTotalScalePixelPerStud, false);
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

				// compute the selected area in pixel unit
				RectangleF selectedAreaInPixel = new RectangleF((float)((mSelectedAreaInStud.X - mTotalAreaInStud.X) * mTotalScalePixelPerStud),
																(float)((mSelectedAreaInStud.Y - mTotalAreaInStud.Y) * mTotalScalePixelPerStud),
																(float)(mSelectedAreaInStud.Width * mTotalScalePixelPerStud),
																(float)(mSelectedAreaInStud.Height * mTotalScalePixelPerStud));

				// draw the watermark
				RectangleF watermarkRectangleInStud = new RectangleF(mTotalAreaInStud.X, mTotalAreaInStud.Y,
																	mSelectedAreaInStud.X - mTotalAreaInStud.X + mSelectedAreaInStud.Width,
																	mSelectedAreaInStud.Y - mTotalAreaInStud.Y + mSelectedAreaInStud.Height);
				Map.Instance.drawWatermark(graphics, watermarkRectangleInStud, mTotalScalePixelPerStud);

				// draw black overlay to over the margins
				graphics.FillRectangle(mMarginBlackOverlayBrush, 0, 0, selectedAreaInPixel.Left, this.previewPictureBox.Image.Height);
				graphics.FillRectangle(mMarginBlackOverlayBrush, selectedAreaInPixel.Right, 0, this.previewPictureBox.Image.Width - selectedAreaInPixel.Right, this.previewPictureBox.Image.Height);
				graphics.FillRectangle(mMarginBlackOverlayBrush, selectedAreaInPixel.Left, 0, selectedAreaInPixel.Width, selectedAreaInPixel.Top);
				graphics.FillRectangle(mMarginBlackOverlayBrush, selectedAreaInPixel.Left, selectedAreaInPixel.Bottom, selectedAreaInPixel.Width, this.previewPictureBox.Image.Height - selectedAreaInPixel.Bottom);

				// and draw the selected area
				graphics.DrawRectangle(mSelectionPen, selectedAreaInPixel.X, selectedAreaInPixel.Y, selectedAreaInPixel.Width, selectedAreaInPixel.Height);

				// draw the grid inside the select area if there's more than one image to export
				float columnWidthInPixel = selectedAreaInPixel.Width / (int)this.columnCountNumericUpDown.Value;
				for (int i = 1; i < this.columnCountNumericUpDown.Value; ++i)
				{
					float lineX = selectedAreaInPixel.X + (columnWidthInPixel * i);
					graphics.DrawLine(mSelectionPen, lineX, selectedAreaInPixel.Top, lineX, selectedAreaInPixel.Bottom);
				}
				float rowHeightInPixel = selectedAreaInPixel.Height / (int)this.rowCountNumericUpDown.Value;
				for (int i = 1; i < this.rowCountNumericUpDown.Value; ++i)
				{
					float lineY = selectedAreaInPixel.Y + (rowHeightInPixel * i);
					graphics.DrawLine(mSelectionPen, selectedAreaInPixel.Left, lineY, selectedAreaInPixel.Right, lineY);
				}
			}
		}

        private void drawAll()
        {
            drawMapImage();
            drawPreviewImage();
        }

		private void updateImage(NumericUpDown firstSender)
		{
			if (mCanUpdateImage)
			{
				drawPreviewImage();
				mFirstSender = firstSender;
				computeImageSizeFromAreaAndScale();
				mFirstSender = null;
			}
		}

		private void suspendUpdateImage()
		{
			mCanUpdateImage = false;
		}

		private void resumeUpdateImage()
		{
			mCanUpdateImage = true;
			updateImage(this.areaLeftNumericUpDown);
		}
		#endregion

		#region compute linked values
		private void updateMinAndMaxScaleAccordingToSelectedArea()
		{
			// get the biggest dimension among width and height of the selected area, and ensure it is not null
			double bigestDimensionOfSelectedArea = Math.Max(1.0, Math.Max(mSelectedAreaInStud.Width / (float)this.columnCountNumericUpDown.Value, mSelectedAreaInStud.Height / (float)this.rowCountNumericUpDown.Value));
			// compute the max scale according to the max size of the export image, and the size of the total area (divided by the grid of exported images)
			double maxScale = MAX_IMAGE_SIZE_IN_PIXEL / bigestDimensionOfSelectedArea;
			double minScale = Math.Min(0.01, maxScale / 2); // 0.01 by default, the second value is to handle extrem case where the max is under 0.01
			double incScale = Math.Min(0.01, maxScale / 4); // 0.01 by default, the second value is to handle extrem case where the distance between min and max is less than 0.01
			this.scaleNumericUpDown.Maximum = (Decimal)maxScale;
			this.scaleNumericUpDown.Minimum = (Decimal)minScale;
			this.scaleNumericUpDown.Increment = (Decimal)incScale;
		}

		private void computeImageSizeFromAreaAndScale()
		{
			float newScaleValue = (float)(this.scaleNumericUpDown.Value);
			if (mFirstSender != this.imageWidthNumericUpDown)
			{
				int newValue = (int)((mSelectedAreaInStud.Width / (float)this.columnCountNumericUpDown.Value) * newScaleValue);
				if (newValue < (int)(this.imageWidthNumericUpDown.Minimum))
					newValue = (int)(this.imageWidthNumericUpDown.Minimum);
				if (newValue > (int)(this.imageWidthNumericUpDown.Maximum))
					newValue = (int)(this.imageWidthNumericUpDown.Maximum);
				this.imageWidthNumericUpDown.Value = (Decimal)newValue;
			}
			if (mFirstSender != this.imageHeightNumericUpDown)
			{
				int newValue = (int)((mSelectedAreaInStud.Height / (float)this.rowCountNumericUpDown.Value) * newScaleValue);
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
		#endregion

		#region event handler
		#region display options
		private void exportWatermarkCheckBox_Click(object sender, EventArgs e)
		{
			// we use the click event and not the CheckedChanged, because we don't want this to be called at startup, when the form is initialized
			Settings.Default.DisplayGeneralInfoWatermark = this.exportWatermarkCheckBox.Checked;
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

		#region aera size
		private void areaLeftNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaLeftNumericUpDown.Value);
			mSelectedAreaInStud.Width += mSelectedAreaInStud.X - newValue;
			mSelectedAreaInStud.X = newValue;
            // if the new Width is valid, update the image, otherwise move the left value also
            if (mSelectedAreaInStud.Width > 0.0f)
            {
			    updateMinAndMaxScaleAccordingToSelectedArea();
			    updateImage(this.areaLeftNumericUpDown);
            }
            else
            {
                this.areaRightNumericUpDown.Value = (Decimal)(newValue + 1.0f);
            }
        }

		private void areaTopNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaTopNumericUpDown.Value);
			mSelectedAreaInStud.Height += mSelectedAreaInStud.Y - newValue;
			mSelectedAreaInStud.Y = newValue;
            // if the new height is valid, update the image, otherwise move the bottom value also (which will update the image)
            if (mSelectedAreaInStud.Height > 0.0f)
            {
                updateMinAndMaxScaleAccordingToSelectedArea();
                updateImage(this.areaTopNumericUpDown);
            }
            else
            {
                this.areaBottomNumericUpDown.Value = (Decimal)(newValue + 1.0f);
            }
		}

		private void areaRightNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaRightNumericUpDown.Value);
			mSelectedAreaInStud.Width = newValue - mSelectedAreaInStud.X;
            // if the new Width is valid, update the image, otherwise move the left value also
            if (mSelectedAreaInStud.Width > 0.0f)
            {
                updateMinAndMaxScaleAccordingToSelectedArea();
                updateImage(this.areaRightNumericUpDown);
            }
            else
            {
                this.areaLeftNumericUpDown.Value = (Decimal)(newValue - 1.0f);
            }
		}

		private void areaBottomNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			float newValue = (float)(this.areaBottomNumericUpDown.Value);
            mSelectedAreaInStud.Height = newValue - mSelectedAreaInStud.Y;
            // if the new height is valid, update the image, otherwise move the top value also
            if (mSelectedAreaInStud.Height > 0.0f)
            {
                updateMinAndMaxScaleAccordingToSelectedArea();
                updateImage(this.areaBottomNumericUpDown);
            }
            else
            {
                this.areaTopNumericUpDown.Value = (Decimal)(newValue - 1.0f);
            }
		}
		#endregion

		#region image count and size 
		private void columnCountNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			// redraw the preview image to update the grid
			drawPreviewImage();
			// the min and max scale depends on the number of column and rows, as if you have multiple col/rows, you can increase the scale
			updateMinAndMaxScaleAccordingToSelectedArea();
			// also update the scale value (after changing the min max of the scale) based on the current value of the image width
			// because if we increase the number of colums, the scale can increase with the same image width
			imageWidthNumericUpDown_ValueChanged(sender, e);
			// after updating the min max of scale, then the scale value (in that order), we update the image size,
			// because changing the number of columns affect the image ratio
			computeImageSizeFromAreaAndScale();
		}

		private void rowCountNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			// redraw the preview image to update the grid
			drawPreviewImage();
			// the min and max scale depends on the number of column and rows, as if you have multiple col/rows, you can increase the scale
			updateMinAndMaxScaleAccordingToSelectedArea();
			// also update the scale value (after changing the min max of the scale) based on the current value of the image height
			// because if we increase the number of rows, the scale can increase with the same image height
			imageHeightNumericUpDown_ValueChanged(sender, e);
			// after updating the min max of scale, then the scale value (in that order), we update the image size again,
			// because changing the number of rows affect the image ratio
			computeImageSizeFromAreaAndScale();
		}

		private void imageWidthNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (mFirstSender == null)
			{
				mFirstSender = this.imageWidthNumericUpDown;
				double newScaleValue = (double)(this.imageWidthNumericUpDown.Value * this.columnCountNumericUpDown.Value) / mSelectedAreaInStud.Width;
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
				double newScaleValue = (double)(this.imageHeightNumericUpDown.Value * this.rowCountNumericUpDown.Value) / mSelectedAreaInStud.Height;
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
		#endregion

		#region mouse event on image preview
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
		#endregion

		#region event for the Form
		private void ExportImageForm_SizeChanged(object sender, EventArgs e)
		{
			computePreviewPictureSizeAndPos();
			drawAll();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			// save the UI settings: if the user has exported, that means he like the option like that, and if he make
			// a new map and export, he wants to find back his last settings
			saveUISettingInDefaultSettings();

			// save the setting chosen in the Map
			Map.Instance.saveExportAreaAndDisplaySettings(mSelectedAreaInStud, (double)(this.scaleNumericUpDown.Value),
                this.exportWatermarkCheckBox.Checked, this.exportElectricCircuitCheckBox.Checked, this.exportConnectionPointCheckBox.Checked);

			// then ask the user to choose a filename and format
			string fileName;
			ImageFormat choosenFormat;
			if (getExportFileName(out fileName, out choosenFormat))
			{
				// the user have chosen a correct filename and format
				// save the new settings in the map
				Map.Instance.saveExportFileSettings(fileName, saveExportImageDialog.FilterIndex);

				// then call the function to export all the images
				exportAllImages(fileName, choosenFormat);
			}
		}

		private void ExportImageForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			// restore the display settings
			restoreDisplaySettings();

			// set the visible to false for catching this strange case:
			// Minimize the window, then right click on the task bar and choose close
			// in such case, next time you try do do a Show Dialog, an exception is raised
			this.Visible = false;
		}

		private bool getExportFileName(out string fileName, out ImageFormat choosenFormat)
		{
			fileName = string.Empty;
			choosenFormat = ImageFormat.Png;

			// check if we need to use the export settings saved in the file
			this.saveExportImageDialog.FilterIndex = Map.Instance.ExportFileTypeIndex; // it's 1 by default anyway
																					   // by default set the same name for the exported picture than the name of the map
			string fullFileName = Map.Instance.MapFileName;
			if (Map.Instance.ExportAbsoluteFileName != string.Empty)
				fullFileName = Map.Instance.ExportAbsoluteFileName;

			// remove the extension from the full file name and also set the starting directory
			this.saveExportImageDialog.FileName = Path.GetFileNameWithoutExtension(fullFileName);
			this.saveExportImageDialog.InitialDirectory = Path.GetDirectoryName(fullFileName);
			if (this.saveExportImageDialog.InitialDirectory.Length == 0)
				this.saveExportImageDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			// open the save dialog
			DialogResult result = this.saveExportImageDialog.ShowDialog(this);
			if (result == DialogResult.OK)
			{
				// find the correct format according to the last extension.
				// Normally the FileName MUST have a valid extension because the SaveFileDialog
				// automatically add an extension if the user forgot to precise one, or if
				// the use put an extension that is not in the list of the filters.
				fileName = saveExportImageDialog.FileName;
				int lastExtensionIndex = fileName.LastIndexOf('.');
				// find the correct format chosen by the user
				if (lastExtensionIndex != -1)
				{
					string extension = fileName.Substring(lastExtensionIndex + 1).ToLower();
					if (extension.Equals("bmp"))
					{
						choosenFormat = ImageFormat.Bmp;
						saveExportImageDialog.FilterIndex = 4;
					}
					else if (extension.Equals("gif"))
					{
						choosenFormat = ImageFormat.Gif;
						saveExportImageDialog.FilterIndex = 3;
					}
					else if (extension.Equals("jpg"))
					{
						choosenFormat = ImageFormat.Jpeg;
						saveExportImageDialog.FilterIndex = 2;
					}
					else if (extension.Equals("png"))
					{
						choosenFormat = ImageFormat.Png;
						saveExportImageDialog.FilterIndex = 1;
					}
					else
					{
						// the extension is not a valid extension (like "txt" for example)
						// so we choose the format according to the filter index
						// and add the correct extension
						switch (saveExportImageDialog.FilterIndex)
						{
							case 4: choosenFormat = ImageFormat.Bmp; fileName += ".bmp"; break;
							case 3: choosenFormat = ImageFormat.Gif; fileName += ".gif"; break;
							case 2: choosenFormat = ImageFormat.Jpeg; fileName += ".jpg"; break;
							case 1: choosenFormat = ImageFormat.Png; fileName += ".png"; break;
							default: choosenFormat = ImageFormat.Png; fileName += ".png"; break;
						}
					}
				}

				// user has correctly choosen the filename and format
				return true;
			}

			// user canceled the export
			return false;
		}

		private bool exportAllImages(string fileName, ImageFormat choosenFormat)
		{
			// compute the column and row size depending on the selected area to export and the number of cells in the grid
			float columnWidthInStud = mSelectedAreaInStud.Width / (int)this.columnCountNumericUpDown.Value;
			float rowHeightInStud = mSelectedAreaInStud.Height / (int)this.rowCountNumericUpDown.Value;

			// get the image width, height and scale from the numeric updown
			int imageWidth = (int)(this.imageWidthNumericUpDown.Value); 
			int imageHeight = (int)(this.imageHeightNumericUpDown.Value);
			double scalePixelPerStud = (double)(this.scaleNumericUpDown.Value);

			// create a fileInfo to get the filename and the extension separately
			FileInfo fileInfo = new FileInfo(fileName);
			int extensionIndex = fileInfo.FullName.LastIndexOf(fileInfo.Extension);
			string fileNameWithoutExtension = (extensionIndex >= 0 ? fileInfo.FullName.Remove(extensionIndex) : fileInfo.FullName) + "_";

			// a variable to know if we should check for overriding files
			// we don't check if the warning message box was forgotten by user (which means he wants override)
			bool shouldCheckForExistingFile = Settings.Default.DisplayWarningMessageForOverridingExportFiles;

			// iterate on the grid of image to export
			for (int i = 0; i < this.columnCountNumericUpDown.Value; ++i)
				for (int j = 0; j < this.rowCountNumericUpDown.Value; ++j)
				{
					// create the Bitmap and get the graphic context from it
					Bitmap image = new Bitmap(imageWidth, imageHeight, PixelFormat.Format24bppRgb);
					Graphics graphics = Graphics.FromImage(image);
					graphics.Clear(Map.Instance.BackgroundColor);
					graphics.SmoothingMode = SmoothingMode.Default; // the HighQuality let appears some grid line above the area cells
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.CompositingMode = CompositingMode.SourceOver;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic; // this one need to be high else there's some rendering bug appearing with a lower mode, the scale of the stud looks not correct when zooming out.

					// define the area to draw depending on the current coordinates of the grid cell that we are exporting
					RectangleF areaInStud = new RectangleF(mSelectedAreaInStud.X + (columnWidthInStud * i), mSelectedAreaInStud.Y + (rowHeightInStud * j), columnWidthInStud, rowHeightInStud);

					// draw the bitmap
					Map.Instance.draw(graphics, areaInStud, scalePixelPerStud, false);
					Map.Instance.drawWatermark(graphics, areaInStud, scalePixelPerStud);

					// construct a filename for each image (if there's more than one image)
					string cellImageFileName = fileName;
					if ((this.columnCountNumericUpDown.Value > 1) || (this.rowCountNumericUpDown.Value > 1))
						cellImageFileName = fileNameWithoutExtension +
											MapData.Tools.AlphabeticIndex.ConvertIndexToHumanFriendlyIndex(i + 1, true) + 
											MapData.Tools.AlphabeticIndex.ConvertIndexToHumanFriendlyIndex(j + 1, false) +
											fileInfo.Extension;

					// check if the file already exist before overriding it
					if (shouldCheckForExistingFile && File.Exists(cellImageFileName))
					{
						// use a local variable to get the value of the checkbox, by default we don't suggest the user to hide it
						bool dontDisplayMessageAgain = false;

						// display the warning message
						DialogResult result = ForgetableMessageBox.Show(this, Resources.ErrorMsgExportFileExists.Replace("&", cellImageFileName),
										Resources.ErrorMsgTitleWarning, MessageBoxButtons.YesNoCancel,
										MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, ref dontDisplayMessageAgain);

						// set back the checkbox value in the settings (don't save the settings now, it will be done when exiting the application)
						Settings.Default.DisplayWarningMessageForOverridingExportFiles = !dontDisplayMessageAgain;
						shouldCheckForExistingFile = !dontDisplayMessageAgain;

						// we should continue with a Yes, goes to the next loop for a no, and stop iterating for a cancel
						if (result == DialogResult.Yes)
							shouldCheckForExistingFile = false;
						else if (result == DialogResult.No)
						{
							// special case, if the user check the don't show the message again, and click "no", we assume it's a "no" for all images, so do a cancel instead
							if (dontDisplayMessageAgain)
								return false; // operation was canceled
							else
								continue;
						}
						else if (result == DialogResult.Cancel)
							return false; // operation was canceled
					}

					// save the bitmap in a file
					image.Save(cellImageFileName, choosenFormat);
				}

			// saving process ended successfully
			return true;
		}
		#endregion
		#endregion
	}
}