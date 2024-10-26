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
using System.Windows.Forms;
using System.Drawing;
using BlueBrick.MapData;
using BlueBrick.Actions;
using BlueBrick.Actions.Layers;
using BlueBrick.Properties;

namespace BlueBrick
{
	public class LayerPanel : FlowLayoutPanel
	{
		private static Color mSelectedColor = Color.FromKnownColor(KnownColor.ActiveBorder);
		private static Color mUnselectedColor = Color.FromKnownColor(KnownColor.ControlLightLight);

		private Layer mLayerReference = null;
		private Label nameLabel;
		protected PictureBox layerTypePictureBox;
		protected Button displayHullButton;
		private Button visibilityButton;

		#region get/set

		public Layer LayerReference
		{
			get { return mLayerReference; }
		}

		#endregion

		#region method for selection

		/// <summary>
		/// Call this method when you want to change the back color of the panel to make it
		/// look selected or not
		/// </summary>
		public void changeBackColor(bool isSelected)
		{
			// check if the panel to select is not already selected
			if (isSelected)
			{
				this.BackColor = mSelectedColor;
				this.nameLabel.BackColor = mSelectedColor;
			}
			else
			{
				this.BackColor = mUnselectedColor;
				this.nameLabel.BackColor = mUnselectedColor;
			}
		}
		#endregion

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerPanel));
			this.visibilityButton = new System.Windows.Forms.Button();
			this.nameLabel = new System.Windows.Forms.Label();
			this.layerTypePictureBox = new System.Windows.Forms.PictureBox();
			this.displayHullButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.layerTypePictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// visibilityButton
			// 
			this.visibilityButton.Image = ((System.Drawing.Image)(resources.GetObject("visibilityButton.Image")));
			this.visibilityButton.Location = new System.Drawing.Point(23, 3);
			this.visibilityButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.visibilityButton.Name = "visibilityButton";
			this.visibilityButton.Size = new System.Drawing.Size(20, 20);
			this.visibilityButton.TabIndex = 0;
			this.visibilityButton.UseVisualStyleBackColor = true;
			this.visibilityButton.Click += new System.EventHandler(this.visibilityButton_Click);
			// 
			// nameLabel
			// 
			this.nameLabel.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.nameLabel.Location = new System.Drawing.Point(3, 28);
			this.nameLabel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(100, 23);
			this.nameLabel.TabIndex = 0;
			this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.nameLabel.UseMnemonic = false;
			this.nameLabel.Click += new System.EventHandler(this.LayerPanel_Click);
			this.nameLabel.DoubleClick += new System.EventHandler(this.LayerPanel_DoubleClick);
			// 
			// layerTypePictureBox
			// 
			this.layerTypePictureBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.layerTypePictureBox.Location = new System.Drawing.Point(3, 5);
			this.layerTypePictureBox.Margin = new System.Windows.Forms.Padding(3, 5, 0, 0);
			this.layerTypePictureBox.Name = "layerTypePictureBox";
			this.layerTypePictureBox.Size = new System.Drawing.Size(20, 20);
			this.layerTypePictureBox.TabIndex = 0;
			this.layerTypePictureBox.TabStop = false;
			this.layerTypePictureBox.Click += new System.EventHandler(this.LayerPanel_Click);
			this.layerTypePictureBox.DoubleClick += new System.EventHandler(this.LayerPanel_DoubleClick);
			// 
			// displayHullButton
			// 
			this.displayHullButton.Location = new System.Drawing.Point(46, 3);
			this.displayHullButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.displayHullButton.Name = "displayHullButton";
			this.displayHullButton.Size = new System.Drawing.Size(20, 20);
			this.displayHullButton.TabIndex = 0;
			this.displayHullButton.UseVisualStyleBackColor = true;
			this.displayHullButton.Click += new System.EventHandler(this.displayHullButton_Click);
			// 
			// LayerPanel
			// 
			this.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.layerTypePictureBox);
			this.Controls.Add(this.visibilityButton);
			this.Controls.Add(this.displayHullButton);
			this.Controls.Add(this.nameLabel);
			this.Size = new System.Drawing.Size(80, 28);
			this.ClientSizeChanged += new System.EventHandler(this.LayerPanel_ClientSizeChanged);
			this.Click += new System.EventHandler(this.LayerPanel_Click);
			this.DoubleClick += new System.EventHandler(this.LayerPanel_DoubleClick);
			((System.ComponentModel.ISupportInitialize)(this.layerTypePictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		/// <summary>
		/// Default constructor only to make work the Form Designer: should not be used!
		/// </summary>
		protected LayerPanel()
		{
		}

		public LayerPanel(Layer layer)
		{
			InitializeComponent();
			mLayerReference = layer;
			updateView();

			// check if this panel is linked with the selected panel to change the color
			if (Map.Instance.SelectedLayer == layer)
			{
				this.BackColor = mSelectedColor;
				this.nameLabel.BackColor = mSelectedColor;
			}
			else
			{
				this.BackColor = mUnselectedColor;
				this.nameLabel.BackColor = mUnselectedColor;
			}
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

		/// <summary>
		/// Update the view of this panel according to the referenced panel data
		/// </summary>
		public void updateView()
		{
			// change the name of the layer
			this.nameLabel.Text = mLayerReference.Name;
			// change the visible button
			if (mLayerReference.Visible)
			{
				System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayerPanel));
				this.visibilityButton.Image = ((System.Drawing.Image)(resources.GetObject("visibilityButton.Image")));
			}
			else
			{
				this.visibilityButton.Image = null;
			}
			// change the display hull button
			if (mLayerReference.DisplayHulls)
				this.displayHullButton.Image = Resources.showHullIcon;
			else
				this.displayHullButton.Image = Resources.hideHullIcon;

			// change the back color if I am selected
			changeBackColor(mLayerReference == Map.Instance.SelectedLayer);
		}

		private void LayerPanel_ClientSizeChanged(object sender, EventArgs e)
		{
			int newLabelWidth = this.Width - visibilityButton.Width - layerTypePictureBox.Width - 20;
			if (displayHullButton.Visible)
				newLabelWidth -= displayHullButton.Width - displayHullButton.Margin.Left - displayHullButton.Margin.Right;
			this.nameLabel.Width = newLabelWidth;
		}

		private void visibilityButton_Click(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			this.Focus();
			// change the visibility
			if (mLayerReference.Visible)
				ActionManager.Instance.doAction(new HideLayer(mLayerReference));
			else
				ActionManager.Instance.doAction(new ShowLayer(mLayerReference));
		}

		private void displayHullButton_Click(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			this.Focus();

			// create a copy of the edited layer to hold the old data (the layer can be on any type, we just want to copy the options)
			LayerText oldLayerData = new LayerText();
			oldLayerData.CopyOptionsFrom(mLayerReference);

			// create a new layer to store the new data, and reverse the display hull flag in the new data
			LayerText newLayerData = new LayerText();
			newLayerData.CopyOptionsFrom(mLayerReference);
			newLayerData.DisplayHulls = !mLayerReference.DisplayHulls;

			// do a change option action
			ActionManager.Instance.doAction(new ChangeLayerOption(mLayerReference, oldLayerData, newLayerData));
		}

		private void LayerPanel_DoubleClick(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			this.Focus();
			// check the type of the layer for option edition
			if (this.GetType().Name == "LayerGridPanel")
			{
				LayerGridOptionForm optionForm = new LayerGridOptionForm(this.mLayerReference as LayerGrid);
				optionForm.ShowDialog(MainForm.Instance);
			}
			else if (this.GetType().Name == "LayerBrickPanel")
			{
				LayerBrickOptionForm optionForm = new LayerBrickOptionForm(this.mLayerReference as LayerBrick);
				optionForm.ShowDialog(MainForm.Instance);
			}
			else if (this.GetType().Name == "LayerTextPanel")
			{
				LayerTextOptionForm optionForm = new LayerTextOptionForm(this.mLayerReference as LayerText);
				optionForm.ShowDialog(MainForm.Instance);
			}
			else if (this.GetType().Name == "LayerAreaPanel")
			{
				LayerAreaOptionForm optionForm = new LayerAreaOptionForm(this.mLayerReference as LayerArea);
				optionForm.ShowDialog(MainForm.Instance);
			}
			else if (this.GetType().Name == "LayerRulerPanel")
			{
				LayerTextOptionForm optionForm = new LayerTextOptionForm(this.mLayerReference as LayerRuler);
				optionForm.ShowDialog(MainForm.Instance);
			}
		}

		private void LayerPanel_Click(object sender, EventArgs e)
		{
			// take the focus anyway if we click the panel
			this.Focus();
			// and select this panel if not already done to avoid adding useless action in the stack
			if (Map.Instance.SelectedLayer != this.mLayerReference)
				ActionManager.Instance.doAction(new SelectLayer(this.mLayerReference));
		}
	}
}
