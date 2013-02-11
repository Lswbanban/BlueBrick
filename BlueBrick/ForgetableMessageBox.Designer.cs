namespace BlueBrick
{
	partial class ForgetableMessageBox
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForgetableMessageBox));
			this.topTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.dontShowCheckBox = new System.Windows.Forms.CheckBox();
			this.iconAndMessageTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.messageLabel = new System.Windows.Forms.Label();
			this.iconPictureBox = new System.Windows.Forms.PictureBox();
			this.buttonFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.topTableLayoutPanel.SuspendLayout();
			this.iconAndMessageTableLayoutPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
			this.buttonFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// topTableLayoutPanel
			// 
			resources.ApplyResources(this.topTableLayoutPanel, "topTableLayoutPanel");
			this.topTableLayoutPanel.Controls.Add(this.dontShowCheckBox, 0, 1);
			this.topTableLayoutPanel.Controls.Add(this.iconAndMessageTableLayoutPanel, 0, 0);
			this.topTableLayoutPanel.Controls.Add(this.buttonFlowLayoutPanel, 0, 2);
			this.topTableLayoutPanel.MinimumSize = new System.Drawing.Size(330, 116);
			this.topTableLayoutPanel.Name = "topTableLayoutPanel";
			// 
			// dontShowCheckBox
			// 
			resources.ApplyResources(this.dontShowCheckBox, "dontShowCheckBox");
			this.dontShowCheckBox.Name = "dontShowCheckBox";
			this.dontShowCheckBox.UseVisualStyleBackColor = true;
			// 
			// iconAndMessageTableLayoutPanel
			// 
			resources.ApplyResources(this.iconAndMessageTableLayoutPanel, "iconAndMessageTableLayoutPanel");
			this.iconAndMessageTableLayoutPanel.Controls.Add(this.messageLabel, 1, 0);
			this.iconAndMessageTableLayoutPanel.Controls.Add(this.iconPictureBox, 0, 0);
			this.iconAndMessageTableLayoutPanel.Name = "iconAndMessageTableLayoutPanel";
			// 
			// messageLabel
			// 
			resources.ApplyResources(this.messageLabel, "messageLabel");
			this.messageLabel.MaximumSize = new System.Drawing.Size(500, 0);
			this.messageLabel.Name = "messageLabel";
			// 
			// iconPictureBox
			// 
			resources.ApplyResources(this.iconPictureBox, "iconPictureBox");
			this.iconPictureBox.MinimumSize = new System.Drawing.Size(32, 32);
			this.iconPictureBox.Name = "iconPictureBox";
			this.iconPictureBox.TabStop = false;
			// 
			// buttonFlowLayoutPanel
			// 
			resources.ApplyResources(this.buttonFlowLayoutPanel, "buttonFlowLayoutPanel");
			this.buttonFlowLayoutPanel.Controls.Add(this.button1);
			this.buttonFlowLayoutPanel.Controls.Add(this.button2);
			this.buttonFlowLayoutPanel.Controls.Add(this.button3);
			this.buttonFlowLayoutPanel.Name = "buttonFlowLayoutPanel";
			// 
			// button1
			// 
			resources.ApplyResources(this.button1, "button1");
			this.button1.Name = "button1";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			resources.ApplyResources(this.button2, "button2");
			this.button2.Name = "button2";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// button3
			// 
			resources.ApplyResources(this.button3, "button3");
			this.button3.Name = "button3";
			this.button3.UseVisualStyleBackColor = true;
			// 
			// ForgetableMessageBox
			// 
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ControlBox = false;
			this.Controls.Add(this.topTableLayoutPanel);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ForgetableMessageBox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.TopMost = true;
			this.topTableLayoutPanel.ResumeLayout(false);
			this.topTableLayoutPanel.PerformLayout();
			this.iconAndMessageTableLayoutPanel.ResumeLayout(false);
			this.iconAndMessageTableLayoutPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
			this.buttonFlowLayoutPanel.ResumeLayout(false);
			this.buttonFlowLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel topTableLayoutPanel;
		private System.Windows.Forms.CheckBox dontShowCheckBox;
		private System.Windows.Forms.FlowLayoutPanel buttonFlowLayoutPanel;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.TableLayoutPanel iconAndMessageTableLayoutPanel;
		private System.Windows.Forms.Label messageLabel;
		private System.Windows.Forms.PictureBox iconPictureBox;
	}
}