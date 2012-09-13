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
			private PointF mPoint1 = new PointF(); // coord of the first point in Stud coord
			private PointF mPoint2 = new PointF(); // coord of the second point in Stud coord
			private float mMesuredDistance = 0.0f; // the distance mesured between the two extremities in stud unit
			private bool mDisplayDistance = true; // if true, the distance is displayed on the ruler. TODO: change it to an enum when we will create the ruler unit

			private Pen mPen = new Pen(Color.Black); // the pen to draw the line
			private SolidBrush mMesurementBrush = new SolidBrush(Color.Black);
			private Font mMesurementFont = new Font(FontFamily.GenericSansSerif, 20.0f, FontStyle.Regular);
			private StringFormat mMesurementStringFormat = new StringFormat();
			private Bitmap mMesurementImage = new Bitmap(1, 1);	// image representing the text to draw in the correct orientation
			private PointF mMesurementTextWidthHalfVector = new Point(); // half the vector along the width of the mesurement text in pixel

			#region get/set
			public PointF Point1
			{
				get { return mPoint1; }
				set
				{
					mPoint1 = value;
					updateDisplayData();
				}
			}

			public PointF Point2
			{
				get { return mPoint2; }
				set
				{
					mPoint2 = value;
					updateDisplayData();
				}
			}

			public Pen Pen
			{
				get { return mPen; }
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
				// get min and max for X
				float minX = mPoint1.X;
				float maxX = mPoint2.X;
				if (minX > maxX)
				{
					minX = mPoint2.X;
					maxX = mPoint1.X;
				}
				// and for Y
				float minY = mPoint1.Y;
				float maxY = mPoint2.Y;
				if (minY > maxY)
				{
					minY = mPoint2.Y;
					maxY = mPoint1.Y;
				}
				// now set the display area from the min and max of X and Y
				float width = maxX - minX;
				float height = maxY - minY;
				mDisplayArea = new RectangleF(minX, minY, width, height);

				// compute the orientation, such as the orientation will stay upside up
				if (mPoint2.X > mPoint1.X)
					mOrientation = (float)((Math.Atan2(mPoint2.Y - mPoint1.Y, width) * 180.0) / Math.PI);
				else
					mOrientation = (float)((Math.Atan2(mPoint1.Y - mPoint2.Y, width) * 180.0) / Math.PI);

				// also compute the distance between the two points
				mMesuredDistance = (float)Math.Sqrt((width * width) + (height * height));

				// update the image
				updateMesurementImage();
			}

			/// <summary>
			/// update the image used to draw the mesurement of the ruler correctly oriented
			/// The image is drawn with the current selected unit.
			/// This method should be called when the mesurement unit change or when one of the
			/// points change
			/// </summary>
			private void updateMesurementImage()
			{
				//TODO: use the correct unit and also the string format
				string distanceAsString = mMesuredDistance.ToString();

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
					float x1 = (float)((mPoint1.X - areaInStud.Left) * scalePixelPerStud);
					float y1 = (float)((mPoint1.Y - areaInStud.Top) * scalePixelPerStud);
					float x2 = (float)((mPoint2.X - areaInStud.Left) * scalePixelPerStud);
					float y2 = (float)((mPoint2.Y - areaInStud.Top) * scalePixelPerStud);

					// draw one or 2 lines
					if (!mDisplayDistance)
					{
						g.DrawLine(mPen, x1, y1, x2, y2);
					}
					else
					{
						// compute the middle points
						float middleX = (x1 + x2) * 0.5f;
						float middleY = (y1 + y2) * 0.5f;

						// compute the middle extremity of the two lines
						float mx1 = middleX - mMesurementTextWidthHalfVector.X;
						float my1 = middleY - mMesurementTextWidthHalfVector.Y;
						float mx2 = middleX + mMesurementTextWidthHalfVector.X;
						float my2 = middleY + mMesurementTextWidthHalfVector.Y;

						// draw the two lines
						if (x1 < x2)
						{
							g.DrawLine(mPen, x1, y1, mx1, my1);
							g.DrawLine(mPen, x2, y2, mx2, my2);
						}
						else
						{
							g.DrawLine(mPen, x1, y1, mx2, my2);
							g.DrawLine(mPen, x2, y2, mx1, my1);
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
				}
			}
			#endregion
		}
	}
}
