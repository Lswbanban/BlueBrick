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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BlueBrick.MapData;
using System.IO;

namespace BlueBrick
{
	public partial class PartListForm : Form
	{
		private class IconEntry
		{
			public Bitmap mImage = null;
			public int mImageIndex = 0;

			/// <summary>
			/// create an icon and store it in the specified image list and keep the index
			/// </summary>
			public IconEntry(string partNumber, ImageList imageList)
			{
				// get the image of the part from the library
				Image originalPartImage = BrickLibrary.Instance.getImage(partNumber);
				// create a snapshot of the current image and replace it in the two image lists
				// but to avoid a stretching effect, we redraw the picture in a square
				// first compute the position and size of the bitmap to draw
				int maxSize = Math.Max(originalPartImage.Width, originalPartImage.Height);
				Rectangle drawRectangle = new Rectangle();
				drawRectangle.Width = (16 * originalPartImage.Width) / maxSize;
				drawRectangle.Height = (16 * originalPartImage.Height) / maxSize;
				drawRectangle.X = (16 - drawRectangle.Width) / 2;
				drawRectangle.Y = (16 - drawRectangle.Height) / 2;
				// create a new bitmap to draw the scaled part
				mImage = new Bitmap(16, 16);
				Graphics graphics = Graphics.FromImage(mImage);
				graphics.Clear(Color.Transparent);
				graphics.DrawImage(originalPartImage, drawRectangle);
				graphics.Flush();
				// add the thumbnailImage to the list
				mImageIndex = imageList.Images.Count;
				imageList.Images.Add(mImage);
			}
		}

		private class BrickEntry
		{
			private string mPartNumber = string.Empty;
			private int mQuantity = 0;
			private int mImageIndex = 0;
			private ListViewItem mItem = null;

			public bool IsQuantityNull
			{
				get { return (mQuantity == 0); }
			}

			public ListViewItem Item
			{
				get { return mItem; }
			}

			/// <summary>
			/// Create a brick entry, and also create a thumbnail image of this part
			/// </summary>
			/// <param name="partNumber"></param>
			/// <param name="group"></param>
			public BrickEntry(string partNumber, int imageIndex)
			{
				mPartNumber = partNumber;
				mImageIndex = imageIndex;

				// get the description and color from the database
				string[] brickInfo = BrickLibrary.Instance.getBrickInfo(mPartNumber);
				// the first text of the array must be the part number because it is treated as the item text itself
				// and the columns are defined in this order: part, quantity, color and description
				// even if the display order is different (quantity, part, color and description)
				string[] itemTexts = { brickInfo[0], mQuantity.ToString(), brickInfo[2], brickInfo[3] };
				mItem = new ListViewItem(itemTexts, mImageIndex);
				mItem.SubItems[2].Tag = brickInfo[1]; // store the color index in the tag of the color subitem, used in the html export
			}

			public void incrementQuantity()
			{
				mQuantity++;
				mItem.SubItems[1].Text = mQuantity.ToString();
			}

			public void decrementQuantity()
			{
				mQuantity--;
				mItem.SubItems[1].Text = mQuantity.ToString();
			}
		}

		private class GroupEntry
		{
			public Dictionary<string, BrickEntry> mBrickEntryList = new Dictionary<string, BrickEntry>();
			public LayerBrick mLayer = null;
			private ListViewGroup mGroup = null;

			public ListViewGroup Group
			{
				get { return mGroup; }
			}

			public GroupEntry(LayerBrick layer, ListView listView)
			{
				mLayer = layer;
				if (layer != null)
					mGroup = new ListViewGroup(layer.Name);
			}
		}

		private MainForm mMainFormReference = null;
		private List<GroupEntry> mGroupEntryList = new List<GroupEntry>();
		private Dictionary<string, IconEntry> mThumbnailImage = new Dictionary<string, IconEntry>();

		#region constructor
		public PartListForm(MainForm parentForm)
		{
			InitializeComponent();
			// set the size of the image (do not change)
			this.listView.SmallImageList = new ImageList();
			this.listView.SmallImageList.ImageSize = new Size(16, 16);
			// save the parent form
			mMainFormReference = parentForm;
		}
		#endregion
		#region update

