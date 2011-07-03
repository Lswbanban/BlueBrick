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
using System.Collections;

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
            public static Hashtable sHashtableForGroupRebuilding = new Hashtable(); // this hashtable is used to recreate the group hierarchy when loading
			public static List<Group> sListForGroupSaving = new List<Group>(); // this list is used during the saving of BBM file to save all the grouping hierarchy

			protected RectangleF mDisplayArea = new RectangleF(); // in stud coordinate
			protected float mOrientation = 0;	// in degree
			protected Group mMyGroup = null; // the group in which this item is

			#region get/set
			/// <summary>
			/// Get or set the orientation of the Item
			/// </summary>
			public virtual float Orientation
			{
				get { return mOrientation; }
				set { mOrientation = value; }
			}

			/// <summary>
			///	Set the position in stud coord. The position of a brick is its top left corner.
			/// </summary>
			public virtual PointF Position
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
			public virtual PointF Center
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
			/// get the width of this item in stud
			/// </summary>
			public float Width
			{
				get { return mDisplayArea.Width; }
			}

			/// <summary>
			/// get the height of this item in stud
			/// </summary>
			public float Height
			{
				get { return mDisplayArea.Height; }
			}

			/// <summary>
			/// get the display area of the item
			/// </summary>
			public RectangleF DisplayArea
			{
				get { return mDisplayArea; }
			}

			/// <summary>
			/// set or get the group of this item
			/// </summary>
			public Group Group
			{
				get { return mMyGroup; }
				set { mMyGroup = value; }
			}

			/// <summary>
			/// This property tells if this Item is a group or not.
			/// This property is overriden by the group class.
			/// </summary>
			public virtual bool IsAGroup
			{
				get { return false; }
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
				readMyGroup(reader);
			}

			protected void readMyGroup(System.Xml.XmlReader reader)
			{
                if (Map.DataVersionOfTheFileLoaded > 4)
                {
					// check if we have a group
                    if (reader.IsEmptyElement)
                    {
                        mMyGroup = null;
                        reader.Read();
                    }
                    else
                    {
                        // get the group id
                        int hashCodeOfTheGroup = reader.ReadElementContentAsInt();
                        // look in the hastable if this connexion alread exists, else create it
                        Group group = sHashtableForGroupRebuilding[hashCodeOfTheGroup] as Group;
                        if (group == null)
                        {
                            // instanciate a new group, and add it in the hash table
                            group = new Group();
                            sHashtableForGroupRebuilding.Add(hashCodeOfTheGroup, group);
                        }
                        // then add this item in the group
                        group.addItem(this);
                    }
                }
			}

			public virtual void WriteXml(System.Xml.XmlWriter writer)
			{
				XmlReadWrite.writeRectangleF(writer, "DisplayArea", mDisplayArea);
				writeMyGroup(writer);
			}

			protected void writeMyGroup(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("MyGroup");
				if (mMyGroup != null)
				{
					writer.WriteString(mMyGroup.GetHashCode().ToString());
					if (!LayerItem.sListForGroupSaving.Contains(mMyGroup))
						LayerItem.sListForGroupSaving.Add(mMyGroup);
				}
				writer.WriteEndElement();
			}
			#endregion

			#region selection
			public void select(List<LayerItem> selectionList, bool addToSelection)
			{
				// check if this item belong to a group, in that case, call the select
				// method of the group, such as we can move up to the top of the tree
				if (mMyGroup != null)
					mMyGroup.select(selectionList, addToSelection);
				else
					selectHierachycally(selectionList, addToSelection);
			}

			public virtual void selectHierachycally(List<LayerItem> selectionList, bool addToSelection)
			{
				// we are on the leaf of the hierarchy, so just select the item
				if (addToSelection)
				{
					if (!selectionList.Contains(this))
						selectionList.Add(this);
				}
				else
				{
					selectionList.Remove(this);
				}
			}
			#endregion
		};

		/// <summary>
		/// A Group is a class that holds reference on Layer Items that are grouped together.
		/// A Group is also a LayerItem such as hierachical grouping is possible.
		/// </summary>
		public class Group : LayerItem
		{
			private List<LayerItem> mItems = new List<LayerItem>();

			#region get/set
			public override bool IsAGroup
			{
				get { return true; }
			}

			/// <summary>
			/// Set the position via the center of the object in stud coord.
			/// </summary>
			public override PointF Center
			{
				get
				{
					return new PointF(mDisplayArea.X + (mDisplayArea.Width * 0.5f), mDisplayArea.Y + (mDisplayArea.Height * 0.5f));
				}

				set
				{
					// compute the new values
					PointF newCorner = new PointF(value.X - (mDisplayArea.Width * 0.5f), value.Y - (mDisplayArea.Height * 0.5f));
					// compute the difference
					PointF translation = new PointF(newCorner.X - mDisplayArea.X, newCorner.Y - mDisplayArea.Y);
					// change the center of the group
					mDisplayArea.X = newCorner.X;
					mDisplayArea.Y = newCorner.Y;
					// add a shift for all the items of the group
					foreach (Layer.LayerItem item in mItems)
					{
						// ask the center first because it computed
						PointF newItemCenter = item.Center;
						newItemCenter.X += translation.X;
						newItemCenter.Y += translation.Y;
						item.Center = newItemCenter;
					}
				}
			}
			#endregion

			#region constructor
			public Group()
			{
			}
			
			public Group(string groupName)
			{
				List<BrickLibrary.Brick.SubPart> groupSubPartList = BrickLibrary.Instance.getGroupSubPartList(groupName);
				if (groupSubPartList != null)
					foreach (BrickLibrary.Brick.SubPart subPart in groupSubPartList)
					{
						// intanciate a new item (can be a group, so we call recursively the constructor)
						LayerItem newItem = null;
						if (subPart.mSubPartBrick.IsAGroup)
							newItem = new Group(subPart.mSubPartNumber);
						else
							newItem = new LayerBrick.Brick(subPart.mSubPartNumber, subPart.getWorldTranslation(), subPart.getWorldOrientation());
						// add the item in this group
						addItem(newItem);
					}
			}
			#endregion

			#region IXmlSerializable Members
			public override void ReadXml(System.Xml.XmlReader reader)
			{
				reader.ReadStartElement();
				readMyGroup(reader);
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("Group");
				writer.WriteAttributeString("id", this.GetHashCode().ToString());
				writeMyGroup(writer); // we just call the write of the group and not base.WriteXml because we don't need the display area for the group
				writer.WriteEndElement();
			}
			#endregion

			#region grouping management
			public void addItem(LayerItem item)
			{
				mItems.Add(item);
				item.Group = this;
			}

			public void removeItem(LayerItem item)
			{
				mItems.Remove(item);
				item.Group = null;
			}

			public void ungroup()
			{
				foreach (LayerItem item in mItems)
					item.Group = null;
			}

			public void regroup()
			{
				foreach (LayerItem item in mItems)
					item.Group = this;
			}

			/// <summary>
			/// This method start from this group as the top of the tree and return the list of all
			/// the leaf items (which are not group) in all the branches below this group
			/// </summary>
			/// <returns>A flat list of all the items found in the group and sub-group</returns>
			public List<LayerItem> getAllChildrenItems()
			{
				List<LayerItem> resultList = new List<LayerItem>(mItems.Count);
				getAllChildrenItemsRecursive(resultList);
				return resultList;
			}

			private void getAllChildrenItemsRecursive(List<LayerItem> resultList)
			{
				foreach (LayerItem item in mItems)
					if (item.IsAGroup)
						(item as Group).getAllChildrenItemsRecursive(resultList);
					else
						resultList.Add(item);
			}
			#endregion

			#region selection
			public override void selectHierachycally(List<LayerItem> selectionList, bool addToSelection)
			{
				// call the same method on all the items of the group
				// in order to select the wall tree
				foreach (LayerItem item in mItems)
					item.selectHierachycally(selectionList, addToSelection);
			}
			#endregion
		}


		// common data to all layers
		protected string mName = BlueBrick.Properties.Resources.DefaultLayerName;
		protected bool mVisible = true;
		protected int mTransparency = 100; // percentage (in int because it is easier to modify with a slider)

		// non serialized members
		private static int nameInstanceCounter = 0; // a counter of instance, just to give a number next to the layer name
		protected List<LayerItem> mSelectedObjects = new List<LayerItem>();
		protected RectangleF mBoundingSelectionRectangle = new RectangleF(); // the rectangle in stud that surrond all the object selected
		protected Pen mBoundingSelectionPen = new Pen(Color.Black, 2);

		public const int NUM_PIXEL_PER_STUD_FOR_BRICKS = 8;	// the images save on the disk use 8 pixel per studs

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
		/// The transparency percentage of the layer.
		/// </summary>
		public virtual int Transparency
		{
			get { return mTransparency; }
			set	{ mTransparency = value; }
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
			Transparency = layerToCopy.Transparency;
		}

		/// <summary>
		/// this function reset the instance counter use to name automatically the new layer created.
		/// This counter is typically reset when a new map is open or created.
		/// </summary>
		public static void resetNameInstanceCounter()
		{
			nameInstanceCounter = 0;
		}

		/// <summary>
		/// Return the number of items in this layer
		/// </summary>
		/// <returns>the number of items in this layer</returns>
		public abstract int getNbItems();
		#endregion

		#region XmlSerializable Members

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public virtual void ReadXml(System.Xml.XmlReader reader)
		{
            // clear all the content of the hash table
            LayerItem.sHashtableForGroupRebuilding.Clear();
            // read the common properties of the layer
			reader.ReadToDescendant("Name");
			mName = reader.ReadElementContentAsString();
			mVisible = reader.ReadElementContentAsBoolean();
			// read the transparency for all the layers
			if (Map.DataVersionOfTheFileLoaded > 4)
				Transparency = reader.ReadElementContentAsInt();

		}

        public virtual void postReadXml(System.Xml.XmlReader reader)
        {
			if (Map.DataVersionOfTheFileLoaded > 4)
			{
				// read the group to reconstruct the hierarchy
				bool groupFound = reader.ReadToDescendant("Group");
				while (groupFound)
				{
					// read the id of the current group
					int groupId = int.Parse(reader.GetAttribute(0));
					// look in the hastable if this group alread exists, else create it
					Group currentGroup = LayerItem.sHashtableForGroupRebuilding[groupId] as Group;
					if (currentGroup == null)
					{
						// instanciate a new group, and add it in the hash table
						currentGroup = new Group();
						LayerItem.sHashtableForGroupRebuilding.Add(groupId, currentGroup);
					}

					// then read the content of this group
					currentGroup.ReadXml(reader);

					// then read the next group
					groupFound = reader.ReadToNextSibling("Group");
				}
				// read the Groups tag, to finish the list of group
				reader.Read();
			}

            // clear the hash table for group to free the memory after loading
            LayerItem.sHashtableForGroupRebuilding.Clear();
        }

		public virtual void WriteXml(System.Xml.XmlWriter writer)
		{
            // clear all the content of the hash table
			LayerItem.sListForGroupSaving.Clear();
			// write the common properties
			writer.WriteElementString("Name", mName);
			writer.WriteElementString("Visible", mVisible.ToString().ToLower());
			writer.WriteElementString("Transparency", mTransparency.ToString());
		}

        public void postWriteXml(System.Xml.XmlWriter writer)
        {
			writer.WriteStartElement("Groups");
			// write the groups: we don't use a foreach because we will grow the list during iteration
			for (int i = 0; i < LayerItem.sListForGroupSaving.Count; ++i)
			{
				// write the group
				Group currentGroup = LayerItem.sListForGroupSaving[i];
				currentGroup.WriteXml(writer);
				// check if this group as a father and add it to the list
				Group fatherGroup = currentGroup.Group;
				if ((fatherGroup != null) && !LayerItem.sListForGroupSaving.Contains(fatherGroup))
					LayerItem.sListForGroupSaving.Add(fatherGroup);
			}
			writer.WriteEndElement();

			// clear the list after iteration
			LayerItem.sListForGroupSaving.Clear();
        }
		#endregion

		#region selection

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
					if (item.DisplayArea.Left < minX)
						minX = item.DisplayArea.Left;
					if (item.DisplayArea.Right > maxX)
						maxX = item.DisplayArea.Right;
					if (item.DisplayArea.Top < minY)
						minY = item.DisplayArea.Top;
					if (item.DisplayArea.Bottom > maxY)
						maxY = item.DisplayArea.Bottom;
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
			obj.select(mSelectedObjects, true);
			updateAfterSelectionChange();
		}

		/// <summary>
		/// Add the specified list of object in the selection list.
		/// This method also refresh the bouding rectangle.
		/// </summary>
		/// <param name="obj">The list of object to add</param>
		public void addObjectInSelection<T>(List<T> objList) where T : LayerItem
		{
			foreach (LayerItem obj in objList)
				obj.select(mSelectedObjects, true);
			updateAfterSelectionChange();
		}

		/// <summary>
		/// Remove the specified object from the selection list.
		/// This method also refresh the bouding rectangle.
		/// </summary>
		/// <param name="obj">The object to remove</param>
		protected void removeObjectFromSelection(LayerItem obj)
		{
			obj.select(mSelectedObjects, false);
			updateAfterSelectionChange();
		}

		/// <summary>
		/// Remove the specified list of objects from the selection list.
		/// This method also refresh the bouding rectangle.
		/// </summary>
		/// <param name="objList">The list of objects to remove</param>
		public void removeObjectFromSelection<T>(List<T> objList) where T : LayerItem
		{
			foreach (LayerItem obj in objList)
				obj.select(mSelectedObjects, false);
			updateAfterSelectionChange();
		}

		/// <summary>
		/// Clear the selected object list
		/// </summary>
		public void clearSelection()
		{
			mSelectedObjects.Clear();
			updateAfterSelectionChange();
		}

		/// <summary>
		/// The necessary common update made after any change in the selection list
		/// </summary>
		private void updateAfterSelectionChange()
		{
			// update the bouding selection rectangle
			updateBoundingSelectionRectangle();
			// clear a flag for continuous rotation in the rotation action (not very clean I know)
			Actions.Bricks.RotateBrick.sLastCenterIsValid = false;
			Actions.Texts.RotateText.sLastCenterIsValid = false;
			// enable the copy on the main form
			MainForm.Instance.enableCopyButton(mSelectedObjects.Count > 0);
            // enable the grouping button on the main form
            bool canGroupSelection = Actions.Items.GroupItems.findItemsToGroup(mSelectedObjects).Count > 1;
            bool canUngroupSelection = Actions.Items.UngroupItems.findItemsToUngroup(mSelectedObjects).Count > 0;
            MainForm.Instance.enableGroupingButton(canGroupSelection, canUngroupSelection);
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
				snappedPoint.X = (float)(Math.Floor(pointInStud.X / CurrentSnapGridSize) * CurrentSnapGridSize);
				snappedPoint.Y = (float)(Math.Floor(pointInStud.Y / CurrentSnapGridSize) * CurrentSnapGridSize);
				return snappedPoint;
			}
			else
			{
				return pointInStud;
			}
		}

		/// <summary>
		/// Get the layer item under the specified mouse coordinate or null if there's no brick under.
		/// The search is done in revers order of the list to get the topmost item.
		/// </summary>
		/// <param name="itemList">the list of layer item in which searching</param>
		/// <param name="mouseCoordInStud">the coordinate of the mouse cursor (in stud), where to look for</param>
		/// <returns>the layer item that is under the mouse coordinate or null if there is none.</returns>
		protected LayerItem getLayerItemUnderMouse<T>(List<T> itemList, PointF mouseCoordInStud) where T : LayerItem
		{
			for (int i = itemList.Count - 1; i >= 0; --i)
			{
				LayerItem item = itemList[i];
				if ((mouseCoordInStud.X > item.DisplayArea.Left) && (mouseCoordInStud.X < item.DisplayArea.Right) &&
					(mouseCoordInStud.Y > item.DisplayArea.Top) && (mouseCoordInStud.Y < item.DisplayArea.Bottom))
				{
					return item;
				}
			}
			return null;
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
		/// Return the cursor that should be display when the mouse is above the map without mouse click
		/// </summary>
		/// <param name="mouseCoordInStud"></param>
		public abstract Cursor getDefaultCursorWithoutMouseClick(PointF mouseCoordInStud);

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
