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
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using BlueBrick.MapData;
using BlueBrick.Actions;

namespace BlueBrick
{
	public class LayerStackPanel : Panel
	{
		#region layout engine
		/// <summary>
		/// We must define a specific layout engine to display the layers from bottom to up
		/// </summary>
		private class BottomUpFlowLayoutEngine : LayoutEngine
		{
			public override bool Layout(object container, LayoutEventArgs layoutEventArgs)
			{
				Control parent = container as Control;

				// Use ClientRectangle so that parent.Padding is honored.
				// do not use DisplayRectangle because this rectangle is not correctly computed in Mono
				Rectangle parentDisplayRectangle = parent.ClientRectangle;
				Point nextControlLocation = new Point(parentDisplayRectangle.Left, parentDisplayRectangle.Bottom);

				// compute the total height of the child control (a layer) including the margin
				// the height is normally the same for all the controls in the list, so we can just take the first one
				int controlTotalHeight = 0;
				int controlTotalWidthMargin = 6;
				if (parent.Controls.Count > 0)
				{
					Control control = parent.Controls[0];
					controlTotalWidthMargin = control.Margin.Left + control.Margin.Right;
					controlTotalHeight = control.Margin.Top + control.Height + control.Margin.Bottom;
				}

				// check if we will need the vertical scrollbar
				int displayHeight = parentDisplayRectangle.Height - 3;
				if (controlTotalHeight * parent.Controls.Count > displayHeight)
				{
					nextControlLocation.Y += (controlTotalHeight * parent.Controls.Count) - displayHeight;
					controlTotalWidthMargin += 18;
				}

				// compute the width for all the controls
				int controlWidth = parent.ClientSize.Width - controlTotalWidthMargin;
				if (parent.Parent != null)
					controlWidth = parent.Parent.ClientSize.Width - controlTotalWidthMargin;

				foreach (Control control in parent.Controls)
				{
					// Only apply layout to visible controls.
					if (!control.Visible)
					{
						continue; // this should never happen
					}

					// Respect the margin of the control:
					// shift over the left and the full height.
					nextControlLocation.Offset(control.Margin.Left, -controlTotalHeight);

					// Set the location of the control.
					control.Location = nextControlLocation;
					// resize each layer item at the correct size
					control.Width = controlWidth;

					// Move X back to the display rectangle origin.
					nextControlLocation.X = parentDisplayRectangle.X;
				}

				// Optional: Return whether or not the container's 
				// parent should perform layout as a result of this 
				// layout. Some layout engines return the value of 
				// the container's AutoSize property.

				return true;
			}
		}
		#endregion

		#region Fields
		// the layout engine for this specific panel
		private BottomUpFlowLayoutEngine mLayoutEngine = new BottomUpFlowLayoutEngine();
		#endregion

		#region get/set
		/// <summary>
		/// The layer stack panel must override its layout engine to display a bottom to up flow of
		/// all its children.
		/// </summary>
		public override LayoutEngine LayoutEngine
		{
			get	{ return mLayoutEngine;	}
		}
		#endregion

		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// LayerStackPanel
			// 
			this.AutoScroll = true;
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Margin = new System.Windows.Forms.Padding(0);
			this.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.LayerStackPanel_ControlAdded);
			this.ResumeLayout(false);

		}

		public LayerStackPanel()
		{
			InitializeComponent();
		}

		/// <summary>
		/// This method is inheritated and is usefull to get the event when the arrow are pressed
		/// </summary>
		/// <param name="keyData"></param>
		/// <returns></returns>
		protected override bool IsInputKey(Keys keyData)
		{
			// we need the four arrow keys
			// also page up and down for rotation
			// and delete and backspace for deleting object
			if ((keyData == Keys.Left) || (keyData == Keys.Right) ||
				(keyData == Keys.Up) || (keyData == Keys.Down) ||
				(keyData == Keys.PageDown) || (keyData == Keys.PageUp) ||
				(keyData == Keys.Home) || (keyData == Keys.End) ||
				(keyData == Keys.Insert) || (keyData == Keys.Delete) ||
				(keyData == Keys.Enter) || (keyData == Keys.Return) ||
				(keyData == Keys.Escape) || (keyData == Keys.Back))
				return true;

			return base.IsInputKey(keyData);
		}
		
		public void updateView(Actions.Action.UpdateViewType layerUpdateType)
		{
			// check if we must recreate the whole list
            if (layerUpdateType == Actions.Action.UpdateViewType.FULL)
			{
				// first suspend the layout because we will recreate the full layout
				this.SuspendLayout();
				this.Visible = false;
				// clear the layer stack and refill it with the map layer list
				this.Controls.Clear();
				// recreate all the layer from the map
				// we create a new panel to handle this layer
				foreach (Layer layer in Map.Instance.LayerList)
				{
					// instanciate a new layer panel according to the layer type
					LayerPanel newLayerPanel = null;
					if (layer is LayerGrid)
						newLayerPanel = new LayerGridPanel(layer);
					else if (layer is LayerText)
						newLayerPanel = new LayerTextPanel(layer);
					else if (layer is LayerArea)
						newLayerPanel = new LayerAreaPanel(layer);
					else if (layer is LayerBrick)
						newLayerPanel = new LayerBrickPanel(layer);
					else if (layer is LayerRuler)
						newLayerPanel = new LayerRulerPanel(layer);

					// add the new layer in the control list
					this.Controls.Add(newLayerPanel);

					// if the selected layer of the map is the current one,
					// use the last added layerPanel as the selected layer panel
					if (Map.Instance.SelectedLayer == layer)
						newLayerPanel.changeBackColor(true);
				}

				// now we can resume the layout
				this.ResumeLayout(true);
				this.Visible = true;
			}
			else
			{
				// not a full update, just check the property of the layers,
				// but the layout is unchanged
				foreach (Control control in this.Controls)
				{
					LayerPanel layerPanel = control as LayerPanel;
					layerPanel.updateView();
				}
			}

			// invalidate the PanelView because we recreated the whole layer panel list
			this.Invalidate();
		}

		private void LayerStackPanel_ControlAdded(object sender, ControlEventArgs e)
		{
			// resize the added control
			e.Control.Width = this.ClientSize.Width - 6;
			// there's a bug in Mono, the scrollbar doesn't appear if the number of controls is more
			// than this panel size. Touching the size will actually force the recomputation of the 
			// need for the vertical scroll bar but since anyway this panel is docked it will not 
			// actually change the size in the long term.
			this.Height += 1;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			// give focus to me so that user can scroll with mouse, just by moving the mouse above the part list view
			this.Focus();
		}
	}
}
