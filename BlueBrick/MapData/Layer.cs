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
		public abstract class LayerItem : IXmlSerializable
		{
            public static Hashtable sHashtableForGroupRebuilding = new Hashtable(); // this hashtable is used to recreate the group hierarchy when loading
			public static List<Group> sListForGroupSaving = new List<Group>(); // this list is used during the saving of BBM file to save all the grouping hierarchy

			protected RectangleF mDisplayArea = new RectangleF(); // in stud coordinate
			protected float mOrientation = 0;	// in degree
			protected Group mMyGroup = null; // the group in which this item is

			[NonSerialized]
			protected Tools.Surface mSelectionArea = null; // in stud coordinate. Most of the time, this is the hull of the item
			[NonSerialized]
			protected PointF mSnapToGridOffset = new PointF(0, 0); // an offset from the center of the part to the point that should snap to the grid border (in stud)

			#region get/set
			/// <summary>
			/// the part number of the item if any. A layer item always return an empty string as part number.
			/// This accessor is overriden in the derivated class.
			/// </summary>
			public virtual string PartNumber
			{
				get { return string.Empty; }
				set { }
			}

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
					// translate the selection area before changing the values
					translateSelectionArea(mDisplayArea.Location, value);
					// then set the new coordinate of the display area
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
					return new PointF(mDisplayArea.X + (mDisplayArea.Width * 0.5f), mDisplayArea.Y + (mDisplayArea.Height * 0.5f));
				}

				set
				{
					// compute the new corner position
					PointF newLocation = new PointF(value.X - (mDisplayArea.Width * 0.5f), value.Y - (mDisplayArea.Height * 0.5f));
					// translate the selection area before changing the values
					translateSelectionArea(mDisplayArea.Location, newLocation);
					// then set the new coordinate of the display area
					mDisplayArea.Location = newLocation;
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
			/// an offset from the center of the part to the point that should snap a corner of the grid
			/// </summary>
			public PointF SnapToGridOffset
			{
				get { return mSnapToGridOffset; }
			}

			/// <summary>
			/// Get the display area of the item in stud. Basically it is an Axis Aligned Bounding Box.
			/// </summary>
			public RectangleF DisplayArea
			{
				get { return mDisplayArea; }
			}

			/// <summary>
			/// Get the selection area of the item in stud. The selection area may be smaller and finer
			/// than the display area, most of the time it's basically the hull of the item.
			/// </summary>
			public Tools.Surface SelectionArea
			{
				get { return mSelectionArea; }
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
			/// Get the top group of the whole hierarchy this item belongs to, 
			/// or null if this item doesn't belong to any group.
			/// </summary>
			public Group TopGroup
			{
				get
				{
					Group topGroup = this.Group;
					if (topGroup != null)
					{
						while (topGroup.Group != null)
							topGroup = topGroup.Group;
					}
					return topGroup;
				}
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

			#region initialization/copy
			/// <summary>
			/// Clone this Item
			/// </summary>
			/// <returns>a new Item which is a conform copy of this</returns>
			public abstract LayerItem Clone();
			#endregion

			#region IXmlSerializable Members

			public System.Xml.Schema.XmlSchema GetSchema()
			{
				return null;
			}

			public virtual void ReadXml(System.Xml.XmlReader reader)
			{
				reader.Read(); // read the starting tag of the item
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

			#region update
			private void translateSelectionArea(PointF oldPosition, PointF newPosition)
			{
				if (mSelectionArea != null)
					mSelectionArea.translate(new PointF(newPosition.X - oldPosition.X, newPosition.Y - oldPosition.Y));
			}

			/// <summary>
			/// Update the snap margin (if any) of this item according to its current orientation and the margin
			/// defined in the part library (for now, if other type of items need to define their snap margin,
			/// some refactoring will be required). This update function should be called in the orientation setter.
			/// </summary>
			protected void updateSnapMargin()
			{
				BrickLibrary.Brick.Margin snapMargin = BrickLibrary.Instance.getSnapMargin(this.PartNumber);
				if ((snapMargin.mLeft != 0.0f) || (snapMargin.mRight != 0.0f) || (snapMargin.mTop != 0.0f) || (snapMargin.mBottom != 0.0f))
				{
					double angleInRadian = mOrientation * Math.PI / 180.0;
					double cosAngle = Math.Cos(angleInRadian);
					double sinAngle = Math.Sin(angleInRadian);
					double xSnapOffset = 0;
					double ySnapOffset = 0;
					if (cosAngle > 0)
					{
						xSnapOffset = snapMargin.mLeft * cosAngle;
						ySnapOffset = snapMargin.mTop * cosAngle;
					}
					else
					{
						cosAngle = -cosAngle;
						xSnapOffset = snapMargin.mRight * cosAngle;
						ySnapOffset = snapMargin.mBottom * cosAngle;
					}
					if (sinAngle > 0)
					{
						xSnapOffset += snapMargin.mBottom * sinAngle;
						ySnapOffset += snapMargin.mLeft * sinAngle;
					}
					else
					{
						sinAngle = -sinAngle;
						xSnapOffset += snapMargin.mTop * sinAngle;
						ySnapOffset += snapMargin.mRight * sinAngle;
					}
					mSnapToGridOffset = new PointF((float)xSnapOffset, (float)ySnapOffset);
				}
				else
				{
					mSnapToGridOffset = new PointF(0, 0);
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
			private bool mCanUngroup = true; // this flag tells if the group can be ungrouped or not
			private string mPartNumber = string.Empty;	// id of the group if any
			private LayerBrick.Brick mBrickThatHoldsActiveConnection = null;

			#region get/set
			public override bool IsAGroup
			{
				get { return true; }
			}

			/// <summary>
			/// Tell if this group is a known set from the part library which has a valid name
			/// </summary>
			public bool IsANamedGroup
			{
				get { return (mPartNumber != string.Empty); }
			}

			/// <summary>
			/// get the number of items in this group (only this level, not including all the children)
			/// </summary>
			public int ItemsCount
			{
				get { return mItems.Count; }
			}

			/// <summary>
			/// Tell if this group can be splitted and disbanded
			/// </summary>
			public bool CanUngroup
			{
				get { return mCanUngroup; }
			}

			/// <summary>
			/// the part number of the group if any.
			/// If the group is created from a part number of the library, this value is not empty, otherwise
			/// the accessor return an empty string.
			/// </summary>
			public override string PartNumber
			{
				get { return mPartNumber; }
			}

			/// <summary>
			///	Set the position in stud coord. The position of a brick is its top left corner.
			/// </summary>
			public override PointF Position
			{
				set
				{
					// translate the whole group
					translate(new PointF(value.X - mDisplayArea.X, value.Y - mDisplayArea.Y));
				}
			}

			/// <summary>
			/// Set the position via the center of the object in stud coord.
			/// </summary>
			public override PointF Center
			{
				set
				{
					// translate the whole group
					translate(new PointF(value.X - (mDisplayArea.Width * 0.5f) - mDisplayArea.X,
										value.Y - (mDisplayArea.Height * 0.5f) - mDisplayArea.Y));
				}
			}

			/// <summary>
			/// The current orientation of the group. This only has meaning if the group comes from the brick library
			/// and the setter accessor is mainly useful to recompute the snap margin of the group.
			/// </summary>
			public override float Orientation
			{
				set
				{
					mOrientation = value;
					updateSnapMargin();
				}
			}

			/// <summary>
			/// This property is only valid for a group of bricks.
			/// Set the active connection index of the whole group. The value is among a concatenation of all
			/// the connections index of the bricks in the group as they are listed in the whole children list
			/// </summary>
			public int ActiveConnectionIndex
			{
				get
				{
					int resultIndex = 0;
					// iterate through all the bricks to reach the correct brick
					if (mBrickThatHoldsActiveConnection != null)
					{
						List<LayerItem> bricksInTheGroup = getAllChildrenItems();
						foreach (Layer.LayerItem item in bricksInTheGroup)
						{
							LayerBrick.Brick brick = item as LayerBrick.Brick;
							if ((brick != null) && brick.HasConnectionPoint)
							{
								if (brick == mBrickThatHoldsActiveConnection)
								{
									resultIndex += brick.ActiveConnectionPointIndex;
									break;
								}
								else
									resultIndex += brick.ConnectionPoints.Count;
							}
						}
					}
					// return the result
					return resultIndex;
				}
				set
				{
					// reset the pointer on the brick and init a connection index variable with the value
					mBrickThatHoldsActiveConnection = null;
					int connexionIndex = value;
					// iterate through all the connection of the first bricks to reach the correct brick
					List<LayerItem> bricksInTheGroup = getAllChildrenItems();
					foreach (Layer.LayerItem item in bricksInTheGroup)
					{
						LayerBrick.Brick brick = item as LayerBrick.Brick;
						if ((brick != null) && brick.HasConnectionPoint)
						{
							int connectionCount = brick.ConnectionPoints.Count;
							if (connexionIndex >= connectionCount)
							{
								connexionIndex -= connectionCount;
							}
							else
							{
								// set the active connexion point with the wanted one
								brick.ActiveConnectionPointIndex = connexionIndex;
								mBrickThatHoldsActiveConnection = brick;
								break;
							}
						}
					}
				}
			}

			/// <summary>
			/// Accessor on the currently active connexion point of the group if any.
			/// This is actually the current active connection point of the brick that hold the current connection.
			/// </summary>
			public LayerBrick.Brick.ConnectionPoint ActiveConnectionPoint
			{
				get
				{
					if (mBrickThatHoldsActiveConnection != null)
						return mBrickThatHoldsActiveConnection.ActiveConnectionPoint;
					return null;
				}
			}

			/// <summary>
			/// This property is only valid for a group of brick. It returns the brick inside the group
			/// that should hold the active connection point of the whole group.
			/// </summary>
			public LayerBrick.Brick BrickThatHoldsActiveConnection
			{
				get { return mBrickThatHoldsActiveConnection; }
				set
				{
					if (mItems.Contains(value))
						mBrickThatHoldsActiveConnection = value;
					else
						mBrickThatHoldsActiveConnection = null;
				}
			}
			#endregion

			#region constructor/copy
			public Group()
			{
			}

			/// <summary>
			/// construct an empty group but specifying if this group can ungroup.
			/// This constructor is used by the loading code of a LDraw file
			/// </summary>
			/// <param name="canUngroup">tell if this group can ungroup or not</param>
			public Group(bool canUngroup)
			{
				mCanUngroup = canUngroup;
			}

			/// <summary>
			/// Copy constructor, only copy the part number and the orientation (to copy the snap margin)
			/// </summary>
			/// <param name="model"></param>
			public Group(Group model)
			{
				this.mPartNumber = model.mPartNumber;
				this.Orientation = model.mOrientation;
			}

			/// <summary>
			/// The public constructor to construct a specific group based on it's name id.
			/// This constructor will call the private recursive one, starting with the identity matrix.
			/// </summary>
			/// <param name="groupName">The part number to use to construct the group</param>
			public Group(string groupName): this(groupName, new Matrix())
			{
				// set the active connection to set the brick that hold it
				this.ActiveConnectionIndex = 0;
			}

			/// <summary>
			/// This private constructor create a whole hierachy of Group by computing the final world transform for
			/// each Bricks in the leafs. This constructor is then recursive, it will call itself for every node which
			/// is a Group.
			/// </summary>
			/// <param name="groupName">The group name top of the tree</param>
			/// <param name="parentTransform">The transform matrix (translation and orientation) of the all parents</param>
			private Group(string groupName, Matrix parentTransform)
			{
				// set the group name
				mPartNumber = groupName;
				// set the orientation of this group after the part number (useful top compute the snap marging)
				this.Orientation = (float)(Math.Atan2(parentTransform.Elements[1], parentTransform.Elements[0]) * 180.0 / Math.PI);
				// set the can ungroup flag
				mCanUngroup = BrickLibrary.Instance.canUngroup(groupName);
				// create all the parts inside the group
				List<BrickLibrary.Brick.SubPart> groupSubPartList = BrickLibrary.Instance.getGroupSubPartList(groupName);
				if (groupSubPartList != null)
					foreach (BrickLibrary.Brick.SubPart subPart in groupSubPartList)
					{
						// compute the world transform
						Matrix worldTransform = subPart.mLocalTransformInStud.Clone();
						worldTransform.Multiply(parentTransform, MatrixOrder.Append);

						// intanciate a new item (can be a group, or a real brick)
						LayerItem newItem = null;
						if (subPart.mSubPartBrick.IsAGroup)
						{
							// call this group constructor again
							newItem = new Group(subPart.mSubPartNumber, worldTransform);
						}
						else
						{
							// get the translation and orientation from the world transform
							PointF translation = new PointF(worldTransform.OffsetX, worldTransform.OffsetY);
							float orientation = (float)(Math.Atan2(worldTransform.Elements[1], worldTransform.Elements[0]) * 180.0 / Math.PI);
							// create the new item
							newItem = new LayerBrick.Brick(subPart.mSubPartNumber, translation, orientation);
						}

						// add the item in this group
						addItem(newItem);
					}
			}

			/// <summary>
			/// Clone this Group
			/// </summary>
			/// <returns>a new Group which is a conform copy of this</returns>
			public override LayerItem Clone()
			{
				// call the copy constructor
				return new Group(this);
			}
			#endregion

			#region IXmlSerializable Members
			public override void ReadXml(System.Xml.XmlReader reader)
			{
				reader.ReadStartElement();
				mPartNumber = BrickLibrary.Instance.getActualPartNumber(reader.ReadElementContentAsString().ToUpperInvariant());
				// set the flag according to the group name
				if (mPartNumber != string.Empty)
					mCanUngroup = BrickLibrary.Instance.canUngroup(mPartNumber);
				readMyGroup(reader);
			}

			public override void WriteXml(System.Xml.XmlWriter writer)
			{
				writer.WriteStartElement("Group");
				writer.WriteAttributeString("id", this.GetHashCode().ToString());
				// we don't need the display area for the group, so we don't call base.WriteXml
				writer.WriteElementString("PartNumber", mPartNumber);
				// Don't save canUngroup, this property is got from the library.
				// Also, we don't save here the mBrickThatHoldsActiveConnection, that's not a big deal
				// since it can be null. This is much easier otherwise we should use another hash table
				// to get back the brick from the id.
				// The consequence is that is a group with a active connection is saved, when you reload
				// the file and select the group via rectangle (no click on it), the group has lost is
				// active connection. But if you select by clicking on it the active connection will be updated
				writeMyGroup(writer);
				writer.WriteEndElement();
			}
			#endregion

			#region Transformation on the group
			public void translate(PointF translation)
			{
				// change the position of the group
				mDisplayArea.X += translation.X;
				mDisplayArea.Y += translation.Y;
				// add the same translation for all the items of the group
				foreach (Layer.LayerItem item in mItems)
				{
					PointF newItemPosition = item.Position;
					newItemPosition.X += translation.X;
					newItemPosition.Y += translation.Y;
					item.Position = newItemPosition;
				}
			}

			/// <summary>
			/// compute the display area from all the children in the group
			/// <param name="doItRecursive">If this flag is true, it will also update the display area of the sub group.
			/// Otherwise only use the display area of immediate children</param>
			/// </summary>
			public void computeDisplayArea(bool doItRecursive)
			{
				if (mItems.Count > 0)
				{
					// init the display area with the one of the first item
					mDisplayArea = mItems[0].DisplayArea;
					// then iterate on all the items
					foreach (Layer.LayerItem item in mItems)
					{
						// check if we also need to update the sub group
						if (doItRecursive && item.IsAGroup)
							(item as Group).computeDisplayArea(doItRecursive);
						// and after increase the area with the area of this item
						increaseDisplayAreaWithThisItem(item);
					}
				}
				else
				{
					mDisplayArea.X = mDisplayArea.Y = mDisplayArea.Width = mDisplayArea.Height = 0.0f;
				}
			}

			private void increaseDisplayAreaWithThisItem(Layer.LayerItem item)
			{
				// check if the new item added increase the size of the display area
				if (item.DisplayArea.Left < mDisplayArea.Left)
				{
					mDisplayArea.Width = mDisplayArea.Right - item.DisplayArea.Left;
					mDisplayArea.X = item.DisplayArea.Left;
				}
				if (item.DisplayArea.Top < mDisplayArea.Top)
				{
					mDisplayArea.Height = mDisplayArea.Bottom - item.DisplayArea.Top;
					mDisplayArea.Y = item.DisplayArea.Top;
				}
				if (item.DisplayArea.Right > mDisplayArea.Right)
					mDisplayArea.Width = item.DisplayArea.Right - mDisplayArea.Left;
				if (item.DisplayArea.Bottom > mDisplayArea.Bottom)
					mDisplayArea.Height = item.DisplayArea.Bottom - mDisplayArea.Top;
			}
			#endregion

			#region grouping management
			public void addItem(LayerItem item)
			{
				mItems.Add(item);
				item.Group = this;
				// if this item is the first added to the group, use its display area for the group
				if (mItems.Count == 1)
					mDisplayArea = item.DisplayArea;
				else
					increaseDisplayAreaWithThisItem(item);
			}

			public void addItem(List<Layer.LayerItem> itemList)
			{
				foreach (Layer.LayerItem item in itemList)
					addItem(item);
			}

			public void removeItem(LayerItem item)
			{
				mItems.Remove(item);
				item.Group = null;
				// recompute the whole display area of the group
				computeDisplayArea(false);
			}

			public void removeItem(List<Layer.LayerItem> itemList)
			{
				// for optim reason call the compute display area one time after removing all the items
				foreach (Layer.LayerItem item in itemList)
				{
					mItems.Remove(item);
					item.Group = null;
				}
				// recompute the whole display area of the group
				computeDisplayArea(false);
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
		protected static Pen sPentoDrawHull = new Pen(Color.Red);

		public const int NUM_PIXEL_PER_STUD_FOR_BRICKS = 8;	// the images save on the disk use 8 pixel per studs

		// grid snapping
		private static float mCurrentSnapGridSize = 32; // size of the snap grid in stud
		private static bool mSnapGridEnabled = true;
		private static float mCurrentRotationStep = 90; // angle in degree of the rotation

		// the action used during a copy/paste
		protected Actions.Items.DuplicateItems mLastDuplicateAction = null; // temp reference use during a ALT+mouse move action (that duplicate and move the bricks at the same time)

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
		/// get the type name id of this type of layer used in the xml file (not localized)
		/// </summary>
		public abstract string XmlTypeName
		{
			get;
		}

		/// <summary>
		/// get the localized name of this type of layer
		/// </summary>
		public abstract string LocalizedTypeName
		{
			get;
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
		/// Get the number of items in this layer
		/// </summary>
		public abstract int NbItems
		{
			get;
		}

		/// <summary>
		/// Tell if there is something that can be selected on this layer.
		/// By default returns true if the NbItem is not null.
		/// </summary>
		public virtual bool HasSomethingToSelect
		{
			get { return (this.NbItems > 0); }
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
			// A new layer is created, of course the selection is empty so 
			// disable some buttons in the toolbar related to an empty selection
			MainForm.Instance.enableToolbarButtonOnItemSelection(false);
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

		protected virtual T readItem<T>(System.Xml.XmlReader reader) where T : LayerItem
		{
			// by default return null for layers that don't have items
			return null;
		}

		private void readItemListFromClipboard(System.Xml.XmlReader reader, ref List<Layer.LayerItem> itemsList)
		{
			this.readItemsListFromXml<Layer.LayerItem>(reader, ref itemsList, "Items", false);
		}

		protected void readItemsListFromXml<T>(System.Xml.XmlReader reader, ref List<T> resultingList, string itemsListName, bool useProgressBar) where T : LayerItem
		{
			// clear all the content of the hash table
			LayerItem.sHashtableForGroupRebuilding.Clear();
			LayerBrick.Brick.ConnectionPoint.sHashtableForLinkRebuilding.Clear();

			// read the starting tag of the list
			reader.ReadStartElement(itemsListName);
			// check if the list is not empty and read the first child
			bool cellFound = !reader.IsEmptyElement;
			while (cellFound)
			{
				// instanciate a new text cell, read and add the new text cell
				LayerItem item = readItem<T>(reader);
				resultingList.Add(item as T);

				// check if the next element is a sibling and not the close element of the list
				cellFound = reader.IsStartElement();

				// step the progress bar for each text cell
				if (useProgressBar)
					MainForm.Instance.stepProgressBar();
			}
			// read the end of the list tag, to finish the list of items
			reader.ReadEndElement();

			// call the post read function to read the groups
			readGroupFromXml(reader);

			// clear again the hash table to free the memory after loading
			LayerBrick.Brick.ConnectionPoint.sHashtableForLinkRebuilding.Clear();
		}

        protected void readGroupFromXml(System.Xml.XmlReader reader)
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
		}

		protected void writeHeaderAndCommonProperties(System.Xml.XmlWriter writer)
		{
			// clear all the content of the hash table
			LayerItem.sListForGroupSaving.Clear();
			// layer with its type and id
			writer.WriteStartElement("Layer");
			writer.WriteAttributeString("type", this.XmlTypeName);
			writer.WriteAttributeString("id", this.GetHashCode().ToString());
			// write the common properties
			writer.WriteElementString("Name", mName);
			writer.WriteElementString("Visible", mVisible.ToString().ToLower());
			writer.WriteElementString("Transparency", mTransparency.ToString());
		}

		protected void writeFooter(System.Xml.XmlWriter writer)
		{
			writer.WriteEndElement(); // end of Layer
		}
		
		private void writeSelectionToClipboard(System.Xml.XmlWriter writer)
		{
			// write the header
			writeHeaderAndCommonProperties(writer);
			// write all the bricks
			writeItemsListToXml(writer, mSelectedObjects, "Items", false);
			// write the footer
			writeFooter(writer);
		}

		protected void writeItemsListToXml<T>(System.Xml.XmlWriter writer, List<T> itemsToWrite, string itemsListName, bool useProgressBar) where T : LayerItem
		{
			// and serialize the items list
			writer.WriteStartElement(itemsListName);
			foreach (T item in itemsToWrite)
			{
				item.WriteXml(writer);
				// step the progress bar for each brick
				if (useProgressBar)
					MainForm.Instance.stepProgressBar();
			}
			writer.WriteEndElement(); // end of itemsListName

			// call the post write to write the group list
			writeGroupToXml(writer);
		}

        protected void writeGroupToXml(System.Xml.XmlWriter writer)
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
		/// This static tool method return the top item of a hierachical group of layer items, or null if all the
		/// items of the list doesn't belong to the same unique hierarchical group.
		/// </summary>
		/// <param name="itemList">a list of layer items among which we should search a top item</param>
		/// <returns>The Group which is at the top of the hierarchical group, or an item, or null</returns>
		public static LayerItem sGetTopItemFromList(List<LayerItem> itemList)
		{
			if (itemList.Count == 1)
			{
				return itemList[0];
			}
			else if (itemList.Count > 1)
			{
				Layer.Group topGroup = null;
				foreach (Layer.LayerItem item in itemList)
				{
					// get the group of the item
					Layer.Group fatherGroup = item.Group;
					// if any item doesn't have any group, since there's several items selected,
					// we know that they cannot be in the same group
					if (fatherGroup == null)
						return null;
					// find the top father
					while (fatherGroup.Group != null)
						fatherGroup = fatherGroup.Group;
					// if the top group is not initialized yet, do it now
					if (topGroup == null)
						topGroup = fatherGroup;
					// if we found two different top father, stop the search
					if (fatherGroup != topGroup)
						return null;
				}
				// iteration finished without finding different top group, so it's ok
				return topGroup;
			}
			// no object selected (selection is empty)
			return null;
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
		/// clear the slection and select all the object in parameters.
		/// This method is unsafe because if one object in parameter is part of a group,
		/// the whole group will not be selected. Also the necessary updates on the MainForms are 
		/// not performed. Only the bounding rectangle is also updated.
		/// This method is used as a temporary selection during the move of a flex move.
		/// </summary>
		/// <param name="obj">The list of object to unsafly select</param>
		public void unsafeSetSelection<T>(List<T> objList) where T : LayerItem
		{
			// set the selection with the specified list
			mSelectedObjects.Clear();
			mSelectedObjects.AddRange(objList as List<LayerItem>);
			// update the bouding selection rectangle
			updateBoundingSelectionRectangle();
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
			// enable or disable the toolbar buttons related to the selection (if it is empty or not)
			MainForm.Instance.enableToolbarButtonOnItemSelection(mSelectedObjects.Count > 0);
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

		#region copy/paste to clipboard
		/// <summary>
		/// This class is used to sort a list of layer item in the same order as in the list provided in the constructor
		/// </summary>
		public class LayerItemComparer<T> : System.Collections.Generic.IComparer<LayerItem> where T : LayerItem
		{
			private List<T> mListOrderToCopy = null;

			public LayerItemComparer(List<T> list)
			{
				mListOrderToCopy = list;
			}

			public int Compare(Layer.LayerItem item1, Layer.LayerItem item2)
			{
				int order1 = mListOrderToCopy.IndexOf(item1 as T);
				int order2 = mListOrderToCopy.IndexOf(item2 as T);
				return (order2 - order1);
			}
		};

		/// <summary>
		/// Serialize the list of the selected items in XML and copy the text to the clipboard,
		/// so that later it can be paste in another layer
		/// This method should be called on a CTRL+C
		/// </summary>
		public virtual void copyCurrentSelectionToClipboard()
		{
		}

		/// <summary>
		/// Serialize the list of the selected items in XML and copy the text to the clipboard,
		/// so that later it can be paste in another layer
		/// This method should be called on a CTRL+C
		/// <param name="allObjList">the list of all the items in the layer, in order to copy the selected items in the same order</param>
		/// </summary>
		protected void copyCurrentSelectionToClipboard<T>(List<T> allObjList) where T : LayerItem
		{
			// do nothing if the selection is empty
			if (SelectedObjects.Count > 0)
			{
				// Sort the seltected list as it is sorted on the layer such as the clone list
				// will be also sorted as on the layer
				LayerItemComparer<T> comparer = new LayerItemComparer<T>(allObjList);
				SelectedObjects.Sort(comparer);

				// we need to serialize the list of items in XML, for that create a xml writer
				System.IO.StringWriter stringWriter = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture);
				System.Xml.XmlWriterSettings xmlSettings = new System.Xml.XmlWriterSettings();
				xmlSettings.CheckCharacters = false;
				xmlSettings.CloseOutput = true;
				xmlSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
				xmlSettings.Encoding = Encoding.UTF8;
				xmlSettings.Indent = true;
				xmlSettings.IndentChars = "\t";
				xmlSettings.OmitXmlDeclaration = true;
				xmlSettings.NewLineOnAttributes = false;
				System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(stringWriter, xmlSettings);

				// then call the serialization method on the list of object
				this.writeSelectionToClipboard(xmlWriter);
				xmlWriter.Flush();

				// finally copy the serialized string into the clipboard
				string text = stringWriter.ToString();
				if (text != string.Empty)
					Clipboard.SetText(text);

				// and close the writer
				xmlWriter.Close();

				// enable the paste buttons
				MainForm.Instance.enablePasteButton(true);
			}
		}

		/// <summary>
		/// Paste (duplicate) the list of bricks that was previously copied with a call to copyCurrentSelectionToClipboard()
		/// This method should be called on a CTRL+V
		/// <returns>true if the paste was successful</returns>
		/// </summary>
		public bool pasteClipboardInLayer()
		{
			string itemTypeName = null;
			return pasteClipboardInLayer(out itemTypeName);
		}

		/// <summary>
		/// Paste (duplicate) the list of bricks that was previously copied with a call to copyCurrentSelectionToClipboard()
		/// and also give in the output parameter the type of items currently copied in the Clipboard.
		/// This method should be called on a CTRL+V
		/// <param name="itemTypeName">the localized type name of items that is in the Clipboard</param>
		/// <returns>true if the paste was successful</returns>
		/// </summary>
		public bool pasteClipboardInLayer(out string itemTypeName)
		{
			// that create a xml reader to read the xml copied in the clipboard
			System.IO.StringReader stringReader = new System.IO.StringReader(Clipboard.GetText());
			System.Xml.XmlReaderSettings xmlSettings = new System.Xml.XmlReaderSettings();
			xmlSettings.CheckCharacters = false;
			xmlSettings.CloseInput = true;
			xmlSettings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
			xmlSettings.IgnoreComments = true;
			xmlSettings.IgnoreWhitespace = true;
			System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader, xmlSettings);

			// get the type of the copied items
			itemTypeName = string.Empty;
			string layerId = string.Empty;
			if (xmlReader.ReadToDescendant("Layer"))
			{
				// get the 'type' attribute of the layer
				xmlReader.ReadAttributeValue();
				itemTypeName = xmlReader.GetAttribute(0);
				layerId = xmlReader.GetAttribute(1);
			}

			// check if we need to add an offset
			int copyStyle = Properties.Settings.Default.OffsetAfterCopyStyle;
			bool addOffset = (copyStyle == 2) || ((copyStyle == 1) && (layerId.Equals(this.GetHashCode().ToString())));

			// read the items
			List<Layer.LayerItem> itemsToDuplicates = new List<Layer.LayerItem>();
			this.readItemListFromClipboard(xmlReader, ref itemsToDuplicates);

			// check if the type of layer match the type of copied items
			bool typeMatch = false;
			mLastDuplicateAction = null;
			if (this is LayerText)
			{
				if (itemTypeName.Equals(this.XmlTypeName))
				{
					mLastDuplicateAction = new Actions.Texts.DuplicateText((this as LayerText), itemsToDuplicates, addOffset);
					typeMatch = true;
				}
				else if (itemTypeName == string.Empty)
				{
					// this seems to be a bold text (not saved in xml) that may be copied in the clipboard from another program
					itemsToDuplicates.Clear();
					itemsToDuplicates.Add(new LayerText.TextCell(Clipboard.GetText(), Properties.Settings.Default.DefaultTextFont, Properties.Settings.Default.DefaultTextColor, StringAlignment.Near));
					mLastDuplicateAction = new Actions.Texts.DuplicateText((this as LayerText), itemsToDuplicates, addOffset);
					typeMatch = true;
				}
			}
			else if (this is LayerBrick)
			{
				if (itemTypeName.Equals(this.XmlTypeName))
				{
					mLastDuplicateAction = new Actions.Bricks.DuplicateBrick((this as LayerBrick), itemsToDuplicates, addOffset);
					typeMatch = true;
				}
			}
			else if (this is LayerRuler)
			{
				if (itemTypeName.Equals(this.XmlTypeName))
				{
					mLastDuplicateAction = new Actions.Rulers.DuplicateRuler((this as LayerRuler), itemsToDuplicates, addOffset);
					typeMatch = true;
				}
			}

			// do the paste action
			if (mLastDuplicateAction != null)
				ActionManager.Instance.doAction(mLastDuplicateAction);

			// localize the item type name
			if (itemTypeName.Equals("brick"))
				itemTypeName = Properties.Resources.ErrorMsgLayerTypeBrick;
			else if (itemTypeName.Equals("text"))
				itemTypeName = Properties.Resources.ErrorMsgLayerTypeText;
			else if (itemTypeName.Equals("ruler"))
				itemTypeName = Properties.Resources.ErrorMsgLayerTypeRuler;
			else if (itemTypeName.Equals("area"))
				itemTypeName = Properties.Resources.ErrorMsgLayerTypeArea;
			else if (itemTypeName.Equals("grid"))
				itemTypeName = Properties.Resources.ErrorMsgLayerTypeGrid;
			else
				itemTypeName = Properties.Resources.ErrorMsgLayerTypeUnknown;

			// return if it is a success
			return typeMatch;
		}
		#endregion

		#region tool on point in stud
		/// <summary>
		/// Convert the specified point in stud coordinate into a point in pixel coordinate given the 
		/// current area displayed in stud and the current scale of the view.
		/// </summary>
		/// <param name="pointInStud">The point to convert</param>
		/// <param name="areaInStud">The current displayed area in stud coordinate</param>
		/// <param name="scalePixelPerStud">The current scale of the view</param>
		/// <returns>The same point but expressed in pixel coordinate in the current view</returns>
		public static PointF sConvertPointInStudToPixel(PointF pointInStud, RectangleF areaInStud, double scalePixelPerStud)
		{
			return new PointF((float)((pointInStud.X - areaInStud.Left) * scalePixelPerStud),
								(float)((pointInStud.Y - areaInStud.Top) * scalePixelPerStud));
		}

		/// <summary>
		/// Convert the specified polygon in stud coordinate into a polygon in pixel coordinate given the 
		/// current area displayed in stud and the current scale of the view.
		/// </summary>
		/// <param name="polygonInStud">The polygon to convert</param>
		/// <param name="areaInStud">The current displayed area in stud coordinate</param>
		/// <param name="scalePixelPerStud">The current scale of the view</param>
		/// <returns>The same polygon but expressed in pixel coordinate in the current view</returns>
		public static PointF[] sConvertPolygonInStudToPixel(PointF[] polygonInStud, RectangleF areaInStud, double scalePixelPerStud)
		{
			// create an array of the same size
			PointF[] polygonInPixel = new PointF[polygonInStud.Length];
			// call the point conversion method on every point
			for (int i = 0; i < polygonInStud.Length; ++i)
				polygonInPixel[i] = sConvertPointInStudToPixel(polygonInStud[i], areaInStud, scalePixelPerStud);
			// return the result
			return polygonInPixel;
		}

		/// <summary>
		/// Convert the specified rectangle in stud coordinates into a rectangle in pixel coordinate given the
		/// current area displayed in stud and the current scale of the view.
		/// </summary>
		/// <param name="polygonInStud">The polygon to convert</param>
		/// <param name="areaInStud">The current displayed area in stud coordinate</param>
		/// <param name="scalePixelPerStud">The current scale of the view</param>
		/// <returns>The same rectangle but expressed in pixel coordinate in the current view</returns>
		public static RectangleF sConvertRectangleInStudToPixel(RectangleF rectangleInStud, RectangleF areaInStud, double scalePixelPerStud)
		{
			return new RectangleF(sConvertPointInStudToPixel(rectangleInStud.Location, areaInStud, scalePixelPerStud),
								new SizeF((float)(rectangleInStud.Width * scalePixelPerStud), (float)(rectangleInStud.Height * scalePixelPerStud)));
		}

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
				if (item.SelectionArea.isPointInside(mouseCoordInStud))
					return item;
			}
			return null;
		}

		#endregion

		#region draw/mouse event
		/// <summary>
		/// get the total area in stud covered by all the items in the specified list of items
		/// </summary>
		/// <returns></returns>
		protected RectangleF getTotalAreaInStud<T>(List<T> itemList) where T : LayerItem
		{
			PointF topLeft = new PointF(float.MaxValue, float.MaxValue);
			PointF bottomRight = new PointF(float.MinValue, float.MinValue);
			foreach (LayerItem item in itemList)
			{
				RectangleF textArea = item.DisplayArea;
				if (textArea.X < topLeft.X)
					topLeft.X = textArea.X;
				if (textArea.Y < topLeft.Y)
					topLeft.Y = textArea.Y;
				if (textArea.Right > bottomRight.X)
					bottomRight.X = textArea.Right;
				if (textArea.Bottom > bottomRight.Y)
					bottomRight.Y = textArea.Bottom;
			}
			return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
		}

		/// <summary>
		/// get the total area in stud covered by all the layer items in this layer
		/// </summary>
		/// <returns></returns>
		public abstract RectangleF getTotalAreaInStud();

		/// <summary>
		/// Draw the layer.
		/// </summary>
		/// <param name="g">the graphic context in which draw the layer</param>
		public virtual void draw(Graphics g, RectangleF areaInStud, double scalePixelPerStud)
		{
			// draw the surrounding selection rectangle
			if (mSelectedObjects.Count > 0)
			{
				PointF upperLeftCorner = sConvertPointInStudToPixel(mBoundingSelectionRectangle.Location, areaInStud, scalePixelPerStud);
				float width = (float)(mBoundingSelectionRectangle.Width * scalePixelPerStud);
				float height = (float)(mBoundingSelectionRectangle.Height * scalePixelPerStud);
				g.DrawRectangle(mBoundingSelectionPen, upperLeftCorner.X, upperLeftCorner.Y, width, height);
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
		/// This function is called to know if this layer is interested by the specified mouse click
		/// </summary>
		/// <param name="e">the mouse event arg that describe the mouse click</param>
		/// <returns>true if this layer wants to handle it</returns>
		public virtual bool handleMouseMoveWithoutClick(MouseEventArgs e, PointF mouseCoordInStud, ref Cursor preferedCursor)
		{
			return false;
		}

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

		/// <summary>
		/// Generic implementation to select all the item inside the rectangle from the specified list
		/// of layer items
		/// </summary>
		/// <param name="selectionRectangeInStud">the rectangle in which select the items</param>
		/// <param name="itemList">the list of layer item in which searching the selection</param>
		protected void selectInRectangle<T>(RectangleF selectionRectangeInStud, List<T> itemList) where T : LayerItem
		{
			// fill it with all the items in the rectangle
			List<LayerItem> objListInRectangle = new List<LayerItem>(itemList.Count);
			foreach (LayerItem item in itemList)
			{
				if (item.SelectionArea.isRectangleIntersect(selectionRectangeInStud))
					objListInRectangle.Add(item);
			}
			// check if it is a brand new selection or a add/remove selection
			if (Control.ModifierKeys != BlueBrick.Properties.Settings.Default.MouseMultipleSelectionKey)
			{
				// the control key is not pressed, it is a brand new selection
				// clear the selection list and add all the object in the rectangle
				mSelectedObjects.Clear();
				addObjectInSelection(objListInRectangle);
			}
			else
			{
				// check if we found new object to add in the selection, then add them
				// else remove all the objects in the rectangle
				bool objectToAddFound = false;
				foreach (LayerItem item in objListInRectangle)
					if (!mSelectedObjects.Contains(item))
					{
						addObjectInSelection(item);
						objectToAddFound = true;
					}
				// check if it is a remove type
				if (!objectToAddFound)
					removeObjectFromSelection(objListInRectangle);
			}
		}
		#endregion
	}
}
