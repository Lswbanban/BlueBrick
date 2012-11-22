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
		/// Tells if the given point is inside this area, assuming the point and the area are
		/// in the same coordinate system
		/// </summary>
		/// <param name="point">the point coordinate to test</param>
		/// <returns>true if the point is inside this area</returns>
		public abstract bool isPointInside(PointF point);

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
					if (((vertex1.Y < point.Y) && (point.Y < vertex2.Y)) ||
						((vertex2.Y < point.Y) && (point.Y < vertex1.Y)))
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
