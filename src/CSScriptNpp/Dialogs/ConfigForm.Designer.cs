namespace CSScriptNpp
{
    partial class ConfigForm
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.checkUpdates = new System.Windows.Forms.CheckBox();
            this.useCS6 = new System.Windows.Forms.CheckBox();
            this.contentControl = new System.Windows.Forms.TabControl();
            this.generalPage = new System.Windows.Forms.TabPage();
            this.restorePanels = new System.Windows.Forms.CheckBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.scriptsDir = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.customEngineLocation = new System.Windows.Forms.TextBox();
            this.installedEngineLocation = new System.Windows.Forms.TextBox();
            this.customEngine = new System.Windows.Forms.RadioButton();
            this.installedEngine = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.embeddedEngine = new System.Windows.Forms.RadioButton();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.contentControl.SuspendLayout();
            this.generalPage.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkUpdates
            // 
            this.checkUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkUpdates.AutoSize = true;
            this.checkUpdates.Location = new System.Drawing.Point(13, 216);
            this.checkUpdates.Name = "checkUpdates";
            this.checkUpdates.Size = new System.Drawing.Size(160, 17);
            this.checkUpdates.TabIndex = 7;
            this.checkUpdates.Text = "Check for updates at startup";
            this.checkUpdates.UseVisualStyleBackColor = true;
            // 
            // useCS6
            // 
            this.useCS6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.useCS6.AutoSize = true;
            this.useCS6.Location = new System.Drawing.Point(143, 140);
            this.useCS6.Name = "useCS6";
            this.useCS6.Size = new System.Drawing.Size(222, 17);
            this.useCS6.TabIndex = 7;
            this.useCS6.Text = "Script execution - handle C# 6.0  (Roslyn)";
            this.useCS6.UseVisualStyleBackColor = true;
            this.useCS6.Visible = false;
            // 
            // contentControl
            // 
            this.contentControl.Controls.Add(this.generalPage);
            this.contentControl.Controls.Add(this.tabPage2);
            this.contentControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentControl.Location = new System.Drawing.Point(0, 0);
            this.contentControl.Name = "contentControl";
            this.contentControl.SelectedIndex = 0;
            this.contentControl.Size = new System.Drawing.Size(423, 288);
            this.contentControl.TabIndex = 8;
            // 
            // generalPage
            // 
            this.generalPage.Controls.Add(this.restorePanels);
            this.generalPage.Controls.Add(this.checkUpdates);
            this.generalPage.Controls.Add(this.useCS6);
            this.generalPage.Controls.Add(this.linkLabel2);
            this.generalPage.Controls.Add(this.linkLabel1);
            this.generalPage.Location = new System.Drawing.Point(4, 22);
            this.generalPage.Name = "generalPage";
            this.generalPage.Padding = new System.Windows.Forms.Padding(3);
            this.generalPage.Size = new System.Drawing.Size(415, 262);
            this.generalPage.TabIndex = 0;
            this.generalPage.Text = "General";
            this.generalPage.UseVisualStyleBackColor = true;
            // 
            // restorePanels
            // 
            this.restorePanels.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.restorePanels.AutoSize = true;
            this.restorePanels.Location = new System.Drawing.Point(13, 193);
            this.restorePanels.Name = "restorePanels";
            this.restorePanels.Size = new System.Drawing.Size(147, 17);
            this.restorePanels.TabIndex = 8;
            this.restorePanels.Text = "Restore panels on startup";
            this.restorePanels.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(10, 242);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(120, 13);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Edit settings file  instead";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.scriptsDir);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(415, 262);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "CS-Script";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Scripts Directory";
            // 
            // scriptsDir
            // 
            this.scriptsDir.Location = new System.Drawing.Point(9, 27);
            this.scriptsDir.Name = "scriptsDir";
            this.scriptsDir.Size = new System.Drawing.Size(398, 20);
            this.scriptsDir.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.customEngineLocation);
            this.groupBox1.Controls.Add(this.installedEngineLocation);
            this.groupBox1.Controls.Add(this.customEngine);
            this.groupBox1.Controls.Add(this.installedEngine);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.embeddedEngine);
            this.groupBox1.Location = new System.Drawing.Point(9, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(398, 161);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Engine Location";
            // 
            // customEngineLocation
            // 
            this.customEngineLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.customEngineLocation.Location = new System.Drawing.Point(43, 120);
            this.customEngineLocation.Name = "customEngineLocation";
            this.customEngineLocation.Size = new System.Drawing.Size(343, 20);
            this.customEngineLocation.TabIndex = 1;
            // 
            // installedEngineLocation
            // 
            this.installedEngineLocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.installedEngineLocation.Location = new System.Drawing.Point(43, 72);
            this.installedEngineLocation.Name = "installedEngineLocation";
            this.installedEngineLocation.ReadOnly = true;
            this.installedEngineLocation.Size = new System.Drawing.Size(343, 20);
            this.installedEngineLocation.TabIndex = 1;
            // 
            // customEngine
            // 
            this.customEngine.AutoSize = true;
            this.customEngine.Location = new System.Drawing.Point(22, 97);
            this.customEngine.Name = "customEngine";
            this.customEngine.Size = new System.Drawing.Size(104, 17);
            this.customEngine.TabIndex = 0;
            this.customEngine.Text = "Custom Location";
            this.customEngine.UseVisualStyleBackColor = true;
            this.customEngine.CheckedChanged += new System.EventHandler(this.engine_CheckedChanged);
            // 
            // installedEngine
            // 
            this.installedEngine.AutoSize = true;
            this.installedEngine.Location = new System.Drawing.Point(22, 49);
            this.installedEngine.Name = "installedEngine";
            this.installedEngine.Size = new System.Drawing.Size(214, 17);
            this.installedEngine.TabIndex = 0;
            this.installedEngine.Text = "Installed CS-Script (%CSSCRIPT_DIR%)";
            this.installedEngine.UseVisualStyleBackColor = true;
            this.installedEngine.CheckedChanged += new System.EventHandler(this.engine_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(22, 49);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(112, 17);
            this.radioButton2.TabIndex = 0;
            this.radioButton2.Text = "Embedded Engine";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // embeddedEngine
            // 
            this.embeddedEngine.AutoSize = true;
            this.embeddedEngine.Location = new System.Drawing.Point(22, 26);
            this.embeddedEngine.Name = "embeddedEngine";
            this.embeddedEngine.Size = new System.Drawing.Size(76, 17);
            this.embeddedEngine.TabIndex = 0;
            this.embeddedEngine.Text = "Embedded";
            this.embeddedEngine.UseVisualStyleBackColor = true;
            this.embeddedEngine.CheckedChanged += new System.EventHandler(this.engine_CheckedChanged);
            // 
            // linkLabel2
            // 
            this.linkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(240, 242);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(167, 13);
            this.linkLabel2.TabIndex = 5;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Edit Visual Studio project template";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 288);
            this.Controls.Add(this.contentControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CS-Script Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConfigForm_KeyDown);
            this.contentControl.ResumeLayout(false);
            this.generalPage.ResumeLayout(false);
            this.generalPage.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkUpdates;
        private System.Windows.Forms.CheckBox useCS6;
        private System.Windows.Forms.TabControl contentControl;
        private System.Windows.Forms.TabPage generalPage;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton customEngine;
        private System.Windows.Forms.RadioButton installedEngine;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton embeddedEngine;
        private System.Windows.Forms.TextBox customEngineLocation;
        private System.Windows.Forms.TextBox installedEngineLocation;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox restorePanels;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox scriptsDir;
        private System.Windows.Forms.LinkLabel linkLabel2;
    }
}