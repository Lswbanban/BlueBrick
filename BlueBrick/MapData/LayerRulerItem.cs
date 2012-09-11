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
				set { mPoint1 = value; }
			}

			public PointF Point2
			{
				get { return mPoint2; }
				set { mPoint2 = value; }
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
			}
			#endregion

			#region draw
			/// <summary>
			/// Draw the ruler.
			/// </summary>
			/// <param name="g">the graphic context in which draw the layer</param>
			public override void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
			{
				// get the 2 coord in 4 vars
				float x1 = mPoint1.X;
				float y1 = mPoint1.Y;
				float x2 = mPoint2.X;
				float y2 = mPoint2.Y;
				// now get min and max X and Y
				float minX = x1;
				float maxX = x2;
				if (minX > maxX)
				{
					minX = x2;
					maxX = x1;
				}
				float minY = y1;
				float maxY = y2;
				if (minY > maxY)
				{
					minY = y2;
					maxY = y1;
				}
				if ((maxX >= areaInStud.Left) && (minX <= areaInStud.Right) && (maxY >= areaInStud.Top) && (minY <= areaInStud.Bottom))
				{
					// transform the coordinates into pixel coordinates
					x1 = (float)((x1 - areaInStud.Left) * scalePixelPerStud);
					y1 = (float)((y1 - areaInStud.Top) * scalePixelPerStud);
					x2 = (float)((x2 - areaInStud.Left) * scalePixelPerStud);
					y2 = (float)((y2 - areaInStud.Top) * scalePixelPerStud);

					// draw the line
					g.DrawLine(mPen, x1, y1, x2, y2);
				}
			}
			#endregion
		}
	}
}
