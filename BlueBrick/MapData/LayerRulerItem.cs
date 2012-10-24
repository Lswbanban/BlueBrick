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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace BlueBrick.MapData
{
	partial class LayerRuler : Layer
	{
		/// <summary>
		/// A ruler item is a geometric item (line, a circle, etc) that can be placed on a Ruler type of Layer
		/// </summary>
		[Serializable]
		public abstract class RulerItem : LayerItem
		{
			/// <summary>
			/// Draw the ruler item.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			public abstract void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, int layerTransparency, ImageAttributes layerImageAttributeWithTransparency);
		}
		
		/// <summary>
		/// A Ruler is a specific RulerItem which is actually represented by a line on which the length of the line is displayed
		/// </summary>
		[Serializable]
		public class Ruler : RulerItem
		{
			// geometrical information
			private PointF mPoint1 = new PointF(); // coord of the first point in Stud coord
			private PointF mPoint2 = new PointF(); // coord of the second point in Stud coord
			private PointF mOffsetPoint1 = new PointF(); // the offset point corresponding to Point1 in stud
			private PointF mOffsetPoint2 = new PointF(); // the offset point corresponding to Point2 in stud
			private PointF mOffsetPoint1Extended = new PointF(); // the even more extended point corresponding to Point1 in stud
			private PointF mOffsetPoint2Extended = new PointF(); // the even more extended point corresponding to Point2 in stud
			private PointF mUnitVector = new PointF(); // the unit vector of the line between point1 and point2
			private float mOffsetDistance = 0.0f; // the offset distance in stud coord
			private Tools.Distance mMesuredDistance = new Tools.Distance(); // the distance mesured between the two extremities in stud unit
			private bool mDisplayDistance = true; // if true, the distance is displayed on the ruler.
			private bool mDisplayUnit = true; // if true display the unit just after the distance

			// variable for the draw
			private Color mColor = Color.Black; // color of the lines
			private float mLineThickness = 4.0f; // the thickness of the lines
			private float mOffsetLineThickness = 1.0f; // the thickness of the guide lines when this ruler has an offset
			private SolidBrush mMesurementBrush = new SolidBrush(Color.Black);
			private Font mMesurementFont = new Font(FontFamily.GenericSansSerif, 20.0f, FontStyle.Regular);
			private StringFormat mMesurementStringFormat = new StringFormat();
			private Bitmap mMesurementImage = new Bitmap(1, 1);	// image representing the text to draw in the correct orientation
			private PointF mMesurementTextWidthHalfVector = new PointF(); // half the vector along the width of the mesurement text in pixel

			#region get/set
			public PointF Point1
			{
				get { return mPoint1; }
				set
				{
					mPoint1 = value;
					updateDisplayDataAndMesurementImage();
				}
			}

			public PointF Point2
			{
				get { return mPoint2; }
				set
				{
					mPoint2 = value;
					updateDisplayDataAndMesurementImage();
				}
			}

			public PointF UnitVector
			{
				get { return mUnitVector; }
			}

			public float OffsetDistance
			{
				get { return mOffsetDistance; }
				set
				{
					mOffsetDistance = value;
					updateDisplayData();
					// don't need to update the mesured distance image as the distance didn't changed
				}
			}
			#endregion

			#region constructor
			public Ruler(PointF point1, PointF point2)
			{
				mMesurementStringFormat.Alignment = StringAlignment.Center;
				mMesurementStringFormat.LineAlignment = StringAlignment.Center;
				mPoint1 = point1;
				mPoint2 = point2;
				updateDisplayData();
			}

			private void updateDisplayDataAndMesurementImage()
			{
				// first update the display area, that also recompute the mesured distance and orientation
				updateDisplayData();
				// Then call the update of the distance image after the computing of the new display area
				updateMesurementImage();
			}

			/// <summary>
			/// Compute and update the display area from the two points of the ruler.
			/// This function also compute the orientation of the ruler based on the two points.
			/// Then it compute the distance between the two points in stud and call the method
			/// to update the image of the mesurement text.
			/// This method should be called when the two points of the ruler is updated
			/// or when the ruler is moved.
			/// </summary>
			private void updateDisplayData()
			{
				// compute the vector of the orientation such as the orientation will stay upside up
				float directorVectorX = Math.Abs(mPoint1.X - mPoint2.X);
				float directorVectorY = (mPoint2.X > mPoint1.X) ? (mPoint2.Y - mPoint1.Y) : (mPoint1.Y - mPoint2.Y);

				// compute the orientation angle
				mOrientation = (float)((Math.Atan2(directorVectorY, directorVectorX) * 180.0) / Math.PI);

				// also compute the distance between the two points
				float distance = (float)Math.Sqrt((directorVectorX * directorVectorX) + (directorVectorY * directorVectorY));

				// compute the unit vector (if the distance is not null)
				if (distance > 0.0f)
					mUnitVector = new PointF(directorVectorX / distance, directorVectorY / distance);
				else
					mUnitVector = new PointF();

				// set the distance in the data member
				mMesuredDistance.DistanceInStud = distance;

				// compute the vector of the offset. This vector is turned by 90 deg from the Orientation, so
				// just invert the X and Y of the normalized vector (the offset vector can be null)
				PointF offsetNormalizedVector = new PointF(mUnitVector.Y, -mUnitVector.X);

				// compute the offset coordinates in stud
				float offsetX = offsetNormalizedVector.X * mOffsetDistance;
				float offsetY = offsetNormalizedVector.Y * mOffsetDistance;
				mOffsetPoint1 = new PointF(mPoint1.X + offsetX, mPoint1.Y + offsetY);
				mOffsetPoint2 = new PointF(mPoint2.X + offsetX, mPoint2.Y + offsetY);

				// extend a little more the offset point to draw a margin
				const float EXTEND_IN_STUD = 2.0f;
				float extendX = offsetNormalizedVector.X * ((mOffsetDistance > 0.0f) ? EXTEND_IN_STUD : -EXTEND_IN_STUD);
				float extendY = offsetNormalizedVector.Y * ((mOffsetDistance > 0.0f) ? EXTEND_IN_STUD : -EXTEND_IN_STUD);
				mOffsetPoint1Extended = new PointF(mOffsetPoint1.X + extendX, mOffsetPoint1.Y + extendY);
				mOffsetPoint2Extended = new PointF(mOffsetPoint2.X + extendX, mOffsetPoint2.Y + extendY);

				// compute the 4 corner of the ruler
				PointF[] corners = { mPoint1, mPoint2, mOffsetPoint1Extended, mOffsetPoint2Extended };

				// now find the min and max
				float minX = corners[0].X;
				float maxX = minX;
				float minY = corners[0].Y;
				float maxY = minY;
				foreach (PointF point in corners)
				{
					if (point.X < minX)
						minX = point.X;
					if (point.X > maxX)
						maxX = point.X;
					if (point.Y < minY)
						minY = point.Y;
					if (point.Y > maxY)
						maxY = point.Y;
				}

				// now set the display area from the min and max of X and Y
				float width = maxX - minX;
				float height = maxY - minY;
				mDisplayArea = new RectangleF(minX, minY, width, height);
			}

			/// <summary>
			/// update the image used to draw the mesurement of the ruler correctly oriented
			/// The image is drawn with the current selected unit.
			/// This method should be called when the mesurement unit change or when one of the
			/// points change
			/// </summary>
			private void updateMesurementImage()
			{
				// get the mesured distance in the current unit
				string distanceAsString = mMesuredDistance.ToString("N2", mDisplayUnit);

				// draw the size
				Graphics graphics = Graphics.FromImage(mMesurementImage);
				SizeF textFontSize = graphics.MeasureString(distanceAsString, mMesurementFont);
				int width = (int)textFontSize.Width;
				int height = (int)textFontSize.Height;

				// create an array with the 4 corner of the text (actually 3 if you exclude the origin)
				// and rotate them according to the orientation
				Matrix rotation = new Matrix();
				rotation.Rotate(mOrientation);
				Point[] points = { new Point(width, 0), new Point(0, height), new Point(width, height) };
				rotation.TransformVectors(points);

				// compute the vector of half the text
				mMesurementTextWidthHalfVector = new PointF((float)(points[0].X) * 0.5f, (float)(points[0].Y) * 0.5f);

				// search the min and max of all the rotated corner to compute the necessary size of the bitmap
				Point min = new Point(0, 0);
				Point max = new Point(0, 0);
				for (int i = 0; i < 3; ++i)
				{
					if (points[i].X < min.X)
						min.X = points[i].X;
					if (points[i].Y < min.Y)
						min.Y = points[i].Y;
					if (points[i].X > max.X)
						max.X = points[i].X;
					if (points[i].Y > max.Y)
						max.Y = points[i].Y;
				}

				// create the bitmap and draw the text inside
				mMesurementImage = new Bitmap(mMesurementImage, new Size(Math.Abs(max.X - min.X), Math.Abs(max.Y - min.Y)));
				graphics = Graphics.FromImage(mMesurementImage);
				rotation.Translate(mMesurementImage.Width / 2, mMesurementImage.Height / 2, MatrixOrder.Append);
				graphics.Transform = rotation;
				graphics.Clear(Color.Transparent);
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.DrawString(distanceAsString, mMesurementFont, mMesurementBrush, 0, 0, mMesurementStringFormat);
				graphics.Flush();
			}
			#endregion

			#region draw
			/// <summary>
			/// Draw the ruler.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, int layerTransparency, ImageAttributes layerImageAttributeWithTransparency)
			{
				// check if the ruler is visible
				if ((mDisplayArea.Right >= areaInStud.Left) && (mDisplayArea.Left <= areaInStud.Right) &&
					(mDisplayArea.Bottom >= areaInStud.Top) && (mDisplayArea.Top <= areaInStud.Bottom))
				{
					// transform the coordinates into pixel coordinates
					PointF offset1InPixel = Layer.sConvertPointInStudToPixel(mOffsetPoint1, areaInStud, scalePixelPerStud);
					PointF offset2InPixel = Layer.sConvertPointInStudToPixel(mOffsetPoint2, areaInStud, scalePixelPerStud);

					// create the pen for the lines
					Color colorWithTransparency = Color.FromArgb((int)(layerTransparency * 2.55f), mColor);
					Pen penForLine = new Pen(colorWithTransparency, mLineThickness);

					// draw one or 2 lines
					if (!mDisplayDistance)
					{
						g.DrawLine(penForLine, offset1InPixel, offset2InPixel);
					}
					else
					{
						// compute the middle points
						float middleX = (offset1InPixel.X + offset2InPixel.X) * 0.5f;
						float middleY = (offset1InPixel.Y + offset2InPixel.Y) * 0.5f;

						// compute the middle extremity of the two lines
						PointF middle1 = new PointF(middleX - mMesurementTextWidthHalfVector.X, middleY - mMesurementTextWidthHalfVector.Y);
						PointF middle2 = new PointF(middleX + mMesurementTextWidthHalfVector.X, middleY + mMesurementTextWidthHalfVector.Y);

						// draw the two lines of the rule between the mesure
						if (offset1InPixel.X < offset2InPixel.X)
						{
							g.DrawLine(penForLine, offset1InPixel, middle1);
							g.DrawLine(penForLine, offset2InPixel, middle2);
						}
						else
						{
							g.DrawLine(penForLine, offset1InPixel, middle2);
							g.DrawLine(penForLine, offset2InPixel, middle1);
						}

						// draw the mesurement text
						// compute the position of the text in pixel coord
						Rectangle destinationRectangle = new Rectangle();
						destinationRectangle.X = (int)middleX - (mMesurementImage.Width / 2);
						destinationRectangle.Y = (int)middleY - (mMesurementImage.Height / 2);
						destinationRectangle.Width = mMesurementImage.Width;
						destinationRectangle.Height = mMesurementImage.Height;

						// draw the image containing the text
						g.DrawImage(mMesurementImage, destinationRectangle, 0, 0, mMesurementImage.Width, mMesurementImage.Height, GraphicsUnit.Pixel, layerImageAttributeWithTransparency);
					}

					// draw the offset if needed
					if (mOffsetDistance != 0.0f)
					{
						PointF point1InPixel = Layer.sConvertPointInStudToPixel(mPoint1, areaInStud, scalePixelPerStud);
						PointF point2InPixel = Layer.sConvertPointInStudToPixel(mPoint2, areaInStud, scalePixelPerStud);
						PointF offsetExtended1InPixel = Layer.sConvertPointInStudToPixel(mOffsetPoint1Extended, areaInStud, scalePixelPerStud);
						PointF offsetExtended2InPixel = Layer.sConvertPointInStudToPixel(mOffsetPoint2Extended, areaInStud, scalePixelPerStud);

						// create the pen for the offset lines
						Pen penForOffsetLine = new Pen(colorWithTransparency, mOffsetLineThickness);

						// draw the two offset
						g.DrawLine(penForOffsetLine, point1InPixel, offsetExtended1InPixel);
						g.DrawLine(penForOffsetLine, point2InPixel, offsetExtended2InPixel);
					}
				}
			}
			#endregion
		}
	}
}
