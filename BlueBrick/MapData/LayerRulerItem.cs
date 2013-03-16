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
	public partial class LayerRuler : Layer
	{
		/// <summary>
		/// A ruler item is a geometric item (line, a circle, etc) that can be placed on a Ruler type of Layer
		/// </summary>
		[Serializable]
		public abstract class RulerItem : LayerItem
		{
			// variable for drawing
			protected Color mColor = Color.Black; // color of the lines
			protected float mLineThickness = 4.0f; // the thickness of the lines
			protected bool mDisplayDistance = true; // if true, the distance is displayed on the ruler.
			protected bool mDisplayUnit = true; // if true display the unit just after the distance

			[NonSerialized]
			protected Tools.Distance mMesuredDistance = new Tools.Distance(); // the distance mesured between the two extremities in stud unit

			// variable for drawing the mesurement value
			[NonSerialized]
			private SolidBrush mMesurementBrush = new SolidBrush(Color.Black);
			[NonSerialized]
			private Font mMesurementFont = new Font(FontFamily.GenericSansSerif, 20.0f, FontStyle.Regular);
			[NonSerialized]
			private StringFormat mMesurementStringFormat = new StringFormat();
			[NonSerialized]
			protected Bitmap mMesurementImage = new Bitmap(1, 1);	// image representing the text to draw in the correct orientation
			[NonSerialized]
			protected PointF mMesurementTextWidthHalfVector = new PointF(); // half the vector along the width of the mesurement text in pixel

			#region get/set
			public virtual bool IsAttached
			{
				get { return false; }
			}

			public abstract PointF CurrentControlPoint
			{
				get;
				set;
			}
			#endregion

			#region constructor
			/// <summary>
			/// Default constructor
			/// </summary>
			public RulerItem()
			{
				mMesurementStringFormat.Alignment = StringAlignment.Center;
				mMesurementStringFormat.LineAlignment = StringAlignment.Center;
			}

			/// <summary>
			/// Update both the display data and the image containing the mesurement
			/// string and the unit. display date is updated first, then the image
			/// </summary>
			protected void updateDisplayDataAndMesurementImage()
			{
				// first update the display area, that also recompute the mesured distance and orientation
				updateDisplayData();
				// Then call the update of the distance image after the computing of the new display area
				updateMesurementImage();
			}

			/// <summary>
			/// This function update the display area according to the geometry of the ruler
			/// </summary>
			protected abstract void updateDisplayData();

			/// <summary>
			/// update the image used to draw the mesurement of the ruler correctly oriented
			/// The image is drawn with the current selected unit.
			/// This method should be called when the mesurement unit change or when one of the
			/// points change
			/// </summary>
			protected void updateMesurementImage()
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

			#region IXmlSerializable Members
			public override void ReadXml(System.Xml.XmlReader reader)
			{
				// read the LayerItem properties
				base.ReadXml(reader);
				// read the common data of the ruler
				mColor = XmlReadWrite.readColor(reader);
				mLineThickness = reader.ReadElementContentAsFloat();
				mDisplayDistance = reader.ReadElementContentAsBoolean();
				mDisplayUnit = reader.ReadElementContentAsBoolean();
				// TODO: need to recompute the brushes, we need the call to an update method
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				// write the LayerItems properties
				base.WriteXml(writer);
				// write the date of the linear ruler
				XmlReadWrite.writeColor(writer, "Color", mColor);
				writer.WriteElementString("LineThickness", mLineThickness.ToString(System.Globalization.CultureInfo.InvariantCulture));
				writer.WriteElementString("DisplayDistance", mDisplayDistance.ToString().ToLower());
				writer.WriteElementString("DisplayUnit", mDisplayUnit.ToString().ToLower());
			}
			#endregion

			#region edition
			/// <summary>
			/// Find the closest control point of the ruler away from the specified point. Memorize it 
			/// and then compute the square distance to it and return it
			/// </summary>
			/// <param name="pointInStud">the position in stud from which searching the nearest the control points</param>
			/// <returns>the square distance from the specified point to the nearest control point in squared studs</returns>
			public abstract float findClosestControlPointAndComputeSquareDistance(PointF pointInStud);

			/// <summary>
			/// Tell if the specified point is inside an area of the ruler that can be grabed (handle)
			/// for scaling purpose.
			/// </summary>
			/// <param name="pointInStud">the position in stud for which testing the scaling handle</param>
			/// <param name="thicknessInStud">For handles that are just lines, give the thickness in stud from which the function consider the point is above</param>
			/// <returns>true if the specified point is above a scaling handle</returns>
			public abstract bool isInsideAScalingHandle(PointF pointInStud, float thicknessInStud);

			/// <summary>
			/// Get the scaling orientation of the ruler depending on the position of the mouse
			/// </summary>
			/// <param name="mouseCoordInStud">the coordinate of the mouse in stud</param>
			/// <returns>return the angle direction of the scale in degrees</returns>
			public abstract float getScalingOrientation(PointF mouseCoordInStud);
			#endregion

			#region draw
			/// <summary>
			/// Draw the ruler item.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="areaInStud">the visible area of the map in stud coordinates</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="layerTransparency">the current transparency of the parent layer</param>
			/// <param name="layerImageAttributeWithTransparency">image attribute containing current transparency in order to draw image (if needed by this ruler)</param>
			/// <param name="isSelected">tell if this ruler is currently selected in its parent layer selection</param>
			/// <param name="selectionBrush">the brush to use if this ruler is selected</param>
			public abstract void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, int layerTransparency, ImageAttributes layerImageAttributeWithTransparency, bool isSelected, SolidBrush selectionBrush);

			/// <summary>
			/// Draw only the control points of this ruler item.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="areaInStud">the visible area of the map in stud coordinates</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="color">the color including transparency in which draw the point</param>
			public abstract void drawControlPoints(Graphics g, RectangleF areaInStud, double scalePixelPerStud, Color color);

			/// <summary>
			/// Draw the specified control points of this ruler item.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="areaInStud">the visible area of the map in stud coordinates</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="color">the color including transparency in which draw the point</param>
			/// <param name="point">the point to draw</param>
			/// <param name="isPointAttached">if the specified point is attached, draw also a circle around it</param>
			protected void drawOneControlPoint(Graphics g, RectangleF areaInStud, double scalePixelPerStud, Color color, PointF point, bool isPointAttached)
			{
				// create the brush for drawing the dot in red
				SolidBrush brush = new SolidBrush(color);
				float radius = BlueBrick.Properties.Settings.Default.RulerControlPointRadiusInPixel;
				float diameter = radius * 2.0f;

				// convert the control points
				PointF pointInPixel = Layer.sConvertPointInStudToPixel(point, areaInStud, scalePixelPerStud);
				pointInPixel.X -= radius;
				pointInPixel.Y -= radius;

				// draw the two points
				g.FillEllipse(brush, pointInPixel.X, pointInPixel.Y, diameter, diameter);

				// if the point is attached, draw a circle around it
				if (isPointAttached)
				{
					// create a pen for the circle
					Pen pen = new Pen(color, radius * 0.34f); // 34% because the min radius is 3 pixel, so I want to have a line of at least 1 pixel
					// make the diameter bigger
					float circleOffsetInPixel = radius * 0.5f;
					diameter += circleOffsetInPixel * 2.0f;
					g.DrawEllipse(pen, pointInPixel.X - circleOffsetInPixel, pointInPixel.Y - circleOffsetInPixel, diameter, diameter);
				}
			}
			#endregion
		}
		
		/// <summary>
		/// A Ruler is a specific RulerItem which is actually represented by a line on which the length of the line is displayed
		/// </summary>
		[Serializable]
		public class LinearRuler : RulerItem
		{
			private enum SelectionAreaIndex
			{
				INTERNAL_1 = 0,
				EXTERNAL_1,
				EXTERNAL_2,
				INTERNAL_2
			}

			// geometrical information, the points look like that:
			// mSelectionArea[1] |        | mSelectionArea[2]
			//     mOffsetPoint1 +---42---+ mOffsetPoint2
			// mSelectionArea[0] |        | mSelectionArea[3]
			//                   |        |
			//           mPoint1 |        | mPoint2 
			private PointF mPoint1 = new PointF(); // coord of the first point in Stud coord
			private PointF mPoint2 = new PointF(); // coord of the second point in Stud coord
			private float mOffsetDistance = 0.0f; // the offset distance in stud coord

			[NonSerialized]
			private PointF mOffsetPoint1 = new PointF(); // the offset point corresponding to Point1 in stud
			[NonSerialized]
			private PointF mOffsetPoint2 = new PointF(); // the offset point corresponding to Point2 in stud
			[NonSerialized]
			private PointF mUnitVector = new PointF(); // the unit vector of the line between point1 and point2
			[NonSerialized]
			private bool mIsCurrentControlPointPoint1 = false; // tells if the current control point is point1 or point2

			// variable for the draw
			private float mOffsetLineThickness = 1.0f; // the thickness of the guide lines when this ruler has an offset
			private float[] mOffsetLineDashPattern = new float[] { 2.0f, 4.0f }; // pattern for the dashed offset line (succesion of dash length and space length, starting with dash)

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

			public override PointF CurrentControlPoint
			{
				get
				{
					if (mIsCurrentControlPointPoint1)
						return this.Point1;
					else
						return this.Point2;
				}
				set
				{
					if (mIsCurrentControlPointPoint1)
						this.Point1 = value;
					else
						this.Point2 = value;
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

			#region constructor/copy
			/// <summary>
			/// this parameter less constructor is requested for the serialization, but should not
			/// be used by the program
			/// </summary>
			public LinearRuler()
			{
			}

			/// <summary>
			/// Constructor used for the construction of a linear ruler with the mouse
			/// </summary>
			/// <param name="point1">First point of the line</param>
			/// <param name="point2">Second point of the line</param>
			public LinearRuler(PointF point1, PointF point2)
				: base()
			{
				mPoint1 = point1;
				mPoint2 = point2;
				updateDisplayData();
			}

			/// <summary>
			/// Constructor used by the clone function
			/// </summary>
			/// <param name="point1">First point of the line</param>
			/// <param name="point2">Second point of the line</param>
			/// <param name="offsetDistance">The offset distance if this ruler is offseted</param>
			public LinearRuler(PointF point1, PointF point2, float offsetDistance)
				: base()
			{
				mPoint1 = point1;
				mPoint2 = point2;
				mOffsetDistance = offsetDistance;
				updateDisplayDataAndMesurementImage();
			}

			/// <summary>
			/// Compute and update the display area from the two points of the ruler.
			/// This function also compute the orientation of the ruler based on the two points.
			/// Then it compute the distance between the two points in stud and call the method
			/// to update the image of the mesurement text.
			/// This method should be called when the two points of the ruler is updated
			/// or when the ruler is moved.
			/// </summary>
			protected override void updateDisplayData()
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
				const float EXTEND_IN_STUD = 2.0f; //TODO: maybe make it a maximum between the font size/2 and this fixed value
				float extendX = offsetNormalizedVector.X * ((mOffsetDistance > 0.0f) ? EXTEND_IN_STUD : -EXTEND_IN_STUD);
				float extendY = offsetNormalizedVector.Y * ((mOffsetDistance > 0.0f) ? EXTEND_IN_STUD : -EXTEND_IN_STUD);
				PointF[] selectionArea = new PointF[4];
				selectionArea[(int)SelectionAreaIndex.EXTERNAL_1] = new PointF(mOffsetPoint1.X + extendX, mOffsetPoint1.Y + extendY);
				selectionArea[(int)SelectionAreaIndex.EXTERNAL_2] = new PointF(mOffsetPoint2.X + extendX, mOffsetPoint2.Y + extendY);
				selectionArea[(int)SelectionAreaIndex.INTERNAL_1] = new PointF(mOffsetPoint1.X - extendX, mOffsetPoint1.Y - extendY);
				selectionArea[(int)SelectionAreaIndex.INTERNAL_2] = new PointF(mOffsetPoint2.X - extendX, mOffsetPoint2.Y - extendY);
				mSelectionArea = new Tools.Polygon(selectionArea);

				// compute the 4 corner of the ruler
				PointF[] corners = { mPoint1, mPoint2, selectionArea[(int)SelectionAreaIndex.EXTERNAL_1], selectionArea[(int)SelectionAreaIndex.EXTERNAL_2] };

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
			/// Clone this LinearRuler
			/// </summary>
			/// <returns>a new LinearRuler which is a conform copy of this</returns>
			public override LayerItem Clone()
			{
				//TODO: is it enough?: I guess not because of all the properties of the base class
				return new LinearRuler(this.Point1, this.Point2, this.OffsetDistance);
			}
			#endregion

			#region IXmlSerializable Members
			public override void ReadXml(System.Xml.XmlReader reader)
			{
				base.ReadXml(reader);
				// read the data of the ruler (don't use accessor to avoid multiple call to the update functions
				mPoint1 = XmlReadWrite.readPointF(reader);
				mPoint2 = XmlReadWrite.readPointF(reader);
				mOffsetDistance = reader.ReadElementContentAsFloat();
				mOffsetLineThickness = reader.ReadElementContentAsFloat();
				// read the end element of the ruler
				reader.ReadEndElement();

				// update the computing data after reading the 2 points and offset
				updateDisplayDataAndMesurementImage();
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("LinearRuler");
				base.WriteXml(writer);
				// write the date of the linear ruler
				XmlReadWrite.writePointF(writer, "Point1", this.Point1);
				XmlReadWrite.writePointF(writer, "Point2", this.Point2);
				writer.WriteElementString("OffsetDistance", this.OffsetDistance.ToString(System.Globalization.CultureInfo.InvariantCulture));
				writer.WriteElementString("OffsetLineThickness", mOffsetLineThickness.ToString(System.Globalization.CultureInfo.InvariantCulture));
				//TODO write the mOffsetLineDashPattern
				writer.WriteEndElement(); // end of LinearRuler
			}
			#endregion

			#region edition
			/// <summary>
			/// Find the closest control point of the ruler away from the specified point. Memorize it 
			/// and then compute the square distance to it and return it.
			/// The control point for a linear ruler are the two extremities.
			/// </summary>
			/// <param name="pointInStud">the position in stud from which searching the nearest the control points</param>
			/// <returns>the square distance from the specified point to the nearest control point in squared studs</returns>
			public override float findClosestControlPointAndComputeSquareDistance(PointF pointInStud)
			{
				float dx1 = pointInStud.X - mPoint1.X;
				float dy1 = pointInStud.Y - mPoint1.Y;
				float squaredDist1 = (dx1 * dx1) + (dy1 * dy1);
				float dx2 = pointInStud.X - mPoint2.X;
				float dy2 = pointInStud.Y - mPoint2.Y;
				float squaredDist2 = (dx2 * dx2) + (dy2 * dy2);
				// witch one is closer?
				mIsCurrentControlPointPoint1 = (squaredDist1 < squaredDist2);
				// and return the correct distance
				if (mIsCurrentControlPointPoint1)
					return squaredDist1;
				else
					return squaredDist2;
			}

			/// <summary>
			/// Tell if the specified point is inside an area of the ruler that can be grabed (handle)
			/// for scaling purpose. For a Linear ruler it is the line itself (for now use the selection area,
			/// but we should use the thickness parameter TODO)
			/// </summary>
			/// <param name="pointInStud">the position in stud for which testing the scaling handle</param>
			/// <param name="thicknessInStud">For handles that are just lines, give the thickness in stud from which the function consider the point is above</param>
			/// <returns>true if the specified point is above a scaling handle</returns>
			public override bool isInsideAScalingHandle(PointF pointInStud, float thicknessInStud)
			{
				return (this.SelectionArea.isPointInside(pointInStud));
			}

			/// <summary>
			/// Get the scaling orientation of the ruler depending on the position of the mouse.
			/// For a Linear Ruler, the scale direction is always the perpendicular of the
			/// orientation of the ruler, no matter the position of the mouse.
			/// </summary>
			/// <param name="mouseCoordInStud">the coordinate of the mouse in stud</param>
			/// <returns>return the angle direction of the scale in degrees</returns>
			public override float getScalingOrientation(PointF mouseCoordInStud)
			{
				float orientation = this.Orientation;
				if (orientation < 90.0f)
					return (this.Orientation + 90.0f);
				else
					return (this.Orientation - 90.0f);
			}
			#endregion

			#region draw
			/// <summary>
			/// Draw the ruler.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="areaInStud">the visible area of the map in stud coordinates</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="layerTransparency">the current transparency of the parent layer</param>
			/// <param name="layerImageAttributeWithTransparency">image attribute containing current transparency in order to draw image (if needed by this ruler)</param>
			/// <param name="isSelected">tell if this ruler is currently selected in its parent layer selection</param>
			/// <param name="selectionBrush">the brush to use if this ruler is selected</param>
			public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, int layerTransparency, ImageAttributes layerImageAttributeWithTransparency, bool isSelected, SolidBrush selectionBrush)
			{
				// check if the ruler is visible
				if ((mDisplayArea.Right >= areaInStud.Left) && (mDisplayArea.Left <= areaInStud.Right) &&
					(mDisplayArea.Bottom >= areaInStud.Top) && (mDisplayArea.Top <= areaInStud.Bottom))
				{
					// check if we will need to draw the dashed offset lines
					bool needToDrawOffset = (mDisplayDistance && (mOffsetDistance != 0.0f));
					bool needToDisplayHull = Properties.Settings.Default.DisplayHull;

					// transform the coordinates into pixel coordinates
					PointF offset1InPixel = Layer.sConvertPointInStudToPixel(mOffsetPoint1, areaInStud, scalePixelPerStud);
					PointF offset2InPixel = Layer.sConvertPointInStudToPixel(mOffsetPoint2, areaInStud, scalePixelPerStud);

					// internal point may be computed only for certain conditions
					PointF offsetInternal1InPixel = new PointF();
					PointF offsetInternal2InPixel = new PointF();
					if (isSelected || needToDisplayHull)
					{
						offsetInternal1InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.INTERNAL_1], areaInStud, scalePixelPerStud);
						offsetInternal2InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.INTERNAL_2], areaInStud, scalePixelPerStud);
					}

					// external point may be computed only for certain conditions
					PointF offsetExternal1InPixel = new PointF();
					PointF offsetExternal2InPixel = new PointF();					
					if (isSelected || needToDisplayHull || needToDrawOffset)
					{
						offsetExternal1InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.EXTERNAL_1], areaInStud, scalePixelPerStud);
						offsetExternal2InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.EXTERNAL_2], areaInStud, scalePixelPerStud);
					}

					// create the pen for the lines
					Color colorWithTransparency = Color.FromArgb((int)(layerTransparency * 2.55f), mColor);
					Pen penForLine = new Pen(colorWithTransparency, mLineThickness);

					// draw one or 2 lines
					if (!mDisplayDistance)
					{
						// draw one single line
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

						// draw the offset if needed
						if (needToDrawOffset)
						{
							PointF point1InPixel = Layer.sConvertPointInStudToPixel(mPoint1, areaInStud, scalePixelPerStud);
							PointF point2InPixel = Layer.sConvertPointInStudToPixel(mPoint2, areaInStud, scalePixelPerStud);

							// create the pen for the offset lines
							Pen penForOffsetLine = new Pen(colorWithTransparency, mOffsetLineThickness);
							penForOffsetLine.DashPattern = mOffsetLineDashPattern;

							// draw the two offset
							g.DrawLine(penForOffsetLine, point1InPixel, offsetExternal1InPixel);
							g.DrawLine(penForOffsetLine, point2InPixel, offsetExternal2InPixel);
						}
					}

					// compute the selection area if it is selected or if we need to draw the hull
					PointF[] selectionArea = null;
					if (isSelected || needToDisplayHull)
						selectionArea = new PointF[] { offsetInternal1InPixel, offsetExternal1InPixel, offsetExternal2InPixel, offsetInternal2InPixel };

					// draw the hull if needed
					if (needToDisplayHull)
						g.DrawPolygon(sPentoDrawHull, selectionArea);

					// draw a frame around the ruler if it is selected
					if (isSelected)						
						g.FillPolygon(selectionBrush, selectionArea);
				}
			}

			/// <summary>
			/// Draw only the control points of this ruler item.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="areaInStud">the visible area of the map in stud coordinates</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="color">the color including transparency in which draw the point</param>
			public override void drawControlPoints(Graphics g, RectangleF areaInStud, double scalePixelPerStud, Color color)
			{
				// check if the ruler is visible
				if ((mDisplayArea.Right >= areaInStud.Left) && (mDisplayArea.Left <= areaInStud.Right) &&
					(mDisplayArea.Bottom >= areaInStud.Top) && (mDisplayArea.Top <= areaInStud.Bottom))
				{
					// draw the two points
					drawOneControlPoint(g, areaInStud, scalePixelPerStud, color, mPoint1, true); //TODO is attached
					drawOneControlPoint(g, areaInStud, scalePixelPerStud, color, mPoint2, true); //TODO is attached
				}
			}
			#endregion
		}

		/// <summary>
		/// A Circle is a specific RulerItem which is represented by a circle on which the diameter is displayed
		/// </summary>
		[Serializable]
		public class CircularRuler : RulerItem
		{
			#region get/set
			/// <summary>
			/// Set or Get the center of this circle
			/// </summary>
			public override PointF Center
			{
				get { return mSelectionArea[0]; }
				set
				{
					base.Center = value;
					updateDisplayData();
				}
			}

			/// <summary>
			/// The control point of a circle is always its center
			/// </summary>
			public override PointF CurrentControlPoint
			{
				get { return this.Center; }
				set { this.Center = value; }
			}

			/// <summary>
			/// Set or Get the radius of this circle
			/// </summary>
			public float Radius
			{
				get { return (mSelectionArea as Tools.Circle).Radius; }
				set
				{
					(mSelectionArea as Tools.Circle).Radius = value;
					updateDisplayDataAndMesurementImage();
				}
			}

			/// <summary>
			/// Define a point that belongs to this circle. The radius is computed with the current 
			/// center then the circle geometry is updated
			/// </summary>
			public PointF OnePointOnCircle
			{
				set
				{
					// compute the new radius
					PointF center = mSelectionArea[0];
					PointF radiusVector = new PointF(value.X - center.X, value.Y - center.Y);
					// set the radius by calling the accessor to trigger the necessary update
					Radius = (float)Math.Sqrt((radiusVector.X * radiusVector.X) + (radiusVector.Y * radiusVector.Y));
				}
			}
			#endregion

			#region constructor/copy
			/// <summary>
			/// this parameter less constructor is requested for the serialization, but should not
			/// be used by the program
			/// </summary>
			public CircularRuler()
			{
				// instanciate an empty area
				mSelectionArea = new Tools.Circle(new PointF(0.0f, 0.0f), 0.0f);
			}

			public CircularRuler(PointF center, float radius) : base()
			{
				// define the selection area
				mSelectionArea = new Tools.Circle(center, radius);
				// update the display area
				updateDisplayDataAndMesurementImage();
			}

			/// <summary>
			/// Update the display area of this layer item from the center point and the radius
			/// </summary>
			protected override void updateDisplayData()
			{
				// get the center and radius
				PointF center = this.Center;
				float radius = this.Radius;
				// compute the display area
				mDisplayArea.X = center.X - radius;
				mDisplayArea.Y = center.Y - radius;
				float diameter = radius * 2.0f;
				mDisplayArea.Width = diameter;
				mDisplayArea.Height = diameter;
				// set the distance in the data member
				mMesuredDistance.DistanceInStud = diameter;
			}

			/// <summary>
			/// Clone this LinearRuler
			/// </summary>
			/// <returns>a new LinearRuler which is a conform copy of this</returns>
			public override LayerItem Clone()
			{
				return new CircularRuler(this.Center, this.Radius);
			}
			#endregion

			#region IXmlSerializable Members
			public override void ReadXml(System.Xml.XmlReader reader)
			{
				base.ReadXml(reader);
				// read data of the ruler (don't use this.Center because at that time the object is out of synch
				// the display area may have been read but not center yet
				mSelectionArea[0] = XmlReadWrite.readPointF(reader);
				this.Radius = reader.ReadElementContentAsFloat();
				// read the end element of the ruler
				reader.ReadEndElement();
				// don't need to update the display area after reading the data values, because the accessor of Radius did it
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("CircularRuler");
				base.WriteXml(writer);
				// write ruler data
				XmlReadWrite.writePointF(writer, "Center", this.Center);
				writer.WriteElementString("Radius", this.Radius.ToString(System.Globalization.CultureInfo.InvariantCulture));
				writer.WriteEndElement(); // end of CircularRuler
			}
			#endregion

			#region edition
			/// <summary>
			/// For a circular ruler, there's only one control point which is the center, so this function
			/// just compute the square distance to the center and return it
			/// </summary>
			/// <param name="pointInStud">the position in stud from which searching the nearest the control points</param>
			/// <returns>the square distance from the specified point to the nearest control point in squared studs</returns>
			public override float findClosestControlPointAndComputeSquareDistance(PointF pointInStud)
			{
				float dx = pointInStud.X - Center.X;
				float dy = pointInStud.Y - Center.Y;
				return ((dx * dx) + (dy * dy));
			}

			/// <summary>
			/// Tell if the specified point is inside an area of the ruler that can be grabed (handle)
			/// for scaling purpose. For a Circular ruler it is the circle with a certain thickness
			/// (but not inside the disk).
			/// </summary>
			/// <param name="pointInStud">the position in stud for which testing the scaling handle</param>
			/// <param name="thicknessInStud">For handles that are just lines, give the thickness in stud from which the function consider the point is above</param>
			/// <returns>true if the specified point is above a scaling handle</returns>
			public override bool isInsideAScalingHandle(PointF pointInStud, float thicknessInStud)
			{
				// compute distance of the mouse to the center
				float dx = pointInStud.X - Center.X;
				float dy = pointInStud.Y - Center.Y;
				float distance = (float)Math.Sqrt((dx * dx) + (dy * dy));
				// true if the difference between the radius and the distance is less than the thikness
				return ((float)Math.Abs(this.Radius - distance) <= thicknessInStud);
			}

			/// <summary>
			/// Get the scaling orientation of the ruler depending on the position of the mouse.
			/// For a Circular Ruler, this is the direction between the mouse coord and the center
			/// of the circle
			/// </summary>
			/// <param name="mouseCoordInStud">the coordinate of the mouse in stud</param>
			/// <returns>return the angle direction of the scale in degrees</returns>
			public override float getScalingOrientation(PointF mouseCoordInStud)
			{
				float dx = mouseCoordInStud.X - Center.X;
				float dy = mouseCoordInStud.Y - Center.Y;
				return (float)(Math.Atan2(dy, dx) * (180.0 / Math.PI));
			}
			#endregion

			#region draw
			/// <summary>
			/// Draw the ruler.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="areaInStud">the visible area of the map in stud coordinates</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="layerTransparency">the current transparency of the parent layer</param>
			/// <param name="layerImageAttributeWithTransparency">image attribute containing current transparency in order to draw image (if needed by this ruler)</param>
			/// <param name="isSelected">tell if this ruler is currently selected in its parent layer selection</param>
			/// <param name="selectionBrush">the brush to use if this ruler is selected</param>
			public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud, int layerTransparency, ImageAttributes layerImageAttributeWithTransparency, bool isSelected, SolidBrush selectionBrush)
			{
				// check if the ruler is visible
				if ((mDisplayArea.Right >= areaInStud.Left) && (mDisplayArea.Left <= areaInStud.Right) &&
					(mDisplayArea.Bottom >= areaInStud.Top) && (mDisplayArea.Top <= areaInStud.Bottom))
				{
					// create the pen to draw the circle
					Color colorWithTransparency = Color.FromArgb((int)(layerTransparency * 2.55f), mColor);
					Pen penForCircle = new Pen(colorWithTransparency, mLineThickness);
					// convert the display area in pixel
					RectangleF displayAreaInPixel = Layer.sConvertRectangleInStudToPixel(mDisplayArea, areaInStud, scalePixelPerStud);
					// draw the circle
					g.DrawEllipse(penForCircle, displayAreaInPixel);

					// draw the image containing the text
					if (mDisplayDistance)
					{
						// draw the mesurement text
						// compute the position of the text in pixel coord
						PointF centerInPixel = Layer.sConvertPointInStudToPixel(Center, areaInStud, scalePixelPerStud);
						Rectangle destinationRectangle = new Rectangle();
						destinationRectangle.X = (int)centerInPixel.X - (mMesurementImage.Width / 2);
						destinationRectangle.Y = (int)centerInPixel.Y - (mMesurementImage.Height / 2);
						destinationRectangle.Width = mMesurementImage.Width;
						destinationRectangle.Height = mMesurementImage.Height;
						// draw the image
						g.DrawImage(mMesurementImage, destinationRectangle, 0, 0, mMesurementImage.Width, mMesurementImage.Height, GraphicsUnit.Pixel, layerImageAttributeWithTransparency);
					}

					// draw the hull if needed
					if (Properties.Settings.Default.DisplayHull)
						g.DrawEllipse(sPentoDrawHull, displayAreaInPixel);

					// draw the selection overlay
					if (isSelected)
						g.FillEllipse(selectionBrush, displayAreaInPixel);
				}
			}

			/// <summary>
			/// Draw only the control points of this ruler item.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="areaInStud">the visible area of the map in stud coordinates</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="color">the color including transparency in which draw the point</param>
			public override void drawControlPoints(Graphics g, RectangleF areaInStud, double scalePixelPerStud, Color color)
			{
				// check if the ruler is visible
				if ((mDisplayArea.Right >= areaInStud.Left) && (mDisplayArea.Left <= areaInStud.Right) &&
					(mDisplayArea.Bottom >= areaInStud.Top) && (mDisplayArea.Top <= areaInStud.Bottom))
				{
					drawOneControlPoint(g, areaInStud, scalePixelPerStud, color, this.Center, true);  //TODO is attached
				}
			}
			#endregion
		}
	}
}
