using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BlueBrick.MapData.Tools
{
	/// <summary>
	/// This is a base and abstract class for an area and
	/// that can be implemented by a circle, a rectangle or a polygon
	/// </summary>
	public abstract class Surface
	{
		#region get / set
		/// <summary>
		/// get/set the geometrical point describing this surface for the specified index
		/// </summary>
		/// <param name="index">the index defining which point you want</param>
		/// <returns>the indexed point</returns>
		public abstract PointF this[int i]
		{
			get;
			set;
		}

		/// <summary>
		/// get or set an array of vertices describing this surface
		/// </summary>
		public abstract PointF[] Vertice
		{
			get;
			set;
		}
		#endregion

		#region functions
		/// <summary>
		/// A abstract clone method because the Surface class is abstract and cannot be instanciated
		/// </summary>
		/// <returns>a conform copy of this instance of Surface</returns>
		public abstract Surface Clone();

		/// <summary>
		/// Tells if the given point is inside this surface, assuming the point and the surface are
		/// in the same coordinate system.
		/// </summary>
		/// <param name="point">the point coordinate to test</param>
		/// <returns>true if the point is inside this surface</returns>
		public abstract bool isPointInside(PointF point);

		/// <summary>
		/// Tells if the specified axis aligned rectangle is intersecting or overlapping this surface,
		/// assuming the rectangle and the surface are in the same coordinate system.
		/// </summary>
		/// <param name="rectangle">the rectangle to test</param>
		/// <returns>true if the rectangle intersects or overlaps this surface</returns>
		public abstract bool isRectangleIntersect(RectangleF rectangle);

		/// <summary>
		/// Translate this surface from the given vector
		/// </summary>
		/// <param name="translationVector">a vector describing the translation</param>
		public abstract void translate(PointF translationVector);
		#endregion
	}

	/// <summary>
	/// This class represent a circle area
	/// </summary>
	public class Circle : Surface
	{
		private PointF mCenter = new PointF();
		private float mRadius = 0.0f;

		#region get / set
		public override PointF this[int i]
		{
			get
			{
				// we don't care of the index, always return the center
				return mCenter;
			}
			set
			{
				// we don't care of the index, always set the center
				mCenter = value;
			}
		}

		/// <summary>
		/// get or set an array of vertices describing this surface
		/// </summary>
		public override PointF[] Vertice
		{
			get { return new PointF[] { mCenter }; }
			set { mCenter = value[0]; }
		}

		/// <summary>
		/// get or set the center of the circle
		/// </summary>
		public PointF Center
		{
			get { return mCenter; }
			set { mCenter = value; }
		}

		/// <summary>
		/// get or set the radius of the circle
		/// </summary>
		public float Radius
		{
			get { return mRadius; }
			set { mRadius = value; }
		}
		#endregion
		
		#region constructor
		public Circle(PointF center, float radius)
		{
			mCenter = center;
			mRadius = radius;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="model">the model from which copy</param>
		public Circle(Circle model)
		{
			this.mCenter = model.Center;
			this.mRadius = model.Radius;
		}

		/// <summary>
		/// Create a copy instance of this instance
		/// </summary>
		/// <returns>a conform copy of this instance of Surface</returns>
		public override Surface Clone()
		{
			// call the copy constructor
			return new Circle(this);
		}
		#endregion

		#region function
		/// <summary>
		/// Tells if the given point is inside this circle, assuming the point and the surface are
		/// in the same coordinate system.
		/// </summary>
		/// <param name="point">the point coordinate to test</param>
		/// <returns>true if the point is inside this circle</returns>
		public override bool isPointInside(PointF point)
		{
			// compute the distance between the point and the center
			float dx = mCenter.X - point.X;
			float dy = mCenter.Y - point.Y;
			// true if the distance is lower than the radius
			float distance = (float)Math.Sqrt((dx * dx) + (dy * dy));
			return (distance < mRadius);
		}

		/// <summary>
		/// Tells if the specified axis aligned rectangle is intersecting or overlapping this surface,
		/// assuming the rectangle and the surface are in the same coordinate system.
		/// </summary>
		/// <param name="rectangle">the rectangle to test</param>
		/// <returns>true if the rectangle intersects or overlaps this surface</returns>
		public override bool isRectangleIntersect(RectangleF rectangle)
		{
			// check the angles first
			bool isOnLeft = (mCenter.X < rectangle.Left);
			bool isOnRight = (mCenter.X > rectangle.Right);
			bool isOnTop = (mCenter.Y < rectangle.Top);
			bool isOnBottom = (mCenter.Y > rectangle.Bottom);
			if (isOnLeft && isOnTop)
			{
				return (isPointInside(new PointF(rectangle.Left, rectangle.Top)));
			}
			else if (isOnRight && isOnTop)
			{
				return (isPointInside(new PointF(rectangle.Right, rectangle.Top)));
			}
			else if (isOnRight && isOnBottom)
			{
				return (isPointInside(new PointF(rectangle.Right, rectangle.Bottom)));
			}
			else if (isOnLeft && isOnBottom)
			{
				return (isPointInside(new PointF(rectangle.Left, rectangle.Bottom))) ;
			}
			else
			{
				// compute an bigger rectangle increased on all border by the radius of the circle
				float diameter = mRadius * 2.0f;
				RectangleF bigRectangle = new RectangleF(rectangle.X - mRadius, rectangle.Y - mRadius,
											rectangle.Width + diameter, rectangle.Height + diameter);
				// then check if the center is inside the big rectangle
				return ((mCenter.X > bigRectangle.Left) && (mCenter.X < bigRectangle.Right) &&
						(mCenter.Y > bigRectangle.Top) && (mCenter.Y < bigRectangle.Bottom));
			}
		}

		/// <summary>
		/// Translate this surface from the given vector
		/// </summary>
		/// <param name="translationVector">a vector describing the translation</param>
		public override void translate(PointF translationVector)
		{
			mCenter.X += translationVector.X;
			mCenter.Y += translationVector.Y;
		}
		#endregion
	}

	/// <summary>
	/// This class represent a random polygon
	/// </summary>
	public class Polygon : Surface
	{
		private PointF[] mVertice = null;

		#region get/set
		/// <summary>
		/// index operator
		/// </summary>
		/// <param name="i">the index for the wanted vertex</param>
		/// <returns>the ith vertex of the polygon</returns>
		public override PointF this[int i]
		{
			get
			{
				return mVertice[i];
			}
			set
			{
				mVertice[i] = value;
			}
		}

		/// <summary>
		/// get or set an array of vertices describing this surface
		/// </summary>
		public override PointF[] Vertice
		{
			get { return mVertice; }
			set { mVertice = value; }
		}
		#endregion

		#region constructor
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="vertice">initial vertices describing the polygon</param>
		public Polygon(PointF[] vertice)
		{
			mVertice = vertice;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="model">the model from which copy</param>
		public Polygon(Polygon model)
		{
			this.mVertice = model.Vertice;
		}

		/// <summary>
		/// Create a copy instance of this instance
		/// </summary>
		/// <returns>a conform copy of this instance of Surface</returns>
		public override Surface Clone()
		{
			// call the copy constructor
			return new Polygon(this);
		}
		#endregion

		#region function
		/// <summary>
		/// A tool function to check if the specified point is inside this polygon.
		/// Both the point to test and the polygon points definition must be in the same coordinate
		/// system for the function to give sensible result.
		/// This function is mainly used for picking an item by checking if the mouse click in inside the
		/// selection area (hull) of the item. This function use the Ray casting algorythm, by casting the ray
		/// horizontally from the the point to test.
		/// </summary>
		/// <param name="point">the point to test</param>
		/// <returns>true if the specified point is inside the specified polygon</returns>
		public override bool isPointInside(PointF point)
		{
			try
			{
				int segmentCrossCount = 0;
				// we assume that the polygon is not degenerated, otherwise the catch will return false
				// start with the last point of the polygon, such as we can close the polygon
				// i.e the first iteration of the loop will test the segment between the last vertex and the first one
				PointF vertex1 = mVertice[mVertice.Length - 1];
				for (int i = 0; i < mVertice.Length; ++i)
				{
					PointF vertex2 = mVertice[i];

					// check if vertex 1 and 2 are above and under the point to have a crossing
                    // use <= on point2 in order to enter in the condition when the mouse point is exactly
                    // on the same Y as one vertex, but do not use it on point1, otherwise we will also enter
                    // in the condition during the next iteration, which will make the segment cross count wrong
					if (((vertex1.Y < point.Y) && (point.Y <= vertex2.Y)) ||
						((vertex2.Y <= point.Y) && (point.Y < vertex1.Y)))
					{
						// check if vertex 1 is on the right or left of the point
						if (point.X < vertex1.X)
						{
							// check if vertex 2 is on the right or left of the point
							if (point.X < vertex2.X)
							{
								// if the two vertice are both on the right, we have a crossing
								segmentCrossCount++;
							}
							else
							{
								// vertex 1 on the right and 2 on the left, we need to compute the crossing
								// compute the slope of the segment (from left to right)
								float slope = (vertex1.X - vertex2.X) / (vertex1.Y - vertex2.Y);
								// then compute the X of the intersection point for Y=point.Y
								// with the line equation f(Y)= slope * X + cst
								float intersectionX = (slope * (point.Y - vertex2.Y)) + vertex2.X;
								// then we have a crossing if the intersection is on the right of the point
								if (point.X < intersectionX)
									segmentCrossCount++;
							}
						}
						else
						{
							// check if vertex 2 is on the right or left of the point
							if (point.X < vertex2.X)
							{
								// vertex 1 on the left and 2 on the right, we need to compute the crossing
								// compute the slope of the segment (from left to right)
								float slope = (vertex2.X - vertex1.X) / (vertex2.Y - vertex1.Y);
								// then compute the X of the intersection point for Y=point.Y
								// with the line equation f(Y)= slope * X + cst
								float intersectionX = (slope * (point.Y - vertex1.Y)) + vertex1.X;
								// then we have a crossing if the intersection is on the right of the point
								if (point.X < intersectionX)
									segmentCrossCount++;
							}
							// else: if both vertice on the left, we don't count the crossing
						}
					}
                    else if (((point.Y == vertex1.Y) && (point.Y == vertex2.Y)) &&
                            ((point.X < vertex1.X) || (point.X < vertex2.X)))
                    {
                        // in this case the point is on the left and on the same line as an perfectly horizontal edge of the polygon
                        segmentCrossCount++;
                    }
					// else: if both vertice above or both under, there is no crossing

					// move the second vertex into the first one for the next iteration
					vertex1 = vertex2;
				}

				// the point is inside the polygon if we have an odd number of crossing
				return ((segmentCrossCount % 2) == 1);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Tells if the specified axis aligned rectangle is intersecting or overlapping this surface,
		/// assuming the rectangle and the surface are in the same coordinate system.
		/// </summary>
		/// <param name="rectangle">the rectangle to test</param>
		/// <returns>true if the rectangle intersects or overlaps this surface</returns>
		public override bool isRectangleIntersect(RectangleF rectangle)
		{
			// first: check if any of the vertex is inside the rectangle
			foreach (PointF vertex in mVertice)
				if ((vertex.X > rectangle.Left) && (vertex.X < rectangle.Right) &&
					(vertex.Y > rectangle.Top) && (vertex.Y < rectangle.Bottom))
					return true;
			// get the 4 corners of the rectangle
			PointF corner1 = new PointF(rectangle.Left, rectangle.Top);
			PointF corner2 = new PointF(rectangle.Right, rectangle.Top);
			PointF corner3 = new PointF(rectangle.Right, rectangle.Bottom);
			PointF corner4 = new PointF(rectangle.Left, rectangle.Bottom);
			// second check if any of the rectangle corner is inside the polygon
			if (isPointInside(corner1) || isPointInside(corner2) || isPointInside(corner3) || isPointInside(corner4))
				return true;
			// third: finally check if one line of the polygon is crossing one line of the rectangle
			for (int i = 1; i < mVertice.Length; i++)
			{
				if (areLinesCrossing(corner1, corner2, mVertice[i - 1], mVertice[i]))
					return true;
				if (areLinesCrossing(corner2, corner3, mVertice[i - 1], mVertice[i]))
					return true;
				if (areLinesCrossing(corner3, corner4, mVertice[i - 1], mVertice[i]))
					return true;
				if (areLinesCrossing(corner4, corner1, mVertice[i - 1], mVertice[i]))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Check if the two segments defined by their two extremities are crossing
		/// </summary>
		/// <param name="line1Point1">first extremity of the first segment</param>
		/// <param name="line1Point2">second extremity of the first segment</param>
		/// <param name="line2Point1">first extremity of the second segment</param>
		/// <param name="line2Point2">second extremity of the second segment</param>
		/// <returns>true if the two segments are crossing</returns>
		private bool areLinesCrossing(PointF line1Point1, PointF line1Point2, PointF line2Point1, PointF line2Point2)
		{
			// compute the two vectors between the two first points and the two second points
			PointF vector1 = new PointF(line1Point2.X - line1Point1.X, line1Point2.Y - line1Point1.Y);
			PointF vector2 = new PointF(line2Point2.X - line2Point1.X, line2Point2.Y - line2Point1.Y);
			// compute a common divider and check if it's not null
			float divider = (-vector2.X * vector1.Y) + (vector1.X * vector2.Y);
			if (divider != 0.0f)
			{
				PointF vector3 = new PointF(line1Point1.X - line2Point1.X, line1Point1.Y - line2Point1.Y);
				float s = ((-vector1.Y * vector3.X) + (vector1.X * vector3.Y)) / divider;
				float t = ((vector2.X * vector3.Y) - (vector2.Y * vector3.X)) / divider;
				return (s >= 0.0f && s <= 1.0f && t >= 0.0f && t <= 1.0f);
			}
			return false;
		}

		/// <summary>
		/// Translate this surface from the given vector
		/// </summary>
		/// <param name="translationVector">a vector describing the translation</param>
		public override void translate(PointF translationVector)
		{
			Matrix translation = new Matrix();
			translation.Translate(translationVector.X, translationVector.Y);
			translation.TransformPoints(mVertice);
		}
		#endregion
	}
}
