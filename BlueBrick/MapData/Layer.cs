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
using System.Windows.Forms;
using System.Xml.Serialization;
using BlueBrick.Actions;

namespace BlueBrick.MapData
{
	/// <summary>
	/// A layer is a set of element that are displayed together at a certain height in the <see cref="Map"/>.
	/// </summary>
	[Serializable]
	public abstract class Layer : IXmlSerializable
	{
		/// <summary>
		/// This class is a base class for all the item you can find in the
		/// different layers. This class should be derivated by specific item
		/// for the specific layers
		/// </summary>
		[Serializable]
		public class LayerItem : IXmlSerializable
		{
			public RectangleF mDisplayArea = new RectangleF(); // in stud coordinate

			#region get/set
			/// <summary>
			///	Set the position in stud coord. The position of a brick is its top left corner.
			/// </summary>
			public PointF Position
			{
				set
				{
					mDisplayArea.X = value.X;
					mDisplayArea.Y = value.Y;
				}
				get { return new PointF(mDisplayArea.X, mDisplayArea.Y); }
			}

			/// <summary>
			/// Set the position via the center of the object in stud coord.
			/// </summary>
			public PointF Center
			{
				get
				{
					return new PointF(mDisplayArea.X + (mDisplayArea.Width / 2), mDisplayArea.Y + (mDisplayArea.Height / 2));
				}

				set
				{
					mDisplayArea.X = value.X - (mDisplayArea.Width / 2);
					mDisplayArea.Y = value.Y - (mDisplayArea.Height / 2);
				}
			}

			/// <summary>
			/// ge the width of this item in stud
			/// </summary>
			public float Width
			{
				get { return mDisplayArea.Width; }
			}

			/// <summary>
			/// ge the height of this item in stud
			/// </summary>
			public float Height
			{
				get { return mDisplayArea.Height; }
			}

			#endregion

			#region IXmlSerializable Members

			public System.Xml.Schema.XmlSchema GetSchema()
			{
				return null;
			}

			public virtual void ReadXml(System.Xml.XmlReader reader)
			{
				mDisplayArea = XmlReadWrite.readRectangleF(reader);
			}

			public virtual void WriteXml(System.Xml.XmlWriter writer)
			{
				XmlReadWrite.writeRectangleF(writer, "DisplayArea", mDisplayArea);
			}

			#endregion
		};

		// common data to all layers
		protected string mName = BlueBrick.Properties.Resources.DefaultLayerName;
		protected bool mVisible = true;

		// non serialized members
		private static int nameInstanceCounter = 0; // a counter of instance, just to give a number next to the layer name
		protected List<LayerItem> mSelectedObjects = new List<LayerItem>();
		protected RectangleF mBoundingSelectionRectangle = new RectangleF(); // the rectangle in stud that surrond all the object selected
		protected Pen mBoundingSelectionPen = new Pen(Color.Black, 2);

		public const int NUM_PIXEL_PER_STUD_FOR_BRICKS = 8;	// the images save on the disk use 4 pixel per studs

		// grid snapping
		private static float mCurrentSnapGridSize = 32; // size of the snap grid in stud
		private static bool mSnapGridEnabled = true;
		private static float mCurrentRotationStep = 90; // angle in degree of the rotation

		// the list for the copy/paste
		protected static List<LayerItem> sCopyItems = new List<LayerItem>(); // a list of items created when user press CTRL+C to copy the current selection

		#region get/set

		/// <summary>
		/// get the name of the layer
		/// </summary>
		public static float CurrentSnapGridSize
		{
			get { return mCurrentSnapGridSize; }
			set { mCurrentSnapGridSize = value; }
		}

		/// <summary>
		/// enable/disable the snap grid
		/// </summary>
		public static bool SnapGridEnabled
		{
			get { return mSnapGridEnabled; }
			set { mSnapGridEnabled = value; }
		}

		/// <summary>
		/// The current rotation step in degree for the rotation operation
		/// </summary>
		public static float CurrentRotationStep
		{
			get { return mCurrentRotationStep; }
			set { mCurrentRotationStep = value; }
		}

		/// <summary>
		/// get the name of the layer
		/// </summary>
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}

		/// <summary>
		/// Tell if this layer is visible or not
		/// </summary>
		public bool Visible
		{
			get { return mVisible; }
			set { mVisible = value; }
		}

		/// <summary>
		/// get the list of current selected objects in this layer.
		/// the type of the object is different according to the type of the layer.
		/// The list can be empty.
		/// </summary>
		/// <returns>the list of current selected objects in this layer (the list can be empty if none is selected)</returns>
		public List<LayerItem> SelectedObjects
		{
			get { return mSelectedObjects; }
		}

		/// <summary>
		/// get the list of current items copied (waiting for a paste).
		/// the type of the object is different according to the type of the layer.
		/// The list can be empty.
		/// </summary>
		/// <returns>the list of current items copied (the list can be empty if none was copied)</returns>
		public static List<LayerItem> CopyItems
		{
			get { return sCopyItems; }
		}		
		#endregion

