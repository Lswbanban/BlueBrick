namespace BlueBrick
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainSplitContainer = new BlueBrick.MainSplitContainer();
            this.mapPanel = new BlueBrick.MapPanel();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusBarProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusBarLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolSplitContainer = new System.Windows.Forms.SplitContainer();
            this.partsTabControl = new BlueBrick.PartLibraryPanel();
            this.layerSplitContainer = new System.Windows.Forms.SplitContainer();
            this.layerStackPanel = new BlueBrick.LayerStackPanel();
            this.LayerButtonFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.trashLayerButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.newLayerBrickButton = new System.Windows.Forms.Button();
            this.newLayerAreaButton = new System.Windows.Forms.Button();
            this.newLayerGridButton = new System.Windows.Forms.Button();
            this.newLayerTextButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.layerUpButton = new System.Windows.Forms.Button();
            this.layerDownButton = new System.Windows.Forms.Button();
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRecentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsPictureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.reloadPartLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.findAndReplaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deselectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupMenuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ungroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.moveStepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveStepDisabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveStep32ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveStep16ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveStep8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveStep4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveStep1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveStep05ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotationStepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotationStep90ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotationStep45ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotationStep22ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotationStep1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateCWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rotateCCWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paintToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paintToolPaintToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paintToolEraseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.paintToolChooseColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.generalInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mapBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentLayerOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolbarMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBarMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.electricCircuitsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.partListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpContentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutBlueBrickToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.toolBarNewButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarOpenButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarSaveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolBarUndoButton = new System.Windows.Forms.ToolStripSplitButton();
            this.toolBarRedoButton = new System.Windows.Forms.ToolStripSplitButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolBarDeleteButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarCutButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarCopyButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarPasteButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolBarSnapGridButton = new System.Windows.Forms.ToolStripSplitButton();
            this.toolBarGrid32Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarGrid16Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarGrid8Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarGrid4Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarGrid1Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarGrid05Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarRotationAngleButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolBarAngle90Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarAngle45Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarAngle22Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarAngle1Button = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBarRotateCCWButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarRotateCWButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarSendToBackButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarBringToFrontButton = new System.Windows.Forms.ToolStripButton();
            this.toolBarPaintButton = new System.Windows.Forms.ToolStripSplitButton();
            this.paintToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eraseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveExportImageDialog = new System.Windows.Forms.SaveFileDialog();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.toolTipForMainForm = new System.Windows.Forms.ToolTip(this.components);
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.mapPanel.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.toolSplitContainer.Panel1.SuspendLayout();
            this.toolSplitContainer.Panel2.SuspendLayout();
            this.toolSplitContainer.SuspendLayout();
            this.layerSplitContainer.Panel1.SuspendLayout();
            this.layerSplitContainer.Panel2.SuspendLayout();
            this.layerSplitContainer.SuspendLayout();
            this.LayerButtonFlowLayoutPanel.SuspendLayout();
            this.menuBar.SuspendLayout();
            this.toolBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.mainSplitContainer, "mainSplitContainer");
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.mapPanel);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.toolSplitContainer);
            // 
            // mapPanel
            // 
            this.mapPanel.BackColor = System.Drawing.Color.CornflowerBlue;
            this.mapPanel.Controls.Add(this.statusBar);
            resources.ApplyResources(this.mapPanel, "mapPanel");
            this.mapPanel.Name = "mapPanel";
            this.mapPanel.ViewScale = 1;
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.SystemColors.Control;
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarProgressBar,
            this.statusBarLabel});
            resources.ApplyResources(this.statusBar, "statusBar");
            this.statusBar.Name = "statusBar";
            this.statusBar.SizingGrip = false;
            // 
            // statusBarProgressBar
            // 
            resources.ApplyResources(this.statusBarProgressBar, "statusBarProgressBar");
            this.statusBarProgressBar.Name = "statusBarProgressBar";
            // 
            // statusBarLabel
            // 
            this.statusBarLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.statusBarLabel.Name = "statusBarLabel";
            resources.ApplyResources(this.statusBarLabel, "statusBarLabel");
            // 
            // toolSplitContainer
            // 
            this.toolSplitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            resources.ApplyResources(this.toolSplitContainer, "toolSplitContainer");
            this.toolSplitContainer.Name = "toolSplitContainer";
            // 
            // toolSplitContainer.Panel1
            // 
            this.toolSplitContainer.Panel1.Controls.Add(this.partsTabControl);
            // 
            // toolSplitContainer.Panel2
            // 
            this.toolSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.toolSplitContainer.Panel2.Controls.Add(this.layerSplitContainer);
            // 
            // partsTabControl
            // 
            resources.ApplyResources(this.partsTabControl, "partsTabControl");
            this.partsTabControl.Name = "partsTabControl";
            this.partsTabControl.SelectedIndex = 0;
            // 
            // layerSplitContainer
            // 
            resources.ApplyResources(this.layerSplitContainer, "layerSplitContainer");
            this.layerSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.layerSplitContainer.Name = "layerSplitContainer";
            // 
            // layerSplitContainer.Panel1
            // 
            this.layerSplitContainer.Panel1.Controls.Add(this.layerStackPanel);
            // 
            // layerSplitContainer.Panel2
            // 
            this.layerSplitContainer.Panel2.Controls.Add(this.LayerButtonFlowLayoutPanel);
            // 
            // layerStackPanel
            // 
            resources.ApplyResources(this.layerStackPanel, "layerStackPanel");
            this.layerStackPanel.BackColor = System.Drawing.SystemColors.Control;
            this.layerStackPanel.Name = "layerStackPanel";
            // 
            // LayerButtonFlowLayoutPanel
            // 
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.trashLayerButton);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.label2);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.newLayerBrickButton);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.newLayerAreaButton);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.newLayerGridButton);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.newLayerTextButton);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.label1);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.layerUpButton);
            this.LayerButtonFlowLayoutPanel.Controls.Add(this.layerDownButton);
            resources.ApplyResources(this.LayerButtonFlowLayoutPanel, "LayerButtonFlowLayoutPanel");
            this.LayerButtonFlowLayoutPanel.Name = "LayerButtonFlowLayoutPanel";
            // 
            // trashLayerButton
            // 
            this.trashLayerButton.BackColor = System.Drawing.SystemColors.Control;
            this.trashLayerButton.FlatAppearance.BorderSize = 0;
            this.trashLayerButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonShadow;
            resources.ApplyResources(this.trashLayerButton, "trashLayerButton");
            this.trashLayerButton.Name = "trashLayerButton";
            this.toolTipForMainForm.SetToolTip(this.trashLayerButton, resources.GetString("trashLayerButton.ToolTip"));
            this.trashLayerButton.UseVisualStyleBackColor = false;
            this.trashLayerButton.Click += new System.EventHandler(this.trashLayerButton_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // newLayerBrickButton
            // 
            this.newLayerBrickButton.FlatAppearance.BorderSize = 0;
            this.newLayerBrickButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonShadow;
            resources.ApplyResources(this.newLayerBrickButton, "newLayerBrickButton");
            this.newLayerBrickButton.Name = "newLayerBrickButton";
            this.toolTipForMainForm.SetToolTip(this.newLayerBrickButton, resources.GetString("newLayerBrickButton.ToolTip"));
            this.newLayerBrickButton.UseVisualStyleBackColor = true;
            this.newLayerBrickButton.Click += new System.EventHandler(this.newLayerBrickButton_Click);
            // 
            // newLayerAreaButton
            // 
            this.newLayerAreaButton.FlatAppearance.BorderSize = 0;
            this.newLayerAreaButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonShadow;
            resources.ApplyResources(this.newLayerAreaButton, "newLayerAreaButton");
            this.newLayerAreaButton.Name = "newLayerAreaButton";
            this.toolTipForMainForm.SetToolTip(this.newLayerAreaButton, resources.GetString("newLayerAreaButton.ToolTip"));
            this.newLayerAreaButton.UseVisualStyleBackColor = true;
            this.newLayerAreaButton.Click += new System.EventHandler(this.newLayerAreaButton_Click);
            // 
            // newLayerGridButton
            // 
            this.newLayerGridButton.FlatAppearance.BorderSize = 0;
            this.newLayerGridButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonShadow;
            resources.ApplyResources(this.newLayerGridButton, "newLayerGridButton");
            this.newLayerGridButton.Name = "newLayerGridButton";
            this.toolTipForMainForm.SetToolTip(this.newLayerGridButton, resources.GetString("newLayerGridButton.ToolTip"));
            this.newLayerGridButton.UseVisualStyleBackColor = true;
            this.newLayerGridButton.Click += new System.EventHandler(this.newLayerGridButton_Click);
            // 
            // newLayerTextButton
            // 
            this.newLayerTextButton.FlatAppearance.BorderSize = 0;
            this.newLayerTextButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonShadow;
            resources.ApplyResources(this.newLayerTextButton, "newLayerTextButton");
            this.newLayerTextButton.Name = "newLayerTextButton";
            this.toolTipForMainForm.SetToolTip(this.newLayerTextButton, resources.GetString("newLayerTextButton.ToolTip"));
            this.newLayerTextButton.UseVisualStyleBackColor = true;
            this.newLayerTextButton.Click += new System.EventHandler(this.newLayerTextButton_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // layerUpButton
            // 
            this.layerUpButton.FlatAppearance.BorderSize = 0;
            this.layerUpButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonShadow;
            resources.ApplyResources(this.layerUpButton, "layerUpButton");
            this.layerUpButton.Name = "layerUpButton";
            this.toolTipForMainForm.SetToolTip(this.layerUpButton, resources.GetString("layerUpButton.ToolTip"));
            this.layerUpButton.UseVisualStyleBackColor = true;
            this.layerUpButton.Click += new System.EventHandler(this.layerUpButton_Click);
            // 
            // layerDownButton
            // 
            this.layerDownButton.FlatAppearance.BorderSize = 0;
            this.layerDownButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ButtonShadow;
            resources.ApplyResources(this.layerDownButton, "layerDownButton");
            this.layerDownButton.Name = "layerDownButton";
            this.toolTipForMainForm.SetToolTip(this.layerDownButton, resources.GetString("layerDownButton.ToolTip"));
            this.layerDownButton.UseVisualStyleBackColor = true;
            this.layerDownButton.Click += new System.EventHandler(this.layerDownButton_Click);
            // 
            // menuBar
            // 
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuBar, "menuBar");
            this.menuBar.Name = "menuBar";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.openRecentToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveToolStripMenuItem,
            this.saveasToolStripMenuItem,
            this.exportAsPictureToolStripMenuItem,
            this.toolStripSeparator2,
            this.reloadPartLibraryToolStripMenuItem,
            this.toolStripSeparator12,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            resources.ApplyResources(this.newToolStripMenuItem, "newToolStripMenuItem");
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            resources.ApplyResources(this.openToolStripMenuItem, "openToolStripMenuItem");
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // openRecentToolStripMenuItem
            // 
            resources.ApplyResources(this.openRecentToolStripMenuItem, "openRecentToolStripMenuItem");
            this.openRecentToolStripMenuItem.Name = "openRecentToolStripMenuItem";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveasToolStripMenuItem
            // 
            this.saveasToolStripMenuItem.Name = "saveasToolStripMenuItem";
            resources.ApplyResources(this.saveasToolStripMenuItem, "saveasToolStripMenuItem");
            this.saveasToolStripMenuItem.Click += new System.EventHandler(this.saveasToolStripMenuItem_Click);
            // 
            // exportAsPictureToolStripMenuItem
            // 
            this.exportAsPictureToolStripMenuItem.Name = "exportAsPictureToolStripMenuItem";
            resources.ApplyResources(this.exportAsPictureToolStripMenuItem, "exportAsPictureToolStripMenuItem");
            this.exportAsPictureToolStripMenuItem.Click += new System.EventHandler(this.exportAsPictureToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // reloadPartLibraryToolStripMenuItem
            // 
            this.reloadPartLibraryToolStripMenuItem.Name = "reloadPartLibraryToolStripMenuItem";
            resources.ApplyResources(this.reloadPartLibraryToolStripMenuItem, "reloadPartLibraryToolStripMenuItem");
            this.reloadPartLibraryToolStripMenuItem.Click += new System.EventHandler(this.reloadPartLibraryToolStripMenuItem_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            resources.ApplyResources(this.toolStripSeparator12, "toolStripSeparator12");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator4,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator5,
            this.findAndReplaceToolStripMenuItem,
            this.toolStripSeparator14,
            this.selectAllToolStripMenuItem,
            this.deselectAllToolStripMenuItem,
            this.selectPathToolStripMenuItem,
            this.groupMenuToolStripMenuItem,
            this.toolStripSeparator8,
            this.moveStepToolStripMenuItem,
            this.rotationStepToolStripMenuItem,
            this.rotateCWToolStripMenuItem,
            this.rotateCCWToolStripMenuItem,
            this.paintToolToolStripMenuItem,
            this.toolStripSeparator10,
            this.generalInformationToolStripMenuItem,
            this.mapBackgroundColorToolStripMenuItem,
            this.currentLayerOptionsToolStripMenuItem,
            this.preferencesMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            // 
            // undoToolStripMenuItem
            // 
            resources.ApplyResources(this.undoToolStripMenuItem, "undoToolStripMenuItem");
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.undoToolStripMenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            resources.ApplyResources(this.redoToolStripMenuItem, "redoToolStripMenuItem");
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.redoToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            resources.ApplyResources(this.cutToolStripMenuItem, "cutToolStripMenuItem");
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            resources.ApplyResources(this.pasteToolStripMenuItem, "pasteToolStripMenuItem");
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // findAndReplaceToolStripMenuItem
            // 
            this.findAndReplaceToolStripMenuItem.Name = "findAndReplaceToolStripMenuItem";
            resources.ApplyResources(this.findAndReplaceToolStripMenuItem, "findAndReplaceToolStripMenuItem");
            this.findAndReplaceToolStripMenuItem.Click += new System.EventHandler(this.findAndReplaceToolStripMenuItem_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            resources.ApplyResources(this.toolStripSeparator14, "toolStripSeparator14");
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            resources.ApplyResources(this.selectAllToolStripMenuItem, "selectAllToolStripMenuItem");
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // deselectAllToolStripMenuItem
            // 
            this.deselectAllToolStripMenuItem.Name = "deselectAllToolStripMenuItem";
            resources.ApplyResources(this.deselectAllToolStripMenuItem, "deselectAllToolStripMenuItem");
            this.deselectAllToolStripMenuItem.Click += new System.EventHandler(this.deselectAllToolStripMenuItem_Click);
            // 
            // selectPathToolStripMenuItem
            // 
            this.selectPathToolStripMenuItem.Name = "selectPathToolStripMenuItem";
            resources.ApplyResources(this.selectPathToolStripMenuItem, "selectPathToolStripMenuItem");
            this.selectPathToolStripMenuItem.Click += new System.EventHandler(this.selectPathToolStripMenuItem_Click);
            // 
            // groupMenuToolStripMenuItem
            // 
            this.groupMenuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.groupToolStripMenuItem,
            this.ungroupToolStripMenuItem});
            this.groupMenuToolStripMenuItem.Name = "groupMenuToolStripMenuItem";
            resources.ApplyResources(this.groupMenuToolStripMenuItem, "groupMenuToolStripMenuItem");
            // 
            // groupToolStripMenuItem
            // 
            this.groupToolStripMenuItem.Name = "groupToolStripMenuItem";
            resources.ApplyResources(this.groupToolStripMenuItem, "groupToolStripMenuItem");
            this.groupToolStripMenuItem.Click += new System.EventHandler(this.groupToolStripMenuItem_Click);
            // 
            // ungroupToolStripMenuItem
            // 
            this.ungroupToolStripMenuItem.Name = "ungroupToolStripMenuItem";
            resources.ApplyResources(this.ungroupToolStripMenuItem, "ungroupToolStripMenuItem");
            this.ungroupToolStripMenuItem.Click += new System.EventHandler(this.ungroupToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            resources.ApplyResources(this.toolStripSeparator8, "toolStripSeparator8");
            // 
            // moveStepToolStripMenuItem
            // 
            this.moveStepToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moveStepDisabledToolStripMenuItem,
            this.moveStep32ToolStripMenuItem,
            this.moveStep16ToolStripMenuItem,
            this.moveStep8ToolStripMenuItem,
            this.moveStep4ToolStripMenuItem,
            this.moveStep1ToolStripMenuItem,
            this.moveStep05ToolStripMenuItem});
            this.moveStepToolStripMenuItem.Name = "moveStepToolStripMenuItem";
            resources.ApplyResources(this.moveStepToolStripMenuItem, "moveStepToolStripMenuItem");
            // 
            // moveStepDisabledToolStripMenuItem
            // 
            this.moveStepDisabledToolStripMenuItem.Name = "moveStepDisabledToolStripMenuItem";
            resources.ApplyResources(this.moveStepDisabledToolStripMenuItem, "moveStepDisabledToolStripMenuItem");
            this.moveStepDisabledToolStripMenuItem.Click += new System.EventHandler(this.moveStepDisabledToolStripMenuItem_Click);
            // 
            // moveStep32ToolStripMenuItem
            // 
            this.moveStep32ToolStripMenuItem.Checked = true;
            this.moveStep32ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.moveStep32ToolStripMenuItem.Name = "moveStep32ToolStripMenuItem";
            resources.ApplyResources(this.moveStep32ToolStripMenuItem, "moveStep32ToolStripMenuItem");
            this.moveStep32ToolStripMenuItem.Click += new System.EventHandler(this.moveStep32ToolStripMenuItem_Click);
            // 
            // moveStep16ToolStripMenuItem
            // 
            this.moveStep16ToolStripMenuItem.Name = "moveStep16ToolStripMenuItem";
            resources.ApplyResources(this.moveStep16ToolStripMenuItem, "moveStep16ToolStripMenuItem");
            this.moveStep16ToolStripMenuItem.Click += new System.EventHandler(this.moveStep16ToolStripMenuItem_Click);
            // 
            // moveStep8ToolStripMenuItem
            // 
            this.moveStep8ToolStripMenuItem.Name = "moveStep8ToolStripMenuItem";
            resources.ApplyResources(this.moveStep8ToolStripMenuItem, "moveStep8ToolStripMenuItem");
            this.moveStep8ToolStripMenuItem.Click += new System.EventHandler(this.moveStep8ToolStripMenuItem_Click);
            // 
            // moveStep4ToolStripMenuItem
            // 
            this.moveStep4ToolStripMenuItem.Name = "moveStep4ToolStripMenuItem";
            resources.ApplyResources(this.moveStep4ToolStripMenuItem, "moveStep4ToolStripMenuItem");
            this.moveStep4ToolStripMenuItem.Click += new System.EventHandler(this.moveStep4ToolStripMenuItem_Click);
            // 
            // moveStep1ToolStripMenuItem
            // 
            this.moveStep1ToolStripMenuItem.Name = "moveStep1ToolStripMenuItem";
            resources.ApplyResources(this.moveStep1ToolStripMenuItem, "moveStep1ToolStripMenuItem");
            this.moveStep1ToolStripMenuItem.Click += new System.EventHandler(this.moveStep1ToolStripMenuItem_Click);
            // 
            // moveStep05ToolStripMenuItem
            // 
            this.moveStep05ToolStripMenuItem.Name = "moveStep05ToolStripMenuItem";
            resources.ApplyResources(this.moveStep05ToolStripMenuItem, "moveStep05ToolStripMenuItem");
            this.moveStep05ToolStripMenuItem.Click += new System.EventHandler(this.moveStep05ToolStripMenuItem_Click);
            // 
            // rotationStepToolStripMenuItem
            // 
            this.rotationStepToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rotationStep90ToolStripMenuItem,
            this.rotationStep45ToolStripMenuItem,
            this.rotationStep22ToolStripMenuItem,
            this.rotationStep1ToolStripMenuItem});
            this.rotationStepToolStripMenuItem.Name = "rotationStepToolStripMenuItem";
            resources.ApplyResources(this.rotationStepToolStripMenuItem, "rotationStepToolStripMenuItem");
            // 
            // rotationStep90ToolStripMenuItem
            // 
            this.rotationStep90ToolStripMenuItem.Checked = true;
            this.rotationStep90ToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rotationStep90ToolStripMenuItem.Name = "rotationStep90ToolStripMenuItem";
            resources.ApplyResources(this.rotationStep90ToolStripMenuItem, "rotationStep90ToolStripMenuItem");
            this.rotationStep90ToolStripMenuItem.Click += new System.EventHandler(this.rotationStep90ToolStripMenuItem_Click);
            // 
            // rotationStep45ToolStripMenuItem
            // 
            this.rotationStep45ToolStripMenuItem.Name = "rotationStep45ToolStripMenuItem";
            resources.ApplyResources(this.rotationStep45ToolStripMenuItem, "rotationStep45ToolStripMenuItem");
            this.rotationStep45ToolStripMenuItem.Click += new System.EventHandler(this.rotationStep45ToolStripMenuItem_Click);
            // 
            // rotationStep22ToolStripMenuItem
            // 
            this.rotationStep22ToolStripMenuItem.Name = "rotationStep22ToolStripMenuItem";
            resources.ApplyResources(this.rotationStep22ToolStripMenuItem, "rotationStep22ToolStripMenuItem");
            this.rotationStep22ToolStripMenuItem.Click += new System.EventHandler(this.rotationStep22ToolStripMenuItem_Click);
            // 
            // rotationStep1ToolStripMenuItem
            // 
            this.rotationStep1ToolStripMenuItem.Name = "rotationStep1ToolStripMenuItem";
            resources.ApplyResources(this.rotationStep1ToolStripMenuItem, "rotationStep1ToolStripMenuItem");
            this.rotationStep1ToolStripMenuItem.Click += new System.EventHandler(this.rotationStep1ToolStripMenuItem_Click);
            // 
            // rotateCWToolStripMenuItem
            // 
            this.rotateCWToolStripMenuItem.Name = "rotateCWToolStripMenuItem";
            resources.ApplyResources(this.rotateCWToolStripMenuItem, "rotateCWToolStripMenuItem");
            this.rotateCWToolStripMenuItem.Click += new System.EventHandler(this.rotateCWToolStripMenuItem_Click);
            // 
            // rotateCCWToolStripMenuItem
            // 
            this.rotateCCWToolStripMenuItem.Name = "rotateCCWToolStripMenuItem";
            resources.ApplyResources(this.rotateCCWToolStripMenuItem, "rotateCCWToolStripMenuItem");
            this.rotateCCWToolStripMenuItem.Click += new System.EventHandler(this.rotateCCWToolStripMenuItem_Click);
            // 
            // paintToolToolStripMenuItem
            // 
            this.paintToolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.paintToolPaintToolStripMenuItem,
            this.paintToolEraseToolStripMenuItem,
            this.toolStripSeparator11,
            this.paintToolChooseColorToolStripMenuItem});
            this.paintToolToolStripMenuItem.Name = "paintToolToolStripMenuItem";
            resources.ApplyResources(this.paintToolToolStripMenuItem, "paintToolToolStripMenuItem");
            // 
            // paintToolPaintToolStripMenuItem
            // 
            this.paintToolPaintToolStripMenuItem.Checked = true;
            this.paintToolPaintToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.paintToolPaintToolStripMenuItem.Name = "paintToolPaintToolStripMenuItem";
            resources.ApplyResources(this.paintToolPaintToolStripMenuItem, "paintToolPaintToolStripMenuItem");
            this.paintToolPaintToolStripMenuItem.Click += new System.EventHandler(this.paintToolPaintToolStripMenuItem_Click);
            // 
            // paintToolEraseToolStripMenuItem
            // 
            this.paintToolEraseToolStripMenuItem.Name = "paintToolEraseToolStripMenuItem";
            resources.ApplyResources(this.paintToolEraseToolStripMenuItem, "paintToolEraseToolStripMenuItem");
            this.paintToolEraseToolStripMenuItem.Click += new System.EventHandler(this.paintToolEraseToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            resources.ApplyResources(this.toolStripSeparator11, "toolStripSeparator11");
            // 
            // paintToolChooseColorToolStripMenuItem
            // 
            this.paintToolChooseColorToolStripMenuItem.Name = "paintToolChooseColorToolStripMenuItem";
            resources.ApplyResources(this.paintToolChooseColorToolStripMenuItem, "paintToolChooseColorToolStripMenuItem");
            this.paintToolChooseColorToolStripMenuItem.Click += new System.EventHandler(this.paintToolChooseColorToolStripMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            resources.ApplyResources(this.toolStripSeparator10, "toolStripSeparator10");
            // 
            // generalInformationToolStripMenuItem
            // 
            this.generalInformationToolStripMenuItem.Name = "generalInformationToolStripMenuItem";
            resources.ApplyResources(this.generalInformationToolStripMenuItem, "generalInformationToolStripMenuItem");
            this.generalInformationToolStripMenuItem.Click += new System.EventHandler(this.generalInformationToolStripMenuItem_Click);
            // 
            // mapBackgroundColorToolStripMenuItem
            // 
            this.mapBackgroundColorToolStripMenuItem.Name = "mapBackgroundColorToolStripMenuItem";
            resources.ApplyResources(this.mapBackgroundColorToolStripMenuItem, "mapBackgroundColorToolStripMenuItem");
            this.mapBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.mapBackgroundColorToolStripMenuItem_Click);
            // 
            // currentLayerOptionsToolStripMenuItem
            // 
            this.currentLayerOptionsToolStripMenuItem.Name = "currentLayerOptionsToolStripMenuItem";
            resources.ApplyResources(this.currentLayerOptionsToolStripMenuItem, "currentLayerOptionsToolStripMenuItem");
            this.currentLayerOptionsToolStripMenuItem.Click += new System.EventHandler(this.currentLayerOptionsToolStripMenuItem_Click);
            // 
            // preferencesMenuItem
            // 
            this.preferencesMenuItem.Name = "preferencesMenuItem";
            resources.ApplyResources(this.preferencesMenuItem, "preferencesMenuItem");
            this.preferencesMenuItem.Click += new System.EventHandler(this.preferencesMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolbarMenuItem,
            this.statusBarMenuItem,
            this.electricCircuitsMenuItem,
            this.partListToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            resources.ApplyResources(this.viewToolStripMenuItem, "viewToolStripMenuItem");
            // 
            // toolbarMenuItem
            // 
            this.toolbarMenuItem.Checked = true;
            this.toolbarMenuItem.CheckOnClick = true;
            this.toolbarMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolbarMenuItem.Name = "toolbarMenuItem";
            resources.ApplyResources(this.toolbarMenuItem, "toolbarMenuItem");
            this.toolbarMenuItem.Click += new System.EventHandler(this.toolbarToolStripMenuItem_Click);
            // 
            // statusBarMenuItem
            // 
            this.statusBarMenuItem.Checked = true;
            this.statusBarMenuItem.CheckOnClick = true;
            this.statusBarMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.statusBarMenuItem.Name = "statusBarMenuItem";
            resources.ApplyResources(this.statusBarMenuItem, "statusBarMenuItem");
            this.statusBarMenuItem.Click += new System.EventHandler(this.statusBarToolStripMenuItem_Click);
            // 
            // electricCircuitsMenuItem
            // 
            this.electricCircuitsMenuItem.CheckOnClick = true;
            this.electricCircuitsMenuItem.Name = "electricCircuitsMenuItem";
            resources.ApplyResources(this.electricCircuitsMenuItem, "electricCircuitsMenuItem");
            this.electricCircuitsMenuItem.Click += new System.EventHandler(this.electricCircuitsMenuItem_Click);
            // 
            // partListToolStripMenuItem
            // 
            this.partListToolStripMenuItem.Name = "partListToolStripMenuItem";
            resources.ApplyResources(this.partListToolStripMenuItem, "partListToolStripMenuItem");
            this.partListToolStripMenuItem.Click += new System.EventHandler(this.partListToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpContentsToolStripMenuItem,
            this.aboutBlueBrickToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // helpContentsToolStripMenuItem
            // 
            this.helpContentsToolStripMenuItem.Name = "helpContentsToolStripMenuItem";
            resources.ApplyResources(this.helpContentsToolStripMenuItem, "helpContentsToolStripMenuItem");
            this.helpContentsToolStripMenuItem.Click += new System.EventHandler(this.helpContentsToolStripMenuItem_Click);
            // 
            // aboutBlueBrickToolStripMenuItem
            // 
            this.aboutBlueBrickToolStripMenuItem.Name = "aboutBlueBrickToolStripMenuItem";
            resources.ApplyResources(this.aboutBlueBrickToolStripMenuItem, "aboutBlueBrickToolStripMenuItem");
            this.aboutBlueBrickToolStripMenuItem.Click += new System.EventHandler(this.aboutBlueBrickToolStripMenuItem_Click);
            // 
            // toolBar
            // 
            this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolBarNewButton,
            this.toolBarOpenButton,
            this.toolBarSaveButton,
            this.toolStripSeparator7,
            this.toolBarUndoButton,
            this.toolBarRedoButton,
            this.toolStripSeparator3,
            this.toolBarDeleteButton,
            this.toolBarCutButton,
            this.toolBarCopyButton,
            this.toolBarPasteButton,
            this.toolStripSeparator6,
            this.toolBarSnapGridButton,
            this.toolBarRotationAngleButton,
            this.toolBarRotateCCWButton,
            this.toolBarRotateCWButton,
            this.toolBarSendToBackButton,
            this.toolBarBringToFrontButton,
            this.toolBarPaintButton});
            resources.ApplyResources(this.toolBar, "toolBar");
            this.toolBar.Name = "toolBar";
            // 
            // toolBarNewButton
            // 
            this.toolBarNewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarNewButton, "toolBarNewButton");
            this.toolBarNewButton.Name = "toolBarNewButton";
            this.toolBarNewButton.Click += new System.EventHandler(this.toolBarNewButton_Click);
            // 
            // toolBarOpenButton
            // 
            this.toolBarOpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarOpenButton, "toolBarOpenButton");
            this.toolBarOpenButton.Name = "toolBarOpenButton";
            this.toolBarOpenButton.Click += new System.EventHandler(this.toolBarOpenButton_Click);
            // 
            // toolBarSaveButton
            // 
            this.toolBarSaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarSaveButton, "toolBarSaveButton");
            this.toolBarSaveButton.Name = "toolBarSaveButton";
            this.toolBarSaveButton.Click += new System.EventHandler(this.toolBarSaveButton_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // toolBarUndoButton
            // 
            this.toolBarUndoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarUndoButton, "toolBarUndoButton");
            this.toolBarUndoButton.Name = "toolBarUndoButton";
            this.toolBarUndoButton.ButtonClick += new System.EventHandler(this.undoToolStripMenuItem_Click);
            this.toolBarUndoButton.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolBarUndoButton_DropDownItemClicked);
            // 
            // toolBarRedoButton
            // 
            this.toolBarRedoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarRedoButton, "toolBarRedoButton");
            this.toolBarRedoButton.Name = "toolBarRedoButton";
            this.toolBarRedoButton.ButtonClick += new System.EventHandler(this.redoToolStripMenuItem_Click);
            this.toolBarRedoButton.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.toolBarRedoButton_DropDownItemClicked);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // toolBarDeleteButton
            // 
            this.toolBarDeleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarDeleteButton, "toolBarDeleteButton");
            this.toolBarDeleteButton.Name = "toolBarDeleteButton";
            this.toolBarDeleteButton.Click += new System.EventHandler(this.toolBarDeleteButton_Click);
            // 
            // toolBarCutButton
            // 
            this.toolBarCutButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarCutButton, "toolBarCutButton");
            this.toolBarCutButton.Name = "toolBarCutButton";
            this.toolBarCutButton.Click += new System.EventHandler(this.toolBarCutButton_Click);
            // 
            // toolBarCopyButton
            // 
            this.toolBarCopyButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarCopyButton, "toolBarCopyButton");
            this.toolBarCopyButton.Name = "toolBarCopyButton";
            this.toolBarCopyButton.Click += new System.EventHandler(this.toolBarCopyButton_Click);
            // 
            // toolBarPasteButton
            // 
            this.toolBarPasteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarPasteButton, "toolBarPasteButton");
            this.toolBarPasteButton.Name = "toolBarPasteButton";
            this.toolBarPasteButton.Click += new System.EventHandler(this.toolBarPasteButton_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // toolBarSnapGridButton
            // 
            this.toolBarSnapGridButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarSnapGridButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolBarGrid32Button,
            this.toolBarGrid16Button,
            this.toolBarGrid8Button,
            this.toolBarGrid4Button,
            this.toolBarGrid1Button,
            this.toolBarGrid05Button});
            resources.ApplyResources(this.toolBarSnapGridButton, "toolBarSnapGridButton");
            this.toolBarSnapGridButton.Name = "toolBarSnapGridButton";
            this.toolBarSnapGridButton.Click += new System.EventHandler(this.toolBarSnapGridButton_Click);
            // 
            // toolBarGrid32Button
            // 
            this.toolBarGrid32Button.Checked = true;
            this.toolBarGrid32Button.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolBarGrid32Button.Name = "toolBarGrid32Button";
            resources.ApplyResources(this.toolBarGrid32Button, "toolBarGrid32Button");
            this.toolBarGrid32Button.Click += new System.EventHandler(this.toolBarGrid32Button_Click);
            // 
            // toolBarGrid16Button
            // 
            this.toolBarGrid16Button.Name = "toolBarGrid16Button";
            resources.ApplyResources(this.toolBarGrid16Button, "toolBarGrid16Button");
            this.toolBarGrid16Button.Click += new System.EventHandler(this.toolBarGrid16Button_Click);
            // 
            // toolBarGrid8Button
            // 
            this.toolBarGrid8Button.Name = "toolBarGrid8Button";
            resources.ApplyResources(this.toolBarGrid8Button, "toolBarGrid8Button");
            this.toolBarGrid8Button.Click += new System.EventHandler(this.toolBarGrid8Button_Click);
            // 
            // toolBarGrid4Button
            // 
            this.toolBarGrid4Button.Name = "toolBarGrid4Button";
            resources.ApplyResources(this.toolBarGrid4Button, "toolBarGrid4Button");
            this.toolBarGrid4Button.Click += new System.EventHandler(this.toolBarGrid4Button_Click);
            // 
            // toolBarGrid1Button
            // 
            this.toolBarGrid1Button.Name = "toolBarGrid1Button";
            resources.ApplyResources(this.toolBarGrid1Button, "toolBarGrid1Button");
            this.toolBarGrid1Button.Click += new System.EventHandler(this.toolBarGrid1Button_Click);
            // 
            // toolBarGrid05Button
            // 
            this.toolBarGrid05Button.Name = "toolBarGrid05Button";
            resources.ApplyResources(this.toolBarGrid05Button, "toolBarGrid05Button");
            this.toolBarGrid05Button.Click += new System.EventHandler(this.toolBarGrid05Button_Click);
            // 
            // toolBarRotationAngleButton
            // 
            this.toolBarRotationAngleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarRotationAngleButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolBarAngle90Button,
            this.toolBarAngle45Button,
            this.toolBarAngle22Button,
            this.toolBarAngle1Button});
            resources.ApplyResources(this.toolBarRotationAngleButton, "toolBarRotationAngleButton");
            this.toolBarRotationAngleButton.Name = "toolBarRotationAngleButton";
            // 
            // toolBarAngle90Button
            // 
            this.toolBarAngle90Button.Checked = true;
            this.toolBarAngle90Button.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolBarAngle90Button.Name = "toolBarAngle90Button";
            resources.ApplyResources(this.toolBarAngle90Button, "toolBarAngle90Button");
            this.toolBarAngle90Button.Click += new System.EventHandler(this.toolBarAngle90Button_Click);
            // 
            // toolBarAngle45Button
            // 
            this.toolBarAngle45Button.Name = "toolBarAngle45Button";
            resources.ApplyResources(this.toolBarAngle45Button, "toolBarAngle45Button");
            this.toolBarAngle45Button.Click += new System.EventHandler(this.toolBarAngle45Button_Click);
            // 
            // toolBarAngle22Button
            // 
            this.toolBarAngle22Button.Name = "toolBarAngle22Button";
            resources.ApplyResources(this.toolBarAngle22Button, "toolBarAngle22Button");
            this.toolBarAngle22Button.Click += new System.EventHandler(this.toolBarAngle22Button_Click);
            // 
            // toolBarAngle1Button
            // 
            this.toolBarAngle1Button.Name = "toolBarAngle1Button";
            resources.ApplyResources(this.toolBarAngle1Button, "toolBarAngle1Button");
            this.toolBarAngle1Button.Click += new System.EventHandler(this.toolBarAngle1Button_Click);
            // 
            // toolBarRotateCCWButton
            // 
            this.toolBarRotateCCWButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarRotateCCWButton, "toolBarRotateCCWButton");
            this.toolBarRotateCCWButton.Name = "toolBarRotateCCWButton";
            this.toolBarRotateCCWButton.Click += new System.EventHandler(this.toolBarRotateCCWButton_Click);
            // 
            // toolBarRotateCWButton
            // 
            this.toolBarRotateCWButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarRotateCWButton, "toolBarRotateCWButton");
            this.toolBarRotateCWButton.Name = "toolBarRotateCWButton";
            this.toolBarRotateCWButton.Click += new System.EventHandler(this.toolBarRotateCWButton_Click);
            // 
            // toolBarSendToBackButton
            // 
            this.toolBarSendToBackButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarSendToBackButton, "toolBarSendToBackButton");
            this.toolBarSendToBackButton.Name = "toolBarSendToBackButton";
            this.toolBarSendToBackButton.Click += new System.EventHandler(this.toolBarSendToBackButton_Click);
            // 
            // toolBarBringToFrontButton
            // 
            this.toolBarBringToFrontButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolBarBringToFrontButton, "toolBarBringToFrontButton");
            this.toolBarBringToFrontButton.Name = "toolBarBringToFrontButton";
            this.toolBarBringToFrontButton.Click += new System.EventHandler(this.toolBarBringToFrontButton_Click);
            // 
            // toolBarPaintButton
            // 
            this.toolBarPaintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBarPaintButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.paintToolStripMenuItem,
            this.eraseToolStripMenuItem});
            resources.ApplyResources(this.toolBarPaintButton, "toolBarPaintButton");
            this.toolBarPaintButton.Name = "toolBarPaintButton";
            this.toolBarPaintButton.ButtonClick += new System.EventHandler(this.toolBarPaintButton_ButtonClick);
            // 
            // paintToolStripMenuItem
            // 
            resources.ApplyResources(this.paintToolStripMenuItem, "paintToolStripMenuItem");
            this.paintToolStripMenuItem.Name = "paintToolStripMenuItem";
            this.paintToolStripMenuItem.Click += new System.EventHandler(this.paintToolStripMenuItem_Click);
            // 
            // eraseToolStripMenuItem
            // 
            resources.ApplyResources(this.eraseToolStripMenuItem, "eraseToolStripMenuItem");
            this.eraseToolStripMenuItem.Name = "eraseToolStripMenuItem";
            this.eraseToolStripMenuItem.Click += new System.EventHandler(this.eraseToolStripMenuItem_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "bbm";
            resources.ApplyResources(this.openFileDialog, "openFileDialog");
            this.openFileDialog.SupportMultiDottedExtensions = true;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "bbm";
            resources.ApplyResources(this.saveFileDialog, "saveFileDialog");
            this.saveFileDialog.SupportMultiDottedExtensions = true;
            // 
            // saveExportImageDialog
            // 
            this.saveExportImageDialog.DefaultExt = "bmp";
            resources.ApplyResources(this.saveExportImageDialog, "saveExportImageDialog");
            this.saveExportImageDialog.SupportMultiDottedExtensions = true;
            // 
            // colorDialog
            // 
            this.colorDialog.Color = System.Drawing.Color.Gold;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.toolBar);
            this.Controls.Add(this.menuBar);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Location", global::BlueBrick.Properties.Settings.Default, "UIMainFormLocation", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Location = global::BlueBrick.Properties.Settings.Default.UIMainFormLocation;
            this.MainMenuStrip = this.menuBar;
            this.Name = "MainForm";
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseWheel);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            this.mainSplitContainer.ResumeLayout(false);
            this.mapPanel.ResumeLayout(false);
            this.mapPanel.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.toolSplitContainer.Panel1.ResumeLayout(false);
            this.toolSplitContainer.Panel2.ResumeLayout(false);
            this.toolSplitContainer.ResumeLayout(false);
            this.layerSplitContainer.Panel1.ResumeLayout(false);
            this.layerSplitContainer.Panel2.ResumeLayout(false);
            this.layerSplitContainer.ResumeLayout(false);
            this.LayerButtonFlowLayoutPanel.ResumeLayout(false);
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.toolBar.ResumeLayout(false);
            this.toolBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuBar;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveasToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStrip toolBar;
		private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolbarMenuItem;
		private BlueBrick.MainSplitContainer mainSplitContainer;
		private System.Windows.Forms.SplitContainer toolSplitContainer;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSplitButton toolBarRedoButton;
		private System.Windows.Forms.ToolStripSplitButton toolBarUndoButton;
		private BlueBrick.MapPanel mapPanel;
		private System.Windows.Forms.SplitContainer layerSplitContainer;
		private System.Windows.Forms.Button newLayerBrickButton;
		private System.Windows.Forms.Button trashLayerButton;
		private System.Windows.Forms.FlowLayoutPanel LayerButtonFlowLayoutPanel;
		private BlueBrick.LayerStackPanel layerStackPanel;
		private System.Windows.Forms.Button newLayerAreaButton;
		private System.Windows.Forms.Button newLayerGridButton;
		private System.Windows.Forms.Button newLayerTextButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button layerUpButton;
		private System.Windows.Forms.Button layerDownButton;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripButton toolBarDeleteButton;
		private System.Windows.Forms.ToolStripSplitButton toolBarSnapGridButton;
		private System.Windows.Forms.ToolStripMenuItem toolBarGrid32Button;
		private System.Windows.Forms.ToolStripMenuItem toolBarGrid16Button;
		private System.Windows.Forms.ToolStripMenuItem toolBarGrid8Button;
		private System.Windows.Forms.ToolStripMenuItem toolBarGrid1Button;
		private System.Windows.Forms.ToolStripMenuItem toolBarGrid05Button;
		private System.Windows.Forms.ToolStripButton toolBarRotateCCWButton;
		private System.Windows.Forms.ToolStripButton toolBarRotateCWButton;
		private System.Windows.Forms.ToolStripDropDownButton toolBarRotationAngleButton;
		private System.Windows.Forms.ToolStripMenuItem toolBarAngle90Button;
		private System.Windows.Forms.ToolStripMenuItem toolBarAngle45Button;
		private System.Windows.Forms.ToolStripMenuItem toolBarAngle22Button;
		private System.Windows.Forms.ToolStripMenuItem toolBarAngle1Button;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutBlueBrickToolStripMenuItem;
		private BlueBrick.PartLibraryPanel partsTabControl;
		private System.Windows.Forms.ToolStripMenuItem preferencesMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem exportAsPictureToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
		private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.SaveFileDialog saveExportImageDialog;
		private System.Windows.Forms.ToolStripMenuItem toolBarGrid4Button;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripButton toolBarCutButton;
		private System.Windows.Forms.ToolStripButton toolBarCopyButton;
		private System.Windows.Forms.ToolStripButton toolBarPasteButton;
		private System.Windows.Forms.ToolStripButton toolBarNewButton;
		private System.Windows.Forms.ToolStripButton toolBarOpenButton;
		private System.Windows.Forms.ToolStripButton toolBarSaveButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripMenuItem partListToolStripMenuItem;
		private System.Windows.Forms.ToolStripSplitButton toolBarPaintButton;
		private System.Windows.Forms.ToolStripMenuItem paintToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem eraseToolStripMenuItem;
		private System.Windows.Forms.ColorDialog colorDialog;
		private System.Windows.Forms.ToolStripMenuItem moveStepToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveStep32ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveStep16ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveStep8ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveStep4ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveStep1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem moveStep05ToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
		private System.Windows.Forms.ToolStripMenuItem moveStepDisabledToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rotationStepToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rotationStep90ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rotationStep45ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rotationStep22ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rotationStep1ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rotateCWToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rotateCCWToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem paintToolToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem paintToolPaintToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem paintToolEraseToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
		private System.Windows.Forms.ToolStripMenuItem paintToolChooseColorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem currentLayerOptionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mapBackgroundColorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectPathToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deselectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem statusBarMenuItem;
		private System.Windows.Forms.StatusStrip statusBar;
		private System.Windows.Forms.ToolStripProgressBar statusBarProgressBar;
		private System.Windows.Forms.ToolStripStatusLabel statusBarLabel;
		private System.Windows.Forms.ToolStripMenuItem openRecentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem reloadPartLibraryToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
		private System.Windows.Forms.ToolTip toolTipForMainForm;
		private System.Windows.Forms.ToolStripMenuItem helpContentsToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolBarSendToBackButton;
		private System.Windows.Forms.ToolStripButton toolBarBringToFrontButton;
		private System.Windows.Forms.ToolStripMenuItem findAndReplaceToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
		private System.Windows.Forms.ToolStripMenuItem electricCircuitsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem generalInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupMenuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem groupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ungroupToolStripMenuItem;
	}
}