		/// <summary>
		/// this method should be called when a new layer is added
		/// </summary>
		public void addLayerNotification(LayerBrick newLayer)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (newLayer == null || !this.Visible)
				return;

			addLayer(newLayer);
		}

		/// <summary>
		/// this method should be called when a new layer is removed
		/// </summary>
		public void removeLayerNotification(LayerBrick deletedLayer)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (deletedLayer == null || !this.Visible)
				return;

			removeLayer(deletedLayer);
		}

		/// <summary>
		/// this method should be called when a new layer is renamed
		/// </summary>
		public void renameLayerNotification(LayerBrick layer)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (layer == null || !this.Visible || !this.useGroupCheckBox.Checked)
				return;

			// search the layer
			foreach (GroupEntry groupEntry in mGroupEntryList)
				if (groupEntry.mLayer == layer)
				{
					groupEntry.Group.Header = layer.Name;
					break;
				}
		}

		/// <summary>
		/// Find and return the group entry in the list that correspond to the specified layer.
		/// </summary>
		/// <param name="layer">The layer you want to search</param>
		/// <param name="createOneIfMissing">if true a group entry will be created if we can't find a match, otherwise null will be returned</param>
		/// <returns>the group entry corresponding to the specified layer or null if it can't be find and it was not specified to create one</returns>
		private GroupEntry getGroupEntryFromLayer(LayerBrick layer, bool createOneIfMissing)
		{
			// search the group entry associated with this layer
			GroupEntry currentGroupEntry = null;
			if (this.useGroupCheckBox.Checked)
			{
				foreach (GroupEntry groupEntry in mGroupEntryList)
					if (groupEntry.mLayer == layer)
					{
						currentGroupEntry = groupEntry;
						break;
					}
				// if the group entry is not found we create one
				if ((currentGroupEntry == null) && createOneIfMissing)
				{
					currentGroupEntry = new GroupEntry(layer, this.listView);
					mGroupEntryList.Add(currentGroupEntry);
					this.listView.Groups.Add(currentGroupEntry.Group);
				}
			}
			else
			{
				currentGroupEntry = mGroupEntryList[0];
			}

			return currentGroupEntry;
		}

		/// <summary>
		/// this method should be called when a brick is added
		/// </summary>
		public void addBrickNotification(LayerBrick layer, Layer.LayerItem brickOrGroup)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (!this.Visible)
				return;

			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(layer, true);

			// add the specified brick
			addBrick(brickOrGroup, currentGroupEntry);
		}

		/// <summary>
		/// this method should be called when a brick is deleted
		/// </summary>
		public void removeBrickNotification(LayerBrick layer, Layer.LayerItem brickOrGroup)
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (!this.Visible)
				return;

			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(layer, false);
			// if the group entry is not found we can exit
			if (currentGroupEntry == null)
				return;

			// remove the specified brick
			removeBrick(brickOrGroup, currentGroupEntry);

			// remove the group from the list view and the mGroupEntryList if it is empty
			if (this.useGroupCheckBox.Checked && (currentGroupEntry.Group.Items.Count == 0))
			{
				this.listView.Groups.Remove(currentGroupEntry.Group);
				mGroupEntryList.Remove(currentGroupEntry);
			}
		}

		private void addLayer(LayerBrick brickLayer)
		{
			// skip the invisible layers
			if (!brickLayer.Visible)
				return;

			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(brickLayer, true);

			// iterate on all the bricks of the list
			foreach (Layer.LayerItem item in brickLayer.LibraryBrickList)
				addBrick(item, currentGroupEntry);
		}

		/// <summary>
		/// this method update the view by adding a brick or incrementing its count
		/// </summary>
		/// <param name="brickOrGroup">the brick to add in the view</param>
		/// <param name="groupEntry">the concerned group in which adding the brick</param>
		private void addBrick(Layer.LayerItem brickOrGroup, GroupEntry groupEntry)
		{
			// get a pointer on the current brick entry list
			Dictionary<string, BrickEntry> brickEntryList = groupEntry.mBrickEntryList;

			// get the part number
			string partNumber = brickOrGroup.PartNumber;

			// try to get an entry in the image dictionary, else add it
			IconEntry iconEntry = null;
			if (!mThumbnailImage.TryGetValue(partNumber, out iconEntry))
			{
				iconEntry = new IconEntry(partNumber, this.listView.SmallImageList);
				mThumbnailImage.Add(partNumber, iconEntry);
			}

			// try to get an entry in the dictionnary for the current brick
			// to get the previous count, then increase the count and store the new value
			BrickEntry brickEntry = null;
			if (!brickEntryList.TryGetValue(partNumber, out brickEntry))
			{
				// create a new entry and add it
				brickEntry = new BrickEntry(partNumber, iconEntry.mImageIndex);
				brickEntryList.Add(partNumber, brickEntry);
			}
			// affect the correct group to the item
			brickEntry.Item.Group = groupEntry.Group;

			// add the item in the list view if not already in
			if (brickEntry.IsQuantityNull)
				this.listView.Items.Add(brickEntry.Item);
			// and increment its count
			brickEntry.incrementQuantity();
		}

		/// <summary>
		/// This method update the view when a layer is removed from the view
		/// </summary>
		private void removeLayer(LayerBrick brickLayer)
		{
			// get the group entry associated with this layer
			GroupEntry currentGroupEntry = getGroupEntryFromLayer(brickLayer, false);
			// if the group entry is not found we can exit
			if (currentGroupEntry == null)
				return;

			// iterate on the bricks of the layer to decrease all the part count in the current dictionary
			foreach (Layer.LayerItem item in brickLayer.LibraryBrickList)
				removeBrick(item, currentGroupEntry);

			// remove the group from the list view and the mGroupEntryList
			if (this.useGroupCheckBox.Checked)
			{
				this.listView.Groups.Remove(currentGroupEntry.Group);
				mGroupEntryList.Remove(currentGroupEntry);
			}
		}

		/// <summary>
		/// update the view by decrementing the brick count or removing it
		/// </summary>
		/// <param name="brickOrGroup">the brick to remove from the view</param>
		/// <param name="brickEntryList">the concerned group from which removing the brick</param>
		private void removeBrick(Layer.LayerItem brickOrGroup, GroupEntry groupEntry)
		{
			BrickEntry brickEntry = null;
			if (groupEntry.mBrickEntryList.TryGetValue(brickOrGroup.PartNumber, out brickEntry))
			{
				brickEntry.decrementQuantity();
				if (brickEntry.IsQuantityNull)
				{
					this.listView.Items.Remove(brickEntry.Item);
				}
			}
		}

		/// <summary>
		/// rebuild the full list from scratch
		/// </summary>
		public void rebuildList()
		{
			// do nothing if the window is not visible
			// because we rebuild everything when it becomes visible
			if (!this.Visible)
				return;

			// clear everyting that we will rebuild
			mThumbnailImage.Clear();
			mGroupEntryList.Clear();
			this.listView.Groups.Clear();
			this.listView.Items.Clear();
			this.listView.SmallImageList.Images.Clear();

			// create a default dictionnary if we don't use the layers
			if (!this.useGroupCheckBox.Checked)
			{
				GroupEntry groupEntry = new GroupEntry(null, null);
				mGroupEntryList.Add(groupEntry);
			}

			// iterate on all the brick of all the brick layers,
			// create on group per layer if needed, and create one entry
			// in the dictionnary
			foreach (Layer layer in Map.Instance.LayerList)
			{
				LayerBrick brickLayer = layer as LayerBrick;
				if (brickLayer != null)
					addLayer(brickLayer);
			}
		}

		#endregion
		#region export list

		private void exportItemsInTxt(StreamWriter writer, int[] columnOrder, int[] maxLength, ListView.ListViewItemCollection itemList)
		{
			foreach (ListViewItem item in itemList)
			{
				writer.Write("| ");
				for (int i = 0; i < maxLength.Length; ++i)
				{
					// prepare the text to write with padding
					string text = item.SubItems[columnOrder[i]].Text;
					int padding = (maxLength[i] - text.Length) + 1;
					for (int j = 0; j < padding; ++j)
						text += " ";
					// write the text and a pipe
					if (i == (maxLength.Length - 1))
						writer.Write(text + "|\n");
					else
						writer.Write(text + "| ");
				}
			}
		}

		private void exportListInTxt(string fileName, int[] columnOrder)
		{
			//compute the max lenght of texts of each column
			int[] maxLength = new int[this.listView.Columns.Count];
			for (int i = 0; i < maxLength.Length; ++i)
				maxLength[i] = 0;
			foreach (ListViewItem item in this.listView.Items)
			{
				for (int i = 0; i < maxLength.Length; ++i)
				{
					int currentTextLength = item.SubItems[columnOrder[i]].Text.Length;
					if (currentTextLength > maxLength[i])
						maxLength[i] = currentTextLength;
				}
			}

			//compute the header line
			int headerLineLength = 0;
			for (int i = 0; i < maxLength.Length; ++i)
				headerLineLength += maxLength[i] + 3;
			headerLineLength--;
			string headerLine = "+";
			for (int i = 0; i < headerLineLength; ++i)
				headerLine += "-";
			headerLine += "+";

			try
			{
				// open a stream
				StreamWriter writer = new StreamWriter(fileName);

				// header
				writer.WriteLine("                    +==================================+");
				writer.WriteLine("                    | Part List generated by BlueBrick |");
				writer.WriteLine("                    +==================================+");
				writer.WriteLine();
				writer.WriteLine("Author: {0}", Map.Instance.Author);
				writer.WriteLine("LUG/LTC: {0}", Map.Instance.LUG);
				writer.WriteLine("Show: {0}", Map.Instance.Show);
				writer.WriteLine("Date: {0}", Map.Instance.Date.ToLongDateString());
				writer.WriteLine("Comment:\n{0}", Map.Instance.Comment);
				writer.WriteLine();
				writer.WriteLine();
				// the parts
				if (this.useGroupCheckBox.Checked)
				{
					foreach (ListViewGroup group in this.listView.Groups)
					{
						writer.WriteLine("| " + group.Header);
						writer.WriteLine(headerLine);
						exportItemsInTxt(writer, columnOrder, maxLength, group.Items);
						writer.WriteLine(headerLine);
						writer.WriteLine();
					}
				}
				else
				{
					writer.WriteLine(headerLine);
					exportItemsInTxt(writer, columnOrder, maxLength, this.listView.Items);
					writer.WriteLine(headerLine);
				}

				// close the stream
				writer.Close();
			}
			catch
			{
			}
		}

		private void exportItemsInHtml(StreamWriter writer, int[] columnOrder, ListView.ListViewItemCollection itemList)
		{
			foreach (ListViewItem item in itemList)
			{
				writer.WriteLine("<TR>");
				for (int i = 0; i < columnOrder.Length; ++i)
				{
					string text = item.SubItems[columnOrder[i]].Text;
					switch (columnOrder[i])
					{
						case 0: //this is the part
							//special case for the part column, we also add the picture
							string colorNum = item.SubItems[2].Tag as string;
							// check if we have an imageURL or if we need to construct the default image path
							string partNumber = text;
							if (colorNum != string.Empty)
								partNumber += "." + colorNum;
							string imageURL = BrickLibrary.Instance.getImageURL(partNumber);
							if (imageURL == null)
								imageURL = colorNum + "/" + text + ".png";
							// construct the text for the IMG tag
							text = "<IMG WIDTH=100% SRC=\"" + imageURL + "\"><BR>" + text;
							// write the cell
							writer.WriteLine("\t<TD WIDTH=20% ALIGN=\"center\">{0}</TD>", text);
							break;
						case 1: //this is the quantity
							writer.WriteLine("\t<TD WIDTH=6% ALIGN=\"center\">{0}</TD>", text);
							break;
						case 2: //this is the color
							writer.WriteLine("\t<TD WIDTH=12% ALIGN=\"center\">{0}</TD>", text);
							break;
						case 3: //this is the description
							writer.WriteLine("\t<TD WIDTH=62%>{0}</TD>", text);
							break;
						default:
							writer.WriteLine("\t<TD>{0}</TD>", text);
							break;
					}
				}
				writer.WriteLine("</TR>");
			}
		}

		private void exportListInHtml(string fileName, int[] columnOrder)
		{
			try
			{
				// open a stream
				StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8);

				// header
				writer.WriteLine("<HTML>\n<HEAD>\n\t<TITLE>Part List generated by BlueBrick</TITLE>");
				writer.WriteLine("\t<BASE HREF=\"http://media.peeron.com/ldraw/images/\">");
				writer.WriteLine("</HEAD>\n<BODY>");
				writer.WriteLine("<CENTER><H2>Part List generated by BlueBrick</H2></CENTER>");
				writer.WriteLine("<TABLE BORDER=0>");
				writer.WriteLine("\t<TR><TD ALIGN=\"right\"><B>Author:</B></TD><TD>{0}</TD></TR>", Map.Instance.Author);
				writer.WriteLine("\t<TR><TD ALIGN=\"right\"><B>LUG/LTC:</B></TD><TD>{0}</TD></TR>", Map.Instance.LUG);
				writer.WriteLine("\t<TR><TD ALIGN=\"right\"><B>Show:</B></TD><TD>{0}</TD></TR>", Map.Instance.Show);
				writer.WriteLine("\t<TR><TD ALIGN=\"right\"><B>Date:</B></TD><TD>{0}</TD></TR>", Map.Instance.Date.ToLongDateString());
				writer.WriteLine("\t<TR><TD ALIGN=\"right\" VALIGN=\"top\"><B>Comment:</B></TD><TD>{0}</TD></TR>", Map.Instance.Comment.Replace("\n","<BR>"));
				writer.WriteLine("</TABLE>\n<BR>\n<BR>\n<CENTER>");
				// the parts
				if (this.useGroupCheckBox.Checked)
				{
					foreach (ListViewGroup group in this.listView.Groups)
					{
						writer.WriteLine("<TABLE BORDER=1 WIDTH=95% CELLPADDING=10>");
						writer.WriteLine("<TR><TD COLSPAN={0}><B>{1}</B></TD></TR>", columnOrder.Length, group.Header);
						exportItemsInHtml(writer, columnOrder, group.Items);
						writer.WriteLine("</TABLE>");
						writer.WriteLine("<BR>");
					}
				}
				else
				{
					writer.WriteLine("<TABLE BORDER=1 WIDTH=95% CELLPADDING=10>");
					writer.WriteLine("<TR>");
					for (int i = 0; i < columnOrder.Length; ++i)
						writer.WriteLine("\t<TD ALIGN=\"center\"><B>{0}</B></TD>", this.listView.Columns[columnOrder[i]].Text);
					writer.WriteLine("</TR>");
					exportItemsInHtml(writer, columnOrder, this.listView.Items);
					writer.WriteLine("</TABLE>");
				}
				writer.WriteLine("</CENTER></BODY>\n</HTML>");

				// close the stream
				writer.Close();
			}
			catch
			{
			}
		}

		#endregion
		#region form events
		private void PartListForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// the form is never closed, only hidden, call the menu item instead
			mMainFormReference.partListToolStripMenuItem_Click(sender, null);
			e.Cancel = true;
		}

		private void buttonClose_Click(object sender, EventArgs e)
		{
			// the form is never closed, only hidden, call the menu item instead
			mMainFormReference.partListToolStripMenuItem_Click(sender, e);
		}

		private void listView_VisibleChanged(object sender, EventArgs e)
		{
			// rebuild the list if the form becomes visible
			if (this.Visible)
				rebuildList();
		}

		private void useGroupCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			rebuildList();
		}

		private void buttonExport_Click(object sender, EventArgs e)
		{
			// open the save file dialog
			DialogResult result = this.saveFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				// compute an array to store the order of the columns
				int[] columnOrder = new int[this.listView.Columns.Count];
				int columnIndex = 0;
				foreach (ColumnHeader columnHeader in this.listView.Columns)
				{
					columnOrder[columnHeader.DisplayIndex] = columnIndex;
					columnIndex++;
				}
				// call the correct exporter
				if (this.saveFileDialog.FileName.EndsWith(".txt"))
					exportListInTxt(this.saveFileDialog.FileName, columnOrder);
				else
					exportListInHtml(this.saveFileDialog.FileName, columnOrder);
			}
		}
		#endregion
	}
}