		#region constructor/copy
		/// <summary>
		/// Constructor
		/// </summary>
		public Layer()
		{
			mBoundingSelectionPen.DashStyle = DashStyle.Dash;
			// increase the layer number and add it to the name
			++nameInstanceCounter;
			mName += " " + nameInstanceCounter.ToString();
			// disable the copy on the main form if the list is empty
			MainForm.Instance.enableCopyButton(false);
		}

		/// <summary>
		/// copy only the option parameters from the specified layer
		/// </summary>
		/// <param name="layerToCopy">the model to copy from</param>
		public virtual void CopyOptionsFrom(Layer layerToCopy)
		{
			mName = layerToCopy.mName;
			mVisible = layerToCopy.mVisible;
		}

		/// <summary>
		/// Return the number of items in this layer
		/// </summary>
		/// <returns>the number of items in this layer</returns>
		public abstract int getNbItems();
		#endregion

		#region IXmlSerializable Members

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public virtual void ReadXml(System.Xml.XmlReader reader)
		{
			reader.ReadToDescendant("Name");
			mName = reader.ReadElementContentAsString();
			mVisible = reader.ReadElementContentAsBoolean();
		}

		public virtual void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString("Name", mName);
			writer.WriteElementString("Visible", mVisible.ToString().ToLower());
		}

		#endregion

		#region selection

		/// <summary>
		/// this function reset the instance counter use to name automatically the new layer created.
		/// This counter is typically reset when a new map is open or created.
		/// </summary>
		public static void resetNameInstanceCounter()
		{
			nameInstanceCounter = 0;
		}

		/// <summary>
		/// Compute the bounding rectangle that surround all the object in the
		/// mSelectedObjects list.
		/// </summary>
		public void updateBoundingSelectionRectangle()
		{
			// compute the bounding rectangle if some object are selected
			if (mSelectedObjects.Count > 0)
			{
				float minX = float.MaxValue;
				float maxX = float.MinValue;
				float minY = float.MaxValue;
				float maxY = float.MinValue;
				foreach (LayerItem item in mSelectedObjects)
				{
					if (item.mDisplayArea.Left < minX)
						minX = item.mDisplayArea.Left;
					if (item.mDisplayArea.Right > maxX)
						maxX = item.mDisplayArea.Right;
					if (item.mDisplayArea.Top < minY)
						minY = item.mDisplayArea.Top;
					if (item.mDisplayArea.Bottom > maxY)
						maxY = item.mDisplayArea.Bottom;
				}
				mBoundingSelectionRectangle = new RectangleF(minX, minY, maxX - minX, maxY - minY);
			}
			else
			{
				mBoundingSelectionRectangle = new RectangleF();
			}
		}

		/// <summary>
		/// Compute the bounding rectangle that surround all the object in the
		/// mSelectedObjects list.
		/// </summary>
		public void moveBoundingSelectionRectangle(PointF move)
		{
			mBoundingSelectionRectangle.X += move.X;
			mBoundingSelectionRectangle.Y += move.Y;
		}

		/// <summary>
		/// Add the specified object in the selection list.
		/// This method also refresh the bouding rectangle.
		/// </summary>
		/// <param name="obj">The object to add</param>
		public void addObjectInSelection(LayerItem obj)
		{
			mSelectedObjects.Add(obj);
			updateBoundingSelectionRectangle();
			// clear a flag for continuous rotation in the rotation action (not very clean I know)
			Actions.Bricks.RotateBrick.sLastCenterIsValid = false;
			Actions.Texts.RotateText.sLastCenterIsValid = false;
			// enable the copy on the main form
			MainForm.Instance.enableCopyButton(true);
		}

		/// <summary>
		/// Add the specified list of object in the selection list.
		/// This method also refresh the bouding rectangle.
		/// </summary>
		/// <param name="obj">The list of object to add</param>
		public void addObjectInSelection(List<LayerItem> objList)
		{
			mSelectedObjects.AddRange(objList);
			updateBoundingSelectionRectangle();
			// clear a flag for continuous rotation in the rotation action (not very clean I know)
			Actions.Bricks.RotateBrick.sLastCenterIsValid = false;
			Actions.Texts.RotateText.sLastCenterIsValid = false;
			// enable the copy on the main form
			MainForm.Instance.enableCopyButton(mSelectedObjects.Count > 0);
		}

		/// <summary>
		/// Remove the specified object from the selection list.
		/// This method also refresh the bouding rectangle.
		/// </summary>
		/// <param name="obj">The object to remove</param>
		protected void removeObjectFromSelection(LayerItem obj)
		{
			mSelectedObjects.Remove(obj);
			updateBoundingSelectionRectangle();
			// clear a flag for continuous rotation in the rotation action (not very clean I know)
			Actions.Bricks.RotateBrick.sLastCenterIsValid = false;
			Actions.Texts.RotateText.sLastCenterIsValid = false;
			// disable the copy on the main form if the list is empty
			MainForm.Instance.enableCopyButton(mSelectedObjects.Count > 0);
		}

		/// <summary>
		/// Remove the specified list of objects from the selection list.
		/// This method also refresh the bouding rectangle.
		/// </summary>
		/// <param name="objList">The list of objects to remove</param>
		public void removeObjectFromSelection(List<LayerItem> objList)
		{
			foreach (LayerItem obj in objList)
				mSelectedObjects.Remove(obj);
			updateBoundingSelectionRectangle();
			// clear a flag for continuous rotation in the rotation action (not very clean I know)
			Actions.Bricks.RotateBrick.sLastCenterIsValid = false;
			Actions.Texts.RotateText.sLastCenterIsValid = false;
			// enable the copy on the main form
			MainForm.Instance.enableCopyButton(mSelectedObjects.Count > 0);
		}

		/// <summary>
		/// Clear the selected object list
		/// </summary>
		public void clearSelection()
		{
			mSelectedObjects.Clear();
			updateBoundingSelectionRectangle();
			// clear a flag for continuous rotation in the rotation action (not very clean I know)
			Actions.Bricks.RotateBrick.sLastCenterIsValid = false;
			Actions.Texts.RotateText.sLastCenterIsValid = false;
			// disable the copy on the main form if the list is empty
			MainForm.Instance.enableCopyButton(false);
		}

		/// <summary>
		/// Select all the items in this layer.
		/// </summary>
		public virtual void selectAll()
		{
		}

		#endregion

		#region tool on point in stud
		/// <summary>
		/// Tell is the specified point (in stud coord) is inside the current
		/// selection rectangle.
		/// </summary>
		/// <param name="pointInStud">The point to test</param>
		/// <returns>true is the point is inside</returns>
		protected bool isPointInsideSelectionRectangle(PointF pointInStud)
		{
			if (mSelectedObjects.Count > 0)
			{
				// the selection is not empty, so we take the mouse down event only
				// if the mouse is inside the selected rectangle
				if ((pointInStud.X > mBoundingSelectionRectangle.Left) && (pointInStud.X < mBoundingSelectionRectangle.Right) &&
					(pointInStud.Y > mBoundingSelectionRectangle.Top) && (pointInStud.Y < mBoundingSelectionRectangle.Bottom))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Snap the specified point onto the current grid
		/// </summary>
		/// <param name="pointInStud">The point in stud coord</param>
		/// <returns>the nearest point on the grid</returns>
		public static PointF snapToGrid(PointF pointInStud)
		{
			if (mSnapGridEnabled)
			{
				PointF snappedPoint = new PointF();
				snappedPoint.X = (float)(Math.Round(pointInStud.X / CurrentSnapGridSize) * CurrentSnapGridSize);
				snappedPoint.Y = (float)(Math.Round(pointInStud.Y / CurrentSnapGridSize) * CurrentSnapGridSize);
				return snappedPoint;
			}
			else
			{
				return pointInStud;
			}
		}

		#endregion

		#region draw/mouse event

		/// <summary>
		/// Draw the layer.
		/// </summary>
		/// <param name="g">the graphic context in which draw the layer</param>
		public virtual void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
		{
			// draw the surrounding selection rectangle
			if (mSelectedObjects.Count > 0)
			{
				float x = (float)((mBoundingSelectionRectangle.X - areaInStud.Left) * scalePixelPerStud);
				float y = (float)((mBoundingSelectionRectangle.Y - areaInStud.Top) * scalePixelPerStud);
				float width = (float)(mBoundingSelectionRectangle.Width * scalePixelPerStud);
				float height = (float)(mBoundingSelectionRectangle.Height * scalePixelPerStud);
				g.DrawRectangle(mBoundingSelectionPen, x, y, width, height);
			}
		}

		/// <summary>
		/// This function is called to know if this layer is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if this layer wants to handle it</returns>
		public abstract bool handleMouseDown(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor);

		/// <summary>
		/// This method is called if the map decided that this layer should handle
		/// this mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public abstract bool mouseDown(MouseEventArgs e, PointF mouseCoordInStud);

		/// <summary>
		/// This method is called when the mouse move.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse move</param>
		/// <returns>true if the view should be refreshed</returns>
		public abstract bool mouseMove(MouseEventArgs e, PointF mouseCoordInStud);

		/// <summary>
		/// This method is called when the mouse button is released.
		/// </summary>
		/// <param name="e">the mouse event arg that describe the click</param>
		/// <returns>true if the view should be refreshed</returns>
		public abstract bool mouseUp(MouseEventArgs e, PointF mouseCoordInStud);

		/// <summary>
		/// Select all the item inside the rectangle in the current selected layer
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		public abstract void selectInRectangle(RectangleF selectionRectangeInStud);

		#endregion
	}
}
