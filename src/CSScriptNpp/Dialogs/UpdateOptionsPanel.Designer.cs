﻿namespace CSScriptNpp.Dialogs
{
    partial class UpdateOptionsPanel
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
            this.msiDeployment = new System.Windows.Forms.RadioButton();
            this.customDeployment = new System.Windows.Forms.RadioButton();
            this.optionsGroup = new System.Windows.Forms.GroupBox();
            this.manualDeployment = new System.Windows.Forms.RadioButton();
            this.okBtn = new System.Windows.Forms.Button();
            this.skipBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.versionLbl = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.progressLbl = new System.Windows.Forms.Label();
            this.releaseNotes = new System.Windows.Forms.LinkLabel();
            this.showOptions = new System.Windows.Forms.CheckBox();
            this.updateAfterExit = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.releaseInfo = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.optionsGroup.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // msiDeployment
            // 
            this.msiDeployment.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.msiDeployment.Location = new System.Drawing.Point(6, 101);
            this.msiDeployment.Name = "msiDeployment";
            this.msiDeployment.Size = new System.Drawing.Size(447, 44);
            this.msiDeployment.TabIndex = 0;
            this.msiDeployment.Tag = "msi";
            this.msiDeployment.Text = "Download &MSI.\r\nNot suitable for Notepad++ portable installation. This option may" +
    " require system reboot.";
            this.msiDeployment.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.msiDeployment.UseVisualStyleBackColor = true;
            this.msiDeployment.Visible = false;
            // 
            // customDeployment
            // 
            this.customDeployment.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.customDeployment.Checked = true;
            this.customDeployment.Location = new System.Drawing.Point(6, 16);
            this.customDeployment.Name = "customDeployment";
            this.customDeployment.Size = new System.Drawing.Size(447, 41);
            this.customDeployment.TabIndex = 1;
            this.customDeployment.TabStop = true;
            this.customDeployment.Tag = "custom";
            this.customDeployment.Text = "Download and &replace binaries (Recommended). Suitable for any Notepad++ installa" +
    "tion.\r\nIt is an equivalent of Notepad++ Plugin Manager.";
            this.customDeployment.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.customDeployment.UseVisualStyleBackColor = true;
            this.customDeployment.CheckedChanged += new System.EventHandler(this.customDeployment_CheckedChanged);
            // 
            // optionsGroup
            // 
            this.optionsGroup.Controls.Add(this.customDeployment);
            this.optionsGroup.Controls.Add(this.manualDeployment);
            this.optionsGroup.Controls.Add(this.msiDeployment);
            this.optionsGroup.Location = new System.Drawing.Point(6, 6);
            this.optionsGroup.Name = "optionsGroup";
            this.optionsGroup.Size = new System.Drawing.Size(464, 149);
            this.optionsGroup.TabIndex = 2;
            this.optionsGroup.TabStop = false;
            // 
            // manualDeployment
            // 
            this.manualDeployment.AutoSize = true;
            this.manualDeployment.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.manualDeployment.Location = new System.Drawing.Point(6, 60);
            this.manualDeployment.Name = "manualDeployment";
            this.manualDeployment.Size = new System.Drawing.Size(287, 30);
            this.manualDeployment.TabIndex = 0;
            this.manualDeployment.Tag = "download";
            this.manualDeployment.Text = "&Download binaries.\r\nThis option requires you to deploy the binaries manually.";
            this.manualDeployment.UseVisualStyleBackColor = true;
            // 
            // okBtn
            // 
            this.okBtn.Location = new System.Drawing.Point(379, 6);
            this.okBtn.Name = "okBtn";
            this.okBtn.Size = new System.Drawing.Size(101, 23);
            this.okBtn.TabIndex = 3;
            this.okBtn.Text = "&Proceed";
            this.okBtn.UseVisualStyleBackColor = true;
            this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
            // 
            // skipBtn
            // 
            this.skipBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.skipBtn.Location = new System.Drawing.Point(379, 35);
            this.skipBtn.Name = "skipBtn";
            this.skipBtn.Size = new System.Drawing.Size(101, 23);
            this.skipBtn.TabIndex = 3;
            this.skipBtn.Text = "Skip This Version";
            this.skipBtn.UseVisualStyleBackColor = true;
            this.skipBtn.Click += new System.EventHandler(this.skipBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Available version: ";
            // 
            // versionLbl
            // 
            this.versionLbl.AutoSize = true;
            this.versionLbl.Location = new System.Drawing.Point(97, 6);
            this.versionLbl.Name = "versionLbl";
            this.versionLbl.Size = new System.Drawing.Size(53, 13);
            this.versionLbl.TabIndex = 4;
            this.versionLbl.Text = "<version>";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(8, 25);
            this.progressBar.MarqueeAnimationSpeed = 50;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(352, 10);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 5;
            // 
            // progressLbl
            // 
            this.progressLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressLbl.Location = new System.Drawing.Point(265, 6);
            this.progressLbl.Name = "progressLbl";
            this.progressLbl.Size = new System.Drawing.Size(97, 15);
            this.progressLbl.TabIndex = 4;
            this.progressLbl.Text = "Downloading";
            this.progressLbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.progressLbl.UseMnemonic = false;
            this.progressLbl.Visible = false;
            // 
            // releaseNotes
            // 
            this.releaseNotes.Location = new System.Drawing.Point(346, 69);
            this.releaseNotes.Name = "releaseNotes";
            this.releaseNotes.Size = new System.Drawing.Size(136, 18);
            this.releaseNotes.TabIndex = 6;
            this.releaseNotes.TabStop = true;
            this.releaseNotes.Text = "View Release Notes";
            this.releaseNotes.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.releaseNotes.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.releaseNotes_LinkClicked);
            // 
            // showOptions
            // 
            this.showOptions.AutoSize = true;
            this.showOptions.Location = new System.Drawing.Point(182, 44);
            this.showOptions.Name = "showOptions";
            this.showOptions.Size = new System.Drawing.Size(138, 17);
            this.showOptions.TabIndex = 7;
            this.showOptions.Text = "Show Updating Options";
            this.showOptions.UseVisualStyleBackColor = true;
            this.showOptions.Visible = false;
            // 
            // updateAfterExit
            // 
            this.updateAfterExit.AutoSize = true;
            this.updateAfterExit.Location = new System.Drawing.Point(8, 44);
            this.updateAfterExit.Name = "updateAfterExit";
            this.updateAfterExit.Size = new System.Drawing.Size(168, 17);
            this.updateAfterExit.TabIndex = 9;
            this.updateAfterExit.Tag = "";
            this.updateAfterExit.Text = "Update after Notepad++ exits.";
            this.toolTip1.SetToolTip(this.updateAfterExit, "Download binaries in background and install them after Notepad++ exits.\r\nThis opt" +
        "ion is inly available for the \"Download and replace binaries\" deployment mode.");
            this.updateAfterExit.UseVisualStyleBackColor = true;
            this.updateAfterExit.CheckedChanged += new System.EventHandler(this.updateAfterExit_CheckedChanged);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(332, 64);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "&Cancel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(4, 71);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(482, 187);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.releaseInfo);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage1.Size = new System.Drawing.Size(474, 161);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Release Info";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // releaseInfo
            // 
            this.releaseInfo.BackColor = System.Drawing.Color.White;
            this.releaseInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.releaseInfo.Location = new System.Drawing.Point(3, 3);
            this.releaseInfo.Multiline = true;
            this.releaseInfo.Name = "releaseInfo";
            this.releaseInfo.ReadOnly = true;
            this.releaseInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.releaseInfo.Size = new System.Drawing.Size(468, 155);
            this.releaseInfo.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.optionsGroup);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage2.Size = new System.Drawing.Size(474, 161);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Update Options";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // UpdateOptionsPanel
            // 
            this.AcceptButton = this.okBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.skipBtn;
            this.ClientSize = new System.Drawing.Size(490, 262);
            this.Controls.Add(this.releaseNotes);
            this.Controls.Add(this.updateAfterExit);
            this.Controls.Add(this.showOptions);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.progressLbl);
            this.Controls.Add(this.versionLbl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.skipBtn);
            this.Controls.Add(this.okBtn);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "UpdateOptionsPanel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CS-Script Update";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UpdateOptionsPanel_FormClosed);
            this.Load += new System.EventHandler(this.UpdateOptionsPanel_Load);
            this.optionsGroup.ResumeLayout(false);
            this.optionsGroup.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton msiDeployment;
        private System.Windows.Forms.RadioButton customDeployment;
        private System.Windows.Forms.GroupBox optionsGroup;
        private System.Windows.Forms.RadioButton manualDeployment;
        private System.Windows.Forms.Button okBtn;
        private System.Windows.Forms.Button skipBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label versionLbl;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label progressLbl;
        private System.Windows.Forms.LinkLabel releaseNotes;
        private System.Windows.Forms.CheckBox showOptions;
        private System.Windows.Forms.CheckBox updateAfterExit;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox releaseInfo;
        private System.Windows.Forms.TabPage tabPage2;
    }
}