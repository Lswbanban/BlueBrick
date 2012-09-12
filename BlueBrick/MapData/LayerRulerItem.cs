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
			public abstract void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud);
		}
		
		/// <summary>
		/// A Ruler is a specific RulerItem which is actually represented by a line on which the length of the line is displayed
		/// </summary>
		[Serializable]
		public class Ruler : RulerItem
		{
			private PointF mPoint1 = new PointF(); // coord of the first point in Stud coord
			private PointF mPoint2 = new PointF(); // coord of the second point in Stud coord
			private Pen mPen = new Pen(Color.Black); // the pen to draw the line

			#region get/set
			public PointF Point1
			{
				get { return mPoint1; }
				set
				{
					mPoint1 = value;
					computeDisplayArea();
				}
			}

			public PointF Point2
			{
				get { return mPoint2; }
				set
				{
					mPoint2 = value;
					computeDisplayArea();
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
				mPoint1 = point1;
				mPoint2 = point2;
				computeDisplayArea();
			}

			/// <summary>
			/// Compute and update the display area from the two points of the ruler.
			/// This method should be called when the two points of the ruler is updated
			/// or when the ruler is moved.
			/// </summary>
			private void computeDisplayArea()
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
				mDisplayArea = new RectangleF(minX, minY, maxX - minX, maxY - minY);
			}
			#endregion

			#region draw
			/// <summary>
			/// Draw the ruler.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
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

					// draw the line
					g.DrawLine(mPen, x1, y1, x2, y2);
				}
			}
			#endregion
		}
	}
}
