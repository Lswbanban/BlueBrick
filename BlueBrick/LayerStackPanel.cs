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
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using BlueBrick.MapData;

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
				Panel panel = container as Panel;

				// early exit if the panel doesn't have control yet, or if the panel is not visible
				if ((panel.Controls.Count == 0) || (!panel.Visible))
					return false;

				// Use ClientRectangle so that panel.Padding is honored.
				// do not use DisplayRectangle because this rectangle is not correctly computed in Mono
				Rectangle panelDisplayRectangle = panel.ClientRectangle;
				Point nextControlLocation = new Point(panelDisplayRectangle.Left, panelDisplayRectangle.Bottom);

				// compute the total height of the child control (a layer) including the margin
				// the height is normally the same for all the controls in the list, so we can just take the first one
				// we know that there's at least one control in the list, because of the early exit
				Control firstLayer = panel.Controls[0];
				int controlTotalWidthMargin = firstLayer.Margin.Left + firstLayer.Margin.Right;
				int controlTotalHeight = firstLayer.Margin.Top + firstLayer.Height + firstLayer.Margin.Bottom;

				// check if we will need the vertical scrollbar
				int displayHeight = panelDisplayRectangle.Height - panel.Margin.Top;
				if (controlTotalHeight * panel.Controls.Count > displayHeight)
				{
					int currentVerticalScrollValue = panel.VerticalScroll != null ? panel.VerticalScroll.Value : 0;
					nextControlLocation.Y += (controlTotalHeight * panel.Controls.Count) - displayHeight - currentVerticalScrollValue;
					controlTotalWidthMargin += System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
				}

				// compute the width for all the controls
				int controlWidth = panel.ClientSize.Width - controlTotalWidthMargin;
				if (panel.Parent != null)
					controlWidth = panel.Parent.ClientSize.Width - controlTotalWidthMargin;

				foreach (Control layer in panel.Controls)
				{
					// Respect the margin of the control:
					// shift over the left and the full height.
					nextControlLocation.Offset(layer.Margin.Left, -controlTotalHeight);

					// Set the location of the control.
					layer.Location = nextControlLocation;
					// resize each layer item at the correct size
					layer.Width = controlWidth;

					// Move X back to the display rectangle origin.
					nextControlLocation.X = panelDisplayRectangle.X;
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
			e.Control.Width = this.ClientSize.Width - 9;
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
