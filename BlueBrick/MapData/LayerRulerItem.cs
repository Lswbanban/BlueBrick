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
using BlueBrick.SaveLoad;

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
			protected const float GUARANTIED_SPACE_FOR_DISTANCE_DRAWING_IN_STUD = 8.0f; // the minimum distance in stud that the ruler will reserved for drawing the distance
			protected const float MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD = 4.0f; // the minimum distance in stud from wich we start to draw something fake to avoid drawing nothing
			protected const float HALF_MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD = MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD * 0.5f; // half of the previous value

			// the main line for the ruler
			protected Color mColor = Properties.Settings.Default.RulerDefaultLineColor; // color of the lines
			protected float mLineThickness = Properties.Settings.Default.RulerDefaultLineThickness; // the thickness of the lines
			protected bool mDisplayDistance = Properties.Settings.Default.RulerDefaultDisplayMeasureText; // if true, the distance is displayed on the ruler.
			protected bool mDisplayUnit = Properties.Settings.Default.RulerDefaultDisplayUnit; // if true display the unit just after the distance

			// the secondary lines (or guidelines) for the ruler
			protected Color mGuidelineColor = Properties.Settings.Default.RulerDefaultGuidelineColor; // color of the secondary lines
			protected float mGuidelineThickness = Properties.Settings.Default.RulerDefaultGuidelineThickness; // the thickness of the guide lines when this ruler has an offset
			protected float[] mGuidelineDashPattern = new float[] { Properties.Settings.Default.RulerDefaultDashPatternLine, Properties.Settings.Default.RulerDefaultDashPatternSpace }; // pattern for the dashed offset line (succesion of dash length and space length, starting with dash)

			[NonSerialized]
			protected Tools.Distance mMeasuredDistance = new Tools.Distance(0.0f, (Tools.Distance.Unit)Properties.Settings.Default.RulerDefaultUnit); // the distance mesured between the two extremities in stud unit

			// variable for drawing the mesurement value
			private Font mMeasureFont = Properties.Settings.Default.RulerDefaultFont;
			[NonSerialized]
			private SolidBrush mMeasureBrush = new SolidBrush(Properties.Settings.Default.RulerDefaultFontColor);
			[NonSerialized]
			private StringFormat mMeasureStringFormat = new StringFormat();
			[NonSerialized]
			private Bitmap mMeasureImage = new Bitmap(1, 1);	// image representing the text to draw in the correct orientation
			[NonSerialized]
			private Point mMeasureTextSizeInPixel = new Point(); // the length and width in pixel of the mesurement text as drawn in the image (which is not the length and width of the image because the image is rotated)
			[NonSerialized]
			private float mMeasureImageScale = 1.0f; // the scaling factor of the image, because sometimes it need to be squizzed

			#region get/set
			public virtual bool IsNotAttached
			{
				get { return true; }
			}

			public virtual bool IsFullyAttached
			{
				get { return false; }
			}

			public virtual bool IsCurrentControlPointAttached
			{
				get { return false; }
			}

			public virtual LayerBrick.Brick BrickAttachedToCurrentControlPoint
			{
				get { return null; }
			}			

			public abstract PointF CurrentControlPoint
			{
				get;
				set;
			}

			public virtual int CurrentControlPointIndex
			{
				get { return 0; }
				set { }
			}

			protected float MesurementTextWidthInPixel
			{
				get { return (mMeasureTextSizeInPixel.X * mMeasureImageScale); }
			}

			protected float MesurementTextHeightInPixel
			{
				get { return (mMeasureTextSizeInPixel.Y * mMeasureImageScale); }
			}

			public Color Color
			{
				get { return mColor; }
				set { mColor = value; }
			}

			public float LineThickness
			{
				get { return mLineThickness; }
				set { mLineThickness = value; }
			}

			public bool DisplayDistance
			{
				get { return mDisplayDistance; }
				set { mDisplayDistance = value; }
			}

			public bool DisplayUnit
			{
				get { return mDisplayUnit; }
				set { mDisplayUnit = value; }
			}

			public Color GuidelineColor
			{
				get { return mGuidelineColor; }
				set { mGuidelineColor = value; }
			}

			public float GuidelineThickness
			{
				get { return mGuidelineThickness; }
				set { mGuidelineThickness = value; }
			}

			public float[] GuidelineDashPattern
			{
				get { return mGuidelineDashPattern; }
				set { mGuidelineDashPattern = value; }
			}

			public Font MeasureFont
			{
				get { return mMeasureFont; }
				set { mMeasureFont = value; }
			}

			public Color MeasureColor
			{
				get { return mMeasureBrush.Color; }
				set { mMeasureBrush.Color = value; }
			}

			public Tools.Distance.Unit CurrentUnit
			{
				get { return mMeasuredDistance.CurrentUnit; }
				set { mMeasuredDistance.CurrentUnit = value; }
			}
			#endregion

			#region constructor
			/// <summary>
			/// Default constructor
			/// </summary>
			public RulerItem()
			{
				mMeasureStringFormat.Alignment = StringAlignment.Center;
				mMeasureStringFormat.LineAlignment = StringAlignment.Center;
			}

			/// <summary>
			/// Copy constructor
			/// </summary>
			public RulerItem(RulerItem model)
				: base (model)
			{
				mColor = model.Color;
				mLineThickness = model.mLineThickness;
				mDisplayDistance = model.mDisplayDistance;
				mDisplayUnit = model.mDisplayUnit;
				mGuidelineColor = model.mGuidelineColor;
				mGuidelineThickness = model.mGuidelineThickness;
				mGuidelineDashPattern = model.mGuidelineDashPattern;
				mMeasuredDistance = new Tools.Distance(model.mMeasuredDistance.DistanceInCurrentUnit, model.mMeasuredDistance.CurrentUnit);
				mMeasureFont = model.mMeasureFont.Clone() as Font;
				mMeasureBrush = new SolidBrush(model.mMeasureBrush.Color);
				mMeasureStringFormat = model.mMeasureStringFormat.Clone() as StringFormat;
				// the rest of the variable should be initialized when the geometry will be created
			}

			/// <summary>
			/// Update both the display data and the image containing the mesurement
			/// string and the unit.
			/// </summary>
			public void updateDisplayDataAndMesurementImage()
			{
				// first recompute the mesured distance and orientation
				updateGeometryData();
				// Then call the update of the distance image after the computing of the new display area
				updateMesurementImage();
				// then update the display data (display area, selection area, etc...)
				updateDisplayData();
			}

			/// <summary>
			/// This function update geometry date of the ruler (distance and orientation)
			/// </summary>
			protected abstract void updateGeometryData();

			/// <summary>
			/// This function update the display area according to the geometry of the ruler
			/// </summary>
			protected abstract void updateDisplayData();

			/// <summary>
			/// This method should be call by the ruler layer that own this ruler when the zooming scale
			/// changed, because some ruler may have their selection area linked to the zooming scale
			/// when the ruler wants to display a selection area independant from the scale
			/// </summary>
			/// <param name="oldScaleInPixelPerStud">The previous scale</param>
			/// <param name="newScaleInPixelPerStud">The new scale</param>
			public virtual void zoomScaleChangeNotification(double oldScaleInPixelPerStud, double newScaleInPixelPerStud)
			{
				updateMesurementImageScale(newScaleInPixelPerStud);
			}

			/// <summary>
			/// When there's not enough space to draw the mesurement text because the size of the ruler is too small,
			/// we actually rescale the image in order to try to fit in the available space
			/// <param name="scalePixelPerStud">the scale of the map that should be used in order to convert stud into screen pixels</param>
			/// </summary>
			protected void updateMesurementImageScale(double scalePixelPerStud)
			{
				// get the current length of the text to draw
				float textLengthInPixel = mMeasureTextSizeInPixel.X;

				// first check if the size is not too big depending on the scale
				int fullAvailableWidth = (int)(Math.Max(mMeasuredDistance.DistanceInStud, GUARANTIED_SPACE_FOR_DISTANCE_DRAWING_IN_STUD) * scalePixelPerStud);
				int availableWidthForTextInPixel = fullAvailableWidth - Math.Min(20, Math.Max(fullAvailableWidth / 20, 4));
				if (textLengthInPixel > availableWidthForTextInPixel)
					mMeasureImageScale = (float)availableWidthForTextInPixel / (float)textLengthInPixel;
				else
					mMeasureImageScale = 1.0f;
			}

			/// <summary>
			/// update the image used to draw the mesurement of the ruler correctly oriented
			/// The image is drawn with the current selected unit.
			/// This method should be called when the mesurement unit change or when one of the
			/// points change
			/// </summary>
			protected void updateMesurementImage()
			{
				// get the mesured distance in the current unit
				string distanceAsString = mMeasuredDistance.ToString("N2", mDisplayUnit);

				// draw the size
				Graphics graphics = Graphics.FromImage(mMeasureImage);
				SizeF textFontSize = graphics.MeasureString(distanceAsString, mMeasureFont);
				int width = (int)textFontSize.Width;
				int height = (int)textFontSize.Height;

				// after setting the text size, call the function to compute the scale
				mMeasureTextSizeInPixel.X = width;
				mMeasureTextSizeInPixel.Y = height;
				updateMesurementImageScale(MainForm.Instance.MapViewScale);

				// create an array with the 4 corner of the text (actually 3 if you exclude the origin)
				// and rotate them according to the orientation
				Matrix rotation = new Matrix();
				rotation.Rotate(mOrientation);
				Point[] points = { new Point(width, 0), new Point(0, height), new Point(width, height) };
				rotation.TransformVectors(points);

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
				mMeasureImage = new Bitmap(mMeasureImage, new Size(Math.Abs(max.X - min.X), Math.Abs(max.Y - min.Y)));
				graphics = Graphics.FromImage(mMeasureImage);
				rotation.Translate(mMeasureImage.Width / 2, mMeasureImage.Height / 2, MatrixOrder.Append);
				graphics.Transform = rotation;
				graphics.Clear(Color.Transparent);
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.DrawString(distanceAsString, mMeasureFont, mMeasureBrush, 0, 0, mMeasureStringFormat);
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
				mGuidelineColor = XmlReadWrite.readColor(reader);
				mGuidelineThickness = XmlReadWrite.readFloat(reader);
				mGuidelineDashPattern = XmlReadWrite.readFloatArray(reader);
				this.CurrentUnit = (Tools.Distance.Unit)(XmlReadWrite.readInteger(reader));
				mMeasureFont = XmlReadWrite.readFont(reader);
				this.MeasureColor = XmlReadWrite.readColor(reader);
				// the update method will be called by the non abstract derivated class
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				// write the LayerItems properties
				base.WriteXml(writer);
				// write the date of the linear ruler
				XmlReadWrite.writeColor(writer, "Color", mColor);
				XmlReadWrite.writeFloat(writer, "LineThickness", mLineThickness);
				XmlReadWrite.writeBoolean(writer, "DisplayDistance", mDisplayDistance);
				XmlReadWrite.writeBoolean(writer, "DisplayUnit", mDisplayUnit);
				XmlReadWrite.writeColor(writer, "GuidelineColor", mGuidelineColor);
				XmlReadWrite.writeFloat(writer, "GuidelineThickness", mGuidelineThickness);
				XmlReadWrite.writeFloatArray(writer, "GuidelineDashPattern", mGuidelineDashPattern);
				XmlReadWrite.writeInteger(writer, "Unit", (int)(this.CurrentUnit));				
				XmlReadWrite.writeFont(writer, "MeasureFont", mMeasureFont);
				XmlReadWrite.writeColor(writer, "MeasureFontColor", this.MeasureColor);
			}

            public override void ReadLDraw(string[] line, ref int index, int version)
            {
                // read the LayerItem properties
                base.ReadLDraw(line, ref index, version);
                // read the common data of the ruler
                mDisplayDistance = LDrawReadWrite.readBoolean(line[index++]);
                mDisplayUnit = LDrawReadWrite.readBoolean(line[index++]);
                mColor = LDrawReadWrite.readColor(line[index++]);
                mGuidelineColor = LDrawReadWrite.readColor(line[index++]);
                this.MeasureColor = LDrawReadWrite.readColor(line[index++]);
                mLineThickness = LDrawReadWrite.readFloat(line[index++]);
                mGuidelineThickness = LDrawReadWrite.readFloat(line[index++]);
                mGuidelineDashPattern = LDrawReadWrite.readFloatArray(line[index++]);
                this.CurrentUnit = (Tools.Distance.Unit)(LDrawReadWrite.readInteger(line[index++]));
                mMeasureFont = LDrawReadWrite.readFont(line[index++]);
                // the update method will be called by the non abstract derivated class
            }

            public override void WriteLDraw(ref string line)
            {
                // write the LayerItems properties
                base.WriteLDraw(ref line);
                // write the date of the linear ruler
                LDrawReadWrite.writeBoolean(ref line, mDisplayDistance);
                LDrawReadWrite.writeBoolean(ref line, mDisplayUnit);
                LDrawReadWrite.writeColor(ref line, mColor);
                LDrawReadWrite.writeColor(ref line, mGuidelineColor);
                LDrawReadWrite.writeColor(ref line, this.MeasureColor);
                LDrawReadWrite.writeFloat(ref line, mLineThickness);
                LDrawReadWrite.writeFloat(ref line, mGuidelineThickness);
                LDrawReadWrite.writeFloatArray(ref line, mGuidelineDashPattern);
                LDrawReadWrite.writeInteger(ref line, (int)(this.CurrentUnit));
                LDrawReadWrite.writeFont(ref line, mMeasureFont);
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
			/// Scale this ruler such as the ruler is above the specified point.
			/// </summary>
			/// <param name="pointInStud">a coordinate to use to find the correct scale</param>
			public abstract void scaleToPoint(PointF pointInStud);

			/// <summary>
			/// Get a random reference point above the ruler according to the current scale of the ruler
			/// </summary>
			/// <return>a point in stud coordinate which match the current scale of the ruler</return>
			public abstract PointF getReferencePointForScale();

			/// <summary>
			/// Get the scaling orientation of the ruler depending on the position of the mouse
			/// </summary>
			/// <param name="mouseCoordInStud">the coordinate of the mouse in stud</param>
			/// <returns>return the angle direction of the scale in degrees</returns>
			public abstract float getScalingOrientation(PointF mouseCoordInStud);

			/// <summary>
			/// Get the control point corresponding to the index number (if this ruler has several control point).
			/// </summary>
			/// <param name="index">the zero based index of the wanted control point</param>
			/// <returns>the position of the control point</returns>
			public abstract PointF getControlPointPosition(int index);

			/// <summary>
			/// Set the position of the control point corresponding to the index number
			/// (if this ruler has several control point)
			/// </summary>
			/// <param name="index">the zero based index of the concerned control point</param>
			/// <param name="value">the new position in stud coordinate</param>
			public abstract void setControlPointPosition(int index, PointF positionInStud);

			/// <summary>
			/// Call this function when you want to attach the current control point to the specified brick.
			/// Be sure to choose the correct current point before calling this function
			/// </summary>
			/// <param name="index">the index of the control point to attach</param>
			/// <param name="brick">the brick to which the current control point will be attached</param>
			public abstract void attachControlPointToBrick(int index, LayerBrick.Brick brick);

			/// <summary>
			/// Call this function when you want to detach the current control point.
			/// Be sure to choose the correct current point before calling this function.
			/// </summary>
			public abstract void detachControlPoint(int index);
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

			/// <summary>
			/// draw the mesurement image at the specified position. The image may be drawn at a smaller scale
			/// if there is not enough space to draw it.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			/// <param name="centerInPixel">Position in pixel of the center of the image to draw</param>
			/// <param name="scalePixelPerStud">the current scale of the map in order to convert stud into screen pixels</param>
			/// <param name="layerImageAttributeWithTransparency">image attribute containing current transparency in order to draw image (if needed by this ruler)</param>
			protected void drawMesurementImage(Graphics g, PointF centerInPixel, double scalePixelPerStud, ImageAttributes layerImageAttributeWithTransparency)
			{
				// draw the mesurement text
				Rectangle destinationRectangle = new Rectangle();

				// compute the position of the text in pixel coord and it size according to the rescaling factor
				destinationRectangle.Width = (int)((float)mMeasureImage.Width * mMeasureImageScale);
				destinationRectangle.Height = (int)((float)mMeasureImage.Height * mMeasureImageScale);
				destinationRectangle.X = (int)centerInPixel.X - (destinationRectangle.Width / 2);
				destinationRectangle.Y = (int)centerInPixel.Y - (destinationRectangle.Height / 2);

				// draw the image containing the text
				g.DrawImage(mMeasureImage, destinationRectangle, 0, 0, mMeasureImage.Width, mMeasureImage.Height, GraphicsUnit.Pixel, layerImageAttributeWithTransparency);
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

			[Serializable]
			private class ControlPoint
			{
				public PointF mPoint = new PointF(); // coord of the first point in Stud coord
				public LayerBrick.Brick mAttachedBrick = null;
				public int mAttachedBrickHashCodeUsedDuringLoading = 0;

				[NonSerialized]
				public PointF mOffsetPoint = new PointF(); // the offset point corresponding to mPoint in stud
			}

			// geometrical information, the points look like that:
			//             mSelectionArea[1] |        | mSelectionArea[2]
			// mControlPoint[0].mOffsetPoint +---42---+ mControlPoint[1].mOffsetPoint
			//             mSelectionArea[0] |        | mSelectionArea[3]
			//                               |        |
			//       mControlPoint[0].mPoint |        | mControlPoint[1].mPoint 
			private ControlPoint[] mControlPoint = { new ControlPoint(), new ControlPoint() };
			private float mOffsetDistance = 0.0f; // the offset distance in stud coord
			private bool mAllowOffset = Properties.Settings.Default.RulerDefaultAllowOffset; // if true the line can be offseted

			[NonSerialized]
			private PointF mUnitVector = new PointF(); // the unit vector of the line between point1 and point2
			[NonSerialized]
			private int mCurrentControlPointIndex = 0;

			#region get/set
			/// <summary>
			/// If you try to set the orientation of a linear ruler, it will rotate the two connection points
			/// from the center of these two points, except if one control point is attached, it will use this
			/// attached point as the pivot. If both point are attached, this will have no effect.
			/// Getting the Orientation will return the angle of the line between the two control points
			/// </summary>
			public override float Orientation
			{
				set
				{
					if (!IsFullyAttached)
					{
						// rotate the unit vector
						PointF[] vector = { mUnitVector };
						Matrix matrix = new Matrix();
						matrix.Rotate(value - mOrientation);
						matrix.TransformVectors(vector);
						// get the current distance
						float distance = mMeasuredDistance.DistanceInStud;
						// revert it if needed (because we will use it to multiply the unit vector)
						if (mControlPoint[1].mPoint.X < mControlPoint[0].mPoint.X)
							distance = -distance;
						// check which control point should be moved
						if (mControlPoint[0].mAttachedBrick != null)
						{
							// point 1 is fixed, move point 2
							mControlPoint[1].mPoint.X = mControlPoint[0].mPoint.X + (vector[0].X * distance);
							mControlPoint[1].mPoint.Y = mControlPoint[0].mPoint.Y + (vector[0].Y * distance);
						}
						else if (mControlPoint[1].mAttachedBrick != null)
						{
							// point 2 is fixed, move point 1
							distance = -distance;
							mControlPoint[0].mPoint.X = mControlPoint[1].mPoint.X + (vector[0].X * distance);
							mControlPoint[0].mPoint.Y = mControlPoint[1].mPoint.Y + (vector[0].Y * distance);
						}
						else
						{
							// both point are free, move them from their centers
							float halfDistance = distance * 0.5f;
							PointF pivot = this.Pivot;
							mControlPoint[1].mPoint.X = pivot.X + (vector[0].X * halfDistance);
							mControlPoint[1].mPoint.Y = pivot.Y + (vector[0].Y * halfDistance);
							halfDistance = -halfDistance;
							mControlPoint[0].mPoint.X = pivot.X + (vector[0].X * halfDistance);
							mControlPoint[0].mPoint.Y = pivot.Y + (vector[0].Y * halfDistance);
						}
						// set the orientation (but anyway it will be recomputed in the update function)
						mOrientation = value;
						// after moving one or two control point, update the data
						updateDisplayDataAndMesurementImage();
					}
				}
			}

			/// <summary>
			/// Set or Get the center of this circle
			/// </summary>
			public override PointF Center
			{
				set
				{
					// compute the shifting offset
					PointF shiftOffset = this.Center;
					shiftOffset.X = value.X - shiftOffset.X;
					shiftOffset.Y = value.Y - shiftOffset.Y;
					// and translate accordingly
					translate(shiftOffset);
				}
			}

			/// <summary>
			/// Set or Get the position of this circle
			/// </summary>
			public override PointF Position
			{
				set
				{
					// compute the shifting offset
					PointF shiftOffset = this.Position;
					shiftOffset.X = value.X - shiftOffset.X;
					shiftOffset.Y = value.Y - shiftOffset.Y;
					// and translate accordingly
					translate(shiftOffset);
				}
			}

			/// <summary>
			/// The pivot of the ruler is the middle point between Point1 and Point2
			/// </summary>
			public override PointF Pivot
			{
				get
				{
					return new PointF((mControlPoint[0].mPoint.X + mControlPoint[1].mPoint.X) * 0.5f,
									(mControlPoint[0].mPoint.Y + mControlPoint[1].mPoint.Y) * 0.5f);
				}
				set
				{
					// compute the shifting offset
					PointF shiftOffset = this.Pivot;
					shiftOffset.X = value.X - shiftOffset.X;
					shiftOffset.Y = value.Y - shiftOffset.Y;
					// and translate accordingly
					translate(shiftOffset);
				}
			}
			public override bool IsNotAttached
			{
				get { return ((mControlPoint[0].mAttachedBrick == null) && (mControlPoint[1].mAttachedBrick == null)); }
			}

			public override bool IsFullyAttached
			{
				get { return ((mControlPoint[0].mAttachedBrick != null) && (mControlPoint[1].mAttachedBrick != null)); }
			}

			public override bool IsCurrentControlPointAttached
			{
				get { return (mControlPoint[mCurrentControlPointIndex].mAttachedBrick != null); }
			}

			public override LayerBrick.Brick BrickAttachedToCurrentControlPoint
			{
				get { return mControlPoint[mCurrentControlPointIndex].mAttachedBrick; }
			}			

			public PointF Point1
			{
				get { return mControlPoint[0].mPoint; }
				set
				{
					mControlPoint[0].mPoint = value;
					updateDisplayDataAndMesurementImage();
				}
			}

			public PointF Point2
			{
				get { return mControlPoint[1].mPoint; }
				set
				{
					mControlPoint[1].mPoint = value;
					updateDisplayDataAndMesurementImage();
				}
			}

			public override PointF CurrentControlPoint
			{
				get { return mControlPoint[mCurrentControlPointIndex].mPoint; }
				set
				{
					mControlPoint[mCurrentControlPointIndex].mPoint = value;
					updateDisplayDataAndMesurementImage();
				}
			}

			public override int CurrentControlPointIndex
			{
				get { return mCurrentControlPointIndex; }
				set { mCurrentControlPointIndex = value; }
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

			public bool AllowOffset
			{
				get { return mAllowOffset; }
				set { mAllowOffset = value; }
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
			/// Copy constructor
			/// </summary>
			public LinearRuler(LinearRuler model)
				: base(model)
			{
				// clone the control point but detach the brick because we don't want to clone the link
				mControlPoint[0] = model.mControlPoint[0];
				mControlPoint[0].mAttachedBrick = null;
				mControlPoint[1] = model.mControlPoint[1];
				mControlPoint[1].mAttachedBrick = null;				
				mOffsetDistance = model.OffsetDistance;
				mAllowOffset = model.mAllowOffset;
				// the unit vector will be computed in the update method
				mCurrentControlPointIndex = model.mCurrentControlPointIndex;
				updateDisplayDataAndMesurementImage();
			}

			/// <summary>
			/// Constructor used for the construction of a linear ruler with the mouse
			/// </summary>
			/// <param name="point1">First point of the line</param>
			/// <param name="point2">Second point of the line</param>
			public LinearRuler(PointF point1, PointF point2)
				: base()
			{
				mControlPoint[0].mPoint = point1;
				mControlPoint[1].mPoint = point2;
				updateGeometryData();
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
				mControlPoint[0].mPoint = point1;
				mControlPoint[1].mPoint = point2;
				mOffsetDistance = offsetDistance;
				updateDisplayDataAndMesurementImage();
			}

			/// <summary>
			/// Clone this LinearRuler
			/// </summary>
			/// <returns>a new LinearRuler which is a conform copy of this</returns>
			public override LayerItem Clone()
			{
				// Just call the copy constructor
				return new LinearRuler(this);
			}

			/// <summary>
			/// This function update geometry date of the ruler (distance and orientation)
			/// </summary>
			protected override void updateGeometryData()
			{
				// compute the vector of the orientation such as the orientation will stay upside up
				PointF point1 = mControlPoint[0].mPoint;
				PointF point2 = mControlPoint[1].mPoint;
				float directorVectorX = Math.Abs(point1.X - point2.X);
				float directorVectorY = (point2.X > point1.X) ? (point2.Y - point1.Y) : (point1.Y - point2.Y);

				// compute the orientation angle
				mOrientation = (float)((Math.Atan2(directorVectorY, directorVectorX) * 180.0) / Math.PI);

				// also compute the distance between the two points
				float distance = (float)Math.Sqrt((directorVectorX * directorVectorX) + (directorVectorY * directorVectorY));

				// compute the unit vector (if the distance is not null)
				if (distance > 0.0f)
					mUnitVector = new PointF(directorVectorX / distance, directorVectorY / distance);
				else
					mUnitVector = new PointF(1.0f, 0.0f);

				// set the distance in the data member
				mMeasuredDistance.DistanceInStud = distance;
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
				// get the geometry data in local variable
				PointF point1 = mControlPoint[0].mPoint;
				PointF point2 = mControlPoint[1].mPoint;
				float distance = mMeasuredDistance.DistanceInStud;

				// compute the vector of the offset. This vector is turned by 90 deg from the Orientation, so
				// just invert the X and Y of the normalized vector (the offset vector can be null)
				PointF offsetNormalizedVector = new PointF(mUnitVector.Y, -mUnitVector.X);

				// compute the offset distance depending on the flag
				float offsetDistance = mAllowOffset ? mOffsetDistance : 0.0f;

				// compute the offset coordinates in stud
				float offsetX = offsetNormalizedVector.X * offsetDistance;
				float offsetY = offsetNormalizedVector.Y * offsetDistance;
				PointF originalOffsetPoint1 = new PointF(point1.X + offsetX, point1.Y + offsetY);
				PointF originalOffsetPoint2 = new PointF(point2.X + offsetX, point2.Y + offsetY);
				PointF offsetPoint1 = originalOffsetPoint1;
				PointF offsetPoint2 = originalOffsetPoint2;

				// special case: if the distance is too small to draw the unit, we widden the offest point
				bool needToEnlargeWithText = mDisplayDistance && (distance < GUARANTIED_SPACE_FOR_DISTANCE_DRAWING_IN_STUD);
				bool needToEnlargeWithoutText = !mDisplayDistance && (distance < MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD);
				if (needToEnlargeWithText || needToEnlargeWithoutText)
				{
					float enlargeSize = 0.0f;
					if (needToEnlargeWithText)
						enlargeSize = (GUARANTIED_SPACE_FOR_DISTANCE_DRAWING_IN_STUD - distance) * 0.5f;
					else if (needToEnlargeWithoutText)
						enlargeSize = HALF_MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD;
					enlargeSize *= (point2.X > point1.X) ? 1.0f : -1.0f;
					PointF enlargeVector = new PointF(mUnitVector.X * enlargeSize, mUnitVector.Y * enlargeSize);
					offsetPoint1 = new PointF(offsetPoint1.X - enlargeVector.X, offsetPoint1.Y - enlargeVector.Y);
					offsetPoint2 = new PointF(offsetPoint2.X + enlargeVector.X, offsetPoint2.Y + enlargeVector.Y);

					// also enlarge the offset if there's the text
					if (needToEnlargeWithText)
					{
						originalOffsetPoint1 = offsetPoint1;
						originalOffsetPoint2 = offsetPoint2;
					}
				}

				// when the offset points are finished to be computed, assign them in the data members
				mControlPoint[0].mOffsetPoint = originalOffsetPoint1;
				mControlPoint[1].mOffsetPoint = originalOffsetPoint2;

				// extend a little more the offset point to draw a margin
				float extendInStud = MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD;
				if (mDisplayDistance)
					extendInStud = (float)(((double)MesurementTextHeightInPixel * 0.5) / MainForm.Instance.MapViewScale);
				float extendX = offsetNormalizedVector.X * ((mOffsetDistance > 0.0f) ? extendInStud : -extendInStud);
				float extendY = offsetNormalizedVector.Y * ((mOffsetDistance > 0.0f) ? extendInStud : -extendInStud);
				PointF[] selectionArea = new PointF[4];
				selectionArea[(int)SelectionAreaIndex.EXTERNAL_1] = new PointF(offsetPoint1.X + extendX, offsetPoint1.Y + extendY);
				selectionArea[(int)SelectionAreaIndex.EXTERNAL_2] = new PointF(offsetPoint2.X + extendX, offsetPoint2.Y + extendY);
				selectionArea[(int)SelectionAreaIndex.INTERNAL_1] = new PointF(offsetPoint1.X - extendX, offsetPoint1.Y - extendY);
				selectionArea[(int)SelectionAreaIndex.INTERNAL_2] = new PointF(offsetPoint2.X - extendX, offsetPoint2.Y - extendY);
				mSelectionArea = new Tools.Polygon(selectionArea);

				// finally update the display area
				updateDisplayArea();
			}

			/// <summary>
			/// call this function when you need to update the display area
			/// </summary>
			private void updateDisplayArea()
			{
				// compute the 4 corner of the ruler
				PointF[] corners = { mControlPoint[0].mPoint, mControlPoint[1].mPoint, mSelectionArea[0], mSelectionArea[1], mSelectionArea[2], mSelectionArea[3] };

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
			/// Strangely, the selection area for a linear ruler is not fixed, but depends on the
			/// scale of the view when the distance is displayed, because the distance is written with
			/// a fixe pixel size. So it need to be adjusted each time the zooming scale is changing.
			/// Since the zooming scale is given in the draw method, this update methode can be called
			/// from the draw, but also the zooming scale can be asked to the map.
			/// </summary>
			public override void zoomScaleChangeNotification(double oldScaleInPixelPerStud, double newScaleInPixelPerStud)
			{
				// memorize the current text height and call the base function to update the scale
				float previousMesurementTextHeight = MesurementTextHeightInPixel;
				base.zoomScaleChangeNotification(oldScaleInPixelPerStud, newScaleInPixelPerStud);

				// then adjust the selection area if needed
				if (mDisplayDistance)
				{
					// compute the difference
					float oldHeightInStud = (float)(previousMesurementTextHeight * 0.5 / oldScaleInPixelPerStud);
					float newHeightInStud = (float)(MesurementTextHeightInPixel * 0.5 / newScaleInPixelPerStud);
					float extendInStud = newHeightInStud - oldHeightInStud;

					// compute the vector of the offset. This vector is turned by 90 deg from the Orientation, so
					// just invert the X and Y of the normalized vector (the offset vector can be null)
					PointF positiveMoveVector = new PointF(mUnitVector.Y * extendInStud, -mUnitVector.X * extendInStud);
					PointF negativeMoveVector = new PointF(-positiveMoveVector.X, -positiveMoveVector.Y);
					PointF[] moveVector = { negativeMoveVector, positiveMoveVector, positiveMoveVector, negativeMoveVector };

					// resize the selection area
					for (int i = 0; i < 4; ++i)
					{
						PointF point = mSelectionArea[i];
						if (mOffsetDistance > 0.0f)
							mSelectionArea[i] = new PointF(point.X + moveVector[i].X, point.Y + moveVector[i].Y);
						else
							mSelectionArea[i] = new PointF(point.X - moveVector[i].X, point.Y - moveVector[i].Y);
					}

					// update also the display area
					updateDisplayArea();
				}
			}
			#endregion

			#region IXmlSerializable Members
			public override void ReadXml(System.Xml.XmlReader reader)
			{
				base.ReadXml(reader);
				// read the data of the ruler (don't use accessor to avoid multiple call to the update functions
				mControlPoint[0].mPoint = XmlReadWrite.readPointF(reader);
				mControlPoint[1].mPoint = XmlReadWrite.readPointF(reader);
				// read the id of the attached brick (if any)
				mControlPoint[0].mAttachedBrickHashCodeUsedDuringLoading = reader.ReadElementContentAsInt();
				mControlPoint[1].mAttachedBrickHashCodeUsedDuringLoading = reader.ReadElementContentAsInt();
				mOffsetDistance = reader.ReadElementContentAsFloat();
				mAllowOffset = reader.ReadElementContentAsBoolean();
				// read the end element of the ruler
				reader.ReadEndElement();
				// update the computing data after reading the 2 points and offset
				updateDisplayDataAndMesurementImage();
			}

			public override void recreateLinksAfterLoading()
			{
				for (int i = 0; i < 2; ++i)
				{
					// try to find the brick with the id we read
					LayerBrick.Brick brick = Map.sHashtableForRulerAttachementRebuilding[mControlPoint[i].mAttachedBrickHashCodeUsedDuringLoading] as LayerBrick.Brick;
					if (brick != null)
					{
						// compute the attach offset in local coordinate
						PointF attachOffset = RulerAttachementSet.Anchor.sComputeLocalOffsetFromLayerItem(brick, mControlPoint[i].mPoint);
						// create a new Anchor
						RulerAttachementSet.Anchor anchor = new RulerAttachementSet.Anchor(this, i, attachOffset);
						brick.attachRuler(anchor);
					}
				}
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("LinearRuler");
				base.WriteXml(writer);
				// write the data of the linear ruler
				XmlReadWrite.writePointF(writer, "Point1", this.Point1);
				XmlReadWrite.writePointF(writer, "Point2", this.Point2);
				XmlReadWrite.writeItemId(writer, "AttachedBrick1", mControlPoint[0].mAttachedBrick);
				XmlReadWrite.writeItemId(writer, "AttachedBrick2", mControlPoint[1].mAttachedBrick);
				XmlReadWrite.writeFloat(writer, "OffsetDistance", this.OffsetDistance);
				XmlReadWrite.writeBoolean(writer, "AllowOffset", mAllowOffset);
				writer.WriteEndElement(); // end of LinearRuler
			}

            public override void ReadLDraw(string[] line, ref int index, int version)
            {
                base.ReadLDraw(line, ref index, version);
                // read the data of the ruler (don't use accessor to avoid multiple call to the update functions
                mControlPoint[0].mPoint = LDrawReadWrite.readPointF(line[index++]);
                mControlPoint[1].mPoint = LDrawReadWrite.readPointF(line[index++]);
                // read the id of the attached brick (if any)
                mControlPoint[0].mAttachedBrickHashCodeUsedDuringLoading = LDrawReadWrite.readItemId(line[index++]);
                mControlPoint[1].mAttachedBrickHashCodeUsedDuringLoading = LDrawReadWrite.readItemId(line[index++]);
                mAllowOffset = LDrawReadWrite.readBoolean(line[index++]);
                mOffsetDistance = LDrawReadWrite.readFloat(line[index++]);
                // update the computing data after reading the 2 points and offset
                updateDisplayDataAndMesurementImage();
            }

            public override void WriteLDraw(ref string line)
            {
                // write the type first
                line += "LINEAR ";
                // call the base class
                base.WriteLDraw(ref line);
                // write the data of the linear ruler
                LDrawReadWrite.writePointF(ref line, this.Point1);
                LDrawReadWrite.writePointF(ref line, this.Point2);
                LDrawReadWrite.writeItemId(ref line, mControlPoint[0].mAttachedBrick);
                LDrawReadWrite.writeItemId(ref line, mControlPoint[1].mAttachedBrick);
                LDrawReadWrite.writeBoolean(ref line, mAllowOffset);
                LDrawReadWrite.writeFloat(ref line, this.OffsetDistance);
            }            
            #endregion

			#region edition
			/// <summary>
			/// This method translate all the point and offset point of the specified value
			/// except if one point is attached
			/// </summary>
			/// <param name="translation">the value to translate in stud coord</param>
			private void translate(PointF translation)
			{
				// to change the center at least one control point must be free
				if (!this.IsFullyAttached)
				{
					// add the offset to the 2 points if there are not attached
					if (mControlPoint[0].mAttachedBrick == null)
					{
						mControlPoint[0].mPoint.X += translation.X;
						mControlPoint[0].mPoint.Y += translation.Y;
					}
					if (mControlPoint[1].mAttachedBrick == null)
					{
						mControlPoint[1].mPoint.X += translation.X;
						mControlPoint[1].mPoint.Y += translation.Y;
					}
					// if both point are free, shift the two offset point
					if (this.IsNotAttached)
					{
						mControlPoint[0].mOffsetPoint.X += translation.X;
						mControlPoint[0].mOffsetPoint.Y += translation.Y;
						mControlPoint[1].mOffsetPoint.X += translation.X;
						mControlPoint[1].mOffsetPoint.Y += translation.Y;
						// unit vector and offset distance don't changes
						// but we need to translate the selection area and the display area
						translateSelectionArea(translation);
						// then set the new coordinate of the display area
						mDisplayArea.Offset(translation);
					}
					else
					{
						// else we need to recompute the shape
						updateDisplayDataAndMesurementImage();
					}
				}
			}

			/// <summary>
			/// Find the closest control point of the ruler away from the specified point. Memorize it 
			/// and then compute the square distance to it and return it.
			/// The control point for a linear ruler are the two extremities.
			/// </summary>
			/// <param name="pointInStud">the position in stud from which searching the nearest the control points</param>
			/// <returns>the square distance from the specified point to the nearest control point in squared studs</returns>
			public override float findClosestControlPointAndComputeSquareDistance(PointF pointInStud)
			{
				float bestSquareDist = float.MaxValue;
				for (int i = 0; i < mControlPoint.Length; ++i)
				{
					float dx = pointInStud.X - mControlPoint[i].mPoint.X;
					float dy = pointInStud.Y - mControlPoint[i].mPoint.Y;
					float squaredDist = (dx * dx) + (dy * dy);
					if (squaredDist < bestSquareDist)
					{
						bestSquareDist = squaredDist;
						this.mCurrentControlPointIndex = i;
					}
				}
				return bestSquareDist;
			}

			/// <summary>
			/// Tell if the specified point is inside an area of the ruler that can be grabed (handle)
			/// for scaling purpose. For a Linear ruler it is the line itself
			/// </summary>
			/// <param name="pointInStud">the position in stud for which testing the scaling handle</param>
			/// <param name="thicknessInStud">For handles that are just lines, give the thickness in stud from which the function consider the point is above</param>
			/// <returns>true if the specified point is above a scaling handle</returns>
			public override bool isInsideAScalingHandle(PointF pointInStud, float thicknessInStud)
			{
				if (mAllowOffset)
					return (this.SelectionArea.isPointInside(pointInStud));
				return false;
			}

			/// <summary>
			/// Scale this ruler such as the ruler is above the specified point.
			/// Compute the distance in stud between the given point and this ruler, then set the offset
			/// of this ruler with this distance
			/// </summary>
			/// <param name="pointInStud">the point in stud coord on which the ruler should be</param>
			public override void scaleToPoint(PointF pointInStud)
			{
				// get the vector to make a vectorial product with the unit vector
				PointF point1 = this.Point1;
				PointF point1ToSpecifiedPoint = new PointF(pointInStud.X - point1.X, pointInStud.Y - point1.Y);
				// compute the vectorial product (x and y are null cause z is null):
				this.OffsetDistance = (point1ToSpecifiedPoint.X * mUnitVector.Y) - (point1ToSpecifiedPoint.Y * mUnitVector.X);
			}

			/// <summary>
			/// Get a random reference point above the ruler according to the current scale of the ruler
			/// </summary>
			/// <return>a point in stud coordinate which match the current scale of the ruler</return>
			public override PointF getReferencePointForScale()
			{
				return mControlPoint[1].mOffsetPoint;
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

			/// <summary>
			/// Get the control point corresponding to the index number.
			/// If index is zero, this method return this.Point1, if index is 1 it returns this.Point2
			/// </summary>
			/// <param name="index">0 if you want Point1, 1 if you want Point2</param>
			/// <returns>the position of Point1 or Point2</returns>
			public override PointF getControlPointPosition(int index)
			{
				return mControlPoint[index].mPoint;
			}

			/// <summary>
			/// Set the position of the control point corresponding to the index number
			/// If index is zero, this method will move this.Point1, if index is 1 it will set this.Point2
			/// </summary>
			/// <param name="index">0 if you want Point1, 1 if you want Point2</param>
			/// <param name="value">the new position in stud coordinate</param>
			public override void setControlPointPosition(int index, PointF positionInStud)
			{
				mControlPoint[index].mPoint = positionInStud;
				updateDisplayDataAndMesurementImage();
			}

			/// <summary>
			/// Call this function when you want to attach the current control point to the specified brick.
			/// Be sure to choose the correct current point before calling this function.
			/// </summary>
			/// <param name="index">the index of the control point to attach</param>
			/// <param name="brick">the brick to which the current control point will be attached</param>
			public override void attachControlPointToBrick(int index, LayerBrick.Brick brick)
			{
				mControlPoint[index].mAttachedBrick = brick;
			}

			/// <summary>
			/// Call this function when you want to detach the current control point.
			/// Be sure to choose the correct current point before calling this function.
			/// <param name="index">the index of the control point to detach</param>
			/// </summary>
			public override void detachControlPoint(int index)
			{
				mControlPoint[index].mAttachedBrick = null;
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
					bool needToDrawOffset = mAllowOffset && (mOffsetDistance != 0.0f);
					bool needToDisplayHull = Properties.Settings.Default.DisplayOtherHull;
					bool needToDrawArrowForSmallDistance = !mDisplayDistance && (mMeasuredDistance.DistanceInStud < MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD);

					// transform the coordinates into pixel coordinates
					PointF offset1InPixel = Layer.sConvertPointInStudToPixel(mControlPoint[0].mOffsetPoint, areaInStud, scalePixelPerStud);
					PointF offset2InPixel = Layer.sConvertPointInStudToPixel(mControlPoint[1].mOffsetPoint, areaInStud, scalePixelPerStud);

					// point1 and 2 only need to be computed if we draw the offset
					PointF point1InPixel = new PointF();
					PointF point2InPixel = new PointF();
					if (needToDrawOffset)
					{
						point1InPixel = Layer.sConvertPointInStudToPixel(this.Point1, areaInStud, scalePixelPerStud);
						point2InPixel = Layer.sConvertPointInStudToPixel(this.Point2, areaInStud, scalePixelPerStud);
					}

					// internal and external point may be computed only for certain conditions
					PointF offsetInternal1InPixel = new PointF();
					PointF offsetInternal2InPixel = new PointF();
					PointF offsetExternal1InPixel = new PointF();
					PointF offsetExternal2InPixel = new PointF();
					if (isSelected || needToDisplayHull || needToDrawOffset || needToDrawArrowForSmallDistance)
					{
						offsetInternal1InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.INTERNAL_1], areaInStud, scalePixelPerStud);
						offsetInternal2InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.INTERNAL_2], areaInStud, scalePixelPerStud);
						offsetExternal1InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.EXTERNAL_1], areaInStud, scalePixelPerStud);
						offsetExternal2InPixel = Layer.sConvertPointInStudToPixel(mSelectionArea[(int)SelectionAreaIndex.EXTERNAL_2], areaInStud, scalePixelPerStud);
					}

					// create the pen for the lines
					Color colorWithTransparency = Color.FromArgb((int)(layerTransparency * 2.55f), mColor);
					Pen penForLine = new Pen(colorWithTransparency, mLineThickness);

					// create the pen for the offset lines (only if need)
					Pen penForGuideline = null;
					if (needToDrawOffset || needToDrawArrowForSmallDistance)
					{
						Color guidelineColorWithTransparency = Color.FromArgb((int)(layerTransparency * 2.55f), mGuidelineColor);
						penForGuideline = new Pen(guidelineColorWithTransparency, mGuidelineThickness);
						penForGuideline.DashPattern = mGuidelineDashPattern;
					}

					// draw one or 2 lines
					if (!mDisplayDistance)
					{
						// draw one single line
						g.DrawLine(penForLine, offset1InPixel, offset2InPixel);
						// if the line is too small draw arrows around it
						if (needToDrawArrowForSmallDistance)
						{
							// draw the arrow
							g.DrawLine(penForGuideline, offset1InPixel, offsetInternal1InPixel);
							g.DrawLine(penForGuideline, offset1InPixel, offsetExternal1InPixel);
							g.DrawLine(penForGuideline, offset2InPixel, offsetInternal2InPixel);
							g.DrawLine(penForGuideline, offset2InPixel, offsetExternal2InPixel);
						}
						// draw the offset if needed
						if (needToDrawOffset)
						{
							// draw the two offset straight
							g.DrawLine(penForGuideline, point1InPixel, offset1InPixel);
							g.DrawLine(penForGuideline, point2InPixel, offset2InPixel);
						}
					}
					else
					{
						// compute the middle points
						float middleX = (offset1InPixel.X + offset2InPixel.X) * 0.5f;
						float middleY = (offset1InPixel.Y + offset2InPixel.Y) * 0.5f;

						// draw the mesurement image
						drawMesurementImage(g, new PointF(middleX, middleY), scalePixelPerStud, layerImageAttributeWithTransparency);

						// compute the middle extremity of the two lines
						float halfTextLength = (this.MesurementTextWidthInPixel * 0.5f);
						float halfTextLengthX = mUnitVector.X * halfTextLength;
						float halfTextLengthY = mUnitVector.Y * halfTextLength;
						PointF middle1 = new PointF(middleX - halfTextLengthX, middleY - halfTextLengthY);
						PointF middle2 = new PointF(middleX + halfTextLengthX, middleY + halfTextLengthY);

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

						// draw the offset if needed
						if (needToDrawOffset)
						{
							if (mMeasuredDistance.DistanceInStud < GUARANTIED_SPACE_FOR_DISTANCE_DRAWING_IN_STUD)
							{
								// draw the two offsets in 2 parts
								g.DrawLine(penForGuideline, point1InPixel, offsetInternal1InPixel);
								g.DrawLine(penForGuideline, offsetInternal1InPixel, offsetExternal1InPixel);
								g.DrawLine(penForGuideline, point2InPixel, offsetInternal2InPixel);
								g.DrawLine(penForGuideline, offsetInternal2InPixel, offsetExternal2InPixel);
							}
							else
							{
								// draw the two offset straight
								g.DrawLine(penForGuideline, point1InPixel, offsetExternal1InPixel);
								g.DrawLine(penForGuideline, point2InPixel, offsetExternal2InPixel);
							}
						}
					}

					// compute the selection area if it is selected or if we need to draw the hull
					PointF[] selectionArea = null;
					if (isSelected || needToDisplayHull)
						selectionArea = new PointF[] { offsetInternal1InPixel, offsetExternal1InPixel, offsetExternal2InPixel, offsetInternal2InPixel };

					// draw the hull if needed
					if (needToDisplayHull)
						g.DrawPolygon(sPenToDrawOtherHull, selectionArea);

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
					drawOneControlPoint(g, areaInStud, scalePixelPerStud, color, this.Point1, mControlPoint[0].mAttachedBrick != null);
					drawOneControlPoint(g, areaInStud, scalePixelPerStud, color, this.Point2, mControlPoint[1].mAttachedBrick != null);
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
			private LayerBrick.Brick mAttachedBrick = null;
			private int mAttachedBrickHashCodeUsedDuringLoading = 0;

			#region get/set
			public override float Orientation
			{
				set
				{
					mOrientation = value;
					if (mOrientation < -90.0f)
						mOrientation += 180.0f;
					if (mOrientation > 90.0f)
						mOrientation -= 180.0f;
					this.updateMesurementImage();
				}
			}

			/// <summary>
			/// Set or Get the center of this circle. The set will have no effect if the control point is attached.
			/// </summary>
			public override PointF Center
			{
				get { return mSelectionArea[0]; }
				set
				{
					// only move if it is not attached
					if (!this.IsFullyAttached)
						base.Center = value;
				}
			}

			public override bool IsNotAttached
			{
				get { return (mAttachedBrick == null); }
			}

			public override bool IsFullyAttached
			{
				get { return (mAttachedBrick != null); }
			}

			public override bool IsCurrentControlPointAttached
			{
				get { return (mAttachedBrick != null); }
			}

			public override LayerBrick.Brick BrickAttachedToCurrentControlPoint
			{
				get { return mAttachedBrick; }
			}			

			/// <summary>
			/// The control point of a circle is always its center
			/// </summary>
			public override PointF CurrentControlPoint
			{
				get { return this.Center; }
				set
				{
					// call the base class cause this check if the center is not attached
					base.Center = value;
				}
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

			/// <summary>
			/// Copy constructor
			/// </summary>
			public CircularRuler(CircularRuler model)
				: base(model)
			{
				// do not copy the link of the attached brick to the clone
				// the radius and center is computed in the base class (LayerItem copy constructor)
				// the rest of the geometry will be computed in the update
				updateDisplayDataAndMesurementImage();
			}


			public CircularRuler(PointF center, float radius) : base()
			{
				// define the selection area
				mSelectionArea = new Tools.Circle(center, radius);
				// update the display area
				updateDisplayDataAndMesurementImage();
			}

			/// <summary>
			/// Clone this CircularRuler
			/// </summary>
			/// <returns>a new CircularRuler which is a conform copy of this</returns>
			public override LayerItem Clone()
			{
				return new CircularRuler(this);
			}

			/// <summary>
			/// This function update geometry date of the ruler (distance and orientation)
			/// </summary>
			protected override void updateGeometryData()
			{
				// set the distance in the data member
				mMeasuredDistance.DistanceInStud = (this.Radius * 2.0f);
			}

			/// <summary>
			/// Update the display area of this layer item from the center point and the radius
			/// </summary>
			protected override void updateDisplayData()
			{
				// get the center and radius
				PointF center = this.Center;
				float radius = Math.Max(this.Radius, HALF_MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD);
				// compute the display area
				mDisplayArea.X = center.X - radius;
				mDisplayArea.Y = center.Y - radius;
				float diameter = radius * 2.0f;
				mDisplayArea.Width = diameter;
				mDisplayArea.Height = diameter;
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
				// read the id of the attached brick (if any)
				mAttachedBrickHashCodeUsedDuringLoading = XmlReadWrite.readItemId(reader);
				// read the end element of the ruler
				reader.ReadEndElement();
				// don't need to update the display area after reading the data values, because the accessor of Radius did it
			}

			public override void recreateLinksAfterLoading()
			{
				// try to find the brick with the id we read
				LayerBrick.Brick brick = Map.sHashtableForRulerAttachementRebuilding[mAttachedBrickHashCodeUsedDuringLoading] as LayerBrick.Brick;
				if (brick != null)
				{
					// compute the attach offset in local coordinate
					PointF attachOffset = RulerAttachementSet.Anchor.sComputeLocalOffsetFromLayerItem(brick, this.Center);
					// create a new Anchor
					RulerAttachementSet.Anchor anchor = new RulerAttachementSet.Anchor(this, 0, attachOffset);
					brick.attachRuler(anchor);
				}
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("CircularRuler");
				base.WriteXml(writer);
				// write ruler data
				XmlReadWrite.writePointF(writer, "Center", this.Center);
				XmlReadWrite.writeFloat(writer, "Radius", this.Radius);
				XmlReadWrite.writeItemId(writer, "AttachedBrick", mAttachedBrick);
				writer.WriteEndElement(); // end of CircularRuler
			}

            public override void ReadLDraw(string[] line, ref int index, int version)
            {
                base.ReadLDraw(line, ref index, version);
                // read data of the ruler (don't use this.Center because at that time the object is out of synch
                // the display area may have been read but not center yet
                mSelectionArea[0] = LDrawReadWrite.readPointF(line[index++]);
                this.Radius = LDrawReadWrite.readFloat(line[index++]);
                // read the id of the attached brick (if any)
                mAttachedBrickHashCodeUsedDuringLoading = LDrawReadWrite.readItemId(line[index++]);
                // don't need to update the display area after reading the data values, because the accessor of Radius did it
            }

            public override void WriteLDraw(ref string line)
            {
                // write the type first
                line += "CIRCULAR ";
                // call the base class
                base.WriteLDraw(ref line);
                // write the data of the linear ruler
                LDrawReadWrite.writePointF(ref line, this.Center);
                LDrawReadWrite.writeFloat(ref line, this.Radius);
                LDrawReadWrite.writeItemId(ref line, mAttachedBrick);
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
				// but use the minimum radius, in case the radius is too small
				float radius = Math.Max(this.Radius, HALF_MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD);
				return ((float)Math.Abs(radius - distance) <= thicknessInStud);
			}

			/// <summary>
			/// Scale this ruler such as the ruler is above the specified point.
			/// The specified point belongs to this circle. The radius is computed with the current 
			/// center, then the circle geometry is updated.
			/// </summary>
			/// <param name="pointInStud">the point in stud coord on which the ruler should be</param>
			public override void scaleToPoint(PointF pointInStud)
			{
				// compute the new radius
				PointF center = mSelectionArea[0];
				PointF radiusVector = new PointF(pointInStud.X - center.X, pointInStud.Y - center.Y);
				// set the radius by calling the accessor to trigger the necessary update
				Radius = (float)Math.Sqrt((radiusVector.X * radiusVector.X) + (radiusVector.Y * radiusVector.Y));
			}

			/// <summary>
			/// Get a random reference point above the ruler according to the current scale of the ruler
			/// </summary>
			/// <return>a point in stud coordinate which match the current scale of the ruler</return>
			public override PointF getReferencePointForScale()
			{
				return new PointF(mSelectionArea[0].X + Radius, mSelectionArea[0].Y);
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

			/// <summary>
			/// There's only one control point for a circular ruler, so the index is always ignored
			/// </summary>
			/// <param name="index">useless parameter</param>
			/// <returns>the center of the circular ruler</returns>
			public override PointF getControlPointPosition(int index)
			{
				return this.CurrentControlPoint;
			}

			/// <summary>
			/// There's only one control point for a circular ruler, so the index is always ignored
			/// </summary>
			/// <param name="index">useless parameter</param>
			/// <param name="positionInStud">the new value for the center of the circular ruler</param>
			public override void setControlPointPosition(int index, PointF positionInStud)
			{
				this.CurrentControlPoint = positionInStud;
			}

			/// <summary>
			/// Call this function when you want to attach the current control point to the specified brick.
			/// Be sure to choose the correct current point before calling this function
			/// </summary>
			/// <param name="index">useless for circular ruler</param>
			/// <param name="brick">the brick to which the current control point will be attached</param>
			public override void attachControlPointToBrick(int index, LayerBrick.Brick brick)
			{
				// the circular ruler only have one attach
				mAttachedBrick = brick;
			}

			/// <summary>
			/// Call this function when you want to detach the current control point.
			/// Be sure to choose the correct current point before calling this function.
			/// <param name="index">useless parameter for a circular ruler</param>
			/// </summary>
			public override void detachControlPoint(int index)
			{
				mAttachedBrick = null;
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

					// convert the center and radius in pixel
					float radiusInPixel = Math.Max((float)(this.Radius * scalePixelPerStud), 0.5f);
					float diameterInPixel = radiusInPixel * 2.0f;
					PointF centerInPixel = Layer.sConvertPointInStudToPixel(this.Center, areaInStud, scalePixelPerStud);
					PointF upperLeftInPixel = new PointF(centerInPixel.X - radiusInPixel, centerInPixel.Y - radiusInPixel);

					// draw the circle
					g.DrawEllipse(penForCircle, upperLeftInPixel.X, upperLeftInPixel.Y, diameterInPixel, diameterInPixel);
					// also draw a fake circle if the circle is too small
					float fakeDiameterInPixel = (float)(MINIMUM_SIZE_FOR_DRAWING_HELPER_IN_STUD * scalePixelPerStud);
					if (diameterInPixel < fakeDiameterInPixel)
					{
						Color guidelineColorWithTransparency = Color.FromArgb((int)(layerTransparency * 2.55f), mGuidelineColor);
						Pen penForFakeCircle = new Pen(guidelineColorWithTransparency, mGuidelineThickness);
						penForFakeCircle.DashPattern = mGuidelineDashPattern;
						float fakeRadius = fakeDiameterInPixel * 0.5f;
						PointF fakeUpperLeftInPixel = new PointF(centerInPixel.X - fakeRadius, centerInPixel.Y - fakeRadius);
						g.DrawEllipse(penForFakeCircle, fakeUpperLeftInPixel.X, fakeUpperLeftInPixel.Y, fakeDiameterInPixel, fakeDiameterInPixel);					
					}

					// draw the image containing the text
					if (mDisplayDistance)
						drawMesurementImage(g, centerInPixel, scalePixelPerStud, layerImageAttributeWithTransparency);

					// draw the hull if needed
                    if (Properties.Settings.Default.DisplayOtherHull)
						g.DrawEllipse(sPenToDrawOtherHull, upperLeftInPixel.X, upperLeftInPixel.Y, diameterInPixel, diameterInPixel);

					// draw the selection overlay
					if (isSelected)
						g.FillEllipse(selectionBrush, upperLeftInPixel.X, upperLeftInPixel.Y, diameterInPixel, diameterInPixel);
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
					drawOneControlPoint(g, areaInStud, scalePixelPerStud, color, this.Center, this.IsFullyAttached);
				}
			}
			#endregion
		}
	}
}
